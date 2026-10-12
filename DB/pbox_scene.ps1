# =====================================================================
#  P-BOX 트윈 캡쳐용 : 네 가지 상태를 만들어 그대로 멈춰 둔다
#  2026-10-12
# =====================================================================
#
#  쓰는 법 (PowerShell)
#
#      .\pbox_scene.ps1 sto1      1Fork 입고 작업 받은 상태
#      .\pbox_scene.ps1 sto2      2Fork 입고(트윈) 작업 받은 상태
#      .\pbox_scene.ps1 ret1      1Fork 출고 작업 받은 상태
#      .\pbox_scene.ps1 ret2      2Fork 출고(트윈) 작업 받은 상태
#      .\pbox_scene.ps1 rethd1    1Fork 출고를 H/S 에 기록한 상태
#      .\pbox_scene.ps1 rethd2    2Fork 출고를 H/S 에 기록한 상태
#
#      .\pbox_scene.ps1 clean     시험 자료를 지우고 태스크를 되살린다
#
#  하는 일
#
#    1) 상태를 움직이는 태스크 셋을 멈춘다 (IO_SCH / SC / CV)
#       멈추지 않으면 1초 안에 다음 단계로 넘어가 버려서 사진을 찍을 수 없다.
#       ScSim·CvSim·HostSim·Client 는 그대로 둔다. Client 는 DB 를 읽어
#       그리므로 화면에는 아래에서 넣은 상태가 그대로 보인다.
#
#    2) JOB_MST / CV_DATA / SC_DATA 를 그 순간의 값으로 맞춘다.
#       값은 실제로 돌려서 나온 것을 그대로 쓴다.
#         입고 트윈 : USE_FK_OD=1  FK1=420 것  FK2=419 것
#         출고 트윈 : 베이 작은 쪽이 포크#1, H/S 는 410 / 411
#         크레인에 나가는 뱅크는 01→1 / 02→2 로 접힌 값
#
#    3) 끝나면 무엇을 찍으면 되는지 알려 준다.
#
#  ※ 다 찍은 뒤에는 반드시 clean 으로 되돌린다.
#     태스크가 멈춘 채로 두면 현장 로직이 돌지 않는다.
# =====================================================================

param([Parameter(Mandatory=$true)][ValidateSet('sto1','sto2','ret1','ret2','rethd1','rethd2','clean')]
      [string]$Scene)

$ErrorActionPreference = 'Stop'

# ---- 설정 : 현장에 맞게 고치면 된다 -------------------------------------
$PSQL    = 'C:\Program Files\PostgreSQL\16\bin\psql.exe'
$PGHOST  = 'localhost'
$PGPORT  = '5432'
$PGDB    = 'KET_WCS'
$PGUSER  = 'KET_WCS'
$env:PGPASSWORD      = 'KET_WCS'
$env:PGCLIENTENCODING = 'UTF8'

$TASKROOT = 'D:\인수인계\한국단자\ReNewal\KET_Renewal\TASKs\KET_TASKs'
$TASKS = @(
  @{ Name='IO_TASK_SEMI_FINISH';  Dir="$TASKROOT\WCS_IO_SCH_Original\bin\Debug" },
  @{ Name='WCS_TASK_SC';          Dir="$TASKROOT\WCS_TASK_SC_Original\bin\Debug" },
  @{ Name='WCS_TASK_CV';          Dir="$TASKROOT\WCS_TASK_CV_original\bin\Debug" }
)

# P-BOX 자리 (기존 ECS CLib::GetScHSTrackPerRank)
$SC      = '911'      # P-BOX 크레인 11호기
$WH      = '20'       # P-BOX 창고
$PLC     = '04'
$STO_FK1 = '420'      # 입고 H/S 포크#1  (작업대 222)
$STO_FK2 = '419'      # 입고 H/S 포크#2  (작업대 221)
$RET_FK1 = '410'      # 출고 H/S 포크#1
$RET_FK2 = '411'      # 출고 H/S 포크#2
$RET_STN = '251'      # 출고 도착 작업대
$TAG     = 'SCENE'    # 이 스크립트가 만든 작업 표시

# -------------------------------------------------------------------------
function Sql([string]$q) {
  & $PSQL -h $PGHOST -p $PGPORT -U $PGUSER -d $PGDB -q -c $q
  if ($LASTEXITCODE -ne 0) { throw "psql 실패: $q" }
}
function Ask([string]$q) {
  (& $PSQL -h $PGHOST -p $PGPORT -U $PGUSER -d $PGDB -A -t -c $q) -join "`n"
}
function Stop-Tasks {
  foreach ($t in $TASKS) {
    $p = Get-Process -Name $t.Name -ErrorAction SilentlyContinue
    if ($p) { $p | Stop-Process -Force; "  멈춤 : $($t.Name)" }
  }
  Start-Sleep -Seconds 2
}
function Start-Tasks {
  foreach ($t in $TASKS) {
    if (Get-Process -Name $t.Name -ErrorAction SilentlyContinue) { continue }
    $exe = Join-Path $t.Dir "$($t.Name).exe"
    if (Test-Path $exe) { Start-Process -FilePath $exe -WorkingDirectory $t.Dir; "  올림 : $($t.Name)" }
    else { "  없음 : $exe" }
  }
}
function Clear-Scene {
  # 시험으로 넣었던 작업은 모두 걷어낸다.
  #   TWIN_TEST / TWINRUN 은 트윈을 실제로 돌려 보며 넣었던 것이다.
  #   이것이 남아 있으면 P-BOX 출고 H/S 에 작업번호가 그대로 떠 있어
  #   현장 자료인 줄 알고 들여다보게 된다.
  Sql "DELETE FROM job_mst WHERE ins_user_id IN ('$TAG','TWIN_TEST','TWINRUN');"
  Sql @"
UPDATE cv_data SET lugg_no_rd='0', lugg_no_od='0', dest_pos_rd='0', dest_pos_od='0',
                   job_typ_rd='0', job_typ_od='0', sensor0_data_rd='0',
                   stohs_ready_rd='0', reths_ready_rd='0', barcode=''
 WHERE plc_no='$PLC' AND mc_no IN ('$STO_FK1','$STO_FK2','$RET_FK1','$RET_FK2');
"@
  Sql @"
UPDATE sc_data SET job_typ_od='0', use_fk_od='0',
       lugg_no_fk1_od='0', start_bank_fk1_od='0', start_bay_fk1_od='0', start_level_fk1_od='0',
       start_hspos_fk1_od='0', dest_bank_fk1_od='0', dest_bay_fk1_od='0', dest_level_fk1_od='0', dest_hspos_fk1_od='0',
       lugg_no_fk2_od='0', start_bank_fk2_od='0', start_bay_fk2_od='0', start_level_fk2_od='0',
       start_hspos_fk2_od='0', dest_bank_fk2_od='0', dest_bay_fk2_od='0', dest_level_fk2_od='0', dest_hspos_fk2_od='0',
       job_typ_rd='0', use_fk_rd='0',
       lugg_no_fk1_rd='0', start_bank_fk1_rd='0', start_bay_fk1_rd='0', start_level_fk1_rd='0',
       dest_bank_fk1_rd='0', dest_bay_fk1_rd='0', dest_level_fk1_rd='0',
       lugg_no_fk2_rd='0', start_bank_fk2_rd='0', start_bay_fk2_rd='0', start_level_fk2_rd='0',
       dest_bank_fk2_rd='0', dest_bay_fk2_rd='0', dest_level_fk2_rd='0',
       itn_lugg_fk1='0', itn_lugg_fk2='0', complete_rd='0', ucstatus_rd='1', od_rq_yn='N'
 WHERE sc_no='$SC';
"@
}

# 작업 한 건 넣기
function Add-Job([string]$lugg, [string]$typ, [string]$startPos, [string]$startLoc,
                 [string]$destPos, [string]$destLoc, [string]$status, [string]$hsTrack, [string]$pair) {
  Sql @"
INSERT INTO job_mst (wh_typ, lugg_no, start_pos, start_location, dest_pos, dest_location,
                     job_typ, job_status, job_priority, hs_track_no, pair_lugg_no,
                     ins_dt, ins_user_id, remarks, wc_step)
VALUES ('$WH','$lugg','$startPos','$startLoc','$destPos','$destLoc',
        '$typ','$status','100','$hsTrack','$pair', NOW(), '$TAG', '', '0');
"@
}
# 화물이 올라와 있는 H/S
function Set-HsLoaded([string]$track, [string]$lugg, [string]$destPos, [string]$jobTyp, [string]$readyCol) {
  Sql @"
UPDATE cv_data SET lugg_no_rd='$lugg', lugg_no_od='$lugg',
                   dest_pos_rd='$destPos', dest_pos_od='$destPos',
                   job_typ_rd='$jobTyp', job_typ_od='$jobTyp',
                   sensor0_data_rd='1', auto_mode_rd='1', error_code='0',
                   tr_pause_rd='0', tr_pause_od='0', $readyCol='1'
 WHERE plc_no='$PLC' AND mc_no='$track';
"@
}
# 크레인이 들고 있는 작업 (지시 + 읽기 둘 다. 읽기가 있어야 화면에 색이 뜬다)
function Set-ScHolding([string]$jobTyp, [string]$useFk,
                       [string]$l1, [string]$sBank1, [string]$sBay1, [string]$sLev1, [string]$sHs1,
                                    [string]$dBank1, [string]$dBay1, [string]$dLev1, [string]$dHs1,
                       [string]$l2, [string]$sBank2, [string]$sBay2, [string]$sLev2, [string]$sHs2,
                                    [string]$dBank2, [string]$dBay2, [string]$dLev2, [string]$dHs2) {
  # 뱅크는 크레인에 나갈 때 1/2 로 접힌다 (ScThread.cs GfSideOfBank)
  function Fold([string]$b) { $n = 0; [void][int]::TryParse($b, [ref]$n); if ($n -le 0) { return '0' }; return [string]((($n + 1) % 2) + 1) }
  Sql @"
UPDATE sc_data SET job_typ_od='$jobTyp', use_fk_od='$useFk',
       lugg_no_fk1_od='$l1', start_bank_fk1_od='$sBank1', start_bay_fk1_od='$sBay1',
       start_level_fk1_od='$sLev1', start_hspos_fk1_od='$sHs1',
       dest_bank_fk1_od='$dBank1', dest_bay_fk1_od='$dBay1', dest_level_fk1_od='$dLev1', dest_hspos_fk1_od='$dHs1',
       lugg_no_fk2_od='$l2', start_bank_fk2_od='$sBank2', start_bay_fk2_od='$sBay2',
       start_level_fk2_od='$sLev2', start_hspos_fk2_od='$sHs2',
       dest_bank_fk2_od='$dBank2', dest_bay_fk2_od='$dBay2', dest_level_fk2_od='$dLev2', dest_hspos_fk2_od='$dHs2',
       job_typ_rd='$jobTyp', use_fk_rd='$useFk',
       lugg_no_fk1_rd='$l1', start_bank_fk1_rd='$(Fold $sBank1)', start_bay_fk1_rd='$sBay1', start_level_fk1_rd='$sLev1',
       dest_bank_fk1_rd='$(Fold $dBank1)', dest_bay_fk1_rd='$dBay1', dest_level_fk1_rd='$dLev1',
       lugg_no_fk2_rd='$l2', start_bank_fk2_rd='$(Fold $sBank2)', start_bay_fk2_rd='$sBay2', start_level_fk2_rd='$sLev2',
       dest_bank_fk2_rd='$(Fold $dBank2)', dest_bay_fk2_rd='$dBay2', dest_level_fk2_rd='$dLev2',
       itn_lugg_fk1='$l1', itn_lugg_fk2='$l2',
       ucstatus_rd='2', complete_rd='0', write_flag_od='1', od_rq_yn='N'
 WHERE sc_no='$SC';
"@
}

# =====================================================================
Write-Host ""
Write-Host "P-BOX 장면 : $Scene" -ForegroundColor Cyan
Write-Host ("-" * 62)

if ($Scene -eq 'clean') {
  Clear-Scene
  Write-Host "시험 자료를 지웠다."
  Start-Tasks
  Write-Host ""
  Write-Host "태스크를 되살렸다. 현장 로직이 다시 돈다." -ForegroundColor Green
  return
}

Write-Host "상태를 움직이는 태스크를 멈춘다 (IO_SCH / SC / CV)"
Stop-Tasks
Clear-Scene

switch ($Scene) {

  # ---------------------------------------------------------- 1Fork 입고
  'sto1' {
    Add-Job '5101' '1' '222' '00-000-00' $SC '01-021-01' '21' '' '0'
    Set-HsLoaded $STO_FK1 '5101' $SC '1' 'stohs_ready_rd'
    Set-ScHolding '1' '0'  '5101' '0' '0' '0' '1'  '01' '021' '01' '0' `
                           '0'    '0' '0' '0' '0'  '0'  '0'   '0'  '0'
    Write-Host ""
    Write-Host "1Fork 입고 작업을 받은 상태" -ForegroundColor Yellow
    Write-Host "  입고 H/S  TRACK $STO_FK1 (작업대 222) : 화물 있음, 작업번호 5101"
    Write-Host "  크레인 $SC : 포크#1 에 5101, USE_FK=0, 도착 랙 01-021-01"
    Write-Host "  포크#2 는 비어 있다"
    Write-Host ""
    Write-Host "찍을 것 : SC 컨트롤 + 입고 H/S($STO_FK1) 컨트롤 + SC 대화상자"
  }

  # ---------------------------------------------------------- 2Fork 입고
  'sto2' {
    Add-Job '5201' '1' '222' '00-000-00' $SC '01-023-01' '21' '' '5202'
    Add-Job '5202' '1' '221' '00-000-00' $SC '02-024-01' '21' '' '5201'
    Set-HsLoaded $STO_FK1 '5201' $SC '1' 'stohs_ready_rd'
    Set-HsLoaded $STO_FK2 '5202' $SC '1' 'stohs_ready_rd'
    Set-ScHolding '1' '1'  '5201' '0' '0' '0' '1'  '01' '023' '01' '0' `
                           '5202' '0' '0' '0' '1'  '02' '024' '01' '0'
    Write-Host ""
    Write-Host "2Fork 입고(트윈) 작업을 받은 상태" -ForegroundColor Yellow
    Write-Host "  입고 H/S  TRACK $STO_FK1 (작업대 222) : 화물 있음, 작업번호 5201  -> 포크#1"
    Write-Host "  입고 H/S  TRACK $STO_FK2 (작업대 221) : 화물 있음, 작업번호 5202  -> 포크#2"
    Write-Host "  크레인 $SC : USE_FK=1 (포크#1,#2 동시), 5201 / 5202"
    Write-Host ""
    Write-Host "찍을 것 : SC 컨트롤 + 입고 H/S($STO_FK1,$STO_FK2) 컨트롤 + SC 대화상자"
  }

  # ---------------------------------------------------------- 1Fork 출고
  'ret1' {
    Add-Job '5301' '2' $SC '01-031-02' $RET_STN '00-000-00' '21' $RET_FK1 '0'
    Set-ScHolding '2' '0'  '5301' '01' '031' '02' '0'  '0' '0' '0' '2' `
                           '0'    '0'  '0'   '0'  '0'  '0' '0' '0' '0'
    Write-Host ""
    Write-Host "1Fork 출고 작업을 받은 상태" -ForegroundColor Yellow
    Write-Host "  크레인 $SC : 포크#1 에 5301, USE_FK=0"
    Write-Host "    꺼낼 자리 01-031-02  (크레인에는 뱅크가 1 로 접혀 나간다)"
    Write-Host "    내려놓을 곳 출고 H/S 랭크 2 -> TRACK $RET_FK1"
    Write-Host "  포크#2 는 비어 있다"
    Write-Host ""
    Write-Host "찍을 것 : SC 컨트롤 + SC 대화상자"
  }

  # ---------------------------------------------------------- 2Fork 출고
  'ret2' {
    # 베이가 작은 쪽이 포크#1 이다 (기존 ECS Job.cpp 1220~1226)
    Add-Job '5401' '2' $SC '01-033-02' $RET_STN '00-000-00' '21' $RET_FK1 '5402'
    Add-Job '5402' '2' $SC '01-036-02' $RET_STN '00-000-00' '21' $RET_FK2 '5401'
    Set-ScHolding '2' '1'  '5401' '01' '033' '02' '0'  '0' '0' '0' '2' `
                           '5402' '01' '036' '02' '0'  '0' '0' '0' '2'
    Write-Host ""
    Write-Host "2Fork 출고(트윈) 작업을 받은 상태" -ForegroundColor Yellow
    Write-Host "  크레인 $SC : USE_FK=1 (포크#1,#2 동시)"
    Write-Host "    포크#1  5401  01-033-02  -> 출고 H/S TRACK $RET_FK1"
    Write-Host "    포크#2  5402  01-036-02  -> 출고 H/S TRACK $RET_FK2"
    Write-Host "    베이가 작은 033 쪽이 포크#1 이다"
    Write-Host ""
    Write-Host "찍을 것 : SC 컨트롤 + SC 대화상자"
  }

  # ------------------------------------------- 1Fork 출고를 H/S 에 기록
  'rethd1' {
    # 상태 11 (CV 구동중) 이다. 29 가 아니다.
    #   29 는 크레인이 내려놓기만 하고 H/S 에 아직 아무것도 안 적힌 때라
    #   도착지가 비어 있다. 도착지가 적히는 바로 그 순간 11 로 넘어간다.
    #   (IO_SCH CV_RETHS_PLC 가 CV_DATA 에 쓰면서 UPDATE_JOB_DATA(ST_CV_RUN))
    #   실제로 돌려 본 값 그대로다. 7501 : 29 -> (재하 센서 ON) -> 11 + 도착지 251
    Add-Job '5301' '2' $SC '01-031-02' $RET_STN '00-000-00' '11' $RET_FK1 '0'
    Set-HsLoaded $RET_FK1 '5301' $RET_STN '2' 'reths_ready_rd'
    Write-Host ""
    Write-Host "1Fork 출고를 H/S 에 기록한 상태" -ForegroundColor Yellow
    Write-Host "  출고 H/S  TRACK $RET_FK1 : 화물 있음, 작업번호 5301, 도착지 $RET_STN"
    Write-Host "  작업 5301 은 상태 11 (CV 구동중 - 방금 H/S 에 도착지를 적었다)"
    Write-Host "  크레인은 비어 있다"
    Write-Host ""
    Write-Host "찍을 것 : 모니터링에서 작업번호 5301 을 고른 상태 + H/S(CV) 컨트롤"
  }

  # ------------------------------------------- 2Fork 출고를 H/S 에 기록
  'rethd2' {
    # rethd1 과 같은 이유로 상태는 11 이다.
    Add-Job '5401' '2' $SC '01-033-02' $RET_STN '00-000-00' '11' $RET_FK1 '5402'
    Add-Job '5402' '2' $SC '01-036-02' $RET_STN '00-000-00' '11' $RET_FK2 '5401'
    Set-HsLoaded $RET_FK1 '5401' $RET_STN '2' 'reths_ready_rd'
    Set-HsLoaded $RET_FK2 '5402' $RET_STN '2' 'reths_ready_rd'
    Write-Host ""
    Write-Host "2Fork 출고를 H/S 에 기록한 상태" -ForegroundColor Yellow
    Write-Host "  출고 H/S  TRACK $RET_FK1 : 화물 있음, 작업번호 5401  (포크#1 이 내려놓은 것)"
    Write-Host "  출고 H/S  TRACK $RET_FK2 : 화물 있음, 작업번호 5402  (포크#2 가 내려놓은 것)"
    Write-Host "  두 작업 모두 상태 11 (각자 제 트랙에 도착지가 적혔다)"
    Write-Host "  크레인은 비어 있다"
    Write-Host ""
    Write-Host "찍을 것 : 모니터링에서 작업번호 5401/5402 를 고른 상태 + H/S(CV) 컨트롤"
  }
}

Write-Host ("-" * 62)
Write-Host "지금 상태" -ForegroundColor Cyan
Write-Host (Ask "SELECT '  JOB    ' || string_agg(lugg_no || ' typ' || job_typ || ' st' || job_status || ' hs' || COALESCE(NULLIF(hs_track_no,''),'-'), '   ' ORDER BY lugg_no) FROM job_mst WHERE ins_user_id = '$TAG';")
Write-Host (Ask "SELECT '  SC     USE_FK=' || use_fk_od || '  FK1=' || lugg_no_fk1_od || '  FK2=' || lugg_no_fk2_od || '  ucstatus=' || ucstatus_rd FROM sc_data WHERE sc_no = '$SC';")
Write-Host (Ask "SELECT '  CV     ' || string_agg('TR#' || mc_no || ' load=' || sensor0_data_rd || ' job=' || lugg_no_rd, '   ' ORDER BY mc_no) FROM cv_data WHERE plc_no = '$PLC' AND mc_no IN ('$STO_FK1','$STO_FK2','$RET_FK1','$RET_FK2') AND sensor0_data_rd = '1';")
Write-Host ""
Write-Host "태스크가 멈춰 있어 이 상태 그대로 있는다. 천천히 찍으면 된다." -ForegroundColor Green
Write-Host "다 찍었으면  .\pbox_scene.ps1 clean  으로 되돌릴 것." -ForegroundColor Green
Write-Host ""

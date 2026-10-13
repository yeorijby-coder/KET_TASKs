# 트윈 입고 -> 트윈 출고 를 계속 반복한다. (상위 HOST 와 피킹 작업자 역할을 이 스크립트가 한다)
#   필요한 것 : CvSim, ScSim, WCS_TASK_CV, WCS_TASK_SC, IO_TASK_SEMI_FINISH 가 떠 있어야 한다.
#   .\twin_loop.ps1               무한 반복
#   .\twin_loop.ps1 -Cycles 3     세 바퀴만
#   멈추기 : 이 창을 닫거나 Ctrl+C. 멈춘 뒤 남은 것은 -Clean 으로 지운다.
param([int]$Cycles = 0, [int]$Wait = 240, [switch]$Clean)
$ErrorActionPreference = 'Stop'
$TOOLS = Split-Path -Parent $MyInvocation.MyCommand.Path
$P = 'C:\Program Files\PostgreSQL\16\bin\psql.exe'
$env:PGPASSWORD = 'KET_WCS'; $env:PGCLIENTENCODING = 'UTF8'
$USER = 'TWINLOOP'
function Sql([string]$q) { & $P -h localhost -p 5432 -U KET_WCS -d KET_WCS -q -c $q; if ($LASTEXITCODE -ne 0) { throw $q } }
function Ask([string]$q) { (& $P -h localhost -p 5432 -U KET_WCS -d KET_WCS -A -t -c $q) -join "" }
function PlcW([int]$addr, $vals) { & "$TOOLS\plc_write.ps1" -Port 9402 -Addr $addr -Values $vals | Out-Null }
function Now { Get-Date -Format 'HH:mm:ss' }
function Say([string]$s) { Write-Host ("[{0}] {1}" -f (Now), $s) }

# 트랙 -> D 주소 : (트랙-400)*5.  419=95, 420=100, 410=50, 411=55, 415=75
$A419 = 95; $A420 = 100; $A410 = 50; $A411 = 55; $A415 = 75
$W_STO = (1 -shl 8) -bor 11      # 상위바이트 작업구분 1=입고, 하위 목적지 11=크레인 11호기

function Clear-Tracks {
  foreach ($a in $A419, $A420, $A410, $A411, $A415) { PlcW $a @(0,0,0,0,0) }
  PlcW 251 0; PlcW 252 0; PlcW 253 0
}
function Clean-All {
  Sql "DELETE FROM job_mst WHERE ins_user_id IN ('$USER','TWINRUN','RUN4B','RAILTEST');"
  Sql "UPDATE sc_data SET complete_rd='0', od_rq_yn='N', itn_lugg_fk1='0', itn_lugg_fk2='0' WHERE sc_no='911';"
  Clear-Tracks
}
if ($Clean) { Clean-All; Say "지웠다."; exit 0 }

$QJOB = "SELECT COALESCE(string_agg(lugg_no||':'||job_status,' ' ORDER BY lugg_no),'-') FROM job_mst WHERE ins_user_id='$USER';"
$QSC  = "SELECT 'useFk='||use_fk_rd||' fk1='||lugg_no_fk1_rd||' fk2='||lugg_no_fk2_rd||' sensor='||sensor_fk_rd FROM sc_data WHERE sc_no='911';"

# 상태가 바뀔 때만 한 줄 찍으며, 조건을 만족하면 true
function Wait-Until([string]$title, [scriptblock]$cond, [int]$sec) {
  $last = ''; $t0 = Get-Date
  while (((Get-Date) - $t0).TotalSeconds -lt $sec) {
    $j = Ask $QJOB; $s = Ask $QSC
    $now = "$j | $s"
    if ($now -ne $last) { Say ("  {0,-14} {1}" -f $title, $now); $last = $now }
    if (& $cond $j) { return $true }
    Start-Sleep -Milliseconds 1500
  }
  Say "  $title : $sec 초 안에 안 끝남"; return $false
}

Clean-All
Start-Sleep -Seconds 3
$n = 0; $lugg = 7000
while ($true) {
  $n++
  if ($Cycles -gt 0 -and $n -gt $Cycles) { break }
  $bay = 20 + ($n % 10)
  $loc1 = ('01-{0:000}-01' -f $bay); $loc2 = ('02-{0:000}-01' -f $bay)
  $l1 = $lugg + 1; $l2 = $lugg + 2; $l3 = $lugg + 3; $l4 = $lugg + 4; $lugg += 10
  Say "===== $n 바퀴  입고 $l1/$l2 -> $loc1/$loc2  ->  출고 $l3/$l4 ====="

  # --- 트윈 입고 : 화물 두 개가 먼저 서고, 상위가 입고 작업을 주고, CV 태스크처럼 420 -> 419 순으로 쓴다
  PlcW $A420 @(0,0,1,0,0); PlcW $A419 @(0,0,1,0,0)
  Start-Sleep -Seconds 3
  Sql "INSERT INTO job_mst (wh_typ,lugg_no,start_pos,start_location,dest_pos,dest_location,job_typ,job_status,job_priority,hs_track_no,pair_lugg_no,ins_dt,ins_user_id,upd_dt,upd_user_id) VALUES ('20','$l1','222','00-000-00','911','$loc1','1','11','5','','',NOW(),'$USER',NOW(),'$USER'),('20','$l2','221','00-000-00','911','$loc2','1','11','5','','',NOW(),'$USER',NOW(),'$USER');"
  PlcW $A420 @($l1,$W_STO,1,0,0)
  Start-Sleep -Seconds 3
  PlcW $A419 @($l2,$W_STO,1,0,0)
  $ok = Wait-Until "트윈 입고" { param($j) ($j -eq "${l1}:29 ${l2}:29") } $Wait
  # 상위가 완료보고를 받았다고 치고 지운다
  Sql "DELETE FROM job_mst WHERE ins_user_id='$USER';"
  Sql "UPDATE sc_data SET complete_rd='0', od_rq_yn='N' WHERE sc_no='911';"
  if (-not $ok) { Clear-Tracks; Start-Sleep 3; continue }
  Start-Sleep -Seconds 3

  # --- 트윈 출고 : 방금 넣은 두 자리에서 피킹대(251)로. 두 건을 한 문장으로 넣는다
  Sql "INSERT INTO job_mst (wh_typ,lugg_no,start_pos,start_location,dest_pos,dest_location,job_typ,job_status,job_priority,hs_track_no,pair_lugg_no,ins_dt,ins_user_id,upd_dt,upd_user_id) VALUES ('20','$l3','911','$loc1','251','00-000-00','2','20','5','','',NOW(),'$USER',NOW(),'$USER'),('20','$l4','911','$loc2','251','00-000-00','2','20','5','','',NOW(),'$USER',NOW(),'$USER');"
  # 크레인이 내려놓고(410/411) 컨베이어가 피킹대까지 가져가 도착완료(19)가 되거나, 둘 다 CV 로 넘어간(11/19) 뒤 20초를 준다
  $ok = Wait-Until "트윈 출고" { param($j) ($j -match "^${l3}:(19|11) ${l4}:(19|11)$") } $Wait
  if ($ok) { Start-Sleep -Seconds 25; Say ("  {0,-14} {1}" -f "트윈 출고", (Ask $QJOB)) }
  # 피킹 작업자가 피킹대에서 꺼냈다고 치고 자리를 비우고 작업을 지운다
  Sql "DELETE FROM job_mst WHERE ins_user_id='$USER';"
  Sql "UPDATE sc_data SET complete_rd='0', od_rq_yn='N' WHERE sc_no='911';"
  Clear-Tracks
  Start-Sleep -Seconds 5
}
Say "끝."

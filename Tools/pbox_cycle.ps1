# P-BOX 순환 : 이동(251->221) -> H/S 도착완료 -> 입고 요구/지시 -> 입고 완료 -> 출고 -> 피킹대 도착 -> 다시 이동
#   이동/입고/출고 작업을 CV·크레인에 쓰는 것은 실제 스케줄러(IO)·CV·SC 태스크가 한다.
#   이 스크립트는 상위(HOST : 작업 생성·완료보고)와 피킹 작업자(화물 두 개) 역할만 한다.
#   필요한 것 : CvSim, ScSim, WCS_TASK_CV, WCS_TASK_SC, IO_TASK_SEMI_FINISH
#   .\pbox_cycle.ps1          계속 돈다 (Ctrl+C / 창 닫기)
#   .\pbox_cycle.ps1 -Clean   남은 작업·화물을 지운다
#   .\pbox_cycle.ps1 -Resume  지우지 않고 지금 상태에서 이어 돈다
param([switch]$Clean, [switch]$Resume, [int]$Boxes = 2)
$ErrorActionPreference = 'Stop'
$TOOLS = Split-Path -Parent $MyInvocation.MyCommand.Path
$P = 'C:\Program Files\PostgreSQL\16\bin\psql.exe'
$env:PGPASSWORD = 'KET_WCS'; $env:PGCLIENTENCODING = 'UTF8'
$USER = 'PBOXCYC'
function Sql([string]$q) { & $P -h localhost -p 5432 -U KET_WCS -d KET_WCS -q -c $q; if ($LASTEXITCODE -ne 0) { throw $q } }
function Ask([string]$q) { @(& $P -h localhost -p 5432 -U KET_WCS -d KET_WCS -A -t -c $q) | Where-Object { $_ -ne '' } }
function PlcW([int]$addr, $vals) { & "$TOOLS\plc_write.ps1" -Port 9402 -Addr $addr -Values $vals | Out-Null }
function Say([string]$s) { Write-Host ("[{0}] {1}" -f (Get-Date -Format 'HH:mm:ss'), $s) }

# 트랙 -> D 주소 : (트랙-400)*5
function A([int]$tr) { return ($tr - 400) * 5 }
$W_RET251 = (2 -shl 8) -bor 251          # 출고 화물이 피킹대에 선 모양 (작업구분 2, 목적지 251)

function Clear-Tracks {
  foreach ($tr in 410,411,412,413,414,415,416,417,418,419,420) { PlcW (A $tr) @(0,0,0,0,0) }
  PlcW 251 0; PlcW 252 0; PlcW 253 0
}
function Clean-All {
  Sql "DELETE FROM job_mst WHERE ins_user_id IN ('$USER','TWINLOOP','TWINRUN','RUN4B','RAILTEST');"
  Sql "UPDATE sc_data SET complete_rd='0', od_rq_yn='N', itn_lugg_fk1='0', itn_lugg_fk2='0' WHERE sc_no='911';"
  Clear-Tracks
}
if ($Clean) { Clean-All; Say "지웠다."; exit 0 }

$INS = "INSERT INTO job_mst (wh_typ,lugg_no,start_pos,start_location,dest_pos,dest_location,job_typ,job_status,job_priority,hs_track_no,pair_lugg_no,ins_dt,ins_user_id,upd_dt,upd_user_id) VALUES "
function Row($l,$sp,$sl,$dp,$dl,$jt,$st) { return "('20','$l','$sp','$sl','$dp','$dl','$jt','$st','5','','',NOW(),'$USER',NOW(),'$USER')" }

$script:lugg = 7000
function NextLugg { $script:lugg++; if ($script:lugg -ge 7999) { $script:lugg = 7001 }; return $script:lugg }
$script:cyc = 0
function NextLocPair { $script:cyc++; $bay = 20 + ($script:cyc % 10); return @(('01-{0:000}-01' -f $bay), ('02-{0:000}-01' -f $bay)) }

if ($Resume) {
  $m = (Ask "SELECT COALESCE(MAX(lugg_no::int),7000) FROM job_mst WHERE ins_user_id='$USER' AND lugg_no ~ '^7[0-9]{3}$';") -join ''
  if ($m) { $script:lugg = [int]$m }
  Say "이어서 돈다. 작업번호는 $($script:lugg) 다음부터."
} else {
  Clean-All
  Start-Sleep -Seconds 3
  # 피킹 작업자 : 화물 두 개를 피킹대 줄에 세워 둔다 (415 에 하나, 414 에 하나가 뒤에서 기다림)
  PlcW (A 415) @(9001,$W_RET251,1,0,0)
  if ($Boxes -ge 2) { Start-Sleep -Seconds 2; PlcW (A 414) @(9002,$W_RET251,1,0,0) }
  Say "화물 $Boxes 개를 피킹대(415) 줄에 세웠다. 순환을 시작한다."
}

$stored = New-Object System.Collections.ArrayList   # 입고가 끝난 랙 위치
$locs = NextLocPair; $locIdx = 0
$last = ''
while ($true) {
  # ── 상태 한 줄 (바뀔 때만)
  $jobs = (Ask "SELECT COALESCE(string_agg(lugg_no||'/'||job_typ||':'||job_status||'@'||start_pos||'>'||dest_pos,' ' ORDER BY lugg_no),'-') FROM job_mst WHERE ins_user_id='$USER';") -join ''
  $cv   = (Ask "SELECT string_agg(mc_no||':'||COALESCE(NULLIF(lugg_no_rd,'0000'),'-')||'/s'||sensor0_data_rd||'/r'||COALESCE(ret_ready_rd,'-')||'/h'||COALESCE(stohs_ready_rd,'-'),' ' ORDER BY mc_no) FROM cv_data WHERE plc_no='04' AND mc_no IN ('415','419','420','410','411');") -join ''
  $sc   = (Ask "SELECT 'fk='||use_fk_rd||' '||lugg_no_fk1_rd||'/'||lugg_no_fk2_rd||' s'||sensor_fk_rd FROM sc_data WHERE sc_no='911';") -join ''
  $now = "JOB $jobs | CV $cv | SC $sc"
  if ($now -ne $last) { Say $now; $last = $now }

  # ── R0 : 도착완료(19)된 출고 작업 -> 상위 완료보고 받은 셈치고 지운다 (피킹대의 화물은 그대로 서 있다)
  foreach ($r in (Ask "SELECT lugg_no FROM job_mst WHERE ins_user_id='$USER' AND job_typ='2' AND job_status='19';")) {
    Sql "DELETE FROM job_mst WHERE lugg_no='$r';"; Say "  출고 $r 피킹대 도착 -> 완료보고"
  }

  # ── R1 : 피킹대(415)에 화물이 서 있고(준비 ON) 그 화물에 작업이 없으면 -> 이동 작업(251 -> 221)
  $r1 = Ask "SELECT cd.lugg_no_rd FROM cv_data cd WHERE cd.plc_no='04' AND cd.mc_no='415' AND cd.sensor0_data_rd='1' AND cd.ret_ready_rd='1' AND cd.od_rq_yn='N' AND NOT EXISTS (SELECT 1 FROM job_mst j WHERE j.lugg_no=cd.lugg_no_rd) AND NOT EXISTS (SELECT 1 FROM job_mst j WHERE j.start_pos='251' AND j.job_status='10');"   # 아직 CV 에 안 쓰인 이동만 겹침으로 본다. 이미 가고 있는 것(11)은 다음 화물을 막지 않는다
  if ($r1) {
    $l = NextLugg
    Sql ($INS + (Row $l '251' '00-000-00' '221' '00-000-00' '6' '10') + ";")
    Say "  이동 $l : 251(415) -> 221  지시 (화물 $($r1 -join ''))"
  }

  # ── R2 : 이동 작업이 H/S 에 도착완료(19) -> 상위 완료보고 + 입고 요구 -> 입고 작업 지시 (그 자리에서 911 로)
  foreach ($row in (Ask "SELECT lugg_no||'|'||dest_pos FROM job_mst WHERE ins_user_id='$USER' AND job_typ='6' AND job_status='19';")) {
    $parts = $row.Split('|'); $old = $parts[0]; $stn = $parts[1]
    if ($locIdx -ge 2) { $locs = NextLocPair; $locIdx = 0 }
    $loc = $locs[$locIdx]; $locIdx++
    $l = NextLugg
    Sql "DELETE FROM job_mst WHERE lugg_no='$old';"
    Sql ($INS + (Row $l $stn '00-000-00' '911' $loc '1' '10') + ";")
    Say "  이동 $old 도착완료($stn) -> 입고 요구 -> 입고 $l : $stn -> 911 $loc"
  }

  # ── R3 : 입고 작업이 크레인 완료(29) -> 완료보고, 랙에 쌓인 것으로 기억
  foreach ($row in (Ask "SELECT lugg_no||'|'||dest_location FROM job_mst WHERE ins_user_id='$USER' AND job_typ='1' AND job_status='29';")) {
    $parts = $row.Split('|')
    Sql "DELETE FROM job_mst WHERE lugg_no='$($parts[0])';"
    [void]$stored.Add($parts[1]); Say "  입고 $($parts[0]) 완료 -> 랙 $($parts[1])  (쌓인 것 $($stored.Count))"
  }

  # ── R4 : 두 개가 다 쌓였고 다른 작업이 없으면 -> 출고 두 건 (트윈) 피킹대로
  $active = (Ask "SELECT COUNT(1) FROM job_mst WHERE ins_user_id='$USER';") -join ''
  if ($stored.Count -ge $Boxes -and $active -eq '0') {
    $rows = @(); foreach ($loc in $stored) { $l = NextLugg; $rows += (Row $l '911' $loc '251' '00-000-00' '2' '20') }
    Sql ($INS + ($rows -join ',') + ";")
    Say "  출고 $($stored.Count) 건 지시 : $($stored -join ', ') -> 251"
    $stored.Clear()
  }
  Start-Sleep -Milliseconds 1500
}

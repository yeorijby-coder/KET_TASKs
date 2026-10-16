# =====================================================================
#  1층 RGV + 지게차 대역 (시뮬레이터 전용)  2026-10-16
# =====================================================================
#
#  왜 필요한가
#    1층 출고는 크레인 출고 H/S -> 259(155, RGV 중간 도착대) -> RGV -> 출고대(103/104/105) 로 간다.
#    RGV(RTV) 태스크와 RGV 시뮬레이터가 없어 화물이 259 에서 영영 선다.
#    또 출고대에 내린 파렛트는 현장에서 지게차가 가져가고, 입고대(101)에 새 파렛트를 올린다.
#    이 둘을 PLC 쓰기로 대신한다. (CvSim PLC 02, 포트 9202)
#
#  하는 일 (3초마다)
#    1) 259 에 화물+작업번호가 있고 그 작업(JOB_MST)의 목적지가 103/104 면
#       259 를 비우고 목적지 트랙(103=206, 104=212)에 같은 작업번호/목적지/구분 2 로 올린다.
#    2) 206/212 에 화물이 있는데 그 작업번호가 JOB_MST 에 없으면(완료보고 끝)
#       그 자리를 비우고 입고대 101(217)이 비어 있으면 거기에 빈 파렛트(작업번호 0)를 올린다.
#
#  쓰는 법
#    .\rgv_forklift_sim.ps1            (Ctrl+C 로 중단)
#    .\rgv_forklift_sim.ps1 -Once      한 번만
# =====================================================================
param([switch]$Once, [int]$IntervalSec = 3)

$ErrorActionPreference = 'Continue'
$HERE = Split-Path -Parent $MyInvocation.MyCommand.Path
$W    = Join-Path $HERE 'plc_write.ps1'
$P    = 'C:\Program Files\PostgreSQL\16\bin\psql.exe'
$env:PGPASSWORD = 'KET_WCS'; $env:PGCLIENTENCODING = 'UTF8'
$PORT = 9202                                   # CvSim PLC 02 (1층 구라인)
function Addr([int]$trk) { return ($trk - 200) * 5 }
function Ask([string]$q) { $o = (& $P -h localhost -U KET_WCS -d KET_WCS -A -t -c $q); if ($null -eq $o) { return "" } ; return (($o | % { "$_" }) -join "`n") }
function PlcW([int]$addr, $vals) { & $W -Port $PORT -Addr $addr -Values $vals | Out-Null }
function Say([string]$m) { Write-Host ("[{0}] {1}" -f (Get-Date -Format 'HH:mm:ss'), $m) }

function DestTrack([int]$d) { switch ($d) { 103 { return 206 } 104 { return 212 } default { return 0 } } }   # 출고대 -> 트랙
$DEST_LIST = @(103, 104)
$STO_TRACK = 217                               # 입고대 101

do {
  try {
    # 1) RGV : 259(155) -> 출고대
    $rows = Ask "SELECT cd.lugg_no_rd, jm.dest_pos, jm.job_typ FROM cv_data cd JOIN job_mst jm ON jm.wh_typ=cd.wh_typ AND jm.lugg_no=cd.lugg_no_rd WHERE cd.plc_no='02' AND cd.mc_no='259' AND cd.sensor0_data_rd='1' AND cd.lugg_no_rd NOT IN ('0','0000') AND jm.job_status='11' AND jm.dest_pos IN ('103','104');"
    foreach ($r in ("$rows" -split "`n")) {
      if ([string]::IsNullOrWhiteSpace($r)) { continue }
      $f = $r.Trim().Split('|'); $lugg = [int]$f[0].Trim(); $dest = [int]$f[1].Trim(); $typ = [int]$f[2].Trim()
      $trk = DestTrack $dest
      if ($trk -eq 0) { Say "RGV: 모르는 출고대 [$dest] (행: $r)"; continue }
      $busy = Ask "SELECT sensor0_data_rd FROM cv_data WHERE plc_no='02' AND mc_no='$trk';"
      if ($busy.Trim() -eq '1') { Say "RGV: 출고대 $dest($trk) 가 차 있어 $lugg 을 기다린다"; continue }
      PlcW (Addr 259) @(0,0,0,0,0)
      Start-Sleep -Milliseconds 500
      PlcW (Addr $trk) @($lugg, (($typ -shl 8) -bor [int]$dest), 1, 0, 0)
      Say "RGV: 259 -> $dest($trk)  작업번호 $lugg"
    }

    # 2) 지게차 : 출고대 -> 입고대 101
    foreach ($dest in $DEST_LIST) {
      $trk = DestTrack $dest
      $r = Ask "SELECT cd.lugg_no_rd FROM cv_data cd WHERE cd.plc_no='02' AND cd.mc_no='$trk' AND cd.sensor0_data_rd='1' AND cd.lugg_no_rd NOT IN ('0','0000') AND NOT EXISTS (SELECT 1 FROM job_mst jm WHERE jm.wh_typ=cd.wh_typ AND jm.lugg_no=cd.lugg_no_rd) AND cd.od_rq_yn='N';"
      if ([string]::IsNullOrWhiteSpace($r)) { continue }
      $lugg = ("$r".Trim() -split "`n")[0].Trim()
      $sto = Ask "SELECT sensor0_data_rd FROM cv_data WHERE plc_no='02' AND mc_no='$STO_TRACK';"
      if ($sto.Trim() -eq '1') { Say "지게차: 입고대 101 이 차 있어 $lugg 을 기다린다"; continue }
      PlcW (Addr $trk) @(0,0,0,0,0)
      Start-Sleep -Milliseconds 500
      PlcW (Addr $STO_TRACK) @(0,0,1,0,0)
      Say "지게차: $dest($trk) 의 파렛트($lugg) 를 101($STO_TRACK) 로 옮겼다 (빈 파렛트로)"
    }
  }
  catch { Say ("오류: " + $_.Exception.Message + " @ line " + $_.InvocationInfo.ScriptLineNumber + ": " + $_.InvocationInfo.Line.Trim()) }
  if (-not $Once) { Start-Sleep -Seconds $IntervalSec }
} while (-not $Once)

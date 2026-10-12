# =====================================================================
#  P-BOX 입고 H/S 에 박스를 올려 두거나 걷어낸다 (시뮬레이터 전용)
#  2026-10-13
# =====================================================================
#
#  크레인이 집어 가는 것, 출고 H/S 에 내려놓고 화물감지를 켜는 것은
#  ScSim 이 알아서 한다. 여기서 하는 일은 하나뿐이다 —
#  "컨베이어를 타고 온 박스가 입고 H/S 에 도착한 순간" 을 만들어 준다.
#  현장에서는 광전센서가 하는 일이다.
#
#  쓰는 법
#    .\pbox_hs_box.ps1 -Lugg1 7801                 포크#1 자리(420)에만
#    .\pbox_hs_box.ps1 -Lugg1 7801 -Lugg2 7802     트윈 (420 + 419)
#    .\pbox_hs_box.ps1 -Clear                      두 자리 다 비운다
#
#  올려 둔 뒤에는 JOB_MST 에 그 작업번호가 상태 11(CV 구동중)로 있어야
#  크레인이 받아 간다. IO_SCH 의 ScStoreRoutine 이 그 상태만 본다.
# =====================================================================
param([string]$Lugg1 = '', [string]$Lugg2 = '', [switch]$Clear)

$ErrorActionPreference = 'Stop'
$HERE = Split-Path -Parent $MyInvocation.MyCommand.Path
$W    = Join-Path $HERE 'plc_write.ps1'

$PORT     = 9402       # CvSim PLC 04 (P-BOX)
$ADDR_420 = 100        # 입고 H/S 포크#1 (작업대 222) : (420-400)*5
$ADDR_419 = 95         # 입고 H/S 포크#2 (작업대 221) : (419-400)*5
$ADDR_RDY = 252        # 입고 H/S 준비 비트 : .0=419  .1=420

# D5n+1 = (상위)작업구분 1=입고  (하위)도착지코드 11 = 크레인 11호기
#   도착지코드는 DeviceMap04.xml 의 UseScDest(from=1 to=11) 를 따른다.
$W1 = (1 -shl 8) -bor 11

# CvSim 은 PLC 한 자리에 소켓 하나만 둔다. 우리가 붙으면 그 자리에 있던
# ScSim 이 밀려난다. ScSim 은 몇 초 뒤 스스로 다시 붙으므로, 다 쓰고 나서
# 그게 돌아올 때까지 기다려 준다. 안 기다리면 크레인이 화물을 못 집어 간다.
function Wait-ScSim([int]$sec = 40) {
  $sc = Get-Process -Name ScSim -ErrorAction SilentlyContinue
  if ($sc -eq $null) { return }
  $t0 = Get-Date
  while (((Get-Date) - $t0).TotalSeconds -lt $sec) {
    $hit = netstat -ano | Select-String ":$PORT\s" | Select-String "ESTABLISHED" |
           Where-Object { $_ -match "\s$($sc.Id)\s*$" }
    if ($hit) { "ScSim 이 다시 붙었다."; return }
    Start-Sleep -Milliseconds 700
  }
  "ScSim 이 아직 안 붙었다. 잠시 뒤 다시 보라." 
}

if ($Clear) {
  & $W -Port $PORT -Addr $ADDR_419 -Values 0,0,0,0,0
  & $W -Port $PORT -Addr $ADDR_420 -Values 0,0,0,0,0
  & $W -Port $PORT -Addr $ADDR_RDY -Values 0
  "입고 H/S 420 / 419 를 비웠다."
  Wait-ScSim
  return
}

if ($Lugg1 -eq '') { throw "작업번호를 달라. -Lugg1 7801 [-Lugg2 7802]" }

# 두 자리에 화물을 먼저 올리고, 둘 다 올라온 것을 확인한 뒤에 준비 비트를 준다.
#   준비 비트가 먼저 서면, 아직 한쪽만 올라온 순간에 IO_SCH 가 보고
#   싱글로 지시해 버린다. 그러면 나머지 한 건이 뒤에 남는다.
if ($Lugg2 -ne '') {
  & $W -Port $PORT -Addr $ADDR_419 -Values ([int]$Lugg2),$W1,1,0,0
}
& $W -Port $PORT -Addr $ADDR_420 -Values ([int]$Lugg1),$W1,1,0,0

# 두 자리가 다 올라왔는지 DB 로 확인한다 (WCS_TASK_CV 가 PLC 에서 읽어 넣는다)
$PSQL = 'C:\Program Files\PostgreSQL\16\bin\psql.exe'
$env:PGPASSWORD = 'KET_WCS'
$want = if ($Lugg2 -ne '') { "'420','419'" } else { "'420'" }
$t0 = Get-Date
while (((Get-Date) - $t0).TotalSeconds -lt 20) {
  $n = (& $PSQL -h localhost -p 5432 -U KET_WCS -d KET_WCS -A -t `
        -c "SELECT count(*) FROM cv_data WHERE plc_no='04' AND mc_no IN ($want) AND sensor0_data_rd='1';") -join ''
  if ([int]$n -ge $(if ($Lugg2 -ne '') { 2 } else { 1 })) { break }
  Start-Sleep -Milliseconds 500
}

& $W -Port $PORT -Addr $ADDR_RDY -Values $(if ($Lugg2 -ne '') { 3 } else { 2 })

if ($Lugg2 -ne '') { "입고 H/S 420 <- $Lugg1 (포크#1), 419 <- $Lugg2 (포크#2). 트윈으로 나간다." }
else               { "입고 H/S 420 <- $Lugg1 (포크#1). 싱글로 나간다." }

Wait-ScSim

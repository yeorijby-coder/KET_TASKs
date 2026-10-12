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
param([string]$Lugg1 = '', [string]$Lugg2 = '', [switch]$Clear, [switch]$Ready)

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

# 안쪽(420)부터 올린다. 바깥(419)에 먼저 올리면 안쪽이 비어 있는 동안
# 컨베이어가 그 화물을 안쪽으로 흘려보낸다(현장 PLC 와 같은 동작).
#
# 준비 비트(D252)는 손대지 않는다. CvSim 이 짝을 기다렸다가 스스로 올린다.
#   EcsDefine.xml <TwinHS pair="419" master="1" wait="60"/>
#   -Ready 를 주면 예전처럼 곧바로 올린다(대기 없이 싱글로 보내고 싶을 때).
& $W -Port $PORT -Addr $ADDR_420 -Values ([int]$Lugg1),$W1,1,0,0

if ($Lugg2 -ne '') {
  Start-Sleep -Seconds 2
  & $W -Port $PORT -Addr $ADDR_419 -Values ([int]$Lugg2),$W1,1,0,0
}

if ($Ready) {
  & $W -Port $PORT -Addr $ADDR_RDY -Values $(if ($Lugg2 -ne '') { 3 } else { 2 })
}

if ($Lugg2 -ne '') { "입고 H/S 420 <- $Lugg1 (포크#1), 419 <- $Lugg2 (포크#2)" }
else               { "입고 H/S 420 <- $Lugg1 (포크#1)" }
if (-not $Ready)   { "준비 비트는 CvSim 이 올린다. 짝이 없으면 60초 뒤 싱글로 선다." }

Wait-ScSim

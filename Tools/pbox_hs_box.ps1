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

if ($Clear) {
  & $W -Port $PORT -Addr $ADDR_419 -Values 0,0,0,0,0
  & $W -Port $PORT -Addr $ADDR_420 -Values 0,0,0,0,0
  & $W -Port $PORT -Addr $ADDR_RDY -Values 0
  "입고 H/S 420 / 419 를 비웠다."
  return
}

if ($Lugg1 -eq '') { throw "작업번호를 달라. -Lugg1 7801 [-Lugg2 7802]" }

if ($Lugg2 -ne '') {
  # 포크#2 자리는 포크#1 자리가 찬 뒤에 쓰는 뒷자리다. 419 만 채우면 안 된다.
  & $W -Port $PORT -Addr $ADDR_419 -Values ([int]$Lugg2),$W1,1,0,0
}
& $W -Port $PORT -Addr $ADDR_420 -Values ([int]$Lugg1),$W1,1,0,0
& $W -Port $PORT -Addr $ADDR_RDY -Values $(if ($Lugg2 -ne '') { 3 } else { 2 })

if ($Lugg2 -ne '') { "입고 H/S 420 <- $Lugg1 (포크#1), 419 <- $Lugg2 (포크#2). 트윈으로 나간다." }
else               { "입고 H/S 420 <- $Lugg1 (포크#1). 싱글로 나간다." }

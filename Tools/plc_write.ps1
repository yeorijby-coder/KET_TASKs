# =====================================================================
#  시뮬레이터 PLC 에 워드를 직접 쓴다 (Melsec Q 3E 바이너리)
#  2026-10-13
# =====================================================================
#
#  왜 필요한가
#    크레인과 컨베이어 사이의 화물 인계는 ScSim 이 직접 한다.
#    그런데 "맨 처음 화물이 입고 H/S 에 올라오는 것" 만은 아무도 못 한다.
#    현장에서는 컨베이어를 타고 온 박스를 광전센서가 잡아 주지만,
#    시뮬레이터만 띄워 놓고 크레인부터 시험할 때는 그 자리에 박스가 없다.
#    그 한 가지를 손으로 올려 주는 도구다.
#
#  쓰는 법
#    .\plc_write.ps1 -Port 9402 -Addr 102 -Values 1
#    .\plc_write.ps1 -Port 9402 -Addr 100 -Values 7801,267,1,0,0
#
#  포트
#    CvSim  PLC n 은 9n02   (P-BOX 는 PLC 04 -> 9402)
#    주소는 DeviceMap0n.xml 을 따른다. 트랙당 5워드인 판은
#      D5n+0 작업번호 / D5n+1 (상위 작업구분, 하위 도착지코드) / D5n+2 화물감지
#    예) 트랙 420 은 (420-400)*5 = 100 번지부터
# =====================================================================
param([int]$Port = 9402, [string]$PlcHost = '127.0.0.1',
      [Parameter(Mandatory=$true)][int]$Addr,
      [Parameter(Mandatory=$true)][int[]]$Values)

$pts  = $Values.Count
$body = New-Object System.Collections.Generic.List[byte]
$body.AddRange([byte[]]@(0x10,0x00))   # CPU 감시 타이머
$body.AddRange([byte[]]@(0x01,0x14))   # 명령 0x1401 = 일괄 쓰기
$body.AddRange([byte[]]@(0x00,0x00))   # 서브명령 (워드 단위)
$body.AddRange([byte[]]@([byte]($Addr -band 0xFF), [byte](($Addr -shr 8) -band 0xFF), [byte](($Addr -shr 16) -band 0xFF)))
$body.Add([byte]0xA8)                  # 디바이스 D
$body.AddRange([byte[]]@([byte]($pts -band 0xFF), [byte](($pts -shr 8) -band 0xFF)))
foreach ($v in $Values) {
  $body.AddRange([byte[]]@([byte]($v -band 0xFF), [byte](($v -shr 8) -band 0xFF)))
}

$len  = $body.Count            # 길이는 타이머부터 끝까지
$head = [byte[]]@(0x50,0x00, 0x00, 0xFF, 0xFF,0x03, 0x00,
                  [byte]($len -band 0xFF), [byte](($len -shr 8) -band 0xFF))
$frame = New-Object byte[] ($head.Count + $body.Count)
[Array]::Copy($head, 0, $frame, 0, $head.Count)
$body.CopyTo($frame, $head.Count)

$cli = New-Object System.Net.Sockets.TcpClient
try   { $cli.Connect($PlcHost, $Port) }
catch { throw "PLC($PlcHost : $Port) 에 못 붙었다. 시뮬레이터가 떠 있는지 보라." }
$st = $cli.GetStream()
$st.Write($frame, 0, $frame.Length)
$st.Flush()
# 응답이 바로 안 올 때가 있다. 1초까지 기다려 본다.
$buf = New-Object byte[] 256
$n = 0
for ($i = 0; $i -lt 10; $i++) {
  Start-Sleep -Milliseconds 100
  if ($st.DataAvailable) { $n = $st.Read($buf, 0, 256); break }
}
$st.Close(); $cli.Close()

# 응답 끝 두 바이트가 종료코드다. 0000 이면 정상.
$ok = ($n -ge 11) -and ($buf[9] -eq 0) -and ($buf[10] -eq 0)
"D$Addr <- $($Values -join ',')   " + $(if ($ok) { "정상" } else { "응답이상 ($n 바이트)" })

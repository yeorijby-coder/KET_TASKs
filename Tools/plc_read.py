# 시뮬레이터 PLC(Melsec Q3E 바이너리)에서 D 워드를 읽어 찍는다.
#   python plc_read.py <port> <addr> <count>
#   예) python plc_read.py 9402 252 2      -> D252(입고 H/S 준비), D253(출고 H/S 준비)
import socket, struct, sys

port, addr, cnt = int(sys.argv[1]), int(sys.argv[2]), int(sys.argv[3])
body = struct.pack("<HHH", 0x0010, 0x0401, 0x0000) + bytes([addr & 0xFF, (addr >> 8) & 0xFF, (addr >> 16) & 0xFF, 0xA8]) + struct.pack("<H", cnt)
head = bytes([0x50, 0x00, 0x00, 0xFF, 0xFF, 0x03, 0x00]) + struct.pack("<H", len(body))
s = socket.create_connection(("127.0.0.1", port), timeout=5)
s.sendall(head + body)
rx = b""
while len(rx) < 11:
    d = s.recv(1024)
    if not d:
        break
    rx += d
s.close()
if len(rx) < 11:
    print("no reply"); sys.exit(1)
end = struct.unpack("<H", rx[9:11])[0]
if end != 0:
    print("err %04X" % end); sys.exit(1)
words = struct.unpack("<%dH" % cnt, rx[11:11 + 2 * cnt])
for i, w in enumerate(words):
    print("D%d = %d (0x%04X)" % (addr + i, w, w))

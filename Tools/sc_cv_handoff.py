# -*- coding: utf-8 -*-
"""
크레인과 컨베이어 사이의 화물 인계를 대신해 주는 시험 하니스.

왜 필요한가
    ScSim 은 크레인 동작과 완료 신호까지는 이 현장대로 흉내 내지만,
    크레인이 컨베이어 H/S 에서 화물을 집거나 내려놓는 것은 하지 않는다.
      - ScSim 의 EcsDefine.xml 은 다른 현장 트랙 번호(101~1xx)로 되어 있고
      - 거기서 만드는 ScStoHS / ScRetHS 객체는 어느 코드에서도 쓰이지 않으며
      - CV 연결 설정([CV02]~[CV06])이 자기 설비 선언(CV_E01)과도 어긋난다
    설정이 틀린 게 아니라 이 현장용으로 구현되어 있지 않다.

    그래서 WCS 는 입고를 완료 처리하는데 CvSim 의 입고 H/S 에는 화물이 남고,
    출고는 크레인이 완료해도 출고 H/S 에 화물이 나타나지 않아 순환이 끊긴다.
    WCS 로직 문제가 아니라 시뮬레이터 설비 모델의 결손이다.

    이 스크립트가 그 인계만 대신한다. 실제 PLC 가 할 일을 흉내 내는 것이라
    WCS 코드에는 손대지 않는다. ScSim 이 이 현장으로 이관되면 필요 없어진다.

하는 일
    입고 완료 (SC_DATA.JOB_TYP_RD=1, COMPLETE_RD<>0)
        크레인이 집어간 작번을 아직 들고 있는 입고 H/S 트랙을 비운다.
        입고 H/S 는 STOHS_READY_RD='1' 과 작번 일치로 찾는다.
        (SC_HS_DEF 에 입고 H/S(HS_NO='01') 행은 905호기밖에 없어서 쓸 수 없다)

    출고 완료 (SC_DATA.JOB_TYP_RD=2, COMPLETE_RD<>0)
        그 호기의 출고 H/S(SC_HS_DEF HS_NO='02')에 화물을 놓는다.

    크레인 활성 유지
        ScSim 은 포크 데이터를 지우면 크레인을 INACTIVE 로 내려 다음 작업을
        진행하지 않는다. 실제 현장처럼 활성으로 되돌린다.

    시뮬레이터 레지스터는 Melsec Q3E 로 직접 쓴다 (CvSim 9n01, ScSim 81nn).

쓰는 법
    python sc_cv_handoff.py            한 번만 훑고 끝낸다
    python sc_cv_handoff.py --loop     계속 돌면서 인계한다 (Ctrl+C 로 종료)
"""
import argparse
import io
import os
import re
import socket
import struct
import subprocess
import sys
import time

DB_HOST = '127.0.0.1'
DB_NAME = 'KET_WCS'
DB_USER = 'KET_WCS'
DB_PASS = 'KET_WCS'
PSQL = r'C:\Program Files\PostgreSQL\16\bin\psql.exe'

WH_TYP = '10'
WORDS_PER_TRACK = 5          # CvSim 의 트랙당 워드 수
HS_NO_RETRIEVE = '02'        # SC_HS_DEF : 출고 H/S (1층)


# ── CvSim 레지스터 주소 계산 ────────────────────────────────────────────
#   트랙 번호 앞자리가 곧 PLC 번호다 (2xx -> 02).
#   CvSim 의 접속 포트는 Ecs.ini [CV_E0n] Port1 = 9n01,
#   트랙 워드 시작 주소는 EcsDefine 의 FirstTrack(=n01) 기준이다.
def plc_of(track):
    return int(str(track)[0])


def port_of(track):
    return 9000 + plc_of(track) * 100 + 1


def base_of(track):
    first = plc_of(track) * 100 + 1
    return (int(track) - first + 1) * WORDS_PER_TRACK


def _q3e(port, body):
    head = b'\x50\x00\x00\xFF\xFF\x03\x00' + struct.pack('<H', len(body))
    s = socket.create_connection(('127.0.0.1', port), timeout=5)
    try:
        s.sendall(head + body)
        rsp = s.recv(4096)
    finally:
        s.close()
    if len(rsp) < 11:
        raise IOError('응답이 짧다: %s' % rsp.hex())
    end = struct.unpack('<H', rsp[9:11])[0]
    if end != 0:
        raise IOError('종료코드 0x%04X' % end)
    return rsp[11:]


def q3e_read(port, addr, count):
    body = struct.pack('<HHH', 0x0010, 0x0401, 0x0000)
    body += struct.pack('<I', addr)[:3] + bytes([0xA8]) + struct.pack('<H', count)
    data = _q3e(port, body)
    n = len(data) // 2
    return list(struct.unpack('<%dH' % n, data[:n * 2]))


def q3e_write(port, addr, values):
    body = struct.pack('<HHH', 0x0010, 0x1401, 0x0000)
    body += struct.pack('<I', addr)[:3] + bytes([0xA8]) + struct.pack('<H', len(values))
    for v in values:
        body += struct.pack('<H', v & 0xFFFF)
    _q3e(port, body)


# ── DB 조회 ─────────────────────────────────────────────────────────────
def query(sql):
    env = dict(os.environ)
    env['PGPASSWORD'] = DB_PASS
    out = subprocess.run([PSQL, '-h', DB_HOST, '-U', DB_USER, '-d', DB_NAME,
                          '-t', '-A', '-F', '|', '-c', sql],
                         capture_output=True, text=True, env=env, encoding='utf-8')
    if out.returncode != 0:
        raise IOError(out.stderr.strip())
    return [tuple(x.strip() for x in ln.split('|'))
            for ln in out.stdout.splitlines() if ln.strip()]


SQL_STORE_DONE = """
SELECT SD.SC_NO, SD.LUGG_NO_FK1_RD, CD.MC_NO
  FROM SC_DATA SD
 INNER JOIN CV_DATA CD
    ON CD.WH_TYP          = SD.WH_TYP
   AND CD.LUGG_NO_RD      = SD.LUGG_NO_FK1_RD
   AND CD.STOHS_READY_RD  = '1'
 WHERE SD.WH_TYP          = '%s'
   AND SD.JOB_TYP_RD      = '1'
   AND COALESCE(SD.COMPLETE_RD,'0') NOT IN ('','0')
   AND SD.LUGG_NO_FK1_RD  NOT IN ('','0','0000')
 ORDER BY SD.SC_NO
""" % WH_TYP

SQL_RETRIEVE_DONE = """
SELECT SD.SC_NO, SD.ITN_LUGG_FK1, SHD.HS_MC_NO
  FROM SC_DATA SD
 INNER JOIN SC_HS_DEF SHD
    ON SHD.WH_TYP = SD.WH_TYP AND SHD.SC_NO = SD.SC_NO AND SHD.HS_NO = '%s'
 WHERE SD.WH_TYP     = '%s'
   AND SD.JOB_TYP_RD = '2'
   AND COALESCE(SD.COMPLETE_RD,'0') NOT IN ('','0')
   AND SD.ITN_LUGG_FK1 NOT IN ('','0','0000')
 ORDER BY SD.SC_NO
""" % (HS_NO_RETRIEVE, WH_TYP)


# ── 출고 대기대의 출발 준비 신호 ────────────────────────────────────────
#   CHECK_CV_RET_START 는 대기대 트랙의 STO_READY_RD='1' 을 요구한다.
#   실제 PLC 는 대기대에 화물이 놓이면 이 비트를 세운다. CvSim 은 대기대를
#   입고대로 모델링하지 않아 세우지 않으므로 여기서 대신 만든다.
#   비트 자리는 DeviceMap 의 StoStation 영역에서 읽는다.
#   (비트 번호는 desc 가 아니라 항목 순서로 정해진다 - DeviceMap.cpp nInOrder = j+1)
DEVICE_MAP = os.path.join(os.path.dirname(os.path.abspath(__file__)),
                          '..', 'WCS_TASK_CV_original', '7_DeviceMap', 'DeviceMap02.xml')


def sto_station_bits():
    """DeviceMap 의 StoStation 영역에서 {트랙: (주소, 비트)} 를 만든다."""
    raw = io.open(DEVICE_MAP, 'rb').read()
    txt = (raw[3:] if raw[:3] == b'\xef\xbb\xbf' else raw).decode('utf-8')
    m = re.search(r'name="StoStation" address="(\d+)">(.*?)</Area>', txt, re.S)
    if not m:
        return {}
    addr = int(m.group(1))
    out = {}
    for i, bm in enumerate(re.finditer(r'<Bit[^>]*Use\s*="(\d+)"\s*tid="(\d+)"', m.group(2))):
        if bm.group(1) == '1':
            out[bm.group(2)] = (addr, i)          # 항목 순서가 곧 비트 번호
    return out


#   대상 : 출고 대기대(SC_HS_DEF.WAIT_TRACK) 와 출고위치 결정대(DEST_POS_DEF 171).
#          둘 다 CHECK_CV_RET_START / CHECK_CV_RET_RESTART 가 출발 준비를 요구하는데
#          CvSim 이 입고대로 모델링하지 않아 신호가 나오지 않는 자리다.
SQL_WAIT_TRACKS = """
SELECT DISTINCT WAIT_TRACK AS TR FROM SC_HS_DEF
 WHERE WH_TYP = '%s' AND COALESCE(WAIT_TRACK,'') <> ''
UNION
SELECT MC_NO AS TR FROM DEST_POS_DEF
 WHERE WH_TYP = '%s' AND TRACK_NO = '171'
 ORDER BY TR
""" % (WH_TYP, WH_TYP)


def wait_track_ready(verbose=True):
    """대기대에 화물이 있으면 출발 준비를 세우고, 없으면 내린다."""
    bits = sto_station_bits()
    if not bits:
        return 0

    done = 0
    for (track,) in query(SQL_WAIT_TRACKS):
        if track not in bits:
            continue                              # DeviceMap 에 없는 대기대는 건드리지 않는다
        addr, bit = bits[track]
        port = port_of(track)

        try:
            words = q3e_read(port, base_of(track), 3)
            cur = q3e_read(port, addr, 1)[0]
        except Exception:
            continue                              # 해당 CV 가 안 떠 있으면 건너뛴다

        holds = (words[0] != 0) or (words[2] & 0x0001)
        on = bool(cur & (1 << bit))

        if holds and not on:
            q3e_write(port, addr, [cur | (1 << bit)])
            done += 1
            if verbose:
                print('  대기대 %s : 화물 있음 -> 출발 준비 ON' % track)
        elif (not holds) and on:
            q3e_write(port, addr, [cur & ~(1 << bit)])
            done += 1
            if verbose:
                print('  대기대 %s : 비었음 -> 출발 준비 OFF' % track)

    return done


# ── 크레인 활성 유지 ────────────────────────────────────────────────────
#   ScSim 은 포크 데이터를 지우면 크레인을 INACTIVE(D109=0)로 내린다.
#   그 상태에서는 다음 지시를 받아도 작업을 진행하지 않는다.
#   실제 현장에서는 상위/운전이 활성으로 유지하므로 여기서 대신 세워 준다.
SC_PORT_BASE = 8100          # ScSim : 901호기 -> 8101
SC_REG_ACTIVE = 109


def sc_port_of(sc_no):
    return SC_PORT_BASE + (int(sc_no) - 900)


SQL_CRANES = """
SELECT SC_NO FROM SC_DATA WHERE WH_TYP = '%s' ORDER BY SC_NO
""" % WH_TYP


def crane_active(verbose=True):
    """INACTIVE 로 내려간 크레인을 다시 활성으로 만든다."""
    done = 0
    for (sc_no,) in query(SQL_CRANES):
        port = sc_port_of(sc_no)
        try:
            cur = q3e_read(port, SC_REG_ACTIVE, 1)[0]
        except Exception:
            continue                                   # 안 떠 있는 호기는 건너뛴다
        if cur == 0:
            q3e_write(port, SC_REG_ACTIVE, [1])
            done += 1
            if verbose:
                print('  %s호기 : INACTIVE -> 활성' % sc_no)
    return done


def handoff_once(verbose=True):
    """완료한 크레인마다 컨베이어 쪽 화물을 옮겨 준다. 바꾼 건수를 돌려준다."""
    done = 0

    # 입고 완료 : 크레인이 집어갔으니 입고 H/S 를 비운다
    for sc_no, lugg, hs in query(SQL_STORE_DONE):
        port, base = port_of(hs), base_of(hs)
        cur = q3e_read(port, base, 3)
        if cur[0] == 0 and (cur[2] & 0x0001) == 0:
            continue
        q3e_write(port, base, [0, 0, 0])
        done += 1
        if verbose:
            print('  입고 완료 %s호기 : 입고 H/S %s 비움 (작번 %s)' % (sc_no, hs, lugg))

    # 출고 완료 : 크레인이 출고 H/S 에 내려놓는다
    for sc_no, lugg, hs in query(SQL_RETRIEVE_DONE):
        port, base = port_of(hs), base_of(hs)
        cur = q3e_read(port, base, 3)
        if cur[0] != 0 or (cur[2] & 0x0001) != 0:
            continue
        q3e_write(port, base, [int(lugg), 0, 1])   # 작번 / 목적지는 WCS 가 넣는다 / 화물감지 ON
        done += 1
        if verbose:
            print('  출고 완료 %s호기 : 출고 H/S %s 에 화물 %s 놓음' % (sc_no, hs, lugg))

    # 대기대 출발 준비
    done += wait_track_ready(verbose)

    # 크레인 활성 유지
    done += crane_active(verbose)

    return done


def main():
    ap = argparse.ArgumentParser(description='크레인-컨베이어 화물 인계 시험 하니스')
    ap.add_argument('--loop', action='store_true', help='계속 돌면서 인계한다')
    ap.add_argument('--interval', type=float, default=3.0, help='--loop 일 때 훑는 간격(초)')
    args = ap.parse_args()

    if args.loop:
        print('크레인-컨베이어 인계를 계속 대신한다. Ctrl+C 로 멈춘다.')
        while True:
            try:
                handoff_once()
            except Exception as ex:
                print('  ! %s' % ex)
            time.sleep(args.interval)
    else:
        print('인계 %d건' % handoff_once())


if __name__ == '__main__':
    sys.exit(main())

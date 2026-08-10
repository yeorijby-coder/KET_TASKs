# -*- coding: utf-8 -*-
"""
이동 -> 입고 -> 출고 한 바퀴를 넣고 끝까지 따라가는 시험 스크립트.

    상위(HOST)가 넣는 것과 같은 모양으로 JOB_MST 에 작업을 만들고,
    상태가 어떻게 흘러가는지 찍는다.
    HostSim 의 작업 생성은 화면 버튼이라 CLI 로 돌릴 수 없어서 이걸 쓴다.

    Tools/sc_cv_handoff.py 를 --loop 로 함께 돌려야 한다.
    (크레인과 컨베이어 사이 화물 인계를 대신해 준다)

    python cycle_test.py
"""
import os
import subprocess
import sys
import time

DB_HOST = '127.0.0.1'
DB_NAME = 'KET_WCS'
DB_USER = 'KET_WCS'
DB_PASS = 'KET_WCS'
PSQL = r'C:\Program Files\PostgreSQL\16\bin\psql.exe'
WH = '10'

RACK = '07-001-01'      # 4호기 담당 (뱅크 07 -> (7+1)/2 = 4호기)
STN_STO = '101'         # 입고대  (트랙 217)
STN_ARV = '107'         # 도착대  (트랙 218)
STN_RET = '105'         # 출고대  (1F 그룹)


def q(sql):
    env = dict(os.environ)
    env['PGPASSWORD'] = DB_PASS
    r = subprocess.run([PSQL, '-h', DB_HOST, '-U', DB_USER, '-d', DB_NAME,
                        '-t', '-A', '-F', '|', '-c', sql],
                       capture_output=True, text=True, env=env, encoding='utf-8')
    if r.returncode != 0:
        raise IOError(r.stderr.strip())
    return [tuple(x.strip() for x in ln.split('|'))
            for ln in r.stdout.splitlines() if ln.strip()]


def add_job(lugg, job_typ, start_pos, start_loc, dest_pos, dest_loc):
    q("INSERT INTO JOB_MST (WH_TYP, LUGG_NO, START_POS, START_LOCATION, DEST_POS, DEST_LOCATION,"
      " JOB_TYP, JOB_STATUS, LOT_NO, JOB_PRIORITY, PRODUCT_SIZE, INS_DT, INS_USER_ID, REMARKS, WC_STEP)"
      " VALUES ('%s','%s','%s','%s','%s','%s','%s','99','','000','0',NOW(),'HOST_TASK','','0');"
      % (WH, lugg, start_pos, start_loc, dest_pos, dest_loc, job_typ))


def follow(lugg, label, timeout=240):
    """작업이 사라질 때까지(= 상위 완료보고 후 삭제) 따라간다."""
    print('\n=== %s (작업 %s) ===' % (label, lugg))
    seen = []
    t0 = time.time()
    while time.time() - t0 < timeout:
        rows = q("SELECT JOB_STATUS FROM JOB_MST WHERE LUGG_NO='%s'" % lugg)
        if not rows:
            print('   %-6s 완료 - 상위 완료보고 후 삭제' % '')
            return True
        st = rows[0][0]
        if not seen or seen[-1] != st:
            seen.append(st)
            pos = q("SELECT MC_NO||'('||DEST_POS_RD||')' FROM CV_DATA"
                    " WHERE LUGG_NO_RD='%s'" % lugg)
            sc = q("SELECT SC_NO||' t'||JOB_TYP_RD||' c'||COMPLETE_RD FROM SC_DATA"
                   " WHERE ITN_LUGG_FK1='%s'" % lugg)
            print('   상태 %-3s   화물 %-14s 크레인 %s'
                  % (st, pos[0][0] if pos else '-', sc[0][0] if sc else '-'))
        time.sleep(2)
    print('   시간 초과 - 마지막 상태 %s' % (seen[-1] if seen else '?'))
    return False


def main():
    print('작업 데이터를 지우고 한 바퀴 돌린다.')
    q('DELETE FROM JOB_MST;')

    base = int(q("SELECT COALESCE(MAX(CAST(NULLIF(LUGG_NO,'') AS INT)),3000) FROM JOB_MST_HIS")[0][0])
    base = max(base, 3000) + 1

    ok = True

    add_job(base, '6', STN_STO, '00-000-00', STN_ARV, '00-000-00')
    ok &= follow(base, '이동  입고대 -> 도착대')

    add_job(base + 1, '1', STN_ARV, '00-000-00', '904', RACK)
    ok &= follow(base + 1, '입고  도착대 -> 4호기 랙 %s' % RACK)

    add_job(base + 2, '2', '000', RACK, STN_RET, '00-000-00')
    ok &= follow(base + 2, '출고  랙 %s -> 출고대' % RACK)

    print('\n%s' % ('한 바퀴 모두 완주했다.' if ok else '완주하지 못한 구간이 있다.'))
    return 0 if ok else 1


if __name__ == '__main__':
    sys.exit(main())

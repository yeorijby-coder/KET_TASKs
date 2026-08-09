-- =====================================================================
--  CV_DATA.STN_KIND 채우기  (PostgreSQL)
--
--  왜 필요한가
--    HOST 태스크는 작업지시(O)를 받으면 출발지/도착지가 쓸 수 있는 작업대인지
--    CV_DATA 를 보고 판정한다. 판정은 STN_KIND 의 비트로 한다.
--
--      0x01 STO     입고대      (입고/이동 작업의 출발지)
--      0x02 RET     출고대      (출고/이동 작업의 도착지)
--      0x04 SC_IN   SC 입고 H/S (입고 작업의 도착지)
--      0x08 SC_OUT  SC 출고 H/S
--
--    그런데 HOST_STN_NO 가 붙어 있는 실제 작업대 행들은 STN_KIND 가 전부 0 이라
--    무엇을 지시하든 거절당한다.
--
--      [ParseOorR] .. 출발지가 올바르지 않습니다.[작업번호:1001][출발지:101]
--      [ParseOorR] .. 도착지가 올바르지 않습니다.[작업번호:1002][도착지:108]
--
--    STN_KIND 가 들어 있는 행은 PLC 01 의 옛 행(MC_NO 102/105/111/...) 뿐인데
--    그쪽은 HOST_STN_NO 가 없어서 지금 판정 경로(HOST_STN_NO 기준)에 걸리지 않는다.
--
--  무엇을 넣는가
--    HostSim 로직이 도는 순환은 이렇다.
--      1) 이동  101 -> 107     : 출발 101 에 STO,  도착 107 에 RET
--      2) 입고  107 -> 랙      : 출발 107 에 STO,  도착은 SC H/S
--      3) 출고  랙 -> 105      : 도착 105 에 RET
--    그래서 입고 라인의 Size Checker 들(101/102/107/108)은 STO|RET 를 모두 갖고
--    출고대(103/104)는 RET 를 갖는다. 옛 행 MC_NO 102 가 51(0x33 = STO|RET|0x10|0x20)
--    이었던 것과 같은 값을 쓴다. (0x10/0x20 은 현장 고유 비트로 보이며 판정에는 쓰이지 않는다)
--
--  ※ 확인이 필요한 것
--    논리 105(1F 그룹 출고대)는 DEST_POS_DEF 상 물리 MC_NO 가 2101 인데
--    CV_DATA 에 그 행이 없다. 즉 105 는 103/104 를 묶은 논리 그룹이라
--    단독 작업대 행이 없다. HostSim 로직의 RetStns 를 103 이나 104 로 두거나,
--    105 에 해당하는 행을 만들어 주어야 한다.  <-- 현장 확인 필요
-- =====================================================================

BEGIN;

-- 입고 라인 Size Checker : 이동의 출발지이자 입고의 출발지, 이동의 도착지도 된다
UPDATE CV_DATA
   SET STN_KIND = '51'                      -- 0x33 = STO(0x01) | RET(0x02) | 0x10 | 0x20
 WHERE WH_TYP      = '10'
   AND HOST_STN_NO IN ('101', '102', '107', '108');

-- 1F 출고대 : 출고/이동의 도착지
UPDATE CV_DATA
   SET STN_KIND = '2'                       -- 0x02 = RET
 WHERE WH_TYP      = '10'
   AND HOST_STN_NO IN ('103', '104');

COMMIT;

-- 확인
-- SELECT HOST_STN_NO, PLC_NO, MC_NO, STN_KIND
--   FROM CV_DATA
--  WHERE WH_TYP = '10' AND COALESCE(HOST_STN_NO,'') <> ''
--  ORDER BY HOST_STN_NO;

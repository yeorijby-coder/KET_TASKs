-- =====================================================================
--  P-BOX 창고를 따로 떼어 낸다  (WH_TYP 10 -> 20)
--  2026-10-12
-- =====================================================================
--
--  WMS 가 P-BOX 창고를 별도로 관리한다. 지금까지는 모든 설비가 WH_TYP 10
--  이어서 상태보고의 창고구분이 전부 'A' 로 나갔고, WMS 는 P-BOX 설비를
--  "알 수 없는 설비" 로 돌려보냈다.
--
--  기존 ECS 도 창고를 갈라 보낸다. (EcsSv/Lib.cpp CLib::GetWareHouseFromPosition)
--    작업대 211 221 222 231 241 242 251  ->  WAREHOUSE_BOX  'B'
--    크레인 11호기                        ->  WAREHOUSE_BOX  'B'
--    그 밖                                ->  WAREHOUSE_PALLET 'A'
--
--  호기·작업대 번호는 기존 ECS 그대로 둔다.
--    크레인 : 11 (EcsSv/ScInfo.cpp 가 ECS_STN_POS_HS_11 = 11 을 보낸다)
--    작업대 : 211 221 222 231 241 242 251
--  바뀌는 것은 창고구분 한 글자뿐이다.
--
--  WH_TYP 20 은 cDefApp.eWHTYP.KET_WH02 로 이미 정의돼 있던 값이다.
--
--  되돌리려면 맨 아래 주석을 참고한다.
-- =====================================================================

BEGIN;

-- ---------------------------------------------------------------------
-- 0) 손대기 전 모습을 남긴다
-- ---------------------------------------------------------------------
DROP TABLE IF EXISTS zz_pbox_split_bak_20261012;
CREATE TABLE zz_pbox_split_bak_20261012 AS
SELECT 'cv_data'      AS tbl, wh_typ, plc_no AS k1, mc_no  AS k2 FROM cv_data      WHERE plc_no = '04'
UNION ALL
SELECT 'sc_data',      wh_typ, plc_no, sc_no            FROM sc_data      WHERE sc_no  = '911'
UNION ALL
SELECT 'sc_hs_def',    wh_typ, sc_no,  hs_no            FROM sc_hs_def    WHERE sc_no  = '911'
UNION ALL
SELECT 'dest_pos_def', wh_typ, track_no, mc_no          FROM dest_pos_def WHERE track_no IN ('211','221','222','231','241','242','251')
UNION ALL
SELECT 'eqp_mst',      wh_typ, eqp_typ, plc_no          FROM eqp_mst      WHERE (eqp_typ = 'CV' AND plc_no = '04')
                                                                             OR (eqp_typ = 'SC' AND plc_no = '11');

-- ---------------------------------------------------------------------
-- 1) C/V : P-BOX 라인은 PLC 04 하나다 (트랙 401~446)
-- ---------------------------------------------------------------------
UPDATE cv_data
   SET wh_typ = '20'
 WHERE wh_typ = '10'
   AND plc_no = '04';

-- ---------------------------------------------------------------------
-- 2) 크레인 : 11호기(SC_NO 911) 하나다. 호기번호는 그대로 둔다.
-- ---------------------------------------------------------------------
UPDATE sc_data
   SET wh_typ = '20'
 WHERE wh_typ = '10'
   AND sc_no  = '911';

UPDATE sc_hs_def
   SET wh_typ = '20'
 WHERE wh_typ = '10'
   AND sc_no  = '911';

-- ---------------------------------------------------------------------
-- 3) 작업대 정의 : 기존 ECS 가 BOX 로 보는 일곱 자리
-- ---------------------------------------------------------------------
UPDATE dest_pos_def
   SET wh_typ = '20'
 WHERE wh_typ = '10'
   AND track_no IN ('211','221','222','231','241','242','251');

-- ---------------------------------------------------------------------
-- 4) 설비 목록
-- ---------------------------------------------------------------------
UPDATE eqp_mst
   SET wh_typ = '20'
 WHERE wh_typ = '10'
   AND ((eqp_typ = 'CV' AND plc_no = '04')
     OR (eqp_typ = 'SC' AND plc_no = '11'));

-- ---------------------------------------------------------------------
-- 5) 살아 있는 작업 : P-BOX 작업대를 오가는 것만 옮긴다
--
--    출발지든 도착지든 P-BOX 작업대(또는 11호기 = 911)면 P-BOX 작업이다.
--    완료보고의 창고구분이 JOB_MST.WH_TYP 에서 나오므로 맞춰 둬야 한다.
--    (기존 ECS 도 완료보고에 JobItem.m_nStartWareHouse 를 싣는다)
-- ---------------------------------------------------------------------
UPDATE job_mst
   SET wh_typ = '20'
 WHERE wh_typ = '10'
   AND (start_pos IN ('211','221','222','231','241','242','251','911')
     OR dest_pos  IN ('211','221','222','231','241','242','251','911'));

COMMIT;

-- ---------------------------------------------------------------------
-- 확인
-- ---------------------------------------------------------------------
SELECT 'cv_data'      AS tbl, wh_typ, count(*) FROM cv_data      GROUP BY wh_typ
UNION ALL
SELECT 'sc_data',      wh_typ, count(*)        FROM sc_data      GROUP BY wh_typ
UNION ALL
SELECT 'sc_hs_def',    wh_typ, count(*)        FROM sc_hs_def    GROUP BY wh_typ
UNION ALL
SELECT 'dest_pos_def', wh_typ, count(*)        FROM dest_pos_def GROUP BY wh_typ
UNION ALL
SELECT 'eqp_mst',      wh_typ, count(*)        FROM eqp_mst      GROUP BY wh_typ
UNION ALL
SELECT 'job_mst',      wh_typ, count(*)        FROM job_mst      GROUP BY wh_typ
ORDER BY 1, 2;

-- =====================================================================
--  되돌리기
-- =====================================================================
--  BEGIN;
--  UPDATE cv_data      SET wh_typ='10' WHERE wh_typ='20' AND plc_no='04';
--  UPDATE sc_data      SET wh_typ='10' WHERE wh_typ='20' AND sc_no='911';
--  UPDATE sc_hs_def    SET wh_typ='10' WHERE wh_typ='20' AND sc_no='911';
--  UPDATE dest_pos_def SET wh_typ='10' WHERE wh_typ='20'
--         AND track_no IN ('211','221','222','231','241','242','251');
--  UPDATE eqp_mst      SET wh_typ='10' WHERE wh_typ='20'
--         AND ((eqp_typ='CV' AND plc_no='04') OR (eqp_typ='SC' AND plc_no='11'));
--  UPDATE job_mst      SET wh_typ='10' WHERE wh_typ='20';
--  COMMIT;
-- =====================================================================

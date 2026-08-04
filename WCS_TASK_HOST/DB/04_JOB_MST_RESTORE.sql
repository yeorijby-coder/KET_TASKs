-- =====================================================================
--  JOB_MST 시험 데이터 복원  (PostgreSQL)
--
--  왜 필요한가
--    HOST 태스크를 붙여 통신 시험을 하면 완료보고(F) 경로가 작업을 하나씩
--    처리하면서 JOB_MST 에서 지우고 JOB_MST_HIS 로 옮긴다. 정상 동작이지만
--    시험을 반복하면 시험용 작업 508건이 다 없어진다.
--    이 스크립트는 그렇게 이력으로 넘어간 작업을 JOB_MST 로 되돌린다.
--
--  기준 데이터 (2026-07-10 일괄 적재분)
--    작업구분 1(입고)        12 건
--    작업구분 3(피킹 출고)  495 건
--    작업구분 6(이동)         1 건
--                          총 508 건,  모두 JOB_STATUS = 29
--
--  실행 : psql -h 127.0.0.1 -p 5432 -U KET_WCS -d KET_WCS -f 04_JOB_MST_RESTORE.sql
--
--  ※ 시험으로 만든 작업(작업구분 2 등)은 되돌리지 않는다.
--  ※ 두 테이블의 공통 컬럼만 옮긴다. (JOB_MST_HIS 에는 INS_DATE / INS_TIME 이 더 있다)
-- =====================================================================

BEGIN;

-- ---------------------------------------------------------------------
-- 1) 이력에만 있고 JOB_MST 에는 없는 시험용 작업을 되돌린다
-- ---------------------------------------------------------------------
DO $$
DECLARE
    v_cols TEXT;
BEGIN
    SELECT string_agg(quote_ident(c.column_name), ', ' ORDER BY c.ordinal_position)
      INTO v_cols
      FROM information_schema.columns c
     WHERE c.table_schema = 'public'
       AND lower(c.table_name) = 'job_mst'
       AND EXISTS (SELECT 1
                     FROM information_schema.columns h
                    WHERE h.table_schema = 'public'
                      AND lower(h.table_name) = 'job_mst_his'
                      AND h.column_name = c.column_name);

    EXECUTE format(
        'INSERT INTO JOB_MST (%s)
         SELECT %s
           FROM JOB_MST_HIS H
          WHERE H.JOB_TYP IN (''1'',''3'',''6'')
            AND H.JOB_STATUS = ''29''
            AND NOT EXISTS (SELECT 1 FROM JOB_MST J WHERE J.LUGG_NO = H.LUGG_NO)',
        v_cols, v_cols);
END $$;

-- ---------------------------------------------------------------------
-- 2) 되돌린 만큼 이력에서 지운다
--    (같은 작업이 "진행 중"과 "완료됨"에 동시에 있으면 헷갈린다)
-- ---------------------------------------------------------------------
DELETE FROM JOB_MST_HIS H
 WHERE H.JOB_TYP IN ('1','3','6')
   AND H.JOB_STATUS = '29'
   AND EXISTS (SELECT 1 FROM JOB_MST J WHERE J.LUGG_NO = H.LUGG_NO);

COMMIT;


-- =====================================================================
--  확인   (508 / 13200 이면 시험 전 상태와 같다)
-- =====================================================================

-- SELECT (SELECT COUNT(*) FROM JOB_MST)     AS JOB_MST
--      , (SELECT COUNT(*) FROM JOB_MST_HIS) AS JOB_MST_HIS;

-- SELECT JOB_TYP, JOB_STATUS, COUNT(*) FROM JOB_MST GROUP BY 1,2 ORDER BY 1;


-- =====================================================================
--  시험으로 생긴 찌꺼기 지우기 (필요할 때만)
-- =====================================================================
-- DELETE FROM JOB_MST     WHERE INS_USER_ID = 'TEST';
-- DELETE FROM JOB_MST_HIS WHERE JOB_TYP = '2';
-- DELETE FROM HOST_IF_LOG;
-- UPDATE CV_DATA SET LUGG_NO_RD = '0000' WHERE HOST_STN_NO IN ('221','222');

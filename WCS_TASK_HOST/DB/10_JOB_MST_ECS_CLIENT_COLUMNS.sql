-- =====================================================================
--  JOB_MST 에 WCS Client(Ecs) 작업정보창이 읽는 컬럼 채우기  (PostgreSQL)
--
--  왜 필요한가
--    작업정보창의 조회가 이 현장에 없는 컬럼을 SELECT 해서 매번 예외로 죽었다.
--
--      SelectSqlForThread: ... 오류: jm.item_info 칼럼 없음
--
--    ADO 계층은 그 예외를 삼키고 건수를 -1 로 둔 채 돌아온다. 그래서 화면에는
--    "조회건수 : -1" 만 뜨고 목록이 늘 비어 있었다. 상위가 준 작업이 JOB_MST 에
--    멀쩡히 들어 있어도 보이지 않았다.
--    (디버거로 확인 : CAdoDB 의 예외 메시지가 위 문구를 반복)
--
--    CV_DATA 도 같은 이유로 트랙 상태가 갱신되지 않았다. 08_ 스크립트 참고.
--
--  왜 컬럼을 없애지 않고 만드는가
--    Client 는 현장 공용 코드다. 다른 현장에서는 이 컬럼들을 쓴다.
--    이 현장에서 안 쓰면 빈 값으로 남으면 되고 화면에도 빈 칸으로 나온다.
--
--  실행 : psql -h 127.0.0.1 -p 5432 -U KET_WCS -d KET_WCS -f 10_JOB_MST_ECS_CLIENT_COLUMNS.sql
-- =====================================================================

BEGIN;

-- 제품정보 / 제품정보2 (작업정보창 목록의 마지막 두 칸)
ALTER TABLE JOB_MST     ADD COLUMN IF NOT EXISTS ITEM_INFO  VARCHAR(50) DEFAULT '';
ALTER TABLE JOB_MST     ADD COLUMN IF NOT EXISTS ITEM_INFO2 VARCHAR(50) DEFAULT '';

-- 이력 테이블도 같은 모양이어야 04_JOB_MST_RESTORE.sql 의 공통 칼럼 복사가 어긋나지 않는다
ALTER TABLE JOB_MST_HIS ADD COLUMN IF NOT EXISTS ITEM_INFO  VARCHAR(50) DEFAULT '';
ALTER TABLE JOB_MST_HIS ADD COLUMN IF NOT EXISTS ITEM_INFO2 VARCHAR(50) DEFAULT '';

UPDATE JOB_MST
   SET ITEM_INFO  = COALESCE(ITEM_INFO,  '')
     , ITEM_INFO2 = COALESCE(ITEM_INFO2, '');

COMMIT;

-- 확인
-- SELECT LUGG_NO, JOB_TYP, JOB_STATUS, ITEM_INFO, ITEM_INFO2 FROM JOB_MST;

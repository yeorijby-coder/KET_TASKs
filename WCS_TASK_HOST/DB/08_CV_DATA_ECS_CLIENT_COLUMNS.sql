-- =====================================================================
--  CV_DATA 에 WCS Client(Ecs) 가 읽는 컬럼 채우기  (PostgreSQL)
--
--  왜 필요한가
--    WCS Client 는 트랙 상태를 CV_DATA 에서 통째로 읽어 화면에 칠한다.
--    (ClientNSim/Ecs/Cv.cpp 의 CCv::GetSelectQry)
--    그 조회가 이 현장에 없는 컬럼을 SELECT 해서 매번 예외로 죽고 있었다.
--
--      SelectSqlForThread: ... 오류: cd.comming_check_tr1 칼럼 없음
--
--    ADO 계층은 그 예외를 삼키고 0건을 돌려주고, CollectDB 는 0건이면
--    SetVar 를 건너뛴다. 그래서 CCv::AutoRunProc 가 한 번도 불리지 않아
--    트랙 색도, 재하 표시도, 작업번호 표시도 영영 갱신되지 않았다.
--    (디버거로 확인 : [collect] nRowCnt=0 만 반복, [setvar]/[autorun] 은 0회)
--
--  왜 컬럼을 없애지 않고 만드는가
--    Client 는 현장 공용 코드다. 다른 현장에서는 이 컬럼들을 쓴다.
--    그리고 CV 태스크도 이미 DeviceMap 에 정의된 컬럼을 같은 방식으로
--    ALTER TABLE ... ADD COLUMN IF NOT EXISTS 로 만들어 쓴다.
--    (WCS_TASK_CV_original/2_Thread/EQP_THREAD/CvThread.cs 의 EnsureCvDataColumns)
--    이 현장에서 안 쓰는 값은 '0' 으로 남아 있으면 되고, 화면 판정에도
--    영향이 없다.
--
--  실행 : psql -h 127.0.0.1 -p 5432 -U KET_WCS -d KET_WCS -f 08_CV_DATA_ECS_CLIENT_COLUMNS.sql
-- =====================================================================

BEGIN;

-- 합류 확인용 트랙 (원본 ECS 의 진입 체크)
ALTER TABLE CV_DATA ADD COLUMN IF NOT EXISTS COMMING_CHECK_TR1    VARCHAR(10) DEFAULT '0';
ALTER TABLE CV_DATA ADD COLUMN IF NOT EXISTS COMMING_CHECK_TR2    VARCHAR(10) DEFAULT '0';
ALTER TABLE CV_DATA ADD COLUMN IF NOT EXISTS COMMING_CHECK_TR3    VARCHAR(10) DEFAULT '0';
ALTER TABLE CV_DATA ADD COLUMN IF NOT EXISTS COMMING_CHECK_TR4    VARCHAR(10) DEFAULT '0';
ALTER TABLE CV_DATA ADD COLUMN IF NOT EXISTS COMMING_DEST_TR      VARCHAR(10) DEFAULT '0';

-- 셔터/도어
ALTER TABLE CV_DATA ADD COLUMN IF NOT EXISTS DOOR_STATUS_RD       VARCHAR(10) DEFAULT '0';
ALTER TABLE CV_DATA ADD COLUMN IF NOT EXISTS DOOR_OPEN_REQ_RD     VARCHAR(10) DEFAULT '0';
ALTER TABLE CV_DATA ADD COLUMN IF NOT EXISTS DOOR_CLOSE_REQ_RD    VARCHAR(10) DEFAULT '0';

-- 디버터 / RGV / 파렛트 픽업
ALTER TABLE CV_DATA ADD COLUMN IF NOT EXISTS DEVERTER_HS_DOWN_RD  VARCHAR(10) DEFAULT '0';
ALTER TABLE CV_DATA ADD COLUMN IF NOT EXISTS DEVERTER_HS_UP_RD    VARCHAR(10) DEFAULT '0';
ALTER TABLE CV_DATA ADD COLUMN IF NOT EXISTS RGV_STA_LOAD_RD      VARCHAR(10) DEFAULT '0';
ALTER TABLE CV_DATA ADD COLUMN IF NOT EXISTS SC_PA_PK_POSSIBLE_RD VARCHAR(10) DEFAULT '0';

-- 모터 구동
ALTER TABLE CV_DATA ADD COLUMN IF NOT EXISTS MTR1_RUN_RD          VARCHAR(10) DEFAULT '0';
ALTER TABLE CV_DATA ADD COLUMN IF NOT EXISTS MTR2_RUN_RD          VARCHAR(10) DEFAULT '0';

-- 사이즈 체크 / 정렬 / 피킹
ALTER TABLE CV_DATA ADD COLUMN IF NOT EXISTS SZ_CHK_LOW_RD        VARCHAR(10) DEFAULT '0';
ALTER TABLE CV_DATA ADD COLUMN IF NOT EXISTS SZ_CHK_HIGH_RD       VARCHAR(10) DEFAULT '0';
ALTER TABLE CV_DATA ADD COLUMN IF NOT EXISTS SRT_READY_STA_RD     VARCHAR(10) DEFAULT '0';
ALTER TABLE CV_DATA ADD COLUMN IF NOT EXISTS PICK4_PASS_RD        VARCHAR(10) DEFAULT '0';

-- 기존 행에도 값을 채운다 (ADD COLUMN 의 DEFAULT 는 새 행에만 적용되는 경우가 있다)
UPDATE CV_DATA
   SET COMMING_CHECK_TR1    = COALESCE(COMMING_CHECK_TR1,    '0')
     , COMMING_CHECK_TR2    = COALESCE(COMMING_CHECK_TR2,    '0')
     , COMMING_CHECK_TR3    = COALESCE(COMMING_CHECK_TR3,    '0')
     , COMMING_CHECK_TR4    = COALESCE(COMMING_CHECK_TR4,    '0')
     , COMMING_DEST_TR      = COALESCE(COMMING_DEST_TR,      '0')
     , DOOR_STATUS_RD       = COALESCE(DOOR_STATUS_RD,       '0')
     , DOOR_OPEN_REQ_RD     = COALESCE(DOOR_OPEN_REQ_RD,     '0')
     , DOOR_CLOSE_REQ_RD    = COALESCE(DOOR_CLOSE_REQ_RD,    '0')
     , DEVERTER_HS_DOWN_RD  = COALESCE(DEVERTER_HS_DOWN_RD,  '0')
     , DEVERTER_HS_UP_RD    = COALESCE(DEVERTER_HS_UP_RD,    '0')
     , RGV_STA_LOAD_RD      = COALESCE(RGV_STA_LOAD_RD,      '0')
     , SC_PA_PK_POSSIBLE_RD = COALESCE(SC_PA_PK_POSSIBLE_RD, '0')
     , MTR1_RUN_RD          = COALESCE(MTR1_RUN_RD,          '0')
     , MTR2_RUN_RD          = COALESCE(MTR2_RUN_RD,          '0')
     , SZ_CHK_LOW_RD        = COALESCE(SZ_CHK_LOW_RD,        '0')
     , SZ_CHK_HIGH_RD       = COALESCE(SZ_CHK_HIGH_RD,       '0')
     , SRT_READY_STA_RD     = COALESCE(SRT_READY_STA_RD,     '0')
     , PICK4_PASS_RD        = COALESCE(PICK4_PASS_RD,        '0');

-- ---------------------------------------------------------------------
--  TR_PAUSE 는 이 현장에서 PLC 신호가 아니라 WCS 내부 변수다.
--  (다른 현장은 CV PLC 에 기록한다. 여기 DeviceMap 에는 TrPause 항목이 없다)
--  내부 변수이므로 값이 비어 있으면 안 된다. '0'(정지 아님)으로 맞춘다.
-- ---------------------------------------------------------------------
ALTER TABLE CV_DATA ALTER COLUMN TR_PAUSE_RD SET DEFAULT '0';
ALTER TABLE CV_DATA ALTER COLUMN TR_PAUSE_OD SET DEFAULT '0';

UPDATE CV_DATA
   SET TR_PAUSE_RD = '0'
 WHERE COALESCE(TR_PAUSE_RD, '') = '';

UPDATE CV_DATA
   SET TR_PAUSE_OD = '0'
 WHERE COALESCE(TR_PAUSE_OD, '') = '';

COMMIT;

-- 확인
-- SELECT COUNT(*) FROM CV_DATA WHERE COALESCE(TR_PAUSE_RD,'') <> '0';

-- =====================================================================
--  WCS_TASK_Display : 데이터 입력 스크립트  (PostgreSQL)
--
--  실행 : psql -h localhost -p 5432 -U KET_WCS -d KET_WCS -f 02_DISPLAY_DATA_INSERT.sql
--         (먼저 01_DISPLAY_DATA_CREATE.sql 을 실행할 것)
--
--  아래 값은 WCS_TASK_Display\bin\Debug\WCS_DB.INI 기준이다.
--      [CNF]   WH_TYP    = 10
--      [COMM0] EQMT      = DISPLAY
--              PLC_NO    = 01
--              IP        = 127.0.0.1
--              CUR_PORT  = 8001   (FROM 8001 ~ TO 8009)
--              CNT       = 2      -> 전광판 2대 (DISP_NO = 1, 2)
--              FR_TRACK  = 2006   -> DISP_NO 1
--              TO_TRACK  = 2012   -> DISP_NO 2
--  INI 를 바꾸면 아래 값도 같이 바꿔야 한다.
--
--  전부 재실행 가능(idempotent)하다. 이미 있는 행은 건드리지 않는다.
-- =====================================================================


-- ---------------------------------------------------------------------
-- 1) DISPLAY_DATA : 전광판 2대 등록
--
--    DISP_DATA 는 앞 8자리만 전광판으로 전송된다(DISP_DATA_LEN = 8).
--    COLOR : 4=Red, 5=Green, 6=Yellow, 0=자동순환
--
--    DispAuto() 는 LUGG_NO 가 "바뀌었을 때만" 전송한다.
--    LUGG_NO 가 '' 또는 '0' 이면 빈 화면(노란색)을 보낸다.
--    아래처럼 LUGG_NO 를 채워두면 태스크 기동 직후 1회 전송이 일어나 동작확인이 된다.
-- ---------------------------------------------------------------------
INSERT INTO DISPLAY_DATA
       (WH_TYP, PLC_NO, DISP_NO, TRACK_NO, DISP_DATA, LUGG_NO, COLOR,
        CMD_RQ_YN, CMD_RQ_ID, CMD_COLOR, SEND_YN, CONNECTED_YN, REG_DT, UPD_DT)
VALUES ('10', '01', '1', '2006', 'KET-1001', 'L0000001', 4,
        'N', 'DATA', 6, 'N', 'N', NOW(), NOW())
ON CONFLICT (WH_TYP, PLC_NO, DISP_NO) DO NOTHING;

INSERT INTO DISPLAY_DATA
       (WH_TYP, PLC_NO, DISP_NO, TRACK_NO, DISP_DATA, LUGG_NO, COLOR,
        CMD_RQ_YN, CMD_RQ_ID, CMD_COLOR, SEND_YN, CONNECTED_YN, REG_DT, UPD_DT)
VALUES ('10', '01', '2', '2012', 'KET-1002', 'L0000002', 5,
        'N', 'DATA', 6, 'N', 'N', NOW(), NOW())
ON CONFLICT (WH_TYP, PLC_NO, DISP_NO) DO NOTHING;


-- ---------------------------------------------------------------------
-- 2) EQP_MST : 설비 등록
--
--    EQP_TYP 는 [COMM*] EQMT 값과 같아야 Communication() 의 UPDATE 가 걸린다.
--    EQP_MST 에는 PK 가 없으므로 WHERE NOT EXISTS 로 중복입력을 막는다.
-- ---------------------------------------------------------------------
INSERT INTO EQP_MST
       (WH_TYP, EQP_TYP, PLC_NO, PLC_IP, PLC_PORT, PLC_PORT_FROM, PLC_PORT_TO,
        SOCK_TIMEOUT, RETRY_YN, USE_YN, CONNECTED_YN, UPD_DT, REMARKS, PRIORITY)
SELECT '10', 'DISPLAY', '01', '127.0.0.1', '8001', '8001', '8009',
       '500', 'Y', 'Y', 'N', NOW(), '전광판 컨트롤러 #01 (WCS_TASK_Display)', '1'
WHERE NOT EXISTS (SELECT 1
                    FROM EQP_MST
                   WHERE WH_TYP  = '10'
                     AND EQP_TYP = 'DISPLAY'
                     AND PLC_NO  = '01');


-- =====================================================================
--  아래는 동작 확인용 참고 쿼리 / 지령 예시  (필요할 때 주석 해제)
-- =====================================================================

-- [확인] 등록 상태
-- SELECT WH_TYP, PLC_NO, DISP_NO, TRACK_NO, DISP_DATA, LUGG_NO, COLOR,
--        CMD_RQ_YN, SEND_YN, LAST_SENT_DATA, SEND_DT
--   FROM DISPLAY_DATA
--  ORDER BY WH_TYP, PLC_NO, DISP_NO;

-- [확인] 통신상태 (태스크가 접속하면 CONNECTED_YN 이 'Y' 로 바뀐다)
-- SELECT WH_TYP, EQP_TYP, PLC_NO, CONNECTED_YN, PLC_PORT, UPD_DT
--   FROM EQP_MST WHERE EQP_TYP = 'DISPLAY';

-- [AUTO 테스트] LUGG_NO 를 바꾸면 다음 폴링에서 전광판으로 전송된다.
-- UPDATE DISPLAY_DATA
--    SET DISP_DATA = 'KET-2001', LUGG_NO = 'L0000009', COLOR = 5, UPD_DT = NOW()
--  WHERE WH_TYP = '10' AND PLC_NO = '01' AND DISP_NO = '1';

-- [MANUAL 테스트] 수동 표시 지령 (화면의 Manual 패널과 동일한 동작)
-- UPDATE DISPLAY_DATA
--    SET CMD_RQ_YN = 'Y', CMD_RQ_ID = 'DATA', CMD_DATA = 'TESTMSG', CMD_COLOR = 4, UPD_DT = NOW()
--  WHERE WH_TYP = '10' AND PLC_NO = '01' AND DISP_NO = '1';

-- [MANUAL 테스트] 수동 화면 지우기
-- UPDATE DISPLAY_DATA
--    SET CMD_RQ_YN = 'Y', CMD_RQ_ID = 'CLEAR', CMD_DATA = '', CMD_COLOR = 6, UPD_DT = NOW()
--  WHERE WH_TYP = '10' AND PLC_NO = '01' AND DISP_NO = '1';

-- [로그] 태스크가 남긴 감사 로그
-- SELECT INS_DT, LOG_SEQ, PGR_NM, LOG_KOR, RQ_INS_ID, EQP_TYP
--   FROM WCS_LOG_PGR WHERE EQP_TYP = 'DISPLAY' ORDER BY INS_DT DESC LIMIT 50;

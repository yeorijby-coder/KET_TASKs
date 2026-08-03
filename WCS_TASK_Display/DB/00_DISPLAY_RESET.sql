-- =====================================================================
--  WCS_TASK_Display : 전광판 테이블 전체 재생성  (PostgreSQL)
--
--  ★★ 주의 ★★
--    이 스크립트는 DISPLAY_DATA / DISPLAY_CTRL 을 DROP 하고 다시 만든다.
--    기존 표시 기록과 접속 제어 설정이 모두 사라진다.
--    운영 DB 에서는 반드시 백업 후 실행할 것.
--    기존 데이터를 유지하면서 없는 것만 만들려면 01_DISPLAY_DATA_CREATE.sql 을 쓴다.
--
--  실행 : psql -h localhost -p 5432 -U KET_WCS -d KET_WCS -f 00_DISPLAY_RESET.sql
--
--  이 스크립트가 만드는 것
--    1) DISPLAY_DATA  : 전광판별 설정(감시 트랙) + 수동 지령 + 표시 기록
--    2) DISPLAY_CTRL  : 컨트롤러별 접속 제어 (화면 / WCS Client 공용)
--    3) 시드 데이터   : WCS_DB.INI 기준 컨트롤러 01, 전광판 2대
--
--  EQP_MST / WCS_LOG_PGR / LOG_SEQ 는 다른 태스크와 공용이므로 건드리지 않는다.
--  필요하면 01_DISPLAY_DATA_CREATE.sql 로 만든다.
-- =====================================================================

BEGIN;

DROP TABLE IF EXISTS DISPLAY_DATA;
DROP TABLE IF EXISTS DISPLAY_CTRL;


-- ---------------------------------------------------------------------
-- 1) DISPLAY_DATA : 전광판 한 대당 한 행
--
--    표시할 내용의 출처는 이 테이블이 아니다.
--    WCS_TASK_Display 가 아래처럼 컨베이어 테이블을 직접 조인해서 읽는다.
--
--      DISPLAY_DATA.TRACK_NO -> CV_DATA.TRACK_NO    (전광판이 감시할 트랙)
--      CV_DATA.LUGG_NO_RD                           (PLC 가 읽은 적재물번호)
--      JOB_MST.PRODUCT_ID                           (레거시 UserData 에 해당하는 표시내용)
--
--    KEY (WH_TYP, PLC_NO, DISP_NO)
--      WH_TYP  : 창고구분        - WCS_DB.INI [CNF] WH_TYP
--      PLC_NO  : 전광판 컨트롤러 - WCS_DB.INI [COMM*] PLC_NO
--      DISP_NO : 컨트롤러 내 전광판 번호 (1 부터, [COMM*] CNT 개)
-- ---------------------------------------------------------------------
CREATE TABLE DISPLAY_DATA
(
    WH_TYP          VARCHAR(4)   NOT NULL,
    PLC_NO          VARCHAR(5)   NOT NULL,
    DISP_NO         VARCHAR(5)   NOT NULL,
    TRACK_NO        VARCHAR(20)  NOT NULL,       -- 감시할 컨베이어 트랙. CV_DATA.TRACK_NO 와 같아야 한다.

    -- 수동 지령 : Client UI / 화면 수동 패널이 기록
    CMD_RQ_YN       VARCHAR(1)   DEFAULT 'N',    -- 'Y' = 수동지령 대기중
    CMD_RQ_ID       VARCHAR(20)  DEFAULT 'DATA', -- 'DATA' | 'CLEAR'
    CMD_DATA        VARCHAR(20),                 -- 수동으로 표시할 내용
    CMD_COLOR       INTEGER      DEFAULT 6,      -- 4:빨강, 5:초록, 6:노랑

    -- 표시 기록 : 전송 성공 후 WCS_TASK_Display 가 기록 (입력이 아님)
    DISP_DATA       VARCHAR(20),                 -- 실제 표시한 품명 (앞 8자리만 전송)
    LUGG_NO         VARCHAR(20),                 -- 그때의 적재물 번호
    COLOR           INTEGER      DEFAULT 0,      -- 그때의 색상
    SEND_YN         VARCHAR(1)   DEFAULT 'N',
    LAST_SENT_DATA  VARCHAR(20),
    LAST_SENT_LUGG  VARCHAR(20),

    REG_DT          TIMESTAMP    DEFAULT NOW(),
    UPD_DT          TIMESTAMP    DEFAULT NOW(),
    SEND_DT         TIMESTAMP,

    CONSTRAINT PK_DISPLAY_DATA PRIMARY KEY (WH_TYP, PLC_NO, DISP_NO)
);

-- 수동지령 폴링이 CMD_RQ_YN='Y' 만 훑기 때문에 부분 인덱스를 둔다.
CREATE INDEX IX_DISPLAY_DATA_CMD
    ON DISPLAY_DATA (WH_TYP, PLC_NO)
    WHERE CMD_RQ_YN = 'Y';


-- ---------------------------------------------------------------------
-- 2) DISPLAY_CTRL : 컨트롤러 한 대당 한 행 (접속 제어)
--
--    화면의 [접속 끊기] 버튼과 [자동 재접속] 체크박스가 이 테이블에 기록하고,
--    WCS_TASK_Display 가 1초 주기로 읽어 반영한다.
--    WCS Client 가 같은 컬럼을 고쳐도 동일하게 동작하므로,
--    Client 에서 원격으로 접속을 끊거나 붙일 수 있다.
--
--      DISCONNECT_YN  = 'Y' 지시하면 소켓/DB 를 닫고 끊긴 상태를 유지한다
--                       'N' 으로 되돌리면 다시 접속한다
--      AUTO_RECONN_YN = 'Y' 통신이 끊기면 스스로 다시 붙는다
--                       'N' 통신이 끊겨도 그대로 둔다.
--                           다시 붙이려면 DISCONNECT_YN 을 'N' 으로 다시 써야 한다.
--      CONNECTED_YN   태스크가 기록하는 현재 접속상태 (읽기 전용으로 볼 것)
-- ---------------------------------------------------------------------
CREATE TABLE DISPLAY_CTRL
(
    WH_TYP          VARCHAR(4)   NOT NULL,
    PLC_NO          VARCHAR(5)   NOT NULL,

    DISCONNECT_YN   VARCHAR(1)   DEFAULT 'N',    -- 'Y' = 접속 끊기 지시
    AUTO_RECONN_YN  VARCHAR(1)   DEFAULT 'Y',    -- 'Y' = 자동 재접속 사용
    CONNECTED_YN    VARCHAR(1)   DEFAULT 'N',    -- 태스크가 기록하는 현재 접속상태

    RQ_USER_ID      VARCHAR(20),                 -- 마지막으로 지시한 주체 (화면 / Client)
    RQ_DT           TIMESTAMP,                   -- 마지막 지시 시각
    REG_DT          TIMESTAMP    DEFAULT NOW(),
    UPD_DT          TIMESTAMP    DEFAULT NOW(),

    CONSTRAINT PK_DISPLAY_CTRL PRIMARY KEY (WH_TYP, PLC_NO)
);


-- ---------------------------------------------------------------------
-- 3) 시드 데이터
--
--    WCS_TASK_Display\bin\Debug\WCS_DB.INI 기준
--      [CNF]   WH_TYP   = 10
--      [COMM0] PLC_NO   = 01,  CNT = 2
--              FR_TRACK = 2006 -> DISP_NO 1
--              TO_TRACK = 2012 -> DISP_NO 2
--    INI 를 바꾸면 아래 값도 같이 바꿔야 한다.
-- ---------------------------------------------------------------------
INSERT INTO DISPLAY_DATA (WH_TYP, PLC_NO, DISP_NO, TRACK_NO, CMD_RQ_YN, CMD_RQ_ID, CMD_COLOR, COLOR, SEND_YN)
VALUES ('10', '01', '1', '2006', 'N', 'DATA', 6, 0, 'N');

INSERT INTO DISPLAY_DATA (WH_TYP, PLC_NO, DISP_NO, TRACK_NO, CMD_RQ_YN, CMD_RQ_ID, CMD_COLOR, COLOR, SEND_YN)
VALUES ('10', '01', '2', '2012', 'N', 'DATA', 6, 0, 'N');

INSERT INTO DISPLAY_CTRL (WH_TYP, PLC_NO, DISCONNECT_YN, AUTO_RECONN_YN, CONNECTED_YN, RQ_USER_ID, RQ_DT)
VALUES ('10', '01', 'N', 'Y', 'N', 'INIT', NOW());

COMMIT;


-- =====================================================================
--  확인 / 조작 예시
-- =====================================================================

-- [확인] 설정과 표시 기록
-- SELECT WH_TYP, PLC_NO, DISP_NO, TRACK_NO, DISP_DATA, LUGG_NO, COLOR, SEND_DT
--   FROM DISPLAY_DATA ORDER BY WH_TYP, PLC_NO, DISP_NO;

-- [확인] 접속 제어 상태
-- SELECT WH_TYP, PLC_NO, DISCONNECT_YN, AUTO_RECONN_YN, CONNECTED_YN, RQ_USER_ID, RQ_DT
--   FROM DISPLAY_CTRL ORDER BY WH_TYP, PLC_NO;

-- [Client] 접속 끊기 지시
-- UPDATE DISPLAY_CTRL SET DISCONNECT_YN='Y', RQ_USER_ID='CLIENT', RQ_DT=NOW(), UPD_DT=NOW()
--  WHERE WH_TYP='10' AND PLC_NO='01';

-- [Client] 다시 접속
-- UPDATE DISPLAY_CTRL SET DISCONNECT_YN='N', RQ_USER_ID='CLIENT', RQ_DT=NOW(), UPD_DT=NOW()
--  WHERE WH_TYP='10' AND PLC_NO='01';

-- [Client] 자동 재접속 끄기
-- UPDATE DISPLAY_CTRL SET AUTO_RECONN_YN='N', RQ_USER_ID='CLIENT', RQ_DT=NOW(), UPD_DT=NOW()
--  WHERE WH_TYP='10' AND PLC_NO='01';

-- [AUTO 테스트] 컨베이어 트랙에 적재물을 올린 것처럼 만든다
-- UPDATE CV_DATA SET LUGG_NO_RD='1467' WHERE WH_TYP='10' AND TRACK_NO='2006';
-- UPDATE CV_DATA SET LUGG_NO_RD='0000' WHERE WH_TYP='10' AND TRACK_NO='2006';

-- [MANUAL 테스트] 수동 표시 / 지우기
-- UPDATE DISPLAY_DATA SET CMD_RQ_YN='Y', CMD_RQ_ID='DATA', CMD_DATA='TESTMSG', CMD_COLOR=4, UPD_DT=NOW()
--  WHERE WH_TYP='10' AND PLC_NO='01' AND DISP_NO='1';
-- UPDATE DISPLAY_DATA SET CMD_RQ_YN='Y', CMD_RQ_ID='CLEAR', CMD_DATA='', CMD_COLOR=6, UPD_DT=NOW()
--  WHERE WH_TYP='10' AND PLC_NO='01' AND DISP_NO='1';

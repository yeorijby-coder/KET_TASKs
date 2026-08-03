-- =====================================================================
--  WCS_TASK_Display : 테이블 생성 스크립트  (PostgreSQL)
--
--  실행 : psql -h localhost -p 5432 -U KET_WCS -d KET_WCS -f 01_DISPLAY_DATA_CREATE.sql
--
--  이 스크립트는 WCS_TASK_Display 가 구동되기 위해 필요한 오브젝트를 만든다.
--    1) DISPLAY_DATA   : 전광판 표시내용 테이블 (이 태스크의 주 테이블)
--    2) EQP_MST        : 설비 마스터            (통신상태 기록용, 이미 있으면 그대로 둠)
--    3) WCS_LOG_PGR    : 프로그램 로그          (감사 로그용,     이미 있으면 그대로 둠)
--    4) LOG_SEQ        : WCS_LOG_PGR.LOG_SEQ 채번 시퀀스
--
--  모두 IF NOT EXISTS 라서 기존 운영 DB 에 실행해도 기존 데이터는 건드리지 않는다.
--  데이터 입력은 02_DISPLAY_DATA_INSERT.sql 참조.
-- =====================================================================


-- ---------------------------------------------------------------------
-- 1) DISPLAY_DATA
--
--    레거시 MFC 에서는 전광판 표시내용을 메모리(CCvTrackInfo)에서 가져왔으나,
--    신규 구조에서는 이 테이블에 담아두고 CV 태스크 / 스케줄러 / Client 가 쓰고
--    WCS_TASK_Display 가 읽어서 TCP 로 전광판에 내려보낸다.
--
--    KEY (WH_TYP, PLC_NO, DISP_NO)
--      WH_TYP : 창고구분        - WCS_DB.INI [CNF] WH_TYP        (예: 10)
--      PLC_NO : 전광판 컨트롤러 - WCS_DB.INI [COMM*] PLC_NO      (예: 01)
--      DISP_NO : 컨트롤러 내 전광판 번호 (1 부터, [COMM*] CNT 개)
-- ---------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS DISPLAY_DATA
(
    WH_TYP          VARCHAR(4)   NOT NULL,        -- 창고구분
    PLC_NO          VARCHAR(5)   NOT NULL,        -- 전광판 컨트롤러 번호
    DISP_NO          VARCHAR(5)   NOT NULL,        -- 컨트롤러 내 전광판 번호(1-base)
    TRACK_NO        VARCHAR(20),                  -- 매핑된 컨베이어 트랙(참조용)

    -- AUTO 표시내용 : CV 태스크 / 스케줄러가 기록
    DISP_DATA       VARCHAR(20),                  -- 표시할 품명 (앞 8자리만 전송)
    LUGG_NO         VARCHAR(20),                  -- 현재 적재물 번호(변경감지 키)
    COLOR           INTEGER      DEFAULT 0,       -- 4:Red, 5:Green, 6:Yellow, 0:자동순환

    -- MANUAL 지령 : Client UI / Display 태스크 수동 패널이 기록
    CMD_RQ_YN       VARCHAR(1)   DEFAULT 'N',     -- 'Y' = 수동지령 대기중
    CMD_RQ_ID       VARCHAR(20)  DEFAULT 'DATA',  -- 'DATA' | 'CLEAR'
    CMD_DATA        VARCHAR(20),                  -- 수동으로 표시할 내용
    CMD_COLOR       INTEGER      DEFAULT 6,       -- 수동지령 색상

    -- 전송결과 : WCS_TASK_Display 가 기록
    SEND_YN         VARCHAR(1)   DEFAULT 'N',
    LAST_SENT_DATA  VARCHAR(20),
    LAST_SENT_LUGG  VARCHAR(20),
    CONNECTED_YN    VARCHAR(1)   DEFAULT 'N',

    REG_DT          TIMESTAMP    DEFAULT NOW(),
    UPD_DT          TIMESTAMP    DEFAULT NOW(),
    SEND_DT         TIMESTAMP,

    CONSTRAINT PK_DISPLAY_DATA PRIMARY KEY (WH_TYP, PLC_NO, DISP_NO)
);

-- 수동지령 폴링(DispManual)이 CMD_RQ_YN='Y' 만 훑기 때문에 부분 인덱스를 둔다.
CREATE INDEX IF NOT EXISTS IX_DISPLAY_DATA_CMD
    ON DISPLAY_DATA (WH_TYP, PLC_NO)
    WHERE CMD_RQ_YN = 'Y';


-- ---------------------------------------------------------------------
-- 2) EQP_MST : 설비 마스터
--
--    DisplayThread.Communication() 이 접속/단절 시점에 아래를 UPDATE 한다.
--      UPDATE EQP_MST SET CONNECTED_YN=..., UPD_DT=NOW(), PLC_PORT=...
--       WHERE WH_TYP=... AND EQP_TYP=... AND PLC_NO=...
--    EQP_TYP 값은 WCS_DB.INI [COMM*] EQMT 와 반드시 같아야 한다. (기본 'DISPLAY')
--
--    CV / SC 태스크와 공용 테이블이라 이미 있는 경우가 대부분이며,
--    그때는 아래 CREATE 는 아무 일도 하지 않는다.
-- ---------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS EQP_MST
(
    WH_TYP          VARCHAR(4),
    EQP_TYP         VARCHAR(10),
    PLC_NO          VARCHAR(5),
    SC_GRP_NO       VARCHAR(3),
    PLC_IP          VARCHAR(20),
    PLC_PORT_FROM   VARCHAR(5),
    PLC_PORT_TO     VARCHAR(5),
    SOCK_TIMEOUT    VARCHAR(5),
    RETRY_YN        VARCHAR(1),
    USE_YN          VARCHAR(1),
    CONNECTED_YN    VARCHAR(1),
    UPD_DT          TIMESTAMP,
    REMARKS         VARCHAR(200),
    PLC_PORT        VARCHAR,
    PRIORITY        VARCHAR(3),
    BATCH           VARCHAR(255),
    PROCESS         VARCHAR(255)
);


-- ---------------------------------------------------------------------
-- 3) WCS_LOG_PGR : 프로그램 감사 로그
--    DisplayThread.InsertWcsLogPgr() 가 INSERT 한다.
-- ---------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS WCS_LOG_PGR
(
    WH_TYP          VARCHAR(20),
    INS_DT          TIMESTAMP,
    LOG_SEQ         INTEGER,
    LUGG_NO         VARCHAR(255),
    BCR_BOTTOM      VARCHAR(255),
    BCR_TOP         VARCHAR(255),
    PGR_NM          VARCHAR(255),
    LOG_KOR         VARCHAR(255),
    TRACK_FROM      VARCHAR(255),
    TRACK_TO        VARCHAR(255),
    JOB_STA         VARCHAR(255),
    RQ_INS_ID       VARCHAR(255),
    RQ_INS_DT       TIMESTAMP,
    EQP_TYP         VARCHAR(255)
);


-- ---------------------------------------------------------------------
-- 4) LOG_SEQ : WCS_LOG_PGR.LOG_SEQ 채번 시퀀스
--    INSERT 문에서 NEXTVAL('LOG_SEQ') 로 직접 참조한다.
-- ---------------------------------------------------------------------
CREATE SEQUENCE IF NOT EXISTS LOG_SEQ START 1;


-- ---------------------------------------------------------------------
-- 확인
-- ---------------------------------------------------------------------
-- SELECT column_name, data_type FROM information_schema.columns
--  WHERE table_name = 'display_data' ORDER BY ordinal_position;

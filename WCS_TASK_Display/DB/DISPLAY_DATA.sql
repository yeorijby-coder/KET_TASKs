-- =====================================================================
--  DISPLAY_DATA  (PostgreSQL)
--  New table for WCS_TASK_Display (To-Be, DB-centric architecture).
--
--  In the legacy MFC system the display board content came from in-memory
--  conveyor track info (CCvTrackInfo). In the new architecture the content
--  is held here and written by the CV task / scheduler / Client; the
--  WCS_TASK_Display task reads this table and pushes the content to the
--  physical display board over TCP.
--
--  Key (WH_TYP, PLC_NO, DISP_NO)
--    WH_TYP : warehouse type            (matches CV_DATA / EQP_MST)
--    PLC_NO : display controller id     (= [COMM*] PLC_NO in WCS_DB.INI)
--    DISP_NO : display number on the controller (1-based)
-- =====================================================================
CREATE TABLE IF NOT EXISTS DISPLAY_DATA
(
    WH_TYP          VARCHAR(10)  NOT NULL,
    PLC_NO          VARCHAR(10)  NOT NULL,
    DISP_NO          VARCHAR(10)  NOT NULL,
    TRACK_NO        VARCHAR(20),                 -- mapped conveyor track (reference)

    -- AUTO display content (written by CV task / scheduler)
    DISP_DATA       VARCHAR(20),                 -- product/luggage text to show (<=8 used)
    LUGG_NO         VARCHAR(20),                 -- current luggage no (change-detection key)
    COLOR           INTEGER      DEFAULT 0,       -- 4=Red,5=Green,6=Yellow, 0=auto cycle

    -- MANUAL command (written by Client UI / WCS_TASK_Display manual panel)
    CMD_RQ_YN       CHAR(1)      DEFAULT 'N',     -- 'Y' = manual command pending
    CMD_RQ_ID       VARCHAR(20),                 -- 'DATA' | 'CLEAR'
    CMD_DATA        VARCHAR(20),
    CMD_COLOR       INTEGER      DEFAULT 6,

    -- status written back by WCS_TASK_Display
    SEND_YN         CHAR(1)      DEFAULT 'N',
    LAST_SENT_DATA  VARCHAR(20),
    LAST_SENT_LUGG  VARCHAR(20),
    CONNECTED_YN    CHAR(1)      DEFAULT 'N',

    REG_DT          TIMESTAMP    DEFAULT NOW(),
    UPD_DT          TIMESTAMP    DEFAULT NOW(),
    SEND_DT         TIMESTAMP,

    CONSTRAINT PK_DISPLAY_DATA PRIMARY KEY (WH_TYP, PLC_NO, DISP_NO)
);

-- ---------------------------------------------------------------------
-- Seed rows for controller 01 with two displays (matches WCS_DB.INI sample:
--   CNT=2, FR_TRACK=2006, TO_TRACK=2012)
-- ---------------------------------------------------------------------
INSERT INTO DISPLAY_DATA (WH_TYP, PLC_NO, DISP_NO, TRACK_NO, DISP_DATA, LUGG_NO, COLOR, CMD_RQ_YN, SEND_YN)
VALUES ('10','01','1','2006','',NULL,0,'N','N')
ON CONFLICT (WH_TYP, PLC_NO, DISP_NO) DO NOTHING;

INSERT INTO DISPLAY_DATA (WH_TYP, PLC_NO, DISP_NO, TRACK_NO, DISP_DATA, LUGG_NO, COLOR, CMD_RQ_YN, SEND_YN)
VALUES ('10','01','2','2012','',NULL,0,'N','N')
ON CONFLICT (WH_TYP, PLC_NO, DISP_NO) DO NOTHING;

-- ---------------------------------------------------------------------
-- EQP_MST registration so Communication() (CONNECTED_YN/PLC_PORT) has a row.
-- EQP_TYP must equal the [COMM*] EQMT value ('DISPLAY').
-- Adjust column list to your existing EQP_MST schema if it differs.
-- ---------------------------------------------------------------------
-- INSERT INTO EQP_MST (WH_TYP, EQP_TYP, PLC_NO, CONNECTED_YN, PLC_PORT, REG_DT, UPD_DT)
-- VALUES ('10','DISPLAY','01','N','8001', NOW(), NOW());

-- WCS_LOG_PGR audit insert uses sequence LOG_SEQ (already present for CV/SC tasks):
-- CREATE SEQUENCE IF NOT EXISTS LOG_SEQ START 1;

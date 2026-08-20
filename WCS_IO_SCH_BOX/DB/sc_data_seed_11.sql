-- ============================================================
-- SC_DATA 11대 레코드 시딩 (KET To-Be : SC #1 ~ #11, 총 11 ea)
-- 생성: 2026-07-09
-- 기준: 기존 901/902 행 컨벤션 (WH_TYP=10, SC_GRP_NO=9, SC_TYP=SINGLE,
--       MC_NO=SC_NO, PLC_NO=SC 순번 2자리, MC_NO_NM='SC n호기')
-- 동작: SC_NO 901~911 중 없는 행만 INSERT (기존 행은 변경하지 않음 / 재실행 안전)
-- 신규 행 초기값: 유휴 정상 상태 (ONLINE/AUTO/ACTIVE/UC='1', ERR='0000',
--       포크 비움 ITN_LUGG_FK1/2='0', OD_RQ_YN='N', CMD_RQ_YN='N', SUSPEND='0')
--       ※ _RD 컬럼은 통신 Task(WCS_TASK_SC)가 PLC 접속 후 실제값으로 갱신한다.
-- 대상 DB: LGLS_MCS_IO (MS-SQL) / PostgreSQL 적용 시 [ ] 구분자만 제거
-- ============================================================

SET NOCOUNT ON;

DECLARE @i INT = 1;
WHILE @i <= 11
BEGIN
    DECLARE @SC_NO  VARCHAR(5)  = CAST(900 + @i AS VARCHAR(5));
    DECLARE @PLC_NO VARCHAR(5)  = RIGHT('0' + CAST(@i AS VARCHAR(2)), 2);
    DECLARE @NM     VARCHAR(100)= N'SC ' + CAST(@i AS VARCHAR(2)) + N'호기';

    IF NOT EXISTS (SELECT 1 FROM dbo.SC_DATA WHERE WH_TYP = '10' AND SC_NO = @SC_NO)
    BEGIN
        INSERT INTO dbo.SC_DATA
            ( WH_TYP, PLC_NO, SC_NO, SC_GRP_NO, MC_NO, MC_NO_NM, SC_TYP
            , AUTO_MODE_RD, UCSTATUS_RD, ONLINE_MODE_RD, ACTIVE_MODE_RD
            , SENSOR_FK_RD, POS_H_RD, POS_V_RD
            , ERR_CODE_RD, COMPLETE_RD, JOB_TYP_RD, JOB_TYP_OD
            , LUGG_NO_FK1_RD, LUGG_NO_FK1_OD, ITN_LUGG_FK1
            , LUGG_NO_FK2_RD, LUGG_NO_FK2_OD, ITN_LUGG_FK2
            , ERR_STA_FK1_RD, ERR_STA_FK2_RD, FORKPOS_FK1_RD, FORKPOS_FK2_RD
            , USE_FK_RD, USE_FK_OD, STOCK_MODE
            , OD_RQ_YN, OD_RQ_FLAG, CMD_RQ_YN, HOST_SEND_YN, HOST_ERR_SEND_YN
            , SUSPEND, READ_UPD_DT, WRITE_UPD_DT, OD_USER_ID
            , WRITE_CONTINUE_OD, WRITE_FLAG_OD, USER_COMMAND_OD
            , CV_WORKBENCH_RD, CV_WORKBENCH_SUB_RD, PLT_INFO_RD
            , SC_PLT_JOB_TYP_RD, SC_PLT_JOB_TYP_OD )
        VALUES
            ( '10', @PLC_NO, @SC_NO, '9', @SC_NO, @NM, 'SINGLE'
            , '1', '1', '1', '1'
            , '0', '0', '0'
            , '0000', '0', '0', '0'
            , '0000', '0000', '0'
            , '0000', '0000', '0'
            , '0', '0', '0', '0'
            , '0', '0', '0'
            --  HOST_ERR_SEND_YN 은 'Y'(보고할 에러 없음) 로 넣는다.
            --  'N' 으로 넣으면 Host TASK 가 새 행마다 에러 보고를 한 번씩 내보낸다.
            , 'N', 'N', 'N', 'N', 'Y'
            , '0', GETDATE(), GETDATE(), 'SEED'
            , '0', '0', '0'
            , '0', '0', '0'
            , '0', '0' );

        PRINT 'INSERT SC_DATA : SC_NO=' + @SC_NO;
    END

    SET @i = @i + 1;
END

SELECT COUNT(*) AS SC_DATA_CNT FROM dbo.SC_DATA WHERE WH_TYP = '10';
SELECT WH_TYP, PLC_NO, SC_NO, SC_GRP_NO, MC_NO, MC_NO_NM, SC_TYP, OD_RQ_YN, ERR_CODE_RD
  FROM dbo.SC_DATA WHERE WH_TYP = '10' ORDER BY SC_NO;

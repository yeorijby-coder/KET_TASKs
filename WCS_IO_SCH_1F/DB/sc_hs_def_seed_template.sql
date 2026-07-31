-- ============================================================
-- SC_HS_DEF 시드 템플릿 : SC 3호기(903) ~ 11호기(911) 의 1층 홈스탠드 정의
-- 생성: 2026-07-09
--
-- ※ 적용 전 반드시 현장 트랙 매핑 확인! (★현장확인)
--    현재 등록된 901/902 의 패턴(추정):
--      SC n호기 : 입고 HS(HS_NO=01) = MC 1n5 계열 (901→115, 902→125)
--                 출고 HS(HS_NO=02) = MC 1n4 계열 (901→114, 902→124)
--    이 패턴이 맞다면 903→135/134 ... 908→185/184 (CV_DATA MC_NO 는 101~188 까지).
--    909~911 은 패턴상 188 을 초과하므로 반드시 실제 트랙 번호로 대체할 것.
--
-- 컬럼 의미 (스케줄러 cThread_SCH 사용 기준):
--    SC_NO      : SC_DATA.SC_NO (901~911)
--    HS_NO      : 01=입고 HS(레거시 ECS RANK_1), 02=출고 HS(RANK_2)
--    HS_MC_NO   : 해당 HS 의 CV_DATA.MC_NO
--    DEST_DEF_DAT : 이 SC 로 입고 가능한 출발지(입출고대 MC) 목록 (참고용)
--    HS_USE_YN  : 'Y' 만 스케줄러가 사용
-- ============================================================

SET NOCOUNT ON;

-- ▼ 패턴 확인 후 주석 해제하여 적용 (903~908)
/*
DECLARE @i INT = 3;
WHILE @i <= 8
BEGIN
    DECLARE @SC VARCHAR(5) = CAST(900 + @i AS VARCHAR(5));
    DECLARE @HS_IN  VARCHAR(5) = CAST(100 + @i * 10 + 5 AS VARCHAR(5));  -- 입고 HS: 135,145,...,185
    DECLARE @HS_OUT VARCHAR(5) = CAST(100 + @i * 10 + 4 AS VARCHAR(5));  -- 출고 HS: 134,144,...,184

    IF NOT EXISTS (SELECT 1 FROM dbo.SC_HS_DEF WHERE WH_TYP='10' AND SC_NO=@SC AND HS_NO='01')
        INSERT INTO dbo.SC_HS_DEF (WH_TYP, SC_NO, HS_NO, HS_MC_NO, DEST_DAT_TOKEN, DEST_DEF_DAT, HS_USE_YN)
        VALUES ('10', @SC, '01', @HS_IN, ',', '102, 105', 'Y');

    IF NOT EXISTS (SELECT 1 FROM dbo.SC_HS_DEF WHERE WH_TYP='10' AND SC_NO=@SC AND HS_NO='02')
        INSERT INTO dbo.SC_HS_DEF (WH_TYP, SC_NO, HS_NO, HS_MC_NO, DEST_DAT_TOKEN, DEST_DEF_DAT, HS_USE_YN)
        VALUES ('10', @SC, '02', @HS_OUT, '', '', 'Y');

    SET @i = @i + 1;
END
*/

-- ▼ 909~911 : 실제 HS 트랙 번호 확인 후 아래 형식으로 등록 (★현장확인)
-- INSERT INTO dbo.SC_HS_DEF (WH_TYP, SC_NO, HS_NO, HS_MC_NO, DEST_DAT_TOKEN, DEST_DEF_DAT, HS_USE_YN)
-- VALUES ('10', '909', '01', '???', ',', '102, 105', 'Y');
-- VALUES ('10', '909', '02', '???', '',  '',         'Y');

SELECT SC_NO, HS_NO, HS_MC_NO, HS_USE_YN FROM dbo.SC_HS_DEF ORDER BY SC_NO, HS_NO;

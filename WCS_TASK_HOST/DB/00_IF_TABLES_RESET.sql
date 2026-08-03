-- =====================================================================
--  WMS <-> WCS 인터페이스 테이블 정합 스크립트  (PostgreSQL)
--
--  대상 : IF_LUGG_STA / IF_MC_STA / IF_REQ_MST 와 각 히스토리 테이블
--
--  근거
--    1) [09]Interface목록서[ECS-자동창고].doc   - 원본 ECS<->WMS 소켓 프로토콜
--    2) 일반비_한국단자_WCS인터페이스정의서.pptx - 현재 태스크 설정
--    3) WCS_IO_SCH_*/CLS/IOSchDB.cs             - 실제로 이 테이블을 읽고 쓰는 소스
--
--  ★★ 주의 ★★
--    이 스크립트는 대상 테이블을 DROP 후 재생성한다. 기존 데이터가 사라진다.
--    운영 DB 에서는 반드시 백업 후 실행할 것.
--    기존 데이터를 살리면서 부족한 것만 채우려면 01_IF_TABLES_ALTER.sql 을 쓴다.
--
--  실행 : psql -h localhost -p 5432 -U KET_WCS -d KET_WCS -f 00_IF_TABLES_RESET.sql
-- =====================================================================

BEGIN;

DROP TABLE IF EXISTS IF_LUGG_STA;
DROP TABLE IF EXISTS IF_LUGG_STA_HIS;
DROP TABLE IF EXISTS IF_MC_STA;
DROP TABLE IF EXISTS IF_MC_STA_HIS;
DROP TABLE IF EXISTS IF_REQ_MST;
DROP TABLE IF EXISTS IF_REQ_MST_HIS;


-- ---------------------------------------------------------------------
-- 1) IF_LUGG_STA : 작업상태 정보  [양방향]
--
--    원본 프로토콜 대응
--      WMS -> WCS  'O' 작업 지시      (Job Order Directive)
--      WMS -> WCS  'R' 재작업 지시    (Alternative Location Directive)
--      WCS -> WMS  'F' 작업 완료 보고 (Job Complete Report)
--      WCS -> WMS  'E' 에러 보고      (Error Report)
--
--    WH_TYP 은 원본 프로토콜의 WareHouse Define 에 해당한다.
--      A : PalletRack 자동창고 / B : P-BoxRack 자동창고
--      IOSchDB.cs 가 모든 조회에서 WH_TYP 을 조건으로 쓰므로 반드시 있어야 한다.
-- ---------------------------------------------------------------------
CREATE TABLE IF_LUGG_STA
(
    CRT_DATE        VARCHAR(8)   NOT NULL,   -- 생성일자 YYYYMMDD
    CRT_TIME        VARCHAR(6)   NOT NULL,   -- 생성시간 HH24MISS
    LUGGNO          VARCHAR(4)   NOT NULL,   -- 작업번호 (프로토콜 Luggage No. Num 4)
    WH_TYP          VARCHAR(4)   NOT NULL,   -- 창고구분 (프로토콜 WareHouse Define)

    JOB_KIND        VARCHAR(7),              -- 작업구분 1:입고 2:출고 3:PICKING출고 4:RACK이동 5:호기간이동 6:이동
    LD_CTN_NO       VARCHAR(20),             -- PLT 번호 (프로토콜 Pallet No Char 20)
    FROM_CV_NO      VARCHAR(10),             -- 시작작업대 (Source Stn.)
    TO_CV_NO        VARCHAR(10),             -- 종료작업대 (Dest. Stn.)
    FROM_SC_NO      VARCHAR(10),             -- 출발 크레인 호기
    TO_SC_NO        VARCHAR(10),             -- 도착 크레인 호기
    FROM_AREA       VARCHAR(10),             -- 출발 LOCATION (Source Bank/Bay/Level)
    TO_AREA         VARCHAR(10),             -- 도착 LOCATION (Dest. Bank/Bay/Level)

    -- 상태정보
    --   00:작업대기 01:작업수신 02:CV지시 03:CV작업중 09:CV완료
    --   [SC 1호기] 11:SC지시 12:SC작업중 19:SC완료
    --   [SC 2호기] 21:SC지시 22:SC작업중 29:SC완료
    --   90:정상완료 91:강제완료(ECS) 98:이상처리(이중입고 재지정, 공출고 삭제) 99:ERROR
    WORK_STA        VARCHAR(7),

    ST_ISHIGH       VARCHAR(1),              -- 화물 높이 구분 (미사용, 프로토콜 Size Checker 값)
    ERRCODE         VARCHAR(4),              -- 오류코드 (프로토콜 Error Code Char 4)
    PRIORITY        VARCHAR(3),              -- 우선순위 기본 100, 클수록 우선 (프로토콜 Priority Num 3)
    IF_STATUS       VARCHAR(1)   DEFAULT 'N',-- 처리상태 N:미처리 Y:정상 E:에러
    UPD_DT          TIMESTAMP,               -- 수정일시
    UPD_USER_ID     VARCHAR(10),             -- 수정 시스템 (WMS, WCS, IO_TASK ...)
    PRDCT_NM        VARCHAR(120),            -- 제품이름 (프로토콜 Product ID / User Data)

    CONSTRAINT PK_IF_LUGG_STA PRIMARY KEY (WH_TYP, LUGGNO)
);

CREATE INDEX IX_IF_LUGG_STA_STATUS ON IF_LUGG_STA (IF_STATUS);
CREATE INDEX IX_IF_LUGG_STA_WORK   ON IF_LUGG_STA (WH_TYP, WORK_STA);


-- ---------------------------------------------------------------------
-- 2) IF_MC_STA : 설비상태  [WCS -> WMS]
--
--    원본 프로토콜 'S' 상태 보고 (Status Report) 에 대응한다.
--      Device Class 1:S/C 2:C/V 3:LGV 4:RGV 5:ROBOT 6:BCR  -> MC_TYP
--      Device No.                                          -> MC_NO
--      Status                                              -> SC_STA / CV_STA / MC_STA
--      Luggage No.                                         -> LUGG_NO
--
--    ★ 현재 운영 DB 의 IF_MC_STA 에는 LUGG_NO 컬럼이 없다.
--      프로토콜의 Status Report 는 Luggage No. 를 함께 보내므로 반드시 필요하다.
-- ---------------------------------------------------------------------
CREATE TABLE IF_MC_STA
(
    MC_TYP          VARCHAR(10)  NOT NULL,   -- 설비구분 SC, CV, BCR
    MC_NO           VARCHAR(10)  NOT NULL,   -- 설비번호 (SC 호기 / CV 트랙번호)
    WH_TYP          VARCHAR(4)   NOT NULL,   -- 창고구분

    CV_STA          VARCHAR(32),             -- 작업대별 화물 유무  0:없음 1:있음
    LUGG_NO         VARCHAR(4),              -- 작업번호 (프로토콜 Status Report 의 Luggage No.)
    CV_IO_MODE      VARCHAR(32),             -- 입출고 스위치 모드 0:없음 1:입고 2:출고 (미사용)
    MC_STA          VARCHAR(32),             -- 입고 가능여부 1:가능 0:불가능
    -- SC 상태 0:정상IDLE 1:입고중 2:출고중 3:RackToRack 4:Online아님 5:에러
    --         6:입고중지 7:출고중지 8:입출고중지
    SC_STA          VARCHAR(32),
    MC_USE_DEF      VARCHAR(10),             -- 설비사용정의
    ST_ISHIGH       VARCHAR(32),             -- 화물높이 (미사용)
    BCR_DATA        VARCHAR(20),             -- BCR 값 (프로토콜 'B' 보고의 Reading Data Char 20)
    PA_REQ_STA      VARCHAR(1),              -- 작업요청여부 (미사용)
    IF_STATUS       VARCHAR(1)   DEFAULT 'N',-- 처리상태 N:미처리 Y:정상 E:에러
    UPD_DT          TIMESTAMP,               -- 수정일시

    CONSTRAINT PK_IF_MC_STA PRIMARY KEY (WH_TYP, MC_TYP, MC_NO)
);

CREATE INDEX IX_IF_MC_STA_STATUS ON IF_MC_STA (IF_STATUS);


-- ---------------------------------------------------------------------
-- 3) IF_REQ_MST : 작업요청  [WCS -> WMS]
--
--    ★ 현재 운영 DB 에 이 테이블이 아예 없다.
--      IOSchDB.cs 가 16곳에서 참조하므로 지금은 그 경로가 전부 실패한다.
--
--    원본 프로토콜 대응
--      'N' 공파렛트 입고 요구 보고 (PalletRack)  - P/M Station No 로 요청
--      'L' 정상 입고 요구 보고     (P-BoxRack)   - 자동입고대기 #1/#2 의 작업번호로 요청
--
--    그래서 MSG_TYP 이 'N' 이면 STN_NO 로, 'L' 이면 LUGG_NO1/LUGG_NO2 로 식별한다.
--    (IOSchDB.cs 의 조회 조건과 동일)
-- ---------------------------------------------------------------------
CREATE TABLE IF_REQ_MST
(
    CRT_DATE        VARCHAR(8)   NOT NULL,   -- 생성일자 YYYYMMDD
    CRT_TIME        VARCHAR(6)   NOT NULL,   -- 생성시간 HH24MISS
    WH_TYP          VARCHAR(4)   NOT NULL,   -- 창고구분
    MSG_TYP         VARCHAR(1)   NOT NULL,   -- L:P-BOX 입고 요청, N:공파레트 입고 요청

    LUGG_NO1        VARCHAR(4),              -- 작업번호 #1 (자동입고대기 #1)
    LUGG_NO2        VARCHAR(4),              -- 작업번호 #2 (자동입고대기 #2, 없으면 0)
    JOB_KIND        VARCHAR(7),              -- 작업구분
    STN_NO          VARCHAR(3),              -- Station 번호 (P/M Station No)
    IF_STATUS       VARCHAR(1)   DEFAULT 'N',-- 처리상태 N:미처리 Y:정상 E:에러
    UPD_DT          TIMESTAMP,               -- 수정일시
    UPD_USER_ID     VARCHAR(10),             -- 수정 시스템

    CONSTRAINT PK_IF_REQ_MST PRIMARY KEY (WH_TYP, MSG_TYP, STN_NO, LUGG_NO1, LUGG_NO2)
);

CREATE INDEX IX_IF_REQ_MST_STATUS ON IF_REQ_MST (IF_STATUS);


-- ---------------------------------------------------------------------
-- 4) 히스토리 테이블
--    인터페이스용이 아니라 자체 이력 보관용이다.
--    원본 테이블과 컬럼을 동일하게 두되 PK 는 두지 않는다.(같은 키가 여러 번 쌓임)
-- ---------------------------------------------------------------------
CREATE TABLE IF_LUGG_STA_HIS (LIKE IF_LUGG_STA);
CREATE TABLE IF_MC_STA_HIS   (LIKE IF_MC_STA);
CREATE TABLE IF_REQ_MST_HIS  (LIKE IF_REQ_MST);

-- 히스토리는 이력이므로 언제 쌓였는지 남긴다
ALTER TABLE IF_LUGG_STA_HIS ADD COLUMN HIS_DT TIMESTAMP DEFAULT NOW();
ALTER TABLE IF_MC_STA_HIS   ADD COLUMN HIS_DT TIMESTAMP DEFAULT NOW();
ALTER TABLE IF_REQ_MST_HIS  ADD COLUMN HIS_DT TIMESTAMP DEFAULT NOW();

CREATE INDEX IX_IF_LUGG_STA_HIS_DT ON IF_LUGG_STA_HIS (HIS_DT);
CREATE INDEX IX_IF_MC_STA_HIS_DT   ON IF_MC_STA_HIS   (HIS_DT);
CREATE INDEX IX_IF_REQ_MST_HIS_DT  ON IF_REQ_MST_HIS  (HIS_DT);

COMMIT;


-- =====================================================================
--  확인
-- =====================================================================
-- SELECT table_name, count(*) AS 컬럼수
--   FROM information_schema.columns
--  WHERE table_name IN ('if_lugg_sta','if_mc_sta','if_req_mst',
--                       'if_lugg_sta_his','if_mc_sta_his','if_req_mst_his')
--  GROUP BY table_name ORDER BY table_name;

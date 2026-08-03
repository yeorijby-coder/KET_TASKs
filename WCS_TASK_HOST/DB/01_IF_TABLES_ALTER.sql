-- =====================================================================
--  WMS <-> WCS 인터페이스 테이블 : 기존 데이터를 살리면서 부족한 것만 채운다
--
--  운영 DB 처럼 이미 데이터가 들어있는 경우 이 스크립트를 쓴다.
--  테이블을 통째로 다시 만들려면 00_IF_TABLES_RESET.sql 을 쓴다.(데이터 삭제됨)
--
--  실행 : psql -h localhost -p 5432 -U KET_WCS -d KET_WCS -f 01_IF_TABLES_ALTER.sql
--
--  현재 운영 DB 와 원본 프로토콜/소스를 대조해 나온 차이
--    (1) IF_REQ_MST     : 테이블 자체가 없다. IOSchDB.cs 가 16곳에서 참조한다.
--    (2) IF_REQ_MST_HIS : 없다.
--    (3) IF_MC_STA      : LUGG_NO 컬럼이 없다.
--                         원본 프로토콜 'S' 상태보고가 Luggage No. 를 함께 보낸다.
--    (4) IF_MC_STA      : WH_TYP 컬럼이 없다. IF_LUGG_STA 에는 있다.
--                         프로토콜의 WareHouse Define(A/B) 을 구분하려면 필요하다.
-- =====================================================================

BEGIN;

-- ---------------------------------------------------------------------
-- (3)(4) IF_MC_STA 부족 컬럼 추가
-- ---------------------------------------------------------------------
ALTER TABLE IF_MC_STA ADD COLUMN IF NOT EXISTS LUGG_NO VARCHAR(4);
ALTER TABLE IF_MC_STA ADD COLUMN IF NOT EXISTS WH_TYP  VARCHAR(4);

-- 기존 행은 창고구분을 알 수 없으므로 기본값을 넣어둔다.(현장 값에 맞게 조정할 것)
UPDATE IF_MC_STA SET WH_TYP = '10' WHERE WH_TYP IS NULL;

-- 히스토리도 본 테이블과 컬럼을 맞춘다.(IOSchDB.cs 가 행을 그대로 복사한다)
ALTER TABLE IF_MC_STA_HIS ADD COLUMN IF NOT EXISTS LUGG_NO VARCHAR(4);
ALTER TABLE IF_MC_STA_HIS ADD COLUMN IF NOT EXISTS WH_TYP  VARCHAR(4);


-- ---------------------------------------------------------------------
-- (1) IF_REQ_MST 신규 생성
--     MSG_TYP 'N' 공파렛트 입고 요구 / 'L' P-BOX 정상 입고 요구
-- ---------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS IF_REQ_MST
(
    CRT_DATE        VARCHAR(8)   NOT NULL,
    CRT_TIME        VARCHAR(6)   NOT NULL,
    WH_TYP          VARCHAR(4)   NOT NULL,
    MSG_TYP         VARCHAR(1)   NOT NULL,
    LUGG_NO1        VARCHAR(4),
    LUGG_NO2        VARCHAR(4),
    JOB_KIND        VARCHAR(7),
    STN_NO          VARCHAR(3),
    IF_STATUS       VARCHAR(1)   DEFAULT 'N',
    UPD_DT          TIMESTAMP,
    UPD_USER_ID     VARCHAR(10),
    CONSTRAINT PK_IF_REQ_MST PRIMARY KEY (WH_TYP, MSG_TYP, STN_NO, LUGG_NO1, LUGG_NO2)
);

CREATE INDEX IF NOT EXISTS IX_IF_REQ_MST_STATUS ON IF_REQ_MST (IF_STATUS);


-- ---------------------------------------------------------------------
-- (2) IF_REQ_MST_HIS 신규 생성
-- ---------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS IF_REQ_MST_HIS
(
    CRT_DATE        VARCHAR(8),
    CRT_TIME        VARCHAR(6),
    WH_TYP          VARCHAR(4),
    MSG_TYP         VARCHAR(1),
    LUGG_NO1        VARCHAR(4),
    LUGG_NO2        VARCHAR(4),
    JOB_KIND        VARCHAR(7),
    STN_NO          VARCHAR(3),
    IF_STATUS       VARCHAR(1),
    UPD_DT          TIMESTAMP,
    UPD_USER_ID     VARCHAR(10),
    HIS_DT          TIMESTAMP DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS IX_IF_REQ_MST_HIS_DT ON IF_REQ_MST_HIS (HIS_DT);


-- ---------------------------------------------------------------------
-- 처리상태 인덱스 (명세서 Indx = Y)
-- ---------------------------------------------------------------------
CREATE INDEX IF NOT EXISTS IX_IF_LUGG_STA_STATUS ON IF_LUGG_STA (IF_STATUS);
CREATE INDEX IF NOT EXISTS IX_IF_MC_STA_STATUS   ON IF_MC_STA   (IF_STATUS);

COMMIT;


-- =====================================================================
--  확인
-- =====================================================================
-- SELECT table_name, count(*) AS 컬럼수 FROM information_schema.columns
--  WHERE table_name IN ('if_lugg_sta','if_mc_sta','if_req_mst',
--                       'if_lugg_sta_his','if_mc_sta_his','if_req_mst_his')
--  GROUP BY table_name ORDER BY table_name;

-- SELECT column_name FROM information_schema.columns
--  WHERE table_name='if_mc_sta' AND column_name IN ('lugg_no','wh_typ');

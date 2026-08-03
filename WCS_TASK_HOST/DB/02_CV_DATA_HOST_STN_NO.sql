-- =====================================================================
--  CV_DATA 에 HOST_STN_NO(작업대 번호) 추가  (PostgreSQL)
--
--  왜 필요한가
--    WMS(HOST) 와 주고받는 전문은 "작업대 번호"(101, 207, 221 ...)를 쓴다.
--    반면 WCS 내부는 트랙 번호로 설비를 식별한다.
--      CV_DATA.TRACK_NO : 4자리 내부 트랙번호   (원본 CNV_STN_POS_* 값)
--      CV_DATA.MC_NO    : 3자리 설비번호        (TRACK_NO 에서 가운데 0 을 뺀 표기)
--    지금까지 이 둘과 작업대 번호를 잇는 정보가 어디에도 없어서
--    HOST 태스크가 작업대 번호를 상수로 들고 있어야 했다.
--    그래서 CV_DATA 에 HOST_STN_NO 를 두어 한곳에서 매칭한다.
--
--  매핑 근거
--    Common/Include/Ecs/EcsDef.h 의 ECS_STN_POS_* (작업대) 와
--    CNV_STN_POS_* (트랙) 짝을 그대로 옮겼다.
--
--  실행 : psql -h 127.0.0.1 -p 5432 -U KET_WCS -d KET_WCS -f 02_CV_DATA_HOST_STN_NO.sql
--
--  ※ H/S(크레인 홈스테이션 1~11, 트랙 201~211) 는 작업대가 아니라 제외했다.
--    필요하면 맨 아래 주석 블록을 참고할 것.
-- =====================================================================

BEGIN;

-- ---------------------------------------------------------------------
-- 1) 컬럼 추가
-- ---------------------------------------------------------------------
ALTER TABLE CV_DATA ADD COLUMN IF NOT EXISTS HOST_STN_NO VARCHAR(5);

COMMENT ON COLUMN CV_DATA.HOST_STN_NO IS 'WMS(HOST) 전문에서 쓰는 작업대 번호. 없으면 HOST 미보고 대상';

-- 작업대 번호로 조회하는 경로가 생기므로 인덱스를 둔다
CREATE INDEX IF NOT EXISTS IX_CV_DATA_HOST_STN ON CV_DATA (WH_TYP, HOST_STN_NO);


-- ---------------------------------------------------------------------
-- 2) 매핑 입력  (TRACK_NO 기준)
--
--    작업대 | 트랙 | MC_NO | 이름
-- ---------------------------------------------------------------------
UPDATE CV_DATA SET HOST_STN_NO = '101' WHERE TRIM(TRACK_NO) = '2017';  -- 1층 입고대 #1
UPDATE CV_DATA SET HOST_STN_NO = '102' WHERE TRIM(TRACK_NO) = '2024';  -- 1층 입고대 #2
UPDATE CV_DATA SET HOST_STN_NO = '103' WHERE TRIM(TRACK_NO) = '2006';  -- 1층 Unit 출고대 #1
UPDATE CV_DATA SET HOST_STN_NO = '104' WHERE TRIM(TRACK_NO) = '2012';  -- 1층 Unit 출고대 #2
UPDATE CV_DATA SET HOST_STN_NO = '105' WHERE TRIM(TRACK_NO) = '2101';  -- 1층 Unit 출고대 Group(가상)
UPDATE CV_DATA SET HOST_STN_NO = '107' WHERE TRIM(TRACK_NO) = '2018';  -- 1층 Size Checker #1
UPDATE CV_DATA SET HOST_STN_NO = '108' WHERE TRIM(TRACK_NO) = '2025';  -- 1층 Size Checker #2

UPDATE CV_DATA SET HOST_STN_NO = '151' WHERE TRIM(TRACK_NO) = '2004';
UPDATE CV_DATA SET HOST_STN_NO = '152' WHERE TRIM(TRACK_NO) = '2008';
UPDATE CV_DATA SET HOST_STN_NO = '153' WHERE TRIM(TRACK_NO) = '2016';
UPDATE CV_DATA SET HOST_STN_NO = '154' WHERE TRIM(TRACK_NO) = '2020';
UPDATE CV_DATA SET HOST_STN_NO = '155' WHERE TRIM(TRACK_NO) = '2059';
UPDATE CV_DATA SET HOST_STN_NO = '156' WHERE TRIM(TRACK_NO) = '2049';
UPDATE CV_DATA SET HOST_STN_NO = '157' WHERE TRIM(TRACK_NO) = '5006';
UPDATE CV_DATA SET HOST_STN_NO = '158' WHERE TRIM(TRACK_NO) = '5012';
UPDATE CV_DATA SET HOST_STN_NO = '159' WHERE TRIM(TRACK_NO) = '5018';
UPDATE CV_DATA SET HOST_STN_NO = '160' WHERE TRIM(TRACK_NO) = '5024';
UPDATE CV_DATA SET HOST_STN_NO = '171' WHERE TRIM(TRACK_NO) = '2032';

UPDATE CV_DATA SET HOST_STN_NO = '200' WHERE TRIM(TRACK_NO) = '3011';  -- 3층 BOX 보충 피킹대
UPDATE CV_DATA SET HOST_STN_NO = '201' WHERE TRIM(TRACK_NO) = '3009';  -- 3층 피킹대 #1
UPDATE CV_DATA SET HOST_STN_NO = '202' WHERE TRIM(TRACK_NO) = '3006';  -- 3층 피킹대 #2
UPDATE CV_DATA SET HOST_STN_NO = '203' WHERE TRIM(TRACK_NO) = '3003';  -- 3층 피킹대 #3
UPDATE CV_DATA SET HOST_STN_NO = '204' WHERE TRIM(TRACK_NO) = '3100';  -- 3층 피킹대 Group(가상)
UPDATE CV_DATA SET HOST_STN_NO = '205' WHERE TRIM(TRACK_NO) = '3012';  -- 3층 Size Checker #1
UPDATE CV_DATA SET HOST_STN_NO = '206' WHERE TRIM(TRACK_NO) = '3004';  -- 3층 Size Checker #2
UPDATE CV_DATA SET HOST_STN_NO = '207' WHERE TRIM(TRACK_NO) = '3001';  -- 3층 Pallet Magazine
UPDATE CV_DATA SET HOST_STN_NO = '208' WHERE TRIM(TRACK_NO) = '3057';  -- 3층 피킹대 #4
UPDATE CV_DATA SET HOST_STN_NO = '209' WHERE TRIM(TRACK_NO) = '3058';  -- 3층 Size Checker #3
UPDATE CV_DATA SET HOST_STN_NO = '212' WHERE TRIM(TRACK_NO) = '6051';  -- 3층 피킹대 #5
UPDATE CV_DATA SET HOST_STN_NO = '213' WHERE TRIM(TRACK_NO) = '6046';  -- 3층 피킹대 #6
UPDATE CV_DATA SET HOST_STN_NO = '214' WHERE TRIM(TRACK_NO) = '6052';  -- 3층 Size Checker #4
UPDATE CV_DATA SET HOST_STN_NO = '215' WHERE TRIM(TRACK_NO) = '6047';  -- 3층 Size Checker #6

UPDATE CV_DATA SET HOST_STN_NO = '211' WHERE TRIM(TRACK_NO) = '4007';  -- 보충 입고대
UPDATE CV_DATA SET HOST_STN_NO = '221' WHERE TRIM(TRACK_NO) = '4019';  -- 자동 입고 대기 #1
UPDATE CV_DATA SET HOST_STN_NO = '222' WHERE TRIM(TRACK_NO) = '4020';  -- 자동 입고 대기 #2
UPDATE CV_DATA SET HOST_STN_NO = '231' WHERE TRIM(TRACK_NO) = '4028';  -- BCR 이동 지시대
UPDATE CV_DATA SET HOST_STN_NO = '241' WHERE TRIM(TRACK_NO) = '4034';  -- BCR 이동 도착대 #1
UPDATE CV_DATA SET HOST_STN_NO = '242' WHERE TRIM(TRACK_NO) = '4031';  -- BCR 이동 도착대 #2
UPDATE CV_DATA SET HOST_STN_NO = '251' WHERE TRIM(TRACK_NO) = '4015';  -- Picking 출고대

COMMIT;


-- =====================================================================
--  확인
-- =====================================================================

-- [확인] 매핑된 작업대 목록
-- SELECT HOST_STN_NO, TRACK_NO, MC_NO, MC_NO_NM
--   FROM CV_DATA
--  WHERE HOST_STN_NO IS NOT NULL
--  ORDER BY HOST_STN_NO::INTEGER;

-- [확인] 매핑되지 않은 트랙 (HOST 보고 대상이 아니면 정상)
-- SELECT TRACK_NO, MC_NO, MC_NO_NM FROM CV_DATA WHERE HOST_STN_NO IS NULL ORDER BY TRACK_NO;

-- [확인] TRACK_NO 와 MC_NO 의 관계가 규칙대로인지 (4자리 -> 가운데 0 제거)
-- SELECT TRACK_NO, MC_NO FROM CV_DATA
--  WHERE LENGTH(TRIM(TRACK_NO)) = 4
--    AND SUBSTR(TRIM(TRACK_NO),2,1) = '0'
--    AND TRIM(MC_NO) <> SUBSTR(TRIM(TRACK_NO),1,1) || SUBSTR(TRIM(TRACK_NO),3);


-- =====================================================================
--  H/S(크레인 홈스테이션) 도 필요하다면
--    작업대 1~11 이 트랙 201~211 에 대응한다.
--    다만 이 트랙번호는 CV 트랙 번호대와 겹칠 수 있어 기본 제외했다.
--    현장 구성을 확인한 뒤에만 적용할 것.
-- =====================================================================
-- UPDATE CV_DATA SET HOST_STN_NO = '1'  WHERE TRIM(TRACK_NO) = '201';
-- UPDATE CV_DATA SET HOST_STN_NO = '2'  WHERE TRIM(TRACK_NO) = '202';
-- UPDATE CV_DATA SET HOST_STN_NO = '3'  WHERE TRIM(TRACK_NO) = '203';
-- UPDATE CV_DATA SET HOST_STN_NO = '4'  WHERE TRIM(TRACK_NO) = '204';
-- UPDATE CV_DATA SET HOST_STN_NO = '5'  WHERE TRIM(TRACK_NO) = '205';
-- UPDATE CV_DATA SET HOST_STN_NO = '6'  WHERE TRIM(TRACK_NO) = '206';
-- UPDATE CV_DATA SET HOST_STN_NO = '7'  WHERE TRIM(TRACK_NO) = '207';
-- UPDATE CV_DATA SET HOST_STN_NO = '8'  WHERE TRIM(TRACK_NO) = '208';
-- UPDATE CV_DATA SET HOST_STN_NO = '9'  WHERE TRIM(TRACK_NO) = '209';
-- UPDATE CV_DATA SET HOST_STN_NO = '10' WHERE TRIM(TRACK_NO) = '210';
-- UPDATE CV_DATA SET HOST_STN_NO = '11' WHERE TRIM(TRACK_NO) = '211';

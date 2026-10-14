-- 테스트 서버(192.168.219.59) 적용 SQL  2026-10-14
-- 대상: KET_WCS  (psql -h 192.168.219.59 -U KET_WCS -d KET_WCS -f 이파일)
-- 1) CV_TASK 가 CvTrackingWrite 에서 읽는 CV_DATA.TRACKING_WRITE_YN 컬럼이 없어 매 주기 조회 오류가 난다.
--    (DeviceMap 컬럼은 CV_TASK 가 스스로 만들지만 이 컬럼은 DeviceMap 밖이라 손으로 넣어야 한다)
BEGIN;
ALTER TABLE cv_data ADD COLUMN IF NOT EXISTS tracking_write_yn VARCHAR(1) DEFAULT 'N';
UPDATE cv_data SET tracking_write_yn = 'N' WHERE tracking_write_yn IS NULL;
COMMIT;

-- 2) (선택) P-BOX 피킹대 415(=251) 를 출고대로도 표시 (STN_KIND 64 -> 66 = 도착대|출고대)
--    HOST_TASK 의 작업대 상태 보고는 (STN_KIND & 3) <> 0 인 트랙만 보내므로, 64 인 채로는
--    415 상태가 HOST 로 안 올라간다. 작업 흐름(이동/입고/출고 완료보고)에는 영향 없다.
--    현장과 맞춰 볼 때까지는 주석으로 둔다. 필요하면 주석을 풀어 실행.
-- UPDATE cv_data SET stn_kind = '66' WHERE wh_typ='20' AND plc_no='04' AND mc_no='415' AND stn_kind='64';

SELECT mc_no, stn_kind, tracking_write_yn FROM cv_data WHERE plc_no='04' AND mc_no IN ('415','419','420') ORDER BY 1;

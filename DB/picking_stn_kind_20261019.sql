-- 3층 피킹대의 STN_KIND 를 208 / 251 과 같게(115 = 입고대+출고대+16+32+도착대) 맞춘다.   2026-10-19
-- 현장에서 208 을 115 로 맞춰 시험한 값이다. 이렇게 하면 HOST 상태 보고(S)가 피킹대를 보고 대상에 넣고,
-- 도착만 해도 화물감지(센서)로 상태가 올라간다.
-- 여러 번 실행해도 된다.
SELECT host_stn_no, track_no, stn_kind FROM cv_data
 WHERE wh_typ = '10' AND host_stn_no IN ('201','202','203','212','213','208') ORDER BY host_stn_no;

UPDATE cv_data SET stn_kind = '115'
 WHERE wh_typ = '10' AND host_stn_no IN ('201','202','203','212','213') AND stn_kind <> '115';

SELECT host_stn_no, track_no, stn_kind FROM cv_data
 WHERE wh_typ = '10' AND host_stn_no IN ('201','202','203','212','213','208') ORDER BY host_stn_no;

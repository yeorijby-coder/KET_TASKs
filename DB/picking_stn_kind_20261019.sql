-- 3층 피킹대와 사이즈체크의 STN_KIND 를 맞춘다.   2026-10-19
--   피킹대   200,201,202,203,212,213 : 115 (입고대+출고대+16+32+도착대)  - 208 / 251 과 같다
--   사이즈체크 205,206,209,214,215    : 65  (입고대+도착대)                - 209 와 같다
-- 이렇게 하면 HOST 상태 보고(S)가 이 작업대들을 보고 대상에 넣고,
-- 도착만 해도 화물감지(센서) / 사이즈 값이 올라간다.
-- 여러 번 실행해도 된다.
SELECT host_stn_no, track_no, stn_kind FROM cv_data
 WHERE wh_typ = '10' AND host_stn_no IN ('200','201','202','203','212','213','208','205','206','209','214','215')
 ORDER BY host_stn_no;

UPDATE cv_data SET stn_kind = '115'
 WHERE wh_typ = '10' AND host_stn_no IN ('200','201','202','203','212','213') AND stn_kind <> '115';

UPDATE cv_data SET stn_kind = '65'
 WHERE wh_typ = '10' AND host_stn_no IN ('205','206','209','214','215') AND stn_kind <> '65';

SELECT host_stn_no, track_no, stn_kind FROM cv_data
 WHERE wh_typ = '10' AND host_stn_no IN ('200','201','202','203','212','213','208','205','206','209','214','215')
 ORDER BY host_stn_no;

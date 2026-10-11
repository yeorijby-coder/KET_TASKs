-- =====================================================================
--  COMMON_CODE : 창고구분(WH_TYP) 이름을 현장 이름으로 바꾼다
--  2026-10-12
-- =====================================================================
--
--  10 번 이름이 '휴온스 C동' 으로 들어 있었다. 다른 현장에서 쓰던
--  자료가 그대로 남은 것이다. Ecs 화면의 창고 콤보에 그 이름이 그대로
--  나온다.
--
--      10  휴온스 C동   ->  파레트창고
--      20  P-BOX 창고   ->  P-BOX 창고   (그대로)
--
--  30(화성동) 40(완제품)은 이 현장 것이 아니다. 지금 클라이언트
--  (Ecs.ini WH_TYP=10)에서는 WH_TYP LIKE '%10%' 에 걸리지 않아 콤보에
--  나오지 않으므로 손대지 않고 둔다.
-- =====================================================================

BEGIN;

UPDATE common_code
   SET ccd_nm_kor = '파레트창고'
     , ccd_nm_eng = 'Pallet'
 WHERE cdx_cd = 'WH_TYP'
   AND ccd_cd = '10';

UPDATE common_code
   SET ccd_nm_kor = 'P-BOX 창고'
     , ccd_nm_eng = 'P-BOX'
 WHERE cdx_cd = 'WH_TYP'
   AND ccd_cd = '20';

COMMIT;

-- 확인 : Ecs 가 창고 콤보를 채울 때 던지는 그 질의
SELECT ccd_cd, COALESCE(ccd_nm_kor, ' ') AS ccd_nm_value
  FROM common_code
 WHERE ccd_cd_yn = 'Y'
   AND cdx_cd = 'WH_TYP'
   AND wh_typ LIKE '%10%'
 ORDER BY ccd_epr_ord;

-- =====================================================================
--  되돌리기
--    UPDATE common_code SET ccd_nm_kor='휴온스 C동', ccd_nm_eng=NULL
--     WHERE cdx_cd='WH_TYP' AND ccd_cd='10';
-- =====================================================================

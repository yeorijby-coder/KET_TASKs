-- =====================================================================
--  COMMON_CODE : 창고구분(WH_TYP)에 P-BOX 창고를 더한다
--  2026-10-12
-- =====================================================================
--
--  P-BOX 를 WH_TYP 20 으로 떼어 냈으나(pbox_warehouse_split_20261012.sql)
--  COMMON_CODE 의 WH_TYP 묶음에 20 이 없었다. 그래서 Ecs 화면의 창고
--  콤보에 P-BOX 가 아예 나오지 않고, 반자동 작업 생성에서 P-BOX 창고를
--  고를 수 없었다.
--
--  CDX_CD   = 'WH_TYP'   묶음
--  CCD_CD   = '20'       값
--  WH_TYP   = '10,20,'   이 줄이 어느 클라이언트에서 보이는지.
--                        CLib::BindCombo 가 WH_TYP LIKE '%<Ecs.ini 의 WH_TYP>%'
--                        로 가린다. 현장 Ecs.ini 는 WH_TYP=10 이라 '20,' 만
--                        적으면 P-BOX 가 콤보에 끝내 나오지 않는다.
--                        한 대의 클라이언트로 두 창고를 다 다루므로 10 도 넣는다.
-- =====================================================================

BEGIN;

UPDATE common_code
   SET ccd_cd_yn  = 'Y'
     , ccd_nm_kor = 'P-BOX 창고'
     , ccd_nm_eng = 'P-BOX'
     , wh_typ     = '10,20,'
 WHERE cdx_cd = 'WH_TYP'
   AND ccd_cd = '20';

INSERT INTO common_code (cdx_cd, ccd_cd, ccd_cd_yn, ccd_epr_ord, ccd_nm_kor, ccd_nm_eng, wh_typ)
SELECT 'WH_TYP', '20', 'Y', 3, 'P-BOX 창고', 'P-BOX', '10,20,'
 WHERE NOT EXISTS (SELECT 1 FROM common_code WHERE cdx_cd = 'WH_TYP' AND ccd_cd = '20');

COMMIT;

-- 확인
SELECT cdx_cd, ccd_cd, ccd_cd_yn, ccd_epr_ord, ccd_nm_kor, wh_typ
  FROM common_code
 WHERE cdx_cd = 'WH_TYP'
 ORDER BY ccd_epr_ord;

-- =====================================================================
--  되돌리기
--    DELETE FROM common_code WHERE cdx_cd = 'WH_TYP' AND ccd_cd = '20';
-- =====================================================================

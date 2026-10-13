-- =====================================================================
--  wcs_sf_location_format : 위치 7자리를 BB-BBB-LL 로 보여 준다
--  2026-10-13
-- =====================================================================
--
--  Ecs 클라이언트의 작업정보 / IO 이력 조회가 이 함수를 부른다.
--  (ViewJobListDlg, LogIoSkinDlg - 2026-09-21 외부 수정본부터)
--  그런데 DB 에 함수가 없어서 조회가 통째로 실패하고 "조회건수 -1" 만
--  찍혔다. 창고 콤보를 바꿔도 아무것도 안 나오던 것이 이것이다.
--
--  '1701406'   -> '17-014-06'   (뱅크2 - 베이3 - 단2, IOSchDB.cs 와 같다)
--  '01-025-01' -> 그대로        (이미 가른 것)
--  NULL / ''   -> NULL          (COALESCE 의 뒷값이 쓰인다)
--  그 밖       -> 그대로
-- =====================================================================

CREATE OR REPLACE FUNCTION public.wcs_sf_location_format(p_loc text)
RETURNS text
LANGUAGE sql
IMMUTABLE
AS $$
  SELECT CASE
           WHEN p_loc IS NULL OR btrim(p_loc) = '' THEN NULL
           WHEN btrim(p_loc) ~ '^[0-9]{7}$'
             THEN substr(btrim(p_loc), 1, 2) || '-' || substr(btrim(p_loc), 3, 3) || '-' || substr(btrim(p_loc), 6, 2)
           ELSE btrim(p_loc)
         END
$$;

-- 확인
SELECT wcs_sf_location_format('1701406')   AS a   -- 17-014-06
     , wcs_sf_location_format('01-025-01') AS b   -- 01-025-01
     , wcs_sf_location_format(NULL)        AS c   -- NULL
     , wcs_sf_location_format('')          AS d;  -- NULL

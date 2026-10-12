-- =====================================================================
--  SC_DATA : P-BOX 크레인(11호기)을 트윈 포크로 둔다
--  2026-10-12
-- =====================================================================
--
--  현장에서 P-BOX 크레인은 포크가 둘인 트윈이다.
--
--  ※ 2026-10-12 현장 자료를 받아 보니 현장 SC_DATA.SC_TYP 는 이미
--    'DOUBLE' 로 들어 있었다. 코드가 "TWIN" 만 보고 있어 한 번도 참이
--    되지 않았던 것이다. 코드 쪽을 DOUBLE/TWIN 둘 다 받게 고쳤고,
--    여기서는 현장 값인 'DOUBLE' 에 맞춘다. 현장은 DB 를 못 바꾸는
--    상황이므로 코드가 현장 값을 따라가는 것이 맞다.
--
--  기존 ECS 도 P-BOX 크레인 하나만 트윈이다.
--    EcsSv/EcsDoc.cpp
--      m_pScInfo[i] = new CScInfo(this, strFileName, i,
--                        (i==SC_1_NUM) ? WAREHOUSE_BOX   : WAREHOUSE_PALLET,
--                        (i==SC_1_NUM) ? SC_ForkTypeTwin : SC_ForkTypeSingle);
--    SC_1_NUM = 0 이고 그 자리가 BOX 창고 크레인이다. 파레트 호기는 모두
--    SC_ForkTypeSingle 이다.
--
--  ※ 이것만으로는 아직 트윈으로 지시되지 않는다. 지금 돌고 있는
--     스케줄러(IOSchDB)는 포크#2(FK2) 를 한 줄도 쓰지 않는다. SC_TYP 를
--     보는 코드는 cThread_SC.cs 뿐인데 그 클래스는 현장 구성에서 빠져
--     있다(참고용). 지시 쪽 작업이 따로 필요하다.
-- =====================================================================

BEGIN;

UPDATE sc_data
   SET sc_typ = 'DOUBLE'
 WHERE sc_no  = '911';

COMMIT;

-- 확인
SELECT wh_typ, sc_no, sc_typ FROM sc_data ORDER BY sc_no;

-- =====================================================================
--  되돌리기
--    UPDATE sc_data SET sc_typ='SINGLE' WHERE sc_no='911';   -- 되돌릴 일은 없을 것이다
-- =====================================================================

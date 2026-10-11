-- =====================================================================
--  SC_DATA : P-BOX 크레인(11호기)을 트윈 포크로 둔다
--  2026-10-12
-- =====================================================================
--
--  현장에서 P-BOX 크레인은 포크가 둘인 트윈이다. 그런데 SC_DATA.SC_TYP
--  가 열한 호기 모두 'SINGLE' 로 들어 있어, 트윈을 가리는 코드가 한
--  번도 참이 되지 않는다. 그래서 한 번에 하나씩만 집는다.
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
   SET sc_typ = 'TWIN'
 WHERE sc_no  = '911';

COMMIT;

-- 확인
SELECT wh_typ, sc_no, sc_typ FROM sc_data ORDER BY sc_no;

-- =====================================================================
--  되돌리기
--    UPDATE sc_data SET sc_typ='SINGLE' WHERE sc_no='911';
-- =====================================================================

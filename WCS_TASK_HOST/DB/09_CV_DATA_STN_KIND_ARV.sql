-- =====================================================================
--  CV_DATA.STN_KIND 에 도착대(ArvStation) 비트 넣기  (PostgreSQL)
--
--  도착대란
--    화물이 내려앉았다가 다시 나가는 자리다. 이 현장 1F 로는 Size Checker 다.
--      이동  101 -> 107   107 이 도착지
--      입고  107 -> 랙    107 이 출발지
--    입고대(화물이 처음 올라오는 자리)와도, 출고대(사람이 가져가는 자리)와도 다르다.
--
--  왜 필요한가
--    예전 코드는 도착대를 (STN_KIND & 0x03) 으로 읽었다. 그건 "입고대이거나
--    출고대" 라는 뜻이지 도착대라는 뜻이 아니다. 그래서 도착대를 출발지로 쓰면
--    HOST 검증에서 거절되고, 스케줄러도 STO_READY_RD 만 보느라 출발시키지 못했다.
--    이제 비트를 따로 뺐다.
--      0x01 입고대  0x02 출고대  0x04 SC 입고 H/S  0x08 SC 출고 H/S
--      0x10 RTV 출발  0x20 RTV 도착  0x40 도착대   <-- 새로 뺀 것
--
--  어디가 도착대인가
--    WCS Client 의 설비 정의(ClientNSim/Ecs/EcsDefine.xml)가 기준이다.
--    각 Track 의 Status 하위에 ArvStation 으로 적혀 있다.
--    아래 목록은 그 파일에서 그대로 뽑은 것이다.
--
--  준비신호
--    도착대는 출고대와 같은 RET_READY_RD 를 쓴다.
--    WCS Client 도 ArvStation 을 RetStation 과 같은 칸에서 처리한다.
--    (ClientNSim/Ecs/TrackInfo.cpp 의 enStatusArvSTReady)
--
--  실행 : psql -h 127.0.0.1 -p 5432 -U KET_WCS -d KET_WCS -f 09_CV_DATA_STN_KIND_ARV.sql
-- =====================================================================

BEGIN;

-- ---------------------------------------------------------------------
-- 1) EcsDefine.xml 이 ArvStation 이라고 한 트랙에 0x40 을 켠다
-- ---------------------------------------------------------------------
UPDATE CV_DATA
   SET STN_KIND = ((COALESCE(NULLIF(STN_KIND,''),'0')::integer) | 64)::varchar
 WHERE WH_TYP = '10'
   AND MC_NO IN ('204','208','216','218','219','225','226','249','259','260',
                 '302','304','305','307','310','312','355','358',
                 '412','416','420','421','429','432',
                 '505','511','517','523',
                 '623','647','648','652')
   AND ((COALESCE(NULLIF(STN_KIND,''),'0')::integer) & 64) = 0;

-- ---------------------------------------------------------------------
-- 2) 도착대인 HOST 스테이션에서 입고대 비트를 뺀다
--
--    107(MC 218) / 108(MC 225) 는 EcsDefine 기준으로 도착대다.
--    앞서 05_CV_DATA_STN_KIND.sql 에서 101/102/107/108 을 한꺼번에 51 로
--    맞춰 놓았는데, 그러면 도착대에도 입고대 비트가 서 있어 출발 판정이
--    STO_READY_RD 쪽으로 잘못 걸린다. (이 현장 PLC 는 D550 에 218 도 함께
--    올려 두어서 늘 '준비' 로 보인다)
--    출고대 비트는 남긴다. 이동 작업의 도착지 검증에 쓰인다.
-- ---------------------------------------------------------------------
UPDATE CV_DATA
   SET STN_KIND = ((COALESCE(NULLIF(STN_KIND,''),'0')::integer) & ~1)::varchar
 WHERE WH_TYP = '10'
   AND HOST_STN_NO IN ('107','108');

COMMIT;

-- 확인
-- SELECT MC_NO, HOST_STN_NO, STN_KIND
--      , CASE WHEN (STN_KIND::integer &  1) <> 0 THEN 'STO ' ELSE '' END
--     || CASE WHEN (STN_KIND::integer &  2) <> 0 THEN 'RET ' ELSE '' END
--     || CASE WHEN (STN_KIND::integer & 64) <> 0 THEN 'ARV ' ELSE '' END AS KINDS
--   FROM CV_DATA
--  WHERE WH_TYP = '10' AND COALESCE(HOST_STN_NO,'') <> ''
--  ORDER BY HOST_STN_NO;

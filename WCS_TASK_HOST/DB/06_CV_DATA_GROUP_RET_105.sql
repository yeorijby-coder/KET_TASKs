-- =====================================================================
--  CV_DATA 에 1F 그룹 출고대(HOST 스테이션 105) 행 만들기  (PostgreSQL)
--
--  왜 필요한가
--    HostSim 로직의 순환은 이렇다.
--      1) 이동  101 -> 107
--      2) 입고  107 -> 랙
--      3) 출고  랙  -> 105
--    3) 의 도착지 105 는 HOST 가 쓰는 "1F 그룹 출고대" 번호인데,
--    CV_DATA 에 HOST_STN_NO = '105' 인 행이 없어서 HOST 태스크의 도착지 판정이
--    통과하지 못한다.
--
--  105 는 무엇인가
--    DEST_POS_DEF 를 보면
--      TRACK_NO 103 -> MC_NO 206   1F 출고대 #1
--      TRACK_NO 104 -> MC_NO 212   1F 출고대 #2
--      TRACK_NO 105 -> MC_NO 2101  1F 그룹 출고대   <-- 실제 트랙이 아니다
--    PLC 02 의 실제 트랙은 MC_NO 201~266 이므로 2101 은 그 범위 밖이다.
--    즉 105 는 출고대 #1/#2 를 묶어 부르는 논리 번호이고, 대응하는 물리 트랙이
--    따로 없다. (그래서 CV_DATA 에도 행이 없었다)
--
--  무엇을 만드는가
--    HOST 가 도착지로 105 를 쓸 수 있도록 논리 행 하나를 만든다.
--      MC_NO       2101   DEST_POS_DEF 가 가리키는 값 그대로
--      HOST_STN_NO 105
--      STN_KIND    2      0x02 = RET (출고/이동의 도착지)
--    CV 태스크는 WCS_DB.INI 의 FR_TRACK/TO_TRACK(201~266) 범위만 폴링하므로
--    이 행은 PLC 통신 대상이 되지 않는다. 판정과 목적지 해석에만 쓰인다.
--
--  ※ 실제 반송은 그룹 안의 어느 출고대로 갈지 스케줄러가 정해야 한다.
--    (DEST_POS_DEF 의 GROUP_NO / RET_CNT 가 그 용도로 보인다)
--    지금 통합 스케줄러에는 그 분배 처리가 없으므로, 그룹이 아니라 출고대를
--    직접 지정하려면 HostSim Logic.xml 의 RetStns 를 103 이나 104 로 두면 된다.
-- =====================================================================

BEGIN;

DELETE FROM CV_DATA
 WHERE WH_TYP = '10' AND MC_NO = '2101';

INSERT INTO CV_DATA (
    WH_TYP, PLC_NO, TRACK_NO, MC_NO, MC_NO_NM,
    HOST_STN_NO, STN_KIND,
    LUGG_NO_RD, DEST_POS_RD, JOB_TYP_RD,
    ERROR_CODE, AUTO_MODE_RD, RET_READY_RD,
    TR_PAUSE_RD, OD_RQ_YN, OD_RQ_FLAG
) VALUES (
    '10', '02', '2101', '2101', '1F 그룹 출고대',
    '105', '2',
    '0000', '000', '0',
    '0', '0', '0',
    '0', 'N', 'N'
);

COMMIT;

-- 확인
-- SELECT WH_TYP, PLC_NO, TRACK_NO, MC_NO, HOST_STN_NO, STN_KIND, MC_NO_NM
--   FROM CV_DATA
--  WHERE WH_TYP = '10' AND HOST_STN_NO IN ('103','104','105')
--  ORDER BY HOST_STN_NO;

-- COMMON_CODE : JOB_STATUS 상태코드 (설비별 대기→지시→중→완료)
-- 대상 DB: LGLS_MCS_IO (MS-SQL). WH_TYP='10' (eWHTYP.SKI_WH01)
-- cThread_SCH.cs 의 상태 상수와 일치해야 함.
SET NOCOUNT ON;
DELETE FROM COMMON_CODE WHERE CDX_CD = 'JOB_STATUS';

INSERT INTO COMMON_CODE (CDX_CD, CCD_CD, CCD_CD_YN, CCD_EPR_ORD, CCD_NM_KOR, CCD_NM_ENG, CCD_NM_HUN, CCD_NM_CHIN, WH_TYP) VALUES
 ('JOB_STATUS','10','Y', 1, N'CV 구동대기',  'CV Drive Wait',  '', '', '10'),
 ('JOB_STATUS','11','Y', 2, N'CV 구동지시',  'CV Drive Cmd',   '', '', '10'),
 ('JOB_STATUS','15','Y', 3, N'CV 구동중',    'CV Driving',     '', '', '10'),
 ('JOB_STATUS','19','Y', 4, N'CV 구동완료',  'CV Drive Done',  '', '', '10'),
 ('JOB_STATUS','20','Y', 5, N'SC 구동대기',  'SC Drive Wait',  '', '', '10'),
 ('JOB_STATUS','21','Y', 6, N'SC 구동지시',  'SC Drive Cmd',   '', '', '10'),
 ('JOB_STATUS','25','Y', 7, N'SC 구동중',    'SC Driving',     '', '', '10'),
 ('JOB_STATUS','29','Y', 8, N'SC 구동완료',  'SC Drive Done',  '', '', '10'),
 ('JOB_STATUS','30','Y', 9, N'RGV 구동대기', 'RGV Drive Wait', '', '', '10'),
 ('JOB_STATUS','31','Y',10, N'RGV 구동지시', 'RGV Drive Cmd',  '', '', '10'),
 ('JOB_STATUS','35','Y',11, N'RGV 구동중',   'RGV Driving',    '', '', '10'),
 ('JOB_STATUS','39','Y',12, N'RGV 구동완료', 'RGV Drive Done', '', '', '10');
GO

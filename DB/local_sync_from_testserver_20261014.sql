-- 로컬(내 PC) DB 를 테스트 서버(192.168.219.59) 와 맞추는 SQL  2026-10-14
-- 대상: KET_WCS  (psql -h localhost -U KET_WCS -d KET_WCS -f 이파일)
-- 테스트 서버에만 있던 것: 위치 포맷 함수, common_code 컬럼/코드, PK·인덱스, 전광판 eqp_mst 정리
BEGIN;

-- 1) 위치 포맷 함수 (테스트 서버 정의 그대로)
CREATE OR REPLACE FUNCTION public.wcs_sf_location_format(p_loc text)
 RETURNS text
 LANGUAGE sql
 IMMUTABLE
AS $function$
  SELECT CASE
           WHEN p_loc IS NULL OR btrim(p_loc) = '' THEN NULL
           WHEN btrim(p_loc) ~ '^[0-9]{7}$'
             THEN substr(btrim(p_loc), 1, 2) || '-' || substr(btrim(p_loc), 3, 3) || '-' || substr(btrim(p_loc), 6, 2)
           ELSE btrim(p_loc)
         END
$function$
;
CREATE OR REPLACE FUNCTION public.wcs_sf_location_format(p_location character varying)
 RETURNS character varying
 LANGUAGE plpgsql
AS $function$
BEGIN
    -- 입력 값의 길이가 7이 아닌 경우, 원래 값을 반환
    IF length(p_LOCATION) <> 7 THEN
        RETURN p_LOCATION;
    END IF;

    -- 길이가 7인 경우 형식 변환 후 반환
    RETURN left(p_LOCATION, 2) || '-' || substring(p_LOCATION, 3, 3) || '-' || right(p_LOCATION, 2);
END;
$function$
;

-- 2) common_code 컬럼 + 테스트 서버에만 있는 코드
ALTER TABLE common_code ADD COLUMN IF NOT EXISTS cvt_cd_key VARCHAR(20);
INSERT INTO common_code (cdx_cd,ccd_cd,ccd_cd_yn,ccd_epr_ord,ccd_nm_kor,ccd_nm_eng,ccd_nm_hun,ccd_nm_chin,wh_typ) SELECT 'CMD_RQ_ID','DUSTO','Y',9,'이중입고','DUAL STORE','DUAL STORE',NULL,'10' WHERE NOT EXISTS (SELECT 1 FROM common_code WHERE cdx_cd='CMD_RQ_ID' AND ccd_cd='DUSTO');
INSERT INTO common_code (cdx_cd,ccd_cd,ccd_cd_yn,ccd_epr_ord,ccd_nm_kor,ccd_nm_eng,ccd_nm_hun,ccd_nm_chin,wh_typ) SELECT 'COLOR','4','Y',4,'빨강','RED','RED',NULL,'10' WHERE NOT EXISTS (SELECT 1 FROM common_code WHERE cdx_cd='COLOR' AND ccd_cd='4');
INSERT INTO common_code (cdx_cd,ccd_cd,ccd_cd_yn,ccd_epr_ord,ccd_nm_kor,ccd_nm_eng,ccd_nm_hun,ccd_nm_chin,wh_typ) SELECT 'COLOR','5','Y',5,'초록','GREEN','GREEN',NULL,NULL WHERE NOT EXISTS (SELECT 1 FROM common_code WHERE cdx_cd='COLOR' AND ccd_cd='5');
INSERT INTO common_code (cdx_cd,ccd_cd,ccd_cd_yn,ccd_epr_ord,ccd_nm_kor,ccd_nm_eng,ccd_nm_hun,ccd_nm_chin,wh_typ) SELECT 'COLOR','6','Y',6,'노랑','YELLOW','YELLOW',NULL,NULL WHERE NOT EXISTS (SELECT 1 FROM common_code WHERE cdx_cd='COLOR' AND ccd_cd='6');
INSERT INTO common_code (cdx_cd,ccd_cd,ccd_cd_yn,ccd_epr_ord,ccd_nm_kor,ccd_nm_eng,ccd_nm_hun,ccd_nm_chin,wh_typ) SELECT 'SC_GRP_NO','903','Y',2,'S/C 3호기','FORMATION3','FORMATION3',NULL,'10' WHERE NOT EXISTS (SELECT 1 FROM common_code WHERE cdx_cd='SC_GRP_NO' AND ccd_cd='903');
INSERT INTO common_code (cdx_cd,ccd_cd,ccd_cd_yn,ccd_epr_ord,ccd_nm_kor,ccd_nm_eng,ccd_nm_hun,ccd_nm_chin,wh_typ) SELECT 'SC_GRP_NO','904','Y',3,'S/C 4호기','FORMATION4','FORMATION4',NULL,'10' WHERE NOT EXISTS (SELECT 1 FROM common_code WHERE cdx_cd='SC_GRP_NO' AND ccd_cd='904');
INSERT INTO common_code (cdx_cd,ccd_cd,ccd_cd_yn,ccd_epr_ord,ccd_nm_kor,ccd_nm_eng,ccd_nm_hun,ccd_nm_chin,wh_typ) SELECT 'SC_GRP_NO','905','Y',4,'S/C 5호기','FORMATION5','FORMATION5',NULL,'10' WHERE NOT EXISTS (SELECT 1 FROM common_code WHERE cdx_cd='SC_GRP_NO' AND ccd_cd='905');
INSERT INTO common_code (cdx_cd,ccd_cd,ccd_cd_yn,ccd_epr_ord,ccd_nm_kor,ccd_nm_eng,ccd_nm_hun,ccd_nm_chin,wh_typ) SELECT 'SC_GRP_NO','906','Y',5,'S/C 6호기','FORMATION6','FORMATION6',NULL,'10' WHERE NOT EXISTS (SELECT 1 FROM common_code WHERE cdx_cd='SC_GRP_NO' AND ccd_cd='906');
INSERT INTO common_code (cdx_cd,ccd_cd,ccd_cd_yn,ccd_epr_ord,ccd_nm_kor,ccd_nm_eng,ccd_nm_hun,ccd_nm_chin,wh_typ) SELECT 'SC_GRP_NO','907','Y',6,'S/C 7호기','FORMATION7','FORMATION7',NULL,'10' WHERE NOT EXISTS (SELECT 1 FROM common_code WHERE cdx_cd='SC_GRP_NO' AND ccd_cd='907');
INSERT INTO common_code (cdx_cd,ccd_cd,ccd_cd_yn,ccd_epr_ord,ccd_nm_kor,ccd_nm_eng,ccd_nm_hun,ccd_nm_chin,wh_typ) SELECT 'SC_GRP_NO','908','Y',7,'S/C 8호기','FORMATION8','FORMATION8',NULL,'10' WHERE NOT EXISTS (SELECT 1 FROM common_code WHERE cdx_cd='SC_GRP_NO' AND ccd_cd='908');
INSERT INTO common_code (cdx_cd,ccd_cd,ccd_cd_yn,ccd_epr_ord,ccd_nm_kor,ccd_nm_eng,ccd_nm_hun,ccd_nm_chin,wh_typ) SELECT 'SC_GRP_NO','909','Y',8,'S/C 9호기','FORMATION9','FORMATION9',NULL,'10' WHERE NOT EXISTS (SELECT 1 FROM common_code WHERE cdx_cd='SC_GRP_NO' AND ccd_cd='909');
INSERT INTO common_code (cdx_cd,ccd_cd,ccd_cd_yn,ccd_epr_ord,ccd_nm_kor,ccd_nm_eng,ccd_nm_hun,ccd_nm_chin,wh_typ) SELECT 'SC_GRP_NO','910','Y',9,'S/C 10호기','FORMATION10','FORMATION10',NULL,'10' WHERE NOT EXISTS (SELECT 1 FROM common_code WHERE cdx_cd='SC_GRP_NO' AND ccd_cd='910');
INSERT INTO common_code (cdx_cd,ccd_cd,ccd_cd_yn,ccd_epr_ord,ccd_nm_kor,ccd_nm_eng,ccd_nm_hun,ccd_nm_chin,wh_typ) SELECT 'SC_GRP_NO','911','Y',10,'S/C 11호기','FORMATION11','FORMATION11',NULL,'10' WHERE NOT EXISTS (SELECT 1 FROM common_code WHERE cdx_cd='SC_GRP_NO' AND ccd_cd='911');
INSERT INTO common_code (cdx_cd,ccd_cd,ccd_cd_yn,ccd_epr_ord,ccd_nm_kor,ccd_nm_eng,ccd_nm_hun,ccd_nm_chin,wh_typ) SELECT 'SC_NO','903','Y',3,'903','903','903','903','10' WHERE NOT EXISTS (SELECT 1 FROM common_code WHERE cdx_cd='SC_NO' AND ccd_cd='903');
INSERT INTO common_code (cdx_cd,ccd_cd,ccd_cd_yn,ccd_epr_ord,ccd_nm_kor,ccd_nm_eng,ccd_nm_hun,ccd_nm_chin,wh_typ) SELECT 'SC_NO','904','Y',4,'904','904','904','904','10' WHERE NOT EXISTS (SELECT 1 FROM common_code WHERE cdx_cd='SC_NO' AND ccd_cd='904');
INSERT INTO common_code (cdx_cd,ccd_cd,ccd_cd_yn,ccd_epr_ord,ccd_nm_kor,ccd_nm_eng,ccd_nm_hun,ccd_nm_chin,wh_typ) SELECT 'SC_NO','905','Y',5,'905','905','905','905','10' WHERE NOT EXISTS (SELECT 1 FROM common_code WHERE cdx_cd='SC_NO' AND ccd_cd='905');
INSERT INTO common_code (cdx_cd,ccd_cd,ccd_cd_yn,ccd_epr_ord,ccd_nm_kor,ccd_nm_eng,ccd_nm_hun,ccd_nm_chin,wh_typ) SELECT 'SC_NO','906','Y',6,'906','906','906','906','10' WHERE NOT EXISTS (SELECT 1 FROM common_code WHERE cdx_cd='SC_NO' AND ccd_cd='906');
INSERT INTO common_code (cdx_cd,ccd_cd,ccd_cd_yn,ccd_epr_ord,ccd_nm_kor,ccd_nm_eng,ccd_nm_hun,ccd_nm_chin,wh_typ) SELECT 'SC_NO','907','Y',7,'907','907','907','907','10' WHERE NOT EXISTS (SELECT 1 FROM common_code WHERE cdx_cd='SC_NO' AND ccd_cd='907');
INSERT INTO common_code (cdx_cd,ccd_cd,ccd_cd_yn,ccd_epr_ord,ccd_nm_kor,ccd_nm_eng,ccd_nm_hun,ccd_nm_chin,wh_typ) SELECT 'SC_NO','908','Y',8,'908','908','908','908','10' WHERE NOT EXISTS (SELECT 1 FROM common_code WHERE cdx_cd='SC_NO' AND ccd_cd='908');
INSERT INTO common_code (cdx_cd,ccd_cd,ccd_cd_yn,ccd_epr_ord,ccd_nm_kor,ccd_nm_eng,ccd_nm_hun,ccd_nm_chin,wh_typ) SELECT 'SC_NO','909','Y',9,'909','909','909','909','10' WHERE NOT EXISTS (SELECT 1 FROM common_code WHERE cdx_cd='SC_NO' AND ccd_cd='909');
INSERT INTO common_code (cdx_cd,ccd_cd,ccd_cd_yn,ccd_epr_ord,ccd_nm_kor,ccd_nm_eng,ccd_nm_hun,ccd_nm_chin,wh_typ) SELECT 'SC_NO','910','Y',10,'910','910','910','910','10' WHERE NOT EXISTS (SELECT 1 FROM common_code WHERE cdx_cd='SC_NO' AND ccd_cd='910');
INSERT INTO common_code (cdx_cd,ccd_cd,ccd_cd_yn,ccd_epr_ord,ccd_nm_kor,ccd_nm_eng,ccd_nm_hun,ccd_nm_chin,wh_typ) SELECT 'SC_NO','911','Y',11,'911','911','911','911','10' WHERE NOT EXISTS (SELECT 1 FROM common_code WHERE cdx_cd='SC_NO' AND ccd_cd='911');
INSERT INTO common_code (cdx_cd,ccd_cd,ccd_cd_yn,ccd_epr_ord,ccd_nm_kor,ccd_nm_eng,ccd_nm_hun,ccd_nm_chin,wh_typ) SELECT 'SEARCH_TYP','2','Y',2,'적재용기(파렛트)','LD_CTN_NO_P','LD_CTN_NO_P',NULL,'10' WHERE NOT EXISTS (SELECT 1 FROM common_code WHERE cdx_cd='SEARCH_TYP' AND ccd_cd='2');

-- 3) 코드 이름을 테스트 서버(현장) 표기로
UPDATE common_code SET ccd_nm_kor='STOP', ccd_nm_eng='STOP' WHERE cdx_cd='ACTIVE_MODE_RD' AND ccd_cd='0';
UPDATE common_code SET ccd_nm_kor='ACTIVE', ccd_nm_eng='ACTIVE' WHERE cdx_cd='ACTIVE_MODE_RD' AND ccd_cd='1';
UPDATE common_code SET ccd_nm_kor='MANUAL', ccd_nm_eng='MANUAL' WHERE cdx_cd='AUTO_MODE_RD' AND ccd_cd='0';
UPDATE common_code SET ccd_nm_kor='AUTO', ccd_nm_eng='AUTO' WHERE cdx_cd='AUTO_MODE_RD' AND ccd_cd='1';
UPDATE common_code SET ccd_nm_kor='작업 대기', ccd_nm_eng='WAIT' WHERE cdx_cd='COMPLETE_RD' AND ccd_cd='0';
UPDATE common_code SET ccd_nm_kor='FK1완료', ccd_nm_eng='FK1 CMP' WHERE cdx_cd='COMPLETE_RD' AND ccd_cd='1';
UPDATE common_code SET ccd_nm_kor='FK1,2 완료', ccd_nm_eng='FK1,2 CMP' WHERE cdx_cd='COMPLETE_RD' AND ccd_cd='3';
UPDATE common_code SET ccd_nm_kor='[07]이중입고재지정', ccd_nm_eng='NEW LOCATION' WHERE cdx_cd='JOB_STATUS' AND ccd_cd='07';
UPDATE common_code SET ccd_nm_kor='[08]공출고', ccd_nm_eng='EMPTY RETRIEVE' WHERE cdx_cd='JOB_STATUS' AND ccd_cd='08';
UPDATE common_code SET ccd_nm_kor='[09]이중입고', ccd_nm_eng='DUPLICATE LOCATION' WHERE cdx_cd='JOB_STATUS' AND ccd_cd='09';
UPDATE common_code SET ccd_nm_kor='[10]CV 구동대기', ccd_nm_eng='CV NEW' WHERE cdx_cd='JOB_STATUS' AND ccd_cd='10';
UPDATE common_code SET ccd_nm_kor='[11]CV 구동중', ccd_nm_eng='CV MOVING' WHERE cdx_cd='JOB_STATUS' AND ccd_cd='11';
UPDATE common_code SET ccd_nm_kor='[12]CV 도착보고', ccd_nm_eng='CV ARRIVED REPORT' WHERE cdx_cd='JOB_STATUS' AND ccd_cd='12';
UPDATE common_code SET ccd_nm_kor='[18]CV 도착보고완료', ccd_nm_eng='MES ARRIVED' WHERE cdx_cd='JOB_STATUS' AND ccd_cd='18';
UPDATE common_code SET ccd_nm_kor='[19]CV 도착완료 보고', ccd_nm_eng='CV ARRIVED' WHERE cdx_cd='JOB_STATUS' AND ccd_cd='19';
UPDATE common_code SET ccd_nm_kor='[20]SC 구동요구', ccd_nm_eng='SC NEW' WHERE cdx_cd='JOB_STATUS' AND ccd_cd='20';
UPDATE common_code SET ccd_nm_kor='[21]SC 구동중', ccd_nm_eng='SC MOVING REPORT' WHERE cdx_cd='JOB_STATUS' AND ccd_cd='21';
UPDATE common_code SET ccd_nm_kor='[22]SC 도착보고', ccd_nm_eng='SC MOVING REPORT' WHERE cdx_cd='JOB_STATUS' AND ccd_cd='22';
UPDATE common_code SET ccd_nm_kor='[28]SC 도착보고완료', ccd_nm_eng='SC COMPLETED' WHERE cdx_cd='JOB_STATUS' AND ccd_cd='28';
UPDATE common_code SET ccd_nm_kor='[29]SC 구동완료 보고', ccd_nm_eng='SC COMPLETED REPORT' WHERE cdx_cd='JOB_STATUS' AND ccd_cd='29';
UPDATE common_code SET ccd_nm_kor='[30]RTV 구동지시', ccd_nm_eng='RTV NEW' WHERE cdx_cd='JOB_STATUS' AND ccd_cd='30';
UPDATE common_code SET ccd_nm_kor='[31]RTV 구동중', ccd_nm_eng='RTV MOVING REPORT' WHERE cdx_cd='JOB_STATUS' AND ccd_cd='31';
UPDATE common_code SET ccd_nm_kor='[32]RTV LOADING 중', ccd_nm_eng='RTV LOADING REPORT' WHERE cdx_cd='JOB_STATUS' AND ccd_cd='32';
UPDATE common_code SET ccd_nm_kor='[33]RTV UNLOADING 중', ccd_nm_eng='RTV UNLOAING REPORT' WHERE cdx_cd='JOB_STATUS' AND ccd_cd='33';
UPDATE common_code SET ccd_nm_kor='[39]RTV 구동완료 보고', ccd_nm_eng='RTV COMPLETED REPORT' WHERE cdx_cd='JOB_STATUS' AND ccd_cd='39';
UPDATE common_code SET ccd_nm_kor='[99]신규', ccd_nm_eng='NEW(error)' WHERE cdx_cd='JOB_STATUS' AND ccd_cd='99';
UPDATE common_code SET ccd_nm_kor='OFFLINE', ccd_nm_eng='OFFLINE' WHERE cdx_cd='ONLINE_MODE_RD' AND ccd_cd='0';
UPDATE common_code SET ccd_nm_kor='ONLINE', ccd_nm_eng='ONLINE' WHERE cdx_cd='ONLINE_MODE_RD' AND ccd_cd='1';
UPDATE common_code SET ccd_nm_kor='IDLE', ccd_nm_eng='IDLE' WHERE cdx_cd='SC_AUTO_MODE' AND ccd_cd='0';
UPDATE common_code SET ccd_nm_kor='AUTO', ccd_nm_eng='AUTO' WHERE cdx_cd='SC_AUTO_MODE' AND ccd_cd='1';
UPDATE common_code SET ccd_nm_kor='REMOTE', ccd_nm_eng='REMOTE' WHERE cdx_cd='SC_AUTO_MODE' AND ccd_cd='2';
UPDATE common_code SET ccd_nm_kor='화물 없음', ccd_nm_eng='EMPTY' WHERE cdx_cd='SENSOR_FK_RD' AND ccd_cd='0';
UPDATE common_code SET ccd_nm_kor='화물 있음', ccd_nm_eng='EXIST' WHERE cdx_cd='SENSOR_FK_RD' AND ccd_cd='1';
UPDATE common_code SET ccd_nm_kor='대기중', ccd_nm_eng='IDLE' WHERE cdx_cd='UCSTATUS_RD' AND ccd_cd='0';
UPDATE common_code SET ccd_nm_kor='대기중', ccd_nm_eng='WAIT' WHERE cdx_cd='UCSTATUS_RD' AND ccd_cd='1';
UPDATE common_code SET ccd_nm_kor='작업 중', ccd_nm_eng='RUNNING' WHERE cdx_cd='UCSTATUS_RD' AND ccd_cd='2';
UPDATE common_code SET ccd_nm_kor='에러', ccd_nm_eng='ERROR' WHERE cdx_cd='UCSTATUS_RD' AND ccd_cd='4';

-- 4) 전광판 eqp_mst : 테스트 서버는 DISPLAY/01 한 줄(process DISP_TASK)
DELETE FROM eqp_mst WHERE wh_typ='10' AND eqp_typ='DISP' AND plc_no='01';
UPDATE eqp_mst SET process='DISP_TASK' WHERE wh_typ='10' AND eqp_typ='DISPLAY' AND plc_no='01' AND coalesce(process,'')='';
COMMIT;

-- 5) PK / 인덱스 (테스트 서버에 있고 로컬에 없는 것). 중복 행이 있으면 그 줄만 실패하므로 한 줄씩 실행한다.
CREATE INDEX IF NOT EXISTS auto_sc_work_idx ON public.auto_sc_work USING btree (wh_typ NULLS FIRST, sc_no NULLS FIRST, lugg_no NULLS FIRST, job_seq NULLS FIRST);
CREATE INDEX IF NOT EXISTS bcr_data_wh_typ_idx ON public.bcr_data USING btree (wh_typ, plc_no, bcr_no);
CREATE INDEX IF NOT EXISTS bcr_mst_idx ON public.bcr_mst USING btree (wh_typ NULLS FIRST, bcr_no NULLS FIRST);
CREATE INDEX IF NOT EXISTS common_code_idx ON public.common_code USING btree (cdx_cd NULLS FIRST, ccd_cd NULLS FIRST);
CREATE INDEX IF NOT EXISTS cv_data_idx ON public.cv_data USING btree (wh_typ NULLS FIRST, plc_no NULLS FIRST, track_no NULLS FIRST, mc_no NULLS FIRST);
CREATE INDEX IF NOT EXISTS cv_flow_def_idx ON public.cv_flow_def USING btree (wh_typ NULLS FIRST, plc_no NULLS FIRST, track_no NULLS FIRST);
CREATE INDEX IF NOT EXISTS del_his_setting_wh_typ_idx ON public.del_his_setting USING btree (wh_typ, table_name);
CREATE INDEX IF NOT EXISTS dest_def_idx ON public.dest_def USING btree (wh_typ NULLS FIRST, dest_no NULLS FIRST);
CREATE INDEX IF NOT EXISTS dn_mst_idx ON public.dn_mst USING btree (dn_no NULLS FIRST);
CREATE INDEX IF NOT EXISTS eqp_err_his_idx ON public.eqp_err_his USING btree (error_dt NULLS FIRST);
CREATE INDEX IF NOT EXISTS eqp_mes_data_idx ON public.eqp_mes_data USING btree (wh_typ NULLS FIRST, eqp_id NULLS FIRST);
CREATE INDEX IF NOT EXISTS eqp_mst_idx ON public.eqp_mst USING btree (wh_typ NULLS FIRST, eqp_typ NULLS FIRST, plc_no NULLS FIRST);
CREATE INDEX IF NOT EXISTS host_empty_plt_idx ON public.host_empty_plt USING btree (wh_typ NULLS FIRST, stn NULLS FIRST);
CREATE INDEX IF NOT EXISTS ix_if_lugg_sta_his_dt ON public.if_lugg_sta_his USING btree (his_dt);
CREATE INDEX IF NOT EXISTS ix_if_lugg_sta_work ON public.if_lugg_sta USING btree (wh_typ, work_sta);
CREATE INDEX IF NOT EXISTS ix_if_mc_sta_his_dt ON public.if_mc_sta_his USING btree (his_dt);
CREATE INDEX IF NOT EXISTS job_mst_his_ins_date_idx ON public.job_mst_his USING btree (ins_date, ins_time, wh_typ, lugg_no, job_status);
CREATE INDEX IF NOT EXISTS mes_if_log_idx ON public.host_if_log USING btree (wh_typ NULLS FIRST, log_date NULLS FIRST, log_time NULLS FIRST);
CREATE INDEX IF NOT EXISTS wc_data_idx ON public.wc_data USING btree (wh_typ NULLS FIRST, plc_no NULLS FIRST, wc_no NULLS FIRST);
CREATE INDEX IF NOT EXISTS wc_his_idx ON public.wc_his USING btree (wh_typ NULLS FIRST, plc_no NULLS FIRST, wc_no NULLS FIRST, ins_dt NULLS FIRST);
CREATE UNIQUE INDEX IF NOT EXISTS auto_sc_work_pkey ON public.auto_sc_work USING btree (wh_typ, sc_no, lugg_no, job_seq);
CREATE UNIQUE INDEX IF NOT EXISTS bcr_data_pk ON public.bcr_data USING btree (wh_typ, plc_no, bcr_no);
CREATE UNIQUE INDEX IF NOT EXISTS bcr_his_pk ON public.bcr_his USING btree (wh_typ, plc_no, bcr_no, ins_dt);
CREATE UNIQUE INDEX IF NOT EXISTS bcr_mst_pkey ON public.bcr_mst USING btree (wh_typ, bcr_no);
CREATE UNIQUE INDEX IF NOT EXISTS cell_dtl_pkey ON public.cell_dtl USING btree (wh_typ);
CREATE UNIQUE INDEX IF NOT EXISTS cell_mes_req_pkey ON public.cell_mes_req USING btree (wh_typ);
CREATE UNIQUE INDEX IF NOT EXISTS cell_mst_pkey ON public.cell_mst USING btree (wh_typ, cell_no, cell_sc_no);
CREATE UNIQUE INDEX IF NOT EXISTS chg_lang_pkey ON public.chg_lang USING btree (from_lang);
CREATE UNIQUE INDEX IF NOT EXISTS common_code_pkey ON public.common_code USING btree (cdx_cd, ccd_cd);
CREATE UNIQUE INDEX IF NOT EXISTS cv_data_pkey ON public.cv_data USING btree (wh_typ, plc_no, track_no, mc_no);
CREATE UNIQUE INDEX IF NOT EXISTS cv_flow_def_pkey ON public.cv_flow_def USING btree (wh_typ, plc_no, track_no);
CREATE UNIQUE INDEX IF NOT EXISTS del_his_setting_pk ON public.del_his_setting USING btree (table_name);
CREATE UNIQUE INDEX IF NOT EXISTS dest_def_pkey ON public.dest_def USING btree (wh_typ, dest_no);
CREATE UNIQUE INDEX IF NOT EXISTS dest_pos_def_pkey ON public.dest_pos_def USING btree (track_no);
CREATE UNIQUE INDEX IF NOT EXISTS dest_pos_def_pkey_1 ON public.dest_pos_def_1 USING btree (track_no);
CREATE UNIQUE INDEX IF NOT EXISTS dn_dtl_pkey ON public.dn_dtl USING btree (dn_no, dn_ln);
CREATE UNIQUE INDEX IF NOT EXISTS dn_mst_pkey ON public.dn_mst USING btree (dn_no);
CREATE UNIQUE INDEX IF NOT EXISTS eqp_mes_data_pkey ON public.eqp_mes_data USING btree (wh_typ, eqp_id);
CREATE UNIQUE INDEX IF NOT EXISTS eqp_mst_pkey ON public.eqp_mst USING btree (wh_typ, eqp_typ, plc_no);
CREATE UNIQUE INDEX IF NOT EXISTS event_log_idx ON public.event_log USING btree (wh_typ NULLS FIRST, issue_date NULLS FIRST, event_seq NULLS FIRST);
CREATE UNIQUE INDEX IF NOT EXISTS event_log_pkey ON public.event_log USING btree (wh_typ, issue_date, event_seq);
CREATE UNIQUE INDEX IF NOT EXISTS fire_mst_pkey ON public.fire_mst USING btree (wh_typ);
CREATE UNIQUE INDEX IF NOT EXISTS ir_ocv_inf_pkey ON public.ir_ocv_inf USING btree (wh_typ);
CREATE UNIQUE INDEX IF NOT EXISTS job_mst_his_pk ON public.job_mst_his USING btree (ins_date, ins_time, wh_typ, lugg_no, job_status);
CREATE UNIQUE INDEX IF NOT EXISTS job_mst_idx ON public.job_mst USING btree (wh_typ NULLS FIRST, lugg_no NULLS FIRST, bcr_bottom NULLS FIRST, bcr_top NULLS FIRST);
CREATE UNIQUE INDEX IF NOT EXISTS job_mst_pkey ON public.job_mst USING btree (wh_typ, lugg_no);
CREATE UNIQUE INDEX IF NOT EXISTS loop_cv_usage_pkey ON public.loop_cv_usage USING btree (wh_typ, loop_nm);
CREATE UNIQUE INDEX IF NOT EXISTS pk_if_lugg_sta ON public.if_lugg_sta USING btree (wh_typ, luggno);
CREATE UNIQUE INDEX IF NOT EXISTS pk_if_mc_sta ON public.if_mc_sta USING btree (wh_typ, mc_typ, mc_no);
CREATE UNIQUE INDEX IF NOT EXISTS rtv_data_pkey_1 ON public.rtv_data USING btree (wh_typ, plc_no, rtv_no);
CREATE UNIQUE INDEX IF NOT EXISTS rtv_hs_def_pkey ON public.rtv_hs_def USING btree (wh_typ, rtv_no, hs_no);
CREATE UNIQUE INDEX IF NOT EXISTS rtv_sch_def_pkey ON public.rtv_sch_def USING btree (wh_typ, rtv_no, rtv_od_typ, rolling_yn);
CREATE UNIQUE INDEX IF NOT EXISTS rtv_sch_prt_pkey ON public.rtv_sch_prt USING btree (wh_typ, rtv_no);
CREATE UNIQUE INDEX IF NOT EXISTS sc_data_idx ON public.sc_data USING btree (wh_typ NULLS FIRST, plc_no NULLS FIRST, sc_no NULLS FIRST, mc_no NULLS FIRST);
CREATE UNIQUE INDEX IF NOT EXISTS sc_data_idx_1 ON public.sc_data_murata USING btree (wh_typ, plc_no, sc_no, mc_no);
CREATE UNIQUE INDEX IF NOT EXISTS sc_data_murata_pkey_1 ON public.sc_data_murata USING btree (wh_typ, plc_no, sc_no, mc_no);
CREATE UNIQUE INDEX IF NOT EXISTS sc_data_pkey ON public.sc_data USING btree (wh_typ, plc_no, sc_no, mc_no);
CREATE UNIQUE INDEX IF NOT EXISTS sc_def_inf_pkey ON public.sc_def_inf USING btree (wh_typ);
CREATE UNIQUE INDEX IF NOT EXISTS sc_lev_def_pkey ON public.sc_lev_def USING btree (wh_typ);
CREATE UNIQUE INDEX IF NOT EXISTS seq_no_gnr_idx ON public.seq_no_gnr USING btree (wh_typ NULLS FIRST, seq_no_typ NULLS FIRST);
CREATE UNIQUE INDEX IF NOT EXISTS seq_no_gnr_pkey ON public.seq_no_gnr USING btree (wh_typ, seq_no_typ);
CREATE UNIQUE INDEX IF NOT EXISTS switch_stock_def_pkey ON public.switch_stock_def USING btree (wh_typ, stock_track_no);
CREATE UNIQUE INDEX IF NOT EXISTS up_down_unique ON public.up_down USING btree (dn_ver, dn_nm);
CREATE UNIQUE INDEX IF NOT EXISTS user_grp_idx ON public.user_grp USING btree (grp_cd NULLS FIRST);
CREATE UNIQUE INDEX IF NOT EXISTS user_grp_pkey ON public.user_grp USING btree (grp_cd);
CREATE UNIQUE INDEX IF NOT EXISTS user_grp_win_idx ON public.user_grp_win USING btree (grp_cd NULLS FIRST, win_id NULLS FIRST);
CREATE UNIQUE INDEX IF NOT EXISTS user_grp_win_pkey ON public.user_grp_win USING btree (grp_cd, win_id);
CREATE UNIQUE INDEX IF NOT EXISTS user_mst_idx ON public.user_mst USING btree (user_id NULLS FIRST);
CREATE UNIQUE INDEX IF NOT EXISTS user_mst_pkey ON public.user_mst USING btree (user_id);
CREATE UNIQUE INDEX IF NOT EXISTS wait_track_pkey ON public.wait_track USING btree (wh_typ);
CREATE UNIQUE INDEX IF NOT EXISTS wc_data_pkey ON public.wc_data USING btree (wh_typ, plc_no, wc_no);
CREATE UNIQUE INDEX IF NOT EXISTS wc_his_pkey ON public.wc_his USING btree (wh_typ, plc_no, wc_no, ins_dt);
CREATE UNIQUE INDEX IF NOT EXISTS wcs_log_pgr_idx ON public.wcs_log_pgr USING btree (wh_typ NULLS FIRST, ins_dt NULLS FIRST, log_seq NULLS FIRST);
CREATE UNIQUE INDEX IF NOT EXISTS wcs_wrk_log_idx ON public.wcs_wrk_log USING btree (log_dttm NULLS FIRST, log_seq NULLS FIRST);
CREATE UNIQUE INDEX IF NOT EXISTS wcs_wrk_log_pkey ON public.wcs_wrk_log USING btree (log_dttm, log_seq);

SELECT proname FROM pg_proc WHERE proname='wcs_sf_location_format';

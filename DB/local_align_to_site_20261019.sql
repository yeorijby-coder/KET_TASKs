-- 로컬 DB 를 현장 DB(시운전/20261019/Data) 에 맞춘다. 여러 번 실행해도 된다. (2026-10-19)
-- 포트/IP(EQP_MST.PLC_PORT 등)는 시뮬레이터용이라 일부러 맞추지 않는다.
BEGIN;
ALTER TABLE cv_data ADD COLUMN IF NOT EXISTS stn_no varchar(10);
-- 1) STN_KIND / HOST_STN_NO / STN_NO : 현장 값 (피킹대·사이즈체크·팔레트 매거진은 64 = 도착대)
UPDATE cv_data SET stn_kind='64', host_stn_no='151', stn_no='151', use_yn=COALESCE(use_yn,'Y'), mc_no_nm='SC#1 중간 도착대' WHERE wh_typ='10' AND plc_no='02' AND track_no='2004';
UPDATE cv_data SET stn_kind='2', host_stn_no='103', stn_no='103', use_yn=COALESCE(use_yn,'Y') WHERE wh_typ='10' AND plc_no='02' AND track_no='2006';
UPDATE cv_data SET stn_kind='64', host_stn_no='152', stn_no='152', use_yn=COALESCE(use_yn,'Y'), mc_no_nm='SC#2 중간 도착대' WHERE wh_typ='10' AND plc_no='02' AND track_no='2008';
UPDATE cv_data SET stn_kind='2', host_stn_no='104', stn_no='104', use_yn=COALESCE(use_yn,'Y'), mc_no_nm='1F 출고대 #2' WHERE wh_typ='10' AND plc_no='02' AND track_no='2012';
UPDATE cv_data SET stn_kind='64', host_stn_no='153', stn_no='153', use_yn=COALESCE(use_yn,'Y'), mc_no_nm='SC#3 중간 도착대' WHERE wh_typ='10' AND plc_no='02' AND track_no='2016';
UPDATE cv_data SET stn_kind='51', host_stn_no='101', stn_no='101', use_yn=COALESCE(use_yn,'Y'), mc_no_nm='1F 입고대 #1' WHERE wh_typ='10' AND plc_no='02' AND track_no='2017';
UPDATE cv_data SET stn_kind='114', host_stn_no='107', stn_no='107', use_yn=COALESCE(use_yn,'Y'), mc_no_nm='1F SizeChecker #1' WHERE wh_typ='10' AND plc_no='02' AND track_no='2018';
UPDATE cv_data SET stn_kind='64', host_stn_no='154', stn_no='154', use_yn=COALESCE(use_yn,'Y'), mc_no_nm='SC#4 중간 도착대' WHERE wh_typ='10' AND plc_no='02' AND track_no='2020';
UPDATE cv_data SET stn_kind='51', host_stn_no='102', stn_no='102', use_yn=COALESCE(use_yn,'Y'), mc_no_nm='1F 입고대 #2' WHERE wh_typ='10' AND plc_no='02' AND track_no='2024';
UPDATE cv_data SET stn_kind='114', host_stn_no='108', stn_no='108', use_yn=COALESCE(use_yn,'Y'), mc_no_nm='1F SizeChecker #2' WHERE wh_typ='10' AND plc_no='02' AND track_no='2025';
UPDATE cv_data SET stn_kind='64', host_stn_no='171', stn_no='171', use_yn=COALESCE(use_yn,'Y'), mc_no_nm='1F 출고 위치 결정대' WHERE wh_typ='10' AND plc_no='02' AND track_no='2032';
UPDATE cv_data SET stn_kind='64', host_stn_no='156', stn_no='156', use_yn=COALESCE(use_yn,'Y'), mc_no_nm='SC#6 중간 도착대' WHERE wh_typ='10' AND plc_no='02' AND track_no='2049';
UPDATE cv_data SET stn_kind='64', host_stn_no='155', stn_no='155', use_yn=COALESCE(use_yn,'Y'), mc_no_nm='SC#5 중간 도착대' WHERE wh_typ='10' AND plc_no='02' AND track_no='2059';
UPDATE cv_data SET stn_kind='2', host_stn_no='105', stn_no='105', use_yn=COALESCE(use_yn,'Y') WHERE wh_typ='10' AND plc_no='02' AND track_no='2101';
UPDATE cv_data SET stn_kind='64', host_stn_no='207', stn_no='207', use_yn=COALESCE(use_yn,'Y'), mc_no_nm='3F 팔레트 매거진' WHERE wh_typ='10' AND plc_no='03' AND track_no='3001';
UPDATE cv_data SET stn_kind='64', host_stn_no='203', stn_no='203', use_yn=COALESCE(use_yn,'Y'), mc_no_nm='3F 피킹대 #3' WHERE wh_typ='10' AND plc_no='03' AND track_no='3003';
UPDATE cv_data SET stn_kind='64', host_stn_no='206', stn_no='206', use_yn=COALESCE(use_yn,'Y'), mc_no_nm='3F SizeChecker #2' WHERE wh_typ='10' AND plc_no='03' AND track_no='3004';
UPDATE cv_data SET stn_kind='64', host_stn_no='202', stn_no='202', use_yn=COALESCE(use_yn,'Y'), mc_no_nm='3F 피킹대 #2' WHERE wh_typ='10' AND plc_no='03' AND track_no='3006';
UPDATE cv_data SET stn_kind='64', host_stn_no='201', stn_no='201', use_yn=COALESCE(use_yn,'Y'), mc_no_nm='3F 피킹대 #1' WHERE wh_typ='10' AND plc_no='03' AND track_no='3009';
UPDATE cv_data SET stn_kind='64', host_stn_no='200', stn_no='200', use_yn=COALESCE(use_yn,'Y'), mc_no_nm='3F Box 보충 피킹대' WHERE wh_typ='10' AND plc_no='03' AND track_no='3011';
UPDATE cv_data SET stn_kind='64', host_stn_no='205', stn_no='205', use_yn=COALESCE(use_yn,'Y'), mc_no_nm='3F SizeChecker #1' WHERE wh_typ='10' AND plc_no='03' AND track_no='3012';
UPDATE cv_data SET stn_kind='115', host_stn_no='208', stn_no='208', use_yn=COALESCE(use_yn,'Y'), mc_no_nm='3F 피킹대 #4' WHERE wh_typ='10' AND plc_no='03' AND track_no='3057';
UPDATE cv_data SET stn_kind='65', host_stn_no='209', stn_no='209', use_yn=COALESCE(use_yn,'Y'), mc_no_nm='3F SizeChecker #3' WHERE wh_typ='10' AND plc_no='03' AND track_no='3058';
UPDATE cv_data SET stn_kind='1', host_stn_no='211', stn_no='211', use_yn=COALESCE(use_yn,'Y'), mc_no_nm='3F Box 보충 입고대' WHERE wh_typ='20' AND plc_no='04' AND track_no='4007';
UPDATE cv_data SET stn_kind='115', host_stn_no='251', stn_no='251', use_yn=COALESCE(use_yn,'Y'), mc_no_nm='3F Box 피킹대' WHERE wh_typ='20' AND plc_no='04' AND track_no='4015';
UPDATE cv_data SET stn_kind='64', host_stn_no='221', stn_no='221', use_yn=COALESCE(use_yn,'Y'), mc_no_nm='3F Box 입고 대기대 #1' WHERE wh_typ='20' AND plc_no='04' AND track_no='4019';
UPDATE cv_data SET stn_kind='64', host_stn_no='222', stn_no='222', use_yn=COALESCE(use_yn,'Y'), mc_no_nm='3F Box 입고 대기대 #2' WHERE wh_typ='20' AND plc_no='04' AND track_no='4020';
UPDATE cv_data SET stn_kind='64', host_stn_no='231', stn_no='231', use_yn=COALESCE(use_yn,'Y'), mc_no_nm='3F BCR 이동 지시대' WHERE wh_typ='20' AND plc_no='04' AND track_no='4028';
UPDATE cv_data SET stn_kind='64', host_stn_no='242', stn_no='242', use_yn=COALESCE(use_yn,'Y'), mc_no_nm='3F BCR 이동 도착대 #2' WHERE wh_typ='20' AND plc_no='04' AND track_no='4031';
UPDATE cv_data SET stn_kind='64', host_stn_no='241', stn_no='241', use_yn=COALESCE(use_yn,'Y'), mc_no_nm='3F BCR 이동 도착대 #1' WHERE wh_typ='20' AND plc_no='04' AND track_no='4034';
UPDATE cv_data SET stn_kind='64', host_stn_no='157', stn_no='157', use_yn=COALESCE(use_yn,'Y'), mc_no_nm='SC#7 중간 도착대' WHERE wh_typ='10' AND plc_no='05' AND track_no='5006';
UPDATE cv_data SET stn_kind='64', host_stn_no='158', stn_no='158', use_yn=COALESCE(use_yn,'Y'), mc_no_nm='SC#8 중간 도착대' WHERE wh_typ='10' AND plc_no='05' AND track_no='5012';
UPDATE cv_data SET stn_kind='64', host_stn_no='159', stn_no='159', use_yn=COALESCE(use_yn,'Y'), mc_no_nm='SC#9 중간 도착대' WHERE wh_typ='10' AND plc_no='05' AND track_no='5018';
UPDATE cv_data SET stn_kind='64', host_stn_no='160', stn_no='160', use_yn=COALESCE(use_yn,'Y'), mc_no_nm='SC#10 중간 도착대' WHERE wh_typ='10' AND plc_no='05' AND track_no='5024';
UPDATE cv_data SET stn_kind='64', host_stn_no='213', stn_no='213', use_yn=COALESCE(use_yn,'Y'), mc_no_nm='3F 피킹대 #6' WHERE wh_typ='10' AND plc_no='06' AND track_no='6046';
UPDATE cv_data SET stn_kind='64', host_stn_no='215', stn_no='215', use_yn=COALESCE(use_yn,'Y'), mc_no_nm='3F SizeChecker #5' WHERE wh_typ='10' AND plc_no='06' AND track_no='6047';
UPDATE cv_data SET stn_kind='64', host_stn_no='212', stn_no='212', use_yn=COALESCE(use_yn,'Y'), mc_no_nm='3F 피킹대 #5' WHERE wh_typ='10' AND plc_no='06' AND track_no='6051';
UPDATE cv_data SET stn_kind='64', host_stn_no='214', stn_no='214', use_yn=COALESCE(use_yn,'Y'), mc_no_nm='3F SizeChecker #4' WHERE wh_typ='10' AND plc_no='06' AND track_no='6052';
-- 2) 현장에만 있는 3100(스테이션 204)
DELETE FROM cv_data WHERE track_no='3100' AND wh_typ='10';
INSERT INTO cv_data (mc_no,plc_no,stn_no,use_yn,wh_typ,barcode,d5n_1_rd,d5n_4_rd,mc_no_nm,od_rq_yn,stn_kind,track_no,cmd_rq_id,cmd_rq_yn,err_rq_od,err_rq_rd,od_upd_dt,comming_od,comming_rd,error_code,job_typ_od,job_typ_rd,lugg_no_od,lugg_no_rd,od_rq_flag,od_user_id,ret_cnt_rd,cmd_rq_parm,dest_pos_od,dest_pos_rd,host_stn_no,mtr1_run_rd,mtr2_run_rd,read_upd_dt,tr_pause_od,tr_pause_rd,auto_mode_rd,emergency_rd,host_send_yn,ret_ready_rd,sto_ready_rd,write_upd_dt,pick4_pass_rd,sz_chk_low_rd,door_status_rd,pulp_sensor_rd,reths_ready_rd,stohs_ready_rd,sz_chk_high_rd,comming_dest_tr,cv_check_pos_rd,delete_track_rd,rgv_sta_load_rd,rtv_lock_sensor,sensor0_data_rd,sensor1_data_rd,sensor2_data_rd,door_open_req_rd,host_err_send_yn,srt_ready_sta_rd,comming_check_tr1,comming_check_tr2,comming_check_tr3,comming_check_tr4,deverter_hs_up_rd,door_close_req_rd,tracking_write_yn,deverter_hs_down_rd,sc_pa_pk_possible_rd,rtv_arrivehs_ready_rd,rtv_deparths_ready_rd) VALUES ('3100','03','204','Y','10',NULL,'','','','N','0','3100',NULL,'N',NULL,NULL,NULL,NULL,NULL,'0',NULL,'0',NULL,'0000','N',NULL,'',NULL,NULL,'000','204',NULL,NULL,'2026-07-04T17:57:21.572',NULL,NULL,NULL,'','N',NULL,NULL,'2026-07-04T17:57:21.572',NULL,NULL,NULL,'',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,'Y',NULL,NULL,NULL,NULL,NULL,NULL,NULL,'N',NULL,NULL,NULL,NULL);
-- 3) 현장에 없는 DEST_POS_DEF 901/902 행 삭제
DELETE FROM dest_pos_def WHERE track_no IN ('901','902') AND mc_no IN ('901','902');
-- 4) 창고구분 이름
UPDATE common_code SET ccd_nm_kor='파레트' WHERE cdx_cd='WH_TYP' AND ccd_cd='10';
UPDATE common_code SET ccd_nm_kor='P-BOX' WHERE cdx_cd='WH_TYP' AND ccd_cd='20';
COMMIT;

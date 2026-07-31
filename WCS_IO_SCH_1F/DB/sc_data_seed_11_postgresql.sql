-- ============================================================
-- SC_DATA 11대 레코드 시딩 (KET To-Be : SC #1 ~ #11) - PostgreSQL(KET_WCS)용
-- 생성: 2026-07-09  (MS-SQL용 sc_data_seed_11.sql 과 동일 내용)
-- 기준: 기존 901/902 행 컨벤션 (WH_TYP=10, MC_NO=SC_NO, PLC_NO=순번 2자리,
--       MC_NO_NM='SC n호기', SC_TYP='SINGLE')
-- 동작: SC_NO 901~911 중 없는 행만 INSERT (재실행 안전)
-- 신규 행 초기값: 유휴 정상 상태. _RD 컬럼은 통신 Task(WCS_TASK_SC)가 갱신.
-- ============================================================

SET client_encoding = 'UTF8';

INSERT INTO sc_data
    ( wh_typ, plc_no, sc_no, sc_grp_no, mc_no, mc_no_nm, sc_typ
    , auto_mode_rd, ucstatus_rd, online_mode_rd, active_mode_rd
    , sensor_fk_rd, pos_h_rd, pos_v_rd
    , err_code_rd, complete_rd, job_typ_rd, job_typ_od
    , lugg_no_fk1_rd, lugg_no_fk1_od, itn_lugg_fk1
    , lugg_no_fk2_rd, lugg_no_fk2_od, itn_lugg_fk2
    , err_sta_fk1_rd, err_sta_fk2_rd, forkpos_fk1_rd, forkpos_fk2_rd
    , use_fk_rd, use_fk_od, stock_mode
    , od_rq_yn, od_rq_flag, cmd_rq_yn, host_send_yn, host_err_send_yn
    , suspend, read_upd_dt, write_upd_dt, od_user_id
    , write_continue_od, write_flag_od, user_command_od
    , cv_workbench_rd, cv_workbench_sub_rd, plt_info_rd
    , sc_plt_job_typ_rd, sc_plt_job_typ_od )
SELECT
      '10', LPAD(n::TEXT, 2, '0'), (900 + n)::TEXT, '9', (900 + n)::TEXT
    , 'SC ' || n || '호기', 'SINGLE'
    , '1', '1', '1', '1'
    , '0', '0', '0'
    , '0000', '0', '0', '0'
    , '0000', '0000', '0'
    , '0000', '0000', '0'
    , '0', '0', '0', '0'
    , '0', '0', '0'
    , 'N', 'N', 'N', 'N', 'N'
    , '0', NOW(), NOW(), 'SEED'
    , '0', '0', '0'
    , '0', '0', '0'
    , '0', '0'
  FROM generate_series(1, 11) AS n
 WHERE NOT EXISTS (SELECT 1 FROM sc_data sd
                    WHERE sd.wh_typ = '10' AND sd.sc_no = (900 + n)::TEXT);

SELECT COUNT(*) AS sc_data_cnt FROM sc_data WHERE wh_typ = '10';
SELECT wh_typ, plc_no, sc_no, sc_grp_no, mc_no, mc_no_nm, sc_typ, od_rq_yn, err_code_rd
  FROM sc_data WHERE wh_typ = '10' ORDER BY sc_no;

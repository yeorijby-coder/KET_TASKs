-- =====================================================================
--  사용하지 않는 테이블 정리 (현장에 데이터 없음 + 소스가 참조하지 않음)   2026-10-19
-- =====================================================================
--  대상 23개. 행이 1건이라도 있으면 지우지 않고 알림만 남긴다. (현장은 모두 0건)
--  다른 객체(뷰/FK)가 물고 있으면 DROP 이 실패하므로 CASCADE 는 쓰지 않는다.
--  실행 전 pg_dump 백업을 권장한다.
--      psql -h <DB IP> -U KET_WCS -d KET_WCS -f drop_unused_tables_20261019.sql
-- =====================================================================
DO $$
DECLARE
    t text;
    n bigint;
    tbls text[] := ARRAY[
        'bcr_his','cell_mes_req','chg_lang','cv_data_pb','dest_def','eqp_mes_data',
        'fire_bank','fire_mst','ir_ocv_inf','mg_dp_inf','motorsense','rtv_data_pb',
        'rtv_hori_def','rtv_hs_def','rtv_sch_def','rtv_sch_prt','sc_data_murata_pb',
        'sc_def_inf','switch_stock_def','terminal_display','up_down','wc_his','wcs_wrk_log'];
BEGIN
    FOREACH t IN ARRAY tbls LOOP
        IF to_regclass('public.' || t) IS NULL THEN
            RAISE NOTICE '% : 없음(건너뜀)', t;
            CONTINUE;
        END IF;
        EXECUTE format('SELECT count(*) FROM public.%I', t) INTO n;
        IF n > 0 THEN
            RAISE NOTICE '% : % 건이 있어 지우지 않음', t, n;
        ELSE
            EXECUTE format('DROP TABLE public.%I', t);
            RAISE NOTICE '% : 삭제', t;
        END IF;
    END LOOP;
END $$;

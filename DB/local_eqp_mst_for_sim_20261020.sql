-- 로컬 시뮬레이터용 EQP_MST 접속정보 (현장 DB 에는 실행하지 않는다)   2026-10-20
-- CV / SC / DISPLAY TASK 는 이제 IP 와 포트를 EQP_MST 에서 읽는다. 로컬은 시뮬레이터(127.0.0.1)에 붙는다.
--   CV  PLC 01~08 : 9101, 9201, 9301, 9401, 9501, 9601, 9701, 9801
--   SC  PLC 01~11 : 8101 ~ 8111
--   DISPLAY       : 8003 (범위 8001~8009)
UPDATE eqp_mst SET plc_ip = '127.0.0.1' WHERE eqp_typ IN ('CV','SC','DISPLAY');
UPDATE eqp_mst SET plc_port = (9000 + to_number(plc_no,'99') * 100 + 1)::text,
                   plc_port_from = (9000 + to_number(plc_no,'99') * 100 + 1)::text,
                   plc_port_to   = (9000 + to_number(plc_no,'99') * 100 + 1)::text
 WHERE eqp_typ = 'CV';
UPDATE eqp_mst SET plc_port = (8100 + to_number(plc_no,'99'))::text,
                   plc_port_from = (8100 + to_number(plc_no,'99'))::text,
                   plc_port_to   = (8100 + to_number(plc_no,'99'))::text
 WHERE eqp_typ = 'SC';
UPDATE eqp_mst SET plc_port = '8003', plc_port_from = '8001', plc_port_to = '8009' WHERE eqp_typ = 'DISPLAY';
SELECT eqp_typ, wh_typ, plc_no, plc_ip, plc_port, plc_port_from, plc_port_to FROM eqp_mst WHERE eqp_typ IN ('CV','SC','DISPLAY') ORDER BY 1,3;

# JOB_MST 시험 데이터 508건 유실 (2026-08-10)

## 무슨 일이 있었나

순환(이동→입고→출고) 검증을 하려는데, JOB_MST 에 상태 29 로 남아 있던
시험 작업 508건이 HOST 태스크의 완료보고 경로로 계속 흘러나가 관찰을 가렸다.
그래서 다음을 실행했다.

```sql
DELETE FROM JOB_MST WHERE JOB_STATUS = '29';
```

이 508건은 이력(JOB_MST_HIS)으로 옮겨지지 않고 그대로 지워졌다.
`04_JOB_MST_RESTORE.sql` 은 **이력에 있는 것을 되돌리는** 스크립트라
이 경우에는 되돌릴 것이 없다. 실행해도 0건이다.

## 무엇이 없어졌나

`04_JOB_MST_RESTORE.sql` 머리말에 적힌 기준 데이터 그대로다.

| 작업구분 | 건수 |
|---|---|
| 1 (입고) | 12 |
| 3 (피킹 출고) | 495 |
| 6 (이동) | 1 |
| **합계** | **508** (모두 JOB_STATUS = 29) |

## 복구 가능성

되돌릴 방법이 없다.

- `JOB_MST_HIS` 에 작업구분 3 인 행이 한 건도 없다. 상태 29 인 1/6 도 없다.
- 저장소 안에 508건을 만들어 내는 적재 스크립트나 덤프가 없다.
  (`04_JOB_MST_RESTORE.sql` 은 이력 → JOB_MST 복사일 뿐이다)
- `job_mst_whtyp_fix_bak` 백업 테이블에는 `LUGG_NO` 와 `OLD_WH_TYP` 만 있고
  출발지/도착지/로케이션 같은 나머지 칼럼이 없다.

## 남겨 둔 단서

508건의 작업번호는 `07_LOST_JOB_MST_LUGG_NO.txt` 에 뽑아 두었다.
원본 적재분(2026-07-10 일괄 적재)이 어딘가 남아 있다면 그 목록으로 대조하면 된다.

## 다시 이런 일이 없도록

시험 때문에 JOB_MST 를 비워야 하면 지우기 전에 이력으로 옮긴다.

```sql
BEGIN;
INSERT INTO JOB_MST_HIS (<공통 칼럼>)
SELECT <공통 칼럼> FROM JOB_MST WHERE JOB_STATUS = '29';
DELETE FROM JOB_MST WHERE JOB_STATUS = '29';
COMMIT;
```

그러면 `04_JOB_MST_RESTORE.sql` 로 그대로 되돌릴 수 있다.
공통 칼럼 목록은 그 스크립트가 `information_schema` 로 만들어 쓰는 방식을
그대로 쓰면 된다.

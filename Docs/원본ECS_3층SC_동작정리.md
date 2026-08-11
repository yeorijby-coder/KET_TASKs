# 원본 ECS(VC++6.0) 3층 SC(크레인) 동작 정리

크레인 한 대가 1층과 3층을 같이 본다. 그 중 **3층 쪽**이 어떤 상태에서
어떤 함수로 어떤 순서에 처리되는지를 정리한 것이다.

- 원본 소스 : `D:\인수인계\한국단자\Ecs Server\EcsSv` (2026-01-09 판)
- 상수 : `Common\Include\Ecs\EcsDef.h`, `Common\Include\Ecs\EcsEnv.h`
- 짝이 되는 문서 : `원본ECS_스테이션_동작정리.md` (CV 쪽 작업대 정리)
- 이 문서도 소스에서 표를 기계적으로 뽑아 만들었다. 손으로 적은 값이 아니다.

## 0. 먼저 알아야 할 것

### 크레인 하나가 층을 두 개 본다 — RANK

크레인은 층을 이름으로 구분하지 않는다. **H/S 자리 번호(RANK)** 로만 구분한다.

| RANK | 값 | 뜻 |
| --- | --- | --- |
| `RANK_1` | 1 | 1층 입고 H/S |
| `RANK_2` | 2 | 1층 출고 H/S |
| `RANK_3` | 3 | **3층 입고 H/S** |
| `RANK_4` | 4 | **3층 출고 H/S** |

기동할 때 `CEcsDoc::InitEquip()` (`EcsDoc.cpp:1489`) 이
`CLib::GetScHSTrackPerRank(호기, RANK)` 를 돌려 RANK ↔ CV 트랙을 묶어 둔다.

```cpp
for(int j=1; j<=CLib::GetScHSCnt(i); j++)
{
    int nTrackNum = CLib::GetScHSTrackPerRank(i, j);
    pCvTrackInfo  = GetCvTrackInfo(nTrackNum);
    if(pCvTrackInfo == NULL)  continue;
    m_pScInfo[i]->SetRankCvTrack(j, pCvTrackInfo);      // RANK -> 트랙
}
```

그 다음부터 크레인 코드는 `GetRankCvTrackInfo(RANK_3)` 한 줄로
3층 입고 H/S 트랙을 꺼내 쓴다. **층 이름은 코드 어디에도 없다.**

### 호기별 H/S 트랙 — `Lib.cpp:534` `GetScHSTrackPerRank`

| 호기 | 창고 | 포크 | RANK_1 1F입고 | RANK_2 1F출고 | RANK_3 **3F입고** | RANK_4 **3F출고** |
| --- | --- | --- | --- | --- | --- | --- |
| SC_1 | BOX | Twin | 4020 / 4019 | 4010 / 4011 | — | — |
| SC_2 | PLT | Single | 2002 | 2003 | **3016** | **3017** |
| SC_3 | PLT | Single | 2010 | 2007 | **3026** | **3024** |
| SC_4 | PLT | Single | 2014 | 2015 | **3031** | **3032** |
| SC_5 | PLT | Single | 2022 | 2021 | **3039** | **3038** |
| SC_6 | PLT | Single | 2057 | 2058 | **3044** | **3045** |
| SC_7 | PLT | Single | 2054 | 2048 | **3054** | **3048** |
| SC_8 | PLT NEW | Single | 5003 | 5004 | **6003** | **6004** |
| SC_9 | PLT NEW | Single | 5009 | 5010 | **6009** | **6010** |
| SC_10 | PLT NEW | Single | 5015 | 5016 | **6015** | **6016** |
| SC_11 | PLT NEW | Single | 5021 | 5022 | **6021** | **6022** |

- `SC_1_HS_CNT = 2`, 나머지는 전부 4다. **SC_1(BOX 크레인)만 3층 랭크가 없다.**
  SC_1 의 RANK_1/RANK_2 는 4020/4010 인데, 이건 3층 BOX 라인이라 1층이 아니다.
  즉 SC_1 은 "1층/3층" 이 아니라 "포크1/포크2" 로 두 자리를 쓴다.
- SC_2~SC_7 은 1층 = PLC02(2xxx), 3층 = PLC03(3xxx)
- SC_8~SC_11 은 1층 = PLC05(5xxx), 3층 = PLC06(6xxx)

### 목적지 작업대가 층을 정한다 — `Lib.cpp:2343` `CLib::GetRank`

출고 작업이 1층으로 나갈지 3층으로 나갈지는 **도착 작업대 번호 하나로** 정해진다.

| 도착 작업대 | RANK | 결과 |
| --- | --- | --- |
| 103, 104, 105 | `RANK_2` | 1층 출고 H/S 로 |
| **200~209, 212~215** | **`RANK_4`** | **3층 출고 H/S 로** |
| 211, 221, 222, 231, 241, 242, 251 | `RANK_2` | 3층 BOX (SC_1 의 RANK_2) |
| HS_01~06, HS_11 | `RANK_1` | H/S 직행 |
| 101, 102, 107, 108, 151~156, 171 | `ECS_ERROR` | 출고 목적지가 될 수 없는 자리 |

여기에 예외가 하나 있다. **창고간 이동(`JOB_PATTERN_AISLE`)은 목적지를 안 본다.**

```cpp
int CJobItem::GetRank()
{
    if(GetPattern() == JOB_PATTERN_AISLE)
//      return RANK_2;    // 원본 소스
        return RANK_4;    // 수정본 소스   <- 2011.02.06 RTV->C/V 교체건
    return CLib::GetRank(m_nDestPos);
}
```

호기 이동은 원래 1층으로 빼던 것을 **3층으로 빼도록 바꿔 놓았다.**
`CJob::FetchScRetJobByScNumberNPriority` (`Job.cpp:1002`) 에도 같은 수정이 들어 있다.

### 크레인에서 읽는 값 — `CSc::ReadStatus` (`Sc.cpp:54`)

`D95` 부터 36 워드를 읽는다 (Melsec Q).

| 주소 | 뜻 | 쓰는 곳 |
| --- | --- | --- |
| D95 | 지상반 모드 (`SC_MODE_ONLINE` `'1'`) | `IsReadyToWork`, `IsFinishTheWork` |
| D100 L | 자동/수동 (`SC_MODE_AUTO` `'1'`) | 〃 |
| D100 H | 화물 적재 (`SC_PROD_EMPTY` `'0'`) | `IsFinishTheWork` |
| D101 | 상태 `'0'`대기 `'1'`아이들 `'2'`이동 `'4'`에러 | `IsReadyToWork`, `ErrorCheck` |
| D102 / D103 | 주행 / 승강 위치 | 화면 표시 |
| D105 | 에러 코드 | `ErrorCheck`, `ErrorRoutine` |
| D106 L / H | 포크1 / 포크2 에러 상태 | 이중입고 재지정 |
| D109 | Active (`SC_ACTIVE` `'1'`) | `IsReadyToWork` |
| D110 | 트랜잭션 (`SC_TRN_COMPLETE` `'3'`) | `IsFinishTheWork` |
| D111 | 작업 종류 | 화면 표시 |
| D112 | **포크1 작업번호** | 지시가 먹었는지 확인, 완료 판정 |
| D113~D116 / D117~D120 | 포크1 출발 / 도착 정보 | 화면 표시 |
| D122 | 포크2 작업번호 | 〃 |
| D123~D126 / D127~D130 | 포크2 출발 / 도착 정보 | 〃 |

### 크레인에 쓰는 값 — 지시는 전부 `D171` 부터 22 워드

| D171 | 지시 | 함수 |
| --- | --- | --- |
| 1 | 입고 | `Store` (`Sc.cpp:1834`) |
| 2 | 출고 | `Retrieve` (`Sc.cpp:1920`) |
| 3 | H/S → H/S (작업대 이동) | `HsToHs` (`Sc.cpp:2006`) |
| 4 | 랙투랙 | `RackToRack` (`Sc.cpp:2068`) |

| 주소 | 입고 | 출고 |
| --- | --- | --- |
| D172 | 포크1 작업번호 | 포크1 작업번호 |
| D173/D174/D175 | — | 출발 Bank / Bay / Level |
| D176 | **출발 H/S 번호** | — |
| D177/D178/D179 | 도착 Bank / Bay / Level | — |
| D180 | — | **도착 H/S 번호** |
| D181 | 포크 사용 (0=1번, 1=양쪽, 2=2번) | 〃 |
| D182~D190 | 포크2 몫, 위와 같은 배치 | 〃 |
| D191 | 1 (쓰기 완료) | 1 |

**H/S 번호 자리에 들어가는 값이 곧 RANK 번호다.**

```cpp
int CSc::GetScSelfStoHS(int nRank) { return nRank; }
int CSc::GetScSelfRetHS(int nRank) { return nRank; }
```

즉 **3층 입고면 D176 = 3**, **3층 출고면 D180 = 4** 가 그대로 나간다.
1층이면 1 과 2 다. 크레인 PLC 는 이 숫자만 보고 층을 판단한다.

## 1. 매 주기에 도는 함수 — `CSc::ThreadProc` (`Sc.cpp:2556`)

400ms 마다 한 바퀴다.

```
HoldConnection()          D299 에 살아있음 표시 (Melsec Q 는 아무것도 안 함)
SendUpdateMsg(NOTIFY_SEND)

ReadStatus()  실패 -> ErrorCheck()                          7 번 연속 실패하면 통신에러로 확정
              성공 -> 상태가 SC_STA_ERROR 면 ErrorCheck() + ErrorRoutine()
                      아니면  IsInvoke() ? CompleteCheck() : InvokeCheck()
```

`IsInvoke()` 는 `SC_INFO->m_bInvoke` 한 개다 (`Sc.cpp:2336`).
**지시를 냈으면 완료만 보고, 안 냈으면 새 지시만 본다.** 두 가지가 섞이지 않는다.

## 2. 상태 → 함수 순서

### 2-1. 3층 입고 — RANK_3

| 상태 | 도는 함수 / 조건 | 결과 |
| --- | --- | --- |
| **평상시** — 3층 입고 H/S 화물 없음, 지시 없음, ONLINE·AUTO | `InvokeCheck()` → `StoreRoutine()` → `StoreRoutine(RANK_3)` <br> `pCvTrackInfo->GetLuggNum() == 0` 이라 즉시 `FALSE` | 아무 일 없음 |
| **CV 가 화물을 올려놓음** — 입고 H/S 에 화물 있고 Up | CV 쪽 `ParsingExtraFrame` 워드 +4 비트 → `SetStoHomeStandReady(TRUE)` <br> 작업번호는 CV 가 `WriteTrackInfo` 로 미리 써 둔 것 | `IsStoHomeStandReady()` TRUE, `GetLuggNum() != 0` |
| **입고 지시 판단** | `StoreRoutine(RANK_3)` (`Sc.cpp:417`) <br> ① `m_bStoreSuspend` 면 중단 <br> ② `JOB->Find(작번)` — 못 찾으면 로그 남기고 중단 <br> ③ `IsStoPalletValid(JobItem)` (`Sc.cpp:2341`) <br> ④ 패턴이 `JOB_PATTERN_SITE` 면 `SiteToSiteRoutine(RANK_3)` 로 빠짐 <br> ⑤ `ReadStatus()` 다시 읽어 에러면 중단 | 통과하면 지시 |
| **입고 지시 전송** | `Store(작번, 목적 LOC, 0, "", GetScSelfStoHS(RANK_3))` <br> → **D171=1, D176=3**, D177/178/179 = Bank/Bay/Level | `m_bInvoke = TRUE` <br> `m_nPrevRtn = ROUTINE_STORE` <br> `m_nStoPrevRtn = RANK_3` <br> `JOB_STA_SC_OPER_INVOKE` |
| **지시가 먹었는지 확인** (`m_bAutoScEStop` 켰을 때만) | 2 초 뒤 `ReadStatus()`, D112 ≠ 지시한 작번이면 실패 <br> 4 번 연속 실패하면 `EStop()` | 실패면 `FALSE` 로 되돌아감 |
| **전송은 실패했는데 크레인은 움직임** | 3 초 뒤 `ReadStatus()`, D112 == 작번이면 <br> "전송 실패 하였으나 설비가 구동하므로 DATA Setting" | 성공과 같은 상태로 맞춰 놓음 |
| **랙에 넣는 중** | `IsInvoke()` TRUE → `CompleteCheck()` 만 돈다 <br> `IsFinishTheWork()` 가 FALSE 라 매번 그냥 나감 | 대기 |
| **입고 완료** | `IsFinishTheWork()` (`ScInfo.cpp:1594`) TRUE <br> = ONLINE·AUTO·ACTIVE + 상태 WAIT/IDLE + `SC_PROD_EMPTY` + `SC_TRN_COMPLETE` + D112 == 내부 작번 | `m_bInvoke = FALSE` <br> `JOB->Complete(작번, SC_JOB_TYPE_STORE)` <br> 작업 삭제 |

`IsStoPalletValid` 가 막는 것 (`Sc.cpp:2341`)

```cpp
패턴이 STO / SITE / AISLE 중 하나가 아니면      -> "작업패턴 이상"
패턴이 STO 인데 JobItem.GetStackerNum() != 내 호기 -> "SC#n 입고 되어야 합니다"
```

창고 확인(`m_nDestWareHouse`) 과 창고간 이동의 호기 확인은 **주석 처리되어 있다.**

### 2-2. 3층 출고 — RANK_4

| 상태 | 도는 함수 / 조건 | 결과 |
| --- | --- | --- |
| **평상시** | `InvokeCheck()` → `RetrieveRoutine()` → `RetrieveRoutine(RANK_4)` <br> `IsRetHomeStandReady()` FALSE 면 즉시 중단 | 아무 일 없음 |
| **3층 출고 H/S 가 비고 Up** | CV 쪽 `ParsingExtraFrame` 워드 +6 비트 → `SetRetHomeStandReady(TRUE)` <br> (출고 H/S 는 **화물 없고 Up** 이 준비 상태다) | `IsRetHomeStandReady()` TRUE |
| **앞 화물이 아직 안 나갔는지 확인** | `IsPrevPalletOnRetHS(트랙ID)` (`Sc.cpp:2387`) <br> → `JOB->FetchOnTheRetHomeStandJob(트랙)` 이 뭔가 찾으면 <br> "출고HS상태 이상으로 출발 못하고 있습니다" 띄우고 중단 | 앞 작업이 남아 있으면 새 출고 안 냄 |
| **출고 작업 고르기** | `JOB->FetchScRetJobByScNumberNPriority(호기, RANK_4)` (`Job.cpp:1002`) <br> ① 상태가 `JOB_STA_SC_OPER_REQUEST` 인 것만 <br> ② 패턴이 RET 또는 AISLE, 출발 호기가 나인 것만 <br> ③ AISLE 이면 `GetRank() == RANK_4 && nRank == RANK_4` 일 때 바로 반환 <br> ④ 아니면 `GetRank() == nRank` 인 것 중 **우선순위 큰 것**, 같으면 **먼저 등록된 것** | 없으면 중단 |
| **202 피킹대 제한** | 도착지가 202 이고, 201/203 에 대기 작업이 있고, <br> 진행 중인 202 작업 수 ≥ `m_nLimitStn202Picking2` 면 그 작업은 건너뛴다 | 202 쏠림 방지 |
| **출고 지시 전송** | `ReadStatus()` 다시 읽어 에러면 중단 <br> `Retrieve(작번, 시작 LOC, 0, "", GetScSelfRetHS(RANK_4))` <br> → **D171=2, D180=4**, D173/174/175 = Bank/Bay/Level | `m_bInvoke = TRUE` <br> `m_nPrevRtn = ROUTINE_RETRIEVE` <br> `m_nRetPrevRtn = RANK_4` <br> **`SetDestTrack(3층 출고 H/S 트랙ID)`** <br> `JOB_STA_SC_OPER_INVOKE` |
| **지시 확인 / 가짜 실패 처리** | 입고와 완전히 같다 (2 초 후 D112 확인 → 4 회 실패 시 `EStop`, <br> 3 초 후 D112 == 작번이면 DATA Setting) | |
| **랙에서 꺼내 오는 중** | `CompleteCheck()` 만 돈다 | 대기 |
| **3층 출고 H/S 에 내려놓음** | `IsFinishTheWork()` TRUE → `CompleteCheck()` (`Sc.cpp:1184`) <br> `m_ucInternalJobType == SC_JOB_TYPE_RETRIEVE` 갈래 | `JOB->SetDestTrack(작업, GetDestTrack())` <br> **`JOB_STA_CV_OPER_REQUEST`** <br> `JOB->Complete(작번, RETRIEVE, 1)` |
| **CV 가 받아 감** | CV 스레드의 `RetInvokeCheck3()` (PLC03) / `RetInvokeCheck6()` (PLC06) <br> ① 트랙 작번 == 0 <br> ② `FetchCvRetJobByTrackNum` <br> ③ 상태가 `JOB_STA_SC_OPER_INVOKE` 면 **return** (크레인이 먼저 처리 중) <br> ④ 상태가 `JOB_STA_CV_OPER_REQUEST` 가 아니면 skip <br> ⑤ `IsOnSensorIO(0)` <br> ⑥ `WriteTrackInfo(트랙, 작번, 작업종류, 목적지)` | `JOB_STA_CV_OPER_INVOKE` <br> 이후는 CV 문서의 출고대 도착 처리로 넘어감 |

`RetInvokeCheck3` 안의 `Sleep(0)` 은 주석 그대로
"크래인이 먼저 작업정보를 수신한것을 처리하기 위해서 연산 우선권을 다른 스레드에게 넘겨줌" 이다.
크레인 스레드와 CV 스레드가 같은 트랙을 동시에 만지는 것을 막는 유일한 장치다.

### 2-3. 3층 안에서 작업대끼리 옮기기 — SITE

입고 H/S 에 올라온 화물의 패턴이 `JOB_PATTERN_SITE` 면 랙에 넣지 않고
바로 다른 H/S 로 넘긴다.

| 상태 | 도는 함수 / 조건 | 결과 |
| --- | --- | --- |
| 입고 H/S 에 SITE 화물 도착 | `StoreRoutine(RANK_3)` 안에서 `GetPattern() == JOB_PATTERN_SITE` <br> → `SiteToSiteRoutine(RANK_3)` (`Sc.cpp:944`) | |
| 목적지 랭크 확인 | `JobItem.GetRank()` 가 `ECS_ERROR` 면 중단 <br> `GetRankCvTrackInfo(그 랭크)` 의 `IsRetHomeStandReady()` 가 아니면 중단 | 도착 H/S 가 비어 있어야 함 |
| 지시 전송 | `HsToHs(작번, GetScSelfStoHS(출발랭크), GetScSelfRetHS(도착랭크))` <br> → **D171=3**, D176=출발 H/S 번호, D180=도착 H/S 번호 | `m_ucInternalJobType = SC_JOB_TYPE_SITE_TO_SITE` <br> `m_nPrevRtn = ROUTINE_RETRIEVE` |
| 완료 | `CompleteCheck()` 의 SITE 갈래 | `SetDestTrack` → **`JOB_STA_CV_OPER_REQUEST`** <br> (`JOB->Complete` 는 부르지 않는다 — 작업이 안 끝났으니까) |

### 2-4. 랙투랙

층과 무관하다. 입고/출고가 둘 다 못 나갈 때만 돈다.

| 상태 | 도는 함수 | 결과 |
| --- | --- | --- |
| 입고·출고 모두 낼 것이 없음 | `RackToRackRoutine()` (`Sc.cpp:1069`) <br> `JOB->FetchScRackToRackJobByScNumber(호기)` | 없으면 아무 일 없음 |
| 지시 | `RackToRack(작번, 출발 LOC, 도착 LOC, ...)` → **D171=4** | `m_ucInternalJobType = SC_JOB_TYPE_RACK_TO_RACK` <br> `m_nPrevRtn = ROUTINE_STORE` |
| 완료 | `CompleteCheck()` 의 랙투랙 갈래 | `JOB->Complete(작번, SC_JOB_TYPE_STORE)` |

### 2-5. 에러

| 상태 | 도는 함수 | 결과 |
| --- | --- | --- |
| D101 == `SC_STA_ERROR` | `ErrorCheck()` (`Sc.cpp:1534`) <br> 같은 에러코드가 계속이면 한 번만 처리 (`m_wPrevErrCode`) | 로그 + `HOST->Error(...)` 보고 |
| 공출고 (`SC_ERR_CODE_EMPTY_RETRIEVE`) | 입고 작업 중에 났으면 "SC 로직체크" 만 남기고 끝 <br> 출고 중이면 `EQUIP_ERRORKIND_RET_EMPTY` 로 상위 보고 | |
| 이중입고 (`SC_ERR_CODE_DUAL_STORE`) | `JOB_STA_ERR_DUAL_STO` 로 바꾸고 상위 보고 <br> 이어서 `ErrorRoutine()` (`Sc.cpp:1382`) 이 <br> `JOB->FetchDualStoJobByScNumber(호기)` 로 재지정 로케이션을 받아 <br> 패턴에 따라 `Store` 또는 `RackToRack` 재전송 | 자동 재지정 |
| 통신 두절 | `ReadStatus()` 7 회 연속 실패 → `SC_ERR_CODE_COMM_ERROR` + `SC_STA_ERROR` | |

### 2-6. 보조 지시 (사람이 누르는 것)

| 함수 | D171 / 주소 | 비고 |
| --- | --- | --- |
| `CallToHome()` (`Sc.cpp:2172`) | 홈 복귀 | 완료 시 `CompleteCheck` 가 작번만 지우고 나감 |
| `EStop()` (`:2194`) | 비상정지 | 지시 오류 4 회 누적 시 자동 호출 |
| `Active()` / `Stop()` (`:2212` / `:2229`) | 기동 / 서행정지 | Stop 은 지상반 데이터를 유지한다 |
| `Reset()` (`:2246`) | 에러 리셋 | |
| `Delete(포크)` (`:2263`) | 현 작업 DATA 삭제 | 지금은 자동 호출부가 전부 주석 처리됨 |
| `RcMode(bOnLine)` (`:2285`) | 지상반 온라인/오프라인 | |

## 3. 1층과 3층 중 어느 쪽을 먼저 하는가

`InvokeCheck()` (`Sc.cpp:214`) 는 **입고와 출고를 번갈아** 본다.

```cpp
void CSc::InvokeCheck()
{
    if (SC_INFO->IsReadyToWork() == FALSE)  return;

    switch (SC_INFO->m_nPrevRtn) {
    case ROUTINE_STORE:                     // 직전이 입고였으면
        if (RetrieveRoutine())    return;   //   출고를 먼저
        if (StoreRoutine())       return;
        if (RackToRackRoutine())  return;
        break;
    case ROUTINE_RETRIEVE:                  // 직전이 출고였으면
        if (StoreRoutine())       return;   //   입고를 먼저
        if (RetrieveRoutine())    return;
        if (RackToRackRoutine())  return;
        break;
    }
}
```

`IsReadyToWork()` (`ScInfo.cpp:1620`) 는 이 중 하나라도 걸리면 FALSE 다.

```
m_bInvoke                    이미 지시를 냈다
m_ucRcMode  != ONLINE        지상반 오프라인
m_ucScMode  != AUTO          수동 모드
m_ucActive  != SC_ACTIVE     정지 상태
m_ucStatus  == SC_STA_MOVE   이동 중
m_ucStatus  == SC_STA_ERROR  에러
```

그 다음 **입고 안에서 1층/3층**, **출고 안에서 1층/3층** 을 다시 번갈아 본다.
`StoreRoutine()` (`Sc.cpp:259`) 과 `RetrieveRoutine()` (`Sc.cpp:321`) 이 같은 모양이다.

```cpp
// 1층과 3층 트랙이 둘 다 입고 준비되어 있는 경우에만 가중치를 본다
if (pTrack1 && pTrack1->IsStoHomeStandReady() && pTrack3 && pTrack3->IsStoHomeStandReady())
{
    if ((nWeightRank == RANK_1) || (nWeightRank == RANK_3))
    {
        nNormalRank = (nWeightRank == RANK_1) ? RANK_3 : RANK_1;

        if (GetHsWorkCount(nWeightRank) < nWeightValue)      // 가중치 층을 nWeightValue 번까지 몰아준다
            if (StoreRoutine(nWeightRank)) { AddHsWorkCount(nWeightRank); InitHsWorkCount(nNormalRank); return TRUE; }

        if (StoreRoutine(nNormalRank))     { AddHsWorkCount(nNormalRank); InitHsWorkCount(nWeightRank); return TRUE; }
        ...
    }
}

InitHsWorkCount(RANK_1);  InitHsWorkCount(RANK_3);

switch (m_nStoPrevRtn) {                                     // 초기값 RANK_1
case RANK_1:  if(StoreRoutine(RANK_3)) return TRUE;  if(StoreRoutine(RANK_1)) return TRUE;  break;
case RANK_3:  if(StoreRoutine(RANK_1)) return TRUE;  if(StoreRoutine(RANK_3)) return TRUE;  break;
}
```

정리하면 이렇다.

| 조건 | 하는 일 |
| --- | --- |
| 1층·3층 H/S 가 **둘 다** 준비됐고 `m_nStoWeightRank` 가 1 또는 3 | 그 층을 `m_nStoWeightValue` 번 연속 먼저 처리, 그 다음 반대 층 |
| 그 외 (한쪽만 준비됐거나 가중치 설정 안 함) | `m_nStoPrevRtn` 의 **반대 층**부터 시도 — 순수 교대 |

- 출고 쪽은 `m_nRetWeightRank` / `m_nRetWeightValue` 로 RANK_2 ↔ RANK_4 를 같은 식으로 가른다.
- 두 값 모두 기본이 **0 (= 가중치 없음)** 이라, 실제로는 아래쪽 순수 교대만 돈다.
  소스 주석에도 `// 0` `// 2` 로 적혀 있고 `nNormalRank` 계산 옆에는 `// 사용안함?` 이 붙어 있다.
- 카운터는 `AddHsWorkCount` / `InitHsWorkCount` 로 층마다 따로 센다.
  가중치를 안 쓰는 경로에 들어오면 매번 둘 다 0 으로 지운다.

## 4. 3층에만 있는 제약

### 4-1. 3층 진입 대수 제한 — `CCv::MovingTrackCheckPlc3` (`Cv.cpp:2495`)

크레인이 아니라 **CV(PLC03) 쪽** 이지만, 3층 출고 흐름을 실제로 조이는 것은 이쪽이다.
피킹 라인 세 줄마다 "지금 라인 안에 몇 개가 돌고 있는지" 를 세서
한도를 넘으면 대기 트랙에서 **출발 허가를 안 준다.**

| 라인 | 대기 트랙 | 작업 트랙 (여기 있는 화물 수를 센다) | 한도 |
| --- | --- | --- | --- |
| 피킹 1 (201) | 3018, 3023 | 3007~3014, 3019~3021 (11개) | `m_nLimitStn201Picking1` |
| 피킹 3 (203) | 3033, 3037 | 3002~3005, 3027~3029, 3034~3036 (10개) | `m_nLimitStn203Picking3` |
| 피킹 4 (208) | 3046, 3049 | 3055~3059, 3041, 3042, 3047, 3050~3052 (11개) | `m_nLimitStn208Picking4` |

```
현재수 = 작업 트랙 중 작번이 있는 개수
       + 대기 트랙 중 출발준비신호(IsReadyStartToMove)가 이미 On 이면서 작번이 있는 개수

현재수 >= 한도  -> 그 라인의 대기 트랙은 건드리지 않는다 (진입 금지 유지)
현재수 <  한도  -> 작번이 있고 아직 출발준비가 아닌 대기 트랙에 비트를 세운다 (진입 허가)

비트 자리 k = 2*라인 + 대기트랙순번  ->  D558 한 워드에 6 비트
```

PLC06 쪽에도 같은 일을 하는 `MovingTrackCheckPlc6()` 이 있다.

### 4-2. 202 피킹대 동시 작업 수 제한

`FetchScRetJobByScNumberNPriority` 안에 박혀 있다 (`Job.cpp:1002`).
크레인이 3층 출고 작업을 고를 때 202 로 갈 작업을 걸러 낸다.

```
nCount202 = 피킹 작업 중 도착지 202 이고 상태가 SC_OPER_REQUEST 가 아닌 것
          + 트랙 3006(=202) 에 피킹 작업이 실물로 있는데 작업목록에는 없는 경우 1 더함
nCountEtc = 피킹 작업 중 도착지가 201 또는 203 인 것

도착지 202 && nCountEtc > 0 && nCount202 >= m_nLimitStn202Picking2  ->  이 작업은 건너뛴다
```

201/203 에 아무것도 없으면 제한하지 않는다. 202 만 쏠릴 때만 막는다.

### 4-3. 호기 이동은 3층으로만 나간다

`CJobItem::GetRank()` 와 `FetchScRetJobByScNumberNPriority` 양쪽에서
`JOB_PATTERN_AISLE` 을 `RANK_4` 로 고정해 두었다 (2011.02.06 RTV→C/V 교체건).
원본은 `RANK_2` (1층) 였고, 지금도 원본 줄이 주석으로 남아 있다.

## 5. 한눈에 보는 3층 출고 한 바퀴

```
[상위]  출고 지시 (도착 작업대 = 200~209 / 212~215)
   |
   |  CLib::GetRank(도착 작업대) -> RANK_4
   v
[JOB]  JOB_STA_SC_OPER_REQUEST
   |
   |  CSc::ThreadProc 400ms
   |  InvokeCheck -> RetrieveRoutine() -> RetrieveRoutine(RANK_4)
   |     IsRetHomeStandReady()          3층 출고 H/S 가 비고 Up
   |     IsPrevPalletOnRetHS()          앞 화물이 안 남아 있음
   |     FetchScRetJobByScNumberNPriority(호기, RANK_4)   우선순위 -> 등록순
   |     202 제한 통과
   v
[크레인] Retrieve(...)  D171=2, D180=4, D173/174/175=Bank/Bay/Level
   |     m_bInvoke = TRUE, SetDestTrack(3층 출고 H/S 트랙)
   v
[JOB]  JOB_STA_SC_OPER_INVOKE
   |
   |  CompleteCheck -> IsFinishTheWork()
   |     ONLINE + AUTO + ACTIVE + 상태 WAIT/IDLE + PROD_EMPTY + TRN_COMPLETE + D112 일치
   v
[JOB]  JOB_STA_CV_OPER_REQUEST     (SetDestTrack 반영, JOB->Complete(RETRIEVE,1))
   |
   |  CCv::RetInvokeCheck3() / RetInvokeCheck6()
   |     트랙 작번 0 + IsOnSensorIO(0) + WriteTrackInfo
   v
[JOB]  JOB_STA_CV_OPER_INVOKE
   |
   |  MovingTrackCheckPlc3() 가 라인 진입을 허가해 줄 때까지 대기 트랙에서 정지
   v
[CV]   StartInvokeCheck / ArrivedCheck  -> 작업대 도착  (CV 문서 참조)
```

## 6. 3층 출고 한 바퀴 — 1층과 다른 점만

| | 1층 | 3층 |
| --- | --- | --- |
| 입고 H/S RANK | 1 | **3** |
| 출고 H/S RANK | 2 | **4** |
| 크레인에 나가는 H/S 번호 | D176=1 / D180=2 | **D176=3 / D180=4** |
| CV PLC | PLC02 (SC_2~7) / PLC05 (SC_8~11) | PLC03 (SC_2~7) / PLC06 (SC_8~11) |
| CV 쪽 출고 H/S 출발 함수 | `RetInvokeCheck2()` / `RetInvokeCheck5()` | `RetInvokeCheck3()` / `RetInvokeCheck6()` |
| 라인 진입 제한 | 없음 (`ReStartRoutine2` 로 결정대→출고대만) | `MovingTrackCheckPlc3/6` 이 D558 로 진입 허가 |
| 피킹대 쏠림 제한 | 없음 | 202 동시 작업 수 제한 |
| 호기 이동(AISLE) 목적지 | 원본은 여기였음 | **지금은 이쪽으로 고정** |

## 7. 참고 — 확인에 쓴 자리

| 파일 | 줄 | 함수 |
| --- | --- | --- |
| `Sc.cpp` | 54 | `ReadStatus` |
| `Sc.cpp` | 214 | `InvokeCheck` |
| `Sc.cpp` | 259 / 321 | `StoreRoutine()` / `RetrieveRoutine()` — 층 교대 |
| `Sc.cpp` | 380 | `StoreRoutine()` — **주석 처리된 옛 판** |
| `Sc.cpp` | 417 / 713 | `StoreRoutine(nRank)` / `RetrieveRoutine(nRank)` |
| `Sc.cpp` | 944 / 1069 | `SiteToSiteRoutine` / `RackToRackRoutine` |
| `Sc.cpp` | 1184 | `CompleteCheck` |
| `Sc.cpp` | 1382 / 1534 | `ErrorRoutine` / `ErrorCheck` |
| `Sc.cpp` | 1834 / 1920 / 2006 / 2068 | `Store` / `Retrieve` / `HsToHs` / `RackToRack` |
| `Sc.cpp` | 2336~2490 | `IsInvoke` `IsStoPalletValid` `IsPrevPalletOnRetHS` `GetScSelfStoHS` `GetScSelfRetHS` `IsRetPalletValid` |
| `Sc.cpp` | 2556 | `ThreadProc` |
| `ScInfo.cpp` | 1548 / 1594 / 1620 | `GetRankCvTrackInfo` / `IsFinishTheWork` / `IsReadyToWork` |
| `CvTrackInfo.cpp` | 585 / 590 | `IsStoHomeStandReady` / `IsRetHomeStandReady` |
| `Cv.cpp` | 132 | `ParsingExtraFrame` — H/S 준비 비트 |
| `Cv.cpp` | 829 / 1003 | `RetInvokeCheck3` / `RetInvokeCheck6` |
| `Cv.cpp` | 2495 | `MovingTrackCheckPlc3` |
| `Lib.cpp` | 384 / 448 | `GetScStoHSTrack` / `GetScRetHSTrack` — PLC 별 H/S 비트 순서 |
| `Lib.cpp` | 513 / 534 | `GetScHSCnt` / `GetScHSTrackPerRank` |
| `Lib.cpp` | 2343 | `GetRank` — 도착 작업대 → 랭크 |
| `Job.cpp` | 1002 | `FetchScRetJobByScNumberNPriority` |
| `Job.cpp` | 1327 / 1349 | `FetchOnTheRetHomeStandJob` |
| `JobItem.cpp` | 587 | `CJobItem::GetRank` |
| `EcsDoc.cpp` | 1489 | `InitEquip` — RANK ↔ 트랙 묶기 |

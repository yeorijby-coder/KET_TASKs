# 원본 ECS(VC++6.0) 스테이션별 동작 정리

CV_DATA 에 작업대 번호(`HOST_STN_NO`)가 들어 있는 트랙 전부에 대해,
원본 ECS 가 그 자리를 어떤 함수로 어떤 순서에 처리하는지 정리한 것이다.

- 원본 소스 : `D:\인수인계\한국단자\Ecs Server\EcsSv` (2026-01-09 판)
- 상수 : `Common\Include\Ecs\EcsDef.h`
- 이 문서는 소스에서 표를 기계적으로 뽑아 만들었다. 손으로 적은 값이 아니다.

## 0. 먼저 알아야 할 것

### 번호가 세 가지다

| 부르는 이름 | 예 | 어디서 쓰나 |
| --- | --- | --- |
| 작업대(스테이션) 번호 | 207 | 상위(HOST)와 주고받는 번호. `ECS_STN_POS_*`, `CV_DATA.HOST_STN_NO` |
| 원본 트랙 ID | 3001 | 원본 ECS 내부 번호. `CNV_STN_POS_*` |
| 새 WCS 트랙 번호 | 301 | `CV_DATA.MC_NO`. 새로 만든 체계라 원본과 다르다 |

`CLib::ConvertCustomToPosition(스테이션)` 이 스테이션 → 원본 트랙 ID,
`CLib::ConvertPositionToCustom(트랙ID)` 가 그 반대다.

### 매 주기에 실제로 도는 함수 - `CvThreadProc.cpp:44~71`

PLC 별 `switch` 블록은 **통째로 주석 처리**되어 있다.
그래서 `StartInvokeCheck2/3/4/6`, `ArrivedCheck2/3/4/6`,
`CopyTrackData2/3/5/6`, `DeleteTrackData2/3/5/6` 은 **죽은 코드**다.
실제로는 아래가 CV 스레드마다 매 주기 평면으로 돈다.

```
ReadStatus()                 PLC 읽기 -> ParsingFrame / ParsingExtraFrame
StartInvokeCheck()           입고대 목록을 돌며 출발 지시      [모든 PLC]
NewStartRoutinePlc5()                                        [PLC5]
NewStartRoutinePlc2()                                        [PLC2]
ReStartRoutine2()            1층 결정대 -> 빈 출고대           [PLC2]
RetInvokeCheck2/3/4/5/6()    크레인 출고 H/S 에서 출발 지시    [각 PLC]
ArrivedCheck()               출고대 목록을 돌며 도착 처리      [모든 PLC]
MovingTrackCheckPlc3() / 6()  움직이는 팔레트 수 집계          [PLC3 / PLC6]
CheckBoxStoArrived()         트윈 포트 도착 작업 요청
CopyTrackData() / DeleteTrackData()   PLC 간 DATA 이동
```

### 신호를 읽는 곳 - `Cv.cpp:132` `ParsingExtraFrame`

| 워드 | 뜻 | 하는 일 |
| --- | --- | --- |
| +0 | 입고대(출발대) 준비 | 입고대 목록 순서대로 비트 → `SetStoStationReady()` |
| +2 | 출고대(도착대) 준비 | 출고대 목록 순서대로 비트 → `SetRetStationReady()` |
| +4 | 입고 H/S 준비 | `SetStoHomeStandReady()` |
| +6 | 출고 H/S 준비 | `SetRetHomeStandReady()` |

**비트 자리는 목록의 순서(seq)** 다. 아래 표의 `입고seq` / `출고seq` 가 그 비트 번호다.

### 자리의 성격을 가르는 술어 - `Lib.cpp`

| 술어 | 줄 | 해당 자리 | 무엇이 달라지나 |
| --- | --- | --- | --- |
| `IsStartNDestStation` | 2469 | 107, 108, 200, 201, 202, 203, 204, 205, 206, 208, 209, 212, 213, 214, 215, 221, 222 | `StartInvokeCheck` 의 출발 조건이 뒤집힌다 |
| `IsPalletMagazine` | 2500 | 트랙 3001 (= 스테이션 207) | 상위에 입고 요청을 스스로 올린다 |
| `IsDeleteDataStation` | 3455 | 207, 251 | 도착하면 트랙의 작번을 지운다 |
| `IsSizeChecker` | 3466 | 트랙 2018, 2025, 3004, 3012, 3058, 6047, 6052 | 사이즈 체커 표시 |

#### `IsStartNDestStation` 이 뒤집는 것 (`Cv.cpp:399~410`)

```cpp
if ( CLib::IsStartNDestStation(nStationNum) == TRUE )
{
    if ( pCvTrackInfo->GetLuggNum() == 0) continue;   // 작번이 있어야 출발
}
else
{
    if ( pCvTrackInfo->GetLuggNum() != 0) continue;   // 작번이 없어야 출발
}
```

- **TRUE** : 도착지이면서 출발지인 자리. 화물(작번)이 놓여 있어야 다음 작업으로 내보낸다.
- **FALSE** : 순수 출발지. 트랙이 비어 있어야 새 화물을 받아 내보낸다.

#### 그룹 출고대 - `IsArrivedToRetStation` (`Lib.cpp:3391`)

목적지가 그룹 번호로 내려오면 그 안의 어느 자리에 도착해도 완료로 본다.

| 목적지 | 도착으로 인정되는 자리 |
| --- | --- |
| 105 | 103, 104 |
| 204 | 201, 202, 203, 208, 212, 213 |
| 221 | 221, 222 |
| 222 | 221, 222 |
| 241 | 4032, 4033, 4034 |
| 242 | 4029, 4030, 4031 |

그 외에는 `default: 목적지 == 현재 스테이션` 일 때만 도착이다.

## 1. 전체 표

| PLC | WCS트랙 | 작업대 | 원본트랙 | 입고seq | 출고seq | 성격 |
| --- | --- | --- | --- | --- | --- | --- |
| 02 | 217 | 101 | 2017 | 0 | - | 일반 |
| 02 | 224 | 102 | 2024 | 2 | - | 일반 |
| 02 | 206 | 103 | 2006 | - | 0 | 그룹105 소속 |
| 02 | 212 | 104 | 2012 | - | 1 | 그룹105 소속 |
| 02 | 2101 | 105 | 2101 | - | 4 | 그룹 목적지 |
| 02 | 218 | 107 | 2018 | 1 | 2 | 출발지겸도착지, 사이즈체커 |
| 02 | 225 | 108 | 2025 | 3 | 3 | 출발지겸도착지, 사이즈체커 |
| 02 | 204 | 151 | 2004 | - | - | **목록에 없음** |
| 02 | 208 | 152 | 2008 | - | - | **목록에 없음** |
| 02 | 216 | 153 | 2016 | - | - | **목록에 없음** |
| 02 | 220 | 154 | 2020 | - | - | **목록에 없음** |
| 02 | 259 | 155 | 2059 | - | - | **목록에 없음** |
| 02 | 249 | 156 | 2049 | - | - | **목록에 없음** |
| 02 | 232 | 171 | 2032 | - | - | **목록에 없음** |
| 03 | 311 | 200 | 3011 | 5 | 5 | 출발지겸도착지 |
| 03 | 309 | 201 | 3009 | 4 | 4 | 출발지겸도착지, 그룹204 소속 |
| 03 | 306 | 202 | 3006 | 3 | 3 | 출발지겸도착지, 그룹204 소속 |
| 03 | 303 | 203 | 3003 | 1 | 1 | 출발지겸도착지, 그룹204 소속 |
| 03 | 312 | 205 | 3012 | 6 | 6 | 출발지겸도착지, 사이즈체커 |
| 03 | 304 | 206 | 3004 | 2 | 2 | 출발지겸도착지, 사이즈체커 |
| 03 | 301 | 207 | 3001 | 0 | 0 | 팔레트 매거진, 도착시 DATA 삭제 |
| 03 | 357 | 208 | 3057 | 7 | 7 | 출발지겸도착지, 그룹204 소속 |
| 03 | 358 | 209 | 3058 | 8 | 8 | 출발지겸도착지, 사이즈체커 |
| 04 | 407 | 211 | 4007 | 0 | - | 일반 |
| 04 | 419 | 221 | 4019 | 2 | 1 | 출발지겸도착지, 그룹 목적지, 그룹221/222 소속 |
| 04 | 420 | 222 | 4020 | 3 | 2 | 출발지겸도착지, 그룹 목적지, 그룹221/222 소속 |
| 04 | 428 | 231 | 4028 | 4 | 3 | 일반 |
| 04 | 434 | 241 | 4034 | - | 5 | 그룹 목적지 |
| 04 | 431 | 242 | 4031 | - | 4 | 그룹 목적지 |
| 04 | 415 | 251 | 4015 | 1 | 0 | 도착시 DATA 삭제 |
| 05 | 506 | 157 | 5006 | - | - | **목록에 없음** |
| 05 | 512 | 158 | 5012 | - | - | **목록에 없음** |
| 05 | 518 | 159 | 5018 | - | - | **목록에 없음** |
| 05 | 524 | 160 | 5024 | - | - | **목록에 없음** |
| 06 | 651 | 212 | 6051 | 1 | 1 | 출발지겸도착지, 그룹204 소속 |
| 06 | 646 | 213 | 6046 | 0 | 0 | 출발지겸도착지, 그룹204 소속 |
| 06 | 652 | 214 | 6052 | 3 | 3 | 출발지겸도착지, 사이즈체커 |
| 06 | 647 | 215 | 6047 | 2 | 2 | 출발지겸도착지, 사이즈체커 |

## 2. 유형별 동작 순서

자리의 성격에 따라 도는 함수와 순서가 갈린다. 여섯 가지다.

### 유형 A — 순수 입고대 (입고대 목록에만 있음)

해당 : 101, 102, 211

| 상태 | 화물 | DATA | 작업 | 신호 | 도는 함수 |
| --- | --- | --- | --- | --- | --- |
| 평상시 | 없음 | 없음 | 없음 | 입고대 OFF | `StartInvokeCheck` 에서 `IsStoStationReady()==FALSE` → continue |
| 화물 놓임 | 있음 | 없음 | 없음 | **입고대 ON** | `ParsingExtraFrame` → `SetStoStationReady(TRUE)` |
| 작업 생김 | 있음 | 없음 | 있음 | 입고대 ON | `JOB->FetchCvStoJobByStartStation(작업대)`, 상태 `JOB_STA_CV_NEW` |
| 출발 지시 | 있음 | **기록** | 있음 | 입고대 ON | `WriteTrackInfo(트랙, 작번, 작업구분, 목적지)` → `JOB->SetStatus(JOB_STA_CV_OPER_INVOKE)` |
| 출발 후 | 없음 | 없음 | 있음 | 입고대 OFF | 화물이 떠나면 다음 주기부터 다시 평상시 |

`IsStartNDestStation` 이 FALSE 라 **트랙이 비어 있어야(`GetLuggNum()==0`) 출발**한다.

### 유형 B — 순수 출고대 (출고대 목록에만 있음)

해당 : 103, 104, 105, 241, 242

| 상태 | 화물 | DATA | 작업 | 신호 | 도는 함수 |
| --- | --- | --- | --- | --- | --- |
| 평상시 | 없음 | 없음 | 없음 | 출고대 OFF | `ArrivedCheck` 에서 `GetLuggNum()==0` → continue |
| 화물 도착 | 있음 | 있음 | 있음 | **출고대 ON** | `ArrivedCheck` 진입 조건 `GetLuggNum()!=0 && IsRetStationReady()` |
| 도착 판정 | 있음 | 있음 | 있음 | 출고대 ON | `JOB->Find(작번)` → 상태 `JOB_STA_CV_OPER_INVOKE` 확인 → `CLib::IsArrivedToRetStation(트랙, 목적지)` |
| 도착 보고 | 있음 | 있음 | **완료** | 출고대 ON | `JOB->Arrived(작번)` / 수동작업이면 `JOB->Remove()` |
| 반출 | 없음 | 없음 | 없음 | 출고대 OFF | 지게차가 가져가면 평상시로 |

목적지가 다른 자리로 바뀌어 도착한 경우(`GetDestPos() != 자기자신`)에는
먼저 `WriteTrackInfo` 로 트랙 목적지를 자기로 고치고 `JOB->SetDestPos()` 로 작업 목적지도 맞춘다.

### 유형 C — 입출고 겸용 (`IsStartNDestStation` TRUE)

해당 : 107, 108, 200, 201, 202, 203, 205, 206, 208, 209, 221, 222, 212, 213, 214, 215

도착지이면서 다음 작업의 출발지다. 피킹대 / 사이즈체커 / 도착대가 여기 속한다.

| 상태 | 화물 | DATA | 작업 | 신호 | 도는 함수 |
| --- | --- | --- | --- | --- | --- |
| 평상시 | 없음 | 없음 | 없음 | 둘 다 OFF | 아무것도 안 함 |
| 화물 도착 | 있음 | 있음 | 있음 | **출고대 ON** | `ArrivedCheck` → `IsArrivedToRetStation` → `JOB->Arrived(작번)` |
| 도착 후 대기 | 있음 | **남음** | 없음 | 입고대 ON | 작번이 트랙에 그대로 남는다 (지우지 않는다) |
| 다음 작업 | 있음 | 남음 | 있음 | 입고대 ON | `StartInvokeCheck` → `IsStartNDestStation==TRUE` 라 **`GetLuggNum()!=0` 이어야 출발** |
| 출발 지시 | 있음 | **갱신** | 있음 | 입고대 ON | `WriteTrackInfo(트랙, 새작번, …)` → `JOB_STA_CV_OPER_INVOKE` |

핵심은 **도착해도 트랙의 작번을 지우지 않는다**는 것이다.
그 작번이 다음 출발의 전제가 된다.

### 유형 D — 입출고 겸용 + 도착 시 DATA 삭제 (`IsDeleteDataStation` TRUE)

해당 : 207, 251

유형 C 와 반대로 **도착하면 트랙의 작번을 지운다**.
원본 주석 : *"현재 화물이 이동없이 다음 작업이 바로 같이 오는 경우는 도착을 확인하고 ECS에서 삭제한다"*

| 상태 | 화물 | DATA | 작업 | 신호 | 도는 함수 |
| --- | --- | --- | --- | --- | --- |
| 평상시 | 없음 | 없음 | 없음 | 둘 다 OFF | 아무것도 안 함 |
| 화물 도착 | 있음 | 있음 | 있음 | **출고대 ON** | `ArrivedCheck` → `IsArrivedToRetStation` 통과 |
| DATA 삭제 | 있음 | **삭제** | 있음 | 출고대 ON | `IsDeleteDataStation` TRUE → `WriteTrackInfo(트랙, **0**, …)` (`Cv.cpp:1200`) |
| 도착 보고 | 있음 | 없음 | 완료 | 출고대 ON | `JOB->Arrived(작번)` |
| 작업 없이 DATA 만 남음 | 있음 | 있음 | **없음** | **출고대 ON** | `JOB->Find()` NULL → `IsDeleteDataStation` TRUE → `WriteTrackInfo(트랙, 0, …)` 후 continue (`Cv.cpp:1075`) |
| 다음 출발 | 없음 | 없음 | 있음 | 입고대 ON | `IsStartNDestStation==FALSE` 라 **`GetLuggNum()==0` 이어야 출발** |

> **주의** — 두 삭제 지점 모두 `GetLuggNum()!=0 && IsRetStationReady()==TRUE` **안쪽**에 있다.
> 즉 **출고대 신호가 켜져 있어야** 청소가 돈다. 꺼져 있으면 `ArrivedCheck` 는 그 자리를 건너뛴다.

### 유형 E — 입고대/출고대 목록에 없음 (RGV 적재 · 대기 트랙)

해당 : 151, 152, 153, 154, 155, 156, 171, 157, 158, 159, 160

`GetLinearStoStnNumPerCvPlc` / `GetLinearRetStnNumPerCvPlc` 어디에도 없다.
그래서 **`StartInvokeCheck` 와 `ArrivedCheck` 가 아예 돌지 않는다.**
대신 `CLib::GetRgvLoadingTrack` / `GetWaitTrackPerPlc` 목록에 들어 있고, 아래가 처리한다.

| 함수 | 가드 | 하는 일 |
| --- | --- | --- |
| `NewStartRoutinePlc2()` | `m_nNum != 1 && m_nNum != 4` | 1층 RGV 적재 트랙을 순서대로 보며 출발시킨다 |
| `NewStartRoutinePlc5()` | `m_nNum != 4` | 1층 신규 라인 쪽 |
| `ReStartRoutine2()` | `m_nNum != 1` | **트랙 2032(작업대 171, 출고위치 결정대)** 에서 빈 출고대로 다시 출발 |

PLC5(1F NEW)는 입고대/출고대 목록 자체가 비어 있어 157~160 은 전부 이 경로다.

### 유형 F — 231 (3F BOX BCR 이동 지시대)

입고대 seq 4 / 출고대 seq 3 이지만 `IsStartNDestStation` 도 `IsDeleteDataStation` 도 아니다.
대신 `StartInvokeCheck` 에 **이 자리만의 예외**가 있다 (`Cv.cpp:456`).

```cpp
if(nStationNum == ECS_STN_POS_3F_BOX_231)
{
    JOB->Arrived(JobItem.m_nLuggNum);   // 출발 지시와 동시에 도착 완료 처리
}
```

출발시키면서 그 자리에서 바로 도착 보고까지 올린다. 도착을 따로 기다리지 않는다.

## 3. 자리별 상세

### PLC 02

#### 작업대 101 — WCS트랙 217 / 원본트랙 2017

- 성격 : **유형 A 순수 입고대**
- 입고대 신호 : `ParsingExtraFrame` 워드+0 의 **비트 0**
- `IsStartNDestStation` **FALSE** — 출발하려면 트랙이 비어 있어야 한다.

#### 작업대 102 — WCS트랙 224 / 원본트랙 2024

- 성격 : **유형 A 순수 입고대**
- 입고대 신호 : `ParsingExtraFrame` 워드+0 의 **비트 2**
- `IsStartNDestStation` **FALSE** — 출발하려면 트랙이 비어 있어야 한다.

#### 작업대 103 — WCS트랙 206 / 원본트랙 2006

- 성격 : **유형 B 순수 출고대**
- 출고대 신호 : `ParsingExtraFrame` 워드+2 의 **비트 0**
- 그룹 105 의 도착 인정 자리다.

#### 작업대 104 — WCS트랙 212 / 원본트랙 2012

- 성격 : **유형 B 순수 출고대**
- 출고대 신호 : `ParsingExtraFrame` 워드+2 의 **비트 1**
- 그룹 105 의 도착 인정 자리다.

#### 작업대 105 — WCS트랙 2101 / 원본트랙 2101 — 1F 그룹 출고대

- 성격 : **유형 B 순수 출고대**
- 출고대 신호 : `ParsingExtraFrame` 워드+2 의 **비트 4**
- **그룹 목적지** — 목적지가 105 로 오면 103, 104 중 어디에 도착해도 완료로 본다.

#### 작업대 107 — WCS트랙 218 / 원본트랙 2018

- 성격 : **유형 C 입출고 겸용**
- 입고대 신호 : `ParsingExtraFrame` 워드+0 의 **비트 1**
- 출고대 신호 : `ParsingExtraFrame` 워드+2 의 **비트 2**
- `IsStartNDestStation` **TRUE** — 출발하려면 트랙에 작번이 있어야 한다.
- `IsSizeChecker` **TRUE** — 사이즈 체커로 표시된다.

#### 작업대 108 — WCS트랙 225 / 원본트랙 2025

- 성격 : **유형 C 입출고 겸용**
- 입고대 신호 : `ParsingExtraFrame` 워드+0 의 **비트 3**
- 출고대 신호 : `ParsingExtraFrame` 워드+2 의 **비트 3**
- `IsStartNDestStation` **TRUE** — 출발하려면 트랙에 작번이 있어야 한다.
- `IsSizeChecker` **TRUE** — 사이즈 체커로 표시된다.

#### 작업대 151 — WCS트랙 204 / 원본트랙 2004

- 성격 : **유형 E 목록 없음 (RGV/대기)**
- 입고대/출고대 목록에 없어 `StartInvokeCheck` / `ArrivedCheck` 가 돌지 않는다.
- `GetRgvLoadingTrack` / `GetWaitTrackPerPlc` 목록에 있고 `NewStartRoutinePlc2/5` 가 처리한다.

#### 작업대 152 — WCS트랙 208 / 원본트랙 2008

- 성격 : **유형 E 목록 없음 (RGV/대기)**
- 입고대/출고대 목록에 없어 `StartInvokeCheck` / `ArrivedCheck` 가 돌지 않는다.
- `GetRgvLoadingTrack` / `GetWaitTrackPerPlc` 목록에 있고 `NewStartRoutinePlc2/5` 가 처리한다.

#### 작업대 153 — WCS트랙 216 / 원본트랙 2016

- 성격 : **유형 E 목록 없음 (RGV/대기)**
- 입고대/출고대 목록에 없어 `StartInvokeCheck` / `ArrivedCheck` 가 돌지 않는다.
- `GetRgvLoadingTrack` / `GetWaitTrackPerPlc` 목록에 있고 `NewStartRoutinePlc2/5` 가 처리한다.

#### 작업대 154 — WCS트랙 220 / 원본트랙 2020

- 성격 : **유형 E 목록 없음 (RGV/대기)**
- 입고대/출고대 목록에 없어 `StartInvokeCheck` / `ArrivedCheck` 가 돌지 않는다.
- `GetRgvLoadingTrack` / `GetWaitTrackPerPlc` 목록에 있고 `NewStartRoutinePlc2/5` 가 처리한다.

#### 작업대 155 — WCS트랙 259 / 원본트랙 2059

- 성격 : **유형 E 목록 없음 (RGV/대기)**
- 입고대/출고대 목록에 없어 `StartInvokeCheck` / `ArrivedCheck` 가 돌지 않는다.
- `GetRgvLoadingTrack` / `GetWaitTrackPerPlc` 목록에 있고 `NewStartRoutinePlc2/5` 가 처리한다.

#### 작업대 156 — WCS트랙 249 / 원본트랙 2049

- 성격 : **유형 E 목록 없음 (RGV/대기)**
- 입고대/출고대 목록에 없어 `StartInvokeCheck` / `ArrivedCheck` 가 돌지 않는다.
- `GetRgvLoadingTrack` / `GetWaitTrackPerPlc` 목록에 있고 `NewStartRoutinePlc2/5` 가 처리한다.

#### 작업대 171 — WCS트랙 232 / 원본트랙 2032

- 성격 : **유형 E 목록 없음 (RGV/대기)**
- 입고대/출고대 목록에 없어 `StartInvokeCheck` / `ArrivedCheck` 가 돌지 않는다.
- `ReStartRoutine2()` 가 이 트랙만 직접 다룬다 (`Cv.cpp:2253` `nTrackNum = CNV_STN_POS_1F_PLT_171`).

### PLC 03

#### 작업대 200 — WCS트랙 311 / 원본트랙 3011

- 성격 : **유형 C 입출고 겸용**
- 입고대 신호 : `ParsingExtraFrame` 워드+0 의 **비트 5**
- 출고대 신호 : `ParsingExtraFrame` 워드+2 의 **비트 5**
- `IsStartNDestStation` **TRUE** — 출발하려면 트랙에 작번이 있어야 한다.

#### 작업대 201 — WCS트랙 309 / 원본트랙 3009

- 성격 : **유형 C 입출고 겸용**
- 입고대 신호 : `ParsingExtraFrame` 워드+0 의 **비트 4**
- 출고대 신호 : `ParsingExtraFrame` 워드+2 의 **비트 4**
- `IsStartNDestStation` **TRUE** — 출발하려면 트랙에 작번이 있어야 한다.
- 그룹 204 의 도착 인정 자리다.

#### 작업대 202 — WCS트랙 306 / 원본트랙 3006

- 성격 : **유형 C 입출고 겸용**
- 입고대 신호 : `ParsingExtraFrame` 워드+0 의 **비트 3**
- 출고대 신호 : `ParsingExtraFrame` 워드+2 의 **비트 3**
- `IsStartNDestStation` **TRUE** — 출발하려면 트랙에 작번이 있어야 한다.
- 그룹 204 의 도착 인정 자리다.

#### 작업대 203 — WCS트랙 303 / 원본트랙 3003

- 성격 : **유형 C 입출고 겸용**
- 입고대 신호 : `ParsingExtraFrame` 워드+0 의 **비트 1**
- 출고대 신호 : `ParsingExtraFrame` 워드+2 의 **비트 1**
- `IsStartNDestStation` **TRUE** — 출발하려면 트랙에 작번이 있어야 한다.
- 그룹 204 의 도착 인정 자리다.

#### 작업대 205 — WCS트랙 312 / 원본트랙 3012

- 성격 : **유형 C 입출고 겸용**
- 입고대 신호 : `ParsingExtraFrame` 워드+0 의 **비트 6**
- 출고대 신호 : `ParsingExtraFrame` 워드+2 의 **비트 6**
- `IsStartNDestStation` **TRUE** — 출발하려면 트랙에 작번이 있어야 한다.
- `IsSizeChecker` **TRUE** — 사이즈 체커로 표시된다.

#### 작업대 206 — WCS트랙 304 / 원본트랙 3004

- 성격 : **유형 C 입출고 겸용**
- 입고대 신호 : `ParsingExtraFrame` 워드+0 의 **비트 2**
- 출고대 신호 : `ParsingExtraFrame` 워드+2 의 **비트 2**
- `IsStartNDestStation` **TRUE** — 출발하려면 트랙에 작번이 있어야 한다.
- `IsSizeChecker` **TRUE** — 사이즈 체커로 표시된다.

#### 작업대 207 — WCS트랙 301 / 원본트랙 3001

- 성격 : **유형 D 입출고 겸용 + DATA 삭제**
- 입고대 신호 : `ParsingExtraFrame` 워드+0 의 **비트 0**
- 출고대 신호 : `ParsingExtraFrame` 워드+2 의 **비트 0**
- `IsPalletMagazine` **TRUE** — 유일. 입고대 신호가 바뀌는 순간
  `CCvTrackInfo::SetStoStationReady`(`CvTrackInfo.cpp:425`) → `SendPMStoRequest()`(`:682`) 로
  상위에 **팔레트 매거진 입고 요청**(`CMD_PM_STO_REQ`)을 올린다.
  조건 `IsPalletMagazine && m_bStoStationReady && m_nLuggNum == 0`.
  화면 상태도 화물감지센서 대신 `m_bStoStationReady` 를 쓴다(`CvTrackInfo.cpp:161`).
- `IsDeleteDataStation` **TRUE** — 도착하면 `WriteTrackInfo(트랙, 0, …)` 로 작번을 지운다.
- `IsStartNDestStation` **FALSE** — 출발하려면 트랙이 비어 있어야 한다.

#### 작업대 208 — WCS트랙 357 / 원본트랙 3057

- 성격 : **유형 C 입출고 겸용**
- 입고대 신호 : `ParsingExtraFrame` 워드+0 의 **비트 7**
- 출고대 신호 : `ParsingExtraFrame` 워드+2 의 **비트 7**
- `IsStartNDestStation` **TRUE** — 출발하려면 트랙에 작번이 있어야 한다.
- 그룹 204 의 도착 인정 자리다.

#### 작업대 209 — WCS트랙 358 / 원본트랙 3058

- 성격 : **유형 C 입출고 겸용**
- 입고대 신호 : `ParsingExtraFrame` 워드+0 의 **비트 8**
- 출고대 신호 : `ParsingExtraFrame` 워드+2 의 **비트 8**
- `IsStartNDestStation` **TRUE** — 출발하려면 트랙에 작번이 있어야 한다.
- `IsSizeChecker` **TRUE** — 사이즈 체커로 표시된다.

### PLC 04

#### 작업대 211 — WCS트랙 407 / 원본트랙 4007

- 성격 : **유형 A 순수 입고대**
- 입고대 신호 : `ParsingExtraFrame` 워드+0 의 **비트 0**
- `IsStartNDestStation` **FALSE** — 출발하려면 트랙이 비어 있어야 한다.

#### 작업대 221 — WCS트랙 419 / 원본트랙 4019

- 성격 : **유형 C 입출고 겸용**
- 입고대 신호 : `ParsingExtraFrame` 워드+0 의 **비트 2**
- 출고대 신호 : `ParsingExtraFrame` 워드+2 의 **비트 1**
- `IsStartNDestStation` **TRUE** — 출발하려면 트랙에 작번이 있어야 한다.
- **그룹 목적지** — 목적지가 221 로 오면 221, 222 중 어디에 도착해도 완료로 본다.
- 그룹 221, 222 의 도착 인정 자리다.

#### 작업대 222 — WCS트랙 420 / 원본트랙 4020

- 성격 : **유형 C 입출고 겸용**
- 입고대 신호 : `ParsingExtraFrame` 워드+0 의 **비트 3**
- 출고대 신호 : `ParsingExtraFrame` 워드+2 의 **비트 2**
- `IsStartNDestStation` **TRUE** — 출발하려면 트랙에 작번이 있어야 한다.
- **그룹 목적지** — 목적지가 222 로 오면 221, 222 중 어디에 도착해도 완료로 본다.
- 그룹 221, 222 의 도착 인정 자리다.

#### 작업대 231 — WCS트랙 428 / 원본트랙 4028

- 성격 : **유형 F 231 전용**
- 입고대 신호 : `ParsingExtraFrame` 워드+0 의 **비트 4**
- 출고대 신호 : `ParsingExtraFrame` 워드+2 의 **비트 3**
- `StartInvokeCheck` 안에 이 자리만의 예외가 있어, 출발 지시와 동시에 `JOB->Arrived()` 를 부른다 (`Cv.cpp:456`).

#### 작업대 241 — WCS트랙 434 / 원본트랙 4034

- 성격 : **유형 B 순수 출고대**
- 출고대 신호 : `ParsingExtraFrame` 워드+2 의 **비트 5**
- **그룹 목적지** — 목적지가 241 로 오면 4032, 4033, 4034 중 어디에 도착해도 완료로 본다.

#### 작업대 242 — WCS트랙 431 / 원본트랙 4031

- 성격 : **유형 B 순수 출고대**
- 출고대 신호 : `ParsingExtraFrame` 워드+2 의 **비트 4**
- **그룹 목적지** — 목적지가 242 로 오면 4029, 4030, 4031 중 어디에 도착해도 완료로 본다.

#### 작업대 251 — WCS트랙 415 / 원본트랙 4015

- 성격 : **유형 D 입출고 겸용 + DATA 삭제**
- 입고대 신호 : `ParsingExtraFrame` 워드+0 의 **비트 1**
- 출고대 신호 : `ParsingExtraFrame` 워드+2 의 **비트 0**
- `IsDeleteDataStation` **TRUE** — 도착하면 `WriteTrackInfo(트랙, 0, …)` 로 작번을 지운다.
- `IsStartNDestStation` **FALSE** — 출발하려면 트랙이 비어 있어야 한다.

### PLC 05

#### 작업대 157 — WCS트랙 506 / 원본트랙 5006

- 성격 : **유형 E 목록 없음 (RGV/대기)**
- 입고대/출고대 목록에 없어 `StartInvokeCheck` / `ArrivedCheck` 가 돌지 않는다.
- `GetRgvLoadingTrack` / `GetWaitTrackPerPlc` 목록에 있고 `NewStartRoutinePlc2/5` 가 처리한다.

#### 작업대 158 — WCS트랙 512 / 원본트랙 5012

- 성격 : **유형 E 목록 없음 (RGV/대기)**
- 입고대/출고대 목록에 없어 `StartInvokeCheck` / `ArrivedCheck` 가 돌지 않는다.
- `GetRgvLoadingTrack` / `GetWaitTrackPerPlc` 목록에 있고 `NewStartRoutinePlc2/5` 가 처리한다.

#### 작업대 159 — WCS트랙 518 / 원본트랙 5018

- 성격 : **유형 E 목록 없음 (RGV/대기)**
- 입고대/출고대 목록에 없어 `StartInvokeCheck` / `ArrivedCheck` 가 돌지 않는다.
- `GetRgvLoadingTrack` / `GetWaitTrackPerPlc` 목록에 있고 `NewStartRoutinePlc2/5` 가 처리한다.

#### 작업대 160 — WCS트랙 524 / 원본트랙 5024

- 성격 : **유형 E 목록 없음 (RGV/대기)**
- 입고대/출고대 목록에 없어 `StartInvokeCheck` / `ArrivedCheck` 가 돌지 않는다.
- `GetRgvLoadingTrack` / `GetWaitTrackPerPlc` 목록에 있고 `NewStartRoutinePlc2/5` 가 처리한다.

### PLC 06

#### 작업대 212 — WCS트랙 651 / 원본트랙 6051

- 성격 : **유형 C 입출고 겸용**
- 입고대 신호 : `ParsingExtraFrame` 워드+0 의 **비트 1**
- 출고대 신호 : `ParsingExtraFrame` 워드+2 의 **비트 1**
- `IsStartNDestStation` **TRUE** — 출발하려면 트랙에 작번이 있어야 한다.
- 그룹 204 의 도착 인정 자리다.

#### 작업대 213 — WCS트랙 646 / 원본트랙 6046

- 성격 : **유형 C 입출고 겸용**
- 입고대 신호 : `ParsingExtraFrame` 워드+0 의 **비트 0**
- 출고대 신호 : `ParsingExtraFrame` 워드+2 의 **비트 0**
- `IsStartNDestStation` **TRUE** — 출발하려면 트랙에 작번이 있어야 한다.
- 그룹 204 의 도착 인정 자리다.

#### 작업대 214 — WCS트랙 652 / 원본트랙 6052

- 성격 : **유형 C 입출고 겸용**
- 입고대 신호 : `ParsingExtraFrame` 워드+0 의 **비트 3**
- 출고대 신호 : `ParsingExtraFrame` 워드+2 의 **비트 3**
- `IsStartNDestStation` **TRUE** — 출발하려면 트랙에 작번이 있어야 한다.
- `IsSizeChecker` **TRUE** — 사이즈 체커로 표시된다.

#### 작업대 215 — WCS트랙 647 / 원본트랙 6047

- 성격 : **유형 C 입출고 겸용**
- 입고대 신호 : `ParsingExtraFrame` 워드+0 의 **비트 2**
- 출고대 신호 : `ParsingExtraFrame` 워드+2 의 **비트 2**
- `IsStartNDestStation` **TRUE** — 출발하려면 트랙에 작번이 있어야 한다.
- `IsSizeChecker` **TRUE** — 사이즈 체커로 표시된다.

## 4. 참고 — 확인에 쓴 자리

| 무엇 | 파일:줄 |
| --- | --- |
| 주기 호출 순서 | `EcsSv/CvThreadProc.cpp:44~71` |
| 신호 파싱 | `EcsSv/Cv.cpp:132` `ParsingExtraFrame` |
| 출발 지시 | `EcsSv/Cv.cpp:389` `StartInvokeCheck` |
| 도착 처리 | `EcsSv/Cv.cpp:1054` `ArrivedCheck` |
| DATA 삭제(작업 없음) | `EcsSv/Cv.cpp:1075` |
| DATA 삭제(도착 후) | `EcsSv/Cv.cpp:1200` |
| 결정대 재출발 | `EcsSv/Cv.cpp:2248` `ReStartRoutine2` |
| 입고대 목록 | `EcsSv/Lib.cpp:133` `GetLinearStoStnNumPerCvPlc` |
| 출고대 목록 | `EcsSv/Lib.cpp:191` `GetLinearRetStnNumPerCvPlc` |
| 출발지겸도착지 | `EcsSv/Lib.cpp:2469` `IsStartNDestStation` |
| 팔레트 매거진 | `EcsSv/Lib.cpp:2500` `IsPalletMagazine` |
| 그룹 도착 판정 | `EcsSv/Lib.cpp:3391` `IsArrivedToRetStation` |
| DATA 삭제 대상 | `EcsSv/Lib.cpp:3455` `IsDeleteDataStation` |
| 사이즈 체커 | `EcsSv/Lib.cpp:3466` `IsSizeChecker` |
| 매거진 입고 요청 | `EcsSv/CvTrackInfo.cpp:225 / 425 / 682` |
| 상수 | `Common/Include/Ecs/EcsDef.h` |

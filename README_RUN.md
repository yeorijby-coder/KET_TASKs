# 전체 구동 방법 (TASK + 시뮬레이터)

2026-08-08 기준. TASK 10종과 ClientNSim 시뮬레이터 4종을 한 PC 에서 함께 돌린 구성이다.

## 구성 요소와 포트

```
[ClientNSim]  https://github.com/yeorijby-coder/ClientNSim.git
  CvSim    CV(PLC) 시뮬레이터  대기 9101~9118 / 9201~9218 / 9301~9318 / 9401~ / 9501~
  ScSim    SC 시뮬레이터        대기 8101, 8111
  HostSim  WMS 시뮬레이터       대기 8400 (ECS 가 붙는 곳), 8401 로 접속해 옴
  Ecs      WCS Client(관제 UI)  DB(ODBC PostgreSQL Unicode) 로 접속

[KET_TASKs]  https://github.com/yeorijby-coder/KET_TASKs.git
  WCS_TASK_CV_1F    -> 127.0.0.1:9101~9108 (CvSim CV_E01)
  WCS_TASK_CV_3F    -> 127.0.0.1:9201      (CvSim CV_E02)
  WCS_TASK_CV_BOX   -> 127.0.0.1:9301      (CvSim CV_E03)
  WCS_TASK_SC_SINGLE-> 127.0.0.1:8101      (ScSim Port1)
  WCS_TASK_SC_TWIN  -> 127.0.0.1:8111      (ScSim Port2)
  WCS_TASK_HOST     -> 127.0.0.1:8400 접속, 8401 대기 (HostSim 과 쌍방향)
  WCS_TASK_Display  -> DB 만 (전광판 실물 없음. DISPLAY_CTRL 로 제어)
  WCS_IO_SCH_1F/3F/BOX -> DB 만
```

DB 는 로컬 PostgreSQL 16, `KET_WCS@KET_WCS/localhost:5432`.

## 기동 순서

1. **시뮬레이터 먼저** (각자 자기 폴더를 작업 디렉터리로!)
   ```
   ClientNSim\Bin\Debug\CvSim.exe    (작업폴더 ClientNSim\CvSim)
   ClientNSim\Bin\Debug\ScSim.exe    (작업폴더 ClientNSim\ScSim)
   ClientNSim\Bin\Debug\HostSim.exe  (작업폴더 ClientNSim\HostSim)
   ClientNSim\Bin\Debug\Ecs.exe      (작업폴더 ClientNSim\Ecs)
   ```
   작업 디렉터리가 다르면 Ecs.ini / EcsDefine.xml / EcsLayout*.xml 을 못 찾는다.

2. **TASK** (각 프로젝트의 bin\Debug 에서)
   ```
   WCS_TASK_CV_1F.exe / WCS_TASK_CV_3F.exe / WCS_TASK_CV_BOX.exe
   WCS_TASK_SC_SINGLE.exe / WCS_TASK_SC_TWIN.exe
   WCS_TASK_Display.exe / TASK_LFC10_G1_ECSCOM.exe(HOST)
   IO_TASK_SEMI_FINISH.exe (IO_SCH 1F/3F/BOX 각 폴더의 것)
   ```
   ※ CV 1F/3F/BOX 의 bin\Debug 에 남아 있는 옛 `WCS_TASK_CV.exe` 는 쓰지 말 것.
     지금 산출물은 층별 이름(`WCS_TASK_CV_1F.exe` 등)이다.

## 빌드

- TASK (C#) : VS2022 MSBuild 로 그대로.
- ClientNSim (C++/MFC) : 프로젝트가 v145(VS2026) 로 저장돼 있어
  이 PC(v143)에서는 툴셋을 넘겨서 빌드한다.
  ```
  msbuild CvSim.sln   /p:Configuration=Debug /p:Platform=Win32 /p:PlatformToolset=v143 /m
  msbuild ScSim.sln   /p:Configuration=Debug /p:Platform=Win32 /p:PlatformToolset=v143 /m
  msbuild Ecs.sln     /p:Configuration=Debug /p:Platform=Win32 /p:PlatformToolset=v143 /m
  msbuild HostSim.sln /p:Configuration=Debug /p:Platform=Win32 /p:PlatformToolset=v143 /m
  ```

## 시험 뒤 데이터 되돌리기

HOST↔HostSim 이 돌면 완료보고가 JOB_MST 시험 작업(508건)을 이력으로 옮긴다.
```
psql -h 127.0.0.1 -U KET_WCS -d KET_WCS -f WCS_TASK_HOST/DB/04_JOB_MST_RESTORE.sql
```

## 확인된 것 / 남은 것 (2026-08-08)

확인된 것
- 14개 프로그램 동시 기동, 전 구간 TCP 접속 성립
- HOST↔HostSim : 전문 왕복 (S/E/F/N/O/R, [09]Interface목록서 규격)
- SC_SINGLE/TWIN↔ScSim : 접속 유지
- **CV 종단간 데이터 흐름** : CvSim 메모리(D레지스터)에 값을 쓰면
  CV TASK 의 Q3E 폴링이 읽어 CV_DATA 까지 반영된다.
  (트랙 101 에 LuggNum=1234 를 넣어 lugg_no_rd/job_typ_rd/read_upd_dt 갱신 확인)
  ※ CV_DATA 의 read_upd_dt 는 "값이 바뀐 트랙만" 갱신된다. 시뮬레이터
    메모리가 전부 0 이면 갱신이 없는 것이 정상이다.

남은 것
- CV_1F 의 WCS_DB.INI 는 COMM0~7 여덟 스레드가 전부 같은 트랙(101~188)을
  폴링하게 되어 있다(시험 흔적). 일부 스레드가 "트랙 데이터 읽기 에러" 를
  간헐적으로 남기는 원인. 실제 PLC 8대 구성에 맞게 COMM 별 트랙 범위를
  나누거나 PROCESS CNT 를 줄이면 된다.
- CvSim 은 시작주소가 10000 을 넘는 요청에 응답하지 않고 연결을 끊는다
  (Cv.cpp CheckRequest 의 주소 가드). 트랙 수를 늘릴 때 주의.
- ScSim 은 EcsDefine.xml 이 SC 4대(901~904)를 정의하지만 레이아웃에는
  1대분 컨트롤만 있어 화면 표시는 1대만 된다. (크래시는 나지 않게 고침)

## 종단간 확인에 쓴 방법 (재현용)

CvSim 트랙 101 에 값 넣기 (Q3E WRITE, 트랙당 5워드라 트랙 101 = 주소 5):
```python
# 주소5 = LuggNum, 주소6 = 하위바이트 DestPos / 상위바이트 JobType
q3e_write(5, [1234, (2<<8)|103])   # 9101 로 전송
```
몇 초 안에 CV_DATA (mc_no=101) 의 lugg_no_rd 가 1234 로 바뀐다.

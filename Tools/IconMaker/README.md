# IconMaker

WCS 태스크 실행파일 아이콘(`.ico`) 생성기.

각 태스크 아이콘은 **불투명 검정 정사각 배경 + 굵은 산세리프 텍스트**이고,
시스템마다 글자색을 달리해서 작업표시줄에서 색만으로 구분되게 하는 규칙을 따른다.
이 도구는 그 규칙대로 아이콘을 찍어낸다.

## 사용법

```
Tools\IconMaker\MakeIcon.cmd <출력.ico> <텍스트> [#RRGGBB]
```

- 텍스트에 `/` 를 넣으면 두 줄로 그린다. 예) `CV/1F`
- 색을 생략하면 `#FF8C1A`(주황)
- Visual Studio 없이 `csc.exe`(.NET Framework 4.0)만 있으면 동작한다

실제로 `WCS_TASK_Display\disp.ico` 를 만들 때 쓴 명령:

```
Tools\IconMaker\MakeIcon.cmd WCS_TASK_Display\disp.ico DISP #FF8C1A
```

## 현재 아이콘 색상

| 프로젝트 | 파일 | 텍스트 | 색상 |
| --- | --- | --- | --- |
| WCS_TASK_CV_1F / _3F / _BOX | `cv.ico` | `CV/1F` 등 | `#56BAEA` 하늘색 |
| WCS_TASK_CV_original | `cv.ico` | `CV` | `#56BAEA` 하늘색 |
| WCS_TASK_SC_SINGLE | `s-sc.ico` | `S-SC` | `#B5E61D` 연두 |
| WCS_TASK_SC_TWIN | `t-sc.ico` | `T-SC` | `#B5E61D` 연두 |
| WCS_TASK_SC_Original | `sc.ico` | `SC` | `#B5E61D` 연두 |
| WCS_TASK_Display | `disp.ico` | `DISP` | `#FF8C1A` 주황 |

새 태스크를 추가할 때는 위와 겹치지 않는 색을 고른다.

**합쳐 놓은 판(`*_Original`)에는 층/포크 표시를 넣지 않는다.**
1F / 3F / BOX 를 다 다루고 SINGLE / TWIN 을 다 다루므로,
아이콘에 `1F` 나 `S-` 가 붙어 있으면 어느 한쪽만 도는 것으로 오해하게 된다.

## IO_SCH 계열 아이콘 - MakeIoIcon

IO_SCH 는 예전부터 **파란 둥근 사각 + 흰 글자**라 위 규칙과 그림체가 다르다.
그 그림체 그대로 찍어내는 별도 생성기를 둔다.

```
Tools\IconMaker\MakeIoIcon.cmd <출력.ico> <윗줄> [아랫줄]
```

```
Tools\IconMaker\MakeIoIcon.cmd WCS_IO_SCH_Original\IO.ico I/O
Tools\IconMaker\MakeIoIcon.cmd WCS_IO_SCH_3F\IO.ico       I/O 3F
```

- 아랫줄을 주면 노란색(`#FFD200`)으로 그린다. 층 표시가 그것이다.
- 기존 아이콘에서 잰 값을 그대로 쓴다.
  모서리 반경 15%, 테두리 1.6%(`#78A0DC`), 바탕 `#2B57AB` → `#142F69` 세로 그라데이션
- `MakeIcon` 은 `/` 를 줄바꿈으로 쓰기 때문에 `I/O` 를 한 줄로 그릴 수 없다.
  그래서 이쪽은 윗줄/아랫줄을 인자로 따로 받는다.

| 프로젝트 | 파일 | 텍스트 |
| --- | --- | --- |
| WCS_IO_SCH_Original | `IO.ico` | `I/O` |
| WCS_IO_SCH_1F | `IO.ico` | `I/O` |
| WCS_IO_SCH_3F / _BOX | `IO.ico` | `I/O` + `3F` / `BOX` |

**1F 는 층 표시를 넣지 않는다.** 현장에서 쓰는 것이 1층이라 `I/O` 만 있으면 되고,
네 프로젝트가 모두 `IO_TASK_SEMI_FINISH.exe` 라는 같은 이름으로 나오므로
아이콘으로 구분하려 들기보다 창 제목(`WCS_IO_SCH_1F` 등)을 보는 편이 확실하다.

## 출력 형식

- 프레임 : 16 / 24 / 32 / 48 / 64 / 128 / 256, 전부 32bpp DIB
- PNG 압축 프레임을 쓰지 않는다. 구형 셸이나 `System.Drawing.Icon` 에서
  PNG 프레임을 못 읽는 경우가 있어서 의도적으로 DIB 로만 넣는다.
- 16 / 24 는 안티에일리어싱하면 네 글자가 뭉개진다. 그래서 이 크기만
  폭이 좁은 **Arial Narrow Bold** 를 픽셀 그리드에 맞춰 또렷하게 찍고,
  32 이상은 계열 표준인 **Arial Black** 을 쓴다.

## 새 아이콘을 프로젝트에 적용하는 법

`.csproj` 의 `ApplicationIcon` 만 바꾸면 **탐색기에 보이는 실행파일 아이콘**만 바뀐다.
창 제목표시줄과 작업표시줄 아이콘은 `Form.Icon` 을 따로 지정해야 바뀐다.
지정하지 않으면 WinForms 기본 아이콘(`wfc.ico`)이 그대로 나온다.

이 저장소에는 두 가지 방식이 섞여 있다.

1. **resx 방식** (CV / SC / HOST / IO_SCH)
   폼 디자이너에서 Icon 속성을 지정하면 `SYS_MAIN.resx` 에 아이콘이 들어가고
   `SYS_MAIN.Designer.cs` 에 아래 줄이 생긴다.

   ```csharp
   this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
   ```

2. **EmbeddedResource 방식** (Display)
   `.csproj` 에 `<EmbeddedResource Include="disp.ico" />` 를 넣고
   생성자에서 읽어 지정한다. `SYS_MAIN.PsSetFormIcon()` 참조.
   아이콘 파일이 한 곳뿐이라 `ApplicationIcon` 과 어긋날 일이 없다.

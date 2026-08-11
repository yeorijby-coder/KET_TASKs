# DocMaker

`Docs\*.md` 를 **워드 문서(.docx)** 로 뽑는 도구.

이 PC 에는 pandoc 도 python-docx 도 없다. 대신 **Word 16.0 이 COM 으로 열려 있어서**,
마크다운을 HTML 로 바꾼 뒤 Word 를 시켜 `.docx` 로 저장한다.

## 사용법

```
Tools\DocMaker\MakeDoc.cmd <입력.md> [출력.docx]
```

출력을 생략하면 입력과 같은 자리에 같은 이름으로 `.docx` 를 만든다.

```
Tools\DocMaker\MakeDoc.cmd Docs\원본ECS_스테이션_동작정리.md
Tools\DocMaker\MakeDoc.cmd Docs\원본ECS_3층SC_동작정리.md
```

필요한 것은 **PATH 에 잡힌 Python 3** 과 **설치된 Word** 뿐이다.

## 구성

| 파일 | 하는 일 |
| --- | --- |
| `md2html.py` | 마크다운 → HTML. 제목 / 표 / 코드블록 / 목록 / `**굵게**` / `` `코드` `` 만 다룬다 |
| `html2docx.ps1` | Word COM 으로 HTML 을 열어 `.docx`(`wdFormatXMLDocument`=16) 로 저장 |
| `MakeDoc.cmd` | 위 둘을 이어 부르고 중간 HTML 을 지운다 |

`html2docx.ps1` 이 저장 전에 손보는 것

- 여백 45pt (기본 여백이면 표가 잘린다)
- 모든 표를 페이지 폭에 맞추고(`AutoFitBehavior(2)`),
  첫 줄을 제목행으로 반복시키고, 행이 페이지 경계에서 쪼개지지 않게 한다
- 바닥글 가운데에 쪽번호

## 손대기 전에 알아 둘 것

- **`html2docx.ps1` 은 ASCII 로만 쓴다.** PowerShell 5.1 은 BOM 없는 `.ps1` 을
  ANSI 로 읽어서, 한글 주석을 넣으면 깨진다.
- **`MakeDoc.cmd` 는 CP949 + CRLF** 다. `Tools\IconMaker\MakeIcon.cmd` 와 같은 규칙이다.
  UTF-8 로 저장하면 cmd.exe 가 망가뜨린다. 그래서 이 파일도 메시지를 영문으로만 적었다.
- `md2html.py` 는 **UTF-8** 이다. 표 칸 안에서 줄을 바꾸고 싶으면 마크다운에 `<br>` 을 쓴다.
- `**`굵게`**` 처럼 굵게가 백틱을 걸쳐 있어도 잡히도록,
  백틱 안을 먼저 자리표시자로 빼 두고 나중에 되돌린다.

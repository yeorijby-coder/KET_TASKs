@echo off
rem ---------------------------------------------------------------------
rem  MakeIoIcon.cs 를 컴파일해서 실행한다. (Visual Studio 없이 csc.exe 만 있으면 된다)
rem
rem    MakeIoIcon.cmd <출력.ico> <윗줄> [아랫줄]
rem
rem  예)  MakeIoIcon.cmd ..\..\WCS_IO_SCH_Original\IO.ico I/O
rem       MakeIoIcon.cmd ..\..\WCS_IO_SCH_1F\IO.ico       I/O 1F
rem ---------------------------------------------------------------------
setlocal

set CSC=%WINDIR%\Microsoft.NET\Framework\v4.0.30319\csc.exe
if not exist "%CSC%" (
    echo [ERROR] csc.exe 를 찾을 수 없습니다: %CSC%
    exit /b 1
)

if "%~2"=="" (
    echo usage: MakeIoIcon.cmd ^<out.ico^> ^<line1^> [line2]
    echo        예^) MakeIoIcon.cmd IO.ico I/O
    echo            MakeIoIcon.cmd IO.ico I/O 1F
    exit /b 1
)

set OUTEXE=%TEMP%\MakeIoIcon.exe

"%CSC%" /nologo /target:exe /platform:anycpu /out:"%OUTEXE%" /r:System.Drawing.dll "%~dp0MakeIoIcon.cs"
if errorlevel 1 (
    echo [ERROR] 컴파일 실패
    exit /b 1
)

"%OUTEXE%" %*
exit /b %errorlevel%

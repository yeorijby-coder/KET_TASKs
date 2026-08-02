@echo off
rem ---------------------------------------------------------------------
rem  MakeIcon.cs 를 컴파일해서 실행한다. (Visual Studio 없이 csc.exe 만 있으면 된다)
rem
rem    MakeIcon.cmd <출력.ico> <텍스트> [#RRGGBB]
rem
rem  예)  MakeIcon.cmd ..\..\WCS_TASK_Display\disp.ico DISP #FF8C1A
rem       MakeIcon.cmd out\cv.ico CV/1F #56BAEA
rem ---------------------------------------------------------------------
setlocal

set CSC=%WINDIR%\Microsoft.NET\Framework\v4.0.30319\csc.exe
if not exist "%CSC%" (
    echo [ERROR] csc.exe 를 찾을 수 없습니다: %CSC%
    exit /b 1
)

if "%~2"=="" (
    echo usage: MakeIcon.cmd ^<out.ico^> ^<text^> [#RRGGBB]
    echo        text 에 '/' 를 넣으면 두 줄로 그립니다.  예^) CV/1F
    exit /b 1
)

set OUTEXE=%TEMP%\MakeIcon.exe

"%CSC%" /nologo /target:exe /platform:anycpu /out:"%OUTEXE%" /r:System.Drawing.dll "%~dp0MakeIcon.cs"
if errorlevel 1 (
    echo [ERROR] 컴파일 실패
    exit /b 1
)

"%OUTEXE%" %*
exit /b %errorlevel%

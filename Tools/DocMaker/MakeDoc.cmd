@echo off
rem ---------------------------------------------------------------------
rem  Markdown -> Word (.docx)
rem
rem     MakeDoc.cmd <in.md> [out.docx]
rem
rem  Needs Python 3 on PATH and Microsoft Word installed.
rem  Keep this file ASCII-only - cmd.exe mangles UTF-8 here.
rem ---------------------------------------------------------------------
setlocal

if "%~1"=="" (
    echo usage: MakeDoc.cmd ^<in.md^> [out.docx]
    exit /b 1
)

set "SRC=%~f1"
set "DST=%~2"
if "%DST%"=="" set "DST=%~dpn1.docx"
set "TMPHTML=%TEMP%\%~n1.html"

python "%~dp0md2html.py" "%SRC%" "%TMPHTML%" "%~n1"
if errorlevel 1 exit /b 1

powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0html2docx.ps1" "%TMPHTML%" "%DST%"
if errorlevel 1 exit /b 1

del "%TMPHTML%" >nul 2>&1
exit /b 0

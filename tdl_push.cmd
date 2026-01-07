@echo off
setlocal
REM Runs tdl_push.ps1 from the same folder. Optional: pass commit message after the command.

powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0tdl_push.ps1" -Message "%*"
endlocal

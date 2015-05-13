@echo off
for /F "tokens=1,2,3" %%a in ('reg query "HKEY_LOCAL_MACHINE\Software\ListenerService" /v servicePath') do if "%%a"=="servicePath" set reg_value=%%c
echo %reg_value%
pause
exit
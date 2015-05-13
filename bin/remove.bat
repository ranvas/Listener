@echo off
::определить папку installUtil
set installName="%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\InstallUtil.exe"
if exist %installName% (
goto removing
) else (
goto exit
)

:removing
for /F "tokens=1,2,3" %%a in ('reg query "HKEY_LOCAL_MACHINE\Software\ListenerService" /v servicePath') do if "%%a"=="servicePath" set serviceName=%%c
%installName% /u "%serviceName%\ListenerService.exe"
goto complete
:exit
echo roll back
pause
Exit
:complete
echo successful
pause
Exit
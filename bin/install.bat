@echo off
::-------------------------------------------------------------------------------------------
::---��������� �� ����� ��������������-------------------------------------------------------
::---������� � ����������� ������ �� sddrogue@gmail.com--------------------------------------
::---������ ���������� �� ���� ������������������ ����� "�������� � ������ �����-�������"---
::-------------------------------------------------------------------------------------------
::---
::---
::����� ������ � ����� �����������
set serviceFolder="%~dp0"
if exist %serviceFolder:~1,-1%ListenerService.exe (
goto installing
) else (
::����� ������ � ����� �������
set serviceFolder="%~dp0..\ListenerService\bin\Debug\"
)
if exist %serviceFolder:~1,-1%ListenerService.exe (
goto installing
) else (
echo place ListenerService.exe into folder with install.bat
goto exit
)
:installing
echo installing service
::���������� ����� installUtil
set installName="%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\InstallUtil.exe"
if exist %installName% (
goto registering
) else (
echo first install .NET framework version 4 or higher
goto exit
)
:registering
echo registering
if "%~1" == "" (
set appdataName="%APPDATA%\ListenerService\"
) else (
set appdataName="%~1"
)
md %appdataName%
cd %appdataName%
goto exist%errorlevel%
:exist0
goto doing
:exist1
echo incorrect install directory
goto exit
:doing
::����������� ������ � appdata
copy %serviceFolder:~1,-1%ListenerService.exe %appdataName% /Y
copy %serviceFolder:~1,-1%ListenerService.exe.config %appdataName% /Y
copy %serviceFolder:~1,-1%ListenerService.exe.manifest %appdataName% /Y
::���������� ������
%installName% "%appdataName:~1,-1%ListenerService.exe"
::��������� � ������ �������� appdataName
reg ADD HKEY_LOCAL_MACHINE\SOFTWARE\ListenerService /f
reg ADD HKEY_LOCAL_MACHINE\SOFTWARE\ListenerService /f /v servicePath /d %appdataName:~,-1%
goto :complete
:exit
echo roll back
Pause
Exit
:complete
echo successful
Pause
Exit

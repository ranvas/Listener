@echo off
rem ----��������� ����� � dll � ����������
set dllName="%~dp0%API.dll"
if exist %dllName% (
echo %dllName% exist
) else (
echo %dllName% not exist
goto exit
)
rem ----������ ���� � � ������� � �������� ���� gacutils
set regkey = ""
For /F "UseBackQ Tokens=2*" %%a In (`Reg Query "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\MSBuild\ToolsVersions\4.0"^|Find /i "SDK40ToolsPath"`) Do (
set regkey=%%b
)
echo %regkey%
set regkey=%regkey:~11,-20%
set folderkey = ""
For /F "UseBackQ Tokens=2*" %%I In (`Reg Query "%regkey%"^|Find /i "InstallationFolder"`) Do (
set folderkey=%%J
echo %%J
)


cd %folderkey%
gacutil /i %dllname%

:exit
pause


:concat
set registrekey=%registrekey% %%J
goto :eof
rem запуск от имени администратора
if not "%1"=="am_admin" (
    powershell -Command "Start-Process -Verb RunAs -FilePath '%0' -ArgumentList 'am_admin'"
    exit /b
)
rem регистрация сервиса
sc create Korolko-Kaspersky-test-filescanner-dev binPath= %~dp0\service\Service-kasp.exe
sc config Korolko-Kaspersky-test-filescanner-dev obj= "NT AUTHORITY\LocalService" password= ""

rem разрешение текущему пользователю запускать и останавливать службу
for /f %%i in ('wmic useraccount where name^="%UserName%" get sid ^| findstr ^S\-d*') do set SID=%%i

for /f %%i in (' sc.exe sdshow Korolko-Kaspersky-test-filescanner-dev') do set THING=%%i

set THING=%THING:~0,2%(A;;RPWP;;;%SID%)%THING:~2%
sc sdset Korolko-Kaspersky-test-filescanner-dev %THING%
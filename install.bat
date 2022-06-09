@echo off
if not "%1"=="am_admin" (
    powershell -Command "Start-Process -Verb RunAs -FilePath '%0' -ArgumentList 'am_admin'"
    exit /b
)
 
sc create Korolko-kaspersky-filescanner-dev binPath= %~dp0\Service-kasp\Service-kasp\bin\Debug\net6.0
sc config Korolko-kaspersky-filescanner-dev obj= "NT AUTHORITY\LocalService" password= ""
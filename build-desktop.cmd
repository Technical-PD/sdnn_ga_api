@echo off
rem Runs build-desktop.ps1 without changing the system PowerShell execution policy.
rem Usage: build-desktop.cmd -OutputDir C:\Builds\SdnnGa -HostPython C:\venvs\sdnnga\Scripts\python.exe -Zip
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0build-desktop.ps1" %*
exit /b %ERRORLEVEL%

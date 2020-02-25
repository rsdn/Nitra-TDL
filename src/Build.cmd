title %~nx0

SET Config=Debug
SET Verbosity=m
SET Root=%~dp0..\..
SET MSBUILDENABLEALLPROPERTYFUNCTIONS=1

@echo %Root%

setlocal ENABLEEXTENSIONS
set KEY_NAME="HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Microsoft\VisualStudio\SxS\VS7"
set VALUE_NAME=15.0

for /f "tokens=2,*" %%a in ('reg query %KEY_NAME% /V %VALUE_NAME%  ^|findstr /ri "REG_SZ"') DO set Value=%%b

IF defined Value (
call "%Value%Common7\Tools\VsDevCmd.bat"
set NoPause=true

msbuild %~dp0Tdl.sln /p:NemerleBinPathRoot=%Root%\nemerle\bin\%Config% /t:Restore /v:%Verbosity%
msbuild %~dp0Tdl.sln /p:NemerleBinPathRoot=%Root%\nemerle\bin\%Config% /v:%Verbosity%
)
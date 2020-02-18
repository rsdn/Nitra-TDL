SET MSBUILDENABLEALLPROPERTYFUNCTIONS=1
SET Config=Debug
SET Verbosity=m

msbuild Tdl.sln /p:NemerleBinPathRoot=c:\RSDN\nemerle\bin\%Config% /t:Restore /v:%Verbosity%
msbuild Tdl.sln /p:NemerleBinPathRoot=c:\RSDN\nemerle\bin\%Config% /v:%Verbosity%
rem pause

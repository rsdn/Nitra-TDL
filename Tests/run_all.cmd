set Tdl2Json=%~dp0\..\bin\Debug\Tdl2Json.exe

cd /D %~dp0\Samples\

for  %%f in (*) do ( 
   echo Run: %%f 
)

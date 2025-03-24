@echo off

cd /d %~dp0
set "targetdir=%cd%\Proto2Cs"
set "protogen=%cd%\Protogen\bin\protogen.exe"

cd proto

for %%i in (*.proto) do (
    %protogen% --csharp_out=..\Proto2Cs %%i +names={original}
    echo From %%i To %%~ni.cs Successfully!  
)

echo "conversation over!"
exit
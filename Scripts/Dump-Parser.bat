cd ..\
call "C:\Program Files (x86)\Microsoft Visual Studio\2022\Community\Common7\Tools\VsDevCmd.bat"
dotnet build /target:Dump-Parser -c %1-Debug --nologo
pushd bin\Dump-Parser\%1-Debug\
Dump-Parser.exe REasyRSZ
REM Dump-Parser.exe All
popd
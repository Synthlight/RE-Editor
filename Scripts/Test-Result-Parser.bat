cd ..\
call "C:\Program Files (x86)\Microsoft Visual Studio\2022\Community\Common7\Tools\VsDevCmd.bat"
dotnet build /target:Test-Result-Parser -c %1-Debug --nologo
pushd bin\Test-Result-Parser\%1-Debug\
Test-Result-Parser.exe
popd
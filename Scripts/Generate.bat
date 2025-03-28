cd ..\
call "C:\Program Files (x86)\Microsoft Visual Studio\2022\Community\Common7\Tools\VsDevCmd.bat"
dotnet build /target:Generator -c %1-Debug --nologo
pushd bin\Generator\%1-Debug\
Generator.exe useGreylist
popd
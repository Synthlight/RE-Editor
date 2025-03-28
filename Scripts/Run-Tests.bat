cd ..\
call "C:\Program Files (x86)\Microsoft Visual Studio\2022\Community\Common7\Tools\VsDevCmd.bat"
REM nuget restore
REM MSBuild /nologo /t:Build /p:Configuration=%1 RE-Editor.sln
REM vstest.console.exe bin\Tests\%1\Tests.dll
rm -rf TestResults/%1/Deploy*
dotnet build /target:Tests -c %1-Debug --nologo
dotnet test /target:Tests -c %1-Debug --nologo --logger:"console;verbosity=normal"
rm -rf TestResults/%1/Deploy*
pause
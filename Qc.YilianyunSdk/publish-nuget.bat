echo y|del .\bin\Release /s /q
dotnet build -c Release
cd ./bin/Release
dotnet nuget push *.nupkg -k %NugetToken% -s https://api.nuget.org/v3/index.json
pause
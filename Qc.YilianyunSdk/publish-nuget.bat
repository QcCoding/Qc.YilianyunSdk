cd ./bin/Release
del *.nupkg /s /q
cd ../../
dotnet build -c Release
cd ./bin/Release
dotnet nuget push *.nupkg -k %NugetToken% -s https://api.nuget.org/v3/index.json
pause
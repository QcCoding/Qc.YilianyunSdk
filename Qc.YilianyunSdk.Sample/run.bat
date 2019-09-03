taskkill /F /T /FI "WINDOWTITLE eq Qc.YilianyunSdk.Sample" /IM dotnet.exe
start "Qc.YilianyunSdk.Sample" dotnet watch run
exit
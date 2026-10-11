@echo off
cd /d "%~dp0"
dotnet publish source\PageReplacement.WinForms -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish\win-x64
if errorlevel 1 goto failed
echo Run publish\win-x64\PageReplacement.exe on Windows x64.
pause
exit /b 0
:failed
echo Publish failed. Install .NET 8 SDK and check network access for runtime packs.
pause
exit /b 1

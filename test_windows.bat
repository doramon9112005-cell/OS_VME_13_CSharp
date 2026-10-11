@echo off
cd /d "%~dp0"
dotnet run --project tests\PageReplacement.Tests -c Release -- .
if errorlevel 1 goto failed
dotnet run --project source\PageReplacement.WinForms -c Release -- --smoke
if errorlevel 1 goto failed
echo All automated checks passed. Complete docs\WINDOWS_ACCEPTANCE.md as well.
pause
exit /b 0
:failed
echo Tests failed. Inspect output and the console above.
pause
exit /b 1

@echo off
cd /d "%~dp0"
set DOTNET_TieredCompilation=0
dotnet run --project source\PageReplacement.Cli -c Release -- benchmark output
pause

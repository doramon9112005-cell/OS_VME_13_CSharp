@echo off
cd /d "%~dp0"
where dotnet >nul 2>nul
if errorlevel 1 (
  echo Please install .NET 8 SDK or open the solution in Visual Studio with .NET desktop development.
  pause
  exit /b 1
)
dotnet run --project source\PageReplacement.WinForms -c Release
if errorlevel 1 pause

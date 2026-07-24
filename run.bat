@echo off
cd /d "%~dp0"
if not exist "bin\Debug\net8.0-windows10.0.19041.0\ArchonPM.exe" (
    echo Building the project
    dotnet build ArchonPM.csproj
)
start "" "bin\Debug\net8.0-windows10.0.19041.0\ArchonPM.exe"

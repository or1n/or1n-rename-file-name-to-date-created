@echo off
REM Install script for or1n-rename-file-name-to-date-created
REM Requirements: Windows 11, Visual Studio 2022+, .NET 8.0 SDK, Windows App SDK

REM Check for .NET SDK
where dotnet >nul 2>nul
if %errorlevel% neq 0 (
    echo .NET SDK not found. Please install .NET 8.0 SDK or later.
    exit /b 1
)

REM Restore NuGet packages
dotnet restore

REM Build the project
dotnet build

echo Installation complete. Open the solution in Visual Studio to run the app.

@echo off
REM ============================================================================
REM Install and Setup Script for or1n-rename-file-name-to-date-created
REM ============================================================================
REM 
REM This script automates the setup of or1n, including:
REM   - Verifying .NET SDK installation and version
REM   - Checking for Windows 11 requirement
REM   - Cleaning previous builds (optional)
REM   - Restoring NuGet packages
REM   - Building the project
REM   - Verifying the build succeeded
REM
REM Requirements: Windows 11, .NET SDK 8.0+
REM Optional: VS Code or Visual Studio 2022
REM
REM ============================================================================

setlocal enabledelayedexpansion
color 0A
cls

echo.
echo ============================================================================
echo   or1n Rename File Name To Date Created - Installation (%date% %time%)
echo ============================================================================
echo.

REM ==========================================================================
REM 1. CHECK WINDOWS VERSION
REM ==========================================================================
echo [1/6] Checking Windows 11 requirement...
for /f "tokens=2 delims==" %%I in ('wmic os get caption /value') do set OS_NAME=%%I

if not defined OS_NAME (
    echo ERROR: Could not detect Windows version
    exit /b 1
)

echo   OS: !OS_NAME!

REM Simple check: if not Windows 11, warn the user
echo !OS_NAME! | find "Windows 11" >nul
if errorlevel 1 (
    echo WARNING: This project is optimized for Windows 11
    echo          but may work on Windows 10 Build 19041+
)
echo.

REM ==========================================================================
REM 2. CHECK FOR .NET SDK INSTALLATION
REM ==========================================================================
echo [2/6] Verifying .NET SDK installation...

where dotnet >nul 2>nul
if errorlevel 1 (
    echo.
    echo ❌ ERROR: .NET SDK not found!
    echo.
    echo To fix:
    echo   1. Install .NET 8.0 SDK from: https://dotnet.microsoft.com/download
    echo   2. Restart your terminal/PowerShell
    echo   3. Run this script again
    echo.
    pause
    exit /b 1
)

echo   ✓ .NET SDK found
echo.

REM ==========================================================================
REM 3. CHECK .NET VERSION
REM ==========================================================================
echo [3/6] Checking .NET SDK version...

for /f "tokens=*" %%I in ('dotnet --version') do set DOTNET_VERSION=%%I
echo   Installed: %DOTNET_VERSION%
echo   Required:  8.0 or later

REM Parse version (major.minor.patch)
for /f "tokens=1,2 delims=." %%A in ("%DOTNET_VERSION%") do (
    set MAJOR=%%A
    set MINOR=%%B
)

if %MAJOR% EQU 8 (
    if %MINOR% GEQ 0 (
        echo   ✓ Version check passed
        goto version_ok
    )
)

echo.
echo ⚠️  WARNING: .NET 8.0 or later is recommended
echo   (You have %DOTNET_VERSION%, which may cause issues)
echo.

:version_ok
echo.

REM ==========================================================================
REM 4. OPTIONAL DEEP CLEAN
REM ==========================================================================
echo [4/6] Cleaning previous builds (optional)...

if exist bin (
    echo   Removing bin\ directory...
    rmdir /s /q bin >nul 2>&1
)

if exist obj (
    echo   Removing obj\ directory...
    rmdir /s /q obj >nul 2>&1
)

echo   ✓ Cleanup complete
echo.

REM ==========================================================================
REM 5. RESTORE & BUILD
REM ==========================================================================
echo [5/6] Restoring NuGet packages and building project...
echo.

dotnet restore
if errorlevel 1 (
    echo.
    echo ❌ ERROR: dotnet restore failed
    echo.
    echo            Try these troubleshooting steps:
    echo   1. Ensure you have internet connection
    echo   2. Check that NuGet servers are accessible
    echo   3. Run: dotnet nuget locals all --clear
    echo   4. Run this script again
    echo.
    pause
    exit /b 1
)

dotnet build
if errorlevel 1 (
    echo.
    echo ❌ ERROR: Build failed
    echo.
    echo   Try these troubleshooting steps:
    echo   1. Run: .\clean_winui.ps1
    echo   2. Run: dotnet clean
    echo   3. Run: dotnet restore
    echo   4. Run: dotnet build
    echo   5. If still failing, check error messages above
    echo.
    pause
    exit /b 1
)

echo   ✓ Build succeeded
echo.

REM ==========================================================================
REM 6. VERIFICATION
REM ==========================================================================
echo [6/6] Verifying build artifacts...

if exist "bin\Debug\net8.0-windows10.0.19041.0\or1n-rename-file-name-to-date-created.exe" (
    echo   ✓ Executable found
) else (
    echo   ⚠️  WARNING: Executable not found at expected location
)

echo.
echo ============================================================================
echo   ✅ INSTALLATION COMPLETE!
echo ============================================================================
echo.
echo Next steps:
echo.
echo Option 1 - VS Code (Recommended):
echo   1. Open this folder in VS Code
echo   2. Install recommended extensions (C# Dev Kit, XAML Support)
echo   3. Press F5 to run with debugger
echo.
echo Option 2 - Visual Studio 2022:
echo   1. Open: or1n-rename-file-name-to-date-created.slnx
echo   2. Wait for packages to restore
echo   3. Press F5 to run
echo.
echo Option 3 - Command Line:
echo   dotnet run --project or1n-rename-file-name-to-date-created.csproj
echo.
echo Option 4 - Watch Mode (Rebuilds on file changes):
echo   dotnet watch run
echo.
echo For more information:
echo   - Setup details:   SETUP.md
echo   - Development:     WORKFLOW.md
echo   - Project layout:  FILES_AND_FOLDERS.md
echo   - Features/TODO:   TODO.md
echo   - Overview:        README.md
echo.
echo ============================================================================
echo.
pause

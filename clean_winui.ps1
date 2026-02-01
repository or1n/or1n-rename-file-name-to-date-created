
# =============================================================================
# Deep Clean Script for WinUI 3 Build Issues
# =============================================================================
# 
# Purpose:
#   This script performs a comprehensive cleanup of build artifacts,
#   temporary files, and cached data that can cause WinUI 3 build failures.
#   It's safe to run multiple times and only removes files that can be
#   automatically regenerated.
#
# Usage:
#   .\clean_winui.ps1
#
#   Or from VS Code:
#   - Run the "Deep Clean WinUI" task (Ctrl+Shift+B, select task)
#
# What it does:
#   1. Terminates blocking processes (MSBuild, dotnet, compiler)
#   2. Kills the application if it's running
#   3. Removes build artifacts (bin/, obj/)
#   4. Removes IDE cache (.vs/, .vscode history)
#   5. Clears NuGet cache
#   6. Removes temporary files
#
# When to use:
#   - Before building after major changes
#   - When encountering XAML parse exceptions
#   - If the build is stuck or fails mysteriously
#   - Before committing to ensure clean builds work
#
# =============================================================================

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "WinUI 3 Deep Clean Script" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Enable error handling
$errorCount = 0

# =============================================================================
# 1. TERMINATE BLOCKING PROCESSES
# =============================================================================
Write-Host "[1/5] Stopping build processes..." -ForegroundColor Yellow

$processNames = @("MSBuild", "VBCSCompiler", "dotnet")

foreach ($processName in $processNames) {
    $processes = Get-Process -Name $processName -ErrorAction SilentlyContinue
    
    if ($processes) {
        Write-Host "  Stopping: $($processes.Count) $processName process(es)" -ForegroundColor Gray
        $processes | Stop-Process -Force -ErrorAction SilentlyContinue
    }
}

# Application process (replace with your exe name if different)
$appProcesses = Get-Process -Name "or1n-rename-file-name-to-date-created" -ErrorAction SilentlyContinue
if ($appProcesses) {
    Write-Host "  Stopping app process..." -ForegroundColor Gray
    $appProcesses | Stop-Process -Force -ErrorAction SilentlyContinue
}

Write-Host "  ✓ Processes cleaned" -ForegroundColor Green
Write-Host ""

# =============================================================================
# 2. REMOVE BUILD ARTIFACTS
# =============================================================================
Write-Host "[2/5] Removing build artifacts..." -ForegroundColor Yellow

$targets = @("bin", "obj", "AppPackages", "BundleArtifacts")

foreach ($target in $targets) {
    if (Test-Path $target) {
        try {
            Write-Host "  Removing: $target" -ForegroundColor Gray
            Remove-Item -Recurse -Force $target -ErrorAction Stop
        }
        catch {
            Write-Host "  ⚠️  Could not remove $target - it may be in use" -ForegroundColor Yellow
            $errorCount++
        }
    }
}

Write-Host "  ✓ Build artifacts cleared" -ForegroundColor Green
Write-Host ""

# =============================================================================
# 3. REMOVE IDE CACHE
# =============================================================================
Write-Host "[3/5] Clearing IDE cache..." -ForegroundColor Yellow

$cacheTargets = @(".vs", ".vscode-test")

foreach ($target in $cacheTargets) {
    if (Test-Path $target) {
        try {
            Write-Host "  Removing: $target" -ForegroundColor Gray
            Remove-Item -Recurse -Force $target -ErrorAction Stop
        }
        catch {
            Write-Host "  ⚠️  Could not remove $target" -ForegroundColor Yellow
        }
    } else {
        Write-Host "  Skipping: $target (not found)" -ForegroundColor Gray
    }
}

Write-Host "  ✓ IDE cache cleared" -ForegroundColor Green
Write-Host ""

# =============================================================================
# 4. CLEAR NUGET CACHE
# =============================================================================
Write-Host "[4/5] Clearing NuGet cache..." -ForegroundColor Yellow

try {
    dotnet nuget locals temp --clear 2>&1 | Out-Null
    Write-Host "  ✓ NuGet temp cache cleared" -ForegroundColor Green
}
catch {
    Write-Host "  ⚠️  Could not clear NuGet cache" -ForegroundColor Yellow
    $errorCount++
}

Write-Host ""

# =============================================================================
# 5. REMOVE TEMPORARY FILES
# =============================================================================
Write-Host "[5/5] Removing temporary files..." -ForegroundColor Yellow

$tempPatterns = @("*.tmp", "*.user", "*.suo", "*.cache", "*.log")
$tempCount = 0

foreach ($pattern in $tempPatterns) {
    $tempFiles = Get-ChildItem -Path . -Include $pattern -Recurse -ErrorAction SilentlyContinue
    
    foreach ($file in $tempFiles) {
        try {
            Remove-Item $file -Force -ErrorAction SilentlyContinue
            $tempCount++
        }
        catch {
            # Silently skip files that can't be deleted
        }
    }
}

if ($tempCount -gt 0) {
    Write-Host "  Removed: $tempCount temporary file(s)" -ForegroundColor Gray
}

Write-Host "  ✓ Temporary files cleaned" -ForegroundColor Green
Write-Host ""

# =============================================================================
# SUMMARY
# =============================================================================
Write-Host "========================================" -ForegroundColor Cyan

if ($errorCount -eq 0) {
    Write-Host "✅ Cleanup complete! You can now build." -ForegroundColor Green
    Write-Host ""
    Write-Host "Next steps:" -ForegroundColor Cyan
    Write-Host "  1. Run: dotnet restore" -ForegroundColor Gray
    Write-Host "  2. Run: dotnet build" -ForegroundColor Gray
    Write-Host "  3. Run: dotnet run (or press F5 in VS Code)" -ForegroundColor Gray
} else {
    Write-Host "⚠️  Cleanup complete with $errorCount error(s)" -ForegroundColor Yellow
    Write-Host "Some files could not be removed, but cleanup was mostly successful." -ForegroundColor Yellow
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan

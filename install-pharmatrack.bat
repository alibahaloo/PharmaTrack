@echo off
:: ============================================
:: PharmaTrack Installer (Batch Wrapper)
:: ============================================

:: Check for admin rights
net session >nul 2>&1
if %errorlevel% neq 0 (
    echo Requesting administrator privileges...
    powershell -Command "Start-Process '%~f0' -Verb RunAs"
    exit /b
)

echo.
echo ============================================
echo     PharmaTrack Installer (via PowerShell)
echo ============================================
echo     Installing ...
echo.

:: Run the PowerShell script and wait
powershell -ExecutionPolicy Bypass -File "%~dp0install-pharmatrack.ps1"

echo.
echo     ... done!
echo     PharmaTrack installed successfully - You can now launch it from your Desktop.
echo ============================================
echo.
pause

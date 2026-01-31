@echo off
setlocal enabledelayedexpansion

:: Поиск csc.exe в стандартных локациях
set "CSC_PATH="

for %%p in (
    "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
    "C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe"
) do (
    if exist %%p (
        set "CSC_PATH=%%p"
        goto :found
    )
)

:: Альтернативный поиск через where
where csc.exe >nul 2>&1
if %ERRORLEVEL% EQU 0 (
    for /f "tokens=*" %%a in ('where csc.exe') do (
        set "CSC_PATH=%%a"
        goto :found
    )
)

echo ERROR: csc.exe not found. Please install .NET Framework 4.0 or later.
exit /b 1

:found
echo Using compiler: %CSC_PATH%

"%CSC_PATH%" /target:winexe /out:PeekThrough.exe ^
    /reference:System.Windows.Forms.dll ^
    /reference:System.Drawing.dll ^
    /optimize ^
    /debug:pdbonly ^
    Program.cs NativeMethods.cs KeyboardHook.cs GhostLogic.cs

if %ERRORLEVEL% EQU 0 (
    echo [SUCCESS] Build completed: PeekThrough.exe
) else (
    echo [FAILED] Build failed with error code %ERRORLEVEL%
)

endlocal

@echo off
set CSC="C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
%CSC% /target:winexe /out:PeekThrough.exe /reference:System.Windows.Forms.dll /reference:System.Drawing.dll Program.cs NativeMethods.cs KeyboardHook.cs GhostLogic.cs
if %ERRORLEVEL% EQU 0 (
    echo Build Successful: PeekThrough.exe
) else (
    echo Build Failed
)

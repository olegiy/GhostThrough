@echo off
echo Building GhostThrough...

set "DOTNET=%ProgramFiles%\dotnet\dotnet.exe"
if not exist "%DOTNET%" (
    echo ERROR: dotnet CLI not found at "%DOTNET%"
    echo Please install the .NET 8 SDK or a compatible dotnet host.
    exit /b 1
)

echo Compiling...
"%DOTNET%" build "%~dp0GhostThrough.csproj" -c Release -v:minimal

if errorlevel 1 (
    echo.
    echo COMPILATION FAILED!
    exit /b 1
)

echo.
echo Build successful: bin\Release\net8.0-windows\GhostThrough.exe

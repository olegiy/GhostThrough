@echo off
echo Building GhostThrough...

set "DOTNET=%ProgramFiles%\dotnet\dotnet.exe"
set "PUBLISH_DIR=%~dp0bin\publish"
if not exist "%DOTNET%" (
    echo ERROR: dotnet CLI not found at "%DOTNET%"
    echo Please install the .NET 8 SDK or a compatible dotnet host.
    exit /b 1
)

echo Publishing single-file executable...
"%DOTNET%" publish "%~dp0GhostThrough.csproj" -c Release -r win-x64 --self-contained true -o "%PUBLISH_DIR%" -v:minimal /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true

if errorlevel 1 (
    echo.
    echo PUBLISH FAILED!
    exit /b 1
)

echo.
echo Publish successful: bin\publish\GhostThrough.exe
echo This executable can be copied by itself.

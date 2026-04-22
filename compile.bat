@echo off
echo Building GhostThrough...

set "DOTNET=%ProgramFiles%\dotnet\dotnet.exe"
if not exist "%DOTNET%" (
    echo ERROR: dotnet CLI not found at "%DOTNET%"
    echo Please install the .NET 6 SDK or a compatible dotnet host.
    exit /b 1
)

if not exist "bin" mkdir "bin"
if not exist "obj" mkdir "obj"

set "OUTPUT=bin\GhostThrough.exe"
if exist "%OUTPUT%" (
    echo Removing old executable...
    del "%OUTPUT%"
)

echo Compiling...
"%DOTNET%" msbuild "%~dp0GhostThrough.csproj" /t:Build /p:Configuration=Release /p:Platform=AnyCPU /p:OutDir=%CD%\bin\ /p:BaseIntermediateOutputPath=%CD%\obj\GhostThrough\ /v:m 2> obj\compile_errors.txt

if errorlevel 1 (
    echo.
    echo COMPILATION FAILED!
    echo.
    type obj\compile_errors.txt
    exit /b 1
)

echo.
echo Build successful: %OUTPUT%
if exist obj\compile_errors.txt del obj\compile_errors.txt

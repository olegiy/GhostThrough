@echo off
echo Building KeyboardHookRegressionTest...

set "DOTNET=%ProgramFiles%\dotnet\dotnet.exe"
if not exist "%DOTNET%" (
    echo ERROR: dotnet CLI not found at "%DOTNET%"
    echo Please install the .NET 6 SDK or a compatible dotnet host.
    exit /b 1
)

if not exist "bin" mkdir "bin"
if not exist "obj" mkdir "obj"

set "OUTPUT=bin\KeyboardHookRegressionTest.exe"

echo Compiling...
"%DOTNET%" msbuild "%~dp0KeyboardHookRegressionTest.csproj" /t:Build /p:Configuration=Release /p:Platform=AnyCPU /p:OutDir=%CD%\bin\ /p:BaseIntermediateOutputPath=%CD%\obj\KeyboardHookRegressionTest\ /v:m 2> obj\compile_errors_test.txt

if errorlevel 1 (
    echo.
    echo COMPILATION FAILED!
    echo.
    type obj\compile_errors_test.txt
    exit /b 1
)

echo.
echo Running regression test...
"%OUTPUT%"
set "TEST_EXIT_CODE=%ERRORLEVEL%"

if exist obj\compile_errors_test.txt del obj\compile_errors_test.txt
exit /b %TEST_EXIT_CODE%

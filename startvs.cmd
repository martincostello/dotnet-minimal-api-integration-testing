@ECHO OFF
SETLOCAL

:: This command launches a Visual Studio solution with environment variables required to use a local version of the .NET SDK.

:: This tells .NET to use the same dotnet.exe that the build script uses.
SET DOTNET_ROOT=%~dp0.dotnetcli
SET DOTNET_ROOT(x86)=%~dp0.dotnetcli\x86

:: Put our local dotnet.exe on PATH first so Visual Studio knows which one to use.
SET PATH=%DOTNET_ROOT%;%PATH%

SET sln=%~dp0TodoApp.sln

IF NOT EXIST "%DOTNET_ROOT%\dotnet.exe" (
    echo The .NET SDK has not yet been installed. Run `%~dp0build.ps1` to install it
    exit /b 1
)

IF "%VSINSTALLDIR%" == "" (
    start "" "%sln%"
) else (
    "%VSINSTALLDIR%\Common7\IDE\devenv.com" "%sln%"
)

@ECHO OFF
SETLOCAL

:: This command launches Visual Studio Code with environment variables required to use a local version of the .NET SDK.

:: This tells .NET to use the same dotnet.exe that the build script uses.
SET DOTNET_ROOT=%~dp0.dotnetcli
SET DOTNET_ROOT(x86)=%~dp0.dotnetcli\x86

:: Put our local dotnet.exe on PATH first so Visual Studio Code knows which one to use.
SET PATH=%DOTNET_ROOT%;%PATH%

:: Sets the Target Framework for Visual Studio Code.
SET TARGET=net9.0

SET FOLDER=%~1

IF NOT EXIST "%DOTNET_ROOT%\dotnet.exe" (
    echo The .NET SDK has not yet been installed. Run `%~dp0build.ps1` to install it
    exit /b 1
)

IF "%FOLDER%"=="" (
    code .
) else (
    code "%FOLDER%"
)

exit /b 1

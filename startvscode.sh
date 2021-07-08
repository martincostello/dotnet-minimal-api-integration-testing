#!/usr/bin/env bash

# This command launches Visual Studio Code with environment variables required to use a local version of the .NET SDK.

# This tells .NET to use the same dotnet.exe that the build script uses.
DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
export DOTNET_ROOT="$DIR/.dotnetcli"

# Put our local dotnet.exe on PATH first so Visual Studio Code knows which one to use.
export PATH="$DOTNET_ROOT:$PATH"

# Sets the Target Framework for Visual Studio Code.
export TARGET=net6.0

if [ ! -f "$DOTNET_ROOT/dotnet" ]; then
    echo "The .NET SDK has not yet been installed. Run `./build.ps1` to install it"
    exit 1
fi

if [ $1 = "" ]; then
  code .
else
  code $1
fi

exit 1

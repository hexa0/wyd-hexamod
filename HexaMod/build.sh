#!/bin/bash
export WINEPREFIX="$HOME/.wine-dotnet-build"
export WINEDEBUG=-all

MSBUILD_PATH="C:/Windows/Microsoft.NET/Framework64/v4.0.30319/MSBuild.exe"

wine "$MSBUILD_PATH" HexaMod.csproj /p:Configuration=Debug /p:Platform="Any CPU"
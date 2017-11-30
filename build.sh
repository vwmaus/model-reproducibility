#!/bin/bash
set -ev
dotnet restore WebInterface/WebInterface.csproj
#dotnet publish ./WebInterface.sln -c Release -o ./obj/Docker/publish
dotnet build WebInterface/WebInterface.csproj -c Release

#!/bin/bash
set -ev
dotnet restore WebInterface.csproj
dotnet publish ./WebInterface.sln -c Release -o ./obj/Docker/publish
dotnet build -c Release

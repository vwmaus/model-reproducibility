#!/bin/bash
set -ev
dotnet restore WebInterface.csproj
dotnet test WebInterface.csproj
dotnet build -c Release WebInterface.csproj

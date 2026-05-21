#!/usr/bin/env bash
set -euo pipefail

dotnet restore ProCharts.slnx
dotnet build ProCharts.slnx -c Release --no-restore
dotnet test tests/ProCharts.Avalonia.Tests/ProCharts.Avalonia.Tests.csproj -c Release --no-build
dotnet test tests/ProCharts.Uno.Tests/ProCharts.Uno.Tests.csproj -c Release --no-build

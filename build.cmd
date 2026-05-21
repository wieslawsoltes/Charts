@echo off
setlocal

dotnet restore ProCharts.slnx || exit /b 1
dotnet build ProCharts.slnx -c Release --no-restore || exit /b 1
dotnet test tests\ProCharts.Avalonia.Tests\ProCharts.Avalonia.Tests.csproj -c Release --no-build || exit /b 1
dotnet test tests\ProCharts.Uno.Tests\ProCharts.Uno.Tests.csproj -c Release --no-build || exit /b 1

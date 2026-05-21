---
title: Installation
description: Unified central package installation guide for ProCharts in Avalonia and Uno Platform solutions.
---

# Installation

ProCharts is distributed via NuGet. This guide outlines how to integrate it using standard .NET tooling or Central Package Management.

## Standard CLI Installation

Run the appropriate command in the root folder of your project:

### To add the Avalonia UI charting library:
```bash
dotnet add [<project-file>] package ProCharts.Avalonia
```

### To add the Uno Platform charting library:
```bash
dotnet add [<project-file>] package ProCharts.Uno
```

---

## Central Package Management (CPM) Setup

If your repository leverages Central Package Management (recommended), add the version declarations to your `Directory.Packages.props` file:

```xml
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
  </PropertyGroup>

  <ItemGroup>
    <!-- ProCharts Libraries -->
    <PackageVersion Include="ProCharts.Avalonia" Version="1.0.0" />
    <PackageVersion Include="ProCharts.Uno" Version="1.0.0" />
  </ItemGroup>
</Project>
```

Then reference the package in your individual project files (`.csproj`) without specifying the `Version` attribute:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <ItemGroup>
    <PackageReference Include="ProCharts.Avalonia" />
  </ItemGroup>
</Project>
```

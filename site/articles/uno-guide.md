---
title: Uno Platform Guide
description: Render high-fidelity charts across desktop, mobile, and web targets using ProCharts.Uno.
---

# Uno Platform Guide

`ProCharts.Uno` maps the same core charting models into the Uno Platform, allowing you to run cross-platform charts on Android, iOS, Windows, macOS, and WebAssembly with uniform looks.

## 1. Single Project Integration

The package is fully compatible with modern `UnoSingleProject` configurations utilizing the `Uno.Sdk`.

Make sure to install `ProCharts.Uno` in your library or multi-head project:

```xml
<ItemGroup>
  <PackageReference Include="ProCharts.Uno" />
</ItemGroup>
```

## 2. Using WinUI XAML Markup

Because Uno Platform follows WinUI XAML namespaces, import `ProCharts.Uno` like so:

```xml
<UserControl x:Class="ProCharts.Uno.Gallery.MainPage"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:pc="using:ProCharts.Uno.Controls"
             xmlns:ps="using:ProCharts.Uno.Series">
  <Grid>
    <pc:CartesianChart Title="Cross-Platform Insights">
      <pc:CartesianChart.Series>
        <ps:LineSeries Title="Global User Growth" Stroke="#8b5cf6">
          <ps:LineSeries.Points>
            <ps:ChartPoint X="0" Y="50" />
            <ps:ChartPoint X="1" Y="120" />
            <ps:ChartPoint X="2" Y="350" />
          </ps:LineSeries.Points>
        </ps:LineSeries>
      </pc:CartesianChart.Series>
    </pc:CartesianChart>
  </Grid>
</UserControl>
```

## 3. WebAssembly Performance

When compiling to WebAssembly (`net10.0-browserwasm`), ProCharts.Uno utilizes Skia-based rendering for ultra-fast, smooth drawing speeds directly inside the HTML canvas. No extra setup is required!

---
title: Getting Started
description: Build your first chart in under 5 minutes with ProCharts.
---

# Getting Started

This guide walks you through setting up a simple cartesian chart with bar and line series using ProCharts.

## 1. Install ProCharts NuGet Package

Add the library to your Avalonia UI or Uno Platform project.

### For Avalonia UI:
```bash
dotnet add package ProCharts.Avalonia
```

### For Uno Platform:
```bash
dotnet add package ProCharts.Uno
```

## 2. Declare the XML Namespace in XAML

Add the ProCharts namespace declaration to your `Window` or `UserControl`:

### For Avalonia:
```xml
<Window xmlns="https://github.com/avaloniaui"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:pc="clr-namespace:ProCharts.Avalonia.Controls;assembly=ProCharts.Avalonia"
        xmlns:ps="clr-namespace:ProCharts.Avalonia.Series;assembly=ProCharts.Avalonia"
        Title="ProCharts Gallery" Height="450" Width="800">
  <Grid>
    <!-- Chart goes here -->
  </Grid>
</Window>
```

### For Uno Platform:
```xml
<UserControl xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:pc="using:ProCharts.Uno.Controls"
             xmlns:ps="using:ProCharts.Uno.Series">
  <Grid>
    <!-- Chart goes here -->
  </Grid>
</UserControl>
```

## 3. Configure a Cartesian Chart

Create a `CartesianChart` control and define its axes and series in XAML:

```xml
<pc:CartesianChart Title="Revenue Forecast" Glassmorphic="True">
  <!-- Series Configuration -->
  <pc:CartesianChart.Series>
    <ps:BarSeries Title="Actual Sales" Fill="#4f46e5" CornerRadius="4">
      <ps:BarSeries.Points>
        <ps:ChartPoint X="1" Y="120" />
        <ps:ChartPoint X="2" Y="180" />
        <ps:ChartPoint X="3" Y="240" />
        <ps:ChartPoint X="4" Y="310" />
      </ps:BarSeries.Points>
    </ps:BarSeries>
    
    <ps:LineSeries Title="Forecasted Sales" Stroke="#06b6d4" StrokeThickness="3">
      <ps:LineSeries.Points>
        <ps:ChartPoint X="1" Y="110" />
        <ps:ChartPoint X="2" Y="170" />
        <ps:ChartPoint X="3" Y="250" />
        <ps:ChartPoint X="4" Y="320" />
      </ps:LineSeries.Points>
    </ps:LineSeries>
  </pc:CartesianChart.Series>
</pc:CartesianChart>
```

Run the application to see the beautifully animated, modern chart render in real time.

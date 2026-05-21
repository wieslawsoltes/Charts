---
title: Avalonia UI Guide
description: Harness advanced glassmorphic charting and fluent layouts in Avalonia applications.
---

# Avalonia UI Guide

`ProCharts.Avalonia` brings our premium, highly responsive charting engine directly to Avalonia UI applications, supporting modern styling hooks, Fluent design themes, and high-DPI scaling.

## 1. Register Styling Resources

Add the ProCharts Avalonia Fluent theme dictionary to your `App.axaml`:

```xml
<Application xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             x:Class="ProCharts.Avalonia.Gallery.App"
             RequestedThemeVariant="Dark">
  <Application.Styles>
    <FluentTheme />
    <!-- Include ProCharts theme styles if applicable -->
  </Application.Styles>
</Application>
```

## 2. Leverage Glassmorphism

Enable stunning semi-transparent, frosted glass backgrounds on any chart control by setting `Glassmorphic="True"`. ProCharts handles background calculations automatically using Avalonia's visual layer APIs:

```xml
<pc:CartesianChart Glassmorphic="True" GlassOpacity="0.15" BorderThickness="1">
  <pc:CartesianChart.Series>
    <ps:AreaSeries Fill="#059669" Stroke="#34d399" />
  </pc:CartesianChart.Series>
</pc:CartesianChart>
```

## 3. High-Performance Bindings

Ensure your models implement `INotifyPropertyChanged` and bind directly to `Points`:

```xml
<ps:LineSeries Points="{Binding ChartPoints}" Stroke="#f43f5e" />
```

---
title: Getting Started
description: Build your first high-performance vector chart in under 5 minutes with ProCharts.
---

# Getting Started

ProCharts is a high-performance vector charting engine designed to bypass traditional heavy control-based hierarchies. This guide walks you through the step-by-step process of installing ProCharts, configuring your first Cartesian chart, establishing standard XAML namespaces, and implementing a fully interactive real-time telemetry dashboard using standard Model-View-ViewModel (MVVM) patterns.

## Installation

Add the ProCharts package matching your target application framework.

### Standard CLI Installation

For applications built on the **Avalonia UI** framework:
```bash
dotnet add package ProCharts.Avalonia
```

For applications built on the **Uno Platform** (WinUI-based targets):
```bash
dotnet add package ProCharts.Uno
```

---

## Declaring XML Namespaces in XAML

To consume the charting controls and geometries in your markup, declare the matching XML namespaces in your root container.

### For Avalonia UI
Map the controls and series namespaces. Specify the assembly parameter:
```xml
<Window xmlns="https://github.com/avaloniaui"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:pc="clr-namespace:ProCharts.Avalonia.Controls;assembly=ProCharts.Avalonia"
        xmlns:ps="clr-namespace:ProCharts.Avalonia.Series;assembly=ProCharts.Avalonia"
        Title="High-Performance Telemetry Monitor" Height="600" Width="1000"
        Background="#0F172A">
  <Grid Padding="24">
    <!-- Chart markup resides here -->
  </Grid>
</Window>
```

### For Uno Platform (WinUI)
Uno Platform leverages WinUI namespace resolution. Import the controls and series namespaces using the `using:` prefix:
```xml
<UserControl xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:pc="using:ProCharts.Uno.Controls"
             xmlns:ps="using:ProCharts.Uno.Series">
  <Grid Padding="24" Background="#0F172A">
    <!-- Chart markup resides here -->
  </Grid>
</UserControl>
```

---

## Designing a Telemetry MVVM Data Pipeline

In production applications, chart series data should be bound to ViewModel collections. ProCharts dynamically listens to collection change events to re-render coordinate projections.

### 1. The Data Model
Define a clear data model representing Cartesian coordinates:

```csharp
namespace TelemetryApp.Models;

public record TelemetryPoint(double TimeIndex, double Value);
```

### 2. The ViewModel
Implement a ViewModel that implements `INotifyPropertyChanged` and exposes an observable collection of coordinates. Below is an implementation of a telemetry monitor that updates values periodically:

```csharp
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using TelemetryApp.Models;

namespace TelemetryApp.ViewModels;

public class TelemetryViewModel : INotifyPropertyChanged
{
    private readonly Timer _timer;
    private readonly SynchronizationContext? _syncContext;
    private double _currentTimeIndex = 0;
    private readonly Random _random = new();

    public ObservableCollection<TelemetryPoint> UplinkSignal { get; } = new();
    public ObservableCollection<TelemetryPoint> DownlinkSignal { get; } = new();

    public TelemetryViewModel()
    {
        // Capture the synchronization context of the UI thread to marshal updates safely
        _syncContext = SynchronizationContext.Current;

        // Pre-populate historical telemetry data
        for (int i = 0; i < 50; i++)
        {
            AppendTelemetrySample();
        }

        // Initialize a low-latency timer to stream live telemetry updates
        _timer = new Timer(OnTimerTick, null, 1000, 250);
    }

    private void OnTimerTick(object? state)
    {
        // Marshal updates back to the UI thread using the captured SynchronizationContext
        if (_syncContext != null)
        {
            _syncContext.Post(_ => AppendTelemetrySample(), null);
        }
        else
        {
            AppendTelemetrySample();
        }
    }

    private void AppendTelemetrySample()
    {
        double uplinkVal = 40 + 20 * Math.Sin(_currentTimeIndex * 0.1) + _random.NextDouble() * 5;
        double downlinkVal = 60 + 15 * Math.Cos(_currentTimeIndex * 0.1) + _random.NextDouble() * 8;

        UplinkSignal.Add(new TelemetryPoint(_currentTimeIndex, uplinkVal));
        DownlinkSignal.Add(new TelemetryPoint(_currentTimeIndex, downlinkVal));

        // Keep a rolling buffer of 100 active data points to limit memory allocation
        if (UplinkSignal.Count > 100)
        {
            UplinkSignal.RemoveAt(0);
            DownlinkSignal.RemoveAt(0);
        }

        _currentTimeIndex++;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
```

---

## Configuring Advanced XAML Layout and Data Bindings

Bind your telemetry series collections directly to the chart. Enable glassmorphism and define custom colors, stroke thicknesses, and interactive legends.

```xml
<pc:CartesianChart Title="Network Performance (Telemetry Feed)"
                   PlotAreaBackground="#00FFFFFF"
                   BorderBrush="#334155"
                   BorderThickness="1"
                   Padding="16"
                   HorizontalAlignment="Stretch"
                   VerticalAlignment="Stretch">
  
  <!-- Series Configurations -->
  <pc:CartesianChart.Series>
    <ps:AreaSeries Title="Uplink Stream" 
                  ItemsSource="{Binding UplinkSignal}" 
                  CategoryPath="TimeIndex"
                  ValuePath="Value"
                  Stroke="#06B6D4" 
                  StrokeThickness="2" 
                  Fill="#4D0891B2"
                  IsSmooth="True" />
                  
    <ps:LineSeries Title="Downlink Stream" 
                  ItemsSource="{Binding DownlinkSignal}" 
                  CategoryPath="TimeIndex"
                  ValuePath="Value"
                  Stroke="#8B5CF6" 
                  StrokeThickness="3" 
                  IsSmooth="True" />
  </pc:CartesianChart.Series>

</pc:CartesianChart>
```

---

## Advanced Control Customization

ProCharts relies on high-fidelity visual layers. You can configure:
- **`IsSmooth`**: Employs mathematical spline interpolation to translate raw coordinates into a continuous Bézier path segment instead of rigid linear strokes.
- **`AreaSeries`**: Automatically generates a vertical linear gradient from your defined `Fill` color down to the chart's bottom boundary baseline, creating a professional glassmorphic flow.
- **Interactive Tooltips & Trackballs**: ProCharts automatically enables hover tooltips and interactive vertical trackballs by default. As the user moves the pointer, the engine snaps to the nearest point on the horizontal axis, draws a dashed alignment intercept line, and renders a floating glassmorphic information card displaying series values.

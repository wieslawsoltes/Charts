---
title: Avalonia UI Guide
description: Harness advanced glassmorphic charting and fluent layouts in Avalonia applications.
---

# Avalonia UI Guide

`ProCharts.Avalonia` delivers a native, highly responsive implementation of the ProCharts vector charting engine tailored specifically for the Avalonia UI framework. By directly hooking into Avalonia's visual rendering pipeline, it enables fluid 60fps animations, rich glassmorphic aesthetics, and native XAML integration.

---

## 1. Out-of-the-Box Resource Configuration

Because `ProCharts.Avalonia` executes all layout and vector composition directly inside its custom rendering pipeline via Skia or Direct2D, the controls are fully self-contained. Unlike traditional theme-dependent component libraries, **ProCharts does not require any external theme dictionaries or StyleInclude files** in `App.axaml`.

You can import the base fluent themes as normal:

```xml
<Application xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             x:Class="TelemetryApp.App"
             RequestedThemeVariant="Dark">
  <Application.Styles>
    <!-- Base Avalonia fluent theme styles -->
    <FluentTheme />
  </Application.Styles>
</Application>
```

The charting controls hook directly into Avalonia's `RequestedThemeVariant` to adapt their visual palette between Light and Dark modes automatically, using adaptive system brushes without requiring separate style templates.

---

## 2. Low-Level Rendering Pipeline

`ProCharts.Avalonia` inherits directly from `Avalonia.Controls.Control` and overrides the standard `Render` callback:

```csharp
public override void Render(DrawingContext context)
{
    base.Render(context);
    
    // Draw background and shadows
    DrawCanvasBackground(context);
    
    // Draw the gridlines and labels
    DrawCanvasGridlines(context);
    
    // Draw data geometries (lines, bars, areas)
    DrawDataGeometries(context);
    
    // Draw overlay elements (crosshairs, active selectors)
    DrawInteractiveOverlays(context);
}
```

By working directly with Avalonia's `DrawingContext`, ProCharts bypasses the logical and visual trees entirely. Each series utilizes drawing calls such as `context.DrawGeometry(...)`, `context.DrawLine(...)`, and `context.DrawRectangle(...)`, which compile straight down into Skia or Direct2D commands.

---

## 3. Glassmorphic Styling and Performance

To render stunning frosted-glass styles, wrap your charting controls inside standard layout borders featuring semi-transparent backgrounds and border highlights. Setting `PlotAreaBackground="#00FFFFFF"` (transparent) allows the underlying panel styles to show through the chart viewport.

```xml
<Border Background="#201E293B" 
        BorderBrush="#33FFFFFF" 
        BorderThickness="1" 
        CornerRadius="12" 
        Padding="16">
  <pc:CartesianChart Title="System Resource Monitoring"
                     PlotAreaBackground="#00FFFFFF">
    <pc:CartesianChart.Series>
      <ps:AreaSeries ItemsSource="{Binding SystemLoad}" 
                    CategoryPath="Timestamp" 
                    ValuePath="Usage" 
                    Fill="#4D0284C7" 
                    Stroke="#38BDF8" 
                    StrokeThickness="3" />
    </pc:CartesianChart.Series>
  </pc:CartesianChart>
</Border>
```

### Implementing Glassmorphic Overlays
In Avalonia UI, the frosted-glass effect is achieved through three layered techniques:
1. **Backdrop Blending**: The container panel leverages an `ExperimentalAcrylicBorder` or semi-transparent solid color brush with a custom alpha color channel.
2. **Noise Texturing**: A subtle layout texture is overlaid to mimic frosted grain.
3. **Contrast Highlights**: A thin, high-contrast, semi-transparent border brush (e.g., `#33FFFFFF` or `#20FFFFFF`) highlights the outer edge, distinguishing the control from dark background elements.

---

## 4. Multi-Threaded Data Streams and thread safety

When handling fast telemetry streams (such as real-time signal analysis or IoT sensors), data updates typically arrive on background threads. Modifying UI-bound collections directly from a background thread raises thread-access exceptions in Avalonia.

Always synchronize collection modifications to the UI thread using Avalonia's `Dispatcher.UIThread`:

```csharp
using Avalonia.Threading;
using System.Collections.ObjectModel;
using TelemetryApp.Models;

public class HighFrequencyPresenter
{
    public ObservableCollection<TelemetryPoint> TelemetryBuffer { get; } = new();

    public void OnTelemetryReceived(TelemetryPoint newSample)
    {
        // Offload data processing to background threads, then marshal the UI updates
        Dispatcher.UIThread.Post(() =>
        {
            TelemetryBuffer.Add(newSample);
            
            // Maintain a sliding window buffer size
            if (TelemetryBuffer.Count > 200)
            {
                TelemetryBuffer.RemoveAt(0);
            }
        }, DispatcherPriority.Render);
    }
}
```

### Optimization Recommendations
- **Leverage DispatcherPriority.Render**: Scheduling your collection updates with `DispatcherPriority.Render` helps synchronize data additions with Avalonia's layout engine, minimizing unnecessary redraw passes.
- **Set IsSmooth prudently**: Spline curves (`IsSmooth="True"`) require cubic Bézier segment calculations. For massive coordinate counts ($N > 2000$), stick to linear line rendering to avoid visual thread bottlenecks.

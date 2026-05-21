# ProCharts

[![Docs](https://img.shields.io/badge/docs-github%20pages-0f766e)](https://wieslawsoltes.github.io/ProCharts/)
![.NET](https://img.shields.io/badge/.NET-10%20%7C%209%20%7C%208-512BD4)
![XAML](https://img.shields.io/badge/XAML-Avalonia%20%7C%20Uno-0F172A)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

A state-of-the-art, high-performance vector charting engine for modern XAML applications. 

ProCharts abandons heavy and slow control-based tree hierarchies in favor of **direct vector rendering via modern drawing pipelines**, ensuring buttery smooth 60fps animations and flawless telemetry rendering even under extreme point density stresses (up to 5,000+ real-time coordinates).

Documentation site: [wieslawsoltes.github.io/ProCharts](https://wieslawsoltes.github.io/ProCharts/)

---

## Packages

| Package | NuGet | Purpose |
| --- | --- | --- |
| `ProCharts.Avalonia` | [![NuGet](https://img.shields.io/nuget/v/ProCharts.Avalonia.svg)](https://www.nuget.org/packages/ProCharts.Avalonia) | Premium charting controls, series, glassmorphism templates, and interactive legends tailored for **Avalonia UI** applications. |
| `ProCharts.Uno` | [![NuGet](https://img.shields.io/nuget/v/ProCharts.Uno.svg)](https://www.nuget.org/packages/ProCharts.Uno) | Symmetric cross-platform charting adapter supporting **Uno Platform** desktop, mobile, and WebAssembly (Skia/Wasm) pipelines. |

---

## Key Features

- **Direct Vector Rendering**: Bypasses the UI logical tree to write directly to the screen via highly optimized GPU/Skia drawing context calls.
- **Rich Aesthetic System**: State-of-the-art dark styling, glassmorphic hover overlays, pulsing concentric rings, and vibrant default palettes.
- **Unified Interactivity**:
  - **Zooming**: Center-of-mouse wheel coordinate transformation.
  - **Panning**: Middle/Right click-and-drag view scaling.
  - **Tooltips & Trackballs**: Rich glassmorphic cursor box with vertical dashed intercept lines and hover physics.
  - **Interactive Legends**: Reusable wrap-panel checkbox controls programmatically bound to toggle series visibilities.
  - **Pie Slices Explosion**: Natural radial center offset animations on segment hover.
- **Robust Math Systems**: Dual-axis mapping, Reversed axes, Logarithmic scaling, and mathematical spline Bézier interpolation.
- **NaN Point Filter**: Seamless handling of broken telemetry intervals via `EmptyPointMode` (`Zero`, `Gap`, `Average`, `Interpolate`).

---

## Architecture

| Layer | Responsibility |
| --- | --- |
| **UI Control Layer** | Captures pointer gestures (zooming, panning, hover), handles layouts, templates interactive tooltips and legends. |
| **Series Drawing Layer** | Listens to data streams, computes splines and animation easing profiles, translates points to vector paths. |
| **Math & Geometry Layer** | Performs raw Cartesian, logarithmic, reversed, and polar coordinate transformations. |

The adapters are intentionally thin and symmetric. They map framework-specific rendering pipelines (`DrawingContext` in Avalonia, Skia/Wasm in Uno Platform) into unified coordinate rendering requests.

---

## Usage

### 1. Installation

Install the package matching your framework via the .NET CLI:

```bash
# For Avalonia UI
dotnet add package ProCharts.Avalonia

# For Uno Platform
dotnet add package ProCharts.Uno
```

### 2. Basic XAML Setup

#### For Avalonia UI:
```xml
<Window xmlns="https://github.com/avaloniaui"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:pc="clr-namespace:ProCharts.Avalonia.Controls;assembly=ProCharts.Avalonia"
        xmlns:ps="clr-namespace:ProCharts.Avalonia.Series;assembly=ProCharts.Avalonia"
        Title="Telemetry Window">
  
  <Grid Padding="24" Background="#0F172A">
    <pc:CartesianChart Title="WAN Traffic (MB/s)" Glassmorphic="True">
      <pc:CartesianChart.Series>
        <ps:LineSeries Title="Uplink" Stroke="#06b6d4" StrokeThickness="3" IsSmooth="True">
          <ps:LineSeries.Points>
            <ps:ChartPoint X="0" Y="12" />
            <ps:ChartPoint X="1" Y="45" />
            <ps:ChartPoint X="2" Y="85" />
          </ps:LineSeries.Points>
        </ps:LineSeries>
      </pc:CartesianChart.Series>
    </pc:CartesianChart>
  </Grid>
</Window>
```

#### For Uno Platform:
```xml
<UserControl x:Class="MyApp.MainPage"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:pc="using:ProCharts.Uno.Controls"
             xmlns:ps="using:ProCharts.Uno.Series">
  
  <Grid Padding="24" Background="#0F172A">
    <pc:CartesianChart Title="WAN Traffic (MB/s)">
      <pc:CartesianChart.Series>
        <ps:LineSeries Title="Uplink" Stroke="#06b6d4" StrokeThickness="3">
          <ps:LineSeries.Points>
            <ps:ChartPoint X="0" Y="12" />
            <ps:ChartPoint X="1" Y="45" />
            <ps:ChartPoint X="2" Y="85" />
          </ps:LineSeries.Points>
        </ps:LineSeries>
      </pc:CartesianChart.Series>
    </pc:CartesianChart>
  </Grid>
</UserControl>
```

---

## Local Development & Automation

This repository provides multiple pre-configured automation scripts to simplify local development:

- **Build and Test**: Run `./build.sh` (macOS/Linux) or `build.cmd` (Windows) to restore, compile, and execute tests across all targets.
- **Create NuGet Packages**: Run `./pack.sh <version>` to generate and validate packed assemblies.
- **Build Documentation**: Run `./build-docs.sh` to compile the documentation site.
- **Run Docs Server**: Run `./serve-docs.sh` to serve and hot-reload the documentation site on `http://127.0.0.1:8080`.

---

## License

MIT. See [LICENSE](LICENSE).

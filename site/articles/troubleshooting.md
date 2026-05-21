---
title: Troubleshooting
description: Technical reference for diagnosing compilation, threading, layout, and rendering issues.
---

# Troubleshooting

This reference guide provides technical solutions to common issues encountered when integrating and running ProCharts inside Avalonia UI or Uno Platform solutions.

---

## 1. Type Resolution and Namespace Compilation Errors

### Issue Description
The compiler raises type resolution errors, such as:
```text
The type 'CartesianChart' was not found. Verify that you are not missing an assembly reference...
```

### Technical Solution
This is typically caused by incorrect XAML namespace mappings or reference mismatch across projects. Ensure that you have specified the correct namespace declarations:

- **For Avalonia UI Projects**:
  ```xml
  xmlns:pc="clr-namespace:ProCharts.Avalonia.Controls;assembly=ProCharts.Avalonia"
  xmlns:ps="clr-namespace:ProCharts.Avalonia.Series;assembly=ProCharts.Avalonia"
  ```
  *(Verify that the assembly reference is explicitly present. Without it, the XAML compiler will fail to resolve the types during compilation.)*

- **For Uno Platform (WinUI) Projects**:
  ```xml
  xmlns:pc="using:ProCharts.Uno.Controls"
  xmlns:ps="using:ProCharts.Uno.Series"
  ```
  *(WinUI maps namespaces using the `using:` syntax instead of `clr-namespace`. Ensure no assembly parameter is appended.)*

---

## 2. Central Package Management (CPM) Conflicts

### Issue Description
The build system outputs NuGet restore error NU1008:
```text
NU1008: Projects that use Central Package Management cannot define project-level PackageReference versions.
```

### Technical Solution
When CPM is active, all package versions must be declared centrally at the root of the repository in `Directory.Packages.props`. Individual project files (`.csproj`) must not specify `Version` attributes on their `<PackageReference>` tags.

1. Open the failing `.csproj` file and remove the `Version` attribute:
   ```xml
   <!-- Incorrect -->
   <PackageReference Include="ProCharts.Avalonia" Version="1.0.0" />

   <!-- Correct -->
   <PackageReference Include="ProCharts.Avalonia" />
   ```
2. Ensure the version is specified in the root `Directory.Packages.props` file:
   ```xml
   <Project>
     <ItemGroup>
       <PackageVersion Include="ProCharts.Avalonia" Version="1.0.0" />
     </ItemGroup>
   </Project>
   ```

---

## 3. Thread-Access Violations and UI Freezing

### Issue Description
When streaming high-frequency data, the application crashes with a thread-access exception (e.g., `System.InvalidOperationException: Call from invalid thread`), or the UI freezes completely.

### Technical Solution
This occurs when background threads (such as network listeners or timer threads) write directly to data collections bound to the UI. ProCharts listens to collection changes and must execute coordinate re-projections on the main rendering thread.

You must wrap collection updates in the host platform's thread scheduler.

#### Thread-Safe Dispatch in Avalonia UI:
```csharp
using Avalonia.Threading;

// Dispatch the data update to the UI thread asynchronously
Dispatcher.UIThread.Post(() =>
{
    TelemetryCollection.Add(newSample);
}, DispatcherPriority.Normal);
```

#### Thread-Safe Dispatch in Uno Platform (WinUI):
```csharp
using Microsoft.UI.Dispatching;

// Dispatch the data update to the WinUI UI thread
this.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Normal, () =>
{
    TelemetryCollection.Add(newSample);
});
```

---

## 4. Blurry Visual Elements on High-DPI or Retina Screens

### Issue Description
Charts and text labels look blurry or fuzzy on High-DPI screens, especially in WebAssembly deployments.

### Technical Solution
This issue is caused by the browser scaling a standard-density canvas, or by missing anti-aliasing configurations.

1. **For Uno Platform WebAssembly**:
   Verify that your project references Skia-based rendering pipelines. The Skia rendering backend supports native rasterization matching the device's physical pixel boundaries.
   Ensure Skia is enabled in your `.csproj`:
   ```xml
   <UnoFeatures>
     SkiaRenderer;
   </UnoFeatures>
   ```
2. **For Avalonia UI Text Sharpness**:
   Apply high-fidelity rendering options inside your root chart containers or app-level style resources:
   ```xml
   <pc:CartesianChart RenderOptions.TextRenderingMode="Antialias"
                      RenderOptions.EdgeMode="Aliased">
   ```
   *(Setting `EdgeMode="Aliased"` is recommended for gridlines to ensure they snap to physical pixel boundaries, preventing sub-pixel interpolation blur.)*

---

## 5. Blank Viewport with Zero-Width Bounds

### Issue Description
The chart renders as an empty viewport with no gridlines or series, or throws a division-by-zero exception during initialization.

### Technical Solution
This happens when all points in a data series have the exact same value on an axis (e.g., a constant timeline where all Y values equal `100.0`). The division-by-zero error occurs during normal min-max domain projection:

$$\text{ScaleFactor} = \frac{\text{Width}}{Y_{\max} - Y_{\min}} = \frac{\text{Width}}{100.0 - 100.0} = \infty$$

While ProCharts implements safety bounds internally by padding equal boundaries (e.g., adjusting the domain to $[99.0, 101.0]$ when they match), you can prevent boundary resolution errors by setting explicit axis boundaries:

```xml
<pc:CartesianChart Title="Constant Pressure Monitor">
  <pc:CartesianChart.XAxis>
    <pc:Axis Minimum="0" Maximum="10" />
  </pc:CartesianChart.XAxis>
  <pc:CartesianChart.YAxis>
    <pc:Axis Minimum="0" Maximum="200" />
  </pc:CartesianChart.YAxis>
  <pc:CartesianChart.Series>
    <ps:LineSeries ItemsSource="{Binding ConstantSeries}" />
  </pc:CartesianChart.Series>
</pc:CartesianChart>
```
Setting explicit axis boundaries prevents dynamic scaling calculations from producing zero-width ranges when data points are uniform.

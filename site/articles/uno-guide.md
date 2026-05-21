---
title: Uno Platform Guide
description: Render high-fidelity charts across desktop, mobile, and web targets using ProCharts.Uno.
---

# Uno Platform Guide

`ProCharts.Uno` provides a symmetric, high-performance implementation of our vector charting engine for the Uno Platform. It bridges the gap between different target operating systems by mapping high-fidelity vector calculations into a single, unified WinUI/Skia API stack. This allows developers to construct charts that render identically across Windows, macOS, Linux, iOS, Android, and WebAssembly.

---

## 1. Multi-Target Project Integration

`ProCharts.Uno` is built to integrate with modern single-project structures supported by the `Uno.Sdk`. To include the charting engine, add the package reference inside your primary class library or shared project:

```xml
<Project Sdk="Uno.Sdk">
  <PropertyGroup>
    <TargetFrameworks>net10.0;net10.0-windows10.0.19041;net10.0-browserwasm;net10.0-ios;net10.0-android</TargetFrameworks>
  </PropertyGroup>

  <ItemGroup>
    <!-- Core charting adapter for Uno Platform -->
    <PackageReference Include="ProCharts.Uno" />
  </ItemGroup>
</Project>
```

---

## 2. Platform Adaptations and the Skia Graphics Engine

To maintain high performance and cross-platform consistency, ProCharts bypasses platform-specific XAML UI elements (which would inflate separate rendering components on iOS, Android, and Windows). Instead, it adapts to the host platform using the Skia rendering engine:

- **Desktop (macOS, Linux, Windows)**: Renders directly using hardware-accelerated Skia surfaces.
- **Mobile (iOS & Android)**: Leverages Skia-backed hardware canvas controls to write directly to native OpenGL/Metal framebuffers.
- **Web (WebAssembly)**: Hooks directly into the HTML5 `<canvas>` rendering pipeline via Skia Wasm.

This strategy ensures that coordinate computations, rounded corners, splines, and gradients render pixel-perfect across all operating systems without depending on standard platform controls.

---

## 3. High-Performance WebAssembly (Wasm) Details

Running interactive charts in web browsers requires careful attention to performance to avoid user interface lag. `ProCharts.Uno` utilizes several WebAssembly-specific optimizations:

### Low-Level Canvas Blitting
On WebAssembly, the charting controls render to an HTML5 `<canvas>` element. Rather than invoking JavaScript APIs for vector strokes, the charting engine compiles down to WebAssembly bytecode that directly draws into the shared Skia graphics buffer. This approach reduces the overhead of JS-to-WebAssembly boundary crossings.

### Assembly Configuration
For production deployments, compile the application using Ahead-Of-Time (AOT) compilation. This step compiles C# code directly into WebAssembly machine instructions, improving coordinate transformation speeds by up to 10x compared to standard interpretation.

To enable full AOT compilation, configure your project properties:
```xml
<PropertyGroup Condition="'$(TargetFramework)' == 'net10.0-browserwasm'">
  <WasmShellMonoRuntimeExecutionMode>InterpreterAndAOT</WasmShellMonoRuntimeExecutionMode>
  <RunAOTCompilation>true</RunAOTCompilation>
</PropertyGroup>
```

---

## 4. High-DPI Scaling and Retina Displays

One of the challenges of cross-platform development is maintaining visual quality across displays with varying pixel densities.

To prevent charts from looking blurry or pixelated on high-density viewports (such as Apple Retina screens or 4K Windows displays), `ProCharts.Uno` monitors layout coordinates and synchronizes them with system scale adjustments.

It queries the system scale parameters dynamically during layout updates:

```csharp
// Retrieve the system scale factor dynamically
double rasterScale = this.XamlRoot?.RasterizationScale ?? 1.0;

// Apply the scale factor to adapt vector stroke paths and text rendering
double scaledStrokeThickness = StrokeThickness * rasterScale;
double scaledFontSize = FontSize * rasterScale;
```

This rasterization scaling step ensures that:
- Line outlines are drawn with precise sub-pixel coordinates.
- Text labels maintain clear, sharp boundaries without depending on browser scaling heuristics.
- Anti-aliasing filters run at double density on high-DPI screens.

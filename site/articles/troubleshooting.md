---
title: Troubleshooting
description: Quick tips for diagnosing common rendering, layout, or compilation issues in ProCharts.
---

# Troubleshooting

This page addresses common issues developers encounter when integrating ProCharts and how to resolve them.

## 1. Missing XML Namespaces or Elements

### Issue:
The compiler raises: `The type 'CartesianChart' was not found. Verify that you are not missing an assembly reference...`

### Solution:
Make sure you have declared the correct namespace mappings depending on your framework:
- **Avalonia**: `xmlns:pc="clr-namespace:ProCharts.Avalonia.Controls;assembly=ProCharts.Avalonia"`
- **Uno Platform**: `xmlns:pc="using:ProCharts.Uno.Controls"`

## 2. Central Package Management Warnings

### Issue:
NU1008: `Projects that use Central Package Management cannot define project-level PackageReference versions.`

### Solution:
Ensure you have completely removed the `Version="..."` attribute from any `<PackageReference>` inside your individual project files. All package versions must reside solely inside the central `Directory.Packages.props` file located at the root of your repository.

## 3. High-DPI Blur or Pixellation in WebAssembly

### Issue:
Charts appear slightly blurry or fuzzy on retina displays when running on Uno WebAssembly.

### Solution:
Ensure that you are enabling the Skia WebAssembly renderer by setting the appropriate features inside your `.csproj`:
```xml
<UnoFeatures>
  SkiaRenderer;
</UnoFeatures>
```
ProCharts utilizes the high-fidelity native Skia pipeline to render crisp vectors at double density on retina and high-DPI viewports.

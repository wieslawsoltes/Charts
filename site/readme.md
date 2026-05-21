---
title: ProCharts
description: Premium Native Vector Charting Engine for Avalonia UI and Uno Platform
---

# ProCharts: Premium Native Vector Charting Engine

Welcome to **ProCharts**, a fully native, high-performance, and visually stunning charting engine built from the ground up for modern XAML applications. 

ProCharts abandons heavy and slow control-based tree hierarchies in favor of **direct vector rendering via modern drawing pipelines**, ensuring buttery smooth 60fps animations and flawless telemetry rendering even under extreme point density stresses (up to 5,000+ real-time coordinates).

ProCharts is available as completely symmetric packages for both **Avalonia UI** and **Uno Platform**.

---

## 🚀 Key Features

* **Direct Vector Rendering**: Bypasses the UI logical tree to write directly to the screen via highly optimized GPU/Skia drawing context calls.
* **Rich Aesthetic System**: State-of-the-art dark styling, glassmorphic hover overlays, pulsing concentric rings, and vibrant default palettes.
* **Unified Interactivity**:
  * **Zooming**: Center-of-mouse wheel coordinate transformation.
  * **Panning**: Middle/Right click-and-drag view scaling.
  * **Tooltips & Trackballs**: Rich glassmorphic cursor box with vertical dashed intercept lines and hover physics.
  * **Interactive Legends**: Reusable wrap-panel checkbox controls programmatically bound to toggle series visibilities.
  * **Pie Slices Explosion**: Natural radial center offset animations on segment hover.
* **Robust Math Systems**: Dual-axis mapping, Reversed axes, Logarithmic scaling, and mathematical spline Bézier interpolation.
* **NaN Point Filter**: Seamless handling of broken telemetry intervals via `EmptyPointMode` (`Zero`, `Gap`, `Average`, `Interpolate`).

---

## 🏛️ Architecture Breakdown

The ProCharts system consists of three architectural layers working together:

```
  ┌──────────────────────────────────────────────────────────┐
  │                 UI LAYER (CartesianChart / PieChart)      │
  │   - TemplatedControls capturing pointer events & size    │
  │   - Unified Legend wrappers & Tooltip visual renders     │
  └────────────────────────────┬─────────────────────────────┘
                               │
                               ▼
  ┌──────────────────────────────────────────────────────────┐
  │         SERIES DRAW LAYER (LineSeries / BarSeries / etc) │
  │   - Listens to ItemsSource collections                   │
  │   - Runs spline calculations & entry ease interpolations  │
  └────────────────────────────┬─────────────────────────────┘
                               │
                               ▼
  ┌──────────────────────────────────────────────────────────┐
  │            MATH & GEOMETRY LAYER (CoordinateTransform)   │
  │   - Direct scaling & logarithmic conversions             │
  │   - Dynamic data point bounding                          │
  └──────────────────────────────────────────────────────────┘
```

### 1. Vector Drawing Pipeline
Rather than defining individual paths as heavy framework UI elements, charts are drawn dynamically in each framework's render loop:
* Grid lines are calculated on-the-fly and rendered as thin, translucent single-stroke paths.
* Bars are represented as `RoundedRect` objects to support custom corner styling.
* Line segments are grouped and drawn either as lines or custom Bézier spline streams to reduce visual artifacting.

### 2. Coordinate Scaling Transforms
Mapping data coordinate tuples `(X, Y)` to visual pixel bounds `(PX, PY)` is handled by `CoordinateTransform.cs`. It computes:

$$PX = Left + (X - XMin) \times \frac{Width}{XMax - XMin}$$

$$PY = Bottom - (Y - YMin) \times \frac{Height}{YMax - YMin}$$

It natively supports reversed coordinates (plotting descending depths) and logarithmic scaling (compressing astronomical telemetry distributions):

$$X_{log} = \log_{10}(X)$$

### 3. NaN Points and Empty Point Modes
When telemetry feeds drop connection, databases write `null` or `double.NaN`. ProCharts handles this elegantly via `EmptyPointMode`:
* **Zero**: Treats missing values as `0.0`.
* **Gap**: Stops rendering the current stroke segment and resumes at the next valid point, drawing a clean visual break.
* **Average**: Replaces the NaN coordinate with the mathematical mean of its immediate left and right valid neighbors.
* **Interpolate**: Performs a linear transition between the neighboring valid points, masking the loss.

---

## 📊 Available Series Geometries

### `LineSeries`
Perfect for continuous numeric timelines. Employs a custom segment builder that separates arrays into non-contiguous slices when NaN bounds are crossed, maintaining pristine gaps.

### `AreaSeries`
Extends `LineSeries`. It parses the exact same coordinate points, closes the vector path at the X-axis baseline, and fills it with an automatically generated HSL-harmonized vertical linear gradient that fades smoothly into the background grid.

### `BarSeries`
Designed for grouped comparisons. When multiple `BarSeries` are declared inside the same `CartesianChart`, they query their peer collections and automatically compute clustering positions, column widths, and offsets, rendering them in stunning rounded rectangles.

### `PieSeries`
Translates standard values into proportional angles ($0^\circ$ to $360^\circ$). Built-in hover listeners dynamically calculate polar radii:

$$R(\theta) = Center + ExplodeDistance \times \cos(\theta)$$

Creating a frictionless radial exit physics effect.

---

## 🔮 Advanced Controls & Visualizations

In addition to traditional Cartesian plots, ProCharts offers dedicated advanced controls for financial analysis, status monitoring, structural hierarchies, statistical distribution, and executive analytics dashboards:

### 📈 Financial Ecosystem
* **`CandlestickSeries`**: Custom OHLC visual representing market trades. Renders high-fidelity wick stems and body blocks with custom brush overrides.
* **`OhlcSeries`**: Elegant tick-bar representation of Open, High, Low, and Close market prices with horizontal tick projections.
* **`HiloSeries`**: Minimalist range plots showcasing high-to-low bands.

### 🎛️ Status & Monitoring Gauges
* **`CircularGauge`**: Radial swept gauge featuring warnings, needle trackers, custom swept thickness, and a central digital readout.
* **`LinearGauge`**: Premium horizontal or vertical glassmorphic slider highlighting qualitative thresholds (Warning/Error).
* **`LiquidFillGauge`**: Dynamic container containing animated vector sine-wave liquid sweeps rising to represent precise telemetry storage.

### 🕸️ Hierarchy & Process Flows
* **`SankeyChart`**: Stunning flow maps using Bézier curves to link variable weight ribbons from source to destination nodes.
* **`TreemapChart`**: Squarified partition rectangles visualizing deep hierarchical structures with adaptive categorical colors.

### 🧪 Statistical Plotting
* **`BoxPlotSeries`**: Full distribution representation highlighting upper/lower bounds, whiskers, median line, interquartile ranges, and floating outlier markers.
* **`BeeswarmPlotSeries`**: Dynamic point-packing swarm scatter coordinates that resolve overlapping scatter collisions.
* **`HistogramSeries`**: Automatic value binning and frequency distributions grouped in clean modern bars.

### 📊 Analytics & KPI Dashboards
* **`KpiCard`**: Ultra-premium glassmorphic card showcasing critical indicators, inline mini-sparkline vectors, and colored positive/negative trend badges.
* **`HeatmapChart`**: Cartesian category load matrix cells representing heat density via smooth palette lookup interpolations.
* **`BulletChart`**: Highly compact linear dashboard control plotting target markers against colored qualitative range intervals.

---

## 🎨 Professional Themes and Palettes

ProCharts includes prebuilt custom palettes to match modern dark desktop trends:
1. **Modern**: Elegant slate indigoes, teals, and light purples (`Palette.Modern`).
2. **Emerald**: Mint, forest, and sage green gradients (`Palette.Emerald`).
3. **Slate Dark**: Sleek metallic lavender and muted gray tones (`Palette.SlateDark`).
4. **Retro**: Vibrant warm orange, pastel yellow, and soft rose (`Palette.Retro`).
5. **Cyberpunk**: Rich high-contrast neon cyan, neon pink, and electric gold (`Palette.Cyberpunk`).

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

using ProCharts.Uno.Components;
using ProCharts.Uno.Controls;
using ProCharts.Uno.Maths;
using ProCharts.Uno.Series;
using ProCharts.Uno.Styles;

using Point = ProCharts.Uno.Maths.Point;

namespace ProCharts.Uno.Gallery
{
    public sealed partial class MainPage : Page
    {
        // --- DATA FIELDS ---
        private readonly ObservableCollection<Point> _lineSeries1Data = new();
        private readonly ObservableCollection<Point> _lineSeries2Data = new();

        private readonly ObservableCollection<Point> _areaSeries1Data = new();
        private readonly ObservableCollection<Point> _areaSeries2Data = new();

        private readonly ObservableCollection<Point> _barSeries1Data = new();
        private readonly ObservableCollection<Point> _barSeries2Data = new();

        // Real-time fields
        private DispatcherTimer? _rtTimer;
        private readonly ObservableCollection<Point> _rtData = new();
        private double _rtTimeIndex = 0;
        private int _rtMaxPoints = 50;

        // Financial
        private readonly ObservableCollection<FinancialSeries.FinancialPoint> _financialData = new();

        // Hierarchy / Flows
        private readonly ObservableCollection<SankeyChart.SankeyNode> _sankeyNodes = new();
        private readonly ObservableCollection<SankeyChart.SankeyLink> _sankeyLinks = new();
        private readonly ObservableCollection<TreemapChart.TreemapItem> _treemapItems = new();

        // Statistical (BoxPlot / Beeswarm / Histogram)
        public class StatDataGroup
        {
            public double Category { get; set; }
            public List<double> Values { get; set; } = new();
        }
        private readonly ObservableCollection<StatDataGroup> _statGroups = new();
        private readonly ObservableCollection<double> _histogramRawValues = new();

        // Analytics (Heatmap Cells)
        private readonly ObservableCollection<HeatmapChart.HeatmapCell> _heatmapCells = new();
        private readonly List<string> _heatmapXLabels = new();
        private readonly List<string> _heatmapYLabels = new();
        private readonly List<double> _sparklineData1 = new();
        private readonly List<double> _sparklineData2 = new();
        private readonly List<double> _sparklineData3 = new();

        // Data Types & Fields
        public class BubblePoint { public double X { get; set; } public double Y { get; set; } public double Size { get; set; } }
        public class StageValueItem { public string Stage { get; set; } = string.Empty; public double Amount { get; set; } }
        public class WordItem { public string Text { get; set; } = string.Empty; public double Weight { get; set; } }
        public class TernaryItem { public double ComponentA { get; set; } public double ComponentB { get; set; } public double ComponentC { get; set; } }
        public class WaterfallPoint { public string Stage { get; set; } = string.Empty; public double Value { get; set; } public bool IsTotal { get; set; } }
        public class DivergingSentimentItem { public string Category { get; set; } = string.Empty; public double LeftValue { get; set; } public double RightValue { get; set; } }
        public class TornadoVariableItem { public string Variable { get; set; } = string.Empty; public double LowValue { get; set; } public double HighValue { get; set; } }
        public class RingItem { public string Title { get; set; } = string.Empty; public double Value { get; set; } }

        private readonly ObservableCollection<BubblePoint> _bubbleSeries1Data = new();
        private readonly ObservableCollection<BubblePoint> _bubbleSeries2Data = new();
        private readonly ObservableCollection<StageValueItem> _funnelData = new();
        private readonly ObservableCollection<StageValueItem> _waffleData = new();
        private readonly ObservableCollection<WordItem> _wordCloudData = new();
        private readonly ObservableCollection<TernaryItem> _ternaryData = new();
        private readonly ObservableCollection<WaterfallPoint> _waterfallData = new();
        private readonly ObservableCollection<DivergingSentimentItem> _divergingData = new();
        private readonly ObservableCollection<TornadoVariableItem> _tornadoData = new();
        private readonly ObservableCollection<RingItem> _ringData = new();
        private readonly ObservableCollection<AlluvialChart.AlluvialNode> _alluvialNodes = new();
        private readonly ObservableCollection<AlluvialChart.AlluvialLink> _alluvialLinks = new();
        private readonly ObservableCollection<SunburstChart.SunburstNode> _sunburstRoots = new();

        // Active Chart Tracking
        private ChartBase _activeChart = null!;
        private string _activeCategory = "Line";

        public MainPage()
        {
            this.InitializeComponent();

            // 1. Setup Initial Data Sets
            ResetSampleData();

            // 2. Setup Chart Series programmatically
            SetupCharts();

            // 3. Initialize ComboBox selections and default states
            _activeChart = LineChart;
            CategoryList.SelectedIndex = 0; // Select first item

            // Clean up timers on unload
            Unloaded += (s, e) =>
            {
                if (_rtTimer != null)
                {
                    _rtTimer.Stop();
                    _rtTimer = null;
                }
            };
        }

        private void ResetSampleData()
        {
            var rand = Random.Shared;

            // Line Chart Data (Include NaN in second series to test EmptyPointMode)
            _lineSeries1Data.Clear();
            _lineSeries1Data.AddRange(new[]
            {
                new Point(0, 120), new Point(1, 150), new Point(2, 110),
                new Point(3, 190), new Point(4, 220), new Point(5, 170),
                new Point(6, 250), new Point(7, 210), new Point(8, 290)
            });

            _lineSeries2Data.Clear();
            _lineSeries2Data.AddRange(new[]
            {
                new Point(0, 80), new Point(1, 110), new Point(2, 95),
                new Point(3, 140), new Point(4, double.NaN), new Point(5, 120), // NaN point
                new Point(6, 180), new Point(7, 150), new Point(8, 210)
            });

            // Area Chart Data
            _areaSeries1Data.Clear();
            _areaSeries1Data.AddRange(new[]
            {
                new Point(0, 45), new Point(1, 55), new Point(2, 40),
                new Point(3, 65), new Point(4, 80), new Point(5, 50),
                new Point(6, 75), new Point(7, 60), new Point(8, 85)
            });

            _areaSeries2Data.Clear();
            _areaSeries2Data.AddRange(new[]
            {
                new Point(0, 25), new Point(1, 35), new Point(2, 22),
                new Point(3, 45), new Point(4, 55), new Point(5, 30),
                new Point(6, 50), new Point(7, 40), new Point(8, 60)
            });

            // Bar Chart Data (5 Categories)
            _barSeries1Data.Clear();
            _barSeries1Data.AddRange(new[]
            {
                new Point(0, 150), new Point(1, 230), new Point(2, 180),
                new Point(3, 310), new Point(4, 250)
            });

            _barSeries2Data.Clear();
            _barSeries2Data.AddRange(new[]
            {
                new Point(0, 120), new Point(1, 190), new Point(2, 220),
                new Point(3, 270), new Point(4, 290)
            });

            // Real-time Data Start
            _rtData.Clear();
            _rtTimeIndex = 0;
            for (int i = 0; i < 50; i++)
            {
                _rtTimeIndex = i;
                double val = 50 + 40 * Math.Sin(i * 0.15) + 15 * Math.Cos(i * 0.4);
                _rtData.Add(new Point(_rtTimeIndex, val));
            }

            // Financial price feed data with volatile random-walk upward trend
            _financialData.Clear();
            double open = 145.0;
            for (int i = 0; i < 20; i++)
            {
                double close = open + (rand.NextDouble() - 0.43) * 14.0;
                double high = Math.Max(open, close) + rand.NextDouble() * 6.0;
                double low = Math.Min(open, close) - rand.NextDouble() * 6.0;

                _financialData.Add(new FinancialSeries.FinancialPoint
                {
                    X = i,
                    Open = open,
                    High = high,
                    Low = low,
                    Close = close
                });

                open = close;
            }

            // Sankey Energy Flow Network
            _sankeyNodes.Clear();
            _sankeyNodes.AddRange(new[]
            {
                new SankeyChart.SankeyNode { Name = "Coal Energy", Value = 30 },
                new SankeyChart.SankeyNode { Name = "Solar Panel", Value = 20 },
                new SankeyChart.SankeyNode { Name = "Wind Farms", Value = 25 },
                new SankeyChart.SankeyNode { Name = "Nuclear Plant", Value = 25 },
                new SankeyChart.SankeyNode { Name = "Electric Grid", Value = 100 },
                new SankeyChart.SankeyNode { Name = "Industrial", Value = 60 },
                new SankeyChart.SankeyNode { Name = "Residential", Value = 40 }
            });

            _sankeyLinks.Clear();
            _sankeyLinks.AddRange(new[]
            {
                new SankeyChart.SankeyLink { Source = "Coal Energy", Target = "Electric Grid", Flow = 30 },
                new SankeyChart.SankeyLink { Source = "Solar Panel", Target = "Electric Grid", Flow = 20 },
                new SankeyChart.SankeyLink { Source = "Wind Farms", Target = "Electric Grid", Flow = 25 },
                new SankeyChart.SankeyLink { Source = "Nuclear Plant", Target = "Electric Grid", Flow = 25 },
                new SankeyChart.SankeyLink { Source = "Electric Grid", Target = "Industrial", Flow = 60 },
                new SankeyChart.SankeyLink { Source = "Electric Grid", Target = "Residential", Flow = 40 }
            });

            // Treemap disk directories
            _treemapItems.Clear();
            _treemapItems.AddRange(new[]
            {
                new TreemapChart.TreemapItem { Label = "Documents", Value = 45.0 },
                new TreemapChart.TreemapItem { Label = "Videos", Value = 35.0 },
                new TreemapChart.TreemapItem { Label = "System Files", Value = 25.0 },
                new TreemapChart.TreemapItem { Label = "Music Library", Value = 18.0 },
                new TreemapChart.TreemapItem { Label = "Cache Folder", Value = 12.0 },
                new TreemapChart.TreemapItem { Label = "Temp Logs", Value = 8.0 }
            });

            // Statistical datasets (Normal distributions Box-Muller)
            _statGroups.Clear();
            for (int cat = 0; cat < 3; cat++)
            {
                var vals = new List<double>();
                double mean = 80.0 + cat * 35.0;
                double stdDev = 12.0 + cat * 6.0;

                for (int i = 0; i < 40; i++)
                {
                    double u1 = 1.0 - rand.NextDouble();
                    double u2 = 1.0 - rand.NextDouble();
                    double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
                    vals.Add(mean + stdDev * randStdNormal);
                }

                // Add Outliers explicitly
                vals.Add(mean - stdDev * 3.6);
                vals.Add(mean + stdDev * 3.8);

                _statGroups.Add(new StatDataGroup
                {
                    Category = cat,
                    Values = vals
                });
            }

            // Histogram values (Bi-modal distribution)
            _histogramRawValues.Clear();
            for (int i = 0; i < 180; i++)
            {
                double u1 = 1.0 - rand.NextDouble();
                double u2 = 1.0 - rand.NextDouble();
                double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
                
                if (rand.NextDouble() < 0.55)
                {
                    _histogramRawValues.Add(70.0 + 12.0 * randStdNormal);
                }
                else
                {
                    _histogramRawValues.Add(130.0 + 16.0 * randStdNormal);
                }
            }

            // Heatmap grid loads
            _heatmapXLabels.Clear();
            _heatmapXLabels.AddRange(new[] { "Mon", "Tue", "Wed", "Thu", "Fri" });

            _heatmapYLabels.Clear();
            _heatmapYLabels.AddRange(new[] { "Night", "Morning", "Midday", "Afternoon", "Evening", "Late" });

            _heatmapCells.Clear();
            for (int x = 0; x < 5; x++)
            {
                for (int y = 0; y < 6; y++)
                {
                    double loadBase = 10.0 + (x * 4.0) + (y * 6.0);
                    if (y == 2 || y == 3 || y == 4) loadBase += 40.0;
                    double cellVal = loadBase + rand.NextDouble() * 15.0;
                    _heatmapCells.Add(new HeatmapChart.HeatmapCell { X = x, Y = y, Value = cellVal });
                }
            }

            // Sparkline trends
            _sparklineData1.Clear();
            _sparklineData2.Clear();
            _sparklineData3.Clear();
            double val1 = 120.0, val2 = 60.0, val3 = 3.0;
            for (int i = 0; i < 15; i++)
            {
                val1 += (rand.NextDouble() - 0.4) * 12.0;
                val2 += (rand.NextDouble() - 0.5) * 8.0;
                val3 += (rand.NextDouble() - 0.52) * 0.35;
                _sparklineData1.Add(val1);
                _sparklineData2.Add(val2);
                _sparklineData3.Add(val3);
            }

            // Bubble Chart Data
            _bubbleSeries1Data.Clear();
            _bubbleSeries1Data.AddRange(new[]
            {
                new BubblePoint { X = 1, Y = 120, Size = 25 },
                new BubblePoint { X = 2, Y = 180, Size = 50 },
                new BubblePoint { X = 3, Y = 140, Size = 15 },
                new BubblePoint { X = 4, Y = 220, Size = 80 },
                new BubblePoint { X = 5, Y = 190, Size = 45 }
            });
            _bubbleSeries2Data.Clear();
            _bubbleSeries2Data.AddRange(new[]
            {
                new BubblePoint { X = 1, Y = 90, Size = 40 },
                new BubblePoint { X = 2, Y = 130, Size = 20 },
                new BubblePoint { X = 3, Y = 160, Size = 60 },
                new BubblePoint { X = 4, Y = 110, Size = 30 },
                new BubblePoint { X = 5, Y = 240, Size = 90 }
            });

            // Funnel Pipeline
            _funnelData.Clear();
            _funnelData.AddRange(new[]
            {
                new StageValueItem { Stage = "Prospects", Amount = 12000 },
                new StageValueItem { Stage = "Qualified Leads", Amount = 8500 },
                new StageValueItem { Stage = "Proposal Sent", Amount = 5000 },
                new StageValueItem { Stage = "Negotiation", Amount = 3000 },
                new StageValueItem { Stage = "Closed Won", Amount = 1800 }
            });

            // Waffle Parts
            _waffleData.Clear();
            _waffleData.AddRange(new[]
            {
                new StageValueItem { Stage = "Frontend", Amount = 45 },
                new StageValueItem { Stage = "Backend", Amount = 35 },
                new StageValueItem { Stage = "Database", Amount = 12 },
                new StageValueItem { Stage = "DevOps", Amount = 8 }
            });

            // Word Cloud
            _wordCloudData.Clear();
            _wordCloudData.AddRange(new[]
            {
                new WordItem { Text = "AvaloniaUI", Weight = 45 },
                new WordItem { Text = "ProCharts", Weight = 40 },
                new WordItem { Text = "Dotnet", Weight = 32 },
                new WordItem { Text = "Vector2D", Weight = 28 },
                new WordItem { Text = "Performance", Weight = 25 },
                new WordItem { Text = "Telemetry", Weight = 22 },
                new WordItem { Text = "Sankey", Weight = 18 },
                new WordItem { Text = "Beeswarm", Weight = 16 },
                new WordItem { Text = "Funnel", Weight = 15 },
                new WordItem { Text = "Waffle", Weight = 12 }
            });

            // Ternary phase diagram
            _ternaryData.Clear();
            _ternaryData.AddRange(new[]
            {
                new TernaryItem { ComponentA = 0.2, ComponentB = 0.5, ComponentC = 0.3 },
                new TernaryItem { ComponentA = 0.4, ComponentB = 0.3, ComponentC = 0.3 },
                new TernaryItem { ComponentA = 0.1, ComponentB = 0.8, ComponentC = 0.1 },
                new TernaryItem { ComponentA = 0.6, ComponentB = 0.1, ComponentC = 0.3 }
            });

            // Waterfall Cashflow
            _waterfallData.Clear();
            _waterfallData.AddRange(new[]
            {
                new WaterfallPoint { Stage = "Start Capital", Value = 150000, IsTotal = false },
                new WaterfallPoint { Stage = "Product Sales", Value = 45000, IsTotal = false },
                new WaterfallPoint { Stage = "Licensing", Value = 18000, IsTotal = false },
                new WaterfallPoint { Stage = "Salaries", Value = -35000, IsTotal = false },
                new WaterfallPoint { Stage = "Infrastructure", Value = -15000, IsTotal = false },
                new WaterfallPoint { Stage = "Marketing", Value = -12000, IsTotal = false },
                new WaterfallPoint { Stage = "Total Profit", Value = 0, IsTotal = true }
            });

            // Diverging sentiments
            _divergingData.Clear();
            _divergingData.AddRange(new[]
            {
                new DivergingSentimentItem { Category = "User Interface", LeftValue = 24, RightValue = 76 },
                new DivergingSentimentItem { Category = "Pricing Model", LeftValue = 48, RightValue = 52 },
                new DivergingSentimentItem { Category = "API Completeness", LeftValue = 15, RightValue = 85 },
                new DivergingSentimentItem { Category = "Documentation", LeftValue = 35, RightValue = 65 },
                new DivergingSentimentItem { Category = "Performance Load", LeftValue = 18, RightValue = 82 }
            });

            // Risk Factor Tornado
            _tornadoData.Clear();
            _tornadoData.AddRange(new[]
            {
                new TornadoVariableItem { Variable = "Exchange Rates", LowValue = -8.5, HighValue = 9.2 },
                new TornadoVariableItem { Variable = "Labor Inflation", LowValue = -6.2, HighValue = 4.8 },
                new TornadoVariableItem { Variable = "Materials Cost", LowValue = -4.5, HighValue = 3.9 },
                new TornadoVariableItem { Variable = "Interest Rates", LowValue = -3.0, HighValue = 2.8 },
                new TornadoVariableItem { Variable = "Shipping Rates", LowValue = -2.1, HighValue = 1.9 }
            });

            // Ring data
            _ringData.Clear();
            _ringData.AddRange(new[]
            {
                new RingItem { Title = "Active Core Utilization", Value = 84 },
                new RingItem { Title = "Memory Buffer Load", Value = 62 },
                new RingItem { Title = "Direct Drawing Pipeline", Value = 45 }
            });

            // Alluvial Flow
            _alluvialNodes.Clear();
            _alluvialNodes.AddRange(new[]
            {
                new AlluvialChart.AlluvialNode { Name = "Landing Page", Stage = 0, Value = 150 },
                new AlluvialChart.AlluvialNode { Name = "Google Ad", Stage = 0, Value = 80 },
                new AlluvialChart.AlluvialNode { Name = "Product Demo", Stage = 1, Value = 130 },
                new AlluvialChart.AlluvialNode { Name = "Pricing Plan", Stage = 1, Value = 100 },
                new AlluvialChart.AlluvialNode { Name = "Trial Sign-up", Stage = 2, Value = 160 },
                new AlluvialChart.AlluvialNode { Name = "Drop-off", Stage = 2, Value = 70 }
            });
            _alluvialLinks.Clear();
            _alluvialLinks.AddRange(new[]
            {
                new AlluvialChart.AlluvialLink { Source = "Landing Page", Target = "Product Demo", Flow = 90 },
                new AlluvialChart.AlluvialLink { Source = "Landing Page", Target = "Pricing Plan", Flow = 60 },
                new AlluvialChart.AlluvialLink { Source = "Google Ad", Target = "Product Demo", Flow = 40 },
                new AlluvialChart.AlluvialLink { Source = "Google Ad", Target = "Pricing Plan", Flow = 40 },
                new AlluvialChart.AlluvialLink { Source = "Product Demo", Target = "Trial Sign-up", Flow = 100 },
                new AlluvialChart.AlluvialLink { Source = "Product Demo", Target = "Drop-off", Flow = 30 },
                new AlluvialChart.AlluvialLink { Source = "Pricing Plan", Target = "Trial Sign-up", Flow = 60 },
                new AlluvialChart.AlluvialLink { Source = "Pricing Plan", Target = "Drop-off", Flow = 40 }
            });

            // Sunburst Hierarchy
            _sunburstRoots.Clear();
            var sunEng = new SunburstChart.SunburstNode { Name = "Engineering", Value = 120 };
            var sunEngFront = new SunburstChart.SunburstNode { Name = "Frontend Team", Value = 50 };
            sunEngFront.Children.Add(new SunburstChart.SunburstNode { Name = "Core UI Devs", Value = 30 });
            sunEngFront.Children.Add(new SunburstChart.SunburstNode { Name = "UX Designers", Value = 20 });
            var sunEngBack = new SunburstChart.SunburstNode { Name = "Backend Team", Value = 70 };
            sunEngBack.Children.Add(new SunburstChart.SunburstNode { Name = "Cloud Services", Value = 45 });
            sunEngBack.Children.Add(new SunburstChart.SunburstNode { Name = "Data Storage", Value = 25 });
            sunEng.Children.Add(sunEngFront);
            sunEng.Children.Add(sunEngBack);

            var sunOps = new SunburstChart.SunburstNode { Name = "Operations", Value = 80 };
            sunOps.Children.Add(new SunburstChart.SunburstNode { Name = "HR & Recruiting", Value = 30 });
            sunOps.Children.Add(new SunburstChart.SunburstNode { Name = "Finance & Legal", Value = 50 });

            _sunburstRoots.Add(sunEng);
            _sunburstRoots.Add(sunOps);
        }

        private void SetupCharts()
        {
            // --- LINE CHART SETUP ---
            LineChart.Series.Clear();
            LineChart.Series.Add(new LineSeries
            {
                Title = "Enterprise Revenue",
                ItemsSource = _lineSeries1Data,
                StrokeThickness = 3,
                ShowMarkers = true,
                MarkerSize = 8
            });
            LineChart.Series.Add(new LineSeries
            {
                Title = "Operating Costs",
                ItemsSource = _lineSeries2Data,
                StrokeThickness = 3,
                ShowMarkers = true,
                MarkerSize = 8
            });

            // --- AREA CHART SETUP ---
            AreaChart.Series.Clear();
            AreaChart.Series.Add(new AreaSeries
            {
                Title = "CPU Utilization",
                ItemsSource = _areaSeries1Data,
                StrokeThickness = 2.0,
                IsSmooth = true,
                ShowMarkers = true,
                MarkerSize = 6
            });
            AreaChart.Series.Add(new AreaSeries
            {
                Title = "RAM Consumption",
                ItemsSource = _areaSeries2Data,
                StrokeThickness = 2.0,
                IsSmooth = true,
                ShowMarkers = true,
                MarkerSize = 6
            });

            // --- BAR CHART SETUP ---
            BarChart.Series.Clear();
            BarChart.Series.Add(new BarSeries
            {
                Title = "Online Sales",
                ItemsSource = _barSeries1Data,
                CornerRadius = 6,
                StrokeThickness = 1
            });
            BarChart.Series.Add(new BarSeries
            {
                Title = "Retail Stores",
                ItemsSource = _barSeries2Data,
                CornerRadius = 6,
                StrokeThickness = 1
            });

            // --- PIE CHART SETUP ---
            PieChartCtrl.Series.Clear();
            PieChartCtrl.Series.Add(new PieSeries { Title = "North America", Value = 45.0, ExplodeDistance = 12 });
            PieChartCtrl.Series.Add(new PieSeries { Title = "Europe Union", Value = 25.0, ExplodeDistance = 12 });
            PieChartCtrl.Series.Add(new PieSeries { Title = "Asia Pacific", Value = 18.0, ExplodeDistance = 12 });
            PieChartCtrl.Series.Add(new PieSeries { Title = "Latin America", Value = 12.0, ExplodeDistance = 12 });

            // --- REAL-TIME CHART SETUP ---
            RealTimeChart.Series.Clear();
            RealTimeChart.Series.Add(new LineSeries
            {
                Title = "Live Telemetry Feed",
                ItemsSource = _rtData,
                StrokeThickness = 2,
                ShowMarkers = false,
                IsSmooth = true
            });

            // --- FINANCIAL CHART SETUP ---
            FinancialChart.Series.Clear();
            FinancialChart.Series.Add(new CandlestickSeries
            {
                Title = "Equity Price Feed",
                ItemsSource = _financialData,
                OpenPath = "Open",
                HighPath = "High",
                LowPath = "Low",
                ClosePath = "Close",
                CategoryPath = "X",
                CandleWidthPercent = 0.7,
                StrokeThickness = 1.5
            });

            // --- HIERARCHY FLOW SETUP ---
            SankeyFlow.Nodes = _sankeyNodes;
            SankeyFlow.Links = _sankeyLinks;
            SankeyFlow.Palette = Palette.Modern;

            TreemapFlow.Items = _treemapItems;
            TreemapFlow.Palette = Palette.Modern;

            // --- STATISTICAL CHART SETUP ---
            StatisticalChart.Series.Clear();
            StatisticalChart.Series.Add(new BoxPlotSeries
            {
                Title = "Whisker Distribution",
                ItemsSource = _statGroups,
                CategoryPath = "Category",
                ValuePath = "Values",
                BoxWidthPercent = 0.5,
                StrokeThickness = 1.5
            });

            // --- ANALYTICS DASHBOARD SETUP ---
            KpiCard1.SparklineData = _sparklineData1;
            KpiCard2.SparklineData = _sparklineData2;
            KpiCard3.SparklineData = _sparklineData3;

            AnalyticsHeatmap.Cells = _heatmapCells;
            AnalyticsHeatmap.XLabels = _heatmapXLabels;
            AnalyticsHeatmap.YLabels = _heatmapYLabels;

            // --- DONUT & SEMI DONUT SETUP ---
            DonutChartCtrl.Series.Clear();
            DonutChartCtrl.Series.Add(new PieSeries { Title = "Active Compute", Value = 55.0 });
            DonutChartCtrl.Series.Add(new PieSeries { Title = "Idle Pool", Value = 30.0 });
            DonutChartCtrl.Series.Add(new PieSeries { Title = "Error Buffer", Value = 15.0 });

            SemiDonutChartCtrl.Series.Clear();
            SemiDonutChartCtrl.Series.Add(new PieSeries { Title = "Used Memory", Value = 72.0 });
            SemiDonutChartCtrl.Series.Add(new PieSeries { Title = "Available Space", Value = 28.0 });

            // --- BAR EXTENSIONS SETUP ---
            WaterfallChartCtrl.ItemsSource = _waterfallData;
            WaterfallChartCtrl.CategoryPath = "Stage";
            WaterfallChartCtrl.ValuePath = "Value";
            WaterfallChartCtrl.IsTotalPath = "IsTotal";

            DivergingBarChartCtrl.ItemsSource = _divergingData;
            DivergingBarChartCtrl.CategoryPath = "Category";
            DivergingBarChartCtrl.LeftValuePath = "LeftValue";
            DivergingBarChartCtrl.RightValuePath = "RightValue";

            TornadoChartCtrl.ItemsSource = _tornadoData;
            TornadoChartCtrl.CategoryPath = "Variable";
            TornadoChartCtrl.LowValuePath = "LowValue";
            TornadoChartCtrl.HighValuePath = "HighValue";
            TornadoChartCtrl.BaseValue = 0.0;

            // --- GAUGES & RINGS EXTENSIONS SETUP ---
            GradRingChart.ItemsSource = _ringData;
            GradRingChart.TitlePath = "Title";
            GradRingChart.ValuePath = "Value";

            ProgDonut.Value = 74.0;
            ProgDonut.Maximum = 100.0;
            ProgDonut.CenterText = "74.0%";
            ProgDonut.CenterSubText = "COMPLETED";

            // --- HIERARCHY FLOW SETUP ---
            AlluvialFlow.Nodes = _alluvialNodes;
            AlluvialFlow.Links = _alluvialLinks;

            SunburstFlow.RootNodes = _sunburstRoots;

            // --- ADVANCED ANALYTICS SETUP ---
            FunnelCtrl.ItemsSource = _funnelData;
            FunnelCtrl.ValuePath = "Amount";
            FunnelCtrl.TitlePath = "Stage";

            WaffleCtrl.ItemsSource = _waffleData;
            WaffleCtrl.ValuePath = "Amount";
            WaffleCtrl.TitlePath = "Stage";

            WordCloudCtrl.ItemsSource = _wordCloudData;
            WordCloudCtrl.TextPath = "Text";
            WordCloudCtrl.WeightPath = "Weight";

            TernaryCtrl.ItemsSource = _ternaryData;
            TernaryCtrl.APath = "ComponentA";
            TernaryCtrl.BPath = "ComponentB";
            TernaryCtrl.CPath = "ComponentC";
        }

        // --- NAVIGATION ROUTINES ---
        private void OnCategoryChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CategoryList == null) return;

            if (CategoryList.SelectedItem is ListViewItem selectedItem && selectedItem.Tag is string tag)
            {
                _activeCategory = tag;

                // Stop active timers if switching away from Real-time
                if (tag != "RealTime" && _rtTimer != null)
                {
                    _rtTimer.Stop();
                    _rtTimer = null;
                    PlayPauseButton.Content = "Start Real-time Telemetry";
                    PlayPauseButton.Background = GetSolidColorBrush("#4F46E5");
                }

                // Hide all view grids
                LineView.Visibility = Visibility.Collapsed;
                AreaView.Visibility = Visibility.Collapsed;
                BarView.Visibility = Visibility.Collapsed;
                PieView.Visibility = Visibility.Collapsed;
                RealTimeView.Visibility = Visibility.Collapsed;
                FinancialView.Visibility = Visibility.Collapsed;
                GaugesView.Visibility = Visibility.Collapsed;
                HierarchyView.Visibility = Visibility.Collapsed;
                StatisticalView.Visibility = Visibility.Collapsed;
                AnalyticsView.Visibility = Visibility.Collapsed;
                AdvancedAnalyticsView.Visibility = Visibility.Collapsed;

                // Hide all config panels
                LineControls.Visibility = Visibility.Collapsed;
                AreaControls.Visibility = Visibility.Collapsed;
                BarControls.Visibility = Visibility.Collapsed;
                PieControls.Visibility = Visibility.Collapsed;
                RealTimeControls.Visibility = Visibility.Collapsed;
                FinancialControls.Visibility = Visibility.Collapsed;
                GaugesControls.Visibility = Visibility.Collapsed;
                HierarchyControls.Visibility = Visibility.Collapsed;
                StatisticalControls.Visibility = Visibility.Collapsed;
                AnalyticsControls.Visibility = Visibility.Collapsed;
                AdvancedAnalyticsControls.Visibility = Visibility.Collapsed;

                switch (tag)
                {
                    case "Line":
                        LineView.Visibility = Visibility.Visible;
                        LineControls.Visibility = Visibility.Visible;
                        _activeChart = LineChart;
                        ActiveCategoryTitle.Text = "Linear & Spline Charts";
                        ActiveCategorySubtitle.Text = "Displays continuous coordinates using high-performance vector rendering, markers, and splines.";
                        break;
                    case "Area":
                        AreaView.Visibility = Visibility.Visible;
                        AreaControls.Visibility = Visibility.Visible;
                        _activeChart = AreaChart;
                        ActiveCategoryTitle.Text = "Area Gradient Charts";
                        ActiveCategorySubtitle.Text = "Displays continuous coordinates with translucent background gradients, creating smooth layered landscapes.";
                        break;
                    case "Bar":
                        BarView.Visibility = Visibility.Visible;
                        BarControls.Visibility = Visibility.Visible;
                        _activeChart = BarChart;
                        ActiveCategoryTitle.Text = "Column & Bar Charts";
                        ActiveCategorySubtitle.Text = "Displays categorical comparisons with clustered vertical columns or horizontal bars and rounded corners.";
                        break;
                    case "Pie":
                        PieView.Visibility = Visibility.Visible;
                        PieControls.Visibility = Visibility.Visible;
                        _activeChart = PieChartCtrl;
                        ActiveCategoryTitle.Text = "Pie & Donut Charts";
                        ActiveCategorySubtitle.Text = "Displays proportional breakdowns in circular coordinates with hollow center configuration and hover slice explosion.";
                        break;
                    case "RealTime":
                        RealTimeView.Visibility = Visibility.Visible;
                        RealTimeControls.Visibility = Visibility.Visible;
                        _activeChart = RealTimeChart;
                        ActiveCategoryTitle.Text = "Real-time Telemetry Stress Test";
                        ActiveCategorySubtitle.Text = "Tests high frequency rendering performance of vectors drawn directly to the drawing context. Multi-axes enabled.";
                        break;
                    case "Financial":
                        FinancialView.Visibility = Visibility.Visible;
                        FinancialControls.Visibility = Visibility.Visible;
                        _activeChart = FinancialChart;
                        ActiveCategoryTitle.Text = "Financial Market Feeds";
                        ActiveCategorySubtitle.Text = "Displays stock market price ranges using interactive candlesticks, tick OHLC points, or high-low Hilo bands.";
                        break;
                    case "Gauges":
                        GaugesView.Visibility = Visibility.Visible;
                        GaugesControls.Visibility = Visibility.Visible;
                        _activeChart = CircGauge;
                        ActiveCategoryTitle.Text = "Premium Status Gauges";
                        ActiveCategorySubtitle.Text = "Interactive meters depicting real-time system metrics, fluid sine waves, and glassmorphic tracks.";
                        break;
                    case "Hierarchy":
                        HierarchyView.Visibility = Visibility.Visible;
                        HierarchyControls.Visibility = Visibility.Visible;
                        _activeChart = SankeyFlow;
                        ActiveCategoryTitle.Text = "Hierarchy & Process Flows";
                        ActiveCategorySubtitle.Text = "Visualizes network flows and recursive area partitions using organic Bezier curves and squarified layouts.";
                        break;
                    case "Statistical":
                        StatisticalView.Visibility = Visibility.Visible;
                        StatisticalControls.Visibility = Visibility.Visible;
                        _activeChart = StatisticalChart;
                        ActiveCategoryTitle.Text = "Statistical Plotting";
                        ActiveCategorySubtitle.Text = "Plots data density, quartiles, and swarms using box whiskers, beeswarm collision-packing, and histograms.";
                        break;
                    case "Analytics":
                        AnalyticsView.Visibility = Visibility.Visible;
                        AnalyticsControls.Visibility = Visibility.Visible;
                        _activeChart = AnalyticsHeatmap;
                        ActiveCategoryTitle.Text = "Executive Analytics Dashboards";
                        ActiveCategorySubtitle.Text = "High-fidelity executive tracking system featuring mini sparklines, responsive categorical grids, and target bands.";
                        break;
                    case "AdvancedAnalytics":
                        AdvancedAnalyticsView.Visibility = Visibility.Visible;
                        AdvancedAnalyticsControls.Visibility = Visibility.Visible;
                        _activeChart = FunnelCtrl;
                        ActiveCategoryTitle.Text = "Advanced Pipeline & WordCloud Analytics";
                        ActiveCategorySubtitle.Text = "Advanced enterprise visualization including segment funnels, word clouds, waffle partitions, and ternary chemical plots.";
                        break;
                }

                // Trigger entry animation on the newly selected chart
                _activeChart?.StartEntryAnimation();
            }
        }


        // --- GLOBAL CHART OPTIONS ---
        private void OnPaletteChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PaletteComboBox == null || _activeChart == null) return;

            if (PaletteComboBox.SelectedItem is ComboBoxItem item && item.Tag is string tag)
            {
                Palette palette = tag switch
                {
                    "Modern" => Palette.Modern,
                    "Emerald" => Palette.Emerald,
                    "SlateDark" => Palette.SlateDark,
                    "Retro" => Palette.Retro,
                    "Cyberpunk" => Palette.Cyberpunk,
                    _ => Palette.Modern
                };

                LineChart.Palette = palette;
                AreaChart.Palette = palette;
                BarChart.Palette = palette;
                PieChartCtrl.Palette = palette;
                RealTimeChart.Palette = palette;
                FinancialChart.Palette = palette;
                CircGauge.Palette = palette;
                LiqGauge.Palette = palette;
                LinGauge.Palette = palette;
                SankeyFlow.Palette = palette;
                TreemapFlow.Palette = palette;
                StatisticalChart.Palette = palette;
                AnalyticsHeatmap.Palette = palette;
                AnalyticsBullet.Palette = palette;

                DonutChartCtrl.Palette = palette;
                SemiDonutChartCtrl.Palette = palette;
                WaterfallChartCtrl.Palette = palette;
                DivergingBarChartCtrl.Palette = palette;
                TornadoChartCtrl.Palette = palette;
                GradRingChart.Palette = palette;
                ProgDonut.Palette = palette;
                AlluvialFlow.Palette = palette;
                SunburstFlow.Palette = palette;
                FunnelCtrl.Palette = palette;
                WaffleCtrl.Palette = palette;
                WordCloudCtrl.Palette = palette;
                TernaryCtrl.Palette = palette;

                _activeChart.InvalidateVisual();
            }
        }

        private void OnEasingChanged(object sender, SelectionChangedEventArgs e)
        {
            if (EasingComboBox == null || _activeChart == null) return;

            if (EasingComboBox.SelectedItem is ComboBoxItem item && item.Tag is string tag)
            {
                if (Enum.TryParse<EasingType>(tag, out var easing))
                {
                    LineChart.Easing = easing;
                    AreaChart.Easing = easing;
                    BarChart.Easing = easing;
                    PieChartCtrl.Easing = easing;
                    RealTimeChart.Easing = easing;
                    FinancialChart.Easing = easing;
                    StatisticalChart.Easing = easing;
                }
            }
        }

        private void OnAnimationDurationChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (AnimationDurationSlider == null || AnimationDurationText == null) return;

            int durationMs = (int)AnimationDurationSlider.Value;
            AnimationDurationText.Text = $"{durationMs}ms";

            TimeSpan duration = TimeSpan.FromMilliseconds(durationMs);
            if (LineChart != null) LineChart.AnimationDuration = duration;
            if (AreaChart != null) AreaChart.AnimationDuration = duration;
            if (BarChart != null) BarChart.AnimationDuration = duration;
            if (PieChartCtrl != null) PieChartCtrl.AnimationDuration = duration;
            if (RealTimeChart != null) RealTimeChart.AnimationDuration = duration;
            if (FinancialChart != null) FinancialChart.AnimationDuration = duration;
            if (StatisticalChart != null) StatisticalChart.AnimationDuration = duration;
        }

        private void OnAnimationChecked(object sender, RoutedEventArgs e)
        {
            if (EnableAnimationCheckBox == null) return;

            bool enabled = EnableAnimationCheckBox.IsChecked == true;
            if (LineChart != null) LineChart.IsAnimationEnabled = enabled;
            if (AreaChart != null) AreaChart.IsAnimationEnabled = enabled;
            if (BarChart != null) BarChart.IsAnimationEnabled = enabled;
            if (PieChartCtrl != null) PieChartCtrl.IsAnimationEnabled = enabled;
            if (RealTimeChart != null) RealTimeChart.IsAnimationEnabled = enabled;
            if (FinancialChart != null) FinancialChart.IsAnimationEnabled = enabled;
            if (StatisticalChart != null) StatisticalChart.IsAnimationEnabled = enabled;
        }

        // --- LINE CHART OPTIONS ---
        private void OnLineSmoothChanged(object sender, RoutedEventArgs e)
        {
            if (LineSmoothCheckBox == null) return;
            foreach (var series in LineChart.Series.OfType<LineSeries>())
            {
                series.IsSmooth = LineSmoothCheckBox.IsChecked == true;
            }
        }

        private void OnLineShowMarkersChanged(object sender, RoutedEventArgs e)
        {
            if (LineShowMarkersCheckBox == null) return;
            foreach (var series in LineChart.Series.OfType<LineSeries>())
            {
                series.ShowMarkers = LineShowMarkersCheckBox.IsChecked == true;
            }
        }

        private void OnLineMarkerSizeChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (LineMarkerSizeSlider == null || LineMarkerSizeText == null) return;

            int size = (int)LineMarkerSizeSlider.Value;
            LineMarkerSizeText.Text = $"{size}px";

            foreach (var series in LineChart.Series.OfType<LineSeries>())
            {
                series.MarkerSize = size;
            }
        }

        private void OnLineThicknessChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (LineThicknessSlider == null || LineThicknessText == null) return;

            int thickness = (int)LineThicknessSlider.Value;
            LineThicknessText.Text = $"{thickness}px";

            foreach (var series in LineChart.Series.OfType<LineSeries>())
            {
                series.StrokeThickness = thickness;
            }
        }

        private void OnLineEmptyModeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LineEmptyModeComboBox == null) return;
            if (LineEmptyModeComboBox.SelectedItem is ComboBoxItem item && item.Tag is string tag)
            {
                if (Enum.TryParse<EmptyPointMode>(tag, out var mode))
                {
                    foreach (var series in LineChart.Series.OfType<LineSeries>())
                    {
                        series.EmptyPointMode = mode;
                    }
                }
            }
        }

        // --- AREA CHART OPTIONS ---
        private void OnAreaSmoothChanged(object sender, RoutedEventArgs e)
        {
            if (AreaSmoothCheckBox == null) return;
            foreach (var series in AreaChart.Series.OfType<AreaSeries>())
            {
                series.IsSmooth = AreaSmoothCheckBox.IsChecked == true;
            }
        }

        private void OnAreaShowMarkersChanged(object sender, RoutedEventArgs e)
        {
            if (AreaShowMarkersCheckBox == null) return;
            foreach (var series in AreaChart.Series.OfType<AreaSeries>())
            {
                series.ShowMarkers = AreaShowMarkersCheckBox.IsChecked == true;
            }
        }

        private void OnAreaThicknessChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (AreaThicknessSlider == null || AreaThicknessText == null) return;

            int thickness = (int)AreaThicknessSlider.Value;
            AreaThicknessText.Text = $"{thickness}px";

            foreach (var series in AreaChart.Series.OfType<AreaSeries>())
            {
                series.StrokeThickness = thickness;
            }
        }

        // --- BAR CHART OPTIONS ---
        private void OnBarHorizontalChanged(object sender, RoutedEventArgs e)
        {
            if (BarHorizontalCheckBox == null) return;
            bool isHorizontal = BarHorizontalCheckBox.IsChecked == true;

            foreach (var series in BarChart.Series.OfType<BarSeries>())
            {
                series.IsHorizontal = isHorizontal;
            }

            // Adjust axes titles based on orientation
            if (BarChart.XAxis != null && BarChart.YAxis != null)
            {
                if (isHorizontal)
                {
                    BarChart.XAxis.Title = "Sales Volume";
                    BarChart.YAxis.Title = "Product Categories";
                }
                else
                {
                    BarChart.XAxis.Title = "Product Categories";
                    BarChart.YAxis.Title = "Sales Volume";
                }
            }
        }

        private void OnBarRadiusChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (BarRadiusSlider == null || BarRadiusText == null) return;

            int radius = (int)BarRadiusSlider.Value;
            BarRadiusText.Text = $"{radius}px";

            foreach (var series in BarChart.Series.OfType<BarSeries>())
            {
                series.CornerRadius = radius;
            }
        }

        // --- PIE CHART OPTIONS ---
        private void OnPieHollowChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (PieHollowSlider == null || PieHollowText == null || PieChartCtrl == null) return;

            double hollow = PieHollowSlider.Value;
            PieHollowText.Text = $"{(int)(hollow * 100)}%";

            PieChartCtrl.HollowRadius = hollow;
        }

        private void OnPieExplodeChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (PieExplodeSlider == null || PieExplodeText == null) return;

            int explode = (int)PieExplodeSlider.Value;
            PieExplodeText.Text = $"{explode}px";

            foreach (var series in PieChartCtrl.Series)
            {
                series.ExplodeDistance = explode;
            }
        }

        // --- REAL-TIME TELEMETRY DRIVER ---
        private void OnToggleRealTime(object sender, RoutedEventArgs e)
        {
            if (_rtTimer == null)
            {
                _rtTimer = new DispatcherTimer();
                _rtTimer.Interval = TimeSpan.FromMilliseconds(RtIntervalSlider.Value);
                _rtTimer.Tick += (s, ev) => OnRtTimerTick();
                _rtTimer.Start();
                PlayPauseButton.Content = "Stop Telemetry Stream";
                PlayPauseButton.Background = GetSolidColorBrush("#EF4444"); // Red highlight
            }
            else
            {
                _rtTimer.Stop();
                _rtTimer = null;
                PlayPauseButton.Content = "Start Real-time Telemetry";
                PlayPauseButton.Background = GetSolidColorBrush("#4F46E5"); // Standard blue
            }
        }

        private void OnRtIntervalChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (RtIntervalSlider == null || RtIntervalText == null) return;

            int interval = (int)RtIntervalSlider.Value;
            RtIntervalText.Text = $"{interval}ms";

            if (_rtTimer != null)
            {
                _rtTimer.Interval = TimeSpan.FromMilliseconds(interval);
            }
        }

        private void OnRtDensityChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RtDensityComboBox == null) return;
            if (RtDensityComboBox.SelectedItem is ComboBoxItem item && item.Tag is string tag)
            {
                if (int.TryParse(tag, out var density))
                {
                    _rtMaxPoints = density;
                    
                    // Re-seed data to fit the density
                    _rtData.Clear();
                    _rtTimeIndex = 0;
                    for (int i = 0; i < density; i++)
                    {
                        _rtTimeIndex = i;
                        double val = 50 + 40 * Math.Sin(i * 0.15) + 15 * Math.Cos(i * 0.4);
                        _rtData.Add(new Point(_rtTimeIndex, val));
                    }
                    if (RealTimeChart != null) RealTimeChart.InvalidateVisual();
                }
            }
        }

        private void OnRtTimerTick()
        {
            _rtTimeIndex++;
            
            // Generate chaotic but continuous wave equations
            double noise = (Random.Shared.NextDouble() - 0.5) * 15;
            double baseWave = 50 + 60 * Math.Sin(_rtTimeIndex * 0.12) + 20 * Math.Cos(_rtTimeIndex * 0.35);
            double val = baseWave + noise;

            _rtData.Add(new Point(_rtTimeIndex, val));

            if (_rtData.Count > _rtMaxPoints)
            {
                _rtData.RemoveAt(0);
            }

            // Force visual invalidation
            RealTimeChart.InvalidateVisual();
        }

        // --- NEW INTERACTIVE CHART HANDLERS ---

        // Financial
        private void OnFinancialModeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FinancialModeComboBox == null || FinancialChart == null) return;
            if (FinancialModeComboBox.SelectedItem is ComboBoxItem item && item.Tag is string tag)
            {
                FinancialChart.Series.Clear();
                double candleWidth = FinancialWidthSlider?.Value ?? 0.7;

                if (tag == "Candlestick")
                {
                    FinancialChart.Series.Add(new CandlestickSeries
                    {
                        Title = "Equity Price Feed",
                        ItemsSource = _financialData,
                        OpenPath = "Open",
                        HighPath = "High",
                        LowPath = "Low",
                        ClosePath = "Close",
                        CategoryPath = "X",
                        CandleWidthPercent = candleWidth,
                        StrokeThickness = 1.5
                    });
                }
                else if (tag == "Ohlc")
                {
                    FinancialChart.Series.Add(new OhlcSeries
                    {
                        Title = "Equity Price Feed",
                        ItemsSource = _financialData,
                        OpenPath = "Open",
                        HighPath = "High",
                        LowPath = "Low",
                        ClosePath = "Close",
                        CategoryPath = "X",
                        StrokeThickness = 1.5
                    });
                }
                else if (tag == "Hilo")
                {
                    FinancialChart.Series.Add(new HiloSeries
                    {
                        Title = "Equity Price Feed",
                        ItemsSource = _financialData,
                        HighPath = "High",
                        LowPath = "Low",
                        CategoryPath = "X",
                        StrokeThickness = 1.5
                    });
                }

                FinancialChart.StartEntryAnimation();
            }
        }

        private void OnFinancialWidthChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (FinancialWidthSlider == null || FinancialWidthText == null || FinancialChart == null) return;

            double width = FinancialWidthSlider.Value;
            FinancialWidthText.Text = $"{width:F1}";

            foreach (var series in FinancialChart.Series.OfType<CandlestickSeries>())
            {
                series.CandleWidthPercent = width;
            }
            FinancialChart.InvalidateVisual();
        }

        // Gauges
        private void OnGaugeValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (GaugeValueSlider == null || GaugeValueText == null) return;

            double val = GaugeValueSlider.Value;
            GaugeValueText.Text = $"{(int)val}%";

            if (CircGauge != null) CircGauge.Value = val;
            if (LiqGauge != null) LiqGauge.Value = val;
            if (LinGauge != null) LinGauge.Value = val * 2.0; // scaled to 0-200 PSI max
        }

        private void OnGaugeAmplitudeChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (GaugeAmplitudeSlider == null || GaugeAmplitudeText == null) return;

            int amp = (int)GaugeAmplitudeSlider.Value;
            GaugeAmplitudeText.Text = $"{amp}px";

            if (LiqGauge != null) LiqGauge.WaveAmplitude = amp;
        }

        private void OnGaugeThicknessChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (GaugeThicknessSlider == null || GaugeThicknessText == null) return;

            int thickness = (int)GaugeThicknessSlider.Value;
            GaugeThicknessText.Text = $"{thickness}px";

            if (CircGauge != null) CircGauge.GaugeThickness = thickness;
            if (LinGauge != null) LinGauge.GaugeThickness = thickness;
        }

        // Hierarchy
        private void OnScrambleHierarchy(object sender, RoutedEventArgs e)
        {
            var rand = Random.Shared;
            foreach (var link in _sankeyLinks)
            {
                link.Flow = 10.0 + rand.NextDouble() * 45.0;
            }
            foreach (var item in _treemapItems)
            {
                item.Value = 4.0 + rand.NextDouble() * 50.0;
            }

            SankeyFlow?.InvalidateVisual();
            TreemapFlow?.InvalidateVisual();

            SankeyFlow?.StartEntryAnimation();
            TreemapFlow?.StartEntryAnimation();
        }

        // Statistical
        private void OnStatisticalModeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (StatisticalModeComboBox == null || StatisticalChart == null) return;
            if (StatisticalModeComboBox.SelectedItem is ComboBoxItem item && item.Tag is string tag)
            {
                StatisticalChart.Series.Clear();

                if (tag == "BoxPlot")
                {
                    StatisticalChart.Series.Add(new BoxPlotSeries
                    {
                        Title = "Whisker Distribution",
                        ItemsSource = _statGroups,
                        CategoryPath = "Category",
                        ValuePath = "Values",
                        BoxWidthPercent = 0.5,
                        StrokeThickness = 1.5
                    });
                }
                else if (tag == "Beeswarm")
                {
                    var beeswarmData = new ObservableCollection<Point>();
                    foreach (var group in _statGroups)
                    {
                        foreach (var val in group.Values)
                        {
                            beeswarmData.Add(new Point(group.Category, val));
                        }
                    }

                    StatisticalChart.Series.Add(new BeeswarmPlotSeries
                    {
                        Title = "Data Points Swarm",
                        ItemsSource = beeswarmData,
                        CircleRadius = 5.0
                    });
                }
                else if (tag == "Histogram")
                {
                    StatisticalChart.Series.Add(new HistogramSeries
                    {
                        Title = "Frequency Distribution",
                        ItemsSource = _histogramRawValues,
                        BinCount = 12,
                        StrokeThickness = 1.0
                    });
                }
                else if (tag == "Violin")
                {
                    StatisticalChart.Series.Add(new ViolinPlotSeries
                    {
                        Title = "Density Violin Distribution",
                        ItemsSource = _statGroups,
                        CategoryPath = "Category",
                        ValuePath = "Values",
                        ViolinWidthPercent = 0.7,
                        StrokeThickness = 1.5
                    });
                }

                StatisticalChart.StartEntryAnimation();
            }
        }

        private void OnResampleStatistical(object sender, RoutedEventArgs e)
        {
            var rand = Random.Shared;
            foreach (var group in _statGroups)
            {
                group.Values.Clear();
                double mean = 80.0 + rand.NextDouble() * 40.0;
                double stdDev = 10.0 + rand.NextDouble() * 12.0;

                for (int i = 0; i < 40; i++)
                {
                    double u1 = 1.0 - rand.NextDouble();
                    double u2 = 1.0 - rand.NextDouble();
                    double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
                    group.Values.Add(mean + stdDev * randStdNormal);
                }

                group.Values.Add(mean - stdDev * 3.7);
                group.Values.Add(mean + stdDev * 3.9);
            }

            _histogramRawValues.Clear();
            for (int i = 0; i < 180; i++)
            {
                double u1 = 1.0 - rand.NextDouble();
                double u2 = 1.0 - rand.NextDouble();
                double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
                
                if (rand.NextDouble() < 0.5)
                {
                    _histogramRawValues.Add(65.0 + 13.0 * randStdNormal);
                }
                else
                {
                    _histogramRawValues.Add(125.0 + 17.0 * randStdNormal);
                }
            }

            OnStatisticalModeChanged(this, null!);
        }

        // Analytics
        private void OnAnalyticsKpiChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (AnalyticsKpiSlider == null || AnalyticsKpiText == null || KpiCard2 == null) return;

            int val = (int)AnalyticsKpiSlider.Value;
            AnalyticsKpiText.Text = $"{val}%";

            KpiCard2.ValueString = $"{val}.0%";
            KpiCard2.InvalidateVisual();
        }

        private void OnAnalyticsBulletChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (AnalyticsBulletSlider == null || AnalyticsBulletText == null || AnalyticsBullet == null) return;

            int val = (int)AnalyticsBulletSlider.Value;
            AnalyticsBulletText.Text = $"{val}";

            AnalyticsBullet.Value = val;
        }

        private void OnSimulateAnalytics(object sender, RoutedEventArgs e)
        {
            var rand = Random.Shared;
            
            KpiCard1.ValueString = $"${76000 + rand.Next(16000):N0}";
            KpiCard1.TrendValue = 7.0 + rand.NextDouble() * 9.0;

            KpiCard3.ValueString = $"{1.1 + rand.NextDouble() * 1.3:F2}%";
            KpiCard3.TrendValue = -13.0 + rand.NextDouble() * 7.0;

            foreach (var cell in _heatmapCells)
            {
                double loadBase = 10.0 + (cell.X * 4.0) + (cell.Y * 6.0);
                if (cell.Y == 2 || cell.Y == 3 || cell.Y == 4) loadBase += 40.0;
                cell.Value = loadBase + rand.NextDouble() * 25.0;
            }

            AnalyticsBullet.Target = 112.0 + rand.NextDouble() * 18.0;

            KpiCard1.StartEntryAnimation();
            KpiCard2.StartEntryAnimation();
            KpiCard3.StartEntryAnimation();
            AnalyticsHeatmap.StartEntryAnimation();
            AnalyticsBullet.StartEntryAnimation();
        }

        // --- BUTTON ACTIONS ---
        private void OnTriggerAnimation(object sender, RoutedEventArgs e)
        {
            if (_activeCategory == "Financial")
            {
                FinancialChart?.StartEntryAnimation();
            }
            else if (_activeCategory == "Gauges")
            {
                CircGauge?.StartEntryAnimation();
                LiqGauge?.StartEntryAnimation();
                LinGauge?.StartEntryAnimation();
                ProgDonut?.StartEntryAnimation();
                GradRingChart?.StartEntryAnimation();
            }
            else if (_activeCategory == "Hierarchy")
            {
                SankeyFlow?.StartEntryAnimation();
                TreemapFlow?.StartEntryAnimation();
                AlluvialFlow?.StartEntryAnimation();
                SunburstFlow?.StartEntryAnimation();
            }
            else if (_activeCategory == "Statistical")
            {
                StatisticalChart?.StartEntryAnimation();
            }
            else if (_activeCategory == "Analytics")
            {
                KpiCard1?.StartEntryAnimation();
                KpiCard2?.StartEntryAnimation();
                KpiCard3?.StartEntryAnimation();
                AnalyticsHeatmap?.StartEntryAnimation();
                AnalyticsBullet?.StartEntryAnimation();
            }
            else if (_activeCategory == "AdvancedAnalytics")
            {
                FunnelCtrl?.StartEntryAnimation();
                WaffleCtrl?.StartEntryAnimation();
                WordCloudCtrl?.StartEntryAnimation();
                TernaryCtrl?.StartEntryAnimation();
            }
            else if (_activeChart != null)
            {
                _activeChart.StartEntryAnimation();
            }
        }

        private void OnInjectNoise(object sender, RoutedEventArgs e)
        {
            var rand = Random.Shared;

            if (_activeCategory == "Line")
            {
                for (int i = 0; i < _lineSeries1Data.Count; i++)
                {
                    double delta1 = (rand.NextDouble() - 0.5) * 60;
                    _lineSeries1Data[i] = new Point(i, Math.Clamp(_lineSeries1Data[i].Y + delta1, 20, 400));

                    if (!double.IsNaN(_lineSeries2Data[i].Y))
                    {
                        double delta2 = (rand.NextDouble() - 0.5) * 40;
                        _lineSeries2Data[i] = new Point(i, Math.Clamp(_lineSeries2Data[i].Y + delta2, 10, 300));
                    }
                }
                for (int i = 0; i < _bubbleSeries1Data.Count; i++)
                {
                    _bubbleSeries1Data[i].Y = Math.Clamp(_bubbleSeries1Data[i].Y + (rand.NextDouble() - 0.5) * 30, 20, 400);
                    _bubbleSeries1Data[i].Size = Math.Clamp(_bubbleSeries1Data[i].Size + (rand.NextDouble() - 0.5) * 15, 10, 100);
                }
                for (int i = 0; i < _bubbleSeries2Data.Count; i++)
                {
                    _bubbleSeries2Data[i].Y = Math.Clamp(_bubbleSeries2Data[i].Y + (rand.NextDouble() - 0.5) * 30, 20, 400);
                    _bubbleSeries2Data[i].Size = Math.Clamp(_bubbleSeries2Data[i].Size + (rand.NextDouble() - 0.5) * 15, 10, 100);
                }
                LineChart?.StartEntryAnimation();
            }
            else if (_activeCategory == "Area")
            {
                for (int i = 0; i < _areaSeries1Data.Count; i++)
                {
                    double delta1 = (rand.NextDouble() - 0.5) * 15;
                    _areaSeries1Data[i] = new Point(i, Math.Clamp(_areaSeries1Data[i].Y + delta1, 10, 95));

                    double delta2 = (rand.NextDouble() - 0.5) * 10;
                    _areaSeries2Data[i] = new Point(i, Math.Clamp(_areaSeries2Data[i].Y + delta2, 5, 80));
                }
                AreaChart?.StartEntryAnimation();
            }
            else if (_activeCategory == "Bar")
            {
                for (int i = 0; i < _barSeries1Data.Count; i++)
                {
                    double delta1 = (rand.NextDouble() - 0.5) * 80;
                    _barSeries1Data[i] = new Point(i, Math.Clamp(_barSeries1Data[i].Y + delta1, 50, 500));

                    double delta2 = (rand.NextDouble() - 0.5) * 60;
                    _barSeries2Data[i] = new Point(i, Math.Clamp(_barSeries2Data[i].Y + delta2, 40, 450));
                }
                foreach (var item in _waterfallData)
                {
                    if (!item.IsTotal)
                    {
                        item.Value = Math.Clamp(item.Value + (rand.NextDouble() - 0.5) * 10000, -50000, 200000);
                    }
                }
                foreach (var item in _divergingData)
                {
                    item.LeftValue = Math.Clamp(item.LeftValue + (rand.NextDouble() - 0.5) * 15, 5, 95);
                    item.RightValue = Math.Clamp(item.RightValue + (rand.NextDouble() - 0.5) * 15, 5, 95);
                }
                foreach (var item in _tornadoData)
                {
                    item.LowValue = Math.Clamp(item.LowValue + (rand.NextDouble() - 0.5) * 3, -15.0, 0.0);
                    item.HighValue = Math.Clamp(item.HighValue + (rand.NextDouble() - 0.5) * 3, 0.0, 15.0);
                }
                BarChart?.StartEntryAnimation();
                WaterfallChartCtrl?.StartEntryAnimation();
                DivergingBarChartCtrl?.StartEntryAnimation();
                TornadoChartCtrl?.StartEntryAnimation();
            }
            else if (_activeCategory == "Pie")
            {
                foreach (var series in PieChartCtrl.Series)
                {
                    series.Value = Math.Clamp(series.Value + (rand.NextDouble() - 0.5) * 15, 5, 100);
                }
                foreach (var series in DonutChartCtrl.Series)
                {
                    series.Value = Math.Clamp(series.Value + (rand.NextDouble() - 0.5) * 15, 5, 100);
                }
                foreach (var series in SemiDonutChartCtrl.Series)
                {
                    series.Value = Math.Clamp(series.Value + (rand.NextDouble() - 0.5) * 15, 5, 100);
                }
                PieChartCtrl?.StartEntryAnimation();
                DonutChartCtrl?.StartEntryAnimation();
                SemiDonutChartCtrl?.StartEntryAnimation();
            }
            else if (_activeCategory == "RealTime")
            {
                double spike = (rand.NextDouble() > 0.5 ? 1.0 : -1.0) * (80 + rand.NextDouble() * 70);
                int lastIdx = _rtData.Count - 1;
                if (lastIdx >= 0)
                {
                    _rtData[lastIdx] = new Point(_rtData[lastIdx].X, _rtData[lastIdx].Y + spike);
                }
                RealTimeChart?.InvalidateVisual();
            }
            else if (_activeCategory == "Financial")
            {
                for (int i = 0; i < _financialData.Count; i++)
                {
                    var fp = _financialData[i];
                    double change = (rand.NextDouble() - 0.5) * 10.0;
                    fp.Open = Math.Clamp(fp.Open + change, 50.0, 300.0);
                    fp.Close = Math.Clamp(fp.Close + change, 50.0, 300.0);
                    fp.High = Math.Max(fp.Open, fp.Close) + rand.NextDouble() * 5.0;
                    fp.Low = Math.Min(fp.Open, fp.Close) - rand.NextDouble() * 5.0;
                    _financialData[i] = fp;
                }
                FinancialChart?.StartEntryAnimation();
            }
            else if (_activeCategory == "Gauges")
            {
                double change = (rand.NextDouble() - 0.5) * 20.0;
                double newVal = Math.Clamp(CircGauge.Value + change, 0.0, 100.0);
                CircGauge.Value = newVal;
                LiqGauge.Value = newVal;
                LinGauge.Value = newVal * 2.0;
                ProgDonut.Value = newVal;
                foreach (var item in _ringData)
                {
                    item.Value = Math.Clamp(item.Value + (rand.NextDouble() - 0.5) * 15, 10, 100);
                }
                if (GaugeValueSlider != null) GaugeValueSlider.Value = newVal;
                CircGauge?.StartEntryAnimation();
                LiqGauge?.StartEntryAnimation();
                LinGauge?.StartEntryAnimation();
                ProgDonut?.StartEntryAnimation();
                GradRingChart?.InvalidateVisual();
            }
            else if (_activeCategory == "Hierarchy")
            {
                OnScrambleHierarchy(this, null!);
                foreach (var node in _alluvialNodes)
                {
                    node.Value = Math.Clamp(node.Value + (rand.NextDouble() - 0.5) * 30, 20, 250);
                }
                foreach (var link in _alluvialLinks)
                {
                    link.Flow = Math.Clamp(link.Flow + (rand.NextDouble() - 0.5) * 20, 10, 150);
                }
                RandomizeSunburst(_sunburstRoots, rand);

                AlluvialFlow?.InvalidateVisual();
                SunburstFlow?.InvalidateVisual();
                AlluvialFlow?.StartEntryAnimation();
                SunburstFlow?.StartEntryAnimation();
            }
            else if (_activeCategory == "Statistical")
            {
                OnResampleStatistical(this, null!);
            }
            else if (_activeCategory == "Analytics")
            {
                OnSimulateAnalytics(this, null!);
            }
            else if (_activeCategory == "AdvancedAnalytics")
            {
                OnSimulateAdvancedAnalytics(this, null!);
            }
        }

        private void RandomizeSunburst(IEnumerable<SunburstChart.SunburstNode> nodes, Random rand)
        {
            foreach (var node in nodes)
            {
                node.Value = Math.Clamp(node.Value + (rand.NextDouble() - 0.5) * 25, 10, 200);
                if (node.Children.Count > 0)
                {
                    RandomizeSunburst(node.Children, rand);
                }
            }
        }

        // --- LINE CHART OPTIONS ---
        private void OnLineSeriesTypeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LineSeriesTypeComboBox == null || LineChart == null) return;
            if (LineSeriesTypeComboBox.SelectedItem is ComboBoxItem item && item.Tag is string tag)
            {
                LineChart.Series.Clear();
                double thickness = LineThicknessSlider?.Value ?? 3;
                bool showMarkers = LineShowMarkersCheckBox?.IsChecked == true;
                double markerSize = LineMarkerSizeSlider?.Value ?? 8;
                bool smooth = LineSmoothCheckBox?.IsChecked == true;

                if (tag == "Line")
                {
                    LineChart.Series.Add(new LineSeries
                    {
                        Title = "Enterprise Revenue",
                        ItemsSource = _lineSeries1Data,
                        StrokeThickness = thickness,
                        ShowMarkers = showMarkers,
                        MarkerSize = markerSize,
                        IsSmooth = smooth
                    });
                    LineChart.Series.Add(new LineSeries
                    {
                        Title = "Operating Costs",
                        ItemsSource = _lineSeries2Data,
                        StrokeThickness = thickness,
                        ShowMarkers = showMarkers,
                        MarkerSize = markerSize,
                        IsSmooth = smooth
                    });
                }
                else if (tag == "StepLine")
                {
                    LineChart.Series.Add(new StepLineSeries
                    {
                        Title = "Enterprise Revenue",
                        ItemsSource = _lineSeries1Data,
                        StrokeThickness = thickness,
                        StepBefore = false
                    });
                    LineChart.Series.Add(new StepLineSeries
                    {
                        Title = "Operating Costs",
                        ItemsSource = _lineSeries2Data,
                        StrokeThickness = thickness,
                        StepBefore = false
                    });
                }
                else if (tag == "Scatter")
                {
                    LineChart.Series.Add(new ScatterSeries
                    {
                        Title = "Enterprise Revenue",
                        ItemsSource = _lineSeries1Data,
                        StrokeThickness = thickness,
                        Size = markerSize
                    });
                    LineChart.Series.Add(new ScatterSeries
                    {
                        Title = "Operating Costs",
                        ItemsSource = _lineSeries2Data,
                        StrokeThickness = thickness,
                        Size = markerSize
                    });
                }
                else if (tag == "Bubble")
                {
                    LineChart.Series.Add(new BubbleSeries
                    {
                        Title = "Market Size Revenue",
                        ItemsSource = _bubbleSeries1Data,
                        CategoryPath = "X",
                        ValuePath = "Y",
                        SizePath = "Size",
                        StrokeThickness = thickness
                    });
                    LineChart.Series.Add(new BubbleSeries
                    {
                        Title = "Market Size Costs",
                        ItemsSource = _bubbleSeries2Data,
                        CategoryPath = "X",
                        ValuePath = "Y",
                        SizePath = "Size",
                        StrokeThickness = thickness
                    });
                }
                LineChart.StartEntryAnimation();
            }
        }

        private void OnAreaModeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AreaModeComboBox == null || AreaChart == null) return;
            if (AreaModeComboBox.SelectedItem is ComboBoxItem item && item.Tag is string tag)
            {
                AreaChart.Series.Clear();
                double thickness = AreaThicknessSlider?.Value ?? 2.0;
                bool smooth = AreaSmoothCheckBox?.IsChecked == true;
                bool showMarkers = AreaShowMarkersCheckBox?.IsChecked == true;

                if (tag == "Area")
                {
                    AreaChart.Series.Add(new AreaSeries
                    {
                        Title = "CPU Utilization",
                        ItemsSource = _areaSeries1Data,
                        StrokeThickness = thickness,
                        IsSmooth = smooth,
                        ShowMarkers = showMarkers,
                        MarkerSize = 6
                    });
                    AreaChart.Series.Add(new AreaSeries
                    {
                        Title = "RAM Consumption",
                        ItemsSource = _areaSeries2Data,
                        StrokeThickness = thickness,
                        IsSmooth = smooth,
                        ShowMarkers = showMarkers,
                        MarkerSize = 6
                    });
                }
                else if (tag == "StackedArea")
                {
                    AreaChart.Series.Add(new StackedAreaSeries
                    {
                        Title = "CPU Utilization Stack",
                        ItemsSource = _areaSeries1Data,
                        StrokeThickness = thickness,
                        IsSmooth = smooth
                    });
                    AreaChart.Series.Add(new StackedAreaSeries
                    {
                        Title = "RAM Consumption Stack",
                        ItemsSource = _areaSeries2Data,
                        StrokeThickness = thickness,
                        IsSmooth = smooth
                    });
                }
                AreaChart.StartEntryAnimation();
            }
        }

        private void OnBarModeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BarModeComboBox == null || BarChart == null || WaterfallChartCtrl == null || DivergingBarChartCtrl == null || TornadoChartCtrl == null) return;
            if (BarModeComboBox.SelectedItem is ComboBoxItem item && item.Tag is string tag)
            {
                BarChart.Visibility = Visibility.Collapsed;
                WaterfallChartCtrl.Visibility = Visibility.Collapsed;
                DivergingBarChartCtrl.Visibility = Visibility.Collapsed;
                TornadoChartCtrl.Visibility = Visibility.Collapsed;

                double radius = BarRadiusSlider?.Value ?? 6.0;
                bool isHorizontal = BarHorizontalCheckBox?.IsChecked == true;

                if (tag == "Bar")
                {
                    BarChart.Visibility = Visibility.Visible;
                    BarChart.Series.Clear();
                    BarChart.Series.Add(new BarSeries
                    {
                        Title = "Online Sales",
                        ItemsSource = _barSeries1Data,
                        CornerRadius = radius,
                        IsHorizontal = isHorizontal,
                        StrokeThickness = 1
                    });
                    BarChart.Series.Add(new BarSeries
                    {
                        Title = "Retail Stores",
                        ItemsSource = _barSeries2Data,
                        CornerRadius = radius,
                        IsHorizontal = isHorizontal,
                        StrokeThickness = 1
                    });
                    _activeChart = BarChart;
                    BarChart.StartEntryAnimation();
                }
                else if (tag == "StackedBar")
                {
                    BarChart.Visibility = Visibility.Visible;
                    BarChart.Series.Clear();
                    BarChart.Series.Add(new StackedBarSeries
                    {
                        Title = "Online Sales (Stack)",
                        ItemsSource = _barSeries1Data,
                        CornerRadius = radius,
                        IsHorizontal = isHorizontal,
                        StrokeThickness = 1
                    });
                    BarChart.Series.Add(new StackedBarSeries
                    {
                        Title = "Retail Stores (Stack)",
                        ItemsSource = _barSeries2Data,
                        CornerRadius = radius,
                        IsHorizontal = isHorizontal,
                        StrokeThickness = 1
                    });
                    _activeChart = BarChart;
                    BarChart.StartEntryAnimation();
                }
                else if (tag == "Waterfall")
                {
                    WaterfallChartCtrl.Visibility = Visibility.Visible;
                    _activeChart = WaterfallChartCtrl;
                    WaterfallChartCtrl.StartEntryAnimation();
                }
                else if (tag == "Diverging")
                {
                    DivergingBarChartCtrl.Visibility = Visibility.Visible;
                    _activeChart = DivergingBarChartCtrl;
                    DivergingBarChartCtrl.StartEntryAnimation();
                }
                else if (tag == "Tornado")
                {
                    TornadoChartCtrl.Visibility = Visibility.Visible;
                    _activeChart = TornadoChartCtrl;
                    TornadoChartCtrl.StartEntryAnimation();
                }
            }
        }

        private void OnPieModeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PieModeComboBox == null || PieChartCtrl == null || DonutChartCtrl == null || SemiDonutChartCtrl == null) return;
            if (PieModeComboBox.SelectedItem is ComboBoxItem item && item.Tag is string tag)
            {
                PieChartCtrl.Visibility = Visibility.Collapsed;
                DonutChartCtrl.Visibility = Visibility.Collapsed;
                SemiDonutChartCtrl.Visibility = Visibility.Collapsed;

                double hollow = PieHollowSlider?.Value ?? 0.5;

                if (tag == "Pie")
                {
                    PieChartCtrl.Visibility = Visibility.Visible;
                    PieChartCtrl.HollowRadius = hollow;
                    _activeChart = PieChartCtrl;
                    PieChartCtrl.StartEntryAnimation();
                }
                else if (tag == "Donut")
                {
                    DonutChartCtrl.Visibility = Visibility.Visible;
                    DonutChartCtrl.HollowRadius = hollow > 0 ? hollow : 0.65;
                    _activeChart = DonutChartCtrl;
                    DonutChartCtrl.StartEntryAnimation();
                }
                else if (tag == "SemiDonut")
                {
                    SemiDonutChartCtrl.Visibility = Visibility.Visible;
                    SemiDonutChartCtrl.HollowRadius = hollow > 0 ? hollow : 0.65;
                    _activeChart = SemiDonutChartCtrl;
                    SemiDonutChartCtrl.StartEntryAnimation();
                }
            }
        }

        private void OnSimulateAdvancedAnalytics(object sender, RoutedEventArgs e)
        {
            var rand = Random.Shared;

            foreach (var item in _funnelData)
            {
                item.Amount = Math.Clamp(item.Amount + (rand.NextDouble() - 0.45) * 2000, 1000, 20000);
            }

            foreach (var item in _waffleData)
            {
                item.Amount = Math.Clamp(item.Amount + (rand.NextDouble() - 0.5) * 10, 5, 80);
            }

            foreach (var item in _wordCloudData)
            {
                item.Weight = Math.Clamp(item.Weight + (rand.NextDouble() - 0.5) * 12, 5, 60);
            }

            foreach (var item in _ternaryData)
            {
                double a = rand.NextDouble();
                double b = rand.NextDouble();
                double c = rand.NextDouble();
                double sum = a + b + c;
                item.ComponentA = a / sum;
                item.ComponentB = b / sum;
                item.ComponentC = c / sum;
            }

            FunnelCtrl?.InvalidateVisual();
            WaffleCtrl?.InvalidateVisual();
            WordCloudCtrl?.InvalidateVisual();
            TernaryCtrl?.InvalidateVisual();

            FunnelCtrl?.StartEntryAnimation();
            WaffleCtrl?.StartEntryAnimation();
            WordCloudCtrl?.StartEntryAnimation();
            TernaryCtrl?.StartEntryAnimation();
        }

        // --- HELPER COLOR PARSER ---
        private static SolidColorBrush GetSolidColorBrush(string hex)
        {
            hex = hex.Replace("#", "");
            byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
            byte a = hex.Length == 8 ? byte.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber) : (byte)255;
            return new SolidColorBrush(Windows.UI.Color.FromArgb(a, r, g, b));
        }
    }

    // --- COLLECTION EXTENSION FOR ADDRANGE ---
    public static class ObservableCollectionExtensions
    {
        public static void AddRange<T>(this ObservableCollection<T> collection, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                collection.Add(item);
            }
        }
    }
}

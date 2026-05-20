using System;
using System.Collections.Generic;
using Windows.Foundation;
using Microsoft.UI.Xaml;
using SkiaSharp;
using Microsoft.UI.Xaml.Media;
using Windows.UI;
using ProCharts.Uno.Maths;

namespace ProCharts.Uno.Series
{
    public class HeikinAshiSeries : CandlestickSeries
    {
        public HeikinAshiSeries()
        {
            // Set smooth candlestick styling
            StrokeThickness = 1.0;
        }

        public override void RenderSeries(SeriesRenderContext context)
        {
            var rawPoints = GetFinancialPoints();
            if (rawPoints.Count == 0) return;

            // 1. Calculate Heikin-Ashi smoothed candlesticks
            var haPoints = new List<FinancialPoint>(rawPoints.Count);
            
            double prevHaOpen = double.NaN;
            double prevHaClose = double.NaN;

            for (int i = 0; i < rawPoints.Count; i++)
            {
                var rp = rawPoints[i];
                if (!rp.IsValid) continue;

                double haClose = (rp.Open + rp.High + rp.Low + rp.Close) / 4.0;
                double haOpen;

                if (double.IsNaN(prevHaOpen))
                {
                    haOpen = (rp.Open + rp.Close) / 2.0;
                }
                else
                {
                    haOpen = (prevHaOpen + prevHaClose) / 2.0;
                }

                double haHigh = Math.Max(rp.High, Math.Max(haOpen, haClose));
                double haLow = Math.Min(rp.Low, Math.Min(haOpen, haClose));

                var haPoint = new FinancialPoint
                {
                    X = rp.X,
                    Open = haOpen,
                    High = haHigh,
                    Low = haLow,
                    Close = haClose
                };

                haPoints.Add(haPoint);

                prevHaOpen = haOpen;
                prevHaClose = haClose;
            }

            if (haPoints.Count == 0) return;

            // 2. Render smoothed candlesticks
            var bullFill = UpFill ?? new SolidSKColorSKPaint(SKColor.Parse("#10B981")); // Emerald
            var bearFill = DownFill ?? new SolidSKColorSKPaint(SKColor.Parse("#EF4444")); // Rose
            var bullStroke = UpStroke ?? new SolidSKColorSKPaint(SKColor.Parse("#059669"));
            var bearStroke = DownStroke ?? new SolidSKColorSKPaint(SKColor.Parse("#DC2626"));

            double strokeThickness = StrokeThickness > 0 ? StrokeThickness : 1.0;
            var bullSKPaint = new Pen(bullStroke, strokeThickness);
            var bearSKPaint = new Pen(bearStroke, strokeThickness);

            double slotWidth = context.PlotArea.Width / Math.Max(1, haPoints.Count);
            double candleWidth = Math.Clamp(slotWidth * CandleWidthPercent, 2.0, 50.0);

            double progress = context.AnimationProgress;

            foreach (var fp in haPoints)
            {
                double open = fp.Open;
                double close = open + (fp.Close - open) * progress;
                double high = open + (fp.High - open) * progress;
                double low = open + (fp.Low - open) * progress;

                bool isBullish = close >= open;
                var fillSKPaint = isBullish ? bullFill : bearFill;
                var candleSKPaint = isBullish ? bullSKPaint : bearSKPaint;

                var ptOpen = context.Transform.ToScreen(fp.X, open);
                var ptClose = context.Transform.ToScreen(fp.X, close);
                var ptHigh = context.Transform.ToScreen(fp.X, high);
                var ptLow = context.Transform.ToScreen(fp.X, low);

                // Draw Wick
                context.Canvas.DrawLine((float)ptHigh.X, (float)ptHigh.Y, (float)ptLow.X, (float)ptLow.Y, candleSKPaint);

                // Draw Body
                double topY = Math.Min(ptOpen.Y, ptClose.Y);
                double bottomY = Math.Max(ptOpen.Y, ptClose.Y);
                double height = Math.Max(1.0, bottomY - topY);

                var bodyRect = new Rect(ptOpen.X - candleWidth / 2.0, topY, candleWidth, height);
                context.Canvas.DrawRectangle(fillSKPaint, candleSKPaint, bodyRect);
            }
        }
    }
}
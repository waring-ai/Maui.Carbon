namespace Maui.Carbon;

/// <summary>
/// Carbon's color scale of heat maps and choropleths (<c>color-scale-utils.ts</c>): a quantize scale over a nice domain onto
/// 11 sequential steps, or 17 diverging steps when the domain crosses zero, or the app's own colors.
/// </summary>
internal sealed class ChartColorScale
{
    ChartColorScale(double d0, double d1, bool diverging, int option, IReadOnlyList<Color>? custom)
    {
        D0 = d0;
        D1 = d1;
        Diverging = diverging;
        Option = option;
        Custom = custom;
    }

    public double D0 { get; }

    public double D1 { get; }

    /// <summary>True when the domain crosses zero (17 diverging steps).</summary>
    public bool Diverging { get; }

    /// <summary>The palette: sequential 1 purple, 2 blue, 3 cyan, 4 teal; diverging 1 red-cyan, 2 purple-teal.</summary>
    public int Option { get; }

    /// <summary>The app's colors (Carbon's <c>color.gradient.colors</c>), or null.</summary>
    public IReadOnlyList<Color>? Custom { get; }

    public int Count => Custom?.Count ?? (Diverging ? 17 : 11);

    /// <summary>
    /// Carbon's getDomain: the nice extent of the values, from 0 when every value is positive, symmetric around 0 when signs
    /// mix; <paramref name="min"/> and <paramref name="max"/> override the ends.
    /// </summary>
    public static (double Min, double Max) Domain(IEnumerable<double> values, double? min, double? max)
    {
        var list = values.Where(v => !double.IsNaN(v) && !double.IsInfinity(v)).ToList();
        double lo = list.Count > 0 ? list.Min() : 0, hi = list.Count > 0 ? list.Max() : 0;
        if (hi > lo) (lo, hi) = ChartTicks.Nice(lo, hi, 10);
        if (lo > 0) lo = 0;
        else if (lo == 0 && hi == 0) hi = 1;
        if (lo < 0 && hi > 0)
        {
            if (Math.Abs(lo) > hi) hi = Math.Abs(lo);
            else lo = -hi;
        }
        if (min is { } a && !double.IsNaN(a)) lo = a;
        if (max is { } b && !double.IsNaN(b)) hi = b;
        return (lo, hi);
    }

    public static ChartColorScale Create(IEnumerable<double> values, double? min, double? max, int option, IList<Color>? custom)
    {
        var (lo, hi) = Domain(values, min, max);
        var diverging = lo < 0 && hi > 0;
        if (!diverging && (option < 1 || option > 4)) option = 1;
        if (diverging && (option < 1 || option > 2)) option = 1;
        var colors = custom is { Count: > 0 } ? custom.Where(c => c is not null).ToList() : null;
        return new ChartColorScale(lo, hi, diverging, option, colors is { Count: > 0 } ? colors : null);
    }

    /// <summary>The step of a value (d3 <c>scaleQuantize</c>): values outside the domain take the end steps.</summary>
    public int Step(double value)
    {
        var n = Count;
        if (D1 == D0) return value < D0 ? 0 : n - 1;
        var i = (int)Math.Floor((value - D0) / (D1 - D0) * n);
        return Math.Clamp(i, 0, n - 1);
    }

    /// <summary>The paint of step <paramref name="step"/>.</summary>
    public ChartPaint StepPaint(int step)
    {
        if (Custom is { } c) return ChartPaint.OfColor(c[Math.Clamp(step, 0, c.Count - 1)]);
        return Diverging ? ChartPaint.OfDiverging(Option, step) : ChartPaint.OfSequential(Option, step);
    }

    /// <summary>The paint of a value.</summary>
    public ChartPaint Paint(double value) => StepPaint(Step(value));
}

/// <summary>The color legend of heat maps and choropleths (Carbon's <c>ColorScaleLegend</c>), drawn into the scene.</summary>
internal static class ChartColorLegend
{
    const float TitleHeight = 16;
    const float AxisY = 10;
    const float BarHeight = 8;
    const float BarWidth = 300;

    /// <summary>Space between the plot and the color legend.</summary>
    public const float Spacer = 15;

    internal readonly record struct Layout(float Height, float BarWidth, float X, float Y, float TitleX, bool UseDefault);

    /// <summary>
    /// Carbon's legend layout: a 300px bar (or the whole width when narrower). A heat map aligns the bar with the start of
    /// its x axis (<paramref name="alignX"/>) and puts the title left of it when it fits beside the y-axis labels.
    /// </summary>
    public static Layout Measure(float width, string? title, float alignX, bool heatmap)
    {
        var useDefault = width > BarWidth;
        var barWidth = useDefault ? BarWidth : Math.Max(0, width);
        float x = 0, y = string.IsNullOrEmpty(title) ? 0 : TitleHeight, titleX = 0;
        if (heatmap && useDefault && alignX > 1)
        {
            x = alignX;
            if (!string.IsNullOrEmpty(title))
            {
                var avail = alignX - CdsChartTextMetrics.Width(title, ChartFont.Tick) - 9;
                if (avail > 1)
                {
                    titleX = avail;
                    y = 0;
                }
                else
                {
                    titleX = alignX;
                    y = TitleHeight;
                }
            }
        }
        if (x + barWidth > width && useDefault) x = Math.Max(0, width - barWidth - 12);
        return new Layout(y + AxisY + 3 + 14, barWidth, x, y, titleX, useDefault);
    }

    /// <summary>Draws the legend with its top at <paramref name="top"/>: title, the gradient or blocks, and min, middle and max ticks.</summary>
    public static void Draw(ChartScene scene, ChartBuildContext ctx, ChartColorScale scale, Layout layout, CdsChartColorLegendType type, string? title, bool heatmap, float top)
    {
        if (!string.IsNullOrEmpty(title))
            scene.Texts.Add(new ChartText { Text = title!, X = layout.TitleX, Y = top - 2, Font = ChartFont.Tick, Color = ChartPaint.OfToken(CdsChartToken.LayerInverseAbsolute) });
        var bx = layout.X;
        var by = top + layout.Y;
        var bw = layout.BarWidth;
        var n = scale.Count;
        float r0 = 0, r1 = bw;
        if (type == CdsChartColorLegendType.Quantize)
        {
            var steps = Enumerable.Range(0, n).ToList();
            if (scale.Custom is null && scale.Diverging) steps.RemoveAt(n / 2);
            var band = bw / Math.Max(1, steps.Count);
            for (var i = 0; i < steps.Count; i++)
                scene.Shapes.Add(new ChartRect { Rect = new RectF(bx + band * i, by, Math.Max(0, band - 1), BarHeight), Fill = scale.StepPaint(steps[i]), Hover = ChartHoverStyle.None });
            if (scale.Custom is null && !scale.Diverging) r0 = band - 1;
        }
        else if (n == 1)
        {
            scene.Shapes.Add(new ChartRect { Rect = new RectF(bx, by, bw, BarHeight), Fill = scale.StepPaint(0), Hover = ChartHoverStyle.None });
        }
        else
        {
            // A gradient through every step: each pixel column is its lower stop with the upper stop laid over it at the
            // interpolation weight, which is exactly SVG's linear sRGB interpolation, from paints that follow the theme.
            var pixels = (int)Math.Floor(bw);
            var segment = -1;
            var segmentStart = 0;
            void CloseSegment(int end)
            {
                if (segment >= 0 && end > segmentStart)
                    scene.Shapes.Add(new ChartRect { Rect = new RectF(bx + segmentStart, by, end - segmentStart, BarHeight), Fill = scale.StepPaint(segment), Hover = ChartHoverStyle.None });
            }
            var overlays = new List<ChartRect>();
            for (var j = 0; j < pixels; j++)
            {
                var t = (j + 0.5) / pixels * (n - 1);
                var i = Math.Min(n - 2, (int)Math.Floor(t));
                var frac = (float)(t - i);
                if (i != segment)
                {
                    CloseSegment(j);
                    segment = i;
                    segmentStart = j;
                }
                if (frac > 0.01f)
                    overlays.Add(new ChartRect { Rect = new RectF(bx + j, by, 1, BarHeight), Fill = scale.StepPaint(i + 1), FillOpacity = frac, Hover = ChartHoverStyle.None });
            }
            CloseSegment(pixels);
            scene.Shapes.AddRange(overlays);
        }
        double[] ticks = [scale.D0, (scale.D0 + scale.D1) / 2, scale.D1];
        for (var i = 0; i < ticks.Length; i++)
        {
            var x = scale.D1 == scale.D0 ? r0 : r0 + (ticks[i] - scale.D0) / (scale.D1 - scale.D0) * (r1 - r0);
            var anchor = i == ticks.Length - 1
                ? (layout.UseDefault ? ChartTextAnchor.Middle : ChartTextAnchor.End)
                : i == 0 ? (layout.UseDefault && heatmap ? ChartTextAnchor.Middle : ChartTextAnchor.Start) : ChartTextAnchor.Middle;
            scene.Texts.Add(new ChartText { Text = ctx.Number(ticks[i]), X = bx + (float)x, Y = by + AxisY + 3 - 2, Anchor = anchor, Font = ChartFont.Tick });
        }
    }

    /// <summary>What a screen reader hears for the legend ("Sessions (k): 0 to 10").</summary>
    public static string Describe(ChartBuildContext ctx, ChartColorScale scale, string? title) =>
        (string.IsNullOrEmpty(title) ? "Color scale" : title) + ": " + ctx.Number(scale.D0) + " to " + ctx.Number(scale.D1);
}

/// <summary>45-degree hatching (Carbon's null-state and missing-data patterns) clipped to polygons, as one stroked path.</summary>
internal static class ChartHatch
{
    /// <summary>
    /// Lines x + y = sqrt(2) * (offset + k * spacing) (the pattern rotated 45 degrees: "/" lines <paramref name="spacing"/> apart)
    /// inside the rings (even-odd), as a path of separate segments.
    /// </summary>
    public static PathF Lines(IReadOnlyList<IReadOnlyList<PointF>> rings, float spacing, float offset = 0)
    {
        var path = new PathF();
        var edges = new List<(PointF A, PointF B, float S0, float S1)>();
        var sMin = float.MaxValue;
        var sMax = float.MinValue;
        foreach (var ring in rings)
        {
            if (ring.Count < 3) continue;
            for (var i = 0; i < ring.Count; i++)
            {
                var a = ring[i];
                var b = ring[(i + 1) % ring.Count];
                var sa = a.X + a.Y;
                var sb = b.X + b.Y;
                edges.Add((a, b, Math.Min(sa, sb), Math.Max(sa, sb)));
                sMin = Math.Min(sMin, sa);
                sMax = Math.Max(sMax, sa);
            }
        }
        if (edges.Count == 0) return path;
        edges.Sort((x, y) => x.S0.CompareTo(y.S0));
        var step = spacing * MathF.Sqrt(2);
        var off = offset * MathF.Sqrt(2);
        var k0 = (int)Math.Ceiling((sMin - off) / step);
        var k1 = (int)Math.Floor((sMax - off) / step);
        var ts = new List<float>();
        for (var k = k0; k <= k1; k++)
        {
            var c = off + k * step;
            ts.Clear();
            foreach (var (a, b, s0, s1) in edges)
            {
                if (s0 > c) break;
                if (s1 <= c) continue;
                var sa = a.X + a.Y - c;
                var sb = b.X + b.Y - c;
                if ((sa > 0) == (sb > 0)) continue;
                var u = sa / (sa - sb);
                var x = a.X + (b.X - a.X) * u;
                var y = a.Y + (b.Y - a.Y) * u;
                ts.Add(x - y);
            }
            if (ts.Count < 2) continue;
            ts.Sort();
            for (var i = 0; i + 1 < ts.Count; i += 2)
            {
                if (ts[i + 1] - ts[i] < 0.01f) continue;
                path.MoveTo((c + ts[i]) / 2, (c - ts[i]) / 2);
                path.LineTo((c + ts[i + 1]) / 2, (c - ts[i + 1]) / 2);
            }
        }
        return path;
    }

    public static IReadOnlyList<PointF> Rect(RectF r) => [new(r.Left, r.Top), new(r.Right, r.Top), new(r.Right, r.Bottom), new(r.Left, r.Bottom)];
}

/// <summary>Scene helpers shared by heat maps and maps.</summary>
internal static class ChartOutline
{
    /// <summary>Carbon's highlight box: a <c>white</c> outline with an outward shadow, drawn only while <paramref name="mark"/> is active.</summary>
    public static void Add(ChartScene scene, RectF rect, float width, int mark)
    {
        scene.Shapes.Add(new ChartRect { Rect = rect.Inflate(width / 2 + 1.5f, width / 2 + 1.5f), Stroke = ChartPaint.OfKey(CdsColorKeys.Shadow), StrokeWidth = 3, Mark = mark, HoverOnly = true, Hover = ChartHoverStyle.None });
        scene.Shapes.Add(new ChartRect { Rect = rect, Stroke = ChartPaint.OfKey(CdsColorKeys.TextOnColor), StrokeWidth = width, Mark = mark, HoverOnly = true, Hover = ChartHoverStyle.None });
    }

    /// <summary>A copy of a context with another plot height (a chart that draws a legend under its graph).</summary>
    public static ChartBuildContext WithHeight(ChartBuildContext ctx, float height) =>
        new(ctx.Width, height, ctx.Items, ctx.Groups, ctx.Groups.Count == ctx.ActiveGroups.Count ? [] : ctx.ActiveGroups, ctx.Colors, ctx.Culture)
        {
            ValueFormat = ctx.ValueFormat,
            ValueFormatter = ctx.ValueFormatter,
            ShowTotal = ctx.ShowTotal,
            TotalLabel = ctx.TotalLabel,
            GroupLabel = ctx.GroupLabel,
        };
}

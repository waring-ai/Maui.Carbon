using System.Collections.Specialized;
using System.Globalization;

namespace Maui.Carbon;

/// <summary>
/// One vertical axis per variable and one line per record across them, colored by group (parallel coordinates): compare
/// records over 3 to 8 measures with different units and find the ones that satisfy several ranges at once. Data is
/// <see cref="CdsParallelRecord"/>s; <see cref="Dimensions"/> lists the axes left to right (default every numeric key).
/// Each axis has its own nice linear scale (not forced to zero) or evenly spaced categories; records are 1.5px lines at
/// 70% opacity, and a missing value breaks the line. <see cref="Filters"/> keep ranges per axis: records outside turn
/// <c>layer-accent-01</c>. For two variables use a scatter chart; for one variable over time a line chart.
/// </summary>
public class CdsParallelCoordinatesChart : CdsChart
{
    /// <summary>Bindable property for <see cref="RecordOpacity"/>.</summary>
    public static readonly BindableProperty RecordOpacityProperty = BindableProperty.Create(
        nameof(RecordOpacity), typeof(double), typeof(CdsParallelCoordinatesChart), 0.7, propertyChanged: Changed);

    const float TitleSpace = 28;
    const float TickPadding = 9;
    const float BrushWidth = 16;

    readonly CdsItemCollection<CdsParallelDimension> _dimensions = [];
    readonly CdsItemCollection<CdsParallelFilter> _filters = [];

    /// <summary>Creates a parallel coordinates chart.</summary>
    public CdsParallelCoordinatesChart()
    {
        _dimensions.CollectionChanged += OnCollectionChanged;
        _filters.CollectionChanged += OnCollectionChanged;
    }

    /// <summary>The axes, left to right (Carbon's <c>parallelCoordinates.dimensions</c>); empty (default) makes one per numeric key of the records.</summary>
    public IList<CdsParallelDimension> Dimensions => _dimensions;

    /// <summary>
    /// The kept range per axis (Carbon's <c>parallelCoordinates.filters</c>, shown as Carbon's brush on the axis). Brushing with
    /// a pointer is not available, so give users these filters through form controls.
    /// </summary>
    public IList<CdsParallelFilter> Filters => _filters;

    /// <summary>The records' opacity at rest (Carbon's <c>parallelCoordinates.opacity</c>). Default 0.7; legend hover fades other groups to 0.3.</summary>
    public double RecordOpacity
    {
        get => (double)GetValue(RecordOpacityProperty);
        set => SetValue(RecordOpacityProperty, value);
    }

    private protected override string ChartKind => "Parallel coordinates chart";

    private protected override ChartSkeleton Skeleton => ChartSkeleton.Lines;

    private protected override bool IsEmpty(IReadOnlyList<object> items) => !items.OfType<CdsParallelRecord>().Any();

    private protected override IReadOnlyList<string> GroupsOf(IReadOnlyList<object> items) =>
        items.OfType<CdsParallelRecord>().Select(r => r.Group ?? "").Distinct().ToList();

    void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => RequestRebuild();

    /// <summary>One resolved axis: its dimension, scale and position.</summary>
    internal sealed class Axis
    {
        public required CdsParallelDimension Dimension { get; init; }

        public bool IsLinear { get; init; }

        public double D0 { get; init; }

        public double D1 { get; init; }

        public IList<string> Categories { get; init; } = [];

        public float X { get; set; }

        public float Y0 { get; init; }

        public float Y1 { get; init; }

        /// <summary>The y of a value, or NaN.</summary>
        public float Map(object? value)
        {
            if (value is null) return float.NaN;
            if (IsLinear)
            {
                if (!CdsParallelRecord.TryNumber(value, out var v)) return float.NaN;
                return D1 == D0 ? (Y0 + Y1) / 2 : (float)(Y1 - (v - D0) / (D1 - D0) * (Y1 - Y0));
            }
            var s = Convert.ToString(value, CultureInfo.InvariantCulture);
            var i = s is null ? -1 : Categories.IndexOf(s);
            return i < 0 ? float.NaN : Y1 - Step * (i + 0.5f);
        }

        public float Step => Categories.Count == 0 ? 0 : (Y1 - Y0) / Categories.Count;

        public IReadOnlyList<object> Ticks(float plotHeight)
        {
            if (!IsLinear) return Categories.Cast<object>().ToList();
            var n = Math.Min(10, Math.Max(2, (int)Math.Floor(plotHeight / (16 * 2.5))));
            return ChartTicks.Ticks(D0, D1, n).Cast<object>().ToList();
        }
    }

    /// <summary>The dimensions in effect: <see cref="Dimensions"/>, else every key with a numeric value in the first 20 records.</summary>
    internal IReadOnlyList<CdsParallelDimension> EffectiveDimensions(IReadOnlyList<CdsParallelRecord> records)
    {
        if (_dimensions.Count > 0) return _dimensions.Where(d => !string.IsNullOrEmpty(d.Key)).ToList();
        var keys = records.Take(20).SelectMany(r => r.Values.Keys).Distinct().ToList();
        return keys.Where(k => records.Any(r => CdsParallelRecord.TryNumber(r[k], out _))).Select(k => new CdsParallelDimension(k)).ToList();
    }

    internal static Axis MakeAxis(CdsParallelDimension d, IReadOnlyList<CdsParallelRecord> records, float y0, float y1)
    {
        var values = records.Select(r => r[d.Key]).Where(v => v is not null).ToList();
        var linear = d.ScaleType switch
        {
            CdsParallelScaleType.Linear => true,
            CdsParallelScaleType.Labels => false,
            _ => d.Categories is null && values.Count > 0 && values.All(v => CdsParallelRecord.TryNumber(v, out _)),
        };
        if (!linear)
        {
            var cats = d.Categories?.ToList() ?? values.Select(v => Convert.ToString(v, CultureInfo.InvariantCulture)!).Distinct().ToList();
            return new Axis { Dimension = d, IsLinear = false, Categories = cats, Y0 = y0, Y1 = y1 };
        }
        var nums = values.Select(v => CdsParallelRecord.TryNumber(v, out var x) ? x : double.NaN).Where(x => !double.IsNaN(x)).ToList();
        double lo, hi;
        if (d.Minimum is { } min && d.Maximum is { } max)
        {
            (lo, hi) = (min, max);
        }
        else
        {
            lo = d.Minimum ?? (nums.Count > 0 ? nums.Min() : 0);
            hi = d.Maximum ?? (nums.Count > 0 ? nums.Max() : 1);
            if (nums.Count == 0 && d.Minimum is null && d.Maximum is null) (lo, hi) = (0, 1);
            if (lo == hi) (lo, hi) = (lo - 1, hi + 1);
            (lo, hi) = ChartTicks.Nice(lo, hi, 10);
        }
        return new Axis { Dimension = d, IsLinear = true, D0 = lo, D1 = hi, Y0 = y0, Y1 = y1 };
    }

    string Format(ChartBuildContext ctx, CdsParallelDimension d, object? v)
    {
        if (v is null) return "-";
        if (d.Formatter is { } f) return f(v);
        return CdsParallelRecord.TryNumber(v, out var x) ? ctx.Number(x) : Convert.ToString(v, ctx.Culture) ?? "";
    }

    /// <summary>The pixel range a filter keeps on its axis (Carbon's brush), or null.</summary>
    internal static (float A, float B)? BrushOf(CdsParallelFilter f, Axis axis)
    {
        if (axis.IsLinear)
        {
            if (f.Minimum is not { } lo || f.Maximum is not { } hi) return null;
            var a = axis.Map(lo);
            var b = axis.Map(hi);
            return (Math.Min(a, b), Math.Max(a, b));
        }
        var ys = (f.Categories ?? []).Select(c => axis.Map(c)).Where(y => !float.IsNaN(y)).ToList();
        if (ys.Count == 0) return null;
        return (ys.Min() - axis.Step / 2, ys.Max() + axis.Step / 2);
    }

    private protected override ChartScene BuildScene(ChartBuildContext ctx)
    {
        var w = ctx.Width;
        var h = ctx.Height;
        var scene = new ChartScene { Width = w, Height = h, Plot = new RectF(0, 0, w, h), HitMode = ChartHitMode.Contains };
        var all = ctx.Items.OfType<CdsParallelRecord>().ToList();
        var dims = EffectiveDimensions(all);
        if (dims.Count == 0) return ChartSkeletons.Build(ChartSkeleton.Lines, w, h, shimmer: false);
        const float y0 = TitleSpace;
        var y1 = Math.Max(y0 + 1, h - 8);
        var plotH = y1 - y0;
        var axes = dims.Select(d => MakeAxis(d, all, y0, y1)).ToList();
        var ticks = axes.Select(a =>
        {
            var vals = a.Ticks(plotH);
            return (Values: vals, Labels: vals.Select(v => CdsChartTruncate.Apply(Format(ctx, a.Dimension, v), CdsChartTruncation.End, 12, 10)).ToList());
        }).ToList();
        var labelW = ticks.Select(t => CdsChartTextMetrics.MaxWidth(t.Labels, ChartFont.Tick)).ToList();
        var titleW = axes.Select(a => CdsChartTextMetrics.Width(a.Dimension.DisplayTitle, ChartFont.AxisTitle)).ToList();
        var x0 = MathF.Ceiling(Math.Max(labelW[0] + TickPadding + 2, titleW[0] / 2));
        var x1 = MathF.Floor(w - Math.Max(8, titleW[^1] / 2));
        x1 = Math.Max(x0 + 1, x1);
        var step = axes.Count > 1 ? (x1 - x0) / (axes.Count - 1) : w;
        for (var i = 0; i < axes.Count; i++) axes[i].X = axes.Count > 1 ? x0 + step * i : (x0 + x1) / 2;

        var brushes = new Dictionary<int, (float A, float B)>();
        foreach (var f in _filters)
        {
            var i = axes.FindIndex(a => a.Dimension.Key == f.Key);
            if (i >= 0 && BrushOf(f, axes[i]) is { } b) brushes[i] = b;
        }
        bool InBrush(CdsParallelRecord r) => brushes.All(kv =>
        {
            var y = axes[kv.Key].Map(r[axes[kv.Key].Dimension.Key]);
            return !float.IsNaN(y) && y >= kv.Value.A - 0.5f && y <= kv.Value.B + 0.5f;
        });
        List<List<PointF>> Segments(CdsParallelRecord r)
        {
            var parts = new List<List<PointF>>();
            List<PointF>? cur = null;
            foreach (var a in axes)
            {
                var y = a.Map(r[a.Dimension.Key]);
                if (float.IsNaN(y))
                {
                    cur = null;
                    continue;
                }
                if (cur is null) parts.Add(cur = []);
                cur.Add(new PointF(a.X, y));
            }
            return parts;
        }
        static PathF PathOf(List<List<PointF>> parts)
        {
            var path = new PathF();
            foreach (var p in parts)
            {
                path.MoveTo(p[0]);
                if (p.Count == 1) path.LineTo(p[0]);
                for (var i = 1; i < p.Count; i++) path.LineTo(p[i]);
            }
            return path;
        }

        var records = all.Where(r => ctx.IsActive(r.Group ?? "")).Select(r => (R: r, Parts: Segments(r), On: InBrush(r))).ToList();
        foreach (var x in records.Where(x => !x.On))
            scene.Shapes.Add(new ChartPath { Path = PathOf(x.Parts), Stroke = ChartPaint.OfKey(CdsColorKeys.LayerAccent01), StrokeWidth = 1, Join = LineJoin.Round, Hover = ChartHoverStyle.None });
        var opacity = (float)Math.Clamp(RecordOpacity, 0, 1);
        var id = 0;
        var overlays = new List<ChartShape>();
        foreach (var x in records.Where(x => x.On))
        {
            var g = x.R.Group ?? "";
            var path = PathOf(x.Parts);
            var mark = id++;
            scene.Shapes.Add(new ChartPath { Path = path, Stroke = ChartPaint.OfSeries(g), StrokeWidth = 1.5f, Opacity = opacity, LegendFade = opacity > 0 ? Math.Min(1, 0.3f / opacity) : 1, Join = LineJoin.Round, Group = g, Mark = mark, Hover = ChartHoverStyle.None });
            overlays.Add(new ChartPath { Path = path, Stroke = ChartPaint.OfSeries(g), StrokeWidth = 3, Join = LineJoin.Round, Group = g, Mark = mark, HoverOnly = true, Hover = ChartHoverStyle.None });
            var rows = new List<ChartTooltipRow>();
            if (x.R.Name is not null) rows.Add(new ChartTooltipRow(x.R.Name, null, Bold: true));
            rows.Add(new ChartTooltipRow(ctx.GroupLabel, x.R.Group ?? "", ChartPaint.OfSeries(g)));
            foreach (var a in axes) rows.Add(new ChartTooltipRow(a.Dimension.DisplayTitle, Format(ctx, a.Dimension, x.R[a.Dimension.Key])));
            var pts = x.Parts.SelectMany(p => p).ToList();
            scene.Marks.Add(new ChartMark
            {
                Id = mark,
                Bounds = GeoScene.Bounds(pts),
                Polygon = GeoScene.HitPolygon(x.Parts.Select(p => GeoScene.StrokeHit(p, 8)).ToList()),
                Anchor = pts.Count > 0 ? pts[0] : null,
                Group = g,
                Rows = rows,
                Description = string.Join(", ", rows.Select(r => r.Value is null ? r.Label : r.Label + " " + r.Value)),
            });
        }
        scene.Shapes.AddRange(overlays);

        // Axes: a border-strong-01 line, tick labels on a background plate (Carbon's halo), the title above, and the brush.
        var maxTitle = Math.Max(4, (int)Math.Floor((axes.Count > 1 ? step - 8 : w) / 7));
        for (var i = 0; i < axes.Count; i++)
        {
            var a = axes[i];
            scene.Texts.Add(new ChartText
            {
                Text = CdsChartTruncate.Apply(a.Dimension.DisplayTitle, CdsChartTruncation.End, maxTitle, Math.Max(3, maxTitle - 2)),
                X = a.X,
                Y = 0,
                Anchor = ChartTextAnchor.Middle,
                Font = ChartFont.AxisTitle,
                Color = ChartPaint.OfKey(CdsColorKeys.TextPrimary),
            });
            scene.Shapes.Add(new ChartLine { From = new(a.X + 0.5f, y0), To = new(a.X + 0.5f, y1), Stroke = ChartPaint.OfKey(CdsColorKeys.BorderStrong01), Hover = ChartHoverStyle.None });
            for (var j = 0; j < ticks[i].Values.Count; j++)
            {
                var y = a.Map(ticks[i].Values[j]);
                if (float.IsNaN(y)) continue;
                var label = ticks[i].Labels[j];
                var lw = CdsChartTextMetrics.Width(label, ChartFont.Tick);
                scene.Shapes.Add(new ChartRect { Rect = new RectF(a.X - TickPadding - lw - 1, y - 6, lw + 2, 12), Fill = ChartPaint.OfKey(CdsColorKeys.Background), FillOpacity = 0.7f, Hover = ChartHoverStyle.None });
                scene.Texts.Add(new ChartText { Text = label, X = a.X - TickPadding, Y = y, Anchor = ChartTextAnchor.End, Baseline = ChartTextBaseline.Middle });
            }
            if (brushes.TryGetValue(i, out var br))
                scene.Shapes.Add(new ChartRect
                {
                    Rect = new RectF(a.X - BrushWidth / 2, br.A, BrushWidth, Math.Max(1, br.B - br.A)),
                    Fill = ChartPaint.OfKey(CdsColorKeys.LayerAccent01),
                    FillOpacity = 0.3f,
                    Stroke = ChartPaint.OfKey(CdsColorKeys.ButtonTertiary),
                    StrokeWidth = 1,
                    Hover = ChartHoverStyle.None,
                });
        }
        var hidden = records.Count(x => !x.On);
        scene.Summary = (id == 1 ? "1 record" : $"{id} records") + $" on {axes.Count} axes" + (hidden > 0 ? $", {hidden} filtered out" : "");
        return scene;
    }

    private protected override ChartTable BuildTable(ChartBuildContext ctx)
    {
        var all = ctx.Items.OfType<CdsParallelRecord>().ToList();
        var dims = EffectiveDimensions(all);
        return new ChartTable(
            new[] { "Name", ctx.GroupLabel }.Concat(dims.Select(d => d.DisplayTitle)).ToList(),
            all.Where(r => ctx.IsActive(r.Group ?? "")).Select(r =>
                new[] { r.Name ?? "", r.Group ?? "" }.Concat(dims.Select(d => r[d.Key] is { } v ? (CdsParallelRecord.TryNumber(v, out var x) ? ctx.Number(x) : Convert.ToString(v, ctx.Culture) ?? "") : "")).ToArray()).ToList());
    }

    static void Changed(BindableObject b, object? o, object? n) => ((CdsParallelCoordinatesChart)b).RequestRebuild();
}

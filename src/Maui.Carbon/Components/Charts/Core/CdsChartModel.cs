using System.Globalization;
using System.Text;

namespace Maui.Carbon;

/// <summary>Everything a chart needs to build its scene at one size: data snapshot, groups and legend state, formatting.</summary>
internal sealed class ChartBuildContext
{
    readonly HashSet<string> _active;
    readonly bool _allActive;

    public ChartBuildContext(float width, float height, IReadOnlyList<object> items, IReadOnlyList<string> groups, IReadOnlyList<string> selected, ChartColors colors, CultureInfo culture)
    {
        Width = width;
        Height = height;
        Items = items;
        Groups = groups;
        Colors = colors;
        Culture = culture;
        _allActive = selected.Count == 0;
        _active = new HashSet<string>(selected, StringComparer.Ordinal);
        ActiveGroups = _allActive ? groups : groups.Where(_active.Contains).ToList();
        Points = items.OfType<CdsChartPoint>().ToList();
        ActivePoints = _allActive ? Points : Points.Where(p => IsActive(p.Group)).ToList();
    }

    /// <summary>The plot area width.</summary>
    public float Width { get; }

    /// <summary>The plot area height.</summary>
    public float Height { get; }

    /// <summary>Every data item, in order.</summary>
    public IReadOnlyList<object> Items { get; }

    /// <summary>Every group (series), in data order; colors are assigned in this order.</summary>
    public IReadOnlyList<string> Groups { get; }

    /// <summary>The groups the legend shows as active.</summary>
    public IReadOnlyList<string> ActiveGroups { get; }

    /// <summary>The <see cref="CdsChartPoint"/> items.</summary>
    public IReadOnlyList<CdsChartPoint> Points { get; }

    /// <summary>The points of active groups (Carbon's display data).</summary>
    public IReadOnlyList<CdsChartPoint> ActivePoints { get; }

    public ChartColors Colors { get; }

    public CultureInfo Culture { get; }

    public string? ValueFormat { get; init; }

    public Func<double, string>? ValueFormatter { get; init; }

    public bool ShowTotal { get; init; } = true;

    public string TotalLabel { get; init; } = "Total";

    public string GroupLabel { get; init; } = "Group";

    public bool IsActive(string? group) => _allActive || group is null || _active.Contains(group);

    /// <summary>A locale number: grouped, up to three decimals (axis ticks, labels).</summary>
    public string Number(double? value) => CdsChartFormat.Number(value, Culture);

    /// <summary>A value for tooltips and the table: the chart's <c>ValueFormatter</c> or <c>ValueFormat</c>, else <see cref="Number"/>.</summary>
    public string Value(double? value)
    {
        if (value is not { } v || double.IsNaN(v)) return "";
        if (ValueFormatter is { } f) return f(v);
        return CdsChartFormat.Number(v, Culture, ValueFormat);
    }

    public string Date(DateTime value) => CdsChartFormat.Date(value, Culture);
}

/// <summary>The "Show as table" and CSV content of a chart: column headers and string rows.</summary>
internal sealed record ChartTable(IReadOnlyList<string> Headers, IReadOnlyList<string[]> Rows)
{
    public static readonly ChartTable Empty = new([], []);

    /// <summary>The table as CSV (RFC 4180 quoting).</summary>
    public string ToCsv()
    {
        static string Esc(string s) => s.IndexOfAny([',', '"', '\n', '\r']) >= 0 ? "\"" + s.Replace("\"", "\"\"") + "\"" : s;
        var sb = new StringBuilder();
        sb.Append(string.Join(",", Headers.Select(Esc)));
        foreach (var row in Rows)
        {
            sb.Append('\n');
            sb.Append(string.Join(",", row.Select(c => Esc(c ?? ""))));
        }
        return sb.ToString();
    }
}

/// <summary>Carbon's legend semantics (<c>toggleDataLabel</c>): click isolates, more clicks add, the last active click resets.</summary>
internal static class ChartLegendModel
{
    public static IReadOnlyList<string> Toggle(IReadOnlyList<string> selected, IReadOnlyList<string> all, string name)
    {
        if (selected.Count == 0) return [name];
        if (selected.Count == 1 && selected[0] == name) return [];
        if (selected.Contains(name)) return selected.Where(g => g != name).ToList();
        var next = all.Where(g => g == name || selected.Contains(g)).ToList();
        // Every group active again: the same as none selected.
        return next.Count == all.Count ? [] : next;
    }

    /// <summary>Groups in <paramref name="order"/> first, then the rest in data order.</summary>
    public static IReadOnlyList<string> Ordered(IReadOnlyList<string> groups, IList<string>? order)
    {
        if (order is null || order.Count == 0) return groups;
        int Rank(string g)
        {
            var i = order.IndexOf(g);
            return i < 0 ? int.MaxValue : i;
        }
        return groups.Select((g, i) => (g, i)).OrderBy(x => Rank(x.g)).ThenBy(x => x.i).Select(x => x.g).ToList();
    }
}

/// <summary>The skeleton a chart shows while loading or empty.</summary>
internal enum ChartSkeleton
{
    /// <summary>A grid backdrop with 5 horizontal and 15 vertical lines (axis charts).</summary>
    Grid,
    /// <summary>Horizontal lines only.</summary>
    Lines,
    /// <summary>A filled disc (pie).</summary>
    Pie,
    /// <summary>A ring (donut, gauge).</summary>
    Donut,
    /// <summary>One horizontal line across the middle on a transparent backdrop (sparkline).</summary>
    Line,
    /// <summary>Nothing (the chart draws its own empty state).</summary>
    None,
}

internal static class ChartSkeletons
{
    /// <summary>The loading/empty scene: Carbon's skeleton in <c>layer-accent-01</c> on the grid background.</summary>
    public static ChartScene Build(ChartSkeleton kind, float width, float height, bool shimmer)
    {
        var scene = new ChartScene { Width = width, Height = height, Plot = new RectF(0, 0, width, height), Shimmer = shimmer, Summary = shimmer ? "Loading" : "No data" };
        var accent = ChartPaint.OfKey(CdsColorKeys.LayerAccent01);
        switch (kind)
        {
            case ChartSkeleton.Grid:
            case ChartSkeleton.Lines:
                scene.Shapes.Add(new ChartRect { Rect = new RectF(0, 0, width, height), Fill = ChartPaint.OfToken(CdsChartToken.GridBg) });
                for (var i = 0; i <= 5; i++)
                {
                    var y = MathF.Round((height - 1) * i / 5f) + 0.5f;
                    scene.Shapes.Add(new ChartLine { From = new(0, y), To = new(width, y), Stroke = accent, Shimmers = true });
                }
                if (kind == ChartSkeleton.Grid)
                    for (var i = 1; i < 15; i++)
                    {
                        var x = MathF.Round(width * i / 15f) + 0.5f;
                        scene.Shapes.Add(new ChartLine { From = new(x, 0), To = new(x, height), Stroke = accent, Shimmers = true });
                    }
                break;
            case ChartSkeleton.Line:
            {
                // Carbon's renderSparklineSkeleton: one line across the middle, no backdrop.
                var y = MathF.Round(height / 2) + 0.5f;
                scene.Shapes.Add(new ChartLine { From = new(0, y), To = new(width, y), Stroke = accent, Shimmers = true });
                break;
            }
            case ChartSkeleton.Pie:
            case ChartSkeleton.Donut:
            {
                var r = Math.Max(0, Math.Min(width, height) / 2 - 2);
                var c = new PointF(r + 2, r + 2);
                var path = new PathF();
                path.AppendCircle(c.X, c.Y, r);
                if (kind == ChartSkeleton.Donut) path.AppendCircle(c.X, c.Y, r * 0.75f);
                scene.Shapes.Add(new ChartPath { Path = path, Fill = accent, Winding = WindingMode.EvenOdd, Shimmers = true });
                break;
            }
        }
        return scene;
    }
}

namespace Maui.Carbon;

/// <summary>What a sparkline draws (the sparkline's <c>type</c>).</summary>
public enum CdsSparklineType
{
    /// <summary>A 1.5px line over Carbon's area gradient (default).</summary>
    Area,
    /// <summary>The line alone.</summary>
    Line,
}

/// <summary>
/// A small, axis-free trend of one series, shown inline in a tile, table cell or KPI card (Carbon's sparkline: an area
/// chart with hidden axes, no grid, legend, toolbar or points, and a transparent backdrop so it sits on its surface). The
/// time domain runs edge to edge and the value axis keeps 10% headroom and includes 0; set the left axis
/// <see cref="CdsChartAxis.IncludeZero"/> off (with <see cref="CdsChartAxis.IsVisible"/> off) to show small changes, and say so.
/// Hovering shows the dashed ruler with the date and value; turn <see cref="CdsCartesianChart.IsRulerEnabled"/> off for purely
/// decorative use. The sparkline only shows shape: put the current value and its unit as text next to it, and name the
/// measure and period in <see cref="CdsChart.Title"/> or an adjacent label. Every <see cref="CdsAreaChart"/> option applies.
/// </summary>
public class CdsSparklineChart : CdsAreaChart
{
    /// <summary>Bindable property for <see cref="Type"/>.</summary>
    public static readonly BindableProperty TypeProperty = BindableProperty.Create(
        nameof(Type), typeof(CdsSparklineType), typeof(CdsSparklineChart), CdsSparklineType.Area,
        propertyChanged: static (b, _, _) => ((CdsSparklineChart)b).RequestRebuild());

    /// <summary>An area with the gradient fill (default) or the line alone.</summary>
    public CdsSparklineType Type
    {
        get => (CdsSparklineType)GetValue(TypeProperty);
        set => SetValue(TypeProperty, value);
    }

    private protected override string ChartKind => "Sparkline";

    private protected override double DefaultHeight => 80;

    private protected override bool DefaultLegendEnabled => false;

    private protected override bool DefaultToolbarEnabled => false;

    private protected override bool DefaultXGrid => false;

    private protected override bool DefaultYGrid => false;

    private protected override bool ShowsArea => Type == CdsSparklineType.Area;

    private protected override bool TransparentBackdrop => true;

    // Loading shows Carbon's single shimmering line on a transparent backdrop; empty data draws the same line (see BuildMarks).
    private protected override ChartSkeleton Skeleton => ChartSkeleton.Line;

    private protected override bool IsEmpty(IReadOnlyList<object> items) => false;

    private protected override IReadOnlyDictionary<AxisPosition, AxisSpec> FallbackAxes => new Dictionary<AxisPosition, AxisSpec>
    {
        [AxisPosition.Left] = new() { Position = AxisPosition.Left, ScaleType = CdsChartScaleType.Linear, IsVisible = false },
        [AxisPosition.Bottom] = new() { Position = AxisPosition.Bottom, ScaleType = CdsChartScaleType.Time, IsVisible = false },
    };

    private protected override void BuildMarks(ChartScene scene, ChartBuildContext ctx, CartesianLayout cs)
    {
        if (ctx.Items.Count == 0)
        {
            // Carbon's renderSparklineSkeleton: one layer-accent-01 line across the middle.
            if (scene.Shapes.Count > 0 && scene.Shapes[0] is ChartRect backdrop) backdrop.Fill = ChartPaint.None;
            var y = MathF.Round(scene.Height / 2) + 0.5f;
            scene.Shapes.Add(new ChartLine { From = new(0, y), To = new(scene.Width, y), Stroke = ChartPaint.OfKey(CdsColorKeys.LayerAccent01) });
            scene.Summary = "No data";
            return;
        }
        base.BuildMarks(scene, ctx, cs);
        var values = ctx.ActivePoints.Count(p => p.Value.HasValue);
        scene.Summary = values == 1 ? "1 value" : $"{values} values";
    }
}

namespace Maui.Carbon;

/// <summary>
/// Bars comparing one value per category or time point (Carbon's simple bar chart), vertical or horizontal, from a zero
/// baseline or floating between <see cref="CdsChartPoint.Low"/> and <see cref="CdsChartPoint.High"/>. Each point is a
/// bar; its group is its legend item and color. Put the labels axis on the left for horizontal bars. For several series
/// per category use <see cref="CdsChartPoint"/>s in a grouped or stacked bar chart instead.
/// </summary>
public class CdsSimpleBarChart : CdsCartesianChart
{
    /// <summary>Bindable property for <see cref="BarWidth"/>.</summary>
    public static readonly BindableProperty BarWidthProperty = BindableProperty.Create(
        nameof(BarWidth), typeof(double?), typeof(CdsSimpleBarChart), null, propertyChanged: Changed);

    /// <summary>Bindable property for <see cref="BarMaxWidth"/>.</summary>
    public static readonly BindableProperty BarMaxWidthProperty = BindableProperty.Create(
        nameof(BarMaxWidth), typeof(double), typeof(CdsSimpleBarChart), 16d, propertyChanged: Changed);

    /// <summary>A fixed bar thickness (Carbon's <c>bars.width</c>); <c>null</c> (default) is the smaller of <see cref="BarMaxWidth"/> and half the category step.</summary>
    public double? BarWidth
    {
        get => (double?)GetValue(BarWidthProperty);
        set => SetValue(BarWidthProperty, value);
    }

    /// <summary>The largest bar thickness (Carbon's <c>bars.maxWidth</c>). Default 16.</summary>
    public double BarMaxWidth
    {
        get => (double)GetValue(BarMaxWidthProperty);
        set => SetValue(BarMaxWidthProperty, value);
    }

    private protected override string ChartKind => "Bar chart";

    private protected override void BuildMarks(ChartScene scene, ChartBuildContext ctx, CartesianLayout cs)
    {
        var rows = ctx.ActivePoints;
        var bw = ChartBars.Width(cs, BarWidth, BarMaxWidth, rows.Count);
        var id = 0;
        foreach (var d in rows)
        {
            if (d.Value is null && !d.HasRange) continue;
            var center = cs.DomainPx(d);
            if (double.IsNaN(center)) continue;
            var x0 = center - bw / 2;
            var x1 = x0 + bw;
            double y0, y1;
            if (d.HasRange)
            {
                y0 = cs.RangePx(d.Group, d.Low!.Value);
                y1 = cs.RangePx(d.Group, d.High!.Value);
            }
            else
            {
                y0 = cs.Baseline(d.Group);
                y1 = ChartBars.MinLength(y0, cs.RangePx(d.Group, d.Value!.Value), d.Value.Value, cs.Vertical);
            }
            var rect = ChartBars.Rect(x0, x1, y0, y1, cs.Vertical);
            var mark = id++;
            scene.Shapes.Add(new ChartRect { Rect = rect, Fill = ChartPaint.OfSeries(d.Group), Group = d.Group, Mark = mark });
            scene.Marks.Add(new ChartMark
            {
                Id = mark,
                Bounds = rect,
                Group = d.Group,
                Rows = CartesianFrame.TooltipRows(ctx, cs, [d]),
                Description = $"{cs.DomainText(d)}, {(d.HasRange ? ctx.Value(d.Low) + " to " + ctx.Value(d.High) : ctx.Value(d.Value))}" + (d.Group is { Length: > 0 } g && g != cs.DomainText(d) ? $", {g}" : ""),
            });
        }
        scene.Summary = id == 1 ? "1 bar" : $"{id} bars";
    }

    static void Changed(BindableObject b, object? o, object? n) => ((CdsSimpleBarChart)b).RequestRebuild();
}

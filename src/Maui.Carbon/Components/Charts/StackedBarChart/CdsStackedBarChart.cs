namespace Maui.Carbon;

/// <summary>
/// Bars that stack each group's value into one bar per category or time point (Carbon's stacked bar chart), showing
/// totals and their composition. Groups stack in group order, the first at the bottom; negatives stack down from zero.
/// Set <see cref="CdsChartAxis.IsPercentage"/> on the value axis for 100% bars. Only the bottom segment shares a baseline,
/// so use <see cref="CdsGroupedBarChart"/> to compare the middle segments, and keep to about five groups.
/// </summary>
public class CdsStackedBarChart : CdsCartesianChart
{
    /// <summary>Bindable property for <see cref="BarWidth"/>.</summary>
    public static readonly BindableProperty BarWidthProperty = BindableProperty.Create(
        nameof(BarWidth), typeof(double?), typeof(CdsStackedBarChart), null, propertyChanged: Changed);

    /// <summary>Bindable property for <see cref="BarMaxWidth"/>.</summary>
    public static readonly BindableProperty BarMaxWidthProperty = BindableProperty.Create(
        nameof(BarMaxWidth), typeof(double), typeof(CdsStackedBarChart), 16d, propertyChanged: Changed);

    /// <summary>Bindable property for <see cref="BarDividerSize"/>.</summary>
    public static readonly BindableProperty BarDividerSizeProperty = BindableProperty.Create(
        nameof(BarDividerSize), typeof(double), typeof(CdsStackedBarChart), 1.5d, propertyChanged: Changed);

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

    /// <summary>
    /// Segments longer than this lose 1px at their far end, so a gap in the plot background separates them (Carbon's
    /// <c>bars.dividerSize</c>). Default 1.5.
    /// </summary>
    public double BarDividerSize
    {
        get => (double)GetValue(BarDividerSizeProperty);
        set => SetValue(BarDividerSizeProperty, value);
    }

    private protected override string ChartKind => "Stacked bar chart";

    private protected override CartesianOptions Options(ChartBuildContext ctx) => new() { Stacked = true };

    private protected override void BuildMarks(ChartScene scene, ChartBuildContext ctx, CartesianLayout cs)
    {
        var count = ChartStackedBars.Add(scene, ctx, cs, ctx.ActivePoints, BarWidth, BarMaxWidth, BarDividerSize, IsRulerEnabled);
        scene.Summary = count == 1 ? "1 stacked bar" : $"{count} stacked bars";
    }

    static void Changed(BindableObject b, object? o, object? n) => ((CdsStackedBarChart)b).RequestRebuild();
}

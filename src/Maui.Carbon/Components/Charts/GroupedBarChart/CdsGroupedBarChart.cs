namespace Maui.Carbon;

/// <summary>
/// Bars for several groups side by side within each category or time point (Carbon's grouped bar chart), for comparing
/// the groups at every point. Give one <see cref="CdsChartPoint"/> per group per category (<see cref="CdsChartPoint.Key"/>
/// on a labels axis, <see cref="CdsChartPoint.Date"/> on a time axis). Compare two to five groups; for one series use
/// <see cref="CdsSimpleBarChart"/>, for part-to-whole <see cref="CdsStackedBarChart"/>. Put the labels axis on the left
/// for horizontal bars. Keep the legend visible: groups are told apart by color and position only.
/// </summary>
public class CdsGroupedBarChart : CdsCartesianChart
{
    /// <summary>Bindable property for <see cref="BarWidth"/>.</summary>
    public static readonly BindableProperty BarWidthProperty = BindableProperty.Create(
        nameof(BarWidth), typeof(double?), typeof(CdsGroupedBarChart), null, propertyChanged: Changed);

    /// <summary>Bindable property for <see cref="BarMaxWidth"/>.</summary>
    public static readonly BindableProperty BarMaxWidthProperty = BindableProperty.Create(
        nameof(BarMaxWidth), typeof(double), typeof(CdsGroupedBarChart), 16d, propertyChanged: Changed);

    /// <summary>Bindable property for <see cref="PreserveSpaceForMissingBars"/>.</summary>
    public static readonly BindableProperty PreserveSpaceForMissingBarsProperty = BindableProperty.Create(
        nameof(PreserveSpaceForMissingBars), typeof(bool), typeof(CdsGroupedBarChart), true, propertyChanged: Changed);

    /// <summary>
    /// A fixed bar thickness (Carbon's <c>bars.width</c>), used when it is no larger than <see cref="BarMaxWidth"/>;
    /// <c>null</c> (default) is min(<see cref="BarMaxWidth"/>, (category step - padding) / groups).
    /// </summary>
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
    /// Whether a category without a value for a group keeps that group's slot (Carbon's
    /// <c>bars.preserveSpaceForMissingBars</c>), so groups stay in the same position in every category. Default <c>true</c>;
    /// <c>false</c> centers only the bars a category has.
    /// </summary>
    public bool PreserveSpaceForMissingBars
    {
        get => (bool)GetValue(PreserveSpaceForMissingBarsProperty);
        set => SetValue(PreserveSpaceForMissingBarsProperty, value);
    }

    private protected override string ChartKind => "Grouped bar chart";

    private protected override void BuildMarks(ChartScene scene, ChartBuildContext ctx, CartesianLayout cs)
    {
        var count = ChartGroupedBars.Add(scene, ctx, cs, ctx.ActivePoints, BarWidth, BarMaxWidth, PreserveSpaceForMissingBars);
        scene.Summary = count == 1 ? "1 bar" : $"{count} bars";
    }

    static void Changed(BindableObject b, object? o, object? n) => ((CdsGroupedBarChart)b).RequestRebuild();
}

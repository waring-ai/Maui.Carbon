namespace Maui.Carbon;

/// <summary>
/// A pie with a hole showing a figure and a label in its center (Carbon's donut chart): by default the total of the shown
/// values over "Total". Use it for parts of a whole whose sum matters (storage used, leads per channel, a budget); one
/// value against a limit is a meter or gauge. Slices, labels, callouts, sort order and colors are a
/// <see cref="CdsPieChart"/>'s; the inner radius is 3/4 of the outer one. Say what the figure means, with its unit, in
/// <see cref="CenterLabel"/>.
/// </summary>
public class CdsDonutChart : CdsPieChart
{
    /// <summary>Bindable property for <see cref="CenterLabel"/>.</summary>
    public static readonly BindableProperty CenterLabelProperty = BindableProperty.Create(
        nameof(CenterLabel), typeof(string), typeof(CdsDonutChart), "Total", propertyChanged: Changed);

    /// <summary>Bindable property for <see cref="CenterNumber"/>.</summary>
    public static readonly BindableProperty CenterNumberProperty = BindableProperty.Create(
        nameof(CenterNumber), typeof(double?), typeof(CdsDonutChart), null, propertyChanged: Changed);

    /// <summary>Bindable property for <see cref="CenterNumberFormatter"/>.</summary>
    public static readonly BindableProperty CenterNumberFormatterProperty = BindableProperty.Create(
        nameof(CenterNumberFormatter), typeof(Func<double, string>), typeof(CdsDonutChart), null, propertyChanged: Changed);

    /// <summary>
    /// The label under the center figure (Carbon's <c>donut.center.label</c>). Default "Total"; empty or <c>null</c> centers
    /// the figure alone. Name what the figure counts, with its unit ("GB used").
    /// </summary>
    public string? CenterLabel
    {
        get => (string?)GetValue(CenterLabelProperty);
        set => SetValue(CenterLabelProperty, value);
    }

    /// <summary>
    /// The center figure (Carbon's <c>donut.center.number</c>); <c>null</c> (default) shows the sum of the shown values, which
    /// follows legend filtering.
    /// </summary>
    public double? CenterNumber
    {
        get => (double?)GetValue(CenterNumberProperty);
        set => SetValue(CenterNumberProperty, value);
    }

    /// <summary>Formats the center figure (Carbon's <c>donut.center.numberFormatter</c>); default: floored and locale formatted.</summary>
    public Func<double, string>? CenterNumberFormatter
    {
        get => (Func<double, string>?)GetValue(CenterNumberFormatterProperty);
        set => SetValue(CenterNumberFormatterProperty, value);
    }

    private protected override string ChartKind => "Donut chart";

    private protected override ChartSkeleton Skeleton => ChartSkeleton.Donut;

    private protected override float InnerRadius(float radius) => radius * 3 / 4;

    private protected override void AddCenter(ChartScene scene, ChartBuildContext ctx, PointF center, float radius)
    {
        var figure = CenterNumber ?? ctx.ActivePoints.Sum(p => p.Value is { } v && !double.IsNaN(v) ? v : 0);
        var text = CenterNumberFormatter is { } f ? f(figure) ?? "" : ctx.Number(Math.Floor(figure));
        var label = CenterLabel;
        var hasLabel = !string.IsNullOrEmpty(label);
        var figureFont = new ChartFont(ChartFontFamily.Condensed, DonutCenter.NumberFontSize(radius));
        scene.Texts.Add(RoundChartText.At(text, center.X, center.Y, figureFont, ChartTextAnchor.Middle,
            hasLabel ? SvgBaseline.Alphabetic : SvgBaseline.Central));
        if (hasLabel)
        {
            var labelFont = new ChartFont(ChartFontFamily.Condensed, DonutCenter.LabelFontSize(radius));
            scene.Texts.Add(RoundChartText.At(label!, center.X, center.Y + DonutCenter.LabelY(radius), labelFont, ChartTextAnchor.Middle));
        }
        scene.Summary += $", {(hasLabel ? label + " " : "")}{text}";
    }

    static void Changed(BindableObject b, object? o, object? n) => ((CdsDonutChart)b).RequestRebuild();
}

/// <summary>Carbon's donut center sizes (configuration.ts donut defaults), scaled by the outer radius.</summary>
internal static class DonutCenter
{
    /// <summary>The figure: min(radius / 100 x 24, 24) px.</summary>
    public static float NumberFontSize(float radius) => Math.Max(1, Math.Min(radius / 100 * 24, 24));

    /// <summary>The label: min(radius / 100 x 15, 15) px.</summary>
    public static float LabelFontSize(float radius) => Math.Max(1, Math.Min(radius / 100 * 15, 15));

    /// <summary>The label's baseline below the center: min(radius / 80 x 20, 20) px.</summary>
    public static float LabelY(float radius) => Math.Min(radius / 80 * 20, 20);
}

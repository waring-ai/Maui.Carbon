namespace Maui.Carbon;

/// <summary>
/// Words sized by a value and colored by group, packed without overlap (Carbon's word cloud chart), for a quick
/// impression of which terms dominate. Each <see cref="CdsChartPoint"/> in <see cref="CdsChart.Data"/> is a word: its
/// <see cref="CdsChartPoint.Key"/> is the word, <see cref="CdsChartPoint.Value"/> sets the font size and
/// <see cref="CdsChartPoint.Group"/> the color and legend item. Words that do not fit are left out; the layout is seeded,
/// so a cloud is the same on every render. A word without a value is an error and nothing is drawn, as in Carbon. Use a
/// bar chart of the top terms to compare values.
/// </summary>
public class CdsWordCloudChart : CdsChart
{
    /// <summary>Bindable property for <see cref="FontSizeRange"/>.</summary>
    public static readonly BindableProperty FontSizeRangeProperty = BindableProperty.Create(
        nameof(FontSizeRange), typeof(Func<Size, (double Min, double Max)>), typeof(CdsWordCloudChart), null, propertyChanged: Changed);

    /// <summary>Bindable property for <see cref="WordLabel"/>.</summary>
    public static readonly BindableProperty WordLabelProperty = BindableProperty.Create(
        nameof(WordLabel), typeof(string), typeof(CdsWordCloudChart), "Word", propertyChanged: Changed);

    /// <summary>Bindable property for <see cref="ValueLabel"/>.</summary>
    public static readonly BindableProperty ValueLabelProperty = BindableProperty.Create(
        nameof(ValueLabel), typeof(string), typeof(CdsWordCloudChart), "Value", propertyChanged: Changed);

    /// <summary>d3-cloud padding around each word.</summary>
    internal const double WordPadding = 5;

    /// <summary>
    /// The smallest and largest font size for a chart size (Carbon's <c>wordCloud.fontSizeRange</c>); <c>null</c> (default)
    /// is Carbon's <c>min(width, height) x [20, 75] / 400</c> of the chart. Sizes scale linearly between the smallest and largest value.
    /// </summary>
    public Func<Size, (double Min, double Max)>? FontSizeRange
    {
        get => (Func<Size, (double Min, double Max)>?)GetValue(FontSizeRangeProperty);
        set => SetValue(FontSizeRangeProperty, value);
    }

    /// <summary>The tooltip and table label of the word (Carbon's <c>tooltip.wordLabel</c>). Default "Word".</summary>
    public string? WordLabel
    {
        get => (string?)GetValue(WordLabelProperty);
        set => SetValue(WordLabelProperty, value);
    }

    /// <summary>The tooltip and table label of the value (Carbon's <c>tooltip.valueLabel</c>). Default "Value".</summary>
    public string? ValueLabel
    {
        get => (string?)GetValue(ValueLabelProperty);
        set => SetValue(ValueLabelProperty, value);
    }

    private protected override string ChartKind => "Word cloud chart";

    /// <summary>A word's font: Plex Sans Condensed, set light (300) above 32px as Carbon's word cloud does.</summary>
    internal static ChartFont WordFont(int size) => new(size > 32 ? ChartFontFamily.CondensedLight : ChartFontFamily.Condensed, size);

    /// <summary>Carbon's default font size range: 20 to 75 per 400px of the chart's smaller side.</summary>
    internal static (double Min, double Max) DefaultRange(Size chart)
    {
        var e = Math.Min(chart.Width, chart.Height);
        return (e * 20 / 400, e * 75 / 400);
    }

    /// <summary>Carbon's getFontSizeScale: linear from the value extent to the size range (the middle of the range for one value).</summary>
    internal static Func<double, double> FontSizeScale(IEnumerable<double> values, (double Min, double Max) range)
    {
        var valid = values.Where(v => v != 0 && !double.IsNaN(v)).ToList();
        if (valid.Count == 0) return _ => 4;
        var lo = valid.Min();
        var hi = valid.Max();
        if (hi == lo) return _ => (range.Min + range.Max) / 2;
        return v => range.Min + (v - lo) / (hi - lo) * (range.Max - range.Min);
    }

    private protected override ChartScene BuildScene(ChartBuildContext ctx)
    {
        var scene = new ChartScene { Width = ctx.Width, Height = ctx.Height, Plot = new RectF(0, 0, ctx.Width, ctx.Height) };
        var rows = ctx.ActivePoints;
        if (rows.Any(p => p.Value is null || double.IsNaN(p.Value.Value)))
        {
            System.Diagnostics.Trace.TraceError("Badly formatted WordCloud data. `value` should only be an integer or float");
            scene.Summary = "No data";
            return scene;
        }
        // Carbon sizes the words from the whole chart (its holder), not just the graph.
        var holder = new Size(Width > 0 ? Width : ctx.Width, Height > 0 ? Height : ctx.Height);
        var range = (FontSizeRange ?? DefaultRange)(holder);
        var scale = FontSizeScale(rows.Select(p => p.Value!.Value), range);
        var words = rows.Select((p, i) => new CloudWord(p.Key ?? "", (int)scale(p.Value!.Value), i)).ToList();
        var seed = ChartRandom.Hash(string.Join("|", rows.Select(p => (p.Key ?? "") + ":" + ctx.Number(p.Value))) + "@" + (int)ctx.Width + "x" + (int)ctx.Height);
        var placed = ChartWordCloud.Layout(words, ctx.Width, ctx.Height, WordPadding, ChartRandom.Seeded(seed), w => WordFont(w.Size));

        var wordLabel = WordLabel ?? "Word";
        var valueLabel = ValueLabel ?? "Value";
        // Marks in reading order: top to bottom, then left to right.
        var id = 0;
        foreach (var w in placed.OrderBy(w => w.Y + w.Y0).ThenBy(w => w.X))
        {
            var p = rows[w.Index];
            var font = WordFont(w.Size);
            var mark = id++;
            scene.Texts.Add(new ChartText
            {
                Text = w.Text,
                X = (float)w.X,
                Y = ChartDraw.MiddleForBaseline((float)w.Y, font),
                Anchor = ChartTextAnchor.Middle,
                Baseline = ChartTextBaseline.Middle,
                Font = font,
                Color = ChartPaint.OfSeries(p.Group),
                Group = p.Group,
                FadesWithLegend = true,
            });
            var ink = new RectF((float)(w.X + w.X0 + WordPadding), (float)(w.Y + w.Y0 + WordPadding), (float)(w.X1 - w.X0 - 2 * WordPadding), (float)(w.Y1 - w.Y0 - 2 * WordPadding));
            scene.Marks.Add(new ChartMark
            {
                Id = mark,
                Bounds = ink,
                Group = p.Group,
                Rows =
                [
                    new ChartTooltipRow(wordLabel, w.Text),
                    new ChartTooltipRow(valueLabel, ctx.Value(p.Value)),
                    new ChartTooltipRow(ctx.GroupLabel, p.Group ?? "", ChartPaint.OfSeries(p.Group)),
                ],
                Description = $"{w.Text}: {ctx.Value(p.Value)}" + (string.IsNullOrEmpty(p.Group) ? "" : $", {p.Group}"),
            });
        }
        scene.Summary = id == 1 ? "1 word" : $"{id} words";
        return scene;
    }

    private protected override ChartTable BuildTable(ChartBuildContext ctx) => new(
        [WordLabel ?? "Word", ctx.GroupLabel, ValueLabel ?? "Value"],
        ctx.ActivePoints.Select(p => new[] { p.Key ?? "", p.Group ?? "", ctx.Value(p.Value) }).ToList());

    static void Changed(BindableObject b, object? o, object? n) => ((CdsWordCloudChart)b).RequestRebuild();
}

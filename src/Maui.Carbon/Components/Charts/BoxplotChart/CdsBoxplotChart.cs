namespace Maui.Carbon;

/// <summary>
/// Boxes that summarize each group's distribution (Carbon's box plot): the box spans Q1 to Q3 with the median across it,
/// whiskers reach the last values within 1.5 x IQR, and the values beyond are outliers. Give one
/// <see cref="CdsChartPoint"/> per observation (<see cref="CdsChartPoint.Group"/> and <see cref="CdsChartPoint.Value"/>);
/// the chart computes the statistics. Box plots are monochrome (Carbon's 1-color pairing; <see cref="CdsChart.ColorPairingOption"/>
/// 1 to 4 and <see cref="CdsChart.ColorScale"/> still apply) and have no legend. A labels axis on the left makes horizontal
/// boxes. Use it with about ten or more observations per group, and explain "box = middle 50%" near the chart for general
/// audiences.
/// </summary>
public class CdsBoxplotChart : CdsCartesianChart
{
    const float BoxOpacity = 0.3f;
    const float BoxHoverOpacity = 0.5f;
    const float OutlierRadius = 4;

    private protected override string ChartKind => "Box plot";

    private protected override bool DefaultLegendEnabled => false;

    private protected override IReadOnlyList<string> LegendGroups => [];

    /// <summary>
    /// One color group: the chart's color pairing then uses the 1-color palette for every box (Carbon's monochrome box
    /// plot), and series paints of real group names resolve to it.
    /// </summary>
    private protected override IReadOnlyList<string> GroupsOf(IReadOnlyList<object> items) => [""];

    /// <summary>The observations with the group as their category (Carbon maps the labels axis to the group).</summary>
    static List<CdsChartPoint> Observations(ChartBuildContext ctx) =>
        ctx.Points.Select(p => new CdsChartPoint { Group = p.Group ?? "", Value = p.Value }).ToList();

    internal static List<ChartBoxplotStats> Statistics(IReadOnlyList<CdsChartPoint> rows)
    {
        var groups = new List<string>();
        var values = new Dictionary<string, List<double>>(StringComparer.Ordinal);
        foreach (var p in rows)
        {
            var g = p.Group ?? "";
            if (!values.TryGetValue(g, out var list))
            {
                values[g] = list = [];
                groups.Add(g);
            }
            if (p.Value is { } v) list.Add(v);
        }
        return groups.Select(g => ChartBoxplotStats.Compute(g, values[g])).OfType<ChartBoxplotStats>().ToList();
    }

    private protected override CartesianOptions Options(ChartBuildContext ctx) => new() { Rows = Observations(ctx) };

    private protected override void BuildMarks(ChartScene scene, ChartBuildContext ctx, CartesianLayout cs)
    {
        if (!cs.IsBand) return;
        var rows = cs.Rows;
        var stats = Statistics(rows);
        var allGroups = rows.Select(p => p.Group ?? "").Distinct().Count();
        var len = cs.Vertical ? cs.Plot.Width : cs.Plot.Height;
        var grid = Math.Floor(len / Math.Max(1, allGroups));
        var bw = Math.Min(grid / 2, 16);
        var band = cs.DomainScale.Band!;
        var s = cs.RangeScale;
        var id = 0;
        var boxes = 0;
        foreach (var b in stats)
        {
            var g = b.Group;
            var x = band.Center(g);
            if (double.IsNaN(x)) continue;
            var paint = ChartPaint.OfSeries(g);
            ChartLine Line(double d0, double r0, double d1, double r1, float width) => new()
            {
                From = cs.XY(d0, r0),
                To = cs.XY(d1, r1),
                Stroke = paint,
                StrokeWidth = width,
                Group = g,
                Hover = ChartHoverStyle.None,
            };
            double yMin = s.Map(b.WhiskerMin), yMax = s.Map(b.WhiskerMax), y1 = s.Map(b.Q1), y3 = s.Map(b.Q3), yMed = s.Map(b.Median);
            scene.Shapes.Add(Line(x, yMin, x, y1, 1));
            scene.Shapes.Add(Line(x, yMax, x, y3, 1));
            var boxId = id++;
            scene.Shapes.Add(new ChartRect
            {
                Rect = ChartBars.Rect(x - bw / 2, x + bw / 2, Math.Min(y1, y3), Math.Max(y1, y3), cs.Vertical),
                Fill = paint,
                FillOpacity = BoxOpacity,
                HoverFillOpacity = BoxHoverOpacity,
                Stroke = paint,
                StrokeWidth = 1,
                Hover = ChartHoverStyle.None,
                Group = g,
                Mark = boxId,
            });
            scene.Shapes.Add(Line(x - bw / 4, yMin, x + bw / 4, yMin, 2));
            scene.Shapes.Add(Line(x - bw / 2, yMed, x + bw / 2, yMed, 2));
            scene.Shapes.Add(Line(x - bw / 4, yMax, x + bw / 4, yMax, 2));
            var area = ChartBars.Rect(x - bw / 2, x + bw / 2, yMin, yMax, cs.Vertical);
            scene.Marks.Add(new ChartMark
            {
                Id = boxId,
                Bounds = area,
                Anchor = cs.XY(x + bw / 2, yMed),
                Group = g,
                Rows =
                [
                    new ChartTooltipRow(ctx.GroupLabel, CdsChartTruncate.Apply(g), paint),
                    new ChartTooltipRow("Minimum", ctx.Value(b.WhiskerMin)),
                    new ChartTooltipRow("Q1", ctx.Value(b.Q1)),
                    new ChartTooltipRow("Median", ctx.Value(b.Median)),
                    new ChartTooltipRow("Q3", ctx.Value(b.Q3)),
                    new ChartTooltipRow("Maximum", ctx.Value(b.WhiskerMax)),
                    new ChartTooltipRow("IQR", ctx.Value(b.Iqr)),
                ],
                Description = $"{g}: median {ctx.Value(b.Median)}, Q1 {ctx.Value(b.Q1)}, Q3 {ctx.Value(b.Q3)}, minimum {ctx.Value(b.WhiskerMin)}, maximum {ctx.Value(b.WhiskerMax)}"
                    + (b.Outliers.Count switch { 0 => "", 1 => ", 1 outlier", var n => $", {n} outliers" }),
            });
            boxes++;
            foreach (var v in b.Outliers)
            {
                var c = cs.XY(x, s.Map(v));
                var mark = id++;
                scene.Shapes.Add(new ChartCircle
                {
                    Center = c,
                    Radius = OutlierRadius,
                    Fill = paint,
                    FillOpacity = BoxOpacity,
                    HoverFillOpacity = 1,
                    Stroke = paint,
                    StrokeWidth = 1,
                    Hover = ChartHoverStyle.None,
                    Group = g,
                    Mark = mark,
                });
                scene.Marks.Add(new ChartMark
                {
                    Id = mark,
                    Bounds = new RectF(c.X - OutlierRadius, c.Y - OutlierRadius, 2 * OutlierRadius, 2 * OutlierRadius),
                    Radius = OutlierRadius + 2,
                    Anchor = c,
                    Group = g,
                    Rows = [new ChartTooltipRow(ctx.GroupLabel, CdsChartTruncate.Apply(g), paint), new ChartTooltipRow("Outlier", ctx.Value(v))],
                    Description = $"{g}, outlier {ctx.Value(v)}",
                });
            }
        }
        scene.Summary = boxes == 1 ? "1 box" : $"{boxes} boxes";
    }

    /// <summary>Carbon's box plot table: Group, Minimum, Q1, Median, Q3, Maximum, IQR and the outliers of every group.</summary>
    private protected override ChartTable BuildTable(ChartBuildContext ctx)
    {
        const string dash = "–";
        string N(double v) => double.IsNaN(v) ? dash : ctx.Value(v);
        var rows = Statistics(Observations(ctx)).Select(b => new[]
        {
            b.Group, N(b.WhiskerMin), N(b.Q1), N(b.Median), N(b.Q3), N(b.WhiskerMax), N(b.Iqr),
            b.Outliers.Count > 0 ? string.Join(", ", b.Outliers.Select(o => ctx.Value(o))) : dash,
        }).ToList();
        return new ChartTable([ctx.GroupLabel, "Minimum", "Q1", "Median", "Q3", "Maximum", "IQR", "Outlier(s)"], rows);
    }
}

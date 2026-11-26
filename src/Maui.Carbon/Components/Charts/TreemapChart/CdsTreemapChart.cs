namespace Maui.Carbon;

/// <summary>
/// Nested rectangles that show part-to-whole within a two-level hierarchy (Carbon's treemap chart): each top-level
/// <see cref="CdsChartNode"/> in <see cref="CdsChart.Data"/> is a group (a legend item and one palette color) and each
/// leaf is sized by its value. Leaves are squarified, sorted largest first, with 1px gaps. Set
/// <see cref="CdsChartNode.ShowLabel"/> only on leaves large enough to hold their name. Use a bar chart when similar
/// values must be compared precisely, and a donut chart for fewer than about five parts.
/// </summary>
public class CdsTreemapChart : CdsChart
{
    /// <summary>The hovered leaf darkens like Carbon's <c>brightness(0.779)</c> filter: black over it at 22.1%.</summary>
    const float HoverDarken = 0.221f;

    private protected override string ChartKind => "Treemap chart";

    private protected override IReadOnlyList<string> GroupsOf(IReadOnlyList<object> items) => TopLevelNames(items);

    internal static IReadOnlyList<string> TopLevelNames(IReadOnlyList<object> items)
    {
        var list = new List<string>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var item in items)
            if (item is CdsChartNode { Name: { } name } && seen.Add(name)) list.Add(name);
        return list;
    }

    internal static List<CdsChartNode> ActiveNodes(ChartBuildContext ctx) =>
        ctx.Items.OfType<CdsChartNode>().Where(n => ctx.IsActive(n.Name)).ToList();

    private protected override bool IsEmpty(IReadOnlyList<object> items) => !items.OfType<CdsChartNode>().Any();

    /// <inheritdoc/>
    protected override void OnContextChanged()
    {
        base.OnContextChanged();
        // Label colors are chosen from the fill each theme gives a group: choose again (once a scene exists; the
        // context is first resolved while the base constructor runs).
        if (Scene != ChartScene.Empty) RequestRebuild();
    }

    private protected override ChartScene BuildScene(ChartBuildContext ctx)
    {
        var scene = new ChartScene { Width = ctx.Width, Height = ctx.Height, Plot = new RectF(0, 0, ctx.Width, ctx.Height) };
        var root = HierarchyNode.Build(Title ?? "Treemap", ActiveNodes(ctx)).Sum().SortByValueDescending();
        ChartSquarify.Treemap(root, ctx.Width, ctx.Height);
        var id = 0;
        foreach (var leaf in root.Leaves())
        {
            if (leaf.Depth == 0) continue;
            var group = leaf.TopParent().Name;
            var rect = new RectF((float)leaf.X0, (float)leaf.Y0, (float)Math.Max(0, leaf.X1 - leaf.X0), (float)Math.Max(0, leaf.Y1 - leaf.Y0));
            var mark = id++;
            scene.Shapes.Add(new ChartRect { Rect = rect, Fill = ChartPaint.OfSeries(group), Group = group, Mark = mark, Hover = ChartHoverStyle.None });
            scene.Shapes.Add(new ChartRect
            {
                Rect = rect, Fill = ChartPaint.OfToken(ctx.Colors.Palette.IsDark ? CdsChartToken.Layer01Absolute : CdsChartToken.LayerInverseAbsolute),
                FillOpacity = HoverDarken, HoverOnly = true, Group = group, Mark = mark, Hover = ChartHoverStyle.None,
            });
            if (leaf.Data?.ShowLabel == true && LabelText(leaf.Name, rect) is { } text)
                scene.Texts.Add(new ChartText
                {
                    Text = text,
                    X = rect.X + 7,
                    Y = rect.Y + ChartDraw.MiddleForBaseline(18, ChartFont.Tick),
                    Baseline = ChartTextBaseline.Middle,
                    Color = ChartDraw.ContrastText(ctx.Colors, ctx.Colors.Series(group)),
                    Group = group,
                    FadesWithLegend = true,
                });
            var name = leaf.Name;
            scene.Marks.Add(new ChartMark
            {
                Id = mark,
                Bounds = rect,
                Group = group,
                Rows =
                [
                    new ChartTooltipRow(group, null, ChartPaint.OfSeries(group), Bold: true),
                    new ChartTooltipRow(name, ctx.Value(leaf.Data?.Value)),
                ],
                Description = leaf.Data?.Description ?? $"{name}: {ctx.Value(leaf.Value)}" + (group != name ? $", {group}" : ""),
            });
        }
        scene.Summary = id == 1 ? "1 leaf" : $"{id} leaves";
        return scene;
    }

    /// <summary>
    /// The label as it shows inside the leaf: Carbon clips it at the leaf's edges (it never moves), so characters past the
    /// right edge are cut and leaves too short for the 18px baseline show none.
    /// </summary>
    internal static string? LabelText(string name, RectF leaf)
    {
        if (leaf.Height < 14 || leaf.Width < 12 || string.IsNullOrEmpty(name)) return null;
        var room = leaf.Width - 7;
        if (CdsChartTextMetrics.Width(name, ChartFont.Tick) <= room) return name;
        var n = name.Length;
        while (n > 0 && CdsChartTextMetrics.Width(name[..n], ChartFont.Tick) > room) n--;
        return n == 0 ? null : name[..n];
    }

    private protected override ChartTable BuildTable(ChartBuildContext ctx)
    {
        var rows = new List<string[]>();
        foreach (var d in ActiveNodes(ctx))
        {
            if (d.Children is { Count: > 0 } children)
                foreach (var c in children) rows.Add([c.Name ?? "", d.Name ?? "", c.Value is null ? "–" : ctx.Value(c.Value)]);
            else if (d.Name is not null && d.Value is { } v && v != 0)
                rows.Add(["–", d.Name, ctx.Value(v)]);
        }
        return new ChartTable(["Child", ctx.GroupLabel, "Value"], rows);
    }
}

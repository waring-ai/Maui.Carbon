using System.Globalization;

namespace Maui.Carbon.Tests.Components;

public class PieChartTests
{
    static List<CdsChartPoint> Tickets() =>
    [
        new() { Group = "Resolved", Value = 412 }, new() { Group = "In progress", Value = 138 },
        new() { Group = "Waiting on customer", Value = 76 }, new() { Group = "Escalated", Value = 9 }, new() { Group = "Reopened", Value = 4 },
    ];

    static CdsPieChart Chart(IEnumerable<CdsChartPoint>? data = null) => new() { Data = (data ?? Tickets()).ToList(), Culture = CultureInfo.GetCultureInfo("en-US") };

    [Fact]
    public void D3_pie_layout_fills_the_circle_with_pads_in_input_order()
    {
        var arcs = PieLayout.Arcs([3, 1], padAngle: 0.007f);
        Assert.Equal(0, arcs[0].Start);
        Assert.Equal(arcs[0].End, arcs[1].Start, 4);
        Assert.Equal(MathF.PI * 2, arcs[1].End, 4);
        var k = (MathF.PI * 2 - 2 * 0.007f) / 4;
        Assert.Equal(3 * k + 0.007f, arcs[0].End - arcs[0].Start, 4);
        Assert.Equal([0, 1], arcs.Select(a => a.Order));
    }

    [Fact]
    public void Pie_layout_orders_by_comparison_and_skips_non_positive_values()
    {
        var arcs = PieLayout.Arcs([1, 2, 3], (a, b) => b.CompareTo(a), 0);
        Assert.Equal([2, 1, 0], arcs.Select(a => a.Order));
        Assert.Equal(0, arcs[2].Start);
        var zero = PieLayout.Arcs([0, 5], padAngle: 0);
        Assert.Equal(zero[0].Start, zero[0].End);
        Assert.Empty(PieLayout.Arcs([]));
    }

    [Fact]
    public void Percent_rounds_to_one_decimal()
    {
        Assert.Equal(64.5, PieLayout.Percent(412, 639));
        Assert.Equal(50, PieLayout.Percent(1, 2));
        Assert.Equal(0, PieLayout.Percent(1, 0));
        Assert.Equal(100.0 / 3, PieLayout.Percent(1, 3, exact: true), 6);
    }

    [Fact]
    public void Sorted_is_by_value_descending_and_stable()
    {
        var rows = PieLayout.Sorted([new() { Group = "a", Value = 1 }, new() { Group = "b", Value = 5 }, new() { Group = "c", Value = 1 }, new() { Group = "d" }]);
        Assert.Equal(["b", "a", "c", "d"], rows.Select(r => r.Group));
    }

    [Fact]
    public void Slices_follow_values_and_carbon_geometry()
    {
        var chart = Chart();
        var scene = chart.RenderScene(600, 400);
        Assert.Equal(5, scene.Marks.Count);
        Assert.Equal("5 slices", scene.Summary);
        // Two paths per slice: the slice and its 3px larger hover arc.
        var paths = scene.Shapes.OfType<ChartPath>().ToList();
        Assert.Equal(10, paths.Count);
        Assert.Equal(5, paths.Count(p => p.HoverOnly));
        // Radius: half the smaller side minus 15; the pie sits 30 + 7 from the left, 20 (+10 with callouts) from the top.
        var radius = 400 / 2f - 15;
        var first = scene.Marks[0];
        Assert.Equal(30 + 7, first.Bounds.Left, 1);
        Assert.Equal(20 + 10, first.Bounds.Top, 1);
        Assert.Equal(radius * 2, first.Bounds.Width, 1);
        Assert.Equal(["Resolved", "In progress", "Waiting on customer", "Escalated", "Reopened"], scene.Marks.Select(m => m.Group));
        Assert.Equal("412", first.Rows.Single().Value);
        Assert.Equal("Resolved", first.Rows.Single().Label);
        Assert.Equal("412, 64.5%, Resolved", first.Description);
    }

    [Fact]
    public void Labels_show_percentages_and_small_slices_get_callouts()
    {
        var scene = Chart().RenderScene(600, 400);
        Assert.Equal(["64.5%", "21.6%", "11.9%", "1.4%", "0.6%"], scene.Texts.Select(t => t.Text));
        // Only Reopened (0.6%) is under 5 degrees (Escalated is 5.1 with its pad): one callout of two lines.
        Assert.Equal(2, scene.Shapes.OfType<ChartLine>().Count());
        var center = scene.Marks[0].Bounds.Center;
        var reopened = scene.Texts[4];
        var escalated = scene.Texts[3];
        Assert.True(reopened.X > escalated.X);
        Assert.True(reopened.Y < escalated.Y);
        Assert.True(reopened.Y < center.Y);
    }

    [Fact]
    public void Without_labels_the_pie_fills_and_centers()
    {
        var chart = Chart();
        chart.ShowLabels = false;
        chart.Alignment = CdsChartAlignment.Center;
        var scene = chart.RenderScene(600, 400);
        Assert.Empty(scene.Texts);
        var b = scene.Marks[0].Bounds;
        Assert.Equal(400, b.Width, 1);
        Assert.Equal(300 + 7, b.Center.X, 1);
        Assert.Equal(0, b.Top, 1);
    }

    [Fact]
    public void Right_alignment_keeps_carbons_30px_offset()
    {
        var chart = Chart();
        chart.Alignment = CdsChartAlignment.Right;
        var b = chart.RenderScene(600, 400).Marks[0].Bounds;
        Assert.Equal(600 - 30 + 7, b.Right, 1);
    }

    [Fact]
    public void Sort_comparison_changes_slice_order_but_not_colors()
    {
        var chart = Chart();
        chart.SortComparison = (a, b) => string.CompareOrdinal(a.Group, b.Group);
        var scene = chart.RenderScene(600, 400);
        Assert.Equal(["Escalated", "In progress", "Reopened", "Resolved", "Waiting on customer"], scene.Marks.Select(m => m.Group));
        // The table (and colors) keep the value order.
        Assert.Equal("Resolved", chart.RenderTable().Rows[0][0]);
    }

    [Fact]
    public void Formatter_receives_the_exact_percentage()
    {
        var chart = Chart();
        chart.LabelFormatter = (p, pct) => $"{p.Group}: {pct:0.00}";
        var scene = chart.RenderScene(600, 400);
        Assert.Contains(scene.Texts, t => t.Text == "Resolved: 64.48");
    }

    [Fact]
    public void Legend_selection_recomputes_percentages()
    {
        var chart = Chart();
        chart.RenderScene(600, 400);
        IReadOnlyList<string>? raised = null;
        chart.SelectedGroupsChanged += (_, e) => raised = e.SelectedGroups;
        chart.ToggleGroup("Resolved");
        chart.ToggleGroup("In progress");
        Assert.Equal(["Resolved", "In progress"], raised);
        var scene = chart.RenderScene(600, 400);
        Assert.Equal(2, scene.Marks.Count);
        Assert.Equal(["74.9%", "25.1%"], scene.Texts.Select(t => t.Text));
    }

    [Fact]
    public void Zero_and_negative_rows_are_not_drawn_but_are_listed()
    {
        var chart = Chart([new() { Group = "A", Value = 5 }, new() { Group = "B", Value = 0 }, new() { Group = "C", Value = -2 }, new() { Group = "D" }]);
        Assert.Single(chart.RenderScene(400, 400).Marks);
        var table = chart.RenderTable();
        Assert.Equal(["Group", "Value"], table.Headers);
        Assert.Equal(4, table.Rows.Count);
        Assert.Contains(table.Rows, r => r.SequenceEqual(["D", "–"]));
        Assert.Equal("C", table.Rows[3][0]);
    }

    [Fact]
    public void Table_is_sorted_by_value()
    {
        var table = Chart([new() { Group = "Small", Value = 1 }, new() { Group = "Big", Value = 1200 }]).RenderTable();
        Assert.Equal(["Big", "1,200"], table.Rows[0]);
    }

    [Fact]
    public void Loading_and_empty_show_the_pie_skeleton()
    {
        var chart = new CdsPieChart { IsLoading = true };
        var loading = chart.RenderScene(400, 300);
        Assert.True(loading.Shimmer);
        Assert.Empty(loading.Marks);
        chart.IsLoading = false;
        var empty = chart.RenderScene(400, 300);
        Assert.False(empty.Shimmer);
        Assert.Equal("No data", empty.Summary);
        Assert.Single(empty.Shapes.OfType<ChartPath>());
    }

    [Fact]
    public void Hit_test_finds_the_slice_under_the_point()
    {
        var scene = Chart().RenderScene(600, 400);
        var resolved = scene.Marks[0];
        Assert.Same(resolved, scene.HitTest(resolved.AnchorPoint));
        Assert.Null(scene.HitTest(new PointF(590, 10)));
    }

    [Fact]
    public void Setting_every_property_does_not_throw()
    {
        var chart = new CdsPieChart
        {
            Title = "Tickets",
            Data = Tickets(),
            ShowLabels = false,
            LabelFormatter = (_, pct) => pct.ToString("0"),
            Alignment = CdsChartAlignment.Right,
            SortComparison = (a, b) => 0,
            LegendPosition = CdsChartLegendPosition.Right,
            IsLegendEnabled = false,
            ColorPairingOption = 2,
            ValueFormat = "N0",
            IsToolbarEnabled = false,
        };
        Assert.NotNull(chart.RenderScene(500, 300));
        Assert.NotNull(chart.RenderScene(10, 10));
    }

    [Fact]
    public void Plain_labels_step_off_a_callout_line()
    {
        var font = ChartFont.Tick;
        // A callout rising to the right from (0, 0), and a plain label whose text sits on its start.
        var callout = new PieCallout(new PointF(0, 0), new PointF(7, -12), new PointF(15, -12));
        List<PieLabel> labels = [new(0, "1.4%", new PointF(-6, 2), null), new(1, "0.6%", new PointF(32, -12), callout)];
        var result = PieLayout.Separate(labels, font);
        Assert.Equal(labels[1], result[1]);
        var moved = result[0];
        Assert.True(moved.At.X < labels[0].At.X);
        Assert.True(moved.At.X + CdsChartTextMetrics.Width("1.4%", font) / 2 < 0);
        // Labels that touch nothing stay where Carbon puts them.
        List<PieLabel> apart = [new(0, "50%", new PointF(-80, 0), null), new(1, "50%", new PointF(80, 0), null)];
        Assert.Equal(apart, PieLayout.Separate(apart, font));
    }

    [Theory]
    [InlineData(600, 400)]
    [InlineData(400, 300)]
    [InlineData(300, 260)]
    [InlineData(240, 240)]
    public void Plain_labels_do_not_touch_callout_lines_or_each_other(float width, float height)
    {
        var scene = Chart().RenderScene(width, height);
        var font = ChartFont.Tick;
        RectF Box(ChartText t) => new(t.X - t.Width / 2, t.Y - 6, t.Width, 12);
        var labels = scene.Texts.Where(t => t.Text.EndsWith('%')).ToList();
        var lines = scene.Shapes.OfType<ChartLine>().Where(l => l.Stroke == ChartPaint.OfKey(CdsColorKeys.TextSecondary)).ToList();
        Assert.Equal(2, lines.Count);
        foreach (var t in labels.Where(t => t.Text != "0.6%"))
        {
            var box = Box(t);
            foreach (var l in lines)
                for (var s = 0; s <= 20; s++)
                {
                    var p = new PointF(l.From.X + (l.To.X - l.From.X) * s / 20, l.From.Y + (l.To.Y - l.From.Y) * s / 20);
                    Assert.False(box.Contains(p), $"{t.Text} touches a callout at {p}");
                }
            foreach (var o in labels)
                if (!ReferenceEquals(o, t)) Assert.False(box.IntersectsWith(Box(o)), $"{t.Text} touches {o.Text}");
        }
    }
}

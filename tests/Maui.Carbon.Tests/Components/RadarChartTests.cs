using System.Globalization;

namespace Maui.Carbon.Tests.Components;

public class RadarChartTests
{
    static readonly string[] Keys = ["Frontend", "Backend", "Testing", "DevOps", "Design", "Security"];

    static List<CdsChartPoint> Skills() =>
        new[] { ("Team A", new double[] { 72, 64, 88, 55, 80, 61 }), ("Team B", new double[] { 58, 82, 69, 77, 52, 74 }) }
            .SelectMany(s => s.Item2.Select((v, i) => new CdsChartPoint(s.Item1, Keys[i], v))).ToList();

    static CdsRadarChart Chart() => new() { Data = Skills(), Culture = CultureInfo.GetCultureInfo("en-US") };

    [Fact]
    public void Label_placement_follows_angle_utils()
    {
        Assert.Equal((ChartTextAnchor.Middle, SvgBaseline.Alphabetic), RadarLayout.LabelPlacement(-Math.PI / 2));
        Assert.Equal((ChartTextAnchor.Start, SvgBaseline.Middle), RadarLayout.LabelPlacement(0));
        Assert.Equal((ChartTextAnchor.Middle, SvgBaseline.Hanging), RadarLayout.LabelPlacement(Math.PI / 2));
        Assert.Equal((ChartTextAnchor.End, SvgBaseline.Middle), RadarLayout.LabelPlacement(Math.PI));
        Assert.Equal((ChartTextAnchor.Start, SvgBaseline.Hanging), RadarLayout.LabelPlacement(Math.PI / 6));
        Assert.Equal((ChartTextAnchor.End, SvgBaseline.Hanging), RadarLayout.LabelPlacement(5 * Math.PI / 6));
        Assert.Equal((ChartTextAnchor.End, SvgBaseline.Alphabetic), RadarLayout.LabelPlacement(-5 * Math.PI / 6));
        Assert.Equal((ChartTextAnchor.Start, SvgBaseline.Alphabetic), RadarLayout.LabelPlacement(-Math.PI / 6));
    }

    [Fact]
    public void Scale_is_nice_from_zero_and_honours_max_value()
    {
        var (min, max, ticks) = RadarLayout.Scale([52, 88], null);
        Assert.Equal(0, min);
        Assert.Equal(100, max);
        Assert.Equal([0, 20, 40, 60, 80, 100], ticks);
        Assert.Equal(10, RadarLayout.Scale([6.5, 9.1], 10).Max);
        // A max value below the data is ignored.
        Assert.Equal(100, RadarLayout.Scale([52, 88], 50).Max);
        Assert.Equal(-10, RadarLayout.Scale([-7, 20], null).Min);
        Assert.Equal(10, RadarLayout.Map(0, 0, 100, 200));
        Assert.Equal(200, RadarLayout.Map(100, 0, 100, 200));
    }

    [Fact]
    public void Spokes_start_at_12_oclock_and_run_clockwise()
    {
        Assert.Equal(-Math.PI / 2, RadarLayout.Angle(0, 6), 6);
        Assert.Equal(-Math.PI / 2 + Math.PI / 3, RadarLayout.Angle(1, 6), 6);
        var p = RadarLayout.Polar(RadarLayout.Angle(0, 4), 10, new PointF(50, 50));
        Assert.Equal(50, p.X, 3);
        Assert.Equal(40, p.Y, 3);
    }

    [Fact]
    public void Scene_has_rings_spokes_shapes_and_one_mark_per_spoke()
    {
        var scene = Chart().RenderScene(600, 400);
        Assert.Equal(6, scene.Marks.Count);
        Assert.Equal("2 groups on 6 axes", scene.Summary);
        // 6 rings (0..100 by 20) + 2 groups x (fill + stroke).
        var paths = scene.Shapes.OfType<ChartPath>().ToList();
        Assert.Equal(6 + 4, paths.Count);
        var fill = paths.First(p => p.Group == "Team A");
        Assert.Equal(0.3f, fill.FillOpacity);
        Assert.Equal(0.1f / 0.3f, fill.LegendFade, 4);
        // Spokes: 6 plain and 6 dashed hover lines; dots per spoke per group, shown on hover.
        Assert.Equal(6, scene.Shapes.OfType<ChartLine>().Count(l => !l.HoverOnly));
        Assert.All(scene.Shapes.OfType<ChartLine>().Where(l => l.HoverOnly), l => Assert.Equal([4f, 4f], l.Dash!));
        Assert.Equal(12, scene.Shapes.OfType<ChartCircle>().Count(c => c.HoverOnly && c.Radius == 5));
        var frontend = scene.Marks[0];
        Assert.Equal(["Team A", "Team B"], frontend.Rows.Select(r => r.Label));
        Assert.Equal(["72", "58"], frontend.Rows.Select(r => r.Value));
        Assert.Equal("Frontend: Team A 72, Team B 58", frontend.Description);
        // Labels: every key and the innermost and outermost tick.
        Assert.Equal(Keys.Concat(["0", "100"]), scene.Texts.Select(t => t.Text));
    }

    [Fact]
    public void Radius_leaves_room_for_labels_and_the_top_label_sits_above()
    {
        var scene = Chart().RenderScene(600, 400);
        var radius = (400 - 2 * (16 + 8)) / 2f;
        var ring = scene.Shapes.OfType<ChartPath>().Take(6).Last().Path.Bounds;
        Assert.Equal(radius * 2, ring.Height, 1);
        var top = scene.Texts[0];
        Assert.Equal(ChartTextAnchor.Middle, top.Anchor);
        Assert.True(top.Y < ring.Top);
    }

    [Fact]
    public void Alignment_moves_the_radar()
    {
        var chart = Chart();
        var left = chart.RenderScene(800, 400).Marks[0].AnchorPoint.X;
        chart.Alignment = CdsChartAlignment.Center;
        var center = chart.RenderScene(800, 400).Marks[0].AnchorPoint.X;
        chart.Alignment = CdsChartAlignment.Right;
        var right = chart.RenderScene(800, 400).Marks[0].AnchorPoint.X;
        Assert.True(left < center && center < right);
        Assert.InRange(center, 380, 420);
    }

    [Fact]
    public void Hit_test_finds_the_spoke()
    {
        var scene = Chart().RenderScene(600, 400);
        var backend = scene.Marks[1];
        Assert.Same(backend, scene.HitTest(backend.AnchorPoint));
    }

    [Fact]
    public void Missing_values_are_skipped_and_the_scale_ignores_the_legend()
    {
        var data = Skills();
        data.RemoveAll(p => p.Group == "Team B" && p.Key == "Testing");
        var chart = new CdsRadarChart { Data = data };
        var scene = chart.RenderScene(600, 400);
        Assert.Single(scene.Marks[2].Rows);
        var table = chart.RenderTable();
        Assert.Equal(["Group", .. Keys], table.Headers);
        Assert.Equal("–", table.Rows[1][3]);
        chart.ToggleGroup("Team B");
        var filtered = chart.RenderScene(600, 400);
        Assert.Equal(["0", "100"], filtered.Texts.Skip(6).Select(t => t.Text));
        Assert.Single(chart.RenderTable().Rows);
    }

    [Fact]
    public void Max_value_raises_the_outer_ring()
    {
        var chart = Chart();
        chart.MaxValue = 200;
        Assert.Contains(chart.RenderScene(600, 400).Texts, t => t.Text == "200");
    }

    [Fact]
    public void Loading_and_empty_show_the_pie_skeleton()
    {
        var chart = new CdsRadarChart { IsLoading = true };
        Assert.True(chart.RenderScene(400, 300).Shimmer);
        chart.IsLoading = false;
        Assert.Equal("No data", chart.RenderScene(400, 300).Summary);
        // Too small for any radius: nothing drawn, nothing thrown.
        Assert.Empty(Chart().RenderScene(40, 40).Marks);
    }

    [Fact]
    public void Setting_every_property_does_not_throw()
    {
        var chart = new CdsRadarChart { Data = Skills(), MaxValue = 5, Alignment = CdsChartAlignment.Right, IsLegendEnabled = false, ColorPairingOption = 3 };
        Assert.NotNull(chart.RenderScene(500, 300));
    }
}

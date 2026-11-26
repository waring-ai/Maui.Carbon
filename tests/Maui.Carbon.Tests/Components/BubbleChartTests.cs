namespace Maui.Carbon.Tests.Components;

public class BubbleChartTests
{
    static List<CdsChartPoint> Products() =>
    [
        new("Storage", 18.2, 31.5) { Key = "Object store", Size = 142 },
        new("Storage", 3.1, 44.6) { Key = "Archive", Size = 38 },
        new("Compute", 9.7, 18.4) { Key = "Virtual servers", Size = 248 },
        new("Compute", 34.8, 29.3) { Key = "Functions", Size = 52 },
    ];

    static CdsBubbleChart Chart() => new()
    {
        Data = Products(),
        RadiusLabel = "Revenue (M USD)",
        BottomAxis = new CdsChartAxis { ScaleType = CdsChartScaleType.Linear, Title = "Growth (%)" },
        LeftAxis = new CdsChartAxis { Title = "Margin (%)" },
    };

    [Fact]
    public void Radius_scale_maps_sizes_linearly_onto_the_range()
    {
        var s = CdsBubbleChart.RadiusScale([10, 20, 30], 2, 22);
        Assert.Equal(2, s(10));
        Assert.Equal(12, s(20));
        Assert.Equal(22, s(30));
        Assert.Equal(2, s(null));
        Assert.Equal(12, CdsBubbleChart.RadiusScale([5, 5], 2, 22)(5));
        Assert.Equal(4, CdsBubbleChart.RadiusScale([null, 0], 2, 22)(null));
        Assert.Equal((3, 25), CdsBubbleChart.DefaultRadiusRange(400, 600));
    }

    [Fact]
    public void Bubbles_draw_largest_first_at_20_percent()
    {
        var scene = Chart().RenderScene(400, 400);
        var dots = scene.Shapes.OfType<ChartCircle>().ToList();
        Assert.Equal(4, dots.Count);
        Assert.Equal(25, dots[0].Radius, 3);
        Assert.Equal(3, dots[^1].Radius, 3);
        Assert.True(dots.Zip(dots.Skip(1)).All(p => p.First.Radius >= p.Second.Radius));
        Assert.All(dots, d =>
        {
            Assert.Equal(0.2f, d.FillOpacity);
            Assert.Equal(1f, d.HoverFillOpacity);
            Assert.False(d.Clip);
        });
        Assert.Equal("4 bubbles", scene.Summary);
    }

    [Fact]
    public void Custom_radius_range_and_fill_opacity()
    {
        var chart = Chart();
        chart.MinimumRadius = 5;
        chart.MaximumRadius = 10;
        chart.BubbleFillOpacity = 0.5;
        var dots = chart.RenderScene(400, 400).Shapes.OfType<ChartCircle>().ToList();
        Assert.Equal(10, dots[0].Radius, 3);
        Assert.Equal(5, dots[^1].Radius, 3);
        Assert.All(dots, d => Assert.Equal(0.5f, d.FillOpacity));
    }

    [Fact]
    public void A_single_bubble_tooltip_shows_the_size_before_the_group()
    {
        var scene = Chart().RenderScene(400, 400);
        var mark = scene.Marks[0];
        Assert.Equal(["Growth (%)", "Margin (%)", "Revenue (M USD)", "Group"], mark.Rows.Select(r => r.Label));
        Assert.Equal("38", mark.Rows[2].Value);
    }

    [Fact]
    public void Ruler_off_marks_each_bubble_in_drawing_order()
    {
        var chart = Chart();
        chart.IsRulerEnabled = false;
        var scene = chart.RenderScene(400, 400);
        Assert.Equal(4, scene.Marks.Count);
        Assert.Equal("9.7, 18.4, Revenue (M USD) 248, Compute", scene.Marks[0].Description);
        var smallest = scene.Shapes.OfType<ChartCircle>().Last();
        Assert.Equal(scene.Marks[^1].Id, scene.HitTest(smallest.Center)!.Id);
    }

    [Fact]
    public void Legend_ends_with_the_radius_item()
    {
        var chart = Chart();
        Assert.True(chart.LegendShown);
        chart.RadiusLegendLabel = "Engineers";
        chart.RadiusLegendLabel = "";
        Assert.NotNull(chart.RenderScene(400, 400));
    }

    [Fact]
    public void Table_adds_the_size_column()
    {
        var table = Chart().RenderTable();
        Assert.Equal(["Group", "Growth (%)", "Margin (%)", "Revenue (M USD)"], table.Headers);
        Assert.Equal(["Storage", "18.2", "31.5", "142"], table.Rows[0]);
    }

    [Fact]
    public void Labels_axis_and_legend_filtering()
    {
        var data = new List<CdsChartPoint>
        {
            new("Platform", "Sprint 1", 62) { Size = 14 }, new("Mobile", "Sprint 1", 48) { Size = 6 }, new("Platform", "Sprint 2", 71) { Size = 9 },
        };
        var chart = new CdsBubbleChart { Data = data, BottomAxis = new CdsChartAxis { ScaleType = CdsChartScaleType.Labels } };
        Assert.Equal(2, chart.RenderScene(500, 300).Marks.Count);
        chart.RenderScene(500, 300);
        chart.ToggleGroup("Mobile");
        Assert.Single(chart.RenderScene(500, 300).Shapes.OfType<ChartCircle>());
    }

    [Fact]
    public void Loading_and_empty_show_the_skeleton()
    {
        var chart = new CdsBubbleChart { IsLoading = true };
        Assert.True(chart.RenderScene(400, 300).Shimmer);
        chart.IsLoading = false;
        Assert.Equal("No data", chart.RenderScene(400, 300).Summary);
    }

    [Fact]
    public void Setting_every_property_does_not_throw()
    {
        var chart = new CdsBubbleChart
        {
            Data = Products(),
            RadiusLabel = null,
            RadiusLegendLabel = null,
            MinimumRadius = 1,
            MaximumRadius = 30,
            BubbleFillOpacity = -1,
            ShowBubbles = false,
            IsRulerEnabled = false,
        };
        Assert.Equal(0, chart.BubbleFillOpacity);
        Assert.Empty(chart.RenderScene(400, 300).Shapes.OfType<ChartCircle>());
        Assert.Equal("Radius", chart.RenderTable().Headers[^1]);
    }
}

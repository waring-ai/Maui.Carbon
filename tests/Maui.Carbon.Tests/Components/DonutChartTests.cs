using System.Globalization;

namespace Maui.Carbon.Tests.Components;

public class DonutChartTests
{
    static List<CdsChartPoint> Storage() =>
    [
        new() { Group = "Documents", Value = 182 }, new() { Group = "Photos", Value = 420 }, new() { Group = "Video", Value = 615 },
        new() { Group = "Backups", Value = 240 }, new() { Group = "Other", Value = 38 },
    ];

    static CdsDonutChart Chart() => new() { Data = Storage(), Culture = CultureInfo.GetCultureInfo("en-US") };

    [Fact]
    public void Center_shows_the_total_and_label_scaled_with_the_radius()
    {
        var chart = Chart();
        chart.CenterLabel = "GB used";
        var scene = chart.RenderScene(600, 400);
        var figure = Assert.Single(scene.Texts, t => t.Text == "1,495");
        var label = Assert.Single(scene.Texts, t => t.Text == "GB used");
        // radius 185: min(1.85 x 24, 24) = 24 and min(1.85 x 15, 15) = 15; the label 20px lower.
        Assert.Equal(24, figure.Font.Size);
        Assert.Equal(15, label.Font.Size);
        Assert.True(label.Y > figure.Y);
        Assert.Equal(ChartTextAnchor.Middle, figure.Anchor);
        Assert.Contains("GB used 1,495", scene.Summary);
    }

    [Fact]
    public void Center_sizes_shrink_for_small_donuts()
    {
        Assert.Equal(12, DonutCenter.NumberFontSize(50));
        Assert.Equal(7.5, DonutCenter.LabelFontSize(50), 3);
        Assert.Equal(12.5, DonutCenter.LabelY(50), 3);
        Assert.Equal(20, DonutCenter.LabelY(200));
    }

    [Fact]
    public void Inner_radius_is_three_quarters_and_the_center_follows_the_legend()
    {
        var chart = Chart();
        chart.RenderScene(600, 400);
        chart.ToggleGroup("Video");
        var scene = chart.RenderScene(600, 400);
        Assert.Single(scene.Marks);
        Assert.Contains(scene.Texts, t => t.Text == "615");
        var mark = scene.Marks[0];
        var c = mark.Bounds.Center;
        var radius = mark.Bounds.Width / 2;
        // The slice's hole: the center point is not inside the slice polygon, a point at 0.8 radius is.
        Assert.False(mark.Contains(c));
        Assert.True(mark.Contains(new PointF(c.X + radius * 0.875f, c.Y)));
        Assert.False(mark.Contains(new PointF(c.X + radius * 0.7f, c.Y)));
    }

    [Fact]
    public void Custom_number_formatter_and_no_label()
    {
        var chart = Chart();
        chart.CenterNumber = 2.854;
        chart.CenterNumberFormatter = n => "$" + n.ToString("0.00", CultureInfo.InvariantCulture) + "M";
        chart.CenterLabel = "";
        var scene = chart.RenderScene(600, 400);
        var figure = Assert.Single(scene.Texts, t => t.Text == "$2.85M");
        // Alone, the figure is centered vertically on the donut center (dominant-baseline central).
        Assert.Equal(scene.Marks[0].Bounds.Center.Y, figure.Y, 1);
        Assert.DoesNotContain(scene.Texts, t => t.Text == "Total");
    }

    [Fact]
    public void Default_label_is_total_and_figure_is_floored()
    {
        var chart = new CdsDonutChart { Data = new[] { new CdsChartPoint { Group = "A", Value = 1.9 }, new CdsChartPoint { Group = "B", Value = 1.2 } } };
        var scene = chart.RenderScene(600, 400);
        Assert.Contains(scene.Texts, t => t.Text == "Total");
        Assert.Contains(scene.Texts, t => t.Text == "3");
    }

    [Fact]
    public void Loading_and_empty_show_the_ring_without_center_text()
    {
        var chart = new CdsDonutChart { IsLoading = true };
        Assert.True(chart.RenderScene(400, 300).Shimmer);
        chart.IsLoading = false;
        var empty = chart.RenderScene(400, 300);
        Assert.Empty(empty.Texts);
        Assert.Equal(WindingMode.EvenOdd, Assert.Single(empty.Shapes.OfType<ChartPath>()).Winding);
    }

    [Fact]
    public void Table_matches_the_pie()
    {
        var table = Chart().RenderTable();
        Assert.Equal(["Group", "Value"], table.Headers);
        Assert.Equal(["Video", "615"], table.Rows[0]);
    }

    [Fact]
    public void Setting_every_property_does_not_throw()
    {
        var chart = new CdsDonutChart
        {
            Data = Storage(),
            CenterLabel = null,
            CenterNumber = 5,
            CenterNumberFormatter = n => n.ToString(CultureInfo.InvariantCulture),
            ShowLabels = false,
            Alignment = CdsChartAlignment.Center,
        };
        Assert.NotNull(chart.RenderScene(300, 300));
    }
}

namespace Maui.Carbon.Tests.Components;

public class ParallelCoordinatesChartTests
{
    static List<CdsParallelRecord> Cars() =>
    [
        new("Civic", "Japan", ("mpg", 35), ("cylinders", 4), ("horsepower", 158)),
        new("Mustang GT", "USA", ("mpg", 18), ("cylinders", 8), ("horsepower", 480)),
        new("Golf", "Europe", ("mpg", 34), ("cylinders", 4), ("horsepower", 148)),
        new("Model 3", "USA", ("mpg", null), ("cylinders", 0), ("horsepower", 283)),
    ];

    static CdsParallelCoordinatesChart Chart()
    {
        var chart = new CdsParallelCoordinatesChart { Data = Cars() };
        chart.Dimensions.Add(new CdsParallelDimension("mpg", "Fuel (mpg)"));
        chart.Dimensions.Add(new CdsParallelDimension("cylinders", "Cylinders"));
        chart.Dimensions.Add(new CdsParallelDimension("horsepower", "Power (hp)"));
        return chart;
    }

    [Fact]
    public void Axes_are_evenly_spaced_with_their_own_nice_scales()
    {
        var scene = Chart().RenderScene(600, 400);
        var axes = scene.Shapes.OfType<ChartLine>().Where(l => l.Stroke == ChartPaint.OfKey(CdsColorKeys.BorderStrong01)).ToList();
        Assert.Equal(3, axes.Count);
        Assert.Equal(axes[1].From.X - axes[0].From.X, axes[2].From.X - axes[1].From.X, 0.01);
        Assert.All(axes, a => Assert.Equal(28, a.From.Y));
        Assert.All(axes, a => Assert.Equal(392, a.To.Y));
        Assert.Contains(scene.Texts, t => t.Text == "Fuel (mpg)" && t.Font == ChartFont.AxisTitle);
        // Horsepower 148..480 nices to 100..500, not from zero.
        var hp = CdsParallelCoordinatesChart.MakeAxis(new CdsParallelDimension("horsepower"), Cars(), 28, 392);
        Assert.True(hp.IsLinear);
        Assert.Equal(100, hp.D0);
        Assert.Equal(500, hp.D1);
        Assert.Equal(392, hp.Map(100), 0.01);
        Assert.Equal(28, hp.Map(500), 0.01);
        Assert.Contains(scene.Texts, t => t.Text == "500");
    }

    [Fact]
    public void Records_are_lines_with_a_comparative_tooltip()
    {
        var scene = Chart().RenderScene(600, 400);
        Assert.Equal(4, scene.Marks.Count);
        Assert.StartsWith("4 records on 3 axes", scene.Summary);
        var civic = scene.Marks[0];
        Assert.Equal(["Civic", "Group", "Fuel (mpg)", "Cylinders", "Power (hp)"], civic.Rows.Select(r => r.Label));
        Assert.True(civic.Rows[0].Bold);
        Assert.Equal(["Japan", "35", "4", "158"], civic.Rows.Skip(1).Select(r => r.Value));
        var line = scene.Shapes.OfType<ChartPath>().Single(s => s.Mark == civic.Id && !s.HoverOnly);
        Assert.Equal(1.5f, line.StrokeWidth);
        Assert.Equal(0.7f, line.Opacity, 0.001);
        Assert.Equal(3f, scene.Shapes.OfType<ChartPath>().Single(s => s.Mark == civic.Id && s.HoverOnly).StrokeWidth);
        Assert.True(civic.Contains(civic.AnchorPoint));
        // A missing value breaks the line: Model 3 has no mpg, so its line starts at the second axis.
        var model3 = scene.Marks[3];
        Assert.Equal("-", model3.Rows[2].Value);
        Assert.True(model3.Bounds.Left > scene.Shapes.OfType<ChartLine>().First().From.X + 10);
    }

    [Fact]
    public void Default_dimensions_are_every_numeric_key()
    {
        var chart = new CdsParallelCoordinatesChart { Data = Cars() };
        var scene = chart.RenderScene(600, 400);
        Assert.Contains(scene.Texts, t => t.Text == "mpg");
        Assert.Contains(scene.Texts, t => t.Text == "horsepower");
        Assert.Equal(3, scene.Shapes.OfType<ChartLine>().Count());
    }

    [Fact]
    public void Labels_axes_space_categories_evenly()
    {
        var records = new List<CdsParallelRecord>
        {
            new("A", "G", ("tier", "Tier 1"), ("cost", 10)),
            new("B", "G", ("tier", "Tier 2"), ("cost", 20)),
        };
        var axis = CdsParallelCoordinatesChart.MakeAxis(new CdsParallelDimension("tier"), records, 0, 100);
        Assert.False(axis.IsLinear);
        Assert.Equal(75, axis.Map("Tier 1"), 0.01);
        Assert.Equal(25, axis.Map("Tier 2"), 0.01);
        Assert.True(float.IsNaN(axis.Map("Tier 9")));
        var chart = new CdsParallelCoordinatesChart { Data = records };
        chart.Dimensions.Add(new CdsParallelDimension("tier", "Tier"));
        chart.Dimensions.Add(new CdsParallelDimension("cost", "Cost"));
        Assert.Contains(chart.RenderScene(400, 300).Texts, t => t.Text == "Tier 2");
    }

    [Fact]
    public void Filters_dim_records_outside_and_draw_the_brush()
    {
        var chart = Chart();
        chart.Filters.Add(new CdsParallelFilter("horsepower", 100, 200));
        var scene = chart.RenderScene(600, 400);
        Assert.Equal(2, scene.Marks.Count);
        Assert.EndsWith("2 filtered out", scene.Summary);
        Assert.Equal(2, scene.Shapes.OfType<ChartPath>().Count(s => s.Stroke == ChartPaint.OfKey(CdsColorKeys.LayerAccent01)));
        var brush = scene.Shapes.OfType<ChartRect>().Single(r => r.Stroke == ChartPaint.OfKey(CdsColorKeys.ButtonTertiary));
        Assert.Equal(16, brush.Rect.Width);
        Assert.Equal(0.3f, brush.FillOpacity);
        var hp = CdsParallelCoordinatesChart.MakeAxis(new CdsParallelDimension("horsepower"), Cars(), 28, 392);
        Assert.Equal(hp.Map(200), brush.Rect.Top, 0.01);
        Assert.Equal(hp.Map(100) - hp.Map(200), brush.Rect.Height, 0.01);
    }

    [Fact]
    public void Legend_selection_and_table()
    {
        var chart = Chart();
        chart.RenderScene(600, 400);
        chart.ToggleGroup("USA");
        Assert.Equal(2, chart.RenderScene(600, 400).Marks.Count);
        var table = chart.RenderTable();
        Assert.Equal(["Name", "Group", "Fuel (mpg)", "Cylinders", "Power (hp)"], table.Headers);
        Assert.Equal(["Mustang GT", "USA", "18", "8", "480"], table.Rows[0]);
        Assert.Equal(["Model 3", "USA", "", "0", "283"], table.Rows[1]);
    }

    [Fact]
    public void Loading_and_empty_show_the_skeleton()
    {
        Assert.True(new CdsParallelCoordinatesChart { IsLoading = true }.RenderScene(400, 300).Shimmer);
        Assert.Equal("No data", new CdsParallelCoordinatesChart().RenderScene(400, 300).Summary);
    }

    [Fact]
    public void Setting_every_property_does_not_throw()
    {
        var chart = Chart();
        chart.RecordOpacity = 0.5;
        chart.Dimensions.Add(new CdsParallelDimension { Key = "weight", Title = "Weight", ScaleType = CdsParallelScaleType.Linear, Minimum = 0, Maximum = 3000, Formatter = v => v + " kg" });
        chart.Filters.Add(new CdsParallelFilter("cylinders", ["4"]));
        chart.Title = "Cars";
        chart.LegendPosition = CdsChartLegendPosition.Top;
        Assert.NotNull(chart.RenderScene(500, 300));
    }
}

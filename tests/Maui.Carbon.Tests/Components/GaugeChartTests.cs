using System.Globalization;

namespace Maui.Carbon.Tests.Components;

public class GaugeChartTests
{
    static CdsGaugeChart Chart(double? value = 76.5, double? delta = null) =>
        new() { Value = value, Delta = delta, Culture = CultureInfo.GetCultureInfo("en-US") };

    [Fact]
    public void Semi_gauge_geometry()
    {
        var g = GaugeGeometry.Layout(600, 250, full: false, 16, 50, CdsChartAlignment.Left);
        Assert.Equal(250, g.Radius);
        Assert.Equal(234, g.Inner);
        Assert.Equal(new PointF(250, 250), g.Center);
        Assert.Equal(-MathF.PI / 2, g.Start, 5);
        Assert.Equal(0, g.Current, 5);
        Assert.Equal(MathF.PI / 2, g.End, 5);
        var narrow = GaugeGeometry.Layout(300, 250, full: false, 16, 50, CdsChartAlignment.Center);
        Assert.Equal(150, narrow.Radius);
        Assert.Equal(150, narrow.Center.X);
    }

    [Fact]
    public void Full_gauge_geometry_and_clamping()
    {
        var g = GaugeGeometry.Layout(600, 300, full: true, 16, 150, CdsChartAlignment.Right);
        Assert.Equal(150, g.Radius);
        Assert.Equal(450, g.Center.X);
        Assert.Equal(0, g.Start);
        Assert.Equal(MathF.PI * 2, g.Current, 5);
        Assert.Equal(0, GaugeGeometry.Layout(600, 300, true, 16, -5, CdsChartAlignment.Left).Current);
    }

    [Fact]
    public void Numbers_round_to_two_decimals_only_when_fractional()
    {
        Assert.Equal(76.5, GaugeGeometry.Round(76.5));
        Assert.Equal(42, GaugeGeometry.Round(42.001));
        Assert.Equal(3.14, GaugeGeometry.Round(3.14159));
    }

    [Fact]
    public void Value_and_delta_text_with_status_arrow()
    {
        var chart = Chart(76.5, -3.2);
        chart.Status = CdsChartStatus.Danger;
        var scene = chart.RenderScene(600, 250);
        Assert.Equal(["76.5", "%", "-3.2%"], scene.Texts.Select(t => t.Text));
        // Value at radius / 2.5, symbol half that, delta radius / 8.
        Assert.Equal(100, scene.Texts[0].Font.Size);
        // Carbon sets the value in Plex Sans Light.
        Assert.Equal(ChartFontFamily.SansLight, scene.Texts[0].Font.Family);
        Assert.Equal(50, scene.Texts[1].Font.Size);
        Assert.Equal(31.25, scene.Texts[2].Font.Size, 2);
        var arrow = scene.Shapes.OfType<ChartPath>().Last();
        Assert.Equal(ChartPaint.OfKey(CdsColorKeys.SupportError), arrow.Fill);
        // The arrow points down: its middle vertex is its lowest point.
        var mark = Assert.Single(scene.Marks);
        Assert.Equal("76.5%", mark.Rows[0].Value);
        Assert.Equal("-3.2%", mark.Rows[1].Value);
        Assert.Contains("danger", mark.Description);
    }

    [Fact]
    public void Semi_gauge_lifts_the_numbers_for_a_delta()
    {
        var without = Chart(42).RenderScene(600, 250).Texts[0];
        var with = Chart(42, 4).RenderScene(600, 250).Texts[0];
        Assert.Equal(250 / 8f + 10, without.Y - with.Y, 2);
    }

    [Fact]
    public void Arcs_split_at_the_value()
    {
        var scene = Chart(25).RenderScene(600, 250);
        var paths = scene.Shapes.OfType<ChartPath>().ToList();
        Assert.Equal(2, paths.Count);
        Assert.Equal(ChartPaint.OfKey(CdsColorKeys.Layer01), paths[0].Fill);
        Assert.Equal(ChartPaint.OfSeries("value"), paths[1].Fill);
        Assert.Equal(0, paths[1].Mark);
        Assert.Equal("25%", scene.Summary);
    }

    [Fact]
    public void Zero_delta_and_no_symbol()
    {
        var chart = Chart(63, 0);
        chart.ShowPercentageSymbol = false;
        chart.Type = CdsGaugeChartType.Full;
        var scene = chart.RenderScene(400, 300);
        Assert.Equal(["63"], scene.Texts.Select(t => t.Text));
    }

    [Fact]
    public void Arrow_direction_and_formatter()
    {
        var up = GaugeGeometry.Arrow(new PointF(0, 0), 16, up: true);
        Assert.Equal(new PointF(8, 6), up[1]);
        var down = GaugeGeometry.Arrow(new PointF(0, 0), 16, up: false);
        Assert.Equal(new PointF(8, 10), down[1]);
        var chart = Chart(88, 4);
        chart.NumberFormatter = n => n.ToString("0.0", CultureInfo.InvariantCulture);
        chart.IsDeltaArrowEnabled = false;
        var scene = chart.RenderScene(400, 300);
        Assert.Contains(scene.Texts, t => t.Text == "88.0");
        Assert.Contains(scene.Texts, t => t.Text == "4.0%");
        Assert.Equal(2, scene.Shapes.Count);
    }

    [Fact]
    public void Data_rows_are_read_when_the_properties_are_not_set()
    {
        var chart = new CdsGaugeChart { Data = new[] { new CdsChartPoint { Group = "value", Value = 30 }, new CdsChartPoint { Group = "delta", Value = 2 } } };
        var scene = chart.RenderScene(600, 250);
        Assert.Contains(scene.Texts, t => t.Text == "30");
        var table = chart.RenderTable();
        Assert.Equal(["Group", "Value"], table.Headers);
        Assert.Equal(["value", "30"], table.Rows[0]);
        Assert.Equal(["delta", "2"], table.Rows[1]);
    }

    [Fact]
    public void No_value_shows_the_skeleton()
    {
        var chart = new CdsGaugeChart();
        var empty = chart.RenderScene(400, 250);
        Assert.Equal("No data", empty.Summary);
        chart.IsLoading = true;
        Assert.True(chart.RenderScene(400, 250).Shimmer);
    }

    [Fact]
    public void Legend_is_off_by_default()
    {
        var chart = Chart();
        chart.RenderScene(400, 250);
        Assert.False(chart.LegendShown);
    }

    [Fact]
    public void Setting_every_property_does_not_throw()
    {
        var chart = new CdsGaugeChart
        {
            Value = 10,
            Delta = -1,
            Type = CdsGaugeChartType.Full,
            Status = CdsChartStatus.Warning,
            Alignment = CdsChartAlignment.Center,
            ArcWidth = 8,
            ShowPercentageSymbol = false,
            NumberFormatter = n => n.ToString(CultureInfo.InvariantCulture),
            IsDeltaArrowEnabled = true,
            DeltaArrowDirection = CdsGaugeDeltaArrowDirection.Up,
            DeltaArrowSize = 12,
        };
        Assert.NotNull(chart.RenderScene(300, 300));
    }
}

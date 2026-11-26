namespace Maui.Carbon.Tests.Components;

public class SimpleBarChartTests
{
    static List<CdsChartPoint> Sales() =>
    [
        new("Qty", "Qty", 65000), new("More", "More", 29123), new("Sold", "Sold", 35213),
        new("Restocking", "Restocking", 51213), new("Misc", "Misc", 16932),
    ];

    [Fact]
    public void Vertical_bars_start_at_zero_and_scale_with_value()
    {
        var chart = new CdsSimpleBarChart { Data = Sales() };
        var scene = chart.RenderScene(600, 300);
        var bars = scene.Shapes.OfType<ChartRect>().Where(r => r.Mark >= 0).ToList();
        Assert.Equal(5, bars.Count);
        Assert.Equal(5, scene.Marks.Count(m => !m.IsReference));
        Assert.All(bars, b => Assert.Equal(16, b.Rect.Width, 0.01));
        var qty = bars[0].Rect;
        var misc = bars[4].Rect;
        Assert.Equal(qty.Bottom, misc.Bottom, 0.01);
        Assert.Equal(65000 / 16932.0, qty.Height / misc.Height, 0.05);
        Assert.Equal("5 bars", scene.Summary);
        Assert.Contains(scene.Texts, t => t.Text == "Restocking");
        Assert.Contains(scene.Texts, t => t.Text == "60,000");
    }

    [Fact]
    public void Labels_axis_on_the_left_makes_horizontal_bars()
    {
        var chart = new CdsSimpleBarChart
        {
            Data = Sales(),
            LeftAxis = new CdsChartAxis { ScaleType = CdsChartScaleType.Labels },
            BottomAxis = new CdsChartAxis { Title = "Units" },
        };
        var scene = chart.RenderScene(600, 300);
        var bars = scene.Shapes.OfType<ChartRect>().Where(r => r.Mark >= 0).ToList();
        Assert.All(bars, b => Assert.Equal(16, b.Rect.Height, 0.01));
        Assert.True(bars[0].Rect.Width > bars[4].Rect.Width);
        Assert.Contains(scene.Texts, t => t.Text == "Units");
    }

    [Fact]
    public void Floating_bars_span_low_to_high_and_tooltips_show_start_and_end()
    {
        var chart = new CdsSimpleBarChart { Data = new[] { new CdsChartPoint { Group = "Toronto", Low = -12, High = 23 } } };
        var scene = chart.RenderScene(400, 300);
        var mark = Assert.Single(scene.Marks);
        Assert.Contains(mark.Rows, r => r.Label == "Start" && r.Value == "-12");
        Assert.Contains(mark.Rows, r => r.Label == "End" && r.Value == "23");
        Assert.Contains(scene.Shapes, s => s is ChartLine);
    }

    [Fact]
    public void Thresholds_add_reference_marks()
    {
        var axis = new CdsChartAxis { Title = "Subscribers" };
        axis.Thresholds.Add(new CdsChartThreshold { Value = 40000, Label = "Target" });
        var chart = new CdsSimpleBarChart { Data = Sales(), LeftAxis = axis };
        var scene = chart.RenderScene(600, 300);
        var reference = Assert.Single(scene.Marks, m => m.IsReference);
        Assert.Equal("Target: 40,000", reference.Title);
    }

    [Fact]
    public void Legend_selection_filters_bars_and_raises_the_event()
    {
        var chart = new CdsSimpleBarChart { Data = Sales() };
        chart.RenderScene(600, 300);
        IReadOnlyList<string>? raised = null;
        chart.SelectedGroupsChanged += (_, e) => raised = e.SelectedGroups;
        chart.ToggleGroup("Sold");
        Assert.Equal(["Sold"], raised);
        Assert.Equal(["Sold"], chart.SelectedGroups);
        var scene = chart.RenderScene(600, 300);
        Assert.Single(scene.Marks);
        chart.ToggleGroup("Sold");
        Assert.Null(chart.SelectedGroups);
    }

    [Fact]
    public void Loading_and_empty_show_the_skeleton()
    {
        var chart = new CdsSimpleBarChart { IsLoading = true };
        var loading = chart.RenderScene(400, 300);
        Assert.True(loading.Shimmer);
        Assert.Empty(loading.Marks);
        chart.IsLoading = false;
        var empty = chart.RenderScene(400, 300);
        Assert.False(empty.Shimmer);
        Assert.Equal("No data", empty.Summary);
    }

    [Fact]
    public void Table_lists_group_domain_and_value()
    {
        var chart = new CdsSimpleBarChart { Data = Sales(), ValueFormat = "N0" };
        var table = chart.RenderTable();
        Assert.Equal(["Group", "Key", "Value"], table.Headers);
        Assert.Equal(["Qty", "Qty", "65,000"], table.Rows[0]);
    }

    [Fact]
    public void Color_scale_pins_series_colors()
    {
        var chart = new CdsSimpleBarChart { Data = Sales(), ColorScale = new Dictionary<string, string> { ["Qty"] = CdsColorKeys.DataVis05 } };
        chart.RenderScene(600, 300);
        var colors = new ChartColors(new CdsChartPalette(CdsThemeName.White, 1), ["Qty"], 1, chart.ColorScale);
        Assert.Equal(CdsPalettes.GetColor(CdsThemeName.White, CdsColorKeys.DataVis05), colors.Series("Qty"));
    }

    [Fact]
    public void Setting_every_property_does_not_throw()
    {
        var chart = new CdsSimpleBarChart
        {
            Title = "Units",
            Data = Sales(),
            BarWidth = 8,
            BarMaxWidth = 24,
            LegendPosition = CdsChartLegendPosition.Right,
            LegendAlignment = CdsChartLegendAlignment.Center,
            IsLegendEnabled = false,
            IsLegendClickable = false,
            LegendOrder = ["Misc"],
            ColorPairingOption = 2,
            IsToolbarEnabled = false,
            ToolbarControls = CdsChartToolbarControls.ShowAsTable,
            ToolbarIconCount = 2,
            IsTooltipEnabled = false,
            ShowTooltipTotal = false,
            TooltipTotalLabel = "Sum",
            ValueFormatter = v => v.ToString("N1"),
            Culture = System.Globalization.CultureInfo.InvariantCulture,
            ExportFileName = "units",
            IsXGridEnabled = false,
            IsYGridEnabled = false,
            IsRulerEnabled = false,
            TimeInterval = CdsChartTimeInterval.Monthly,
            TimeEdgeSpace = 0,
            ShowDayName = true,
        };
        Assert.NotNull(chart.RenderScene(500, 300));
    }
}

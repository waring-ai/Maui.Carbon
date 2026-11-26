namespace Maui.Carbon.Tests.Components;

public class ComboChartTests
{
    static readonly string[] Days = ["Mon", "Tue", "Wed"];

    static List<CdsChartPoint> Data() =>
    [
        .. new double[] { 42, 51, 48 }.Select((v, i) => new CdsChartPoint("Visitors", Days[i], v)),
        .. new double[] { 15, 17, 16 }.Select((v, i) => new CdsChartPoint("Temperature", Days[i], v)),
    ];

    static CdsComboChart Chart(bool dual)
    {
        var chart = new CdsComboChart
        {
            Data = Data(),
            LeftAxis = new CdsChartAxis { Title = "Visitors" },
            BottomAxis = new CdsChartAxis { ScaleType = CdsChartScaleType.Labels },
        };
        if (dual) chart.RightAxis = new CdsChartAxis { Title = "Temperature", CorrespondingDatasets = ["Temperature"] };
        chart.ChartTypes.Add(new CdsComboChartType { Graph = CdsComboChartGraph.SimpleBar, CorrespondingDatasets = ["Visitors"] });
        chart.ChartTypes.Add(new CdsComboChartType { Graph = CdsComboChartGraph.Line, CorrespondingDatasets = ["Temperature"], PointRadius = 4 });
        return chart;
    }

    [Fact]
    public void Each_graph_draws_its_datasets_with_one_shared_ruler()
    {
        var scene = Chart(dual: false).RenderScene(600, 300);
        Assert.Equal(3, scene.Shapes.OfType<ChartRect>().Count(r => r.Group == "Visitors"));
        Assert.Single(scene.Shapes.OfType<ChartPath>(), p => p.Group == "Temperature");
        var dots = scene.Shapes.OfType<ChartCircle>().Where(c => c.Group == "Temperature").ToList();
        Assert.Equal(3, dots.Count);
        Assert.All(dots, d => Assert.Equal(4, d.Radius));
        var rulers = scene.Marks.Where(m => m.RulerAt.HasValue).ToList();
        Assert.Equal(3, rulers.Count);
        Assert.All(dots, d => Assert.Contains(rulers, r => r.Id == d.Mark));
        Assert.Equal("Total", rulers[0].Rows[^1].Label);
        Assert.Equal(scene.Marks.Count, scene.Marks.Select(m => m.Id).Distinct().Count());
    }

    [Fact]
    public void A_secondary_axis_drops_the_total_and_scales_its_datasets()
    {
        var scene = Chart(dual: true).RenderScene(600, 300);
        var ruler = scene.Marks.First(m => m.RulerAt.HasValue);
        Assert.DoesNotContain(ruler.Rows, r => r.Label == "Total");
        // On its own axis the temperature line reaches near the top, far above where 15-17 would sit on a 0-56 visitors scale.
        var dots = scene.Shapes.OfType<ChartCircle>().Where(c => c.Group == "Temperature").ToList();
        Assert.All(dots, d => Assert.True(d.Center.Y < scene.Plot.Top + scene.Plot.Height * 0.3f));
    }

    [Fact]
    public void Stacked_graphs_extend_the_value_axis_to_the_sums()
    {
        var chart = new CdsComboChart
        {
            Data = new List<CdsChartPoint> { new("A", "Q1", 40), new("B", "Q1", 30), new("Target", "Q1", 50) },
            BottomAxis = new CdsChartAxis { ScaleType = CdsChartScaleType.Labels },
        };
        chart.ChartTypes.Add(new CdsComboChartType { Graph = CdsComboChartGraph.StackedBar, CorrespondingDatasets = ["A", "B"] });
        chart.ChartTypes.Add(new CdsComboChartType { Graph = CdsComboChartGraph.Line, CorrespondingDatasets = ["Target"] });
        var scene = chart.RenderScene(500, 300);
        Assert.Contains(scene.Texts, t => t.Text == "70");
    }

    [Fact]
    public void Legend_selection_hides_a_whole_graph()
    {
        var chart = Chart(dual: false);
        chart.RenderScene(600, 300);
        chart.ToggleGroup("Visitors");
        var scene = chart.RenderScene(600, 300);
        Assert.DoesNotContain(scene.Shapes, s => s.Group == "Temperature");
        Assert.Equal(3, scene.Shapes.OfType<ChartRect>().Count(r => r.Group == "Visitors"));
    }

    [Fact]
    public void Every_graph_type_builds()
    {
        foreach (var graph in Enum.GetValues<CdsComboChartGraph>())
        {
            var chart = new CdsComboChart { Data = Data(), BottomAxis = new CdsChartAxis { ScaleType = CdsChartScaleType.Labels } };
            chart.ChartTypes.Add(new CdsComboChartType { Graph = graph, Curve = CdsChartCurve.MonotoneX, ShowPoints = true, BarMaxWidth = 12 });
            var scene = chart.RenderScene(500, 300);
            Assert.NotEmpty(scene.Marks);
        }
    }
}

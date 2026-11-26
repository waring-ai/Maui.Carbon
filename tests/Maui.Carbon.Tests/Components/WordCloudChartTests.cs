namespace Maui.Carbon.Tests.Components;

public class WordCloudChartTests
{
    static List<CdsChartPoint> Tickets() =>
    [
        new("Account", "login", 96), new("Account", "password", 81), new("Account", "MFA", 44), new("Account", "SSO", 27),
        new("Billing", "invoice", 72), new("Billing", "refund", 58), new("Billing", "tax", 19),
        new("Performance", "timeout", 88), new("Performance", "slow", 64), new("Performance", "cache", 12),
        new("Features", "export", 53), new("Features", "API", 49), new("Features", "webhook", 24),
    ];

    [Fact]
    public void Words_are_sized_linearly_between_carbon_s_range_and_colored_by_group()
    {
        var chart = new CdsWordCloudChart { Data = Tickets() };
        var scene = chart.RenderScene(600, 400);
        Assert.True(scene.Marks.Count >= 11);
        Assert.Equal($"{scene.Marks.Count} words", scene.Summary);
        // min(600, 400) x [20, 75] / 400.
        var login = scene.Texts.Single(t => t.Text == "login");
        Assert.Equal(75, login.Font.Size);
        Assert.Equal(ChartPaint.OfSeries("Account"), login.Color);
        Assert.True(login.FadesWithLegend);
        Assert.Equal(ChartTextAnchor.Middle, login.Anchor);
        if (scene.Texts.SingleOrDefault(t => t.Text == "cache") is { } cache) Assert.Equal(20, cache.Font.Size);
        Assert.Equal((20, 75), CdsWordCloudChart.DefaultRange(new Size(600, 400)));
        var scale = CdsWordCloudChart.FontSizeScale([12, 96], (20, 75));
        Assert.Equal(47.5, scale(54), 9);
        Assert.Equal(15, CdsWordCloudChart.FontSizeScale([5, 5], (10, 20))(5));
    }

    [Fact]
    public void Words_do_not_overlap_and_the_cloud_is_stable()
    {
        var chart = new CdsWordCloudChart { Data = Tickets() };
        var a = chart.RenderScene(600, 400);
        var b = chart.RenderScene(600, 400);
        Assert.Equal(a.Texts.Select(t => (t.Text, t.X, t.Y)), b.Texts.Select(t => (t.Text, t.X, t.Y)));
        foreach (var m in a.Marks)
            foreach (var n in a.Marks)
                if (m.Id != n.Id) Assert.False(m.Bounds.IntersectsWith(n.Bounds));
        Assert.All(a.Marks, m => Assert.True(m.Bounds.Left >= 0 && m.Bounds.Right <= 600 && m.Bounds.Top >= 0 && m.Bounds.Bottom <= 400));
    }

    [Fact]
    public void Tooltip_shows_word_value_and_group()
    {
        var chart = new CdsWordCloudChart { Data = Tickets(), WordLabel = "Keyword", ValueLabel = "Mentions" };
        var mark = chart.RenderScene(600, 400).Marks.Single(m => m.SpokenText.StartsWith("login"));
        Assert.Equal(new ChartTooltipRow("Keyword", "login"), mark.Rows[0]);
        Assert.Equal(new ChartTooltipRow("Mentions", "96"), mark.Rows[1]);
        Assert.Equal(new ChartTooltipRow("Group", "Account", ChartPaint.OfSeries("Account")), mark.Rows[2]);
        Assert.Equal("login: 96, Account", mark.SpokenText);
        var table = chart.RenderTable();
        Assert.Equal(["Keyword", "Group", "Mentions"], table.Headers);
        Assert.Equal(["login", "Account", "96"], table.Rows[0]);
    }

    [Fact]
    public void Legend_isolates_groups_and_a_missing_value_draws_nothing()
    {
        var chart = new CdsWordCloudChart { Data = Tickets() };
        chart.RenderScene(600, 400);
        chart.ToggleGroup("Billing");
        var scene = chart.RenderScene(600, 400);
        Assert.All(scene.Texts, t => Assert.Equal("Billing", t.Group));
        // The isolated group is laid out again from Carbon's full range.
        Assert.Equal(75, scene.Texts.Single(t => t.Text == "invoice").Font.Size);
        var bad = new CdsWordCloudChart { Data = new[] { new CdsChartPoint("a", "word", 3), new CdsChartPoint { Group = "a", Key = "broken" } } };
        Assert.Empty(bad.RenderScene(600, 400).Texts);
    }

    [Fact]
    public void Font_size_range_is_settable_and_loading_shows_the_skeleton()
    {
        var chart = new CdsWordCloudChart { Data = Tickets(), FontSizeRange = _ => (10, 30) };
        Assert.Equal(30, chart.RenderScene(600, 400).Texts.Max(t => t.Font.Size));
        Assert.True(new CdsWordCloudChart { IsLoading = true }.RenderScene(300, 200).Shimmer);
        chart = new CdsWordCloudChart { Data = Tickets(), IsLegendEnabled = false, Title = "Keywords", WordLabel = null, ValueLabel = null, ColorPairingOption = 2 };
        Assert.Equal("Word", chart.RenderScene(600, 400).Marks[0].Rows[0].Label);
    }

    [Fact]
    public void Words_larger_than_32px_are_set_light()
    {
        var scene = new CdsWordCloudChart { Data = Tickets() }.RenderScene(600, 400);
        Assert.Equal(ChartFontFamily.CondensedLight, scene.Texts.Single(t => t.Text == "login").Font.Family);
        Assert.All(scene.Texts, t => Assert.Equal(t.Font.Size > 32 ? ChartFontFamily.CondensedLight : ChartFontFamily.Condensed, t.Font.Family));
        Assert.Contains(scene.Texts, t => t.Font.Family == ChartFontFamily.Condensed);
    }
}

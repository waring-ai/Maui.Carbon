namespace Maui.Carbon.Tests.Components;

public class ChartWordCloudTests
{
    static ChartFont Font(CloudWord w) => new(ChartFontFamily.Condensed, w.Size);

    static List<CloudWord> Words() =>
        new[] { ("login", 75), ("timeout", 68), ("password", 62), ("invoice", 55), ("slow", 48), ("export", 40), ("API", 36), ("MFA", 33), ("cache", 20), ("tax", 22), ("SSO", 25), ("refund", 44) }
            .Select((w, i) => new CloudWord(w.Item1, w.Item2, i)).ToList();

    [Fact]
    public void Placed_words_never_overlap_and_stay_inside()
    {
        var placed = ChartWordCloud.Layout(Words(), 600, 400, 5, ChartRandom.Seeded(42), Font);
        Assert.True(placed.Count >= 10);
        foreach (var a in placed)
        {
            Assert.True(a.X + a.X0 >= 0 && a.X + a.X1 <= 600 && a.Y + a.Y0 >= 0 && a.Y + a.Y1 <= 400);
            foreach (var b in placed)
                if (!ReferenceEquals(a, b)) Assert.False(ChartWordCloud.Collides(a, b));
        }
        // Largest first, the first word stays where it started (near the centre).
        Assert.Equal("login", placed[0].Text);
        Assert.InRange(placed[0].X, 150, 450);
        Assert.InRange(placed[0].Y, 100, 300);
    }

    [Fact]
    public void The_same_seed_gives_the_same_cloud()
    {
        var a = ChartWordCloud.Layout(Words(), 600, 400, 5, ChartRandom.Seeded(7), Font).Select(w => (w.Text, w.X, w.Y)).ToList();
        var b = ChartWordCloud.Layout(Words(), 600, 400, 5, ChartRandom.Seeded(7), Font).Select(w => (w.Text, w.X, w.Y)).ToList();
        Assert.Equal(a, b);
    }

    [Fact]
    public void Words_that_do_not_fit_are_left_out()
    {
        var placed = ChartWordCloud.Layout([new CloudWord("enormous", 200, 0), new CloudWord("ok", 12, 1)], 120, 60, 5, ChartRandom.Seeded(1), Font);
        Assert.Equal(["ok"], placed.Select(w => w.Text));
    }

    [Fact]
    public void Ink_boxes_follow_ascenders_and_descenders()
    {
        var font = new ChartFont(ChartFontFamily.Condensed, 40);
        var low = ChartWordCloud.InkBox("ocean", font);
        var tall = ChartWordCloud.InkBox("Tall", font);
        var deep = ChartWordCloud.InkBox("query", font);
        Assert.True(tall.Y0 < low.Y0);
        Assert.True(deep.Y1 > low.Y1);
        Assert.Equal(CdsChartTextMetrics.Width("ocean", font), low.X1 - low.X0, 0.01);
    }

    [Fact]
    public void Spiral_and_random_match_d3_cloud_and_mulberry32()
    {
        Assert.Equal((0, 0), ChartWordCloud.Spiral(600, 400, 0));
        var (x, y) = ChartWordCloud.Spiral(600, 400, 10);
        Assert.Equal(1.5 * Math.Cos(1), x, 9);
        Assert.Equal(Math.Sin(1), y, 9);
        // mulberry32(1) and FNV-1a reference values (JavaScript).
        Assert.Equal(0.6270739405881613, ChartRandom.Seeded(1)(), 12);
        Assert.Equal(0xe40c292cu, ChartRandom.Hash("a"));
    }
}

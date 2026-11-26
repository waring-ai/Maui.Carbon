using System.Globalization;

namespace Maui.Carbon.Tests.Components;

public class MeterChartTests
{
    static readonly CultureInfo En = CultureInfo.GetCultureInfo("en-US");

    static CdsMeterChart Single(double value, double? peak = null)
    {
        var chart = new CdsMeterChart { Data = new[] { new CdsChartPoint { Group = "Memory used", Value = value } }, Peak = peak, Culture = En };
        chart.StatusRanges.Add(new CdsMeterStatusRange(0, 60, CdsChartStatus.Success));
        chart.StatusRanges.Add(new CdsMeterStatusRange(60, 85, CdsChartStatus.Warning));
        chart.StatusRanges.Add(new CdsMeterStatusRange(85, 100, CdsChartStatus.Danger));
        return chart;
    }

    static CdsMeterChart Mailbox() => new()
    {
        Data = new[]
        {
            new CdsChartPoint { Group = "Emails", Value = 202 }, new CdsChartPoint { Group = "Photos", Value = 654 },
            new CdsChartPoint { Group = "Video", Value = 1480 }, new CdsChartPoint { Group = "Documents", Value = 310 },
            new CdsChartPoint { Group = "Other", Value = 96 },
        },
        IsProportional = true,
        Total = 4000,
        Unit = "MB",
        Culture = En,
    };

    [Fact]
    public void Status_picks_the_first_matching_range()
    {
        CdsMeterStatusRange[] ranges = [new(0, 60, CdsChartStatus.Success), new(60, 85, CdsChartStatus.Warning)];
        Assert.Equal(CdsChartStatus.Success, MeterLayout.Status(60, false, ranges));
        Assert.Equal(CdsChartStatus.Warning, MeterLayout.Status(61, false, ranges));
        Assert.Null(MeterLayout.Status(90, false, ranges));
        // A single meter's total is capped at 100; a proportional one is not.
        Assert.Equal(CdsChartStatus.Danger, MeterLayout.Status(150, false, [new(85, 100, CdsChartStatus.Danger)]));
        Assert.Null(MeterLayout.Status(150, true, [new(85, 100, CdsChartStatus.Danger)]));
        Assert.Null(MeterLayout.Status(10, false, null));
    }

    [Fact]
    public void Stacked_segments_leave_a_2px_divider()
    {
        var s = MeterLayout.Stacked([25, 25, 50], 100, 400);
        Assert.Equal((0f, 98f), s[0]);
        Assert.Equal((100f, 98f), s[1]);
        Assert.Equal((200f, 198f), s[2]);
        Assert.Equal(100, MeterLayout.Max(false, 4000, 10));
        Assert.Equal(4000, MeterLayout.Max(true, 4000, 10));
        Assert.Equal(10, MeterLayout.Max(true, null, 10));
    }

    [Fact]
    public void Single_meter_draws_title_percentage_status_bar_and_peak()
    {
        var scene = Single(78, 91).RenderScene(500, 37);
        Assert.Equal(["Memory used", "78%"], scene.Texts.Select(t => t.Text));
        Assert.Equal(ChartFontFamily.SansSemiBold, scene.Texts[0].Font.Family);
        Assert.Equal(16, scene.Texts[0].Font.Size);
        var rects = scene.Shapes.OfType<ChartRect>().ToList();
        // The track at 21 + 8, 8px high, then the value bar 2px shorter than 78% of the width.
        Assert.Equal(new RectF(0, 29, 500, 8), rects[0].Rect);
        var bar = rects[1];
        Assert.Equal(ChartPaint.OfKey(CdsColorKeys.SupportWarning), bar.Fill);
        Assert.Equal(ChartPaint.OfToken(CdsChartToken.AlertStroke), bar.Stroke);
        Assert.Equal(390 - 2, bar.Rect.Width + 1, 1);
        // The peak line and the status icon (a 16px circle at the right).
        var peak = scene.Shapes.OfType<ChartLine>().Single(l => l.StrokeWidth == 2);
        Assert.Equal(500 * 0.91f, peak.From.X, 1);
        var circle = Assert.Single(scene.Shapes.OfType<ChartCircle>());
        Assert.Equal(new PointF(492, 8), circle.Center);
        Assert.Equal(2, scene.Marks.Count);
        Assert.Equal("Memory used, 78%, warning", scene.Marks[0].Description);
        Assert.Equal("Peak, 91%", scene.Marks[1].Description);
        Assert.Equal("78%, warning", scene.Summary);
    }

    [Fact]
    public void Without_status_the_bar_uses_the_pairing_color()
    {
        var chart = new CdsMeterChart { Data = new[] { new CdsChartPoint { Group = "Progress", Value = 30 } } };
        var scene = chart.RenderScene(400, 37);
        Assert.Equal(ChartPaint.OfSeries("Progress"), scene.Shapes.OfType<ChartRect>().ElementAt(1).Fill);
        Assert.Empty(scene.Shapes.OfType<ChartCircle>());
    }

    [Fact]
    public void Long_titles_are_truncated()
    {
        var chart = new CdsMeterChart { Data = new[] { new CdsChartPoint { Group = "Memory used by every workload of the production cluster", Value = 30 } } };
        var title = chart.RenderScene(200, 37).Texts[0];
        Assert.EndsWith("...", title.Text);
        Assert.True(CdsChartTextMetrics.Width(title.Text, title.Font) <= 200);
        Assert.Equal("Short", RoundChartText.Fit("Short", 500, ChartFont.Tick));
    }

    [Fact]
    public void Proportional_meter_shows_breakdown_total_segments_and_tooltips()
    {
        var chart = Mailbox();
        var scene = chart.RenderScene(500, 40);
        Assert.Equal(["4,000 MB total", "2,742 MB used (1,258 MB available)"], scene.Texts.Select(t => t.Text));
        var segments = scene.Shapes.OfType<ChartRect>().Skip(1).ToList();
        Assert.Equal(5, segments.Count);
        Assert.All(segments, s => Assert.Equal(16, s.Rect.Height));
        Assert.Equal(24, segments[0].Rect.Top);
        Assert.Equal(202 / 4000f * 500 - 2, segments[0].Rect.Width, 2);
        Assert.Equal(202 / 4000f * 500, segments[1].Rect.Left, 2);
        Assert.Equal(5, scene.Marks.Count);
        Assert.Equal(new ChartTooltipRow("Photos", "654"), scene.Marks[1].Rows.Single());
        Assert.Equal("5 parts, 2,742 of 4,000 MB", scene.Summary);
        Assert.True(chart.LegendShown);
    }

    [Fact]
    public void Proportional_formatters()
    {
        var chart = Mailbox();
        chart.TotalFormatter = t => $"Budget {t}";
        chart.BreakdownFormatter = (used, total) => $"{used} of {total}";
        var scene = chart.RenderScene(500, 40);
        Assert.Equal(["Budget 4000", "2742 of 4000"], scene.Texts.Select(t => t.Text));
        chart.Total = null;
        Assert.Equal(["Budget 2742"], chart.RenderScene(500, 40).Texts.Select(t => t.Text));
    }

    [Fact]
    public void Legend_selection_filters_the_parts()
    {
        var chart = Mailbox();
        chart.RenderScene(500, 40);
        chart.ToggleGroup("Video");
        var scene = chart.RenderScene(500, 40);
        Assert.Single(scene.Marks);
    }

    [Fact]
    public void Tables()
    {
        var single = Single(78).RenderTable();
        Assert.Equal(["Group", "Value", "Status"], single.Headers);
        Assert.Equal(["Memory used", "78", "warning"], Assert.Single(single.Rows));
        var proportional = Mailbox().RenderTable();
        Assert.Equal(["Group", "Value", "Percentage of total"], proportional.Headers);
        Assert.Equal(["Emails", "202", "5.05 %"], proportional.Rows[0]);
    }

    [Fact]
    public void Defaults_and_empty_state()
    {
        var chart = new CdsMeterChart();
        Assert.False(chart.ToolbarShown);
        Assert.False(chart.IsLegendClickable);
        var empty = chart.RenderScene(400, 37);
        Assert.Equal("No data", empty.Summary);
        Assert.Equal(ChartPaint.OfKey(CdsColorKeys.LayerAccent01), Assert.Single(empty.Shapes).Fill);
        chart.IsLoading = true;
        Assert.True(chart.RenderScene(400, 37).Shimmer);
    }

    [Fact]
    public void Height_follows_the_content()
    {
        var chart = Single(40);
        Assert.Equal(37, chart.ContentHeight(400));
        chart.Title = "Cluster A";
        Assert.Equal(37 + 22 + 24, chart.ContentHeight(400));
        var mailbox = Mailbox();
        Assert.Equal(40 + 24 + 24, mailbox.ContentHeight(1000));
        Assert.True(mailbox.ContentHeight(120) > mailbox.ContentHeight(1000));
        chart.ShowLabels = false;
        chart.BarHeight = 4;
        Assert.Equal(4 + 22 + 24, chart.ContentHeight(400));
    }

    [Fact]
    public void Status_glyphs_are_drawn_inside_the_16px_icon()
    {
        foreach (var s in Enum.GetValues<CdsChartStatus>())
        {
            var b = MeterLayout.StatusGlyph(s, new PointF(100, 0)).Bounds;
            Assert.True(b.Left >= 100 && b.Right <= 116 && b.Top >= 0 && b.Bottom <= 16);
        }
    }

    [Fact]
    public void Setting_every_property_does_not_throw()
    {
        var chart = new CdsMeterChart
        {
            Data = new[] { new CdsChartPoint { Group = "A", Value = 120 } },
            IsProportional = true,
            Total = 100,
            Unit = "GB",
            TotalFormatter = t => t.ToString(CultureInfo.InvariantCulture),
            BreakdownFormatter = (a, b) => a + "/" + b,
            StatusRanges = [new CdsMeterStatusRange(0, 200, CdsChartStatus.Success)],
            Peak = -5,
            BarHeight = 12,
            ShowLabels = true,
            ShowPercentage = false,
        };
        Assert.NotNull(chart.RenderScene(300, 60));
        chart.IsProportional = false;
        Assert.NotNull(chart.RenderScene(300, 60));
    }

    [Fact]
    public void Loading_shimmers_the_empty_bar()
    {
        var chart = new CdsMeterChart { IsLoading = true };
        var scene = chart.RenderScene(400, 37);
        Assert.True(scene.Shimmer);
        var bar = Assert.IsType<ChartRect>(Assert.Single(scene.Shapes));
        Assert.True(bar.Shimmers);
        Assert.Equal(ChartPaint.OfKey(CdsColorKeys.LayerAccent01), bar.Fill);
        Assert.Equal(new RectF(0, MeterLayout.TitleRowSingle + MeterLayout.TitleSpacer, 400, 8), bar.Rect);
        // The same bar as empty data, which does not shimmer.
        chart.IsLoading = false;
        Assert.Equal(bar.Rect, Assert.IsType<ChartRect>(Assert.Single(chart.RenderScene(400, 37).Shapes)).Rect);
    }
}

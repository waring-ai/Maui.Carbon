using Maui.Carbon.Gallery.Infrastructure;
using Maui.Carbon.Gallery.Pages.Charts.Data;

namespace Maui.Carbon.Gallery.Pages.Charts;

[GalleryPage("Sparkline chart", GalleryCategory.Charts, Order = 70, Spec = "SparklineChart",
    Summary = "A small, axis-free trend line or area that shows the shape of a series inline, in a tile, table cell or KPI card.")]
public partial class SparklineChartPage : GalleryPage
{
    public SparklineChartPage()
    {
        InitializeComponent();
        RevenueValue.Text = $"{PackageBSamples.Revenue[^1].Value:N0}k USD";
        SemanticProperties.SetDescription(RevenueTrend, "Revenue, last 30 days");
    }

    void OnTypeClicked(object? sender, EventArgs e)
    {
        Interactive.Type = Interactive.Type == CdsSparklineType.Area ? CdsSparklineType.Line : CdsSparklineType.Area;
        UpdateStatus();
    }

    void OnRulerClicked(object? sender, EventArgs e)
    {
        Interactive.IsRulerEnabled = !Interactive.IsRulerEnabled;
        UpdateStatus();
    }

    void UpdateStatus() => Status.Text = $"{Interactive.Type}, ruler {(Interactive.IsRulerEnabled ? "on" : "off")}";
}

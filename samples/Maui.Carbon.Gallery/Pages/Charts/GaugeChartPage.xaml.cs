using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Charts;

[GalleryPage("Gauge chart", GalleryCategory.Charts, Order = 170, Spec = "GaugeChart",
    Summary = "One percentage on a semicircular or circular arc, with a large value and an optional change (delta) with an arrow.")]
public partial class GaugeChartPage : GalleryPage
{
    public GaugeChartPage()
    {
        InitializeComponent();
    }

    void OnDecrease(object? sender, EventArgs e) => Step(-7.5);

    void OnIncrease(object? sender, EventArgs e) => Step(7.5);

    void Step(double by)
    {
        var old = Interactive.Value ?? 0;
        var next = Math.Clamp(old + by, 0, 100);
        Interactive.Value = next;
        Interactive.Delta = next - old;
        Interactive.Status = next < 60 ? CdsChartStatus.Success : next < 85 ? CdsChartStatus.Warning : CdsChartStatus.Danger;
        Status.Text = $"{next:0.#}%, change {next - old:+0.#;-0.#;0}%, {Interactive.Status.ToString()!.ToLowerInvariant()}";
    }
}

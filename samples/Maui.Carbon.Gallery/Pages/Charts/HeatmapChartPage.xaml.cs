using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Charts;

[GalleryPage("Heatmap chart", GalleryCategory.Charts, Order = 200, Spec = "HeatmapChart",
    Summary = "A grid of cells on two category axes, each cell colored by its value on a sequential or diverging scale, with a color legend.")]
public partial class HeatmapChartPage : GalleryPage
{
    public HeatmapChartPage()
    {
        InitializeComponent();
    }

    void OnLegendTypeClicked(object? sender, EventArgs e)
    {
        Interactive.ColorLegendType = Interactive.ColorLegendType == CdsChartColorLegendType.Linear ? CdsChartColorLegendType.Quantize : CdsChartColorLegendType.Linear;
        UpdateStatus();
    }

    void OnDividerClicked(object? sender, EventArgs e)
    {
        Interactive.Divider = Interactive.Divider switch
        {
            CdsHeatmapDivider.Auto => CdsHeatmapDivider.Off,
            CdsHeatmapDivider.Off => CdsHeatmapDivider.On,
            _ => CdsHeatmapDivider.Auto,
        };
        UpdateStatus();
    }

    void UpdateStatus() => Status.Text = (Interactive.ColorLegendType == CdsChartColorLegendType.Linear ? "Linear legend" : "Quantized legend") + ", "
        + Interactive.Divider switch { CdsHeatmapDivider.Off => "no dividers", CdsHeatmapDivider.On => "dividers on", _ => "automatic dividers" };
}

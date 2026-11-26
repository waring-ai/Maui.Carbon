using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Charts;

[GalleryPage("Meter chart", GalleryCategory.Charts, Order = 180, Spec = "MeterChart",
    Summary = "A thin bar showing one percentage with a title, status and peak, or a proportional bar showing parts of a total with a breakdown and legend.")]
public partial class MeterChartPage : GalleryPage
{
    public MeterChartPage()
    {
        InitializeComponent();
        Spend.TotalFormatter = t => $"Budget {t:0.#}k USD";
        Spend.BreakdownFormatter = (used, total) => $"{used:0.0}k USD spent of {total:0.#}k USD";
        Show(55);
    }

    double _value;

    void OnDecrease(object? sender, EventArgs e) => Show(Math.Max(0, _value - 8));

    void OnIncrease(object? sender, EventArgs e) => Show(Math.Min(100, _value + 8));

    void OnLoadingClicked(object? sender, EventArgs e) => Interactive.IsLoading = !Interactive.IsLoading;

    void Show(double value)
    {
        _value = value;
        Interactive.Data = new[] { new CdsChartPoint { Group = "Memory used", Value = value } };
        var status = value <= 60 ? "success" : value <= 85 ? "warning" : "danger";
        Status.Text = $"{value:0}%, {status}";
    }
}

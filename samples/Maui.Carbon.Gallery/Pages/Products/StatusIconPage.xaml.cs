using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Products;

[GalleryPage("Status icon", GalleryCategory.Products, Order = 120, Spec = "StatusIcon",
    Summary = "A single status glyph that tells the severity or state of an object at a glance, with a required description that names the status.")]
public partial class StatusIconPage : GalleryPage
{
    static readonly (CdsStatusIconKind Kind, string Name)[] Steps =
    [
        (CdsStatusIconKind.Normal, "Normal"),
        (CdsStatusIconKind.InProgress, "In progress"),
        (CdsStatusIconKind.MinorWarning, "Minor warning"),
        (CdsStatusIconKind.MajorWarning, "Major warning"),
        (CdsStatusIconKind.Critical, "Critical"),
        (CdsStatusIconKind.Fatal, "Fatal"),
    ];

    int _step;

    public StatusIconPage()
    {
        InitializeComponent();
    }

    void OnNextClicked(object? sender, EventArgs e) => Show((_step + 1) % Steps.Length);

    void OnResetClicked(object? sender, EventArgs e) => Show(0);

    void Show(int step)
    {
        _step = step;
        var (kind, name) = Steps[step];
        Live.Kind = kind;
        Live.IconDescription = name;
        Status.Text = $"prod-eu-de-1: {name}";
    }
}

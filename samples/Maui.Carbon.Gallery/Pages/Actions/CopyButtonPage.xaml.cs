using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Actions;

[GalleryPage("Copy button", GalleryCategory.Actions, Order = 40, Spec = "CopyButton",
    Summary = "An icon button with the copy glyph that shows a 'Copied!' tooltip after it is activated.")]
public partial class CopyButtonPage : GalleryPage
{
    public CopyButtonPage()
    {
        InitializeComponent();
    }

    void OnCopied(object? sender, EventArgs e) =>
        Status.Text = $"Copied \"{(sender as CdsCopyButton)?.Text}\" at {DateTime.Now:HH:mm:ss}";
}

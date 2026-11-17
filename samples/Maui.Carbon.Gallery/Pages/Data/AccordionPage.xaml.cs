using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Data;

[GalleryPage("Accordion", GalleryCategory.Data, Order = 10,
    Summary = "A vertical list of headings that each expand to reveal a panel of content.")]
public partial class AccordionPage : GalleryPage
{
    public AccordionPage()
    {
        InitializeComponent();
    }

    void OnHeadingClicked(object? sender, EventArgs e)
    {
        if (sender is CdsAccordionItem item) Status.Text = $"{item.Title}: {(item.IsOpen ? "open" : "closed")}";
    }
}

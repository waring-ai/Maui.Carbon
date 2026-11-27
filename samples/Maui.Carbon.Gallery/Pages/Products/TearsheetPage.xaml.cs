using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Products;

[GalleryPage("Tearsheet", GalleryCategory.Products, Order = 20, Spec = "Tearsheet",
    Summary = "A tall modal surface that rises from the bottom of the page for a substantial task, in wide and narrow sizes.")]
public partial class TearsheetPage : GalleryPage
{
    public TearsheetPage()
    {
        InitializeComponent();
        RepoDropdown.ItemsSource = new[] { "github.com/acme/checkout-api", "github.com/acme/payments", "github.com/acme/web" };
        RepoDropdown.SelectedItem = "github.com/acme/checkout-api";
    }

    void OnWideClicked(object? sender, EventArgs e) => WideSheet.IsOpen = true;

    void OnWideClosing(object? sender, CdsModalRequestCloseEventArgs e) => Status.Text = "Pipeline not created";

    void OnWideCancel(object? sender, EventArgs e) => WideSheet.IsOpen = false;

    void OnWideNext(object? sender, EventArgs e) => Status.Text = "Next step";

    void OnNarrowClicked(object? sender, EventArgs e) => NarrowSheet.IsOpen = true;

    void OnNarrowCancel(object? sender, EventArgs e) => NarrowSheet.IsOpen = false;

    void OnNarrowSubmit(object? sender, EventArgs e)
    {
        NarrowSheet.IsOpen = false;
        Status.Text = "Quota requested";
    }

    void OnPassiveClicked(object? sender, EventArgs e) => PassiveSheet.IsOpen = true;

    void OnStackClicked(object? sender, EventArgs e) => StackSheet1.IsOpen = true;

    void OnStack2(object? sender, EventArgs e) => StackSheet2.IsOpen = true;

    void OnStack3(object? sender, EventArgs e) => StackSheet3.IsOpen = true;

    void OnStackClose(object? sender, EventArgs e)
    {
        if (StackSheet3.IsOpen) StackSheet3.IsOpen = false;
        else if (StackSheet2.IsOpen) StackSheet2.IsOpen = false;
        else StackSheet1.IsOpen = false;
    }
}

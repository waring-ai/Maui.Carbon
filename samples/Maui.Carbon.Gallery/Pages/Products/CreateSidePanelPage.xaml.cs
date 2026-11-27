using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Products;

[GalleryPage("Create side panel", GalleryCategory.Products, Order = 41, Spec = "CreateSidePanel",
    Summary = "A one-step create form in a side panel that slides in beside the page.")]
public partial class CreateSidePanelPage : GalleryPage
{
    public CreateSidePanelPage()
    {
        InitializeComponent();
        Panel.PageContent = PageArea;
        Metric.ItemsSource = new[] { "CPU utilization", "Memory utilization", "Request latency" };
        Metric.SelectedItem = "CPU utilization";
    }

    void OnCreateClicked(object? sender, EventArgs e) => Panel.IsOpen = true;

    void OnNameChanged(object? sender, TextChangedEventArgs e) => Panel.IsSubmitEnabled = !string.IsNullOrWhiteSpace(RuleName.Text);

    void OnSubmit(object? sender, EventArgs e)
    {
        Panel.IsOpen = false;
        Status.Text = $"Rule {RuleName.Text} created";
    }
}

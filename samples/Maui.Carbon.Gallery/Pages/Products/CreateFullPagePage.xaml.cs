using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Products;

[GalleryPage("Create full page", GalleryCategory.Products, Order = 42, Spec = "CreateFullPage",
    Summary = "A multi-step create flow that fills a page, with a progress column and a cancel confirmation.")]
public partial class CreateFullPagePage : GalleryPage
{
    public CreateFullPagePage()
    {
        InitializeComponent();
        CdsTokens.SetBrush(FlowFrame, Border.StrokeProperty, CdsColorKeys.BorderSubtle01);
        RegionDropdown.ItemsSource = new[] { "Frankfurt", "London", "Dallas", "Tokyo" };
        RegionDropdown.SelectedItem = "Frankfurt";
        Flow.SubmitAsync = async () =>
        {
            await Task.Delay(600);
            return true;
        };
    }

    void OnClose(object? sender, EventArgs e)
    {
        Status.Text = "Creation cancelled";
        Flow.Restart();
    }

    void OnSubmitted(object? sender, EventArgs e) => Status.Text = "Cluster created";

    void OnStepClicked(object? sender, int step)
    {
        if (step < Flow.CurrentStep) Flow.GoToStep(step);
    }
}

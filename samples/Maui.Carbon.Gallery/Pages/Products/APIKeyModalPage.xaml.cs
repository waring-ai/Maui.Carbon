using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Products;

[GalleryPage("API key modal", GalleryCategory.Products, Order = 50, Spec = "APIKeyModal",
    Summary = "Creates, edits or shows an API key in one or more steps, with copy and download.")]
public partial class APIKeyModalPage : GalleryPage
{
    public APIKeyModalPage()
    {
        InitializeComponent();
    }

    void OnGenerateClicked(object? sender, EventArgs e)
    {
        GenerateModal.ApiKey = null;
        GenerateModal.IsError = false;
        GenerateModal.IsOpen = true;
    }

    void OnInstantClicked(object? sender, EventArgs e) => InstantModal.IsOpen = true;

    async void OnRequestGenerate(object? sender, CdsAPIKeyModalEventArgs e)
    {
        GenerateModal.IsLoading = true;
        await Task.Delay(1000);
        GenerateModal.ApiKey = "111-111-111-111";
        GenerateModal.IsLoading = false;
        Status.Text = $"Generated key {e.Value}";
    }

    void OnRequestClose(object? sender, CdsModalRequestCloseEventArgs e) => Status.Text = "Closed";
}

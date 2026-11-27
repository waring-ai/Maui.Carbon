using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Products;

[GalleryPage("Create modal", GalleryCategory.Products, Order = 60, Spec = "CreateModal",
    Summary = "A small modal for creating a simple resource quickly, with a short form and a primary button disabled until it is valid.")]
public partial class CreateModalPage : GalleryPage
{
    public CreateModalPage()
    {
        InitializeComponent();
    }

    void OnCreateClicked(object? sender, EventArgs e)
    {
        NameInput.Text = string.Empty;
        CreateModal.IsOpen = true;
    }

    void OnNameChanged(object? sender, TextChangedEventArgs e) =>
        CreateModal.PrimaryButtonDisabled = string.IsNullOrWhiteSpace(e.NewTextValue);

    void OnCreateSubmit(object? sender, EventArgs e)
    {
        CreateModal.IsOpen = false;
        Status.Text = $"Created partition {NameInput.Text}";
    }

    void OnRequestClose(object? sender, CdsModalRequestCloseEventArgs e) => Status.Text = "Cancelled";
}

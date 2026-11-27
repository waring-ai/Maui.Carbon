using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Products;

[GalleryPage("Remove modal", GalleryCategory.Products, Order = 10, Spec = "RemoveModal",
    Summary = "Confirms removing or deleting a resource; for high-impact deletes the person types the resource name first.")]
public partial class RemoveModalPage : GalleryPage
{
    public RemoveModalPage()
    {
        InitializeComponent();
    }

    void OnRemoveClicked(object? sender, EventArgs e) => RemoveModal.IsOpen = true;

    void OnDeleteClicked(object? sender, EventArgs e) => DeleteModal.IsOpen = true;

    void OnRemoveSubmit(object? sender, EventArgs e)
    {
        RemoveModal.IsOpen = false;
        Status.Text = "Removed bx1001";
    }

    void OnDeleteSubmit(object? sender, EventArgs e)
    {
        DeleteModal.IsOpen = false;
        Status.Text = "Deleted bx1001";
    }

    void OnRequestClose(object? sender, CdsModalRequestCloseEventArgs e) => Status.Text = "Cancelled";
}

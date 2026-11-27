using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Products;

[GalleryPage("Export modal", GalleryCategory.Products, Order = 20, Spec = "ExportModal",
    Summary = "Exports a file: the person names it or picks a format, then sees the export run and succeed or fail.")]
public partial class ExportModalPage : GalleryPage
{
    bool _fail;

    public ExportModalPage()
    {
        InitializeComponent();
    }

    void OnExportClicked(object? sender, EventArgs e) => Open(false, ExportButton);

    void OnFailClicked(object? sender, EventArgs e) => Open(true, FailButton);

    void Open(bool fail, VisualElement launcher)
    {
        _fail = fail;
        ExportModal.IsLoading = false;
        ExportModal.IsSuccessful = false;
        ExportModal.IsError = false;
        ExportModal.LauncherElement = launcher;
        ExportModal.IsOpen = true;
    }

    async void OnExportSubmit(object? sender, CdsExportModalSubmitEventArgs e)
    {
        ExportModal.IsLoading = true;
        await Task.Delay(1000);
        ExportModal.IsLoading = false;
        if (_fail) ExportModal.IsError = true;
        else ExportModal.IsSuccessful = true;
        Status.Text = _fail ? $"Export of {e.FileName} failed" : $"Exported {e.FileName}";
    }

    void OnRequestClose(object? sender, CdsModalRequestCloseEventArgs e)
    {
        if (!ExportModal.IsSuccessful && !ExportModal.IsError) Status.Text = "Cancelled";
    }
}

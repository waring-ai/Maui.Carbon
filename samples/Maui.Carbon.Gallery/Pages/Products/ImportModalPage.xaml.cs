using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Products;

[GalleryPage("Import modal", GalleryCategory.Products, Order = 30, Spec = "ImportModal",
    Summary = "Imports a file by drag and drop, the file picker or a URL, with size and type validation.")]
public partial class ImportModalPage : GalleryPage
{
    public ImportModalPage()
    {
        InitializeComponent();
        // Sample files for the static examples: a small PNG that passes and a text file that does not.
        var folder = Path.Combine(Path.GetTempPath(), "Maui.Carbon.Gallery");
        Directory.CreateDirectory(folder);
        var png = Path.Combine(folder, "cluster-diagram.png");
        var text = Path.Combine(folder, "notes.txt");
        if (!File.Exists(png)) File.WriteAllBytes(png, new byte[2048]);
        if (!File.Exists(text)) File.WriteAllText(text, "Worker pool notes");
        ChosenExample.AddFile(png);
        InvalidExample.AddFile(text);
    }

    void OnImportClicked(object? sender, EventArgs e) => ImportModal.IsOpen = true;

    void OnImportSubmit(object? sender, CdsImportModalSubmitEventArgs e)
    {
        ImportModal.IsOpen = false;
        Status.Text = $"Imported {string.Join(", ", e.Files.Where(f => !f.IsInvalid).Select(f => f.Name))}";
    }

    void OnRequestClose(object? sender, CdsModalRequestCloseEventArgs e) => Status.Text = "Cancelled";
}

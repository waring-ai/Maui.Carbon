using Maui.Carbon.Gallery.Infrastructure;
using Maui.Carbon.Gallery.Pages.Forms.Data;

namespace Maui.Carbon.Gallery.Pages.Forms;

[GalleryPage("File uploader", GalleryCategory.Forms, Order = 90, Spec = "FileUploader",
    Summary = "Lets users pick files from their device and shows each file with its upload status, through a button or a dashed drag-and-drop container.")]
public partial class FileUploaderPage : GalleryPage
{
    public FileUploaderPage()
    {
        InitializeComponent();

        // Sample rows so the page shows every status without interaction.
        ButtonUploader.Files.Add(new CdsUploadedFile("cluster-eu-de-1.log") { Status = CdsFileStatus.Complete });
        ButtonUploader.Files.Add(new CdsUploadedFile("worker-events.txt") { Status = CdsFileStatus.Uploading });
        ButtonUploader.Files.Add(new CdsUploadedFile("topology.png"));

        DropUploader.Files.Add(new CdsUploadedFile("settings.yaml") { Status = CdsFileStatus.Complete });

        InvalidUploader.Files.Add(new CdsUploadedFile("full-dump.log")
        {
            IsInvalid = true,
            ErrorSubject = "File is too large",
            ErrorBody = "Choose a file under 500 MB.",
        });
        InvalidUploader.Files.Add(new CdsUploadedFile("audit.log") { Status = CdsFileStatus.Complete });

        // Choose a local file: the view model owns the path and its validation.
        LocalFileExample.BindingContext = new GLocalFileForm();
    }

    void OnFilesAdded(object? sender, CdsFilesAddedEventArgs e)
    {
        // Pretend to upload: the real work belongs to the app.
        foreach (var file in e.AddedFiles) file.Status = CdsFileStatus.Complete;
    }

    async void OnLiveAdded(object? sender, CdsFilesAddedEventArgs e)
    {
        Status.Text = $"Added {e.AddedFiles.Count} file(s); uploading";
        foreach (var file in e.AddedFiles) file.Status = CdsFileStatus.Uploading;
        await Task.Delay(1500);
        foreach (var file in e.AddedFiles) file.Status = CdsFileStatus.Complete;
        Status.Text = $"{LiveUploader.Files.Count} file(s) uploaded";
    }

    void OnLiveRemoved(object? sender, CdsFileRemovedEventArgs e) =>
        Status.Text = $"Removed {e.File?.Name}; {LiveUploader.Files.Count} file(s) left";
}

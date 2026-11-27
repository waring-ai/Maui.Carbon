namespace Maui.Carbon.Tests.Components;

public class ImportModalTests
{
    [Theory]
    [InlineData("photo.png", "image/png", "image/png", false)]
    [InlineData("photo.png", null, ".png", false)]
    [InlineData("photo.PNG", null, ".png", false)]
    [InlineData("photo.gif", "image/gif", "image/png, .jpg", true)]
    [InlineData("report", null, ".pdf", true)]
    [InlineData("report.pdf", null, "", false)]
    public void File_type_matches_a_mime_type_or_an_extension(string name, string? mime, string accept, bool invalid) =>
        Assert.Equal(invalid, ImportModalModel.IsInvalidFileType(name, mime, FileUploaderModel.ParseAccept(accept)));

    [Fact]
    public void Errors_are_classified_in_carbon_order()
    {
        Assert.Equal(ImportFileError.Fetch, ImportModalModel.Classify(true, true, 10, 1));
        Assert.Equal(ImportFileError.FileType, ImportModalModel.Classify(false, true, 10, 1));
        Assert.Equal(ImportFileError.FileSize, ImportModalModel.Classify(false, false, 500001, 500000));
        Assert.Equal(ImportFileError.None, ImportModalModel.Classify(false, false, 500000, 500000));
        Assert.Equal(ImportFileError.None, ImportModalModel.Classify(false, false, 10_000_000, 0));
        Assert.Equal(ImportFileError.None, ImportModalModel.Classify(false, false, null, 100));
    }

    [Fact]
    public void Messages_fall_back_to_the_defaults()
    {
        var texts = new CdsImportModal { DefaultErrorHeader = "Import failed", DefaultErrorBody = "Select a new file and try again.", FetchErrorBody = "Unable to fetch URL." };
        Assert.Equal(("Import failed", "Unable to fetch URL."), ImportModalModel.Messages(ImportFileError.Fetch, texts));
        Assert.Equal(("Import failed", "Select a new file and try again."), ImportModalModel.Messages(ImportFileError.FileType, texts));
        texts.MaxFileSizeErrorHeader = "Too large";
        texts.MaxFileSizeErrorBody = "500kb max file size.";
        Assert.Equal(("Too large", "500kb max file size."), ImportModalModel.Messages(ImportFileError.FileSize, texts));
        Assert.Equal((null, null), ImportModalModel.Messages(ImportFileError.None, texts));
    }

    [Theory]
    [InlineData("https://example.com/data/report.csv", "report.csv")]
    [InlineData("https://example.com/data/report.csv?token=1", "report.csv")]
    [InlineData("https://example.com/data/report.csv#top", "report.csv")]
    [InlineData("https://example.com/", "download")]
    [InlineData("", "download")]
    [InlineData(null, "download")]
    [InlineData("report.json", "report.json")]
    public void File_name_comes_from_the_url(string? url, string expected) =>
        Assert.Equal(expected, ImportModalModel.FileNameFromUrl(url));

    [Fact]
    public void Status_and_buttons()
    {
        Assert.Equal("1 / 2 files uploaded", ImportModalModel.StatusText(1, 2, "files uploaded"));
        Assert.Equal("0 / 1", ImportModalModel.StatusText(0, 1, null));
        Assert.True(ImportModalModel.IsPrimaryDisabled(0, 0));
        Assert.True(ImportModalModel.IsPrimaryDisabled(1, 0));
        Assert.False(ImportModalModel.IsPrimaryDisabled(1, 1));
        Assert.True(ImportModalModel.IsFetchDisabled("", false));
        Assert.True(ImportModalModel.IsFetchDisabled("  ", false));
        Assert.True(ImportModalModel.IsFetchDisabled("https://example.com/a.csv", true));
        Assert.False(ImportModalModel.IsFetchDisabled("https://example.com/a.csv", false));
    }

    [Fact]
    public void Constructs_and_takes_every_property()
    {
        var modal = new CdsImportModal
        {
            Title = "Import",
            Description = "You can specify a file to import by either dragging it into the drag and drop area or by specifying a URL. (Maximum file size of 500KB)",
            Accept = "image/png, image/jpeg, image/gif",
            MaxFileSize = 500000,
            DefaultErrorHeader = "Import failed",
            DefaultErrorBody = "Select a new file and try again.",
            FetchErrorHeader = "Import failed",
            FetchErrorBody = "Unable to fetch URL.",
            InvalidFileTypeErrorHeader = "Import failed",
            InvalidFileTypeErrorBody = "Invalid file type.",
            MaxFileSizeErrorHeader = "Import failed",
            MaxFileSizeErrorBody = "500kb max file size. Select a new file and try again.",
            FileDropHeader = "Add files using drag and drop",
            FileDropLabel = "Drag and drop files here or click to upload",
            FileUploadLabel = "files uploaded",
            InputLabel = "Add a file by specifying a URL",
            HideInputLabel = true,
            InputPlaceholder = "URL",
            InputButtonText = "Add file",
            InputButtonIcon = true,
            InvalidIconDescription = "Delete",
            PrimaryButtonText = "Import",
            SecondaryButtonText = "Cancel",
            RequestSubmitCommand = new Command(() => { }),
            HttpClient = new HttpClient(),
        };
        modal.IsInline = true;
        Assert.Equal(CdsIconName.Add, modal.FetchButton.Icon);
        Assert.True(modal.Footer!.PrimaryButtonDisabled);
    }

    [Fact]
    public void A_valid_file_enables_import_and_locks_the_inputs()
    {
        var path = Path.Combine(Path.GetTempPath(), $"maui-carbon-import-{Guid.NewGuid():N}.png");
        File.WriteAllBytes(path, new byte[100]);
        try
        {
            var modal = new CdsImportModal { Accept = "image/png", MaxFileSize = 500, FileUploadLabel = "files uploaded", PrimaryButtonText = "Import", SecondaryButtonText = "Cancel" };
            modal.UrlInput.Text = "https://example.com/a.png";
            Assert.True(modal.FetchButton.IsEnabled);
            modal.AddFile(path);
            var file = Assert.Single(modal.Files);
            Assert.False(file.IsInvalid);
            Assert.False(modal.Footer!.PrimaryButtonDisabled);
            Assert.False(modal.DropArea.IsEnabled);
            Assert.False(modal.UrlInput.IsEnabled);
            Assert.False(modal.FetchButton.IsEnabled);
            Assert.Equal("1 / 1 files uploaded", modal.StatusLine.Text);

            IReadOnlyList<CdsUploadedFile>? submitted = null;
            modal.RequestSubmit += (_, e) => submitted = e.Files;
            var primary = (CdsModalButton)modal.Footer.Buttons.Last();
            ((IButton)primary.HitTarget).Clicked();
            Assert.Same(file, Assert.Single(submitted!));

            modal.RemoveFile(file);
            Assert.Empty(modal.Files);
            Assert.True(modal.DropArea.IsEnabled);
            Assert.True(modal.Footer.PrimaryButtonDisabled);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Wrong_type_and_size_show_invalid_rows()
    {
        var big = Path.Combine(Path.GetTempPath(), $"maui-carbon-import-{Guid.NewGuid():N}.png");
        var text = Path.Combine(Path.GetTempPath(), $"maui-carbon-import-{Guid.NewGuid():N}.txt");
        File.WriteAllBytes(big, new byte[2000]);
        File.WriteAllText(text, "hello");
        try
        {
            var modal = new CdsImportModal
            {
                Accept = ".png",
                MaxFileSize = 1000,
                DefaultErrorHeader = "Import failed",
                DefaultErrorBody = "Select a new file and try again.",
                InvalidFileTypeErrorBody = "Invalid file type.",
                PrimaryButtonText = "Import",
            };
            modal.AddFile(big);
            var file = Assert.Single(modal.Files);
            Assert.True(file.IsInvalid);
            Assert.Equal("Import failed", file.ErrorSubject);
            Assert.Equal("Select a new file and try again.", file.ErrorBody);
            Assert.True(modal.Footer!.PrimaryButtonDisabled);

            modal.AddFile(text);
            file = Assert.Single(modal.Files);
            Assert.Equal("Invalid file type.", file.ErrorBody);
        }
        finally
        {
            File.Delete(big);
            File.Delete(text);
        }
    }

    [Fact]
    public async Task A_failed_fetch_shows_a_fetch_error_row()
    {
        var modal = new CdsImportModal { FetchErrorHeader = "Import failed", FetchErrorBody = "Unable to fetch URL.", HttpClient = new HttpClient(new FailingHandler()) };
        modal.UrlInput.Text = "https://example.com/data/report.csv";
        await modal.FetchAsync();
        var file = Assert.Single(modal.Files);
        Assert.Equal("report.csv", file.Name);
        Assert.True(file.IsInvalid);
        Assert.Equal("Unable to fetch URL.", file.ErrorBody);
        Assert.Equal(CdsFileStatus.Edit, file.Status);
    }

    [Fact]
    public async Task A_fetched_file_is_validated_like_a_picked_one()
    {
        var modal = new CdsImportModal { Accept = "text/csv", HttpClient = new HttpClient(new CsvHandler()), PrimaryButtonText = "Import" };
        modal.UrlInput.Text = "https://example.com/data/report.csv?x=1";
        await modal.FetchAsync();
        var file = Assert.Single(modal.Files);
        Assert.Equal("report.csv", file.Name);
        Assert.False(file.IsInvalid);
        Assert.NotNull(file.Path);
        Assert.Equal("a,b", await File.ReadAllTextAsync(file.Path!, TestContext.Current.CancellationToken));
        Assert.False(modal.Footer!.PrimaryButtonDisabled);
    }

    [Fact]
    public void Closing_clears_the_files_and_url()
    {
        var path = Path.Combine(Path.GetTempPath(), $"maui-carbon-import-{Guid.NewGuid():N}.csv");
        File.WriteAllText(path, "a,b");
        try
        {
            var modal = new CdsImportModal();
            modal.IsOpen = true;
            modal.AddFile(path);
            modal.UrlInput.Text = "https://example.com";
            modal.IsOpen = false;
            Assert.Empty(modal.Files);
            Assert.Equal(string.Empty, modal.UrlInput.Text);
        }
        finally
        {
            File.Delete(path);
        }
    }

    sealed class FailingHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.NotFound));
    }

    sealed class CsvHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var content = new StringContent("a,b");
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/csv");
            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK) { Content = content });
        }
    }
}

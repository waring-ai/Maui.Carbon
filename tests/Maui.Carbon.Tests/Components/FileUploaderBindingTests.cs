using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Maui.Carbon.Tests.Foundation;

namespace Maui.Carbon.Tests.Components;

/// <summary>The file uploader as an MVVM file picker: bound paths, view-model validation, commands and the accept list.</summary>
public class FileUploaderBindingTests
{
    sealed class ImportForm : INotifyPropertyChanged
    {
        string? _archivePath;
        IReadOnlyList<string>? _paths;
        bool _isInvalid;
        string? _error;
        string? _errorBody;

        public event PropertyChangedEventHandler? PropertyChanged;

        public List<object?> Added { get; } = [];

        public List<object?> Removed { get; } = [];

        public ICommand AddedCommand => new Command(p => Added.Add(p));

        public ICommand RemovedCommand => new Command(p => Removed.Add(p));

        public string? ArchivePath
        {
            get => _archivePath;
            set
            {
                if (!Set(ref _archivePath, value)) return;
                // Validates the way an app would: synchronously when the path changes.
                var missing = !string.IsNullOrEmpty(value) && value.Contains("missing", StringComparison.Ordinal);
                ErrorBody = missing ? "Choose a .zip export that exists." : null;
                Error = missing ? "File not found" : null;
                IsInvalid = missing;
            }
        }

        public IReadOnlyList<string>? Paths
        {
            get => _paths;
            set => Set(ref _paths, value);
        }

        public bool IsInvalid
        {
            get => _isInvalid;
            set => Set(ref _isInvalid, value);
        }

        public string? Error
        {
            get => _error;
            set => Set(ref _error, value);
        }

        public string? ErrorBody
        {
            get => _errorBody;
            set => Set(ref _errorBody, value);
        }

        bool Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            return true;
        }
    }

    sealed class Grant : IDisposable
    {
        public bool Disposed { get; private set; }

        public void Dispose() => Disposed = true;
    }

    static IEnumerable<Element> Tree(Element e) => ContainerSemanticsAuditTests.Walk(e);

    static CdsFileUploader Bound(ImportForm form, bool bindPaths = false)
    {
        var uploader = new CdsFileUploader { Accept = ".zip", AccessMode = CdsFileAccessMode.Path, BindingContext = form };
        uploader.SetBinding(CdsFileUploader.FilePathProperty, nameof(ImportForm.ArchivePath));
        if (bindPaths) uploader.SetBinding(CdsFileUploader.FilePathsProperty, nameof(ImportForm.Paths));
        uploader.SetBinding(CdsFileUploader.IsInvalidProperty, nameof(ImportForm.IsInvalid));
        uploader.SetBinding(CdsFileUploader.ErrorSubjectProperty, nameof(ImportForm.Error));
        uploader.SetBinding(CdsFileUploader.ErrorBodyProperty, nameof(ImportForm.ErrorBody));
        uploader.SetBinding(CdsFileUploader.FilesAddedCommandProperty, nameof(ImportForm.AddedCommand));
        uploader.SetBinding(CdsFileUploader.FileRemovedCommandProperty, nameof(ImportForm.RemovedCommand));
        return uploader;
    }

    static FormsTestHost Host(ImportForm form, out CdsFileUploader uploader, bool bindPaths = false) =>
        FormsTestHost.Load(() => Bound(form, bindPaths), out uploader);

    // The rows in display order (the tree walk is depth-first from the end).
    static IEnumerable<CdsFileUploaderItem> Rows(CdsFileUploader uploader) =>
        Tree(uploader).OfType<Layout>().FirstOrDefault(l => l.Children.OfType<CdsFileUploaderItem>().Any())?.Children.OfType<CdsFileUploaderItem>() ?? [];

    static void RemoveFirstRow(CdsFileUploader uploader) =>
        Tree(uploader).OfType<CdsFieldIconButton>().First(b => b.IsVisible).HitTarget.RaiseKeyboardActivation();

    // ---- The chosen file, both ways ----

    [Fact]
    public void A_path_the_view_model_pre_fills_is_shown_as_the_chosen_file()
    {
        var form = new ImportForm { ArchivePath = "/Users/maya/exports/domains.zip" };
        using var host = Host(form, out var uploader);
        var file = Assert.Single(uploader.Files);
        Assert.Equal("domains.zip", file.Name);
        Assert.Equal("/Users/maya/exports/domains.zip", file.Path);
        Assert.Null(file.Result); // a path, not a platform pick
        Assert.Equal(["/Users/maya/exports/domains.zip"], uploader.FilePaths);
        Assert.Equal("domains.zip", Assert.Single(Rows(uploader)).Name);
    }

    [Fact]
    public void Picking_a_file_replaces_the_choice_and_updates_the_view_model()
    {
        var form = new ImportForm { ArchivePath = "/exports/domains.zip" };
        using var host = Host(form, out var uploader);
        uploader.AddResults([new FileResult("/Volumes/backup/domains-2026-11.zip")]);
        Assert.Equal("/Volumes/backup/domains-2026-11.zip", form.ArchivePath);
        Assert.Equal("domains-2026-11.zip", Assert.Single(uploader.Files).Name);
        Assert.Single(Rows(uploader));
        Assert.Equal([new[] { "/Volumes/backup/domains-2026-11.zip" }], form.Added.Cast<IReadOnlyList<string>>());
    }

    [Fact]
    public void Files_the_accept_list_does_not_allow_change_nothing()
    {
        var form = new ImportForm { ArchivePath = "/exports/domains.zip" };
        using var host = Host(form, out var uploader);
        uploader.AddResults([new FileResult("/exports/notes.txt")]);
        Assert.Equal("/exports/domains.zip", form.ArchivePath);
        Assert.Empty(form.Added);
    }

    [Fact]
    public void Removing_the_row_clears_the_path_and_runs_the_command()
    {
        var form = new ImportForm { ArchivePath = "/exports/domains.zip" };
        using var host = Host(form, out var uploader);
        RemoveFirstRow(uploader);
        Assert.Null(form.ArchivePath);
        Assert.Empty(uploader.Files);
        Assert.Empty(Rows(uploader));
        Assert.Equal(["/exports/domains.zip"], form.Removed);
        Assert.Empty(uploader.FilePaths!);
    }

    [Fact]
    public void Setting_the_path_from_the_view_model_replaces_or_clears_the_row()
    {
        var form = new ImportForm { ArchivePath = "/a/one.zip" };
        using var host = Host(form, out var uploader);
        form.ArchivePath = "/b/two.zip";
        Assert.Equal("two.zip", Assert.Single(uploader.Files).Name);
        form.ArchivePath = "";
        Assert.Empty(uploader.Files);
        Assert.Equal("", form.ArchivePath); // "" and null both mean no file: the view model's empty string is left alone
        form.ArchivePath = "/c/three.zip";
        Assert.Equal("three.zip", Assert.Single(uploader.Files).Name);
        form.ArchivePath = null;
        Assert.Empty(uploader.Files);
    }

    [Fact]
    public void Code_that_edits_files_updates_the_bound_path()
    {
        var form = new ImportForm();
        using var host = Host(form, out var uploader);
        uploader.Files.Add(CdsUploadedFile.FromPath("/x/domains.zip"));
        Assert.Equal("/x/domains.zip", form.ArchivePath);
        uploader.Files.Clear();
        Assert.Null(form.ArchivePath);
    }

    // ---- Several files ----

    [Fact]
    public void File_paths_follow_the_files_and_the_files_follow_a_new_list()
    {
        var form = new ImportForm();
        using var host = Host(form, out var uploader, bindPaths: true);
        uploader.Accept = ".log";
        uploader.Multiple = true;
        uploader.AddResults([new FileResult("/logs/a.log"), new FileResult("/logs/b.log")]);
        Assert.Equal(["/logs/a.log", "/logs/b.log"], form.Paths);
        Assert.Equal("/logs/a.log", form.ArchivePath);

        var b = uploader.Files[1];
        b.Status = CdsFileStatus.Complete;
        form.Paths = ["/logs/b.log", "/logs/c.log", "", "/logs/c.log"];
        Assert.Equal(["b.log", "c.log"], uploader.Files.Select(f => f.Name));
        Assert.Same(b, uploader.Files[0]); // a path that stays keeps its entry and status
        Assert.Equal(CdsFileStatus.Complete, uploader.Files[0].Status);
        Assert.Equal(["b.log", "c.log"], Rows(uploader).Select(r => r.Name));
        Assert.Equal("/logs/b.log", form.ArchivePath);
    }

    [Fact]
    public void The_uploader_never_changes_a_list_it_was_given()
    {
        var form = new ImportForm();
        using var host = Host(form, out var uploader, bindPaths: true);
        uploader.Multiple = true;
        uploader.Accept = null;
        var given = new List<string> { "/a/1.csv" };
        form.Paths = given;
        uploader.AddResults([new FileResult("/a/2.csv")]);
        Assert.Equal(["/a/1.csv"], given);
        Assert.Equal(["/a/1.csv", "/a/2.csv"], form.Paths);
    }

    [Fact]
    public void One_file_uploaders_keep_the_first_of_a_bound_list()
    {
        var form = new ImportForm();
        using var host = Host(form, out var uploader, bindPaths: true);
        form.Paths = ["/a/1.zip", "/a/2.zip"];
        Assert.Equal("1.zip", Assert.Single(uploader.Files).Name);
        Assert.Equal(["/a/1.zip"], form.Paths);
    }

    [Fact]
    public void Rows_follow_inserts_and_moves_in_order()
    {
        var uploader = new CdsFileUploader { Multiple = true };
        uploader.Files.Add(new CdsUploadedFile("a.csv"));
        uploader.Files.Add(new CdsUploadedFile("c.csv"));
        uploader.Files.Insert(1, new CdsUploadedFile("b.csv"));
        uploader.Files.Move(0, 2);
        Assert.Equal(["b.csv", "c.csv", "a.csv"], Rows(uploader).Select(r => r.Name));
    }

    // ---- Validation from the view model ----

    [Fact]
    public void A_missing_file_shows_as_an_invalid_row_with_the_view_models_error()
    {
        var form = new ImportForm { ArchivePath = "/exports/missing.zip" };
        using var host = Host(form, out var uploader);
        var file = Assert.Single(uploader.Files);
        Assert.True(file.IsInvalid);
        Assert.Equal("File not found", file.ErrorSubject);
        Assert.Equal("Choose a .zip export that exists.", file.ErrorBody);
        var row = Assert.Single(Rows(uploader));
        Assert.True(row.IsInvalid);
        Assert.Equal("File not found", row.ErrorSubject);
        // aria-describedby: the error describes the row's remove button.
        var remove = Tree(row).OfType<CdsFieldIconButton>().Single(b => b.IsVisible);
        Assert.Equal("File not found. Choose a .zip export that exists.", SemanticProperties.GetHint(remove));
        Assert.Equal("Remove file - missing.zip", remove.Description);

        form.ArchivePath = "/exports/domains.zip";
        var chosen = Assert.Single(uploader.Files);
        Assert.False(chosen.IsInvalid);
        Assert.Null(chosen.ErrorSubject);
    }

    [Fact]
    public void An_invalid_choice_is_announced_once_as_an_alert()
    {
        var form = new ImportForm { ArchivePath = "/exports/domains.zip" };
        using var host = Host(form, out var uploader);
        using var heard = CdsAnnouncer.Capture();
        form.ArchivePath = "/exports/missing.zip";
        Assert.Equal(
            new CdsAnnouncement("missing.zip: File not found. Choose a .zip export that exists.", CdsAnnouncePriority.Assertive),
            Assert.Single(heard.Announcements, a => a.Priority == CdsAnnouncePriority.Assertive));
    }

    [Fact]
    public void Error_properties_apply_to_the_chosen_file_and_the_next_one()
    {
        var uploader = new CdsFileUploader { FilePath = "/certs/server.pem" };
        uploader.ErrorSubject = "Certificate expired";
        uploader.IsInvalid = true;
        Assert.True(uploader.Files[0].IsInvalid);
        Assert.Equal("Certificate expired", uploader.Files[0].ErrorSubject);
        uploader.FilePath = "/certs/other.pem";
        Assert.True(uploader.Files[0].IsInvalid); // the properties describe whichever file is chosen
        uploader.IsInvalid = false;
        Assert.False(uploader.Files[0].IsInvalid);
    }

    [Fact]
    public void Unused_error_properties_leave_file_entries_alone()
    {
        var uploader = new CdsFileUploader();
        uploader.Files.Add(new CdsUploadedFile("big.mov") { IsInvalid = true, ErrorSubject = "Too large" });
        Assert.True(uploader.Files[0].IsInvalid);
        Assert.Equal("Too large", uploader.Files[0].ErrorSubject);
    }

    [Fact]
    public void File_status_sets_the_chosen_rows_status()
    {
        var uploader = new CdsFileUploader { FilePath = "/a/domains.zip" };
        Assert.Equal(CdsFileStatus.Edit, uploader.Files[0].Status);
        uploader.FileStatus = CdsFileStatus.Uploading;
        Assert.Equal(CdsFileStatus.Uploading, Assert.Single(Rows(uploader)).Status);
        uploader.FilePath = "/a/next.zip";
        Assert.Equal(CdsFileStatus.Uploading, uploader.Files[0].Status);
        uploader.FileStatus = CdsFileStatus.Complete;
        Assert.Equal(CdsFileStatus.Complete, uploader.Files[0].Status);
        uploader.FileStatus = null;
        Assert.Equal(CdsFileStatus.Complete, uploader.Files[0].Status); // null leaves the file as it is
    }

    [Fact]
    public void Several_files_ignore_the_single_file_state()
    {
        var uploader = new CdsFileUploader { Multiple = true, IsInvalid = true, ErrorSubject = "Bad", FileStatus = CdsFileStatus.Complete };
        uploader.AddResults([new FileResult("/a/1.csv")]);
        Assert.False(uploader.Files[0].IsInvalid);
        Assert.Equal(CdsFileStatus.Edit, uploader.Files[0].Status);
    }

    // ---- A chosen file is not an upload ----

    [Fact]
    public void The_button_says_change_file_once_a_file_is_chosen()
    {
        var uploader = new CdsFileUploader { ButtonLabel = "Choose file", ChosenButtonLabel = "Change file" };
        var button = Tree(uploader).OfType<CdsButton>().Single();
        Assert.Equal("Choose file", button.Text);
        uploader.FilePath = "/a/domains.zip";
        Assert.Equal("Change file", button.Text);
        uploader.Files.Clear();
        Assert.Equal("Choose file", button.Text);
        uploader.ChosenButtonLabel = null;
        uploader.FilePath = "/a/domains.zip";
        Assert.Equal("Choose file", button.Text); // Carbon's FileUploader keeps its label by default
    }

    // ---- Paths and platform access ----

    [Fact]
    public void Uploaded_files_carry_their_full_path()
    {
        Assert.Equal("/tmp/a.csv", new CdsUploadedFile("a.csv", new FileResult("/tmp/a.csv")).Path);
        Assert.Null(new CdsUploadedFile("a.csv").Path);
        var file = CdsUploadedFile.FromPath("/var/data/export.zip");
        Assert.Equal(("export.zip", "/var/data/export.zip"), (file.Name, file.Path));
        Assert.Throws<ArgumentException>(() => CdsUploadedFile.FromPath(" "));
    }

    [Fact]
    public async Task Uploaded_files_open_from_their_path()
    {
        var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"carbon-{Guid.NewGuid():N}.txt");
        await File.WriteAllTextAsync(path, "prod-eu-de-1", TestContext.Current.CancellationToken);
        try
        {
            await using var stream = await CdsUploadedFile.FromPath(path).OpenReadAsync();
            using var reader = new StreamReader(stream);
            Assert.Equal("prod-eu-de-1", await reader.ReadToEndAsync(TestContext.Current.CancellationToken));
        }
        finally
        {
            File.Delete(path);
        }
        await Assert.ThrowsAsync<InvalidOperationException>(() => new CdsUploadedFile("name-only.csv").OpenReadAsync());
    }

    [Fact]
    public void Access_to_a_file_ends_when_it_leaves_the_uploader()
    {
        var uploader = new CdsFileUploader { Multiple = true };
        var kept = new Grant();
        var removed = new Grant();
        var a = CdsUploadedFile.FromPath("/a/1.zip");
        var b = CdsUploadedFile.FromPath("/a/2.zip");
        a.Access = kept;
        b.Access = removed;
        uploader.Files.Add(a);
        uploader.Files.Add(b);
        uploader.FilePaths = ["/a/1.zip"];
        Assert.False(kept.Disposed); // re-assigning a list keeps the entries (and access) of paths that stay
        Assert.True(removed.Disposed);
        uploader.Multiple = false;
        uploader.FilePath = "/a/3.zip";
        Assert.True(kept.Disposed); // a new choice replaces it
    }

    [Fact]
    public void A_rejected_drop_releases_its_access_at_once()
    {
        var uploader = new CdsFileUploader { Accept = ".zip" };
        var rejected = new Grant();
        var accepted = new Grant();
        uploader.AddGrants([new CdsFileGrant("/a/notes.txt", rejected), new CdsFileGrant("/a/domains.zip", accepted)]);
        Assert.True(rejected.Disposed);
        Assert.False(accepted.Disposed);
        Assert.Equal("/a/domains.zip", Assert.Single(uploader.Files).Path);
        uploader.Files.Clear();
        Assert.True(accepted.Disposed);
    }

    [Fact]
    public void A_null_list_changes_nothing_and_an_empty_one_clears()
    {
        var uploader = new CdsFileUploader { FilePath = "/a/domains.zip" };
        uploader.FilePaths = null;
        Assert.Single(uploader.Files);
        uploader.FilePaths = [];
        Assert.Empty(uploader.Files);
        Assert.Null(uploader.FilePath);
    }

    [Fact]
    public void New_properties_can_be_set_without_throwing()
    {
        var uploader = new CdsFileUploader
        {
            FilePath = "/a/b.zip", FilePaths = ["/a/b.zip"], IsInvalid = true, ErrorSubject = "S", ErrorBody = "B",
            FileStatus = CdsFileStatus.Complete, ChosenButtonLabel = "Change file", AccessMode = CdsFileAccessMode.Path,
            FilesAddedCommand = new Command(() => { }), FileRemovedCommand = new Command(() => { }),
        };
        Assert.Equal(CdsFileAccessMode.Path, uploader.AccessMode);
        Assert.Equal(CdsFileAccessMode.Stream, new CdsFileUploader().AccessMode);
    }

    // ---- Accept list → platform types (pure) ----

    static string? FakeUti(string extension) => extension switch
    {
        ".heic" => "public.heic",
        ".gz" => "org.gnu.gnu-zip-archive",
        _ => null, // the system has only a dynamic type
    };

    static string? FakeMimeUti(string mime) => mime == "application/x-pem-file" ? "public.x509-certificate" : null;

    [Fact]
    public void Extensions_map_to_uniform_types_at_runtime_with_the_table_as_fallback()
    {
        var accept = FileUploaderModel.ParseAccept(".heic, .zip, .pem, .tar.gz");
        Assert.Equal(["public.heic", "public.zip-archive", ".pem", "org.gnu.gnu-zip-archive"],
            FileUploaderModel.UniformTypes(accept, FakeUti));
        // Without the platform database the table still answers, and unknown extensions stay extensions.
        Assert.Equal([".heic", "public.zip-archive", ".pem", ".gz"], FileUploaderModel.UniformTypes(accept));
    }

    [Fact]
    public void Mime_types_and_wildcards_map_to_uniform_types()
    {
        Assert.Equal(["public.image", "public.movie", "public.audio", "public.text", "public.data", "public.font"],
            FileUploaderModel.UniformTypes(FileUploaderModel.ParseAccept("image/*, video/*, audio/*, text/*, application/*, font/*")));
        Assert.Equal(["public.x509-certificate", "public.zip-archive"],
            FileUploaderModel.UniformTypes(FileUploaderModel.ParseAccept("application/x-pem-file, application/zip"), utiOfMime: FakeMimeUti));
        Assert.Equal([FileUploaderModel.AnyUniformType], FileUploaderModel.UniformTypes(FileUploaderModel.ParseAccept("*/*")));
        // Nothing maps: offer any item and let the accept check filter, rather than offering nothing.
        Assert.Equal([FileUploaderModel.AnyUniformType], FileUploaderModel.UniformTypes(FileUploaderModel.ParseAccept("application/x-unknown")));
        Assert.Empty(FileUploaderModel.UniformTypes([]));
    }

    [Theory]
    [InlineData("photo.heic", "image/*", true)]
    [InlineData("server.pem", "application/x-pem-file", true)]
    [InlineData("backup.tar.gz", ".tar.gz", true)]
    [InlineData("backup.gz", ".tar.gz", false)]
    [InlineData("anything.bin", "*/*", true)]
    [InlineData("domains.zip", "application/*", true)]
    public void Matching_uses_the_runtime_mime_type(string file, string accept, bool expected)
    {
        static string? Mime(string extension) => extension == ".heic" ? "image/heic" : null;
        Assert.Equal(expected, FileUploaderModel.Matches(file, FileUploaderModel.ParseAccept(accept), Mime));
    }

    [Fact]
    public void Path_helpers()
    {
        Assert.True(FileUploaderModel.SamePath(null, ""));
        Assert.False(FileUploaderModel.SamePath("/a", "/A"));
        Assert.True(FileUploaderModel.SamePaths(null, []));
        Assert.False(FileUploaderModel.SamePaths(["/a", "/b"], ["/b", "/a"]));
        Assert.Equal(["/a", "/b"], FileUploaderModel.NormalizePaths(["/a", null, "", "/b", "/a"], multiple: true));
        Assert.Equal(["/a"], FileUploaderModel.NormalizePaths(["/a", "/b"], multiple: false));
        Assert.Equal("Change file", FileUploaderModel.ButtonText("Choose file", "Change file", hasFiles: true));
        Assert.Equal("Choose file", FileUploaderModel.ButtonText("Choose file", "Change file", hasFiles: false));
        Assert.Equal("Choose file", FileUploaderModel.ButtonText("Choose file", null, hasFiles: true));
    }
}

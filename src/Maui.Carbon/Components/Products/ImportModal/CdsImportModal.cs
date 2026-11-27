using System.Collections.ObjectModel;

namespace Maui.Carbon;

/// <summary>Event data of <see cref="CdsImportModal.RequestSubmit"/>: the chosen files.</summary>
public sealed class CdsImportModalSubmitEventArgs : EventArgs
{
    /// <summary>Creates the event data.</summary>
    public CdsImportModalSubmitEventArgs(IReadOnlyList<CdsUploadedFile> files) => Files = files;

    /// <summary>Every chosen file, invalid ones included (check <see cref="CdsUploadedFile.IsInvalid"/>); read each with <see cref="CdsUploadedFile.OpenReadAsync"/>.</summary>
    public IReadOnlyList<CdsUploadedFile> Files { get; }
}

/// <summary>
/// The modal of the import pattern (Carbon for IBM Products <c>ImportModal</c>): the person adds a file by dropping it on
/// the drop area, by choosing it in the file picker (activate the area) or by typing a URL and fetching it. The file shows
/// as a row; one over <see cref="MaxFileSize"/>, of a type outside <see cref="Accept"/> or that could not be fetched shows
/// as an invalid row with its error, and Import stays disabled until a valid file is chosen. Remove the file to choose
/// another. The modal has no close button and never closes on a tap outside; Cancel and Escape ask to close and clear the
/// files. It does not upload: <see cref="RequestSubmit"/> hands the files over.
/// </summary>
public partial class CdsImportModal : CdsProductModal
{
    static readonly Lazy<HttpClient> s_http = new(() => new HttpClient());

    readonly CdsProductModalHeader _header = new(CdsProductModalHeaderStyle.Import);
    readonly CdsModalFooter _footer = new();
    readonly CdsEndInset _descriptionBox;
    readonly CdsText _description;
    readonly CdsText _dropHeader = new() { TypeStyle = CdsTypeStyle.HeadingCompact01, Margin = new Thickness(0, 0, 0, CdsSpacing.Spacing03), IsVisible = false };
    readonly CdsFileDropContainer _drop = new() { Margin = new Thickness(0, 0, 0, CdsSpacing.Spacing05) };
    readonly CdsText _inputHeading = new() { TypeStyle = CdsTypeStyle.HeadingCompact01, Margin = new Thickness(0, 0, 0, CdsSpacing.Spacing03), IsVisible = false };
    readonly CdsTextInput _url = new() { HideLabel = true };
    readonly CdsButton _fetch = new() { Size = CdsSize.Md, Margin = new Thickness(CdsSpacing.Spacing03, 0, 0, 0), VerticalOptions = LayoutOptions.Start };
    readonly CdsText _status = new() { TypeStyle = CdsTypeStyle.HelperText01, Tone = CdsTextTone.Helper, Margin = new Thickness(0, CdsSpacing.Spacing06, 0, CdsSpacing.Spacing03), IsVisible = false };
    readonly VerticalStackLayout _rows = new() { Spacing = CdsSpacing.Spacing03 };
    readonly ObservableCollection<CdsUploadedFile> _files = [];
    readonly Dictionary<CdsUploadedFile, CdsFileUploaderItem> _rowOf = [];
    readonly Dictionary<CdsUploadedFile, long?> _sizes = [];
    CancellationTokenSource? _fetching;

    /// <summary>Creates a closed import modal.</summary>
    public CdsImportModal() : base(CdsModalSize.Sm, preventCloseOnClickOutside: true)
    {
        (_descriptionBox, _description) = CdsProductModalParts.Paragraph(CdsTypeStyle.Body01, ProductModalModel.TwentyPercentLessSpacing05,
            new Thickness(0, 0, 0, CdsSpacing.Spacing06));

        var urlRow = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) } };
        urlRow.Add(_url, 0);
        urlRow.Add(_fetch, 1);

        var stack = new VerticalStackLayout { Spacing = 0 };
        stack.Add(_descriptionBox);
        stack.Add(_dropHeader);
        stack.Add(_drop);
        stack.Add(_inputHeading);
        stack.Add(urlRow);
        stack.Add(_status);
        stack.Add(_rows);

        _drop.Activated += async (_, _) => await PickFileAsync();
        _drop.FilesDropped += (_, grants) => AddGrants(grants);
        _url.TextChanged += (_, _) => Sync();
        _url.Completed += async (_, _) => await FetchAsync();
        _fetch.Clicked += async (_, _) => await FetchAsync();
        _files.CollectionChanged += (_, _) => SyncRows();
        _footer.RequestSubmit += (_, _) => Submit();
        SetParts(_header, new CdsModalBody { HasForm = true, Body = stack }, _footer);
        Sync();
    }

    /// <summary>
    /// Raised with the files when the primary button is activated (<c>onRequestSubmit</c>). The modal stays open: import,
    /// then close it.
    /// </summary>
    public event EventHandler<CdsImportModalSubmitEventArgs>? RequestSubmit;

    /// <summary>The chosen files, valid and invalid, in the order they were added.</summary>
    public IReadOnlyList<CdsUploadedFile> Files => _files;

    /// <summary>
    /// The client that fetches a URL (default: one shared <see cref="System.Net.Http.HttpClient"/>). Set your own for
    /// authentication headers, a proxy or tests.
    /// </summary>
    public HttpClient? HttpClient { get; set; }

    /// <summary>The URL field (for tests).</summary>
    internal CdsTextInput UrlInput => _url;

    /// <summary>The fetch button (for tests).</summary>
    internal CdsButton FetchButton => _fetch;

    /// <summary>The drop area (for tests).</summary>
    internal CdsFileDropContainer DropArea => _drop;

    /// <summary>The count line above the rows (for tests).</summary>
    internal CdsText StatusLine => _status;

    private protected override VisualElement? DefaultInitialFocus => _drop;

    private protected override void OnOpening() => Clear();

    private protected override void OnClosed() => Clear();

    /// <summary>
    /// Adds a file the app already has (a path it remembered, a file it received), validated like a picked one. Any
    /// chosen file is replaced: the modal imports one file at a time.
    /// </summary>
    public void AddFile(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        long? size = File.Exists(path) ? new FileInfo(path).Length : null;
        Replace(CdsUploadedFile.FromPath(path), size, MimeOf(Path.GetFileName(path)), fetchError: false);
    }

    /// <summary>Opens the platform file picker (filtered by <see cref="Accept"/>) and adds the choice.</summary>
    public async Task PickFileAsync()
    {
        if (_files.Count > 0) return;
        var accept = FileUploaderModel.ParseAccept(Accept);
        try
        {
            var options = new PickOptions { PickerTitle = FileDropLabel };
            if (accept.Count > 0)
            {
                options.FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    [DevicePlatform.WinUI] = FileUploaderModel.Extensions(accept),
                    [DevicePlatform.Android] = FileUploaderModel.MimeTypes(accept),
                    [DevicePlatform.iOS] = UniformTypes(accept),
                    [DevicePlatform.MacCatalyst] = UniformTypes(accept),
                });
            }
            if (await FilePicker.Default.PickAsync(options) is not { } result) return;
            long? size = null;
            try
            {
                if (!string.IsNullOrEmpty(result.FullPath) && File.Exists(result.FullPath)) size = new FileInfo(result.FullPath).Length;
                else
                {
                    await using var stream = await result.OpenReadAsync();
                    size = stream.CanSeek ? stream.Length : null;
                }
            }
            catch (IOException)
            {
                // The size stays unknown; the size limit then cannot reject the file.
            }
            Replace(new CdsUploadedFile(result.FileName, result), size, result.ContentType ?? MimeOf(result.FileName), fetchError: false);
        }
        catch (Exception ex) when (ex is OperationCanceledException or PermissionException or NotSupportedException or FeatureNotSupportedException)
        {
            // Cancelled, denied or not available on this platform: nothing to add.
        }
    }

    /// <summary>Fetches the typed URL and adds the downloaded file, or an invalid row when the fetch fails.</summary>
    public async Task FetchAsync()
    {
        var url = _url.Text?.Trim();
        if (ImportModalModel.IsFetchDisabled(url, _files.Count > 0)) return;
        var name = ImportModalModel.FileNameFromUrl(url);
        var pending = new CdsUploadedFile(name) { Status = CdsFileStatus.Uploading };
        SetFiles(pending, null);
        _fetching?.Cancel();
        var cts = _fetching = new CancellationTokenSource();
        try
        {
            using var response = await (HttpClient ?? s_http.Value).GetAsync(url, cts.Token);
            if (!response.IsSuccessStatusCode) throw new HttpRequestException(response.StatusCode.ToString());
            var bytes = await response.Content.ReadAsByteArrayAsync(cts.Token);
            var folder = Path.Combine(Path.GetTempPath(), "Maui.Carbon.Import", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(folder);
            var path = Path.Combine(folder, name);
            await File.WriteAllBytesAsync(path, bytes, cts.Token);
            if (cts.IsCancellationRequested || !_files.Contains(pending)) return;
            Replace(CdsUploadedFile.FromPath(path), bytes.LongLength, response.Content.Headers.ContentType?.MediaType ?? MimeOf(name), fetchError: false);
        }
        catch (Exception ex) when (ex is HttpRequestException or IOException or InvalidOperationException or UriFormatException or TaskCanceledException or UnauthorizedAccessException)
        {
            if (cts.IsCancellationRequested || !_files.Contains(pending)) return;
            Replace(new CdsUploadedFile(name), null, null, fetchError: true);
        }
    }

    /// <summary>Removes a chosen file (its row's remove button does this).</summary>
    public void RemoveFile(CdsUploadedFile file)
    {
        if (!_files.Remove(file)) return;
        file.ReleaseAccess();
        _sizes.Remove(file);
        Sync();
    }

    void AddGrants(IReadOnlyList<CdsFileGrant> grants)
    {
        if (_files.Count > 0 || grants.Count == 0)
        {
            foreach (var g in grants) g.Access?.Dispose();
            return;
        }
        var grant = grants[0];
        foreach (var other in grants.Skip(1)) other.Access?.Dispose();
        var name = Path.GetFileName(grant.Path);
        var file = string.IsNullOrEmpty(name) ? CdsUploadedFile.FromPath(grant.Path) : new CdsUploadedFile(name, new FileResult(grant.Path));
        file.Access = grant.Access;
        long? size = File.Exists(grant.Path) ? new FileInfo(grant.Path).Length : null;
        Replace(file, size, MimeOf(file.Name ?? string.Empty), fetchError: false);
    }

    void Replace(CdsUploadedFile file, long? size, string? mimeType, bool fetchError)
    {
        var accept = FileUploaderModel.ParseAccept(Accept);
        var error = ImportModalModel.Classify(fetchError, !fetchError && ImportModalModel.IsInvalidFileType(file.Name, mimeType, accept), size, MaxFileSize);
        var (subject, body) = ImportModalModel.Messages(error, this);
        file.Status = CdsFileStatus.Edit;
        file.IsInvalid = error != ImportFileError.None;
        file.ErrorSubject = subject;
        file.ErrorBody = body;
        SetFiles(file, size);
        if (!IsShowing || !IsLoaded) return;
        CdsAnnouncer.Announce(file.IsInvalid
            ? FileUploaderModel.InvalidAnnouncement(file.Name, FileUploaderModel.ErrorText(true, subject, body))
            : FileUploaderModel.AddedAnnouncement([file.Name ?? string.Empty]));
    }

    void SetFiles(CdsUploadedFile file, long? size)
    {
        foreach (var old in _files.ToList())
        {
            if (ReferenceEquals(old, file)) continue;
            old.ReleaseAccess();
            _sizes.Remove(old);
        }
        _files.Clear();
        _sizes[file] = size;
        _files.Add(file);
        Sync();
    }

    void Clear()
    {
        _fetching?.Cancel();
        _fetching = null;
        foreach (var f in _files) f.ReleaseAccess();
        _files.Clear();
        _sizes.Clear();
        _url.Text = string.Empty;
        Sync();
    }

    void Submit()
    {
        if (_footer.PrimaryButtonDisabled) return;
        var files = _files.ToList();
        RequestSubmit?.Invoke(this, new CdsImportModalSubmitEventArgs(files));
        if (RequestSubmitCommand?.CanExecute(files) == true) RequestSubmitCommand.Execute(files);
    }

    void SyncRows()
    {
        foreach (var gone in _rowOf.Keys.Where(f => !_files.Contains(f)).ToList())
        {
            _rows.Remove(_rowOf[gone]);
            _rowOf.Remove(gone);
        }
        foreach (var file in _files)
        {
            if (_rowOf.ContainsKey(file)) continue;
            var row = new CdsFileUploaderItem { Size = CdsSize.Lg, IconDescription = InvalidIconDescription };
            row.SetBinding(CdsFileUploaderItem.NameProperty, new Binding(nameof(CdsUploadedFile.Name), source: file));
            row.SetBinding(CdsFileUploaderItem.StatusProperty, new Binding(nameof(CdsUploadedFile.Status), source: file));
            row.SetBinding(CdsFileUploaderItem.IsInvalidProperty, new Binding(nameof(CdsUploadedFile.IsInvalid), source: file));
            row.SetBinding(CdsFileUploaderItem.ErrorSubjectProperty, new Binding(nameof(CdsUploadedFile.ErrorSubject), source: file));
            row.SetBinding(CdsFileUploaderItem.ErrorBodyProperty, new Binding(nameof(CdsUploadedFile.ErrorBody), source: file));
            row.Deleted += (_, _) => RemoveFile(file);
            _rowOf[file] = row;
            _rows.Add(row);
        }
        Sync();
    }

    void Sync()
    {
        _header.Title = Title;
        SetAccessibleName(Title);
        _description.Text = Description;
        _descriptionBox.IsVisible = !string.IsNullOrEmpty(Description);
        _dropHeader.Text = FileDropHeader;
        _dropHeader.IsVisible = !string.IsNullOrEmpty(FileDropHeader);

        var total = _files.Count;
        var valid = _files.Count(f => !f.IsInvalid);
        var hasFiles = total > 0;
        _drop.Text = FileDropLabel;
        _drop.IsEnabled = !hasFiles;

        _inputHeading.Text = InputLabel;
        _inputHeading.IsVisible = !HideInputLabel && !string.IsNullOrEmpty(InputLabel);
        _url.Label = InputLabel;
        _url.Placeholder = InputPlaceholder;
        _url.IsEnabled = !hasFiles;
        _fetch.Text = InputButtonText;
        _fetch.Icon = InputButtonIcon ? CdsIconName.Add : null;
        _fetch.IsEnabled = !ImportModalModel.IsFetchDisabled(_url.Text, hasFiles);

        _status.Text = ImportModalModel.StatusText(valid, total, FileUploadLabel);
        _status.IsVisible = hasFiles;
        foreach (var row in _rowOf.Values) row.IconDescription = InvalidIconDescription;

        _footer.PrimaryButtonText = PrimaryButtonText;
        _footer.SecondaryButtonText = SecondaryButtonText;
        _footer.PrimaryButtonDisabled = ImportModalModel.IsPrimaryDisabled(total, valid);
    }

    static string? MimeOf(string fileName) => FileUploaderModel.MimeOf(fileName,
#if IOS || MACCATALYST
        CdsAppleFileTypes.MimeOfExtension
#else
        null
#endif
        );

    static IReadOnlyList<string> UniformTypes(IReadOnlyList<string> accept) =>
#if IOS || MACCATALYST
        FileUploaderModel.UniformTypes(accept, CdsAppleFileTypes.UniformTypeOfExtension, CdsAppleFileTypes.UniformTypeOfMime);
#else
        FileUploaderModel.UniformTypes(accept);
#endif
}

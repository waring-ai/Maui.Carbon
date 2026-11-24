using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Maui.Carbon;

/// <summary>
/// Lets people pick files from their device and shows each file with its upload status, through an "Add file" button or a
/// dashed drag-and-drop container (<see cref="Variant"/>). The title and description state the size and type limits;
/// <see cref="Accept"/> limits what the picker offers (other files are ignored). The control does not upload: it reports
/// <see cref="FilesAdded"/> and you drive each <see cref="CdsUploadedFile.Status"/>. Reject a bad file with an invalid row
/// rather than dropping it silently. Drag and drop is never the only way in: the container also opens the picker.
/// For a form that chooses a local file (an import, a certificate) bind <see cref="FilePath"/> (or <see cref="FilePaths"/>),
/// <see cref="IsInvalid"/> and <see cref="ErrorSubject"/> to the view model and set <see cref="AccessMode"/> to
/// <see cref="CdsFileAccessMode.Path"/> to read the original file at that path.
/// </summary>
public partial class CdsFileUploader : CdsControl
{
    readonly VerticalStackLayout _root = new() { Spacing = 0 };
    readonly CdsText _title = new() { TypeStyle = CdsTypeStyle.HeadingCompact01, Margin = new Thickness(0, 0, 0, CdsSpacing.Spacing02) };
    readonly CdsText _description = new() { Tone = CdsTextTone.Secondary, Margin = new Thickness(0, 0, 0, CdsSpacing.Spacing05) };
    readonly CdsButton _button = new() { HorizontalOptions = LayoutOptions.Start };
    readonly CdsFileDropContainer _drop = new();
    readonly VerticalStackLayout _list = new() { Spacing = CdsSpacing.Spacing03 };
    readonly Dictionary<CdsUploadedFile, CdsFileUploaderItem> _rows = [];
    readonly CdsCoalescedAction _applyChosen;
    // The uploader itself is changing Files: rows, paths and the chosen file's state settle once at the end.
    bool _changingFiles;
    bool _applyingChosen;

    /// <summary>Creates a button-variant uploader.</summary>
    public CdsFileUploader()
    {
        _applyChosen = new CdsCoalescedAction(this, ApplyChosenState);
        _drop.IsVisible = false;
        _root.Add(_title);
        _root.Add(_description);
        _root.Add(_button);
        _root.Add(_drop);
        _root.Add(_list);
        SemanticProperties.SetHeadingLevel(_title, SemanticHeadingLevel.Level2);

        _button.Clicked += async (_, _) => await PickFilesAsync();
        _drop.Activated += async (_, _) => await PickFilesAsync();
        _drop.FilesDropped += (_, grants) => AddGrants(grants);
        Files.CollectionChanged += OnFilesChanged;

        SetChrome(_root);
        UpdateLayout();
    }

    /// <summary>Raised after files were added to <see cref="Files"/> by the picker or a drop.</summary>
    public event EventHandler<CdsFilesAddedEventArgs>? FilesAdded;

    /// <summary>Raised after the user removed a file with its remove button.</summary>
    public event EventHandler<CdsFileRemovedEventArgs>? FileRemoved;

    /// <summary>
    /// The files shown as rows. Add, remove or change entries from code to drive status and errors; for MVVM bind
    /// <see cref="FilePath"/> or <see cref="FilePaths"/> instead. A file's platform access (<see cref="AccessMode"/>)
    /// ends when it leaves this collection.
    /// </summary>
    public ObservableCollection<CdsUploadedFile> Files { get; } = [];

    /// <summary>
    /// Opens the platform file picker (filtered by <see cref="Accept"/>, one or many files by <see cref="Multiple"/>) and
    /// adds the choice. With <see cref="CdsFileAccessMode.Path"/> on iOS and Mac Catalyst it opens the document picker in
    /// open-in-place mode and keeps access to the original files.
    /// </summary>
    public async Task PickFilesAsync()
    {
        if (!IsEnabled) return;
        var accept = FileUploaderModel.ParseAccept(Accept);
        try
        {
#if IOS || MACCATALYST
            if (AccessMode == CdsFileAccessMode.Path)
            {
                AddGrants(await CdsDocumentPicker.PickInPlaceAsync(UniformTypes(accept), Multiple));
                return;
            }
#endif
            var options = new PickOptions { PickerTitle = ButtonLabel };
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
            var picked = new List<FileResult>();
            if (Multiple)
            {
                if (await FilePicker.Default.PickMultipleAsync(options) is { } many) picked.AddRange(many.OfType<FileResult>());
            }
            else if (await FilePicker.Default.PickAsync(options) is { } one)
            {
                picked.Add(one);
            }
            AddResults(picked);
        }
        catch (Exception ex) when (ex is OperationCanceledException or PermissionException or NotSupportedException or FeatureNotSupportedException)
        {
            // Cancelled, denied or not available on this platform: nothing to add.
        }
    }

    /// <summary>
    /// Adds files to <see cref="Files"/> (those the <see cref="Accept"/> list does not allow are ignored; with
    /// <see cref="Multiple"/> false the first replaces the chosen file) and raises <see cref="FilesAdded"/>.
    /// </summary>
    public void AddResults(IEnumerable<FileResult> results) =>
        AddFiles(results.Select(r => new CdsUploadedFile(r.FileName, r)));

    /// <summary>Re-applies sizes, texts and variant after a property change.</summary>
    protected virtual void UpdateLayout()
    {
        var drop = Variant == CdsFileUploaderVariant.Drop;
        _title.Text = LabelTitle;
        _title.IsVisible = !string.IsNullOrEmpty(LabelTitle);
        _description.Text = LabelDescription;
        _description.IsVisible = !string.IsNullOrEmpty(LabelDescription);
        _button.IsVisible = !drop;
        _button.Kind = ButtonKind;
        _button.Size = Size;
        _drop.IsVisible = drop;
        _drop.Text = DropLabel;
        _list.Margin = new Thickness(0, drop ? CdsSpacing.Spacing03 : CdsSpacing.Spacing05, 0, 0);
        foreach (var row in _rows.Values)
        {
            row.Size = Size;
            row.IconDescription = IconDescription;
        }
        UpdateButtonText();
        RefreshVisuals();
    }

    /// <inheritdoc/>
    protected override void ApplyVisuals()
    {
        var disabled = Is(CdsInteractionState.Disabled);
        SetColor(_title, Microsoft.Maui.Controls.Label.TextColorProperty, disabled ? CdsColorKeys.TextDisabled : CdsColorKeys.TextPrimary);
        SetColor(_description, Microsoft.Maui.Controls.Label.TextColorProperty, disabled ? CdsColorKeys.TextDisabled : CdsColorKeys.TextSecondary);
        _button.IsEnabled = IsEnabled;
        _drop.IsEnabled = IsEnabled;
    }

    // The Apple picker's types: the platform's type database first, the built-in table as the fallback.
    static IReadOnlyList<string> UniformTypes(IReadOnlyList<string> accept) =>
#if IOS || MACCATALYST
        FileUploaderModel.UniformTypes(accept, CdsAppleFileTypes.UniformTypeOfExtension, CdsAppleFileTypes.UniformTypeOfMime);
#else
        FileUploaderModel.UniformTypes(accept);
#endif

    static Func<string, string?>? MimeOfExtension =>
#if IOS || MACCATALYST
        CdsAppleFileTypes.MimeOfExtension;
#else
        null;
#endif

    /// <summary>Adds dropped or picked files with the access held for them (released at once for a file that is not added).</summary>
    internal void AddGrants(IReadOnlyList<CdsFileGrant> grants) =>
        AddFiles(grants.Select(g =>
        {
            var name = Path.GetFileName(g.Path);
            var file = string.IsNullOrEmpty(name) ? CdsUploadedFile.FromPath(g.Path) : new CdsUploadedFile(name, new FileResult(g.Path));
            file.Access = g.Access;
            return file;
        }));

    void AddFiles(IEnumerable<CdsUploadedFile> candidates)
    {
        var accept = FileUploaderModel.ParseAccept(Accept);
        var added = new List<CdsUploadedFile>();
        foreach (var file in candidates)
        {
            if ((!Multiple && added.Count == 1) || !FileUploaderModel.Matches(file.Name ?? string.Empty, accept, MimeOfExtension))
            {
                file.ReleaseAccess();
                continue;
            }
            file.Status = FilenameStatus;
            added.Add(file);
        }
        if (added.Count == 0) return;
        ChangeFiles(() =>
        {
            // One file: the new choice replaces the previous row.
            if (!Multiple) Files.Clear();
            foreach (var file in added) Files.Add(file);
        });
        CdsAnnouncer.Announce(FileUploaderModel.AddedAnnouncement([.. added.Select(f => f.Name ?? string.Empty)]));
        FilesAdded?.Invoke(this, new CdsFilesAddedEventArgs(added));
        string[] paths = FileUploaderModel.PathsOf(added.Select(f => f.Path));
        if (FilesAddedCommand?.CanExecute(paths) == true) FilesAddedCommand.Execute(paths);
    }

    void ChangeFiles(Action change)
    {
        if (_changingFiles)
        {
            change();
            return;
        }
        _changingFiles = true;
        try
        {
            change();
        }
        finally
        {
            _changingFiles = false;
            OnFilesSettled();
        }
    }

    void OnFilesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (!_changingFiles) OnFilesSettled();
    }

    // Files changed (by the app or the uploader): rebuild only the rows that changed, publish the paths, give the chosen
    // file its bound state, and relabel the button.
    void OnFilesSettled()
    {
        SyncRows();
        PublishPaths();
        _applyChosen.Cancel();
        ApplyChosenState();
        UpdateButtonText();
    }

    void SyncRows()
    {
        var present = new HashSet<CdsUploadedFile>(Files);
        foreach (var file in _rows.Keys.Where(f => !present.Contains(f)).ToList()) RemoveRow(file);
        for (var i = 0; i < Files.Count; i++)
        {
            var file = Files[i];
            if (!_rows.TryGetValue(file, out var row)) row = AddRow(file);
            if (i < _list.Count && ReferenceEquals(_list[i], row)) continue;
            _list.Remove(row);
            _list.Insert(Math.Min(i, _list.Count), row);
        }
    }

    void PublishPaths()
    {
        var path = Files.Count > 0 ? Files[0].Path : null;
        if (!FileUploaderModel.SamePath(FilePath, path)) FilePath = path;
        var paths = FileUploaderModel.PathsOf(Files.Select(f => f.Path));
        if (!FileUploaderModel.SamePaths(FilePaths, paths)) FilePaths = paths;
    }

    void OnFilePathSet(string? value)
    {
        if (FileUploaderModel.SamePath(Files.Count > 0 ? Files[0].Path : null, value)) return;
        ChangeFiles(() =>
        {
            Files.Clear();
            if (!string.IsNullOrWhiteSpace(value)) Files.Add(Initial(CdsUploadedFile.FromPath(value)));
        });
    }

    void OnFilePathsSet(IReadOnlyList<string>? value)
    {
        // null is "no list yet" (a view model's initial value), so binding it never clears a pre-filled FilePath.
        if (value is null) return;
        var wanted = FileUploaderModel.NormalizePaths(value, Multiple);
        if (FileUploaderModel.SamePaths(FileUploaderModel.PathsOf(Files.Select(f => f.Path)), wanted) && Files.All(f => f.Path is not null)) return;
        // Keep the entry (status, error, access) of every path that stays.
        var existing = Files.Where(f => f.Path is not null).GroupBy(f => f.Path!, StringComparer.Ordinal).ToDictionary(g => g.Key, g => g.First(), StringComparer.Ordinal);
        var next = wanted.Select(p => existing.TryGetValue(p, out var file) ? file : Initial(CdsUploadedFile.FromPath(p))).ToList();
        ChangeFiles(() =>
        {
            Files.Clear();
            foreach (var file in next) Files.Add(file);
        });
    }

    CdsUploadedFile Initial(CdsUploadedFile file)
    {
        file.Status = FilenameStatus;
        return file;
    }

    // IsInvalid, ErrorSubject, ErrorBody and FileStatus describe the one chosen file, once the app uses them.
    void ApplyChosenState()
    {
        if (Multiple || Files.Count == 0) return;
        var usesError = IsSet(IsInvalidProperty) || IsSet(ErrorSubjectProperty) || IsSet(ErrorBodyProperty);
        if (!usesError && FileStatus is null) return;
        var file = Files[0];
        _applyingChosen = true;
        try
        {
            if (usesError)
            {
                file.IsInvalid = IsInvalid;
                file.ErrorSubject = ErrorSubject;
                file.ErrorBody = ErrorBody;
            }
            if (FileStatus is { } status) file.Status = status;
        }
        finally
        {
            _applyingChosen = false;
        }
        // One update, so an error that appears is spoken once, whole.
        if (_rows.TryGetValue(file, out var row)) Sync(row, file);
    }

    void UpdateButtonText() => _button.Text = FileUploaderModel.ButtonText(ButtonLabel, ChosenButtonLabel, Files.Count > 0);

    CdsFileUploaderItem AddRow(CdsUploadedFile file)
    {
        var row = new CdsFileUploaderItem { Size = Size, IconDescription = IconDescription };
        Sync(row, file);
        row.Deleted += (_, _) =>
        {
            ChangeFiles(() => Files.Remove(file));
            FileRemoved?.Invoke(this, new CdsFileRemovedEventArgs(file));
            if (FileRemovedCommand?.CanExecute(file.Path) == true) FileRemovedCommand.Execute(file.Path);
        };
        file.PropertyChanged += OnFilePropertyChanged;
        _rows[file] = row;
        return row;
    }

    void RemoveRow(CdsUploadedFile file)
    {
        if (!_rows.Remove(file, out var row)) return;
        file.PropertyChanged -= OnFilePropertyChanged;
        file.ReleaseAccess();
        _list.Remove(row);
    }

    void OnFilePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_applyingChosen) return;
        if (sender is CdsUploadedFile file && _rows.TryGetValue(file, out var row)) Sync(row, file);
    }

    static void Sync(CdsFileUploaderItem row, CdsUploadedFile file) => row.Batch(() =>
    {
        row.Name = file.Name;
        row.Status = file.Status;
        row.IsInvalid = file.IsInvalid;
        row.ErrorSubject = file.ErrorSubject;
        row.ErrorBody = file.ErrorBody;
    });
}

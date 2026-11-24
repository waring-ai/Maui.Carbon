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

    /// <summary>Creates a button-variant uploader.</summary>
    public CdsFileUploader()
    {
        _drop.IsVisible = false;
        _root.Add(_title);
        _root.Add(_description);
        _root.Add(_button);
        _root.Add(_drop);
        _root.Add(_list);
        SemanticProperties.SetHeadingLevel(_title, SemanticHeadingLevel.Level2);

        _button.Clicked += async (_, _) => await PickFilesAsync();
        _drop.Activated += async (_, _) => await PickFilesAsync();
        _drop.FilesDropped += (_, paths) => AddPaths(paths);
        Files.CollectionChanged += OnFilesChanged;

        SetChrome(_root);
        UpdateLayout();
    }

    /// <summary>Raised after files were added to <see cref="Files"/> by the picker or a drop.</summary>
    public event EventHandler<CdsFilesAddedEventArgs>? FilesAdded;

    /// <summary>Raised after the user removed a file with its remove button.</summary>
    public event EventHandler<CdsFileRemovedEventArgs>? FileRemoved;

    /// <summary>The files shown as rows. Add, remove or change entries from code to drive status and errors.</summary>
    public ObservableCollection<CdsUploadedFile> Files { get; } = [];

    /// <summary>Opens the platform file picker (filtered by <see cref="Accept"/>, one or many files by <see cref="Multiple"/>) and adds the choice.</summary>
    public async Task PickFilesAsync()
    {
        if (!IsEnabled) return;
        var accept = FileUploaderModel.ParseAccept(Accept);
        var options = new PickOptions { PickerTitle = ButtonLabel };
        if (accept.Count > 0)
        {
            options.FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                [DevicePlatform.WinUI] = FileUploaderModel.Extensions(accept),
                [DevicePlatform.Android] = FileUploaderModel.MimeTypes(accept),
                [DevicePlatform.iOS] = FileUploaderModel.UniformTypes(accept),
                [DevicePlatform.MacCatalyst] = FileUploaderModel.UniformTypes(accept),
            });
        }
        try
        {
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

    /// <summary>Adds files to <see cref="Files"/> (those the <see cref="Accept"/> list does not allow are ignored) and raises <see cref="FilesAdded"/>.</summary>
    public void AddResults(IEnumerable<FileResult> results)
    {
        var accept = FileUploaderModel.ParseAccept(Accept);
        var added = new List<CdsUploadedFile>();
        foreach (var result in results)
        {
            if (!FileUploaderModel.Matches(result.FileName, accept)) continue;
            if (!Multiple && added.Count == 1) break;
            added.Add(new CdsUploadedFile(result.FileName, result) { Status = FilenameStatus });
        }
        if (added.Count == 0) return;
        if (!Multiple) Files.Clear();
        foreach (var file in added) Files.Add(file);
        CdsAnnouncer.Announce(FileUploaderModel.AddedAnnouncement([.. added.Select(f => f.Name ?? string.Empty)]));
        FilesAdded?.Invoke(this, new CdsFilesAddedEventArgs(added));
    }

    /// <summary>Re-applies sizes, texts and variant after a property change.</summary>
    protected virtual void UpdateLayout()
    {
        var drop = Variant == CdsFileUploaderVariant.Drop;
        _title.Text = LabelTitle;
        _title.IsVisible = !string.IsNullOrEmpty(LabelTitle);
        _description.Text = LabelDescription;
        _description.IsVisible = !string.IsNullOrEmpty(LabelDescription);
        _button.IsVisible = !drop;
        _button.Text = ButtonLabel;
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

    void AddPaths(IReadOnlyList<string> paths) => AddResults(paths.Select(p => new FileResult(p)));

    void OnFilesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
        {
            foreach (CdsUploadedFile file in e.OldItems) RemoveRow(file);
        }
        if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            foreach (var file in _rows.Keys.ToList()) RemoveRow(file);
        }
        if (e.NewItems is not null)
        {
            foreach (CdsUploadedFile file in e.NewItems) AddRow(file);
        }
    }

    void AddRow(CdsUploadedFile file)
    {
        var row = new CdsFileUploaderItem { Size = Size, IconDescription = IconDescription };
        Sync(row, file);
        row.Deleted += (_, _) =>
        {
            Files.Remove(file);
            FileRemoved?.Invoke(this, new CdsFileRemovedEventArgs(file));
        };
        file.PropertyChanged += OnFilePropertyChanged;
        _rows[file] = row;
        _list.Add(row);
    }

    void RemoveRow(CdsUploadedFile file)
    {
        if (!_rows.Remove(file, out var row)) return;
        file.PropertyChanged -= OnFilePropertyChanged;
        _list.Remove(row);
    }

    void OnFilePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is CdsUploadedFile file && _rows.TryGetValue(file, out var row)) Sync(row, file);
    }

    static void Sync(CdsFileUploaderItem row, CdsUploadedFile file)
    {
        row.Name = file.Name;
        row.Status = file.Status;
        row.IsInvalid = file.IsInvalid;
        row.ErrorSubject = file.ErrorSubject;
        row.ErrorBody = file.ErrorBody;
    }
}

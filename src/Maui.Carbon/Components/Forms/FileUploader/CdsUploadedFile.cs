using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Maui.Carbon;

/// <summary>The upload status shown at the end of a file row.</summary>
public enum CdsFileStatus
{
    /// <summary>Uploading: a small spinner.</summary>
    Uploading,

    /// <summary>Editable: a remove button (the default for newly added files).</summary>
    Edit,

    /// <summary>Complete: a filled check.</summary>
    Complete,
}

/// <summary>
/// How a <see cref="CdsFileUploader"/> gives the app access to the files people choose. The difference is on iOS and
/// Mac Catalyst, where the system document picker grants access to a file outside the app's sandbox only through a
/// security-scoped URL.
/// </summary>
public enum CdsFileAccessMode
{
    /// <summary>
    /// Read each file through <see cref="CdsUploadedFile.OpenReadAsync"/> (or its <see cref="CdsUploadedFile.Result"/>):
    /// the default, for uploads. The picker is MAUI's: on iOS and Mac Catalyst it opens the original file in place (no
    /// copy) but holds its security-scoped access only while a stream from <c>OpenReadAsync</c> is open, so a sandboxed
    /// app may not be able to open <see cref="CdsUploadedFile.Path"/> directly. On Android the path can be a copy in the
    /// app's cache.
    /// </summary>
    Stream,

    /// <summary>
    /// Keep access to the original file at <see cref="CdsUploadedFile.Path"/>, for "choose a local file to read in place"
    /// (an import that streams a large archive, a certificate). On iOS and Mac Catalyst the uploader opens the document
    /// picker in open-in-place mode itself and holds each file's security-scoped access from the pick (or drop) until the
    /// file leaves <see cref="CdsFileUploader.Files"/> (removed, replaced by a new choice, or cleared), so
    /// <c>File.OpenRead(path)</c> and APIs that take a path work on the original, never on a copy. Open the file while it
    /// is chosen: a stream already open keeps working after access ends, a new open does not. Access does not outlive the
    /// app process (persisting it needs a security-scoped bookmark, which the app owns). Windows: the same as
    /// <see cref="Stream"/> (the picker returns the original path). Android: the same as <see cref="Stream"/>.
    /// </summary>
    Path,
}

/// <summary>Whether a <see cref="CdsFileUploader"/> picks files with a button or a dashed drop container.</summary>
public enum CdsFileUploaderVariant
{
    /// <summary>An "Add file" button.</summary>
    Button,

    /// <summary>A drag-and-drop container that also opens the picker on click.</summary>
    Drop,
}

/// <summary>
/// A file in a <see cref="CdsFileUploader"/>: its name, full path, upload status and error. Drive <see cref="Status"/>
/// yourself. Create one for a known file with <see cref="FromPath"/>.
/// </summary>
public sealed class CdsUploadedFile : INotifyPropertyChanged
{
    CdsFileStatus _status = CdsFileStatus.Edit;
    bool _isInvalid;
    string? _errorSubject;
    string? _errorBody;
    string? _name;

    /// <summary>Creates a file entry; its <see cref="Path"/> is the result's full path.</summary>
    public CdsUploadedFile(string name, FileResult? result = null)
    {
        _name = name;
        Result = result;
        Path = result?.FullPath is { Length: > 0 } path ? path : null;
    }

    CdsUploadedFile(string name, string path, FileResult? result)
    {
        _name = name;
        Result = result;
        Path = path;
    }

    /// <summary>
    /// Creates the entry of a file at a full path (a default the app pre-fills, a path it remembered). The name is the
    /// path's file name. The file does not have to exist: validate it and mark the entry invalid if it is missing.
    /// </summary>
    public static CdsUploadedFile FromPath(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        var name = System.IO.Path.GetFileName(path);
        return new CdsUploadedFile(string.IsNullOrEmpty(name) ? path : name, path, null);
    }

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>A unique id for the entry.</summary>
    public string Id { get; } = Guid.NewGuid().ToString("N");

    /// <summary>The platform file, when the entry came from the picker or a drop (<c>null</c> for <see cref="FromPath"/>).</summary>
    public FileResult? Result { get; }

    /// <summary>
    /// The file's full path, or <c>null</c> for an entry created from a name only. With
    /// <see cref="CdsFileAccessMode.Path"/> it is the original file, readable while the entry is in the uploader.
    /// </summary>
    public string? Path { get; }

    /// <summary>
    /// Opens the file for reading: through <see cref="Result"/> (which holds the platform's access for the stream's
    /// lifetime) or else from <see cref="Path"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">The entry has neither a result nor a path.</exception>
    public Task<Stream> OpenReadAsync()
    {
        if (Result is not null) return Result.OpenReadAsync();
        if (Path is null) throw new InvalidOperationException($"The file entry '{Name}' has no path to read.");
        return Task.FromResult<Stream>(File.OpenRead(Path));
    }

    /// <summary>The platform access held for the file while it is chosen (a security-scoped URL on iOS and Mac Catalyst).</summary>
    internal IDisposable? Access { get; set; }

    /// <summary>Ends the access held for the file, if any.</summary>
    internal void ReleaseAccess()
    {
        Access?.Dispose();
        Access = null;
    }

    /// <summary>The file name shown in the row.</summary>
    public string? Name
    {
        get => _name;
        set => Set(ref _name, value);
    }

    /// <summary>The status icon: spinner, remove button or check.</summary>
    public CdsFileStatus Status
    {
        get => _status;
        set => Set(ref _status, value);
    }

    /// <summary>Draws the row with a 2px error outline and its message. Always set <see cref="ErrorSubject"/> with it.</summary>
    public bool IsInvalid
    {
        get => _isInvalid;
        set => Set(ref _isInvalid, value);
    }

    /// <summary>The error title: what went wrong ("File is too large").</summary>
    public string? ErrorSubject
    {
        get => _errorSubject;
        set => Set(ref _errorSubject, value);
    }

    /// <summary>How to fix it ("Choose a file under 10 MB").</summary>
    public string? ErrorBody
    {
        get => _errorBody;
        set => Set(ref _errorBody, value);
    }

    void Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

/// <summary>Event data of <see cref="CdsFileUploader.FilesAdded"/>.</summary>
public sealed class CdsFilesAddedEventArgs : EventArgs
{
    /// <summary>Creates the event data.</summary>
    public CdsFilesAddedEventArgs(IReadOnlyList<CdsUploadedFile> addedFiles) => AddedFiles = addedFiles;

    /// <summary>The files that were just added to <see cref="CdsFileUploader.Files"/>.</summary>
    public IReadOnlyList<CdsUploadedFile> AddedFiles { get; }
}

/// <summary>Event data of <see cref="CdsFileUploader.FileRemoved"/> and <see cref="CdsFileUploaderItem.Deleted"/>.</summary>
public sealed class CdsFileRemovedEventArgs : EventArgs
{
    /// <summary>Creates the event data.</summary>
    public CdsFileRemovedEventArgs(CdsUploadedFile? file) => File = file;

    /// <summary>The removed file; <c>null</c> for a standalone <see cref="CdsFileUploaderItem"/>.</summary>
    public CdsUploadedFile? File { get; }
}

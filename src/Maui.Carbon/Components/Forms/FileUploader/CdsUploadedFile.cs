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

/// <summary>Whether a <see cref="CdsFileUploader"/> picks files with a button or a dashed drop container.</summary>
public enum CdsFileUploaderVariant
{
    /// <summary>An "Add file" button.</summary>
    Button,

    /// <summary>A drag-and-drop container that also opens the picker on click.</summary>
    Drop,
}

/// <summary>A file in a <see cref="CdsFileUploader"/>: its name, upload status and error. Drive <see cref="Status"/> yourself.</summary>
public sealed class CdsUploadedFile : INotifyPropertyChanged
{
    CdsFileStatus _status = CdsFileStatus.Edit;
    bool _isInvalid;
    string? _errorSubject;
    string? _errorBody;
    string? _name;

    /// <summary>Creates a file entry.</summary>
    public CdsUploadedFile(string name, FileResult? result = null)
    {
        _name = name;
        Result = result;
    }

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>A unique id for the entry.</summary>
    public string Id { get; } = Guid.NewGuid().ToString("N");

    /// <summary>The platform file, when the entry came from the picker or a drop.</summary>
    public FileResult? Result { get; }

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

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Maui.Carbon.Gallery.Pages.Forms.Data;

/// <summary>
/// Sample view model for "Choose a local file": it owns the path of a domain-list export that an import reads in place,
/// pre-fills a default, and reports a missing or unreadable file as the uploader's invalid row.
/// </summary>
public sealed class GLocalFileForm : INotifyPropertyChanged
{
    string? _archivePath;
    bool _isInvalid;
    string? _errorSubject;
    string? _errorBody;
    string _summary = "No file chosen";

    public GLocalFileForm()
    {
        UseDefaultCommand = new Command(() => ArchivePath = DefaultPath);
        ArchivePath = DefaultPath;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>The default export location the form pre-fills.</summary>
    public static string DefaultPath =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads", "domains.zip");

    /// <summary>The chosen export, two-way with the uploader's FilePath.</summary>
    public string? ArchivePath
    {
        get => _archivePath;
        set
        {
            if (Set(ref _archivePath, value)) Validate();
        }
    }

    public bool IsInvalid
    {
        get => _isInvalid;
        private set => Set(ref _isInvalid, value);
    }

    public string? ErrorSubject
    {
        get => _errorSubject;
        private set => Set(ref _errorSubject, value);
    }

    public string? ErrorBody
    {
        get => _errorBody;
        private set => Set(ref _errorBody, value);
    }

    public string Summary
    {
        get => _summary;
        private set => Set(ref _summary, value);
    }

    public ICommand UseDefaultCommand { get; }

    void Validate()
    {
        if (string.IsNullOrWhiteSpace(ArchivePath))
        {
            ShowError(null, null);
            Summary = "No file chosen";
            return;
        }
        var file = new FileInfo(ArchivePath);
        if (!file.Exists)
        {
            ShowError("File not found", "Choose a .zip export that exists, or check the path.");
            Summary = $"Nothing to import from {file.Name}";
            return;
        }
        try
        {
            // Opening is the whole check: the import streams the archive in place, it is never unpacked or copied.
            using var stream = File.OpenRead(file.FullName);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            ShowError("File can't be read", "Check that you have permission to read it, then choose it again.");
            Summary = $"Nothing to import from {file.Name}";
            return;
        }
        ShowError(null, null);
        Summary = $"Ready to import {file.Name} ({file.Length / (1024.0 * 1024.0):0.#} MB), read in place";
    }

    // Subject and body first, then the flag, so an error that appears is spoken once, whole.
    void ShowError(string? subject, string? body)
    {
        if (subject is null) IsInvalid = false;
        ErrorSubject = subject;
        ErrorBody = body;
        if (subject is not null) IsInvalid = true;
    }

    bool Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        return true;
    }
}

namespace Maui.Carbon;

/// <summary>Why an imported file is rejected.</summary>
internal enum ImportFileError
{
    /// <summary>The file is accepted.</summary>
    None,

    /// <summary>The URL could not be fetched.</summary>
    Fetch,

    /// <summary>The file type is not in the accept list.</summary>
    FileType,

    /// <summary>The file is larger than the size limit.</summary>
    FileSize,
}

/// <summary>The import modal's validation and status rules (pure).</summary>
internal static class ImportModalModel
{
    /// <summary>
    /// True when an accept list is set and neither the file's MIME type nor its ".ext" is in it. Entries compare without
    /// case; an empty list accepts every file.
    /// </summary>
    public static bool IsInvalidFileType(string? fileName, string? mimeType, IReadOnlyList<string> accept)
    {
        if (accept.Count == 0) return false;
        var name = fileName ?? string.Empty;
        var dot = name.LastIndexOf('.');
        var extension = dot >= 0 ? name[dot..] : "." + name;
        return !accept.Any(a => string.Equals(a, extension, StringComparison.OrdinalIgnoreCase)
            || (!string.IsNullOrEmpty(mimeType) && string.Equals(a, mimeType, StringComparison.OrdinalIgnoreCase)));
    }

    /// <summary>The first reason a file is rejected, in Carbon's order: a fetch error, then the type, then the size.</summary>
    public static ImportFileError Classify(bool fetchError, bool invalidFileType, long? fileSize, long? maxFileSize)
    {
        if (fetchError) return ImportFileError.Fetch;
        if (invalidFileType) return ImportFileError.FileType;
        if (maxFileSize is > 0 && (fileSize ?? 0) > maxFileSize) return ImportFileError.FileSize;
        return ImportFileError.None;
    }

    /// <summary>The error header and body of a rejected file: the specific texts, else the defaults.</summary>
    public static (string? Subject, string? Body) Messages(ImportFileError error, CdsImportModal texts) => error switch
    {
        ImportFileError.Fetch => (texts.FetchErrorHeader ?? texts.DefaultErrorHeader, texts.FetchErrorBody ?? texts.DefaultErrorBody),
        ImportFileError.FileType => (texts.InvalidFileTypeErrorHeader ?? texts.DefaultErrorHeader, texts.InvalidFileTypeErrorBody ?? texts.DefaultErrorBody),
        ImportFileError.FileSize => (texts.MaxFileSizeErrorHeader ?? texts.DefaultErrorHeader, texts.MaxFileSizeErrorBody ?? texts.DefaultErrorBody),
        _ => (null, null),
    };

    /// <summary>The file name of a URL: what follows the last slash, without the query; "download" when that is empty.</summary>
    public static string FileNameFromUrl(string? url)
    {
        var value = url ?? string.Empty;
        var name = value[(value.LastIndexOf('/') + 1)..].Split('?')[0].Split('#')[0];
        return string.IsNullOrWhiteSpace(name) ? "download" : name;
    }

    /// <summary>The line above the rows: "1 / 2 files uploaded" (valid / all, then the label).</summary>
    public static string StatusText(int valid, int total, string? label) =>
        string.IsNullOrEmpty(label) ? $"{valid} / {total}" : $"{valid} / {total} {label}";

    /// <summary>Import is disabled until there is at least one valid file.</summary>
    public static bool IsPrimaryDisabled(int total, int valid) => total == 0 || valid == 0;

    /// <summary>The fetch button is disabled without a URL or once a file is chosen.</summary>
    public static bool IsFetchDisabled(string? url, bool hasFiles) => string.IsNullOrWhiteSpace(url) || hasFiles;
}

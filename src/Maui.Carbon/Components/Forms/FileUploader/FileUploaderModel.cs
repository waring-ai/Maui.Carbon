namespace Maui.Carbon;

/// <summary>Pure accept-list and label logic of the file uploader.</summary>
internal static class FileUploaderModel
{
    static readonly Dictionary<string, string> s_mime = new(StringComparer.OrdinalIgnoreCase)
    {
        [".csv"] = "text/csv",
        [".txt"] = "text/plain",
        [".log"] = "text/plain",
        [".md"] = "text/markdown",
        [".json"] = "application/json",
        [".xml"] = "application/xml",
        [".yaml"] = "application/x-yaml",
        [".yml"] = "application/x-yaml",
        [".pdf"] = "application/pdf",
        [".zip"] = "application/zip",
        [".png"] = "image/png",
        [".jpg"] = "image/jpeg",
        [".jpeg"] = "image/jpeg",
        [".gif"] = "image/gif",
        [".svg"] = "image/svg+xml",
        [".xlsx"] = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        [".docx"] = "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        [".pem"] = "application/x-pem-file",
    };

    static readonly Dictionary<string, string> s_uti = new(StringComparer.OrdinalIgnoreCase)
    {
        [".csv"] = "public.comma-separated-values-text",
        [".txt"] = "public.plain-text",
        [".log"] = "public.plain-text",
        [".md"] = "net.daringfireball.markdown",
        [".json"] = "public.json",
        [".xml"] = "public.xml",
        [".yaml"] = "public.yaml",
        [".yml"] = "public.yaml",
        [".pdf"] = "com.adobe.pdf",
        [".zip"] = "public.zip-archive",
        [".png"] = "public.png",
        [".jpg"] = "public.jpeg",
        [".jpeg"] = "public.jpeg",
        [".gif"] = "com.compuserve.gif",
        [".svg"] = "public.svg-image",
        [".xlsx"] = "org.openxmlformats.spreadsheetml.sheet",
        [".docx"] = "org.openxmlformats.wordprocessingml.document",
    };

    // The broad types of MIME wildcards ("image/*"), for the iOS and Mac Catalyst picker.
    static readonly Dictionary<string, string> s_wildcardUti = new(StringComparer.OrdinalIgnoreCase)
    {
        ["image/"] = "public.image",
        ["text/"] = "public.text",
        ["video/"] = "public.movie",
        ["audio/"] = "public.audio",
        ["font/"] = "public.font",
        ["application/"] = "public.data",
    };

    /// <summary>The type the iOS and Mac Catalyst picker offers when an accept list allows any file.</summary>
    public const string AnyUniformType = "public.item";

    /// <summary>Splits an accept list such as ".csv, image/*" into trimmed entries.</summary>
    public static IReadOnlyList<string> ParseAccept(string? accept)
    {
        if (string.IsNullOrWhiteSpace(accept)) return [];
        return accept.Split([',', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    /// <summary>
    /// The MIME type of a file name by its extension, or <c>null</c> when unknown: from <paramref name="resolve"/>
    /// (the platform's type database, given ".ext") first, then the built-in table.
    /// </summary>
    public static string? MimeOf(string fileName, Func<string, string?>? resolve = null)
    {
        var extension = Path.GetExtension(fileName);
        if (string.IsNullOrEmpty(extension)) return null;
        return resolve?.Invoke(extension) ?? (s_mime.TryGetValue(extension, out var mime) ? mime : null);
    }

    static bool IsAny(string entry) => entry is "*" or "*/*";

    /// <summary>
    /// True when the file is allowed by the accept list: extensions (".csv", ".tar.gz") match the name, MIME types
    /// ("text/csv", "image/*") match the type by extension (<paramref name="mimeOf"/> resolves an extension at
    /// runtime, before the built-in table), "*/*" allows anything. An empty list allows everything.
    /// </summary>
    public static bool Matches(string fileName, IReadOnlyList<string> accept, Func<string, string?>? mimeOf = null)
    {
        if (accept.Count == 0) return true;
        var mime = MimeOf(fileName, mimeOf);
        foreach (var entry in accept)
        {
            if (IsAny(entry)) return true;
            if (entry.StartsWith('.'))
            {
                if (fileName.EndsWith(entry, StringComparison.OrdinalIgnoreCase)) return true;
            }
            else if (entry.EndsWith("/*", StringComparison.Ordinal))
            {
                if (mime is not null && mime.StartsWith(entry[..^1], StringComparison.OrdinalIgnoreCase)) return true;
            }
            else if (entry.Contains('/'))
            {
                if (string.Equals(mime, entry, StringComparison.OrdinalIgnoreCase)) return true;
            }
        }
        return false;
    }

    /// <summary>The extensions of an accept list, for the Windows file picker.</summary>
    public static IReadOnlyList<string> Extensions(IReadOnlyList<string> accept) =>
        [.. accept.Where(a => a.StartsWith('.'))];

    /// <summary>The MIME types of an accept list, for the Android file picker (extensions are mapped; unknown ones become any type).</summary>
    public static IReadOnlyList<string> MimeTypes(IReadOnlyList<string> accept)
    {
        var result = new List<string>();
        foreach (var entry in accept)
        {
            var mime = entry.StartsWith('.') ? (s_mime.TryGetValue(entry, out var m) ? m : "*/*") : entry;
            if (!result.Contains(mime)) result.Add(mime);
        }
        return result;
    }

    /// <summary>
    /// The types of an accept list for the iOS and Mac Catalyst file picker. An extension maps to its uniform type
    /// identifier through <paramref name="utiOfExtension"/> (the platform's type database, given ".ext"; it
    /// returns <c>null</c> for a type the system does not declare), then the built-in table; an extension neither knows
    /// stays as ".ext", which the picker resolves to the extension's dynamic type (so ".pem" offers .pem files rather
    /// than nothing or everything). A multi-part extension (".tar.gz") is offered by its last part (".gz"); the accept
    /// check after the pick still requires the whole of it. A MIME type maps through <paramref name="utiOfMime"/>,
    /// then the table; a wildcard ("image/*") to its broad type ("public.image"); "*/*" to any item. When nothing maps,
    /// the picker offers any item and the accept check filters the choice.
    /// </summary>
    public static IReadOnlyList<string> UniformTypes(
        IReadOnlyList<string> accept, Func<string, string?>? utiOfExtension = null, Func<string, string?>? utiOfMime = null)
    {
        var result = new List<string>();
        foreach (var entry in accept)
        {
            string? uti;
            if (IsAny(entry))
            {
                uti = AnyUniformType;
            }
            else if (entry.StartsWith('.'))
            {
                var extension = Path.GetExtension("f" + entry);
                if (string.IsNullOrEmpty(extension)) continue;
                uti = utiOfExtension?.Invoke(extension) ?? (s_uti.TryGetValue(extension, out var known) ? known : extension);
            }
            else if (entry.EndsWith("/*", StringComparison.Ordinal))
            {
                uti = s_wildcardUti.TryGetValue(entry[..^1], out var broad) ? broad : null;
            }
            else
            {
                uti = utiOfMime?.Invoke(entry)
                    ?? (s_mime.FirstOrDefault(p => string.Equals(p.Value, entry, StringComparison.OrdinalIgnoreCase)).Key is { } ext
                        && s_uti.TryGetValue(ext, out var u) ? u : null);
            }
            if (uti is not null && !result.Contains(uti, StringComparer.OrdinalIgnoreCase)) result.Add(uti);
        }
        if (accept.Count > 0 && result.Count == 0) result.Add(AnyUniformType);
        return result;
    }

    /// <summary>Whether two paths name the same choice: equal, or both empty (<c>null</c> and "" both mean no file).</summary>
    public static bool SamePath(string? a, string? b) =>
        string.IsNullOrEmpty(a) ? string.IsNullOrEmpty(b) : string.Equals(a, b, StringComparison.Ordinal);

    /// <summary>Whether two path lists hold the same paths in the same order (<c>null</c> is an empty list).</summary>
    public static bool SamePaths(IReadOnlyList<string>? a, IReadOnlyList<string>? b)
    {
        var left = a ?? [];
        var right = b ?? [];
        return left.Count == right.Count && left.SequenceEqual(right, StringComparer.Ordinal);
    }

    /// <summary>The paths of the files that have one, in order.</summary>
    public static string[] PathsOf(IEnumerable<string?> paths) => [.. paths.Where(p => !string.IsNullOrEmpty(p)).Cast<string>()];

    /// <summary>
    /// The paths a view model asked for, as the uploader keeps them: blanks and repeats dropped, and only the first
    /// when the uploader keeps one file.
    /// </summary>
    public static string[] NormalizePaths(IEnumerable<string?>? paths, bool multiple)
    {
        var result = PathsOf(paths ?? []).Distinct(StringComparer.Ordinal).ToArray();
        return multiple || result.Length <= 1 ? result : [result[0]];
    }

    /// <summary>
    /// The button text: <paramref name="chosenLabel"/> ("Change file") once a file is chosen, when the app gives one,
    /// else the button label. Carbon's FileUploaderButton changes its label after a pick; the FileUploader keeps it.
    /// </summary>
    public static string? ButtonText(string? buttonLabel, string? chosenLabel, bool hasFiles) =>
        hasFiles && !string.IsNullOrEmpty(chosenLabel) ? chosenLabel : buttonLabel;

    /// <summary>Maximum width of a file row: 20rem.</summary>
    public const double MaxRowWidth = 320;

    /// <summary>The height of a file row: sm 32, md 40, lg 48 (larger sizes use 48).</summary>
    public static double RowHeight(CdsSize size) => size switch
    {
        CdsSize.Xs or CdsSize.Sm => CdsSizing.Small,
        CdsSize.Md => CdsSizing.Medium,
        _ => CdsSizing.Large,
    };

    /// <summary>Padding above and below an invalid row's content: sm 4, md 8, lg 12.</summary>
    public static double InvalidRowPadding(CdsSize size) => size switch
    {
        CdsSize.Xs or CdsSize.Sm => 4,
        CdsSize.Md => 8,
        _ => 12,
    };

    /// <summary>The gap between an invalid row's divider and its message: sm 7, md 11, lg 16.</summary>
    public static double InvalidMessageGap(CdsSize size) => size switch
    {
        CdsSize.Xs or CdsSize.Sm => 7,
        CdsSize.Md => 11,
        _ => 16,
    };

    /// <summary>The accessible name of a remove button: "Remove file - name".</summary>
    public static string RemoveLabel(string? iconDescription, string? fileName) =>
        string.IsNullOrEmpty(fileName) ? iconDescription ?? "Remove file" : $"{iconDescription ?? "Remove file"} - {fileName}";

    /// <summary>
    /// The error text of an invalid row, "subject. body" (either part may be missing); <c>null</c> when the row is not
    /// invalid or has no text. It is the row's alert and the remove button's description.
    /// </summary>
    public static string? ErrorText(bool isInvalid, string? subject, string? body)
    {
        if (!isInvalid) return null;
        var parts = new[] { subject, body }.Where(s => !string.IsNullOrEmpty(s)).ToArray();
        return parts.Length == 0 ? null : string.Join(". ", parts);
    }

    /// <summary>The alert spoken when a row becomes invalid: "name: subject. body" (Carbon: the message is <c>role="alert"</c>).</summary>
    public static string? InvalidAnnouncement(string? fileName, string? errorText) =>
        string.IsNullOrEmpty(errorText) ? null : string.IsNullOrEmpty(fileName) ? errorText : $"{fileName}: {errorText}";

    /// <summary>The announcement when a file's upload finishes: "report.csv uploaded".</summary>
    public static string CompletedAnnouncement(string? fileName) =>
        string.IsNullOrEmpty(fileName) ? "Upload complete" : $"{fileName} uploaded";

    /// <summary>The announcement after adding files: "report.csv added" or "3 files added".</summary>
    public static string AddedAnnouncement(IReadOnlyList<string> names) => names.Count switch
    {
        0 => string.Empty,
        1 => $"{names[0]} added",
        _ => $"{names.Count} files added",
    };
}

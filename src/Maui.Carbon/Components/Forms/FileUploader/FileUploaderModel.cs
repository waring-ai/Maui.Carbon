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

    /// <summary>Splits an accept list such as ".csv, image/*" into trimmed entries.</summary>
    public static IReadOnlyList<string> ParseAccept(string? accept)
    {
        if (string.IsNullOrWhiteSpace(accept)) return [];
        return accept.Split([',', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    /// <summary>The MIME type of a file name by its extension, or <c>null</c> when unknown.</summary>
    public static string? MimeOf(string fileName) =>
        s_mime.TryGetValue(Path.GetExtension(fileName), out var mime) ? mime : null;

    /// <summary>
    /// True when the file is allowed by the accept list: extensions (".csv") match the name, MIME types ("text/csv",
    /// "image/*") match the type by extension. An empty list allows everything.
    /// </summary>
    public static bool Matches(string fileName, IReadOnlyList<string> accept)
    {
        if (accept.Count == 0) return true;
        var mime = MimeOf(fileName);
        foreach (var entry in accept)
        {
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

    /// <summary>The uniform type identifiers of an accept list, for the iOS and Mac Catalyst file picker.</summary>
    public static IReadOnlyList<string> UniformTypes(IReadOnlyList<string> accept)
    {
        var result = new List<string>();
        foreach (var entry in accept)
        {
            string? uti = null;
            if (entry.StartsWith('.')) s_uti.TryGetValue(entry, out uti);
            else if (entry.EndsWith("/*", StringComparison.Ordinal))
                uti = entry.StartsWith("image/", StringComparison.Ordinal) ? "public.image"
                    : entry.StartsWith("text/", StringComparison.Ordinal) ? "public.text"
                    : entry.StartsWith("video/", StringComparison.Ordinal) ? "public.movie"
                    : entry.StartsWith("audio/", StringComparison.Ordinal) ? "public.audio" : null;
            else
                uti = s_mime.FirstOrDefault(p => string.Equals(p.Value, entry, StringComparison.OrdinalIgnoreCase)).Key is { } ext && s_uti.TryGetValue(ext, out var u) ? u : null;
            if (uti is not null && !result.Contains(uti)) result.Add(uti);
        }
        return result;
    }

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

    /// <summary>The announcement after adding files: "report.csv added" or "3 files added".</summary>
    public static string AddedAnnouncement(IReadOnlyList<string> names) => names.Count switch
    {
        0 => string.Empty,
        1 => $"{names[0]} added",
        _ => $"{names.Count} files added",
    };
}

namespace Maui.Carbon;

/// <summary>The export modal's validation and naming rules (pure).</summary>
internal static class ExportModalModel
{
    /// <summary>
    /// True when extension validation applies (the name was edited and left, and valid extensions are set) and the name has
    /// no extension or one not in the list. Extensions are compared without case ("PDF" passes for "pdf").
    /// </summary>
    public static bool HasInvalidExtension(string? name, IReadOnlyCollection<string>? validExtensions, bool dirty)
    {
        if (!dirty || validExtensions is null || validExtensions.Count == 0) return false;
        name ??= string.Empty;
        var dot = name.LastIndexOf('.');
        if (dot < 0) return true;
        var extension = name[(dot + 1)..];
        return !validExtensions.Any(v => string.Equals(v?.TrimStart('.'), extension, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>The name handed to the submit: the preset name plus the chosen format's lowercase extension, else the typed name.</summary>
    public static string SubmitName(string? filename, string? name, string? extension) =>
        !string.IsNullOrEmpty(extension) ? $"{filename}.{extension.ToLowerInvariant()}" : name ?? string.Empty;

    /// <summary>The primary button is disabled while loading, with an empty name, or with an invalid extension.</summary>
    public static bool IsPrimaryDisabled(bool loading, string? name, bool invalidExtension) =>
        loading || string.IsNullOrEmpty(name) || invalidExtension;

    /// <summary>After a submit (loading, error or success) only the status message shows: no fields, no footer.</summary>
    public static bool IsSubmitted(bool loading, bool error, bool successful) => loading || error || successful;

    /// <summary>The format chosen when the modal opens: the first one, or none.</summary>
    public static string? DefaultExtension(IEnumerable<CdsExportExtension>? extensions) =>
        extensions?.FirstOrDefault()?.Extension is { Length: > 0 } first ? first : null;

    /// <summary>The radio button label of a format: "YAML (best for IBM managed cloud)".</summary>
    public static string ExtensionLabel(CdsExportExtension extension) =>
        string.IsNullOrEmpty(extension.Description) ? extension.Extension ?? string.Empty : $"{extension.Extension} ({extension.Description})";
}

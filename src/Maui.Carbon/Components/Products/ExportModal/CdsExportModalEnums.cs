namespace Maui.Carbon;

/// <summary>The kind of file name input of a <see cref="CdsExportModal"/> (<c>inputType</c>).</summary>
public enum CdsExportModalInputType
{
    /// <summary>A plain text input (the default).</summary>
    Text,

    /// <summary>A password input with a show / hide toggle, for names that should not show on screen.</summary>
    Password,
}

/// <summary>
/// A file format a <see cref="CdsExportModal"/> offers as a radio button (<c>preformattedExtensions</c>): the
/// extension and a short description, shown as "YAML (best for IBM managed cloud)".
/// </summary>
public sealed class CdsExportExtension
{
    /// <summary>Creates an empty entry (for XAML).</summary>
    public CdsExportExtension()
    {
    }

    /// <summary>Creates an entry.</summary>
    public CdsExportExtension(string extension, string? description = null)
    {
        Extension = extension;
        Description = description;
    }

    /// <summary>The extension without its dot ("YAML"); the exported name ends in its lowercase form.</summary>
    public string? Extension { get; set; }

    /// <summary>When to choose it ("best for IBM managed cloud").</summary>
    public string? Description { get; set; }
}

/// <summary>Event data of <see cref="CdsExportModal.RequestSubmit"/>: the name to export under.</summary>
public sealed class CdsExportModalSubmitEventArgs : EventArgs
{
    /// <summary>Creates the event data.</summary>
    public CdsExportModalSubmitEventArgs(string fileName) => FileName = fileName;

    /// <summary>The file name: the typed name, or the preset name with the chosen format's lowercase extension.</summary>
    public string FileName { get; }
}

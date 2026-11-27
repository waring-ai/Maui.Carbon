namespace Maui.Carbon;

/// <summary>
/// One step of a multi-step <see cref="CdsAPIKeyModal"/> (<c>customSteps</c>): its title, its content (the fields that
/// gather what the key needs: name, permissions, resources) and whether it is valid. The primary button reads "Next"
/// until the last step and stays disabled while <see cref="IsValid"/> is false.
/// </summary>
[ContentProperty(nameof(Content))]
public class CdsAPIKeyModalStep : BindableObject
{
    /// <summary>Bindable property for <see cref="Title"/>.</summary>
    public static readonly BindableProperty TitleProperty = BindableProperty.Create(
        nameof(Title), typeof(string), typeof(CdsAPIKeyModalStep), null);

    /// <summary>Bindable property for <see cref="Content"/>.</summary>
    public static readonly BindableProperty ContentProperty = BindableProperty.Create(
        nameof(Content), typeof(View), typeof(CdsAPIKeyModalStep), null);

    /// <summary>Bindable property for <see cref="IsValid"/>.</summary>
    public static readonly BindableProperty IsValidProperty = BindableProperty.Create(
        nameof(IsValid), typeof(bool?), typeof(CdsAPIKeyModalStep), null);

    /// <summary>The modal title while this step shows (<c>title</c>).</summary>
    public string? Title
    {
        get => (string?)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>The step's fields (<c>content</c>).</summary>
    public View? Content
    {
        get => (View?)GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    /// <summary>
    /// Whether the step's input is complete (<c>valid</c>): false keeps the primary button disabled. Leave it unset
    /// (<c>null</c>) for a step that needs no validation.
    /// </summary>
    public bool? IsValid
    {
        get => (bool?)GetValue(IsValidProperty);
        set => SetValue(IsValidProperty, value);
    }
}

/// <summary>Event data of the <see cref="CdsAPIKeyModal"/> requests: the key name to generate or save, or the key to copy.</summary>
public sealed class CdsAPIKeyModalEventArgs : EventArgs
{
    /// <summary>Creates the event data.</summary>
    public CdsAPIKeyModalEventArgs(string value) => Value = value;

    /// <summary>The key name (<c>RequestGenerate</c>, <c>RequestEdit</c>) or the API key (<c>CopyRequested</c>).</summary>
    public string Value { get; }
}

/// <summary>The file format of a downloaded API key (<c>downloadFileType</c>).</summary>
public enum CdsAPIKeyFileType
{
    /// <summary>A JSON file, <c>{"apiKey":"..."}</c> (the default).</summary>
    Json,

    /// <summary>A plain text file holding only the key.</summary>
    Txt,
}

/// <summary>
/// Event data of <see cref="CdsAPIKeyDownloader.SaveRequested"/>: the file about to be saved. Set <see cref="Handled"/>
/// after saving it yourself (with a file saver) to skip the share sheet.
/// </summary>
public sealed class CdsAPIKeySaveEventArgs : EventArgs
{
    /// <summary>Creates the event data.</summary>
    public CdsAPIKeySaveEventArgs(string fileName, string content, string contentType)
    {
        FileName = fileName;
        Content = content;
        ContentType = contentType;
    }

    /// <summary>The file name with its extension: "apikey.json".</summary>
    public string FileName { get; }

    /// <summary>The file content.</summary>
    public string Content { get; }

    /// <summary>The MIME type: <c>application/json</c> or <c>text/plain</c>.</summary>
    public string ContentType { get; }

    /// <summary>Set it when you saved the file: the downloader then does not open the share sheet.</summary>
    public bool Handled { get; set; }
}

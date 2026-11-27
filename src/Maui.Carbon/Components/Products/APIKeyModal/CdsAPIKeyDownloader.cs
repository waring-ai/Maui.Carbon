namespace Maui.Carbon;

/// <summary>
/// The download line of a <see cref="CdsAPIKeyModal"/> (Carbon for IBM Products <c>APIKeyDownloader</c>): a sentence and
/// a link that saves the key as a file ("apikey.json" or ".txt"). Activating the link raises <see cref="SaveRequested"/>;
/// handle it to save with your own file saver, or let the downloader write the file and open the platform share sheet,
/// where people save it to Files or send it on. Usable on its own wherever a secret is shown once.
/// </summary>
public class CdsAPIKeyDownloader : CdsControl
{
    /// <summary>Bindable property for <see cref="ApiKey"/>.</summary>
    public static readonly BindableProperty ApiKeyProperty = BindableProperty.Create(
        nameof(ApiKey), typeof(string), typeof(CdsAPIKeyDownloader), null);

    /// <summary>Bindable property for <see cref="BodyText"/>.</summary>
    public static readonly BindableProperty BodyTextProperty = BindableProperty.Create(
        nameof(BodyText), typeof(string), typeof(CdsAPIKeyDownloader), null,
        propertyChanged: static (b, _, _) => ((CdsAPIKeyDownloader)b).Sync());

    /// <summary>Bindable property for <see cref="FileName"/>.</summary>
    public static readonly BindableProperty FileNameProperty = BindableProperty.Create(
        nameof(FileName), typeof(string), typeof(CdsAPIKeyDownloader), "apikey");

    /// <summary>Bindable property for <see cref="FileType"/>.</summary>
    public static readonly BindableProperty FileTypeProperty = BindableProperty.Create(
        nameof(FileType), typeof(CdsAPIKeyFileType), typeof(CdsAPIKeyDownloader), CdsAPIKeyFileType.Json);

    /// <summary>Bindable property for <see cref="LinkText"/>.</summary>
    public static readonly BindableProperty LinkTextProperty = BindableProperty.Create(
        nameof(LinkText), typeof(string), typeof(CdsAPIKeyDownloader), null,
        propertyChanged: static (b, _, _) => ((CdsAPIKeyDownloader)b).Sync());

    /// <summary>Bindable property for <see cref="LinkLabel"/>.</summary>
    public static readonly BindableProperty LinkLabelProperty = BindableProperty.Create(
        nameof(LinkLabel), typeof(string), typeof(CdsAPIKeyDownloader), null,
        propertyChanged: static (b, _, _) => ((CdsAPIKeyDownloader)b).Sync());

    readonly CdsParagraph _paragraph = new() { TypeStyle = CdsTypeStyle.Label01 };
    readonly Span _body = new();
    readonly CdsLinkSpan _link = new();

    /// <summary>Creates an empty downloader.</summary>
    public CdsAPIKeyDownloader()
    {
        _link.Clicked += async (_, _) => await DownloadAsync();
        _paragraph.FormattedText = new FormattedString { Spans = { _body, _link } };
        SetChrome(_paragraph);
        Sync();
    }

    /// <summary>
    /// Raised with the file when the link is activated. Save it yourself (for example with a file saver) and set
    /// <see cref="CdsAPIKeySaveEventArgs.Handled"/> to skip the share sheet.
    /// </summary>
    public event EventHandler<CdsAPIKeySaveEventArgs>? SaveRequested;

    /// <summary>Raised after the file was handed to your handler or to the share sheet.</summary>
    public event EventHandler? Downloaded;

    /// <summary>The key to save (<c>apiKey</c>).</summary>
    public string? ApiKey
    {
        get => (string?)GetValue(ApiKeyProperty);
        set => SetValue(ApiKeyProperty, value);
    }

    /// <summary>The sentence before the link (<c>body</c>): "This key cannot be shown again. Save it now."</summary>
    public string? BodyText
    {
        get => (string?)GetValue(BodyTextProperty);
        set => SetValue(BodyTextProperty, value);
    }

    /// <summary>The file name without extension (<c>fileName</c>). Default <c>apikey</c>.</summary>
    public string? FileName
    {
        get => (string?)GetValue(FileNameProperty);
        set => SetValue(FileNameProperty, value);
    }

    /// <summary>JSON (default, <c>{"apiKey":"..."}</c>) or plain text (<c>fileType</c>).</summary>
    public CdsAPIKeyFileType FileType
    {
        get => (CdsAPIKeyFileType)GetValue(FileTypeProperty);
        set => SetValue(FileTypeProperty, value);
    }

    /// <summary>The link text (<c>linkText</c>): "Download as JSON".</summary>
    public string? LinkText
    {
        get => (string?)GetValue(LinkTextProperty);
        set => SetValue(LinkTextProperty, value);
    }

    /// <summary>The link's accessible name when the text alone is not enough (<c>downloadLinkLabel</c>): "Download the API key as a JSON file".</summary>
    public string? LinkLabel
    {
        get => (string?)GetValue(LinkLabelProperty);
        set => SetValue(LinkLabelProperty, value);
    }

    /// <summary>The paragraph (for tests).</summary>
    internal CdsParagraph Paragraph => _paragraph;

    /// <summary>
    /// Saves the key as <see cref="FileName"/> with the <see cref="FileType"/> extension: raises
    /// <see cref="SaveRequested"/>, and unless a handler saved it, writes the file to the app's cache and opens the share sheet.
    /// </summary>
    public async Task DownloadAsync()
    {
        var args = new CdsAPIKeySaveEventArgs(
            APIKeyDownloaderModel.FileName(FileName, FileType),
            APIKeyDownloaderModel.Content(ApiKey, FileType),
            APIKeyDownloaderModel.ContentType(FileType));
        SaveRequested?.Invoke(this, args);
        if (!args.Handled)
        {
            try
            {
                var folder = Path.Combine(CacheDirectory(), "Maui.Carbon.APIKey");
                Directory.CreateDirectory(folder);
                var path = Path.Combine(folder, args.FileName);
                await File.WriteAllTextAsync(path, args.Content);
                await Share.Default.RequestAsync(new ShareFileRequest
                {
                    Title = LinkText,
                    File = new ShareFile(path, args.ContentType),
                });
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or NotImplementedException or FeatureNotSupportedException or PermissionException)
            {
                // No share sheet on this platform: handle SaveRequested to save the file another way.
                return;
            }
        }
        Downloaded?.Invoke(this, EventArgs.Empty);
    }

    static string CacheDirectory()
    {
        try
        {
            return FileSystem.Current.CacheDirectory;
        }
        catch (NotImplementedException)
        {
            return Path.GetTempPath();
        }
    }

    void Sync()
    {
        _body.Text = string.IsNullOrEmpty(BodyText) ? null : BodyText + " ";
        _link.Text = LinkText;
        foreach (var target in _paragraph.LinkTargets)
            SemanticProperties.SetDescription(target, string.IsNullOrEmpty(LinkLabel) ? LinkText : LinkLabel);
    }
}

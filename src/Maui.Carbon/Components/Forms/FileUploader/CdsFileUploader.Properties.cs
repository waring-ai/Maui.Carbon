namespace Maui.Carbon;

public partial class CdsFileUploader
{
    /// <summary>Bindable property for <see cref="LabelTitle"/>.</summary>
    public static readonly BindableProperty LabelTitleProperty = BindableProperty.Create(
        nameof(LabelTitle), typeof(string), typeof(CdsFileUploader), "Upload files", propertyChanged: OnUploaderChanged);

    /// <summary>Bindable property for <see cref="LabelDescription"/>.</summary>
    public static readonly BindableProperty LabelDescriptionProperty = BindableProperty.Create(
        nameof(LabelDescription), typeof(string), typeof(CdsFileUploader), null, propertyChanged: OnUploaderChanged);

    /// <summary>Bindable property for <see cref="ButtonLabel"/>.</summary>
    public static readonly BindableProperty ButtonLabelProperty = BindableProperty.Create(
        nameof(ButtonLabel), typeof(string), typeof(CdsFileUploader), "Add file", propertyChanged: OnUploaderChanged);

    /// <summary>Bindable property for <see cref="DropLabel"/>.</summary>
    public static readonly BindableProperty DropLabelProperty = BindableProperty.Create(
        nameof(DropLabel), typeof(string), typeof(CdsFileUploader), "Drag and drop files here or click to upload", propertyChanged: OnUploaderChanged);

    /// <summary>Bindable property for <see cref="Accept"/>.</summary>
    public static readonly BindableProperty AcceptProperty = BindableProperty.Create(
        nameof(Accept), typeof(string), typeof(CdsFileUploader), null);

    /// <summary>Bindable property for <see cref="Multiple"/>.</summary>
    public static readonly BindableProperty MultipleProperty = BindableProperty.Create(
        nameof(Multiple), typeof(bool), typeof(CdsFileUploader), false,
        // Coercion runs on every set, even of the current value, so it tells an app's choice from the variant default.
        coerceValue: static (b, v) => ((CdsFileUploader)b).OnMultipleSet(v));

    /// <summary>Bindable property for <see cref="Variant"/>.</summary>
    public static readonly BindableProperty VariantProperty = BindableProperty.Create(
        nameof(Variant), typeof(CdsFileUploaderVariant), typeof(CdsFileUploader), CdsFileUploaderVariant.Button,
        propertyChanged: static (b, _, _) => ((CdsFileUploader)b).OnVariantChanged());

    /// <summary>Bindable property for <see cref="ButtonKind"/>.</summary>
    public static readonly BindableProperty ButtonKindProperty = BindableProperty.Create(
        nameof(ButtonKind), typeof(CdsButtonKind), typeof(CdsFileUploader), CdsButtonKind.Primary, propertyChanged: OnUploaderChanged);

    /// <summary>Bindable property for <see cref="Size"/>.</summary>
    public static readonly BindableProperty SizeProperty = BindableProperty.Create(
        nameof(Size), typeof(CdsSize), typeof(CdsFileUploader), CdsSize.Lg, propertyChanged: OnUploaderChanged);

    /// <summary>Bindable property for <see cref="FilenameStatus"/>.</summary>
    public static readonly BindableProperty FilenameStatusProperty = BindableProperty.Create(
        nameof(FilenameStatus), typeof(CdsFileStatus), typeof(CdsFileUploader), CdsFileStatus.Edit);

    /// <summary>Bindable property for <see cref="IconDescription"/>.</summary>
    public static readonly BindableProperty IconDescriptionProperty = BindableProperty.Create(
        nameof(IconDescription), typeof(string), typeof(CdsFileUploader), "Remove file", propertyChanged: OnUploaderChanged);

    /// <summary>The heading above the control. Default "Upload files".</summary>
    public string? LabelTitle
    {
        get => (string?)GetValue(LabelTitleProperty);
        set => SetValue(LabelTitleProperty, value);
    }

    /// <summary>The size and file-type limits ("Max file size is 500 MB. Supported types are .log, .txt and .png.").</summary>
    public string? LabelDescription
    {
        get => (string?)GetValue(LabelDescriptionProperty);
        set => SetValue(LabelDescriptionProperty, value);
    }

    /// <summary>The button text: a short verb phrase. Default "Add file" (the button keeps one file by default; Carbon suggests "Add files" with <see cref="Multiple"/>).</summary>
    public string? ButtonLabel
    {
        get => (string?)GetValue(ButtonLabelProperty);
        set => SetValue(ButtonLabelProperty, value);
    }

    /// <summary>The drop container text. Default "Drag and drop files here or click to upload".</summary>
    public string? DropLabel
    {
        get => (string?)GetValue(DropLabelProperty);
        set => SetValue(DropLabelProperty, value);
    }

    /// <summary>Comma-separated extensions and MIME types the picker offers, for example <c>.csv, image/*</c>. Other files are ignored.</summary>
    public string? Accept
    {
        get => (string?)GetValue(AcceptProperty);
        set => SetValue(AcceptProperty, value);
    }

    /// <summary>
    /// Allows several files at once; false keeps one file (a new pick replaces it). The default follows
    /// <see cref="Variant"/> as in Carbon: the drop container allows several, the button keeps one. Setting it overrides
    /// that default for either variant.
    /// </summary>
    public bool Multiple
    {
        get => (bool)GetValue(MultipleProperty);
        set => SetValue(MultipleProperty, value);
    }

    /// <summary>A button (default) or a dashed drop container.</summary>
    public CdsFileUploaderVariant Variant
    {
        get => (CdsFileUploaderVariant)GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    /// <summary>The kind of the Add button. Default primary.</summary>
    public CdsButtonKind ButtonKind
    {
        get => (CdsButtonKind)GetValue(ButtonKindProperty);
        set => SetValue(ButtonKindProperty, value);
    }

    /// <summary>Height of the button and the file rows: sm 32, md 40, lg 48 (default).</summary>
    public CdsSize Size
    {
        get => (CdsSize)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    /// <summary>The status given to files added by the picker or a drop. Default <see cref="CdsFileStatus.Edit"/>.</summary>
    public CdsFileStatus FilenameStatus
    {
        get => (CdsFileStatus)GetValue(FilenameStatusProperty);
        set => SetValue(FilenameStatusProperty, value);
    }

    /// <summary>Accessible name of each remove button, followed by " - " and the file name. Default "Remove file".</summary>
    public string? IconDescription
    {
        get => (string?)GetValue(IconDescriptionProperty);
        set => SetValue(IconDescriptionProperty, value);
    }

    static void OnUploaderChanged(BindableObject b, object? o, object? n) => ((CdsFileUploader)b).UpdateLayout();

    // Multiple has a variant-dependent default (drop: several files, button: one) until the app sets it.
    bool _multipleSetByApp;
    bool _settingMultipleDefault;

    object OnMultipleSet(object value)
    {
        if (!_settingMultipleDefault) _multipleSetByApp = true;
        return value;
    }

    void OnVariantChanged()
    {
        if (!_multipleSetByApp)
        {
            _settingMultipleDefault = true;
            try
            {
                Multiple = Variant == CdsFileUploaderVariant.Drop;
            }
            finally
            {
                _settingMultipleDefault = false;
            }
        }
        UpdateLayout();
    }
}

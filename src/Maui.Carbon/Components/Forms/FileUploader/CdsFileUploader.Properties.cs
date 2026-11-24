using System.Windows.Input;

namespace Maui.Carbon;

public partial class CdsFileUploader
{
    /// <summary>Bindable property for <see cref="FilePath"/>.</summary>
    public static readonly BindableProperty FilePathProperty = BindableProperty.Create(
        nameof(FilePath), typeof(string), typeof(CdsFileUploader), null, BindingMode.TwoWay,
        propertyChanged: static (b, _, n) => ((CdsFileUploader)b).OnFilePathSet((string?)n));

    /// <summary>Bindable property for <see cref="FilePaths"/>.</summary>
    public static readonly BindableProperty FilePathsProperty = BindableProperty.Create(
        nameof(FilePaths), typeof(IReadOnlyList<string>), typeof(CdsFileUploader), null, BindingMode.TwoWay,
        propertyChanged: static (b, _, n) => ((CdsFileUploader)b).OnFilePathsSet((IReadOnlyList<string>?)n));

    /// <summary>Bindable property for <see cref="IsInvalid"/>.</summary>
    public static readonly BindableProperty IsInvalidProperty = BindableProperty.Create(
        nameof(IsInvalid), typeof(bool), typeof(CdsFileUploader), false, propertyChanged: OnChosenStateChanged);

    /// <summary>Bindable property for <see cref="ErrorSubject"/>.</summary>
    public static readonly BindableProperty ErrorSubjectProperty = BindableProperty.Create(
        nameof(ErrorSubject), typeof(string), typeof(CdsFileUploader), null, propertyChanged: OnChosenStateChanged);

    /// <summary>Bindable property for <see cref="ErrorBody"/>.</summary>
    public static readonly BindableProperty ErrorBodyProperty = BindableProperty.Create(
        nameof(ErrorBody), typeof(string), typeof(CdsFileUploader), null, propertyChanged: OnChosenStateChanged);

    /// <summary>Bindable property for <see cref="FileStatus"/>.</summary>
    public static readonly BindableProperty FileStatusProperty = BindableProperty.Create(
        nameof(FileStatus), typeof(CdsFileStatus?), typeof(CdsFileUploader), null, propertyChanged: OnChosenStateChanged);

    /// <summary>Bindable property for <see cref="ChosenButtonLabel"/>.</summary>
    public static readonly BindableProperty ChosenButtonLabelProperty = BindableProperty.Create(
        nameof(ChosenButtonLabel), typeof(string), typeof(CdsFileUploader), null, propertyChanged: OnUploaderChanged);

    /// <summary>Bindable property for <see cref="AccessMode"/>.</summary>
    public static readonly BindableProperty AccessModeProperty = BindableProperty.Create(
        nameof(AccessMode), typeof(CdsFileAccessMode), typeof(CdsFileUploader), CdsFileAccessMode.Stream);

    /// <summary>Bindable property for <see cref="FilesAddedCommand"/>.</summary>
    public static readonly BindableProperty FilesAddedCommandProperty = BindableProperty.Create(
        nameof(FilesAddedCommand), typeof(ICommand), typeof(CdsFileUploader), null);

    /// <summary>Bindable property for <see cref="FileRemovedCommand"/>.</summary>
    public static readonly BindableProperty FileRemovedCommandProperty = BindableProperty.Create(
        nameof(FileRemovedCommand), typeof(ICommand), typeof(CdsFileUploader), null);

    /// <summary>
    /// The full path of the chosen file (the first in <see cref="Files"/>), two-way: picking or dropping a file sets it,
    /// removing the row clears it to <c>null</c>, and setting it shows that file as the only row (<c>null</c> or empty
    /// removes it), so a view model can own the choice and pre-fill a default. The file does not have to exist: validate
    /// it when the path changes and report a missing or unreadable file with <see cref="IsInvalid"/> and
    /// <see cref="ErrorSubject"/>. Meant for one file (<see cref="Multiple"/> false); use <see cref="FilePaths"/> for several.
    /// </summary>
    public string? FilePath
    {
        get => (string?)GetValue(FilePathProperty);
        set => SetValue(FilePathProperty, value);
    }

    /// <summary>
    /// The full paths of the files in <see cref="Files"/>, in order, two-way. The uploader replaces the list (a new array)
    /// whenever its files change, and never changes a list you give it; assign a new list to change the files (rows
    /// whose path stays keep their status and error; blank and repeated paths are dropped; only the first is kept when
    /// <see cref="Multiple"/> is false; an empty list removes every file, <c>null</c> changes nothing). Bind it, or
    /// <see cref="FilePath"/>, not both, to an <c>IReadOnlyList&lt;string&gt;</c> or <c>IEnumerable&lt;string&gt;</c>
    /// property.
    /// </summary>
    public IReadOnlyList<string>? FilePaths
    {
        get => (IReadOnlyList<string>?)GetValue(FilePathsProperty);
        set => SetValue(FilePathsProperty, value);
    }

    /// <summary>
    /// Marks the chosen file invalid (one file, <see cref="Multiple"/> false): its row gets the 2px error outline and,
    /// under a divider, <see cref="ErrorSubject"/> and <see cref="ErrorBody"/>, spoken as an alert and describing the
    /// remove button. Always set <see cref="ErrorSubject"/> with it. Once any of the three is set, they describe whichever
    /// file is chosen, so update them when <see cref="FilePath"/> changes. For several files set
    /// <see cref="CdsUploadedFile.IsInvalid"/> on each entry instead.
    /// </summary>
    public bool IsInvalid
    {
        get => (bool)GetValue(IsInvalidProperty);
        set => SetValue(IsInvalidProperty, value);
    }

    /// <summary>The chosen file's error title: what went wrong ("File not found"). See <see cref="IsInvalid"/>.</summary>
    public string? ErrorSubject
    {
        get => (string?)GetValue(ErrorSubjectProperty);
        set => SetValue(ErrorSubjectProperty, value);
    }

    /// <summary>The chosen file's supporting error text: how to fix it ("Choose a .zip export that exists."). See <see cref="IsInvalid"/>.</summary>
    public string? ErrorBody
    {
        get => (string?)GetValue(ErrorBodyProperty);
        set => SetValue(ErrorBodyProperty, value);
    }

    /// <summary>
    /// The chosen file's status (one file, <see cref="Multiple"/> false): uploading (spinner), edit (remove button) or
    /// complete (check). <c>null</c> (default) leaves each file the status it was given (<see cref="FilenameStatus"/> when added).
    /// </summary>
    public CdsFileStatus? FileStatus
    {
        get => (CdsFileStatus?)GetValue(FileStatusProperty);
        set => SetValue(FileStatusProperty, value);
    }

    /// <summary>
    /// The button text while a file is chosen, for a field that picks a file rather than uploads one ("Change file").
    /// Carbon's FileUploaderButton changes its label after a pick; <c>null</c> (default) keeps <see cref="ButtonLabel"/>
    /// as Carbon's FileUploader does.
    /// </summary>
    public string? ChosenButtonLabel
    {
        get => (string?)GetValue(ChosenButtonLabelProperty);
        set => SetValue(ChosenButtonLabelProperty, value);
    }

    /// <summary>
    /// How the app reads the chosen files: <see cref="CdsFileAccessMode.Stream"/> (default, for uploads) or
    /// <see cref="CdsFileAccessMode.Path"/> to read the original file in place at its path (on iOS and Mac Catalyst
    /// the uploader keeps the file's security-scoped access while it is chosen).
    /// </summary>
    public CdsFileAccessMode AccessMode
    {
        get => (CdsFileAccessMode)GetValue(AccessModeProperty);
        set => SetValue(AccessModeProperty, value);
    }

    /// <summary>
    /// Run after people add files with the picker or a drop (with <see cref="FilesAdded"/>); the parameter is the added
    /// files' full paths, an <c>IReadOnlyList&lt;string&gt;</c>.
    /// </summary>
    public ICommand? FilesAddedCommand
    {
        get => (ICommand?)GetValue(FilesAddedCommandProperty);
        set => SetValue(FilesAddedCommandProperty, value);
    }

    /// <summary>
    /// Run after people remove a file with its remove button (with <see cref="FileRemoved"/>); the parameter is the
    /// removed file's full path (a <c>string</c>, <c>null</c> for an entry without one).
    /// </summary>
    public ICommand? FileRemovedCommand
    {
        get => (ICommand?)GetValue(FileRemovedCommandProperty);
        set => SetValue(FileRemovedCommandProperty, value);
    }

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

    static void OnChosenStateChanged(BindableObject b, object? o, object? n) => ((CdsFileUploader)b)._applyChosen.Request();

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

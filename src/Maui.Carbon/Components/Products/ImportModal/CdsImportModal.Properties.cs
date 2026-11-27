using System.Windows.Input;

namespace Maui.Carbon;

public partial class CdsImportModal
{
    /// <summary>Bindable property for <see cref="Title"/>.</summary>
    public static readonly BindableProperty TitleProperty = BindableProperty.Create(
        nameof(Title), typeof(string), typeof(CdsImportModal), null,
        propertyChanged: static (b, _, _) => ((CdsImportModal)b).Sync());

    /// <summary>Bindable property for <see cref="Description"/>.</summary>
    public static readonly BindableProperty DescriptionProperty = BindableProperty.Create(
        nameof(Description), typeof(string), typeof(CdsImportModal), null,
        propertyChanged: static (b, _, _) => ((CdsImportModal)b).Sync());

    /// <summary>Bindable property for <see cref="Accept"/>.</summary>
    public static readonly BindableProperty AcceptProperty = BindableProperty.Create(
        nameof(Accept), typeof(string), typeof(CdsImportModal), null,
        propertyChanged: static (b, _, _) => ((CdsImportModal)b).Sync());

    /// <summary>Bindable property for <see cref="MaxFileSize"/>.</summary>
    public static readonly BindableProperty MaxFileSizeProperty = BindableProperty.Create(
        nameof(MaxFileSize), typeof(long), typeof(CdsImportModal), 0L,
        propertyChanged: static (b, _, _) => ((CdsImportModal)b).Sync());

    /// <summary>Bindable property for <see cref="DefaultErrorHeader"/>.</summary>
    public static readonly BindableProperty DefaultErrorHeaderProperty = BindableProperty.Create(
        nameof(DefaultErrorHeader), typeof(string), typeof(CdsImportModal), null,
        propertyChanged: static (b, _, _) => ((CdsImportModal)b).Sync());

    /// <summary>Bindable property for <see cref="DefaultErrorBody"/>.</summary>
    public static readonly BindableProperty DefaultErrorBodyProperty = BindableProperty.Create(
        nameof(DefaultErrorBody), typeof(string), typeof(CdsImportModal), null,
        propertyChanged: static (b, _, _) => ((CdsImportModal)b).Sync());

    /// <summary>Bindable property for <see cref="FetchErrorHeader"/>.</summary>
    public static readonly BindableProperty FetchErrorHeaderProperty = BindableProperty.Create(
        nameof(FetchErrorHeader), typeof(string), typeof(CdsImportModal), null,
        propertyChanged: static (b, _, _) => ((CdsImportModal)b).Sync());

    /// <summary>Bindable property for <see cref="FetchErrorBody"/>.</summary>
    public static readonly BindableProperty FetchErrorBodyProperty = BindableProperty.Create(
        nameof(FetchErrorBody), typeof(string), typeof(CdsImportModal), null,
        propertyChanged: static (b, _, _) => ((CdsImportModal)b).Sync());

    /// <summary>Bindable property for <see cref="InvalidFileTypeErrorHeader"/>.</summary>
    public static readonly BindableProperty InvalidFileTypeErrorHeaderProperty = BindableProperty.Create(
        nameof(InvalidFileTypeErrorHeader), typeof(string), typeof(CdsImportModal), null,
        propertyChanged: static (b, _, _) => ((CdsImportModal)b).Sync());

    /// <summary>Bindable property for <see cref="InvalidFileTypeErrorBody"/>.</summary>
    public static readonly BindableProperty InvalidFileTypeErrorBodyProperty = BindableProperty.Create(
        nameof(InvalidFileTypeErrorBody), typeof(string), typeof(CdsImportModal), null,
        propertyChanged: static (b, _, _) => ((CdsImportModal)b).Sync());

    /// <summary>Bindable property for <see cref="MaxFileSizeErrorHeader"/>.</summary>
    public static readonly BindableProperty MaxFileSizeErrorHeaderProperty = BindableProperty.Create(
        nameof(MaxFileSizeErrorHeader), typeof(string), typeof(CdsImportModal), null,
        propertyChanged: static (b, _, _) => ((CdsImportModal)b).Sync());

    /// <summary>Bindable property for <see cref="MaxFileSizeErrorBody"/>.</summary>
    public static readonly BindableProperty MaxFileSizeErrorBodyProperty = BindableProperty.Create(
        nameof(MaxFileSizeErrorBody), typeof(string), typeof(CdsImportModal), null,
        propertyChanged: static (b, _, _) => ((CdsImportModal)b).Sync());

    /// <summary>Bindable property for <see cref="FileDropHeader"/>.</summary>
    public static readonly BindableProperty FileDropHeaderProperty = BindableProperty.Create(
        nameof(FileDropHeader), typeof(string), typeof(CdsImportModal), null,
        propertyChanged: static (b, _, _) => ((CdsImportModal)b).Sync());

    /// <summary>Bindable property for <see cref="FileDropLabel"/>.</summary>
    public static readonly BindableProperty FileDropLabelProperty = BindableProperty.Create(
        nameof(FileDropLabel), typeof(string), typeof(CdsImportModal), null,
        propertyChanged: static (b, _, _) => ((CdsImportModal)b).Sync());

    /// <summary>Bindable property for <see cref="FileUploadLabel"/>.</summary>
    public static readonly BindableProperty FileUploadLabelProperty = BindableProperty.Create(
        nameof(FileUploadLabel), typeof(string), typeof(CdsImportModal), null,
        propertyChanged: static (b, _, _) => ((CdsImportModal)b).Sync());

    /// <summary>Bindable property for <see cref="InputLabel"/>.</summary>
    public static readonly BindableProperty InputLabelProperty = BindableProperty.Create(
        nameof(InputLabel), typeof(string), typeof(CdsImportModal), null,
        propertyChanged: static (b, _, _) => ((CdsImportModal)b).Sync());

    /// <summary>Bindable property for <see cref="HideInputLabel"/>.</summary>
    public static readonly BindableProperty HideInputLabelProperty = BindableProperty.Create(
        nameof(HideInputLabel), typeof(bool), typeof(CdsImportModal), false,
        propertyChanged: static (b, _, _) => ((CdsImportModal)b).Sync());

    /// <summary>Bindable property for <see cref="InputPlaceholder"/>.</summary>
    public static readonly BindableProperty InputPlaceholderProperty = BindableProperty.Create(
        nameof(InputPlaceholder), typeof(string), typeof(CdsImportModal), null,
        propertyChanged: static (b, _, _) => ((CdsImportModal)b).Sync());

    /// <summary>Bindable property for <see cref="InputButtonText"/>.</summary>
    public static readonly BindableProperty InputButtonTextProperty = BindableProperty.Create(
        nameof(InputButtonText), typeof(string), typeof(CdsImportModal), null,
        propertyChanged: static (b, _, _) => ((CdsImportModal)b).Sync());

    /// <summary>Bindable property for <see cref="InputButtonIcon"/>.</summary>
    public static readonly BindableProperty InputButtonIconProperty = BindableProperty.Create(
        nameof(InputButtonIcon), typeof(bool), typeof(CdsImportModal), false,
        propertyChanged: static (b, _, _) => ((CdsImportModal)b).Sync());

    /// <summary>Bindable property for <see cref="InvalidIconDescription"/>.</summary>
    public static readonly BindableProperty InvalidIconDescriptionProperty = BindableProperty.Create(
        nameof(InvalidIconDescription), typeof(string), typeof(CdsImportModal), "Delete",
        propertyChanged: static (b, _, _) => ((CdsImportModal)b).Sync());

    /// <summary>Bindable property for <see cref="PrimaryButtonText"/>.</summary>
    public static readonly BindableProperty PrimaryButtonTextProperty = BindableProperty.Create(
        nameof(PrimaryButtonText), typeof(string), typeof(CdsImportModal), null,
        propertyChanged: static (b, _, _) => ((CdsImportModal)b).Sync());

    /// <summary>Bindable property for <see cref="SecondaryButtonText"/>.</summary>
    public static readonly BindableProperty SecondaryButtonTextProperty = BindableProperty.Create(
        nameof(SecondaryButtonText), typeof(string), typeof(CdsImportModal), null,
        propertyChanged: static (b, _, _) => ((CdsImportModal)b).Sync());

    /// <summary>Bindable property for <see cref="RequestSubmitCommand"/>.</summary>
    public static readonly BindableProperty RequestSubmitCommandProperty = BindableProperty.Create(
        nameof(RequestSubmitCommand), typeof(ICommand), typeof(CdsImportModal), null);

    /// <summary>The heading (<c>title</c>): "Import". It also names the dialog.</summary>
    public string? Title
    {
        get => (string?)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>The paragraph at the top (<c>description</c>): how to add a file and the size limit.</summary>
    public string? Description
    {
        get => (string?)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    /// <summary>The file types that can be imported (<c>accept</c>), comma separated MIME types or extensions: "image/png, .jpg". Other files show as invalid rows. Empty accepts any file.</summary>
    public string? Accept
    {
        get => (string?)GetValue(AcceptProperty);
        set => SetValue(AcceptProperty, value);
    }

    /// <summary>The largest file in bytes (<c>maxFileSize</c>); larger files show as invalid rows. 0 (the default) means no limit.</summary>
    public long MaxFileSize
    {
        get => (long)GetValue(MaxFileSizeProperty);
        set => SetValue(MaxFileSizeProperty, value);
    }

    /// <summary>Error title of a rejected file when no specific one is set (<c>defaultErrorHeader</c>): "Import failed".</summary>
    public string? DefaultErrorHeader
    {
        get => (string?)GetValue(DefaultErrorHeaderProperty);
        set => SetValue(DefaultErrorHeaderProperty, value);
    }

    /// <summary>Error text of a rejected file when no specific one is set (<c>defaultErrorBody</c>): "Select a new file and try again."</summary>
    public string? DefaultErrorBody
    {
        get => (string?)GetValue(DefaultErrorBodyProperty);
        set => SetValue(DefaultErrorBodyProperty, value);
    }

    /// <summary>Error title when the URL cannot be fetched (<c>fetchErrorHeader</c>).</summary>
    public string? FetchErrorHeader
    {
        get => (string?)GetValue(FetchErrorHeaderProperty);
        set => SetValue(FetchErrorHeaderProperty, value);
    }

    /// <summary>Error text when the URL cannot be fetched (<c>fetchErrorBody</c>): "Unable to fetch URL."</summary>
    public string? FetchErrorBody
    {
        get => (string?)GetValue(FetchErrorBodyProperty);
        set => SetValue(FetchErrorBodyProperty, value);
    }

    /// <summary>Error title of a file whose type is not accepted (<c>invalidFileTypeErrorHeader</c>).</summary>
    public string? InvalidFileTypeErrorHeader
    {
        get => (string?)GetValue(InvalidFileTypeErrorHeaderProperty);
        set => SetValue(InvalidFileTypeErrorHeaderProperty, value);
    }

    /// <summary>Error text of a file whose type is not accepted (<c>invalidFileTypeErrorBody</c>): "Invalid file type."</summary>
    public string? InvalidFileTypeErrorBody
    {
        get => (string?)GetValue(InvalidFileTypeErrorBodyProperty);
        set => SetValue(InvalidFileTypeErrorBodyProperty, value);
    }

    /// <summary>Error title of a file over <see cref="MaxFileSize"/> (<c>maxFileSizeErrorHeader</c>).</summary>
    public string? MaxFileSizeErrorHeader
    {
        get => (string?)GetValue(MaxFileSizeErrorHeaderProperty);
        set => SetValue(MaxFileSizeErrorHeaderProperty, value);
    }

    /// <summary>Error text of a file over <see cref="MaxFileSize"/> (<c>maxFileSizeErrorBody</c>): "500kb max file size."</summary>
    public string? MaxFileSizeErrorBody
    {
        get => (string?)GetValue(MaxFileSizeErrorBodyProperty);
        set => SetValue(MaxFileSizeErrorBodyProperty, value);
    }

    /// <summary><c>heading-compact-01</c> heading above the drop area (<c>fileDropHeader</c>): "Add files using drag and drop".</summary>
    public string? FileDropHeader
    {
        get => (string?)GetValue(FileDropHeaderProperty);
        set => SetValue(FileDropHeaderProperty, value);
    }

    /// <summary>Text of the drop area (<c>fileDropLabel</c>): "Drag and drop files here or click to upload". Activating it opens the file picker.</summary>
    public string? FileDropLabel
    {
        get => (string?)GetValue(FileDropLabelProperty);
        set => SetValue(FileDropLabelProperty, value);
    }

    /// <summary>Word after the count above the rows (<c>fileUploadLabel</c>): "files uploaded" gives "1 / 1 files uploaded".</summary>
    public string? FileUploadLabel
    {
        get => (string?)GetValue(FileUploadLabelProperty);
        set => SetValue(FileUploadLabelProperty, value);
    }

    /// <summary>Heading of the URL field (<c>inputLabel</c>), also the field's accessible name: "Add a file by specifying a URL".</summary>
    public string? InputLabel
    {
        get => (string?)GetValue(InputLabelProperty);
        set => SetValue(InputLabelProperty, value);
    }

    /// <summary>Hides the URL field heading (<c>hideInputLabel</c>); the field keeps it as its accessible name.</summary>
    public bool HideInputLabel
    {
        get => (bool)GetValue(HideInputLabelProperty);
        set => SetValue(HideInputLabelProperty, value);
    }

    /// <summary>Placeholder of the URL field (<c>inputPlaceholder</c>): "URL".</summary>
    public string? InputPlaceholder
    {
        get => (string?)GetValue(InputPlaceholderProperty);
        set => SetValue(InputPlaceholderProperty, value);
    }

    /// <summary>Label of the fetch button next to the URL field (<c>inputButtonText</c>): "Add file".</summary>
    public string? InputButtonText
    {
        get => (string?)GetValue(InputButtonTextProperty);
        set => SetValue(InputButtonTextProperty, value);
    }

    /// <summary>Shows an add icon in the fetch button (<c>inputButtonIcon</c>).</summary>
    public bool InputButtonIcon
    {
        get => (bool)GetValue(InputButtonIconProperty);
        set => SetValue(InputButtonIconProperty, value);
    }

    /// <summary>Accessible name of each row's remove button (<c>invalidIconDescription</c>). Default <c>Delete</c>.</summary>
    public string InvalidIconDescription
    {
        get => (string)GetValue(InvalidIconDescriptionProperty);
        set => SetValue(InvalidIconDescriptionProperty, value);
    }

    /// <summary>Label of the primary button (<c>primaryButtonText</c>): "Import". It enables once a valid file is chosen.</summary>
    public string? PrimaryButtonText
    {
        get => (string?)GetValue(PrimaryButtonTextProperty);
        set => SetValue(PrimaryButtonTextProperty, value);
    }

    /// <summary>Label of the secondary button (<c>secondaryButtonText</c>), "Cancel"; it asks to close and clears the files.</summary>
    public string? SecondaryButtonText
    {
        get => (string?)GetValue(SecondaryButtonTextProperty);
        set => SetValue(SecondaryButtonTextProperty, value);
    }

    /// <summary>Runs with the files (an <see cref="IReadOnlyList{T}"/> of <see cref="CdsUploadedFile"/>) when the primary button is activated.</summary>
    public ICommand? RequestSubmitCommand
    {
        get => (ICommand?)GetValue(RequestSubmitCommandProperty);
        set => SetValue(RequestSubmitCommandProperty, value);
    }
}

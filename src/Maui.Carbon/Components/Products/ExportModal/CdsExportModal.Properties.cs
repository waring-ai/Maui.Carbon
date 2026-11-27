using System.Windows.Input;

namespace Maui.Carbon;

public partial class CdsExportModal
{
    /// <summary>Bindable property for <see cref="Title"/>.</summary>
    public static readonly BindableProperty TitleProperty = BindableProperty.Create(
        nameof(Title), typeof(string), typeof(CdsExportModal), null,
        propertyChanged: static (b, _, _) => ((CdsExportModal)b).Sync());

    /// <summary>Bindable property for <see cref="BodyText"/>.</summary>
    public static readonly BindableProperty BodyTextProperty = BindableProperty.Create(
        nameof(BodyText), typeof(string), typeof(CdsExportModal), null,
        propertyChanged: static (b, _, _) => ((CdsExportModal)b).Sync());

    /// <summary>Bindable property for <see cref="Filename"/>.</summary>
    public static readonly BindableProperty FilenameProperty = BindableProperty.Create(
        nameof(Filename), typeof(string), typeof(CdsExportModal), null,
        propertyChanged: static (b, _, _) => ((CdsExportModal)b).OnFilenameChanged());

    /// <summary>Bindable property for <see cref="InputLabel"/>.</summary>
    public static readonly BindableProperty InputLabelProperty = BindableProperty.Create(
        nameof(InputLabel), typeof(string), typeof(CdsExportModal), null,
        propertyChanged: static (b, _, _) => ((CdsExportModal)b).Sync());

    /// <summary>Bindable property for <see cref="InputType"/>.</summary>
    public static readonly BindableProperty InputTypeProperty = BindableProperty.Create(
        nameof(InputType), typeof(CdsExportModalInputType), typeof(CdsExportModal), CdsExportModalInputType.Text,
        propertyChanged: static (b, _, _) => ((CdsExportModal)b).Sync());

    /// <summary>Bindable property for <see cref="InvalidInputText"/>.</summary>
    public static readonly BindableProperty InvalidInputTextProperty = BindableProperty.Create(
        nameof(InvalidInputText), typeof(string), typeof(CdsExportModal), null,
        propertyChanged: static (b, _, _) => ((CdsExportModal)b).Sync());

    /// <summary>Bindable property for <see cref="PreformattedExtensionsLabel"/>.</summary>
    public static readonly BindableProperty PreformattedExtensionsLabelProperty = BindableProperty.Create(
        nameof(PreformattedExtensionsLabel), typeof(string), typeof(CdsExportModal), null,
        propertyChanged: static (b, _, _) => ((CdsExportModal)b).Sync());

    /// <summary>Bindable property for <see cref="ShowPasswordLabel"/>.</summary>
    public static readonly BindableProperty ShowPasswordLabelProperty = BindableProperty.Create(
        nameof(ShowPasswordLabel), typeof(string), typeof(CdsExportModal), null,
        propertyChanged: static (b, _, _) => ((CdsExportModal)b).Sync());

    /// <summary>Bindable property for <see cref="HidePasswordLabel"/>.</summary>
    public static readonly BindableProperty HidePasswordLabelProperty = BindableProperty.Create(
        nameof(HidePasswordLabel), typeof(string), typeof(CdsExportModal), null,
        propertyChanged: static (b, _, _) => ((CdsExportModal)b).Sync());

    /// <summary>Bindable property for <see cref="PrimaryButtonText"/>.</summary>
    public static readonly BindableProperty PrimaryButtonTextProperty = BindableProperty.Create(
        nameof(PrimaryButtonText), typeof(string), typeof(CdsExportModal), null,
        propertyChanged: static (b, _, _) => ((CdsExportModal)b).Sync());

    /// <summary>Bindable property for <see cref="SecondaryButtonText"/>.</summary>
    public static readonly BindableProperty SecondaryButtonTextProperty = BindableProperty.Create(
        nameof(SecondaryButtonText), typeof(string), typeof(CdsExportModal), null,
        propertyChanged: static (b, _, _) => ((CdsExportModal)b).Sync());

    /// <summary>Bindable property for <see cref="IsLoading"/>.</summary>
    public static readonly BindableProperty IsLoadingProperty = BindableProperty.Create(
        nameof(IsLoading), typeof(bool), typeof(CdsExportModal), false,
        propertyChanged: static (b, _, _) => ((CdsExportModal)b).Sync());

    /// <summary>Bindable property for <see cref="LoadingMessage"/>.</summary>
    public static readonly BindableProperty LoadingMessageProperty = BindableProperty.Create(
        nameof(LoadingMessage), typeof(string), typeof(CdsExportModal), null,
        propertyChanged: static (b, _, _) => ((CdsExportModal)b).Sync());

    /// <summary>Bindable property for <see cref="IsSuccessful"/>.</summary>
    public static readonly BindableProperty IsSuccessfulProperty = BindableProperty.Create(
        nameof(IsSuccessful), typeof(bool), typeof(CdsExportModal), false,
        propertyChanged: static (b, _, _) => ((CdsExportModal)b).Sync());

    /// <summary>Bindable property for <see cref="SuccessMessage"/>.</summary>
    public static readonly BindableProperty SuccessMessageProperty = BindableProperty.Create(
        nameof(SuccessMessage), typeof(string), typeof(CdsExportModal), null,
        propertyChanged: static (b, _, _) => ((CdsExportModal)b).Sync());

    /// <summary>Bindable property for <see cref="IsError"/>.</summary>
    public static readonly BindableProperty IsErrorProperty = BindableProperty.Create(
        nameof(IsError), typeof(bool), typeof(CdsExportModal), false,
        propertyChanged: static (b, _, _) => ((CdsExportModal)b).Sync());

    /// <summary>Bindable property for <see cref="ErrorMessage"/>.</summary>
    public static readonly BindableProperty ErrorMessageProperty = BindableProperty.Create(
        nameof(ErrorMessage), typeof(string), typeof(CdsExportModal), null,
        propertyChanged: static (b, _, _) => ((CdsExportModal)b).Sync());

    /// <summary>Bindable property for <see cref="RequestSubmitCommand"/>.</summary>
    public static readonly BindableProperty RequestSubmitCommandProperty = BindableProperty.Create(
        nameof(RequestSubmitCommand), typeof(ICommand), typeof(CdsExportModal), null);

    /// <summary>The heading (<c>title</c>): "Export". It also names the dialog.</summary>
    public string? Title
    {
        get => (string?)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>Optional message above the field (<c>body</c>): "File must be exported in a PDF format."</summary>
    public string? BodyText
    {
        get => (string?)GetValue(BodyTextProperty);
        set => SetValue(BodyTextProperty, value);
    }

    /// <summary>The suggested file name (<c>filename</c>), put in the input each time the modal opens. With formats (<see cref="CdsExportModal.PreformattedExtensions"/>) it is the name without extension.</summary>
    public string? Filename
    {
        get => (string?)GetValue(FilenameProperty);
        set => SetValue(FilenameProperty, value);
    }

    /// <summary>Label of the file name input (<c>inputLabel</c>): "File name".</summary>
    public string? InputLabel
    {
        get => (string?)GetValue(InputLabelProperty);
        set => SetValue(InputLabelProperty, value);
    }

    /// <summary>A plain text input (default) or a password input with a visibility toggle (<c>inputType</c>).</summary>
    public CdsExportModalInputType InputType
    {
        get => (CdsExportModalInputType)GetValue(InputTypeProperty);
        set => SetValue(InputTypeProperty, value);
    }

    /// <summary>Shown under the input when the name lacks a valid extension (<c>invalidInputText</c>): "File must have valid extension .pdf".</summary>
    public string? InvalidInputText
    {
        get => (string?)GetValue(InvalidInputTextProperty);
        set => SetValue(InvalidInputTextProperty, value);
    }

    /// <summary>Legend of the format radio buttons (<c>preformattedExtensionsLabel</c>): "Choose an export format".</summary>
    public string? PreformattedExtensionsLabel
    {
        get => (string?)GetValue(PreformattedExtensionsLabelProperty);
        set => SetValue(PreformattedExtensionsLabelProperty, value);
    }

    /// <summary>Tooltip and name of the show toggle of a password input (<c>showPasswordLabel</c>).</summary>
    public string? ShowPasswordLabel
    {
        get => (string?)GetValue(ShowPasswordLabelProperty);
        set => SetValue(ShowPasswordLabelProperty, value);
    }

    /// <summary>Tooltip and name of the hide toggle of a password input (<c>hidePasswordLabel</c>).</summary>
    public string? HidePasswordLabel
    {
        get => (string?)GetValue(HidePasswordLabelProperty);
        set => SetValue(HidePasswordLabelProperty, value);
    }

    /// <summary>Label of the primary button (<c>primaryButtonText</c>): "Export".</summary>
    public string? PrimaryButtonText
    {
        get => (string?)GetValue(PrimaryButtonTextProperty);
        set => SetValue(PrimaryButtonTextProperty, value);
    }

    /// <summary>Label of the secondary button (<c>secondaryButtonText</c>), "Cancel"; it asks to close.</summary>
    public string? SecondaryButtonText
    {
        get => (string?)GetValue(SecondaryButtonTextProperty);
        set => SetValue(SecondaryButtonTextProperty, value);
    }

    /// <summary>The export runs (<c>loading</c>): the fields and footer give way to an inline loading with <see cref="LoadingMessage"/>.</summary>
    public bool IsLoading
    {
        get => (bool)GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    /// <summary>Text next to the loading indicator (<c>loadingMessage</c>): "Exporting file..."</summary>
    public string? LoadingMessage
    {
        get => (string?)GetValue(LoadingMessageProperty);
        set => SetValue(LoadingMessageProperty, value);
    }

    /// <summary>The export finished (<c>successful</c>): a check and <see cref="SuccessMessage"/> replace the fields, and focus moves to the close button.</summary>
    public bool IsSuccessful
    {
        get => (bool)GetValue(IsSuccessfulProperty);
        set => SetValue(IsSuccessfulProperty, value);
    }

    /// <summary>Text of the success state (<c>successMessage</c>): "The file has been exported."</summary>
    public string? SuccessMessage
    {
        get => (string?)GetValue(SuccessMessageProperty);
        set => SetValue(SuccessMessageProperty, value);
    }

    /// <summary>The export failed (<c>error</c>): an error icon and <see cref="ErrorMessage"/> replace the fields.</summary>
    public bool IsError
    {
        get => (bool)GetValue(IsErrorProperty);
        set => SetValue(IsErrorProperty, value);
    }

    /// <summary>Text of the error state (<c>errorMessage</c>): "Server error 500".</summary>
    public string? ErrorMessage
    {
        get => (string?)GetValue(ErrorMessageProperty);
        set => SetValue(ErrorMessageProperty, value);
    }

    /// <summary>Runs with the file name (a string) when the primary button is activated, after <see cref="CdsExportModal.RequestSubmit"/>.</summary>
    public ICommand? RequestSubmitCommand
    {
        get => (ICommand?)GetValue(RequestSubmitCommandProperty);
        set => SetValue(RequestSubmitCommandProperty, value);
    }
}

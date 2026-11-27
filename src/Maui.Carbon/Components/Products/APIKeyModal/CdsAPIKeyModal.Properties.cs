using System.Windows.Input;

namespace Maui.Carbon;

public partial class CdsAPIKeyModal
{
    /// <summary>Bindable property for <see cref="ApiKey"/>.</summary>
    public static readonly BindableProperty ApiKeyProperty = BindableProperty.Create(
        nameof(ApiKey), typeof(string), typeof(CdsAPIKeyModal), null,
        propertyChanged: static (b, _, _) => ((CdsAPIKeyModal)b).Sync());

    /// <summary>Bindable property for <see cref="ApiKeyLabel"/>.</summary>
    public static readonly BindableProperty ApiKeyLabelProperty = BindableProperty.Create(
        nameof(ApiKeyLabel), typeof(string), typeof(CdsAPIKeyModal), null,
        propertyChanged: static (b, _, _) => ((CdsAPIKeyModal)b).Sync());

    /// <summary>Bindable property for <see cref="ApiKeyName"/>.</summary>
    public static readonly BindableProperty ApiKeyNameProperty = BindableProperty.Create(
        nameof(ApiKeyName), typeof(string), typeof(CdsAPIKeyModal), null,
        propertyChanged: static (b, _, _) => ((CdsAPIKeyModal)b).OnApiKeyNameChanged());

    /// <summary>Bindable property for <see cref="BodyText"/>.</summary>
    public static readonly BindableProperty BodyTextProperty = BindableProperty.Create(
        nameof(BodyText), typeof(string), typeof(CdsAPIKeyModal), null,
        propertyChanged: static (b, _, _) => ((CdsAPIKeyModal)b).Sync());

    /// <summary>Bindable property for <see cref="ModalLabel"/>.</summary>
    public static readonly BindableProperty ModalLabelProperty = BindableProperty.Create(
        nameof(ModalLabel), typeof(string), typeof(CdsAPIKeyModal), null,
        propertyChanged: static (b, _, _) => ((CdsAPIKeyModal)b).Sync());

    /// <summary>Bindable property for <see cref="GenerateTitle"/>.</summary>
    public static readonly BindableProperty GenerateTitleProperty = BindableProperty.Create(
        nameof(GenerateTitle), typeof(string), typeof(CdsAPIKeyModal), null,
        propertyChanged: static (b, _, _) => ((CdsAPIKeyModal)b).Sync());

    /// <summary>Bindable property for <see cref="GenerateButtonText"/>.</summary>
    public static readonly BindableProperty GenerateButtonTextProperty = BindableProperty.Create(
        nameof(GenerateButtonText), typeof(string), typeof(CdsAPIKeyModal), null,
        propertyChanged: static (b, _, _) => ((CdsAPIKeyModal)b).Sync());

    /// <summary>Bindable property for <see cref="GenerateSuccessMessage"/>.</summary>
    public static readonly BindableProperty GenerateSuccessMessageProperty = BindableProperty.Create(
        nameof(GenerateSuccessMessage), typeof(string), typeof(CdsAPIKeyModal), null,
        propertyChanged: static (b, _, _) => ((CdsAPIKeyModal)b).Sync());

    /// <summary>Bindable property for <see cref="GenerateSuccessBody"/>.</summary>
    public static readonly BindableProperty GenerateSuccessBodyProperty = BindableProperty.Create(
        nameof(GenerateSuccessBody), typeof(string), typeof(CdsAPIKeyModal), null,
        propertyChanged: static (b, _, _) => ((CdsAPIKeyModal)b).Sync());

    /// <summary>Bindable property for <see cref="CopyButtonText"/>.</summary>
    public static readonly BindableProperty CopyButtonTextProperty = BindableProperty.Create(
        nameof(CopyButtonText), typeof(string), typeof(CdsAPIKeyModal), null,
        propertyChanged: static (b, _, _) => ((CdsAPIKeyModal)b).Sync());

    /// <summary>Bindable property for <see cref="CopyIconDescription"/>.</summary>
    public static readonly BindableProperty CopyIconDescriptionProperty = BindableProperty.Create(
        nameof(CopyIconDescription), typeof(string), typeof(CdsAPIKeyModal), null,
        propertyChanged: static (b, _, _) => ((CdsAPIKeyModal)b).Sync());

    /// <summary>Bindable property for <see cref="CopyErrorText"/>.</summary>
    public static readonly BindableProperty CopyErrorTextProperty = BindableProperty.Create(
        nameof(CopyErrorText), typeof(string), typeof(CdsAPIKeyModal), null,
        propertyChanged: static (b, _, _) => ((CdsAPIKeyModal)b).Sync());

    /// <summary>Bindable property for <see cref="CloseButtonText"/>.</summary>
    public static readonly BindableProperty CloseButtonTextProperty = BindableProperty.Create(
        nameof(CloseButtonText), typeof(string), typeof(CdsAPIKeyModal), null,
        propertyChanged: static (b, _, _) => ((CdsAPIKeyModal)b).Sync());

    /// <summary>Bindable property for <see cref="IsEditing"/>.</summary>
    public static readonly BindableProperty IsEditingProperty = BindableProperty.Create(
        nameof(IsEditing), typeof(bool), typeof(CdsAPIKeyModal), false,
        propertyChanged: static (b, _, _) => ((CdsAPIKeyModal)b).Sync());

    /// <summary>Bindable property for <see cref="EditButtonText"/>.</summary>
    public static readonly BindableProperty EditButtonTextProperty = BindableProperty.Create(
        nameof(EditButtonText), typeof(string), typeof(CdsAPIKeyModal), null,
        propertyChanged: static (b, _, _) => ((CdsAPIKeyModal)b).Sync());

    /// <summary>Bindable property for <see cref="IsEditSuccess"/>.</summary>
    public static readonly BindableProperty IsEditSuccessProperty = BindableProperty.Create(
        nameof(IsEditSuccess), typeof(bool), typeof(CdsAPIKeyModal), false,
        propertyChanged: static (b, _, _) => ((CdsAPIKeyModal)b).Sync());

    /// <summary>Bindable property for <see cref="EditSuccessMessage"/>.</summary>
    public static readonly BindableProperty EditSuccessMessageProperty = BindableProperty.Create(
        nameof(EditSuccessMessage), typeof(string), typeof(CdsAPIKeyModal), null,
        propertyChanged: static (b, _, _) => ((CdsAPIKeyModal)b).Sync());

    /// <summary>Bindable property for <see cref="IsLoading"/>.</summary>
    public static readonly BindableProperty IsLoadingProperty = BindableProperty.Create(
        nameof(IsLoading), typeof(bool), typeof(CdsAPIKeyModal), false,
        propertyChanged: static (b, _, _) => ((CdsAPIKeyModal)b).Sync());

    /// <summary>Bindable property for <see cref="LoadingText"/>.</summary>
    public static readonly BindableProperty LoadingTextProperty = BindableProperty.Create(
        nameof(LoadingText), typeof(string), typeof(CdsAPIKeyModal), null,
        propertyChanged: static (b, _, _) => ((CdsAPIKeyModal)b).Sync());

    /// <summary>Bindable property for <see cref="IsError"/>.</summary>
    public static readonly BindableProperty IsErrorProperty = BindableProperty.Create(
        nameof(IsError), typeof(bool), typeof(CdsAPIKeyModal), false,
        propertyChanged: static (b, _, _) => ((CdsAPIKeyModal)b).Sync());

    /// <summary>Bindable property for <see cref="ErrorText"/>.</summary>
    public static readonly BindableProperty ErrorTextProperty = BindableProperty.Create(
        nameof(ErrorText), typeof(string), typeof(CdsAPIKeyModal), null,
        propertyChanged: static (b, _, _) => ((CdsAPIKeyModal)b).Sync());

    /// <summary>Bindable property for <see cref="IsNameRequired"/>.</summary>
    public static readonly BindableProperty IsNameRequiredProperty = BindableProperty.Create(
        nameof(IsNameRequired), typeof(bool), typeof(CdsAPIKeyModal), false,
        propertyChanged: static (b, _, _) => ((CdsAPIKeyModal)b).Sync());

    /// <summary>Bindable property for <see cref="NameLabel"/>.</summary>
    public static readonly BindableProperty NameLabelProperty = BindableProperty.Create(
        nameof(NameLabel), typeof(string), typeof(CdsAPIKeyModal), null,
        propertyChanged: static (b, _, _) => ((CdsAPIKeyModal)b).Sync());

    /// <summary>Bindable property for <see cref="NamePlaceholder"/>.</summary>
    public static readonly BindableProperty NamePlaceholderProperty = BindableProperty.Create(
        nameof(NamePlaceholder), typeof(string), typeof(CdsAPIKeyModal), null,
        propertyChanged: static (b, _, _) => ((CdsAPIKeyModal)b).Sync());

    /// <summary>Bindable property for <see cref="NameHelperText"/>.</summary>
    public static readonly BindableProperty NameHelperTextProperty = BindableProperty.Create(
        nameof(NameHelperText), typeof(string), typeof(CdsAPIKeyModal), null,
        propertyChanged: static (b, _, _) => ((CdsAPIKeyModal)b).Sync());

    /// <summary>Bindable property for <see cref="HasAPIKeyVisibilityToggle"/>.</summary>
    public static readonly BindableProperty HasAPIKeyVisibilityToggleProperty = BindableProperty.Create(
        nameof(HasAPIKeyVisibilityToggle), typeof(bool), typeof(CdsAPIKeyModal), false,
        propertyChanged: static (b, _, _) => ((CdsAPIKeyModal)b).Sync());

    /// <summary>Bindable property for <see cref="ShowAPIKeyLabel"/>.</summary>
    public static readonly BindableProperty ShowAPIKeyLabelProperty = BindableProperty.Create(
        nameof(ShowAPIKeyLabel), typeof(string), typeof(CdsAPIKeyModal), null,
        propertyChanged: static (b, _, _) => ((CdsAPIKeyModal)b).Sync());

    /// <summary>Bindable property for <see cref="HideAPIKeyLabel"/>.</summary>
    public static readonly BindableProperty HideAPIKeyLabelProperty = BindableProperty.Create(
        nameof(HideAPIKeyLabel), typeof(string), typeof(CdsAPIKeyModal), null,
        propertyChanged: static (b, _, _) => ((CdsAPIKeyModal)b).Sync());

    /// <summary>Bindable property for <see cref="HelperText"/>.</summary>
    public static readonly BindableProperty HelperTextProperty = BindableProperty.Create(
        nameof(HelperText), typeof(string), typeof(CdsAPIKeyModal), null,
        propertyChanged: static (b, _, _) => ((CdsAPIKeyModal)b).Sync());

    /// <summary>Bindable property for <see cref="HasDownloadLink"/>.</summary>
    public static readonly BindableProperty HasDownloadLinkProperty = BindableProperty.Create(
        nameof(HasDownloadLink), typeof(bool), typeof(CdsAPIKeyModal), false,
        propertyChanged: static (b, _, _) => ((CdsAPIKeyModal)b).Sync());

    /// <summary>Bindable property for <see cref="DownloadBodyText"/>.</summary>
    public static readonly BindableProperty DownloadBodyTextProperty = BindableProperty.Create(
        nameof(DownloadBodyText), typeof(string), typeof(CdsAPIKeyModal), null,
        propertyChanged: static (b, _, _) => ((CdsAPIKeyModal)b).Sync());

    /// <summary>Bindable property for <see cref="DownloadFileName"/>.</summary>
    public static readonly BindableProperty DownloadFileNameProperty = BindableProperty.Create(
        nameof(DownloadFileName), typeof(string), typeof(CdsAPIKeyModal), "apikey",
        propertyChanged: static (b, _, _) => ((CdsAPIKeyModal)b).Sync());

    /// <summary>Bindable property for <see cref="DownloadFileType"/>.</summary>
    public static readonly BindableProperty DownloadFileTypeProperty = BindableProperty.Create(
        nameof(DownloadFileType), typeof(CdsAPIKeyFileType), typeof(CdsAPIKeyModal), CdsAPIKeyFileType.Json,
        propertyChanged: static (b, _, _) => ((CdsAPIKeyModal)b).Sync());

    /// <summary>Bindable property for <see cref="DownloadLinkText"/>.</summary>
    public static readonly BindableProperty DownloadLinkTextProperty = BindableProperty.Create(
        nameof(DownloadLinkText), typeof(string), typeof(CdsAPIKeyModal), null,
        propertyChanged: static (b, _, _) => ((CdsAPIKeyModal)b).Sync());

    /// <summary>Bindable property for <see cref="DownloadLinkLabel"/>.</summary>
    public static readonly BindableProperty DownloadLinkLabelProperty = BindableProperty.Create(
        nameof(DownloadLinkLabel), typeof(string), typeof(CdsAPIKeyModal), null,
        propertyChanged: static (b, _, _) => ((CdsAPIKeyModal)b).Sync());

    /// <summary>Bindable property for <see cref="NextStepButtonText"/>.</summary>
    public static readonly BindableProperty NextStepButtonTextProperty = BindableProperty.Create(
        nameof(NextStepButtonText), typeof(string), typeof(CdsAPIKeyModal), null,
        propertyChanged: static (b, _, _) => ((CdsAPIKeyModal)b).Sync());

    /// <summary>Bindable property for <see cref="PreviousStepButtonText"/>.</summary>
    public static readonly BindableProperty PreviousStepButtonTextProperty = BindableProperty.Create(
        nameof(PreviousStepButtonText), typeof(string), typeof(CdsAPIKeyModal), null,
        propertyChanged: static (b, _, _) => ((CdsAPIKeyModal)b).Sync());

    /// <summary>Bindable property for <see cref="CurrentStep"/>.</summary>
    public static readonly BindableProperty CurrentStepProperty = BindableProperty.Create(
        nameof(CurrentStep), typeof(int), typeof(CdsAPIKeyModal), 0, BindingMode.TwoWay,
        propertyChanged: static (b, _, _) => ((CdsAPIKeyModal)b).Sync());

    /// <summary>Bindable property for <see cref="RequestGenerateCommand"/>.</summary>
    public static readonly BindableProperty RequestGenerateCommandProperty = BindableProperty.Create(
        nameof(RequestGenerateCommand), typeof(ICommand), typeof(CdsAPIKeyModal), null);

    /// <summary>Bindable property for <see cref="RequestEditCommand"/>.</summary>
    public static readonly BindableProperty RequestEditCommandProperty = BindableProperty.Create(
        nameof(RequestEditCommand), typeof(ICommand), typeof(CdsAPIKeyModal), null);

    /// <summary>Bindable property for <see cref="CopyCommand"/>.</summary>
    public static readonly BindableProperty CopyCommandProperty = BindableProperty.Create(
        nameof(CopyCommand), typeof(ICommand), typeof(CdsAPIKeyModal), null);

    /// <summary>The generated key (<c>apiKey</c>): set it when the request succeeds. It shows read-only, the primary button becomes Copy and focus moves to it.</summary>
    public string? ApiKey
    {
        get => (string?)GetValue(ApiKeyProperty);
        set => SetValue(ApiKeyProperty, value);
    }

    /// <summary>Label of the key field (<c>apiKeyLabel</c>): "API key".</summary>
    public string? ApiKeyLabel
    {
        get => (string?)GetValue(ApiKeyLabelProperty);
        set => SetValue(ApiKeyLabelProperty, value);
    }

    /// <summary>The key name put in the name field each time the modal opens (<c>apiKeyName</c>); supply it when editing.</summary>
    public string? ApiKeyName
    {
        get => (string?)GetValue(ApiKeyNameProperty);
        set => SetValue(ApiKeyNameProperty, value);
    }

    /// <summary>Optional message above the fields (<c>body</c>).</summary>
    public string? BodyText
    {
        get => (string?)GetValue(BodyTextProperty);
        set => SetValue(BodyTextProperty, value);
    }

    /// <summary>Small label above the title (<c>modalLabel</c>), also the dialog's name when there is no title: "Generate an API key".</summary>
    public string? ModalLabel
    {
        get => (string?)GetValue(ModalLabelProperty);
        set => SetValue(ModalLabelProperty, value);
    }

    /// <summary>The title (<c>generateTitle</c>): "Generate an API key". Custom steps show their own titles until the key is loaded.</summary>
    public string? GenerateTitle
    {
        get => (string?)GetValue(GenerateTitleProperty);
        set => SetValue(GenerateTitleProperty, value);
    }

    /// <summary>Primary button before the key exists (<c>generateButtonText</c>): "Generate API key".</summary>
    public string? GenerateButtonText
    {
        get => (string?)GetValue(GenerateButtonTextProperty);
        set => SetValue(GenerateButtonTextProperty, value);
    }

    /// <summary>Success line once the key is loaded (<c>generateSuccessMessage</c>): "API key successfully created".</summary>
    public string? GenerateSuccessMessage
    {
        get => (string?)GetValue(GenerateSuccessMessageProperty);
        set => SetValue(GenerateSuccessMessageProperty, value);
    }

    /// <summary>Information line once the key is loaded, when there is no download link (<c>generateSuccessBody</c>): "This is your unique API key and is non-recoverable."</summary>
    public string? GenerateSuccessBody
    {
        get => (string?)GetValue(GenerateSuccessBodyProperty);
        set => SetValue(GenerateSuccessBodyProperty, value);
    }

    /// <summary>Primary button once the key is loaded (<c>copyButtonText</c>): "Copy".</summary>
    public string? CopyButtonText
    {
        get => (string?)GetValue(CopyButtonTextProperty);
        set => SetValue(CopyButtonTextProperty, value);
    }

    /// <summary>Accessible name of the copy action (<c>copyIconDescription</c>); added to the primary button as its hint once the key is loaded.</summary>
    public string? CopyIconDescription
    {
        get => (string?)GetValue(CopyIconDescriptionProperty);
        set => SetValue(CopyIconDescriptionProperty, value);
    }

    /// <summary>Error line when copying to the clipboard fails (<c>copyErrorText</c>).</summary>
    public string? CopyErrorText
    {
        get => (string?)GetValue(CopyErrorTextProperty);
        set => SetValue(CopyErrorTextProperty, value);
    }

    /// <summary>Secondary button (<c>closeButtonText</c>), "Close"; it asks to close.</summary>
    public string? CloseButtonText
    {
        get => (string?)GetValue(CloseButtonTextProperty);
        set => SetValue(CloseButtonTextProperty, value);
    }

    /// <summary>Edit mode (<c>editing</c>): the name field shows, the primary button saves it (<see cref="CdsAPIKeyModal.RequestEdit"/>).</summary>
    public bool IsEditing
    {
        get => (bool)GetValue(IsEditingProperty);
        set => SetValue(IsEditingProperty, value);
    }

    /// <summary>Primary button in edit mode (<c>editButtonText</c>): "Save".</summary>
    public string? EditButtonText
    {
        get => (string?)GetValue(EditButtonTextProperty);
        set => SetValue(EditButtonTextProperty, value);
    }

    /// <summary>The edit was saved (<c>editSuccess</c>): shows <see cref="EditSuccessMessage"/>.</summary>
    public bool IsEditSuccess
    {
        get => (bool)GetValue(IsEditSuccessProperty);
        set => SetValue(IsEditSuccessProperty, value);
    }

    /// <summary>Success line after a saved edit (<c>editSuccessMessage</c>): "API key successfully saved".</summary>
    public string? EditSuccessMessage
    {
        get => (string?)GetValue(EditSuccessMessageProperty);
        set => SetValue(EditSuccessMessageProperty, value);
    }

    /// <summary>A request runs (<c>loading</c>): an inline loading with <see cref="LoadingText"/> shows, the name field and the primary button are disabled.</summary>
    public bool IsLoading
    {
        get => (bool)GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    /// <summary>Text of the inline loading (<c>loadingText</c>): "Generating..."</summary>
    public string? LoadingText
    {
        get => (string?)GetValue(LoadingTextProperty);
        set => SetValue(LoadingTextProperty, value);
    }

    /// <summary>The request failed (<c>error</c>): shows <see cref="ErrorText"/> as an alert.</summary>
    public bool IsError
    {
        get => (bool)GetValue(IsErrorProperty);
        set => SetValue(IsErrorProperty, value);
    }

    /// <summary>Error line of a failed request (<c>errorText</c>).</summary>
    public string? ErrorText
    {
        get => (string?)GetValue(ErrorTextProperty);
        set => SetValue(ErrorTextProperty, value);
    }

    /// <summary>The key needs a name (<c>nameRequired</c>): the name field shows and the primary button stays disabled until it is filled. Leave it off with custom steps, which validate themselves.</summary>
    public bool IsNameRequired
    {
        get => (bool)GetValue(IsNameRequiredProperty);
        set => SetValue(IsNameRequiredProperty, value);
    }

    /// <summary>Label of the name field (<c>nameLabel</c>): "Name of the API key".</summary>
    public string? NameLabel
    {
        get => (string?)GetValue(NameLabelProperty);
        set => SetValue(NameLabelProperty, value);
    }

    /// <summary>Placeholder of the name field (<c>namePlaceholder</c>).</summary>
    public string? NamePlaceholder
    {
        get => (string?)GetValue(NamePlaceholderProperty);
        set => SetValue(NamePlaceholderProperty, value);
    }

    /// <summary>Helper text of the name field (<c>nameHelperText</c>).</summary>
    public string? NameHelperText
    {
        get => (string?)GetValue(NameHelperTextProperty);
        set => SetValue(NameHelperTextProperty, value);
    }

    /// <summary>Hides the key behind dots with a show / hide toggle (<c>hasAPIKeyVisibilityToggle</c>); otherwise it shows as plain read-only text.</summary>
    public bool HasAPIKeyVisibilityToggle
    {
        get => (bool)GetValue(HasAPIKeyVisibilityToggleProperty);
        set => SetValue(HasAPIKeyVisibilityToggleProperty, value);
    }

    /// <summary>Tooltip and name of the show toggle (<c>showAPIKeyLabel</c>): "Show key".</summary>
    public string? ShowAPIKeyLabel
    {
        get => (string?)GetValue(ShowAPIKeyLabelProperty);
        set => SetValue(ShowAPIKeyLabelProperty, value);
    }

    /// <summary>Tooltip and name of the hide toggle (<c>hideAPIKeyLabel</c>): "Hide key".</summary>
    public string? HideAPIKeyLabel
    {
        get => (string?)GetValue(HideAPIKeyLabelProperty);
        set => SetValue(HideAPIKeyLabelProperty, value);
    }

    /// <summary>Helper text under the hidden key field (<c>helperText</c>).</summary>
    public string? HelperText
    {
        get => (string?)GetValue(HelperTextProperty);
        set => SetValue(HelperTextProperty, value);
    }

    /// <summary>Offers to save the key as a file once it is loaded (<c>hasDownloadLink</c>), through a <see cref="CdsAPIKeyDownloader"/> line.</summary>
    public bool HasDownloadLink
    {
        get => (bool)GetValue(HasDownloadLinkProperty);
        set => SetValue(HasDownloadLinkProperty, value);
    }

    /// <summary>The sentence before the download link (<c>downloadBodyText</c>).</summary>
    public string? DownloadBodyText
    {
        get => (string?)GetValue(DownloadBodyTextProperty);
        set => SetValue(DownloadBodyTextProperty, value);
    }

    /// <summary>The downloaded file name without extension (<c>downloadFileName</c>). Default <c>apikey</c>.</summary>
    public string? DownloadFileName
    {
        get => (string?)GetValue(DownloadFileNameProperty);
        set => SetValue(DownloadFileNameProperty, value);
    }

    /// <summary>The downloaded file format (<c>downloadFileType</c>): JSON (default) or text.</summary>
    public CdsAPIKeyFileType DownloadFileType
    {
        get => (CdsAPIKeyFileType)GetValue(DownloadFileTypeProperty);
        set => SetValue(DownloadFileTypeProperty, value);
    }

    /// <summary>The download link text (<c>downloadLinkText</c>): "Download as JSON".</summary>
    public string? DownloadLinkText
    {
        get => (string?)GetValue(DownloadLinkTextProperty);
        set => SetValue(DownloadLinkTextProperty, value);
    }

    /// <summary>The download link's accessible name (<c>downloadLinkLabel</c>); defaults to the link text.</summary>
    public string? DownloadLinkLabel
    {
        get => (string?)GetValue(DownloadLinkLabelProperty);
        set => SetValue(DownloadLinkLabelProperty, value);
    }

    /// <summary>Primary button on every custom step but the last (<c>nextStepButtonText</c>): "Next".</summary>
    public string? NextStepButtonText
    {
        get => (string?)GetValue(NextStepButtonTextProperty);
        set => SetValue(NextStepButtonTextProperty, value);
    }

    /// <summary>Secondary button after the first custom step (<c>previousStepButtonText</c>): "Back".</summary>
    public string? PreviousStepButtonText
    {
        get => (string?)GetValue(PreviousStepButtonTextProperty);
        set => SetValue(PreviousStepButtonTextProperty, value);
    }

    /// <summary>The custom step shown (two-way, zero-based); it goes back to 0 each time the modal opens.</summary>
    public int CurrentStep
    {
        get => (int)GetValue(CurrentStepProperty);
        set => SetValue(CurrentStepProperty, value);
    }

    /// <summary>Runs with the key name (a string) when Generate is activated, after <see cref="CdsAPIKeyModal.RequestGenerate"/>.</summary>
    public ICommand? RequestGenerateCommand
    {
        get => (ICommand?)GetValue(RequestGenerateCommandProperty);
        set => SetValue(RequestGenerateCommandProperty, value);
    }

    /// <summary>Runs with the key name (a string) when Save is activated in edit mode, after <see cref="CdsAPIKeyModal.RequestEdit"/>.</summary>
    public ICommand? RequestEditCommand
    {
        get => (ICommand?)GetValue(RequestEditCommandProperty);
        set => SetValue(RequestEditCommandProperty, value);
    }

    /// <summary>Copies the key (a string) instead of the built-in clipboard copy (<c>onCopy</c>).</summary>
    public ICommand? CopyCommand
    {
        get => (ICommand?)GetValue(CopyCommandProperty);
        set => SetValue(CopyCommandProperty, value);
    }
}

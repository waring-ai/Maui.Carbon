using System.ComponentModel;

namespace Maui.Carbon;

/// <summary>
/// Creates, edits or shows an API key (Carbon for IBM Products <c>APIKeyModal</c>). In the instant flow the key is
/// generated before the modal opens: set <see cref="ApiKey"/> and the modal shows it with Copy. In the standard flow the
/// person names the key (<see cref="IsNameRequired"/>) and activates Generate (<see cref="RequestGenerate"/>); show the wait
/// with <see cref="IsLoading"/>, then set <see cref="ApiKey"/> or <see cref="IsError"/>. <see cref="CustomSteps"/> gather more
/// than a name over several steps (Next and Back), and <see cref="IsEditing"/> renames an existing key. The key shows
/// read-only, hidden behind a toggle with <see cref="HasAPIKeyVisibilityToggle"/>; Copy puts it on the clipboard and
/// <see cref="HasDownloadLink"/> offers to save it as a file. Tell people the key cannot be shown again. The modal has no
/// close button and never closes on a tap outside; Close and Escape ask to close.
/// </summary>
[ContentProperty(nameof(CustomSteps))]
public partial class CdsAPIKeyModal : CdsProductModal
{
    readonly CdsProductModalHeader _header = new(CdsProductModalHeaderStyle.Standard) { ShowsCloseButton = false };
    readonly CdsModalFooter _footer = new();
    readonly ContentView _stepHost = new() { IsVisible = false };
    readonly CdsEndInset _bodyBox;
    readonly CdsText _bodyText;
    readonly CdsPasswordInput _keyHidden = new() { IsReadOnly = true, IsVisible = false };
    readonly CdsTextInput _keyShown = new() { IsReadOnly = true, IsVisible = false };
    readonly CdsTextInput _nameInput = new() { IsVisible = false };
    readonly CdsInlineLoading _loading = new() { IsVisible = false, MinimumHeightRequest = CdsSpacing.Spacing09 };
    readonly CdsEndInset _errorBox;
    readonly CdsText _errorText;
    readonly CdsEndInset _infoBox;
    readonly CdsText _infoText = new() { TypeStyle = CdsTypeStyle.Label01, LineBreakMode = LineBreakMode.WordWrap, VerticalOptions = LayoutOptions.Center };
    readonly CdsAPIKeyDownloader _downloader = new() { VerticalOptions = LayoutOptions.Center };
    readonly CdsEndInset _successBox;
    readonly CdsText _successText;
    readonly CdsItemCollection<CdsAPIKeyModalStep> _steps = [];
    string? _name;
    bool _copyError;
    bool _keyWasLoaded;
    string? _alerted;

    /// <summary>Creates a closed API key modal.</summary>
    public CdsAPIKeyModal() : base(CdsModalSize.Sm, preventCloseOnClickOutside: true)
    {
        (_bodyBox, _bodyText) = CdsProductModalParts.Paragraph(CdsTypeStyle.Body01, ProductModalModel.TwentyPercentLessSpacing05,
            new Thickness(0, 0, 0, CdsSpacing.Spacing05));
        (_errorBox, _errorText) = Message(CdsIconName.ErrorFilled, CdsProductModalTokens.ErrorIcon);
        (_successBox, _successText) = Message(CdsIconName.CheckmarkFilled, CdsProductModalTokens.SuccessIcon);

        // The information line: an icon, then the download link or the success body.
        var infoIcon = new CdsIcon(CdsIconName.InformationFilled, 16) { VerticalOptions = LayoutOptions.Start };
        CdsTokens.SetColor(infoIcon, CdsIcon.ColorProperty, CdsProductModalTokens.InformationIcon);
        var infoRow = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star) } };
        var infoContent = new Grid { Margin = new Thickness(CdsSpacing.Spacing03, 0, 0, 0) };
        infoContent.Add(_infoText);
        infoContent.Add(_downloader);
        infoRow.Add(infoIcon, 0);
        infoRow.Add(infoContent, 1);
        _infoBox = new CdsEndInset(infoRow, ProductModalModel.TwentyPercentLessSpacing05) { Margin = new Thickness(0, CdsSpacing.Spacing03, 0, 0), IsVisible = false };

        var stack = new VerticalStackLayout { Spacing = 0 };
        stack.Add(_stepHost);
        stack.Add(_bodyBox);
        stack.Add(_keyHidden);
        stack.Add(_keyShown);
        stack.Add(_nameInput);
        stack.Add(_loading);
        stack.Add(_errorBox);
        stack.Add(_infoBox);
        stack.Add(_successBox);

        _nameInput.TextChanged += (_, e) =>
        {
            _name = e.NewTextValue;
            Sync();
        };
        _nameInput.Completed += async (_, _) => await SubmitAsync();
        _footer.RequestSubmit += async (_, _) => await SubmitAsync();
        // The secondary button goes back a step or asks to close; it never closes on its own.
        _footer.SecondarySubmit += (_, _) => Back();
        _steps.CollectionChanged += (_, e) =>
        {
            if (e.OldItems is not null) foreach (CdsAPIKeyModalStep s in e.OldItems) s.PropertyChanged -= OnStepChanged;
            if (e.NewItems is not null)
            {
                foreach (CdsAPIKeyModalStep s in e.NewItems)
                {
                    s.PropertyChanged += OnStepChanged;
                    s.BindingContext = BindingContext;
                }
            }
            Sync();
        };
        SetParts(_header, new CdsModalBody { HasForm = true, Body = stack }, _footer);
        Sync();
    }

    /// <summary>Raised with the key name when Generate is activated (<c>onRequestGenerate</c>): set <see cref="IsLoading"/>, then <see cref="ApiKey"/> or <see cref="IsError"/>.</summary>
    public event EventHandler<CdsAPIKeyModalEventArgs>? RequestGenerate;

    /// <summary>Raised with the new name when Save is activated in edit mode (<c>onRequestEdit</c>): then set <see cref="IsEditSuccess"/> or <see cref="IsError"/>.</summary>
    public event EventHandler<CdsAPIKeyModalEventArgs>? RequestEdit;

    /// <summary>
    /// Raised with the key when Copy is activated (<c>onCopy</c>). With a handler (or <see cref="CopyCommand"/>) the
    /// modal does not copy itself; otherwise it puts the key on the clipboard and shows <see cref="CopyErrorText"/> if that fails.
    /// </summary>
    public event EventHandler<CdsAPIKeyModalEventArgs>? CopyRequested;

    /// <summary>The custom steps (<c>customSteps</c>), in order. Empty (the default) is the single name step.</summary>
    public IList<CdsAPIKeyModalStep> CustomSteps => _steps;

    /// <summary>The name typed so far (for tests).</summary>
    internal string? CurrentName => _name;

    /// <summary>The name field (for tests).</summary>
    internal CdsTextInput NameInput => _nameInput;

    /// <summary>The hidden key field (for tests).</summary>
    internal CdsPasswordInput HiddenKeyInput => _keyHidden;

    /// <summary>The plain key field (for tests).</summary>
    internal CdsTextInput ShownKeyInput => _keyShown;

    /// <summary>The download line (for tests).</summary>
    internal CdsAPIKeyDownloader Downloader => _downloader;

    /// <summary>Whether the last copy failed (for tests).</summary>
    internal bool HasCopyError => _copyError;

    private protected override VisualElement? DefaultInitialFocus
    {
        get
        {
            var s = Inputs();
            if (APIKeyModalModel.ShowsNameInput(s)) return _nameInput;
            return null;
        }
    }

    private protected override void OnOpening()
    {
        _name = ApiKeyName;
        _copyError = false;
        _keyWasLoaded = APIKeyModalModel.KeyLoaded(Inputs());
        CurrentStep = 0;
        Sync();
    }

    private protected override void OnClosed()
    {
        _name = string.Empty;
        CurrentStep = 0;
    }

    /// <inheritdoc/>
    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        foreach (var step in _steps) step.BindingContext = BindingContext;
    }

    /// <summary>Does what the primary button does: next step, copy, save or generate (for tests and the name field's Enter).</summary>
    internal async Task SubmitAsync()
    {
        var s = Inputs();
        if (APIKeyModalModel.IsPrimaryDisabled(s)) return;
        var name = _name ?? string.Empty;
        switch (APIKeyModalModel.PrimaryAction(s))
        {
            case APIKeyPrimaryAction.NextStep:
                CurrentStep = APIKeyModalModel.MoveStep(CurrentStep, 1, _steps.Count);
                break;
            case APIKeyPrimaryAction.Copy:
                await CopyAsync();
                break;
            case APIKeyPrimaryAction.Edit:
                RequestEdit?.Invoke(this, new CdsAPIKeyModalEventArgs(name));
                if (RequestEditCommand?.CanExecute(name) == true) RequestEditCommand.Execute(name);
                break;
            default:
                RequestGenerate?.Invoke(this, new CdsAPIKeyModalEventArgs(name));
                if (RequestGenerateCommand?.CanExecute(name) == true) RequestGenerateCommand.Execute(name);
                break;
        }
    }

    /// <summary>Does what the secondary button does: back a step, or ask to close.</summary>
    internal void Back()
    {
        if (APIKeyModalModel.SecondaryGoesBack(Inputs())) CurrentStep = APIKeyModalModel.MoveStep(CurrentStep, -1, _steps.Count);
        else RequestCloseFromChrome();
    }

    async Task CopyAsync()
    {
        var key = ApiKey ?? string.Empty;
        if (CopyRequested is not null || CopyCommand is not null)
        {
            CopyRequested?.Invoke(this, new CdsAPIKeyModalEventArgs(key));
            if (CopyCommand?.CanExecute(key) == true) CopyCommand.Execute(key);
            return;
        }
        try
        {
            await Clipboard.Default.SetTextAsync(key);
            _copyError = false;
        }
        catch (Exception ex) when (ex is NotImplementedException or FeatureNotSupportedException or PermissionException or InvalidOperationException or UnauthorizedAccessException)
        {
            _copyError = true;
        }
        Sync();
    }

    static (CdsEndInset Box, CdsText Text) Message(CdsIconName icon, string colorKey)
    {
        var (row, _, text) = CdsProductModalParts.MessageRow(icon, colorKey, CdsTypeStyle.Label01);
        row.IsVisible = true;
        var box = new CdsEndInset(row, ProductModalModel.TwentyPercentLessSpacing05) { Margin = new Thickness(0, CdsSpacing.Spacing03, 0, 0), IsVisible = false };
        return (box, text);
    }

    void OnStepChanged(object? sender, PropertyChangedEventArgs e) => Sync();

    void OnApiKeyNameChanged()
    {
        _name = ApiKeyName;
        Sync();
    }

    APIKeyModalInputs Inputs()
    {
        var step = CurrentStep >= 0 && CurrentStep < _steps.Count ? _steps[CurrentStep] : null;
        return new APIKeyModalInputs(ApiKey, IsLoading, IsEditing, IsEditSuccess, _steps.Count, CurrentStep, step?.IsValid,
            IsNameRequired, _name, HasAPIKeyVisibilityToggle, HasDownloadLink, _copyError, IsError);
    }

    void Sync()
    {
        var s = Inputs();
        var step = CurrentStep >= 0 && CurrentStep < _steps.Count ? _steps[CurrentStep] : null;
        var title = APIKeyModalModel.Title(s, GenerateTitle, step?.Title);
        _header.Title = title;
        _header.Label = ModalLabel;
        SetAccessibleName(string.IsNullOrEmpty(title) ? ModalLabel : title);

        var stepContent = APIKeyModalModel.ShowsStepContent(s);
        _stepHost.Content = stepContent ? step?.Content : null;
        _stepHost.IsVisible = stepContent;

        _bodyText.Text = BodyText;
        _bodyBox.IsVisible = !stepContent && !string.IsNullOrEmpty(BodyText);

        var showsKey = APIKeyModalModel.ShowsKey(s);
        _keyHidden.IsVisible = showsKey && HasAPIKeyVisibilityToggle;
        _keyShown.IsVisible = showsKey && !HasAPIKeyVisibilityToggle;
        _keyHidden.Text = ApiKey;
        _keyHidden.Label = ApiKeyLabel;
        _keyHidden.HelperText = HelperText;
        _keyHidden.ShowPasswordLabel = ShowAPIKeyLabel;
        _keyHidden.HidePasswordLabel = HideAPIKeyLabel;
        _keyShown.Text = ApiKey;
        _keyShown.Label = ApiKeyLabel;

        _nameInput.IsVisible = APIKeyModalModel.ShowsNameInput(s);
        _nameInput.Label = NameLabel;
        _nameInput.Placeholder = NamePlaceholder;
        _nameInput.HelperText = NameHelperText;
        _nameInput.IsEnabled = !IsLoading;
        if (_nameInput.Text != _name) _nameInput.Text = _name;

        _loading.IsVisible = !stepContent && IsLoading;
        _loading.Status = IsLoading ? CdsInlineLoadingStatus.Active : CdsInlineLoadingStatus.Inactive;
        _loading.Description = LoadingText;

        var error = _copyError ? CopyErrorText : ErrorText;
        _errorBox.IsVisible = APIKeyModalModel.ShowsError(s);
        _errorText.Text = error;

        var info = APIKeyModalModel.ShowsInformation(s);
        _infoBox.IsVisible = info && (HasDownloadLink || !string.IsNullOrEmpty(GenerateSuccessBody));
        _downloader.IsVisible = HasDownloadLink;
        _infoText.IsVisible = !HasDownloadLink;
        _infoText.Text = GenerateSuccessBody;
        _downloader.ApiKey = ApiKey;
        _downloader.BodyText = DownloadBodyText;
        _downloader.FileName = DownloadFileName;
        _downloader.FileType = DownloadFileType;
        _downloader.LinkText = DownloadLinkText;
        _downloader.LinkLabel = DownloadLinkLabel ?? DownloadLinkText;

        var success = APIKeyModalModel.SuccessMessage(s, EditSuccessMessage, GenerateSuccessMessage);
        _successBox.IsVisible = APIKeyModalModel.ShowsSuccess(s, success);
        _successText.Text = success;

        var action = APIKeyModalModel.PrimaryAction(s);
        _footer.PrimaryButtonText = action switch
        {
            APIKeyPrimaryAction.NextStep => NextStepButtonText,
            APIKeyPrimaryAction.Copy => CopyButtonText,
            APIKeyPrimaryAction.Edit => EditButtonText,
            _ => GenerateButtonText,
        };
        _footer.SecondaryButtonText = APIKeyModalModel.SecondaryGoesBack(s) ? PreviousStepButtonText : CloseButtonText;
        _footer.PrimaryButtonDisabled = APIKeyModalModel.IsPrimaryDisabled(s);
        if (_footer.Buttons.LastOrDefault() is CdsModalButton primary)
            SemanticProperties.SetHint(primary.HitTarget, action == APIKeyPrimaryAction.Copy ? CopyIconDescription : null);

        AnnounceAlerts(error, success);
        var loaded = APIKeyModalModel.KeyLoaded(s);
        // Focus moves to Copy once the key arrives (after the footer has its new button on screen).
        if (loaded && !_keyWasLoaded && IsShowing && IsLoaded && _footer.InitialFocus(false) is { } copy)
            Dispatcher.Dispatch(() => FocusWhileOpen(copy));
        _keyWasLoaded = loaded;
    }

    // The error and success lines are assertive alerts: each new one is spoken once while the modal is shown.
    void AnnounceAlerts(string? error, string? success)
    {
        var text = _errorBox.IsVisible ? error : _successBox.IsVisible ? success : null;
        if (text == _alerted) return;
        _alerted = text;
        if (IsShowing && IsLoaded && !string.IsNullOrEmpty(text)) CdsAnnouncer.Announce(text, CdsAnnouncePriority.Assertive);
    }
}

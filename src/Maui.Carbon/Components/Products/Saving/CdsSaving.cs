namespace Maui.Carbon;

/// <summary>
/// Tells people whether their work is saved (Carbon for IBM Products' Saving). <see cref="CdsSavingType.Auto"/> shows a
/// status line ("Saving...", "Saved", an error icon and "Failed to save"); <see cref="CdsSavingType.Manual"/> shows a
/// cancel button, enabled only while a save is in progress, and a primary save button that carries the status text and
/// icon and is disabled while saving. Drive <see cref="Status"/> from your save logic: handle <see cref="RequestSave"/>,
/// set <see cref="CdsSavingStatus.InProgress"/>, then <see cref="CdsSavingStatus.Success"/> or
/// <see cref="CdsSavingStatus.Fail"/>.
/// </summary>
public partial class CdsSaving : CdsControl
{
    readonly Grid _root = new() { HorizontalOptions = LayoutOptions.Start };
    HorizontalStackLayout? _line;
    CdsIcon? _errorIcon;
    CdsText? _text;
    HorizontalStackLayout? _buttons;
    CdsButton? _cancel;
    CdsButton? _save;
    CdsSavingStatus _announced;

    /// <summary>Creates an auto-save status line in the default state.</summary>
    public CdsSaving()
    {
        HorizontalOptions = LayoutOptions.Start;
        VerticalOptions = LayoutOptions.Center;
        SetChrome(_root);
        UpdateLayout();
    }

    /// <summary>Raised when the save button is activated (manual): start the save and set <see cref="Status"/>.</summary>
    public event EventHandler? RequestSave;

    /// <summary>Raised when the cancel button is activated while a save is in progress (manual): stop the save.</summary>
    public event EventHandler? RequestCancel;

    /// <summary>What the current status shows.</summary>
    public CdsSavingVisuals CurrentVisuals => CdsSavingTokens.Resolve(Status);

    /// <summary>The text of the current status.</summary>
    public string? CurrentText => CdsSavingTokens.Text(Status, DefaultText, InProgressText, SuccessText, FailText);

    /// <inheritdoc/>
    protected override void ApplyVisuals()
    {
        if (_errorIcon is not null) SetColor(_errorIcon, CdsIcon.ColorProperty, CdsSavingTokens.ErrorIconColor);
        if (_text is not null) SetColor(_text, Label.TextColorProperty, CdsSavingTokens.TextColor);
    }

    void UpdateLayout()
    {
        if (Type == CdsSavingType.Auto) EnsureLine();
        else EnsureButtons();
        if (_line is not null) _line.IsVisible = Type == CdsSavingType.Auto;
        if (_buttons is not null) _buttons.IsVisible = Type == CdsSavingType.Manual;
        UpdateContent();
    }

    void EnsureLine()
    {
        if (_line is not null) return;
        _errorIcon = new CdsIcon(CdsIconName.ErrorFilled, 16) { Margin = new Thickness(0, 0, CdsSavingTokens.ErrorIconGap, 0) };
        _text = new CdsText { TypeStyle = CdsTypeStyle.BodyCompact01, VerticalOptions = LayoutOptions.Center };
        _line = new HorizontalStackLayout { Spacing = 0 };
        _line.Add(_errorIcon);
        _line.Add(_text);
        _root.Add(_line);
    }

    void EnsureButtons()
    {
        if (_buttons is not null) return;
        _cancel = new CdsButton { Kind = CdsButtonKind.Secondary };
        _save = new CdsButton { Kind = CdsButtonKind.Primary };
        _cancel.Clicked += (_, _) => RequestCancel?.Invoke(this, EventArgs.Empty);
        _save.Clicked += (_, _) => RequestSave?.Invoke(this, EventArgs.Empty);
        _buttons = new HorizontalStackLayout { Spacing = 0 };
        _buttons.Add(_cancel);
        _buttons.Add(_save);
        _root.Add(_buttons);
    }

    void UpdateContent()
    {
        var v = CurrentVisuals;
        var text = CurrentText;
        if (_line is not null && _text is not null && _errorIcon is not null)
        {
            _errorIcon.IsVisible = v.ShowsErrorIcon;
            _errorIcon.Description = v.ShowsErrorIcon ? FailIconDescription : null;
            _text.Text = text;
            _line.IsVisible = Type == CdsSavingType.Auto;
        }
        if (_cancel is not null && _save is not null)
        {
            _cancel.Text = SecondaryButtonText;
            _cancel.IsEnabled = v.IsCancelEnabled;
            _cancel.Command = CancelCommand;
            _cancel.CommandParameter = CancelCommandParameter;
            _save.Text = text;
            _save.Icon = v.ButtonIcon;
            _save.IconDescription = IconDescription(Status);
            _save.IsEnabled = v.IsSaveEnabled;
            _save.Command = SaveCommand;
            _save.CommandParameter = SaveCommandParameter;
        }
        RefreshVisuals();
    }

    string? IconDescription(CdsSavingStatus status) => status switch
    {
        CdsSavingStatus.InProgress => InProgressIconDescription,
        CdsSavingStatus.Success => SuccessIconDescription,
        CdsSavingStatus.Fail => FailIconDescription,
        _ => DefaultIconDescription,
    };

    void OnStatusChanged()
    {
        UpdateContent();
        Announce();
    }

    // The status changes on its own (no focus moves), so it is a live region: polite, a failure assertive.
    void Announce()
    {
        if (Status == _announced) return;
        _announced = Status;
        if (!IsLoaded || !CdsDeck.IsShown(this)) return;
        var priority = Status == CdsSavingStatus.Fail ? CdsAnnouncePriority.Assertive : CdsAnnouncePriority.Polite;
        CdsAnnouncer.Announce(CurrentText, priority);
    }

    static void OnLayoutChanged(BindableObject b, object? o, object? n) => ((CdsSaving)b).UpdateLayout();

    static void OnContentChanged(BindableObject b, object? o, object? n) => ((CdsSaving)b).UpdateContent();
}

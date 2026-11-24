namespace Maui.Carbon;

/// <summary>
/// A dialog over a scrim that blocks the page until the user responds. Declare it anywhere in a page (it draws nothing
/// inline); it shows its <see cref="CdsContainer.Body"/> in the page's overlay host while <see cref="IsOpen"/> is true.
/// The all-in-one form: heading, body and optional footer buttons. Use <see cref="CdsComposedModal"/> for custom layouts.
/// Use it to confirm a decision (<see cref="IsDanger"/> for destructive ones), ask for a small input that blocks the
/// task, or show urgent information (<see cref="IsPassive"/>). Open it only from a user action.
/// </summary>
public partial class CdsModal : CdsContainer
{
    readonly CdsModalPresenter _presenter;
    CdsModalHeader? _header;
    CdsModalBody? _bodyPart;
    CdsModalFooter? _footer;
    CdsModalSurface? _surface;
    bool _openedOnLoad;

    /// <summary>Creates a closed modal.</summary>
    public CdsModal()
    {
        HorizontalOptions = LayoutOptions.Start;
        VerticalOptions = LayoutOptions.Start;
        WidthRequest = 0;
        HeightRequest = 0;
        InputTransparent = true;
        AutomationProperties.SetIsInAccessibleTree(this, false);
        SetChrome(new ContentView());
        _presenter = new CdsModalPresenter(this);
        _presenter.Released += (_, _) =>
        {
            if (_bodyPart is not null) _bodyPart.Body = null;
            if (_footer is not null) _footer.Body = null;
            _header = null;
            _bodyPart = null;
            _footer = null;
            _surface = null;
        };
        _presenter.Dismissed += (_, _) => { if (IsOpen) IsOpen = false; };
        Loaded += (_, _) =>
        {
            if (!IsOpen || _presenter.IsShown) return;
            _openedOnLoad = true;
            Show();
        };
        Unloaded += (_, _) => _presenter.Hide();
    }

    /// <summary>Raised when the primary button is activated. The modal stays open: validate, then set <see cref="IsOpen"/> to false.</summary>
    public event EventHandler? RequestSubmit;

    /// <summary>
    /// Raised when the modal is asked to close: close button, Escape, the secondary button (unless
    /// <see cref="SecondarySubmit"/> has a handler) or a scrim tap on a passive modal. Set
    /// <see cref="System.ComponentModel.CancelEventArgs.Cancel"/> to keep it open; otherwise
    /// <see cref="RequestCloseCommand"/> runs and <see cref="IsOpen"/> becomes false. Not raised while
    /// <see cref="LoadingStatus"/> is not <see cref="CdsInlineLoadingStatus.Inactive"/>: the submit is in flight, so the
    /// close button is disabled and Escape and the scrim do nothing.
    /// </summary>
    public event EventHandler<CdsModalRequestCloseEventArgs>? RequestClose;

    /// <summary>Raised when the secondary button is activated; its handlers replace the default close.</summary>
    public event EventHandler? SecondarySubmit;

    /// <summary>Raised when the optional third button is activated.</summary>
    public event EventHandler? TertiarySubmit;

    /// <summary>
    /// Raised when the primary button's inline loading reports success (<c>onLoadingSuccess</c>): the success delay after
    /// <see cref="LoadingStatus"/> became <see cref="CdsInlineLoadingStatus.Finished"/>. Close the modal here.
    /// </summary>
    public event EventHandler? LoadingSuccess;

    /// <inheritdoc/>
    protected override void OnBodyChanged(View? oldBody, View? newBody)
    {
        if (_bodyPart is not null) _bodyPart.Body = newBody;
    }

    /// <inheritdoc/>
    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        if (_surface is not null) _surface.BindingContext = BindingContext;
    }

    void OnIsOpenChanged(bool open)
    {
        if (open)
        {
            if (IsLoaded) Show();
        }
        else
        {
            _presenter.Hide();
        }
    }

    void Reshow()
    {
        if (!_presenter.IsShown) return;
        _presenter.Hide();
        if (IsOpen) Show();
    }

    void SyncParts()
    {
        if (_header is not null)
        {
            _header.Label = Label;
            _header.Title = Heading;
            _header.IconDescription = CloseButtonLabel;
            _header.IsCloseEnabled = !IsLoading;
        }
        if (_bodyPart is not null)
        {
            _bodyPart.HasForm = HasForm;
            _bodyPart.IsFullWidth = IsFullWidth;
            _bodyPart.HasScrollingContent = HasScrollingContent;
        }
        if (_footer is not null)
        {
            _footer.PrimaryButtonText = PrimaryButtonText;
            _footer.SecondaryButtonText = SecondaryButtonText;
            _footer.TertiaryButtonText = TertiaryButtonText;
            _footer.IsDanger = IsDanger;
            _footer.PrimaryButtonDisabled = PrimaryButtonDisabled;
            _footer.LoadingStatus = LoadingStatus;
            _footer.LoadingDescription = LoadingDescription;
            _footer.LoadingIconDescription = LoadingIconDescription;
        }
        _surface?.SetName(Heading);
        _surface?.SetDescription(IsAlert ? CdsModalAlert.Describe(Body) : null);
    }

    /// <summary>The shown surface, while the modal is open (for tests).</summary>
    internal CdsModalSurface? Surface => _surface;

    internal void Show()
    {
        if (_presenter.IsShown) return;

        _header = new CdsModalHeader { LabelSpacing = CdsSpacing.Spacing02 };
        _header.CloseButtonClicked += (_, _) => TryClose();
        _bodyPart = new CdsModalBody { Body = Body };
        _footer = null;
        if (!IsPassive)
        {
            _footer = new CdsModalFooter();
            _footer.RequestSubmit += (_, _) =>
            {
                RequestSubmit?.Invoke(this, EventArgs.Empty);
                if (RequestSubmitCommand?.CanExecute(null) == true) RequestSubmitCommand.Execute(null);
            };
            _footer.SecondarySubmit += (_, _) =>
            {
                if (SecondarySubmit is not null) SecondarySubmit.Invoke(this, EventArgs.Empty);
                else TryClose();
            };
            _footer.TertiarySubmit += (_, _) => TertiarySubmit?.Invoke(this, EventArgs.Empty);
            _footer.LoadingSuccess += (_, _) => LoadingSuccess?.Invoke(this, EventArgs.Empty);
        }
        SyncParts();

        _surface = new CdsModalSurface(CdsModalMode.Modal, Size, false, _header, _bodyPart, _footer) { BindingContext = BindingContext };
        _surface.SetName(Heading);
        var alert = IsAlert ? CdsModalAlert.Describe(Body) : null;
        _surface.SetDescription(alert);
        _surface.BackdropTapped += (_, _) =>
        {
            if (IsPassive && !PreventCloseOnClickOutside) TryClose();
        };

        var focus = _openedOnLoad ? null : CdsModalFocus.Resolve(InitialFocusElement, IsDanger, _header, _bodyPart, _footer);
        _openedOnLoad = false;
        _presenter.Show(_surface, new CdsOverlayOptions
        {
            IsModal = true,
            LightDismiss = false,
            CloseOnEscape = false,
            ReturnFocusTo = LauncherElement,
            KeyHandler = e =>
            {
                if (e.Key != CdsKey.Escape) return false;
                TryClose();
                return true;
            },
        }, focus);
        if (IsAlert) CdsModalAlert.Announce(Heading, alert);
    }

    /// <summary>True while <see cref="LoadingStatus"/> is not Inactive: the submit is in flight and the modal cannot be closed from its chrome.</summary>
    internal bool IsLoading => LoadingStatus != CdsInlineLoadingStatus.Inactive;

    /// <summary>
    /// Asks to close from the chrome (close button, Escape, secondary button, scrim): refused while loading or when a
    /// <see cref="RequestClose"/> handler cancels; otherwise runs <see cref="RequestCloseCommand"/> and closes.
    /// </summary>
    internal bool TryClose()
    {
        if (IsLoading) return false;
        var args = new CdsModalRequestCloseEventArgs();
        RequestClose?.Invoke(this, args);
        if (args.Cancel) return false;
        if (RequestCloseCommand?.CanExecute(null) == true) RequestCloseCommand.Execute(null);
        IsOpen = false;
        return true;
    }
}

/// <summary>Chooses the element focused when a modal opens (the Carbon order).</summary>
internal static class CdsModalFocus
{
    /// <summary>Explicit element, else Cancel in a danger modal, else the first enabled field, else the primary button, else the close button.</summary>
    public static VisualElement? Resolve(VisualElement? explicitTarget, bool danger, CdsModalHeader? header, CdsModalBody? body, CdsModalFooter? footer)
    {
        if (explicitTarget is not null) return explicitTarget;
        if (danger && footer?.InitialFocus(true) is { } cancel) return cancel;
        if (body?.FirstFocusable() is { } field) return field;
        if (footer?.InitialFocus(false) is { } primary) return primary;
        return header?.CloseTarget;
    }
}

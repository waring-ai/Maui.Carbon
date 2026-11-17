using System.Windows.Input;

namespace Maui.Carbon;

/// <summary>
/// Carbon's newer dialog primitive. With <see cref="IsModal"/> it behaves like <see cref="CdsModal"/> (scrim, page blocked,
/// a tap on the scrim requests close); without it, the dialog is a non-modal panel docked at the top end of the page
/// that leaves the page usable (find and replace, contextual help). Declare it in a page (it draws nothing inline); it
/// shows while <see cref="IsOpen"/> is true. The parent owns the state: close from <see cref="RequestClose"/>, or leave the
/// two-way <see cref="IsOpen"/> to do it. Passive dialogs have no footer, transactional ones Cancel plus one primary
/// action, danger ones a destructive primary, acknowledgment dialogs a single button.
/// </summary>
[ContentProperty(nameof(Body))]
public partial class CdsDialog : CdsContainer
{
    readonly CdsModalPresenter _presenter;
    CdsModalHeader? _header;
    CdsModalBody? _bodyPart;
    CdsModalFooter? _footer;
    CdsModalSurface? _surface;
    bool _openedOnLoad;

    /// <summary>Creates a closed non-modal dialog.</summary>
    public CdsDialog()
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

    /// <summary>Raised when the primary button is activated. The dialog stays open until you close it.</summary>
    public event EventHandler? RequestSubmit;

    /// <summary>
    /// Raised when the dialog is asked to close: close button, Escape, the secondary button (unless
    /// <see cref="SecondarySubmit"/> has a handler) or a scrim tap on a modal dialog. Set
    /// <see cref="System.ComponentModel.CancelEventArgs.Cancel"/> to keep it open; otherwise <see cref="IsOpen"/> becomes false.
    /// </summary>
    public event EventHandler<CdsModalRequestCloseEventArgs>? RequestClose;

    /// <summary>Raised when the secondary button is activated; its handlers replace the default close.</summary>
    public event EventHandler? SecondarySubmit;

    /// <summary>Raised when the optional third button is activated.</summary>
    public event EventHandler? TertiarySubmit;

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
            _header.Label = Subtitle;
            _header.Title = Title;
            _header.IconDescription = CloseButtonLabel;
        }
        if (_bodyPart is not null) _bodyPart.HasScrollingContent = HasScrollingContent;
        if (_footer is not null && Footer is null)
        {
            _footer.PrimaryButtonText = PrimaryButtonText;
            _footer.SecondaryButtonText = SecondaryButtonText;
            _footer.TertiaryButtonText = TertiaryButtonText;
            _footer.IsDanger = IsDanger;
            _footer.PrimaryButtonDisabled = PrimaryButtonDisabled;
        }
        _surface?.SetName(Title);
    }

    void Show()
    {
        if (_presenter.IsShown) return;

        _header = new CdsModalHeader { LabelSpacing = CdsSpacing.Spacing01 };
        _header.CloseButtonClicked += (_, _) => TryClose();
        // A dialog's body is not narrowed to 80%: it takes the full measure, with the 8 / 16 / 48 padding.
        _bodyPart = new CdsModalBody { Body = Body, HasForm = true };
        _footer = null;
        if (!IsPassive)
        {
            _footer = new CdsModalFooter { Body = Footer };
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
        }
        SyncParts();

        var mode = IsModal ? CdsModalMode.Modal : CdsModalMode.Floating;
        _surface = new CdsModalSurface(mode, Size, true, _header, _bodyPart, _footer) { BindingContext = BindingContext };
        _surface.SetName(Title);
        if (IsModal) _surface.BackdropTapped += (_, _) => TryClose();

        var focus = _openedOnLoad ? null : CdsModalFocus.Resolve(InitialFocusElement, IsDanger, _header, _bodyPart, _footer);
        _openedOnLoad = false;
        _presenter.Show(_surface, new CdsOverlayOptions
        {
            IsModal = IsModal,
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
    }

    void TryClose()
    {
        var args = new CdsModalRequestCloseEventArgs();
        RequestClose?.Invoke(this, args);
        if (args.Cancel) return;
        if (RequestCloseCommand?.CanExecute(null) == true) RequestCloseCommand.Execute(null);
        IsOpen = false;
    }
}

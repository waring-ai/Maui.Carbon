namespace Maui.Carbon;

/// <summary>
/// The composable form of <see cref="CdsModal"/>: put a <see cref="CdsModalHeader"/>, a <see cref="CdsModalBody"/> and a
/// <see cref="CdsModalFooter"/> in <see cref="Parts"/> to build custom layouts (a progress modal, a form with its own
/// footer buttons). Declare it in a page; it draws nothing inline and shows its parts while <see cref="IsOpen"/> is true.
/// The header close button, Escape and the footer's secondary button raise <see cref="RequestClose"/>; a scrim tap does
/// too, but only on a passive modal (one without footer buttons), so a form is never lost to a stray click outside.
/// </summary>
[ContentProperty(nameof(Parts))]
public class CdsComposedModal : CdsControl
{
    /// <summary>Bindable property for <see cref="IsOpen"/>.</summary>
    public static readonly BindableProperty IsOpenProperty = BindableProperty.Create(
        nameof(IsOpen), typeof(bool), typeof(CdsComposedModal), false, BindingMode.TwoWay,
        propertyChanged: static (b, _, n) => ((CdsComposedModal)b).OnIsOpenChanged((bool)n));

    /// <summary>Bindable property for <see cref="Size"/>.</summary>
    public static readonly BindableProperty SizeProperty = BindableProperty.Create(
        nameof(Size), typeof(CdsModalSize), typeof(CdsComposedModal), CdsModalSize.Md);

    /// <summary>Bindable property for <see cref="IsDanger"/>.</summary>
    public static readonly BindableProperty IsDangerProperty = BindableProperty.Create(
        nameof(IsDanger), typeof(bool), typeof(CdsComposedModal), false);

    /// <summary>Bindable property for <see cref="PreventCloseOnClickOutside"/>.</summary>
    public static readonly BindableProperty PreventCloseOnClickOutsideProperty = BindableProperty.Create(
        nameof(PreventCloseOnClickOutside), typeof(bool), typeof(CdsComposedModal), false);

    /// <summary>Bindable property for <see cref="LauncherElement"/>.</summary>
    public static readonly BindableProperty LauncherElementProperty = BindableProperty.Create(
        nameof(LauncherElement), typeof(VisualElement), typeof(CdsComposedModal), null);

    /// <summary>Bindable property for <see cref="InitialFocusElement"/>.</summary>
    public static readonly BindableProperty InitialFocusElementProperty = BindableProperty.Create(
        nameof(InitialFocusElement), typeof(VisualElement), typeof(CdsComposedModal), null);

    readonly CdsModalPresenter _presenter;
    readonly List<View> _parts = [];
    CdsModalSurface? _surface;
    Action? _unwire;
    bool _openedOnLoad;

    /// <summary>Creates a closed composed modal.</summary>
    public CdsComposedModal()
    {
        HorizontalOptions = LayoutOptions.Start;
        VerticalOptions = LayoutOptions.Start;
        WidthRequest = 0;
        HeightRequest = 0;
        InputTransparent = true;
        AutomationProperties.SetIsInAccessibleTree(this, false);
        SetChrome(new ContentView());
        _presenter = new CdsModalPresenter(this);
        _presenter.Dismissed += (_, _) => { Unwire(); if (IsOpen) IsOpen = false; };
        Loaded += (_, _) =>
        {
            if (!IsOpen || _presenter.IsShown) return;
            _openedOnLoad = true;
            Show();
        };
        Unloaded += (_, _) => { _presenter.Hide(); Unwire(); };
    }

    /// <summary>
    /// Raised when the modal is asked to close (close button, Escape, the footer's secondary button, a scrim tap on a passive modal). Set
    /// <see cref="System.ComponentModel.CancelEventArgs.Cancel"/> to keep it open; otherwise <see cref="IsOpen"/> becomes false.
    /// </summary>
    public event EventHandler<CdsModalRequestCloseEventArgs>? RequestClose;

    /// <summary>The header, body and footer (in any order); other views are placed in a body.</summary>
    public IList<View> Parts => _parts;

    /// <summary>Open state (two-way).</summary>
    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    /// <summary>Width step. Default <see cref="CdsModalSize.Md"/>.</summary>
    public CdsModalSize Size
    {
        get => (CdsModalSize)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    /// <summary>Danger confirmation: the footer's primary button is a danger button and focus starts on Cancel.</summary>
    public bool IsDanger
    {
        get => (bool)GetValue(IsDangerProperty);
        set => SetValue(IsDangerProperty, value);
    }

    /// <summary>
    /// Keep a passive modal (no footer buttons) open when the scrim is tapped. A transactional modal (one with footer
    /// buttons) never closes on a scrim tap; a passive one does unless this is set.
    /// </summary>
    public bool PreventCloseOnClickOutside
    {
        get => (bool)GetValue(PreventCloseOnClickOutsideProperty);
        set => SetValue(PreventCloseOnClickOutsideProperty, value);
    }

    /// <summary>The element that opened the modal; focus returns to it on close.</summary>
    public VisualElement? LauncherElement
    {
        get => (VisualElement?)GetValue(LauncherElementProperty);
        set => SetValue(LauncherElementProperty, value);
    }

    /// <summary>The element that takes focus when the modal opens.</summary>
    public VisualElement? InitialFocusElement
    {
        get => (VisualElement?)GetValue(InitialFocusElementProperty);
        set => SetValue(InitialFocusElementProperty, value);
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
            Unwire();
        }
    }

    internal void Show()
    {
        if (_presenter.IsShown) return;

        CdsModalHeader? header = null;
        CdsModalBody? body = null;
        CdsModalFooter? footer = null;
        List<View> others = [];
        foreach (var part in _parts)
        {
            switch (part)
            {
                case CdsModalHeader h when header is null: header = h; break;
                case CdsModalBody b when body is null: body = b; break;
                case CdsModalFooter f when footer is null: footer = f; break;
                default: others.Add(part); break;
            }
        }
        if (body is null && others.Count > 0)
        {
            View content = others.Count == 1 ? others[0] : new VerticalStackLayout();
            if (others.Count > 1) foreach (var v in others) ((VerticalStackLayout)content).Add(v);
            body = new CdsModalBody { Body = content };
        }

        if (footer is not null && IsDanger) footer.IsDanger = true;
        void OnHeaderClose(object? s, EventArgs e) => TryClose();
        void OnFooterClose(object? s, EventArgs e) => TryClose();
        if (header is not null) header.CloseButtonClicked += OnHeaderClose;
        if (footer is not null) footer.RequestClose += OnFooterClose;
        var wiredHeader = header;
        var wiredFooter = footer;
        _unwire = () =>
        {
            if (wiredHeader is not null) wiredHeader.CloseButtonClicked -= OnHeaderClose;
            if (wiredFooter is not null) wiredFooter.RequestClose -= OnFooterClose;
        };

        _surface = new CdsModalSurface(CdsModalMode.Modal, Size, false, header, body, footer) { BindingContext = BindingContext };
        _surface.SetName(header?.Title);
        _surface.BackdropTapped += (_, _) =>
        {
            if (ClosesOnScrimTap(footer, PreventCloseOnClickOutside)) TryClose();
        };

        var focus = _openedOnLoad ? null : CdsModalFocus.Resolve(InitialFocusElement, IsDanger || footer?.IsDanger == true, header, body, footer);
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
    }

    /// <summary>
    /// Carbon: a transactional modal does not close on a scrim tap; a passive one (no footer buttons) does unless
    /// <c>preventCloseOnClickOutside</c>. Read at tap time, since footer buttons can come and go while it is open.
    /// </summary>
    internal static bool ClosesOnScrimTap(CdsModalFooter? footer, bool preventCloseOnClickOutside) =>
        !preventCloseOnClickOutside && footer is not { HasContent: true };

    void Unwire()
    {
        _unwire?.Invoke();
        _unwire = null;
        _surface = null;
    }

    void TryClose()
    {
        var args = new CdsModalRequestCloseEventArgs();
        RequestClose?.Invoke(this, args);
        if (args.Cancel) return;
        IsOpen = false;
    }
}

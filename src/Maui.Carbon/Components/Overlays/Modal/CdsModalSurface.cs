using Microsoft.Maui.Controls.Shapes;

namespace Maui.Carbon;

/// <summary>How a modal surface sits in the page host.</summary>
internal enum CdsModalMode
{
    /// <summary>Centered over the page host's scrim; the area around the container reports taps.</summary>
    Modal,

    /// <summary>A bare container with a fixed width, shown in the host's non-blocking region (non-modal dialog).</summary>
    Floating,
}

/// <summary>
/// The container shared by <see cref="CdsModal"/>, <see cref="CdsComposedModal"/> and <see cref="CdsDialog"/>: a
/// <c>layer-01</c> box with a 1px <c>border-subtle-01</c> border, square, no shadow, holding header, scrolling body and
/// footer. In modal mode it fills the overlay and reports taps outside the container through <see cref="BackdropTapped"/>.
/// </summary>
internal sealed class CdsModalSurface : Grid
{
    readonly CdsModalPanel _panel = new();
    readonly Border _box;
    readonly CdsModalMode _mode;
    readonly bool _dialogSizing;
    CdsOverlayHost? _host;

    public CdsModalSurface(CdsModalMode mode, CdsModalSize size, bool dialogSizing, CdsModalHeader? header, CdsModalBody? body, CdsModalFooter? footer)
    {
        _mode = mode;
        Size = size;
        _dialogSizing = dialogSizing;
        Header = header;
        BodyPart = body;
        Footer = footer is { HasContent: true } ? footer : null;

        _box = new Border { StrokeThickness = 1, StrokeShape = new Rectangle(), Padding = 0, Content = _panel };
        CdsTokens.SetBrush(_box, Border.StrokeProperty, CdsModalTokens.ContainerBorder);
        CdsTokens.SetColor(_box, BackgroundColorProperty, CdsModalTokens.ContainerFill);
        _panel.SetParts(header, body, Footer);

        if (mode == CdsModalMode.Modal)
        {
            HorizontalOptions = LayoutOptions.Fill;
            VerticalOptions = LayoutOptions.Fill;
            var backdrop = new ContentView { BackgroundColor = Colors.Transparent };
            var tap = new TapGestureRecognizer();
            tap.Tapped += (_, _) => BackdropTapped?.Invoke(this, EventArgs.Empty);
            backdrop.GestureRecognizers.Add(tap);
            AutomationProperties.SetIsInAccessibleTree(backdrop, false);
            Children.Add(backdrop);
            _box.HorizontalOptions = LayoutOptions.Center;
            _box.VerticalOptions = LayoutOptions.Center;
            Children.Add(_box);
        }
        else
        {
            HorizontalOptions = LayoutOptions.End;
            VerticalOptions = LayoutOptions.Start;
            Children.Add(_box);
        }
    }

    public CdsModalSize Size { get; set; }

    public CdsModalHeader? Header { get; }

    public CdsModalBody? BodyPart { get; }

    public CdsModalFooter? Footer { get; }

    /// <summary>Raised when the area around the container is tapped (modal mode).</summary>
    public event EventHandler? BackdropTapped;

    /// <summary>Gives the container its accessible name.</summary>
    public void SetName(string? name) => CdsSemantics.SetGroupName(_box, name);

    /// <summary>Follows the size of the page host.</summary>
    public void Attach(CdsOverlayHost? host)
    {
        _host = host;
        if (host is null) return;
        host.SizeChanged += OnHostSizeChanged;
        ApplyViewport();
    }

    /// <summary>Stops following the host and hands the parts back so they can be shown again.</summary>
    public void Release()
    {
        if (_host is not null) _host.SizeChanged -= OnHostSizeChanged;
        _host = null;
        _panel.Release();
    }

    void OnHostSizeChanged(object? sender, EventArgs e) => ApplyViewport();

    void ApplyViewport()
    {
        if (_host is not { Width: > 0, Height: > 0 } host) return;
        var vw = host.Width;
        var vh = host.Height;
        double width, maxHeight;
        if (_mode == CdsModalMode.Floating)
        {
            width = Math.Min(CdsModalSizing.DialogInlineWidth(Size), Math.Max(0, vw - 2 * CdsSpacing.Spacing05));
            maxHeight = Math.Max(0, CdsModalSizing.DialogMaxHeight(vw, vh) - CdsSpacing.Spacing09 - CdsSpacing.Spacing05);
        }
        else if (_dialogSizing)
        {
            width = CdsModalSizing.DialogWidth(Size, vw);
            maxHeight = CdsModalSizing.DialogMaxHeight(vw, vh);
        }
        else
        {
            width = CdsModalSizing.ModalWidth(Size, vw);
            maxHeight = CdsModalSizing.ModalMaxHeight(Size, vw, vh);
        }

        _box.WidthRequest = width;
        _panel.MaxPanelHeight = Math.Max(0, maxHeight - 2); // the 1px border on each side
        if (_mode == CdsModalMode.Modal)
            _box.VerticalOptions = !_dialogSizing && vw < CdsModalSizing.BreakpointMd ? LayoutOptions.Start : LayoutOptions.Center;
    }
}

/// <summary>Opens a modal surface as an overlay and keeps track of it (shared by the modal and dialog controls).</summary>
internal sealed class CdsModalPresenter(VisualElement owner)
{
    CdsOverlay? _overlay;
    CdsModalSurface? _surface;
    bool _hiding;

    public bool IsShown => _overlay is not null;

    /// <summary>Raised when the overlay closed without <see cref="Hide"/> (the host closed it).</summary>
    public event EventHandler? Dismissed;

    /// <summary>Raised whenever the overlay closed and the surface released its parts.</summary>
    public event EventHandler? Released;

    public void Show(CdsModalSurface surface, CdsOverlayOptions options, VisualElement? focus)
    {
        if (_overlay is not null) return;
        _surface = surface;
        var host = CdsOverlayHost.Find(owner) ?? CdsOverlayService.FindCurrentHost();
        options.InitialFocus = focus;
        surface.Attach(host);
        _overlay = CdsOverlayService.Open(surface, options, owner);
        _overlay.Closed += OnClosed;
        if (focus is not null)
        {
            // The target may not have a native view yet when the overlay attaches: focus again once it is loaded.
            void Refocus(object? s, EventArgs e)
            {
                surface.Loaded -= Refocus;
                focus.Focus();
            }
            surface.Loaded += Refocus;
        }
    }

    public void Hide()
    {
        var overlay = _overlay;
        if (overlay is null) return;
        _hiding = true;
        overlay.Close();
        _hiding = false;
    }

    void OnClosed(object? sender, EventArgs e)
    {
        if (sender is CdsOverlay o) o.Closed -= OnClosed;
        _overlay = null;
        _surface?.Release();
        _surface = null;
        Released?.Invoke(this, EventArgs.Empty);
        if (!_hiding) Dismissed?.Invoke(this, EventArgs.Empty);
    }
}

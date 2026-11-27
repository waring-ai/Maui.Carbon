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
    CdsModalAiColors? _aiColors;
    bool _flat;

    public CdsModalSurface(CdsModalMode mode, CdsModalSize size, bool dialogSizing, CdsModalHeader? header, CdsModalBody? body, CdsModalFooter? footer)
    {
        _mode = mode;
        Size = size;
        _dialogSizing = dialogSizing;
        Header = header;
        BodyPart = body;
        Footer = footer is { HasContent: true } ? footer : null;

        _box = new Border { StrokeThickness = 1, StrokeShape = new Rectangle(), Padding = 0, Content = _panel };
        // An AI modal never binds the flat tokens: they would resolve after the gradient and paint over it.
        var ai = header?.IsAiPresence == true;
        if (!ai) BindFlat();
        _panel.SetParts(header, body, Footer);
        if (header is not null)
        {
            header.AiPresenceChanged += OnHeaderAiPresenceChanged;
            if (ai) SetAi(true);
        }

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

    /// <summary>Raised when the container turns the AI presence on or off.</summary>
    public event EventHandler? AiChanged;

    /// <summary>
    /// Whether the container shows Carbon's AI presence: the header's decorator is an AI label (it stays on while the label shows its
    /// revert button, as in Carbon's <c>:has(.cds--ai-label)</c> selectors).
    /// </summary>
    public bool IsAi { get; private set; }

    /// <summary>The color key of the scrim behind this container: <c>ai-overlay</c> for an AI modal, else <c>null</c> (the host's default).</summary>
    public string? ScrimColorKey => IsAi ? CdsAiTokens.Overlay : null;

    /// <summary>The container's box (for tests).</summary>
    internal Border Box => _box;

    /// <summary>
    /// Turns Carbon's AI presence of the container on or off: a fill with the aura rising from the bottom edge (the bottom 64px stay
    /// flat when there is a footer) and the inner glow, a 1px <c>ai-border-start</c> to <c>ai-border-end</c> gradient border and the
    /// <c>0 24px 40px</c> <c>ai-drop-shadow</c>. Every color is bound once to its token, so theme and layer switches rebuild the brushes.
    /// </summary>
    public void SetAi(bool on)
    {
        if (IsAi == on) return;
        IsAi = on;
        if (on)
        {
            if (_flat)
            {
                // The flat border token must not repaint over the gradient when the theme changes: park the border on transparent.
                _box.RemoveDynamicResource(BackgroundColorProperty);
                CdsTokens.SetBrush(_box, Border.StrokeProperty, null);
                _flat = false;
            }
            // Two targets: a color proxy binds at most four keys.
            CdsColorProxies.Bind(_panel, colors =>
            {
                if (colors[0] is { } start && colors[1] is { } end && colors[2] is { } surface && colors[3] is { } glow)
                {
                    _aiColors = new CdsModalAiColors(start, end, surface, glow);
                    RebuildAiFill();
                }
            }, CdsAiTokens.Aura, CdsAiTokens.AuraEnd, CdsModalTokens.ContainerFill, CdsAiTokens.InnerShadow);
            CdsColorProxies.Bind(_box, colors =>
            {
                if (colors[0] is { } start && colors[1] is { } end) _box.Stroke = CdsAiTreatment.CreateBorder(start, end);
            }, CdsAiTokens.BorderStart, CdsAiTokens.BorderEnd);
            CdsTokens.SetShadow(_box, CdsAiTokens.DropShadow, offsetY: 24, radius: 40);
            _box.SizeChanged += OnBoxSizeChanged;
        }
        else
        {
            _box.SizeChanged -= OnBoxSizeChanged;
            CdsColorProxies.Unbind(_panel);
            CdsColorProxies.Unbind(_box);
            CdsTokens.SetShadow(_box, null);
            _aiColors = null;
            BodyPart?.SetAiFade(null);
            // The gradient stays until the flat fill replaces it, and the border is parked on transparent: bind the tokens again.
            _box.Background = null;
            BindFlat();
        }
        AiChanged?.Invoke(this, EventArgs.Empty);
    }

    void BindFlat()
    {
        _flat = true;
        CdsTokens.SetBrush(_box, Border.StrokeProperty, CdsModalTokens.ContainerBorder);
        CdsTokens.SetColor(_box, BackgroundColorProperty, CdsModalTokens.ContainerFill);
    }

    void OnHeaderAiPresenceChanged(object? sender, EventArgs e) => SetAi(Header?.IsAiPresence == true);

    void OnBoxSizeChanged(object? sender, EventArgs e) => RebuildAiFill();

    void RebuildAiFill()
    {
        if (_aiColors is not { } colors) return;
        var footer = Footer is null ? 0 : CdsModalSizing.FooterHeight;
        _box.Background = CdsModalAiFill.CreateFill(colors, _box.Height, footer);
        BodyPart?.SetAiFade(_box.Height > 0 ? CdsModalAiFill.CreateFade(colors, _box.Height, footer) : null);
    }

    /// <summary>Gives the container its accessible name.</summary>
    public void SetName(string? name) => CdsSemantics.SetGroupName(_box, name);

    /// <summary>Describes the container (an alert dialog is described by its body text); <c>null</c> removes it.</summary>
    public void SetDescription(string? description) => CdsSemantics.SetGroupHint(_box, description);

    /// <summary>The accessible description set by <see cref="SetDescription"/>.</summary>
    internal string? Description => CdsSemantics.GetGroupHint(_box);

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
        if (Header is not null) Header.AiPresenceChanged -= OnHeaderAiPresenceChanged;
        SetAi(false);
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

/// <summary>The <c>alertdialog</c> role of a modal or dialog (<c>alert</c> / <c>role="alertdialog"</c>), shared by both.</summary>
internal static class CdsModalAlert
{
    /// <summary>
    /// The text that describes an alert dialog: the visible text of the labels in its body, in order. Carbon controls in
    /// the body are skipped (they carry their own names).
    /// </summary>
    public static string? Describe(View? body)
    {
        var parts = new List<string>();
        Collect(body, parts);
        return parts.Count == 0 ? null : string.Join(" ", parts);
    }

    /// <summary>Announces an alert dialog that has just opened: its heading, then its description, assertively.</summary>
    public static void Announce(string? heading, string? description)
    {
        var text = string.Join(". ", new[] { heading?.Trim().TrimEnd('.'), description?.Trim() }.Where(s => !string.IsNullOrEmpty(s)));
        CdsAnnouncer.Announce(text, CdsAnnouncePriority.Assertive);
    }

    static void Collect(Element? element, List<string> parts)
    {
        switch (element)
        {
            case null:
            case VisualElement { IsVisible: false }:
            case CdsControl:
                return;
            case Label label:
                var text = label.Text ?? label.FormattedText?.ToString();
                if (!string.IsNullOrWhiteSpace(text)) parts.Add(text.Trim());
                return;
            case IVisualTreeElement tree:
                foreach (var child in tree.GetVisualChildren()) Collect(child as Element, parts);
                return;
        }
    }
}

/// <summary>Opens a modal surface as an overlay and keeps track of it (shared by the modal and dialog controls).</summary>
internal sealed class CdsModalPresenter(VisualElement owner)
{
    CdsOverlay? _overlay;
    CdsModalSurface? _surface;
    CdsOverlayOptions? _options;
    CdsOverlayHost? _host;
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
        // An AI modal dims the page with ai-overlay; the host colors the scrim of the topmost modal.
        options.ScrimColorKey = surface.ScrimColorKey;
        _options = options;
        _host = host;
        surface.AiChanged += OnAiChanged;
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

    // The decorator changed while the modal is open: recolor the scrim (it is one view the host fades for every modal).
    void OnAiChanged(object? sender, EventArgs e)
    {
        if (_options is null || _surface is null) return;
        _options.ScrimColorKey = _surface.ScrimColorKey;
        _host?.UpdateScrim();
    }

    void OnClosed(object? sender, EventArgs e)
    {
        if (sender is CdsOverlay o) o.Closed -= OnClosed;
        _overlay = null;
        if (_surface is not null) _surface.AiChanged -= OnAiChanged;
        _options = null;
        _host = null;
        _surface?.Release();
        _surface = null;
        Released?.Invoke(this, EventArgs.Empty);
        if (!_hiding) Dismissed?.Invoke(this, EventArgs.Empty);
    }
}

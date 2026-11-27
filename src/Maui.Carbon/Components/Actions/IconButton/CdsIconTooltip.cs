namespace Maui.Carbon;

/// <summary>
/// The icon tooltip of icon-only buttons: same behavior as <see cref="CdsTooltip"/> (opens after 100 ms of hover or at
/// once on keyboard focus, closes 300 ms after the pointer leaves, on blur and on Escape) with the icon-tooltip
/// measurements (2/16 padding, 8x4 caret, 8 offset) and the ability to be pinned open (CopyButton feedback).
/// </summary>
internal sealed class CdsIconTooltip
{
    readonly CdsControl _owner;
    readonly VisualElement _anchor;
    readonly CdsTooltipTimer _timer = new();
    CdsOverlay? _overlay;
    CancellationTokenSource? _pin;

    public CdsIconTooltip(CdsControl owner, VisualElement anchor)
    {
        _owner = owner;
        _anchor = anchor;
        _timer.Changed += (_, open) =>
        {
            if (_pin is not null) return;
            if (open) Show(); else Hide();
        };
        owner.Unloaded += (_, _) => Dispose();
    }

    /// <summary>The text to show; read each time the tooltip opens.</summary>
    public string? Text { get; set; }

    public CdsPlacement Placement { get; set; } = CdsPlacement.Top;
    public int EnterDelay { get; set; } = 100;
    public int LeaveDelay { get; set; } = 300;
    public bool IsHighContrast { get; set; } = true;
    public bool HasDropShadow { get; set; }
    public bool IsOpen => _overlay is not null;

    /// <summary>Feeds the owner's hover and focus state: call it from the <see cref="CdsControl.StateChanged"/> handler.</summary>
    public void StateChanged(CdsControl source)
    {
        if (_pin is not null) return;
        if (!source.IsEnabled)
        {
            Dispose();
            return;
        }
        _timer.FocusChanged(source.Is(CdsInteractionState.FocusVisible));
        if (source.Is(CdsInteractionState.Hover)) _timer.PointerEntered(EnterDelay);
        else _timer.PointerExited(LeaveDelay);
    }

    /// <summary>Shows the tooltip now and keeps it open for <paramref name="milliseconds"/> regardless of hover and focus.</summary>
    public void Pin(int milliseconds)
    {
        _timer.Reset();
        Hide();
        _pin?.Cancel();
        var cts = _pin = new CancellationTokenSource();
        Show();
        var dispatcher = _owner.Dispatcher;
        _ = Task.Delay(Math.Max(0, milliseconds), cts.Token).ContinueWith(t =>
        {
            if (t.IsCanceled) return;
            dispatcher.Dispatch(() =>
            {
                if (!ReferenceEquals(_pin, cts)) return;
                _pin = null;
                Hide();
            });
        }, TaskScheduler.Default);
    }

    public void Dispose()
    {
        _pin?.Cancel();
        _pin = null;
        _timer.Reset();
        Hide();
    }

    void Show()
    {
        if (_overlay is not null || string.IsNullOrEmpty(Text) || !_anchor.IsLoaded) return;
        var text = new CdsText(Text, CdsTypeStyle.BodyCompact01, IsHighContrast ? CdsTextTone.Inverse : CdsTextTone.Primary)
        {
            Padding = new Thickness(CdsSpacing.Spacing05, CdsSpacing.Spacing01),
            LineBreakMode = LineBreakMode.WordWrap,
        };
        var surface = new CdsPopoverSurface(text, hasCaret: true, highContrast: IsHighContrast, dropShadow: HasDropShadow, maxWidth: 288,
            caretWidth: CdsPopoverSurface.TooltipCaretWidth, caretHeight: CdsPopoverSurface.TooltipCaretHeight);
        _overlay = CdsOverlayService.Open(surface, new CdsOverlayOptions
        {
            Anchor = _anchor,
            Placement = Placement,
            Offset = surface.OffsetFor(8),
            LightDismiss = false,
            InputTransparent = true,
            ReturnFocus = false,
        });
        surface.Track(_overlay);
        var overlay = _overlay;
        overlay.Closed += (_, _) =>
        {
            if (ReferenceEquals(_overlay, overlay)) _overlay = null;
            if (_pin is null) _timer.Reset();
        };
    }

    void Hide()
    {
        var overlay = _overlay;
        _overlay = null;
        overlay?.Close();
    }
}

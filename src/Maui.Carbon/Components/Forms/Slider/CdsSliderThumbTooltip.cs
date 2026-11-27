namespace Maui.Carbon;

/// <summary>
/// The value tooltip of one slider thumb, shown when the slider hides its number box (Carbon's <c>ThumbWrapper</c> with
/// <c>hasTooltip</c>): the formatted value on a high-contrast surface above the thumb, with the icon-tooltip
/// measurements (8 inline and 2 block padding, 8x4 caret, 288 max width). It follows the value while the thumb moves.
/// </summary>
internal sealed class CdsSliderThumbTooltip
{
    readonly CdsControl _owner;
    readonly Func<VisualElement> _anchor;
    readonly Func<string> _text;
    readonly Func<double> _gap;
    readonly SliderTooltipModel _model = new();
    CancellationTokenSource? _pending;
    CdsOverlay? _overlay;
    CdsText? _label;
    string? _shown;
    double _shownAt;

    public CdsSliderThumbTooltip(CdsControl owner, Func<VisualElement> anchor, Func<string> text, Func<double> gap)
    {
        _owner = owner;
        _anchor = anchor;
        _text = text;
        _gap = gap;
    }

    public bool IsOpen => _overlay is not null;

    public string? Text => _label?.Text;

    public void HoverChanged(bool on) => Apply(SliderTooltipSource.Hover, on);

    public void FocusChanged(bool on) => Apply(SliderTooltipSource.Focus, on);

    public void DragChanged(bool on) => Apply(SliderTooltipSource.Drag, on);

    /// <summary>Follows the thumb: new text when the value changed, and a new position when the thumb moved.</summary>
    public void Refresh(double position)
    {
        if (_overlay is null || _label is null) return;
        var text = _text();
        var moved = position != _shownAt;
        if (text == _shown && !moved) return;
        _shownAt = position;
        if (text != _shown)
        {
            _shown = text;
            _label.Text = text;
        }
        _overlay.Reposition();
    }

    public void Dispose()
    {
        _pending?.Cancel();
        _pending = null;
        _model.Reset();
        Hide();
    }

    void Apply(SliderTooltipSource source, bool on)
    {
        var step = _model.Set(source, on, _overlay is not null);
        _pending?.Cancel();
        _pending = null;
        if (step is not { } due) return;
        if (due.DelayMs <= 0)
        {
            Set(due.Open);
            return;
        }
        var cts = _pending = new CancellationTokenSource();
        var dispatcher = _owner.Dispatcher;
        _ = Task.Delay(due.DelayMs, cts.Token).ContinueWith(t =>
        {
            if (t.IsCanceled) return;
            dispatcher.Dispatch(() =>
            {
                if (!cts.IsCancellationRequested) Set(due.Open);
            });
        }, TaskScheduler.Default);
    }

    void Set(bool open)
    {
        if (open) Show();
        else Hide();
    }

    void Show()
    {
        if (_overlay is not null || !_owner.IsLoaded || !_owner.IsEnabled) return;
        var anchor = _anchor();
        _shown = _text();
        _label = new CdsText(_shown, CdsTypeStyle.BodyCompact01, CdsTextTone.Inverse)
        {
            Padding = new Thickness(CdsSpacing.Spacing03, CdsSpacing.Spacing01),
            LineBreakMode = LineBreakMode.WordWrap,
        };
        var surface = new CdsPopoverSurface(_label, hasCaret: true, highContrast: true, dropShadow: false, maxWidth: 288,
            caretWidth: CdsPopoverSurface.TooltipCaretWidth, caretHeight: CdsPopoverSurface.TooltipCaretHeight);
        // The thumb already speaks its value (aria-valuetext); the tooltip is only for the eye.
        AutomationProperties.SetExcludedWithChildren(surface, true);
        var overlay = _overlay = CdsOverlayService.Open(surface, new CdsOverlayOptions
        {
            Anchor = anchor,
            Placement = CdsPlacement.Top,
            Offset = surface.OffsetFor(_gap()),
            LightDismiss = false,
            InputTransparent = true,
            ReturnFocus = false,
        });
        surface.Track(overlay);
        overlay.Closed += (_, _) =>
        {
            if (!ReferenceEquals(_overlay, overlay)) return;
            _overlay = null;
            _label = null;
        };
    }

    void Hide()
    {
        var overlay = _overlay;
        _overlay = null;
        _label = null;
        overlay?.Close();
    }
}

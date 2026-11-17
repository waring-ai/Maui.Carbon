using Microsoft.Maui.Controls.Shapes;

namespace Maui.Carbon;

/// <summary>
/// A flat button of a code snippet (the copy button, Show more / Show less): <c>layer-01</c> fill with hover and pressed
/// fills, a 2px inset focus ring, an optional icon and an optional label.
/// </summary>
internal sealed class CdsSnippetAction : CdsControl
{
    readonly Grid _root = new();
    readonly BoxView _fill = new() { InputTransparent = true };
    readonly HorizontalStackLayout _row = new() { Spacing = CdsSpacing.Spacing03, InputTransparent = true, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
    readonly CdsText _label = new() { TypeStyle = CdsTypeStyle.BodyCompact01, VerticalOptions = LayoutOptions.Center, IsVisible = false, InputTransparent = true };
    readonly CdsIcon _icon = new() { VerticalOptions = LayoutOptions.Center, InputTransparent = true };
    readonly CdsFocusRing _ring = new(CdsFocusRingStyle.Inset);
    readonly CdsHitTarget _hit;

    public CdsSnippetAction(double width, double height, CdsIconName icon, string? text = null)
    {
        _hit = CreateHitTarget();
        _root.HeightRequest = height;
        if (width > 0) _root.WidthRequest = width;
        _icon.Icon = icon;
        _label.Text = text;
        _label.IsVisible = !string.IsNullOrEmpty(text);
        if (!string.IsNullOrEmpty(text)) _row.Margin = new Thickness(CdsSpacing.Spacing05, 0);
        AutomationProperties.SetExcludedWithChildren(_fill, true);
        AutomationProperties.SetExcludedWithChildren(_row, true);
        _row.Add(_label);
        _row.Add(_icon);
        _root.Add(_fill);
        _root.Add(_row);
        _root.Add(_ring);
        _root.Add(_hit);
        SetChrome(_root);
        RefreshVisuals();
    }

    public event EventHandler? Clicked;

    public string? Text
    {
        get => _label.Text;
        set
        {
            _label.Text = value;
            _label.IsVisible = !string.IsNullOrEmpty(value);
            SemanticProperties.SetDescription(_hit, value);
        }
    }

    public string? Description
    {
        get => SemanticProperties.GetDescription(_hit);
        set => SemanticProperties.SetDescription(_hit, value);
    }

    public double IconRotation
    {
        get => _icon.Rotation;
        set => _icon.Rotation = value;
    }

    public bool? IsExpanded
    {
        get => _hit.IsExpanded;
        set
        {
            _hit.IsExpanded = value;
            _hit.Role = value is null ? CdsAccessibilityRole.Button : CdsAccessibilityRole.Expander;
        }
    }

    protected override void OnActivated() => Clicked?.Invoke(this, EventArgs.Empty);

    protected override void ApplyVisuals()
    {
        var disabled = Is(CdsInteractionState.Disabled);
        SetColor(_fill, BoxView.ColorProperty, CdsCodeSnippetTokens.Fill(State));
        SetColor(_label, Label.TextColorProperty, CdsCodeSnippetTokens.Text(disabled));
        SetColor(_icon, CdsIcon.ColorProperty, CdsCodeSnippetTokens.Icon(disabled));
        _ring.IsActive = Is(CdsInteractionState.FocusVisible);
    }
}

/// <summary>
/// The small high-contrast bubble of a code snippet: the copy button's description on hover and focus, and the
/// "Copied!" feedback after a copy. One bubble at a time; it ignores input.
/// </summary>
internal sealed class CdsSnippetTip
{
    CdsOverlay? _overlay;
    CancellationTokenSource? _timeout;

    public void Show(VisualElement anchor, string text, CdsPlacement placement, int closeAfterMs)
    {
        Hide();
        if (string.IsNullOrEmpty(text) || !anchor.IsLoaded) return;
        var label = new CdsText(text, CdsTypeStyle.BodyCompact01, CdsTextTone.Inverse)
        {
            Padding = new Thickness(CdsSpacing.Spacing05, 3),
            LineBreakMode = LineBreakMode.WordWrap,
        };
        var surface = new CdsPopoverSurface(label, hasCaret: true, highContrast: true, dropShadow: false, maxWidth: 208);
        var overlay = _overlay = CdsOverlayService.Open(surface, new CdsOverlayOptions
        {
            Anchor = anchor,
            Placement = placement,
            Offset = surface.OffsetFor(8),
            LightDismiss = false,
            InputTransparent = true,
            ReturnFocus = false,
            CloseOnEscape = false,
        });
        surface.Track(overlay);
        overlay.Closed += (_, _) => { if (ReferenceEquals(_overlay, overlay)) _overlay = null; };
        if (closeAfterMs > 0)
        {
            var cts = _timeout = new CancellationTokenSource();
            var dispatcher = anchor.Dispatcher;
            _ = Task.Delay(closeAfterMs, cts.Token).ContinueWith(t =>
            {
                if (t.IsCanceled) return;
                dispatcher.Dispatch(() => { if (!cts.IsCancellationRequested && ReferenceEquals(_overlay, overlay)) Hide(); });
            }, TaskScheduler.Default);
        }
    }

    public void Hide()
    {
        _timeout?.Cancel();
        _timeout = null;
        var overlay = _overlay;
        _overlay = null;
        overlay?.Close();
    }
}

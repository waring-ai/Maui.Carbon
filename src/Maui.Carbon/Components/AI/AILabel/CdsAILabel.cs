using Microsoft.Maui.Controls.Shapes;
using MauiLabel = Microsoft.Maui.Controls.Label;

namespace Maui.Carbon;

/// <summary>
/// Marks content or a field as AI-generated: a small "AI" button that opens an explainability callout, a toggletip with the AI
/// gradient (<see cref="CdsAILabelContent"/>: what the AI did, how confident it is, where the data came from, then
/// <see cref="CdsAILabelActions"/>). Use it for anything an AI generated or suggested, as the single entry point to explainability; not
/// as decoration or a generic info button (use a toggletip), and not to trigger AI actions (use a button). It always opens the same
/// click-to-open callout, and is never disabled.
/// <para>
/// The default button is a square with a 1px <c>border-inverse</c> border and semibold <c>text-primary</c> text (hover fills with
/// <c>border-inverse</c>); the inline kind is a small square before "AI" with no box, bordered when it has <see cref="TextLabel"/>.
/// Sizes are mini 16, 2xs 20, xs 24 (default), sm 32, md 40, lg 48 and xl 64; mini and 2xs get a 24 hit area. Use mini on tiles, xs
/// beside fields and table headers, larger sizes next to large type. Place it top-end of containers with a 16 margin, right after the
/// text of a legend or label, leftmost in a group of icons. When the person edits the AI value, remove the AI styling
/// (<see cref="CdsAiTreatment.Remove"/>) and set <see cref="IsRevertActive"/>: the button becomes a ghost revert icon button.
/// </para>
/// Enter, Space or a click toggle; Escape and a tap outside close the callout and focus returns to the button.
/// </summary>
[ContentProperty(nameof(Callout))]
public class CdsAILabel : CdsControl
{
    /// <summary>Bindable property for <see cref="Size"/>.</summary>
    public static readonly BindableProperty SizeProperty = BindableProperty.Create(
        nameof(Size), typeof(CdsAILabelSize), typeof(CdsAILabel), CdsAILabelSize.Xs, propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="Kind"/>.</summary>
    public static readonly BindableProperty KindProperty = BindableProperty.Create(
        nameof(Kind), typeof(CdsAILabelKind), typeof(CdsAILabel), CdsAILabelKind.Default, propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="AiText"/>.</summary>
    public static readonly BindableProperty AiTextProperty = BindableProperty.Create(
        nameof(AiText), typeof(string), typeof(CdsAILabel), "AI", propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="TextLabel"/>.</summary>
    public static readonly BindableProperty TextLabelProperty = BindableProperty.Create(
        nameof(TextLabel), typeof(string), typeof(CdsAILabel), null, propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="Label"/>.</summary>
    public static readonly BindableProperty LabelProperty = BindableProperty.Create(
        nameof(Label), typeof(string), typeof(CdsAILabel), "Show information", propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="Placement"/>.</summary>
    public static readonly BindableProperty PlacementProperty = BindableProperty.Create(
        nameof(Placement), typeof(CdsPlacement), typeof(CdsAILabel), CdsPlacement.Bottom);

    /// <summary>Bindable property for <see cref="IsOpen"/>.</summary>
    public static readonly BindableProperty IsOpenProperty = BindableProperty.Create(
        nameof(IsOpen), typeof(bool), typeof(CdsAILabel), false, BindingMode.TwoWay,
        propertyChanged: static (b, _, n) => ((CdsAILabel)b).OnIsOpenChanged((bool)n));

    /// <summary>Bindable property for <see cref="IsRevertActive"/>.</summary>
    public static readonly BindableProperty IsRevertActiveProperty = BindableProperty.Create(
        nameof(IsRevertActive), typeof(bool), typeof(CdsAILabel), false, propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="RevertLabel"/>.</summary>
    public static readonly BindableProperty RevertLabelProperty = BindableProperty.Create(
        nameof(RevertLabel), typeof(string), typeof(CdsAILabel), "Revert to AI input", propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="Callout"/>.</summary>
    public static readonly BindableProperty CalloutProperty = BindableProperty.Create(
        nameof(Callout), typeof(CdsAILabelContent), typeof(CdsAILabel), null);

    readonly Grid _root = new();
    readonly Grid _face = new();
    readonly Border _box = new() { StrokeThickness = 1, StrokeShape = new Rectangle(), InputTransparent = true };
    readonly Border _ring = new() { StrokeThickness = 1, StrokeShape = new Rectangle(), IsVisible = false, InputTransparent = true, BackgroundColor = Colors.Transparent };
    readonly HorizontalStackLayout _content = new() { Spacing = 0, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center, InputTransparent = true };
    readonly BoxView _dot = new() { IsVisible = false, VerticalOptions = LayoutOptions.Center, InputTransparent = true };
    readonly MauiLabel _text = new() { FontFamily = CdsFonts.SansSemiBold, VerticalOptions = LayoutOptions.Center, InputTransparent = true, LineBreakMode = LineBreakMode.NoWrap };
    readonly MauiLabel _extra = new() { FontFamily = CdsFonts.Sans, VerticalOptions = LayoutOptions.Center, InputTransparent = true, LineBreakMode = LineBreakMode.NoWrap, IsVisible = false };
    readonly CdsHitTarget _hit;
    readonly CdsIconButton _revert = new() { Kind = CdsIconButtonKind.Ghost, Icon = CdsIconName.Undo, IsVisible = false };
    CdsOverlay? _overlay;
    CdsPopoverSurface? _surface;
    bool _openPending;

    /// <summary>Creates an xs default label.</summary>
    public CdsAILabel()
    {
        HorizontalOptions = LayoutOptions.Start;
        VerticalOptions = LayoutOptions.Start;
        AutomationProperties.SetExcludedWithChildren(_box, true);
        AutomationProperties.SetExcludedWithChildren(_content, true);
        AutomationProperties.SetExcludedWithChildren(_ring, true);
        _content.Add(_dot);
        _content.Add(_text);
        _content.Add(_extra);
        _hit = CreateHitTarget(CdsAccessibilityRole.Button);
        _face.Add(_box);
        _face.Add(_ring);
        _face.Add(_content);
        _face.Add(_hit);
        _root.Add(_face);
        _root.Add(_revert);
        _revert.Clicked += (_, _) => RevertClicked?.Invoke(this, EventArgs.Empty);
        SetChrome(_root);
        Loaded += (_, _) =>
        {
            if (_openPending) Dispatcher.Dispatch(() => OnIsOpenChanged(true));
        };
        Unloaded += (_, _) => Hide(silent: true);
        Update();
    }

    /// <summary>Raised when the revert button is activated (<see cref="IsRevertActive"/>).</summary>
    public event EventHandler? RevertClicked;

    /// <summary>Raised after the callout closes.</summary>
    public event EventHandler? Closed;

    /// <summary>The button size: mini 16, 2xs 20, xs 24 (default), sm 32, md 40, lg 48, xl 64.</summary>
    public CdsAILabelSize Size
    {
        get => (CdsAILabelSize)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    /// <summary>Default (a square around "AI") or inline (a small square before it, no box).</summary>
    public CdsAILabelKind Kind
    {
        get => (CdsAILabelKind)GetValue(KindProperty);
        set => SetValue(KindProperty, value);
    }

    /// <summary>The text in the button. Default "AI".</summary>
    public string AiText
    {
        get => (string)GetValue(AiTextProperty);
        set => SetValue(AiTextProperty, value);
    }

    /// <summary>Text after "AI" in the inline kind (the bordered "inline with content" look), and its accessible name.</summary>
    public string? TextLabel
    {
        get => (string?)GetValue(TextLabelProperty);
        set => SetValue(TextLabelProperty, value);
    }

    /// <summary>What the callout explains, for the accessible name ("AI Why bx2.8x32"). Default "Show information".</summary>
    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <summary>Where the callout opens relative to the button. Default <see cref="CdsPlacement.Bottom"/>; it flips when it does not fit.</summary>
    public CdsPlacement Placement
    {
        get => (CdsPlacement)GetValue(PlacementProperty);
        set => SetValue(PlacementProperty, value);
    }

    /// <summary>Whether the callout is open (two-way). A label set open before it is loaded opens when it loads.</summary>
    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    /// <summary>Swaps the button for a ghost revert icon button (the person edited the AI value).</summary>
    public bool IsRevertActive
    {
        get => (bool)GetValue(IsRevertActiveProperty);
        set => SetValue(IsRevertActiveProperty, value);
    }

    /// <summary>Tooltip and accessible name of the revert button. Default "Revert to AI input".</summary>
    public string RevertLabel
    {
        get => (string)GetValue(RevertLabelProperty);
        set => SetValue(RevertLabelProperty, value);
    }

    /// <summary>The callout: an <see cref="CdsAILabelContent"/> holding the explanation and optionally actions.</summary>
    public CdsAILabelContent? Callout
    {
        get => (CdsAILabelContent?)GetValue(CalloutProperty);
        set => SetValue(CalloutProperty, value);
    }

    /// <inheritdoc/>
    protected override void OnActivated() => IsOpen = !IsOpen;

    /// <inheritdoc/>
    protected override void ApplyVisuals()
    {
        var v = CdsAILabelTokens.Resolve(Kind, !string.IsNullOrEmpty(TextLabel), State);
        SetColor(_box, BackgroundColorProperty, v.Background);
        SetBrush(_box, Border.StrokeProperty, v.Border);
        SetBrush(_ring, Border.StrokeProperty, v.InnerRing);
        _ring.IsVisible = v.InnerRing is not null;
        SetColor(_text, MauiLabel.TextColorProperty, v.Text);
        SetColor(_extra, MauiLabel.TextColorProperty, v.SecondaryText);
        SetColor(_dot, BoxView.ColorProperty, v.Dot);
    }

    /// <summary>
    /// Builds the callout as an ordinary view without opening an overlay (no caret): for documentation and previews. The
    /// <see cref="Callout"/> is moved into it, so call this only on a label that is not currently open.
    /// </summary>
    public View CreateCalloutPreview()
    {
        var surface = CreateSurface(hasCaret: false);
        surface.HorizontalOptions = LayoutOptions.Start;
        surface.VerticalOptions = LayoutOptions.Start;
        return surface;
    }

    CdsPopoverSurface CreateSurface(bool hasCaret)
    {
        var content = Callout ?? new CdsAILabelContent();
        var surface = new CdsPopoverSurface(content, hasCaret, highContrast: false, dropShadow: false, border: false, maxWidth: 320 + 2 * CdsSpacing.Spacing06)
        {
            BindingContext = BindingContext,
        };
        var box = surface.Box;
        box.StrokeShape = new RoundRectangle { CornerRadius = 8 };
        box.StrokeThickness = 1;
        box.MinimumWidthRequest = CdsAILabelContent.MinimumWidth;
        CdsAiTreatment.Apply(box, CdsAiTokens.CalloutBackground, shadow: false);
        // The caret takes the callout fill (Carbon: --cds-ai-popover-background).
        if (surface.Children.OfType<Polygon>().FirstOrDefault() is { } caret)
            CdsTokens.SetBrush(caret, Shape.FillProperty, CdsAiTokens.CalloutBackground);
        SemanticProperties.SetDescription(surface, CdsAILabelTokens.AccessibleName(AiText, Label, TextLabel, Kind));
        return surface;
    }

    void OnIsOpenChanged(bool open)
    {
        _hit.IsExpanded = open;
        SetState(CdsInteractionState.Open, open);
        if (open) Show();
        else Hide(silent: false);
    }

    void Show()
    {
        if (_overlay is not null) return;
        if (!IsLoaded)
        {
            _openPending = true;
            return;
        }
        _openPending = false;
        if (Callout is null) return;
        var surface = CreateSurface(hasCaret: true);
        _surface = surface;
        _overlay = CdsOverlayService.Open(surface, new CdsOverlayOptions
        {
            Anchor = _face,
            Placement = Placement,
            Offset = surface.OffsetFor(13),
            LightDismiss = true,
            CloseOnEscape = true,
            ReturnFocusTo = this,
        });
        surface.Track(_overlay);
        _overlay.Closed += OnOverlayClosed;
    }

    void Hide(bool silent)
    {
        var overlay = _overlay;
        if (overlay is null) return;
        _overlay = null;
        overlay.Closed -= OnOverlayClosed;
        overlay.Close();
        Release();
        if (!silent) Closed?.Invoke(this, EventArgs.Empty);
    }

    void OnOverlayClosed(object? sender, EventArgs e)
    {
        if (sender is CdsOverlay o) o.Closed -= OnOverlayClosed;
        _overlay = null;
        Release();
        if (IsOpen) IsOpen = false;
        Closed?.Invoke(this, EventArgs.Empty);
    }

    void Release()
    {
        // Hand the callout content back so the label can show it again.
        if (_surface is not null) _surface.Box.Content = null;
        _surface = null;
    }

    void Update()
    {
        var size = Size;
        var inline = Kind == CdsAILabelKind.Inline;
        var hasText = !string.IsNullOrEmpty(TextLabel);

        _revert.IsVisible = IsRevertActive;
        _face.IsVisible = !IsRevertActive;
        _revert.Size = CdsAILabelTokens.RevertSize(size);
        _revert.Label = RevertLabel;

        _text.Text = AiText;
        _extra.Text = TextLabel;
        _extra.IsVisible = inline && hasText;

        if (!inline)
        {
            var side = CdsAILabelTokens.Side(size);
            _face.WidthRequest = side;
            _face.HeightRequest = side;
            _box.StrokeShape = new Rectangle();
            _box.Margin = 0;
            _ring.Margin = new Thickness(1);
            _text.FontSize = CdsAILabelTokens.FontSize(size);
            _dot.IsVisible = false;
            _content.Spacing = 0;
            // mini and 2xs get a 24 hit area that extends past the box.
            var hit = CdsAILabelTokens.HitArea(size);
            var extend = Math.Max(0, (hit - side) / 2);
            _hit.Margin = new Thickness(-extend);
        }
        else
        {
            _face.WidthRequest = -1;
            _face.HeightRequest = -1;
            _box.StrokeShape = new RoundRectangle { CornerRadius = 1 };
            _ring.Margin = new Thickness(1);
            var fontSize = CdsAILabelTokens.InlineFontSize(size);
            _text.FontSize = fontSize;
            _extra.FontSize = size == CdsAILabelSize.Md ? 14 : fontSize == 12 ? 12 : 16;
            var dot = CdsAILabelTokens.InlineDot(size);
            _dot.IsVisible = true;
            _dot.WidthRequest = dot;
            _dot.HeightRequest = dot;
            _dot.Margin = new Thickness(0, 0, CdsAILabelTokens.InlineGap(size), 0);
            _content.Spacing = CdsSpacing.Spacing02;
            _content.Padding = hasText ? new Thickness(CdsSpacing.Spacing03, 2) : new Thickness(CdsSpacing.Spacing02, 1);
            _hit.Margin = 0;
        }
        SemanticProperties.SetDescription(_hit, CdsAILabelTokens.AccessibleName(AiText, Label, TextLabel, Kind));
        RefreshVisuals();
    }

    static void OnChanged(BindableObject b, object? o, object? n) => ((CdsAILabel)b).Update();
}

namespace Maui.Carbon;

/// <summary>
/// One status glyph (Carbon for IBM Products' StatusIcon): fatal, critical, major or minor warning, undefined, unknown,
/// normal, info, in progress, running or pending, each with its own shape and color. <see cref="IconDescription"/> is
/// required: it is the accessible name and the tooltip. Place a text label beside it wherever there is room; for new
/// designs prefer <see cref="CdsIconIndicator"/>, which pairs the glyph with its label. Colors follow the theme scope the
/// icon sits in (wrap it in a <see cref="CdsTheme"/> to force the light or dark palette).
/// </summary>
public class CdsStatusIcon : CdsControl
{
    const string SpinAnimation = "cds-status-icon-spin";

    /// <summary>Bindable property for <see cref="Kind"/>.</summary>
    public static readonly BindableProperty KindProperty = BindableProperty.Create(
        nameof(Kind), typeof(CdsStatusIconKind), typeof(CdsStatusIcon), CdsStatusIconKind.Normal, propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="Size"/>.</summary>
    public static readonly BindableProperty SizeProperty = BindableProperty.Create(
        nameof(Size), typeof(CdsSize), typeof(CdsStatusIcon), CdsSize.Sm, propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="IconDescription"/>.</summary>
    public static readonly BindableProperty IconDescriptionProperty = BindableProperty.Create(
        nameof(IconDescription), typeof(string), typeof(CdsStatusIcon), null, propertyChanged: OnChanged);

    readonly Grid _root = new() { HorizontalOptions = LayoutOptions.End, VerticalOptions = LayoutOptions.Center };
    readonly CdsIcon _icon = new();
    CdsStatusGlyphView? _glyph;
    CdsStatusGlyphView? _underlay;

    /// <summary>Creates a small "normal" status icon.</summary>
    public CdsStatusIcon()
    {
        HorizontalOptions = LayoutOptions.Start;
        VerticalOptions = LayoutOptions.Center;
        _root.Add(_icon);
        SetChrome(_root);
        Loaded += (_, _) => UpdateSpin();
        Unloaded += (_, _) => StopSpin();
        Update();
    }

    /// <summary>Creates a status icon.</summary>
    public CdsStatusIcon(CdsStatusIconKind kind, string iconDescription, CdsSize size = CdsSize.Sm) : this()
    {
        Kind = kind;
        IconDescription = iconDescription;
        Size = size;
    }

    /// <summary>The status. Default <see cref="CdsStatusIconKind.Normal"/>.</summary>
    public CdsStatusIconKind Kind
    {
        get => (CdsStatusIconKind)GetValue(KindProperty);
        set => SetValue(KindProperty, value);
    }

    /// <summary>Glyph size: <see cref="CdsSize.Sm"/> 16 (default), Md 20, Lg 24, Xl 32 (Xs draws 16, Xxl 32).</summary>
    public CdsSize Size
    {
        get => (CdsSize)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    /// <summary>The status in words ("Critical", "3 nodes failed"). Required: the accessible name and the tooltip.</summary>
    public string? IconDescription
    {
        get => (string?)GetValue(IconDescriptionProperty);
        set => SetValue(IconDescriptionProperty, value);
    }

    /// <inheritdoc/>
    protected override void ApplyVisuals()
    {
        var color = CdsStatusIconTokens.Color(Kind);
        var inner = CdsStatusIconTokens.InnerColor(Kind, ContextTheme.IsDark());
        SetColor(_icon, CdsIcon.ColorProperty, color);
        // The bundled icon keeps its inner mark a hole unless a color is set; only the warning triangles get one.
        if (inner is not null) SetColor(_icon, CdsIcon.InnerColorProperty, inner);
        else SetColor(_icon, CdsIcon.InnerColorProperty, null);
        if (_glyph is not null) SetColor(_glyph, CdsStatusGlyphView.FillProperty, color);
        if (_underlay is not null) SetColor(_underlay, CdsStatusGlyphView.FillProperty, inner);
    }

    /// <inheritdoc/>
    protected override void OnContextChanged() => RefreshVisuals();

    void Update()
    {
        var size = CdsStatusIconTokens.GlyphSize(Size);
        var icon = CdsStatusIconTokens.Icon(Kind);
        var description = string.IsNullOrEmpty(IconDescription) ? null : IconDescription;

        _icon.Icon = icon;
        _icon.Size = size;
        _icon.IsVisible = icon is not null;
        _icon.Description = icon is not null ? description : null;

        if (icon is null) EnsureGlyph();
        if (_glyph is not null && _underlay is not null)
        {
            _glyph.IsVisible = icon is null;
            _underlay.IsVisible = icon is null;
            _glyph.GlyphSize = size;
            // A solid triangle 85% the size sits behind the cut-out "!" so the mark reads dark, as in Carbon for IBM
            // Products; scaled about the center it stays inside the outer triangle.
            _underlay.GlyphSize = size * 0.85;
            AutomationProperties.SetIsInAccessibleTree(_glyph, icon is null && description is not null);
            SemanticProperties.SetDescription(_glyph, icon is null ? description : null);
        }

        _root.WidthRequest = size;
        _root.HeightRequest = size;
        if (description is not null) ToolTipProperties.SetText(_root, description);
        else _root.ClearValue(ToolTipProperties.TextProperty);
        RefreshVisuals();
        UpdateSpin();
    }

    void EnsureGlyph()
    {
        if (_glyph is not null) return;
        _underlay = new CdsStatusGlyphView { Shape = CdsStatusShape.TriangleDown, HorizontalOptions = LayoutOptions.Center };
        _glyph = new CdsStatusGlyphView { Shape = CdsStatusShape.TriangleDownBang, HorizontalOptions = LayoutOptions.Center };
        _root.Insert(0, _underlay);
        _root.Insert(1, _glyph);
    }

    void UpdateSpin()
    {
        if (!CdsStatusIconTokens.Spins(Kind) || !IsLoaded || CdsMotion.IsReducedMotion)
        {
            StopSpin();
            return;
        }
        // Carbon: scaleY(-1) rotate(360deg -> 0deg) over 8 s, linear.
        _icon.ScaleY = -1;
        CdsLoop.Start(_icon, SpinAnimation, phase => _icon.Rotation = 360 * (1 - phase), 0, CdsStatusIconTokens.SpinDuration);
    }

    void StopSpin()
    {
        CdsLoop.Stop(_icon, SpinAnimation);
        _icon.Rotation = 0;
        _icon.ScaleY = 1;
    }

    static void OnChanged(BindableObject b, object? o, object? n) => ((CdsStatusIcon)b).Update();
}

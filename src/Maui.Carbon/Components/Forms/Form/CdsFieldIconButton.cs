namespace Maui.Carbon;

/// <summary>The look of an icon button inside a field.</summary>
internal enum CdsFieldButtonKind
{
    /// <summary>Steppers and the password toggle: hover and press use <c>field-hover-01</c>.</summary>
    Field,

    /// <summary>The search clear button: hover <c>field-hover-01</c>, pressed <c>background-selected</c>.</summary>
    Search,

    /// <summary>A collapsed search or a plain icon button: hover <c>background-hover</c>.</summary>
    Plain,
}

/// <summary>Color keys of a field icon button; <c>null</c> background is transparent.</summary>
internal readonly record struct CdsFieldButtonVisuals(string? Background, string Icon);

/// <summary>Maps field icon button kinds and states to tokens.</summary>
internal static class CdsFieldButtonTokens
{
    /// <summary>Resolves the colors. <paramref name="muted"/> dims the icon (read-only steppers).</summary>
    public static CdsFieldButtonVisuals Resolve(CdsFieldButtonKind kind, CdsInteractionState state, bool muted = false)
    {
        if (state.HasFlag(CdsInteractionState.Disabled))
            return new(null, kind == CdsFieldButtonKind.Search ? CdsColorKeys.IconOnColorDisabled : CdsColorKeys.IconDisabled);
        if (muted) return new(null, CdsColorKeys.IconDisabled);
        if (state.HasFlag(CdsInteractionState.Pressed))
        {
            return new(kind switch
            {
                CdsFieldButtonKind.Search => CdsColorKeys.BackgroundSelected,
                CdsFieldButtonKind.Plain => CdsColorKeys.BackgroundActive,
                _ => CdsColorKeys.FieldHover01,
            }, CdsColorKeys.IconPrimary);
        }
        if (state.HasFlag(CdsInteractionState.Hover))
            return new(kind == CdsFieldButtonKind.Plain ? CdsColorKeys.BackgroundHover : CdsColorKeys.FieldHover01, CdsColorKeys.IconPrimary);
        return new(null, CdsColorKeys.IconPrimary);
    }
}

/// <summary>
/// A square icon button that sits inside a field (password toggle, number steppers, search clear, file remove). It owns a
/// hit target for focus, press and activation.
/// </summary>
internal sealed class CdsFieldIconButton : CdsControl
{
    readonly Grid _root = new();
    readonly BoxView _fill = new() { InputTransparent = true };
    readonly CdsIcon _icon = new();
    readonly CdsFocusRing _ring = new(CdsFocusRingStyle.Inset);
    readonly CdsHitTarget _hit;
    CdsFieldButtonKind _kind;
    bool _muted;
    string? _description;

    public CdsFieldIconButton(CdsIconName icon, CdsFieldButtonKind kind = CdsFieldButtonKind.Field, double width = 40, double height = 40)
    {
        _kind = kind;
        _icon.Icon = icon;
        _hit = CreateHitTarget();
        AutomationProperties.SetExcludedWithChildren(_icon, true);
        _root.Add(_fill);
        _root.Add(_icon);
        _root.Add(_ring);
        _root.Add(_hit);
        _root.WidthRequest = width;
        _root.HeightRequest = height;
        SetChrome(_root);
        ApplyVisuals();
    }

    /// <summary>Raised when the button is activated.</summary>
    public event EventHandler? Clicked;

    public CdsFieldButtonKind Kind
    {
        get => _kind;
        set { _kind = value; RefreshVisuals(); }
    }

    public CdsIconName Icon
    {
        set => _icon.Icon = value;
    }

    public double IconSize
    {
        set => _icon.Size = value;
    }

    /// <summary>Dims the icon and ignores presses (read-only steppers).</summary>
    public bool Muted
    {
        get => _muted;
        set { _muted = value; _hit.InputTransparent = value; RefreshVisuals(); }
    }

    /// <summary>The accessible name.</summary>
    public string? Description
    {
        get => _description;
        set { _description = value; SemanticProperties.SetDescription(_hit, value); }
    }

    /// <summary>
    /// Whether Tab stops on the button. Default on. Number-input steppers turn it off (Carbon: <c>tabIndex=-1</c>); they
    /// stay clickable and reachable by screen readers.
    /// </summary>
    public bool IsTabStop
    {
        get => _hit.IsTabStop;
        set => _hit.IsTabStop = value;
    }

    /// <summary>The accessibility role. Default <see cref="CdsAccessibilityRole.Button"/>.</summary>
    public CdsAccessibilityRole Role
    {
        get => _hit.Role;
        set => _hit.Role = value;
    }

    /// <summary>The expanded state reported with an expander role (<c>aria-expanded</c>); <c>null</c> for none.</summary>
    public bool? IsExpanded
    {
        get => _hit.IsExpanded;
        set => _hit.IsExpanded = value;
    }

    /// <summary>The button's hit target (for tests).</summary>
    internal CdsHitTarget HitTarget => _hit;

    public void SetSize(double width, double height)
    {
        _root.WidthRequest = width;
        _root.HeightRequest = height;
    }

    protected override void OnActivated() => Clicked?.Invoke(this, EventArgs.Empty);

    protected override void ApplyVisuals()
    {
        var v = CdsFieldButtonTokens.Resolve(_kind, State, _muted);
        SetColor(_fill, BoxView.ColorProperty, v.Background);
        SetColor(_icon, CdsIcon.ColorProperty, v.Icon);
        _ring.IsActive = Is(CdsInteractionState.FocusVisible);
    }
}

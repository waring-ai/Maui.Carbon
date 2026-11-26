using Microsoft.Maui.Controls.Shapes;

namespace Maui.Carbon;

/// <summary>How an action face lays out its label and icon.</summary>
internal enum CdsActionFaceMode
{
    /// <summary>A square with a centered icon.</summary>
    IconOnly,
    /// <summary>Label on the start edge, icon pinned 16 from the end edge (Carbon's filled and outlined buttons).</summary>
    Pinned,
    /// <summary>Icon 8 after the label (Carbon's ghost buttons).</summary>
    Inline,
}

/// <summary>
/// The shared button chrome of the action components (IconButton, CopyButton, OverflowMenu, MenuButton, ComboButton):
/// a bordered fill, a label and icon laid out like <see cref="CdsButton"/>, an optional badge and a native hit
/// target. Colors come from a resolver, so each component brings its own tokens. Internal: the public controls wrap it.
/// </summary>
internal sealed class CdsActionFace : CdsControl
{
    readonly Grid _root = new();
    readonly Border _box = new() { StrokeThickness = 1, StrokeShape = new Rectangle(), Padding = 0 };
    readonly Grid _layout = new() { ColumnSpacing = 0 };
    readonly CdsText _label = new() { LineBreakMode = LineBreakMode.WordWrap };
    readonly CdsIcon _icon = new() { IsVisible = false };
    readonly CdsHitTarget _hit;
    readonly CdsButtonOutline _outline = new();
    readonly Border _badge = new() { StrokeThickness = 0, InputTransparent = true, StrokeShape = new RoundRectangle { CornerRadius = 100 } };
    readonly CdsText _badgeText = new(string.Empty, CdsTypeStyle.Label01, CdsTextTone.OnColor) { InputTransparent = true };
    CdsFocusRing _ring;
    CdsFocusRingStyle _ringStyle = CdsFocusRingStyle.Inset;
    bool _suspend = true;

    string? _text;
    CdsIconName? _iconName;
    CdsGlyph? _glyph;
    CdsActionFaceMode _mode = CdsActionFaceMode.IconOnly;
    double _height = CdsSizing.Large;
    double _minWidth;
    double _iconSize = 16;
    int? _badgeCount;
    bool _isSelected;
    bool _isOpen;
    bool _rotateWhenOpen;
    bool _truncate;
    bool _expandable;
    string? _name;
    CdsAccessibilityRole _role = CdsAccessibilityRole.Button;
    Func<CdsInteractionState, CdsButtonVisuals> _resolver = static _ => new(null, null, CdsColorKeys.TextPrimary, CdsColorKeys.IconPrimary);
    Func<bool>? _openShadow;

    public CdsActionFace()
    {
        _layout.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        _layout.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        _layout.Add(_label, 0);
        _layout.Add(_icon, 1);
        _box.Content = _layout;
        _box.InputTransparent = true;
        AutomationProperties.SetExcludedWithChildren(_box, true);

        _badge.Content = _badgeText;
        _badge.IsVisible = false;
        _badge.HorizontalOptions = LayoutOptions.End;
        _badge.VerticalOptions = LayoutOptions.Start;
        CdsTokens.SetColor(_badge, BackgroundColorProperty, CdsColorKeys.SupportError);
        AutomationProperties.SetExcludedWithChildren(_badge, true);

        _hit = CreateHitTarget();
        _ring = new CdsFocusRing(_ringStyle);
        _root.Add(_box);
        _root.Add(_outline);
        _root.Add(_badge);
        _root.Add(_ring);
        _root.Add(_hit);
        HorizontalOptions = LayoutOptions.Start;
        VerticalOptions = LayoutOptions.Start;
        SetChrome(_root);
        _suspend = false;
        UpdateLayout();
    }

    /// <summary>Raised when the face is activated (click, tap, Enter, Space).</summary>
    public event EventHandler? Activated;

    /// <summary>The native hit target (focus, accessibility).</summary>
    public CdsHitTarget Hit => _hit;

    public string? Text { get => _text; set { _text = value; UpdateLayout(); } }
    public CdsIconName? Icon { get => _iconName; set { _iconName = value; UpdateLayout(); } }

    /// <summary>A custom glyph drawn when <see cref="Icon"/> is not set (icons the bundled set lacks, such as the chart toolbar's table icon).</summary>
    public CdsGlyph? Glyph { get => _glyph; set { _glyph = value; UpdateLayout(); } }
    public CdsActionFaceMode Mode { get => _mode; set { _mode = value; UpdateLayout(); } }
    public double ButtonHeight { get => _height; set { _height = value; UpdateLayout(); } }
    public double MinWidth { get => _minWidth; set { _minWidth = value; UpdateLayout(); } }
    public double IconSize { get => _iconSize; set { _iconSize = value; UpdateLayout(); } }
    public bool TruncateLabel { get => _truncate; set { _truncate = value; UpdateLayout(); } }
    public string? AccessibleName { get => _name; set { _name = value; UpdateLayout(); } }
    public CdsAccessibilityRole HitRole { get => _role; set { _role = value; UpdateLayout(); } }

    /// <summary>The button has a popup: the hit target reports expanded state.</summary>
    public bool Expandable { get => _expandable; set { _expandable = value; UpdateLayout(); } }

    /// <summary>The icon turns 180 degrees while <see cref="IsOpen"/> (menu chevrons).</summary>
    public bool RotateIconWhenOpen { get => _rotateWhenOpen; set { _rotateWhenOpen = value; UpdateRotation(animate: false); } }

    /// <summary>Gives the face the menu drop shadow while open (the overflow menu trigger).</summary>
    public bool ShadowWhenOpen { get => _openShadow is not null; set => _openShadow = value ? (() => _isOpen) : null; }

    public CdsFocusRingStyle RingStyle
    {
        get => _ringStyle;
        set
        {
            if (_ringStyle == value) return;
            _ringStyle = value;
            var index = _root.Children.IndexOf(_ring);
            if (index < 0) return;
            _root.Children.RemoveAt(index);
            _ring = new CdsFocusRing(value);
            _root.Children.Insert(index, _ring);
            RefreshVisuals();
        }
    }

    public Func<CdsInteractionState, CdsButtonVisuals> Resolver
    {
        get => _resolver;
        set { _resolver = value; RefreshVisuals(); }
    }

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            _isSelected = value;
            SetState(CdsInteractionState.Selected, value);
            UpdateLayout();
        }
    }

    public bool IsOpen
    {
        get => _isOpen;
        set
        {
            if (_isOpen == value) return;
            _isOpen = value;
            SetState(CdsInteractionState.Open, value);
            UpdateRotation(animate: true);
            UpdateLayout();
        }
    }

    /// <summary>Count in the badge: <c>null</c> none, 0 a dot, otherwise the number.</summary>
    public int? BadgeCount { get => _badgeCount; set { _badgeCount = value; UpdateLayout(); } }

    /// <summary>The accessible description the badge adds to the hit target: its count text while a count shows, else null.</summary>
    internal string? BadgeHint => _badgeCount is > 0 && IsEnabled ? CdsIconButtonTokens.BadgeText(_badgeCount) : null;

    /// <inheritdoc/>
    protected override void OnActivated() => Activated?.Invoke(this, EventArgs.Empty);

    /// <inheritdoc/>
    protected override void ApplyVisuals()
    {
        var v = _resolver(State);
        // The fill goes on the root, which covers the whole box: on Mac Catalyst a Border paints its background and stroke
        // half a point inside its frame, so a filled face drew 1pt smaller than its layout box (a 2pt gap in combo buttons).
        SetColor(_root, BackgroundColorProperty, v.Background);
        // The outline is drawn by hairlines on the box edge, for the same reason; the Border keeps its 1px (transparent)
        // stroke so the label and icon keep their inset.
        SetBrush(_box, Border.StrokeProperty, null);
        SetColor(_outline, CdsButtonOutline.ColorProperty, v.Border);
        SetColor(_label, Label.TextColorProperty, v.Foreground);
        SetColor(_icon, CdsIcon.ColorProperty, v.Icon);
        _ring.IsActive = Is(CdsInteractionState.FocusVisible);
        if (_openShadow is not null) CdsTokens.SetShadow(_root, _isOpen ? CdsColorKeys.Shadow : null, offsetY: 2, radius: 6);
    }

    void UpdateRotation(bool animate)
    {
        var target = _rotateWhenOpen && _isOpen ? 180d : 0d;
        if (Math.Abs(_icon.Rotation - target) < 0.1) return;
        if (!animate)
        {
            _icon.Rotation = target;
            return;
        }
        var from = _icon.Rotation;
        _ = CdsMotion.AnimateAsync(_icon, "CdsChevron", v => _icon.Rotation = v, from, target, CdsDuration.Fast02, CdsEasing.StandardProductive);
    }

    void UpdateLayout()
    {
        if (_suspend) return;
        var height = _height;
        var iconOnly = _mode == CdsActionFaceMode.IconOnly;
        var lineHeight = 18d;
        var iconSize = _iconSize;

        _label.Text = _text;
        _label.LineBreakMode = _truncate ? LineBreakMode.TailTruncation : LineBreakMode.WordWrap;
        _label.MaxLines = _truncate ? 1 : -1;
        _label.IsVisible = !iconOnly;
        _icon.Icon = _iconName;
        _icon.Glyph = _iconName is null ? _glyph : null;
        _icon.Size = iconSize;
        _icon.IsVisible = _iconName is not null || _glyph is not null;

        var padTop = Math.Min((height - lineHeight) / 2 - 1, 14);
        var iconTop = Math.Min((height - iconSize) / 2 - 1, 14) + 1;

        _root.MinimumHeightRequest = height;
        _root.MinimumWidthRequest = iconOnly ? height : _minWidth;
        if (iconOnly)
        {
            _root.WidthRequest = height;
            _root.HeightRequest = height;
            Grid.SetColumn(_icon, 0);
            Grid.SetColumnSpan(_icon, 2);
            _icon.HorizontalOptions = LayoutOptions.Center;
            _icon.VerticalOptions = LayoutOptions.Center;
            _icon.Margin = 0;
        }
        else
        {
            _root.WidthRequest = -1;
            _root.HeightRequest = -1;
            _label.VerticalOptions = LayoutOptions.Start;
            if (_mode == CdsActionFaceMode.Inline)
            {
                _label.Margin = new Thickness(15, padTop, _iconName is null ? 15 : 0, padTop);
                Grid.SetColumn(_icon, 1);
                Grid.SetColumnSpan(_icon, 1);
                _icon.HorizontalOptions = LayoutOptions.Start;
                _icon.Margin = new Thickness(8, iconTop - 1, 15, 0);
            }
            else
            {
                _label.Margin = new Thickness(15, padTop, 63, padTop);
                Grid.SetColumn(_icon, 0);
                Grid.SetColumnSpan(_icon, 2);
                _icon.HorizontalOptions = LayoutOptions.End;
                _icon.Margin = new Thickness(0, iconTop - 1, 15, 0);
            }
            _icon.VerticalOptions = LayoutOptions.Start;
        }

        // Badge: a dot (8x8) at count 0, otherwise a pill with the number.
        if (_badgeCount is { } count && IsEnabled)
        {
            _badge.IsVisible = true;
            if (count <= 0)
            {
                _badgeText.IsVisible = false;
                _badge.Padding = 0;
                _badge.WidthRequest = 8;
                _badge.HeightRequest = 8;
                _badge.Margin = new Thickness(0, 8, 8, 0);
            }
            else
            {
                _badgeText.IsVisible = true;
                _badgeText.Text = CdsIconButtonTokens.BadgeText(count);
                _badge.Padding = new Thickness(4, 0, 4, 2);
                _badge.WidthRequest = -1;
                _badge.HeightRequest = -1;
                _badge.MinimumWidthRequest = 8;
                _badge.MaximumHeightRequest = 16;
                _badge.Margin = new Thickness(0, 4, 4, 0);
            }
        }
        else
        {
            _badge.IsVisible = false;
        }

        SemanticProperties.SetDescription(_hit, _name);
        // The count is announced after the name (Carbon ties the badge to the button with aria-describedby); a dot has
        // no text, so its meaning belongs in the label.
        if (BadgeHint is { } hint) SemanticProperties.SetHint(_hit, hint);
        else _hit.ClearValue(SemanticProperties.HintProperty);
        _hit.Role = _role;
        _hit.IsChecked = _role == CdsAccessibilityRole.ToggleButton ? _isSelected : false;
        _hit.IsExpanded = _expandable ? _isOpen : null;
        RefreshVisuals();
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
        if (propertyName == IsEnabledProperty.PropertyName) UpdateLayout();
    }
}

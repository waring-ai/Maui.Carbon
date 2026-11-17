namespace Maui.Carbon;

/// <summary>
/// One tappable row of a menu level. Keyboard focus never moves into the menu (the trigger keeps it); the row is
/// "highlighted" by the menu's virtual focus or by the pointer, and then shows the hover fill (and the inset focus ring
/// when the highlight came from the keyboard).
/// </summary>
internal sealed class CdsMenuRow : CdsControl
{
    readonly Grid _root = new() { Padding = new Thickness(CdsSpacing.Spacing05, 0) };
    readonly CdsText _label = new() { LineBreakMode = LineBreakMode.TailTruncation, MaxLines = 1, VerticalOptions = LayoutOptions.Center };
    readonly CdsText? _shortcut;
    readonly CdsIcon? _check;
    readonly CdsIcon? _icon;
    readonly CdsIcon? _caret;
    readonly CdsFocusRing _ring = new(CdsFocusRingStyle.Inset);
    readonly CdsHitTarget _hit;
    bool _highlighted;
    bool _showRing;
    bool _submenuOpen;

    public CdsMenuRow(MenuRowInfo info, double height, bool hasIcons, bool hasSelection)
    {
        Info = info;
        HorizontalOptions = LayoutOptions.Fill;

        _root.HeightRequest = height;
        _root.ColumnSpacing = CdsSpacing.Spacing04;
        var column = 0;
        if (hasSelection)
        {
            _root.ColumnDefinitions.Add(new ColumnDefinition(16));
            _check = new CdsIcon(CdsIconName.Checkmark, 16) { IsVisible = info.IsSelected };
            _root.Add(_check, column++);
        }
        if (hasIcons)
        {
            _root.ColumnDefinitions.Add(new ColumnDefinition(16));
            if (info.Icon is { } icon)
            {
                _icon = new CdsIcon(icon, 16);
                _root.Add(_icon, column);
            }
            column++;
        }
        _root.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        _label.Text = info.Text;
        _root.Add(_label, column++);

        if (info.HasSubmenu)
        {
            _root.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
            _caret = new CdsIcon(CdsIconName.CaretRight, 16);
            _root.Add(_caret, column);
        }
        else if (!string.IsNullOrEmpty(info.Shortcut))
        {
            _root.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
            _shortcut = new CdsText(info.Shortcut, CdsTypeStyle.BodyCompact01) { VerticalOptions = LayoutOptions.Center };
            _root.Add(_shortcut, column);
        }

        foreach (var child in _root.Children.OfType<View>()) child.InputTransparent = true;
        _ring.InputTransparent = true;
        _hit = CreateHitTarget(info.Kind switch
        {
            MenuRowKind.Selectable => CdsAccessibilityRole.ToggleButton,
            MenuRowKind.Radio => CdsAccessibilityRole.Radio,
            _ => CdsAccessibilityRole.MenuItem,
        });
        SemanticProperties.SetDescription(_hit, info.Text);
        if (!string.IsNullOrEmpty(info.GroupLabel)) SemanticProperties.SetHint(_hit, info.GroupLabel);
        if (info.Kind is MenuRowKind.Selectable or MenuRowKind.Radio) _hit.IsChecked = info.IsSelected;
        if (info.HasSubmenu) _hit.IsExpanded = false;
        IsEnabled = info.IsEnabled;

        var cell = new Grid();
        cell.Add(_root);
        cell.Add(_ring);
        cell.Add(_hit);
        SetChrome(cell);
    }

    public MenuRowInfo Info { get; }

    /// <summary>Raised when the pointer enters the row.</summary>
    public event EventHandler? HoverEntered;

    /// <summary>Raised when the row is chosen by tap, click, Enter or Space on the hit target.</summary>
    public event EventHandler? Chosen;

    /// <summary>The menu's virtual focus is on this row.</summary>
    public bool IsHighlighted
    {
        get => _highlighted;
        set
        {
            if (_highlighted == value) return;
            _highlighted = value;
            RefreshVisuals();
        }
    }

    /// <summary>Show the inset focus ring (the highlight came from the keyboard).</summary>
    public bool ShowFocusRing
    {
        get => _showRing;
        set
        {
            if (_showRing == value) return;
            _showRing = value;
            RefreshVisuals();
        }
    }

    /// <summary>The row's submenu is open (reported as expanded).</summary>
    public bool IsSubmenuOpen
    {
        get => _submenuOpen;
        set
        {
            _submenuOpen = value;
            if (Info.HasSubmenu) _hit.IsExpanded = value;
        }
    }

    /// <inheritdoc/>
    protected override void OnActivated() => Chosen?.Invoke(this, EventArgs.Empty);

    /// <inheritdoc/>
    protected override void OnStateChanged(CdsInteractionState previous)
    {
        base.OnStateChanged(previous);
        if (Is(CdsInteractionState.Hover) && (previous & CdsInteractionState.Hover) == 0 && Info.IsFocusable)
            HoverEntered?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc/>
    protected override void ApplyVisuals()
    {
        var on = Info.IsFocusable && (_highlighted || Is(CdsInteractionState.Pressed));
        var v = CdsMenuTokens.Resolve(Info.IsDanger, on, Info.IsEnabled);
        SetColor(_root, BackgroundColorProperty, v.Background);
        SetColor(_label, Label.TextColorProperty, v.Text);
        if (_shortcut is not null) SetColor(_shortcut, Label.TextColorProperty, v.Text);
        if (_icon is not null) SetColor(_icon, CdsIcon.ColorProperty, v.Icon);
        if (_check is not null) SetColor(_check, CdsIcon.ColorProperty, v.Icon);
        if (_caret is not null) SetColor(_caret, CdsIcon.ColorProperty, v.Icon);
        _ring.IsActive = Info.IsFocusable && _highlighted && _showRing;
    }
}

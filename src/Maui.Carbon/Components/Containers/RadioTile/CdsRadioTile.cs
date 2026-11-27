namespace Maui.Carbon;

/// <summary>
/// A tile that acts as a radio button, grouped by a <see cref="CdsTileGroup"/> so exactly one can be chosen (a plan, a
/// region, a node size). A 16px check mark appears 16px from the top-right corner when it is chosen, together with a 1px
/// <c>layer-selected-inverse</c> border. Give tiles in a group equal heights and do not pre-select without a sensible default.
/// </summary>
public class CdsRadioTile : CdsTileBase
{
    /// <summary>Bindable property for <see cref="Value"/>.</summary>
    public static readonly BindableProperty ValueProperty = BindableProperty.Create(
        nameof(Value), typeof(object), typeof(CdsRadioTile), null,
        propertyChanged: static (b, _, _) => ((CdsRadioTile)b).ValueChanged?.Invoke(b, EventArgs.Empty));

    /// <summary>Bindable property for <see cref="IsChecked"/>.</summary>
    public static readonly BindableProperty IsCheckedProperty = BindableProperty.Create(
        nameof(IsChecked), typeof(bool), typeof(CdsRadioTile), false, BindingMode.TwoWay,
        propertyChanged: static (b, _, n) => ((CdsRadioTile)b).OnCheckedChanged((bool)n));

    /// <summary>Bindable property for <see cref="AccessibleName"/>.</summary>
    public static readonly BindableProperty AccessibleNameProperty = BindableProperty.Create(
        nameof(AccessibleName), typeof(string), typeof(CdsRadioTile), null,
        propertyChanged: static (b, _, _) => ((CdsRadioTile)b).UpdateAccessibility());

    readonly CdsIcon _glyph = new(CdsIconName.CheckmarkFilled, 16) { HorizontalOptions = LayoutOptions.End, VerticalOptions = LayoutOptions.Start, Margin = new Thickness(0, CdsSpacing.Spacing05, CdsSpacing.Spacing05, 0) };
    readonly CdsHitTarget _hit;
    bool _inheritedDisabled;

    /// <summary>Creates an unchosen tile.</summary>
    public CdsRadioTile()
    {
        SealBody();
        BodyHost.Padding = new Thickness(CdsSpacing.Spacing05, CdsSpacing.Spacing05, CdsSpacing.Spacing09, CdsSpacing.Spacing05);
        AutomationProperties.SetExcludedWithChildren(_glyph, true);
        _hit = CreateHitTarget(CdsAccessibilityRole.Radio);
        Stack.Add(_glyph);
        Stack.Add(_hit);
        Loaded += (_, _) => UpdateAccessibility();
        UpdateAccessibility();
    }

    /// <summary>Raised when <see cref="IsChecked"/> changes.</summary>
    public event EventHandler<CheckedChangedEventArgs>? CheckedChanged;

    internal event EventHandler? UserSelected;
    internal event EventHandler? CheckedPropertyChanged;
    internal event EventHandler? ValueChanged;

    /// <summary>The value the group reports in <see cref="CdsTileGroup.SelectedValue"/> when this tile is chosen.</summary>
    public object? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    /// <summary>Whether this tile is the chosen one (two-way). In a group the group keeps exactly one chosen.</summary>
    public bool IsChecked
    {
        get => (bool)GetValue(IsCheckedProperty);
        set => SetValue(IsCheckedProperty, value);
    }

    /// <summary>Overrides the accessible name, which is otherwise all of the body's text (put the title first).</summary>
    public string? AccessibleName
    {
        get => (string?)GetValue(AccessibleNameProperty);
        set => SetValue(AccessibleNameProperty, value);
    }

    internal bool IsEffectivelyDisabled => Is(CdsInteractionState.Disabled) || _inheritedDisabled;

    internal void SetInheritedDisabled(bool disabled)
    {
        if (_inheritedDisabled == disabled) return;
        _inheritedDisabled = disabled;
        RefreshVisuals();
    }

    internal bool FocusTile() => _hit.Focus();

    /// <summary>The tile's hit target (the group's roving tab stop moves between these).</summary>
    internal CdsHitTarget Hit => _hit;

    /// <summary>Whether the tile can take keyboard focus: shown and not disabled (by itself or its group).</summary>
    internal bool CanTakeFocus => IsVisible && !IsEffectivelyDisabled;

    /// <inheritdoc/>
    protected override CdsTileKind TileKind => CdsTileKind.Radio;

    /// <inheritdoc/>
    protected override CdsAILabelSize? DecoratorSize => CdsAILabelSize.Xs;

    // Carbon moves the label from 16 to 40 when the check mark appears (inset-inline-end transition, fast-02 standard productive).
    /// <inheritdoc/>
    protected override double DecoratorEndInset => IsChecked ? CdsSpacing.Spacing08 : CdsSpacing.Spacing05;

    /// <inheritdoc/>
    protected override void OnDecoratorReplaced() =>
        BodyHost.Padding = new Thickness(CdsSpacing.Spacing05, CdsSpacing.Spacing05, CdsTileTokens.SelectableEndPadding(Decorator is not null), CdsSpacing.Spacing05);

    /// <inheritdoc/>
    protected override CdsInteractionState TokenState
    {
        get
        {
            var state = State;
            if (_inheritedDisabled) state |= CdsInteractionState.Disabled;
            return IsChecked ? state | CdsInteractionState.Selected : state;
        }
    }

    /// <inheritdoc/>
    protected override double BorderThickness => 1;

    /// <inheritdoc/>
    protected override void OnActivated()
    {
        if (IsEffectivelyDisabled) return;
        if (!IsChecked) IsChecked = true;
        UserSelected?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc/>
    protected override void OnBodyReplaced() => UpdateAccessibility();

    /// <inheritdoc/>
    protected override void ApplyTileVisuals(CdsTileVisuals visuals)
    {
        _glyph.IsVisible = IsChecked;
        SetColor(_glyph, CdsIcon.ColorProperty, visuals.Icon);
        _hit.IsEnabled = !IsEffectivelyDisabled;
    }

    void OnCheckedChanged(bool value)
    {
        _hit.IsChecked = value;
        UpdateDecoratorInset(animate: true);
        RefreshVisuals();
        CheckedChanged?.Invoke(this, new CheckedChangedEventArgs(value));
        CheckedPropertyChanged?.Invoke(this, EventArgs.Empty);
    }

    void UpdateAccessibility()
    {
        SemanticProperties.SetDescription(_hit, AccessibleName ?? BodyText());
        _hit.IsChecked = IsChecked;
        RefreshVisuals();
    }
}

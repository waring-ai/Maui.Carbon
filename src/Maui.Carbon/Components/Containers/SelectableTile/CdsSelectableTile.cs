namespace Maui.Carbon;

/// <summary>
/// A tile that toggles a selected state, announced as a checkbox, for picking any number of cards from a set (add-ons,
/// plans, features with a description or price). A 16px checkbox glyph sits 16px from the top-right corner; selected
/// tiles get a 1px <c>layer-selected-inverse</c> border. Each tile is its own tab stop; click anywhere, Space or Enter
/// toggles it. For exactly one choice use <see cref="CdsRadioTile"/> in a <see cref="CdsTileGroup"/>.
/// </summary>
public class CdsSelectableTile : CdsTileBase
{
    /// <summary>Bindable property for <see cref="IsSelected"/>.</summary>
    public static readonly BindableProperty IsSelectedProperty = BindableProperty.Create(
        nameof(IsSelected), typeof(bool), typeof(CdsSelectableTile), false, BindingMode.TwoWay,
        propertyChanged: static (b, o, n) => ((CdsSelectableTile)b).OnSelectedChanged((bool)o, (bool)n));

    /// <summary>Bindable property for <see cref="AccessibleName"/>.</summary>
    public static readonly BindableProperty AccessibleNameProperty = BindableProperty.Create(
        nameof(AccessibleName), typeof(string), typeof(CdsSelectableTile), null,
        propertyChanged: static (b, _, _) => ((CdsSelectableTile)b).UpdateAccessibility());

    readonly CdsIcon _glyph = new() { Size = 16, HorizontalOptions = LayoutOptions.End, VerticalOptions = LayoutOptions.Start, Margin = new Thickness(0, CdsSpacing.Spacing05, CdsSpacing.Spacing05, 0) };
    readonly CdsHitTarget _hit;

    /// <summary>Creates an unselected tile.</summary>
    public CdsSelectableTile()
    {
        SealBody();
        BodyHost.Padding = new Thickness(CdsSpacing.Spacing05, CdsSpacing.Spacing05, CdsSpacing.Spacing09, CdsSpacing.Spacing05);
        AutomationProperties.SetExcludedWithChildren(_glyph, true);
        _hit = CreateHitTarget(CdsAccessibilityRole.Checkbox);
        Stack.Add(_glyph);
        Stack.Add(_hit);
        Loaded += (_, _) => UpdateAccessibility();
        UpdateAccessibility();
    }

    /// <summary>Raised when <see cref="IsSelected"/> changes (by the user or in code).</summary>
    public event EventHandler<CdsChoiceChangedEventArgs<bool>>? SelectedChanged;

    /// <summary>Whether the tile is selected (two-way).</summary>
    public bool IsSelected
    {
        get => (bool)GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    /// <summary>Overrides the accessible name, which is otherwise all of the body's text (put the title first).</summary>
    public string? AccessibleName
    {
        get => (string?)GetValue(AccessibleNameProperty);
        set => SetValue(AccessibleNameProperty, value);
    }

    /// <inheritdoc/>
    protected override CdsTileKind TileKind => CdsTileKind.Selectable;

    /// <inheritdoc/>
    protected override CdsInteractionState TokenState => IsSelected ? State | CdsInteractionState.Selected : State;

    /// <inheritdoc/>
    protected override double BorderThickness => 1;

    /// <inheritdoc/>
    protected override void OnActivated() => IsSelected = !IsSelected;

    /// <inheritdoc/>
    protected override void OnBodyReplaced() => UpdateAccessibility();

    /// <inheritdoc/>
    protected override void ApplyTileVisuals(CdsTileVisuals visuals)
    {
        // Selected: Carbon's filled checkbox with the check knocked out (checkbox--checked--filled), not the outline glyph.
        _glyph.Icon = IsSelected ? CdsIconName.CheckboxCheckedFilled : CdsIconName.Checkbox;
        SetColor(_glyph, CdsIcon.ColorProperty, visuals.Icon);
        _hit.IsEnabled = !Is(CdsInteractionState.Disabled);
    }

    void OnSelectedChanged(bool oldValue, bool newValue)
    {
        _hit.IsChecked = newValue;
        RefreshVisuals();
        SelectedChanged?.Invoke(this, new CdsChoiceChangedEventArgs<bool>(oldValue, newValue));
    }

    void UpdateAccessibility()
    {
        SemanticProperties.SetDescription(_hit, AccessibleName ?? BodyText());
        _hit.IsChecked = IsSelected;
        RefreshVisuals();
    }
}

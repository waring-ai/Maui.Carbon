namespace Maui.Carbon;

/// <summary>
/// A toggle tag the user can switch on and off, used for picking several options from a set of chips. Unselected it is
/// an outlined <c>layer-01</c> chip; selected it fills with <c>layer-selected-inverse</c>. A native toggle button: Tab
/// moves between tags, Enter and Space toggle. Wrap a set in a labelled group. There is no color type.
/// </summary>
public class CdsSelectableTag : CdsTag
{
    /// <summary>Bindable property for <see cref="IsSelected"/>.</summary>
    public static readonly BindableProperty IsSelectedProperty = BindableProperty.Create(
        nameof(IsSelected), typeof(bool), typeof(CdsSelectableTag), false, BindingMode.TwoWay,
        propertyChanged: static (b, _, n) => ((CdsSelectableTag)b).OnSelectedChanged((bool)n));

    CdsHitTarget? _hit;
    CdsIcon _iconView = null!;
    CdsText _labelView = null!;
    CdsFocusRing _ring = null!;

    /// <summary>Creates an unselected medium selectable tag.</summary>
    public CdsSelectableTag()
    {
    }

    /// <summary>Creates a selectable tag.</summary>
    public CdsSelectableTag(string text, bool selected = false) : this()
    {
        Text = text;
        IsSelected = selected;
    }

    /// <summary>Raised on every click, after the selection toggled.</summary>
    public event EventHandler? Clicked;

    /// <summary>Raised when <see cref="IsSelected"/> changes.</summary>
    public event EventHandler<ToggledEventArgs>? SelectedChanged;

    /// <summary>Whether the tag is on. Two-way; the user toggles it.</summary>
    public bool IsSelected
    {
        get => (bool)GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    /// <inheritdoc/>
    protected override bool SupportsDecorator => false;

    /// <inheritdoc/>
    protected override View BuildChrome(Border pill)
    {
        CdsTagParts.Rebuild(pill, Row, out _iconView, out _labelView);
        AutomationProperties.SetExcludedWithChildren(pill, true);
        pill.InputTransparent = true;
        var hit = CreateHitTarget(CdsAccessibilityRole.ToggleButton);
        _hit = hit;
        _ring = new CdsFocusRing(CdsFocusRingStyle.Offset, 16);
        var root = new Grid();
        root.Add(pill);
        root.Add(_ring);
        root.Add(hit);
        return root;
    }

    /// <inheritdoc/>
    protected override void OnActivated()
    {
        IsSelected = !IsSelected;
        Clicked?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc/>
    protected override void ApplyVisuals()
    {
        if (_iconView is null) return;
        var v = CdsInteractiveTagTokens.Selectable(IsSelected, State);
        SetColor(Pill, BackgroundColorProperty, v.Background);
        SetBrush(Pill, Border.StrokeProperty, v.Border);
        Pill.StrokeThickness = 1;
        SetColor(_labelView, Label.TextColorProperty, v.Foreground);
        SetColor(_iconView, CdsIcon.ColorProperty, v.Foreground);
        _ring.IsActive = Is(CdsInteractionState.FocusVisible);
        if (_hit is not null)
        {
            SemanticProperties.SetDescription(_hit, Text);
            _hit.IsChecked = IsSelected;
        }
    }

    void OnSelectedChanged(bool selected)
    {
        SetState(CdsInteractionState.Selected, selected);
        RefreshVisuals();
        SelectedChanged?.Invoke(this, new ToggledEventArgs(selected));
    }
}

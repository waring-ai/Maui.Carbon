namespace Maui.Carbon;

/// <summary>
/// One option of a <see cref="CdsRadioButtonGroup"/>: an 18px circle with a 1px <c>icon-primary</c> ring and a dot at
/// half size when selected, and a <c>body-compact-01</c> label. The circle and label are one click target; focus is a
/// 2px <c>focus</c> outline around the circle. Used alone it selects itself on activation.
/// </summary>
public class CdsRadioButton : CdsControl
{
    /// <summary>Bindable property for <see cref="Label"/>.</summary>
    public static readonly BindableProperty LabelProperty = BindableProperty.Create(
        nameof(Label), typeof(string), typeof(CdsRadioButton), null,
        propertyChanged: static (b, _, _) => ((CdsRadioButton)b).UpdateChrome());

    /// <summary>Bindable property for <see cref="Value"/>.</summary>
    public static readonly BindableProperty ValueProperty = BindableProperty.Create(
        nameof(Value), typeof(object), typeof(CdsRadioButton), null,
        propertyChanged: static (b, _, _) => ((CdsRadioButton)b).ValueChanged?.Invoke(b, EventArgs.Empty));

    /// <summary>Bindable property for <see cref="IsChecked"/>.</summary>
    public static readonly BindableProperty IsCheckedProperty = BindableProperty.Create(
        nameof(IsChecked), typeof(bool), typeof(CdsRadioButton), false, BindingMode.TwoWay,
        propertyChanged: static (b, _, n) => ((CdsRadioButton)b).OnCheckedChanged((bool)n));

    /// <summary>Bindable property for <see cref="LabelPosition"/>.</summary>
    public static readonly BindableProperty LabelPositionProperty = BindableProperty.Create(
        nameof(LabelPosition), typeof(CdsRadioLabelPosition), typeof(CdsRadioButton), CdsRadioLabelPosition.Right,
        propertyChanged: static (b, _, _) => ((CdsRadioButton)b).UpdateChrome());

    /// <summary>Bindable property for <see cref="HideLabel"/>.</summary>
    public static readonly BindableProperty HideLabelProperty = BindableProperty.Create(
        nameof(HideLabel), typeof(bool), typeof(CdsRadioButton), false,
        propertyChanged: static (b, _, _) => ((CdsRadioButton)b).UpdateChrome());

    /// <summary>Bindable property for <see cref="IsReadOnly"/>.</summary>
    public static readonly BindableProperty IsReadOnlyProperty = BindableProperty.Create(
        nameof(IsReadOnly), typeof(bool), typeof(CdsRadioButton), false,
        propertyChanged: static (b, _, _) => ((CdsRadioButton)b).RefreshVisuals());

    readonly Grid _root = new() { ColumnSpacing = 0, RowSpacing = 0 };
    readonly Grid _circleHost = new() { WidthRequest = 18, HeightRequest = 18, VerticalOptions = LayoutOptions.Start, HorizontalOptions = LayoutOptions.Start };
    readonly CdsRadioGlyph _circle = new();
    readonly CdsFocusRing _ring = new(CdsFocusRingStyle.Offset, CdsRadius.Circle(18));
    readonly CdsText _label = new() { LineBreakMode = LineBreakMode.WordWrap, VerticalOptions = LayoutOptions.Start };
    readonly CdsHitTarget _hit;
    CdsInteractionState _inherited;

    /// <summary>Creates an unselected radio button.</summary>
    public CdsRadioButton()
    {
        _root.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        _root.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        _circleHost.Add(_circle);
        _circleHost.Add(_ring);
        _label.InputTransparent = true;
        AutomationProperties.SetExcludedWithChildren(_circleHost, true);
        AutomationProperties.SetExcludedWithChildren(_label, true);
        _hit = CreateHitTarget(CdsAccessibilityRole.Radio);
        _root.Add(_circleHost, 0, 0);
        _root.Add(_label, 1, 0);
        _root.Add(_hit, 0, 0);
        Grid.SetColumnSpan(_hit, 2);
        SetChrome(_root);
        UpdateChrome();
    }

    /// <summary>Creates a radio button with a label and a value.</summary>
    public CdsRadioButton(string label, object? value = null) : this()
    {
        Label = label;
        Value = value ?? label;
    }

    /// <summary>Raised when <see cref="IsChecked"/> changes.</summary>
    public event EventHandler<CheckedChangedEventArgs>? CheckedChanged;

    internal event EventHandler? UserSelected;
    internal event EventHandler? CheckedPropertyChanged;
    internal event EventHandler? ValueChanged;

    /// <summary>The visible label and accessible name; keep it under about three words, sentence case, never truncated.</summary>
    public string? Label
    {
        get => (string?)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <summary>The value the owning group reports in <see cref="CdsRadioButtonGroup.SelectedValue"/> when this radio is selected.</summary>
    public object? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    /// <summary>Whether this radio is selected (two-way). In a group the group keeps exactly one selected.</summary>
    public bool IsChecked
    {
        get => (bool)GetValue(IsCheckedProperty);
        set => SetValue(IsCheckedProperty, value);
    }

    /// <summary>Label right of the circle (default) or left of it.</summary>
    public CdsRadioLabelPosition LabelPosition
    {
        get => (CdsRadioLabelPosition)GetValue(LabelPositionProperty);
        set => SetValue(LabelPositionProperty, value);
    }

    /// <summary>Hides the label visually; it stays the accessible name.</summary>
    public bool HideLabel
    {
        get => (bool)GetValue(HideLabelProperty);
        set => SetValue(HideLabelProperty, value);
    }

    /// <summary>Read-only: focusable and announced, but the selection cannot change; <c>icon-disabled</c> ring.</summary>
    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    internal bool IsEffectivelyDisabled => Is(CdsInteractionState.Disabled) || _inherited.HasFlag(CdsInteractionState.Disabled);

    internal bool IsEffectivelyReadOnly => IsReadOnly || _inherited.HasFlag(CdsInteractionState.ReadOnly);

    internal void SetInherited(CdsInteractionState flags, CdsRadioLabelPosition? position)
    {
        _inherited = flags;
        if (position is { } p && p != _groupPosition)
        {
            _groupPosition = p;
            UpdateChrome();
        }
        RefreshVisuals();
    }

    /// <summary>Moves keyboard focus to the radio.</summary>
    internal bool FocusRadio() => _hit.Focus();

    CdsRadioLabelPosition? _groupPosition;

    /// <inheritdoc/>
    protected override void OnActivated()
    {
        if (IsEffectivelyDisabled || IsEffectivelyReadOnly) return;
        if (!IsChecked) IsChecked = true;
        UserSelected?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc/>
    protected override void ApplyVisuals()
    {
        var state = State | _inherited;
        if (IsReadOnly) state |= CdsInteractionState.ReadOnly;
        var v = CdsRadioTokens.Resolve(state);
        SetColor(_circle, CdsRadioGlyph.RingColorProperty, v.Ring);
        SetColor(_circle, CdsRadioGlyph.DotColorProperty, v.Dot);
        _circle.ShowDot = IsChecked;
        SetColor(_label, Microsoft.Maui.Controls.Label.TextColorProperty, v.Label);
        _ring.IsActive = Is(CdsInteractionState.FocusVisible);
        _hit.IsEnabled = !state.HasFlag(CdsInteractionState.Disabled);
    }

    void UpdateChrome()
    {
        _label.Text = Label;
        _label.IsVisible = !HideLabel && !string.IsNullOrEmpty(Label);
        var left = (_groupPosition ?? LabelPosition) == CdsRadioLabelPosition.Left;
        if (left)
        {
            Grid.SetColumn(_label, 0);
            Grid.SetColumn(_circleHost, 1);
            _root.ColumnDefinitions[0].Width = GridLength.Star;
            _root.ColumnDefinitions[1].Width = GridLength.Auto;
            _label.Margin = new Thickness(0, 2, CdsSpacing.Spacing03, 0);
            _circleHost.Margin = new Thickness(0, 1, 2, 2);
            _label.HorizontalTextAlignment = TextAlignment.End;
        }
        else
        {
            Grid.SetColumn(_circleHost, 0);
            Grid.SetColumn(_label, 1);
            _root.ColumnDefinitions[0].Width = GridLength.Auto;
            _root.ColumnDefinitions[1].Width = GridLength.Star;
            _circleHost.Margin = new Thickness(2, 1, _label.IsVisible ? 10 : 2, 2);
            _label.Margin = new Thickness(0, 2, 0, 0);
            _label.HorizontalTextAlignment = TextAlignment.Start;
        }
        SemanticProperties.SetDescription(_hit, Label);
        RefreshVisuals();
    }

    void OnCheckedChanged(bool value)
    {
        _hit.IsChecked = value;
        RefreshVisuals();
        CheckedChanged?.Invoke(this, new CheckedChangedEventArgs(value));
        CheckedPropertyChanged?.Invoke(this, EventArgs.Empty);
    }
}

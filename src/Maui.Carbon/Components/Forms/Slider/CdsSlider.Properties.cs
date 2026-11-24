namespace Maui.Carbon;

/// <summary>Event data for <see cref="CdsSlider.ValueChanged"/>.</summary>
public sealed class CdsSliderChangedEventArgs : EventArgs
{
    /// <summary>Creates the event data.</summary>
    public CdsSliderChangedEventArgs(double value, double upperValue)
    {
        Value = value;
        UpperValue = upperValue;
    }

    /// <summary>The (lower) value.</summary>
    public double Value { get; }

    /// <summary>The upper value of a range slider.</summary>
    public double UpperValue { get; }
}

public partial class CdsSlider
{
    /// <summary>Bindable property for <see cref="Value"/>.</summary>
    public static readonly BindableProperty ValueProperty = BindableProperty.Create(
        nameof(Value), typeof(double), typeof(CdsSlider), 0d, BindingMode.TwoWay,
        coerceValue: static (b, v) => ((CdsSlider)b).CoerceToRange((double)v),
        propertyChanged: static (b, _, _) => ((CdsSlider)b).OnValuesChanged());

    /// <summary>Bindable property for <see cref="UpperValue"/>.</summary>
    public static readonly BindableProperty UpperValueProperty = BindableProperty.Create(
        nameof(UpperValue), typeof(double), typeof(CdsSlider), 100d, BindingMode.TwoWay,
        coerceValue: static (b, v) => ((CdsSlider)b).CoerceToRange((double)v),
        propertyChanged: static (b, _, _) => ((CdsSlider)b).OnValuesChanged());

    /// <summary>Bindable property for <see cref="IsRange"/>.</summary>
    public static readonly BindableProperty IsRangeProperty = BindableProperty.Create(
        nameof(IsRange), typeof(bool), typeof(CdsSlider), false,
        propertyChanged: static (b, _, _) => ((CdsSlider)b).UpdateLayout());

    /// <summary>Bindable property for <see cref="Min"/>.</summary>
    public static readonly BindableProperty MinProperty = BindableProperty.Create(
        nameof(Min), typeof(double), typeof(CdsSlider), 0d,
        propertyChanged: static (b, _, _) => ((CdsSlider)b).OnBoundsChanged());

    /// <summary>Bindable property for <see cref="Max"/>.</summary>
    public static readonly BindableProperty MaxProperty = BindableProperty.Create(
        nameof(Max), typeof(double), typeof(CdsSlider), 100d,
        propertyChanged: static (b, _, _) => ((CdsSlider)b).OnBoundsChanged());

    /// <summary>Bindable property for <see cref="Step"/>.</summary>
    public static readonly BindableProperty StepProperty = BindableProperty.Create(
        nameof(Step), typeof(double), typeof(CdsSlider), 1d,
        propertyChanged: static (b, _, _) => ((CdsSlider)b).OnBoundsChanged());

    /// <summary>Bindable property for <see cref="StepMultiplier"/>.</summary>
    public static readonly BindableProperty StepMultiplierProperty = BindableProperty.Create(
        nameof(StepMultiplier), typeof(double), typeof(CdsSlider), 4d);

    /// <summary>Bindable property for <see cref="Label"/>.</summary>
    public static readonly BindableProperty LabelProperty = BindableProperty.Create(
        nameof(Label), typeof(string), typeof(CdsSlider), null,
        propertyChanged: static (b, _, _) => ((CdsSlider)b).UpdateLayout());

    /// <summary>Bindable property for <see cref="HideLabel"/>.</summary>
    public static readonly BindableProperty HideLabelProperty = BindableProperty.Create(
        nameof(HideLabel), typeof(bool), typeof(CdsSlider), false,
        propertyChanged: static (b, _, _) => ((CdsSlider)b).UpdateLayout());

    /// <summary>Bindable property for <see cref="MinLabel"/>.</summary>
    public static readonly BindableProperty MinLabelProperty = BindableProperty.Create(
        nameof(MinLabel), typeof(string), typeof(CdsSlider), null,
        propertyChanged: static (b, _, _) => ((CdsSlider)b).UpdateTexts());

    /// <summary>Bindable property for <see cref="MaxLabel"/>.</summary>
    public static readonly BindableProperty MaxLabelProperty = BindableProperty.Create(
        nameof(MaxLabel), typeof(string), typeof(CdsSlider), null,
        propertyChanged: static (b, _, _) => ((CdsSlider)b).UpdateTexts());

    /// <summary>Bindable property for <see cref="ValueFormatter"/>.</summary>
    public static readonly BindableProperty ValueFormatterProperty = BindableProperty.Create(
        nameof(ValueFormatter), typeof(Func<double, string>), typeof(CdsSlider), null,
        propertyChanged: static (b, _, _) => ((CdsSlider)b).UpdateTexts());

    /// <summary>Bindable property for <see cref="HideTextInput"/>.</summary>
    public static readonly BindableProperty HideTextInputProperty = BindableProperty.Create(
        nameof(HideTextInput), typeof(bool), typeof(CdsSlider), false,
        propertyChanged: static (b, _, _) => ((CdsSlider)b).UpdateLayout());

    /// <summary>Bindable property for <see cref="InputLabel"/>.</summary>
    public static readonly BindableProperty InputLabelProperty = BindableProperty.Create(
        nameof(InputLabel), typeof(string), typeof(CdsSlider), null,
        propertyChanged: static (b, _, _) => ((CdsSlider)b).UpdateAccessibility());

    /// <summary>Bindable property for <see cref="InputLabelUpper"/>.</summary>
    public static readonly BindableProperty InputLabelUpperProperty = BindableProperty.Create(
        nameof(InputLabelUpper), typeof(string), typeof(CdsSlider), null,
        propertyChanged: static (b, _, _) => ((CdsSlider)b).UpdateAccessibility());

    /// <summary>Bindable property for <see cref="IsInvalid"/>.</summary>
    public static readonly BindableProperty IsInvalidProperty = BindableProperty.Create(
        nameof(IsInvalid), typeof(bool), typeof(CdsSlider), false,
        propertyChanged: static (b, _, _) => ((CdsSlider)b).RefreshVisuals());

    /// <summary>Bindable property for <see cref="InvalidText"/>.</summary>
    public static readonly BindableProperty InvalidTextProperty = BindableProperty.Create(
        nameof(InvalidText), typeof(string), typeof(CdsSlider), "Value is out of range",
        propertyChanged: static (b, _, _) => ((CdsSlider)b).RefreshVisuals());

    /// <summary>Bindable property for <see cref="IsWarning"/>.</summary>
    public static readonly BindableProperty IsWarningProperty = BindableProperty.Create(
        nameof(IsWarning), typeof(bool), typeof(CdsSlider), false,
        propertyChanged: static (b, _, _) => ((CdsSlider)b).RefreshVisuals());

    /// <summary>Bindable property for <see cref="WarnText"/>.</summary>
    public static readonly BindableProperty WarnTextProperty = BindableProperty.Create(
        nameof(WarnText), typeof(string), typeof(CdsSlider), null,
        propertyChanged: static (b, _, _) => ((CdsSlider)b).RefreshVisuals());

    /// <summary>Bindable property for <see cref="IsReadOnly"/>.</summary>
    public static readonly BindableProperty IsReadOnlyProperty = BindableProperty.Create(
        nameof(IsReadOnly), typeof(bool), typeof(CdsSlider), false,
        propertyChanged: static (b, _, _) => ((CdsSlider)b).UpdateLayout());

    /// <summary>The value, or the lower value of a range (two-way). Clamped to <see cref="Min"/> and <see cref="Max"/>.</summary>
    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    /// <summary>The upper value of a range slider (two-way); only used when <see cref="IsRange"/> is on.</summary>
    public double UpperValue
    {
        get => (double)GetValue(UpperValueProperty);
        set => SetValue(UpperValueProperty, value);
    }

    /// <summary>Two handles (<see cref="Value"/> and <see cref="UpperValue"/>) choose a range; the lower box sits left of the min label, the upper box right of the max label.</summary>
    public bool IsRange
    {
        get => (bool)GetValue(IsRangeProperty);
        set => SetValue(IsRangeProperty, value);
    }

    /// <summary>The smallest value. Default 0.</summary>
    public double Min
    {
        get => (double)GetValue(MinProperty);
        set => SetValue(MinProperty, value);
    }

    /// <summary>The largest value. Default 100.</summary>
    public double Max
    {
        get => (double)GetValue(MaxProperty);
        set => SetValue(MaxProperty, value);
    }

    /// <summary>The increment of keyboard, drag and tap changes, counted from <see cref="Min"/>. Default 1; avoid large, uneven steps.</summary>
    public double Step
    {
        get => (double)GetValue(StepProperty);
        set => SetValue(StepProperty, value);
    }

    /// <summary>Shift+arrow, PageUp and PageDown move by <see cref="Step"/> times this. Default 4.</summary>
    public double StepMultiplier
    {
        get => (double)GetValue(StepMultiplierProperty);
        set => SetValue(StepMultiplierProperty, value);
    }

    /// <summary>The name of the value with its unit ("Size (GB)"); the accessible name of a single slider.</summary>
    public string? Label
    {
        get => (string?)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <summary>Hides the label visually; it stays the accessible name.</summary>
    public bool HideLabel
    {
        get => (bool)GetValue(HideLabelProperty);
        set => SetValue(HideLabelProperty, value);
    }

    /// <summary>Appended to the min label by the default formatter ("GB").</summary>
    public string? MinLabel
    {
        get => (string?)GetValue(MinLabelProperty);
        set => SetValue(MinLabelProperty, value);
    }

    /// <summary>Appended to the max label by the default formatter ("GB").</summary>
    public string? MaxLabel
    {
        get => (string?)GetValue(MaxLabelProperty);
        set => SetValue(MaxLabelProperty, value);
    }

    /// <summary>Formats the min and max labels and the spoken value (Carbon's <c>formatLabel</c>); overrides <see cref="MinLabel"/> and <see cref="MaxLabel"/>.</summary>
    public Func<double, string>? ValueFormatter
    {
        get => (Func<double, string>?)GetValue(ValueFormatterProperty);
        set => SetValue(ValueFormatterProperty, value);
    }

    /// <summary>Removes the number box(es).</summary>
    public bool HideTextInput
    {
        get => (bool)GetValue(HideTextInputProperty);
        set => SetValue(HideTextInputProperty, value);
    }

    /// <summary>Name of the (lower) number box and handle; for a range the default is "Lower bound".</summary>
    public string? InputLabel
    {
        get => (string?)GetValue(InputLabelProperty);
        set => SetValue(InputLabelProperty, value);
    }

    /// <summary>Name of the upper number box and handle of a range; default "Upper bound".</summary>
    public string? InputLabelUpper
    {
        get => (string?)GetValue(InputLabelUpperProperty);
        set => SetValue(InputLabelUpperProperty, value);
    }

    /// <summary>Error state: <c>support-error</c> box outline and icon with <see cref="InvalidText"/>. A typed value outside the range also shows it until it is clamped on blur. Ignored while disabled or read-only.</summary>
    public bool IsInvalid
    {
        get => (bool)GetValue(IsInvalidProperty);
        set => SetValue(IsInvalidProperty, value);
    }

    /// <summary>The error message. Default "Value is out of range".</summary>
    public string? InvalidText
    {
        get => (string?)GetValue(InvalidTextProperty);
        set => SetValue(InvalidTextProperty, value);
    }

    /// <summary>Warning state: warning icon in the box and <see cref="WarnText"/>. Ignored when invalid, disabled or read-only.</summary>
    public bool IsWarning
    {
        get => (bool)GetValue(IsWarningProperty);
        set => SetValue(IsWarningProperty, value);
    }

    /// <summary>The warning message.</summary>
    public string? WarnText
    {
        get => (string?)GetValue(WarnTextProperty);
        set => SetValue(WarnTextProperty, value);
    }

    /// <summary>Read-only: thumbs are hidden (and not focusable); the value is still shown and its box stays focusable.</summary>
    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }
}

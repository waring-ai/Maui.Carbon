namespace Maui.Carbon;

public partial class CdsNumberInput
{
    /// <summary>Bindable property for <see cref="Value"/>.</summary>
    public static readonly BindableProperty ValueProperty = BindableProperty.Create(
        nameof(Value), typeof(double?), typeof(CdsNumberInput), 0d, BindingMode.TwoWay,
        propertyChanged: static (b, _, n) => ((CdsNumberInput)b).OnValuePropertyChanged((double?)n));

    /// <summary>Bindable property for <see cref="Min"/>.</summary>
    public static readonly BindableProperty MinProperty = BindableProperty.Create(
        nameof(Min), typeof(double?), typeof(CdsNumberInput), null, propertyChanged: OnNumberChanged);

    /// <summary>Bindable property for <see cref="Max"/>.</summary>
    public static readonly BindableProperty MaxProperty = BindableProperty.Create(
        nameof(Max), typeof(double?), typeof(CdsNumberInput), null, propertyChanged: OnNumberChanged);

    /// <summary>Bindable property for <see cref="Step"/>.</summary>
    public static readonly BindableProperty StepProperty = BindableProperty.Create(
        nameof(Step), typeof(double), typeof(CdsNumberInput), 1d, propertyChanged: OnNumberChanged,
        coerceValue: static (_, v) => (double)v > 0 ? v : 1d);

    /// <summary>Bindable property for <see cref="AllowEmpty"/>.</summary>
    public static readonly BindableProperty AllowEmptyProperty = BindableProperty.Create(
        nameof(AllowEmpty), typeof(bool), typeof(CdsNumberInput), false, propertyChanged: OnNumberChanged);

    /// <summary>Bindable property for <see cref="HideSteppers"/>.</summary>
    public static readonly BindableProperty HideSteppersProperty = BindableProperty.Create(
        nameof(HideSteppers), typeof(bool), typeof(CdsNumberInput), false, propertyChanged: OnNumberChanged);

    /// <summary>Bindable property for <see cref="Placeholder"/>.</summary>
    public static readonly BindableProperty PlaceholderProperty = BindableProperty.Create(
        nameof(Placeholder), typeof(string), typeof(CdsNumberInput), null, propertyChanged: OnNumberChanged);

    /// <summary>Bindable property for <see cref="IncrementLabel"/>.</summary>
    public static readonly BindableProperty IncrementLabelProperty = BindableProperty.Create(
        nameof(IncrementLabel), typeof(string), typeof(CdsNumberInput), "Increment number", propertyChanged: OnNumberChanged);

    /// <summary>Bindable property for <see cref="DecrementLabel"/>.</summary>
    public static readonly BindableProperty DecrementLabelProperty = BindableProperty.Create(
        nameof(DecrementLabel), typeof(string), typeof(CdsNumberInput), "Decrement number", propertyChanged: OnNumberChanged);

    /// <summary>The value (two-way); <c>null</c> when the field is empty. Default 0.</summary>
    public double? Value
    {
        get => (double?)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    /// <summary>The smallest allowed value; <c>null</c> for none.</summary>
    public double? Min
    {
        get => (double?)GetValue(MinProperty);
        set => SetValue(MinProperty, value);
    }

    /// <summary>The largest allowed value; <c>null</c> for none.</summary>
    public double? Max
    {
        get => (double?)GetValue(MaxProperty);
        set => SetValue(MaxProperty, value);
    }

    /// <summary>How much a stepper changes the value, and the decimal precision it keeps. Default 1.</summary>
    public double Step
    {
        get => (double)GetValue(StepProperty);
        set => SetValue(StepProperty, value);
    }

    /// <summary>Lets the field be empty without being invalid (otherwise empty is an error).</summary>
    public bool AllowEmpty
    {
        get => (bool)GetValue(AllowEmptyProperty);
        set => SetValue(AllowEmptyProperty, value);
    }

    /// <summary>Hides the − / + buttons; the value is typed or changed with Up / Down.</summary>
    public bool HideSteppers
    {
        get => (bool)GetValue(HideSteppersProperty);
        set => SetValue(HideSteppersProperty, value);
    }

    /// <summary>A format hint shown while the field is empty.</summary>
    public string? Placeholder
    {
        get => (string?)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    /// <summary>Accessible name of the + button. Default "Increment number".</summary>
    public string? IncrementLabel
    {
        get => (string?)GetValue(IncrementLabelProperty);
        set => SetValue(IncrementLabelProperty, value);
    }

    /// <summary>Accessible name of the − button. Default "Decrement number".</summary>
    public string? DecrementLabel
    {
        get => (string?)GetValue(DecrementLabelProperty);
        set => SetValue(DecrementLabelProperty, value);
    }

    static void OnNumberChanged(BindableObject b, object? o, object? n) => ((CdsNumberInput)b).UpdateChrome();
}

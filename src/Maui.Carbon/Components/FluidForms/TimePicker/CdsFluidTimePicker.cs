namespace Maui.Carbon;

/// <summary>
/// A time field for fluid forms: the time input and its AM/PM and time zone selects are 64px parts of one row separated by 1px dividers, each with its label inside; one message sits under all parts inside the outline. Use it inside a <see cref="CdsFluidForm"/>, usually next to a <see cref="CdsFluidDatePicker"/>.
/// It is a <see cref="CdsTimePicker"/> that always renders its fluid layout, even outside a <see cref="CdsFluidForm"/>;
/// every other property and event is the base control's.
/// </summary>
public class CdsFluidTimePicker : CdsTimePicker
{
    /// <summary>Creates a fluid TimePicker.</summary>
    public CdsFluidTimePicker()
    {
        Chrome.IsFluid = true;
    }
}

/// <summary>
/// The loading placeholder of <see cref="CdsFluidTimePicker"/> (Carbon's <c>FluidTimePickerSkeleton</c>): three 64px
/// <c>skeleton-background</c> parts split 25% / 25% / 50% like the time, AM/PM and time zone parts, or two parts 50/50 with
/// <see cref="IsOnlyTwo"/>, each with a label bar and a value bar and divided by 1px <c>skeleton-element</c> lines.
/// Decorative: not in the accessibility tree.
/// </summary>
public class CdsFluidTimePickerSkeleton : CdsControl
{
    /// <summary>Bindable property for <see cref="IsOnlyTwo"/>.</summary>
    public static readonly BindableProperty IsOnlyTwoProperty = BindableProperty.Create(
        nameof(IsOnlyTwo), typeof(bool), typeof(CdsFluidTimePickerSkeleton), false,
        propertyChanged: static (b, _, _) => ((CdsFluidTimePickerSkeleton)b).UpdateLayout());

    /// <summary>Creates a three-part skeleton.</summary>
    public CdsFluidTimePickerSkeleton()
    {
        AutomationProperties.SetIsInAccessibleTree(this, false);
        UpdateLayout();
    }

    /// <summary>Two parts 50/50 (a time picker with one select) instead of three. Default false.</summary>
    public bool IsOnlyTwo
    {
        get => (bool)GetValue(IsOnlyTwoProperty);
        set => SetValue(IsOnlyTwoProperty, value);
    }

    void UpdateLayout() => SetChrome(IsOnlyTwo ? FluidSkeleton.Parts(1, 1) : FluidSkeleton.Parts(1, 1, 2));
}

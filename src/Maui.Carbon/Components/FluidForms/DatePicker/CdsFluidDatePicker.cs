namespace Maui.Carbon;

/// <summary>
/// A date picker for fluid forms: each date field is 64px with its label inside, a range puts its two fields side by side with a 1px divider, and the calendar opens under the first field. Helper text and size are not shown; the message sits inside the first field. Use it inside a <see cref="CdsFluidForm"/>.
/// It is a <see cref="CdsDatePicker"/> that always renders its fluid layout, even outside a <see cref="CdsFluidForm"/>;
/// every other property and event is the base control's.
/// </summary>
public class CdsFluidDatePicker : CdsDatePicker
{
    /// <summary>Creates a fluid DatePicker.</summary>
    public CdsFluidDatePicker()
    {
        Chrome.IsFluid = true;
    }
}

/// <summary>
/// The loading placeholder of <see cref="CdsFluidDatePicker"/> (Carbon's <c>FluidDatePickerSkeleton</c>): one 64px
/// <c>skeleton-background</c> field with a label bar and a value bar, or two side by side split by a 1px
/// <c>skeleton-element</c> line when <see cref="Range"/>. Decorative: not in the accessibility tree.
/// </summary>
public class CdsFluidDatePickerSkeleton : CdsControl
{
    /// <summary>Bindable property for <see cref="Range"/>.</summary>
    public static readonly BindableProperty RangeProperty = BindableProperty.Create(
        nameof(Range), typeof(bool), typeof(CdsFluidDatePickerSkeleton), false,
        propertyChanged: static (b, _, _) => ((CdsFluidDatePickerSkeleton)b).UpdateLayout());

    /// <summary>Creates a single-field skeleton.</summary>
    public CdsFluidDatePickerSkeleton()
    {
        AutomationProperties.SetIsInAccessibleTree(this, false);
        UpdateLayout();
    }

    /// <summary>Two fields 50/50, for a range picker.</summary>
    public bool Range
    {
        get => (bool)GetValue(RangeProperty);
        set => SetValue(RangeProperty, value);
    }

    void UpdateLayout() => SetChrome(Range ? FluidSkeleton.Parts(1, 1) : FluidSkeleton.Parts(1));
}

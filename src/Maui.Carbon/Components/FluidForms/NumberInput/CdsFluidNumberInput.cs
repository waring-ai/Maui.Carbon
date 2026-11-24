namespace Maui.Carbon;

/// <summary>
/// A numeric field for fluid forms: the label sits inside the 64px field and the 40px decrement and increment steppers sit at the bottom end. Helper text and size are not shown. Use it inside a <see cref="CdsFluidForm"/>.
/// It is a <see cref="CdsNumberInput"/> that always renders its fluid layout, even outside a <see cref="CdsFluidForm"/>;
/// every other property and event is the base control's.
/// </summary>
public class CdsFluidNumberInput : CdsNumberInput
{
    /// <summary>Creates a fluid NumberInput.</summary>
    public CdsFluidNumberInput()
    {
        Chrome.IsFluid = true;
    }
}

/// <summary>
/// The loading placeholder of <see cref="CdsFluidNumberInput"/> (Carbon's <c>FluidNumberInputSkeleton</c>): a 64px <c>skeleton-background</c>
/// field with a <c>skeleton-element</c> bottom rule, a label bar and a value bar, matching the field it turns into. It is
/// decorative and stays out of the accessibility tree; announce the loading state elsewhere if it matters.
/// </summary>
public class CdsFluidNumberInputSkeleton : CdsControl
{
    /// <summary>Creates the skeleton.</summary>
    public CdsFluidNumberInputSkeleton()
    {
        AutomationProperties.SetIsInAccessibleTree(this, false);
        SetChrome(FluidSkeleton.Field(CdsFieldChrome.FluidFieldHeight));
    }
}

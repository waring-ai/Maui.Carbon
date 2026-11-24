namespace Maui.Carbon;

/// <summary>
/// A single-line text field for fluid forms: the label sits inside the 64px field (top start, 13 from the top) and the text below it. There is no helper text, size or inline mode; an invalid or warning message shows inside the field under a divider. Use it only inside a <see cref="CdsFluidForm"/> next to other fluid fields.
/// It is a <see cref="CdsTextInput"/> that always renders its fluid layout, even outside a <see cref="CdsFluidForm"/>;
/// every other property and event is the base control's.
/// </summary>
public class CdsFluidTextInput : CdsTextInput
{
    /// <summary>Creates a fluid TextInput.</summary>
    public CdsFluidTextInput()
    {
        Chrome.IsFluid = true;
    }
}

/// <summary>
/// The loading placeholder of <see cref="CdsFluidTextInput"/> (Carbon's <c>FluidTextInputSkeleton</c>): a 64px <c>skeleton-background</c>
/// field with a <c>skeleton-element</c> bottom rule, a label bar and a value bar, matching the field it turns into. It is
/// decorative and stays out of the accessibility tree; announce the loading state elsewhere if it matters.
/// </summary>
public class CdsFluidTextInputSkeleton : CdsControl
{
    /// <summary>Creates the skeleton.</summary>
    public CdsFluidTextInputSkeleton()
    {
        AutomationProperties.SetIsInAccessibleTree(this, false);
        SetChrome(FluidSkeleton.Field(CdsFieldChrome.FluidFieldHeight));
    }
}

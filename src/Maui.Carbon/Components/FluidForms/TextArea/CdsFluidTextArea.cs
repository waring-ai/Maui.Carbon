namespace Maui.Carbon;

/// <summary>
/// A multi-line text field for fluid forms: the label sits inside the field above the editor and the counter at the label row's end. Helper text and size are not shown; messages sit inside the field. Use it inside a <see cref="CdsFluidForm"/>.
/// It is a <see cref="CdsTextArea"/> that always renders its fluid layout, even outside a <see cref="CdsFluidForm"/>;
/// every other property and event is the base control's.
/// </summary>
public class CdsFluidTextArea : CdsTextArea
{
    /// <summary>Creates a fluid TextArea.</summary>
    public CdsFluidTextArea()
    {
        Chrome.IsFluid = true;
    }
}

/// <summary>
/// The loading placeholder of <see cref="CdsFluidTextArea"/> (Carbon's <c>FluidTextAreaSkeleton</c>): a 128px <c>skeleton-background</c>
/// field with a <c>skeleton-element</c> bottom rule, a label bar and a value bar, matching the field it turns into. It is
/// decorative and stays out of the accessibility tree; announce the loading state elsewhere if it matters.
/// </summary>
public class CdsFluidTextAreaSkeleton : CdsControl
{
    /// <summary>Height of the skeleton field: 8rem.</summary>
    internal const double SkeletonHeight = 128;

    /// <summary>Creates the skeleton.</summary>
    public CdsFluidTextAreaSkeleton()
    {
        AutomationProperties.SetIsInAccessibleTree(this, false);
        SetChrome(FluidSkeleton.Field(SkeletonHeight));
    }
}

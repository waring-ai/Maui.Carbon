namespace Maui.Carbon;

/// <summary>
/// A select for fluid forms: the label sits inside the 64px field above the native picker. Helper text, size and inline mode are not shown. Use it inside a <see cref="CdsFluidForm"/>.
/// It is a <see cref="CdsSelect"/> that always renders its fluid layout, even outside a <see cref="CdsFluidForm"/>;
/// every other property and event is the base control's.
/// </summary>
public class CdsFluidSelect : CdsSelect
{
    /// <summary>Creates a fluid Select.</summary>
    public CdsFluidSelect()
    {
        Chrome.IsFluid = true;
    }
}

/// <summary>
/// The loading placeholder of <see cref="CdsFluidSelect"/> (Carbon's <c>FluidSelectSkeleton</c>): a 64px <c>skeleton-background</c>
/// field with a <c>skeleton-element</c> bottom rule, a label bar and a value bar, matching the field it turns into. It is
/// decorative and stays out of the accessibility tree; announce the loading state elsewhere if it matters.
/// </summary>
public class CdsFluidSelectSkeleton : CdsControl
{
    /// <summary>Creates the skeleton.</summary>
    public CdsFluidSelectSkeleton()
    {
        AutomationProperties.SetIsInAccessibleTree(this, false);
        SetChrome(FluidSkeleton.Field(CdsFieldChrome.FluidFieldHeight));
    }
}

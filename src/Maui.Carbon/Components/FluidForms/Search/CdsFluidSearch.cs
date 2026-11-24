namespace Maui.Carbon;

/// <summary>
/// A search field for fluid forms and filter bars: the label sits inside the 64px field, the magnifier at the bottom end and the clear button beside it. Like Carbon's FluidSearch it is never expandable and has no invalid or read-only look.
/// It is a <see cref="CdsSearch"/> that always renders its fluid layout, even outside a <see cref="CdsFluidForm"/>;
/// every other property and event is the base control's.
/// </summary>
public class CdsFluidSearch : CdsSearch
{
    /// <summary>Creates a fluid Search.</summary>
    public CdsFluidSearch()
    {
        Chrome.IsFluid = true;
    }
}

/// <summary>
/// The loading placeholder of <see cref="CdsFluidSearch"/> (Carbon's <c>FluidSearchSkeleton</c>): a 64px <c>skeleton-background</c>
/// field with a <c>skeleton-element</c> bottom rule, a label bar and a value bar, matching the field it turns into. It is
/// decorative and stays out of the accessibility tree; announce the loading state elsewhere if it matters.
/// </summary>
public class CdsFluidSearchSkeleton : CdsControl
{
    /// <summary>Creates the skeleton.</summary>
    public CdsFluidSearchSkeleton()
    {
        AutomationProperties.SetIsInAccessibleTree(this, false);
        SetChrome(FluidSkeleton.Field(CdsFieldChrome.FluidFieldHeight));
    }
}

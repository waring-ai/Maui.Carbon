namespace Maui.Carbon;

/// <summary>
/// A dropdown for fluid forms: the label sits inside the top of a 64px field and the chosen item below it; the open list has 64px rows (40px with <see cref="CdsListBox.IsCondensed"/>) and is at most 220px tall. Helper text, size and the inline type are not shown. Use it inside a <see cref="CdsFluidForm"/>.
/// It is a <see cref="CdsDropdown"/> that always renders its fluid layout, even outside a <see cref="CdsFluidForm"/>;
/// every other property and event is the base control's.
/// </summary>
public class CdsFluidDropdown : CdsDropdown
{
    /// <summary>Creates a fluid Dropdown.</summary>
    public CdsFluidDropdown()
    {
        Chrome.IsFluid = true;
    }
}

/// <summary>
/// The loading placeholder of <see cref="CdsFluidDropdown"/> (Carbon's <c>FluidDropdownSkeleton</c>): a 64px <c>skeleton-background</c>
/// field with a <c>skeleton-element</c> bottom rule, a label bar and a value bar, matching the field it turns into. It is
/// decorative and stays out of the accessibility tree; announce the loading state elsewhere if it matters.
/// </summary>
public class CdsFluidDropdownSkeleton : CdsControl
{
    /// <summary>Creates the skeleton.</summary>
    public CdsFluidDropdownSkeleton()
    {
        AutomationProperties.SetIsInAccessibleTree(this, false);
        SetChrome(FluidSkeleton.Field(CdsFieldChrome.FluidFieldHeight));
    }
}

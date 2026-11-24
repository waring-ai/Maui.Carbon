namespace Maui.Carbon;

/// <summary>
/// A multi-select for fluid forms: a 64px field with the label inside it, the count tag and the chevron; the open list has 64px checkbox rows (40px with <see cref="CdsListBox.IsCondensed"/>) and is at most 220px tall. Helper text, size and the inline type are not shown. Use it inside a <see cref="CdsFluidForm"/>.
/// It is a <see cref="CdsMultiSelect"/> that always renders its fluid layout, even outside a <see cref="CdsFluidForm"/>;
/// every other property and event is the base control's.
/// </summary>
public class CdsFluidMultiSelect : CdsMultiSelect
{
    /// <summary>Creates a fluid MultiSelect.</summary>
    public CdsFluidMultiSelect()
    {
        Chrome.IsFluid = true;
    }
}

/// <summary>
/// The loading placeholder of <see cref="CdsFluidMultiSelect"/> (Carbon's <c>FluidMultiSelectSkeleton</c>): a 64px <c>skeleton-background</c>
/// field with a <c>skeleton-element</c> bottom rule, a label bar and a value bar, matching the field it turns into. It is
/// decorative and stays out of the accessibility tree; announce the loading state elsewhere if it matters.
/// </summary>
public class CdsFluidMultiSelectSkeleton : CdsControl
{
    /// <summary>Creates the skeleton.</summary>
    public CdsFluidMultiSelectSkeleton()
    {
        AutomationProperties.SetIsInAccessibleTree(this, false);
        SetChrome(FluidSkeleton.Field(CdsFieldChrome.FluidFieldHeight));
    }
}

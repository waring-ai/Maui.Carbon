namespace Maui.Carbon;

/// <summary>
/// A combo box for fluid forms: the label sits inside the 64px field above the text entry; the open list has 64px rows (40px with <see cref="CdsListBox.IsCondensed"/>) and is at most 220px tall. Helper text and size are not shown. Use it inside a <see cref="CdsFluidForm"/>.
/// It is a <see cref="CdsComboBox"/> that always renders its fluid layout, even outside a <see cref="CdsFluidForm"/>;
/// every other property and event is the base control's.
/// </summary>
public class CdsFluidComboBox : CdsComboBox
{
    /// <summary>Creates a fluid ComboBox.</summary>
    public CdsFluidComboBox()
    {
        Chrome.IsFluid = true;
    }
}

/// <summary>
/// The loading placeholder of <see cref="CdsFluidComboBox"/> (Carbon's <c>FluidComboBoxSkeleton</c>): a 64px <c>skeleton-background</c>
/// field with a <c>skeleton-element</c> bottom rule, a label bar and a value bar, matching the field it turns into. It is
/// decorative and stays out of the accessibility tree; announce the loading state elsewhere if it matters.
/// </summary>
public class CdsFluidComboBoxSkeleton : CdsControl
{
    /// <summary>Creates the skeleton.</summary>
    public CdsFluidComboBoxSkeleton()
    {
        AutomationProperties.SetIsInAccessibleTree(this, false);
        SetChrome(FluidSkeleton.Field(CdsFieldChrome.FluidFieldHeight));
    }
}

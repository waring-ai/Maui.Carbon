namespace Maui.Carbon;

/// <summary>
/// A multi-select for fluid forms: a 64px field with the label inside it, the count tag and the chevron; the open list has 64px checkbox rows. Helper text, size and the inline type are not shown. Use it inside a <see cref="CdsFluidForm"/>.
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

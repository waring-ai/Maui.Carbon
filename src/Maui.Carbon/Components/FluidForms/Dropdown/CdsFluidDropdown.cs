namespace Maui.Carbon;

/// <summary>
/// A dropdown for fluid forms: the label sits inside the top of a 64px field and the chosen item below it; the open list has 64px rows. Helper text, size and the inline type are not shown. Use it inside a <see cref="CdsFluidForm"/>.
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

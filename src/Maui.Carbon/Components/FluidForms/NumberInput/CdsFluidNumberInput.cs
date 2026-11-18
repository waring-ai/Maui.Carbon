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

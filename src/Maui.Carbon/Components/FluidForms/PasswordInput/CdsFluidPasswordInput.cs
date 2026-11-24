namespace Maui.Carbon;

/// <summary>
/// The fluid password input (Carbon's <c>FluidTextInput isPassword</c>): a <see cref="CdsFluidTextInput"/>-style field
/// that masks its value, with a 32x32 show/hide toggle 26px from the top and 8px from the end of the 64px field. Use it only inside a
/// <see cref="CdsFluidForm"/> next to other fluid fields; fluid fields have no helper text, so put the password rules in
/// the label or validate on blur and say what is wrong in <see cref="CdsTextInput.InvalidText"/>.
/// It is a <see cref="CdsPasswordInput"/> that always renders its fluid layout, even outside a <see cref="CdsFluidForm"/>;
/// every other property and event is the base control's.
/// </summary>
public class CdsFluidPasswordInput : CdsPasswordInput
{
    /// <summary>Creates a fluid password input.</summary>
    public CdsFluidPasswordInput()
    {
        Chrome.IsFluid = true;
    }
}

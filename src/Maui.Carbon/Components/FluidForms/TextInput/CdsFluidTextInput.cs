namespace Maui.Carbon;

/// <summary>
/// A single-line text field for fluid forms: the label sits inside the 64px field (top start, 13 from the top) and the text below it. There is no helper text, size or inline mode; an invalid or warning message shows inside the field under a divider. Use it only inside a <see cref="CdsFluidForm"/> next to other fluid fields.
/// It is a <see cref="CdsTextInput"/> that always renders its fluid layout, even outside a <see cref="CdsFluidForm"/>;
/// every other property and event is the base control's.
/// </summary>
public class CdsFluidTextInput : CdsTextInput
{
    /// <summary>Creates a fluid TextInput.</summary>
    public CdsFluidTextInput()
    {
        Chrome.IsFluid = true;
    }
}

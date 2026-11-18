namespace Maui.Carbon;

/// <summary>
/// A multi-line text field for fluid forms: the label sits inside the field above the editor and the counter at the label row's end. Helper text and size are not shown; messages sit inside the field. Use it inside a <see cref="CdsFluidForm"/>.
/// It is a <see cref="CdsTextArea"/> that always renders its fluid layout, even outside a <see cref="CdsFluidForm"/>;
/// every other property and event is the base control's.
/// </summary>
public class CdsFluidTextArea : CdsTextArea
{
    /// <summary>Creates a fluid TextArea.</summary>
    public CdsFluidTextArea()
    {
        Chrome.IsFluid = true;
    }
}

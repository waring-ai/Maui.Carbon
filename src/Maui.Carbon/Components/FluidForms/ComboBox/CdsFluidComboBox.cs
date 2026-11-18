namespace Maui.Carbon;

/// <summary>
/// A combo box for fluid forms: the label sits inside the 64px field above the text entry; the open list has 64px rows. Helper text and size are not shown. Use it inside a <see cref="CdsFluidForm"/>.
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

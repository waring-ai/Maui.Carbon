namespace Maui.Carbon;

/// <summary>
/// A select for fluid forms: the label sits inside the 64px field above the native picker. Helper text, size and inline mode are not shown. Use it inside a <see cref="CdsFluidForm"/>.
/// It is a <see cref="CdsSelect"/> that always renders its fluid layout, even outside a <see cref="CdsFluidForm"/>;
/// every other property and event is the base control's.
/// </summary>
public class CdsFluidSelect : CdsSelect
{
    /// <summary>Creates a fluid Select.</summary>
    public CdsFluidSelect()
    {
        Chrome.IsFluid = true;
    }
}

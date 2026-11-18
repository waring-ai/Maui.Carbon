namespace Maui.Carbon;

/// <summary>
/// A date picker for fluid forms: each date field is 64px with its label inside, a range puts its two fields side by side with a 1px divider, and the calendar opens under the first field. Helper text and size are not shown; the message sits inside the first field. Use it inside a <see cref="CdsFluidForm"/>.
/// It is a <see cref="CdsDatePicker"/> that always renders its fluid layout, even outside a <see cref="CdsFluidForm"/>;
/// every other property and event is the base control's.
/// </summary>
public class CdsFluidDatePicker : CdsDatePicker
{
    /// <summary>Creates a fluid DatePicker.</summary>
    public CdsFluidDatePicker()
    {
        Chrome.IsFluid = true;
    }
}

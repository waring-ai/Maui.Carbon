namespace Maui.Carbon;

/// <summary>
/// A time field for fluid forms: the time input and its AM/PM and time zone selects are 64px parts of one row separated by 1px dividers, each with its label inside; one message sits under all parts inside the outline. Use it inside a <see cref="CdsFluidForm"/>, usually next to a <see cref="CdsFluidDatePicker"/>.
/// It is a <see cref="CdsTimePicker"/> that always renders its fluid layout, even outside a <see cref="CdsFluidForm"/>;
/// every other property and event is the base control's.
/// </summary>
public class CdsFluidTimePicker : CdsTimePicker
{
    /// <summary>Creates a fluid TimePicker.</summary>
    public CdsFluidTimePicker()
    {
        Chrome.IsFluid = true;
    }
}

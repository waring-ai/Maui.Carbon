namespace Maui.Carbon;

/// <summary>
/// A search field for fluid forms and filter bars: the label sits inside the 64px field, the magnifier at the bottom end and the clear button beside it. Like Carbon's FluidSearch it is never expandable and has no invalid or read-only look.
/// It is a <see cref="CdsSearch"/> that always renders its fluid layout, even outside a <see cref="CdsFluidForm"/>;
/// every other property and event is the base control's.
/// </summary>
public class CdsFluidSearch : CdsSearch
{
    /// <summary>Creates a fluid Search.</summary>
    public CdsFluidSearch()
    {
        Chrome.IsFluid = true;
    }
}

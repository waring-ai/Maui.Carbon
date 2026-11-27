namespace Maui.Carbon;

/// <summary>What changed a thumb tooltip's inputs.</summary>
internal enum SliderTooltipSource
{
    Hover,
    Focus,
    Drag,
}

/// <summary>An open or close that is due after a delay.</summary>
internal readonly record struct SliderTooltipStep(bool Open, int DelayMs);

/// <summary>
/// When a slider thumb's value tooltip is open (Carbon's Tooltip.tsx): hover opens it after 100 ms, keyboard focus and
/// dragging open it at once, and it stays open while any of them lasts. Leaving the thumb or ending a drag closes it
/// after 300 ms (unless another input still holds it); blur closes it at once. Pure: the caller owns the timer.
/// </summary>
internal sealed class SliderTooltipModel
{
    public const int EnterDelayMs = 100;
    public const int LeaveDelayMs = 300;

    public bool Hover { get; private set; }

    public bool Focus { get; private set; }

    public bool Drag { get; private set; }

    /// <summary>True while any input wants the tooltip.</summary>
    public bool Wanted => Hover || Focus || Drag;

    /// <summary>Records an input and returns what is due, or null when the tooltip is already as wanted.</summary>
    public SliderTooltipStep? Set(SliderTooltipSource source, bool on, bool isOpen)
    {
        switch (source)
        {
            case SliderTooltipSource.Hover: Hover = on; break;
            case SliderTooltipSource.Focus: Focus = on; break;
            default: Drag = on; break;
        }
        if (Wanted)
        {
            if (isOpen) return null;
            return new SliderTooltipStep(true, Focus || Drag ? 0 : EnterDelayMs);
        }
        if (!isOpen) return null;
        return new SliderTooltipStep(false, source == SliderTooltipSource.Focus ? 0 : LeaveDelayMs);
    }

    /// <summary>Forgets every input.</summary>
    public void Reset() => Hover = Focus = Drag = false;
}

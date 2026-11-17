namespace Maui.Carbon;

/// <summary>How a <see cref="CdsDatePicker"/> takes dates.</summary>
public enum CdsDatePickerType
{
    /// <summary>A typed date field with no calendar: dates people know by heart or approximate past dates.</summary>
    Simple,

    /// <summary>One date from a calendar (or typed): scheduling a snapshot or a go-live. The default.</summary>
    Single,

    /// <summary>A start and end date in two fields sharing one calendar: a usage export or a report period.</summary>
    Range,
}

/// <summary>Data for <see cref="CdsDatePicker.DateChanged"/>.</summary>
public sealed class CdsDateChangedEventArgs : EventArgs
{
    /// <summary>Creates event data.</summary>
    public CdsDateChangedEventArgs(DateTime? startDate, DateTime? endDate)
    {
        StartDate = startDate;
        EndDate = endDate;
    }

    /// <summary>The date (single and simple) or the start of the range; <c>null</c> when cleared.</summary>
    public DateTime? StartDate { get; }

    /// <summary>The end of the range; <c>null</c> for single and simple pickers or while the end is open.</summary>
    public DateTime? EndDate { get; }
}

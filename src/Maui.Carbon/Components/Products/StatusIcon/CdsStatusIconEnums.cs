namespace Maui.Carbon;

/// <summary>The statuses a <see cref="CdsStatusIcon"/> can show, from most to least severe, then the activity states.</summary>
public enum CdsStatusIconKind
{
    /// <summary>Fatal: a circle with an x in <c>icon-primary</c> (black in light themes).</summary>
    Fatal,
    /// <summary>Critical: a circle with an x in <c>status-red</c>.</summary>
    Critical,
    /// <summary>Major warning: an inverted triangle with a dark "!" in <c>status-orange</c>.</summary>
    MajorWarning,
    /// <summary>Minor warning: a triangle with a dark "!" in <c>status-yellow</c>.</summary>
    MinorWarning,
    /// <summary>Undefined: a diamond with a bar in <c>status-purple</c>.</summary>
    Undefined,
    /// <summary>Unknown: a diamond with a "?" in <c>status-gray</c>.</summary>
    Unknown,
    /// <summary>Normal: a circle with a check in <c>status-green</c>.</summary>
    Normal,
    /// <summary>Information: a square with an "i" in <c>status-blue</c>.</summary>
    Info,
    /// <summary>In progress: a part-filled circle in <c>status-blue</c> that turns slowly (still under reduced motion).</summary>
    InProgress,
    /// <summary>Running: an outlined circle with a check in <c>status-green</c>.</summary>
    Running,
    /// <summary>Pending: a circle with three dots in <c>status-gray</c>.</summary>
    Pending,
}

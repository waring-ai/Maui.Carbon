namespace Maui.Carbon;

/// <summary>The pure rules of <see cref="CdsChecklist"/>.</summary>
internal static class ChecklistModel
{
    /// <summary>The chart shows only with both a value and a label (a value of 0 is valid).</summary>
    public static bool ShowsChart(double? value, string? label) => value is not null && !string.IsNullOrEmpty(label);

    /// <summary>The header (and with it the toggle) shows with a title or a chart.</summary>
    public static bool ShowsHeader(string? title, bool showsChart) => !string.IsNullOrEmpty(title) || showsChart;

    /// <summary>The progress arc's sweep in degrees: the value (0..1) times 360, clamped; NaN is 0.</summary>
    public static double Sweep(double value) => double.IsNaN(value) ? 0 : Math.Clamp(value * 360, 0, 360);

    /// <summary>The accessible name of a task: its label and status ("Connect a data source, Complete").</summary>
    public static string TaskName(string? label, string? status) =>
        string.IsNullOrEmpty(status) ? label ?? string.Empty : string.IsNullOrEmpty(label) ? status : $"{label}, {status}";

    /// <summary>Height at <paramref name="progress"/> (clamped to 0..1) between two heights.</summary>
    public static double HeightAt(double from, double to, double progress) => from + (to - from) * Math.Clamp(progress, 0, 1);

    /// <summary>Outer size of the progress chart (40) and the width of its ring (4: a 32px inner disc).</summary>
    public const double ChartSize = CdsSpacing.Spacing08;

    /// <summary>The ring width of the progress chart.</summary>
    public const double ChartRing = (CdsSpacing.Spacing08 - CdsSpacing.Spacing07) / 2;
}

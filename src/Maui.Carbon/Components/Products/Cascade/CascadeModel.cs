namespace Maui.Carbon;

/// <summary>
/// Pure timing of <see cref="CdsCascade"/>, from <c>_cascade.scss</c>: each element fades in from 3rem below over
/// <c>duration-moderate-02</c> with <c>standard-productive</c> easing, the n-th one 60 ms after the one before it, up to the
/// eighth (later ones start with the eighth).
/// </summary>
internal static class CascadeModel
{
    /// <summary>The gap between two elements' starts, in milliseconds.</summary>
    public const int Step = 60;

    /// <summary>The number of elements that get their own delay; the rest share the last one.</summary>
    public const int MaxStaggered = 8;

    /// <summary>How far below its place an element starts (<c>translateY(3rem)</c>, <c>spacing-09</c>).</summary>
    public const double Offset = CdsSpacing.Spacing09;

    /// <summary>How long one element's entrance takes (<c>duration-moderate-02</c>).</summary>
    public const uint Duration = CdsDuration.Moderate02;

    /// <summary>The delay before the element at <paramref name="index"/> (zero-based) starts: 60, 120, … 480 ms.</summary>
    public static int Delay(int index) => Math.Min(Math.Max(index, 0) + 1, MaxStaggered) * Step;

    /// <summary>When the last of <paramref name="count"/> elements has finished, in milliseconds (0 for none).</summary>
    public static int TotalDuration(int count) => count <= 0 ? 0 : Delay(count - 1) + (int)Duration;

    /// <summary>The opacity and vertical offset of an element at <paramref name="progress"/> (0 to 1) of its entrance.</summary>
    public static (double Opacity, double TranslationY) At(double progress)
    {
        var p = Math.Clamp(progress, 0, 1);
        return (p, Offset * (1 - p));
    }
}

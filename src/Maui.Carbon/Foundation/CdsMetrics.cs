using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Maui.Carbon;

/// <summary>
/// The names of the metrics Maui.Carbon publishes through <see cref="System.Diagnostics.Metrics"/>, for a
/// <see cref="MeterListener"/>, <c>dotnet-counters monitor --counters Maui.Carbon</c> or OpenTelemetry. Every instrument
/// is on the meter <see cref="MeterName"/>. The durations are histograms in seconds: their count is how often the work ran
/// (a redraw, a layout pass) and their sum is what it cost. Each one carries the control type in <see cref="ComponentTag"/>.
/// <para>
/// Nothing is measured until a listener enables an instrument. Until then a measured call costs one flag check.
/// </para>
/// </summary>
public static class CdsMetrics
{
    /// <summary>The name of the meter that holds every Carbon instrument.</summary>
    public const string MeterName = "Maui.Carbon";

    /// <summary>The tag holding the control type that did the work (<c>CdsIcon</c>, <c>CdsDonutChart</c>, ...).</summary>
    public const string ComponentTag = "carbon.component";

    /// <summary>The tag holding the theme switched to, on <see cref="ThemeSwitchDuration"/>.</summary>
    public const string ThemeTag = "carbon.theme";

    /// <summary>
    /// One draw of a self-drawing <c>GraphicsView</c>: icons, choice and toggle glyphs, spinners, status shapes and
    /// chart canvases. Its count is the number of redraws. Chart canvases are tagged with the chart's type.
    /// </summary>
    public const string DrawDuration = "carbon.draw.duration";

    /// <summary>One <c>ApplyVisuals</c> of a <see cref="CdsControl"/>: re-applying token colors after a state, property or context change.</summary>
    public const string VisualsDuration = "carbon.visuals.duration";

    /// <summary>
    /// One measure pass of a <see cref="CdsControl"/>. It includes the control's content, so the time of nested Carbon
    /// controls is counted again in their ancestors. Read the count to find layout thrash.
    /// </summary>
    public const string MeasureDuration = "carbon.measure.duration";

    /// <summary>One arrange pass of a <see cref="CdsControl"/>. Inclusive of its content, like <see cref="MeasureDuration"/>.</summary>
    public const string ArrangeDuration = "carbon.arrange.duration";

    /// <summary>
    /// One coalesced rebuild: a chart building its scene, a list, structured list or breadcrumb rebuilding its rows, a
    /// notification announcing itself. One per batch of changes. Many per step means changes are not being batched.
    /// </summary>
    public const string RebuildDuration = "carbon.rebuild.duration";

    /// <summary>
    /// One activation (click, tap, Enter or Space) of a control's hit target, from the click to its handler and command
    /// returning. This is the time the UI thread is held before the press can show its result.
    /// </summary>
    public const string ActivationDuration = "carbon.activation.duration";

    /// <summary>One app theme switch (<see cref="CdsThemeManager.SetTheme"/>): swapping the resources and running every handler of <see cref="CdsThemeManager.ThemeChanged"/>.</summary>
    public const string ThemeSwitchDuration = "carbon.theme.switch.duration";

    /// <summary>One frame of the shared loading-animation clock: advancing every shown spinner, skeleton and shimmer.</summary>
    public const string LoopTickDuration = "carbon.loop.tick.duration";

    /// <summary>The number of loading animations (spinners, skeletons, shimmers) running now, shown or not. Observable.</summary>
    public const string ActiveLoops = "carbon.loop.active";
}

/// <summary>The instruments behind <see cref="CdsMetrics"/>, and the timing helpers the controls use.</summary>
internal static class CdsMeter
{
    static readonly Meter Meter = new(CdsMetrics.MeterName, typeof(CdsMeter).Assembly.GetName().Version?.ToString());

    public static readonly Histogram<double> Draw = Duration(CdsMetrics.DrawDuration, "A self-drawing GraphicsView's draw.");
    public static readonly Histogram<double> Visuals = Duration(CdsMetrics.VisualsDuration, "A control re-applying its token visuals.");
    public static readonly Histogram<double> Measure = Duration(CdsMetrics.MeasureDuration, "A control's measure pass, including its content.");
    public static readonly Histogram<double> Arrange = Duration(CdsMetrics.ArrangeDuration, "A control's arrange pass, including its content.");
    public static readonly Histogram<double> Rebuild = Duration(CdsMetrics.RebuildDuration, "A coalesced rebuild of a chart, list or announcement.");
    public static readonly Histogram<double> Activation = Duration(CdsMetrics.ActivationDuration, "A control's activation handler and command.");
    public static readonly Histogram<double> ThemeSwitch = Duration(CdsMetrics.ThemeSwitchDuration, "An app theme switch and its handlers.");
    public static readonly Histogram<double> LoopTick = Duration(CdsMetrics.LoopTickDuration, "One frame of the shared loading-animation clock.");

    static Func<int>? s_activeLoops;

    // Kept in a field so the instrument is created with the meter; its callback reads whatever CdsLoop registered.
    static readonly ObservableUpDownCounter<int> ActiveLoops = Meter.CreateObservableUpDownCounter(
        CdsMetrics.ActiveLoops, () => s_activeLoops?.Invoke() ?? 0, "{animation}", "Loading animations running now.");

    static Histogram<double> Duration(string name, string description) => Meter.CreateHistogram<double>(name, "s", description);

    /// <summary>Sets what <see cref="CdsMetrics.ActiveLoops"/> reports.</summary>
    public static void ObserveActiveLoops(Func<int> count) => s_activeLoops = count;

    /// <summary>A timestamp to pass to <c>Stop</c>, or 0 when no listener wants <paramref name="instrument"/>.</summary>
    public static long Start(Histogram<double> instrument) => instrument.Enabled ? Stopwatch.GetTimestamp() : 0;

    /// <summary>Records the time since <paramref name="start"/>, tagged with <paramref name="component"/>; nothing when <paramref name="start"/> is 0.</summary>
    public static void Stop(Histogram<double> instrument, long start, string component)
    {
        if (start == 0) return;
        instrument.Record(Stopwatch.GetElapsedTime(start).TotalSeconds, new KeyValuePair<string, object?>(CdsMetrics.ComponentTag, component));
    }

    /// <summary>Records the time since <paramref name="start"/>, tagged with the type of <paramref name="owner"/> (looked up only when recording).</summary>
    public static void Stop(Histogram<double> instrument, long start, object owner)
    {
        if (start == 0) return;
        Stop(instrument, start, owner.GetType().Name);
    }

    /// <summary>Records the time since <paramref name="start"/> with one tag of its own; nothing when <paramref name="start"/> is 0.</summary>
    public static void Stop(Histogram<double> instrument, long start, string tag, string value)
    {
        if (start == 0) return;
        instrument.Record(Stopwatch.GetElapsedTime(start).TotalSeconds, new KeyValuePair<string, object?>(tag, value));
    }

    /// <summary>Records the time since <paramref name="start"/> untagged; nothing when <paramref name="start"/> is 0.</summary>
    public static void Stop(Histogram<double> instrument, long start)
    {
        if (start == 0) return;
        instrument.Record(Stopwatch.GetElapsedTime(start).TotalSeconds);
    }
}

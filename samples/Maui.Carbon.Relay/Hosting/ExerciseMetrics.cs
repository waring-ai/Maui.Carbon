using System.Collections.Concurrent;
using System.Diagnostics.Metrics;
using System.Globalization;
using System.Text;

namespace Maui.Carbon.Relay.Hosting;

/// <summary>
/// Listens to Maui.Carbon's metrics (<see cref="CdsMetrics"/>) while <see cref="ExerciseRunner"/> runs. It totals them
/// per step (<see cref="ExerciseLog.Step"/>), instrument and component, and counts the draws made while a screen should
/// be idle (<see cref="BeginIdle"/>). The report then names the components that draw, lay out, re-apply visuals or rebuild
/// the most, the slowest single operations, the slowest activations, and what keeps drawing on a settled screen.
/// </summary>
public sealed class ExerciseMetrics : IDisposable
{
    /// <summary>Count, total and worst duration of one instrument for one component in one step.</summary>
    public sealed class Total
    {
        public long Count;
        public double Seconds;
        public double Max;
    }

    sealed record IdleWindow(string State)
    {
        public ConcurrentDictionary<string, long> Draws { get; } = new(StringComparer.Ordinal);
        public int Loops { get; set; }
    }

    // Work a step itself causes, each counted once (measure and arrange include their nested controls; the animation
    // clock ticks whatever the step does, and is reported on its own).
    static readonly string[] Exclusive = [CdsMetrics.DrawDuration, CdsMetrics.VisualsDuration, CdsMetrics.RebuildDuration, CdsMetrics.ActivationDuration, CdsMetrics.ThemeSwitchDuration];

    readonly MeterListener _listener = new();
    readonly ConcurrentDictionary<(string Step, string Instrument, string Component), Total> _totals = new();
    readonly List<IdleWindow> _idle = [];
    volatile IdleWindow? _window;
    int _loops;

    public ExerciseMetrics()
    {
        _listener.InstrumentPublished = (instrument, listener) =>
        {
            if (instrument.Meter.Name == CdsMetrics.MeterName) listener.EnableMeasurementEvents(instrument);
        };
        _listener.SetMeasurementEventCallback<double>(OnMeasurement);
        _listener.SetMeasurementEventCallback<int>((instrument, value, _, _) =>
        {
            if (instrument.Name == CdsMetrics.ActiveLoops) _loops = value;
        });
        _listener.Start();
    }

    void OnMeasurement(Instrument instrument, double seconds, ReadOnlySpan<KeyValuePair<string, object?>> tags, object? state)
    {
        var component = "";
        foreach (var tag in tags)
            if (tag.Key is CdsMetrics.ComponentTag or CdsMetrics.ThemeTag) component = tag.Value as string ?? "";
        var total = _totals.GetOrAdd((ExerciseLog.Step ?? "(before start)", instrument.Name, component), static _ => new Total());
        lock (total)
        {
            total.Count++;
            total.Seconds += seconds;
            if (seconds > total.Max) total.Max = seconds;
        }
        if (_window is { } window && instrument.Name == CdsMetrics.DrawDuration)
            window.Draws.AddOrUpdate(component, 1, static (_, n) => n + 1);
    }

    /// <summary>Starts counting draws as idle ones: <paramref name="state"/> is on screen and should have settled.</summary>
    public void BeginIdle(string state)
    {
        _listener.RecordObservableInstruments();
        var window = new IdleWindow(state) { Loops = _loops };
        lock (_idle) _idle.Add(window);
        _window = window;
    }

    /// <summary>Stops counting idle draws.</summary>
    public void EndIdle() => _window = null;

    static string Short(string instrument) => instrument.Replace("carbon.", "", StringComparison.Ordinal).Replace(".duration", "", StringComparison.Ordinal);

    static string Ms(double seconds) => (seconds * 1000).ToString(seconds < 0.001 ? "0.00" : "0.0", CultureInfo.InvariantCulture);

    /// <summary>The report sections.</summary>
    public void WriteReport(StringBuilder sb)
    {
        var rows = _totals.Select(kv => (kv.Key.Step, kv.Key.Instrument, kv.Key.Component, kv.Value)).ToList();
        sb.AppendLine().AppendLine("## Carbon metrics").AppendLine();
        if (rows.Count == 0)
        {
            sb.AppendLine("Nothing recorded.");
            return;
        }
        sb.AppendLine("From the Maui.Carbon meter (`CdsMetrics`). Measure and arrange times include nested controls, so they add up past wall time.");
        var ticks = rows.Where(r => r.Instrument == CdsMetrics.LoopTickDuration).ToList();
        List<IdleWindow> settled;
        lock (_idle) settled = [.. _idle];
        if (ticks.Count > 0)
        {
            var busiest = settled.MaxBy(w => w.Loops);
            sb.AppendLine()
                .Append(CultureInfo.InvariantCulture, $"- Animation clock: {ticks.Sum(r => r.Value.Count)} frames, {Ms(ticks.Sum(r => r.Value.Seconds))} ms, worst frame {Ms(ticks.Max(r => r.Value.Max))} ms. ")
                .Append(CultureInfo.InvariantCulture, $"Animations registered at settle checks: {settled.Count(w => w.Loops > 0)} of {settled.Count} states had some, most {busiest?.Loops ?? 0}{(busiest is { Loops: > 0 } ? $" ({busiest.State})" : "")}. ")
                .AppendLine("The clock ticks every frame while any animation is registered, even a hidden one.");
        }

        sb.AppendLine().AppendLine("### Busiest components").AppendLine();
        sb.AppendLine("| Instrument | Component | Count | Total ms | Mean ms | Worst ms |").AppendLine("|---|---|---:|---:|---:|---:|");
        foreach (var instrument in rows.Select(r => r.Instrument).Distinct().Order(StringComparer.Ordinal))
        {
            var byComponent = rows.Where(r => r.Instrument == instrument)
                .GroupBy(r => r.Component)
                .Select(g => (Component: g.Key, Count: g.Sum(r => r.Value.Count), Seconds: g.Sum(r => r.Value.Seconds), Max: g.Max(r => r.Value.Max)))
                .OrderByDescending(c => c.Seconds).Take(instrument == CdsMetrics.LoopTickDuration ? 1 : 8);
            foreach (var c in byComponent)
                sb.AppendLine(CultureInfo.InvariantCulture, $"| {Short(instrument)} | {(c.Component.Length == 0 ? "-" : c.Component)} | {c.Count} | {Ms(c.Seconds)} | {Ms(c.Seconds / c.Count)} | {Ms(c.Max)} |");
        }

        sb.AppendLine().AppendLine("### Busiest steps").AppendLine();
        sb.AppendLine("Ranked by time in draws, visuals, rebuilds, activations and theme switches.").AppendLine();
        sb.AppendLine("| Step | ms | Draws | Visuals | Measures | Arranges | Rebuilds | Top component |").AppendLine("|---|---:|---:|---:|---:|---:|---:|---|");
        foreach (var step in rows.GroupBy(r => r.Step)
                     .Select(g => (Step: g.Key, Rows: g.ToList(), Seconds: g.Where(r => Exclusive.Contains(r.Instrument)).Sum(r => r.Value.Seconds)))
                     .OrderByDescending(s => s.Seconds).Take(15))
        {
            long Count(string instrument) => step.Rows.Where(r => r.Instrument == instrument).Sum(r => r.Value.Count);
            var top = step.Rows.Where(r => Exclusive.Contains(r.Instrument) && r.Component.Length > 0)
                .GroupBy(r => r.Component).OrderByDescending(g => g.Sum(r => r.Value.Seconds)).FirstOrDefault()?.Key ?? "-";
            sb.AppendLine(CultureInfo.InvariantCulture, $"| {step.Step} | {Ms(step.Seconds)} | {Count(CdsMetrics.DrawDuration)} | {Count(CdsMetrics.VisualsDuration)} | {Count(CdsMetrics.MeasureDuration)} | {Count(CdsMetrics.ArrangeDuration)} | {Count(CdsMetrics.RebuildDuration)} | {top} |");
        }

        sb.AppendLine().AppendLine("### Single operations over one frame (16 ms)").AppendLine();
        var slow = rows.Where(r => r.Value.Max > 0.016 && r.Instrument != CdsMetrics.MeasureDuration && r.Instrument != CdsMetrics.ArrangeDuration)
            .OrderByDescending(r => r.Value.Max).Take(20).ToList();
        if (slow.Count == 0) sb.AppendLine("None.");
        foreach (var r in slow) sb.AppendLine(CultureInfo.InvariantCulture, $"- {Ms(r.Value.Max)} ms {Short(r.Instrument)} of {r.Component} during {r.Step}");
        var slowLayout = rows.Where(r => r.Value.Max > 0.016 && (r.Instrument == CdsMetrics.MeasureDuration || r.Instrument == CdsMetrics.ArrangeDuration))
            .GroupBy(r => (r.Instrument, r.Component)).Select(g => g.MaxBy(r => r.Value.Max)).OrderByDescending(r => r.Value.Max).Take(10).ToList();
        if (slowLayout.Count > 0)
        {
            sb.AppendLine().AppendLine("Layout passes over 16 ms (inclusive of nested controls; the innermost slow one is usually the cause):").AppendLine();
            foreach (var r in slowLayout) sb.AppendLine(CultureInfo.InvariantCulture, $"- {Ms(r.Value.Max)} ms {Short(r.Instrument)} of {r.Component} during {r.Step}");
        }

        sb.AppendLine().AppendLine("### Slow activations (over 50 ms on the UI thread)").AppendLine();
        var activations = rows.Where(r => r.Instrument == CdsMetrics.ActivationDuration && r.Value.Max > 0.05).OrderByDescending(r => r.Value.Max).Take(15).ToList();
        if (activations.Count == 0) sb.AppendLine("None.");
        foreach (var r in activations) sb.AppendLine(CultureInfo.InvariantCulture, $"- {Ms(r.Value.Max)} ms {r.Component} during {r.Step}");

        sb.AppendLine().AppendLine("### Drawing on a settled screen").AppendLine();
        sb.AppendLine("Draws during the audit's 600 ms settle check, when nothing should change. Loading animations (spinners, skeletons, shimmer) draw every frame by design; the running count is given.").AppendLine();
        var idle = settled.SelectMany(w => w.Draws.Select(d => (w.State, w.Loops, Component: d.Key, Draws: d.Value)))
            .Where(d => d.Draws >= 3)
            .GroupBy(d => d.Component)
            .Select(g => (Component: g.Key, States: g.Count(), Draws: g.Sum(d => d.Draws), Worst: g.MaxBy(d => d.Draws)))
            .OrderByDescending(g => g.Draws).Take(15).ToList();
        if (idle.Count == 0) sb.AppendLine("None.");
        foreach (var g in idle)
            sb.AppendLine(CultureInfo.InvariantCulture, $"- **{g.Component}**: {g.Draws} draws over {g.States} states; worst {g.Worst.Draws} in {g.Worst.State} ({g.Worst.Loops} animations running)");
    }

    /// <summary>Every total, for the JSON findings.</summary>
    public object Data() => _totals
        .Select(kv => new { kv.Key.Step, Instrument = kv.Key.Instrument, kv.Key.Component, kv.Value.Count, Ms = kv.Value.Seconds * 1000, MaxMs = kv.Value.Max * 1000 })
        .OrderBy(t => t.Step, StringComparer.Ordinal).ThenBy(t => t.Instrument, StringComparer.Ordinal).ToList();

    public void Dispose() => _listener.Dispose();
}

using System.Collections.Concurrent;
using Serilog.Core;
using Serilog.Events;

namespace Maui.Carbon.Relay.Hosting;

/// <summary>
/// Collects warnings and errors while <see cref="ExerciseRunner"/> runs, each tagged with the step that was current
/// when it was logged. A static step (not LogContext) because MAUI logs from native layout and handler callbacks that
/// do not flow the runner's async context.
/// </summary>
public sealed class ExerciseLog : ILogEventSink
{
    public static ExerciseLog Instance { get; } = new();

    public sealed record Entry(DateTimeOffset Time, LogEventLevel Level, string Source, string Message, string? Exception, string Step);

    readonly ConcurrentQueue<Entry> _entries = new();
    readonly ConcurrentDictionary<string, byte> _stacks = new();

    /// <summary>Null outside exercise mode: nothing is collected.</summary>
    public static string? Step { get; set; }

    public IReadOnlyCollection<Entry> Entries => _entries;

    public void Emit(LogEvent e)
    {
        if (Step is not { } step) return;
        var source = e.Properties.TryGetValue("SourceContext", out var sc) ? sc.ToString().Trim('"') : "";
        var message = e.RenderMessage();
        var detail = e.Exception?.ToString();
        // MAUI's warnings say what went wrong but not who did it: keep the stack of the first of each kind (it is
        // logged synchronously from the offending call, e.g. Element.SetParent).
        if (detail is null && e.Level == LogEventLevel.Warning && _stacks.TryAdd(source + message, 0))
            detail = "Logged from:\n" + string.Join('\n', new System.Diagnostics.StackTrace(1, false).ToString()
                .Split('\n').Where(l => !l.Contains("Serilog", StringComparison.Ordinal) && !l.Contains("Microsoft.Extensions.Logging", StringComparison.Ordinal)).Take(18));
        _entries.Enqueue(new Entry(e.Timestamp, e.Level, source, message, detail, step));
    }
}

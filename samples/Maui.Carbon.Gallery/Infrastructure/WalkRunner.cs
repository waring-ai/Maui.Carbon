using System.Diagnostics;
using System.Globalization;
using System.Text;
using JetBrains.Profiler.Api;

namespace Maui.Carbon.Gallery.Infrastructure;

/// <summary>
/// Walk mode for performance work: launch with <c>--walk &lt;dir&gt;</c> or CARBON_WALK_DIR (tools/profile-gallery.sh does this,
/// under dotTrace or dotMemory). The app visits every page in sidebar order, measures each one, writes
/// <c>walk.csv</c> and <c>walk-report.md</c> to the directory and quits.
/// </summary>
/// <remarks>
/// <para>Per page, from the start of navigation until the next page starts: the navigation and load times, the time until
/// the main thread goes quiet (no stall of 50 ms or more for 250 ms), the longest main-thread stall, the total time lost
/// to stalls, managed allocations, GC counts, first-chance exceptions, and the managed heap and working set afterwards.</para>
/// <para>Options (environment variables; the first two also as <c>--category</c> and <c>--pages</c>):
/// CARBON_WALK_CATEGORY (a <see cref="GalleryCategory"/> name), CARBON_WALK_PAGES (titles without spaces or routes, comma
/// separated), CARBON_WALK_PASSES (default 1; a second pass revisits the pages Shell has cached), CARBON_WALK_SCROLL=1
/// (also scroll each page to the bottom) and CARBON_WALK_SETTLE_MS (extra time on each page once quiet, default 300).</para>
/// <para>Each row records when the page started, in seconds since the process started, to find it on a dotTrace Timeline.
/// Under dotMemory started with <c>--use-api</c>, the walk takes a memory snapshot before it starts and after each pass;
/// without dotMemory those calls do nothing. dotTrace needs no API calls: it records the whole run and saves on exit.</para>
/// </remarks>
public static class WalkRunner
{
    public static void AttachIfRequested(Window window)
    {
        var args = Environment.GetCommandLineArgs();
        string? Arg(string name, string env)
        {
            var i = Array.IndexOf(args, name);
            return i >= 0 && i + 1 < args.Length ? args[i + 1] : Environment.GetEnvironmentVariable(env);
        }

        var dir = Arg("--walk", "CARBON_WALK_DIR");
        if (string.IsNullOrEmpty(dir)) return;
        Directory.CreateDirectory(dir);
        var options = new WalkOptions(
            dir,
            Arg("--category", "CARBON_WALK_CATEGORY"),
            Arg("--pages", "CARBON_WALK_PAGES")?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
            Math.Max(1, EnvInt("CARBON_WALK_PASSES", 1)),
            Environment.GetEnvironmentVariable("CARBON_WALK_SCROLL") == "1",
            Math.Max(0, EnvInt("CARBON_WALK_SETTLE_MS", 300)));

        var started = false;
        async void Start()
        {
            if (started) return;
            started = true;
            try
            {
                await RunAsync(window.Dispatcher, options);
            }
            catch (Exception ex)
            {
                File.AppendAllText(Path.Combine(dir, "walk-error.txt"), ex + "\n");
            }
            Application.Current?.Quit();
        }
        // As in SnapshotRunner: launched from a shell the window may never activate, so also start from timers.
        window.Created += (_, _) => window.Dispatcher.DispatchDelayed(TimeSpan.FromSeconds(2), Start);
        window.Activated += (_, _) => Start();
        Application.Current?.Dispatcher.DispatchDelayed(TimeSpan.FromSeconds(4), Start);
    }

    static int EnvInt(string name, int fallback) =>
        int.TryParse(Environment.GetEnvironmentVariable(name), NumberStyles.Integer, CultureInfo.InvariantCulture, out var v) ? v : fallback;

    static async Task RunAsync(IDispatcher dispatcher, WalkOptions o)
    {
        var entries = GalleryRegistry.Entries
            .Where(e => o.Category is null || string.Equals(e.Info.Category.ToString(), o.Category, StringComparison.OrdinalIgnoreCase))
            .Where(e => o.Pages is null || o.Pages.Any(p =>
                string.Equals(p, e.Info.Title.Replace(" ", "", StringComparison.Ordinal), StringComparison.OrdinalIgnoreCase) ||
                string.Equals(p, e.Info.Route, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        var exceptions = 0;
        AppDomain.CurrentDomain.FirstChanceException += (_, _) => Interlocked.Increment(ref exceptions);
        var memory = (MemoryProfiler.GetFeatures() & MemoryFeatures.Ready) != 0;
        // tools/profile-gallery.sh says which profiler it started (dotTrace's timeline mode leaves no trace in the process).
        var profiler = Environment.GetEnvironmentVariable("CARBON_WALK_PROFILER") is { Length: > 0 } named ? named : memory ? "dotMemory" : "none";
        var processStart = Process.GetCurrentProcess().StartTime;
        var beat = new Heartbeat(dispatcher);
        var rows = new List<WalkRow>();

        // Let startup finish so the first page is not charged for it.
        await Task.Delay(1500);
        await WaitForQuietAsync(beat, beat.Now);
        if (memory)
        {
            MemoryProfiler.CollectAllocations(true);
            MemoryProfiler.GetSnapshot("Before walk");
        }

        var walkClock = Stopwatch.StartNew();
        for (var pass = 1; pass <= o.Passes; pass++)
        {
            for (var i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                rows.Add(await VisitAsync(entry, i + 1, pass, beat, o, () => exceptions, (DateTime.Now - processStart).TotalSeconds));
            }
            if (memory) MemoryProfiler.GetSnapshot($"After pass {pass}");
        }
        walkClock.Stop();
        beat.Stop();

        await File.WriteAllTextAsync(Path.Combine(o.Dir, "walk.csv"), WalkRow.ToCsv(rows));
        await File.WriteAllTextAsync(Path.Combine(o.Dir, "walk-report.md"), Report(rows, o, walkClock.Elapsed, profiler));
        await File.WriteAllTextAsync(Path.Combine(o.Dir, "walk-done"), $"{rows.Count} visits\n");
    }

    static async Task<WalkRow> VisitAsync(GalleryEntry entry, int index, int pass, Heartbeat beat, WalkOptions o, Func<int> exceptions, double atSeconds)
    {
        var shell = Shell.Current;
        var alreadyOpen = shell.CurrentState.Location.OriginalString.EndsWith("/" + entry.Info.Route, StringComparison.Ordinal);
        var meter = beat.Begin();
        var allocated = GC.GetTotalAllocatedBytes(precise: false);
        var gc0 = GC.CollectionCount(0);
        var gc1 = GC.CollectionCount(1);
        var gc2 = GC.CollectionCount(2);
        var thrown = exceptions();
        var start = beat.Now;
        double navigated = 0, loaded = 0, quiet = 0, scrollStall = 0;
        string? error = null;
        try
        {
            await shell.GoToAsync("//" + entry.Info.Route);
            navigated = beat.Now - start;
            // GoToAsync can return before Shell has created the page, so wait for the page itself and its Loaded state.
            Page? page;
            while (((page = shell.CurrentPage) is null || page.GetType() != entry.PageType || !page.IsLoaded) && beat.Now - start < 10_000)
                await Task.Delay(4);
            if (page is null || page.GetType() != entry.PageType) throw new TimeoutException("the page did not load within 10 s");
            loaded = beat.Now - start;
            quiet = await WaitForQuietAsync(beat, start) - start;
            if (o.Scroll) scrollStall = await ScrollThroughAsync(page, beat);
            if (o.SettleMs > 0) await Task.Delay(o.SettleMs);
        }
        catch (Exception ex)
        {
            error = $"{ex.GetType().Name}: {ex.Message}";
            File.AppendAllText(Path.Combine(o.Dir, "walk-exceptions.txt"), $"{entry.Info.Route} (pass {pass}): {ex}\n\n");
        }
        beat.End(meter);
        return new WalkRow(
            pass, index, entry.Info.Category.ToString(), entry.Info.Title, entry.Info.Route, atSeconds,
            navigated, loaded, quiet, meter.MaxGapMs, meter.StalledMs, scrollStall,
            (GC.GetTotalAllocatedBytes(precise: false) - allocated) / 1048576.0,
            GC.CollectionCount(0) - gc0, GC.CollectionCount(1) - gc1, GC.CollectionCount(2) - gc2,
            exceptions() - thrown,
            GC.GetTotalMemory(forceFullCollection: false) / 1048576.0,
            Environment.WorkingSet / 1048576.0,
            alreadyOpen ? "already open" : null,
            error);
    }

    /// <summary>Waits until the main thread has had no stall for <paramref name="quietMs"/> and returns when the last stall ended (or <paramref name="from"/>).</summary>
    static async Task<double> WaitForQuietAsync(Heartbeat beat, double from, double quietMs = 250, double capMs = 15_000)
    {
        while (true)
        {
            await Task.Delay(20);
            var busyUntil = Math.Max(from, beat.LastStallEnd);
            if (beat.Now - busyUntil >= quietMs || beat.Now - from >= capMs) return busyUntil;
        }
    }

    /// <summary>Scrolls the page's main vertical scroller to the bottom a screen at a time; returns the longest stall meanwhile.</summary>
    static async Task<double> ScrollThroughAsync(Page page, Heartbeat beat)
    {
        var scroll = (page as ContentPage)?.Content as ScrollView
            ?? page.GetVisualTreeDescendants().OfType<ScrollView>()
                .FirstOrDefault(s => s.Orientation != ScrollOrientation.Horizontal && s.Height >= page.Height / 2);
        if (scroll is null || scroll.Height <= 0) return 0;
        var meter = beat.Begin();
        var max = scroll.ContentSize.Height - scroll.Height;
        for (double y = 0; y < max - 1;)
        {
            y = Math.Min(y + scroll.Height * 0.8, max);
            await scroll.ScrollToAsync(0, y, animated: false);
            await Task.Delay(50);
        }
        await WaitForQuietAsync(beat, beat.Now);
        await scroll.ScrollToAsync(0, 0, animated: false);
        beat.End(meter);
        return meter.MaxGapMs;
    }

    static string Report(List<WalkRow> rows, WalkOptions o, TimeSpan elapsed, string profiler)
    {
        var config =
#if DEBUG
            "Debug";
#else
            "Release";
#endif
        var sb = new StringBuilder();
        sb.AppendLine(CultureInfo.InvariantCulture, $"# Gallery walk {DateTime.Now:yyyy-MM-dd HH:mm}");
        sb.AppendLine();
        sb.AppendLine(CultureInfo.InvariantCulture, $"{rows.Count} visits ({rows.Select(r => r.Route).Distinct().Count()} pages x {o.Passes} pass(es)) in {elapsed.TotalSeconds:0.0} s. {config} build, {DeviceInfo.Platform} {DeviceInfo.VersionString}, .NET {Environment.Version}. " +
            $"Profiler: {profiler}{(o.Scroll ? ", pages scrolled" : "")}.");
        sb.AppendLine();
        sb.AppendLine("Times are from the start of navigation. *Quiet* is when the main thread last stalled (50 ms or more) before going quiet for 250 ms; " +
            "*stalled* sums the time lost to stalls; *alloc* counts managed allocations while the page was shown. *At* is when the page started, in seconds since launch " +
            "(the time axis of a dotTrace Timeline snapshot). Pages Shell has cached load faster on a second pass.");
        var errors = rows.Where(r => r.Error is not null).ToList();
        if (errors.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("## Errors");
            foreach (var r in errors) sb.AppendLine(CultureInfo.InvariantCulture, $"- {r.Title} (pass {r.Pass}): {r.Error}");
        }
        Table(sb, "Slowest to go quiet", rows.OrderByDescending(r => r.QuietMs), r => $"{r.QuietMs:0} ms");
        Table(sb, "Longest main-thread stall", rows.OrderByDescending(r => r.MaxStallMs), r => $"{r.MaxStallMs:0} ms");
        Table(sb, "Most time stalled", rows.OrderByDescending(r => r.StalledMs), r => $"{r.StalledMs:0} ms");
        Table(sb, "Most allocated", rows.OrderByDescending(r => r.AllocMB), r => $"{r.AllocMB:0.0} MB, GC {r.Gen0}/{r.Gen1}/{r.Gen2}");
        if (o.Scroll) Table(sb, "Longest stall while scrolling", rows.OrderByDescending(r => r.ScrollStallMs), r => $"{r.ScrollStallMs:0} ms");
        Table(sb, "Most first-chance exceptions", rows.Where(r => r.Exceptions > 0).OrderByDescending(r => r.Exceptions), r => $"{r.Exceptions}");
        var growth = rows.Zip(rows.Skip(1), (a, b) => (Row: b, Growth: b.HeapMB - a.HeapMB));
        Table(sb, "Largest managed heap growth", growth.OrderByDescending(g => g.Growth).Select(g => g.Row),
            r => $"+{growth.First(g => g.Row == r).Growth:0.0} MB (heap {r.HeapMB:0} MB)");
        if (rows.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine(CultureInfo.InvariantCulture, $"End of walk: managed heap {rows[^1].HeapMB:0} MB, working set {rows[^1].WorkingSetMB:0} MB.");
        }
        return sb.ToString();
    }

    static void Table(StringBuilder sb, string title, IEnumerable<WalkRow> ordered, Func<WalkRow, string> value)
    {
        var top = ordered.Take(15).ToList();
        if (top.Count == 0) return;
        sb.AppendLine();
        sb.AppendLine(CultureInfo.InvariantCulture, $"## {title}");
        sb.AppendLine();
        sb.AppendLine("| Page | Category | Pass | At | Value |");
        sb.AppendLine("|---|---|---|---|---|");
        foreach (var r in top)
            sb.AppendLine(CultureInfo.InvariantCulture, $"| {r.Title}{(r.Note is null ? "" : $" ({r.Note})")} | {r.Category} | {r.Pass} | {r.AtSeconds:0.0} s | {value(r)} |");
    }

    sealed record WalkOptions(string Dir, string? Category, string[]? Pages, int Passes, bool Scroll, int SettleMs);

    sealed record WalkRow(
        int Pass, int Index, string Category, string Title, string Route, double AtSeconds,
        double NavigateMs, double LoadedMs, double QuietMs, double MaxStallMs, double StalledMs, double ScrollStallMs,
        double AllocMB, int Gen0, int Gen1, int Gen2, int Exceptions, double HeapMB, double WorkingSetMB, string? Note, string? Error)
    {
        public static string ToCsv(IEnumerable<WalkRow> rows)
        {
            var sb = new StringBuilder("pass,index,category,title,route,at_s,navigate_ms,loaded_ms,quiet_ms,max_stall_ms,stalled_ms,scroll_stall_ms,alloc_mb,gen0,gen1,gen2,exceptions,heap_mb,working_set_mb,note,error\n");
            foreach (var r in rows)
            {
                sb.AppendLine(string.Join(',',
                    r.Pass.ToString(CultureInfo.InvariantCulture), r.Index.ToString(CultureInfo.InvariantCulture), r.Category, Quote(r.Title), r.Route,
                    r.AtSeconds.ToString("0.000", CultureInfo.InvariantCulture), F(r.NavigateMs), F(r.LoadedMs), F(r.QuietMs), F(r.MaxStallMs), F(r.StalledMs), F(r.ScrollStallMs),
                    r.AllocMB.ToString("0.00", CultureInfo.InvariantCulture),
                    r.Gen0.ToString(CultureInfo.InvariantCulture), r.Gen1.ToString(CultureInfo.InvariantCulture), r.Gen2.ToString(CultureInfo.InvariantCulture),
                    r.Exceptions.ToString(CultureInfo.InvariantCulture),
                    r.HeapMB.ToString("0.0", CultureInfo.InvariantCulture), r.WorkingSetMB.ToString("0.0", CultureInfo.InvariantCulture),
                    Quote(r.Note), Quote(r.Error)));
            }
            return sb.ToString();
        }

        static string F(double ms) => ms.ToString("0", CultureInfo.InvariantCulture);

        static string Quote(string? s) => s is null ? "" : "\"" + s.Replace("\"", "\"\"", StringComparison.Ordinal) + "\"";
    }

    /// <summary>
    /// A main-thread timer that ticks every 8 ms. A gap of 50 ms or more between ticks is a stall: the main thread was busy
    /// (layout, measuring, handler creation, GC) and could not render.
    /// </summary>
    sealed class Heartbeat
    {
        const double StallMs = 50;
        const double FrameMs = 1000.0 / 60;
        readonly Stopwatch _clock = Stopwatch.StartNew();
        readonly IDispatcherTimer _timer;
        readonly List<StallMeter> _meters = [];
        double _last;

        public Heartbeat(IDispatcher dispatcher)
        {
            _timer = dispatcher.CreateTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(8);
            _timer.Tick += OnTick;
            _last = Now;
            _timer.Start();
        }

        public double Now => _clock.Elapsed.TotalMilliseconds;

        /// <summary>When the latest stall ended, in <see cref="Now"/> time.</summary>
        public double LastStallEnd { get; private set; }

        public StallMeter Begin()
        {
            var meter = new StallMeter();
            _meters.Add(meter);
            return meter;
        }

        public void End(StallMeter meter) => _meters.Remove(meter);

        public void Stop() => _timer.Stop();

        void OnTick(object? sender, EventArgs e)
        {
            var now = Now;
            var gap = now - _last;
            _last = now;
            if (gap >= StallMs) LastStallEnd = now;
            foreach (var m in _meters)
            {
                m.MaxGapMs = Math.Max(m.MaxGapMs, gap);
                if (gap >= StallMs) m.StalledMs += gap - FrameMs;
            }
        }
    }

    sealed class StallMeter
    {
        public double MaxGapMs { get; set; }
        public double StalledMs { get; set; }
    }
}

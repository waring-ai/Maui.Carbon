using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Globalization;
using System.Text;

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
/// <para>Once a page has settled the walk counts its visual tree: elements, native views (hidden ones too) and, per Carbon
/// control type, the native views the control builds itself (walk-controls.csv and the report's "Native views by Carbon
/// control"). It also totals the Maui.Carbon meter per page (draws, visuals, measures, arranges, rebuilds). On Mac Catalyst a
/// page's stall time and allocations grow with its native views (about 0.9 ms and 35 KB each), and dotTrace's Timeline stacks
/// stop at the UIKit-to-managed transitions, so these counts are what attribute a slow page to the controls on it.</para>
/// <para>Each row records when the page started, in seconds since the process started, to find it on a dotTrace Timeline.
/// Under dotMemory (CARBON_WALK_DOTMEMORY_REQUESTS and CARBON_WALK_DOTMEMORY_LOG, set by tools/profile-gallery.sh), the walk
/// turns on allocation collection and takes a memory snapshot before it starts and after each pass. dotTrace needs nothing
/// from the walk: it records the whole run and saves on exit.</para>
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
        var dotMemory = DotMemoryControl.FromEnvironment();
        // tools/profile-gallery.sh says which profiler it started (dotTrace's timeline mode leaves no trace in the process).
        var profiler = Environment.GetEnvironmentVariable("CARBON_WALK_PROFILER") is { Length: > 0 } named ? named : "none";
        var processStart = Process.GetCurrentProcess().StartTime;
        var beat = new Heartbeat(dispatcher);
        using var metrics = new VisitMetrics();
        var rows = new List<WalkRow>();

        // Let startup finish so the first page is not charged for it.
        await Task.Delay(1500);
        await WaitForQuietAsync(beat, beat.Now);
        if (dotMemory is not null && await dotMemory.WaitConnectedAsync())
        {
            dotMemory.CollectAllocations();
            await dotMemory.SnapshotAsync("Before walk");
        }

        var walkClock = Stopwatch.StartNew();
        for (var pass = 1; pass <= o.Passes; pass++)
        {
            for (var i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                rows.Add(await VisitAsync(entry, i + 1, pass, beat, metrics, o, () => exceptions, (DateTime.Now - processStart).TotalSeconds));
            }
            if (dotMemory is { Connected: true }) await dotMemory.SnapshotAsync($"After pass {pass}");
        }
        walkClock.Stop();
        beat.Stop();
        if (dotMemory is not null)
            profiler += dotMemory.Connected ? $" ({dotMemory.Saved} of {dotMemory.Requested} snapshots saved)" : " (dotMemory never connected: no snapshots)";

        await File.WriteAllTextAsync(Path.Combine(o.Dir, "walk.csv"), WalkRow.ToCsv(rows));
        await File.WriteAllTextAsync(Path.Combine(o.Dir, "walk-controls.csv"),
            "control,instances,own_native_views,own_hidden_native_views,pages\n" + string.Concat(ControlTotals(rows).Select(c =>
                FormattableString.Invariant($"{c.Control},{c.Instances},{c.OwnNative},{c.OwnHidden},{c.Pages}\n"))));
        await File.WriteAllTextAsync(Path.Combine(o.Dir, "walk-report.md"), Report(rows, o, walkClock.Elapsed, profiler, metrics.Seen));
        await File.WriteAllTextAsync(Path.Combine(o.Dir, "walk-done"), $"{rows.Count} visits\n");
    }

    static async Task<WalkRow> VisitAsync(GalleryEntry entry, int index, int pass, Heartbeat beat, VisitMetrics metrics, WalkOptions o, Func<int> exceptions, double atSeconds)
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
        var census = TreeCensus.Empty;
        metrics.Begin();
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
            census = TreeCensus.Of(page);
        }
        catch (Exception ex)
        {
            error = $"{ex.GetType().Name}: {ex.Message}";
            File.AppendAllText(Path.Combine(o.Dir, "walk-exceptions.txt"), $"{entry.Info.Route} (pass {pass}): {ex}\n\n");
        }
        beat.End(meter);
        var work = metrics.End();
        return new WalkRow(
            pass, index, entry.Info.Category.ToString(), entry.Info.Title, entry.Info.Route, atSeconds,
            navigated, loaded, quiet, meter.MaxGapMs, meter.StalledMs, scrollStall,
            (GC.GetTotalAllocatedBytes(precise: false) - allocated) / 1048576.0,
            GC.CollectionCount(0) - gc0, GC.CollectionCount(1) - gc1, GC.CollectionCount(2) - gc2,
            exceptions() - thrown,
            GC.GetTotalMemory(forceFullCollection: false) / 1048576.0,
            Environment.WorkingSet / 1048576.0,
            alreadyOpen ? "already open" : null,
            error,
            census,
            work);
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

    static string Report(List<WalkRow> rows, WalkOptions o, TimeSpan elapsed, string profiler, bool metricsSeen)
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
        Table(sb, "Largest visual trees", rows.OrderByDescending(r => r.Tree.Elements),
            r => $"{r.Tree.Elements} elements, {r.Tree.NativeViews} native views ({r.Tree.TopTypes})");
        ControlTable(sb, rows);
        if (metricsSeen)
        {
            Table(sb, "Most Carbon layout passes", rows.OrderByDescending(r => r.Work.Measures + r.Work.Arranges),
                r => $"{r.Work.Measures} measures, {r.Work.Arranges} arranges (most measured: {r.Work.MostMeasured})");
            Table(sb, "Most Carbon draw, visuals and rebuild time", rows.OrderByDescending(r => r.Work.ExclusiveMs),
                r => $"{r.Work.ExclusiveMs:0} ms: {r.Work.Draws} draws, {r.Work.Visuals} visuals, {r.Work.Rebuilds} rebuilds ({r.Work.Busiest})");
        }
        else
        {
            sb.AppendLine().AppendLine("No Maui.Carbon metrics arrived: the build trimmed them (set MetricsSupport=true).");
        }
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

    /// <summary>Carbon controls ranked by the native views they build across the walk's first pass.</summary>
    static IEnumerable<(string Control, int Instances, int OwnNative, int OwnHidden, int Pages)> ControlTotals(IEnumerable<WalkRow> rows) =>
        rows.Where(r => r.Pass == 1)
            .SelectMany(r => r.Tree.Controls.Select(c => (Control: c.Key, c.Value.Instances, c.Value.OwnNative, c.Value.OwnHidden)))
            .GroupBy(c => c.Control)
            .Select(g => (g.Key, g.Sum(c => c.Instances), g.Sum(c => c.OwnNative), g.Sum(c => c.OwnHidden), g.Count()))
            .OrderByDescending(c => c.Item3);

    static void ControlTable(StringBuilder sb, List<WalkRow> rows)
    {
        var totals = ControlTotals(rows).Take(25).ToList();
        if (totals.Count == 0) return;
        var all = rows.Where(r => r.Pass == 1).Sum(r => r.Tree.NativeViews);
        var hidden = rows.Where(r => r.Pass == 1).Sum(r => r.Tree.HiddenNative);
        sb.AppendLine().AppendLine("## Native views by Carbon control").AppendLine();
        sb.AppendLine(CultureInfo.InvariantCulture, $"Each native view costs layout, memory and creation time on the main thread. {all} native views across the pages, {hidden} of them hidden (IsVisible false here or above); " +
            $"*own* counts the views a control builds itself, not those of Carbon controls nested in it.").AppendLine();
        sb.AppendLine("| Control | Instances | Own native views | Per instance | Of them hidden | Share | Pages |").AppendLine("|---|---:|---:|---:|---:|---:|---:|");
        foreach (var c in totals)
            sb.AppendLine(CultureInfo.InvariantCulture, $"| {c.Control} | {c.Instances} | {c.OwnNative} | {(double)c.OwnNative / c.Instances:0.0} | {c.OwnHidden} | {100.0 * c.OwnNative / Math.Max(1, all):0.0}% | {c.Pages} |");
        var hiddenTop = ControlTotals(rows).OrderByDescending(c => c.OwnHidden).Take(12).Where(c => c.OwnHidden > 0).ToList();
        if (hiddenTop.Count > 0)
        {
            sb.AppendLine().AppendLine("Most hidden native views (built but not shown; candidates for creating on demand):").AppendLine();
            foreach (var c in hiddenTop)
                sb.AppendLine(CultureInfo.InvariantCulture, $"- {c.Control}: {c.OwnHidden} hidden of {c.OwnNative} ({(double)c.OwnHidden / c.Instances:0.0} per instance)");
        }
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

    /// <summary>
    /// Asks dotMemory for snapshots through its service messages: tools/profile-gallery.sh starts dotmemory with
    /// <c>--service-input</c> on the requests file and <c>--service-output</c> into app.log, where dotMemory confirms each one.
    /// </summary>
    /// <remarks>
    /// Not JetBrains.Profiler.Api: under <c>dotmemory start --use-api</c> the profiler disconnects from the Mac Catalyst app
    /// about a second after start-up and takes no snapshot (as dotTrace's Sampling mode does), while the same session
    /// controlled by messages works.
    /// </remarks>
    sealed class DotMemoryControl(string requests, string log)
    {
        public bool Connected { get; private set; }
        public int Requested { get; private set; }
        public int Saved { get; private set; }

        public static DotMemoryControl? FromEnvironment() =>
            Environment.GetEnvironmentVariable("CARBON_WALK_DOTMEMORY_REQUESTS") is { Length: > 0 } requests &&
            Environment.GetEnvironmentVariable("CARBON_WALK_DOTMEMORY_LOG") is { Length: > 0 } log
                ? new DotMemoryControl(requests, log)
                : null;

        public async Task<bool> WaitConnectedAsync() => Connected = await WaitForAsync("connected", 0, TimeSpan.FromSeconds(30));

        public void CollectAllocations() => Send("collect-allocations-on");

        /// <summary>Requests a snapshot and waits for dotMemory to confirm it, so taking it is not charged to the next page.</summary>
        public async Task SnapshotAsync(string name)
        {
            var before = Count("snapshot-saved");
            Requested++;
            Send("get-snapshot", $"{{\"name\": \"{name}\"}}");
            if (await WaitForAsync("snapshot-saved", before, TimeSpan.FromMinutes(3))) Saved++;
        }

        async Task<bool> WaitForAsync(string message, int seen, TimeSpan timeout)
        {
            var clock = Stopwatch.StartNew();
            while (Count(message) <= seen)
            {
                if (clock.Elapsed > timeout) return false;
                await Task.Delay(250);
            }
            return true;
        }

        // A message is '##dotMemory' and a JSON array, on a line of its own.
        void Send(string message, string? properties = null) =>
            File.AppendAllText(requests, $"##dotMemory[\"{message}\"{(properties is null ? "" : ", " + properties)}]\n");

        int Count(string message)
        {
            try
            {
                using var stream = new FileStream(log, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
                var text = new StreamReader(stream).ReadToEnd();
                var needle = $"##dotMemory[\"{message}\"";
                var n = 0;
                for (var i = text.IndexOf(needle, StringComparison.Ordinal); i >= 0; i = text.IndexOf(needle, i + needle.Length, StringComparison.Ordinal)) n++;
                return n;
            }
            catch (IOException)
            {
                return 0;
            }
        }
    }

    sealed record WalkOptions(string Dir, string? Category, string[]? Pages, int Passes, bool Scroll, int SettleMs);

    sealed record WalkRow(
        int Pass, int Index, string Category, string Title, string Route, double AtSeconds,
        double NavigateMs, double LoadedMs, double QuietMs, double MaxStallMs, double StalledMs, double ScrollStallMs,
        double AllocMB, int Gen0, int Gen1, int Gen2, int Exceptions, double HeapMB, double WorkingSetMB, string? Note, string? Error,
        TreeCensus Tree, WorkSummary Work)
    {
        public static string ToCsv(IEnumerable<WalkRow> rows)
        {
            var sb = new StringBuilder("pass,index,category,title,route,at_s,navigate_ms,loaded_ms,quiet_ms,max_stall_ms,stalled_ms,scroll_stall_ms,alloc_mb,gen0,gen1,gen2,exceptions,heap_mb,working_set_mb,note,error," +
                "elements,native_views,hidden_native_views,top_types,draws,draw_ms,visuals,visuals_ms,measures,arranges,rebuilds,rebuild_ms,busiest,most_measured\n");
            foreach (var r in rows)
            {
                sb.AppendLine(string.Join(',',
                    r.Pass.ToString(CultureInfo.InvariantCulture), r.Index.ToString(CultureInfo.InvariantCulture), r.Category, Quote(r.Title), r.Route,
                    r.AtSeconds.ToString("0.000", CultureInfo.InvariantCulture), F(r.NavigateMs), F(r.LoadedMs), F(r.QuietMs), F(r.MaxStallMs), F(r.StalledMs), F(r.ScrollStallMs),
                    r.AllocMB.ToString("0.00", CultureInfo.InvariantCulture),
                    r.Gen0.ToString(CultureInfo.InvariantCulture), r.Gen1.ToString(CultureInfo.InvariantCulture), r.Gen2.ToString(CultureInfo.InvariantCulture),
                    r.Exceptions.ToString(CultureInfo.InvariantCulture),
                    r.HeapMB.ToString("0.0", CultureInfo.InvariantCulture), r.WorkingSetMB.ToString("0.0", CultureInfo.InvariantCulture),
                    Quote(r.Note), Quote(r.Error),
                    I(r.Tree.Elements), I(r.Tree.NativeViews), I(r.Tree.HiddenNative), Quote(r.Tree.TopTypes),
                    I(r.Work.Draws), F(r.Work.DrawMs), I(r.Work.Visuals), F(r.Work.VisualsMs), I(r.Work.Measures), I(r.Work.Arranges),
                    I(r.Work.Rebuilds), F(r.Work.RebuildMs), Quote(r.Work.Busiest), Quote(r.Work.MostMeasured)));
            }
            return sb.ToString();
        }

        static string F(double ms) => ms.ToString("0", CultureInfo.InvariantCulture);

        static string I(long n) => n.ToString(CultureInfo.InvariantCulture);

        static string Quote(string? s) => s is null ? "" : "\"" + s.Replace("\"", "\"\"", StringComparison.Ordinal) + "\"";
    }

    /// <summary>The shown page's visual tree once it has settled: every element, those with a native view, and the commonest types.</summary>
    /// <remarks>
    /// <see cref="Controls"/> holds, per Carbon control type on the page, its instances and the native views they build
    /// themselves: their subtree's native views minus those of nested Carbon controls (and of a content view's content).
    /// </remarks>
    sealed record TreeCensus(int Elements, int NativeViews, int HiddenNative, string TopTypes, IReadOnlyDictionary<string, (int Instances, int OwnNative, int OwnHidden)> Controls)
    {
        public static readonly TreeCensus Empty = new(0, 0, 0, "", new Dictionary<string, (int, int, int)>());

        public static TreeCensus Of(Page page)
        {
            var elements = 0;
            var native = 0;
            var types = new Dictionary<string, int>(StringComparer.Ordinal);
            var hiddenNative = 0;
            var controls = new Dictionary<string, (int Instances, int OwnNative, int OwnHidden)>(StringComparer.Ordinal);

            // Returns the native views (all, and those hidden by IsVisible here or above) of e's subtree that belong to no
            // nested Carbon control. A hidden view still has its native view: these are the ones lazy creation would save.
            (int All, int Hidden) Visit(IVisualTreeElement e, bool hiddenAbove)
            {
                elements++;
                var hidden = hiddenAbove || e is VisualElement { IsVisible: false };
                var isNative = e is IElement { Handler.PlatformView: not null };
                var mine = isNative ? 1 : 0;
                var mineHidden = isNative && hidden ? 1 : 0;
                native += mine;
                hiddenNative += mineHidden;
                var type = e.GetType();
                types[type.Name] = types.GetValueOrDefault(type.Name) + 1;
                foreach (var child in e.GetVisualChildren())
                {
                    var below = Visit(child, hidden);
                    if (!IsCarbon(child))
                    {
                        mine += below.All;
                        mineHidden += below.Hidden;
                    }
                }
                if (IsCarbon(e))
                {
                    var c = controls.GetValueOrDefault(type.Name);
                    controls[type.Name] = (c.Instances + 1, c.OwnNative + mine, c.OwnHidden + mineHidden);
                }
                return (mine, mineHidden);
            }

            // A Carbon control's own native views; not the gallery's or MAUI's, and not a page-level Carbon container
            // whose content is the app's (its subtree still counts, through the controls inside it).
            static bool IsCarbon(IVisualTreeElement e) =>
                e.GetType().Namespace?.StartsWith("Maui.Carbon", StringComparison.Ordinal) == true &&
                e.GetType().Namespace?.StartsWith("Maui.Carbon.Gallery", StringComparison.Ordinal) != true;

            Visit(page, false);
            var top = string.Join(", ", types.OrderByDescending(t => t.Value).Take(4).Select(t => $"{t.Key} {t.Value}"));
            return new TreeCensus(elements, native, hiddenNative, top, controls);
        }
    }

    /// <summary>Totals of the Maui.Carbon meter (<see cref="CdsMetrics"/>) for one visit.</summary>
    sealed record WorkSummary(
        long Draws, double DrawMs, long Visuals, double VisualsMs, long Measures, long Arranges, long Rebuilds, double RebuildMs,
        string Busiest, string MostMeasured)
    {
        public static readonly WorkSummary Empty = new(0, 0, 0, 0, 0, 0, 0, 0, "", "");

        /// <summary>Draws, visuals and rebuilds: work that is not counted again in an ancestor.</summary>
        public double ExclusiveMs => DrawMs + VisualsMs + RebuildMs;
    }

    /// <summary>
    /// Listens to the Maui.Carbon meter and totals it per visit and component, as Relay's ExerciseMetrics does per step. The
    /// gallery sets MetricsSupport so optimized builds keep the instruments; with no listener they cost one flag check.
    /// </summary>
    sealed class VisitMetrics : IDisposable
    {
        sealed class Total
        {
            public long Count;
            public double Seconds;
        }

        readonly MeterListener _listener = new();
        volatile ConcurrentDictionary<(string Instrument, string Component), Total>? _visit;

        public VisitMetrics()
        {
            _listener.InstrumentPublished = (instrument, listener) =>
            {
                if (instrument.Meter.Name == CdsMetrics.MeterName) listener.EnableMeasurementEvents(instrument);
            };
            _listener.SetMeasurementEventCallback<double>(OnMeasurement);
            _listener.Start();
        }

        /// <summary>Whether any Carbon measurement arrived (none means the instruments were trimmed away).</summary>
        public bool Seen { get; private set; }

        void OnMeasurement(Instrument instrument, double seconds, ReadOnlySpan<KeyValuePair<string, object?>> tags, object? state)
        {
            Seen = true;
            if (_visit is not { } visit) return;
            var component = "";
            foreach (var tag in tags)
                if (tag.Key == CdsMetrics.ComponentTag) component = tag.Value as string ?? "";
            var total = visit.GetOrAdd((instrument.Name, component), static _ => new Total());
            lock (total)
            {
                total.Count++;
                total.Seconds += seconds;
            }
        }

        public void Begin() => _visit = new();

        public WorkSummary End()
        {
            var visit = _visit;
            _visit = null;
            if (visit is null || visit.IsEmpty) return WorkSummary.Empty;
            var rows = visit.Select(kv => (kv.Key.Instrument, kv.Key.Component, kv.Value.Count, Ms: kv.Value.Seconds * 1000)).ToList();
            long Count(string instrument) => rows.Where(r => r.Instrument == instrument).Sum(r => r.Count);
            double Ms(string instrument) => rows.Where(r => r.Instrument == instrument).Sum(r => r.Ms);
            string[] exclusive = [CdsMetrics.DrawDuration, CdsMetrics.VisualsDuration, CdsMetrics.RebuildDuration];
            var busiest = string.Join(", ", rows.Where(r => exclusive.Contains(r.Instrument) && r.Component.Length > 0)
                .GroupBy(r => r.Component).Select(g => (g.Key, Ms: g.Sum(r => r.Ms), Count: g.Sum(r => r.Count)))
                .OrderByDescending(g => g.Ms).Take(3).Select(g => $"{g.Key} {g.Ms:0} ms/{g.Count}"));
            var measured = string.Join(", ", rows.Where(r => r.Instrument == CdsMetrics.MeasureDuration && r.Component.Length > 0)
                .OrderByDescending(r => r.Count).Take(3).Select(r => $"{r.Component} {r.Count}"));
            return new WorkSummary(
                Count(CdsMetrics.DrawDuration), Ms(CdsMetrics.DrawDuration), Count(CdsMetrics.VisualsDuration), Ms(CdsMetrics.VisualsDuration),
                Count(CdsMetrics.MeasureDuration), Count(CdsMetrics.ArrangeDuration), Count(CdsMetrics.RebuildDuration), Ms(CdsMetrics.RebuildDuration),
                busiest, measured);
        }

        public void Dispose() => _listener.Dispose();
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

namespace Maui.Carbon.Gallery.Infrastructure;

/// <summary>
/// Screenshot mode for visual review: launch with <c>--snapshot &lt;dir&gt; [--pages a,b] [--themes white,g100]</c>
/// (or the CARBON_SNAPSHOT_DIR / CARBON_SNAPSHOT_PAGES / CARBON_SNAPSHOT_THEMES environment variables). The app visits
/// each page in each theme, saves <c>&lt;route&gt;-&lt;theme&gt;.png</c> and quits.
/// </summary>
public static class SnapshotRunner
{
    public static void AttachIfRequested(Window window)
    {
        var args = Environment.GetCommandLineArgs();
        string? Arg(string name, string env)
        {
            var i = Array.IndexOf(args, name);
            return i >= 0 && i + 1 < args.Length ? args[i + 1] : Environment.GetEnvironmentVariable(env);
        }

        var dir = Arg("--snapshot", "CARBON_SNAPSHOT_DIR");
        if (string.IsNullOrEmpty(dir)) return;
        Directory.CreateDirectory(dir);
        void Trace(string m) => File.AppendAllText(Path.Combine(dir, "trace.txt"), $"{DateTime.Now:HH:mm:ss.fff} {m}\n");
        Trace("attach");
        var category = Arg("--category", "CARBON_SNAPSHOT_CATEGORY");
        var pages = Arg("--pages", "CARBON_SNAPSHOT_PAGES")?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var themes = (Arg("--themes", "CARBON_SNAPSHOT_THEMES") ?? "white,g10,g90,g100")
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(t => Enum.Parse<CdsThemeName>(t, ignoreCase: true))
            .ToArray();

        var started = false;
        async void Start()
        {
            if (started) return;
            started = true;
            Console.WriteLine($"snapshot: writing to {dir}");
            await RunAsync(dir, pages, category, themes);
        }
        // Launched from a shell the app may never become active, so start from the first layout pass.
        window.Created += (_, _) =>
        {
            Trace("created");
            window.Dispatcher.DispatchDelayed(TimeSpan.FromSeconds(2), Start);
        };
        window.Activated += (_, _) => { Trace("activated"); Start(); };
        Application.Current?.Dispatcher.DispatchDelayed(TimeSpan.FromSeconds(4), () => { Trace("fallback timer"); Start(); });
    }

    static async Task RunAsync(string dir, string[]? pages, string? category, CdsThemeName[] themes)
    {
        Directory.CreateDirectory(dir);
        await Task.Delay(1500);
        var entries = GalleryRegistry.Entries
            .Where(e => category is null || string.Equals(e.Info.Category.ToString(), category, StringComparison.OrdinalIgnoreCase))
            .Where(e => pages is null || pages.Any(p =>
                string.Equals(p, e.Info.Title.Replace(" ", ""), StringComparison.OrdinalIgnoreCase) ||
                string.Equals(p, e.Info.Route, StringComparison.OrdinalIgnoreCase)))
            .ToList();
        var log = new List<string>();
        foreach (var entry in entries)
        {
            foreach (var theme in themes)
            {
                try
                {
                    var watch = System.Diagnostics.Stopwatch.StartNew();
                    CdsThemeManager.SetTheme(theme);
                    File.AppendAllText(Path.Combine(dir, "trace.txt"), $"{DateTime.Now:HH:mm:ss.fff} {entry.Info.Route} switch {theme} {watch.ElapsedMilliseconds}ms\n");
                    await Shell.Current.GoToAsync("//" + entry.Info.Route);
                    await Task.Delay(int.TryParse(Environment.GetEnvironmentVariable("CARBON_SNAPSHOT_SETTLE_MS"), out var settle) ? settle : 1500);
                    var page = Shell.Current.CurrentPage;
                    await WaitForOverlaysAsync(page);
                    var shot = await page.CaptureAsync();
                    if (shot is null)
                    {
                        log.Add($"{entry.Info.Route} {theme}: capture returned null");
                        continue;
                    }
                    var path = Path.Combine(dir, $"{entry.Info.Route}-{theme.ToString().ToLowerInvariant()}.png");
                    await using var src = await shot.OpenReadAsync(ScreenshotFormat.Png);
                    await using var dst = File.Create(path);
                    await src.CopyToAsync(dst);
                    var host = page is ContentPage hp ? CdsOverlayHost.Find(hp) : null;
                    var overlays = host is null ? "no host" : string.Join("; ", host.Overlays.Select(o =>
                        $"{o.Content.GetType().Name} open={o.IsOpen} placement={o.ActualPlacement} size={o.Content.Width:0}x{o.Content.Height:0} " +
                        $"opacity={(o.Content.Parent as VisualElement)?.Opacity:0.##} bounds={(o.Content.Parent as VisualElement)?.Bounds}"));
                    log.Add($"{entry.Info.Route} {theme}: overlays [{overlays}]");
                    log.Add($"{entry.Info.Route} {theme}: ok bg={page.BackgroundColor?.ToArgbHex()} template={(page as ContentPage)?.ControlTemplate is not null} host={(page is ContentPage cp ? CdsOverlayHost.Find(cp) is not null : false)} style={page.Style is not null}");
                    Console.WriteLine($"snapshot: {entry.Info.Route} {theme}");
                }
                catch (Exception ex)
                {
                    log.Add($"{entry.Info.Route} {theme}: {ex.GetType().Name}: {ex.Message}");
                }
            }
        }
        await File.AppendAllLinesAsync(Path.Combine(dir, "snapshot-log.txt"), log);
        Application.Current?.Quit();
    }

    /// <summary>
    /// Waits (up to 4s) until every open overlay on the page has faded in: in Debug builds a slow theme switch can block
    /// the main thread long enough that a capture would catch an overlay mid fade.
    /// </summary>
    static async Task WaitForOverlaysAsync(Page page)
    {
        if (page is not ContentPage content || CdsOverlayHost.Find(content) is not { } host) return;
        for (var i = 0; i < 20; i++)
        {
            if (host.Overlays.All(o => (o.Content.Parent as VisualElement)?.Opacity is null or >= 0.99)) return;
            await Task.Delay(200);
        }
    }
}

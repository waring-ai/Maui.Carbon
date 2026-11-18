using Maui.Carbon.Relay.Abstractions;
using Maui.Carbon.Relay.Models;
using Maui.Carbon.Relay.Pages;
using Maui.Carbon.Relay.Services;
using Maui.Carbon.Relay.ViewModels;

namespace Maui.Carbon.Relay.Hosting;

/// <summary>
/// Screenshot mode for visual review, adapted from the gallery's SnapshotRunner. Launch with
/// <c>CARBON_SNAPSHOT_DIR=&lt;dir&gt;</c> (optionally <c>CARBON_SNAPSHOT_PAGES=issues,board</c>,
/// <c>CARBON_SNAPSHOT_THEMES=white,g100</c>, <c>CARBON_SNAPSHOT_SETTLE_MS</c>) or the matching <c>--snapshot</c>,
/// <c>--pages</c>, <c>--themes</c> arguments. The app captures the login page, signs in, visits every screen plus a few
/// open states (panels, modals) in each theme, writes <c>&lt;shot&gt;-&lt;theme&gt;.png</c> and quits. Mock latency and
/// the live notification timer are off in this mode.
/// </summary>
public static class RouteSnapshotRunner
{
    static string? Arg(string name, string env) => LaunchArgs.Get(name, env);

    public static bool IsRequested => Arg("--snapshot", "CARBON_SNAPSHOT_DIR") is not null;

    sealed record Shot(string Name, Func<IServiceProvider, Task> Setup, Func<IServiceProvider, Page> Page);

    static Page MainPageOf(IServiceProvider s) => s.GetRequiredService<MainPage>();

    static Shot Route(string name, string route, Func<IServiceProvider, object?>? parameter = null, Action<IServiceProvider>? then = null) =>
        new(name, async s =>
        {
            s.GetRequiredService<ShellViewModel>().ActivePanel = ShellPanel.None;
            await s.GetRequiredService<INavigator>().NavigateAsync(route, parameter?.Invoke(s));
            then?.Invoke(s);
        }, MainPageOf);

    static Shot Panel(string name, ShellPanel panel, Func<IServiceProvider, Task>? before = null) =>
        new(name, async s =>
        {
            await s.GetRequiredService<INavigator>().NavigateAsync(Routes.Dashboard);
            if (before is not null) await before(s);
            s.GetRequiredService<ShellViewModel>().ActivePanel = panel;
        }, MainPageOf);

    static string DetailKey(IServiceProvider s) =>
        s.GetRequiredService<IIssueService>().All.First(i => i.StackTrace is not null && i.IsOpen).Key;

    static IReadOnlyList<Shot> Shots() =>
    [
        Route("dashboard", Routes.Dashboard),
        Route("issues", Routes.Issues),
        Route("issues-delete", Routes.Issues, then: s =>
        {
            var vm = s.GetRequiredService<IssuesViewModel>();
            if (vm.Rows.Count > 1) vm.Selected = vm.Rows.Take(2).ToList();
            vm.RequestDeleteSelectedCommand.Execute(null);
        }),
        Route("issue", Routes.IssueDetail, DetailKey),
        Route("create", Routes.CreateIssue),
        Route("board", Routes.Board),
        Route("backlog", Routes.Backlog),
        Route("projects", Routes.Projects),
        Route("team", Routes.Team),
        Route("reports", Routes.Reports),
        Route("settings", Routes.Settings),
        Panel("panel-notifications", ShellPanel.Notifications),
        Panel("panel-switcher", ShellPanel.Switcher),
        Panel("panel-account", ShellPanel.Account),
        Panel("panel-assistant", ShellPanel.Assistant, s => s.GetRequiredService<AssistantViewModel>().SummarizeSprintCommand.ExecuteAsync(null)),
        new("about", async s =>
        {
            await s.GetRequiredService<INavigator>().NavigateAsync(Routes.Dashboard);
            s.GetRequiredService<ShellViewModel>().ShowAboutCommand.Execute(null);
        }, MainPageOf),
    ];

    public static void AttachIfRequested(Window window, IServiceProvider services)
    {
        var dir = Arg("--snapshot", "CARBON_SNAPSHOT_DIR");
        if (string.IsNullOrEmpty(dir)) return;
        LiveFeedTimer.Enabled = false;
        Directory.CreateDirectory(dir);
        var pages = Arg("--pages", "CARBON_SNAPSHOT_PAGES")?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var themes = (Arg("--themes", "CARBON_SNAPSHOT_THEMES") ?? "white,g10,g90,g100")
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(t => Enum.Parse<ThemeChoice>(t, ignoreCase: true))
            .ToArray();

        var started = false;
        async void Start()
        {
            if (started) return;
            started = true;
            try
            {
                await RunAsync(dir, pages, themes, services);
            }
            catch (Exception ex)
            {
                Serilog.Log.ForContext(typeof(RouteSnapshotRunner)).Fatal(ex, "Screenshot run failed");
                File.AppendAllText(Path.Combine(dir, "snapshot-log.txt"), $"fatal: {ex}\n");
            }
            Application.Current?.Quit();
        }
        // Launched from a shell the app may never become active, so also start from a timer.
        window.Created += (_, _) => window.Dispatcher.DispatchDelayed(TimeSpan.FromSeconds(2), Start);
        window.Activated += (_, _) => Start();
        Application.Current?.Dispatcher.DispatchDelayed(TimeSpan.FromSeconds(4), Start);
    }

    static async Task RunAsync(string dir, string[]? pages, ThemeChoice[] themes, IServiceProvider services)
    {
        await Task.Delay(1500);
        var settle = int.TryParse(Environment.GetEnvironmentVariable("CARBON_SNAPSHOT_SETTLE_MS"), out var ms) ? ms : 1500;
        var theme = services.GetRequiredService<IThemeService>();
        var log = new List<string>();
        bool Wanted(string name) => pages is null || pages.Contains(name, StringComparer.OrdinalIgnoreCase);

        if (Wanted("login"))
        {
            foreach (var t in themes)
            {
                theme.Apply(t);
                await Task.Delay(settle);
                await CaptureAsync(services.GetRequiredService<LoginPage>(), dir, "login", t, log);
            }
        }

        var auth = services.GetRequiredService<IAuthService>();
        await auth.SignInAsync(LoginViewModel.DemoEmail, "snapshot", remember: false);
        services.GetRequiredService<ISessionHost>().ShowApp();
        await Task.Delay(settle);

        foreach (var shot in Shots().Where(s => Wanted(s.Name)))
        {
            foreach (var t in themes)
            {
                try
                {
                    theme.Apply(t);
                    await shot.Setup(services);
                    await Task.Delay(settle);
                    await CaptureAsync(shot.Page(services), dir, shot.Name, t, log);
                }
                catch (Exception ex)
                {
                    Serilog.Log.ForContext(typeof(RouteSnapshotRunner)).Error(ex, "Screenshot {Shot} in {Theme} failed", shot.Name, t);
                    log.Add($"{shot.Name} {t}: {ex.GetType().Name}: {ex.Message}");
                }
            }
            // Leave modals and panels closed for the next shot.
            services.GetRequiredService<IssuesViewModel>().CancelDeleteCommand.Execute(null);
            services.GetRequiredService<ShellViewModel>().ActivePanel = ShellPanel.None;
            services.GetRequiredService<ShellViewModel>().IsAboutOpen = false;
        }
        await File.AppendAllLinesAsync(Path.Combine(dir, "snapshot-log.txt"), log);
    }

    static async Task CaptureAsync(Page page, string dir, string name, ThemeChoice theme, List<string> log)
    {
        if (page is ContentPage content && CdsOverlayHost.Find(content) is { } host)
        {
            // Wait (up to 4s) for open overlays to finish fading in.
            for (var i = 0; i < 20 && !host.Overlays.All(o => (o.Content.Parent as VisualElement)?.Opacity is null or >= 0.99); i++)
                await Task.Delay(200);
        }
        var shot = await page.CaptureAsync();
        if (shot is null)
        {
            log.Add($"{name} {theme}: capture returned null");
            return;
        }
        var path = Path.Combine(dir, $"{name}-{theme.ToString().ToLowerInvariant()}.png");
        await using var src = await shot.OpenReadAsync(ScreenshotFormat.Png);
        await using var dst = File.Create(path);
        await src.CopyToAsync(dst);
        log.Add($"{name} {theme}: ok");
    }
}

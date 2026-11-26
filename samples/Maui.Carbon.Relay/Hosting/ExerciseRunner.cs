using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Maui.Carbon.Relay.Abstractions;
using Maui.Carbon.Relay.Models;
using Maui.Carbon.Relay.Pages;
using Maui.Carbon.Relay.Services;
using Maui.Carbon.Relay.ViewModels;
using Serilog.Events;

namespace Maui.Carbon.Relay.Hosting;

/// <summary>
/// Exercise mode: drives Relay through everything it can do and reports what the Carbon framework (and MAUI under it)
/// got wrong. Launch with <c>RELAY_EXERCISE_DIR=&lt;dir&gt;</c> or <c>--exercise &lt;dir&gt;</c>
/// (optionally <c>RELAY_EXERCISE_THEMES=white,g100</c>, <c>RELAY_EXERCISE_SIZES=480,1440</c>,
/// <c>RELAY_EXERCISE_QUICK=1</c> for fewer combinations). It runs, then quits, leaving in the directory:
/// <list type="bullet">
/// <item><c>exercise-report.md</c> - findings grouped and ranked; <c>exercise-findings.json</c> - the same as data.</item>
/// <item><c>relay-&lt;date&gt;.log</c> - the full Serilog log; <c>findings/*.png</c> - a capture of the first state each
/// visual finding appeared in.</item>
/// </list>
/// Phases: the login page; every screen and open state (the screenshot list) in every theme at every window size;
/// interactions (filters, empty states, validation, dialogs, wizard steps, tabs, toasts); leak checks (repeat a cycle,
/// collect garbage, look for element types whose live count grew with it); stress (rapid navigation, theme flips,
/// resizes, panel toggles). Every state gets a <see cref="VisualAudit"/>; every warning or error logged meanwhile is
/// attributed to the step that was running; a heartbeat on the UI thread records hangs; Maui.Carbon's metrics are totalled
/// per step and component (<see cref="ExerciseMetrics"/>), including what still draws once a screen has settled.
/// </summary>
public static class ExerciseRunner
{
    public static bool IsRequested => Dir is not null;

    static string? Dir => LaunchArgs.Get("--exercise", "RELAY_EXERCISE_DIR");

    public static void AttachIfRequested(Window window, IServiceProvider services)
    {
        if (Dir is not { } dir) return;
        LiveFeedTimer.Enabled = false;
        Directory.CreateDirectory(dir);
        var started = false;
        async void Start()
        {
            if (started) return;
            started = true;
            var run = new Run(dir, window, services);
            try
            {
                await run.ExecuteAsync();
            }
            catch (Exception ex)
            {
                Serilog.Log.ForContext(typeof(ExerciseRunner)).Fatal(ex, "Exercise run aborted");
                run.Aborted = ex;
            }
            try
            {
                run.WriteReport();
            }
            catch (Exception ex)
            {
                File.WriteAllText(Path.Combine(dir, "exercise-report.md"), $"# Report failed\n\n{ex}\n\nRun aborted: {run.Aborted}");
            }
            ExerciseLog.Step = null;
            Application.Current?.Quit();
        }
        window.Created += (_, _) => window.Dispatcher.DispatchDelayed(TimeSpan.FromSeconds(2), Start);
        window.Activated += (_, _) => Start();
        Application.Current?.Dispatcher.DispatchDelayed(TimeSpan.FromSeconds(4), Start);
    }

    sealed record Size(string Name, double Width, double Height);

    sealed record Interaction(string Name, string Route, Func<IServiceProvider, object?>? Parameter, Func<IServiceProvider, Task> Act, Func<IServiceProvider, Task>? Undo = null);

    sealed class Group(string kind, string key)
    {
        public string Kind { get; } = kind;
        public string Key { get; } = key;
        public string Example { get; set; } = "";
        public double? Worst { get; set; }
        public int Hits { get; set; }
        public SortedSet<string> States { get; } = new(StringComparer.Ordinal);
        public string? Capture { get; set; }
    }

    sealed class Run(string dir, Window window, IServiceProvider services)
    {
        const int LeakCycles = 6;

        readonly Dictionary<string, Group> _groups = [];
        readonly List<(string Step, double Ms)> _hangs = [];
#if IOS || MACCATALYST
        Foundation.NSObject? _activity;
#endif
        readonly List<(string Step, double Ms)> _slow = [];
        readonly List<(string Scenario, string Type, int Before, int After)> _leaks = [];
        readonly List<(string Scenario, string Type, string Roots)> _leakRoots = [];
        readonly List<(string Scenario, string Subscription, int Before, int After)> _retainers = [];
        readonly List<(string Scenario, string Held, string Path)> _leakPaths = [];
        readonly List<string> _notes = [];
        readonly List<(string Requested, string Actual)> _sizes = [];
        readonly Stopwatch _clock = Stopwatch.StartNew();
        readonly ExerciseMetrics _metrics = new();
        int _states;
        int _captures;
        string _sizeName = "lg";
        DateTime _lastBeat = DateTime.UtcNow;

        public Exception? Aborted { get; set; }

        T Get<T>() where T : notnull => services.GetRequiredService<T>();

        MainPage Main => Get<MainPage>();

        static ThemeChoice[] ParseThemes(string? s, ThemeChoice[] fallback) => s is null
            ? fallback
            : s.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Select(t => Enum.Parse<ThemeChoice>(t, true)).ToArray();

        static Size SizeFor(double width) => new(width switch
        {
            < 672 => "sm",
            < 1056 => "md",
            < 1312 => "lg",
            < 1584 => "xlg",
            _ => "max",
        } + $"-{width:0}", width, 900);

        public async Task ExecuteAsync()
        {
            var quick = LaunchArgs.Get("--quick", "RELAY_EXERCISE_QUICK") is not null;
            var themes = ParseThemes(LaunchArgs.Get("--themes", "RELAY_EXERCISE_THEMES"),
                quick ? [ThemeChoice.White, ThemeChoice.G100] : [ThemeChoice.White, ThemeChoice.G10, ThemeChoice.G90, ThemeChoice.G100]);
            var sizes = (LaunchArgs.Get("--sizes", "RELAY_EXERCISE_SIZES") ?? (quick ? "480,1440" : "480,800,1200,1600"))
                .Split(',', StringSplitOptions.RemoveEmptyEntries).Select(w => SizeFor(double.Parse(w, CultureInfo.InvariantCulture))).ToArray();
            // RELAY_EXERCISE_PHASES=login,screens,interactions,leaks,stress,live-resize runs only those.
            var phases = LaunchArgs.Get("--phases", "RELAY_EXERCISE_PHASES")?.Split(',', StringSplitOptions.TrimEntries).ToHashSet();
            bool Phase(string name) => phases is null || phases.Contains(name);
            var pairThemes = themes.Length > 1 ? new[] { themes[0], themes[^1] } : themes;
            var pairSizes = sizes.Length > 1 ? new[] { sizes[0], sizes[^1] } : sizes;
            var large = sizes[^1];

            StartHeartbeat();
            Step("start");
            await Task.Delay(1500);

            // 1. Login page.
            foreach (var size in Phase("login") ? pairSizes : [])
            {
                await ResizeAsync(size);
                foreach (var theme in pairThemes) await LoginStatesAsync(theme);
            }

            await SignInAsync();

            // 2. Every screen and open state, every theme, every size.
            foreach (var size in Phase("screens") ? sizes : [])
            {
                await ResizeAsync(size);
                foreach (var theme in themes)
                {
                    Theme(theme);
                    foreach (var shot in RouteSnapshotRunner.Shots())
                    {
                        var state = $"{shot.Name} @ {_sizeName}/{Name(theme)}";
                        await Guard(state, async () =>
                        {
                            Step(state);
                            var sw = Stopwatch.StartNew();
                            await shot.Setup(services);
                            if (sw.Elapsed.TotalMilliseconds > 300) _slow.Add((state, sw.Elapsed.TotalMilliseconds));
                            await Task.Delay(500);
                            await AuditAsync(state, shot.Page(services));
                        });
                        await ResetAsync();
                    }
                }
            }

            // 3. Interactions.
            foreach (var size in Phase("interactions") ? pairSizes : [])
            {
                await ResizeAsync(size);
                foreach (var theme in pairThemes)
                {
                    Theme(theme);
                    foreach (var i in Interactions())
                    {
                        var state = $"{i.Name} @ {_sizeName}/{Name(theme)}";
                        await Guard(state, async () =>
                        {
                            Step(state);
                            await Get<INavigator>().NavigateAsync(i.Route, i.Parameter?.Invoke(services));
                            await Task.Delay(300);
                            await i.Act(services);
                            await Task.Delay(500);
                            await AuditAsync(state, Main);
                            if (i.Undo is not null) await i.Undo(services);
                        });
                        await ResetAsync();
                    }
                    await CreateWizardAsync(theme);
                    await IssueTabsAsync(theme);
                }
            }

            WriteReport();

            // 4. Leaks.
            await ResizeAsync(large);
            Theme(themes[0]);
            if (Phase("leaks")) await LeakChecksAsync();

            WriteReport();

            // Profiling aid (phase "visit", not in the default run): RELAY_EXERCISE_VISIT=<route> shows that route 30 times
            // from the dashboard, timing each visit, so a sampler attached meanwhile sees mostly that screen's work.
            if (phases?.Contains("visit") == true && LaunchArgs.Get("--visit", "RELAY_EXERCISE_VISIT") is { } visit)
            {
                var nav = Get<INavigator>();
                // VISIT=panel opens and closes the notifications panel instead.
                for (var i = 0; i < 30 && visit == "panel"; i++)
                {
                    Step($"visit panel #{i}");
                    Get<ShellViewModel>().ActivePanel = ShellPanel.Notifications;
                    await Task.Delay(900);
                    Get<ShellViewModel>().ActivePanel = ShellPanel.None;
                    await Task.Delay(500);
                }
                for (var i = 0; i < 30 && visit != "panel"; i++)
                {
                    await nav.NavigateAsync(Routes.Dashboard);
                    await Task.Delay(400);
                    Step($"visit {visit} #{i}");
                    await nav.NavigateAsync(visit);
                    await Task.Delay(1200);
                }
            }

            // 5. Stress. Resizing with motion on goes last: it is the step most likely to take the app down.
            if (Phase("stress")) await StressAsync();
            WriteReport();
            if (Phase("live-resize")) await LiveResizeAsync(sizes);

            // Surface unobserved task exceptions before reporting.
            Step("final collect");
            await CollectAsync();
            await Task.Delay(500);
        }

        // ---- phases ----

        async Task LoginStatesAsync(ThemeChoice theme)
        {
            var host = Get<ISessionHost>();
            host.ShowLogin();
            Theme(theme);
            var login = Get<LoginViewModel>();
            var page = Get<LoginPage>();
            var suffix = $"@ {_sizeName}/{Name(theme)}";

            async Task State(string name, Func<Task> act)
            {
                var state = $"login-{name} {suffix}";
                await Guard(state, async () =>
                {
                    Step(state);
                    await act();
                    await Task.Delay(400);
                    await AuditAsync(state, page);
                });
            }

            await State("email", () => { login.ChangeEmailCommand.Execute(null); login.Email = LoginViewModel.DemoEmail; return Task.CompletedTask; });
            await State("invalid-email", async () => { login.Email = "not-an-email"; await login.ContinueCommand.ExecuteAsync(null); });
            await State("password", async () => { login.Email = LoginViewModel.DemoEmail; await login.ContinueCommand.ExecuteAsync(null); });
            await State("password-error", async () => { login.Password = "x"; await login.SignInCommand.ExecuteAsync(null); });
            login.ChangeEmailCommand.Execute(null);
            login.Email = LoginViewModel.DemoEmail;
            login.Password = "";
        }

        async Task SignInAsync()
        {
            Step("sign in");
            await Get<IAuthService>().SignInAsync(LoginViewModel.DemoEmail, "exercise", remember: false);
            Get<ISessionHost>().ShowApp();
            await Task.Delay(800);
        }

        IEnumerable<Interaction> Interactions()
        {
            Task Do(Action a) { a(); return Task.CompletedTask; }
            IssuesViewModel Issues(IServiceProvider s) => s.GetRequiredService<IssuesViewModel>();
            BoardViewModel Board(IServiceProvider s) => s.GetRequiredService<BoardViewModel>();
            ProjectsViewModel Projects(IServiceProvider s) => s.GetRequiredService<ProjectsViewModel>();
            TeamViewModel Team(IServiceProvider s) => s.GetRequiredService<TeamViewModel>();
            ReportsViewModel Reports(IServiceProvider s) => s.GetRequiredService<ReportsViewModel>();
            SettingsViewModel Settings(IServiceProvider s) => s.GetRequiredService<SettingsViewModel>();
            object? DetailKey(IServiceProvider s) => s.GetRequiredService<IIssueService>().All.First(i => i.StackTrace is not null && i.IsOpen).Key;

            yield return new("dashboard-dismiss-notice", Routes.Dashboard, null,
                s => Do(() => s.GetRequiredService<DashboardViewModel>().DismissSprintNoticeCommand.Execute(null)));
            yield return new("issues-filter-panel", Routes.Issues, null, s => Do(() => Issues(s).IsFilterPanelOpen = true), s => Do(() => Issues(s).IsFilterPanelOpen = false));
            yield return new("issues-open-only", Routes.Issues, null, s => Do(() => Issues(s).OpenOnly = true), s => Do(() => Issues(s).ClearFiltersCommand.Execute(null)));
            yield return new("issues-empty", Routes.Issues, null, s => Do(() => Issues(s).SearchText = "zzzq no match"), s => Do(() => Issues(s).SearchText = ""));
            yield return new("issues-page-2", Routes.Issues, null, s => Do(() => Issues(s).Page = 2), s => Do(() => Issues(s).Page = 1));
            yield return new("issues-scope-1", Routes.Issues, null, s => Do(() => Issues(s).ScopeIndex = 1), s => Do(() => Issues(s).ScopeIndex = 0));
            yield return new("board-select-mode", Routes.Board, null, s => Do(() => Board(s).IsSelectMode = true), s => Do(() => Board(s).IsSelectMode = false));
            yield return new("board-filters", Routes.Board, null, s => Do(() =>
            {
                foreach (var f in Board(s).Filters) Board(s).ToggleFilterCommand.Execute(f);
            }), s => Do(() => Board(s).ClearFiltersCommand.Execute(null)));
            yield return new("board-blocked", Routes.Board, null, s => Do(() => Board(s).ToggleBlockedCommand.Execute(null)), s => Do(() => Board(s).ShowBlockedOnly = false));
            yield return new("projects-create", Routes.Projects, null, s => Do(() =>
            {
                Projects(s).NewProjectCommand.Execute(null);
                Projects(s).ShowValidation = true;
                Projects(s).RepositoryQuery = "ma";
            }), s => Do(() => Projects(s).CancelCreateCommand.Execute(null)));
            yield return new("projects-empty", Routes.Projects, null, s => Do(() => Projects(s).SearchText = "zzzq no match"), s => Do(() => Projects(s).SearchText = ""));
            yield return new("projects-starred", Routes.Projects, null, s => Do(() => Projects(s).StarredOnly = true), s => Do(() => Projects(s).StarredOnly = false));
            yield return new("team-selected", Routes.Team, null, s => Do(() => Team(s).Selected = Team(s).Rows.FirstOrDefault()), s => Do(() => Team(s).Selected = null));
            yield return new("team-empty", Routes.Team, null, s => Do(() => Team(s).SearchText = "zzzq no match"), s => Do(() => Team(s).ClearSearchCommand.Execute(null)));
            yield return new("reports-range-0", Routes.Reports, null, s => Do(() => Reports(s).RangeIndex = 0), s => Do(() => Reports(s).RangeIndex = 1));
            yield return new("reports-range-2", Routes.Reports, null, s => Do(() => Reports(s).RangeIndex = 2), s => Do(() => Reports(s).RangeIndex = 1));
            yield return new("reports-custom", Routes.Reports, null, s => Do(() =>
            {
                Reports(s).CustomStart = Reports(s).Today.AddDays(-20);
                Reports(s).CustomEnd = Reports(s).Today;
            }), s => Do(() => Reports(s).ClearCustomRangeCommand.Execute(null)));
            yield return new("settings-invalid", Routes.Settings, null, s => Do(() =>
            {
                Settings(s).DisplayName = "";
                Settings(s).SlackEnabled = true;
                Settings(s).SlackChannel = "general";
            }), s => Do(() => { if (Settings(s).DiscardCommand.CanExecute(null)) Settings(s).DiscardCommand.Execute(null); }));
            yield return new("settings-delete", Routes.Settings, null, s => Do(() =>
            {
                Settings(s).RequestDeleteCommand.Execute(null);
                Settings(s).DeleteConfirmText = "wrong";
            }), s => Do(() => Settings(s).CancelDeleteCommand.Execute(null)));
            yield return new("issue-move-dialog", Routes.IssueDetail, DetailKey,
                s => Do(() => CurrentDetail(s)?.OpenMoveDialogCommand.Execute(null)), s => Do(() => CurrentDetail(s)?.CancelMoveCommand.Execute(null)));
            yield return new("issue-delete-confirm", Routes.IssueDetail, DetailKey,
                s => Do(() => CurrentDetail(s)?.RequestDeleteCommand.Execute(null)), s => Do(() => CurrentDetail(s)?.CancelDeleteCommand.Execute(null)));
            yield return new("issue-long-comment", Routes.IssueDetail, DetailKey,
                s => Do(() => { if (CurrentDetail(s) is { } d) { d.SelectedTabIndex = 1; d.NewComment = string.Concat(Enumerable.Repeat("A long draft comment without breaks ", 12)); } }),
                s => Do(() => { if (CurrentDetail(s) is { } d) { d.NewComment = ""; d.SelectedTabIndex = 0; } }));
            yield return new("toasts", Routes.Dashboard, null, s => Do(() =>
            {
                var f = s.GetRequiredService<IUserFeedback>();
                f.Toast(FeedbackKind.Error, "Exercise error toast", "A subtitle long enough to wrap onto a second line inside the toast body.");
                f.Toast(FeedbackKind.Warning, "Exercise warning toast");
                f.Toast(FeedbackKind.Success, "Exercise success toast", "Done");
            }), s => Do(CloseOverlays));
            yield return new("notifications-read", Routes.Dashboard, null, s => Do(() =>
            {
                var shell = s.GetRequiredService<ShellViewModel>();
                shell.ActivePanel = ShellPanel.Notifications;
                shell.MarkAllReadCommand.Execute(null);
            }));
        }

        static IssueDetailViewModel? CurrentDetail(IServiceProvider s) =>
            s.GetRequiredService<Navigator>().CurrentView?.BindingContext as IssueDetailViewModel;

        async Task CreateWizardAsync(ThemeChoice theme)
        {
            var suffix = $"@ {_sizeName}/{Name(theme)}";
            await Guard($"create-wizard {suffix}", async () =>
            {
                Step($"create-wizard {suffix}");
                await Get<INavigator>().NavigateAsync(Routes.CreateIssue);
                await Task.Delay(300);
                if (Get<Navigator>().CurrentView?.BindingContext is not CreateIssueViewModel vm) return;

                async Task State(string name, Func<Task> act)
                {
                    var state = $"create-{name} {suffix}";
                    Step(state);
                    await act();
                    await Task.Delay(450);
                    await AuditAsync(state, Main);
                }

                await State("type-invalid", () => vm.NextCommand.ExecuteAsync(null));
                await State("details", () =>
                {
                    vm.SelectedType = Enum.GetValues<IssueType>()[0];
                    vm.SelectedProject = vm.ProjectOptions.FirstOrDefault();
                    return vm.NextCommand.ExecuteAsync(null);
                });
                await State("details-invalid", () => vm.NextCommand.ExecuteAsync(null));
                await State("schedule", () =>
                {
                    vm.Summary = "Exercise summary that is fairly long so the review wraps";
                    vm.Description = "x";
                    return vm.NextCommand.ExecuteAsync(null);
                });
                await State("review", () => vm.NextCommand.ExecuteAsync(null));
                await State("discard-confirm", () => vm.CancelCommand.ExecuteAsync(null));
                vm.KeepEditingCommand.Execute(null);
            });
            await ResetAsync();
        }

        async Task IssueTabsAsync(ThemeChoice theme)
        {
            var suffix = $"@ {_sizeName}/{Name(theme)}";
            await Guard($"issue-tabs {suffix}", async () =>
            {
                var key = Get<IIssueService>().All.OrderByDescending(i => i.StackTrace is not null).First().Key;
                await Get<INavigator>().NavigateAsync(Routes.IssueDetail, key);
                await Task.Delay(300);
                if (CurrentDetail(services) is not { } vm) return;
                for (var tab = 1; tab < 4; tab++)
                {
                    var state = $"issue-tab-{tab} {suffix}";
                    Step(state);
                    vm.SelectedTabIndex = tab;
                    await Task.Delay(450);
                    await AuditAsync(state, Main);
                }
                vm.SelectedTabIndex = 0;
            });
            await ResetAsync();
        }

        async Task LeakChecksAsync()
        {
            var nav = Get<INavigator>();
            var shell = Get<ShellViewModel>();
            var keys = Get<IIssueService>().All.Select(i => i.Key).Take(LeakCycles).ToArray();

            await LeakAsync("issue detail open/leave", async i =>
            {
                await nav.NavigateAsync(Routes.IssueDetail, keys[i % keys.Length]);
                await Task.Delay(400);
                VisualAudit.TrackTree(Main);
                await nav.NavigateAsync(Routes.Dashboard);
            });
            await LeakAsync("create issue open/leave", async _ =>
            {
                await nav.NavigateAsync(Routes.CreateIssue);
                await Task.Delay(400);
                VisualAudit.TrackTree(Main);
                await nav.NavigateAsync(Routes.Dashboard);
            });
            await LeakAsync("header panels open/close", async _ =>
            {
                foreach (var panel in new[] { ShellPanel.Notifications, ShellPanel.Switcher, ShellPanel.Account, ShellPanel.Assistant })
                {
                    shell.ActivePanel = panel;
                    await Task.Delay(350);
                    VisualAudit.TrackTree(Main);
                    shell.ActivePanel = ShellPanel.None;
                    await Task.Delay(250);
                }
            });
            await LeakAsync("modals open/close", async _ =>
            {
                shell.ShowAboutCommand.Execute(null);
                await Task.Delay(400);
                VisualAudit.TrackTree(Main);
                shell.CloseAboutCommand.Execute(null);
                await nav.NavigateAsync(Routes.Projects);
                Get<ProjectsViewModel>().NewProjectCommand.Execute(null);
                await Task.Delay(400);
                VisualAudit.TrackTree(Main);
                Get<ProjectsViewModel>().CancelCreateCommand.Execute(null);
                await nav.NavigateAsync(Routes.Settings);
                Get<SettingsViewModel>().RequestDeleteCommand.Execute(null);
                await Task.Delay(400);
                VisualAudit.TrackTree(Main);
                Get<SettingsViewModel>().CancelDeleteCommand.Execute(null);
                await nav.NavigateAsync(Routes.Dashboard);
                await Task.Delay(300);
            });
            await LeakAsync("toasts show/close", async _ =>
            {
                Get<IUserFeedback>().Toast(FeedbackKind.Info, "Leak check toast", "Subtitle");
                Get<IUserFeedback>().Toast(FeedbackKind.Error, "Leak check error toast");
                await Task.Delay(400);
                VisualAudit.TrackTree(Main);
                CloseOverlays();
                await Task.Delay(400);
            });
            var original = shell.CurrentProject;
            var projects = Get<IProjectService>().All;
            await LeakAsync("project switch", async i =>
            {
                shell.SelectProjectCommand.Execute(projects[(i + 1) % projects.Count]);
                await Task.Delay(400);
                VisualAudit.TrackTree(Main);
            });
            shell.SelectProjectCommand.Execute(original);
            await LeakAsync("theme cycle", async _ =>
            {
                foreach (var t in new[] { ThemeChoice.G100, ThemeChoice.G10, ThemeChoice.G90, ThemeChoice.White })
                {
                    Theme(t);
                    await Task.Delay(250);
                }
                VisualAudit.TrackTree(Main);
            });
            await LeakAsync("top-level routes", async _ =>
            {
                foreach (var r in new[] { Routes.Issues, Routes.Board, Routes.Backlog, Routes.Projects, Routes.Team, Routes.Reports, Routes.Settings, Routes.Dashboard })
                {
                    await nav.NavigateAsync(r);
                    await Task.Delay(250);
                    VisualAudit.TrackTree(Main);
                }
            });
            await LeakAsync("sign out/in", async _ =>
            {
                Get<ISessionHost>().ShowLogin();
                await Task.Delay(400);
                VisualAudit.TrackTree(Get<LoginPage>());
                await SignInAsync();
                VisualAudit.TrackTree(Main);
            });
        }

        async Task LeakAsync(string scenario, Func<int, Task> cycle)
        {
            await Guard($"leak: {scenario}", async () =>
            {
                Step($"leak: {scenario}");
                // One warm-up cycle so lazily created, then cached, elements are not counted.
                await cycle(0);
                await CollectAsync();
                var before = VisualAudit.CountAlive();
                var subsBefore = RetainerProbe.Snapshot(services);
                for (var i = 1; i <= LeakCycles; i++) await cycle(i);
                await Task.Delay(500);
                await CollectAsync();
                var after = VisualAudit.CountAlive();
                foreach (var (sub, count) in RetainerProbe.Snapshot(services))
                {
                    var was = subsBefore.GetValueOrDefault(sub);
                    if (count - was >= LeakCycles - 1) _retainers.Add((scenario, sub, was, count));
                }
                foreach (var (type, count) in after)
                {
                    var was = before.GetValueOrDefault(type);
                    if (count - was < LeakCycles - 1) continue;
                    _leaks.Add((scenario, type, was, count));
                    _leakRoots.Add((scenario, type, string.Join(", ", VisualAudit.Roots(type).OrderByDescending(r => r.Value).Take(8).Select(r => $"{r.Value} {r.Key}"))));
                }
                // What holds it: the reference chain to the top of one leaked subtree (one per distinct top type).
                var traced = new HashSet<Type>();
                foreach (var (type, _) in after.Where(a => a.Value - before.GetValueOrDefault(a.Key) >= LeakCycles - 1).OrderByDescending(a => a.Value - before.GetValueOrDefault(a.Key)))
                {
                    // Screens registered as singletons are meant to live while off screen: trace what else is held.
                    if (traced.Count >= 3 || VisualAudit.DetachedTop(type, t => ReferenceEquals(services.GetService(t.GetType()), t)) is not { } top
                        || !traced.Add(top.GetType())) continue;
                    _leakPaths.Add((scenario, $"{top.GetType().Name} (holding {type})", HeapPath.Find(top)));
                }
            });
        }

        async Task StressAsync()
        {
            var nav = Get<INavigator>();
            var shell = Get<ShellViewModel>();
            var routes = new[] { Routes.Issues, Routes.Board, Routes.Backlog, Routes.Projects, Routes.Team, Routes.Reports, Routes.Settings, Routes.Dashboard };

            await Guard("stress: rapid navigation", async () =>
            {
                Step("stress: rapid navigation");
                for (var round = 0; round < 3; round++)
                {
                    foreach (var r in routes)
                    {
                        _ = nav.NavigateAsync(r);
                        await Task.Delay(30);
                    }
                }
                await Task.Delay(1500);
                await AuditAsync("stress: after rapid navigation", Main);
            });
            await Guard("stress: theme flips", async () =>
            {
                Step("stress: theme flips");
                await nav.NavigateAsync(Routes.Issues);
                for (var i = 0; i < 24; i++)
                {
                    Theme((ThemeChoice)(i % 4));
                    await Task.Delay(40);
                }
                Theme(ThemeChoice.G100);
                await Task.Delay(1000);
                await AuditAsync("stress: after theme flips", Main);
                Theme(ThemeChoice.White);
            });
            await Guard("stress: panel toggles", async () =>
            {
                Step("stress: panel toggles");
                for (var i = 0; i < 24; i++)
                {
                    shell.ActivePanel = (ShellPanel)(i % 5);
                    await Task.Delay(30);
                }
                shell.ActivePanel = ShellPanel.None;
                await Task.Delay(800);
                await AuditAsync("stress: after panel toggles", Main);
            });
            await Guard("stress: navigation during load", async () =>
            {
                Step("stress: navigation during load");
                var latency = Get<MockLatency>();
                latency.Scale = 1;
                try
                {
                    var keys = Get<IIssueService>().All.Select(i => i.Key).Take(5).ToArray();
                    foreach (var key in keys)
                    {
                        _ = nav.NavigateAsync(Routes.IssueDetail, key);
                        await Task.Delay(60);
                        _ = nav.NavigateAsync(Routes.Reports);
                        await Task.Delay(60);
                    }
                    await nav.NavigateAsync(Routes.Dashboard);
                    await Task.Delay(2500);
                }
                finally
                {
                    latency.Scale = 0;
                }
                await AuditAsync("stress: after navigation during load", Main);
            });
        }

        /// <summary>
        /// Resizes the window across breakpoints with motion on, the way a person dragging the window edge would. Last,
        /// because it can crash the app; the report written before it then says how far the run got.
        /// </summary>
        async Task LiveResizeAsync(Size[] sizes)
        {
            if (sizes.Length < 2) return;
            File.WriteAllText(Path.Combine(dir, "exercise-live-resize.txt"), "started\n");
            await Guard("stress: live resize", async () =>
            {
                var nav = Get<INavigator>();
                await nav.NavigateAsync(Routes.Board);
                CdsMotion.ReducedMotionOverride = false;
                foreach (var (from, to) in new[] { (sizes[^1], sizes[0]), (sizes[0], sizes[^1]) })
                {
                    Step($"stress: live resize {from.Name} -> {to.Name}");
                    window.Width = to.Width;
                    window.Height = to.Height;
                    await Task.Delay(1200);
                }
                CdsMotion.ReducedMotionOverride = null;
                await AuditAsync("stress: after live resize", Main);
            });
            File.AppendAllText(Path.Combine(dir, "exercise-live-resize.txt"), "survived\n");
        }

        // ---- helpers ----

        void Step(string step)
        {
            ExerciseLog.Step = step;
            // One line per step, written straight away, so a crash can be pinned on the step that caused it.
            File.AppendAllText(Path.Combine(dir, "exercise-steps.txt"), $"{_clock.Elapsed:mm\\:ss\\.f} {step}\n");
            Serilog.Log.ForContext(typeof(ExerciseRunner)).Debug("Step {Step}", step);
        }

        static string Name(ThemeChoice theme) => theme.ToString().ToLowerInvariant();

        void Theme(ThemeChoice theme) => Get<IThemeService>().Apply(theme);

        void CloseOverlays()
        {
            if (CdsOverlayHost.Find(Main) is { } host) host.CloseAll();
        }

        async Task ResetAsync()
        {
            try
            {
                Get<IssuesViewModel>().CancelDeleteCommand.Execute(null);
                var shell = Get<ShellViewModel>();
                shell.ActivePanel = ShellPanel.None;
                shell.IsAboutOpen = false;
                CloseOverlays();
            }
            catch (Exception ex)
            {
                Serilog.Log.ForContext(typeof(ExerciseRunner)).Error(ex, "Reset failed after {Step}", ExerciseLog.Step);
            }
            await Task.Delay(150);
        }

        async Task Guard(string state, Func<Task> body)
        {
            try
            {
                await body();
            }
            catch (Exception ex)
            {
                Serilog.Log.ForContext(typeof(ExerciseRunner)).Error(ex, "Exercise step {State} threw", state);
            }
        }

        async Task ResizeAsync(Size size)
        {
            Step($"resize to {size.Name}");
            // Under reduced motion: resizing across a breakpoint with the side nav animating recurses until the stack
            // overflows (see LiveResizeAsync), which would end the run here.
            var motion = CdsMotion.ReducedMotionOverride;
            CdsMotion.ReducedMotionOverride = true;
            window.Width = size.Width;
            window.Height = size.Height;
            await Task.Delay(900);
            CdsMotion.ReducedMotionOverride = motion;
            var page = window.Page;
            var actual = page is null ? "?" : $"{page.Width:0}x{page.Height:0}";
            _sizes.Add(($"{size.Width:0}x{size.Height:0}", actual));
            _sizeName = page is { Width: > 0 } p ? SizeFor(p.Width).Name : size.Name;
        }

        static IEnumerable<Element> Descendants(Element e)
        {
            yield return e;
            if (e is IVisualTreeElement t)
                foreach (var c in t.GetVisualChildren().OfType<Element>())
                    foreach (var d in Descendants(c)) yield return d;
        }

        async Task AuditAsync(string state, Page page)
        {
            _states++;
            var before = VisualAudit.Frames(page);
            var findings = VisualAudit.Audit(page);
            _metrics.BeginIdle(state);
            await Task.Delay(600);
            _metrics.EndIdle();
            findings.AddRange(VisualAudit.Unsettled(before, VisualAudit.Frames(page)));
            var capture = false;
            foreach (var f in findings)
            {
                var id = f.Kind + "|" + f.Key;
                if (!_groups.TryGetValue(id, out var g))
                {
                    g = _groups[id] = new Group(f.Kind, f.Key) { Example = f.Detail };
                    if (_captures < 80)
                    {
                        g.Capture = $"findings/{++_captures:000}.png";
                        capture = true;
                    }
                }
                g.Hits++;
                g.States.Add(state);
                if (f.Score is { } score && (g.Worst is null || (f.Kind == "contrast" ? score < g.Worst : score > g.Worst)))
                {
                    g.Worst = score;
                    g.Example = f.Detail + $"  [{state}]";
                }
            }
            if (capture) await CaptureAsync(page, _groups.Values.Where(g => g.Capture is not null && !File.Exists(Path.Combine(dir, g.Capture))).Select(g => g.Capture!).ToList());
        }

        async Task CaptureAsync(Page page, List<string> files)
        {
            try
            {
                var shot = await page.CaptureAsync();
                if (shot is null) return;
                Directory.CreateDirectory(Path.Combine(dir, "findings"));
                foreach (var file in files)
                {
                    await using var src = await shot.OpenReadAsync(ScreenshotFormat.Png);
                    await using var dst = File.Create(Path.Combine(dir, file));
                    await src.CopyToAsync(dst);
                }
            }
            catch (Exception ex)
            {
                _notes.Add($"capture failed: {ex.Message}");
            }
        }

        static async Task CollectAsync()
        {
            for (var i = 0; i < 3; i++)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                await Task.Delay(150);
            }
            GC.Collect();
        }

        void StartHeartbeat()
        {
#if IOS || MACCATALYST
            // An idle app's timers are throttled by App Nap, which shows up here as false hangs of about 500 ms.
            _activity = Foundation.NSProcessInfo.ProcessInfo.BeginActivity(
                Foundation.NSActivityOptions.UserInitiated | Foundation.NSActivityOptions.LatencyCritical, "Relay exercise timing");
#endif
            var timer = Application.Current!.Dispatcher.CreateTimer();
            timer.Interval = TimeSpan.FromMilliseconds(50);
            timer.Tick += (_, _) =>
            {
                var now = DateTime.UtcNow;
                var gap = (now - _lastBeat).TotalMilliseconds;
                _lastBeat = now;
                // GC pauses from the leak checks are ours, not the app's.
                if (gap > 300 && ExerciseLog.Step is { } step && !step.StartsWith("leak", StringComparison.Ordinal) && step != "final collect")
                    _hangs.Add((step, gap));
            };
            timer.Start();
        }

        // ---- report ----

        public void WriteReport()
        {
            // The leak probe walks Parent on orphans whose parent was collected, which makes MAUI warn; those warnings
            // are the probe's, and the orphans are reported under the leak section instead.
            static bool ProbeNoise(ExerciseLog.Entry e) => e.Step.StartsWith("leak:", StringComparison.Ordinal) && e.Message.Contains("has been Garbage Collected", StringComparison.Ordinal);
            var all = ExerciseLog.Instance.Entries.ToList();
            var entries = all.Where(e => !ProbeNoise(e)).ToList();
            var noise = all.Count - entries.Count;
            var logGroups = entries
                .GroupBy(e => (e.Level >= LogEventLevel.Error ? "error" : "warning", e.Source, Normalize(e.Message)))
                .Select(g => new
                {
                    Level = g.Key.Item1,
                    g.Key.Source,
                    Message = g.First().Message,
                    Count = g.Count(),
                    Steps = g.Select(e => e.Step).Distinct().ToList(),
                    Exception = g.Select(e => e.Exception).FirstOrDefault(x => x is not null),
                })
                .OrderBy(g => g.Level == "error" ? 0 : 1).ThenByDescending(g => g.Count)
                .ToList();

            var sb = new StringBuilder();
            sb.AppendLine("# Relay exercise report").AppendLine();
            sb.AppendLine(CultureInfo.InvariantCulture, $"- Ran {_clock.Elapsed:mm\\:ss}, audited {_states} states.");
            sb.AppendLine(CultureInfo.InvariantCulture, $"- Window sizes (requested -> page): {string.Join(", ", _sizes.Distinct().Select(s => $"{s.Requested} -> {s.Actual}"))}");
            sb.AppendLine(CultureInfo.InvariantCulture, $"- Log: {logGroups.Count(g => g.Level == "error")} error kinds, {logGroups.Count(g => g.Level == "warning")} warning kinds; visual findings: {_groups.Count}; leak suspects: {_leaks.Count}; UI hangs over 300 ms: {_hangs.Count}.");
            if (Aborted is not null) sb.AppendLine(CultureInfo.InvariantCulture, $"- **Run aborted:** {Aborted.GetType().Name}: {Aborted.Message}");
            if (noise > 0) sb.AppendLine(CultureInfo.InvariantCulture, $"- Left out: {noise} MAUI \"RealParent has been Garbage Collected\" warnings raised by the leak probe walking orphaned elements.");
            foreach (var n in _notes.Distinct()) sb.AppendLine(CultureInfo.InvariantCulture, $"- Note: {n}");

            sb.AppendLine().AppendLine("## Logged errors and warnings").AppendLine();
            if (logGroups.Count == 0) sb.AppendLine("None.");
            foreach (var g in logGroups)
            {
                sb.AppendLine(CultureInfo.InvariantCulture, $"### [{g.Level}] {g.Source} (x{g.Count})").AppendLine();
                sb.AppendLine(CultureInfo.InvariantCulture, $"{g.Message}").AppendLine();
                sb.AppendLine(CultureInfo.InvariantCulture, $"Steps ({g.Steps.Count}): {string.Join("; ", g.Steps.Take(8))}{(g.Steps.Count > 8 ? "; ..." : "")}").AppendLine();
                if (g.Exception is { } ex) sb.AppendLine("```").AppendLine(string.Join('\n', ex.Split('\n').Take(20))).AppendLine("```").AppendLine();
            }

            foreach (var kind in new[] { "overflow", "contrast", "collapsed-text", "unsettled" })
            {
                var groups = _groups.Values.Where(g => g.Kind == kind)
                    .OrderBy(g => kind == "contrast" ? g.Worst ?? 99 : -(g.Worst ?? g.States.Count))
                    .ToList();
                sb.AppendLine().AppendLine(CultureInfo.InvariantCulture, $"## Visual: {kind} ({groups.Count})").AppendLine();
                if (groups.Count == 0) { sb.AppendLine("None."); continue; }
                foreach (var g in groups)
                {
                    sb.AppendLine(CultureInfo.InvariantCulture, $"- **{g.Key}** - {g.States.Count} states, {g.Hits} hits{(g.Capture is null ? "" : $", capture `{g.Capture}`")}");
                    sb.AppendLine(CultureInfo.InvariantCulture, $"  - worst: {g.Example}");
                    sb.AppendLine(CultureInfo.InvariantCulture, $"  - in: {string.Join("; ", g.States.Take(6))}{(g.States.Count > 6 ? "; ..." : "")}");
                }
            }

            sb.AppendLine().AppendLine("## Leak suspects").AppendLine();
            sb.AppendLine(CultureInfo.InvariantCulture, $"Live elements of a type that grew by at least {LeakCycles - 1} over {LeakCycles} cycles (after a warm-up cycle and garbage collection).").AppendLine();
            if (_leaks.Count == 0) sb.AppendLine("None.");
            foreach (var s in _leaks.GroupBy(l => l.Scenario))
            {
                sb.AppendLine(CultureInfo.InvariantCulture, $"- **{s.Key}**: {string.Join(", ", s.OrderByDescending(l => l.After - l.Before).Take(14).Select(l => $"{l.Type} {l.Before}->{l.After}"))}");
            }

            sb.AppendLine().AppendLine("### Where the leaked elements hang").AppendLine();
            foreach (var s in _leakRoots.GroupBy(l => l.Scenario))
            {
                sb.AppendLine(CultureInfo.InvariantCulture, $"- **{s.Key}**");
                foreach (var l in s.Take(14)) sb.AppendLine(CultureInfo.InvariantCulture, $"  - {l.Type}: {l.Roots}");
            }

            sb.AppendLine().AppendLine("### Growing subscriptions (who holds them)").AppendLine();
            if (_retainers.Count == 0) sb.AppendLine("None.");
            foreach (var r in _retainers) sb.AppendLine(CultureInfo.InvariantCulture, $"- {r.Scenario}: `{r.Subscription}` {r.Before}->{r.After}");

            sb.AppendLine().AppendLine("### What holds them (shortest reference chain from a static field)").AppendLine();
            if (_leakPaths.Count == 0) sb.AppendLine("None.");
            foreach (var p in _leakPaths)
                sb.AppendLine(CultureInfo.InvariantCulture, $"- {p.Scenario}: {p.Held}").AppendLine("  ```").AppendLine("  " + p.Path).AppendLine("  ```");

            sb.AppendLine().AppendLine("## UI thread hangs over 300 ms").AppendLine();
            if (_hangs.Count == 0) sb.AppendLine("None.");
            foreach (var h in _hangs.OrderByDescending(h => h.Ms).Take(25)) sb.AppendLine(CultureInfo.InvariantCulture, $"- {h.Ms:0} ms during {h.Step}");

            sb.AppendLine().AppendLine("## Slow screen setups (over 300 ms)").AppendLine();
            if (_slow.Count == 0) sb.AppendLine("None.");
            foreach (var s in _slow.OrderByDescending(s => s.Ms).Take(25)) sb.AppendLine(CultureInfo.InvariantCulture, $"- {s.Ms:0} ms: {s.Step}");

            _metrics.WriteReport(sb);

            File.WriteAllText(Path.Combine(dir, "exercise-report.md"), sb.ToString());
            File.WriteAllText(Path.Combine(dir, "exercise-findings.json"), JsonSerializer.Serialize(new
            {
                log = logGroups,
                visual = _groups.Values.Select(g => new { g.Kind, g.Key, g.Example, g.Worst, g.Hits, States = g.States.ToList(), g.Capture }),
                leaks = _leaks.Select(l => new { l.Scenario, l.Type, l.Before, l.After }),
                retainers = _retainers.Select(r => new { r.Scenario, r.Subscription, r.Before, r.After }),
                hangs = _hangs.Select(h => new { h.Step, h.Ms }),
                slow = _slow.Select(s => new { s.Step, s.Ms }),
                metrics = _metrics.Data(),
                sizes = _sizes.Distinct().Select(s => new { s.Requested, s.Actual }),
                aborted = Aborted?.ToString(),
            }, new JsonSerializerOptions { WriteIndented = true }));
        }

        static string Normalize(string message) => Regex.Replace(message, @"\d+", "#");
    }
}

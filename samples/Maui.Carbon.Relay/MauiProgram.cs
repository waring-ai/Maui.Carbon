using Maui.Carbon.Relay.Abstractions;
using Maui.Carbon.Relay.Hosting;
using Maui.Carbon.Relay.Pages;
using Maui.Carbon.Relay.Services;
using Maui.Carbon.Relay.ViewModels;
using Maui.Carbon.Relay.Views;
using Microsoft.Extensions.Logging;

namespace Maui.Carbon.Relay;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseCarbon();

        // Serilog log file (see AppLog for where it goes), first so start-up failures are caught.
        AppLog.Configure(builder);

        var services = builder.Services;

        // Mock backend: one in-memory database, services over it. Screenshot and exercise modes turn latency off.
        services.AddSingleton(new MockLatency { Scale = RouteSnapshotRunner.IsRequested || ExerciseRunner.IsRequested ? 0 : 1 });
        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton(sp => MockDataSeed.Create(sp.GetRequiredService<IClock>().Now));
        services.AddSingleton<IPeopleService, InMemoryPeopleService>();
        services.AddSingleton<IProjectService, InMemoryProjectService>();
        services.AddSingleton<IIssueService, InMemoryIssueService>();
        services.AddSingleton<ISprintService, InMemorySprintService>();
        services.AddSingleton<INotificationFeed, InMemoryNotificationFeed>();
        services.AddSingleton<ISettingsService, InMemorySettingsService>();
        services.AddSingleton<IWorkspace, Workspace>();
        services.AddSingleton<IAuthService, MockAuthService>();
        services.AddSingleton<IAssistantService, MockAssistantService>();

        // App services behind Core's abstractions.
        services.AddSingleton(sp => new Navigator(sp, sp.GetRequiredService<ILogger<Navigator>>())
            .Map<DashboardView, DashboardViewModel>(Routes.Dashboard)
            .Map<IssuesView, IssuesViewModel>(Routes.Issues)
            .Map<IssueDetailView, IssueDetailViewModel>(Routes.IssueDetail)
            .Map<CreateIssueView, CreateIssueViewModel>(Routes.CreateIssue)
            .Map<BoardView, BoardViewModel>(Routes.Board)
            .Map<BacklogView, BacklogViewModel>(Routes.Backlog)
            .Map<ProjectsView, ProjectsViewModel>(Routes.Projects)
            .Map<TeamView, TeamViewModel>(Routes.Team)
            .Map<ReportsView, ReportsViewModel>(Routes.Reports)
            .Map<SettingsView, SettingsViewModel>(Routes.Settings));
        services.AddSingleton<INavigator>(sp => sp.GetRequiredService<Navigator>());
        services.AddSingleton<IUserFeedback, UserFeedback>();
        services.AddSingleton<IThemeService, ThemeService>();
        services.AddSingleton<ISessionHost, SessionHost>();
        services.AddSingleton<LiveFeedTimer>();

        // Frame and pages.
        services.AddSingleton<ShellViewModel>();
        services.AddSingleton<AssistantViewModel>();
        services.AddSingleton<LoginViewModel>();
        services.AddSingleton<MainPage>();
        services.AddSingleton<LoginPage>();

        // Top-level screens are singletons, so filters and scroll state survive moving between them.
        AddScreen<DashboardView, DashboardViewModel>(services, singleton: true);
        AddScreen<IssuesView, IssuesViewModel>(services, singleton: true);
        AddScreen<BoardView, BoardViewModel>(services, singleton: true);
        AddScreen<BacklogView, BacklogViewModel>(services, singleton: true);
        AddScreen<ProjectsView, ProjectsViewModel>(services, singleton: true);
        AddScreen<TeamView, TeamViewModel>(services, singleton: true);
        AddScreen<ReportsView, ReportsViewModel>(services, singleton: true);
        AddScreen<SettingsView, SettingsViewModel>(services, singleton: true);
        // Detail screens start fresh every time.
        AddScreen<IssueDetailView, IssueDetailViewModel>(services, singleton: false);
        AddScreen<CreateIssueView, CreateIssueViewModel>(services, singleton: false);

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }

    static void AddScreen<TView, TViewModel>(IServiceCollection services, bool singleton)
        where TView : View
        where TViewModel : class
    {
        if (singleton)
        {
            services.AddSingleton<TView>();
            services.AddSingleton<TViewModel>();
        }
        else
        {
            services.AddTransient<TView>();
            services.AddTransient<TViewModel>();
        }
    }
}

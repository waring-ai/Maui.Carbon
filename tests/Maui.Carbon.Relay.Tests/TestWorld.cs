namespace Maui.Carbon.Relay.Tests;

/// <summary>Records navigation instead of showing screens.</summary>
public sealed class FakeNavigator : INavigator
{
    public List<(string Route, object? Parameter)> History { get; } = [];

    public string? CurrentRoute => History.Count == 0 ? null : History[^1].Route;

    public bool CanGoBack => History.Count > 1;

    public event EventHandler<string>? Navigated;

    public Task NavigateAsync(string route, object? parameter = null)
    {
        History.Add((route, parameter));
        Navigated?.Invoke(this, route);
        return Task.CompletedTask;
    }

    public Task GoBackAsync()
    {
        if (History.Count > 0) History.RemoveAt(History.Count - 1);
        return Task.CompletedTask;
    }
}

/// <summary>Records toasts.</summary>
public sealed class FakeFeedback : IUserFeedback
{
    public List<(FeedbackKind Kind, string Title, string? Subtitle)> Toasts { get; } = [];

    public void Toast(FeedbackKind kind, string title, string? subtitle = null) => Toasts.Add((kind, title, subtitle));
}

public sealed class FakeTheme : IThemeService
{
    public ThemeChoice Current { get; private set; }

    public void Apply(ThemeChoice theme) => Current = theme;
}

public sealed class FakeSession : ISessionHost
{
    public string? Shown { get; private set; }

    public void ShowApp() => Shown = "app";

    public void ShowLogin() => Shown = "login";
}

/// <summary>
/// The whole mock backend with no latency and a fixed clock, plus fakes for the app-side abstractions. Each test gets
/// its own world (the database is mutable).
/// </summary>
public sealed class TestWorld
{
    public static readonly DateTime Now = new(2026, 11, 18, 10, 0, 0);

    public TestWorld()
    {
        Clock = new FixedClock(Now);
        Latency = MockLatency.None;
        Db = MockDataSeed.Create(Now);
        People = new InMemoryPeopleService(Db);
        Projects = new InMemoryProjectService(Db, Latency);
        Issues = new InMemoryIssueService(Db, People, Projects, Clock, Latency);
        Sprints = new InMemorySprintService(Db, Clock, Latency);
        Feed = new InMemoryNotificationFeed(Db, Issues, Clock);
        Settings = new InMemorySettingsService(Latency);
        Workspace = new Workspace(Projects, Settings);
        Auth = new MockAuthService(People, Latency);
        Assistant = new MockAssistantService(Issues, People, Sprints, Projects, Latency);
    }

    public FixedClock Clock { get; }

    public MockLatency Latency { get; }

    public MockDatabase Db { get; }

    public IPeopleService People { get; }

    public IProjectService Projects { get; }

    public IIssueService Issues { get; }

    public ISprintService Sprints { get; }

    public INotificationFeed Feed { get; }

    public ISettingsService Settings { get; }

    public IWorkspace Workspace { get; }

    public IAuthService Auth { get; }

    public IAssistantService Assistant { get; }

    public FakeNavigator Navigator { get; } = new();

    public FakeFeedback Feedback { get; } = new();

    public FakeTheme Theme { get; } = new();

    public FakeSession Session { get; } = new();
}

using Maui.Carbon.Relay.Models;

namespace Maui.Carbon.Relay.Abstractions;

/// <summary>Route names. A detail route takes a parameter (the issue key).</summary>
public static class Routes
{
    public const string Dashboard = "dashboard";
    public const string Issues = "issues";
    public const string IssueDetail = "issue";
    public const string CreateIssue = "issues/new";
    public const string Board = "board";
    public const string Backlog = "backlog";
    public const string Projects = "projects";
    public const string Team = "team";
    public const string Reports = "reports";
    public const string Settings = "settings";

    /// <summary>Every top-level route, in side-nav order.</summary>
    public static IReadOnlyList<string> TopLevel { get; } = [Dashboard, Issues, Board, Backlog, Projects, Team, Reports, Settings];

    /// <summary>Routes pushed on top of another (they get a back stack and a breadcrumb).</summary>
    public static bool IsDetail(string route) => route is IssueDetail or CreateIssue;
}

/// <summary>Moves between screens inside the app frame. Implemented by the app on top of CdsUIShell.</summary>
public interface INavigator
{
    /// <summary>The route on screen.</summary>
    string? CurrentRoute { get; }

    bool CanGoBack { get; }

    /// <summary>Shows <paramref name="route"/>. Top-level routes reset the back stack; detail routes push onto it.</summary>
    Task NavigateAsync(string route, object? parameter = null);

    /// <summary>Returns to the screen under the current detail route.</summary>
    Task GoBackAsync();

    /// <summary>Raised after a screen is shown, with its route.</summary>
    event EventHandler<string>? Navigated;
}

/// <summary>Transient feedback (Carbon toasts in the app).</summary>
public interface IUserFeedback
{
    void Toast(FeedbackKind kind, string title, string? subtitle = null);
}

/// <summary>Applies the app theme (CdsThemeManager in the app).</summary>
public interface IThemeService
{
    ThemeChoice Current { get; }

    void Apply(ThemeChoice theme);
}

/// <summary>Leaves the signed-in app and returns to the login page.</summary>
public interface ISessionHost
{
    void ShowApp();

    void ShowLogin();
}

/// <summary>A screen's view model: told when it is shown (with the route parameter) and when it is left.</summary>
public interface IActivatable
{
    Task ActivateAsync(object? parameter);

    void Deactivate();
}

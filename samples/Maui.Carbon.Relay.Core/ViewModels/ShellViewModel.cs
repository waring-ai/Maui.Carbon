using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Maui.Carbon.Relay.Abstractions;
using Maui.Carbon.Relay.Models;
using Maui.Carbon.Relay.Services;

namespace Maui.Carbon.Relay.ViewModels;

/// <summary>The header panel that is open (one at a time).</summary>
public enum ShellPanel
{
    None,
    Notifications,
    Switcher,
    Account,
    Assistant,
}

/// <summary>
/// The app frame: navigation, the current project, the header panels (notifications, project switcher, account,
/// assistant), the unread count and the theme.
/// </summary>
public sealed partial class ShellViewModel : ObservableObject
{
    readonly INavigator _navigator;
    readonly IWorkspace _workspace;
    readonly INotificationFeed _feed;
    readonly IProjectService _projects;
    readonly IThemeService _theme;
    readonly IAuthService _auth;
    readonly ISessionHost _session;
    readonly IClock _clock;

    public ShellViewModel(
        INavigator navigator,
        IWorkspace workspace,
        INotificationFeed feed,
        IProjectService projects,
        IPeopleService people,
        IThemeService theme,
        IAuthService auth,
        ISessionHost session,
        IClock clock,
        AssistantViewModel assistant)
    {
        _navigator = navigator;
        _workspace = workspace;
        _feed = feed;
        _projects = projects;
        _theme = theme;
        _auth = auth;
        _session = session;
        _clock = clock;
        Assistant = assistant;
        Me = people.Me;
        Theme = theme.Current;
        _navigator.Navigated += (_, route) => CurrentRoute = route;
        _workspace.CurrentProjectChanged += (_, _) => RefreshProjects();
        _projects.Changed += (_, _) => RefreshProjects();
        _feed.Changed += (_, _) => RefreshNotifications();
        RefreshProjects();
        RefreshNotifications();
    }

    public Person Me { get; }

    public AssistantViewModel Assistant { get; }

    [ObservableProperty]
    public partial string? CurrentRoute { get; set; }

    [ObservableProperty]
    public partial Project CurrentProject { get; set; } = null!;

    /// <summary>Active (not archived) projects for the switcher, starred first.</summary>
    [ObservableProperty]
    public partial IReadOnlyList<Project> SwitcherProjects { get; set; } = [];

    public ObservableCollection<NotificationRow> Notifications { get; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasUnread), nameof(UnreadText))]
    public partial int UnreadCount { get; set; }

    public bool HasUnread => UnreadCount > 0;

    public string UnreadText => UnreadCount switch
    {
        0 => "You're all caught up",
        1 => "1 unread notification",
        var n => $"{n} unread notifications",
    };

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsPanelOpen), nameof(IsNotificationsOpen), nameof(IsSwitcherOpen), nameof(IsAccountOpen), nameof(IsAssistantOpen))]
    public partial ShellPanel ActivePanel { get; set; }

    public bool IsNotificationsOpen => ActivePanel == ShellPanel.Notifications;

    public bool IsSwitcherOpen => ActivePanel == ShellPanel.Switcher;

    public bool IsAccountOpen => ActivePanel == ShellPanel.Account;

    public bool IsAssistantOpen => ActivePanel == ShellPanel.Assistant;

    public bool IsPanelOpen
    {
        get => ActivePanel != ShellPanel.None;
        set
        {
            if (!value) ActivePanel = ShellPanel.None;
        }
    }

    [ObservableProperty]
    public partial ThemeChoice Theme { get; set; }

    /// <summary>The passive "About Relay" modal.</summary>
    [ObservableProperty]
    public partial bool IsAboutOpen { get; set; }

    [RelayCommand]
    void ShowAbout()
    {
        ActivePanel = ShellPanel.None;
        IsAboutOpen = true;
    }

    [RelayCommand]
    void CloseAbout() => IsAboutOpen = false;

    partial void OnThemeChanged(ThemeChoice value) => _theme.Apply(value);

    // Concurrent: a slow screen load must not disable the side nav links bound to this command.
    [RelayCommand(AllowConcurrentExecutions = true)]
    Task NavigateAsync(string route)
    {
        ActivePanel = ShellPanel.None;
        return _navigator.NavigateAsync(route);
    }

    [RelayCommand]
    Task CreateIssueAsync()
    {
        ActivePanel = ShellPanel.None;
        return _navigator.NavigateAsync(Routes.CreateIssue);
    }

    [RelayCommand]
    void TogglePanel(ShellPanel panel) => ActivePanel = ActivePanel == panel ? ShellPanel.None : panel;

    [RelayCommand]
    void SelectProject(Project project)
    {
        _workspace.SelectProject(project.Id);
        ActivePanel = ShellPanel.None;
    }

    [RelayCommand]
    void MarkAllRead() => _feed.MarkAllRead();

    [RelayCommand]
    async Task OpenNotificationAsync(NotificationRow row)
    {
        _feed.MarkRead(row.Notification);
        ActivePanel = ShellPanel.None;
        if (row.Notification.IssueKey is { } key) await _navigator.NavigateAsync(Routes.IssueDetail, key);
    }

    [RelayCommand]
    void SignOut()
    {
        ActivePanel = ShellPanel.None;
        _auth.SignOut();
        _session.ShowLogin();
    }

    void RefreshProjects()
    {
        CurrentProject = _workspace.CurrentProject;
        SwitcherProjects = _projects.All.Where(p => !p.IsArchived).OrderByDescending(p => p.IsStarred).ThenBy(p => p.Name).ToList();
    }

    void RefreshNotifications()
    {
        var now = _clock.Now;
        Notifications.Clear();
        foreach (var n in _feed.Items) Notifications.Add(new NotificationRow(n, Formatting.Relative(n.When, now)));
        UnreadCount = _feed.UnreadCount;
    }
}

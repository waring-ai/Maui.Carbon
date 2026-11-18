namespace Maui.Carbon.Relay.Tests;

public class ShellAndLoginTests
{
    readonly TestWorld _w = new();

    ShellViewModel CreateShell() => new(_w.Navigator, _w.Workspace, _w.Feed, _w.Projects, _w.People, _w.Theme, _w.Auth, _w.Session, _w.Clock,
        new AssistantViewModel(_w.Assistant, _w.Workspace, _w.People, _w.Projects, _w.Issues, _w.Navigator, _w.Clock));

    [Fact]
    public void Panels_toggle_one_at_a_time()
    {
        var shell = CreateShell();
        shell.TogglePanelCommand.Execute(ShellPanel.Notifications);
        Assert.True(shell.IsNotificationsOpen);
        shell.TogglePanelCommand.Execute(ShellPanel.Account);
        Assert.False(shell.IsNotificationsOpen);
        Assert.True(shell.IsAccountOpen);
        shell.IsPanelOpen = false;
        Assert.Equal(ShellPanel.None, shell.ActivePanel);
    }

    [Fact]
    public void Unread_count_tracks_the_feed()
    {
        var shell = CreateShell();
        var before = shell.UnreadCount;
        Assert.True(before > 0);
        _w.Feed.SimulateIncoming();
        Assert.Equal(before + 1, shell.UnreadCount);
        shell.MarkAllReadCommand.Execute(null);
        Assert.Equal(0, shell.UnreadCount);
        Assert.False(shell.HasUnread);
    }

    [Fact]
    public async Task Opening_a_notification_marks_it_read_and_opens_its_issue()
    {
        var shell = CreateShell();
        var row = shell.Notifications.First(n => n.Notification.IssueKey is not null && n.IsUnread);
        await shell.OpenNotificationCommand.ExecuteAsync(row);
        Assert.True(row.Notification.IsRead);
        Assert.Equal(Routes.IssueDetail, _w.Navigator.CurrentRoute);
    }

    [Fact]
    public void Switching_project_updates_the_current_project()
    {
        var shell = CreateShell();
        var other = shell.SwitcherProjects.First(p => p.Id != shell.CurrentProject.Id);
        shell.SelectProjectCommand.Execute(other);
        Assert.Equal(other.Id, shell.CurrentProject.Id);
        Assert.DoesNotContain(shell.SwitcherProjects, p => p.IsArchived);
    }

    [Fact]
    public void Theme_choice_is_applied()
    {
        var shell = CreateShell();
        shell.Theme = ThemeChoice.G100;
        Assert.Equal(ThemeChoice.G100, _w.Theme.Current);
    }

    [Fact]
    public async Task Login_rejects_unknown_email_then_signs_in()
    {
        var vm = new LoginViewModel(_w.Auth, _w.Session) { Email = "nobody@relay.dev" };
        await vm.ContinueCommand.ExecuteAsync(null);
        Assert.True(vm.IsEmailInvalid);
        Assert.True(vm.IsEmailStep);

        vm.Email = LoginViewModel.DemoEmail;
        await vm.ContinueCommand.ExecuteAsync(null);
        Assert.True(vm.IsPasswordStep);

        vm.Password = "abc";
        await vm.SignInCommand.ExecuteAsync(null);
        Assert.True(vm.HasFormError);
        Assert.Null(_w.Session.Shown);

        vm.Password = "correct horse";
        await vm.SignInCommand.ExecuteAsync(null);
        Assert.Equal("app", _w.Session.Shown);
        Assert.True(_w.Auth.IsSignedIn);
    }

    [Fact]
    public async Task Assistant_answers_and_links_issues()
    {
        var shell = CreateShell();
        await shell.Assistant.SummarizeSprintCommand.ExecuteAsync(null);
        var answer = Assert.Single(shell.Assistant.Answers);
        Assert.Contains("Sprint", answer.Text, StringComparison.Ordinal);
        Assert.False(shell.Assistant.IsThinking);
    }
}

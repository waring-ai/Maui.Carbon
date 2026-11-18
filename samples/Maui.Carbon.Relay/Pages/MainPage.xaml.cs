using System.ComponentModel;
using Maui.Carbon.Relay.Abstractions;
using Maui.Carbon.Relay.Hosting;
using Maui.Carbon.Relay.ViewModels;

namespace Maui.Carbon.Relay.Pages;

/// <summary>Hosts <see cref="CdsUIShell"/>, gives its body to the navigator and marks the current nav items.</summary>
public partial class MainPage : ContentPage
{
    readonly ShellViewModel _vm;
    readonly Dictionary<string, CdsShellItem[]> _activeItems;

    public MainPage(ShellViewModel vm, Navigator navigator)
    {
        InitializeComponent();
        // Explicit fill: page captures (snapshot mode) leave out the page's own background.
        CdsTokens.SetColor(Shell, VisualElement.BackgroundColorProperty, CdsColorKeys.Background);
        _vm = vm;
        BindingContext = vm;
        _activeItems = new()
        {
            [Routes.Dashboard] = [NavDashboard],
            [Routes.Issues] = [NavIssues, HeaderIssues],
            [Routes.IssueDetail] = [NavIssues, HeaderIssues],
            [Routes.CreateIssue] = [NavIssues, HeaderIssues],
            [Routes.Board] = [NavBoard],
            [Routes.Backlog] = [NavBacklog],
            [Routes.Projects] = [NavProjects, HeaderProjects],
            [Routes.Team] = [NavTeam, HeaderTeam],
            [Routes.Reports] = [NavReports, HeaderReports],
            [Routes.Settings] = [NavSettings, HeaderSettings],
        };
        navigator.Navigated += (_, route) => MarkActive(route);
        navigator.Attach(Shell);
        if (navigator.CurrentRoute is { } current) MarkActive(current);
        vm.PropertyChanged += OnViewModelChanged;
        BuildSwitcher();
        UpdatePanelLabel();
    }

    void MarkActive(string route)
    {
        var active = _activeItems.TryGetValue(route, out var items) ? items : [];
        foreach (var item in _activeItems.Values.SelectMany(i => i).Distinct()) item.IsActive = active.Contains(item);
        HeaderWorkspace.IsActive = active.Any(a => a == HeaderProjects || a == HeaderTeam || a == HeaderSettings);
    }

    void OnViewModelChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(ShellViewModel.SwitcherProjects):
            case nameof(ShellViewModel.CurrentProject):
                BuildSwitcher();
                break;
            case nameof(ShellViewModel.ActivePanel):
                UpdatePanelLabel();
                break;
        }
    }

    /// <summary>The switcher lists projects, starred first, with the current one active.</summary>
    void BuildSwitcher()
    {
        ProjectSwitcher.Items.Clear();
        var starred = true;
        foreach (var project in _vm.SwitcherProjects)
        {
            if (starred && !project.IsStarred && ProjectSwitcher.Items.Count > 0) ProjectSwitcher.Items.Add(new CdsSwitcherDivider());
            starred = project.IsStarred;
            ProjectSwitcher.Items.Add(new CdsSwitcherItem
            {
                Text = project.Name,
                IsActive = project.Id == _vm.CurrentProject.Id,
                Command = _vm.SelectProjectCommand,
                CommandParameter = project,
            });
        }
        ProjectSwitcher.Items.Add(new CdsSwitcherDivider());
        ProjectSwitcher.Items.Add(new CdsSwitcherItem { Text = "All projects", Command = _vm.NavigateCommand, CommandParameter = Routes.Projects });
    }

    void UpdatePanelLabel()
    {
        Panel.Label = _vm.ActivePanel switch
        {
            ShellPanel.Notifications => "Notifications",
            ShellPanel.Switcher => "Switch project",
            ShellPanel.Account => "Account",
            ShellPanel.Assistant => "Assistant",
            _ => "Panel",
        };
        Panel.ReturnFocusTo = _vm.ActivePanel switch
        {
            ShellPanel.Notifications => NotificationsAction,
            ShellPanel.Switcher => SwitcherAction,
            ShellPanel.Account => AccountAction,
            ShellPanel.Assistant => AssistantAction,
            _ => Panel.ReturnFocusTo,
        };
    }
}

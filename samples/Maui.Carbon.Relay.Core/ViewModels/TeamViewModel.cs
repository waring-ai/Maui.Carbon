using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Maui.Carbon.Relay.Abstractions;
using Maui.Carbon.Relay.Models;
using Maui.Carbon.Relay.Services;

namespace Maui.Carbon.Relay.ViewModels;

/// <summary>
/// The team screen: a searchable table of workspace members with one selected at a time, and a detail panel for that
/// person (contact, skills and the open issues assigned to them).
/// </summary>
public sealed partial class TeamViewModel : PageViewModel
{
    readonly IPeopleService _people;
    readonly IIssueService _issues;
    readonly INavigator _navigator;
    bool _loaded;

    public TeamViewModel(IPeopleService people, IIssueService issues, INavigator navigator)
    {
        _people = people;
        _issues = issues;
        _navigator = navigator;
        Title = "Team";
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CountText), nameof(IsEmpty))]
    public partial IReadOnlyList<TeamMemberRow> Rows { get; set; } = [];

    public string CountText => Rows.Count == 1 ? "1 person" : $"{Rows.Count} people";

    public bool IsEmpty => _loaded && Rows.Count == 0;

    [ObservableProperty]
    public partial string SearchText { get; set; } = "";

    /// <summary>The person the detail panel shows (the table's radio selection).</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelection))]
    public partial TeamMemberRow? Selected { get; set; }

    public bool HasSelection => Selected is not null;

    /// <summary>The selected person's open issues, most urgent first (at most <see cref="MaxIssueLinks"/>).</summary>
    [ObservableProperty]
    public partial IReadOnlyList<TeamIssueLink> SelectedIssues { get; set; } = [];

    /// <summary>How many open issues the selected person has in all.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasOpenIssues), nameof(OpenIssuesHeading), nameof(HasMoreIssues), nameof(MoreIssuesText))]
    public partial int SelectedIssueTotal { get; set; }

    public const int MaxIssueLinks = 6;

    public bool HasOpenIssues => SelectedIssueTotal > 0;

    public string OpenIssuesHeading => SelectedIssueTotal == 0 ? "Open issues" : $"Open issues ({SelectedIssueTotal})";

    public bool HasMoreIssues => SelectedIssueTotal > SelectedIssues.Count;

    public string MoreIssuesText => $"and {SelectedIssueTotal - SelectedIssues.Count} more";

    protected override async Task OnActivatedAsync(object? parameter, CancellationToken ct)
    {
        _issues.Changed += OnIssuesChanged;
        if (!_loaded)
        {
            IsLoading = true;
            await _issues.ListAsync(ct);
            _loaded = true;
            IsLoading = false;
        }
        if (parameter is string personId && _people.Find(personId) is not null) Refresh(personId);
        else Refresh();
    }

    protected override void OnDeactivated() => _issues.Changed -= OnIssuesChanged;

    void OnIssuesChanged(object? sender, IssuesChangedEventArgs e) => Refresh();

    partial void OnSearchTextChanged(string value) => Refresh();

    partial void OnSelectedChanged(TeamMemberRow? value) => LoadIssues(value);

    /// <summary>Selects the person with <paramref name="personId"/> (from the table's radio selection).</summary>
    public void Select(string? personId) => Selected = personId is null ? null : Rows.FirstOrDefault(r => r.Id == personId) ?? Selected;

    [RelayCommand]
    Task OpenIssueAsync(TeamIssueLink link) => _navigator.NavigateAsync(Routes.IssueDetail, link.Key);

    [RelayCommand]
    void ClearSearch() => SearchText = "";

    /// <summary>True when the person matches the search: name, title, location, role or a skill.</summary>
    public static bool Matches(Person person, string text)
    {
        text = text.Trim();
        return text.Length == 0
            || person.Name.Contains(text, StringComparison.OrdinalIgnoreCase)
            || person.Title.Contains(text, StringComparison.OrdinalIgnoreCase)
            || person.Location.Contains(text, StringComparison.OrdinalIgnoreCase)
            || person.RoleText.Contains(text, StringComparison.OrdinalIgnoreCase)
            || person.Skills.Any(s => s.Contains(text, StringComparison.OrdinalIgnoreCase));
    }

    void Refresh(string? selectId = null)
    {
        if (!_loaded) return;
        var open = _issues.All.Where(i => i.IsOpen && i.AssigneeId is not null)
            .GroupBy(i => i.AssigneeId!)
            .ToDictionary(g => g.Key, g => g.Count());
        Rows = _people.All
            .Where(p => Matches(p, SearchText))
            .OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase)
            .Select(p => new TeamMemberRow(p, open.GetValueOrDefault(p.Id)))
            .ToList();
        // Keep the selected person while they are visible; otherwise fall back to the signed-in user, then the first row.
        var id = selectId ?? Selected?.Id ?? _people.Me.Id;
        var next = Rows.FirstOrDefault(r => r.Id == id) ?? Rows.FirstOrDefault(r => r.Id == _people.Me.Id) ?? Rows.FirstOrDefault();
        if (Equals(next, Selected)) LoadIssues(next);
        else Selected = next;
    }

    void LoadIssues(TeamMemberRow? row)
    {
        var all = row is null
            ? []
            : _issues.All
                .Where(i => i.IsOpen && i.AssigneeId == row.Id)
                .OrderByDescending(i => i.Priority)
                .ThenByDescending(i => i.Updated)
                .Select(i => new TeamIssueLink(i.Key, i.Title, i.Status, i.Priority))
                .ToList();
        SelectedIssues = all.Take(MaxIssueLinks).ToList();
        SelectedIssueTotal = all.Count;
        OnPropertyChanged(nameof(HasMoreIssues));
        OnPropertyChanged(nameof(MoreIssuesText));
    }
}

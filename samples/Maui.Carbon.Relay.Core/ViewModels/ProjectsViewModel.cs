using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Maui.Carbon.Relay.Abstractions;
using Maui.Carbon.Relay.Logic;
using Maui.Carbon.Relay.Models;
using Maui.Carbon.Relay.Services;

namespace Maui.Carbon.Relay.ViewModels;

/// <summary>
/// The projects screen: search and category chips over a grid of project cards (open one to scope the workspace to
/// it, or star it), and a "New project" modal whose key is suggested from the name and checked as you type.
/// </summary>
public sealed partial class ProjectsViewModel : PageViewModel
{
    /// <summary>Repositories the mock workspace can link (the repository search looks through these).</summary>
    public static IReadOnlyList<string> Repositories { get; } =
    [
        "relay/atlas-ios", "relay/atlas-android", "relay/atlas-sync-service", "relay/billing-api", "relay/ledger",
        "relay/tax-engine", "relay/harbor-tokens", "relay/harbor-components", "relay/insights-pipelines",
        "relay/portal-web", "relay/webhooks-gateway", "relay/relay-cli",
    ];

    readonly IProjectService _projects;
    readonly IIssueService _issues;
    readonly IPeopleService _people;
    readonly IWorkspace _workspace;
    readonly INavigator _navigator;
    readonly IUserFeedback _feedback;
    readonly IClock _clock;
    readonly HashSet<ProjectCategory> _categories = [];
    readonly HashSet<string> _memberIds = [];
    bool _loaded;
    bool _suggestingKey;

    public ProjectsViewModel(IProjectService projects, IIssueService issues, IPeopleService people, IWorkspace workspace, INavigator navigator, IUserFeedback feedback, IClock clock)
    {
        _projects = projects;
        _issues = issues;
        _people = people;
        _workspace = workspace;
        _navigator = navigator;
        _feedback = feedback;
        _clock = clock;
        Title = "Projects";
        CategoryOptions = AllCategories.Select(c => new CategoryOption(c, c.Text())).ToList();
        NewCategory = CategoryOptions[0];
        NewLead = people.Me;
    }

    // ---- the grid ----

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEmpty), nameof(CountText))]
    public partial IReadOnlyList<ProjectCard> Cards { get; set; } = [];

    /// <summary>Placeholders shown while the first load runs.</summary>
    public IReadOnlyList<int> SkeletonCards { get; } = [1, 2, 3, 4];

    public bool IsEmpty => !IsLoading && _loaded && Cards.Count == 0;

    public string CountText => Cards.Count == 1 ? "1 project" : $"{Cards.Count} projects";

    // ---- filters ----

    public IReadOnlyList<ProjectCategory> AllCategories { get; } = Enum.GetValues<ProjectCategory>();

    public IReadOnlySet<ProjectCategory> SelectedCategories => _categories;

    [ObservableProperty]
    public partial string SearchText { get; set; } = "";

    [ObservableProperty]
    public partial bool StarredOnly { get; set; }

    [ObservableProperty]
    public partial bool ShowArchived { get; set; }

    [ObservableProperty]
    public partial bool HasFilters { get; set; }

    /// <summary>Raised when the filters are cleared from code, so the view can reset the category chips it builds.</summary>
    public event EventHandler? FiltersReset;

    // ---- new project ----

    [ObservableProperty]
    public partial bool IsCreateOpen { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CreateDisabled), nameof(CreateButtonText))]
    public partial bool IsCreating { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NameError), nameof(IsNameInvalid), nameof(CreateDisabled))]
    public partial string NewName { get; set; } = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(KeyError), nameof(IsKeyInvalid), nameof(CreateDisabled))]
    public partial string NewKey { get; set; } = "";

    [ObservableProperty]
    public partial string NewDescription { get; set; } = "";

    public IReadOnlyList<CategoryOption> CategoryOptions { get; }

    [ObservableProperty]
    public partial CategoryOption NewCategory { get; set; }

    public IReadOnlyList<Person> People => _people.All;

    [ObservableProperty]
    public partial Person? NewLead { get; set; }

    public IReadOnlyList<string> Templates { get; } = ["Scrum", "Kanban", "Bug tracking", "Design review", "Blank project"];

    [ObservableProperty]
    public partial string? NewTemplate { get; set; } = "Scrum";

    public IReadOnlySet<string> MemberIds => _memberIds;

    [ObservableProperty]
    public partial double? NewWipLimit { get; set; } = 5;

    [ObservableProperty]
    public partial DateTime? NewTargetDate { get; set; }

    [ObservableProperty]
    public partial TimeSpan? NewStandupTime { get; set; } = new TimeSpan(9, 30, 0);

    [ObservableProperty]
    public partial string RepositoryQuery { get; set; } = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasRepositoryMatches), nameof(RepositoryHint))]
    public partial IReadOnlyList<string> RepositoryMatches { get; set; } = [];

    [ObservableProperty]
    public partial string? NewRepository { get; set; }

    public bool HasRepositoryMatches => RepositoryMatches.Count > 0;

    public string RepositoryHint => RepositoryQuery.Trim().Length == 0
        ? $"Search {Repositories.Count} repositories to link one (optional)."
        : HasRepositoryMatches ? "Choose the repository to link." : $"No repository matches \"{RepositoryQuery.Trim()}\".";

    /// <summary>The earliest target date the date picker offers.</summary>
    public DateTime MinTargetDate => _clock.Now.Date;

    /// <summary>Validation shows once the field has been edited or Create was pressed.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNameInvalid), nameof(IsKeyInvalid))]
    public partial bool ShowValidation { get; set; }

    bool _keyEdited;
    bool _nameEdited;

    public string? NameError => NewName.Trim().Length == 0 ? "Enter a project name." : null;

    public bool IsNameInvalid => (ShowValidation || _nameEdited) && NameError is not null;

    public string? KeyError
    {
        get
        {
            var key = NewKey.Trim();
            if (key.Length == 0) return "Enter a key of 2 to 5 capital letters.";
            if (!ProjectsQuery.IsWellFormedKey(key)) return "Use 2 to 5 capital letters, A to Z.";
            if (!_projects.IsKeyAvailable(key))
            {
                var owner = _projects.All.FirstOrDefault(p => p.Key == key);
                return owner is null ? $"{key} is not available." : $"{key} is already used by {owner.Name}.";
            }
            return null;
        }
    }

    public bool IsKeyInvalid => (ShowValidation || _keyEdited) && KeyError is not null;

    public bool CreateDisabled => IsCreating || NameError is not null || KeyError is not null;

    public string CreateButtonText => IsCreating ? "Creating..." : "Create project";

    // ---- lifecycle ----

    protected override async Task OnActivatedAsync(object? parameter, CancellationToken ct)
    {
        _projects.Changed += OnDataChanged;
        _issues.Changed += OnIssuesChanged;
        if (!_loaded)
        {
            IsLoading = true;
            OnPropertyChanged(nameof(IsEmpty));
            await _projects.ListAsync(ct);
            await _issues.ListAsync(ct);
            _loaded = true;
            IsLoading = false;
        }
        Refresh();
    }

    protected override void OnDeactivated()
    {
        _projects.Changed -= OnDataChanged;
        _issues.Changed -= OnIssuesChanged;
        IsCreateOpen = false;
    }

    void OnDataChanged(object? sender, EventArgs e) => Refresh();

    void OnIssuesChanged(object? sender, IssuesChangedEventArgs e) => Refresh();

    partial void OnSearchTextChanged(string value) => Refresh();

    partial void OnStarredOnlyChanged(bool value) => Refresh();

    partial void OnShowArchivedChanged(bool value) => Refresh();

    partial void OnNewNameChanged(string value)
    {
        if (value.Length > 0) _nameEdited = true;
        if (_keyEdited) return;
        _suggestingKey = true;
        try
        {
            NewKey = ProjectsQuery.SuggestKey(value, _projects.IsKeyAvailable);
        }
        finally
        {
            _suggestingKey = false;
        }
    }

    partial void OnNewKeyChanged(string value)
    {
        if (_suggestingKey) return;
        // Typing a key stops the suggestion; clearing it hands the field back to the suggestion.
        _keyEdited = value.Length > 0;
        if (value != value.ToUpperInvariant())
        {
            NewKey = value.ToUpperInvariant();
            return;
        }
        OnPropertyChanged(nameof(IsKeyInvalid));
    }

    partial void OnRepositoryQueryChanged(string value)
    {
        var text = value.Trim();
        RepositoryMatches = text.Length == 0
            ? []
            : Repositories.Where(r => r.Contains(text, StringComparison.OrdinalIgnoreCase)).Take(4).ToList();
        if (NewRepository is not null && !RepositoryMatches.Contains(NewRepository)) NewRepository = null;
        OnPropertyChanged(nameof(RepositoryHint));
    }

    // ---- view entry points ----

    public void ToggleCategory(ProjectCategory category, bool on)
    {
        if (on ? _categories.Add(category) : _categories.Remove(category)) Refresh();
    }

    public void SetMembers(IEnumerable<Person> members)
    {
        _memberIds.Clear();
        _memberIds.UnionWith(members.Select(m => m.Id));
    }

    // ---- commands ----

    [RelayCommand]
    void ClearFilters()
    {
        _categories.Clear();
        SearchText = "";
        StarredOnly = false;
        ShowArchived = false;
        FiltersReset?.Invoke(this, EventArgs.Empty);
        Refresh();
    }

    [RelayCommand]
    Task OpenProjectAsync(ProjectCard card)
    {
        _workspace.SelectProject(card.Id);
        return _navigator.NavigateAsync(Routes.Dashboard);
    }

    [RelayCommand]
    void ToggleStar(ProjectCard card)
    {
        _projects.ToggleStar(card.Project);
        _feedback.Toast(FeedbackKind.Success, card.Project.IsStarred ? $"{card.Name} starred" : $"{card.Name} removed from starred");
    }

    [RelayCommand]
    void NewProject()
    {
        ResetDraft();
        IsCreateOpen = true;
    }

    [RelayCommand]
    void CancelCreate() => IsCreateOpen = false;

    [RelayCommand]
    async Task CreateProjectAsync()
    {
        ShowValidation = true;
        if (CreateDisabled) return;
        var key = NewKey.Trim();
        var project = new Project
        {
            Id = key.ToLowerInvariant(),
            Key = key,
            Name = NewName.Trim(),
            Description = NewDescription.Trim(),
            LeadId = NewLead?.Id ?? _people.Me.Id,
            Category = NewCategory.Value,
            Created = _clock.Now,
            TargetDate = NewTargetDate,
        };
        IsCreating = true;
        try
        {
            await _projects.CreateAsync(project);
        }
        catch (InvalidOperationException ex)
        {
            _feedback.Toast(FeedbackKind.Error, "Project not created", ex.Message);
            return;
        }
        finally
        {
            IsCreating = false;
        }
        IsCreateOpen = false;
        _feedback.Toast(FeedbackKind.Success, $"{project.Name} created", CreatedSummary(project));
        Refresh();
    }

    string CreatedSummary(Project project)
    {
        var parts = new List<string> { $"Key {project.Key}", $"lead {_people.NameOf(project.LeadId)}" };
        if (!string.IsNullOrWhiteSpace(NewTemplate)) parts.Add($"{NewTemplate} template");
        if (_memberIds.Count > 0) parts.Add(_memberIds.Count == 1 ? "1 member" : $"{_memberIds.Count} members");
        if (NewStandupTime is { } t) parts.Add($"stand-up {DateTime.Today.Add(t).ToString("h:mm tt", CultureInfo.InvariantCulture)}");
        if (NewRepository is not null) parts.Add($"linked to {NewRepository}");
        return string.Join(" · ", parts);
    }

    void ResetDraft()
    {
        ShowValidation = false;
        _keyEdited = false;
        NewName = "";
        _nameEdited = false;
        NewKey = "";
        NewDescription = "";
        NewCategory = CategoryOptions[0];
        NewLead = _people.Me;
        NewTemplate = "Scrum";
        _memberIds.Clear();
        NewWipLimit = 5;
        NewTargetDate = null;
        NewStandupTime = new TimeSpan(9, 30, 0);
        RepositoryQuery = "";
        NewRepository = null;
        DraftReset?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Raised when the new project form is reset, so the view can clear the member multi-select.</summary>
    public event EventHandler? DraftReset;

    // ---- query ----

    ProjectFilter CurrentFilter() => new()
    {
        SearchText = SearchText,
        Categories = _categories,
        StarredOnly = StarredOnly,
        ShowArchived = ShowArchived,
    };

    void Refresh()
    {
        HasFilters = SearchText.Trim().Length > 0 || _categories.Count > 0 || StarredOnly || ShowArchived;
        if (!_loaded) return;
        var open = _issues.All.Where(i => i.IsOpen).GroupBy(i => i.ProjectId).ToDictionary(g => g.Key, g => g.Count());
        Cards = ProjectsQuery.Filter(_projects.All, CurrentFilter())
            .Select(p =>
            {
                var lead = _people.Find(p.LeadId);
                return new ProjectCard(p, lead?.Name ?? "No lead", lead?.Initials ?? "–", open.GetValueOrDefault(p.Id));
            })
            .ToList();
    }
}

using System.Globalization;
using System.Text;
using System.Text.Json;
using Maui.Carbon.Relay.Logic;
using Maui.Carbon.Relay.Models;

namespace Maui.Carbon.Relay.Services;

public sealed class InMemoryPeopleService(MockDatabase db) : IPeopleService
{
    public IReadOnlyList<Person> All => db.People;

    public Person Me => db.People.First(p => p.Id == MockDataSeed.MeId);

    public Person? Find(string? id) => id is null ? null : db.People.FirstOrDefault(p => p.Id == id);

    public string NameOf(string? id) => Find(id)?.Name ?? "Unassigned";
}

public sealed class InMemoryIssueService(MockDatabase db, IPeopleService people, IProjectService projects, IClock clock, MockLatency latency) : IIssueService
{
    int _activity;

    public event EventHandler<IssuesChangedEventArgs>? Changed;

    public IReadOnlyList<Issue> All => db.Issues;

    public IReadOnlyList<string> Labels => db.Issues.SelectMany(i => i.Labels).Distinct().Order(StringComparer.Ordinal).ToList();

    public Issue? Find(string key) => db.Issues.FirstOrDefault(i => string.Equals(i.Key, key, StringComparison.OrdinalIgnoreCase));

    public async Task<IReadOnlyList<Issue>> ListAsync(CancellationToken ct = default)
    {
        await latency.WaitAsync(700, ct);
        return db.Issues.ToList();
    }

    public async Task<Issue?> GetAsync(string key, CancellationToken ct = default)
    {
        await latency.WaitAsync(400, ct);
        return Find(key);
    }

    public async Task<Issue> CreateAsync(IssueDraft draft, CancellationToken ct = default)
    {
        await latency.WaitAsync(900, ct);
        var project = projects.Find(draft.ProjectId) ?? throw new InvalidOperationException("Choose a project.");
        var now = clock.Now;
        var key = $"{project.Key}-{db.NextIssueNumber(project.Key)}";
        var issue = new Issue
        {
            Id = db.Issues.Count == 0 ? 1 : db.Issues.Max(i => i.Id) + 1,
            Key = key,
            ProjectId = project.Id,
            Title = draft.Title.Trim(),
            Description = draft.Description.Trim(),
            Type = draft.Type ?? IssueType.Task,
            Status = IssueStatus.Todo,
            Priority = draft.Priority,
            AssigneeId = draft.AssigneeId,
            ReporterId = people.Me.Id,
            Component = draft.Component,
            Labels = draft.Labels.ToList(),
            StoryPoints = draft.Type == IssueType.Epic ? null : draft.StoryPoints,
            StartDate = draft.StartDate,
            DueDate = draft.DueDate,
            ReminderTime = draft.ReminderTime,
            Created = now,
            Updated = now,
        };
        issue.WatcherIds.Add(people.Me.Id);
        if (!draft.Watch) issue.WatcherIds.Remove(people.Me.Id);
        if (draft.AssigneeId is not null) issue.WatcherIds.Add(draft.AssigneeId);
        Record(issue, ActivityKind.Created, "created the issue");
        db.Issues.Add(issue);
        Raise([issue], "created");
        return issue;
    }

    public async Task SetStatusAsync(IEnumerable<Issue> issues, IssueStatus status, CancellationToken ct = default)
    {
        var list = issues.ToList();
        await latency.WaitAsync(500, ct);
        foreach (var issue in list.Where(i => i.Status != status))
        {
            issue.Status = status;
            issue.Resolution = status switch
            {
                IssueStatus.Done => IssueResolution.Fixed,
                IssueStatus.Cancelled => issue.Resolution == IssueResolution.None ? IssueResolution.WontFix : issue.Resolution,
                _ => IssueResolution.None,
            };
            Touch(issue, ActivityKind.StatusChanged, $"moved it to {status.Text()}");
        }
        Raise(list, "status");
    }

    public async Task AssignAsync(IEnumerable<Issue> issues, string? personId, CancellationToken ct = default)
    {
        var list = issues.ToList();
        await latency.WaitAsync(400, ct);
        foreach (var issue in list)
        {
            issue.AssigneeId = personId;
            if (personId is not null) issue.WatcherIds.Add(personId);
            Touch(issue, ActivityKind.Assigned, personId is null ? "removed the assignee" : $"assigned it to {people.NameOf(personId)}");
        }
        Raise(list, "assignee");
    }

    public async Task ResolveAsync(Issue issue, IssueResolution resolution, CancellationToken ct = default)
    {
        await latency.WaitAsync(500, ct);
        issue.Resolution = resolution;
        issue.Status = resolution == IssueResolution.Fixed ? IssueStatus.Done : IssueStatus.Cancelled;
        Touch(issue, ActivityKind.Resolved, $"resolved it as {resolution.Text()}");
        Raise([issue], "resolved");
    }

    public async Task UpdateAsync(Issue issue, Action<Issue> edit, string summary, CancellationToken ct = default)
    {
        await latency.WaitAsync(300, ct);
        edit(issue);
        Touch(issue, ActivityKind.Edited, summary);
        Raise([issue], "edited");
    }

    public async Task<Comment> AddCommentAsync(Issue issue, string body, CancellationToken ct = default)
    {
        await latency.WaitAsync(500, ct);
        var comment = new Comment($"c{issue.Id}-{issue.Comments.Count}-{++_activity}", people.Me.Id, body.Trim(), clock.Now);
        issue.Comments.Add(comment);
        Touch(issue, ActivityKind.Commented, "commented");
        Raise([issue], "commented");
        return comment;
    }

    public async Task AddAttachmentAsync(Issue issue, string fileName, long sizeBytes, CancellationToken ct = default)
    {
        await latency.WaitAsync(800, ct);
        issue.Attachments.Add(new Attachment(fileName, sizeBytes, people.Me.Id, clock.Now));
        Touch(issue, ActivityKind.Attached, $"attached {fileName}");
        Raise([issue], "attached");
    }

    public async Task RemoveAttachmentAsync(Issue issue, string fileName, CancellationToken ct = default)
    {
        await latency.WaitAsync(300, ct);
        issue.Attachments.RemoveAll(a => a.Name == fileName);
        Touch(issue, ActivityKind.Edited, $"removed {fileName}");
        Raise([issue], "attached");
    }

    public Task ToggleWatchAsync(Issue issue, CancellationToken ct = default)
    {
        if (!issue.WatcherIds.Remove(people.Me.Id)) issue.WatcherIds.Add(people.Me.Id);
        Raise([issue], "watch");
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(IEnumerable<Issue> issues, CancellationToken ct = default)
    {
        var list = issues.ToList();
        await latency.WaitAsync(700, ct);
        foreach (var issue in list) db.Issues.Remove(issue);
        Raise(list, "deleted");
    }

    public async Task RestoreAsync(IEnumerable<Issue> issues, CancellationToken ct = default)
    {
        var list = issues.ToList();
        await latency.WaitAsync(300, ct);
        foreach (var issue in list.Where(i => !db.Issues.Contains(i))) db.Issues.Add(issue);
        Raise(list, "restored");
    }

    public async Task<string> ExportAsync(IEnumerable<Issue> issues, string format, CancellationToken ct = default)
    {
        var list = issues.ToList();
        await latency.WaitAsync(600, ct);
        if (string.Equals(format, "json", StringComparison.OrdinalIgnoreCase))
        {
            return JsonSerializer.Serialize(list.Select(i => new
            {
                i.Key, i.Title, Type = i.Type.Text(), Status = i.Status.Text(), Priority = i.Priority.Text(),
                Assignee = people.NameOf(i.AssigneeId), i.StoryPoints, Due = i.DueDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            }), new JsonSerializerOptions { WriteIndented = true });
        }
        var csv = new StringBuilder("Key,Title,Type,Status,Priority,Assignee,Points,Due\n");
        foreach (var i in list)
        {
            csv.Append(CultureInfo.InvariantCulture,
                $"{i.Key},\"{i.Title.Replace("\"", "\"\"")}\",{i.Type.Text()},{i.Status.Text()},{i.Priority.Text()},{people.NameOf(i.AssigneeId)},{i.StoryPoints},{i.DueDate:yyyy-MM-dd}\n");
        }
        return csv.ToString();
    }

    void Touch(Issue issue, ActivityKind kind, string text)
    {
        issue.Updated = clock.Now;
        Record(issue, kind, text);
    }

    void Record(Issue issue, ActivityKind kind, string text) =>
        issue.Activity.Add(new ActivityEvent($"a{issue.Id}-x{++_activity}", issue.Key, people.Me.Id, kind, text, clock.Now));

    void Raise(IReadOnlyList<Issue> issues, string reason) =>
        Changed?.Invoke(this, new IssuesChangedEventArgs(issues.Select(i => i.Key).ToList(), reason));
}

public sealed class InMemoryProjectService(MockDatabase db, MockLatency latency) : IProjectService
{
    public event EventHandler? Changed;

    public IReadOnlyList<Project> All => db.Projects;

    public Project? Find(string? id) => id is null ? null : db.Projects.FirstOrDefault(p => p.Id == id);

    public async Task<IReadOnlyList<Project>> ListAsync(CancellationToken ct = default)
    {
        await latency.WaitAsync(600, ct);
        return db.Projects.ToList();
    }

    public async Task<Project> CreateAsync(Project project, CancellationToken ct = default)
    {
        await latency.WaitAsync(900, ct);
        if (!IsKeyAvailable(project.Key)) throw new InvalidOperationException($"The key {project.Key} is already used.");
        db.Projects.Add(project);
        Changed?.Invoke(this, EventArgs.Empty);
        return project;
    }

    public void ToggleStar(Project project)
    {
        project.IsStarred = !project.IsStarred;
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public bool IsKeyAvailable(string key) =>
        key.Length is >= 2 and <= 5 && key.All(char.IsAsciiLetterUpper) && db.Projects.All(p => p.Key != key);
}

public sealed class InMemorySprintService(MockDatabase db, IClock clock, MockLatency latency) : ISprintService
{
    public event EventHandler? Changed;

    public IReadOnlyList<Sprint> ForProject(string projectId) =>
        db.Sprints.Where(s => s.ProjectId == projectId).OrderByDescending(s => s.Start).ToList();

    public Sprint? Active(string projectId) => db.Sprints.FirstOrDefault(s => s.ProjectId == projectId && s.State == SprintState.Active);

    public Sprint? Find(string? id) => id is null ? null : db.Sprints.FirstOrDefault(s => s.Id == id);

    public async Task StartAsync(Sprint sprint, CancellationToken ct = default)
    {
        await latency.WaitAsync(1200, ct);
        if (Active(sprint.ProjectId) is { } running) throw new InvalidOperationException($"Complete {running.Name} first.");
        sprint.State = SprintState.Active;
        sprint.Start = clock.Now.Date;
        sprint.End = sprint.Start.AddDays(14);
        sprint.CommittedPoints = db.Issues.Where(i => i.SprintId == sprint.Id).Sum(i => i.StoryPoints ?? 0);
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public async Task CompleteAsync(Sprint sprint, CancellationToken ct = default)
    {
        await latency.WaitAsync(1200, ct);
        var issues = db.Issues.Where(i => i.SprintId == sprint.Id).ToList();
        sprint.CompletedPoints = issues.Where(i => i.Status == IssueStatus.Done).Sum(i => i.StoryPoints ?? 0);
        sprint.State = SprintState.Completed;
        // Unfinished work rolls over to the next planned sprint, or the backlog.
        var next = db.Sprints.Where(s => s.ProjectId == sprint.ProjectId && s.State == SprintState.Planned).OrderBy(s => s.Start).FirstOrDefault();
        foreach (var issue in issues.Where(i => i.IsOpen)) issue.SprintId = next?.Id;
        Changed?.Invoke(this, EventArgs.Empty);
    }
}

public sealed class InMemoryNotificationFeed(MockDatabase db, IIssueService issues, IClock clock) : INotificationFeed
{
    int _next = 100;

    public event EventHandler? Changed;

    public IReadOnlyList<AppNotification> Items => db.Notifications.OrderByDescending(n => n.When).ToList();

    public int UnreadCount => db.Notifications.Count(n => !n.IsRead);

    public void MarkRead(AppNotification notification)
    {
        if (notification.IsRead) return;
        notification.IsRead = true;
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public void MarkAllRead()
    {
        foreach (var n in db.Notifications) n.IsRead = true;
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public void Clear()
    {
        db.Notifications.Clear();
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public void Push(AppNotification notification)
    {
        db.Notifications.Add(notification);
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public AppNotification SimulateIncoming()
    {
        var n = _next++;
        var open = issues.All.Where(i => i.IsOpen).ToList();
        var issue = open.Count == 0 ? null : open[n % open.Count];
        var templates = new (FeedbackKind Kind, string Title, string Body)[]
        {
            (FeedbackKind.Info, $"Hana Sato commented on {issue?.Key}", "\"Verified the fix on the nightly build.\""),
            (FeedbackKind.Success, $"Build passed for {issue?.Key}", "All 412 checks passed on the pull request."),
            (FeedbackKind.Warning, $"{issue?.Key} is due tomorrow", issue?.Title ?? ""),
            (FeedbackKind.Info, $"Priya Raman assigned {issue?.Key} to you", issue?.Title ?? ""),
        };
        var t = templates[n % templates.Length];
        var notification = new AppNotification { Id = $"n{n}", Kind = t.Kind, Title = t.Title, Body = t.Body, When = clock.Now, IssueKey = issue?.Key };
        Push(notification);
        return notification;
    }
}

public sealed class Workspace : IWorkspace
{
    readonly IProjectService _projects;
    string _currentId;

    public Workspace(IProjectService projects, ISettingsService settings)
    {
        _projects = projects;
        _currentId = projects.Find(settings.Current.DefaultProjectId)?.Id ?? projects.All[0].Id;
    }

    public event EventHandler? CurrentProjectChanged;

    public Project CurrentProject => _projects.Find(_currentId) ?? _projects.All[0];

    public void SelectProject(string projectId)
    {
        if (projectId == _currentId || _projects.Find(projectId) is null) return;
        _currentId = projectId;
        CurrentProjectChanged?.Invoke(this, EventArgs.Empty);
    }
}

public sealed class MockAuthService(IPeopleService people, MockLatency latency) : IAuthService
{
    public bool IsSignedIn { get; private set; }

    public async Task<SignInResult> CheckEmailAsync(string email, CancellationToken ct = default)
    {
        await latency.WaitAsync(600, ct);
        email = email.Trim();
        if (email.Length == 0) return new(false, "email", "Enter your work email.");
        if (!email.Contains('@') || email.StartsWith('@') || email.EndsWith('@')) return new(false, "email", "Enter an email like name@company.com.");
        return people.All.Any(p => string.Equals(p.Email, email, StringComparison.OrdinalIgnoreCase))
            ? new(true, null, null)
            : new(false, "email", "There is no Relay account for this email.");
    }

    public async Task<SignInResult> SignInAsync(string email, string password, bool remember, CancellationToken ct = default)
    {
        await latency.WaitAsync(900, ct);
        if (password.Length == 0) return new(false, "password", "Enter your password.");
        if (password.Length < 6) return new(false, null, "The email or password is incorrect. Check them and try again.");
        IsSignedIn = true;
        return new(true, null, null);
    }

    public void SignOut() => IsSignedIn = false;
}

public sealed class MockAssistantService(IIssueService issues, IPeopleService people, ISprintService sprints, IProjectService projects, MockLatency latency) : IAssistantService
{
    public async Task<AssistantSummary> SummarizeSprintAsync(string projectId, CancellationToken ct = default)
    {
        await latency.WaitAsync(1800, ct);
        var sprint = sprints.Active(projectId);
        var project = projects.Find(projectId);
        if (sprint is null) return new($"{project?.Name} has no active sprint. Plan the next one from the backlog.", [], []);
        var list = issues.All.Where(i => i.SprintId == sprint.Id).ToList();
        var done = list.Count(i => i.Status == IssueStatus.Done);
        var blocked = list.Where(i => i.Labels.Contains("blocked") && i.IsOpen).ToList();
        var risky = list.Where(i => i.IsOpen && i.Priority >= IssuePriority.High).OrderByDescending(i => i.Priority).Take(3).ToList();
        var progress = ReportMath.SprintProgress(list);
        var text = $"{sprint.Name} is {progress:P0} done by points with {(sprint.End - DateTime.Today).Days} days left. " +
                   $"{done} of {list.Count} issues are closed. " +
                   (risky.Count > 0 ? $"The main risk is {risky[0].Key}, a {risky[0].Priority.Text().ToLowerInvariant()} priority {risky[0].Type.Text().ToLowerInvariant()} still {risky[0].Status.Text().ToLowerInvariant()}." : "No high-priority work is open.");
        var highlights = new List<string>
        {
            $"{list.Count(i => i.Status == IssueStatus.InReview)} issues are waiting for review",
            blocked.Count > 0 ? $"{blocked.Count} issues are labeled blocked" : "Nothing is labeled blocked",
            $"Goal: {sprint.Goal}",
        };
        return new(text, highlights, risky.Select(i => i.Key).ToList());
    }

    public async Task<AssistantSummary> SummarizeWeekAsync(string personId, CancellationToken ct = default)
    {
        await latency.WaitAsync(1600, ct);
        var name = people.NameOf(personId).Split(' ')[0];
        var mine = issues.All.Where(i => i.AssigneeId == personId).ToList();
        var recent = mine.Where(i => i.Updated > DateTime.Now.AddDays(-7)).OrderByDescending(i => i.Updated).ToList();
        var closed = recent.Count(i => !i.IsOpen);
        var text = $"This week {name} touched {recent.Count} issues and closed {closed}. " +
                   $"{mine.Count(i => i.IsOpen)} issues are still open, {mine.Count(i => i.IsOpen && i.Priority >= IssuePriority.High)} of them high priority or above.";
        return new(text, recent.Take(3).Select(i => $"{i.Key}: {i.Status.Text()}").ToList(), recent.Take(5).Select(i => i.Key).ToList());
    }

    public async Task<AssigneeSuggestion?> SuggestAssigneeAsync(Issue issue, CancellationToken ct = default)
    {
        await latency.WaitAsync(1200, ct);
        // Whoever closed the most issues in the same component, weighted by how little open work they have.
        var candidates = issues.All
            .Where(i => i.ProjectId == issue.ProjectId && i.Component == issue.Component && i.AssigneeId is not null && i.Key != issue.Key)
            .GroupBy(i => i.AssigneeId!)
            .Select(g => (Id: g.Key, Closed: g.Count(i => !i.IsOpen), Open: issues.All.Count(i => i.AssigneeId == g.Key && i.IsOpen)))
            .OrderByDescending(c => c.Closed * 2 - c.Open)
            .ToList();
        if (candidates.Count == 0) return null;
        var best = candidates[0];
        var confidence = Math.Clamp(0.55 + best.Closed * 0.08 - best.Open * 0.02, 0.35, 0.95);
        return new(best.Id, $"{people.NameOf(best.Id)} closed {best.Closed} {issue.Component} issues and has {best.Open} open.", confidence);
    }

    public async Task<IReadOnlyList<DuplicateCandidate>> FindDuplicatesAsync(CancellationToken ct = default)
    {
        await latency.WaitAsync(2000, ct);
        var open = issues.All.Where(i => i.IsOpen && i.Type != IssueType.Epic).ToList();
        var pairs = new List<DuplicateCandidate>();
        for (var a = 0; a < open.Count; a++)
        {
            for (var b = a + 1; b < open.Count; b++)
            {
                var s = Similarity(open[a].Title, open[b].Title);
                if (open[a].Component == open[b].Component) s += 0.15;
                if (s >= 0.35) pairs.Add(new DuplicateCandidate(open[a], open[b], Math.Min(s, 0.99)));
            }
        }
        return pairs.OrderByDescending(p => p.Similarity).Take(5).ToList();
    }

    static double Similarity(string a, string b)
    {
        static HashSet<string> Words(string s) =>
            s.ToLowerInvariant().Split([' ', '-', ','], StringSplitOptions.RemoveEmptyEntries).Where(w => w.Length > 3).ToHashSet();
        var x = Words(a);
        var y = Words(b);
        if (x.Count == 0 || y.Count == 0) return 0;
        return x.Intersect(y).Count() / (double)x.Union(y).Count();
    }
}

public sealed class InMemorySettingsService(MockLatency latency) : ISettingsService
{
    public AppSettings Current { get; private set; } = new();

    public async Task SaveAsync(AppSettings settings, CancellationToken ct = default)
    {
        await latency.WaitAsync(900, ct);
        Current = settings;
    }

    public async Task<string> RegenerateTokenAsync(CancellationToken ct = default)
    {
        await latency.WaitAsync(700, ct);
        Current.ApiToken = "rly_live_" + Guid.NewGuid().ToString("N")[..20];
        return Current.ApiToken;
    }
}

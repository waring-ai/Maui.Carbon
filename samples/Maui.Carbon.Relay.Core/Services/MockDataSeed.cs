using Maui.Carbon.Relay.Models;

namespace Maui.Carbon.Relay.Services;

/// <summary>The in-memory "server": every list the services read and change.</summary>
public sealed class MockDatabase
{
    public List<Person> People { get; } = [];

    public List<Project> Projects { get; } = [];

    public List<Sprint> Sprints { get; } = [];

    public List<Issue> Issues { get; } = [];

    public List<AppNotification> Notifications { get; } = [];

    public int NextIssueNumber(string projectKey) =>
        Issues.Where(i => i.Key.StartsWith(projectKey + "-", StringComparison.Ordinal))
            .Select(i => int.Parse(i.Key[(projectKey.Length + 1)..], System.Globalization.CultureInfo.InvariantCulture))
            .DefaultIfEmpty(100)
            .Max() + 1;
}

/// <summary>
/// Builds the same workspace every launch (seeded random), with dates relative to <c>now</c> so "updated 3 h ago"
/// and the active sprint stay current. The signed-in user is <see cref="MeId"/> (Maya Chen).
/// </summary>
public static class MockDataSeed
{
    public const string MeId = "p01";

    public static MockDatabase Create(DateTime now)
    {
        var db = new MockDatabase();
        var rng = new Random(20261118);
        AddPeople(db);
        AddProjects(db, now);
        AddSprints(db, now);
        AddIssues(db, rng, now);
        AddNotifications(db, now);
        return db;
    }

    static void AddPeople(MockDatabase db)
    {
        Person P(string id, string name, TeamRole role, string title, string location, params string[] skills) =>
            new(id, name, $"{name.ToLowerInvariant().Replace(' ', '.')}@relay.dev", role, title, location, skills);

        db.People.AddRange(
        [
            P("p01", "Maya Chen", TeamRole.Engineer, "Senior software engineer", "Toronto", "iOS", "Swift", "Offline sync"),
            P("p02", "Daniel Okafor", TeamRole.EngineeringManager, "Engineering manager", "London", "Planning", "Hiring", "Go"),
            P("p03", "Priya Raman", TeamRole.ProductManager, "Product manager, mobile", "Bengaluru", "Discovery", "Analytics"),
            P("p04", "Lucas Moreau", TeamRole.Engineer, "Software engineer", "Lyon", "Android", "Kotlin", "Maps"),
            P("p05", "Sofia Alvarez", TeamRole.Designer, "Product designer", "Madrid", "Interaction design", "Prototyping"),
            P("p06", "Ethan Brooks", TeamRole.Engineer, "Staff engineer, payments", "Austin", "Payments", "PostgreSQL", "C#"),
            P("p07", "Hana Sato", TeamRole.QualityEngineer, "Quality engineer", "Tokyo", "Test automation", "Accessibility"),
            P("p08", "Oliver Grant", TeamRole.Engineer, "Software engineer", "Dublin", "TypeScript", "React", "Webhooks"),
            P("p09", "Amara Nwosu", TeamRole.Designer, "Design systems lead", "Lagos", "Design tokens", "Figma", "Documentation"),
            P("p10", "Jonas Weber", TeamRole.Engineer, "Data engineer", "Berlin", "Python", "Spark", "dbt"),
            P("p11", "Leila Haddad", TeamRole.ProductManager, "Product manager, billing", "Montreal", "Pricing", "Compliance"),
            P("p12", "Tomás Silva", TeamRole.Engineer, "Software engineer", "Lisbon", "C#", "Tax engines", "APIs"),
        ]);
    }

    static void AddProjects(MockDatabase db, DateTime now)
    {
        db.Projects.AddRange(
        [
            new Project
            {
                Id = "atl", Key = "ATL", Name = "Atlas mobile", LeadId = "p02", Category = ProjectCategory.Mobile,
                Description = "The iOS and Android app field teams use to plan routes, log visits and work offline.",
                Created = now.AddDays(-410), TargetDate = now.Date.AddDays(46), IsStarred = true,
                Components = ["Sync", "Offline", "Sign-in", "Maps", "Push notifications"],
            },
            new Project
            {
                Id = "bil", Key = "BIL", Name = "Billing platform", LeadId = "p11", Category = ProjectCategory.Platform,
                Description = "Invoicing, payments, tax and the ledger behind every subscription.",
                Created = now.AddDays(-620), TargetDate = now.Date.AddDays(88), IsStarred = true,
                Components = ["Invoices", "Payments", "Tax", "Webhooks", "Ledger"],
            },
            new Project
            {
                Id = "hbr", Key = "HBR", Name = "Harbor design system", LeadId = "p09", Category = ProjectCategory.Design,
                Description = "Tokens, components and guidance shared by every Relay product surface.",
                Created = now.AddDays(-300), TargetDate = now.Date.AddDays(20),
                Components = ["Tokens", "Components", "Icons", "Documentation"],
            },
            new Project
            {
                Id = "ins", Key = "INS", Name = "Insights dashboards", LeadId = "p10", Category = ProjectCategory.Data,
                Description = "Usage and revenue dashboards for customer success and finance.",
                Created = now.AddDays(-150),
                Components = ["Pipelines", "Dashboards", "Exports"],
            },
            new Project
            {
                Id = "por", Key = "POR", Name = "Customer portal", LeadId = "p08", Category = ProjectCategory.Web,
                Description = "Self-service account, invoices and support for customers.",
                Created = now.AddDays(-90), TargetDate = now.Date.AddDays(120),
                Components = ["Account", "Invoices", "Support"],
            },
            new Project
            {
                Id = "leg", Key = "LEG", Name = "Legacy importer", LeadId = "p06", Category = ProjectCategory.Platform,
                Description = "One-off migration of accounts from the previous billing system. Archived after cut-over.",
                Created = now.AddDays(-800), IsArchived = true,
                Components = ["Import"],
            },
        ]);
    }

    static void AddSprints(MockDatabase db, DateTime now)
    {
        // Two-week sprints; the active one started 12 days ago, so it ends in 2 days.
        foreach (var (project, first, goals) in new (string, int, string[])[]
        {
            ("atl", 19, ["Offline visit logging", "Route planner beta", "Crash-free sessions above 99.5%", "Push notification settings", "Sync conflict resolution", "Map performance"]),
            ("bil", 31, ["Usage-based pricing", "EU VAT reverse charge", "Invoice PDF redesign", "Webhook retries", "Ledger reconciliation", "Dunning emails"]),
            ("hbr", 8, ["Dark theme tokens", "Data table v2", "Icon audit", "Docs search", "Accessible focus states", "Motion tokens"]),
        })
        {
            var activeStart = now.Date.AddDays(-12);
            for (var i = 0; i < 6; i++)
            {
                var start = activeStart.AddDays(14 * (i - 4));
                var state = i < 4 ? SprintState.Completed : i == 4 ? SprintState.Active : SprintState.Planned;
                var committed = 26 + (i * 7 + first) % 12;
                db.Sprints.Add(new Sprint
                {
                    Id = $"{project}-s{first + i}",
                    ProjectId = project,
                    Name = $"Sprint {first + i}",
                    Goal = goals[i],
                    Start = start,
                    End = start.AddDays(14),
                    State = state,
                    CommittedPoints = state == SprintState.Planned ? 0 : committed,
                    CompletedPoints = state == SprintState.Completed ? committed - (i * 5 + first) % 7 : 0,
                });
            }
        }
    }

    // Titles per project: (type, component, title). Epics come first so stories can point at them.
    static readonly Dictionary<string, (IssueType Type, string Component, string Title)[]> Titles = new()
    {
        ["atl"] =
        [
            (IssueType.Epic, "Offline", "Offline visit logging"),
            (IssueType.Epic, "Maps", "Route planner"),
            (IssueType.Epic, "Sync", "Conflict-free sync"),
            (IssueType.Bug, "Sync", "Sync stalls when the device switches from Wi-Fi to cellular"),
            (IssueType.Bug, "Maps", "Map tiles flicker after the app resumes from background"),
            (IssueType.Bug, "Sign-in", "SSO sign-in loops back to the login screen on iPadOS"),
            (IssueType.Bug, "Offline", "Photos attached offline upload twice after reconnecting"),
            (IssueType.Bug, "Push notifications", "Visit reminders arrive an hour late after a time zone change"),
            (IssueType.Bug, "Sync", "Deleted visits reappear after a full resync"),
            (IssueType.Bug, "Maps", "Route line is drawn under building labels"),
            (IssueType.Bug, "Offline", "App crashes when the offline queue holds more than 500 items"),
            (IssueType.Story, "Offline", "Log a visit without a connection"),
            (IssueType.Story, "Offline", "Show how many changes are waiting to sync"),
            (IssueType.Story, "Maps", "Reorder stops on a route by dragging"),
            (IssueType.Story, "Maps", "Estimate arrival times for every stop"),
            (IssueType.Story, "Sync", "Resolve edit conflicts by keeping the newest field"),
            (IssueType.Story, "Sync", "Let users retry a failed sync from the status bar"),
            (IssueType.Story, "Push notifications", "Choose which visit reminders to receive"),
            (IssueType.Story, "Sign-in", "Sign in with a passkey"),
            (IssueType.Story, "Maps", "Show traffic on the route preview"),
            (IssueType.Story, "Offline", "Download a region's maps for offline use"),
            (IssueType.Task, "Sync", "Add tracing to the sync engine"),
            (IssueType.Task, "Sign-in", "Rotate the OAuth client secret"),
            (IssueType.Task, "Maps", "Upgrade the map SDK to version 11"),
            (IssueType.Task, "Push notifications", "Migrate to the new push certificate"),
            (IssueType.Task, "Offline", "Write a migration for the offline queue schema"),
            (IssueType.Task, "Sync", "Load test sync with 10,000 visits"),
            (IssueType.Task, "Maps", "Remove the unused geocoding fallback"),
            (IssueType.Story, "Sign-in", "Remember the last workspace on sign-in"),
            (IssueType.Bug, "Sign-in", "Biometric prompt shows twice on cold start"),
            (IssueType.Story, "Push notifications", "Group reminders for the same customer"),
            (IssueType.Task, "Offline", "Document offline limits for support"),
        ],
        ["bil"] =
        [
            (IssueType.Epic, "Invoices", "Usage-based pricing"),
            (IssueType.Epic, "Tax", "EU VAT compliance"),
            (IssueType.Epic, "Webhooks", "Reliable webhooks"),
            (IssueType.Bug, "Invoices", "Invoice totals round differently from the line items"),
            (IssueType.Bug, "Payments", "Card retries ignore the customer's retry schedule"),
            (IssueType.Bug, "Tax", "Reverse charge note missing on German invoices"),
            (IssueType.Bug, "Webhooks", "Webhook signatures fail for payloads over 1 MB"),
            (IssueType.Bug, "Ledger", "Refunds post to the ledger twice when retried"),
            (IssueType.Bug, "Invoices", "Credit notes show the wrong currency symbol for CHF"),
            (IssueType.Bug, "Payments", "3-D Secure challenge times out on slow networks"),
            (IssueType.Story, "Invoices", "Bill metered usage at the end of each period"),
            (IssueType.Story, "Invoices", "Preview the next invoice before it is issued"),
            (IssueType.Story, "Tax", "Validate VAT numbers against VIES"),
            (IssueType.Story, "Tax", "Show tax breakdown per line item"),
            (IssueType.Story, "Webhooks", "Retry failed webhooks with exponential backoff"),
            (IssueType.Story, "Webhooks", "Let customers replay a webhook from the portal"),
            (IssueType.Story, "Payments", "Support SEPA Direct Debit"),
            (IssueType.Story, "Ledger", "Reconcile the ledger with the payment provider nightly"),
            (IssueType.Story, "Invoices", "Add a purchase order number to invoices"),
            (IssueType.Story, "Payments", "Send a receipt after every successful payment"),
            (IssueType.Task, "Ledger", "Partition the ledger table by month"),
            (IssueType.Task, "Webhooks", "Publish the webhook event catalog"),
            (IssueType.Task, "Tax", "Update tax rates for January"),
            (IssueType.Task, "Payments", "Rotate the payment provider API keys"),
            (IssueType.Task, "Invoices", "Archive invoices older than seven years"),
            (IssueType.Task, "Ledger", "Add alerts for unbalanced journal entries"),
            (IssueType.Bug, "Webhooks", "Webhook dashboard shows events out of order"),
            (IssueType.Story, "Invoices", "Email invoices in the customer's language"),
            (IssueType.Task, "Payments", "Remove the deprecated charges endpoint"),
            (IssueType.Story, "Tax", "Collect tax IDs during checkout"),
        ],
        ["hbr"] =
        [
            (IssueType.Epic, "Tokens", "Dark theme"),
            (IssueType.Epic, "Components", "Data table v2"),
            (IssueType.Bug, "Components", "Focus ring is clipped inside scrolling tables"),
            (IssueType.Bug, "Tokens", "Disabled text fails contrast in the dark theme"),
            (IssueType.Bug, "Icons", "Warning icon is misaligned at 16 px"),
            (IssueType.Bug, "Components", "Dropdown menu opens off screen near the bottom edge"),
            (IssueType.Story, "Tokens", "Publish layer tokens for nested surfaces"),
            (IssueType.Story, "Components", "Sticky header for long tables"),
            (IssueType.Story, "Components", "Inline editing in table cells"),
            (IssueType.Story, "Documentation", "Usage guidance for notifications"),
            (IssueType.Story, "Icons", "Add icons for sync states"),
            (IssueType.Story, "Documentation", "Search across component docs"),
            (IssueType.Task, "Tokens", "Generate tokens from the design file automatically"),
            (IssueType.Task, "Components", "Visual regression tests for every component"),
            (IssueType.Task, "Documentation", "Migrate docs to the new site generator"),
            (IssueType.Task, "Icons", "Remove duplicate arrow icons"),
            (IssueType.Story, "Components", "Skeleton states for cards"),
            (IssueType.Bug, "Documentation", "Code samples lose indentation when copied"),
        ],
        ["ins"] =
        [
            (IssueType.Story, "Dashboards", "Monthly recurring revenue by plan"),
            (IssueType.Story, "Pipelines", "Load usage events hourly instead of daily"),
            (IssueType.Bug, "Exports", "CSV export drops rows with commas in the name"),
            (IssueType.Task, "Pipelines", "Backfill churn data for 2025"),
            (IssueType.Story, "Dashboards", "Cohort retention chart"),
            (IssueType.Bug, "Dashboards", "Revenue chart double counts refunds"),
            (IssueType.Task, "Exports", "Schedule weekly exports to finance"),
            (IssueType.Story, "Dashboards", "Filter every dashboard by region"),
        ],
        ["por"] =
        [
            (IssueType.Story, "Account", "Invite teammates to the customer portal"),
            (IssueType.Story, "Invoices", "Download all invoices as one archive"),
            (IssueType.Bug, "Support", "Support form loses attachments on validation errors"),
            (IssueType.Task, "Account", "Add audit logging for role changes"),
            (IssueType.Story, "Support", "Show the status of open support cases"),
            (IssueType.Bug, "Account", "Password reset email links expire too early"),
            (IssueType.Story, "Invoices", "Pay an overdue invoice from the portal"),
        ],
    };

    static readonly string[] LabelPool = ["regression", "customer-reported", "performance", "accessibility", "security", "tech-debt", "good-first-issue", "needs-design", "blocked", "ios", "android"];

    static readonly string[] CommentPool =
    [
        "I can reproduce this on the latest build. Attaching logs.",
        "Is this still happening after yesterday's release?",
        "Picked this up. I'll have a draft PR by tomorrow.",
        "Design is ready, see the attached frames.",
        "This blocks the release checklist, raising the priority.",
        "Customer success flagged two more accounts with the same problem.",
        "Root cause is a race between the retry timer and the cancel path.",
        "Moving to review. The tests cover the reconnect case now.",
        "Can we split the settings part into its own story?",
        "Verified on staging. Looks good to me.",
    ];

    const string StackTraceSample =
        "System.InvalidOperationException: Sequence contains no elements\n" +
        "   at Atlas.Sync.QueueProcessor.DequeueBatch(Int32 size) in QueueProcessor.cs:line 142\n" +
        "   at Atlas.Sync.SyncEngine.FlushAsync(CancellationToken ct) in SyncEngine.cs:line 87\n" +
        "   at Atlas.Sync.ConnectivityMonitor.OnNetworkChanged(Object sender, NetworkEventArgs e) in ConnectivityMonitor.cs:line 39";

    static void AddIssues(MockDatabase db, Random rng, DateTime now)
    {
        var id = 1;
        var assigneesByProject = new Dictionary<string, string[]>
        {
            ["atl"] = ["p01", "p04", "p05", "p07", "p02"],
            ["bil"] = ["p06", "p12", "p08", "p11", "p07"],
            ["hbr"] = ["p09", "p05", "p01", "p08"],
            ["ins"] = ["p10", "p03"],
            ["por"] = ["p08", "p12", "p05"],
        };

        foreach (var (projectId, titles) in Titles)
        {
            var project = db.Projects.First(p => p.Id == projectId);
            var sprints = db.Sprints.Where(s => s.ProjectId == projectId).ToList();
            var active = sprints.FirstOrDefault(s => s.State == SprintState.Active);
            var planned = sprints.FirstOrDefault(s => s.State == SprintState.Planned);
            var completed = sprints.Where(s => s.State == SprintState.Completed).ToList();
            var epics = new List<string>();
            var number = 100 + rng.Next(5, 40);
            var team = assigneesByProject[projectId];

            for (var n = 0; n < titles.Length; n++)
            {
                var (type, component, title) = titles[n];
                number += 1 + rng.Next(0, 3);
                var key = $"{project.Key}-{number}";
                var created = now.AddDays(-rng.Next(2, 140)).AddHours(-rng.Next(0, 24));

                // Spread work across completed sprints, the active sprint, the next sprint and the backlog.
                var bucket = type == IssueType.Epic ? 4 : rng.Next(0, 10);
                Sprint? sprint;
                IssueStatus status;
                if (active is not null && bucket <= 3)
                {
                    sprint = active;
                    status = bucket switch { 0 => IssueStatus.Todo, 1 => IssueStatus.InProgress, 2 => IssueStatus.InReview, _ => IssueStatus.Done };
                }
                else if (completed.Count > 0 && bucket <= 6)
                {
                    sprint = completed[rng.Next(completed.Count)];
                    status = rng.Next(0, 12) == 0 ? IssueStatus.Cancelled : IssueStatus.Done;
                }
                else if (planned is not null && bucket == 7)
                {
                    sprint = planned;
                    status = IssueStatus.Todo;
                }
                else
                {
                    sprint = null;
                    status = type == IssueType.Epic ? IssueStatus.InProgress : rng.Next(0, 3) == 0 ? IssueStatus.Todo : IssueStatus.Backlog;
                }

                var priority = type == IssueType.Bug
                    ? (IssuePriority)Math.Min(4, 1 + rng.Next(0, 4))
                    : (IssuePriority)rng.Next(0, 4);
                var assignee = status == IssueStatus.Backlog && rng.Next(0, 3) == 0 ? null : team[rng.Next(team.Length)];
                var reporter = db.People[rng.Next(db.People.Count)].Id;
                var labels = LabelPool.OrderBy(_ => rng.Next()).Take(rng.Next(0, 3)).ToList();
                if (type == IssueType.Bug && rng.Next(0, 2) == 0 && !labels.Contains("customer-reported")) labels.Add("customer-reported");
                var updated = status is IssueStatus.Done or IssueStatus.Cancelled && sprint is not null
                    ? Min(sprint.End.AddHours(-rng.Next(2, 200)), now)
                    : Min(created.AddDays(rng.Next(0, 30)).AddHours(rng.Next(0, 24)), now.AddMinutes(-rng.Next(5, 600)));
                if (updated < created) updated = created;

                var issue = new Issue
                {
                    Id = id++,
                    Key = key,
                    ProjectId = projectId,
                    Title = title,
                    Type = type,
                    Status = status,
                    Priority = priority,
                    Resolution = status == IssueStatus.Done ? IssueResolution.Fixed : status == IssueStatus.Cancelled ? IssueResolution.WontFix : IssueResolution.None,
                    AssigneeId = assignee,
                    ReporterId = reporter,
                    Component = component,
                    Labels = labels,
                    StoryPoints = type == IssueType.Epic ? null : new[] { 1, 2, 3, 5, 8, 13 }[rng.Next(0, 6)],
                    SprintId = sprint?.Id,
                    ParentKey = type is IssueType.Story or IssueType.Task && epics.Count > 0 ? epics[rng.Next(epics.Count)] : null,
                    Created = created,
                    Updated = updated,
                    StartDate = sprint?.Start,
                    DueDate = type == IssueType.Epic ? project.TargetDate : sprint?.End ?? (rng.Next(0, 4) == 0 ? now.Date.AddDays(rng.Next(-6, 40)) : null),
                    Description = Describe(type, title, component),
                    Branch = status is IssueStatus.InProgress or IssueStatus.InReview ? $"{key.ToLowerInvariant()}-{Slug(title)}" : null,
                };
                if (type == IssueType.Epic) epics.Add(key);
                if (type == IssueType.Bug)
                {
                    issue.ReproSteps.AddRange(ReproFor(component));
                    if (component is "Sync" or "Offline") issue.StackTrace = StackTraceSample;
                }
                issue.WatcherIds.Add(reporter);
                if (assignee is not null) issue.WatcherIds.Add(assignee);
                if (rng.Next(0, 3) == 0) issue.WatcherIds.Add(MockDataSeed.MeId);

                issue.Activity.Add(new ActivityEvent($"a{issue.Id}-0", key, reporter, ActivityKind.Created, "created the issue", created));
                if (assignee is not null)
                    issue.Activity.Add(new ActivityEvent($"a{issue.Id}-1", key, reporter, ActivityKind.Assigned, $"assigned it to {db.People.First(p => p.Id == assignee).Name}", created.AddHours(1)));
                var commentCount = rng.Next(0, 4);
                for (var c = 0; c < commentCount; c++)
                {
                    var when = created + (updated - created) * ((c + 1) / (double)(commentCount + 1));
                    var author = c == 0 && assignee is not null ? assignee : db.People[rng.Next(db.People.Count)].Id;
                    issue.Comments.Add(new Comment($"c{issue.Id}-{c}", author, CommentPool[rng.Next(CommentPool.Length)], when));
                    issue.Activity.Add(new ActivityEvent($"a{issue.Id}-c{c}", key, author, ActivityKind.Commented, "commented", when));
                }
                if (status != IssueStatus.Backlog && status != IssueStatus.Todo)
                    issue.Activity.Add(new ActivityEvent($"a{issue.Id}-s", key, assignee ?? reporter, ActivityKind.StatusChanged, $"moved it to {status.Text()}", updated));
                if (type == IssueType.Bug && rng.Next(0, 2) == 0)
                    issue.Attachments.Add(new Attachment(component == "Maps" ? "map-flicker.mov" : "device-log.txt", 20_000 + rng.Next(0, 4_000_000), reporter, created.AddMinutes(10)));
                db.Issues.Add(issue);
            }
        }

        // Link a few related issues both ways.
        foreach (var (a, b) in new[] { (3, 8), (12, 4), (36, 40), (37, 54), (66, 70) })
        {
            var x = db.Issues.FirstOrDefault(i => i.Id == a);
            var y = db.Issues.FirstOrDefault(i => i.Id == b);
            if (x is null || y is null) continue;
            x.LinkedKeys.Add(y.Key);
            y.LinkedKeys.Add(x.Key);
        }

        // Make sure the signed-in user has a realistic personal queue.
        foreach (var issue in db.Issues.Where(i => i.ProjectId == "atl" && i.IsOpen && i.Type != IssueType.Epic).Take(8))
        {
            issue.AssigneeId = MockDataSeed.MeId;
            issue.WatcherIds.Add(MockDataSeed.MeId);
        }
    }

    static void AddNotifications(MockDatabase db, DateTime now)
    {
        var mine = db.Issues.Where(i => i.AssigneeId == MeId && i.IsOpen).ToList();
        var review = db.Issues.FirstOrDefault(i => i.Status == IssueStatus.InReview);
        var bug = db.Issues.FirstOrDefault(i => i.Type == IssueType.Bug && i.Priority == IssuePriority.Critical) ?? db.Issues.First(i => i.Type == IssueType.Bug);
        db.Notifications.AddRange(
        [
            new AppNotification { Id = "n1", Kind = FeedbackKind.Warning, Title = "Sprint 23 ends in 2 days", Body = "Atlas mobile has 9 open issues in the sprint.", When = now.AddMinutes(-18) },
            new AppNotification { Id = "n2", Kind = FeedbackKind.Info, Title = $"Lucas Moreau mentioned you on {mine.ElementAtOrDefault(0)?.Key ?? "ATL-120"}", Body = "\"@Maya can you check the reconnect path?\"", When = now.AddHours(-2), IssueKey = mine.ElementAtOrDefault(0)?.Key },
            new AppNotification { Id = "n3", Kind = FeedbackKind.Error, Title = $"{bug.Key} raised to {bug.Priority.Text()}", Body = bug.Title, When = now.AddHours(-5), IssueKey = bug.Key },
            new AppNotification { Id = "n4", Kind = FeedbackKind.Success, Title = $"{review?.Key ?? "BIL-140"} is ready for review", Body = review?.Title ?? "", When = now.AddHours(-9), IssueKey = review?.Key, IsRead = true },
            new AppNotification { Id = "n5", Kind = FeedbackKind.Info, Title = "Daniel Okafor added you to Harbor design system", Body = "You can now create and edit issues in HBR.", When = now.AddDays(-1), IsRead = true },
        ]);
    }

    static string Describe(IssueType type, string title, string component) => type switch
    {
        IssueType.Bug => $"{title}.\n\nThis affects the {component} area. It was first noticed after the last release and happens on about one in five attempts.\n\nExpected: the action completes normally.\nActual: see the steps to reproduce.",
        IssueType.Story => $"As a user, I want to {char.ToLowerInvariant(title[0])}{title[1..]}, so that I spend less time on routine work.\n\nAcceptance criteria:\n- Works offline and online\n- Meets the accessibility checklist\n- Covered by automated tests",
        IssueType.Epic => $"{title}: the umbrella for every story and task in this initiative. Track scope and progress here.",
        _ => $"{title}. Part of keeping the {component} area healthy.",
    };

    static IEnumerable<string> ReproFor(string component) => component switch
    {
        "Sync" => ["Sign in on a phone with Wi-Fi on", "Log three visits", "Turn Wi-Fi off while the sync indicator is spinning", "Wait for the cellular connection"],
        "Maps" => ["Open a route with at least five stops", "Switch to another app for 30 seconds", "Return to Atlas"],
        "Payments" => ["Create a subscription with a test card", "Trigger a failed payment", "Check the retry schedule in the dashboard"],
        _ => ["Open the affected screen", "Perform the action described in the summary", "Compare the result with the expected behavior"],
    };

    static string Slug(string title) =>
        string.Join('-', title.ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries).Take(4)
            .Select(w => new string(w.Where(char.IsLetterOrDigit).ToArray())).Where(w => w.Length > 0));

    static DateTime Min(DateTime a, DateTime b) => a < b ? a : b;
}

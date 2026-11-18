using System.ComponentModel;
using System.Globalization;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Maui.Carbon.Relay.Abstractions;
using Maui.Carbon.Relay.Models;
using Maui.Carbon.Relay.Services;

namespace Maui.Carbon.Relay.ViewModels;

/// <summary>
/// The settings screen. Edits a copy of <see cref="AppSettings"/> (Save writes it back, Discard reloads it); the theme
/// applies at once and the API token is reissued at once, so neither counts as an unsaved change. The danger zone's
/// delete asks for the workspace name before it enables.
/// </summary>
public sealed partial class SettingsViewModel : PageViewModel
{
    /// <summary>The name to type before the workspace can be deleted.</summary>
    public const string WorkspaceName = "Relay";

    static readonly string[] Tracked =
    [
        nameof(DisplayName), nameof(TimeZone), nameof(DateFormat), nameof(DefaultProject), nameof(ReduceMotion),
        nameof(CompactTables), nameof(EmailOnAssign), nameof(EmailOnMention), nameof(EmailOnStatus), nameof(DailyDigest),
        nameof(DigestHour), nameof(SlackEnabled), nameof(SlackChannel), nameof(GitHubEnabled),
    ];

    readonly ISettingsService _settings;
    readonly IProjectService _projects;
    readonly IThemeService _theme;
    readonly IUserFeedback _feedback;
    bool _loading;
    bool _syncingTheme;

    public SettingsViewModel(ISettingsService settings, IProjectService projects, IThemeService theme, IUserFeedback feedback, IClock clock)
    {
        _settings = settings;
        _projects = projects;
        _theme = theme;
        _feedback = feedback;
        Title = "Settings";
        DateFormats = new[] { "MMM d, yyyy", "d MMM yyyy", "yyyy-MM-dd" }
            .Select(f => new DateFormatOption(f, clock.Now.ToString(f, CultureInfo.InvariantCulture)))
            .ToList();
        ProjectOptions = projects.All.Where(p => !p.IsArchived).Select(p => new ProjectOption(p.Id, p.Name)).ToList();
        DefaultProject = ProjectOptions[0];
        Load();
    }

    // ---- choices ----

    public IReadOnlyList<ProjectOption> ProjectOptions { get; private set; }

    /// <summary>Time zone ids (IANA) with their display names, for the select.</summary>
    public IReadOnlyList<TimeZoneOption> TimeZones { get; } =
    [
        new("America/Vancouver", "Pacific time (Vancouver)"),
        new("America/Toronto", "Eastern time (Toronto)"),
        new("Europe/London", "Greenwich mean time (London)"),
        new("Europe/Berlin", "Central European time (Berlin)"),
        new("Asia/Kolkata", "India standard time (Bengaluru)"),
        new("Asia/Tokyo", "Japan standard time (Tokyo)"),
    ];

    /// <summary>The date formats offered, each labelled with today's date in that format.</summary>
    public IReadOnlyList<DateFormatOption> DateFormats { get; }

    /// <summary>The digest slider's value text: 8 → "8:00 AM".</summary>
    public static string FormatHour(double hour)
    {
        var h = (int)Math.Round(Math.Clamp(hour, 0, 23));
        return DateTime.Today.AddHours(h).ToString("h:mm tt", CultureInfo.InvariantCulture);
    }

    public Func<double, string> HourFormatter { get; } = FormatHour;

    // ---- General ----

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayNameError), nameof(IsDisplayNameInvalid), nameof(WebhookPayload))]
    public partial string DisplayName { get; set; } = "";

    public string? DisplayNameError => DisplayName.Trim().Length == 0 ? "Enter the name people see on your issues and comments." : null;

    public bool IsDisplayNameInvalid => DisplayNameError is not null;

    [ObservableProperty]
    public partial string Email { get; set; } = "";

    [ObservableProperty]
    public partial TimeZoneOption? TimeZone { get; set; }

    [ObservableProperty]
    public partial string DateFormat { get; set; } = "";

    [ObservableProperty]
    public partial ProjectOption DefaultProject { get; set; }

    /// <summary>The app theme. Applied as soon as it changes; not part of Save.</summary>
    [ObservableProperty]
    public partial ThemeChoice Theme { get; set; }

    [ObservableProperty]
    public partial bool ReduceMotion { get; set; }

    [ObservableProperty]
    public partial bool CompactTables { get; set; }

    // ---- Notifications ----

    [ObservableProperty]
    public partial bool EmailOnAssign { get; set; }

    [ObservableProperty]
    public partial bool EmailOnMention { get; set; }

    [ObservableProperty]
    public partial bool EmailOnStatus { get; set; }

    [ObservableProperty]
    public partial bool DailyDigest { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DigestHourText))]
    public partial double DigestHour { get; set; }

    public string DigestHourText => $"The digest arrives at {FormatHour(DigestHour)} in your time zone.";

    // ---- Integrations ----

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(WebhookPayload), nameof(SlackChannelError), nameof(IsSlackChannelInvalid))]
    public partial bool SlackEnabled { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SlackChannelError), nameof(IsSlackChannelInvalid), nameof(WebhookPayload))]
    public partial string SlackChannel { get; set; } = "";

    public string? SlackChannelError => SlackEnabled && !SlackChannel.Trim().StartsWith('#') ? "Channel names start with #, for example #atlas-dev." : null;

    public bool IsSlackChannelInvalid => SlackChannelError is not null;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(WebhookPayload))]
    public partial bool GitHubEnabled { get; set; }

    /// <summary>The API token. Reissued at once by <see cref="RegenerateTokenCommand"/>; not part of Save.</summary>
    [ObservableProperty]
    public partial string ApiToken { get; set; } = "";

    [ObservableProperty]
    public partial bool IsRegenerating { get; set; }

    public string WebhookUrl => _settings.Current.WebhookUrl;

    /// <summary>A sample of what the webhook posts when an issue changes, reflecting the integration settings.</summary>
    public string WebhookPayload => JsonSerializer.Serialize(new
    {
        @event = "issue.status_changed",
        delivered_to = WebhookUrl,
        issue = new { key = "ATL-142", title = "Offline edits lost after sync conflict", status = "in_review", previous_status = "in_progress" },
        actor = new { id = "p01", name = DisplayName },
        integrations = new { slack = SlackEnabled ? SlackChannel : null, github = GitHubEnabled },
        sent_at = "2026-11-18T10:00:00Z",
    }, PayloadOptions);

    static readonly JsonSerializerOptions PayloadOptions = new() { WriteIndented = true };

    // ---- save / discard ----

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand), nameof(DiscardCommand))]
    [NotifyPropertyChangedFor(nameof(SaveDisabled))]
    public partial bool IsDirty { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand), nameof(DiscardCommand))]
    [NotifyPropertyChangedFor(nameof(SaveDisabled), nameof(IsIdle))]
    public partial bool IsSaving { get; set; }

    public bool IsIdle => !IsSaving;

    bool CanSave => IsDirty && !IsSaving && !IsDisplayNameInvalid && !IsSlackChannelInvalid;

    public bool SaveDisabled => !CanSave;

    bool CanDiscard => IsDirty && !IsSaving;

    [RelayCommand(CanExecute = nameof(CanSave))]
    async Task SaveAsync()
    {
        IsSaving = true;
        try
        {
            var current = _settings.Current;
            var next = new AppSettings
            {
                DisplayName = DisplayName.Trim(),
                Email = current.Email,
                TimeZone = TimeZone?.Id ?? current.TimeZone,
                DateFormat = DateFormat,
                DefaultProjectId = DefaultProject.Id ?? current.DefaultProjectId,
                Theme = _theme.Current,
                ReduceMotion = ReduceMotion,
                CompactTables = CompactTables,
                EmailOnAssign = EmailOnAssign,
                EmailOnMention = EmailOnMention,
                EmailOnStatus = EmailOnStatus,
                DailyDigest = DailyDigest,
                DigestHour = (int)Math.Round(DigestHour),
                SlackEnabled = SlackEnabled,
                SlackChannel = SlackChannel.Trim(),
                GitHubEnabled = GitHubEnabled,
                ApiToken = current.ApiToken,
                WebhookUrl = current.WebhookUrl,
            };
            await _settings.SaveAsync(next);
        }
        finally
        {
            IsSaving = false;
        }
        UpdateDirty();
        _feedback.Toast(FeedbackKind.Success, "Settings saved");
    }

    [RelayCommand(CanExecute = nameof(CanDiscard))]
    void Discard()
    {
        Load();
        _feedback.Toast(FeedbackKind.Info, "Changes discarded");
    }

    [RelayCommand]
    async Task RegenerateTokenAsync()
    {
        IsRegenerating = true;
        try
        {
            ApiToken = await _settings.RegenerateTokenAsync();
        }
        finally
        {
            IsRegenerating = false;
        }
        _feedback.Toast(FeedbackKind.Success, "New API token issued", "The previous token stopped working. Update your scripts and CI secrets.");
    }

    // ---- danger zone ----

    [ObservableProperty]
    public partial bool IsDeleteOpen { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanConfirmDelete), nameof(DeleteDisabled))]
    [NotifyCanExecuteChangedFor(nameof(ConfirmDeleteCommand))]
    public partial string DeleteConfirmText { get; set; } = "";

    /// <summary>True once the workspace name is typed exactly (case-sensitive).</summary>
    public bool CanConfirmDelete => DeleteConfirmText.Trim() == WorkspaceName;

    public bool DeleteDisabled => !CanConfirmDelete;

    [RelayCommand]
    void RequestDelete()
    {
        DeleteConfirmText = "";
        IsDeleteOpen = true;
    }

    [RelayCommand(CanExecute = nameof(CanConfirmDelete))]
    void ConfirmDelete()
    {
        IsDeleteOpen = false;
        DeleteConfirmText = "";
        _feedback.Toast(FeedbackKind.Warning, "Workspace deletion scheduled", "This is a demo workspace, so nothing was deleted.");
    }

    [RelayCommand]
    void CancelDelete()
    {
        IsDeleteOpen = false;
        DeleteConfirmText = "";
    }

    // ---- lifecycle ----

    protected override Task OnActivatedAsync(object? parameter, CancellationToken ct)
    {
        // Projects may have been added since the last visit.
        var options = _projects.All.Where(p => !p.IsArchived).Select(p => new ProjectOption(p.Id, p.Name)).ToList();
        if (!options.Select(o => o.Id).SequenceEqual(ProjectOptions.Select(o => o.Id)))
        {
            var keep = DefaultProject.Id;
            ProjectOptions = options;
            OnPropertyChanged(nameof(ProjectOptions));
            _loading = true;
            DefaultProject = options.FirstOrDefault(o => o.Id == keep) ?? options[0];
            _loading = false;
        }
        // Unsaved edits survive leaving the screen; otherwise show what is saved now.
        if (!IsDirty) Load();
        SyncTheme();
        return Task.CompletedTask;
    }

    protected override void OnDeactivated() => CancelDelete();

    /// <summary>Shows the theme in use without applying it again (it can change from the account panel).</summary>
    public void SyncTheme()
    {
        _syncingTheme = true;
        try
        {
            Theme = _theme.Current;
        }
        finally
        {
            _syncingTheme = false;
        }
    }

    partial void OnThemeChanged(ThemeChoice value)
    {
        if (_loading || _syncingTheme || value == _theme.Current) return;
        _theme.Apply(value);
        _feedback.Toast(FeedbackKind.Info, $"Theme changed to {ThemeText(value)}");
    }

    static string ThemeText(ThemeChoice theme) => theme switch
    {
        ThemeChoice.G10 => "Gray 10",
        ThemeChoice.G90 => "Gray 90",
        ThemeChoice.G100 => "Gray 100",
        ThemeChoice.System => "your system setting",
        _ => "White",
    };

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (!_loading && e.PropertyName is { } name && Tracked.Contains(name)) UpdateDirty();
        if (e.PropertyName is nameof(IsDisplayNameInvalid) or nameof(IsSlackChannelInvalid))
        {
            SaveCommand.NotifyCanExecuteChanged();
            OnPropertyChanged(nameof(SaveDisabled));
        }
    }

    void Load()
    {
        var s = _settings.Current;
        _loading = true;
        try
        {
            DisplayName = s.DisplayName;
            Email = s.Email;
            TimeZone = TimeZones.FirstOrDefault(t => t.Id == s.TimeZone) ?? TimeZones[0];
            DateFormat = s.DateFormat;
            DefaultProject = ProjectOptions.FirstOrDefault(o => o.Id == s.DefaultProjectId) ?? ProjectOptions[0];
            Theme = _theme.Current;
            ReduceMotion = s.ReduceMotion;
            CompactTables = s.CompactTables;
            EmailOnAssign = s.EmailOnAssign;
            EmailOnMention = s.EmailOnMention;
            EmailOnStatus = s.EmailOnStatus;
            DailyDigest = s.DailyDigest;
            DigestHour = s.DigestHour;
            SlackEnabled = s.SlackEnabled;
            SlackChannel = s.SlackChannel;
            GitHubEnabled = s.GitHubEnabled;
            ApiToken = s.ApiToken;
        }
        finally
        {
            _loading = false;
        }
        UpdateDirty();
    }

    void UpdateDirty()
    {
        var s = _settings.Current;
        IsDirty = DisplayName != s.DisplayName
            || (TimeZone?.Id ?? s.TimeZone) != s.TimeZone
            || DateFormat != s.DateFormat
            || (DefaultProject.Id ?? s.DefaultProjectId) != s.DefaultProjectId
            || ReduceMotion != s.ReduceMotion
            || CompactTables != s.CompactTables
            || EmailOnAssign != s.EmailOnAssign
            || EmailOnMention != s.EmailOnMention
            || EmailOnStatus != s.EmailOnStatus
            || DailyDigest != s.DailyDigest
            || (int)Math.Round(DigestHour) != s.DigestHour
            || SlackEnabled != s.SlackEnabled
            || SlackChannel != s.SlackChannel
            || GitHubEnabled != s.GitHubEnabled;
    }
}

/// <summary>A date format choice: the format string and today's date written in it.</summary>
public sealed record DateFormatOption(string Format, string Example);

/// <summary>A time zone in the settings select.</summary>
public sealed record TimeZoneOption(string Id, string Name)
{
    public override string ToString() => Name;
}

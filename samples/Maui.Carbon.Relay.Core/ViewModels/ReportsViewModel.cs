using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Maui.Carbon.Relay.Logic;
using Maui.Carbon.Relay.Models;
using Maui.Carbon.Relay.Services;

namespace Maui.Carbon.Relay.ViewModels;

/// <summary>
/// Throughput and workload reports: created vs resolved issues per day or week (<see cref="ReportMath.Throughput"/>)
/// for a week, month or quarter or a custom date range, summary numbers, and open work per assignee
/// (<see cref="ReportMath.Workload"/>), all filtered by project. Every change recomputes with a short simulated
/// delay so the view shows its skeleton.
/// </summary>
public sealed partial class ReportsViewModel : PageViewModel
{
    /// <summary>The height of the chart's plot area; bar heights are scaled into it.</summary>
    public const double PlotHeight = 220;

    /// <summary>The simulated recompute time (scaled by <see cref="MockLatency"/>, so 0 in tests and screenshots).</summary>
    public const int RecomputeMilliseconds = 450;

    readonly IIssueService _issues;
    readonly IPeopleService _people;
    readonly IClock _clock;
    readonly MockLatency _latency;
    CancellationTokenSource? _recompute;
    bool _loaded;
    bool _applyingCustomRange;

    public ReportsViewModel(IIssueService issues, IProjectService projects, IPeopleService people, IClock clock, MockLatency latency)
    {
        _issues = issues;
        _people = people;
        _clock = clock;
        _latency = latency;
        Title = "Reports";
        ProjectOptions = [new ProjectOption(null, "All projects"), .. projects.All.Where(p => !p.IsArchived).Select(p => new ProjectOption(p.Id, p.Name))];
        ProjectFilter = ProjectOptions[0];
    }

    // ---- filters ----

    /// <summary>The content switcher: 0 week, 1 month, 2 quarter.</summary>
    [ObservableProperty]
    public partial int RangeIndex { get; set; } = 1;

    public ReportRange Range => (ReportRange)Math.Clamp(RangeIndex, 0, 2);

    public IReadOnlyList<ProjectOption> ProjectOptions { get; }

    [ObservableProperty]
    public partial ProjectOption ProjectFilter { get; set; }

    /// <summary>The custom range start (two-way with the date picker). Both ends set → the chart covers that range.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasCustomRange))]
    public partial DateTime? CustomStart { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasCustomRange))]
    public partial DateTime? CustomEnd { get; set; }

    public bool HasCustomRange => CustomStart is not null && CustomEnd is not null;

    /// <summary>The latest date the range picker allows.</summary>
    public DateTime Today => _clock.Now.Date;

    // ---- state ----

    /// <summary>True while numbers are recomputed after a filter change (the view shows skeletons).</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsBusy))]
    public partial bool IsRecomputing { get; set; }

    /// <summary>First load or recompute: the chart and table areas show skeletons.</summary>
    public bool IsBusy => IsLoading || IsRecomputing;

    protected override void OnPropertyChanged(System.ComponentModel.PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.PropertyName == nameof(IsLoading)) base.OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs(nameof(IsBusy)));
    }

    // ---- chart ----

    [ObservableProperty]
    public partial IReadOnlyList<ChartBar> Bars { get; set; } = [];

    /// <summary>Value-axis labels, top to bottom.</summary>
    [ObservableProperty]
    public partial IReadOnlyList<string> AxisLabels { get; set; } = [];

    [ObservableProperty]
    public partial string RangeText { get; set; } = "";

    [ObservableProperty]
    public partial string XAxisTitle { get; set; } = "";

    [ObservableProperty]
    public partial string ChartDescription { get; set; } = "";

    // ---- summary tiles ----

    [ObservableProperty]
    public partial int CreatedTotal { get; set; }

    [ObservableProperty]
    public partial int ResolvedTotal { get; set; }

    [ObservableProperty]
    public partial string NetText { get; set; } = "";

    [ObservableProperty]
    public partial string NetHelper { get; set; } = "";

    /// <summary>Above zero: the backlog grew; below: it shrank.</summary>
    [ObservableProperty]
    public partial int Net { get; set; }

    [ObservableProperty]
    public partial string ResolutionRateText { get; set; } = "";

    [ObservableProperty]
    public partial string ResolutionRateHelper { get; set; } = "";

    [ObservableProperty]
    public partial int OpenNow { get; set; }

    [ObservableProperty]
    public partial string OpenNowHelper { get; set; } = "";

    // ---- workload ----

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasWorkload))]
    public partial IReadOnlyList<WorkloadRow> Workload { get; set; } = [];

    public bool HasWorkload => Workload.Count > 0;

    [ObservableProperty]
    public partial string WorkloadCaption { get; set; } = "";

    // ---- lifecycle ----

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
        Compute();
    }

    protected override void OnDeactivated()
    {
        _issues.Changed -= OnIssuesChanged;
        _recompute?.Cancel();
        IsRecomputing = false;
    }

    void OnIssuesChanged(object? sender, IssuesChangedEventArgs e) => Compute();

    partial void OnRangeIndexChanged(int value)
    {
        // Picking a preset range leaves the custom one.
        if (!_applyingCustomRange && HasCustomRange)
        {
            _applyingCustomRange = true;
            CustomStart = null;
            CustomEnd = null;
            _applyingCustomRange = false;
        }
        Recompute();
    }

    partial void OnProjectFilterChanged(ProjectOption value) => Recompute();

    partial void OnCustomStartChanged(DateTime? value) => OnCustomRangeChanged();

    partial void OnCustomEndChanged(DateTime? value) => OnCustomRangeChanged();

    void OnCustomRangeChanged()
    {
        if (_applyingCustomRange) return;
        // Recompute when the range is complete, and when it was cleared (back to the preset).
        if (HasCustomRange || (CustomStart is null && CustomEnd is null)) Recompute();
    }

    [RelayCommand]
    void ClearCustomRange()
    {
        _applyingCustomRange = true;
        CustomStart = null;
        CustomEnd = null;
        _applyingCustomRange = false;
        Recompute();
    }

    // ---- computing ----

    /// <summary>Recomputes after the simulated delay; a newer change cancels an older one.</summary>
    async void Recompute()
    {
        if (!_loaded) return;
        _recompute?.Cancel();
        var cts = CancellationTokenSource.CreateLinkedTokenSource(Lifetime);
        _recompute = cts;
        IsRecomputing = true;
        try
        {
            await _latency.WaitAsync(RecomputeMilliseconds, cts.Token);
            Compute();
        }
        catch (OperationCanceledException)
        {
            // Superseded by a newer change, or the screen was left.
        }
        finally
        {
            if (ReferenceEquals(_recompute, cts)) IsRecomputing = false;
        }
    }

    IEnumerable<Issue> Scoped() =>
        ProjectFilter.Id is { } id ? _issues.All.Where(i => i.ProjectId == id) : _issues.All;

    void Compute()
    {
        if (!_loaded) return;
        var now = _clock.Now;
        var scoped = Scoped().ToList();
        var custom = HasCustomRange;
        var buckets = custom
            ? ReportsChart.CustomThroughput(scoped, CustomStart!.Value, CustomEnd!.Value)
            : ReportMath.Throughput(scoped, Range, now);

        var daily = buckets.Count > 0 && (buckets[0].End - buckets[0].Start).TotalDays <= 1;
        var axisMax = ReportsChart.AxisMax(buckets.Select(b => Math.Max(b.Created, b.Resolved)).DefaultIfEmpty(0).Max());
        Bars = buckets.Select(b => new ChartBar(b, Scale(b.Created, axisMax), Scale(b.Resolved, axisMax), Period(b, daily))).ToList();
        AxisLabels = ReportsChart.Ticks(axisMax).Select(t => t.ToString(CultureInfo.InvariantCulture)).ToList();
        XAxisTitle = daily ? "Day" : "Week starting";
        RangeText = custom
            ? $"{Date(buckets[0].Start)} – {Date(buckets[^1].End.AddDays(-1))}"
            : Range switch
            {
                ReportRange.Week => "Last 7 days",
                ReportRange.Month => "Last 5 weeks",
                _ => "Last 13 weeks",
            };

        CreatedTotal = buckets.Sum(b => b.Created);
        ResolvedTotal = buckets.Sum(b => b.Resolved);
        Net = CreatedTotal - ResolvedTotal;
        NetText = Net switch { > 0 => $"+{Net}", < 0 => $"−{-Net}", _ => "0" };
        NetHelper = Net switch
        {
            > 0 => $"The backlog grew by {Net}",
            < 0 => $"The backlog shrank by {-Net}",
            _ => "Created and resolved are even",
        };
        ResolutionRateText = CreatedTotal == 0 ? "—" : $"{ResolvedTotal / (double)CreatedTotal:P0}";
        ResolutionRateHelper = CreatedTotal == 0 ? "Nothing was created" : "Resolved per created issue";
        var open = scoped.Where(i => i.IsOpen).ToList();
        OpenNow = open.Count;
        var overdue = open.Count(i => i.IsOverdue(now));
        OpenNowHelper = overdue == 0 ? "None overdue" : $"{overdue} overdue";
        ChartDescription = $"Created and resolved issues, {RangeText.ToLowerInvariant()}: {CreatedTotal} created, {ResolvedTotal} resolved.";

        Workload = BuildWorkload(scoped);
        var assigned = Workload.Count(w => w.AssigneeId is not null);
        WorkloadCaption = assigned == 1 ? "1 person has open work" : $"{assigned} people have open work";
    }

    IReadOnlyList<WorkloadRow> BuildWorkload(IEnumerable<Issue> scoped)
    {
        var load = ReportMath.Workload(scoped);
        var totalPoints = load.Sum(l => l.Points);
        var people = load.Where(l => l.AssigneeId is not null).ToList();
        var average = people.Count == 0 ? 0 : people.Average(l => l.Points);
        return load.Select(l =>
        {
            var person = _people.Find(l.AssigneeId);
            var level = l.AssigneeId is null ? WorkloadLevel.Unassigned
                : l.Points > average * 1.4 ? WorkloadLevel.Heavy
                : l.Points < average * 0.6 ? WorkloadLevel.Light
                : WorkloadLevel.Balanced;
            var share = totalPoints == 0 ? "—" : $"{l.Points / (double)totalPoints:P0}";
            return new WorkloadRow(l.AssigneeId, person?.Name ?? "Unassigned", person?.RoleText ?? "—", l.Open, l.Points, share, level);
        }).ToList();
    }

    static double Scale(int value, int axisMax) => axisMax <= 0 ? 0 : Math.Round(PlotHeight * value / axisMax, 1);

    static string Date(DateTime d) => d.ToString("MMM d", CultureInfo.InvariantCulture);

    static string Period(ReportBucket b, bool daily)
    {
        if (daily) return b.Start.ToString("ddd, MMM d", CultureInfo.InvariantCulture);
        var last = b.End.AddDays(-1);
        return last.Month == b.Start.Month
            ? $"{Date(b.Start)}–{last.Day.ToString(CultureInfo.InvariantCulture)}"
            : $"{Date(b.Start)} – {Date(last)}";
    }
}

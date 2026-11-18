namespace Maui.Carbon.Relay.Tests;

public class ReportsViewModelTests
{
    readonly TestWorld _w = new();

    ReportsViewModel Create(MockLatency? latency = null) =>
        new(_w.Issues, _w.Projects, _w.People, _w.Clock, latency ?? MockLatency.None);

    async Task<ReportsViewModel> Activated()
    {
        var vm = Create();
        await vm.ActivateAsync(null);
        return vm;
    }

    [Fact]
    public async Task Activation_shows_the_last_month_by_week()
    {
        var vm = await Activated();
        var expected = ReportMath.Throughput(_w.Issues.All, ReportRange.Month, TestWorld.Now);
        Assert.False(vm.IsLoading);
        Assert.False(vm.IsBusy);
        Assert.Equal(ReportRange.Month, vm.Range);
        Assert.Equal(5, vm.Bars.Count);
        Assert.Equal("Last 5 weeks", vm.RangeText);
        Assert.Equal("Week starting", vm.XAxisTitle);
        Assert.Equal(expected.Sum(b => b.Created), vm.CreatedTotal);
        Assert.Equal(expected.Sum(b => b.Resolved), vm.ResolvedTotal);
        Assert.Equal(expected.Select(b => b.Label), vm.Bars.Select(b => b.Label));
    }

    [Theory]
    [InlineData(0, 7, "Day", "Last 7 days")]
    [InlineData(1, 5, "Week starting", "Last 5 weeks")]
    [InlineData(2, 13, "Week starting", "Last 13 weeks")]
    public async Task The_range_switcher_changes_the_buckets(int index, int bars, string axis, string text)
    {
        var vm = await Activated();
        vm.RangeIndex = index;
        Assert.Equal(bars, vm.Bars.Count);
        Assert.Equal(axis, vm.XAxisTitle);
        Assert.Equal(text, vm.RangeText);
    }

    [Fact]
    public async Task Bars_scale_into_the_plot_and_the_axis_reaches_the_tallest_bar()
    {
        var vm = await Activated();
        vm.RangeIndex = 2;
        var max = vm.Bars.Max(b => Math.Max(b.Created, b.Resolved));
        var axisMax = int.Parse(vm.AxisLabels[0], System.Globalization.CultureInfo.InvariantCulture);
        Assert.True(axisMax >= max);
        Assert.Equal("0", vm.AxisLabels[^1]);
        Assert.All(vm.Bars, b =>
        {
            Assert.InRange(b.CreatedHeight, 0, ReportsViewModel.PlotHeight);
            Assert.InRange(b.ResolvedHeight, 0, ReportsViewModel.PlotHeight);
        });
        var tallest = vm.Bars.First(b => b.Created == max || b.Resolved == max);
        Assert.Equal(Math.Round(ReportsViewModel.PlotHeight * max / axisMax, 1), Math.Max(tallest.CreatedHeight, tallest.ResolvedHeight));
    }

    [Fact]
    public async Task Each_bar_tooltip_gives_the_exact_numbers()
    {
        var vm = await Activated();
        vm.RangeIndex = 0;
        var bar = vm.Bars[^1];
        Assert.Equal($"Wed, Nov 18: {bar.Created} created", bar.CreatedTip);
        Assert.Equal($"Wed, Nov 18: {bar.Resolved} resolved", bar.ResolvedTip);
        Assert.Contains($"{bar.Created} created, {bar.Resolved} resolved", bar.Description, StringComparison.Ordinal);
    }

    [Fact]
    public async Task The_project_filter_scopes_chart_summary_and_workload()
    {
        var vm = await Activated();
        vm.ProjectFilter = vm.ProjectOptions.First(p => p.Id == "bil");
        var bil = _w.Issues.All.Where(i => i.ProjectId == "bil").ToList();
        var expected = ReportMath.Throughput(bil, ReportRange.Month, TestWorld.Now);
        Assert.Equal(expected.Sum(b => b.Created), vm.CreatedTotal);
        Assert.Equal(bil.Count(i => i.IsOpen), vm.OpenNow);
        var load = ReportMath.Workload(bil);
        Assert.Equal(load.Select(l => l.AssigneeId), vm.Workload.Select(w => w.AssigneeId));
        Assert.Equal(load.Select(l => l.Points), vm.Workload.Select(w => w.Points));
    }

    [Fact]
    public async Task Archived_projects_are_not_offered()
    {
        var vm = await Activated();
        Assert.Null(vm.ProjectOptions[0].Id);
        Assert.DoesNotContain(vm.ProjectOptions, p => p.Id == "leg");
    }

    [Fact]
    public async Task Workload_rows_name_people_and_flag_load_against_the_average()
    {
        var vm = await Activated();
        Assert.True(vm.HasWorkload);
        var people = vm.Workload.Where(w => w.AssigneeId is not null).ToList();
        var average = people.Average(w => w.Points);
        Assert.All(people, w =>
        {
            Assert.NotEqual("Unassigned", w.Name);
            var expected = w.Points > average * 1.4 ? WorkloadLevel.Heavy : w.Points < average * 0.6 ? WorkloadLevel.Light : WorkloadLevel.Balanced;
            Assert.Equal(expected, w.Level);
        });
        Assert.Contains(vm.Workload, w => w.Level == WorkloadLevel.Heavy || w.Level == WorkloadLevel.Light);
        var unassigned = vm.Workload.Single(w => w.AssigneeId is null);
        Assert.Equal("Unassigned", unassigned.Name);
        Assert.Equal("Needs an owner", unassigned.LevelText);
        Assert.Equal($"{people.Count} people have open work", vm.WorkloadCaption);
    }

    [Fact]
    public async Task Summary_tiles_show_net_change_and_resolution_rate()
    {
        var vm = await Activated();
        Assert.Equal(vm.CreatedTotal - vm.ResolvedTotal, vm.Net);
        var expectedNet = vm.Net switch { > 0 => $"+{vm.Net}", < 0 => $"−{-vm.Net}", _ => "0" };
        Assert.Equal(expectedNet, vm.NetText);
        Assert.Equal($"{vm.ResolvedTotal / (double)vm.CreatedTotal:P0}", vm.ResolutionRateText);
        Assert.Equal(_w.Issues.All.Count(i => i.IsOpen), vm.OpenNow);
    }

    [Fact]
    public async Task A_custom_range_replaces_the_preset_until_cleared()
    {
        var vm = await Activated();
        vm.CustomStart = new DateTime(2026, 11, 9);
        Assert.Equal(5, vm.Bars.Count); // half a range changes nothing yet
        vm.CustomEnd = new DateTime(2026, 11, 18);
        Assert.True(vm.HasCustomRange);
        Assert.Equal(10, vm.Bars.Count);
        Assert.Equal("Day", vm.XAxisTitle);
        Assert.Equal("Nov 9 – Nov 18", vm.RangeText);

        vm.ClearCustomRangeCommand.Execute(null);
        Assert.False(vm.HasCustomRange);
        Assert.Equal(5, vm.Bars.Count);
    }

    [Fact]
    public async Task Picking_a_preset_range_leaves_the_custom_range()
    {
        var vm = await Activated();
        vm.CustomStart = new DateTime(2026, 9, 1);
        vm.CustomEnd = new DateTime(2026, 11, 18);
        Assert.Equal("Week starting", vm.XAxisTitle);
        vm.RangeIndex = 0;
        Assert.False(vm.HasCustomRange);
        Assert.Null(vm.CustomStart);
        Assert.Equal(7, vm.Bars.Count);
    }

    [Fact]
    public async Task Issue_changes_recompute_the_report()
    {
        var vm = await Activated();
        vm.RangeIndex = 0;
        var open = vm.OpenNow;
        var resolvedToday = vm.Bars[^1].Resolved;
        var issue = _w.Issues.All.First(i => i.IsOpen && i.Type != IssueType.Epic);
        await _w.Issues.SetStatusAsync([issue], IssueStatus.Done, TestContext.Current.CancellationToken);
        Assert.Equal(open - 1, vm.OpenNow);
        Assert.Equal(resolvedToday + 1, vm.Bars[^1].Resolved);
    }

    [Fact]
    public async Task Recomputing_shows_the_busy_state_until_the_numbers_are_ready()
    {
        var vm = Create(new MockLatency { Scale = 0.02 });
        await vm.ActivateAsync(null);
        Assert.False(vm.IsBusy);
        vm.RangeIndex = 2;
        Assert.True(vm.IsRecomputing);
        Assert.True(vm.IsBusy);
        for (var i = 0; i < 100 && vm.IsRecomputing; i++) await Task.Delay(20, TestContext.Current.CancellationToken);
        Assert.False(vm.IsRecomputing);
        Assert.Equal(13, vm.Bars.Count);
    }

    [Fact]
    public async Task A_newer_change_supersedes_a_pending_recompute()
    {
        var vm = Create(new MockLatency { Scale = 0.05 });
        await vm.ActivateAsync(null);
        vm.RangeIndex = 2;
        vm.RangeIndex = 0;
        for (var i = 0; i < 100 && vm.IsRecomputing; i++) await Task.Delay(20, TestContext.Current.CancellationToken);
        Assert.Equal(7, vm.Bars.Count);
    }
}

public class ReportsChartTests
{
    [Theory]
    [InlineData(0, 4)]
    [InlineData(3, 4)]
    [InlineData(7, 8)]
    [InlineData(9, 10)]
    [InlineData(13, 20)]
    [InlineData(37, 40)]
    [InlineData(41, 50)]
    [InlineData(120, 200)]
    public void Axis_max_is_a_round_number_at_or_above_the_data(int max, int expected) =>
        Assert.Equal(expected, ReportsChart.AxisMax(max));

    [Theory]
    [InlineData(20, new[] { 20, 15, 10, 5, 0 })]
    [InlineData(50, new[] { 50, 40, 30, 20, 10, 0 })]
    [InlineData(10, new[] { 10, 8, 6, 4, 2, 0 })]
    public void Ticks_are_whole_numbers_from_the_top_to_zero(int axisMax, int[] expected) =>
        Assert.Equal(expected, ReportsChart.Ticks(axisMax));

    [Fact]
    public void Short_custom_ranges_get_a_bar_per_day()
    {
        var w = new TestWorld();
        var buckets = ReportsChart.CustomThroughput(w.Issues.All, new DateTime(2026, 11, 5), new DateTime(2026, 11, 18));
        Assert.Equal(14, buckets.Count);
        Assert.All(buckets, b => Assert.Equal(1, (b.End - b.Start).TotalDays));
        Assert.Equal(new DateTime(2026, 11, 19), buckets[^1].End);
    }

    [Fact]
    public void Long_custom_ranges_get_weekly_bars_and_stay_readable()
    {
        var w = new TestWorld();
        var start = new DateTime(2026, 1, 1);
        var end = new DateTime(2026, 11, 18);
        var buckets = ReportsChart.CustomThroughput(w.Issues.All, start, end);
        Assert.InRange(buckets.Count, 2, ReportsChart.MaxCustomBars);
        Assert.Equal(0, (buckets[0].End - buckets[0].Start).TotalDays % 7);
        Assert.Equal(end.AddDays(1), buckets[^1].End);
        var created = w.Issues.All.Count(i => i.Created >= start && i.Created < end.AddDays(1));
        Assert.Equal(created, buckets.Sum(b => b.Created));
    }

    [Fact]
    public void Reversed_dates_are_swapped()
    {
        var w = new TestWorld();
        var buckets = ReportsChart.CustomThroughput(w.Issues.All, new DateTime(2026, 11, 18), new DateTime(2026, 11, 12));
        Assert.Equal(7, buckets.Count);
        Assert.Equal(new DateTime(2026, 11, 12), buckets[0].Start);
    }
}

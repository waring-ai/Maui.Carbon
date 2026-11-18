namespace Maui.Carbon.Relay.Tests;

public class BoardViewModelTests
{
    readonly TestWorld _w = new();

    BoardViewModel Create() => new(_w.Issues, _w.Sprints, _w.Workspace, _w.People, _w.Projects, _w.Navigator, _w.Feedback, _w.Clock);

    Sprint Active => _w.Sprints.Active(_w.Workspace.CurrentProject.Id)!;

    List<Issue> SprintIssues() => BoardRules.SprintIssues(_w.Issues.All, Active.Id).ToList();

    BoardColumn Column(BoardViewModel vm, IssueStatus status) => vm.Columns.Single(c => c.Status == status);

    [Fact]
    public async Task Activation_lays_out_the_active_sprint_in_four_columns()
    {
        var vm = Create();
        await vm.ActivateAsync(null);

        Assert.False(vm.IsLoading);
        Assert.True(vm.HasActiveSprint);
        Assert.False(vm.IsEmpty);
        Assert.Equal(Active.Name, vm.SprintName);
        Assert.Equal(["To do", "In progress", "In review", "Done"], vm.Columns.Select(c => c.Title));
        var issues = SprintIssues();
        Assert.Equal(issues.Count, vm.TotalCount);
        Assert.Equal(issues.Count, vm.Columns.Sum(c => c.Cards.Count));
        foreach (var column in vm.Columns)
        {
            Assert.All(column.Cards, c => Assert.Equal(column.Status, c.Status));
            Assert.Equal(column.Count.ToString(System.Globalization.CultureInfo.InvariantCulture), column.CountText);
        }
        Assert.DoesNotContain(vm.Columns.SelectMany(c => c.Cards), c => c.Type == IssueType.Epic);
    }

    [Fact]
    public async Task Cards_are_ordered_most_urgent_first()
    {
        var vm = Create();
        await vm.ActivateAsync(null);
        foreach (var column in vm.Columns)
            Assert.Equal(column.Cards.OrderByDescending(c => c.Priority).Select(c => c.Priority), column.Cards.Select(c => c.Priority));
    }

    [Fact]
    public async Task Card_menu_offers_every_other_column_then_open_issue()
    {
        var vm = Create();
        await vm.ActivateAsync(null);
        var card = vm.Columns.SelectMany(c => c.Cards).First(c => c.Status == IssueStatus.InProgress);
        Assert.Equal(["Move to To do", "Move to In review", "Move to Done", "Open issue"], card.Actions.Select(a => a.Text));
    }

    [Fact]
    public async Task Bugs_filter_shows_only_bugs_and_counts_shown_of_total()
    {
        var vm = Create();
        await vm.ActivateAsync(null);
        var total = vm.TotalCount;

        vm.ToggleFilterCommand.Execute(vm.Filters.Single(f => f.Kind == BoardFilterKind.Bugs));

        var cards = vm.Columns.SelectMany(c => c.Cards).ToList();
        Assert.All(cards, c => Assert.Equal(IssueType.Bug, c.Type));
        Assert.Equal(SprintIssues().Count(i => i.Type == IssueType.Bug), cards.Count);
        Assert.True(vm.HasActiveFilters);
        Assert.Equal($"Showing {cards.Count} of {total} issues", vm.SummaryText);
        // The WIP count still covers the whole column.
        Assert.Equal(total, vm.Columns.Sum(c => c.Count));
        var filtered = vm.Columns.FirstOrDefault(c => c.ShownCount != c.Count);
        if (filtered is not null) Assert.Equal($"{filtered.ShownCount} of {filtered.Count}", filtered.CountText);

        vm.ClearFiltersCommand.Execute(null);
        Assert.False(vm.HasActiveFilters);
        Assert.Equal(total, vm.Columns.Sum(c => c.Cards.Count));
    }

    [Fact]
    public async Task Filters_combine()
    {
        var vm = Create();
        await vm.ActivateAsync(null);
        vm.ToggleFilterCommand.Execute(vm.Filters.Single(f => f.Kind == BoardFilterKind.Mine));
        vm.ToggleFilterCommand.Execute(vm.Filters.Single(f => f.Kind == BoardFilterKind.HighPriority));

        var expected = SprintIssues().Count(i => i.AssigneeId == MockDataSeed.MeId && i.Priority >= IssuePriority.High);
        var cards = vm.Columns.SelectMany(c => c.Cards).ToList();
        Assert.Equal(expected, cards.Count);
        Assert.All(cards, c => Assert.Equal("Maya Chen", c.AssigneeName));
    }

    [Fact]
    public async Task Blocked_badge_counts_blocked_issues_and_the_button_filters_to_them()
    {
        var target = SprintIssues().First(i => !BoardRules.IsBlocked(i));
        var vm = Create();
        await vm.ActivateAsync(null);
        var before = vm.BlockedCount;

        await _w.Issues.UpdateAsync(target, i => i.Labels.Add(BoardRules.BlockedLabel), "added the blocked label", TestContext.Current.CancellationToken);

        Assert.Equal(before + 1, vm.BlockedCount);
        vm.ToggleBlockedCommand.Execute(null);
        Assert.True(vm.ShowBlockedOnly);
        var cards = vm.Columns.SelectMany(c => c.Cards).ToList();
        Assert.Equal(vm.BlockedCount, cards.Count);
        Assert.Contains(cards, c => c.Key == target.Key);
        Assert.All(cards, c => Assert.True(c.IsBlocked));
    }

    [Fact]
    public async Task Moving_a_card_shows_loading_on_the_target_column_then_toasts()
    {
        _w.Latency.Scale = 0.05; // SetStatusAsync waits 25 ms, long enough to see the busy column
        var vm = Create();
        await vm.ActivateAsync(null);
        var card = vm.Columns.SelectMany(c => c.Cards).First(c => c.Status == IssueStatus.Todo);
        var move = card.Actions.Single(a => a.MoveTo == IssueStatus.InReview);

        var task = vm.RunCardActionCommand.ExecuteAsync(move);
        var review = Column(vm, IssueStatus.InReview);
        Assert.True(review.IsBusy);
        Assert.Equal($"Moving {card.Key} to In review", review.BusyText);
        await task;

        Assert.False(review.IsBusy);
        Assert.Equal(IssueStatus.InReview, card.Issue.Status);
        Assert.Contains(review.Cards, c => c.Key == card.Key);
        Assert.DoesNotContain(Column(vm, IssueStatus.Todo).Cards, c => c.Key == card.Key);
        Assert.Equal((FeedbackKind.Success, $"{card.Key} moved to In review", (string?)null), _w.Feedback.Toasts.Single());
    }

    [Fact]
    public async Task Open_issue_action_navigates_to_the_detail()
    {
        var vm = Create();
        await vm.ActivateAsync(null);
        var card = vm.Columns.SelectMany(c => c.Cards).First();
        await vm.RunCardActionCommand.ExecuteAsync(card.Actions.Last());
        Assert.Equal((Routes.IssueDetail, (object?)card.Key), _w.Navigator.History.Last());
    }

    [Fact]
    public async Task Wip_limit_is_flagged_once_a_column_goes_over()
    {
        var vm = Create();
        await vm.ActivateAsync(null);
        var review = Column(vm, IssueStatus.InReview);
        Assert.Equal(5, review.WipLimit);
        Assert.Null(Column(vm, IssueStatus.Todo).WipLimit);
        Assert.Equal(WipState.Unlimited, Column(vm, IssueStatus.Done).Wip);

        // Fill In review up to its limit, then one past it.
        var issues = SprintIssues();
        var inReview = issues.Where(i => i.Status == IssueStatus.InReview).ToList();
        if (inReview.Count > 5) await _w.Issues.SetStatusAsync(inReview.Skip(5), IssueStatus.Todo, TestContext.Current.CancellationToken);
        else if (inReview.Count < 5) await _w.Issues.SetStatusAsync(issues.Where(i => i.Status != IssueStatus.InReview).Take(5 - inReview.Count), IssueStatus.InReview, TestContext.Current.CancellationToken);
        Assert.Equal(WipState.AtLimit, review.Wip);
        Assert.False(review.IsOverLimit);
        Assert.Equal("At WIP limit: 5", review.WipText);

        await _w.Issues.SetStatusAsync(SprintIssues().Where(i => i.Status != IssueStatus.InReview).Take(1), IssueStatus.InReview, TestContext.Current.CancellationToken);
        Assert.True(review.IsOverLimit);
        Assert.Equal("Over WIP limit: 6 of 5", review.WipText);
    }

    [Fact]
    public async Task Select_mode_moves_the_selected_cards_together()
    {
        var vm = Create();
        await vm.ActivateAsync(null);
        Assert.False(vm.MoveSelectedCommand.CanExecute(IssueStatus.Done));

        vm.IsSelectMode = true;
        var cards = vm.Columns.SelectMany(c => c.Cards).ToList();
        Assert.All(cards, c => Assert.True(c.IsSelectMode));
        var picked = cards.Where(c => c.Status != IssueStatus.Done).Take(2).ToList();
        foreach (var c in picked) c.IsSelected = true;
        Assert.Equal(2, vm.Selected.Count);
        Assert.Equal("2 cards selected", vm.SelectionText);
        Assert.True(vm.MoveSelectedCommand.CanExecute(IssueStatus.Done));

        await vm.MoveSelectedCommand.ExecuteAsync(IssueStatus.Done);

        Assert.All(picked, c => Assert.Equal(IssueStatus.Done, c.Issue.Status));
        Assert.Empty(vm.Selected);
        Assert.Contains(_w.Feedback.Toasts, t => t.Title == "2 issues moved to Done");
        Assert.True(vm.IsSelectMode);
    }

    [Fact]
    public async Task Selection_survives_refreshes_and_leaving_select_mode_clears_it()
    {
        var vm = Create();
        await vm.ActivateAsync(null);
        vm.IsSelectMode = true;
        var card = vm.Columns.SelectMany(c => c.Cards).First();
        card.IsSelected = true;

        // An unrelated change rebuilds the cards; the selection is kept by key.
        var other = SprintIssues().First(i => i.Key != card.Key);
        await _w.Issues.UpdateAsync(other, i => i.Title += " (edited)", "edited the title", TestContext.Current.CancellationToken);
        Assert.Equal(card.Key, Assert.Single(vm.Selected).Key);
        Assert.True(vm.Columns.SelectMany(c => c.Cards).Single(c => c.Key == card.Key).IsSelected);

        vm.IsSelectMode = false;
        Assert.Empty(vm.Selected);
        Assert.All(vm.Columns.SelectMany(c => c.Cards), c => Assert.False(c.IsSelectMode));
    }

    [Fact]
    public async Task Filtering_a_selected_card_out_deselects_it()
    {
        var vm = Create();
        await vm.ActivateAsync(null);
        vm.IsSelectMode = true;
        var card = vm.Columns.SelectMany(c => c.Cards).First(c => c.Type != IssueType.Bug);
        card.IsSelected = true;
        vm.ToggleFilterCommand.Execute(vm.Filters.Single(f => f.Kind == BoardFilterKind.Bugs));
        Assert.Empty(vm.Selected);
    }

    [Fact]
    public async Task Project_without_an_active_sprint_shows_the_empty_state()
    {
        var vm = Create();
        await vm.ActivateAsync(null);

        _w.Workspace.SelectProject("ins");

        Assert.False(vm.HasActiveSprint);
        Assert.True(vm.IsEmpty);
        Assert.False(vm.ShowBoard);
        Assert.All(vm.Columns, c => Assert.Empty(c.Cards));
        Assert.Contains("Backlog and sprints", vm.EmptyText, StringComparison.Ordinal);

        await vm.GoToBacklogCommand.ExecuteAsync(null);
        Assert.Equal(Routes.Backlog, _w.Navigator.CurrentRoute);
    }

    [Fact]
    public async Task Completing_the_sprint_empties_the_board()
    {
        var vm = Create();
        await vm.ActivateAsync(null);
        await _w.Sprints.CompleteAsync(Active, TestContext.Current.CancellationToken);
        Assert.True(vm.IsEmpty);
        Assert.Contains("planned sprint", vm.EmptyText, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Leaving_the_screen_stops_listening()
    {
        var vm = Create();
        await vm.ActivateAsync(null);
        var count = vm.TotalCount;
        vm.Deactivate();
        _w.Workspace.SelectProject("ins");
        Assert.Equal(count, vm.TotalCount);
        Assert.True(vm.HasActiveSprint);
    }
}

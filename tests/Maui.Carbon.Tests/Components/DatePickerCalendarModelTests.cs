using System.Globalization;

namespace Maui.Carbon.Tests.Components;

public class DatePickerCalendarModelTests
{
    // March 2026 starts on a Sunday; today is Wednesday 18 March 2026.
    static readonly DateOnly Today = new(2026, 3, 18);

    static CultureInfo Culture(DayOfWeek firstDay)
    {
        var culture = (CultureInfo)CultureInfo.InvariantCulture.Clone();
        culture.DateTimeFormat.FirstDayOfWeek = firstDay;
        return culture;
    }

    static CalendarModel Model(DayOfWeek firstDay = DayOfWeek.Sunday, bool range = false) =>
        new(Culture(firstDay), Today, range);

    [Fact]
    public void The_grid_is_six_weeks_of_seven_days_starting_on_the_first_day_of_the_week()
    {
        var grid = Model().BuildGrid();
        Assert.Equal(42, grid.Count);
        Assert.Equal(new DateOnly(2026, 3, 1), grid[0].Date); // Sunday start, March 1 is a Sunday
        Assert.Equal(new DateOnly(2026, 4, 11), grid[41].Date);
        Assert.All(grid.Take(31), d => Assert.True(d.InMonth));
        Assert.All(grid.Skip(31), d => Assert.False(d.InMonth));
    }

    [Fact]
    public void A_monday_week_start_adds_leading_days_from_the_previous_month()
    {
        var grid = Model(DayOfWeek.Monday).BuildGrid();
        Assert.Equal(new DateOnly(2026, 2, 23), grid[0].Date);
        Assert.Equal(DayOfWeek.Monday, grid[0].Date.DayOfWeek);
        Assert.All(grid.Take(6), d => Assert.False(d.InMonth));
        Assert.True(grid[6].InMonth);
        Assert.Equal(new DateOnly(2026, 3, 1), grid[6].Date);
    }

    [Theory]
    [InlineData(DayOfWeek.Sunday)]
    [InlineData(DayOfWeek.Monday)]
    [InlineData(DayOfWeek.Saturday)]
    public void The_first_column_is_always_the_cultures_first_day(DayOfWeek first)
    {
        var model = Model(first);
        Assert.Equal(first, model.FirstDayOfWeek);
        Assert.Equal(first, model.BuildGrid()[0].Date.DayOfWeek);
        Assert.Equal(Culture(first).DateTimeFormat.GetShortestDayName(first), model.WeekdayLabels()[0]);
        Assert.Equal(7, model.WeekdayLabels().Count);
    }

    [Fact]
    public void Weekday_labels_follow_the_week_start()
    {
        var sunday = Model().WeekdayLabels();
        var monday = Model(DayOfWeek.Monday).WeekdayLabels();
        Assert.Equal(sunday[1], monday[0]);
        Assert.Equal(sunday[0], monday[6]);
    }

    [Fact]
    public void Showing_a_month_leaves_the_grid_covering_the_whole_month_in_every_case()
    {
        foreach (var month in Enumerable.Range(1, 12))
        {
            var model = Model(DayOfWeek.Monday);
            model.Show(new DateOnly(2026, month, 15));
            var inMonth = model.BuildGrid().Count(d => d.InMonth);
            Assert.Equal(DateTime.DaysInMonth(2026, month), inMonth);
        }
    }

    [Fact]
    public void Only_today_is_flagged_as_today()
    {
        var grid = Model().BuildGrid();
        Assert.Equal([Today], grid.Where(d => d.IsToday).Select(d => d.Date));
    }

    [Fact]
    public void The_month_label_names_the_visible_month()
    {
        var model = Model();
        model.Show(new DateOnly(2027, 11, 3));
        Assert.Equal(new DateOnly(2027, 11, 1), model.VisibleMonth);
        Assert.Equal(new DateOnly(2027, 11, 1).ToString("Y", model.Culture), model.MonthLabel);
    }

    // ---- limits ----

    [Fact]
    public void Days_outside_min_and_max_and_on_the_predicate_are_disabled()
    {
        var model = Model();
        model.Min = new DateOnly(2026, 3, 5);
        model.Max = new DateOnly(2026, 3, 25);
        model.IsDisabledDate = d => d.DayOfWeek == DayOfWeek.Sunday;
        var grid = model.BuildGrid().ToDictionary(d => d.Date);
        Assert.True(grid[new DateOnly(2026, 3, 4)].IsDisabled);
        Assert.False(grid[new DateOnly(2026, 3, 5)].IsDisabled);
        Assert.False(grid[new DateOnly(2026, 3, 25)].IsDisabled);
        Assert.True(grid[new DateOnly(2026, 3, 26)].IsDisabled);
        Assert.True(grid[new DateOnly(2026, 3, 8)].IsDisabled); // a Sunday
        Assert.False(grid[new DateOnly(2026, 3, 9)].IsDisabled);
    }

    [Fact]
    public void Clamp_pulls_a_date_into_the_limits()
    {
        var model = Model();
        model.Min = new DateOnly(2026, 3, 5);
        model.Max = new DateOnly(2026, 3, 25);
        Assert.Equal(new DateOnly(2026, 3, 5), model.Clamp(new DateOnly(2026, 1, 1)));
        Assert.Equal(new DateOnly(2026, 3, 25), model.Clamp(new DateOnly(2027, 1, 1)));
        Assert.Equal(new DateOnly(2026, 3, 10), model.Clamp(new DateOnly(2026, 3, 10)));
    }

    [Fact]
    public void Show_clamps_the_cursor_and_follows_it_to_its_month()
    {
        var model = Model();
        model.Min = new DateOnly(2026, 6, 10);
        model.Show(new DateOnly(2026, 3, 1));
        Assert.Equal(new DateOnly(2026, 6, 10), model.Cursor);
        Assert.Equal(new DateOnly(2026, 6, 1), model.VisibleMonth);
    }

    [Fact]
    public void Navigation_stops_at_the_limits()
    {
        var model = Model();
        model.Min = new DateOnly(2026, 3, 10);
        model.Max = new DateOnly(2026, 3, 20);
        model.Show(Today);
        Assert.False(model.CanGoPrevious);
        Assert.False(model.CanGoNext);
        Assert.False(model.GoPrevious());
        Assert.False(model.GoNext());
        Assert.Equal(new DateOnly(2026, 3, 1), model.VisibleMonth);
    }

    [Fact]
    public void Navigation_is_open_without_limits_and_moves_the_cursor_by_a_month()
    {
        var model = Model();
        model.Show(new DateOnly(2026, 1, 31));
        Assert.True(model.GoNext());
        Assert.Equal(new DateOnly(2026, 2, 28), model.Cursor); // day of month clamped
        Assert.Equal(new DateOnly(2026, 2, 1), model.VisibleMonth);
        Assert.True(model.GoPrevious());
        Assert.Equal(new DateOnly(2026, 1, 28), model.Cursor);
        Assert.Equal(new DateOnly(2026, 1, 1), model.VisibleMonth);
    }

    [Fact]
    public void Navigation_crosses_year_boundaries()
    {
        var model = Model();
        model.Show(new DateOnly(2026, 12, 20));
        model.GoNext();
        Assert.Equal(new DateOnly(2027, 1, 20), model.Cursor);
        model.GoPrevious();
        model.GoPrevious();
        Assert.Equal(new DateOnly(2026, 11, 20), model.Cursor);
    }

    [Fact]
    public void Going_next_is_allowed_while_the_next_month_starts_inside_max()
    {
        var model = Model();
        model.Max = new DateOnly(2026, 4, 1);
        model.Show(Today);
        Assert.True(model.CanGoNext);
        Assert.True(model.GoNext());
        Assert.False(model.CanGoNext);
    }

    // ---- keyboard ----

    [Theory]
    [InlineData(CdsKey.Left, 2026, 3, 17)]
    [InlineData(CdsKey.Right, 2026, 3, 19)]
    [InlineData(CdsKey.Up, 2026, 3, 11)]
    [InlineData(CdsKey.Down, 2026, 3, 25)]
    [InlineData(CdsKey.PageUp, 2026, 2, 18)]
    [InlineData(CdsKey.PageDown, 2026, 4, 18)]
    public void Arrows_move_by_day_and_week_and_page_keys_by_month(CdsKey key, int y, int m, int d)
    {
        var model = Model();
        model.Show(Today);
        Assert.True(model.MoveCursor(key));
        Assert.Equal(new DateOnly(y, m, d), model.Cursor);
        Assert.Equal(new DateOnly(y, m, 1), model.VisibleMonth);
    }

    [Fact]
    public void Page_keys_clamp_the_day_of_the_month()
    {
        var model = Model();
        model.Show(new DateOnly(2026, 3, 31));
        model.MoveCursor(CdsKey.PageUp);
        Assert.Equal(new DateOnly(2026, 2, 28), model.Cursor);
        model.MoveCursor(CdsKey.PageDown);
        Assert.Equal(new DateOnly(2026, 3, 28), model.Cursor);
    }

    [Fact]
    public void Arrows_cross_month_boundaries_and_the_view_follows()
    {
        var model = Model();
        model.Show(new DateOnly(2026, 3, 31));
        model.MoveCursor(CdsKey.Right);
        Assert.Equal(new DateOnly(2026, 4, 1), model.Cursor);
        Assert.Equal(new DateOnly(2026, 4, 1), model.VisibleMonth);
        model.MoveCursor(CdsKey.Left);
        Assert.Equal(new DateOnly(2026, 3, 31), model.Cursor);
        Assert.Equal(new DateOnly(2026, 3, 1), model.VisibleMonth);
    }

    [Theory]
    [InlineData(DayOfWeek.Sunday)]
    [InlineData(DayOfWeek.Monday)]
    public void Home_and_end_go_to_the_first_and_last_day_of_the_month(DayOfWeek first)
    {
        // CA-27: the reference calendar moves Home to the 1st and End to the month's last day, whatever the week start.
        var model = Model(first);
        model.Show(Today);
        Assert.True(model.MoveCursor(CdsKey.Home));
        Assert.Equal(new DateOnly(2026, 3, 1), model.Cursor);
        Assert.True(model.MoveCursor(CdsKey.End));
        Assert.Equal(new DateOnly(2026, 3, 31), model.Cursor);
        Assert.Equal(new DateOnly(2026, 3, 1), model.VisibleMonth);
    }

    [Fact]
    public void End_knows_short_months_and_leap_years()
    {
        var model = Model();
        model.Show(new DateOnly(2028, 2, 10));
        model.MoveCursor(CdsKey.End);
        Assert.Equal(new DateOnly(2028, 2, 29), model.Cursor);
        model.Show(new DateOnly(2026, 2, 10));
        model.MoveCursor(CdsKey.End);
        Assert.Equal(new DateOnly(2026, 2, 28), model.Cursor);
        model.MoveCursor(CdsKey.Home);
        Assert.Equal(new DateOnly(2026, 2, 1), model.Cursor);
    }

    [Fact]
    public void Home_and_end_are_clamped_into_min_and_max()
    {
        var model = Model();
        model.Min = new DateOnly(2026, 3, 10);
        model.Max = new DateOnly(2026, 3, 20);
        model.Show(Today);
        model.MoveCursor(CdsKey.Home);
        Assert.Equal(new DateOnly(2026, 3, 10), model.Cursor);
        model.MoveCursor(CdsKey.End);
        Assert.Equal(new DateOnly(2026, 3, 20), model.Cursor);
    }

    // ---- CA-28: what the screen reader hears for the cursor day ----

    [Fact]
    public void The_cursor_announcement_is_the_full_date_with_its_state()
    {
        var model = Model();
        model.Show(Today);
        Assert.Equal(Today.ToString("D", model.Culture), model.CursorAnnouncement());
        model.SetSelection(Today, null);
        Assert.Equal(Today.ToString("D", model.Culture) + ", selected", model.CursorAnnouncement());
        model.IsDisabledDate = d => d.Day == 19;
        var next = Today.AddDays(1);
        Assert.Equal(next.ToString("D", model.Culture) + ", unavailable", model.DayAnnouncement(next));
    }

    [Fact]
    public void Both_ends_of_a_range_are_announced_as_selected()
    {
        var model = Model(range: true);
        model.SetSelection(new DateOnly(2026, 3, 10), new DateOnly(2026, 3, 14));
        Assert.EndsWith(", selected", model.DayAnnouncement(new DateOnly(2026, 3, 10)));
        Assert.EndsWith(", selected", model.DayAnnouncement(new DateOnly(2026, 3, 14)));
        Assert.DoesNotContain("selected", model.DayAnnouncement(new DateOnly(2026, 3, 12)));
    }

    [Fact]
    public void The_cursor_never_leaves_min_and_max()
    {
        var model = Model();
        model.Min = new DateOnly(2026, 3, 17);
        model.Max = new DateOnly(2026, 3, 19);
        model.Show(Today);
        model.MoveCursor(CdsKey.Left);
        model.MoveCursor(CdsKey.Left);
        Assert.Equal(new DateOnly(2026, 3, 17), model.Cursor);
        model.MoveCursor(CdsKey.PageDown);
        Assert.Equal(new DateOnly(2026, 3, 19), model.Cursor);
        model.MoveCursor(CdsKey.Down);
        Assert.Equal(new DateOnly(2026, 3, 19), model.Cursor);
        model.MoveCursor(CdsKey.End);
        Assert.Equal(new DateOnly(2026, 3, 19), model.Cursor);
    }

    [Fact]
    public void Other_keys_are_not_calendar_keys()
    {
        var model = Model();
        model.Show(Today);
        Assert.False(model.MoveCursor(CdsKey.Enter));
        Assert.False(model.MoveCursor(CdsKey.Tab));
        Assert.False(model.MoveCursor(CdsKey.Escape));
        Assert.Equal(Today, model.Cursor);
    }

    [Fact]
    public void The_cursor_starts_on_the_selection_or_today()
    {
        var model = Model();
        model.ShowSelectionOrToday();
        Assert.Equal(Today, model.Cursor);
        model.SetSelection(new DateOnly(2026, 7, 4), null);
        model.ShowSelectionOrToday();
        Assert.Equal(new DateOnly(2026, 7, 4), model.Cursor);
        Assert.Equal(new DateOnly(2026, 7, 1), model.VisibleMonth);
    }

    // ---- selection ----

    [Fact]
    public void Picking_in_single_mode_selects_one_date()
    {
        var model = Model();
        Assert.Equal(CalendarPick.Completed, model.Pick(new DateOnly(2026, 3, 10)));
        Assert.Equal(new DateOnly(2026, 3, 10), model.Start);
        Assert.Null(model.End);
        Assert.Equal(CalendarPick.Completed, model.Pick(new DateOnly(2026, 3, 12)));
        Assert.Equal(new DateOnly(2026, 3, 12), model.Start);
        Assert.Equal([new DateOnly(2026, 3, 12)], model.BuildGrid().Where(d => d.IsSelected).Select(d => d.Date));
    }

    [Fact]
    public void A_range_takes_two_picks_and_a_third_starts_again()
    {
        var model = Model(range: true);
        Assert.Equal(CalendarPick.RangeStarted, model.Pick(new DateOnly(2026, 3, 10)));
        Assert.Equal(new DateOnly(2026, 3, 10), model.Start);
        Assert.Null(model.End);
        Assert.Equal(CalendarPick.Completed, model.Pick(new DateOnly(2026, 3, 14)));
        Assert.Equal(new DateOnly(2026, 3, 14), model.End);
        Assert.Equal(CalendarPick.RangeStarted, model.Pick(new DateOnly(2026, 3, 20)));
        Assert.Equal(new DateOnly(2026, 3, 20), model.Start);
        Assert.Null(model.End);
    }

    [Fact]
    public void An_earlier_second_pick_swaps_start_and_end()
    {
        var model = Model(range: true);
        model.Pick(new DateOnly(2026, 3, 14));
        model.Pick(new DateOnly(2026, 3, 10));
        Assert.Equal(new DateOnly(2026, 3, 10), model.Start);
        Assert.Equal(new DateOnly(2026, 3, 14), model.End);
    }

    [Fact]
    public void Picking_the_same_day_twice_makes_a_one_day_range()
    {
        var model = Model(range: true);
        model.Pick(new DateOnly(2026, 3, 10));
        Assert.Equal(CalendarPick.Completed, model.Pick(new DateOnly(2026, 3, 10)));
        Assert.Equal(model.Start, model.End);
    }

    [Fact]
    public void Disabled_days_cannot_be_picked()
    {
        var model = Model(range: true);
        model.Min = new DateOnly(2026, 3, 5);
        Assert.Equal(CalendarPick.Ignored, model.Pick(new DateOnly(2026, 3, 1)));
        Assert.Null(model.Start);
        model.Pick(new DateOnly(2026, 3, 6));
        Assert.Equal(CalendarPick.Ignored, model.Pick(new DateOnly(2026, 3, 2)));
        Assert.Null(model.End);
    }

    [Fact]
    public void A_complete_range_marks_its_ends_and_the_days_between()
    {
        var model = Model(range: true);
        model.SetSelection(new DateOnly(2026, 3, 10), new DateOnly(2026, 3, 13));
        var grid = model.BuildGrid().ToDictionary(d => d.Date);
        Assert.True(grid[new DateOnly(2026, 3, 10)].IsRangeStart);
        Assert.True(grid[new DateOnly(2026, 3, 10)].IsSelected);
        Assert.True(grid[new DateOnly(2026, 3, 13)].IsRangeEnd);
        Assert.True(grid[new DateOnly(2026, 3, 13)].IsSelected);
        Assert.True(grid[new DateOnly(2026, 3, 11)].InRange);
        Assert.True(grid[new DateOnly(2026, 3, 12)].InRange);
        Assert.False(grid[new DateOnly(2026, 3, 10)].InRange);
        Assert.False(grid[new DateOnly(2026, 3, 13)].InRange);
        Assert.False(grid[new DateOnly(2026, 3, 14)].InRange);
    }

    [Fact]
    public void An_open_range_selects_only_its_start()
    {
        var model = Model(range: true);
        model.SetSelection(new DateOnly(2026, 3, 10), null);
        var grid = model.BuildGrid();
        Assert.Single(grid, d => d.IsSelected);
        Assert.DoesNotContain(grid, d => d.InRange || d.IsRangeStart || d.IsRangeEnd);
    }

    [Fact]
    public void Setting_a_reversed_range_orders_it_and_a_single_ignores_the_end()
    {
        var range = Model(range: true);
        range.SetSelection(new DateOnly(2026, 3, 14), new DateOnly(2026, 3, 10));
        Assert.Equal(new DateOnly(2026, 3, 10), range.Start);
        Assert.Equal(new DateOnly(2026, 3, 14), range.End);

        var single = Model();
        single.SetSelection(new DateOnly(2026, 3, 14), new DateOnly(2026, 3, 20));
        Assert.Null(single.End);
        Assert.Single(single.BuildGrid(), d => d.IsSelected);
    }

    [Fact]
    public void Picking_the_cursor_picks_the_day_under_the_keyboard()
    {
        var model = Model();
        model.Show(Today);
        model.MoveCursor(CdsKey.Right);
        Assert.Equal(CalendarPick.Completed, model.PickCursor());
        Assert.Equal(new DateOnly(2026, 3, 19), model.Start);
    }

    [Fact]
    public void Selection_in_a_neighbouring_month_is_flagged_in_the_grid()
    {
        var model = Model();
        model.SetSelection(new DateOnly(2026, 4, 2), null);
        model.Show(Today);
        var cell = model.BuildGrid().Single(d => d.Date == new DateOnly(2026, 4, 2));
        Assert.True(cell.IsSelected);
        Assert.False(cell.InMonth);
    }
}

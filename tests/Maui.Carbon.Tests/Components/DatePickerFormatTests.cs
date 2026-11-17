namespace Maui.Carbon.Tests.Components;

public class DatePickerFormatTests
{
    [Theory]
    [InlineData("m/d/Y", "MM\\/dd\\/yyyy")]
    [InlineData("d.m.Y", "dd\\.MM\\.yyyy")]
    [InlineData("n/j/y", "M\\/d\\/yy")]
    [InlineData("Y-m-d", "yyyy\\-MM\\-dd")]
    [InlineData(null, "MM\\/dd\\/yyyy")]
    [InlineData("", "MM\\/dd\\/yyyy")]
    public void Flatpickr_tokens_map_to_net_format(string? flatpickr, string net) =>
        Assert.Equal(net, DatePickerFormat.ToNetFormat(flatpickr));

    [Fact]
    public void A_lone_token_is_forced_to_a_custom_format()
    {
        Assert.Equal("%M", DatePickerFormat.ToNetFormat("n"));
        Assert.Equal("%d", DatePickerFormat.ToNetFormat("j"));
    }

    [Theory]
    [InlineData("m/d/Y", "mm/dd/yyyy")]
    [InlineData("n/j/y", "m/d/yy")]
    [InlineData("d.m.Y", "dd.mm.yyyy")]
    [InlineData(null, "mm/dd/yyyy")]
    public void The_placeholder_is_derived_from_the_format(string? format, string placeholder) =>
        Assert.Equal(placeholder, DatePickerFormat.Placeholder(format));

    [Theory]
    [InlineData("m/d/Y", "03/04/2026")]
    [InlineData("d.m.Y", "04.03.2026")]
    [InlineData("n/j/y", "3/4/26")]
    [InlineData("Y-m-d", "2026-03-04")]
    public void Dates_format_with_the_chosen_tokens(string format, string text) =>
        Assert.Equal(text, DatePickerFormat.Format(new DateOnly(2026, 3, 4), format));

    [Theory]
    [InlineData("m/d/Y", "03/04/2026", 2026, 3, 4)]
    [InlineData("m/d/Y", "3/4/2026", 2026, 3, 4)] // lenient: one digit is fine
    [InlineData("m/d/Y", "  12/31/2025  ", 2025, 12, 31)]
    [InlineData("d.m.Y", "04.03.2026", 2026, 3, 4)]
    [InlineData("Y-m-d", "2026-03-04", 2026, 3, 4)]
    [InlineData("m/d/Y", "02/29/2024", 2024, 2, 29)]
    public void Typed_dates_parse(string format, string text, int y, int m, int d)
    {
        Assert.True(DatePickerFormat.TryParse(text, format, out var date));
        Assert.Equal(new DateOnly(y, m, d), date);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("abc")]
    [InlineData("13/01/2026")]
    [InlineData("02/30/2026")]
    [InlineData("02/29/2025")]
    [InlineData("03-04-2026")]
    [InlineData("3/4/26")]
    public void Text_that_is_not_a_date_does_not_parse(string text) =>
        Assert.False(DatePickerFormat.TryParse(text, "m/d/Y", out _));

    [Fact]
    public void Empty_text_is_valid_and_gives_no_date()
    {
        var (date, error) = DatePickerFormat.Validate("  ", "m/d/Y", null, null, null);
        Assert.Null(date);
        Assert.Equal(CdsDateEntryError.None, error);
    }

    [Fact]
    public void A_valid_date_inside_the_limits_is_accepted()
    {
        var (date, error) = DatePickerFormat.Validate("03/10/2026", "m/d/Y", new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31), null);
        Assert.Equal(new DateOnly(2026, 3, 10), date);
        Assert.Equal(CdsDateEntryError.None, error);
    }

    [Fact]
    public void Limits_are_inclusive_and_violations_are_named()
    {
        var min = new DateOnly(2026, 3, 1);
        var max = new DateOnly(2026, 3, 31);
        Assert.Equal(CdsDateEntryError.None, DatePickerFormat.Validate("03/01/2026", "m/d/Y", min, max, null).Error);
        Assert.Equal(CdsDateEntryError.None, DatePickerFormat.Validate("03/31/2026", "m/d/Y", min, max, null).Error);
        Assert.Equal(CdsDateEntryError.BeforeMin, DatePickerFormat.Validate("02/28/2026", "m/d/Y", min, max, null).Error);
        Assert.Equal(CdsDateEntryError.AfterMax, DatePickerFormat.Validate("04/01/2026", "m/d/Y", min, max, null).Error);
        Assert.Equal(CdsDateEntryError.Unparseable, DatePickerFormat.Validate("soon", "m/d/Y", min, max, null).Error);
    }

    [Fact]
    public void Disabled_days_are_rejected()
    {
        var (date, error) = DatePickerFormat.Validate("03/08/2026", "m/d/Y", null, null, d => d.DayOfWeek == DayOfWeek.Sunday);
        Assert.Null(date);
        Assert.Equal(CdsDateEntryError.Disabled, error);
    }

    [Fact]
    public void Messages_name_the_problem_in_the_users_format()
    {
        Assert.Equal("Enter a date as mm/dd/yyyy", DatePickerFormat.Message(CdsDateEntryError.Unparseable, "m/d/Y", null, null));
        Assert.Equal("Choose 03/01/2026 or later", DatePickerFormat.Message(CdsDateEntryError.BeforeMin, "m/d/Y", new DateOnly(2026, 3, 1), null));
        Assert.Equal("Choose 31.03.2026 or earlier", DatePickerFormat.Message(CdsDateEntryError.AfterMax, "d.m.Y", null, new DateOnly(2026, 3, 31)));
        Assert.NotEmpty(DatePickerFormat.Message(CdsDateEntryError.Disabled, "m/d/Y", null, null));
        Assert.NotEmpty(DatePickerFormat.Message(CdsDateEntryError.EndBeforeStart, "m/d/Y", null, null));
        Assert.Empty(DatePickerFormat.Message(CdsDateEntryError.None, "m/d/Y", null, null));
    }

    [Fact]
    public void Dates_convert_between_date_time_and_date_only()
    {
        Assert.Equal(new DateOnly(2026, 3, 4), DatePickerFormat.ToDate(new DateTime(2026, 3, 4, 17, 30, 0)));
        Assert.Null(DatePickerFormat.ToDate(null));
        Assert.Equal(new DateTime(2026, 3, 4), DatePickerFormat.ToDateTime(new DateOnly(2026, 3, 4)));
        Assert.Null(DatePickerFormat.ToDateTime(null));
    }

    // ---- calendar tokens ----

    [Fact]
    public void An_ordinary_day_is_primary_text_with_a_hover_fill()
    {
        Assert.Equal(new CdsCalendarDayVisuals(null, CdsColorKeys.TextPrimary, null, null, false),
            CdsCalendarTokens.Resolve(true, false, false, false, false, false, false));
        Assert.Equal(CdsColorKeys.LayerHover01, CdsCalendarTokens.Resolve(true, false, false, false, false, true, false).Background);
    }

    [Fact]
    public void Neighbouring_month_days_use_helper_text()
    {
        Assert.Equal(CdsColorKeys.TextHelper, CdsCalendarTokens.Resolve(false, false, false, false, false, false, false).Text);
    }

    [Fact]
    public void Today_is_a_semibold_link_color_with_a_dot()
    {
        var v = CdsCalendarTokens.Resolve(true, true, false, false, false, false, false);
        Assert.Equal(CdsColorKeys.LinkPrimary, v.Text);
        Assert.Equal(CdsColorKeys.LinkPrimary, v.Dot);
        Assert.True(v.IsSemiBold);
    }

    [Fact]
    public void The_selected_day_is_filled_with_button_primary()
    {
        var v = CdsCalendarTokens.Resolve(true, false, false, true, false, true, false);
        Assert.Equal(CdsColorKeys.ButtonPrimary, v.Background); // hover does not change it
        Assert.Equal(CdsColorKeys.TextOnColor, v.Text);
        Assert.Null(v.Dot);
        Assert.Null(v.Ring);
    }

    [Fact]
    public void Today_selected_gets_a_focus_ring_and_no_dot()
    {
        var v = CdsCalendarTokens.Resolve(true, true, false, true, false, false, false);
        Assert.Equal(CdsColorKeys.Focus, v.Ring);
        Assert.Null(v.Dot);
    }

    [Fact]
    public void Days_inside_a_range_use_the_highlight_and_ignore_hover()
    {
        var v = CdsCalendarTokens.Resolve(true, false, false, false, true, true, false);
        Assert.Equal(CdsColorKeys.Highlight, v.Background);
        Assert.Equal(CdsColorKeys.TextPrimary, v.Text);
    }

    [Fact]
    public void Disabled_days_are_dim_and_never_hover()
    {
        var v = CdsCalendarTokens.Resolve(true, false, true, false, false, true, false);
        Assert.Null(v.Background);
        Assert.Equal(CdsColorKeys.TextDisabled, v.Text);
    }

    [Fact]
    public void The_keyboard_cursor_draws_a_button_primary_ring_and_a_focus_ring_on_a_selected_day()
    {
        Assert.Equal(CdsColorKeys.ButtonPrimary, CdsCalendarTokens.Resolve(true, false, false, false, false, false, true).Ring);
        Assert.Equal(CdsColorKeys.Focus, CdsCalendarTokens.Resolve(true, false, false, true, false, false, true).Ring);
    }

    [Fact]
    public void Navigation_buttons_dim_at_the_limit_and_fill_on_hover()
    {
        Assert.Equal((null, CdsColorKeys.IconPrimary), CdsCalendarTokens.ResolveNavigation(true, false));
        Assert.Equal((CdsColorKeys.LayerHover01, CdsColorKeys.IconPrimary), CdsCalendarTokens.ResolveNavigation(true, true));
        Assert.Equal((null, CdsColorKeys.IconDisabled), CdsCalendarTokens.ResolveNavigation(false, true));
    }

    [Theory]
    [InlineData(CdsDatePickerType.Single, false, false, 288)]
    [InlineData(CdsDatePickerType.Range, false, false, 143.5)]
    [InlineData(CdsDatePickerType.Simple, false, false, 120)]
    [InlineData(CdsDatePickerType.Simple, false, true, 152)]
    [InlineData(CdsDatePickerType.Simple, true, false, 91.2)]
    public void Field_widths_follow_the_type(CdsDatePickerType type, bool isShort, bool status, double width) =>
        Assert.Equal(width, CdsCalendarTokens.FieldWidth(type, isShort, status));

    [Fact]
    public void The_calendar_does_not_grow_with_the_field_size()
    {
        Assert.Equal(288, CdsCalendarTokens.Width);
        Assert.Equal(336, CdsCalendarTokens.Height);
    }
}

using System.Globalization;

namespace Maui.Carbon.Tests.Components;

public class DatePickerTests
{
    sealed class TestPicker : CdsDatePicker
    {
        public CdsNativeEntry Start => StartEntry;

        public CdsNativeEntry End => EndEntry;

        public bool Invalid => EffectiveInvalid;

        public string? InvalidMessage => EffectiveInvalidText;

        public void CompleteStart() => ((IEntry)StartEntry).Completed();

        public void CompleteEnd() => ((IEntry)EndEntry).Completed();

        public bool EndChromeVisible => EndChrome.IsVisible;

        public double StartWidth => Chrome.WidthRequest;
    }

    static TestPicker Make(CdsDatePickerType type = CdsDatePickerType.Single) =>
        new() { Label = "Snapshot date", Type = type };

    [Fact]
    public void Constructing_and_setting_every_property_does_not_throw()
    {
        var picker = new CdsDatePicker
        {
            Label = "From",
            EndLabel = "To",
            HideLabel = true,
            Placeholder = "mm/dd/yyyy",
            EndPlaceholder = "mm/dd/yyyy",
            HelperText = "h",
            IsInvalid = true,
            InvalidText = "e",
            IsWarning = true,
            WarnText = "w",
            IsReadOnly = true,
            IsEnabled = false,
            Size = CdsSize.Sm,
            Type = CdsDatePickerType.Range,
            DateFormat = "d.m.Y",
            Locale = "de-DE",
            MinDate = new DateTime(2026, 1, 1),
            MaxDate = new DateTime(2026, 12, 31),
            IsDateDisabled = d => d.DayOfWeek == DayOfWeek.Sunday,
            IsShort = true,
            StartDate = new DateTime(2026, 3, 4),
            EndDate = new DateTime(2026, 3, 9),
            IsOpen = true,
        };
        Assert.Equal(new DateTime(2026, 3, 4), picker.Value);
        Assert.False(picker.IsOpen);
    }

    [Fact]
    public void Value_and_start_date_are_the_same_date_and_drop_the_time()
    {
        var picker = Make();
        picker.Value = new DateTime(2026, 3, 4, 15, 45, 0);
        Assert.Equal(new DateTime(2026, 3, 4), picker.StartDate);
        Assert.Equal(new DateTime(2026, 3, 4), picker.Value);
        picker.StartDate = new DateTime(2026, 5, 6, 1, 2, 3);
        Assert.Equal(new DateTime(2026, 5, 6), picker.Value);
        picker.Value = null;
        Assert.Null(picker.StartDate);
    }

    [Fact]
    public void Setting_a_date_fills_the_field_in_the_format()
    {
        var picker = Make();
        picker.StartDate = new DateTime(2026, 3, 4);
        Assert.Equal("03/04/2026", picker.Start.Text);
        picker.DateFormat = "d.m.Y";
        Assert.Equal("04.03.2026", picker.Start.Text);
        picker.DateFormat = "n/j/y";
        Assert.Equal("3/4/26", picker.Start.Text);
    }

    [Fact]
    public void A_range_fills_both_fields()
    {
        var picker = Make(CdsDatePickerType.Range);
        picker.StartDate = new DateTime(2026, 3, 4);
        picker.EndDate = new DateTime(2026, 3, 9);
        Assert.Equal("03/04/2026", picker.Start.Text);
        Assert.Equal("03/09/2026", picker.End.Text);
        Assert.True(picker.EndChromeVisible);
    }

    [Fact]
    public void Only_a_range_shows_the_end_field()
    {
        Assert.False(Make().EndChromeVisible);
        Assert.False(Make(CdsDatePickerType.Simple).EndChromeVisible);
        Assert.True(Make(CdsDatePickerType.Range).EndChromeVisible);
    }

    [Theory]
    [InlineData(CdsDatePickerType.Single, 288)]
    [InlineData(CdsDatePickerType.Range, 143.5)]
    [InlineData(CdsDatePickerType.Simple, 120)]
    public void Field_widths_follow_the_type(CdsDatePickerType type, double width) =>
        Assert.Equal(width, Make(type).StartWidth);

    [Fact]
    public void Typed_dates_apply_on_enter_and_reformat_the_text()
    {
        var picker = Make();
        picker.Start.Text = "3/4/2026";
        Assert.Null(picker.StartDate); // not applied until enter or leaving the field
        picker.CompleteStart();
        Assert.Equal(new DateTime(2026, 3, 4), picker.StartDate);
        Assert.Equal("03/04/2026", picker.Start.Text);
        Assert.False(picker.Invalid);
    }

    [Fact]
    public void Typed_dates_raise_date_changed()
    {
        var picker = Make();
        CdsDateChangedEventArgs? args = null;
        picker.DateChanged += (_, e) => args = e;
        picker.Start.Text = "03/04/2026";
        picker.CompleteStart();
        Assert.Equal(new DateTime(2026, 3, 4), args!.StartDate);
        Assert.Null(args.EndDate);
    }

    [Fact]
    public void Text_that_is_not_a_date_is_invalid_with_a_message_and_keeps_the_text()
    {
        var picker = Make();
        picker.StartDate = new DateTime(2026, 3, 4);
        picker.Start.Text = "tomorrow";
        picker.CompleteStart();
        Assert.True(picker.Invalid);
        Assert.Equal("Enter a date as mm/dd/yyyy", picker.InvalidMessage);
        Assert.Equal("tomorrow", picker.Start.Text);
        Assert.Equal(new DateTime(2026, 3, 4), picker.StartDate); // unchanged
    }

    [Fact]
    public void Typing_again_clears_the_error()
    {
        var picker = Make();
        picker.Start.Text = "tomorrow";
        picker.CompleteStart();
        Assert.True(picker.Invalid);
        picker.Start.Text = "03/0";
        Assert.False(picker.Invalid);
    }

    [Fact]
    public void A_valid_date_after_an_error_clears_it()
    {
        var picker = Make();
        picker.Start.Text = "tomorrow";
        picker.CompleteStart();
        picker.Start.Text = "03/04/2026";
        picker.CompleteStart();
        Assert.False(picker.Invalid);
        Assert.Equal(new DateTime(2026, 3, 4), picker.StartDate);
    }

    [Fact]
    public void Dates_outside_min_and_max_are_rejected_with_a_message()
    {
        var picker = Make();
        picker.MinDate = new DateTime(2026, 3, 1);
        picker.MaxDate = new DateTime(2026, 3, 31);
        picker.Start.Text = "02/28/2026";
        picker.CompleteStart();
        Assert.True(picker.Invalid);
        Assert.Equal("Choose 03/01/2026 or later", picker.InvalidMessage);
        picker.Start.Text = "04/01/2026";
        picker.CompleteStart();
        Assert.Equal("Choose 03/31/2026 or earlier", picker.InvalidMessage);
        Assert.Null(picker.StartDate);
    }

    [Fact]
    public void Disabled_dates_are_rejected()
    {
        var picker = Make();
        picker.IsDateDisabled = d => d.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;
        picker.Start.Text = "03/07/2026"; // a Saturday
        picker.CompleteStart();
        Assert.True(picker.Invalid);
        picker.Start.Text = "03/09/2026";
        picker.CompleteStart();
        Assert.False(picker.Invalid);
        Assert.Equal(new DateTime(2026, 3, 9), picker.StartDate);
    }

    [Fact]
    public void Clearing_the_text_clears_the_date()
    {
        var picker = Make();
        picker.StartDate = new DateTime(2026, 3, 4);
        picker.Start.Text = "";
        picker.CompleteStart();
        Assert.Null(picker.StartDate);
        Assert.False(picker.Invalid);
    }

    [Fact]
    public void An_end_date_before_the_start_is_rejected()
    {
        var picker = Make(CdsDatePickerType.Range);
        picker.StartDate = new DateTime(2026, 3, 10);
        picker.End.Text = "03/05/2026";
        picker.CompleteEnd();
        Assert.True(picker.Invalid);
        Assert.Equal("The end date can't be before the start date", picker.InvalidMessage);
        Assert.Null(picker.EndDate);
        picker.End.Text = "03/12/2026";
        picker.CompleteEnd();
        Assert.False(picker.Invalid);
        Assert.Equal(new DateTime(2026, 3, 12), picker.EndDate);
    }

    [Fact]
    public void The_consumers_invalid_flag_and_text_still_work()
    {
        var picker = Make();
        picker.IsInvalid = true;
        picker.InvalidText = "Choose a range of 92 days or less";
        Assert.True(picker.Invalid);
        Assert.Equal("Choose a range of 92 days or less", picker.InvalidMessage);
    }

    [Fact]
    public void Simple_pickers_never_open_a_calendar()
    {
        var picker = Make(CdsDatePickerType.Simple);
        picker.IsOpen = true;
        Assert.False(picker.IsOpen);
    }

    [Fact]
    public void Read_only_disabled_pickers_do_not_open_and_changing_to_simple_closes()
    {
        var readOnly = Make();
        readOnly.IsReadOnly = true;
        readOnly.IsOpen = true;
        Assert.False(readOnly.IsOpen);

        var picker = Make();
        picker.IsOpen = true;
        Assert.True(picker.IsOpen);
        picker.Type = CdsDatePickerType.Simple;
        Assert.False(picker.IsOpen);
    }

    [Fact]
    public void The_locale_gives_the_culture_and_unknown_locales_fall_back()
    {
        var picker = Make();
        Assert.Equal(CultureInfo.CurrentCulture.Name, picker.Culture.Name);
        picker.Locale = "de-DE";
        Assert.Equal(CultureInfo.GetCultureInfo("de-DE").Name, picker.Culture.Name);
        picker.Locale = "??";
        Assert.Equal(CultureInfo.CurrentCulture.Name, picker.Culture.Name);
    }

    [Fact]
    public void Dates_and_is_open_are_two_way_properties()
    {
        Assert.Equal(BindingMode.TwoWay, CdsDatePicker.ValueProperty.DefaultBindingMode);
        Assert.Equal(BindingMode.TwoWay, CdsDatePicker.StartDateProperty.DefaultBindingMode);
        Assert.Equal(BindingMode.TwoWay, CdsDatePicker.EndDateProperty.DefaultBindingMode);
        Assert.Equal(BindingMode.TwoWay, CdsDatePicker.IsOpenProperty.DefaultBindingMode);
    }

    [Fact]
    public void The_skeleton_has_a_single_or_range_layout()
    {
        var skeleton = new CdsDatePickerSkeleton { Range = true, HideLabel = true };
        Assert.True(skeleton.Range);
        Assert.True(skeleton.HideLabel);
        skeleton.Range = false;
        Assert.False(skeleton.Range);
    }
}

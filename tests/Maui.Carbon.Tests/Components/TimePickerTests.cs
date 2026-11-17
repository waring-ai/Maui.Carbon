namespace Maui.Carbon.Tests.Components;

public class TimePickerTests
{
    [Theory]
    [InlineData("1:00", 1, 0)]
    [InlineData("2:30", 2, 30)]
    [InlineData("12:59", 12, 59)]
    [InlineData("10:05", 10, 5)]
    [InlineData("9:15 ", 9, 15)]
    public void ParsesTwelveHourTimes(string text, int hour, int minute)
    {
        Assert.True(TimePickerModel.TryParse(text, true, out var h, out var m));
        Assert.Equal((hour, minute), (h, m));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("0:30")]
    [InlineData("13:00")]
    [InlineData("1:60")]
    [InlineData("1:5")]
    [InlineData("1230")]
    [InlineData("ab:cd")]
    [InlineData("1:30pm")]
    [InlineData("01:30")]
    public void RejectsMalformedTwelveHourTimes(string? text) => Assert.False(TimePickerModel.IsValid(text, true));

    [Theory]
    [InlineData("0:00", 0, 0)]
    [InlineData("00:00", 0, 0)]
    [InlineData("9:05", 9, 5)]
    [InlineData("13:45", 13, 45)]
    [InlineData("23:59", 23, 59)]
    public void ParsesTwentyFourHourTimes(string text, int hour, int minute)
    {
        Assert.True(TimePickerModel.TryParse(text, false, out var h, out var m));
        Assert.Equal((hour, minute), (h, m));
    }

    [Theory]
    [InlineData("24:00")]
    [InlineData("12:60")]
    [InlineData("7")]
    [InlineData("-1:00")]
    public void RejectsMalformedTwentyFourHourTimes(string text) => Assert.False(TimePickerModel.IsValid(text, false));

    [Theory]
    [InlineData("12:00", CdsTimePeriod.Am, 0, 0)]
    [InlineData("12:30", CdsTimePeriod.Pm, 12, 30)]
    [InlineData("1:15", CdsTimePeriod.Am, 1, 15)]
    [InlineData("1:15", CdsTimePeriod.Pm, 13, 15)]
    [InlineData("11:59", CdsTimePeriod.Pm, 23, 59)]
    public void ConvertsTwelveHourToTimeOfDay(string text, CdsTimePeriod period, int hours, int minutes) =>
        Assert.Equal(new TimeSpan(hours, minutes, 0), TimePickerModel.ToTimeOfDay(text, true, period));

    [Fact]
    public void TwentyFourHourIgnoresThePeriod()
    {
        Assert.Equal(new TimeSpan(14, 5, 0), TimePickerModel.ToTimeOfDay("14:05", false, CdsTimePeriod.Am));
        Assert.Null(TimePickerModel.ToTimeOfDay("25:00", false, CdsTimePeriod.Pm));
        Assert.Null(TimePickerModel.ToTimeOfDay("", true, CdsTimePeriod.Pm));
    }

    [Theory]
    [InlineData(0, 0, "12:00", CdsTimePeriod.Am)]
    [InlineData(12, 0, "12:00", CdsTimePeriod.Pm)]
    [InlineData(14, 30, "2:30", CdsTimePeriod.Pm)]
    [InlineData(9, 5, "9:05", CdsTimePeriod.Am)]
    [InlineData(23, 59, "11:59", CdsTimePeriod.Pm)]
    public void FormatsTwelveHour(int hours, int minutes, string text, CdsTimePeriod period) =>
        Assert.Equal((text, period), TimePickerModel.Format(new TimeSpan(hours, minutes, 0), true));

    [Fact]
    public void FormatsTwentyFourHour() =>
        Assert.Equal(("14:05", CdsTimePeriod.Pm), TimePickerModel.Format(new TimeSpan(14, 5, 0), false));

    [Fact]
    public void FormatAndParseRoundTrip()
    {
        for (var minutes = 0; minutes < 24 * 60; minutes += 7)
        {
            var time = TimeSpan.FromMinutes(minutes);
            var (text, period) = TimePickerModel.Format(time, true);
            Assert.Equal(time, TimePickerModel.ToTimeOfDay(text, true, period));
            var (text24, _) = TimePickerModel.Format(time, false);
            Assert.Equal(time, TimePickerModel.ToTimeOfDay(text24, false, CdsTimePeriod.Am));
        }
    }

    [Fact]
    public void ValidateFlagsMalformedTextAndClearsWhenFixed()
    {
        var picker = new CdsTimePicker { Label = "Start time", Text = "25:99" };
        Assert.False(picker.IsTextValid);
        Assert.False(picker.Validate());
        picker.Text = "2:30";
        Assert.True(picker.IsTextValid);
        Assert.True(picker.Validate());
    }

    [Fact]
    public void EmptyTextIsValid()
    {
        var picker = new CdsTimePicker { Label = "Start time" };
        Assert.True(picker.Validate());
    }

    [Fact]
    public void TimeFollowsTextAndPeriod()
    {
        var picker = new CdsTimePicker { Label = "Start time", Text = "2:30" };
        Assert.Equal(new TimeSpan(2, 30, 0), picker.Time);
        picker.Period = CdsTimePeriod.Pm;
        Assert.Equal(new TimeSpan(14, 30, 0), picker.Time);
        picker.Text = "nope";
        Assert.Null(picker.Time);
    }

    [Fact]
    public void SettingTimeWritesTextAndPeriod()
    {
        var picker = new CdsTimePicker { Label = "Start time" };
        picker.Time = new TimeSpan(18, 5, 0);
        Assert.Equal("6:05", picker.Text);
        Assert.Equal(CdsTimePeriod.Pm, picker.Period);
        picker.Is24Hour = true;
        picker.Time = new TimeSpan(18, 6, 0);
        Assert.Equal("18:06", picker.Text);
    }

    [Fact]
    public void ControlSetsEveryPropertyWithoutThrowing()
    {
        var picker = new CdsTimePicker
        {
            Label = "Start time", HideLabel = true, Placeholder = "hh:mm", IsInvalid = true, InvalidText = "Use hh:mm", IsWarning = true,
            WarnText = "Late", IsReadOnly = true, Size = CdsSize.Sm, Is24Hour = true,
        };
        picker.TimeZones.Add("UTC");
        picker.TimeZones.Add("CET");
        picker.SelectedTimeZone = "CET";
        picker.IsEnabled = false;
        Assert.Equal("CET", picker.SelectedTimeZone);
    }
}

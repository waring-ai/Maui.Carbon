namespace Maui.Carbon.Tests.Components;

public class BadgeIndicatorTests
{
    [Theory]
    [InlineData(null, "")]
    [InlineData(0, "")]
    [InlineData(-3, "")]
    [InlineData(1, "1")]
    [InlineData(42, "42")]
    [InlineData(999, "999")]
    [InlineData(1000, "999+")]
    public void Format_caps_at_999_by_default(int? count, string expected) =>
        Assert.Equal(expected, BadgeIndicatorModel.Format(count, 999));

    [Theory]
    [InlineData(99, "99")]
    [InlineData(100, "99+")]
    public void Format_honors_a_custom_cap(int count, string expected) =>
        Assert.Equal(expected, BadgeIndicatorModel.Format(count, 99));

    [Fact]
    public void A_missing_or_zero_count_is_a_dot()
    {
        Assert.True(BadgeIndicatorModel.IsDot(null));
        Assert.True(BadgeIndicatorModel.IsDot(0));
        Assert.False(BadgeIndicatorModel.IsDot(3));
    }

    [Fact]
    public void Control_follows_its_count()
    {
        var badge = new CdsBadgeIndicator();
        Assert.Equal("", badge.DisplayText);
        Assert.Equal(new Thickness(0, 8, 8, 0), badge.Margin);

        badge.Count = 3;
        Assert.Equal("3", badge.DisplayText);
        Assert.Equal(new Thickness(0, 4, 4, 0), badge.Margin);

        badge.Count = 5000;
        Assert.Equal("999+", badge.DisplayText);
        badge.MaxCount = 99;
        Assert.Equal("99+", badge.DisplayText);

        badge.Count = null;
        badge.IsCompactHost = true;
        Assert.Equal(new Thickness(0, 4, 4, 0), badge.Margin);
    }

    [Fact]
    public void Max_count_is_at_least_one()
    {
        Assert.Equal(1, new CdsBadgeIndicator { MaxCount = 0 }.MaxCount);
    }
}

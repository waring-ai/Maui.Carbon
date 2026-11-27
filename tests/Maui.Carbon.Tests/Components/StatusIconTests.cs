namespace Maui.Carbon.Tests.Components;

public class StatusIconTests
{
    [Theory]
    [InlineData(CdsStatusIconKind.Fatal, CdsColorKeys.IconPrimary)]
    [InlineData(CdsStatusIconKind.Critical, CdsColorKeys.StatusRed)]
    [InlineData(CdsStatusIconKind.MajorWarning, CdsColorKeys.StatusOrange)]
    [InlineData(CdsStatusIconKind.MinorWarning, CdsColorKeys.StatusYellow)]
    [InlineData(CdsStatusIconKind.Undefined, CdsColorKeys.StatusPurple)]
    [InlineData(CdsStatusIconKind.Unknown, CdsColorKeys.StatusGray)]
    [InlineData(CdsStatusIconKind.Normal, CdsColorKeys.StatusGreen)]
    [InlineData(CdsStatusIconKind.Info, CdsColorKeys.StatusBlue)]
    [InlineData(CdsStatusIconKind.InProgress, CdsColorKeys.StatusBlue)]
    [InlineData(CdsStatusIconKind.Running, CdsColorKeys.StatusGreen)]
    [InlineData(CdsStatusIconKind.Pending, CdsColorKeys.StatusGray)]
    public void Kind_maps_to_its_color(CdsStatusIconKind kind, string key) =>
        Assert.Equal(key, CdsStatusIconTokens.Color(kind));

    [Theory]
    [InlineData(CdsStatusIconKind.Fatal, CdsIconName.ErrorFilled)]
    [InlineData(CdsStatusIconKind.Critical, CdsIconName.ErrorFilled)]
    [InlineData(CdsStatusIconKind.MinorWarning, CdsIconName.WarningAltFilled)]
    [InlineData(CdsStatusIconKind.Undefined, CdsIconName.Undefined)]
    [InlineData(CdsStatusIconKind.Unknown, CdsIconName.Unknown)]
    [InlineData(CdsStatusIconKind.Normal, CdsIconName.CheckmarkFilled)]
    [InlineData(CdsStatusIconKind.Info, CdsIconName.InformationSquareFilled)]
    [InlineData(CdsStatusIconKind.InProgress, CdsIconName.InProgress)]
    [InlineData(CdsStatusIconKind.Running, CdsIconName.CheckmarkOutline)]
    [InlineData(CdsStatusIconKind.Pending, CdsIconName.PendingFilled)]
    public void Kind_maps_to_its_icon(CdsStatusIconKind kind, CdsIconName icon) =>
        Assert.Equal(icon, CdsStatusIconTokens.Icon(kind));

    [Fact]
    public void Major_warning_is_drawn_as_the_inverted_triangle() =>
        Assert.Null(CdsStatusIconTokens.Icon(CdsStatusIconKind.MajorWarning));

    [Fact]
    public void Kinds_that_share_a_color_differ_in_shape()
    {
        var groups = Enum.GetValues<CdsStatusIconKind>().GroupBy(CdsStatusIconTokens.Color).Where(g => g.Count() > 1);
        foreach (var group in groups)
        {
            var icons = group.Select(CdsStatusIconTokens.Icon).ToList();
            Assert.Equal(icons.Count, icons.Distinct().Count());
        }
    }

    [Theory]
    [InlineData(CdsStatusIconKind.MajorWarning, false, CdsColorKeys.IconPrimary)]
    [InlineData(CdsStatusIconKind.MajorWarning, true, CdsColorKeys.IconInverse)]
    [InlineData(CdsStatusIconKind.MinorWarning, false, CdsColorKeys.IconPrimary)]
    [InlineData(CdsStatusIconKind.MinorWarning, true, CdsColorKeys.IconInverse)]
    [InlineData(CdsStatusIconKind.Critical, false, null)]
    [InlineData(CdsStatusIconKind.Normal, true, null)]
    public void Only_warning_triangles_have_a_dark_mark(CdsStatusIconKind kind, bool dark, string? key) =>
        Assert.Equal(key, CdsStatusIconTokens.InnerColor(kind, dark));

    [Theory]
    [InlineData(CdsSize.Xs, 16)]
    [InlineData(CdsSize.Sm, 16)]
    [InlineData(CdsSize.Md, 20)]
    [InlineData(CdsSize.Lg, 24)]
    [InlineData(CdsSize.Xl, 32)]
    [InlineData(CdsSize.Xxl, 32)]
    public void Size_maps_to_the_glyph_size(CdsSize size, double expected) =>
        Assert.Equal(expected, CdsStatusIconTokens.GlyphSize(size));

    [Fact]
    public void Only_in_progress_spins()
    {
        Assert.True(CdsStatusIconTokens.Spins(CdsStatusIconKind.InProgress));
        Assert.Equal(8000u, CdsStatusIconTokens.SpinDuration);
        foreach (var kind in Enum.GetValues<CdsStatusIconKind>().Where(k => k != CdsStatusIconKind.InProgress))
            Assert.False(CdsStatusIconTokens.Spins(kind));
    }

    [Fact]
    public void Defaults_and_properties_can_be_set_without_a_platform()
    {
        var icon = new CdsStatusIcon();
        Assert.Equal(CdsStatusIconKind.Normal, icon.Kind);
        Assert.Equal(CdsSize.Sm, icon.Size);
        foreach (var kind in Enum.GetValues<CdsStatusIconKind>())
            foreach (var size in Enum.GetValues<CdsSize>())
            {
                icon.Kind = kind;
                icon.Size = size;
            }
        icon.IconDescription = "Critical";
        icon.IconDescription = null;
        icon.IsEnabled = false;
    }

    [Fact]
    public void Constructor_sets_kind_description_and_size()
    {
        var icon = new CdsStatusIcon(CdsStatusIconKind.MajorWarning, "Major warning", CdsSize.Lg);
        Assert.Equal(CdsStatusIconKind.MajorWarning, icon.Kind);
        Assert.Equal("Major warning", icon.IconDescription);
        Assert.Equal(CdsSize.Lg, icon.Size);
    }

    [Fact]
    public void A_loaded_icon_can_switch_kinds()
    {
        var icon = new CdsStatusIcon(CdsStatusIconKind.InProgress, "Deploying");
        var window = new Window(new ContentPage { Content = icon });
        icon.Kind = CdsStatusIconKind.MajorWarning;
        icon.Kind = CdsStatusIconKind.InProgress;
        Assert.NotNull(window);
    }
}

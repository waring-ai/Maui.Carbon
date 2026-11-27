namespace Maui.Carbon.Tests.Components;

public class UserAvatarTests
{
    [Theory]
    [InlineData("thomas j. watson", "TW")]
    [InlineData("Thomas J. Watson", "TW")]
    [InlineData("Thomas", "TH")]
    [InlineData("TW", "TW")]
    [InlineData("t", "T")]
    [InlineData("ada lovelace", "AL")]
    [InlineData("  ada   lovelace  ", "AL")]
    [InlineData("élodie durand", "ÉD")]
    public void Initials_follow_carbon_for_ibm_products(string name, string expected) =>
        Assert.Equal(expected, UserAvatarModel.Initials(name));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void No_name_has_no_initials(string? name) => Assert.Null(UserAvatarModel.Initials(name));

    [Theory]
    [InlineData(true, true, "TW", CdsUserAvatarContent.Image)]
    [InlineData(false, true, "TW", CdsUserAvatarContent.Icon)]
    [InlineData(false, false, "TW", CdsUserAvatarContent.Initials)]
    [InlineData(false, false, null, CdsUserAvatarContent.DefaultIcon)]
    [InlineData(false, false, " ", CdsUserAvatarContent.DefaultIcon)]
    [InlineData(true, false, null, CdsUserAvatarContent.Image)]
    public void Content_precedence_is_image_icon_initials_default(bool image, bool icon, string? name, CdsUserAvatarContent expected) =>
        Assert.Equal(expected, UserAvatarModel.Content(image, icon, name));

    [Theory]
    [InlineData(CdsSize.Xs, 24, 16)]
    [InlineData(CdsSize.Sm, 24, 16)]
    [InlineData(CdsSize.Md, 32, 20)]
    [InlineData(CdsSize.Lg, 48, 24)]
    [InlineData(CdsSize.Xl, 64, 32)]
    [InlineData(CdsSize.Xxl, 64, 32)]
    public void Sizes_follow_the_spacing_scale(CdsSize size, double diameter, double icon)
    {
        Assert.Equal(diameter, CdsUserAvatarTokens.Diameter(size));
        Assert.Equal(icon, CdsUserAvatarTokens.IconSize(size));
    }

    [Theory]
    [InlineData(CdsSize.Sm, CdsTypeStyle.Label01)]
    [InlineData(CdsSize.Md, CdsTypeStyle.BodyCompact01)]
    [InlineData(CdsSize.Lg, CdsTypeStyle.Heading03)]
    [InlineData(CdsSize.Xl, CdsTypeStyle.Heading04)]
    public void Initials_type_follows_the_size(CdsSize size, CdsTypeStyle style) =>
        Assert.Equal(style, CdsUserAvatarTokens.TypeStyle(size));

    [Theory]
    [InlineData(CdsUserAvatarColor.Order1Cyan, CdsColorKeys.TagBorderCyan)]
    [InlineData(CdsUserAvatarColor.Order2Gray, CdsColorKeys.TagBorderGray)]
    [InlineData(CdsUserAvatarColor.Order3Green, CdsColorKeys.TagBorderGreen)]
    [InlineData(CdsUserAvatarColor.Order4Magenta, CdsColorKeys.TagBorderMagenta)]
    [InlineData(CdsUserAvatarColor.Order5Purple, CdsColorKeys.TagBorderPurple)]
    [InlineData(CdsUserAvatarColor.Order6Teal, CdsColorKeys.TagBorderTeal)]
    [InlineData(CdsUserAvatarColor.Order8Gray, CdsColorKeys.TextSecondary)]
    [InlineData(CdsUserAvatarColor.Order9Green, CdsColorKeys.DataVis07)]
    [InlineData(CdsUserAvatarColor.Order11Purple, CdsColorKeys.DataVis14)]
    public void Dark_themes_use_the_matching_palette_step(CdsUserAvatarColor color, string key) =>
        Assert.Equal(key, CdsUserAvatarTokens.Background(color, darkTheme: true));

    [Theory]
    [InlineData(CdsUserAvatarColor.Order2Gray, CdsColorKeys.StatusGray)]
    [InlineData(CdsUserAvatarColor.Order3Green, CdsColorKeys.DataVis07)]
    [InlineData(CdsUserAvatarColor.Order5Purple, CdsColorKeys.StatusPurple)]
    [InlineData(CdsUserAvatarColor.Order8Gray, CdsColorKeys.BackgroundInverse)]
    [InlineData(CdsUserAvatarColor.Order1Cyan, CdsColorKeys.TagColorCyan)]
    [InlineData(CdsUserAvatarColor.Order7Cyan, CdsColorKeys.DataVis12)]
    public void Light_themes_use_the_closest_token(CdsUserAvatarColor color, string key) =>
        Assert.Equal(key, CdsUserAvatarTokens.Background(color, darkTheme: false));

    [Fact]
    public void Every_color_resolves_to_a_theme_key_in_both_themes()
    {
        foreach (var color in Enum.GetValues<CdsUserAvatarColor>())
            foreach (var dark in new[] { false, true })
                Assert.Contains(CdsUserAvatarTokens.Background(color, dark), CdsColorKeys.All);
    }

    [Fact]
    public void The_dark_set_is_twelve_distinct_colors()
    {
        var keys = Enum.GetValues<CdsUserAvatarColor>().Select(c => CdsUserAvatarTokens.Background(c, true)).ToList();
        Assert.Equal(keys.Count, keys.Distinct().Count());
    }

    [Fact]
    public void Foreground_is_inverse() 
    {
        Assert.Equal(CdsColorKeys.TextInverse, CdsUserAvatarTokens.Foreground);
        Assert.Equal(CdsColorKeys.IconInverse, CdsUserAvatarTokens.IconColor);
    }

    [Fact]
    public void Defaults_follow_the_readme()
    {
        var avatar = new CdsUserAvatar();
        Assert.Equal(CdsSize.Md, avatar.Size);
        Assert.Equal(CdsUserAvatarColor.Order1Cyan, avatar.AvatarColor);
        Assert.Equal(CdsPlacement.Bottom, avatar.TooltipPlacement);
        Assert.Equal(CdsUserAvatarContent.DefaultIcon, avatar.CurrentContent);
    }

    [Fact]
    public void Properties_can_be_set_without_a_platform()
    {
        var avatar = new CdsUserAvatar("Thomas J. Watson", CdsUserAvatarColor.Order5Purple);
        Assert.Equal(CdsUserAvatarContent.Initials, avatar.CurrentContent);
        Assert.Equal("TW", avatar.CurrentInitials);
        avatar.Icon = CdsIconName.UserAvatar;
        Assert.Equal(CdsUserAvatarContent.Icon, avatar.CurrentContent);
        avatar.Image = ImageSource.FromFile("headshot.png");
        avatar.ImageDescription = "Avatar of Thomas J. Watson";
        Assert.Equal(CdsUserAvatarContent.Image, avatar.CurrentContent);
        avatar.Image = null;
        foreach (var size in Enum.GetValues<CdsSize>()) avatar.Size = size;
        foreach (var color in Enum.GetValues<CdsUserAvatarColor>()) avatar.AvatarColor = color;
        avatar.TooltipText = "TW, Thomas J. Watson user profile";
        avatar.TooltipPlacement = CdsPlacement.Right;
        avatar.Size = CdsSize.Xl;
        avatar.TooltipText = null;
        avatar.TooltipText = "Thomas J. Watson";
        avatar.IsEnabled = false;
    }

    [Fact]
    public void A_tooltip_makes_the_avatar_a_trigger_and_removing_it_restores_the_face()
    {
        var avatar = new CdsUserAvatar("Ada Lovelace") { TooltipText = "Ada Lovelace" };
        var window = new Window(new ContentPage { Content = avatar });
        var tooltip = FindDescendant<CdsTooltip>(avatar);
        Assert.NotNull(tooltip);
        Assert.Equal("Ada Lovelace", tooltip!.Text);
        avatar.TooltipText = null;
        Assert.Null(FindDescendant<CdsTooltip>(avatar));
        Assert.NotNull(window);
    }

    static T? FindDescendant<T>(Element root) where T : Element
    {
        foreach (var child in ((IElementController)root).LogicalChildren)
        {
            if (child is T match) return match;
            if (FindDescendant<T>(child) is { } found) return found;
        }
        return null;
    }
}

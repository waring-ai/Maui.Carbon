namespace Maui.Carbon.Tests.Components;

public class EmptyStateTokenTests
{
    [Theory]
    [InlineData(CdsEmptyStateSize.Lg, 80)]
    [InlineData(CdsEmptyStateSize.Sm, 64)]
    public void Illustration_is_spacing_11_at_lg_and_spacing_10_at_sm(CdsEmptyStateSize size, double expected) =>
        Assert.Equal(expected, CdsEmptyStateTokens.IllustrationSize(size));

    [Theory]
    [InlineData(CdsEmptyStateSize.Lg, CdsTypeStyle.Heading03, CdsTypeStyle.Body01)]
    [InlineData(CdsEmptyStateSize.Sm, CdsTypeStyle.BodyCompact02, CdsTypeStyle.Label01)]
    public void Type_follows_the_size(CdsEmptyStateSize size, CdsTypeStyle title, CdsTypeStyle subtitle)
    {
        Assert.Equal(title, CdsEmptyStateTokens.TitleStyle(size));
        Assert.Equal(subtitle, CdsEmptyStateTokens.SubtitleStyle(size));
    }

    [Fact]
    public void Gaps_and_colors_match_the_stylesheet()
    {
        Assert.Equal(16, CdsEmptyStateTokens.IllustrationGap);
        Assert.Equal(8, CdsEmptyStateTokens.TitleGap);
        Assert.Equal(16, CdsEmptyStateTokens.SubtitleGap);
        Assert.Equal(16, CdsEmptyStateTokens.ActionGap);
        Assert.Equal(CdsColorKeys.TextPrimary, CdsEmptyStateTokens.TitleColor);
        Assert.Equal(CdsColorKeys.TextSecondary, CdsEmptyStateTokens.SubtitleColor);
    }

    [Theory]
    [InlineData(CdsButtonKind.Primary, CdsButtonKind.Primary)]
    [InlineData(CdsButtonKind.Secondary, CdsButtonKind.Secondary)]
    [InlineData(CdsButtonKind.Tertiary, CdsButtonKind.Tertiary)]
    [InlineData(CdsButtonKind.Ghost, CdsButtonKind.Tertiary)]
    [InlineData(CdsButtonKind.Danger, CdsButtonKind.Tertiary)]
    public void Action_kind_is_primary_secondary_or_tertiary(CdsButtonKind requested, CdsButtonKind expected) =>
        Assert.Equal(expected, CdsEmptyStateTokens.ActionKind(requested));
}

public class EmptyStateModelTests
{
    [Theory]
    [InlineData(CdsEmptyStateIllustrationPosition.Top, false, 0, 1)]
    [InlineData(CdsEmptyStateIllustrationPosition.Bottom, false, 1, 0)]
    [InlineData(CdsEmptyStateIllustrationPosition.Left, true, 0, 1)]
    [InlineData(CdsEmptyStateIllustrationPosition.Right, true, 1, 0)]
    public void Positions_map_to_flex_directions(CdsEmptyStateIllustrationPosition position, bool row, int illustration, int content)
    {
        var placement = EmptyStateModel.Place(position);
        Assert.Equal(row, placement.IsRow);
        Assert.Equal(illustration, placement.IllustrationIndex);
        Assert.Equal(content, placement.ContentIndex);
    }

    [Fact]
    public void Action_and_link_need_text()
    {
        Assert.False(EmptyStateModel.ShowsAction(null));
        Assert.False(EmptyStateModel.ShowsAction(""));
        Assert.True(EmptyStateModel.ShowsAction("Create bucket"));
        Assert.False(EmptyStateModel.ShowsLink(null));
        Assert.True(EmptyStateModel.ShowsLink("View documentation"));
    }
}

public class EmptyStateTests
{
    [Fact]
    public void Defaults_follow_the_source()
    {
        var empty = new CdsEmptyState { Title = "Start by adding data assets" };
        Assert.Equal(CdsEmptyStateSize.Lg, empty.Size);
        Assert.Equal(CdsEmptyStateIllustrationPosition.Top, empty.IllustrationPosition);
        Assert.Equal(CdsIllustrationTheme.Auto, empty.IllustrationTheme);
        Assert.Equal(CdsButtonKind.Tertiary, empty.ActionKind);
        Assert.Null(empty.HeadingLevel);
        Assert.False(empty.HasPresetIllustration);
        Assert.Null(empty.ActionButton);
        Assert.Null(empty.Link);
        Assert.Equal(CdsTypeStyle.Heading03, empty.TitleLabel.TypeStyle);
        Assert.Equal("Start by adding data assets", empty.TitleLabel.Text);
    }

    [Fact]
    public void Every_property_can_be_set_without_a_platform()
    {
        var command = new Command(() => { });
        var empty = new CdsEmptyState
        {
            Title = "No buckets yet",
            Subtitle = "Create a bucket to start storing objects.",
            Size = CdsEmptyStateSize.Sm,
            Illustration = ImageSource.FromUri(new Uri("https://example.com/empty.png")),
            IllustrationDescription = "An empty bucket",
            IllustrationPosition = CdsEmptyStateIllustrationPosition.Left,
            IllustrationTheme = CdsIllustrationTheme.Dark,
            HeadingLevel = 1,
            ActionText = "Create bucket",
            ActionKind = CdsButtonKind.Primary,
            ActionIcon = CdsIconName.Add,
            ActionCommand = command,
            ActionCommandParameter = "bucket",
            LinkText = "View documentation",
            LinkHref = "https://www.carbondesignsystem.com",
            LinkIcon = CdsIconName.ArrowRight,
            LinkCommand = command,
            LinkCommandParameter = "docs",
            IsEnabled = false,
        };
        empty.ActionClicked += (_, _) => { };
        empty.LinkClicked += (_, _) => { };
        foreach (var position in Enum.GetValues<CdsEmptyStateIllustrationPosition>()) empty.IllustrationPosition = position;
        Assert.Equal(CdsTypeStyle.BodyCompact02, empty.TitleLabel.TypeStyle);
        Assert.NotNull(empty.ActionButton);
        Assert.Equal(CdsButtonKind.Primary, empty.ActionButton!.Kind);
        Assert.Equal(CdsSize.Sm, empty.ActionButton.Size);
        Assert.Equal(CdsIconName.Add, empty.ActionButton.Icon);
        Assert.Equal("View documentation", empty.Link!.Text);
        Assert.Equal("https://www.carbondesignsystem.com", empty.Link.Href);
    }

    [Fact]
    public void Action_kind_outside_the_three_falls_back_to_tertiary()
    {
        var empty = new CdsEmptyState { Title = "No data", ActionText = "Retry", ActionKind = CdsButtonKind.Danger };
        Assert.Equal(CdsButtonKind.Tertiary, empty.ActionButton!.Kind);
    }

    [Fact]
    public void Clearing_the_action_text_hides_the_button()
    {
        var empty = new CdsEmptyState { Title = "No data", ActionText = "Retry", LinkText = "Help" };
        empty.ActionText = null;
        empty.LinkText = "";
        Assert.False(empty.ActionButton!.IsVisible);
        Assert.False(empty.Link!.IsVisible);
    }

    public static TheoryData<Func<CdsEmptyState>, string> Presets => new()
    {
        { () => new CdsErrorEmptyState(), "Error" },
        { () => new CdsNoDataEmptyState(), "NoData" },
        { () => new CdsNoTagsEmptyState(), "NoTags" },
        { () => new CdsNotFoundEmptyState(), "NotFound" },
        { () => new CdsNotificationsEmptyState(), "Notifications" },
        { () => new CdsUnauthorizedEmptyState(), "Unauthorized" },
    };

    [Theory]
    [MemberData(nameof(Presets))]
    public void Presets_draw_their_light_and_dark_artwork(Func<CdsEmptyState> create, string name)
    {
        var preset = create();
        preset.Title = "Empty";
        Assert.True(preset.HasPresetIllustration);
        Assert.Equal(name + "Light", preset.CurrentPreset!.Name);
        preset.IllustrationTheme = CdsIllustrationTheme.Dark;
        Assert.Equal(name + "Dark", preset.CurrentPreset!.Name);
        preset.IllustrationTheme = CdsIllustrationTheme.Light;
        Assert.Equal(name + "Light", preset.CurrentPreset!.Name);
        preset.Size = CdsEmptyStateSize.Sm;
        preset.IllustrationDescription = "Illustration";
        preset.Illustration = ImageSource.FromUri(new Uri("https://example.com/ignored.png"));
        Assert.Equal(name + "Light", preset.CurrentPreset!.Name);
    }

    [Fact]
    public void The_title_is_one_level_below_its_context_unless_forced()
    {
        var empty = new CdsEmptyState { Title = "No data" };
        Assert.Equal(2, empty.TitleLabel.Level);
        empty.HeadingLevel = 1;
        Assert.Equal(1, empty.TitleLabel.Level);
        Assert.Equal(SemanticHeadingLevel.Level1, SemanticProperties.GetHeadingLevel(empty.TitleLabel));
    }
}

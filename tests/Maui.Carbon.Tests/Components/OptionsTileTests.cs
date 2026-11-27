namespace Maui.Carbon.Tests.Components;

public class OptionsTileTests
{
    const CdsInteractionState None = CdsInteractionState.None;

    // ----- model -----

    [Theory]
    [InlineData(true, true, true, CdsOptionsTileStatus.Invalid)]
    [InlineData(false, true, true, CdsOptionsTileStatus.Warning)]
    [InlineData(false, false, true, CdsOptionsTileStatus.Locked)]
    [InlineData(false, false, false, CdsOptionsTileStatus.None)]
    public void Invalid_wins_over_warning_and_warning_over_locked(bool invalid, bool warn, bool locked, CdsOptionsTileStatus expected) =>
        Assert.Equal(expected, OptionsTileModel.Status(invalid, warn, locked));

    [Fact]
    public void Only_a_plain_lock_disables_the_toggle()
    {
        Assert.True(OptionsTileModel.IsToggleLocked(false, false, true));
        Assert.False(OptionsTileModel.IsToggleLocked(true, false, true));
        Assert.False(OptionsTileModel.IsToggleLocked(false, true, true));
        Assert.False(OptionsTileModel.IsToggleLocked(false, false, false));
    }

    [Fact]
    public void Summary_text_follows_the_state()
    {
        const string summary = "English | Locale: English";
        Assert.Equal(summary, OptionsTileModel.SummaryText(CdsOptionsTileStatus.None, true, summary, "bad", "warn", "locked"));
        Assert.Equal("bad", OptionsTileModel.SummaryText(CdsOptionsTileStatus.Invalid, true, summary, "bad", "warn", "locked"));
        Assert.Equal("warn", OptionsTileModel.SummaryText(CdsOptionsTileStatus.Warning, true, summary, "bad", "warn", "locked"));
        // Locked: a static tile shows the locked text; an expandable one keeps its summary unless it has none.
        Assert.Equal("locked", OptionsTileModel.SummaryText(CdsOptionsTileStatus.Locked, false, summary, "bad", "warn", "locked"));
        Assert.Equal(summary, OptionsTileModel.SummaryText(CdsOptionsTileStatus.Locked, true, summary, "bad", "warn", "locked"));
        Assert.Equal("locked", OptionsTileModel.SummaryText(CdsOptionsTileStatus.Locked, true, null, "bad", "warn", "locked"));
    }

    [Fact]
    public void Summary_hides_while_open_unless_a_state_shows()
    {
        Assert.True(OptionsTileModel.IsSummaryHidden(CdsOptionsTileStatus.None, true));
        Assert.False(OptionsTileModel.IsSummaryHidden(CdsOptionsTileStatus.None, false));
        Assert.False(OptionsTileModel.IsSummaryHidden(CdsOptionsTileStatus.Invalid, true));
        Assert.False(OptionsTileModel.IsSummaryHidden(CdsOptionsTileStatus.Locked, true));
    }

    [Theory]
    [InlineData(CdsSize.Xs, CdsSize.Lg, 48, 2)]
    [InlineData(CdsSize.Lg, CdsSize.Lg, 48, 2)]
    [InlineData(CdsSize.Xl, CdsSize.Xl, 64, 4)]
    [InlineData(CdsSize.Xxl, CdsSize.Xl, 64, 4)]
    public void Sizes_are_lg_or_xl(CdsSize size, CdsSize normalized, double height, double gap)
    {
        Assert.Equal(normalized, OptionsTileModel.Normalize(size));
        Assert.Equal(height, OptionsTileModel.HeaderHeight(size));
        Assert.Equal(gap, OptionsTileModel.SummaryGap(size));
    }

    [Fact]
    public void Measurements_come_from_the_spacing_scale()
    {
        Assert.Equal(64, OptionsTileModel.ToggleColumnWidth);
        Assert.Equal(48, OptionsTileModel.ChevronColumnWidth);
        Assert.Equal(new Thickness(48, 8, 16, 24), OptionsTileModel.ContentPadding);
        Assert.Equal(25, OptionsTileModel.HeightAt(0, 100, 0.25));
        Assert.Equal(100, OptionsTileModel.HeightAt(0, 100, 3));
        Assert.Equal(0, OptionsTileModel.HeightAt(0, 100, -1));
    }

    // ----- tokens -----

    [Fact]
    public void Header_hovers_to_background_hover()
    {
        Assert.Null(CdsOptionsTileTokens.HeaderBackground(None));
        Assert.Equal(CdsColorKeys.BackgroundHover, CdsOptionsTileTokens.HeaderBackground(CdsInteractionState.Hover));
        Assert.Null(CdsOptionsTileTokens.HeaderBackground(CdsInteractionState.Hover | CdsInteractionState.Disabled));
    }

    [Fact]
    public void Summary_colors_and_glyphs_per_state()
    {
        Assert.Equal(new CdsOptionsTileSummaryVisuals(CdsColorKeys.TextSecondary, null, CdsColorKeys.IconSecondary), CdsOptionsTileTokens.Summary(CdsOptionsTileStatus.None, None));
        Assert.Equal(new CdsOptionsTileSummaryVisuals(CdsColorKeys.SupportError, CdsIconName.WarningFilled, CdsColorKeys.SupportError), CdsOptionsTileTokens.Summary(CdsOptionsTileStatus.Invalid, None));
        Assert.Equal(new CdsOptionsTileSummaryVisuals(CdsColorKeys.TextPrimary, CdsIconName.WarningAltFilled, CdsColorKeys.SupportWarning), CdsOptionsTileTokens.Summary(CdsOptionsTileStatus.Warning, None));
        Assert.Equal(new CdsOptionsTileSummaryVisuals(CdsColorKeys.TextSecondary, CdsOptionsTileTokens.LockedIcon, CdsColorKeys.IconSecondary), CdsOptionsTileTokens.Summary(CdsOptionsTileStatus.Locked, None));
        Assert.Equal(CdsColorKeys.TextDisabled, CdsOptionsTileTokens.Summary(CdsOptionsTileStatus.Invalid, CdsInteractionState.Disabled).Text);
        Assert.Equal(CdsColorKeys.TextPrimary, CdsOptionsTileTokens.Title(None));
        Assert.Equal(CdsColorKeys.IconDisabled, CdsOptionsTileTokens.Chevron(CdsInteractionState.Disabled));
    }

    // ----- control -----

    [Fact]
    public void Every_property_can_be_set_without_a_platform()
    {
        var tile = new CdsOptionsTile
        {
            Title = "Language",
            Summary = "English | Locale: English",
            Size = CdsSize.Xl,
            IsToggled = true,
            IsInvalid = true,
            InvalidText = "Your system does not support this configuration",
            IsWarning = true,
            WarnText = "A restart is required to apply these settings",
            IsLocked = true,
            LockedText = "This option is managed by your administrator",
            Body = new CdsText("User interface defines the language the application is displayed in."),
            IsOpen = true,
        };
        tile.IsInvalid = false;
        tile.IsWarning = false;
        tile.IsToggled = null;
        tile.Body = null;
        tile.IsEnabled = false;
        Assert.False(tile.IsExpandable);
    }

    [Fact]
    public void Activating_the_header_opens_an_expandable_tile_and_hides_the_summary()
    {
        var tile = new CdsOptionsTile { Title = "Language", Summary = "English", Body = new CdsText("Settings") };
        CdsChoiceChangedEventArgs<bool>? seen = null;
        tile.OpenChanged += (_, e) => seen = e;
        Assert.Equal("English", tile.DisplayedSummary);
        var hit = Hit(tile);
        Assert.Equal(CdsAccessibilityRole.Expander, hit.Role);
        Assert.Equal("Language", SemanticProperties.GetDescription(hit));
        Assert.False(hit.IsExpanded);
        hit.RaiseKeyboardActivation();
        Assert.True(tile.IsOpen);
        Assert.True(seen!.NewValue);
        Assert.True(hit.IsExpanded);
        Assert.Null(tile.DisplayedSummary);
    }

    [Fact]
    public void A_static_tile_has_no_expander()
    {
        var tile = new CdsOptionsTile { Title = "Language", Summary = "English" };
        Assert.DoesNotContain(Maui.Carbon.Tests.Foundation.ContainerSemanticsAuditTests.Walk(tile).OfType<CdsHitTarget>(), h => h.Role == CdsAccessibilityRole.Expander && h.IsVisible);
        tile.IsOpen = true;
        Assert.Equal("English", tile.DisplayedSummary);
    }

    [Fact]
    public void The_toggle_shows_only_with_a_value_and_syncs_both_ways()
    {
        var tile = new CdsOptionsTile { Title = "Language", Summary = "English" };
        Assert.Null(tile.Toggle);
        tile.IsToggled = false;
        var toggle = tile.Toggle!;
        Assert.True(toggle.IsVisible);
        Assert.Equal("Language", SemanticProperties.GetDescription(toggle));
        ToggledEventArgs? seen = null;
        tile.Toggled += (_, e) => seen = e;
        toggle.IsToggled = true;
        Assert.True(tile.IsToggled);
        Assert.True(seen!.Value);
        tile.IsToggled = false;
        Assert.False(toggle.IsToggled);
        tile.IsToggled = null;
        Assert.False(toggle.IsVisible);
    }

    [Fact]
    public void Locking_disables_the_toggle_unless_invalid_or_warning()
    {
        var tile = new CdsOptionsTile { Title = "Language", IsToggled = true, IsLocked = true, LockedText = "Managed by your administrator" };
        Assert.False(tile.Toggle!.IsEnabled);
        Assert.Equal("Managed by your administrator", tile.DisplayedSummary);
        tile.IsWarning = true;
        tile.WarnText = "Restart required";
        Assert.True(tile.Toggle.IsEnabled);
        Assert.Equal("Restart required", tile.DisplayedSummary);
    }

    static CdsHitTarget Hit(CdsOptionsTile tile) =>
        Maui.Carbon.Tests.Foundation.ContainerSemanticsAuditTests.Walk(tile).OfType<CdsHitTarget>().First(h => h.Role == CdsAccessibilityRole.Expander);
}

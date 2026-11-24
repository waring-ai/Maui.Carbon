namespace Maui.Carbon.Tests.Components;

public class LoadingTests
{
    [Fact]
    public void Spinner_geometry_matches_carbon()
    {
        Assert.Equal(10, SpinnerModel.StrokeWidth(small: false));
        Assert.Equal(16, SpinnerModel.StrokeWidth(small: true));
        Assert.Equal(0.81, SpinnerModel.ArcFraction(false), 2);
        Assert.Equal(0.48, SpinnerModel.ArcFraction(true), 2);
        Assert.Equal(690u, SpinnerModel.RotationMilliseconds);
        Assert.Equal(180, SpinnerModel.Rotation(0.5), 6);
    }

    [Fact]
    public void Defaults_follow_the_readme()
    {
        var loading = new CdsLoading();
        Assert.True(loading.IsActive);
        Assert.False(loading.IsSmall);
        Assert.True(loading.WithOverlay);
        Assert.Equal("loading", loading.Description);
        Assert.Null(loading.Overlay);
    }

    [Fact]
    public void Properties_can_be_set_without_a_platform()
    {
        var loading = new CdsLoading("Recalculating billing summary", small: true, withOverlay: false);
        loading.IsActive = false;
        loading.IsActive = true;
        loading.WithOverlay = true;
        loading.Description = "Refreshing metrics";
        Assert.Null(loading.Overlay);
    }

    [Fact]
    public void Motion_loop_falls_back_to_the_rest_phase_without_a_handler()
    {
        var phase = -1.0;
        var view = new BoxView();
        var running = CdsLoop.Start(view, "test", p => phase = p, 0.2, 100);
        Assert.False(running);
        Assert.Equal(0.2, phase);
    }
}

public class InlineLoadingTests
{
    [Theory]
    [InlineData(CdsInlineLoadingStatus.Active, true, null)]
    [InlineData(CdsInlineLoadingStatus.Inactive, false, null)]
    [InlineData(CdsInlineLoadingStatus.Finished, false, CdsIconName.CheckmarkFilled)]
    [InlineData(CdsInlineLoadingStatus.Error, false, CdsIconName.ErrorFilled)]
    public void Status_maps_to_spinner_or_icon(CdsInlineLoadingStatus status, bool spinner, CdsIconName? icon)
    {
        var v = CdsInlineLoadingTokens.Resolve(status);
        Assert.Equal(spinner, v.Spinner);
        Assert.Equal(icon, v.Icon);
    }

    [Fact]
    public void Finished_is_green_and_error_is_red()
    {
        Assert.Equal(CdsColorKeys.SupportSuccess, CdsInlineLoadingTokens.Resolve(CdsInlineLoadingStatus.Finished).IconColor);
        Assert.Equal(CdsColorKeys.SupportError, CdsInlineLoadingTokens.Resolve(CdsInlineLoadingStatus.Error).IconColor);
    }

    [Theory]
    [InlineData(CdsInlineLoadingStatus.Active, "loading")]
    [InlineData(CdsInlineLoadingStatus.Inactive, "not loading")]
    [InlineData(CdsInlineLoadingStatus.Finished, "finished")]
    [InlineData(CdsInlineLoadingStatus.Error, "error")]
    public void Default_icon_descriptions(CdsInlineLoadingStatus status, string description) =>
        Assert.Equal(description, CdsInlineLoadingTokens.Resolve(status).DefaultIconDescription);

    [Fact]
    public void Defaults_follow_the_readme()
    {
        var inline = new CdsInlineLoading();
        Assert.Equal(CdsInlineLoadingStatus.Active, inline.Status);
        Assert.Equal(1500, inline.SuccessDelay);
    }

    [Fact]
    public void Every_status_can_be_set_without_a_platform()
    {
        var inline = new CdsInlineLoading(CdsInlineLoadingStatus.Active, "Saving...") { IconDescription = "Saving changes", SuccessDelay = -1 };
        Assert.Equal(0, inline.SuccessDelay);
        foreach (var status in Enum.GetValues<CdsInlineLoadingStatus>()) inline.Status = status;
        inline.Description = null;
    }
}

public class LoadingRegionTests
{
    [Fact]
    public void Page_scope_is_the_default()
    {
        var loading = new CdsLoading();
        Assert.Equal(CdsLoadingOverlayScope.Page, loading.OverlayScope);
        Assert.Equal(LayoutOptions.Center, loading.HorizontalOptions);
        Assert.False(loading.IsRegionOverlayShown);
    }

    [Fact]
    public void A_region_overlay_covers_its_cell_and_blocks_only_the_views_beside_it()
    {
        var table = new Label { Text = "Snapshots" };
        var loading = new CdsLoading("Restoring snapshot") { OverlayScope = CdsLoadingOverlayScope.Region };
        var region = new Grid { Children = { table, loading } };
        var outside = new CdsButton { Text = "Cancel" };
        using var heard = CdsAnnouncer.Capture();
        _ = TestWindow.Show(region, outside);

        Assert.Equal(LayoutOptions.Fill, loading.HorizontalOptions);
        Assert.Equal(LayoutOptions.Fill, loading.VerticalOptions);
        Assert.True(loading.IsRegionOverlayShown);
        Assert.Null(loading.Overlay);
        Assert.False(loading.InputTransparent);
        Assert.True(table.InputTransparent);
        Assert.True(AutomationProperties.GetExcludedWithChildren(table));
        Assert.False(outside.InputTransparent);
        Assert.Equal("Restoring snapshot", Assert.Single(heard.Announcements).Text);

        loading.IsActive = false;
        Assert.False(loading.IsRegionOverlayShown);
        Assert.True(loading.InputTransparent);
        Assert.False(table.InputTransparent);
        Assert.Null(AutomationProperties.GetExcludedWithChildren(table));
    }

    [Fact]
    public void Leaving_region_scope_restores_the_region()
    {
        var table = new Label { InputTransparent = false };
        var loading = new CdsLoading { OverlayScope = CdsLoadingOverlayScope.Region };
        _ = TestWindow.Show(new Grid { Children = { table, loading } });
        Assert.True(table.InputTransparent);

        loading.WithOverlay = false;
        Assert.False(table.InputTransparent);
        Assert.False(loading.InputTransparent);
        loading.OverlayScope = CdsLoadingOverlayScope.Page;
        Assert.Equal(LayoutOptions.Center, loading.HorizontalOptions);
    }
}

public class InlineLoadingLiveModeTests
{
    [Fact]
    public void Text_changes_are_assertive_by_default()
    {
        var inline = new CdsInlineLoading(CdsInlineLoadingStatus.Active, "Deploying...");
        Assert.Equal(CdsLiveMode.Assertive, inline.LiveMode);
        using var heard = CdsAnnouncer.Capture();
        _ = TestWindow.Show(inline);
        inline.Description = "Rolling out";
        Assert.Equal([new CdsAnnouncement("Rolling out", CdsAnnouncePriority.Assertive)], heard.Announcements);
    }

    [Fact]
    public void Polite_and_off_modes()
    {
        var inline = new CdsInlineLoading(CdsInlineLoadingStatus.Active, "Deploying...") { LiveMode = CdsLiveMode.Polite };
        using var heard = CdsAnnouncer.Capture();
        _ = TestWindow.Show(inline);
        inline.Description = "Phase 2 of 5";
        Assert.Equal(CdsAnnouncePriority.Polite, Assert.Single(heard.Announcements).Priority);

        inline.LiveMode = CdsLiveMode.Off;
        inline.Description = "Phase 3 of 5";
        Assert.Single(heard.Announcements);
    }

    [Fact]
    public void Hidden_or_unloaded_indicators_and_repeats_are_silent()
    {
        using var heard = CdsAnnouncer.Capture();
        var detached = new CdsInlineLoading(CdsInlineLoadingStatus.Active, "Saving...");
        detached.Description = "Saved";

        var hidden = new CdsInlineLoading(CdsInlineLoadingStatus.Active, "Saving...") { IsVisible = false };
        var shown = new CdsInlineLoading(CdsInlineLoadingStatus.Active, "Saving...");
        _ = TestWindow.Show(hidden, shown);
        hidden.Description = "Saved";
        Assert.Empty(heard.Announcements);

        shown.Status = CdsInlineLoadingStatus.Finished;
        shown.Status = CdsInlineLoadingStatus.Finished;
        shown.Description = "Saved";
        shown.Description = "Saved";
        Assert.Equal(["Saving...", "Saved"], heard.Announcements.Select(a => a.Text));
    }
}

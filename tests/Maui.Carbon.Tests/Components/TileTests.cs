namespace Maui.Carbon.Tests.Components;

public class TileTests
{
    const CdsInteractionState None = CdsInteractionState.None;

    [Fact]
    public void Static_tile_is_layer_01_in_every_pointer_state()
    {
        foreach (var state in new[] { None, CdsInteractionState.Hover, CdsInteractionState.Pressed })
            Assert.Equal(CdsColorKeys.Layer01, CdsTileTokens.Resolve(CdsTileKind.Static, state).Background);
    }

    [Fact]
    public void Clickable_hovers_to_layer_hover_and_presses_to_layer_active()
    {
        Assert.Equal(CdsColorKeys.Layer01, CdsTileTokens.Resolve(CdsTileKind.Clickable, None).Background);
        Assert.Equal(CdsColorKeys.LayerHover01, CdsTileTokens.Resolve(CdsTileKind.Clickable, CdsInteractionState.Hover).Background);
        Assert.Equal(CdsColorKeys.LayerActive01, CdsTileTokens.Resolve(CdsTileKind.Clickable, CdsInteractionState.Hover | CdsInteractionState.Pressed).Background);
        Assert.Equal(CdsColorKeys.IconInteractive, CdsTileTokens.Resolve(CdsTileKind.Clickable, None).Icon);
    }

    [Theory]
    [InlineData(CdsTileKind.Static)]
    [InlineData(CdsTileKind.Clickable)]
    [InlineData(CdsTileKind.Selectable)]
    [InlineData(CdsTileKind.Radio)]
    [InlineData(CdsTileKind.Expandable)]
    [InlineData(CdsTileKind.ExpandableInteractive)]
    public void Disabled_tiles_do_not_hover_and_use_the_disabled_icon(CdsTileKind kind)
    {
        var v = CdsTileTokens.Resolve(kind, CdsInteractionState.Disabled | CdsInteractionState.Hover | CdsInteractionState.Pressed);
        Assert.Equal(CdsColorKeys.Layer01, v.Background);
        Assert.Equal(CdsColorKeys.IconDisabled, v.Icon);
        Assert.Null(v.Border);
    }

    [Theory]
    [InlineData(CdsTileKind.Selectable)]
    [InlineData(CdsTileKind.Radio)]
    public void Selected_tiles_get_a_layer_selected_inverse_border_or_border_disabled(CdsTileKind kind)
    {
        Assert.Null(CdsTileTokens.Resolve(kind, None).Border);
        Assert.Equal(CdsColorKeys.LayerSelectedInverse, CdsTileTokens.Resolve(kind, CdsInteractionState.Selected).Border);
        Assert.Equal(CdsColorKeys.BorderDisabled, CdsTileTokens.Resolve(kind, CdsInteractionState.Selected | CdsInteractionState.Disabled).Border);
        Assert.Equal(CdsColorKeys.LayerHover01, CdsTileTokens.Resolve(kind, CdsInteractionState.Hover).Background);
    }

    [Fact]
    public void Selectable_glyph_is_secondary_then_primary_when_selected()
    {
        Assert.Equal(CdsColorKeys.IconSecondary, CdsTileTokens.Resolve(CdsTileKind.Selectable, None).Icon);
        Assert.Equal(CdsColorKeys.IconPrimary, CdsTileTokens.Resolve(CdsTileKind.Selectable, CdsInteractionState.Selected).Icon);
        Assert.Equal(CdsColorKeys.IconPrimary, CdsTileTokens.Resolve(CdsTileKind.Radio, CdsInteractionState.Selected).Icon);
    }

    [Fact]
    public void Non_selectable_tiles_never_get_a_border()
    {
        foreach (var kind in new[] { CdsTileKind.Static, CdsTileKind.Clickable, CdsTileKind.Expandable, CdsTileKind.ExpandableInteractive })
            Assert.Null(CdsTileTokens.Resolve(kind, CdsInteractionState.Selected).Border);
    }

    [Fact]
    public void Expandable_tile_hovers_but_the_interactive_variant_only_hovers_its_chevron()
    {
        Assert.Equal(CdsColorKeys.LayerHover01, CdsTileTokens.Resolve(CdsTileKind.Expandable, CdsInteractionState.Hover).Background);
        var interactive = CdsTileTokens.Resolve(CdsTileKind.ExpandableInteractive, CdsInteractionState.Hover);
        Assert.Equal(CdsColorKeys.Layer01, interactive.Background);
        Assert.Equal(CdsColorKeys.LayerHover01, interactive.ChevronBackground);
        Assert.Null(CdsTileTokens.Resolve(CdsTileKind.ExpandableInteractive, None).ChevronBackground);
        Assert.Equal(CdsColorKeys.IconPrimary, interactive.Icon);
    }

    [Fact]
    public void Tile_text_collects_every_label_in_order()
    {
        var body = new VerticalStackLayout
        {
            new CdsText("View clusters"),
            new Grid { new Label { Text = "12 clusters" }, new BoxView() },
            new Label { Text = "" },
        };
        Assert.Equal("View clusters 12 clusters", CdsTileText.Collect(body));
        Assert.Null(CdsTileText.Collect(new BoxView()));
        Assert.Null(CdsTileText.Collect(null));
    }

    [Fact]
    public void Tiles_can_be_constructed_with_a_body_and_every_property_set()
    {
        var tile = new CdsTile { Body = new CdsText("Storage used") };
        Assert.NotNull(tile.Body);

        var clickable = new CdsClickableTile { Body = new CdsText("View clusters"), Href = "https://example.com", Icon = CdsIconName.Launch, AccessibleName = "Open", IsEnabled = false };
        clickable.Icon = null;
        clickable.Clicked += (_, _) => { };
        Assert.Null(clickable.Icon);

        var selectable = new CdsSelectableTile { Body = new CdsText("Log analysis"), AccessibleName = "Logs" };
        bool? seen = null;
        selectable.SelectedChanged += (_, e) => seen = e.NewValue;
        selectable.IsSelected = true;
        Assert.True(seen);
        selectable.IsEnabled = false;
    }
}

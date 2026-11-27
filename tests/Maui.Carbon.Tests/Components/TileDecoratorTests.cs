using Microsoft.Maui.Controls.Shapes;

namespace Maui.Carbon.Tests.Components;

/// <summary>The AI label decorator of the tiles (Carbon's <c>decorator</c> slot and AI presence).</summary>
public class TileDecoratorTests
{
    const CdsInteractionState None = CdsInteractionState.None;

    static Color Token(string key) => CdsPalettes.GetTokenColor(CdsThemeName.White, key);

    static Grid StackOf(CdsTileBase tile) => Private.Field<Grid>(tile, "Stack");

    static Border SurfaceOf(CdsTileBase tile) => Private.Field<Border>(tile, "Surface");

    // ---- the token resolver

    [Theory]
    [InlineData(CdsTileKind.Static)]
    [InlineData(CdsTileKind.Clickable)]
    [InlineData(CdsTileKind.Selectable)]
    [InlineData(CdsTileKind.Radio)]
    [InlineData(CdsTileKind.Expandable)]
    [InlineData(CdsTileKind.ExpandableInteractive)]
    public void Every_ai_tile_rests_on_the_aura_over_layer_01_with_the_gradient_border(CdsTileKind kind)
    {
        var v = CdsTileTokens.ResolveAi(kind, None);
        Assert.Equal(CdsColorKeys.AiAuraStart, v.AuraKey);
        Assert.Equal(CdsColorKeys.Layer01, v.SurfaceKey);
        Assert.Null(v.Border);
    }

    [Theory]
    [InlineData(CdsTileKind.Clickable)]
    [InlineData(CdsTileKind.Selectable)]
    [InlineData(CdsTileKind.Radio)]
    public void Clickable_selectable_and_radio_tiles_swap_to_the_hover_fill_on_hover_and_press(CdsTileKind kind)
    {
        foreach (var state in new[] { CdsInteractionState.Hover, CdsInteractionState.Pressed })
        {
            var v = CdsTileTokens.ResolveAi(kind, state);
            Assert.Equal(CdsColorKeys.AiAuraHoverStart, v.AuraKey);
            Assert.Equal(CdsColorKeys.AiAuraHoverBackground, v.SurfaceKey);
        }
    }

    [Theory]
    [InlineData(CdsTileKind.Static)]
    [InlineData(CdsTileKind.Expandable)]
    [InlineData(CdsTileKind.ExpandableInteractive)]
    public void Static_and_expandable_tiles_keep_the_default_fill_on_hover(CdsTileKind kind)
    {
        var v = CdsTileTokens.ResolveAi(kind, CdsInteractionState.Hover | CdsInteractionState.Pressed);
        Assert.Equal(CdsColorKeys.AiAuraStart, v.AuraKey);
        Assert.Equal(CdsColorKeys.Layer01, v.SurfaceKey);
    }

    [Theory]
    [InlineData(CdsTileKind.Selectable)]
    [InlineData(CdsTileKind.Radio)]
    public void A_selected_tile_has_the_inverse_border_which_hover_keeps(CdsTileKind kind)
    {
        var selected = CdsTileTokens.ResolveAi(kind, CdsInteractionState.Selected);
        Assert.Equal(CdsColorKeys.BorderInverse, selected.Border);
        Assert.Equal(CdsColorKeys.AiAuraStart, selected.AuraKey);
        Assert.Equal(CdsColorKeys.Layer01, selected.SurfaceKey);

        var hovered = CdsTileTokens.ResolveAi(kind, CdsInteractionState.Selected | CdsInteractionState.Hover);
        Assert.Equal(CdsColorKeys.BorderInverse, hovered.Border);
        Assert.Equal(CdsColorKeys.AiAuraHoverStart, hovered.AuraKey);
    }

    [Theory]
    [InlineData(CdsTileKind.Clickable)]
    [InlineData(CdsTileKind.Selectable)]
    [InlineData(CdsTileKind.Radio)]
    public void A_disabled_ai_tile_keeps_the_default_fill_and_has_no_hover(CdsTileKind kind)
    {
        var v = CdsTileTokens.ResolveAi(kind, CdsInteractionState.Disabled | CdsInteractionState.Hover);
        Assert.Equal(CdsColorKeys.AiAuraStart, v.AuraKey);
        Assert.Equal(CdsColorKeys.Layer01, v.SurfaceKey);
    }

    // ---- the decorator

    [Fact]
    public void The_decorator_sits_above_the_tiles_hit_target_and_is_ai_presence_only_for_an_ai_label()
    {
        var tile = new CdsClickableTile();
        Assert.False(tile.IsAiPresence);
        var label = new CdsAILabel();
        tile.Decorator = label;
        var children = StackOf(tile).Children;
        Assert.True(tile.IsAiPresence);
        Assert.Equal(children.Count - 1, children.IndexOf(label));

        var plain = new BoxView();
        tile.Decorator = plain;
        Assert.False(tile.IsAiPresence);
        Assert.DoesNotContain(label, children);
        Assert.Equal(children.Count - 1, children.IndexOf(plain));

        tile.Decorator = null;
        Assert.DoesNotContain(plain, children);
    }

    [Fact]
    public void Replacing_the_decorator_leaves_one_child()
    {
        var tile = new CdsTile();
        var before = StackOf(tile).Children.Count;
        tile.Decorator = new CdsAILabel();
        tile.Decorator = new CdsAILabel();
        Assert.Equal(before + 1, StackOf(tile).Children.Count);
    }

    [Fact]
    public void Opening_or_reverting_the_label_does_not_change_the_tile()
    {
        var tile = new CdsSelectableTile();
        var label = new CdsAILabel();
        tile.Decorator = label;
        label.IsRevertActive = true;
        label.IsOpen = true;
        Assert.False(tile.IsSelected);
        Assert.True(tile.IsAiPresence);
    }

    [Fact]
    public void Selectable_radio_and_expandable_tiles_force_the_label_to_xs_and_the_others_keep_its_size()
    {
        CdsTileBase[] forced = [new CdsSelectableTile(), new CdsRadioTile(), new CdsExpandableTile()];
        foreach (var tile in forced)
        {
            var label = new CdsAILabel { Size = CdsAILabelSize.Lg };
            tile.Decorator = label;
            Assert.Equal(CdsAILabelSize.Xs, label.Size);
        }
        CdsTileBase[] kept = [new CdsTile(), new CdsClickableTile()];
        foreach (var tile in kept)
        {
            var label = new CdsAILabel { Size = CdsAILabelSize.Lg };
            tile.Decorator = label;
            Assert.Equal(CdsAILabelSize.Lg, label.Size);
        }
    }

    [Fact]
    public void The_label_sits_16_from_the_top_and_16_from_the_end_or_40_beside_a_glyph()
    {
        CdsTileBase[] plain = [new CdsTile(), new CdsClickableTile(), new CdsExpandableTile()];
        foreach (var tile in plain)
        {
            var label = new CdsAILabel();
            tile.Decorator = label;
            Assert.Equal(new Thickness(0, CdsSpacing.Spacing05, CdsSpacing.Spacing05, 0), label.Margin);
            Assert.Equal(LayoutOptions.End, label.HorizontalOptions);
            Assert.Equal(LayoutOptions.Start, label.VerticalOptions);
        }

        var selectable = new CdsSelectableTile();
        var selectableLabel = new CdsAILabel();
        selectable.Decorator = selectableLabel;
        Assert.Equal(new Thickness(0, CdsSpacing.Spacing05, CdsSpacing.Spacing08, 0), selectableLabel.Margin);
    }

    [Fact]
    public void A_radio_tiles_label_moves_from_16_to_40_when_the_check_mark_appears()
    {
        var tile = new CdsRadioTile();
        var label = new CdsAILabel();
        tile.Decorator = label;
        Assert.Equal(CdsSpacing.Spacing05, label.Margin.Right);
        tile.IsChecked = true;
        Assert.Equal(CdsSpacing.Spacing08, label.Margin.Right);
        tile.IsChecked = false;
        Assert.Equal(CdsSpacing.Spacing05, label.Margin.Right);

        var checkedFirst = new CdsRadioTile { IsChecked = true };
        var other = new CdsAILabel();
        checkedFirst.Decorator = other;
        Assert.Equal(CdsSpacing.Spacing08, other.Margin.Right);
    }

    [Fact]
    public void Selectable_and_radio_tiles_keep_72_of_end_padding_with_a_decorator()
    {
        foreach (var tile in new CdsTileBase[] { new CdsSelectableTile(), new CdsRadioTile() })
        {
            var host = Private.Field<ContentView>(tile, "BodyHost");
            Assert.Equal(CdsSpacing.Spacing09, host.Padding.Right);
            tile.Decorator = new CdsAILabel();
            Assert.Equal(72, host.Padding.Right);
            tile.Decorator = null;
            Assert.Equal(CdsSpacing.Spacing09, host.Padding.Right);
        }
    }

    // ---- the AI surface

    [Fact]
    public void An_ai_tile_draws_the_gradient_fill_and_border_and_the_drop_shadow()
    {
        var tile = new CdsTile { Decorator = new CdsAILabel() };
        _ = new CdsTheme { Theme = CdsThemeName.White, Body = tile };
        var surface = SurfaceOf(tile);
        var fill = Assert.IsType<LinearGradientBrush>(surface.Background);
        Assert.Equal(Token("layer-01"), fill.GradientStops[1].Color);
        Assert.Equal(1, surface.StrokeThickness);
        var border = Assert.IsType<LinearGradientBrush>(surface.Stroke);
        Assert.Equal(Token("ai-border-start"), border.GradientStops[0].Color);
        Assert.Equal(Token("ai-border-end"), border.GradientStops[1].Color);
        Assert.NotNull(surface.Shadow);
    }

    [Fact]
    public void A_selected_ai_tile_has_the_inverse_border_and_the_default_fill()
    {
        var tile = new CdsSelectableTile { Decorator = new CdsAILabel(), IsSelected = true };
        _ = new CdsTheme { Theme = CdsThemeName.White, Body = tile };
        var surface = SurfaceOf(tile);
        Assert.IsType<LinearGradientBrush>(surface.Background);
        var stroke = Assert.IsType<SolidColorBrush>(surface.Stroke);
        Assert.Equal(Token("border-inverse"), stroke.Color);

        tile.IsSelected = false;
        Assert.IsType<LinearGradientBrush>(surface.Stroke);
    }

    [Fact]
    public void The_label_in_revert_state_keeps_the_ai_styling_as_in_carbon()
    {
        var label = new CdsAILabel();
        var tile = new CdsTile { Decorator = label };
        _ = new CdsTheme { Theme = CdsThemeName.White, Body = tile };
        label.IsRevertActive = true;
        Assert.True(tile.IsAiPresence);
        Assert.IsType<LinearGradientBrush>(SurfaceOf(tile).Background);
        label.IsRevertActive = false;
        Assert.True(tile.IsAiPresence);
    }

    [Fact]
    public void Removing_the_decorator_restores_the_flat_fill_and_no_border_or_shadow()
    {
        var tile = new CdsTile { Decorator = new CdsAILabel() };
        _ = new CdsTheme { Theme = CdsThemeName.White, Body = tile };
        tile.Decorator = null;
        var surface = SurfaceOf(tile);
        Assert.Equal(0, surface.StrokeThickness);
        Assert.Equal(Token("layer-01"), surface.BackgroundColor);
        Assert.IsNotType<LinearGradientBrush>(surface.Background);
        Assert.IsNotType<LinearGradientBrush>(surface.Stroke);
        Assert.Null(surface.Shadow);
    }

    [Fact]
    public void A_selectable_tile_returns_to_its_flat_border_without_the_decorator()
    {
        var tile = new CdsSelectableTile { Decorator = new CdsAILabel(), IsSelected = true };
        _ = new CdsTheme { Theme = CdsThemeName.White, Body = tile };
        tile.Decorator = null;
        var surface = SurfaceOf(tile);
        Assert.Equal(1, surface.StrokeThickness);
        var stroke = Assert.IsType<SolidColorBrush>(surface.Stroke);
        Assert.Equal(Token("layer-selected-inverse"), stroke.Color);
    }

    [Fact]
    public void Rounded_corners_apply_only_with_an_ai_label()
    {
        var tile = new CdsTile { HasRoundedCorners = true };
        Assert.IsType<Rectangle>(SurfaceOf(tile).StrokeShape);
        tile.Decorator = new BoxView();
        Assert.IsType<Rectangle>(SurfaceOf(tile).StrokeShape);
        tile.Decorator = new CdsAILabel();
        var round = Assert.IsType<RoundRectangle>(SurfaceOf(tile).StrokeShape);
        Assert.Equal(new CornerRadius(CdsSpacing.Spacing03), round.CornerRadius);
        tile.HasRoundedCorners = false;
        Assert.IsType<Rectangle>(SurfaceOf(tile).StrokeShape);
    }

    // ---- the expandable tile

    [Fact]
    public void A_decorator_makes_an_expandable_tile_the_interactive_variant()
    {
        var tile = new CdsExpandableTile { Body = new CdsText("Cluster summary") };
        Assert.False(tile.IsInteractiveVariant);
        tile.Decorator = new CdsAILabel();
        Assert.True(tile.IsInteractiveVariant);
        tile.Decorator = null;
        Assert.False(tile.IsInteractiveVariant);
    }

    [Fact]
    public void An_explicit_variant_wins_over_the_decorator()
    {
        var tile = new CdsExpandableTile { Body = new CdsText("Cluster summary"), IsInteractive = false, Decorator = new CdsAILabel() };
        Assert.False(tile.IsInteractiveVariant);
        Assert.True(ExpandableTileModel.UseInteractiveVariant(null, null, null, hasDecorator: true));
        Assert.False(ExpandableTileModel.UseInteractiveVariant(false, null, null, hasDecorator: true));
    }

    [Fact]
    public void An_ai_expandable_tile_keeps_its_borders_inside_the_collapsed_height()
    {
        var tile = new CdsExpandableTile { Body = new CdsText("Cluster summary"), Decorator = new CdsAILabel(), HasRoundedCorners = true };
        Assert.Equal(1, SurfaceOf(tile).StrokeThickness);
    }

    // ---- the group

    [Fact]
    public void A_group_with_decorated_tiles_still_handles_arrow_keys()
    {
        var one = new CdsRadioTile { Value = "one", Body = new CdsText("One"), Decorator = new CdsAILabel() };
        var two = new CdsRadioTile { Value = "two", Body = new CdsText("Two") };
        var group = new CdsTileGroup { Legend = "Plan", Body = new VerticalStackLayout { one, two } };
        Assert.True(group.OnKeyDown(new CdsKeyEventArgs(CdsKey.Down)));
    }

    // ---- construction

    [Fact]
    public void Every_tile_takes_every_decorator_property_without_a_platform()
    {
        CdsTileBase[] tiles =
        [
            new CdsTile(), new CdsClickableTile(), new CdsSelectableTile(), new CdsRadioTile(), new CdsExpandableTile(),
        ];
        foreach (var tile in tiles)
        {
            tile.HasRoundedCorners = true;
            tile.Decorator = new CdsAILabel { IsRevertActive = true };
            tile.Decorator = new CdsAILabel();
            tile.IsEnabled = false;
            tile.IsEnabled = true;
            tile.Decorator = null;
            Assert.False(tile.IsAiPresence);
        }
    }
}

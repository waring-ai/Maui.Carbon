using Microsoft.Maui.Controls.Shapes;

namespace Maui.Carbon.Tests.Components;

/// <summary>The AI presence of CdsModal and CdsComposedModal: the decorator slot, the AI container and the AI scrim.</summary>
public class ModalAiTests
{
    static Color Token(string key) => CdsPalettes.GetTokenColor(CdsThemeName.White, key);

    static (CdsOverlayHost Host, VerticalStackLayout Page, CdsTheme Theme) Themed()
    {
        var page = new VerticalStackLayout();
        var host = new CdsOverlayHost { Body = page };
        // An element holds its parent weakly: the caller keeps the theme alive for the whole test.
        var theme = new CdsTheme { Theme = CdsThemeName.White, Body = host };
        return (host, page, theme);
    }

    static CdsModal OpenModal(VerticalStackLayout page, View? decorator, string? primary = "Save summary")
    {
        var modal = new CdsModal { Heading = "Summarize incident", PrimaryButtonText = primary, SecondaryButtonText = "Cancel", Decorator = decorator };
        page.Add(modal);
        modal.IsOpen = true;
        modal.Show();
        return modal;
    }

    static Thickness MarginOf(View view) => view.Margin;

    [Fact]
    public void An_ai_label_decorator_is_shown_small_in_the_header_before_the_close_button()
    {
        var (_, page, theme) = Themed();
        var label = new CdsAILabel { Size = CdsAILabelSize.Lg };
        var modal = OpenModal(page, label);
        var header = modal.Surface!.Header!;

        Assert.True(modal.IsAiPresence);
        Assert.Same(label, header.Decorator);
        Assert.Equal(CdsAILabelSize.Sm, label.Size);
        var root = Private.Field<Grid>(header, "_root");
        var close = Private.Field<CdsModalCloseButton>(header, "_close");
        Assert.True(root.Children.IndexOf(label) < root.Children.IndexOf(close));
        Assert.Equal(LayoutOptions.End, label.HorizontalOptions);
        Assert.Equal(new Thickness(0, 10, 48, 0), MarginOf(label));
        modal.IsOpen = false;
        GC.KeepAlive(theme);
    }

    [Fact]
    public void Without_a_decorator_the_modal_keeps_the_flat_fill_and_border()
    {
        var (_, page, theme) = Themed();
        var modal = OpenModal(page, null);
        var box = modal.Surface!.Box;
        Assert.False(modal.IsAiPresence);
        Assert.False(modal.Surface.IsAi);
        Assert.Null(modal.Surface.ScrimColorKey);
        Assert.Equal(Token("layer-01"), box.BackgroundColor);
        Assert.Equal(Token("border-subtle-01"), Assert.IsType<SolidColorBrush>(box.Stroke).Color);
        Assert.Null(box.Shadow);
        modal.IsOpen = false;
        GC.KeepAlive(theme);
    }

    [Fact]
    public void An_ai_modal_has_the_aura_fill_the_gradient_border_and_the_drop_shadow()
    {
        var (_, page, theme) = Themed();
        var modal = OpenModal(page, new CdsAILabel());
        var surface = modal.Surface!;
        var box = surface.Box;
        Assert.True(surface.IsAi);
        Assert.IsType<LinearGradientBrush>(box.Background);
        var stroke = Assert.IsType<LinearGradientBrush>(box.Stroke);
        Assert.Equal(Token("ai-border-start"), stroke.GradientStops[0].Color);
        Assert.Equal(Token("ai-border-end"), stroke.GradientStops[^1].Color);
        Assert.Equal(1, box.StrokeThickness);
        Assert.NotNull(box.Shadow);
        Assert.Equal(24, box.Shadow.Offset.Y);
        Assert.Equal(40, box.Shadow.Radius);
        modal.IsOpen = false;
        GC.KeepAlive(theme);
    }

    [Fact]
    public void Revert_keeps_the_ai_presence_as_Carbons_selectors_do_and_moves_the_label()
    {
        var (_, page, theme) = Themed();
        var label = new CdsAILabel();
        var modal = OpenModal(page, label);
        var surface = modal.Surface!;

        label.IsRevertActive = true;
        Assert.True(surface.IsAi);
        Assert.IsType<LinearGradientBrush>(surface.Box.Background);
        Assert.Equal(CdsColorKeys.AiOverlay, surface.ScrimColorKey);
        // 1.475rem is the center line of the revert button: 23.6 - 32 / 2.
        Assert.Equal(7.6, MarginOf(label).Top, 3);
        Assert.Equal(48, MarginOf(label).Right);

        label.IsRevertActive = false;
        Assert.Equal(10, MarginOf(label).Top);
        modal.IsOpen = false;
        GC.KeepAlive(theme);
    }

    [Fact]
    public void Another_view_is_placed_one_rem_from_the_top_without_ai_styling()
    {
        var (_, page, theme) = Themed();
        var tag = new CdsTag { Text = "Draft" };
        var modal = OpenModal(page, tag);
        Assert.False(modal.IsAiPresence);
        Assert.False(modal.Surface!.IsAi);
        Assert.Same(tag, modal.Surface.Header!.Decorator);
        Assert.Equal(16, MarginOf(tag).Top);
        Assert.Equal(Token("layer-01"), modal.Surface.Box.BackgroundColor);
        modal.IsOpen = false;
        GC.KeepAlive(theme);
    }

    [Fact]
    public void Setting_and_removing_the_decorator_while_open_switches_the_presence_and_restores_the_flat_tokens()
    {
        var (_, page, theme) = Themed();
        var modal = OpenModal(page, null);
        var surface = modal.Surface!;

        modal.Decorator = new CdsAILabel();
        Assert.True(surface.IsAi);

        modal.Decorator = null;
        Assert.False(surface.IsAi);
        // The border is the token again, not the transparent the treatment leaves.
        Assert.Equal(Token("border-subtle-01"), Assert.IsType<SolidColorBrush>(surface.Box.Stroke).Color);
        Assert.Equal(Token("layer-01"), surface.Box.BackgroundColor);
        Assert.Null(surface.Box.Shadow);
        modal.IsOpen = false;
        GC.KeepAlive(theme);
    }

    [Fact]
    public void The_decorator_is_handed_back_when_the_modal_closes_and_shown_again_on_reopen()
    {
        var (_, page, theme) = Themed();
        var label = new CdsAILabel();
        var modal = OpenModal(page, label);
        modal.IsOpen = false;
        Assert.Null(label.Parent);
        modal.IsOpen = true;
        modal.Show();
        Assert.Same(label, modal.Surface!.Header!.Decorator);
        Assert.True(modal.Surface.IsAi);
        modal.IsOpen = false;
        GC.KeepAlive(theme);
    }

    [Fact]
    public void The_ai_scrim_colors_the_hosts_scrim_and_the_default_returns_with_a_plain_modal_on_top()
    {
        var (host, page, theme) = Themed();
        var scrim = Private.Field<BoxView>(host, "_scrim");
        var ai = OpenModal(page, new CdsAILabel());
        Assert.Single(host.Overlays);
        Assert.True(ai.Surface!.IsAi, "surface");
        Assert.Equal(CdsColorKeys.AiOverlay, host.Overlays[0].Options.ScrimColorKey);
        Assert.Equal(Token("ai-overlay"), scrim.Color);

        var plain = OpenModal(page, null);
        Assert.Equal(Token("overlay"), scrim.Color);

        plain.IsOpen = false;
        Assert.Equal(Token("ai-overlay"), scrim.Color);
        ai.IsOpen = false;
        GC.KeepAlive(theme);
    }

    [Fact]
    public void The_scrim_follows_the_decorator_while_the_modal_is_open()
    {
        var (host, page, theme) = Themed();
        var scrim = Private.Field<BoxView>(host, "_scrim");
        var modal = OpenModal(page, null);
        Assert.Equal(Token("overlay"), scrim.Color);
        modal.Decorator = new CdsAILabel();
        Assert.Equal(Token("ai-overlay"), scrim.Color);
        modal.Decorator = null;
        Assert.Equal(Token("overlay"), scrim.Color);
        modal.IsOpen = false;
        GC.KeepAlive(theme);
    }

    [Fact]
    public void A_footer_keeps_its_band_flat_and_the_fade_paints_the_fill_under_it()
    {
        var (_, page, theme) = Themed();
        var modal = OpenModal(page, new CdsAILabel());
        var surface = modal.Surface!;
        Assert.NotNull(surface.Footer);
        // The fill is only built once the box has a height; build it for a size.
        var colors = new CdsModalAiColors(Token("ai-aura-start"), Token("ai-aura-end"), Token("layer-01"), Token("ai-inner-shadow"));
        var fill = CdsModalAiFill.CreateFill(colors, 400, CdsModalSizing.FooterHeight);
        Assert.Equal(Token("layer-01"), CdsModalAiFill.ColorAt(colors, 400, 64, 30));
        Assert.Equal(Token("layer-01"), CdsModalAiFill.ColorAt(colors, 400, 64, 64));
        Assert.NotEqual(Token("layer-01"), CdsModalAiFill.ColorAt(colors, 400, 64, 80));
        Assert.True(fill.GradientStops.Count > 10);
        modal.IsOpen = false;
        GC.KeepAlive(theme);
    }

    [Fact]
    public void Without_a_footer_the_aura_starts_at_the_bottom_edge_and_fades_out_by_half_the_height()
    {
        var colors = new CdsModalAiColors(Token("ai-aura-start"), Token("ai-aura-end"), Token("layer-01"), Token("ai-inner-shadow"));
        var surface = Token("layer-01");
        var bottom = CdsModalAiFill.ColorAt(colors, 600, 0, 0);
        Assert.NotEqual(surface, bottom);
        Assert.Equal(1, bottom.Alpha, 3);
        // Past 50% of the height and past the glow there is only the surface.
        Assert.Equal(surface, CdsModalAiFill.ColorAt(colors, 600, 0, 300));
        // Strictly between the aura and the surface by 15% (the gradient hint) the aura has lost half its strength.
        var atHint = CdsModalAiFill.ColorAt(colors, 1000, 0, 150);
        var atStart = CdsModalAiFill.ColorAt(colors, 1000, 0, 1);
        Assert.True(Math.Abs(atHint.Blue - surface.Blue) < Math.Abs(atStart.Blue - surface.Blue) + 1e-6);
    }

    [Fact]
    public void The_footer_fill_stops_stay_in_order_and_end_on_the_surface()
    {
        var colors = new CdsModalAiColors(Token("ai-aura-start"), Token("ai-aura-end"), Token("layer-01"), Token("ai-inner-shadow"));
        foreach (var footer in new[] { 0d, 64d })
        {
            var fill = CdsModalAiFill.CreateFill(colors, 480, footer);
            var offsets = fill.GradientStops.Select(s => s.Offset).ToArray();
            Assert.Equal(offsets.OrderBy(o => o), offsets);
            Assert.Equal(0, offsets[0]);
            Assert.Equal(1, offsets[^1]);
            Assert.Equal(Token("layer-01"), fill.GradientStops[^1].Color);
        }
        var fade = CdsModalAiFill.CreateFade(colors, 480, 64);
        Assert.Equal(0, fade.GradientStops[0].Color.Alpha, 3);
        Assert.Equal(1, fade.GradientStops[^1].Color.Alpha, 3);
    }

    [Fact]
    public void The_inner_glow_follows_a_blurred_edge()
    {
        Assert.Equal(0.5, CdsModalAiFill.Phi(0), 4);
        Assert.Equal(0.8413, CdsModalAiFill.Phi(1), 3);
        Assert.Equal(0.1587, CdsModalAiFill.Phi(-1), 3);
        // The band is 80 - 16 = 64px, the footer's height.
        Assert.Equal(CdsModalSizing.FooterHeight, CdsModalAiFill.BandHeight);
    }

    [Fact]
    public void The_decorator_top_margin_follows_the_modal_scss()
    {
        Assert.Equal(10, CdsModalSizing.DecoratorTopMargin(true, false, 32));
        Assert.Equal(23.6 - 16, CdsModalSizing.DecoratorTopMargin(true, true, 32), 6);
        Assert.Equal(16, CdsModalSizing.DecoratorTopMargin(false, false, 20));
        Assert.Equal(16, CdsModalSizing.DecoratorTopMargin(false, true, 20));
    }

    [Fact]
    public void The_heading_stops_short_of_the_decorator_on_a_narrow_modal_and_is_unchanged_without_one()
    {
        var header = new CdsModalHeader { Title = "Summarize incident" };
        header.Measure(300, double.PositiveInfinity);
        header.Arrange(new Rect(0, 0, 300, 100));
        var title = Private.Field<CdsText>(header, "_title");
        Assert.Equal(CdsModalSizing.HeadingEndPadding(300), title.Margin.Right);

        header.Decorator = new CdsAILabel();
        Assert.Equal(32 + 8, title.Margin.Right);

        header.Decorator = null;
        Assert.Equal(CdsModalSizing.HeadingEndPadding(300), title.Margin.Right);
    }

    [Fact]
    public void A_dialog_and_its_surface_are_unchanged()
    {
        var header = new CdsModalHeader { Title = "Delete" };
        var surface = new CdsModalSurface(CdsModalMode.Modal, CdsModalSize.Md, true, header, new CdsModalBody(), null);
        Assert.False(surface.IsAi);
        Assert.Null(surface.ScrimColorKey);
        Assert.Null(surface.Box.Shadow);
        surface.Release();
    }

    [Fact]
    public void A_composed_modal_forwards_its_decorator_to_the_header_part_and_hands_it_back()
    {
        var (host, page, theme) = Themed();
        var scrim = Private.Field<BoxView>(host, "_scrim");
        var header = new CdsModalHeader { Title = "Generating summary" };
        var modal = new CdsComposedModal { Parts = { header, new CdsModalBody { Body = new CdsText("Working") } } };
        var label = new CdsAILabel();
        modal.Decorator = label;
        page.Add(modal);
        modal.IsOpen = true;
        modal.Show();

        Assert.True(modal.IsAiPresence);
        Assert.Same(label, header.Decorator);
        Assert.True(header.IsAiPresence, "header");
        var surf = Private.Field<CdsModalSurface>(modal, "_surface");
        Assert.True(surf.IsAi, "surface");
        Assert.Equal(Token("ai-overlay"), scrim.Color);

        // Changing it while open updates the live header.
        var other = new CdsAILabel();
        modal.Decorator = other;
        Assert.Same(other, header.Decorator);
        Assert.Null(label.Parent);

        modal.IsOpen = false;
        Assert.Null(header.Decorator);
        Assert.Null(other.Parent);
        // Nothing listens to the label any more: changing it after close touches no surface.
        other.IsRevertActive = true;
        label.IsRevertActive = true;
        GC.KeepAlive(theme);
    }

    [Fact]
    public void A_composed_modal_without_a_header_part_ignores_the_decorator()
    {
        var (_, page, theme) = Themed();
        var modal = new CdsComposedModal { Parts = { new CdsModalBody { Body = new CdsText("Working") } }, Decorator = new CdsAILabel() };
        page.Add(modal);
        modal.IsOpen = true;
        modal.Show();
        Assert.True(modal.IsOpen);
        modal.IsOpen = false;
        GC.KeepAlive(theme);
    }
}

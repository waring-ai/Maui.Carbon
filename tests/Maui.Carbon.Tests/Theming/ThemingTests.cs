using Microsoft.Maui.Graphics;

namespace Maui.Carbon.Tests.Theming;

public class ThemingTests
{
    static Color Token(CdsThemeName theme, string token) => CdsPalettes.GetTokenColor(theme, token);

    static Label Probe(string key = CdsColorKeys.Layer01)
    {
        var label = new Label();
        label.SetDynamicResource(Label.TextColorProperty, key);
        return label;
    }

    [Fact]
    public void Switching_themes_in_any_order_resolves_every_color_of_the_current_theme()
    {
        var resources = new CdsResources();
        var probes = CdsColorKeys.All.Select(k => Probe(k)).ToList();
        var root = new VerticalStackLayout();
        foreach (var p in probes) root.Add(p);
        root.Resources = resources;
        CdsThemeName[] order = [CdsThemeName.G10, CdsThemeName.G100, CdsThemeName.G90, CdsThemeName.White, CdsThemeName.G100, CdsThemeName.G10, CdsThemeName.G90, CdsThemeName.White];
        foreach (var theme in order)
        {
            resources.Theme = theme;
            var colors = CdsPaletteData.Get(theme);
            for (var i = 0; i < probes.Count; i++) Assert.Equal(colors[i], probes[i].TextColor);
            Assert.True(resources.TryGetValue(CdsContextKeys.ThemeId, out var id));
            Assert.Equal(theme, id);
        }
    }

    [Fact]
    public void A_theme_switch_raises_only_the_colors_that_differ_from_the_first_theme()
    {
        // White and G10 share all but 17 colors; the theme id is always raised, constant context keys never.
        var delta = CdsPalettes.CreateThemeDelta(CdsThemeName.White, CdsThemeName.White, CdsThemeName.G10);
        Assert.Equal(18, delta.Count);
        Assert.False(delta.ContainsKey(CdsContextKeys.LayerLevel));
        Assert.False(delta.ContainsKey(CdsContextKeys.Transparent));
        // Back to the first theme: the 17 that differed revert, with the first theme's values.
        var back = CdsPalettes.CreateThemeDelta(CdsThemeName.White, CdsThemeName.G10, CdsThemeName.White);
        Assert.Equal(18, back.Count);
        Assert.Equal(Token(CdsThemeName.White, "background"), back[CdsColorKeys.Background]);
        Assert.Equal(Token(CdsThemeName.G10, "background"), delta[CdsColorKeys.Background]);
    }

    [Fact]
    public void Every_theme_has_every_color()
    {
        foreach (var theme in Enum.GetValues<CdsThemeName>())
            Assert.Equal(CdsColorKeys.All.Count, CdsPaletteData.Get(theme).Length);
        // 197 Carbon theme tokens plus the 21 ai-* tokens.
        Assert.Equal(218, CdsColorKeys.All.Count);
    }

    [Fact]
    public void Only_the_three_known_tokens_inherit_values()
    {
        Assert.Equal(["status-orange-outline", "status-yellow-outline", "notification-action-hover"], CdsPaletteData.InheritedTokens);
    }

    [Fact]
    public void Palette_values_match_tokens_json()
    {
        Assert.Equal(Color.FromArgb("#0f62fe"), Token(CdsThemeName.White, "focus"));
        Assert.Equal(Color.FromArgb("#ffffff"), Token(CdsThemeName.G100, "focus"));
        Assert.Equal(Color.FromArgb("#262626"), Token(CdsThemeName.G100, "layer-01"));
        var overlay = Token(CdsThemeName.White, "overlay");
        Assert.Equal(153 / 255f, overlay.Alpha, 3);
    }

    [Fact]
    public void Zone_applies_its_theme()
    {
        var label = Probe();
        _ = new CdsTheme { Theme = CdsThemeName.G90, Body = label };
        Assert.Equal(Token(CdsThemeName.G90, "layer-01"), label.TextColor);
    }

    [Fact]
    public void Nested_layers_step_up_and_stop_at_three()
    {
        var label = Probe();
        var deepest = new CdsLayer { Body = label };
        _ = new CdsTheme
        {
            Theme = CdsThemeName.G90,
            Body = new CdsLayer { Body = new CdsLayer { Body = deepest } },
        };
        Assert.Equal(3, deepest.EffectiveLevel);
        Assert.Equal(Token(CdsThemeName.G90, "layer-03"), label.TextColor);
        Assert.Equal(3, CdsLayer.GetLevel(label));
    }

    [Fact]
    public void Layer_follows_a_runtime_zone_theme_switch()
    {
        var label = Probe(CdsColorKeys.Field01);
        var zone = new CdsTheme { Theme = CdsThemeName.G100, Body = new CdsLayer { Body = label } };
        Assert.Equal(Token(CdsThemeName.G100, "field-02"), label.TextColor);

        zone.Theme = CdsThemeName.White;
        Assert.Equal(Token(CdsThemeName.White, "field-02"), label.TextColor);
        Assert.Equal(CdsThemeName.White, CdsTheme.GetEffectiveTheme(label));
    }

    [Fact]
    public void Reparenting_a_layer_recomputes_its_level()
    {
        var label = Probe();
        var inner = new CdsLayer { Body = label };
        var outer = new CdsLayer { Body = inner };
        var stack = new VerticalStackLayout { outer };
        _ = new CdsTheme { Theme = CdsThemeName.G10, Body = stack };
        Assert.Equal(Token(CdsThemeName.G10, "layer-03"), label.TextColor);

        outer.Body = null;
        stack.Add(inner);
        Assert.Equal(2, inner.EffectiveLevel);
        Assert.Equal(Token(CdsThemeName.G10, "layer-02"), label.TextColor);
    }

    [Fact]
    public void Zone_inside_a_layer_resets_to_level_one()
    {
        var label = Probe(CdsColorKeys.BorderSubtle01);
        _ = new CdsTheme
        {
            Theme = CdsThemeName.White,
            Body = new CdsLayer { Body = new CdsTheme { Theme = CdsThemeName.G100, Body = label } },
        };
        Assert.Equal(Token(CdsThemeName.G100, "border-subtle-01"), label.TextColor);
        Assert.Equal(1, CdsLayer.GetLevel(label));
    }

    [Fact]
    public void Explicit_level_one_resets_inside_a_layer()
    {
        var label = Probe();
        _ = new CdsTheme
        {
            Theme = CdsThemeName.G90,
            Body = new CdsLayer { Body = new CdsLayer { Level = 1, Body = label } },
        };
        Assert.Equal(Token(CdsThemeName.G90, "layer-01"), label.TextColor);
    }

    [Fact]
    public void App_theme_switch_updates_layers_outside_zones()
    {
        var resources = new CdsResources();
        var label = Probe();
        var root = new ContentView { Content = new CdsLayer { Body = label } };
        root.Resources = resources;
        Assert.Equal(Token(CdsThemeName.White, "layer-02"), label.TextColor);

        resources.Theme = CdsThemeName.G100;
        Assert.Equal(Token(CdsThemeName.G100, "layer-02"), label.TextColor);
    }

    [Fact]
    public void Non_layer_tokens_pass_through_layers()
    {
        var label = Probe(CdsColorKeys.TextPrimary);
        _ = new CdsTheme { Theme = CdsThemeName.G100, Body = new CdsLayer { Body = label } };
        Assert.Equal(Token(CdsThemeName.G100, "text-primary"), label.TextColor);
    }

    [Fact]
    public void Type_specs_convert_rem_to_px()
    {
        var body = CdsTypeSpec.Of(CdsTypeStyle.BodyCompact01);
        Assert.Equal(14, body.FontSize);
        Assert.Equal(0.16, body.CharacterSpacing);
        Assert.Equal(CdsFonts.Sans, body.FontFamily);
        Assert.Equal(-1, body.MauiLineHeight);
        Assert.Equal(CdsFonts.SansSemiBold, CdsTypeSpec.Of(CdsTypeStyle.HeadingCompact01).FontFamily);
        Assert.Equal(CdsFonts.SansLight, CdsTypeSpec.Of(CdsTypeStyle.Heading06).FontFamily);
        Assert.Equal(CdsFonts.Mono, CdsTypeSpec.Of(CdsTypeStyle.Code02).FontFamily);
        Assert.Equal(16, CdsSpacing.Spacing05);
        Assert.Equal(48, CdsSizing.Large);
        Assert.Equal(70u, CdsDuration.Fast01);
    }

    [Theory]
    [InlineData(0.2, 0, 0.38, 0.9)]
    [InlineData(0, 0, 0.38, 0.9)]
    [InlineData(0.4, 0.14, 0.3, 1)]
    public void Cubic_bezier_is_monotonic_with_fixed_endpoints(double x1, double y1, double x2, double y2)
    {
        Assert.Equal(0, CdsCubicBezier.Evaluate(x1, y1, x2, y2, 0));
        Assert.Equal(1, CdsCubicBezier.Evaluate(x1, y1, x2, y2, 1));
        var last = 0.0;
        for (var x = 0.05; x < 1; x += 0.05)
        {
            var y = CdsCubicBezier.Evaluate(x1, y1, x2, y2, x);
            Assert.True(y >= last - 1e-9, $"not monotonic at {x}");
            last = y;
        }
        // linear curve sanity
        Assert.Equal(0.5, CdsCubicBezier.Evaluate(1 / 3.0, 1 / 3.0, 2 / 3.0, 2 / 3.0, 0.5), 4);
    }

    [Fact]
    public void Breakpoints_resolve_from_width()
    {
        Assert.Equal(CdsBreakpoint.Sm, CdsBreakpointExtensions.FromWidth(400));
        Assert.Equal(CdsBreakpoint.Md, CdsBreakpointExtensions.FromWidth(672));
        Assert.Equal(CdsBreakpoint.Lg, CdsBreakpointExtensions.FromWidth(1100));
        Assert.Equal(CdsBreakpoint.Max, CdsBreakpointExtensions.FromWidth(2000));
        Assert.Equal(16, CdsBreakpoint.Lg.Columns());
    }
}

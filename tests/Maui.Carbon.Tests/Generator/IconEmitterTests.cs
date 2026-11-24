using System.Globalization;
using System.Text.RegularExpressions;
using Maui.Carbon.Generator;

namespace Maui.Carbon.Tests.Generator;

public sealed class SvgPathParserTests
{
    static string Parse(string d) => SvgPathParser.Parse(d).ToDataString();

    [Fact]
    public void Relative_commands_become_absolute()
    {
        Assert.Equal("M 1 2 L 4 6 L 5 6 L 5 5 Z", Parse("M1 2 l3 4 h1 v-1 z"));
        Assert.Equal("M 1 2 C 2 3 3 3 4 2", Parse("m1 2 c1 1 2 1 3 0"));
    }

    [Fact]
    public void Implicit_repeats_are_linetos()
    {
        Assert.Equal("M 0 0 L 10 0 L 10 10", Parse("M0 0 10 0 10 10"));
        Assert.Equal("M 1 1 L 3 3 L 6 6", Parse("m1 1 2 2 3 3"));
        Assert.Equal("M 0 0 L 1 0 L 2 0", Parse("M0,0H1 2"));
    }

    [Fact]
    public void Compact_numbers_are_split()
    {
        Assert.Equal("M 1 -2.5 L 0.5 -1", Parse("M1-2.5.5-1"));
        Assert.Equal("M 0.001 0.5 L 10 0", Parse("M1e-3.5 10 0"));
    }

    [Fact]
    public void Smooth_cubic_reflects_the_control_point()
    {
        Assert.Equal("M 0 0 C 0 10 10 10 10 0 C 10 -10 20 -10 20 0", Parse("M0 0C0 10 10 10 10 0S20-10 20 0"));
        // S after a non-cubic uses the current point as the first control point.
        Assert.Equal("M 0 0 L 5 0 C 5 0 6 1 7 0", Parse("M0 0L5 0S6 1 7 0"));
    }

    [Fact]
    public void Quadratics_are_elevated_to_cubics()
    {
        Assert.Equal("M 0 0 C 2 2 4 2 6 0", Parse("M0 0Q3 3 6 0"));
        Assert.Equal("M 0 0 C 2 2 4 2 6 0 C 8 -2 10 -2 12 0", Parse("M0 0Q3 3 6 0T12 0"));
    }

    [Fact]
    public void Close_then_draw_starts_a_new_subpath_at_the_start_point()
    {
        Assert.Equal("M 0 0 L 4 0 L 4 4 Z M 0 0 L 0 4", Parse("M0 0h4v4zv4"));
    }

    [Fact]
    public void Zero_radius_arc_is_a_line()
    {
        Assert.Equal("M 0 0 L 5 5", Parse("M0 0A0 3 0 0 1 5 5"));
    }

    [Fact]
    public void Two_arcs_make_a_circle()
    {
        var path = SvgPathParser.Parse("M0 5A5 5 0 1 1 10 5A5 5 0 1 1 0 5Z");
        const double cx = 5.0, cy = 5.0, r = 5.0;
        var (px, py) = (0.0, 5.0);
        var curves = 0;
        foreach (var s in path.Segments)
        {
            if (s.Op == 'M') { (px, py) = (s.Coords[0], s.Coords[1]); continue; }
            if (s.Op != 'C') continue;
            curves++;
            var c = s.Coords;
            foreach (var t in new[] { 0.25, 0.5, 0.75 })
            {
                var u = 1 - t;
                var x = u * u * u * px + 3 * u * u * t * c[0] + 3 * u * t * t * c[2] + t * t * t * c[4];
                var y = u * u * u * py + 3 * u * u * t * c[1] + 3 * u * t * t * c[3] + t * t * t * c[5];
                Assert.InRange(Math.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy)), r - 0.01, r + 0.01);
            }
            Assert.InRange(Math.Sqrt((c[4] - cx) * (c[4] - cx) + (c[5] - cy) * (c[5] - cy)), r - 1e-9, r + 1e-9);
            (px, py) = (c[4], c[5]);
        }
        Assert.Equal(4, curves);
        Assert.Equal(0, px, 9);
        Assert.Equal(5, py, 9);
    }

    [Fact]
    public void Undersized_radii_are_scaled_up()
    {
        // Radius 1 cannot span a chord of 10: it becomes 5 and gives a half circle.
        var path = SvgPathParser.Parse("M0 0A1 1 0 0 1 10 0");
        Assert.Equal(2, path.Segments.Count(s => s.Op == 'C'));
        var last = path.Segments[^1].Coords;
        Assert.Equal(10, last[4], 9);
        Assert.Equal(0, last[5], 9);
        foreach (var p in path.Points()) Assert.InRange(p.Y, -5.01, 0.01);
    }

    [Fact]
    public void Sweep_flag_picks_the_side()
    {
        var up = SvgPathParser.Parse("M0 0A5 5 0 0 1 10 0").Bounds()!.Value;
        var down = SvgPathParser.Parse("M0 0A5 5 0 0 0 10 0").Bounds()!.Value;
        Assert.True(up.MinY < -4); // sweep=1 is clockwise in y-down space: through negative y
        Assert.True(down.MaxY > 4);
    }

    [Fact]
    public void Compact_arc_flags_parse() =>
        Assert.Equal(Parse("M0 5A5 5 0 1 1 10 5"), Parse("M0 5a5 5 0 1110 0"));

    [Fact]
    public void Numbers_are_formatted_invariantly_without_exponents()
    {
        Assert.Equal("0", PathData.FormatNumber(-0.0001));
        Assert.Equal("1.235", PathData.FormatNumber(1.23456));
        Assert.Equal("12", PathData.FormatNumber(12.0));
        Assert.Equal("0", PathData.FormatNumber(1e-9));
    }
}

public sealed class AffineTests
{
    static void Near((double X, double Y) actual, double x, double y)
    {
        Assert.Equal(x, actual.X, 9);
        Assert.Equal(y, actual.Y, 9);
    }

    [Fact]
    public void Translate() => Near(Affine.Parse("translate(10 20)").Apply(1, 2), 11, 22);

    [Fact]
    public void Translate_with_one_argument() => Near(Affine.Parse("translate(5)").Apply(1, 2), 6, 2);

    [Fact]
    public void Rotate_about_a_point() => Near(Affine.Parse("rotate(90 8 8)").Apply(16, 8), 8, 16);

    [Fact]
    public void Rotate_about_centre_keeps_centre() => Near(Affine.Parse("rotate(-45 16 16)").Apply(16, 16), 16, 16);

    [Fact]
    public void Matrix() => Near(Affine.Parse("matrix(2 0 0 3 1 1)").Apply(1, 1), 3, 4);

    [Fact]
    public void Several_transforms_apply_right_to_left() =>
        Near(Affine.Parse("translate(10 0) scale(2)").Apply(1, 1), 12, 2);

    [Fact]
    public void Skew() => Near(Affine.Parse("skewX(45)").Apply(0, 2), 2, 2);

    [Fact]
    public void Carbon_style_translate_rotate() =>
        Near(Affine.Parse("translate(0 32) rotate(-90)").Apply(4, 0), 0, 28);

    [Fact]
    public void Transform_is_baked_into_path_points()
    {
        var svg = """<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 16 16"><g transform="translate(2 0)"><path transform="scale(2)" d="M1 1h2v2z"/></g></svg>""";
        var g = SvgIconConverter.Convert(svg, "t-16.svg");
        Assert.Equal("M 4 2 L 8 2 L 8 6 Z", g.Paths.Single().Data);
    }

    [Fact]
    public void Arcs_are_converted_before_transforming()
    {
        var svg = """<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 16 16"><path transform="translate(8 0)" d="M0 8a4 4 0 1 0 0.001 0z"/></svg>""";
        var g = SvgIconConverter.Convert(svg, "t-16.svg");
        Assert.DoesNotContain("A", g.Paths.Single().Data);
        Assert.StartsWith("M 8 8", g.Paths.Single().Data);
    }
}

public sealed class SvgIconConverterTests
{
    static string Svg(string body, int size = 32, string head = "") =>
        $"""<?xml version="1.0"?><svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 {size} {size}">{head}{body}</svg>""";

    [Fact]
    public void Transparent_rectangles_are_dropped()
    {
        var svg = Svg(
            """
            <style>.cls-1{fill:none}</style>
            <path d="M10 10h10v10z"/>
            <g id="_Transparent_Rectangle_" data-name="&lt;Transparent Rectangle&gt;"><rect class="cls-1" width="32" height="32"/></g>
            <rect fill="none" x="1" y="1" width="30" height="30"/>
            <rect class="cls-1" x="2" y="2" width="5" height="5"/>
            <rect style="fill: none" x="3" y="3" width="5" height="5"/>
            <rect class="st0" width="32" height="32"/>
            <rect id="_x3C_Transparent_Rectangle_x3E_" width="10" height="10"/>
            <rect data-name="&lt;Transparent Rectangle&gt;" width="9" height="9"/>
            """);
        var g = SvgIconConverter.Convert(svg, "t-32.svg");
        Assert.Equal("M 10 10 L 20 10 L 20 20 Z", Assert.Single(g.Paths).Data);
    }

    [Fact]
    public void Class_set_to_fill_none_in_a_style_block_is_dropped_even_with_spaces()
    {
        var svg = Svg("""<path d="M10 10h5v5z"/><path class="a b" d="M20 20h5v5z"/>""", head: "<style>.b { fill : none; }</style>");
        Assert.Single(SvgIconConverter.Convert(svg, "t-32.svg").Paths);
    }

    [Fact]
    public void Child_fill_overrides_group_fill_none()
    {
        var svg = Svg("""<g fill="none"><path fill="#000" d="M10 10h5v5z"/><path d="M20 20h5v5z"/></g>""");
        Assert.Single(SvgIconConverter.Convert(svg, "t-32.svg").Paths);
    }

    [Fact]
    public void Inner_paths_are_kept_separately_even_when_invisible()
    {
        var svg = Svg("""<path d="M4 4h24v24H4z"/><path fill="none" opacity="0" data-icon-path="inner-path" d="M10 10h5v5z"/><path id="inner-path" d="M16 16h2v2z"/>""");
        var g = SvgIconConverter.Convert(svg, "t-32.svg");
        Assert.Single(g.Paths);
        Assert.Equal(2, g.InnerPaths.Count);
        Assert.Equal("M 10 10 L 15 10 L 15 15 Z", g.InnerPaths[0].Data);
    }

    [Fact]
    public void Inner_paths_do_not_influence_the_crop()
    {
        var svg = Svg("""<path d="M8 8h16v16H8z"/><path data-icon-path="inner-path" d="M0 0h32v32z"/>""");
        Assert.Equal(24f, SvgIconConverter.Convert(svg, "t-32.svg").ViewBoxWidth);
    }

    [Fact]
    public void Evenodd_is_read_from_attribute_style_and_ancestors()
    {
        var svg = Svg("""<g fill-rule="evenodd"><path d="M10 10h5v5z"/></g><path style="fill-rule:evenodd" d="M20 20h5v5z"/><path d="M5 5h5v5z"/>""");
        var g = SvgIconConverter.Convert(svg, "t-32.svg");
        Assert.Equal([true, true, false], g.Paths.Select(p => p.EvenOdd));
    }

    [Fact]
    public void Shapes_become_paths()
    {
        var svg = Svg("""<circle cx="16" cy="16" r="4"/><rect x="2" y="3" width="4" height="5"/><polyline points="1,1 2,2 3,1"/><line x1="0" y1="0" x2="5" y2="5"/><ellipse cx="8" cy="8" rx="2" ry="1"/>""");
        var g = SvgIconConverter.Convert(svg, "t-32.svg");
        Assert.Equal(4, g.Paths.Count);
        Assert.Equal("M 2 3 L 6 3 L 6 8 L 2 8 Z", g.Paths[1].Data);
        Assert.Equal("M 1 1 L 2 2 L 3 1", g.Paths[2].Data);
        Assert.StartsWith("M 20 16 C 20 18.209", g.Paths[0].Data);
    }

    [Fact]
    public void Rounded_rect_uses_cubic_corners_and_clamps_radii()
    {
        var p = SvgShapes.Rect(0, 0, 10, 4, 100);
        Assert.Equal(4, p.Segments.Count(s => s.Op == 'C'));
        var b = p.Bounds()!.Value;
        Assert.Equal((0d, 0d, 10d, 4d), (b.MinX, b.MinY, b.MaxX, b.MaxY));
    }

    [Fact]
    public void Art_inside_the_centre_24_is_cropped()
    {
        var g = SvgIconConverter.Convert(Svg("""<path d="M4 4h24v24H4z"/>"""), "t-32.svg");
        Assert.Equal((4f, 4f, 24f, 24f), (g.ViewBoxX, g.ViewBoxY, g.ViewBoxWidth, g.ViewBoxHeight));
    }

    [Fact]
    public void Close_and_checkbox_keep_the_full_box_and_add_uses_the_centre_26()
    {
        var art = Svg("""<path d="M8 8h16v16H8z"/>""");
        Assert.Equal(32f, SvgIconConverter.Convert(art, "close-32.svg").ViewBoxWidth);
        Assert.Equal(32f, SvgIconConverter.Convert(art, "checkbox-32.svg").ViewBoxWidth);
        var add = SvgIconConverter.Convert(art, "add-32.svg");
        Assert.Equal((3f, 3f, 26f, 26f), (add.ViewBoxX, add.ViewBoxY, add.ViewBoxWidth, add.ViewBoxHeight));
        Assert.Equal(26f, SvgIconConverter.Convert(art, "subtract-32.svg").ViewBoxWidth);
        Assert.Equal(16f, SvgIconConverter.Convert(Svg("""<path d="M6 6h4v4z"/>""", 16), "close-16.svg").ViewBoxWidth);
    }

    [Fact]
    public void Art_touching_the_edge_is_not_cropped()
    {
        var g = SvgIconConverter.Convert(Svg("""<path d="M2 4h24v24H2z"/>"""), "t-32.svg");
        Assert.Equal((0f, 0f, 32f, 32f), (g.ViewBoxX, g.ViewBoxY, g.ViewBoxWidth, g.ViewBoxHeight));
    }

    [Fact]
    public void Small_artboards_are_never_cropped()
    {
        var g = SvgIconConverter.Convert(Svg("""<path d="M6 6h4v4z"/>""", 16), "t-16.svg");
        Assert.Equal((0f, 0f, 16f, 16f), (g.ViewBoxX, g.ViewBoxY, g.ViewBoxWidth, g.ViewBoxHeight));
    }

    [Fact]
    public void Artboard_is_read_from_width_when_there_is_no_viewbox()
    {
        var svg = """<svg xmlns="http://www.w3.org/2000/svg" width="20px" height="20px"><path d="M1 1h4v4z"/></svg>""";
        Assert.Equal(20, SvgIconConverter.Convert(svg, "t-20.svg").ArtboardSize);
    }

    [Fact]
    public void Artboard_must_match_the_file_name()
    {
        var ex = Assert.Throws<FormatException>(() => SvgIconConverter.Convert(Svg("""<path d="M1 1h4v4z"/>""", 16), "t-32.svg"));
        Assert.Contains("t-32.svg", ex.Message);
    }

    [Theory]
    [InlineData("arrow--right", "ArrowRight")]
    [InlineData("cloud-foundry--1", "CloudFoundry1")]
    [InlineData("ai-generate", "AiGenerate")]
    [InlineData("information--square--filled", "InformationSquareFilled")]
    [InlineData("overflow-menu--horizontal", "OverflowMenuHorizontal")]
    public void Member_names(string stem, string expected) => Assert.Equal(expected, IconEmitter.MemberName(stem));

    static GlyphData Glyph(string stem, int size) =>
        new(stem, size, 0, 0, size, size, [new GlyphPathData("M 0 0 L 1 1", false)], []);

    [Fact]
    public void Colliding_names_throw() =>
        Assert.Throws<InvalidOperationException>(() => IconEmitter.Render([Glyph("a-b", 16), Glyph("a--b", 16)]));

    [Fact]
    public void Output_is_sorted_with_sizes_ascending_and_lf_only()
    {
        var body = IconEmitter.Render([Glyph("b", 32), Glyph("a", 20), Glyph("a", 16)]);
        Assert.DoesNotContain('\r', body);
        Assert.True(body.IndexOf("    A,", StringComparison.Ordinal) < body.IndexOf("    B,", StringComparison.Ordinal));
        Assert.True(body.IndexOf("\"a\", 16", StringComparison.Ordinal) < body.IndexOf("\"a\", 20", StringComparison.Ordinal));
        Assert.Contains("(16, 20).", body);
    }
}

public sealed partial class RealIconTests
{
    static readonly string Repo = RepoPaths.FindRoot(AppContext.BaseDirectory);
    static readonly string IconsDir = Path.Combine(Repo, "design", "carbon", "icons");

    static readonly Lazy<IReadOnlyList<GlyphData>> All = new(() =>
        Directory.GetFiles(IconsDir, "*.svg").Order(StringComparer.Ordinal).Select(SvgIconConverter.ConvertFile).ToList());

    [GeneratedRegex(@"^M(?: (?:[MLCZ]|-?\d+(?:\.\d+)?))*$")]
    private static partial Regex Grammar();

    static double[] Numbers(string data) =>
        Regex.Matches(data, @"-?\d+(?:\.\d+)?").Select(m => double.Parse(m.Value, CultureInfo.InvariantCulture)).ToArray();

    [Fact]
    public void Every_svg_converts() => Assert.Equal(Directory.GetFiles(IconsDir, "*.svg").Length, All.Value.Count);

    [Fact]
    public void Every_glyph_has_a_non_empty_path() =>
        Assert.All(All.Value, g =>
        {
            Assert.NotEmpty(g.Paths);
            Assert.All(g.Paths, p => Assert.Matches("[LC]", p.Data));
        });

    [Fact]
    public void Path_data_uses_only_absolute_mlcz_and_plain_numbers() =>
        Assert.All(All.Value.SelectMany(g => g.Paths.Concat(g.InnerPaths).Select(p => (g.Stem, p.Data))),
            x => Assert.True(Grammar().IsMatch(x.Data), $"{x.Stem}: {x.Data}"));

    [Fact]
    public void No_path_is_a_full_artboard_rectangle() =>
        Assert.All(All.Value, g => Assert.All(g.Paths, p =>
        {
            if (p.Data.Contains('C')) return;
            var nums = Numbers(p.Data);
            if (nums.Length / 2 > 5) return; // a rectangle has 4 corners (+ optional repeat of the first)
            var xs = nums.Where((_, i) => i % 2 == 0).ToArray();
            var ys = nums.Where((_, i) => i % 2 == 1).ToArray();
            var full = xs.Min() <= 0.05 && ys.Min() <= 0.05
                       && xs.Max() >= g.ArtboardSize - 0.05 && ys.Max() >= g.ArtboardSize - 0.05;
            Assert.False(full, $"{g.Stem}-{g.ArtboardSize} has a full-artboard path: {p.Data}");
        }));

    [Fact]
    public void Points_lie_inside_the_artboard()
    {
        // On-curve points must be inside; Bézier control points of curves hugging the edge may overshoot a little.
        Assert.All(All.Value, g => Assert.All(g.Paths.Concat(g.InnerPaths), p =>
        {
            Assert.All(Numbers(p.Data), n => Assert.InRange(n, -0.5, g.ArtboardSize + 0.5));
            foreach (var seg in Regex.Matches(p.Data, @"[MLC](?: -?\d+(?:\.\d+)?)+").Select(m => m.Value))
            {
                var nums = Numbers(seg);
                Assert.All(nums.TakeLast(2), n => Assert.InRange(n, -0.05, g.ArtboardSize + 0.05));
            }
        }));
    }

    [Fact]
    public void Only_32px_glyphs_are_cropped_to_the_centre() =>
        Assert.All(All.Value, g =>
        {
            if (g.ViewBoxX == 0) { Assert.Equal(g.ArtboardSize, g.ViewBoxWidth); return; }
            Assert.Equal(32, g.ArtboardSize);
            var expected = g.Stem is "add" or "subtract" ? (3f, 3f, 26f, 26f) : (4f, 4f, 24f, 24f);
            Assert.Equal(expected, (g.ViewBoxX, g.ViewBoxY, g.ViewBoxWidth, g.ViewBoxHeight));
        });

    // Icon/README.md: close is drawn from its full 32 box (an 8px cross at 16px, as Carbon's 16px glyph), add from its
    // centre 26x26; the checkbox glyphs, whose 16px versions are the 32 art at half scale, are not cropped either.
    [Theory]
    [InlineData("close", 0f, 32f)]
    [InlineData("checkbox", 0f, 32f)]
    [InlineData("checkbox--checked", 0f, 32f)]
    [InlineData("checkbox--indeterminate", 0f, 32f)]
    [InlineData("add", 3f, 26f)]
    [InlineData("subtract", 3f, 26f)]
    [InlineData("caret--down", 4f, 24f)]
    [InlineData("overflow-menu--vertical", 4f, 24f)]
    public void Carbon_crop_rules_for_32_only_glyphs(string stem, float origin, float size)
    {
        var g = All.Value.Single(x => x.Stem == stem && x.ArtboardSize == 32);
        Assert.Equal((origin, origin, size, size), (g.ViewBoxX, g.ViewBoxY, g.ViewBoxWidth, g.ViewBoxHeight));
    }

    [Fact]
    public void Close_draws_an_8px_cross_at_16px()
    {
        var g = All.Value.Single(x => x.Stem == "close" && x.ArtboardSize == 32);
        var xs = Numbers(g.Paths[0].Data).Where((_, i) => i % 2 == 0).ToArray();
        var drawn = (xs.Max() - xs.Min()) * 16 / g.ViewBoxWidth;
        Assert.InRange(drawn, 7.9, 8.1);
    }

    [Fact]
    public void Filled_status_icons_have_inner_paths()
    {
        Assert.NotEmpty(All.Value.Single(g => g.Stem == "warning--filled" && g.ArtboardSize == 32).InnerPaths);
        Assert.NotEmpty(All.Value.Single(g => g.Stem == "warning--filled" && g.ArtboardSize == 16).InnerPaths);
        Assert.NotEmpty(All.Value.Single(g => g.Stem == "checkmark--filled" && g.ArtboardSize == 16).InnerPaths);
    }

    [Fact]
    public void Generated_file_is_up_to_date()
    {
        var generated = CarbonGenerator.Generate(Path.Combine(Repo, "design", "carbon"))["CdsIcons.g.cs"];
        var committed = File.ReadAllText(Path.Combine(Repo, "src", "Maui.Carbon", "Generated", "CdsIcons.g.cs"));
        Assert.Equal(committed, generated);
    }
}

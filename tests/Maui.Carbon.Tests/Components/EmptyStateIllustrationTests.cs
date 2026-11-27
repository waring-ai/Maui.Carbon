using Microsoft.Maui.Graphics;

namespace Maui.Carbon.Tests.Components;

public class EmptyStateIllustrationTests
{
    [Fact]
    public void Path_data_parses_moves_lines_curves_and_closes()
    {
        var path = IllustrationPath.Parse("M0 0L10 0C10 5 5 10 0 10Z");
        Assert.Equal(5, path.Count);
        Assert.Equal(1, path.SubPathCount);
        Assert.True(path.IsSubPathClosed(0));
        Assert.Equal(new PointF(10, 0), path[1]);
        Assert.Equal(new PointF(0, 10), path[4]);
    }

    [Fact]
    public void Path_data_reads_signs_as_separators()
    {
        var path = IllustrationPath.Parse("M1-2L-3.5-4");
        Assert.Equal(new PointF(1, -2), path[0]);
        Assert.Equal(new PointF(-3.5f, -4), path[1]);
    }

    [Theory]
    [InlineData("Q1 2 3 4")]
    [InlineData("M1")]
    [InlineData("Mx y")]
    public void Path_data_rejects_anything_else(string data) => Assert.Throws<FormatException>(() => IllustrationPath.Parse(data));

    [Fact]
    public void Fit_keeps_the_aspect_ratio_and_centers()
    {
        var fit = IllustrationModel.Fit(80, 80, 200, 100, IllustrationAlignment.Center);
        Assert.Equal(1.25f, fit.Scale);
        Assert.Equal(50, fit.OffsetX);
        Assert.Equal(0, fit.OffsetY);
    }

    [Fact]
    public void Fit_bottom_start_anchors_to_the_bottom_left()
    {
        var fit = IllustrationModel.Fit(1584, 916, 792, 800, IllustrationAlignment.BottomStart);
        Assert.Equal(0.5f, fit.Scale);
        Assert.Equal(0, fit.OffsetX);
        Assert.Equal(800 - 458, fit.OffsetY);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(10, 0)]
    public void Fit_into_nothing_draws_nothing(float width, float height) =>
        Assert.Equal(0, IllustrationModel.Fit(80, 80, width, height, IllustrationAlignment.Center).Scale);

    [Theory]
    [InlineData(CdsIllustrationTheme.Auto, CdsThemeName.White, false)]
    [InlineData(CdsIllustrationTheme.Auto, CdsThemeName.G10, false)]
    [InlineData(CdsIllustrationTheme.Auto, CdsThemeName.G90, true)]
    [InlineData(CdsIllustrationTheme.Auto, CdsThemeName.G100, true)]
    [InlineData(CdsIllustrationTheme.Light, CdsThemeName.G100, false)]
    [InlineData(CdsIllustrationTheme.Dark, CdsThemeName.White, true)]
    public void Dark_artwork_follows_the_theme_or_the_override(CdsIllustrationTheme theme, CdsThemeName zone, bool dark) =>
        Assert.Equal(dark, IllustrationModel.UsesDark(theme, zone));

    [Fact]
    public void Pick_falls_back_to_the_light_drawing()
    {
        Assert.Same(IllustrationArt.HttpError404, IllustrationModel.Pick(IllustrationArt.HttpError404, null, CdsIllustrationTheme.Dark, CdsThemeName.G100));
        Assert.Same(IllustrationArt.ErrorDark, IllustrationModel.Pick(IllustrationArt.ErrorLight, IllustrationArt.ErrorDark, CdsIllustrationTheme.Auto, CdsThemeName.G90));
    }

    [Fact]
    public void Height_follows_the_aspect_ratio() => Assert.Equal(458, IllustrationModel.HeightForWidth(IllustrationArt.HttpError403, 792), 3);

    [Fact]
    public void Palette_colors_carry_their_opacity()
    {
        var color = IllustrationPalette.ToColor(0xFF8000, 0.5f);
        Assert.Equal(1, color.Red, 3);
        Assert.Equal(128 / 255f, color.Green, 3);
        Assert.Equal(0, color.Blue, 3);
        Assert.Equal(0.5f, color.Alpha, 3);
        Assert.Equal(1, IllustrationPalette.ToColor(IllustrationPalette.White, 2).Alpha);
    }

    [Fact]
    public void Every_drawing_parses_and_stays_inside_its_viewbox()
    {
        foreach (var art in IllustrationArt.All)
        {
            Assert.NotEmpty(art.Shapes);
            for (var i = 0; i < art.Shapes.Count; i++)
            {
                var path = art.PathAt(i);
                Assert.True(path.Count > 1, $"{art} shape {i} is empty");
                for (var p = 0; p < path.Count; p++)
                {
                    var point = path[p];
                    Assert.True(point.X >= -art.Width * 0.1f && point.X <= art.Width * 1.1f, $"{art} shape {i} x {point}");
                    Assert.True(point.Y >= -art.Height * 0.1f && point.Y <= art.Height * 1.1f, $"{art} shape {i} y {point}");
                }
                _ = art.ClipAt(i);
                var shape = art.Shapes[i];
                Assert.True(shape.Fill is not null || shape.Stroke is not null, $"{art} shape {i} paints nothing");
                Assert.InRange(shape.Opacity, 0.001f, 1f);
            }
        }
    }

    [Fact]
    public void Empty_state_artwork_is_80_square_with_distinct_variants()
    {
        var pairs = new[]
        {
            (IllustrationArt.ErrorLight, IllustrationArt.ErrorDark), (IllustrationArt.NoDataLight, IllustrationArt.NoDataDark),
            (IllustrationArt.NoTagsLight, IllustrationArt.NoTagsDark), (IllustrationArt.NotFoundLight, IllustrationArt.NotFoundDark),
            (IllustrationArt.NotificationsLight, IllustrationArt.NotificationsDark), (IllustrationArt.UnauthorizedLight, IllustrationArt.UnauthorizedDark),
        };
        foreach (var (light, dark) in pairs)
        {
            Assert.Equal((80f, 80f), (light.Width, light.Height));
            Assert.Equal((80f, 80f), (dark.Width, dark.Height));
            Assert.NotEqual(PaintSignature(light), PaintSignature(dark));
        }
    }

    static string PaintSignature(IllustrationDrawing art) => string.Join(",", art.Shapes.Select(s =>
        s.Fill is null ? "-" : s.Fill.Kind == IllustrationPaintKind.Linear ? string.Join("/", s.Fill.Stops.Select(x => x.Rgb)) : s.Fill.Rgb.ToString()));

    [Fact]
    public void Full_page_error_drawings_use_only_theme_paints()
    {
        foreach (var art in new[] { IllustrationArt.FullPageError403, IllustrationArt.FullPageError404, IllustrationArt.FullPageErrorCustom })
        {
            Assert.All(art.Shapes, s =>
            {
                Assert.Equal(IllustrationPaintKind.ThemeFill, s.Fill!.Kind);
                Assert.Equal(IllustrationPaintKind.ThemeStroke, s.Stroke!.Kind);
            });
        }
    }

    [Fact]
    public void Http_error_drawings_are_1584_by_916_with_fixed_colors()
    {
        foreach (var art in new[] { IllustrationArt.HttpError403, IllustrationArt.HttpError404, IllustrationArt.HttpErrorOther })
        {
            Assert.Equal((1584f, 916f), (art.Width, art.Height));
            Assert.DoesNotContain(art.Shapes, s => s.Fill?.Kind is IllustrationPaintKind.ThemeFill or IllustrationPaintKind.ThemeStroke);
            Assert.Contains(art.Shapes, s => s.Clip is not null);
        }
    }

    [Fact]
    public void Gradients_are_relative_to_the_viewbox()
    {
        var paint = IllustrationPaint.Linear(20, 40, 60, 80, [new(0, IllustrationPalette.Gray10, 1), new(1, IllustrationPalette.Gray30, 0.5f)]);
        var gradient = Assert.IsType<LinearGradientPaint>(paint.Gradient(80, 80));
        Assert.Equal(new Point(0.25, 0.5), gradient.StartPoint);
        Assert.Equal(new Point(0.75, 1), gradient.EndPoint);
        Assert.Equal(2, gradient.GradientStops.Length);
    }

    [Fact]
    public void Every_drawing_renders_onto_a_canvas()
    {
        foreach (var art in IllustrationArt.All)
        {
            var canvas = new PictureCanvas(0, 0, 400, 300);
            CdsIllustrationView.Render(canvas, new RectF(0, 0, 400, 300), art, IllustrationAlignment.Center, Colors.Transparent, Colors.Transparent);
            Assert.NotNull(canvas.Picture);
        }
    }

    [Fact]
    public void The_view_switches_variants_and_can_be_named()
    {
        var view = new CdsIllustrationView { Light = IllustrationArt.NoDataLight, Dark = IllustrationArt.NoDataDark, SizesToWidth = true, Alignment = IllustrationAlignment.BottomStart };
        Assert.Same(IllustrationArt.NoDataLight, view.Current);
        view.Theme = CdsIllustrationTheme.Dark;
        Assert.Same(IllustrationArt.NoDataDark, view.Current);
        view.SetDescription("An empty table");
        Assert.True(AutomationProperties.GetIsInAccessibleTree(view));
        view.SetDescription(null);
        Assert.False(AutomationProperties.GetIsInAccessibleTree(view));
        view.ThemeFillKey = CdsColorKeys.Layer01;
        view.ThemeStrokeKey = CdsColorKeys.BorderStrong01;
    }
}

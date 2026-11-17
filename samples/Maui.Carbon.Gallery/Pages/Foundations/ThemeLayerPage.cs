using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Foundations;

[GalleryPage("Theme and Layer", GalleryCategory.Foundations, Order = 50, Spec = "Layer",
    Summary = "Theme zones switch every token in a subtree; layers step the -01 tokens up to the next level. Components never name -02/-03 tokens.")]
public sealed class ThemeLayerPage : GalleryPage
{
    public ThemeLayerPage()
    {
        var stack = new VerticalStackLayout { Spacing = CdsSpacing.Spacing09 };

        stack.Add(new GalleryExample
        {
            Title = "Layering",
            Description = "Each CdsLayer steps one level: background, layer-01, layer-02, layer-03. Fields inside step to field-02 and field-03.",
            Body = Nest(3),
        });

        stack.Add(new GalleryExample
        {
            Title = "Nested theme zones",
            Description = "A g90 zone inside the page theme, with a white zone inside it. A zone resets layering to level one.",
            Body = new CdsTheme
            {
                Theme = CdsThemeName.G90,
                Body = new VerticalStackLayout
                {
                    Padding = CdsSpacing.Spacing06,
                    Spacing = CdsSpacing.Spacing05,
                    Children =
                    {
                        new CdsText("Gray 90 zone", CdsTypeStyle.HeadingCompact02),
                        new CdsLayer { WithBackground = true, Body = new ContentView { Padding = CdsSpacing.Spacing05, Content = new CdsTextInput { Label = "Field on layer 2" } } },
                        new CdsTheme
                        {
                            Theme = CdsThemeName.White,
                            Body = new VerticalStackLayout
                            {
                                Padding = CdsSpacing.Spacing05,
                                Spacing = CdsSpacing.Spacing05,
                                Children = { new CdsText("White zone inside it", CdsTypeStyle.HeadingCompact01), new CdsButton { Text = "Primary action" } },
                            },
                        },
                    },
                },
            },
        });

        Body = stack;
    }

    static View Nest(int levels)
    {
        // A surface paints layer-01 *at its own level*; a CdsLayer inside it steps its content up one level.
        View Surface(int level)
        {
            var content = new VerticalStackLayout
            {
                Padding = CdsSpacing.Spacing05,
                Spacing = CdsSpacing.Spacing05,
                Children =
                {
                    new CdsText($"Surface on layer-0{level}", CdsTypeStyle.Code02),
                    new CdsTextInput { Label = $"Field (field-0{Math.Min(level + 1, 3)})", Placeholder = "Placeholder", WidthRequest = 280, HorizontalOptions = LayoutOptions.Start },
                },
            };
            if (level < levels) content.Add(Surface(level + 1));
            var surface = new ContentView { Content = new CdsLayer { Body = content } };
            surface.SetDynamicResource(BackgroundColorProperty, CdsColorKeys.Layer01);
            return surface;
        }

        return new VerticalStackLayout
        {
            Spacing = CdsSpacing.Spacing05,
            Children =
            {
                new CdsTextInput { Label = "Field on the page (field-01)", Placeholder = "Placeholder", WidthRequest = 280, HorizontalOptions = LayoutOptions.Start },
                Surface(1),
            },
        };
    }
}

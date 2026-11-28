namespace Maui.Carbon.Gallery.Infrastructure;

/// <summary>Flyout header: product name, theme switcher and reduced-motion switch.</summary>
public sealed class GalleryFlyoutHeader : ContentView
{
    public GalleryFlyoutHeader()
    {
        var stack = new VerticalStackLayout { Spacing = CdsSpacing.Spacing05, Padding = new Thickness(CdsSpacing.Spacing05, CdsSpacing.Spacing06, CdsSpacing.Spacing05, CdsSpacing.Spacing05) };
        stack.Add(new CdsText("Carbon for .NET MAUI", CdsTypeStyle.HeadingCompact02));
        stack.Add(new CdsText("Theme", CdsTypeStyle.Label01, CdsTextTone.Secondary));

        var themes = new Grid { ColumnSpacing = 0 };
        var names = Enum.GetValues<CdsThemeName>();
        var buttons = new List<CdsButton>();
        for (var i = 0; i < names.Length; i++)
        {
            var theme = names[i];
            themes.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            var button = new CdsButton
            {
                Text = theme switch { CdsThemeName.White => "White", CdsThemeName.G10 => "G10", CdsThemeName.G90 => "G90", _ => "G100" },
                Kind = CdsButtonKind.Ghost,
                Size = CdsSize.Sm,
                IsSelected = theme == CdsThemeManager.Current,
            };
            button.Clicked += (_, _) =>
            {
                CdsThemeManager.SetTheme(theme);
                foreach (var b in buttons) b.IsSelected = b == button;
            };
            buttons.Add(button);
            themes.Add(button, i);
        }
        stack.Add(themes);

        var motion = new CdsButton { Text = "Reduce motion", Kind = CdsButtonKind.Ghost, Size = CdsSize.Sm, IsSelected = false };
        motion.Clicked += (_, _) => CdsMotion.ReducedMotionOverride = motion.IsSelected == true ? true : null;
        stack.Add(motion);
        Content = stack;
        // Shell keeps the header fixed and scrolls the page list underneath it, so the header needs its own fill.
        this.SetDynamicResource(BackgroundColorProperty, CdsColorKeys.Layer01);
    }
}

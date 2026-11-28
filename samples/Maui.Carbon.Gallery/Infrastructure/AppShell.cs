using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery;

/// <summary>The gallery shell: one flyout section per category, built from the page registry.</summary>
public sealed class AppShell : Shell
{
    public AppShell()
    {
        FlyoutBehavior = DeviceInfo.Idiom == DeviceIdiom.Desktop || DeviceInfo.Idiom == DeviceIdiom.Tablet
            ? FlyoutBehavior.Locked
            : FlyoutBehavior.Flyout;
        FlyoutWidth = 256;
        this.SetDynamicResource(FlyoutBackgroundColorProperty, CdsColorKeys.Layer01);
        this.SetDynamicResource(BackgroundColorProperty, CdsColorKeys.Background);
        this.SetDynamicResource(ForegroundColorProperty, CdsColorKeys.TextPrimary);
        this.SetDynamicResource(TitleColorProperty, CdsColorKeys.TextPrimary);
        SetNavBarIsVisible(this, false);
        // Each category is a FlyoutItem holding several pages, which Shell also shows as a bottom tab bar. The flyout
        // already lists every page, so the tab bar would only repeat it (and Shell gives its tabs no icons).
        SetTabBarIsVisible(this, false);

        FlyoutHeader = new GalleryFlyoutHeader();
        FlyoutHeaderBehavior = FlyoutHeaderBehavior.Fixed;
        ItemTemplate = new DataTemplate(() =>
        {
            var label = new CdsText { TypeStyle = CdsTypeStyle.BodyCompact01, Tone = CdsTextTone.Secondary, Padding = new Thickness(CdsSpacing.Spacing07, CdsSpacing.Spacing03, CdsSpacing.Spacing05, CdsSpacing.Spacing03) };
            label.SetBinding(Label.TextProperty, static (BaseShellItem item) => item.Title);
            return label;
        });

        foreach (var group in GalleryRegistry.Entries.GroupBy(e => e.Info.Category))
        {
            var section = new FlyoutItem { Title = Spaced(group.Key), FlyoutDisplayOptions = FlyoutDisplayOptions.AsMultipleItems };
            foreach (var entry in group)
            {
                section.Items.Add(new ShellContent
                {
                    Title = entry.Info.Title,
                    Route = entry.Info.Route,
                    ContentTemplate = new DataTemplate(entry.PageType),
                });
            }
            Items.Add(section);
        }
    }

    static string Spaced(GalleryCategory c) => c switch
    {
        GalleryCategory.FluidForms => "Fluid forms",
        GalleryCategory.AI => "AI",
        _ => c.ToString(),
    };
}

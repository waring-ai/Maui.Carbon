using Microsoft.Maui.Controls.Shapes;

namespace Maui.Carbon.Tests.Components;

/// <summary>Visual-review follow-ups: outlines on the box edge, app alignment kept, highlighted rows, inline select outline.</summary>
public class LayoutConformance2Tests
{
    static Color Token(string name) => CdsPalettes.GetTokenColor(CdsThemeName.White, name);

    // ---- Outlined buttons: the outline is drawn on the box edge, not by the Border stroke ----

    [Theory]
    [InlineData(CdsButtonKind.Tertiary, "button-tertiary")]
    [InlineData(CdsButtonKind.DangerTertiary, "button-danger-secondary")]
    public void Outlined_button_draws_its_outline_with_hairlines_on_the_box_edge(CdsButtonKind kind, string token)
    {
        var button = new CdsButton("Export logs", kind);
        _ = new CdsTheme { Theme = CdsThemeName.White, Body = button };
        var outline = Private.Field<CdsButtonOutline>(button, "_outline");
        var box = Private.Field<Border>(button, "_box");
        Assert.True(outline.IsVisible);
        Assert.Equal(Token(token), outline.Color);
        Assert.All(outline.Edges, e => Assert.Equal(Token(token), e.Color));
        Assert.True(box.Stroke is null || box.Stroke is SolidColorBrush { Color.Alpha: 0 });
        Assert.Equal(1, box.StrokeThickness);

        // The hairlines are 1 thick and pinned to the four edges of the cell the face fills.
        Assert.Equal((1d, LayoutOptions.Start), (outline.Edges[0].HeightRequest, outline.Edges[0].VerticalOptions));
        Assert.Equal((1d, LayoutOptions.End), (outline.Edges[1].HeightRequest, outline.Edges[1].VerticalOptions));
        Assert.Equal((1d, LayoutOptions.Start), (outline.Edges[2].WidthRequest, outline.Edges[2].HorizontalOptions));
        Assert.Equal((1d, LayoutOptions.End), (outline.Edges[3].WidthRequest, outline.Edges[3].HorizontalOptions));
        Assert.Equal(Thickness.Zero, outline.Margin);
        Assert.Same(Private.Field<Grid>(button, "_root"), outline.Parent);
    }

    [Fact]
    public void Filled_button_has_no_visible_outline_of_another_color_and_ghost_none()
    {
        var primary = new CdsButton("Create cluster");
        var ghost = new CdsButton("View nodes", CdsButtonKind.Ghost);
        _ = new CdsTheme { Theme = CdsThemeName.White, Body = new VerticalStackLayout { primary, ghost } };
        Assert.Equal(Token("button-primary"), Private.Field<CdsButtonOutline>(primary, "_outline").Color);
        Assert.False(Private.Field<CdsButtonOutline>(ghost, "_outline").IsVisible);
    }

    [Fact]
    public void Tertiary_icon_button_draws_its_outline_on_the_box_edge()
    {
        var button = new CdsIconButton { Icon = CdsIconName.Copy, Kind = CdsIconButtonKind.Tertiary };
        _ = new CdsTheme { Theme = CdsThemeName.White, Body = button };
        var face = Private.Field<CdsActionFace>(button, "_face");
        var outline = Private.Field<CdsButtonOutline>(face, "_outline");
        Assert.True(outline.IsVisible);
        Assert.Equal(Token("button-tertiary"), outline.Color);
        var box = Private.Field<Border>(face, "_box");
        Assert.True(box.Stroke is null || box.Stroke is SolidColorBrush { Color.Alpha: 0 });
    }

    // ---- Controls keep the app's alignment ----

    [Fact]
    public void Expandable_search_keeps_the_apps_alignment_and_fills_only_when_expanded()
    {
        var search = new CdsSearch { Label = "Search", IsExpandable = true, HorizontalOptions = LayoutOptions.End };
        Assert.False(search.FillsWidth);
        search.IsExpanded = true;
        Assert.True(search.FillsWidth);
        search.IsExpanded = false;
        Assert.False(search.FillsWidth);
        Assert.Equal(LayoutOptions.End, search.HorizontalOptions);

        var plain = new CdsSearch { Label = "Search", HorizontalOptions = LayoutOptions.Start };
        plain.Size = CdsSize.Lg;
        Assert.True(plain.FillsWidth);
        Assert.Equal(LayoutOptions.Start, plain.HorizontalOptions);
    }

    [Fact]
    public void Region_loading_covers_its_cell_without_touching_the_apps_alignment()
    {
        var loading = new CdsLoading { HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.End };
        loading.OverlayScope = CdsLoadingOverlayScope.Region;
        Assert.Equal(LayoutOptions.Start, loading.HorizontalOptions);
        Assert.Equal(LayoutOptions.End, loading.VerticalOptions);
        Assert.True(loading.FillsSpace);
        var root = Private.Field<Grid>(loading, "_root");
        var measure = root.GetType().GetMethod("MeasureOverride", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;
        Assert.Equal(new Size(600, 400), (Size)measure.Invoke(root, [600d, 400d])!);

        loading.OverlayScope = CdsLoadingOverlayScope.Page;
        Assert.False(loading.FillsSpace);
        Assert.Equal(LayoutOptions.Start, loading.HorizontalOptions);
        Assert.Equal(LayoutOptions.Center, new CdsLoading().HorizontalOptions);
    }

    [Fact]
    public void Header_menu_and_items_keep_the_apps_alignment_across_presentations()
    {
        var item = new CdsHeaderMenuItem("Catalog") { HorizontalOptions = LayoutOptions.Center };
        var menu = new CdsHeaderMenu { Label = "Manage", HorizontalOptions = LayoutOptions.Center };
        var link = new CdsHeaderMenuItem("Users");
        menu.Items.Add(link);

        item.Presentation = CdsShellPresentation.SideNav;
        menu.Presentation = CdsShellPresentation.SideNav;
        Assert.True(item.FillsWidth);
        Assert.True(menu.FillsWidth);
        item.Presentation = CdsShellPresentation.Bar;
        menu.Presentation = CdsShellPresentation.Bar;
        Assert.False(item.FillsWidth);
        Assert.False(menu.FillsWidth);
        Assert.Equal(LayoutOptions.Center, item.HorizontalOptions);
        Assert.Equal(LayoutOptions.Center, menu.HorizontalOptions);
        Assert.Equal(LayoutOptions.Start, new CdsHeaderMenuItem().HorizontalOptions);
        Assert.Equal(LayoutOptions.Start, new CdsHeaderMenu().HorizontalOptions);
    }

    [Fact]
    public void Actionable_notification_keeps_the_apps_alignment_between_layouts()
    {
        var n = new CdsActionableNotification { Title = "Backup failed", ActionButtonLabel = "Retry", HorizontalOptions = LayoutOptions.Center };
        n.IsInline = true;
        Assert.Equal(LayoutOptions.Center, n.HorizontalOptions);
        n.IsInline = false;
        Assert.Equal(LayoutOptions.Center, n.HorizontalOptions);
        Assert.Equal(288, n.WidthRequest);

        // By default the toast layout sits at the start and the inline layout still fills the offered width.
        var inline = new CdsActionableNotification { Title = "Backup failed", IsInline = true };
        Assert.Equal(LayoutOptions.Start, inline.HorizontalOptions);
        var box = Private.Field<CdsMaxWidthBox>(inline, "_box");
        Assert.Equal(500, ((ICrossPlatformLayout)box).CrossPlatformMeasure(500, double.PositiveInfinity).Width);
    }

    [Fact]
    public void Progress_bar_fills_the_width_under_a_start_alignment()
    {
        var bar = new CdsProgressBar("Uploading usage-march.csv", 40) { HorizontalOptions = LayoutOptions.Start, HelperText = "40%" };
        Assert.True(bar.FillsWidth);
        Assert.Equal(LayoutOptions.Start, bar.HorizontalOptions);
        var box = Private.Field<CdsMaxWidthBox>(bar, "_fillBox");
        Assert.Same(bar.Content, box);
        Assert.Equal(640, ((ICrossPlatformLayout)box).CrossPlatformMeasure(640, double.PositiveInfinity).Width);
    }

    // ---- The keyboard-highlighted list row gets the hover fill ----

    [Fact]
    public void Highlighted_list_box_row_shows_the_hover_layer()
    {
        var row = new CdsListBoxRow(new ListBoxEntry("one", "Option 1"), 40, inline: false, multi: false, isSelected: false, content: null);
        _ = new CdsTheme { Theme = CdsThemeName.White, Body = row };
        var root = Private.Field<Grid>(row, "_root");
        Assert.True(root.BackgroundColor is null || root.BackgroundColor.Alpha == 0);
        row.IsHighlighted = true;
        row.ShowFocusRing = true;
        Assert.Equal(Token("layer-hover-01"), root.BackgroundColor);
    }

    // ---- The inline select's outline frames exactly the field ----

    [Fact]
    public void Field_outline_cancels_whatever_padding_the_field_has()
    {
        var select = new CdsSelect { Label = "Sort by", IsInline = true, IsInvalid = true, InvalidText = "Choose one" };
        var chrome = Private.Field<CdsFieldChrome>(select, "Chrome");
        Assert.Equal(CdsSpacing.Spacing03, chrome.Field.Padding.Left);
        Assert.Equal(new Thickness(-CdsSpacing.Spacing03, 0, -CdsSpacing.Spacing05, 0), chrome.OverlayGrid.Margin);

        select.IsInline = false;
        Assert.Equal(new Thickness(-CdsSpacing.Spacing05, 0), chrome.OverlayGrid.Margin);
    }
}

using Microsoft.Maui.Layouts;

namespace Maui.Carbon.Tests.Components;

/// <summary>Visual-review fixes of 2026-11-26: B-02, D-03, E-14, C-09, A-01, A-02, A-03, B-03.</summary>
public class LayoutConformanceTests
{
    static CdsButtonSet FluidSet(params CdsButton[] buttons)
    {
        var set = new CdsButtonSet { HorizontalOptions = LayoutOptions.Start, WidthRequest = 640, IsFluid = true };
        foreach (var b in buttons) set.Items.Add(b);
        return set;
    }

    // ---- B-02, D-03: a rebuild never overwrites the app's alignment ----

    [Fact]
    public void Button_set_keeps_the_apps_alignment_through_every_rebuild()
    {
        var set = FluidSet(new CdsButton("Delete key", CdsButtonKind.Danger), new CdsButton("Cancel", CdsButtonKind.Secondary));
        Assert.Equal(LayoutOptions.Start, set.HorizontalOptions);
        set.IsStacked = true;
        set.IsStacked = false;
        set.IsFluid = false;
        set.IsFluid = true;
        Assert.Equal(LayoutOptions.Start, set.HorizontalOptions);

        set.HorizontalOptions = LayoutOptions.End;
        set.Items.Add(new CdsButton("Skip", CdsButtonKind.Ghost));
        set.IsFluid = false;
        Assert.Equal(LayoutOptions.End, set.HorizontalOptions);
    }

    [Fact]
    public void Fluid_set_fills_the_width_it_is_offered_and_sizes_its_buttons_while_arranging()
    {
        var cancel = new CdsButton("Cancel", CdsButtonKind.Secondary);
        var delete = new CdsButton("Delete key", CdsButtonKind.Danger);
        var set = FluidSet(delete, cancel);
        Assert.True(set.FillsWidth);
        var row = Assert.IsType<CdsFluidButtonRow>(set.LayoutParts.Layout);

        var size = ((ICrossPlatformLayout)row).CrossPlatformMeasure(640, double.PositiveInfinity);
        Assert.Equal(640, size.Width);
        ((ICrossPlatformLayout)row).CrossPlatformArrange(new Rect(0, 0, 640, 64));
        // Two buttons in a set of at most 44rem split it 50/50, secondary before danger.
        Assert.Equal(new Rect(0, 0, 320, 64), cancel.Frame);
        Assert.Equal(new Rect(320, 0, 320, 64), delete.Frame);

        // Wider: a quarter each (at most 14.5rem), right-aligned.
        ((ICrossPlatformLayout)row).CrossPlatformArrange(new Rect(0, 0, 1000, 64));
        Assert.Equal(new Rect(536, 0, 232, 64), cancel.Frame);
        Assert.Equal(new Rect(768, 0, 232, 64), delete.Frame);
    }

    [Fact]
    public void Equally_spaced_progress_steps_fill_without_touching_the_apps_alignment()
    {
        var indicator = new CdsProgressIndicator { SpaceEqually = true, HorizontalOptions = LayoutOptions.Start, WidthRequest = 720 };
        foreach (var label in new[] { "Account", "Billing", "Review", "Deploy" })
            indicator.Steps.Add(new CdsProgressStep { Label = label });
        Assert.Equal(LayoutOptions.Start, indicator.HorizontalOptions);
        Assert.True(indicator.FillsWidth);

        indicator.SpaceEqually = false;
        Assert.False(indicator.FillsWidth);
        indicator.SpaceEqually = true;
        indicator.IsVertical = true;
        Assert.False(indicator.FillsWidth);
        Assert.Equal(LayoutOptions.Start, indicator.HorizontalOptions);
    }

    [Fact]
    public void Content_switcher_keeps_the_apps_alignment()
    {
        var switcher = new CdsContentSwitcher { HorizontalOptions = LayoutOptions.End };
        switcher.Items.Add(new CdsSwitch { Text = "All" });
        switcher.Items.Add(new CdsSwitch { Text = "Running" });
        switcher.IsIconOnly = true;
        switcher.IsIconOnly = false;
        Assert.Equal(LayoutOptions.End, switcher.HorizontalOptions);
        Assert.True(switcher.FillsWidth);
        Assert.Equal(LayoutOptions.Start, new CdsContentSwitcher().HorizontalOptions);
    }

    // ---- E-14: a filled button is full width ----

    [Fact]
    public void Filled_button_drops_the_20rem_cap_and_others_keep_it()
    {
        var button = new CdsButton("Continue");
        var root = Private.Field<Grid>(button, "_root");
        Assert.Equal(320, root.MaximumWidthRequest);
        button.HorizontalOptions = LayoutOptions.Fill;
        Assert.Equal(double.PositiveInfinity, root.MaximumWidthRequest);
        button.HorizontalOptions = LayoutOptions.Center;
        Assert.Equal(320, root.MaximumWidthRequest);
    }

    // ---- B-03: the fill covers the whole face ----

    [Fact]
    public void Button_fill_is_painted_on_the_root_not_inside_the_border()
    {
        var button = new CdsButton("Create cluster");
        _ = new CdsTheme { Theme = CdsThemeName.White, Body = button };
        var root = Private.Field<Grid>(button, "_root");
        var box = Private.Field<Border>(button, "_box");
        Assert.Equal(CdsPalettes.GetTokenColor(CdsThemeName.White, "button-primary"), root.BackgroundColor);
        Assert.True(box.BackgroundColor is null || box.BackgroundColor.Alpha == 0);
    }

    // ---- C-09: inline list boxes and selects hug their value ----

    [Fact]
    public void Inline_dropdown_and_select_fields_hug_their_content_up_to_30rem()
    {
        var dropdown = new CdsDropdown { Type = CdsListBoxType.Inline, Label = "Sort by" };
        var chrome = Private.Field<CdsFieldChrome>(dropdown, "Chrome");
        Assert.True(chrome.InlineFieldHugsContent);
        Assert.Equal(LayoutOptions.Start, chrome.Field.HorizontalOptions);
        Assert.Equal(CdsFieldChrome.InlineFieldMaxWidth, chrome.Field.MaximumWidthRequest);
        Assert.Equal(GridLength.Auto, chrome.ColumnDefinitions[1].Width);

        dropdown.Type = CdsListBoxType.Default;
        Assert.False(chrome.InlineFieldHugsContent);
        Assert.Equal(LayoutOptions.Fill, chrome.Field.HorizontalOptions);

        var select = new CdsSelect { Label = "Sort by", IsInline = true };
        Assert.True(Private.Field<CdsFieldChrome>(select, "Chrome").InlineFieldHugsContent);

        // An inline text input keeps filling the row.
        var input = new CdsTextInput { Label = "Name", IsInline = true };
        var inputChrome = Private.Field<CdsFieldChrome>(input, "Chrome");
        Assert.False(inputChrome.InlineFieldHugsContent);
        Assert.Equal(GridLength.Star, inputChrome.ColumnDefinitions[1].Width);
    }

    // ---- A-01: the selected tile shows the filled checkbox ----

    [Fact]
    public void Selected_tile_uses_the_filled_checkbox_glyph()
    {
        var tile = new CdsSelectableTile { IsSelected = true };
        _ = new CdsTheme { Theme = CdsThemeName.White, Body = tile };
        var glyph = Private.Field<CdsIcon>(tile, "_glyph");
        Assert.Equal(CdsIconName.CheckboxCheckedFilled, glyph.Icon);
        tile.IsSelected = false;
        Assert.Equal(CdsIconName.Checkbox, glyph.Icon);

        var filled = CdsIcons.GetGlyph(CdsIconName.CheckboxCheckedFilled, 16)!;
        Assert.True(filled.Paths.Single().EvenOdd);
        Assert.Equal(new RectF(0, 0, 32, 32), filled.ViewBox);
    }

    // ---- A-02: the ratio survives a stretching parent ----

    [Fact]
    public void Aspect_ratio_box_starts_at_the_top_and_arranges_children_at_its_ratio()
    {
        var box = new CdsAspectRatio { CustomRatio = 2.39 };
        Assert.Equal(LayoutOptions.Start, box.VerticalOptions);
        var fill = new BoxView();
        box.Add(fill);
        ((ICrossPlatformLayout)box).CrossPlatformMeasure(240, double.PositiveInfinity);
        ((ICrossPlatformLayout)box).CrossPlatformArrange(new Rect(0, 0, 240, 135));
        Assert.Equal(240, fill.Frame.Width);
        Assert.Equal(240 / 2.39, fill.Frame.Height, 3);
    }
}

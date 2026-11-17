namespace Maui.Carbon.Tests.Components;

public class MenuTests
{
    [Theory]
    [InlineData(false, false, true, CdsColorKeys.TextSecondary)]
    [InlineData(false, true, true, CdsColorKeys.TextPrimary)]
    [InlineData(true, true, true, CdsColorKeys.TextOnColor)]
    [InlineData(true, false, true, CdsColorKeys.TextSecondary)]
    [InlineData(false, false, false, CdsColorKeys.TextDisabled)]
    [InlineData(false, true, false, CdsColorKeys.TextDisabled)]
    [InlineData(true, true, false, CdsColorKeys.TextDisabled)]
    public void Row_text_follows_state(bool danger, bool highlighted, bool enabled, string text) =>
        Assert.Equal(text, CdsMenuTokens.Resolve(danger, highlighted, enabled).Text);

    [Fact]
    public void Highlighted_rows_fill_with_layer_hover_and_danger_rows_with_button_danger()
    {
        Assert.Equal(CdsColorKeys.LayerHover01, CdsMenuTokens.Resolve(false, true, true).Background);
        Assert.Equal(CdsColorKeys.ButtonDangerPrimary, CdsMenuTokens.Resolve(true, true, true).Background);
        Assert.Null(CdsMenuTokens.Resolve(false, false, true).Background);
    }

    [Fact]
    public void Disabled_rows_never_get_a_hover_fill()
    {
        Assert.Null(CdsMenuTokens.Resolve(false, true, false).Background);
        Assert.Null(CdsMenuTokens.Resolve(true, true, false).Background);
    }

    [Theory]
    [InlineData(CdsSize.Xs, 24)]
    [InlineData(CdsSize.Sm, 32)]
    [InlineData(CdsSize.Md, 40)]
    [InlineData(CdsSize.Lg, 48)]
    [InlineData(CdsSize.Xl, 48)]
    [InlineData(CdsSize.Xxl, 48)]
    public void Row_height_follows_size_and_clamps_to_lg(CdsSize size, double height) =>
        Assert.Equal(height, CdsMenuTokens.RowHeight(size));

    [Fact]
    public void Minimum_width_is_10rem_or_12rem_with_icons_and_never_beyond_the_maximum()
    {
        Assert.Equal(160, CdsMenuTokens.MinimumWidth(false));
        Assert.Equal(192, CdsMenuTokens.MinimumWidth(true));
        Assert.Equal(240, CdsMenuTokens.MinimumWidth(false, 240));
        Assert.Equal(288, CdsMenuTokens.MinimumWidth(true, 900));
        Assert.Equal(160, CdsMenuTokens.MinimumWidth(false, 100));
    }

    [Theory]
    [InlineData(CdsMenuAlignment.Bottom, CdsPlacement.BottomStart, true)]
    [InlineData(CdsMenuAlignment.BottomStart, CdsPlacement.BottomStart, false)]
    [InlineData(CdsMenuAlignment.BottomEnd, CdsPlacement.BottomEnd, false)]
    [InlineData(CdsMenuAlignment.Top, CdsPlacement.TopStart, true)]
    [InlineData(CdsMenuAlignment.TopStart, CdsPlacement.TopStart, false)]
    [InlineData(CdsMenuAlignment.TopEnd, CdsPlacement.TopEnd, false)]
    public void Alignment_maps_to_placement_and_width_rule(CdsMenuAlignment alignment, CdsPlacement placement, bool fills)
    {
        Assert.Equal(placement, alignment.ToPlacement());
        Assert.Equal(fills, alignment.FillsTrigger());
    }

    [Fact]
    public void A_menu_collects_items_and_defaults_to_size_sm()
    {
        var menu = new CdsMenu { Label = "Object actions" };
        menu.Items.Add(new CdsMenuItem("Download", CdsIconName.Download) { Shortcut = "Ctrl+D" });
        menu.Items.Add(new CdsMenuItemDivider());
        menu.Items.Add(new CdsMenuItem("Delete object") { Kind = CdsMenuItemKind.Danger });
        Assert.Equal(3, menu.Items.Count);
        Assert.Equal(CdsSize.Sm, menu.Size);
        Assert.False(menu.IsOpen);
    }

    [Fact]
    public void Menu_size_is_clamped_to_xs_through_lg()
    {
        var menu = new CdsMenu { Size = CdsSize.Xxl };
        Assert.Equal(CdsSize.Lg, menu.Size);
        menu.Size = CdsSize.Xs;
        Assert.Equal(CdsSize.Xs, menu.Size);
    }

    [Fact]
    public void Opening_requires_a_loaded_anchor()
    {
        var menu = new CdsMenu();
        Assert.Throws<InvalidOperationException>(() => menu.Open(new Button()));
        Assert.Throws<ArgumentNullException>(() => menu.Open(null!));
    }

    [Fact]
    public void Close_on_a_closed_menu_is_harmless() => new CdsMenu().Close();

    [Fact]
    public void The_binding_context_reaches_items_and_submenu_items()
    {
        var menu = new CdsMenu();
        var parent = new CdsMenuItem("Share");
        var child = new CdsMenuItem("Copy link");
        parent.Items.Add(child);
        menu.Items.Add(parent);
        var context = new object();
        menu.BindingContext = context;
        Assert.Same(context, parent.BindingContext);
        Assert.Same(context, child.BindingContext);
        var late = new CdsMenuItem("Late");
        menu.Items.Add(late);
        Assert.Same(context, late.BindingContext);
    }

    [Fact]
    public void Item_Invoke_raises_Clicked_and_runs_the_command()
    {
        var clicks = 0;
        object? parameter = null;
        var item = new CdsMenuItem("Run") { CommandParameter = 7, Command = new Command(p => parameter = p) };
        item.Clicked += (_, _) => clicks++;
        item.Invoke();
        Assert.Equal(1, clicks);
        Assert.Equal(7, parameter);
    }

    [Fact]
    public void A_command_that_cannot_execute_makes_the_item_not_actionable()
    {
        var item = new CdsMenuItem("Run") { Command = new Command(() => { }, () => false) };
        Assert.False(item.IsActionable);
        item.Command = null;
        Assert.True(item.IsActionable);
        item.IsEnabled = false;
        Assert.False(item.IsActionable);
    }

    [Fact]
    public void Selectable_items_raise_SelectedChanged()
    {
        var item = new CdsMenuItemSelectable { Text = "Show hidden" };
        CdsMenuSelectionChangedEventArgs? args = null;
        item.SelectedChanged += (_, e) => args = e;
        item.IsSelected = true;
        Assert.NotNull(args);
        Assert.Equal(false, args!.OldValue);
        Assert.Equal(true, args.NewValue);
    }

    [Fact]
    public void Radio_groups_raise_SelectedItemChanged()
    {
        var group = new CdsMenuItemRadioGroup();
        group.Items.Add("Name");
        group.Items.Add("Date");
        CdsMenuSelectionChangedEventArgs? args = null;
        group.SelectedItemChanged += (_, e) => args = e;
        group.SelectedItem = "Date";
        Assert.Equal("Date", args!.NewValue);
    }

    [Fact]
    public void A_preview_builds_a_surface_without_a_host()
    {
        var menu = new CdsMenu { Label = "Preview" };
        menu.Items.Add(new CdsMenuItem("Edit", CdsIconName.Edit));
        menu.Items.Add(new CdsMenuItemDivider());
        menu.Items.Add(new CdsMenuItemSelectable { Text = "Show hidden", IsSelected = true });
        menu.Items.Add(new CdsMenuItem("Delete") { Kind = CdsMenuItemKind.Danger });
        Assert.NotNull(menu.CreatePreview());
        Assert.NotNull(menu.CreatePreview(highlightedIndex: 0));
    }
}

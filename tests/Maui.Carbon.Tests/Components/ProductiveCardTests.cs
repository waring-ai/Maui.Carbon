using System.Windows.Input;
using Maui.Carbon.Tests.Foundation;

namespace Maui.Carbon.Tests.Components;

public class ProductiveCardTests
{
    const CdsInteractionState None = CdsInteractionState.None;

    static CardInputs Productive(
        int icons = 0, int overflow = 0, bool primary = false, bool secondary = false,
        CdsCardPlacement actions = CdsCardPlacement.Top, CdsCardPlacement primaryAt = CdsCardPlacement.Bottom,
        CdsCardPlacement secondaryAt = CdsCardPlacement.Bottom, bool click = false, CdsCardClickZone zone = CdsCardClickZone.One,
        bool label = false, bool title = true, bool description = false, CdsCardTitleSize size = CdsCardTitleSize.Default, bool decorator = false) =>
        new(true, icons, overflow, primary, secondary, actions, primaryAt, secondaryAt, click, zone, label, title, description, size, decorator);

    // ----- layout (Card.tsx) -----

    [Fact]
    public void Icons_default_to_the_header_and_leave_no_bottom_bar()
    {
        var layout = CardModel.Layout(Productive(icons: 2));
        Assert.True(layout.HasHeaderActions);
        Assert.False(layout.HasFooterActions);
        Assert.False(layout.HasBottomBar);
        Assert.False(layout.UsesOverflow);
    }

    [Fact]
    public void A_bottom_primary_button_makes_the_bottom_bar()
    {
        var layout = CardModel.Layout(Productive(icons: 2, primary: true));
        Assert.True(layout.HasHeaderActions);
        Assert.True(layout.HasBottomBar);
        Assert.True(layout.HasFooterButton);
        Assert.True(layout.PrimaryInFooter);
        Assert.False(layout.PrimaryInHeader);
    }

    [Fact]
    public void Bottom_actions_share_the_bar_with_the_button()
    {
        var layout = CardModel.Layout(Productive(icons: 1, primary: true, actions: CdsCardPlacement.Bottom, title: false, label: true));
        Assert.False(layout.HasHeaderActions);
        Assert.True(layout.HasFooterActions);
        Assert.True(layout.HasFooterButton);
        Assert.True(layout.LabelOnly);
    }

    [Fact]
    public void A_top_primary_button_counts_as_a_header_action()
    {
        var layout = CardModel.Layout(Productive(primary: true, primaryAt: CdsCardPlacement.Top));
        Assert.True(layout.HasHeaderActions);
        Assert.True(layout.PrimaryInHeader);
        Assert.False(layout.HasBottomBar);
    }

    [Fact]
    public void A_top_secondary_button_shows_only_beside_other_header_actions()
    {
        var alone = CardModel.Layout(Productive(secondary: true, secondaryAt: CdsCardPlacement.Top));
        Assert.False(alone.SecondaryInHeader);
        Assert.False(alone.SecondaryInFooter);
        Assert.True(alone.HasBottomBar); // Carbon still draws the (empty) bar

        var withIcons = CardModel.Layout(Productive(icons: 1, secondary: true, secondaryAt: CdsCardPlacement.Top));
        Assert.True(withIcons.SecondaryInHeader);
    }

    [Fact]
    public void Overflow_replaces_icons_on_productive_cards_only()
    {
        Assert.True(CardModel.Layout(Productive(icons: 2, overflow: 3)).UsesOverflow);
        var expressive = Productive(overflow: 3) with { Productive = false };
        Assert.False(CardModel.Layout(expressive).UsesOverflow);
        Assert.False(CardModel.Layout(expressive).HasHeaderActions);
    }

    [Theory]
    [InlineData(CdsCardClickZone.One)]
    [InlineData(CdsCardClickZone.Two)]
    [InlineData(CdsCardClickZone.Three)]
    public void Productive_cards_use_their_click_zone(CdsCardClickZone zone)
    {
        Assert.Equal(zone, CardModel.Layout(Productive(click: true, zone: zone)).ClickZone);
        Assert.Null(CardModel.Layout(Productive(click: false, zone: zone)).ClickZone);
    }

    [Fact]
    public void Expressive_cards_are_always_clickable_as_a_whole()
    {
        var inputs = Productive(click: true, zone: CdsCardClickZone.Three) with { Productive = false };
        Assert.Equal(CdsCardClickZone.One, CardModel.Layout(inputs).ClickZone);
    }

    [Theory]
    [InlineData(false, false, false, 0)]
    [InlineData(true, false, false, 8)]
    [InlineData(false, true, false, 32)]
    [InlineData(true, true, false, 40)]
    [InlineData(false, true, true, 48)]
    [InlineData(true, true, true, 48)]
    public void Header_end_padding_follows_actions_and_decorator(bool actions, bool decorator, bool largeOrLabel, double expected) =>
        Assert.Equal(expected, CardModel.HeaderEndPadding(actions, decorator, largeOrLabel));

    [Fact]
    public void A_label_only_header_has_8_below_and_titles_16()
    {
        Assert.Equal(CdsSpacing.Spacing03, CardModel.Layout(Productive(title: false, label: true)).HeaderBottomPadding);
        Assert.Equal(CdsSpacing.Spacing05, CardModel.Layout(Productive()).HeaderBottomPadding);
        Assert.False(CardModel.Layout(Productive(label: true, description: true, title: false)).LabelOnly);
    }

    [Fact]
    public void Header_actions_rise_2_unless_the_title_is_large_or_labelled()
    {
        Assert.Equal(-CdsSpacing.Spacing01, CardModel.Layout(Productive(icons: 1)).HeaderActionsTopMargin);
        Assert.Equal(0, CardModel.Layout(Productive(icons: 1, size: CdsCardTitleSize.Large)).HeaderActionsTopMargin);
        Assert.Equal(0, CardModel.Layout(Productive(icons: 1, label: true)).HeaderActionsTopMargin);
        Assert.Equal(0, CardModel.Layout(Productive(icons: 1) with { Productive = false }).HeaderActionsTopMargin);
    }

    [Fact]
    public void Decorator_with_large_title_reserves_48()
    {
        var layout = CardModel.Layout(Productive(icons: 1, decorator: true, size: CdsCardTitleSize.Large));
        Assert.Equal(CdsSpacing.Spacing09, layout.HeaderEndPadding);
    }

    [Theory]
    [InlineData(0, 1, true)]
    [InlineData(0, 2, false)]
    [InlineData(1, 2, true)]
    [InlineData(2, 3, true)]
    public void Ghost_header_buttons_draw_a_separator_except_the_first_of_several(int index, int count, bool expected) =>
        Assert.Equal(expected, CardModel.ShowsSeparator(index, count));

    [Fact]
    public void Productive_bottom_buttons_are_ghosts_and_expressive_keep_their_kind()
    {
        Assert.Equal(CdsButtonKind.Ghost, CardModel.FooterKind(true, true, CdsCardPrimaryButtonKind.Primary, CdsCardSecondaryButtonKind.Secondary));
        Assert.Equal(CdsButtonKind.Ghost, CardModel.FooterKind(true, false, CdsCardPrimaryButtonKind.Primary, CdsCardSecondaryButtonKind.Secondary));
        Assert.Equal(CdsButtonKind.Primary, CardModel.FooterKind(false, true, CdsCardPrimaryButtonKind.Primary, CdsCardSecondaryButtonKind.Ghost));
        Assert.Equal(CdsButtonKind.Ghost, CardModel.FooterKind(false, true, CdsCardPrimaryButtonKind.Ghost, CdsCardSecondaryButtonKind.Secondary));
        Assert.Equal(CdsButtonKind.Secondary, CardModel.FooterKind(false, false, CdsCardPrimaryButtonKind.Ghost, CdsCardSecondaryButtonKind.Secondary));
        Assert.Equal(CdsButtonKind.Ghost, CardModel.FooterKind(false, false, CdsCardPrimaryButtonKind.Primary, CdsCardSecondaryButtonKind.Ghost));
    }

    [Fact]
    public void Icon_buttons_are_small_in_the_header_and_medium_in_a_productive_bar()
    {
        Assert.Equal(CdsSize.Sm, CardModel.ActionSize(true, CdsCardPlacement.Top));
        Assert.Equal(CdsSize.Md, CardModel.ActionSize(true, CdsCardPlacement.Bottom));
        Assert.Equal(CdsSize.Sm, CardModel.ActionSize(false, CdsCardPlacement.Bottom));
    }

    [Fact]
    public void Bottom_bar_spreads_parts_or_ends_them_without_a_button()
    {
        Assert.Empty(CardModel.FooterColumns(0, true));
        Assert.Equal([GridLength.Star, GridLength.Auto], CardModel.FooterColumns(1, false));
        Assert.Equal(1, CardModel.FooterColumn(0, 1, false));
        Assert.Equal([GridLength.Auto, GridLength.Star], CardModel.FooterColumns(1, true));
        Assert.Equal(0, CardModel.FooterColumn(0, 1, true));
        Assert.Equal([GridLength.Auto, GridLength.Star, GridLength.Auto], CardModel.FooterColumns(2, true));
        Assert.Equal(2, CardModel.FooterColumn(1, 2, true));
        Assert.Equal(5, CardModel.FooterColumns(3, true).Count);
        Assert.Equal(4, CardModel.FooterColumn(2, 3, true));
    }

    [Fact]
    public void Click_target_is_named_by_label_and_title_or_by_the_body_for_zone_three()
    {
        Assert.Equal("Usage, Cluster health", CardModel.AccessibleName(CdsCardClickZone.One, "Usage", "Cluster health", "3 nodes"));
        Assert.Equal("Cluster health", CardModel.AccessibleName(CdsCardClickZone.Two, null, "Cluster health", null));
        Assert.Equal("3 nodes", CardModel.AccessibleName(CdsCardClickZone.Three, "Usage", "Cluster health", "3 nodes"));
        Assert.Equal("3 nodes", CardModel.AccessibleName(CdsCardClickZone.One, null, " ", "3 nodes"));
        Assert.Null(CardModel.AccessibleName(CdsCardClickZone.One, null, null, null));
    }

    // ----- tokens -----

    [Fact]
    public void Plain_cards_are_layer_01_with_no_border_and_never_hover()
    {
        var v = CdsCardTokens.Resolve(null, CdsInteractionState.Hover);
        Assert.Equal(CdsColorKeys.Layer01, v.Background);
        Assert.Null(v.Border);
        Assert.Null(v.ZoneBackground);
    }

    [Fact]
    public void Clickable_cards_get_the_tile_border_and_hover_fill()
    {
        Assert.Equal(new CdsCardVisuals(CdsColorKeys.Layer01, CdsColorKeys.BorderTile01, null), CdsCardTokens.Resolve(CdsCardClickZone.One, None));
        Assert.Equal(CdsColorKeys.LayerHover01, CdsCardTokens.Resolve(CdsCardClickZone.One, CdsInteractionState.Hover).Background);
        Assert.Equal(CdsColorKeys.Layer01, CdsCardTokens.Resolve(CdsCardClickZone.One, CdsInteractionState.Hover | CdsInteractionState.Disabled).Background);
    }

    [Theory]
    [InlineData(CdsCardClickZone.Two)]
    [InlineData(CdsCardClickZone.Three)]
    public void Partial_zones_hover_only_their_zone(CdsCardClickZone zone)
    {
        var rest = CdsCardTokens.Resolve(zone, None);
        Assert.Null(rest.ZoneBackground);
        Assert.Null(rest.Border);
        var hover = CdsCardTokens.Resolve(zone, CdsInteractionState.Hover);
        Assert.Equal(CdsColorKeys.Layer01, hover.Background);
        Assert.Equal(CdsColorKeys.LayerHover01, hover.ZoneBackground);
    }

    [Fact]
    public void Title_styles_follow_the_card_kind_and_size()
    {
        Assert.Equal(CdsTypeStyle.HeadingCompact02, CdsCardTokens.TitleStyle(true, CdsCardTitleSize.Default));
        Assert.Equal(CdsTypeStyle.Heading03, CdsCardTokens.TitleStyle(true, CdsCardTitleSize.Large));
        Assert.Equal(CdsTypeStyle.Heading03, CdsCardTokens.TitleStyle(false, CdsCardTitleSize.Default));
    }

    // ----- control -----

    [Fact]
    public void Every_property_can_be_set_without_a_platform()
    {
        var command = new Command(() => { });
        var card = new CdsProductiveCard
        {
            Title = "Cluster health",
            Label = "Usage",
            Description = "Last 24 hours",
            Body = new CdsText("3 of 3 nodes ready"),
            Decorator = new CdsAILabel(),
            ActionsPlacement = CdsCardPlacement.Bottom,
            ClickZone = CdsCardClickZone.Two,
            TitleSize = CdsCardTitleSize.Large,
            PrimaryButtonText = "View details",
            PrimaryButtonIcon = CdsIconName.ArrowRight,
            PrimaryButtonHref = "https://www.ibm.com",
            PrimaryButtonPlacement = CdsCardPlacement.Top,
            PrimaryButtonCommand = command,
            IsPrimaryButtonEnabled = false,
            SecondaryButtonText = "Dismiss",
            SecondaryButtonIcon = CdsIconName.Close,
            SecondaryButtonHref = "https://www.ibm.com",
            SecondaryButtonPlacement = CdsCardPlacement.Top,
            SecondaryButtonCommand = command,
            IsSecondaryButtonEnabled = false,
            OverflowDescription = "More actions",
            Command = command,
            CommandParameter = 1,
        };
        card.ActionIcons.Add(new CdsCardAction(CdsIconName.Edit, "Edit"));
        card.OverflowActions.Add(new CdsMenuItem("Edit"));
        card.OverflowActions.Add(new CdsMenuItem("Delete"));
        card.Decorator = null;
        card.OverflowActions.Clear();
        card.ActionIcons.Clear();
        card.IsEnabled = false;
        Assert.True(card.IsClickable);
    }

    [Fact]
    public void Handling_Clicked_makes_the_card_clickable_and_activation_raises_it()
    {
        var card = new CdsProductiveCard { Title = "Cluster health", Label = "Usage", Description = "Last 24 hours" };
        Assert.False(card.IsClickable);
        Assert.Null(card.HitTarget);
        var clicks = 0;
        card.Clicked += (_, _) => clicks++;
        Assert.True(card.IsClickable);
        var hit = card.HitTarget!;
        Assert.Equal("Usage, Cluster health", SemanticProperties.GetDescription(hit));
        Assert.Equal("Last 24 hours", SemanticProperties.GetHint(hit));
        hit.RaiseKeyboardActivation();
        Assert.Equal(1, clicks);
    }

    [Fact]
    public void A_command_also_makes_the_card_clickable()
    {
        var ran = 0;
        var card = new CdsProductiveCard { Title = "Cluster health", Command = new Command(() => ran++) };
        Assert.NotNull(card.HitTarget);
        card.HitTarget!.RaiseKeyboardActivation();
        Assert.Equal(1, ran);
        card.Command = null;
        Assert.Null(card.HitTarget);
    }

    [Fact]
    public void Buttons_raise_their_events_and_commands()
    {
        var primary = 0;
        var secondary = 0;
        var commands = 0;
        var card = new CdsProductiveCard
        {
            Title = "Cluster health",
            PrimaryButtonText = "View details",
            SecondaryButtonText = "Dismiss",
            PrimaryButtonCommand = new Command(() => commands++),
        };
        card.PrimaryButtonClicked += (_, _) => primary++;
        card.SecondaryButtonClicked += (_, _) => secondary++;
        var buttons = ContainerSemanticsAuditTests.Walk(card).OfType<CdsButton>().ToList();
        Assert.Equal(["Dismiss", "View details"], buttons.Select(b => b.Text).Order());
        Assert.All(buttons, b => Assert.Equal(CdsButtonKind.Ghost, b.Kind));
        foreach (var button in buttons) ContainerSemanticsAuditTests.Walk(button).OfType<CdsHitTarget>().First().RaiseKeyboardActivation();
        Assert.Equal(1, primary);
        Assert.Equal(1, secondary);
        Assert.Equal(1, commands);
    }

    [Fact]
    public void Action_icons_are_named_ghost_buttons_and_raise_their_action()
    {
        var card = new CdsProductiveCard { Title = "Cluster health" };
        var edits = 0;
        var edit = new CdsCardAction(CdsIconName.Edit, "Edit");
        edit.Clicked += (_, _) => edits++;
        card.ActionIcons.Add(edit);
        card.ActionIcons.Add(new CdsCardAction(CdsIconName.TrashCan, "Delete"));
        var buttons = ContainerSemanticsAuditTests.Walk(card).OfType<CdsButton>().ToList();
        Assert.Equal(2, buttons.Count);
        Assert.All(buttons, b => Assert.True(b.IsIconOnly));
        Assert.All(buttons, b => Assert.Equal(CdsSize.Sm, b.Size));
        var editButton = Assert.Single(buttons, b => b.IconDescription == "Edit");
        ContainerSemanticsAuditTests.Walk(editButton).OfType<CdsHitTarget>().First().RaiseKeyboardActivation();
        Assert.Equal(1, edits);

        card.ActionsPlacement = CdsCardPlacement.Bottom;
        Assert.All(ContainerSemanticsAuditTests.Walk(card).OfType<CdsButton>(), b => Assert.Equal(CdsSize.Md, b.Size));
    }

    [Fact]
    public void Overflow_actions_replace_the_icons_with_one_menu()
    {
        var card = new CdsProductiveCard { Title = "Cluster health" };
        card.ActionIcons.Add(new CdsCardAction(CdsIconName.Edit, "Edit"));
        card.OverflowActions.Add(new CdsMenuItem("Edit"));
        card.OverflowActions.Add(new CdsMenuItem("Delete"));
        var menu = Assert.Single(ContainerSemanticsAuditTests.Walk(card).OfType<CdsOverflowMenu>());
        Assert.Equal(2, menu.Items.Count);
        Assert.Equal("Options", menu.IconDescription);
        Assert.Empty(ContainerSemanticsAuditTests.Walk(card).OfType<CdsButton>());
        card.ActionsPlacement = CdsCardPlacement.Bottom;
        menu = Assert.Single(ContainerSemanticsAuditTests.Walk(card).OfType<CdsOverflowMenu>());
        Assert.Equal(CdsSize.Md, menu.Size);
    }
}

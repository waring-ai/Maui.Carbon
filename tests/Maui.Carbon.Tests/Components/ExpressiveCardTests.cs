using Maui.Carbon.Tests.Foundation;

namespace Maui.Carbon.Tests.Components;

public class ExpressiveCardTests
{
    [Fact]
    public void Every_property_can_be_set_without_a_platform()
    {
        var command = new Command(() => { });
        var card = new CdsExpressiveCard
        {
            Title = "Deploy a sample app",
            Label = "Getting started",
            Description = "About 10 minutes",
            Body = new CdsText("Run a starter app on Code Engine."),
            Media = new CdsAspectRatio(),
            MediaPosition = CdsCardMediaPosition.Left,
            Pictogram = CdsIconName.Dashboard,
            PrimaryButtonText = "Start",
            PrimaryButtonKind = CdsCardPrimaryButtonKind.Ghost,
            PrimaryButtonIcon = CdsIconName.ArrowRight,
            PrimaryButtonHref = "https://www.ibm.com",
            PrimaryButtonCommand = command,
            SecondaryButtonText = "Remove",
            SecondaryButtonKind = CdsCardSecondaryButtonKind.Ghost,
            SecondaryButtonIcon = CdsIconName.TrashCan,
            SecondaryButtonHref = "https://www.ibm.com",
            SecondaryButtonCommand = command,
            Decorator = new CdsAILabel(),
            Command = command,
            CommandParameter = "app",
        };
        card.ActionIcons.Add(new CdsCardAction(CdsIconName.ArrowRight, "Open") { Href = "https://www.ibm.com" });
        card.MediaPosition = CdsCardMediaPosition.Top;
        card.Media = null;
        card.Pictogram = null;
        card.Decorator = null;
        card.IsEnabled = false;
        Assert.True(card.IsClickable);
    }

    [Fact]
    public void Bottom_bar_keeps_the_chosen_button_kinds_in_secondary_then_primary_order()
    {
        var card = new CdsExpressiveCard { Title = "Deploy a sample app", PrimaryButtonText = "Start", SecondaryButtonText = "Remove", SecondaryButtonKind = CdsCardSecondaryButtonKind.Ghost };
        var buttons = ContainerSemanticsAuditTests.Walk(card).OfType<CdsButton>().ToList();
        Assert.Equal(CdsButtonKind.Ghost, Assert.Single(buttons, b => b.Text == "Remove").Kind);
        Assert.Equal(CdsButtonKind.Primary, Assert.Single(buttons, b => b.Text == "Start").Kind);
        Assert.All(buttons, b => Assert.Equal(CdsSize.Md, b.Size));
    }

    [Fact]
    public void Action_icons_are_small_ghost_icon_buttons_in_the_bottom_bar()
    {
        var card = new CdsExpressiveCard { Title = "Deploy a sample app" };
        var opened = 0;
        var action = new CdsCardAction(CdsIconName.ArrowRight, "Open the tutorial");
        action.Clicked += (_, _) => opened++;
        card.ActionIcons.Add(action);
        var button = Assert.Single(ContainerSemanticsAuditTests.Walk(card).OfType<CdsIconButton>());
        Assert.Equal(CdsIconButtonKind.Ghost, button.Kind);
        Assert.Equal(CdsSize.Sm, button.Size);
        Assert.Equal("Open the tutorial", button.Label);
        Assert.True(card.CurrentLayout.HasFooterActions);
        ContainerSemanticsAuditTests.Walk(button).OfType<CdsHitTarget>().First().RaiseKeyboardActivation();
        Assert.Equal(1, opened);
    }

    [Fact]
    public void A_clickable_expressive_card_is_one_target()
    {
        var card = new CdsExpressiveCard { Title = "Deploy a sample app", Label = "Getting started" };
        var clicks = 0;
        card.Clicked += (_, _) => clicks++;
        Assert.Equal(CdsCardClickZone.One, card.CurrentLayout.ClickZone);
        Assert.Equal("Getting started, Deploy a sample app", SemanticProperties.GetDescription(card.HitTarget!));
        card.HitTarget!.RaiseKeyboardActivation();
        Assert.Equal(1, clicks);
    }

    [Fact]
    public void Expressive_layout_ignores_productive_only_options()
    {
        var card = new CdsExpressiveCard { Title = "Deploy a sample app", PrimaryButtonText = "Start" };
        Assert.True(card.CurrentLayout.PrimaryInFooter);
        Assert.False(card.CurrentLayout.HasHeaderActions);
        Assert.Equal(0, card.CurrentLayout.HeaderActionsTopMargin);
    }
}

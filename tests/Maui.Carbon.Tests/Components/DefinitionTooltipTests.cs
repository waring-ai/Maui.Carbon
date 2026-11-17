namespace Maui.Carbon.Tests.Components;

public class DefinitionTooltipTests
{
    [Fact]
    public void Underline_turns_interactive_on_hover_and_focus()
    {
        Assert.Equal(CdsColorKeys.BorderStrong01, CdsDefinitionTooltipTokens.Underline(CdsInteractionState.None));
        Assert.Equal(CdsColorKeys.BorderInteractive, CdsDefinitionTooltipTokens.Underline(CdsInteractionState.Hover));
        Assert.Equal(CdsColorKeys.BorderInteractive, CdsDefinitionTooltipTokens.Underline(CdsInteractionState.FocusVisible));
        Assert.Equal(CdsColorKeys.BorderInteractive, CdsDefinitionTooltipTokens.Underline(CdsInteractionState.Focused));
        Assert.Equal(CdsColorKeys.TextPrimary, CdsDefinitionTooltipTokens.Text);
    }

    [Fact]
    public void Term_can_be_created_and_toggled_before_loading()
    {
        var term = new CdsDefinitionTooltip
        {
            Text = "GB-hours",
            Definition = "Memory allocated multiplied by hours running.",
            OpenOnHover = true,
            Placement = CdsPlacement.Top,
            TypeStyle = CdsTypeStyle.Body02,
        };
        term.IsOpen = true;
        term.IsOpen = false;
        Assert.True(term.OpenOnHover);
        Assert.Equal(CdsPlacement.Bottom, new CdsDefinitionTooltip().Placement);
    }
}

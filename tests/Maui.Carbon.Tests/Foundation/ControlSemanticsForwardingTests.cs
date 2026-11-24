namespace Maui.Carbon.Tests.Foundation;

/// <summary>
/// A Description or Hint a consumer sets on a Cds control (a ContentView) must not reach the control's own platform view:
/// on iOS and Mac Catalyst that makes it one accessibility element and hides its hit target. It goes to the one hit
/// target, or names the control as a group when there are several.
/// </summary>
public class ControlSemanticsForwardingTests
{
    static Semantics? Platform(IView view) => view.Semantics;

    [Fact]
    public void A_description_on_a_button_names_its_hit_target_and_stays_off_the_content_view()
    {
        var button = new CdsButton { Text = "Deploy" };
        var hit = (CdsHitTarget)button.AccessibilityTarget!;
        Assert.Equal("Deploy", SemanticProperties.GetDescription(hit));

        SemanticProperties.SetDescription(button, "Deploy cluster prod-eu-de-1");
        SemanticProperties.SetHint(button, "Starts a rolling deployment");
        Assert.Same(hit, button.SemanticForwardTarget);
        Assert.Equal("Deploy cluster prod-eu-de-1", SemanticProperties.GetDescription(hit));
        Assert.Equal("Starts a rolling deployment", SemanticProperties.GetHint(hit));
        Assert.Null(Platform(button)?.Description);
        Assert.Null(Platform(button)?.Hint);

        // The control's own updates do not take the consumer's name away.
        button.Text = "Redeploy";
        Assert.Equal("Deploy cluster prod-eu-de-1", SemanticProperties.GetDescription(hit));

        // Clearing it gives the hit target the control's own name back.
        SemanticProperties.SetDescription(button, null);
        SemanticProperties.SetHint(button, null);
        Assert.Equal("Redeploy", SemanticProperties.GetDescription(hit));
        Assert.Null(SemanticProperties.GetHint(hit));
        Assert.Null(button.SemanticForwardTarget);
    }

    [Fact]
    public void A_hint_is_added_after_the_controls_own_hint()
    {
        var input = new CdsTextInput { Label = "Cluster name", HelperText = "Lowercase letters and dashes" };
        var entry = input.AccessibilityTarget!;
        var own = SemanticProperties.GetHint(entry);
        SemanticProperties.SetHint(input, "Used in the URL");
        Assert.Equal(string.IsNullOrEmpty(own) ? "Used in the URL" : $"{own}. Used in the URL", SemanticProperties.GetHint(entry));
        Assert.Equal("Cluster name", SemanticProperties.GetDescription(entry));
        Assert.Null(Platform(input)?.Hint);
    }

    [Fact]
    public void A_control_with_several_targets_is_named_as_a_group()
    {
        // Waring Management sets the Description on multi-line code snippets (copy and show-more buttons inside).
        var snippet = new CdsCodeSnippet { Type = CdsCodeSnippetType.Multi, Code = "line 1\nline 2" };
        SemanticProperties.SetDescription(snippet, "Legend");
        Assert.Equal("Legend", CdsSemantics.GetGroupName(snippet));
        Assert.Null(Platform(snippet)?.Description);
        Assert.Null(snippet.SemanticForwardTarget);
        // The audit reads the attached value, which the snippet still holds; only the snippet itself may show up, and its
        // platform semantics (above) carry no Description.
        Assert.DoesNotContain(ContainerSemanticsAuditTests.Violations(snippet, "snippet"), v => !v.StartsWith("snippet: CdsCodeSnippet ", StringComparison.Ordinal));

        SemanticProperties.SetDescription(snippet, null);
        Assert.Null(CdsSemantics.GetGroupName(snippet));
    }

    [Fact]
    public void A_control_that_names_itself_as_a_group_keeps_its_own_name()
    {
        var nav = new CdsSideNav { Label = "Main navigation" };
        nav.Body = new VerticalStackLayout { new CdsHitTarget() };
        SemanticProperties.SetDescription(nav, "Something else");
        Assert.Equal("Main navigation", CdsSemantics.GetGroupName(nav));
        Assert.Null(Platform(nav)?.Description);
    }

    [Fact]
    public void A_control_with_nothing_interactive_keeps_the_name_itself()
    {
        var tile = new CdsTile { Body = new CdsText("Storage used") };
        SemanticProperties.SetDescription(tile, "Storage used: 4.2 TB of 10 TB");
        Assert.Equal("Storage used: 4.2 TB of 10 TB", Platform(tile)?.Description);
        Assert.Null(CdsSemantics.GetGroupName(tile));
    }

    [Fact]
    public void A_heading_level_on_the_control_still_reaches_the_platform()
    {
        var tile = new CdsClickableTile { Body = new CdsText("View clusters") };
        SemanticProperties.SetHeadingLevel(tile, SemanticHeadingLevel.Level3);
        SemanticProperties.SetDescription(tile, "Clusters");
        Assert.Equal(SemanticHeadingLevel.Level3, Platform(tile)?.HeadingLevel);
        Assert.Null(Platform(tile)?.Description);
        Assert.Equal("Clusters", SemanticProperties.GetDescription(tile.AccessibilityTarget!));
    }
}

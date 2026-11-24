using Maui.Carbon.Tests.Foundation;

namespace Maui.Carbon.Tests.Components;

public class CodeSnippetTests
{
    static CdsSnippetAction CopyButton(CdsCodeSnippet snippet) =>
        ContainerSemanticsAuditTests.Walk(snippet).OfType<CdsSnippetAction>().Single(a => a.Description == snippet.CopyButtonDescription);

    static void Activate(CdsControl control) =>
        ContainerSemanticsAuditTests.Walk(control).OfType<CdsHitTarget>().First().RaiseKeyboardActivation();

    // ----- MS-04: a code change updates the text in place -----

    [Theory]
    [InlineData(CdsCodeSnippetType.Single)]
    [InlineData(CdsCodeSnippetType.Multi)]
    public void Changing_only_the_code_keeps_the_layout_buttons_and_code_area(CdsCodeSnippetType type)
    {
        var snippet = new CdsCodeSnippet { Type = type, Code = "line 1" };
        var chrome = snippet.Content;
        var copy = CopyButton(snippet);
        var area = snippet.CodeArea;
        snippet.Code = "line 1\nline 2\nline 3";
        Assert.Same(chrome, snippet.Content);
        Assert.Same(copy, CopyButton(snippet));
        Assert.Same(area, snippet.CodeArea);
        var code = ContainerSemanticsAuditTests.Walk(snippet).OfType<CdsText>().Single(t => t.TypeStyle == CdsTypeStyle.Code01);
        Assert.Equal("line 1\nline 2\nline 3", code.Text);
        if (type == CdsCodeSnippetType.Multi) Assert.Equal(3, code.MaxLines);
    }

    [Fact]
    public void Layout_properties_still_rebuild_the_snippet()
    {
        var snippet = new CdsCodeSnippet { Type = CdsCodeSnippetType.Multi, Code = "a" };
        var chrome = snippet.Content;
        snippet.IsWrapText = true;
        Assert.NotSame(chrome, snippet.Content);
        chrome = snippet.Content;
        snippet.HideCopyButton = true;
        Assert.NotSame(chrome, snippet.Content);
        chrome = snippet.Content;
        snippet.Type = CdsCodeSnippetType.Single;
        Assert.NotSame(chrome, snippet.Content);
    }

    // ----- CN-09: the code area is a focusable, read-only text box -----

    [Theory]
    [InlineData(CdsCodeSnippetType.Single)]
    [InlineData(CdsCodeSnippetType.Multi)]
    public void The_code_area_is_a_read_only_text_box_named_by_the_label(CdsCodeSnippetType type)
    {
        var snippet = new CdsCodeSnippet { Type = type, Code = "cloud login --sso", HideCopyButton = true };
        var area = Assert.IsType<CdsHitTarget>(snippet.CodeArea);
        Assert.Equal(CdsAccessibilityRole.TextBox, area.Role);
        Assert.True(area.IsReadOnly);
        Assert.True(area.IsTabStop);
        // Without a label the code is the name; with one the code becomes the description.
        Assert.Equal("cloud login --sso", SemanticProperties.GetDescription(area));
        snippet.Label = "Log in command";
        Assert.Equal("Log in command", SemanticProperties.GetDescription(area));
        Assert.Equal("cloud login --sso", SemanticProperties.GetHint(area));
        // The name lives on the text box, never on a container (that would hide the buttons from VoiceOver).
        Assert.Null(SemanticProperties.GetDescription(snippet));
        Assert.DoesNotContain(ContainerSemanticsAuditTests.Walk(snippet).OfType<ScrollView>(), s => SemanticProperties.GetDescription(s) is not null);
    }

    [Fact]
    public void Inline_snippets_have_no_code_area_and_their_button_is_named_by_the_label()
    {
        var snippet = new CdsCodeSnippet { Type = CdsCodeSnippetType.Inline, Code = "kubectl get nodes", Label = "Node list command" };
        Assert.Null(snippet.CodeArea);
        Assert.False(snippet.WantsNavigationKeys);
        var hit = ContainerSemanticsAuditTests.Walk(snippet).OfType<CdsHitTarget>().Single();
        Assert.Equal("Node list command", SemanticProperties.GetDescription(hit));
        Assert.Equal("kubectl get nodes", SemanticProperties.GetHint(hit));
    }

    [Fact]
    public void A_disabled_snippet_takes_its_code_area_out_of_the_tab_order()
    {
        var snippet = new CdsCodeSnippet { Type = CdsCodeSnippetType.Multi, Code = "a\nb" };
        snippet.IsEnabled = false;
        Assert.False(snippet.CodeArea!.IsEnabled);
        snippet.IsEnabled = true;
        Assert.True(snippet.CodeArea!.IsEnabled);
    }

    [Fact]
    public void Keys_reach_the_code_area_only_while_it_has_focus()
    {
        var snippet = new CdsCodeSnippet { Type = CdsCodeSnippetType.Multi, Code = "a\nb" };
        Assert.True(snippet.WantsNavigationKeys);
        Assert.False(snippet.OnKeyDown(new CdsKeyEventArgs(CdsKey.Down)));
    }

    [Theory]
    // key, x, y, expected x, expected y (viewport 200x100, content 500x400)
    [InlineData(CdsKey.Down, 0, 0, 0, 40)]
    [InlineData(CdsKey.Up, 0, 20, 0, 0)]
    [InlineData(CdsKey.Right, 0, 0, 40, 0)]
    [InlineData(CdsKey.Left, 10, 0, 0, 0)]
    [InlineData(CdsKey.PageDown, 0, 0, 0, 60)]
    [InlineData(CdsKey.PageUp, 0, 300, 0, 240)]
    [InlineData(CdsKey.End, 30, 0, 30, 300)]
    [InlineData(CdsKey.Home, 30, 200, 30, 0)]
    [InlineData(CdsKey.Down, 0, 300, 0, 300)] // at the bottom: still taken, clamped
    public void Keys_scroll_the_code_area(CdsKey key, double x, double y, double toX, double toY) =>
        Assert.Equal((toX, toY), CdsCodeSnippetModel.ScrollOffset(key, x, y, 200, 100, 500, 400));

    [Fact]
    public void A_single_line_scrolls_sideways_only()
    {
        Assert.Null(CdsCodeSnippetModel.ScrollOffset(CdsKey.Down, 0, 0, 200, 16, 500, 16));
        Assert.Equal((300d, 0d), CdsCodeSnippetModel.ScrollOffset(CdsKey.End, 0, 0, 200, 16, 500, 16));
        Assert.Equal((0d, 0d), CdsCodeSnippetModel.ScrollOffset(CdsKey.Home, 100, 0, 200, 16, 500, 16));
        Assert.Null(CdsCodeSnippetModel.ScrollOffset(CdsKey.Right, 0, 0, 200, 16, 150, 16)); // fits: nothing to scroll
        Assert.Null(CdsCodeSnippetModel.ScrollOffset(CdsKey.Enter, 0, 0, 200, 16, 500, 16));
    }

    [Fact]
    public void The_text_box_role_is_reported_on_every_platform()
    {
        Assert.Equal("text box", CdsHitTargetStates.AndroidRoleDescription(CdsAccessibilityRole.TextBox));
        Assert.Equal("android.view.View", CdsHitTargetStates.AndroidClassName(CdsAccessibilityRole.TextBox));
        Assert.Equal("read only", CdsHitTargetStates.AppleValue(CdsAccessibilityRole.TextBox, false, null, true, CdsCurrentKind.None, true));
    }

    // ----- CN-10: copy feedback reflects the clipboard result -----

    [Fact]
    public void A_failed_copy_says_so_and_skips_clicked_and_the_command()
    {
        var ran = false;
        var snippet = new CdsCodeSnippet { Code = "secret", Command = new Command(() => ran = true) };
        snippet.WriteClipboard = _ => Task.FromException(new InvalidOperationException("no clipboard"));
        var clicked = 0;
        var failed = 0;
        snippet.Clicked += (_, _) => clicked++;
        snippet.CopyFailed += (_, _) => failed++;
        using var heard = CdsAnnouncer.Capture();
        Activate(CopyButton(snippet));
        Assert.Equal(1, failed);
        Assert.Equal(0, clicked);
        Assert.False(ran);
        Assert.Equal([new CdsAnnouncement("Copy failed", CdsAnnouncePriority.Assertive)], heard.Announcements);
    }

    [Fact]
    public void A_successful_copy_writes_the_copy_text_and_announces_the_feedback()
    {
        string? written = null;
        var ran = false;
        var snippet = new CdsCodeSnippet { Code = "cloud login", CopyText = "cloud login --sso", Feedback = "Copied to clipboard", Command = new Command(() => ran = true) };
        snippet.WriteClipboard = text => { written = text; return Task.CompletedTask; };
        var clicked = 0;
        var failed = 0;
        snippet.Clicked += (_, _) => clicked++;
        snippet.CopyFailed += (_, _) => failed++;
        using var heard = CdsAnnouncer.Capture();
        Activate(CopyButton(snippet));
        Assert.Equal("cloud login --sso", written);
        Assert.Equal(1, clicked);
        Assert.Equal(0, failed);
        Assert.True(ran);
        Assert.Equal([new CdsAnnouncement("Copied to clipboard", CdsAnnouncePriority.Polite)], heard.Announcements);
    }

    [Fact]
    public void Enter_on_the_code_area_does_not_copy()
    {
        var writes = 0;
        var snippet = new CdsCodeSnippet { Code = "a" };
        snippet.WriteClipboard = _ => { writes++; return Task.CompletedTask; };
        snippet.CodeArea!.RaiseKeyboardActivation();
        Assert.Equal(0, writes);
        Activate(CopyButton(snippet));
        Assert.Equal(1, writes);
    }

    [Fact]
    public void Lines_are_split_on_every_line_ending()
    {
        Assert.Empty(CdsCodeSnippetModel.SplitLines(null));
        Assert.Empty(CdsCodeSnippetModel.SplitLines(""));
        Assert.Equal(["a"], CdsCodeSnippetModel.SplitLines("a"));
        Assert.Equal(["a", "b", "c"], CdsCodeSnippetModel.SplitLines("a\nb\r\nc"));
        Assert.Equal(["a", "b"], CdsCodeSnippetModel.SplitLines("a\rb"));
        Assert.Equal(3, CdsCodeSnippetModel.LineCount("a\nb\n"));
    }

    [Theory]
    [InlineData(16, false, 48)]    // fewer than the minimum rows: padded to three rows
    [InlineData(100, false, 100)]
    [InlineData(400, false, 240)]  // capped at fifteen rows
    [InlineData(400, true, 400)]   // expanded: unlimited
    [InlineData(100, true, 256)]   // expanded: at least sixteen rows
    public void Viewport_height_is_clamped_to_the_row_limits(double content, bool expanded, double expectedHeight) =>
        Assert.Equal(expectedHeight, CdsCodeSnippetModel.ViewportHeight(content, expanded, 3, 15, 16, 0));

    [Fact]
    public void Expanded_height_respects_a_maximum()
    {
        Assert.Equal(20 * 16, CdsCodeSnippetModel.ViewportHeight(1000, true, 3, 15, 16, 20));
        Assert.Equal(16 * 16, CdsCodeSnippetModel.ViewportHeight(1000, true, 3, 15, 16, 10));
    }

    [Fact]
    public void Expand_button_only_shows_for_code_taller_than_the_collapsed_maximum()
    {
        Assert.False(CdsCodeSnippetModel.ShowsExpandButton(240, 15));
        Assert.True(CdsCodeSnippetModel.ShowsExpandButton(241, 15));
        Assert.False(CdsCodeSnippetModel.ShowsExpandButton(40, 15));
    }

    [Fact]
    public void Expand_text_and_chevron_follow_state()
    {
        Assert.Equal("Show more", CdsCodeSnippetModel.ExpandText(false, "Show more", "Show less"));
        Assert.Equal("Show less", CdsCodeSnippetModel.ExpandText(true, "Show more", "Show less"));
        Assert.Equal(0, CdsCodeSnippetModel.ChevronRotation(false));
        Assert.Equal(180, CdsCodeSnippetModel.ChevronRotation(true));
    }

    [Fact]
    public void Copy_text_replaces_the_code()
    {
        Assert.Equal("code", CdsCodeSnippetModel.TextToCopy("code", null));
        Assert.Equal("other", CdsCodeSnippetModel.TextToCopy("code", "other"));
        Assert.Equal("", CdsCodeSnippetModel.TextToCopy(null, null));
    }

    [Fact]
    public void Fill_follows_state_and_disabled_stays_flat()
    {
        Assert.Equal(CdsColorKeys.Layer01, CdsCodeSnippetTokens.Fill(CdsInteractionState.None));
        Assert.Equal(CdsColorKeys.LayerHover01, CdsCodeSnippetTokens.Fill(CdsInteractionState.Hover));
        Assert.Equal(CdsColorKeys.LayerActive01, CdsCodeSnippetTokens.Fill(CdsInteractionState.Hover | CdsInteractionState.Pressed));
        Assert.Equal(CdsColorKeys.Layer01, CdsCodeSnippetTokens.Fill(CdsInteractionState.Disabled | CdsInteractionState.Hover));
        Assert.Equal(CdsColorKeys.Layer01, CdsCodeSnippetTokens.PlainFill);
    }

    [Fact]
    public void Text_icon_and_border_tokens()
    {
        Assert.Equal(CdsColorKeys.TextPrimary, CdsCodeSnippetTokens.Text(false));
        Assert.Equal(CdsColorKeys.TextDisabled, CdsCodeSnippetTokens.Text(true));
        Assert.Equal(CdsColorKeys.IconPrimary, CdsCodeSnippetTokens.Icon(false));
        Assert.Equal(CdsColorKeys.IconDisabled, CdsCodeSnippetTokens.Icon(true));
        Assert.Equal(CdsColorKeys.Focus, CdsCodeSnippetTokens.InlineBorder(CdsInteractionState.FocusVisible));
        Assert.Null(CdsCodeSnippetTokens.InlineBorder(CdsInteractionState.Hover));
    }

    [Theory]
    [InlineData(CdsCodeSnippetType.Single)]
    [InlineData(CdsCodeSnippetType.Inline)]
    [InlineData(CdsCodeSnippetType.Multi)]
    public void Every_type_can_be_created_and_reconfigured(CdsCodeSnippetType type)
    {
        var snippet = new CdsCodeSnippet { Code = "cloud login --sso", Type = type };
        snippet.HideCopyButton = true;
        snippet.IsWrapText = true;
        snippet.HideCopyButton = false;
        snippet.IsExpanded = true;
        snippet.MaxCollapsedNumberOfRows = 5;
        snippet.Feedback = "Copied to clipboard";
        snippet.IsEnabled = false;
        Assert.Equal(type, snippet.Type);
        Assert.Equal(2000, snippet.FeedbackTimeout);
        Assert.Equal("Copy to clipboard", snippet.CopyButtonDescription);
        Assert.Equal(CdsPlacement.BottomEnd, snippet.FeedbackPlacement);
    }
}

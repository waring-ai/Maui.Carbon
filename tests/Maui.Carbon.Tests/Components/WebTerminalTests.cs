namespace Maui.Carbon.Tests.Components;

public class WebTerminalTests
{
    [Fact]
    public void Opens_docked_and_narrows_the_page()
    {
        var page = new VerticalStackLayout();
        var host = new CdsOverlayHost { Body = page };
        var content = new ContentView();
        page.Add(content);
        var terminal = new CdsWebTerminal { PageContent = content, Body = new Label { Text = "$" } };
        terminal.Actions.Add(new CdsIconButton(CdsIconName.Copy, "Copy output"));
        terminal.DocumentationLinks.Add(new CdsMenuItem { Text = "Commands" });
        page.Add(terminal);
        terminal.IsOpen = true;
        terminal.Show();
        var overlay = Assert.Single(host.Overlays);
        Assert.Equal(CdsOverlayDock.End, overlay.Options.Dock);
        Assert.False(overlay.Options.IsModal);
        Assert.Equal(CdsWebTerminal.PanelWidth, content.Margin.Right);
        terminal.IsOpen = false;
        Assert.Empty(host.Overlays);
        Assert.Equal(0, content.Margin.Right);
    }
}

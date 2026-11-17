using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Navigation;

[GalleryPage("Tabs", GalleryCategory.Navigation, Order = 30,
    Summary = "Switches between related views of the same context without leaving the page.")]
public partial class TabsPage : GalleryPage
{
    public TabsPage()
    {
        InitializeComponent();

        foreach (var name in new[] { "main.tf", "variables.tf", "outputs.tf" })
            FileTabs.Items.Add(new CdsTab { Text = name, Body = new CdsText($"Contents of {name}", CdsTypeStyle.Body01) });

        foreach (var name in new[] { "Overview", "Nodes", "Networking", "Storage", "Access", "Logging", "Monitoring", "Billing" })
            ManyTabs.Items.Add(new CdsTab { Text = name });
    }

    void OnTabCloseRequested(object? sender, CdsTabCloseRequestedEventArgs e)
    {
        FileTabs.Items.Remove(e.Tab);
        CloseStatus.Text = $"{FileTabs.Items.Count} files open (closed {e.Tab.Text})";
    }

    void OnSelectionChanged(object? sender, CdsTabsSelectionChangedEventArgs e) =>
        Status.Text = $"Selected: {InteractiveTabs.SelectedTab?.Text}";
}

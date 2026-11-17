using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Navigation;

[GalleryPage("UI shell", GalleryCategory.Navigation, Order = 50, Spec = "UIShell",
    Summary = "The application frame: a 48px header with the product name, navigation and global actions, and a side nav that is either fully expanded (256px) or a 48px icon rail.")]
public partial class UIShellPage : GalleryPage
{
    CdsShellItem[] _navItems = [];

    public UIShellPage()
    {
        InitializeComponent();

        foreach (var frame in new[] { ShellFrame, RailFrame, RailOpenFrame, SmallFrame })
        {
            frame.StrokeThickness = 1;
            CdsTokens.SetBrush(frame, Microsoft.Maui.Controls.Border.StrokeProperty, CdsColorKeys.BorderSubtle01);
        }

        _navItems = [NavDashboard, NavClusters, NavBuckets, NavNetworks, NavBilling];

        // A header menu shown open, next to its closed trigger, so a screenshot shows the dropdown.
        var preview = new CdsHeaderMenu { Label = "Manage", MenuLabel = "Manage" };
        preview.Items.Add(new CdsHeaderMenuItem("Users"));
        preview.Items.Add(new CdsHeaderMenuItem("Access groups", isActive: true));
        preview.Items.Add(new CdsHeaderMenuItem("Audit log"));
        MenuPreviewHost.Content = preview.CreateDropdownPreview(highlightedIndex: 0);

        var trigger = new CdsHeaderMenu { Label = "Manage", MenuLabel = "Manage" };
        trigger.Items.Add(new CdsHeaderMenuItem("Users", isActive: true));
        MenuTriggerHost.Content = new VerticalStackLayout
        {
            Spacing = 8,
            Children =
            {
                new CdsText("Closed with a current child", CdsTypeStyle.Label01, CdsTextTone.Secondary),
                new CdsHeader { Label = "Closed trigger", WidthRequest = 220, Items = { trigger } },
            },
        };
    }

    void OnNavClicked(object? sender, EventArgs e)
    {
        if (sender is not CdsSideNavRow clicked) return;
        foreach (var item in _navItems) item.IsActive = ReferenceEquals(item, clicked);
        BodyTitle.Text = clicked.Text;
        BodyText.Text = $"The body swaps when {clicked.Text} is chosen; navigation is yours (Clicked or Command).";
        Status.Text = $"Current page: {clicked.Text}";
    }

    void OnSwitcherClicked(object? sender, EventArgs e)
    {
        MainPanel.IsExpanded = !MainPanel.IsExpanded;
        SwitcherAction.IsActive = MainPanel.IsExpanded;
    }

    void OnHeaderNameClicked(object? sender, EventArgs e) => Status.Text = "Header name activated (go home)";

    void OnToggleNavClicked(object? sender, EventArgs e)
    {
        if (MainNav.IsFixed) return;
        MainNav.IsExpanded = !MainNav.IsExpanded;
        Status.Text = MainNav.IsExpanded ? "Side nav expanded" : "Side nav collapsed";
    }

    void OnOpenMenuClicked(object? sender, EventArgs e) => ManageMenu.IsExpanded = !ManageMenu.IsExpanded;
}

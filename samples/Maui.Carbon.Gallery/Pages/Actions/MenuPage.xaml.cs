using Maui.Carbon.Gallery.Infrastructure;
using Maui.Carbon.Gallery.Pages.Actions.Data;

namespace Maui.Carbon.Gallery.Pages.Actions;

[GalleryPage("Menu", GalleryCategory.Actions, Order = 50, Spec = "Menu",
    Summary = "A floating list of actions: items, dividers, groups, selectable items, radio groups and nested submenus.")]
public partial class MenuPage : GalleryPage
{
    readonly CdsMenu _actions = ActionsMenuSamples.Create(ActionsMenuSample.Actions, CdsSize.Md);
    readonly CdsMenu _withSubmenu = ActionsMenuSamples.Create(ActionsMenuSample.Submenu, CdsSize.Md);

    public MenuPage()
    {
        InitializeComponent();

        // The submenu surface drawn next to its parent: the same builder the overlay uses.
        var share = (CdsMenuItem)ActionsMenuSamples.Create(ActionsMenuSample.Submenu).Items[1];
        var child = new CdsMenu { Label = "Share", Size = CdsSize.Sm };
        foreach (var item in share.Items.ToArray())
        {
            share.Items.Remove(item);
            child.Items.Add(item);
        }
        SubmenuHost.Content = child.CreatePreview(0);
        SubmenuHost.VerticalOptions = LayoutOptions.Start;
        SubmenuHost.Margin = new Thickness(-14, 40, 0, 0);

        foreach (var menu in new[] { _actions, _withSubmenu })
        {
            WireItems(menu.Items);
        }
    }

    void WireItems(IList<CdsMenuItemBase> items)
    {
        foreach (var item in items)
        {
            if (item is CdsMenuItem menuItem)
            {
                menuItem.Clicked += (_, _) => Status.Text = $"Chose \"{menuItem.Text}\"";
                WireItems(menuItem.Items);
            }
        }
    }

    void OnOpenClicked(object? sender, EventArgs e) => _actions.Open(OpenButton, CdsPlacement.BottomStart);

    void OnOpenSubmenuClicked(object? sender, EventArgs e) => _withSubmenu.Open(OpenSubmenuButton, CdsPlacement.BottomStart);
}

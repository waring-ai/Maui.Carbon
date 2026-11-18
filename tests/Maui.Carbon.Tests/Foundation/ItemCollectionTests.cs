namespace Maui.Carbon.Tests.Foundation;

/// <summary>Clearing a control's children takes them out of the control, as removing them one by one does.</summary>
public class ItemCollectionTests
{
    [Fact]
    public void Clear_raises_one_remove_that_lists_every_item()
    {
        var items = new CdsItemCollection<string> { "a", "b" };
        var events = new List<System.Collections.Specialized.NotifyCollectionChangedEventArgs>();
        items.CollectionChanged += (_, e) => events.Add(e);
        items.Clear();
        Assert.Empty(items);
        var e = Assert.Single(events);
        Assert.Equal(System.Collections.Specialized.NotifyCollectionChangedAction.Remove, e.Action);
        Assert.Equal(0, e.OldStartingIndex);
        Assert.Equal(["a", "b"], e.OldItems!.Cast<string>());
        items.Clear();
        Assert.Single(events);
    }

    [Fact]
    public void Clearing_a_switcher_removes_its_items()
    {
        var switcher = new CdsSwitcher();
        var item = new CdsSwitcherItem { Text = "Atlas" };
        switcher.Items.Add(item);
        Assert.NotNull(item.Parent);
        switcher.Items.Clear();
        Assert.Null(item.Parent);
    }

    [Fact]
    public void Clearing_header_navigation_and_tabs_removes_their_children()
    {
        var nav = new CdsHeaderNavigation();
        var link = new CdsHeaderMenuItem { Text = "Docs" };
        nav.Items.Add(link);
        nav.Items.Clear();
        Assert.Null(link.Parent);

        var buttons = new CdsButtonSet();
        var button = new CdsButton { Text = "Save" };
        buttons.Items.Add(button);
        buttons.Items.Clear();
        Assert.Null(button.Parent);
    }
}

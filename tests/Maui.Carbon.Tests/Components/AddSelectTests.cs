namespace Maui.Carbon.Tests.Components;

public class AddSelectTests
{
    static CdsAddSelectItem Item(string id, string title, string? role = null, params CdsAddSelectItem[] children)
    {
        var item = new CdsAddSelectItem { Id = id, Title = title, Value = id };
        if (role is not null) item.Attributes["role"] = role;
        foreach (var c in children) item.Children.Add(c);
        return item;
    }

    static List<CdsAddSelectItem> Tree() =>
    [
        Item("eu", "Europe", null, Item("fra", "Frankfurt", "Admin"), Item("lon", "London", "Viewer")),
        Item("us", "United States", null, Item("dal", "Dallas", "Admin")),
        Item("tok", "Tokyo", "Editor"),
    ];

    [Fact]
    public void Flatten_and_options()
    {
        var tree = Tree();
        Assert.Equal(6, AddSelectModel.Flatten(tree).Count());
        Assert.True(AddSelectModel.IsHierarchical(tree));
        Assert.Equal(["Admin", "Viewer", "Editor"], AddSelectModel.FilterOptions(tree, "role"));
        Assert.Equal("Frankfurt", AddSelectModel.Attribute(tree[0].Children[0], "title"));
    }

    [Fact]
    public void Visible_follows_path_search_filters_and_sort()
    {
        var tree = Tree();
        var none = new Dictionary<string, string>();
        Assert.Equal(["Europe", "United States", "Tokyo"], AddSelectModel.Visible(tree, [], null, none, null, false).Select(i => i.Title));
        Assert.Equal(["Frankfurt", "London"], AddSelectModel.Visible(tree, [tree[0]], null, none, null, false).Select(i => i.Title));
        Assert.Equal(["London"], AddSelectModel.Visible(tree, [], "lon", none, null, false).Select(i => i.Title));
        var admins = new Dictionary<string, string> { ["role"] = "Admin" };
        Assert.Equal(["Dallas", "Frankfurt"], AddSelectModel.Visible(tree, [], null, admins, "title", false).Select(i => i.Title));
        Assert.Equal(["Frankfurt", "Dallas"], AddSelectModel.Visible(tree, [], null, admins, "title", true).Select(i => i.Title));
    }

    [Fact]
    public void Single_add_select_submits_one_item()
    {
        var page = new VerticalStackLayout();
        var host = new CdsOverlayHost { Body = page };
        var select = new CdsSingleAddSelect { Title = "Add a region" };
        foreach (var i in Tree()) select.Items.Add(i);
        page.Add(select);
        select.IsOpen = true;
        select.Show();
        Assert.False(select.SubmitButton.IsEnabled);
        var body = select.AddSelect;
        body.Navigate(body.Items[0]);
        Assert.Equal(["Frankfurt", "London"], body.VisibleItems.Select(i => i.Title));
        body.Toggle(body.VisibleItems[1]);
        Assert.True(select.SubmitButton.IsEnabled);
        CdsAddSelectSubmittedEventArgs? submitted = null;
        select.Submitted += (_, e) => submitted = e;
        select.Submit();
        Assert.Equal("lon", Assert.Single(submitted!.Items).Id);
        Assert.False(select.IsOpen);
        Assert.Empty(host.Overlays);
    }

    [Fact]
    public void Multi_add_select_collects_items_and_modifiers()
    {
        var page = new VerticalStackLayout();
        _ = new CdsOverlayHost { Body = page };
        var modifier = new CdsAddSelectModifier { Id = "role", Title = "Role" };
        modifier.Options.Add("Viewer");
        modifier.Options.Add("Editor");
        var select = new CdsMultiAddSelect { Title = "Add members", Modifier = modifier };
        foreach (var i in Tree()) select.Items.Add(i);
        page.Add(select);
        select.IsOpen = true;
        select.Show();
        var body = select.AddSelect;
        var tokyo = body.Items[2];
        body.Toggle(tokyo);
        body.Toggle(body.Items[0]);
        body.SetModifier(tokyo, "Editor");
        CdsAddSelectSubmittedEventArgs? submitted = null;
        select.Submitted += (_, e) => submitted = e;
        select.Submit();
        Assert.Equal(["tok", "eu"], submitted!.Items.Select(i => i.Id));
        Assert.Equal("Editor", submitted.Modifiers["tok"]);
    }
}

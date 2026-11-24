namespace Maui.Carbon.Tests.Components;

public class SelectTests
{
    static CdsSelect Regions()
    {
        var select = new CdsSelect { Label = "Region" };
        select.Items.Add(new CdsSelectItem { Value = "us-south", Text = "Dallas (us-south)" });
        select.Items.Add(new CdsSelectItem { Value = "us-east", Text = "Washington DC (us-east)", IsDisabled = true });
        select.Items.Add(new CdsSelectItem { Value = "hidden", Text = "Hidden", IsHidden = true });
        select.Items.Add(new CdsSelectItemGroup
        {
            Label = "Europe",
            Items = { new CdsSelectItem { Value = "eu-de", Text = "Frankfurt (eu-de)" }, new CdsSelectItem { Text = "London" } },
        });
        return select;
    }

    [Fact]
    public void FlattenSkipsHiddenAndDisabledAndFlattensGroups()
    {
        var select = Regions();
        var rows = SelectModel.Flatten(select.Items);
        Assert.Equal(["Dallas (us-south)", "Frankfurt (eu-de)", "London"], rows.Select(r => r.Text));
        Assert.Equal("eu-de", rows[1].Value);
        Assert.Equal("London", rows[2].Value); // no value: the text stands in
    }

    [Fact]
    public void DisabledGroupLeavesItsItemsOut()
    {
        var group = new CdsSelectItemGroup { IsDisabled = true, Items = { new CdsSelectItem { Text = "A" } } };
        Assert.Empty(SelectModel.Flatten([group]));
    }

    [Fact]
    public void BuildsRowsFromASourceWithItemToString()
    {
        var rows = SelectModel.FromSource(new[] { 1, 2, 3 }, o => $"No. {o}");
        Assert.Equal(["No. 1", "No. 2", "No. 3"], rows.Select(r => r.Text));
        Assert.Empty(SelectModel.FromSource(null, null));
        Assert.Equal("5", SelectModel.FromSource(new object[] { 5 }, null)[0].Text);
    }

    [Fact]
    public void FindsIndexesAndValues()
    {
        var rows = SelectModel.FromSource(new[] { "a", "b" }, null);
        Assert.Equal(1, SelectModel.IndexOf(rows, "b"));
        Assert.Equal(-1, SelectModel.IndexOf(rows, "z"));
        Assert.Equal(-1, SelectModel.IndexOf(rows, null));
        Assert.Equal("a", SelectModel.ValueAt(rows, 0));
        Assert.Null(SelectModel.ValueAt(rows, 5));
        Assert.Null(SelectModel.ValueAt(rows, -1));
    }

    [Fact]
    public void SelectedItemAndIndexStayInSync()
    {
        var select = Regions();
        Assert.Equal(-1, select.SelectedIndex);
        select.SelectedItem = "eu-de";
        Assert.Equal(1, select.SelectedIndex);
        select.SelectedIndex = 2;
        Assert.Equal("London", select.SelectedItem);
        select.SelectedIndex = -1;
        Assert.Null(select.SelectedItem);
    }

    [Fact]
    public void ItemsSourceSelectionUsesTheDataItems()
    {
        var items = new System.Collections.ObjectModel.ObservableCollection<string> { "small", "medium" };
        var changes = 0;
        var select = new CdsSelect { ItemsSource = items, SelectedItem = "medium" };
        select.SelectedItemChanged += (_, _) => changes++;
        Assert.Equal(1, select.SelectedIndex);
        items.Add("large");
        select.SelectedIndex = 2;
        Assert.Equal("large", select.SelectedItem);
        Assert.Equal(1, changes);
    }

    [Fact]
    public void SelectionSetBeforeTheItemsArriveSurvives()
    {
        var select = new CdsSelect { SelectedItem = "b" };
        select.ItemsSource = new[] { "a", "b" };
        Assert.Equal(1, select.SelectedIndex);
        Assert.Equal("b", select.SelectedItem);
    }

    [Fact]
    public void ControlSetsEveryPropertyWithoutThrowing()
    {
        var select = Regions();
        select.HideLabel = true;
        select.HelperText = "Data stays here";
        select.IsInvalid = true;
        select.InvalidText = "Choose one";
        select.IsWarning = true;
        select.WarnText = "Costs more";
        select.IsReadOnly = true;
        select.Size = CdsSize.Sm;
        select.IsInline = true;
        select.Placeholder = "Choose a region";
        select.ItemToString = o => o.ToString()!;
        select.IsEnabled = false;
        Assert.Equal(CdsSize.Sm, select.Size);
    }

    [Fact]
    public void The_skeleton_has_a_label_bar_that_can_be_hidden()
    {
        // CA-34: Carbon's SelectSkeleton (hideLabel).
        var skeleton = new CdsSelectSkeleton();
        Assert.False(skeleton.HideLabel);
        Assert.False(AutomationProperties.GetIsInAccessibleTree(skeleton) ?? true);
        skeleton.HideLabel = true;
        Assert.True(skeleton.HideLabel);
    }
}

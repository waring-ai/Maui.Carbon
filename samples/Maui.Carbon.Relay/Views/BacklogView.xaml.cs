using System.ComponentModel;
using Maui.Carbon.Relay.Logic;
using Maui.Carbon.Relay.Presentation;
using Maui.Carbon.Relay.ViewModels;

namespace Maui.Carbon.Relay.Views;

/// <summary>
/// Backlog and sprints. Bindings carry the values; this code-behind builds the backlog tree's nodes from the view
/// model's tree (a tree view takes node objects, not an items source), keeps collapsed epics collapsed across refreshes
/// and opens an issue when a leaf is activated.
/// </summary>
public partial class BacklogView : ContentView
{
    readonly HashSet<string> _collapsed = new(StringComparer.Ordinal);
    BacklogViewModel? _vm;
    bool _building;

    public BacklogView()
    {
        InitializeComponent();
        CdsTokens.SetColor(Scroller, VisualElement.BackgroundColorProperty, CdsColorKeys.Background);
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        if (_vm is not null) _vm.PropertyChanged -= OnViewModelChanged;
        _vm = BindingContext as BacklogViewModel;
        if (_vm is null) return;
        _vm.PropertyChanged += OnViewModelChanged;
        BuildTree();
    }

    void OnViewModelChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(BacklogViewModel.Tree)) BuildTree();
    }

    static string BranchId(BacklogBranch branch) => branch.EpicKey ?? "unassigned";

    void BuildTree()
    {
        if (_vm is null) return;
        _building = true;
        try
        {
            Tree.Nodes.Clear();
            foreach (var branch in _vm.Tree)
            {
                var id = BranchId(branch);
                var node = new CdsTreeNode(branch.Label, branch.IsUnassigned ? CdsIconName.Folder : CarbonMap.Icon(Models.IssueType.Epic))
                {
                    Id = id,
                    Value = branch,
                    IsExpanded = !_collapsed.Contains(id),
                };
                foreach (var leaf in branch.Items)
                    node.Children.Add(new CdsTreeNode(leaf.Label, CarbonMap.Icon(leaf.Type)) { Id = leaf.Key, Value = leaf });
                Tree.Nodes.Add(node);
            }
        }
        finally
        {
            _building = false;
        }
    }

    void OnNodeToggled(object? sender, CdsTreeNodeEventArgs e)
    {
        if (_building || e.Node.Id is not { } id || e.Node.Value is not BacklogBranch) return;
        if (e.Node.IsExpanded) _collapsed.Remove(id);
        else _collapsed.Add(id);
    }

    void OnNodeActivated(object? sender, CdsTreeNodeEventArgs e)
    {
        if (_building) return;
        if (e.Node.Value is BacklogLeaf leaf) _vm?.OpenIssueCommand.Execute(leaf.Key);
    }

    void OnStartErrorClosed(object? sender, CdsNotificationCloseEventArgs e) => _vm?.DismissStartErrorCommand.Execute(null);
}

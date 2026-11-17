using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Data;

[GalleryPage("Tree view", GalleryCategory.Data, Order = 90, Spec = "TreeView",
    Summary = "A hierarchical list of expandable nodes for navigating nested data such as folders, resource groups or documentation sections.")]
public partial class TreeViewPage : GalleryPage
{
    public TreeViewPage()
    {
        InitializeComponent();
        Folders.Select(Folders.FindNode("m03")!);
        Multi.Select(Multi.FindNode("a2")!, Multi.FindNode("b")!);
        foreach (var tree in new[] { SizeSm, SizeXs })
        {
            tree.Nodes.Add(Sample());
            tree.Nodes.Add(new CdsTreeNode("notes.txt", CdsIconName.Document));
        }
        Strip.Template = new DataTemplate(() =>
        {
            var tree = new CdsTreeView { Label = "Folders", HideLabel = true, Size = CdsTreeViewSize.Sm };
            var root = new CdsTreeNode("2026", CdsIconName.Folder) { IsExpanded = true };
            var march = new CdsTreeNode("03 (March)", CdsIconName.Folder);
            root.Children.Add(march);
            root.Children.Add(new CdsTreeNode("02 (February)", CdsIconName.Folder));
            root.Children.Add(new CdsTreeNode("01 (archived)", CdsIconName.Folder) { IsDisabled = true });
            tree.Nodes.Add(root);
            tree.Nodes.Add(new CdsTreeNode("2025", CdsIconName.Folder));
            tree.Select(march);
            return tree;
        });
    }

    static CdsTreeNode Sample()
    {
        var root = new CdsTreeNode("2026", CdsIconName.Folder) { IsExpanded = true };
        root.Children.Add(new CdsTreeNode("03 (March)", CdsIconName.Folder));
        root.Children.Add(new CdsTreeNode("02 (February)", CdsIconName.Folder));
        return root;
    }

    void OnFolderSelected(object? sender, CdsTreeNodeEventArgs e) => FolderStatus.Text = $"Selected: {e.Node.Label}";

    void OnFolderToggled(object? sender, CdsTreeNodeEventArgs e) =>
        FolderStatus.Text = $"{(e.Node.IsExpanded ? "Expanded" : "Collapsed")}: {e.Node.Label}";

    void OnMultiSelected(object? sender, CdsTreeNodeEventArgs e) => MultiStatus.Text = $"{e.SelectedNodes.Count} folders selected";
}

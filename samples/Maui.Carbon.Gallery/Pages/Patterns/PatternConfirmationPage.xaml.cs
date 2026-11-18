using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Patterns;

[GalleryPage("Confirmation pattern", GalleryCategory.Patterns, Order = 71, Spec = "PatternConfirmation",
    Summary = "How much confirmation a destructive or lossy action needs: none with Undo for low impact, a danger modal for moderate impact, typing the resource name for high impact, and a transactional modal for discarding unsaved work.")]
public partial class PatternConfirmationPage : GalleryPage
{
    const string ClusterName = "prod-eu-01";
    const string ConfirmHelper = "The Delete cluster button turns on when the name matches.";

    static readonly string[] s_allLabels = ["env:production", "team:payments", "cost-center:4410", "tier:gold"];

    readonly List<string> _labels = [.. s_allLabels.Where(l => l != "team:payments")];
    string? _removed = "team:payments";

    public PatternConfirmationPage()
    {
        InitializeComponent();
        RebuildLabels();
        ShowUndo();
    }

    void RebuildLabels()
    {
        LabelsRow.Clear();
        foreach (var label in _labels)
        {
            var tag = new CdsDismissibleTag(label) { DismissLabel = "Remove label", Margin = new Thickness(0, 0, 8, 8) };
            tag.Dismissed += (_, _) =>
            {
                _labels.Remove(label);
                _removed = label;
                RebuildLabels();
                ShowUndo();
            };
            LabelsRow.Add(tag);
        }
    }

    void ShowUndo()
    {
        UndoNotice.IsVisible = _removed is not null;
        UndoNotice.Subtitle = _removed is null ? null : $"{_removed} was removed from {ClusterName}.";
    }

    void OnUndoClicked(object? sender, EventArgs e)
    {
        if (_removed is null) return;
        var restored = _removed;
        _removed = null;
        // Put the label back where it was in the full list.
        var index = s_allLabels.TakeWhile(l => l != restored).Count(l => _labels.Contains(l));
        _labels.Insert(index, restored);
        RebuildLabels();
        ShowUndo();
        Status.Text = $"Restored label {restored}";
    }

    void OnUndoClosed(object? sender, CdsNotificationCloseEventArgs e) => _removed = null;

    void OnSnapshotsClicked(object? sender, EventArgs e) => SnapshotsModal.IsOpen = true;

    void OnSnapshotsDelete(object? sender, EventArgs e)
    {
        SnapshotsModal.IsOpen = false;
        Status.Text = "Deleted 2 snapshots from prod-eu-01";
    }

    void OnSnapshotsClose(object? sender, CdsModalRequestCloseEventArgs e) => Status.Text = "Delete snapshots canceled";

    void OnClusterClicked(object? sender, EventArgs e)
    {
        ClusterConfirmInput.Text = "";
        ClusterModal.PrimaryButtonDisabled = true;
        ClusterModal.IsOpen = true;
    }

    void OnClusterConfirmChanged(object? sender, TextChangedEventArgs e)
    {
        var ok = e.NewTextValue == ClusterName;
        ClusterModal.PrimaryButtonDisabled = !ok;
        ClusterConfirmInput.HelperText = ok ? "Name matches." : ConfirmHelper;
    }

    void OnClusterDelete(object? sender, EventArgs e)
    {
        ClusterModal.IsOpen = false;
        CdsToast.Show(this, new CdsToastOptions
        {
            Kind = CdsNotificationKind.Success,
            Title = "Cluster deleted",
            Subtitle = $"{ClusterName} was deleted.",
            Timeout = 5000,
        });
        Status.Text = $"Deleted cluster {ClusterName}";
    }

    void OnClusterClose(object? sender, CdsModalRequestCloseEventArgs e) => Status.Text = "Delete cluster canceled";

    void OnInlineConfirmChanged(object? sender, TextChangedEventArgs e)
    {
        var ok = e.NewTextValue == ClusterName;
        InlineConfirmFooter.PrimaryButtonDisabled = !ok;
        InlineConfirmInput.HelperText = ok ? "Name matches." : ConfirmHelper;
    }

    void OnDiscardClicked(object? sender, EventArgs e) => DiscardModal.IsOpen = true;

    void OnKeepEditing(object? sender, EventArgs e)
    {
        DiscardModal.IsOpen = false;
        Status.Text = "Kept editing";
    }

    void OnDiscard(object? sender, EventArgs e)
    {
        DiscardModal.IsOpen = false;
        Status.Text = "Changes discarded";
    }
}

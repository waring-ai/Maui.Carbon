using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Overlays;

[GalleryPage("Dialog", GalleryCategory.Overlays, Order = 45,
    Summary = "A dialog that is either modal over a scrim or a non-modal panel that leaves the page usable.")]
public partial class DialogPage : GalleryPage
{
    public DialogPage()
    {
        InitializeComponent();
    }

    void OnDeleteClicked(object? sender, EventArgs e) => DeleteDialog.IsOpen = true;

    void OnRenameClicked(object? sender, EventArgs e) => RenameDialog.IsOpen = true;

    void OnAckClicked(object? sender, EventArgs e) => AckDialog.IsOpen = true;

    void OnPassiveClicked(object? sender, EventArgs e) => PassiveDialog.IsOpen = true;

    void OnFindClicked(object? sender, EventArgs e) => FindDialog.IsOpen = !FindDialog.IsOpen;

    void OnHelpClicked(object? sender, EventArgs e) => HelpDialog.IsOpen = !HelpDialog.IsOpen;

    void OnXsClicked(object? sender, EventArgs e) => XsDialog.IsOpen = true;

    void OnSmClicked(object? sender, EventArgs e) => SmDialog.IsOpen = true;

    void OnMdClicked(object? sender, EventArgs e) => MdDialog.IsOpen = true;

    void OnLgClicked(object? sender, EventArgs e) => LgDialog.IsOpen = true;

    void OnDeleteSubmit(object? sender, EventArgs e)
    {
        DeleteDialog.IsOpen = false;
        Status.Text = "Bucket deleted";
    }

    void OnRenameSubmit(object? sender, EventArgs e)
    {
        RenameDialog.IsOpen = false;
        Status.Text = "Cluster renamed";
    }

    void OnAckSubmit(object? sender, EventArgs e)
    {
        AckDialog.IsOpen = false;
        Status.Text = "Signed in again";
    }
}

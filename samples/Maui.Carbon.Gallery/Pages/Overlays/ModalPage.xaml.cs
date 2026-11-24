using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Overlays;

[GalleryPage("Modal", GalleryCategory.Overlays, Order = 40,
    Summary = "A dialog over a scrim that blocks the page until the user responds.")]
public partial class ModalPage : GalleryPage
{
    public ModalPage()
    {
        InitializeComponent();
    }

    void OnCreateClicked(object? sender, EventArgs e) => OpenOnLoadModal.IsOpen = true;

    void OnThreeClicked(object? sender, EventArgs e) => ThreeButtonModal.IsOpen = true;

    void OnScrollingClicked(object? sender, EventArgs e) => ScrollingModal.IsOpen = true;

    void OnDangerClicked(object? sender, EventArgs e) => DangerModal.IsOpen = true;

    void OnPassiveClicked(object? sender, EventArgs e) => PassiveModal.IsOpen = true;

    void OnXsClicked(object? sender, EventArgs e) => XsModal.IsOpen = true;

    void OnSmClicked(object? sender, EventArgs e) => SmModal.IsOpen = true;

    void OnMdClicked(object? sender, EventArgs e) => MdModal.IsOpen = true;

    void OnLgClicked(object? sender, EventArgs e) => LgModal.IsOpen = true;

    void OnComposedClicked(object? sender, EventArgs e) => ComposedModal.IsOpen = true;

    void OnOpenOnLoadSubmit(object? sender, EventArgs e)
    {
        // Validate before closing: keep the modal open and show the error on the field.
        var name = KeyNameInput.Text ?? "";
        if (name.Length == 0 || name.Any(char.IsUpper) || name.Contains(' '))
        {
            KeyNameInput.IsInvalid = true;
            KeyNameInput.InvalidText = "Use lowercase letters, numbers and dashes.";
            return;
        }
        KeyNameInput.IsInvalid = false;
        OpenOnLoadModal.IsOpen = false;
        Status.Text = $"Created key {name}";
    }

    void OnDiscard(object? sender, EventArgs e)
    {
        ThreeButtonModal.IsOpen = false;
        Status.Text = "Changes discarded";
    }

    void OnSaveDraft(object? sender, EventArgs e)
    {
        ThreeButtonModal.IsOpen = false;
        Status.Text = "Draft saved";
    }

    async void OnDeleteBucket(object? sender, EventArgs e)
    {
        // A short wait after submit: the primary shows an inline loading and the footer is disabled until it settles.
        DangerModal.LoadingDescription = "Deleting bucket...";
        DangerModal.LoadingStatus = CdsInlineLoadingStatus.Active;
        await Task.Delay(1500);
        DangerModal.LoadingDescription = "Bucket deleted";
        DangerModal.LoadingStatus = CdsInlineLoadingStatus.Finished;
    }

    void OnBucketDeleted(object? sender, EventArgs e)
    {
        DangerModal.IsOpen = false;
        DangerModal.LoadingStatus = CdsInlineLoadingStatus.Inactive;
        Status.Text = "Bucket deleted";
    }

    void OnComposedSubmit(object? sender, EventArgs e)
    {
        ComposedModal.IsOpen = false;
        Status.Text = "Cluster creation started";
    }

    void OnComposedRequestClose(object? sender, CdsModalRequestCloseEventArgs e) => Status.Text = "Composed modal closed";
}

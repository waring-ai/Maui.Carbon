using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Products;

[GalleryPage("Saving", GalleryCategory.Products, Order = 110, Spec = "Saving",
    Summary = "Tells people whether their work is saved: a short status line for auto-save, or a pair of buttons whose save button shows the status for manual save.")]
public partial class SavingPage : GalleryPage
{
    CancellationTokenSource? _autoSave;
    CancellationTokenSource? _manualSave;
    bool _manualFailsNext;

    public SavingPage()
    {
        InitializeComponent();
    }

    async void OnNotesChanged(object? sender, TextChangedEventArgs e)
    {
        _autoSave?.Cancel();
        var cts = _autoSave = new CancellationTokenSource();
        try
        {
            await Task.Delay(1000, cts.Token);
            AutoSaving.Status = CdsSavingStatus.InProgress;
            await Task.Delay(1000, cts.Token);
            AutoSaving.Status = Succeeds.IsToggled ? CdsSavingStatus.Success : CdsSavingStatus.Fail;
        }
        catch (TaskCanceledException)
        {
        }
    }

    async void OnRequestSave(object? sender, EventArgs e)
    {
        _manualSave?.Cancel();
        var cts = _manualSave = new CancellationTokenSource();
        ManualSaving.Status = CdsSavingStatus.InProgress;
        Status.Text = "Saving deployment settings...";
        try
        {
            await Task.Delay(2000, cts.Token);
            // Alternate outcomes so both are easy to see.
            var fails = _manualFailsNext;
            _manualFailsNext = !_manualFailsNext;
            ManualSaving.Status = fails ? CdsSavingStatus.Fail : CdsSavingStatus.Success;
            Status.Text = fails ? "The save failed; try again" : "Deployment settings saved";
        }
        catch (TaskCanceledException)
        {
        }
    }

    void OnRequestCancel(object? sender, EventArgs e)
    {
        _manualSave?.Cancel();
        ManualSaving.Status = CdsSavingStatus.Default;
        Status.Text = "Save canceled";
    }
}

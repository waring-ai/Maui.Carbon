using System.ComponentModel;
using Maui.Carbon.Relay.Models;
using Maui.Carbon.Relay.ViewModels;

namespace Maui.Carbon.Relay.Views;

/// <summary>
/// The issue screen. Bindings carry the values; this code-behind adapts what bindings can't reach: menu item clicks,
/// the tab labels and AI callout (which sit outside the visual tree), the watch item's text, and the file uploader.
/// </summary>
public partial class IssueDetailView : ContentView
{
    IssueDetailViewModel? _vm;

    public IssueDetailView()
    {
        InitializeComponent();
        CdsTokens.SetColor(Scroller, VisualElement.BackgroundColorProperty, CdsColorKeys.Background);
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        if (_vm is not null) _vm.PropertyChanged -= OnViewModelChanged;
        _vm = BindingContext as IssueDetailViewModel;
        // Tabs and the AI label's callout are not children in the visual tree, so they get the context by hand.
        foreach (var tab in Tabs.Items) tab.BindingContext = _vm;
        AiCallout.BindingContext = _vm;
        if (_vm is null) return;
        _vm.PropertyChanged += OnViewModelChanged;
        SyncWatchItem();
    }

    void OnViewModelChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(IssueDetailViewModel.WatchText):
                SyncWatchItem();
                break;
            case nameof(IssueDetailViewModel.Key):
                // A different issue: start on the first tab with an empty uploader.
                Uploader.Files.Clear();
                break;
        }
    }

    void SyncWatchItem()
    {
        if (_vm is null) return;
        WatchItem.Text = _vm.WatchText;
        WatchItem.Icon = _vm.IsWatching ? CdsIconName.ViewOff : CdsIconName.View;
    }

    // ---- header actions ----

    void OnResolveFixed(object? sender, EventArgs e) => _vm?.ResolveCommand.Execute(IssueResolution.Fixed);

    void OnResolveWontFix(object? sender, EventArgs e) => _vm?.ResolveCommand.Execute(IssueResolution.WontFix);

    void OnResolveDuplicate(object? sender, EventArgs e) => _vm?.ResolveCommand.Execute(IssueResolution.Duplicate);

    void OnResolveCannotReproduce(object? sender, EventArgs e) => _vm?.ResolveCommand.Execute(IssueResolution.CannotReproduce);

    void OnWatchClicked(object? sender, EventArgs e) => _vm?.ToggleWatchCommand.Execute(null);

    void OnMoveClicked(object? sender, EventArgs e) => _vm?.OpenMoveDialogCommand.Execute(null);

    void OnDeleteClicked(object? sender, EventArgs e) => _vm?.RequestDeleteCommand.Execute(null);

    void OnRevertAssignee(object? sender, EventArgs e) => _vm?.RevertAssigneeCommand.Execute(null);

    void OnApplySuggestionClicked(object? sender, EventArgs e)
    {
        AssigneeAiLabel.IsOpen = false;
        _vm?.ApplySuggestionCommand.Execute(null);
    }

    void OnLinkedIssueClicked(object? sender, EventArgs e)
    {
        if (sender is BindableObject { BindingContext: IssueRow row }) _vm?.OpenIssueCommand.Execute(row);
    }

    // ---- attachments ----

    /// <summary>Uploads each picked or dropped file, showing a spinner on its row until the issue has it.</summary>
    async void OnFilesAdded(object? sender, CdsFilesAddedEventArgs e)
    {
        if (_vm is null) return;
        foreach (var file in e.AddedFiles) file.Status = CdsFileStatus.Uploading;
        foreach (var file in e.AddedFiles)
        {
            try
            {
                await _vm.AddAttachmentAsync(file.Name ?? "file", SizeOf(file));
                // The attached-files list below now shows it; the uploader only shows work in flight.
                Uploader.Files.Remove(file);
            }
            catch (Exception ex)
            {
                Serilog.Log.ForContext<IssueDetailView>().Error(ex, "Uploading {File} failed", file.Name);
                file.IsInvalid = true;
                file.ErrorSubject = "Upload failed";
                file.ErrorBody = ex.Message;
                file.Status = CdsFileStatus.Edit;
            }
        }
    }

    static long SizeOf(CdsUploadedFile file)
    {
        try
        {
            if (file.Result?.FullPath is { Length: > 0 } path && File.Exists(path)) return new FileInfo(path).Length;
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
        return 48 * 1024;
    }

    void OnAttachmentDeleted(object? sender, CdsFileRemovedEventArgs e)
    {
        if (sender is BindableObject { BindingContext: AttachmentRow row }) _vm?.RemoveAttachmentCommand.Execute(row.Name);
    }
}

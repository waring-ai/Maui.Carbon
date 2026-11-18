using Maui.Carbon.Gallery.Infrastructure;
using Maui.Carbon.Gallery.Pages.Patterns.Data;

namespace Maui.Carbon.Gallery.Pages.Patterns;

[GalleryPage("Notifications pattern", GalleryCategory.Patterns, Order = 70, Spec = "PatternNotifications",
    Summary = "How to choose between inline, toast, actionable, banner, callout and modal messages, with each shown where it belongs on a cloud console page.")]
public partial class PatternNotificationsPage : GalleryPage
{
    int _exports;

    public PatternNotificationsPage()
    {
        InitializeComponent();
        BucketTable.Headers.Add(new CdsDataTableHeader { Key = "name", Header = "Name", Width = new GridLength(220), ValueSelector = r => ((K2Bucket)r).Name });
        BucketTable.Headers.Add(new CdsDataTableHeader { Key = "loc", Header = "Location", Width = GridLength.Star, ValueSelector = r => ((K2Bucket)r).Location });
        BucketTable.Headers.Add(new CdsDataTableHeader { Key = "size", Header = "Size", Width = new GridLength(100), Align = CdsTableAlignment.End, ValueSelector = r => ((K2Bucket)r).Size });
        BucketTable.RowKeySelector = r => ((K2Bucket)r).Name;
        BucketTable.ItemsSource = K2Data.Buckets;
    }

    void OnInlineClicked(object? sender, EventArgs e)
    {
        TrialInline.IsVisible = true;
        Status.Text = "Inline notification shown at the top of the form";
    }

    void OnToastClicked(object? sender, EventArgs e)
    {
        _exports++;
        CdsToast.Show(this, new CdsToastOptions
        {
            Kind = CdsNotificationKind.Success,
            Title = "Export ready",
            Subtitle = $"usage-oct-2026-{_exports}.csv is ready.",
            Caption = "Just now",
            Timeout = 5000,
        });
        Status.Text = "Success toast shown (closes after 5 s)";
    }

    void OnErrorToastClicked(object? sender, EventArgs e)
    {
        CdsToast.Show(this, new CdsToastOptions
        {
            Kind = CdsNotificationKind.Error,
            Title = "Deployment failed",
            Subtitle = "checkout-api 4.2.0 failed its health check. Roll back or retry.",
            Caption = "Just now",
        });
        Status.Text = "Error toast shown (stays until closed)";
    }

    void OnActionableClicked(object? sender, EventArgs e)
    {
        TrialUndo.IsVisible = true;
        Status.Text = "Bucket deleted; Undo stays until used or closed";
    }

    void OnUndoClicked(object? sender, EventArgs e)
    {
        TrialUndo.IsVisible = false;
        Status.Text = "Undo: bucket restored";
    }

    void OnModalClicked(object? sender, EventArgs e) => SessionModal.IsOpen = true;

    void OnSessionStay(object? sender, EventArgs e)
    {
        SessionModal.IsOpen = false;
        Status.Text = "Session extended";
    }

    void OnSessionLogOut(object? sender, CdsModalRequestCloseEventArgs e) => Status.Text = "Logged out";
}

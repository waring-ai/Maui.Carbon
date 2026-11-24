namespace Maui.Carbon.Tests.Components;

public class DialogTests
{
    [Fact]
    public void Dialog_defaults_follow_the_readme()
    {
        var dialog = new CdsDialog();
        Assert.False(dialog.IsOpen);
        Assert.False(dialog.IsModal);
        Assert.Equal(CdsModalSize.Md, dialog.Size);
        Assert.Equal("Save", dialog.PrimaryButtonText);
        Assert.Equal("Cancel", dialog.SecondaryButtonText);
        Assert.Equal("Close", dialog.CloseButtonLabel);
    }

    [Fact]
    public void Dialog_can_be_configured_and_toggled_before_loading()
    {
        var dialog = new CdsDialog
        {
            Title = "Delete bucket?",
            Subtitle = "logs-archive-prod",
            IsModal = true,
            Size = CdsModalSize.Xs,
            PrimaryButtonText = "Delete",
            IsDanger = true,
            IsPassive = false,
            HasScrollingContent = true,
            Body = new Label { Text = "All objects are deleted." },
        };
        dialog.IsOpen = true;
        dialog.IsOpen = false;
        Assert.Equal(CdsModalSize.Xs, dialog.Size);
    }

    [Fact]
    public void Single_button_footer_takes_the_end_half()
    {
        var footer = new CdsModalFooter { PrimaryButtonText = "Got it" };
        Assert.Equal(1, CdsModalSizing.FooterColumns(1).Spacer);
        Assert.True(footer.IsVisible);
    }

    [Fact]
    public void Dialog_loading_and_alert_reach_the_footer_and_the_screen_reader()
    {
        var page = new VerticalStackLayout();
        _ = new CdsOverlayHost { Body = page };
        var dialog = new CdsDialog
        {
            Title = "Delete bucket?",
            IsModal = true,
            IsDanger = true,
            PrimaryButtonText = "Delete",
            IsAlert = true,
            Body = new Label { Text = "All 1,204 objects are deleted." },
        };
        page.Add(dialog);
        dialog.IsOpen = true;
        using (var heard = CdsAnnouncer.Capture())
        {
            dialog.Show();
            var said = Assert.Single(heard.Announcements);
            Assert.Equal(CdsAnnouncePriority.Assertive, said.Priority);
            Assert.Equal("Delete bucket?. All 1,204 objects are deleted.", said.Text);
        }
        Assert.Equal("All 1,204 objects are deleted.", dialog.Surface!.Description);

        dialog.LoadingDescription = "Deleting...";
        dialog.LoadingStatus = CdsInlineLoadingStatus.Active;
        var footer = dialog.Surface.Footer!;
        Assert.Equal("Deleting...", footer.PrimaryLoading!.Description);
        Assert.All(footer.Buttons, b => Assert.False(b.IsEnabled));
        dialog.LoadingStatus = CdsInlineLoadingStatus.Inactive;
        Assert.All(footer.Buttons, b => Assert.True(b.IsEnabled));
        dialog.IsOpen = false;
    }
}

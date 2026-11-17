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
}

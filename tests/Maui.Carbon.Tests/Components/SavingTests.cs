namespace Maui.Carbon.Tests.Components;

public class SavingTests
{
    [Theory]
    [InlineData(CdsSavingStatus.Default, CdsIconName.Save, true, false, false)]
    [InlineData(CdsSavingStatus.InProgress, CdsIconName.InProgress, false, true, false)]
    [InlineData(CdsSavingStatus.Success, CdsIconName.CheckmarkOutline, true, false, false)]
    [InlineData(CdsSavingStatus.Fail, CdsIconName.ErrorOutline, true, false, true)]
    public void Status_resolves_icon_and_button_states(CdsSavingStatus status, CdsIconName icon, bool save, bool cancel, bool error)
    {
        var v = CdsSavingTokens.Resolve(status);
        Assert.Equal(icon, v.ButtonIcon);
        Assert.Equal(save, v.IsSaveEnabled);
        Assert.Equal(cancel, v.IsCancelEnabled);
        Assert.Equal(error, v.ShowsErrorIcon);
    }

    [Theory]
    [InlineData(CdsSavingStatus.Default, "Save")]
    [InlineData(CdsSavingStatus.InProgress, "Saving...")]
    [InlineData(CdsSavingStatus.Success, "Saved")]
    [InlineData(CdsSavingStatus.Fail, "Failed to save")]
    public void Status_picks_its_text(CdsSavingStatus status, string expected) =>
        Assert.Equal(expected, CdsSavingTokens.Text(status, "Save", "Saving...", "Saved", "Failed to save"));

    [Fact]
    public void Tokens_follow_the_readme()
    {
        Assert.Equal(CdsColorKeys.ButtonDangerPrimary, CdsSavingTokens.ErrorIconColor);
        Assert.Equal(CdsColorKeys.TextPrimary, CdsSavingTokens.TextColor);
        Assert.Equal(16, CdsSavingTokens.ErrorIconGap);
    }

    [Fact]
    public void Defaults_and_every_property_can_be_set_without_a_platform()
    {
        var saving = new CdsSaving();
        Assert.Equal(CdsSavingType.Auto, saving.Type);
        Assert.Equal(CdsSavingStatus.Default, saving.Status);
        saving.DefaultText = "Save";
        saving.InProgressText = "Saving...";
        saving.SuccessText = "Saved";
        saving.FailText = "Failed to save";
        saving.DefaultIconDescription = "Save";
        saving.InProgressIconDescription = "Saving";
        saving.SuccessIconDescription = "Saved";
        saving.FailIconDescription = "Error";
        saving.SecondaryButtonText = "Cancel";
        saving.SaveCommand = new Command(() => { });
        saving.SaveCommandParameter = 1;
        saving.CancelCommand = new Command(() => { });
        saving.CancelCommandParameter = 2;
        foreach (var type in Enum.GetValues<CdsSavingType>())
            foreach (var status in Enum.GetValues<CdsSavingStatus>())
            {
                saving.Type = type;
                saving.Status = status;
            }
        saving.IsEnabled = false;
    }

    [Fact]
    public void Current_text_follows_the_status()
    {
        var saving = new CdsSaving { InProgressText = "Saving...", SuccessText = "Saved", FailText = "Failed to save" };
        Assert.Null(saving.CurrentText);
        saving.Status = CdsSavingStatus.InProgress;
        Assert.Equal("Saving...", saving.CurrentText);
        Assert.False(saving.CurrentVisuals.IsSaveEnabled);
        saving.Status = CdsSavingStatus.Fail;
        Assert.Equal("Failed to save", saving.CurrentText);
        Assert.True(saving.CurrentVisuals.ShowsErrorIcon);
    }

    [Fact]
    public void Manual_buttons_follow_the_status()
    {
        var saving = new CdsSaving { Type = CdsSavingType.Manual, DefaultText = "Save", InProgressText = "Saving...", SecondaryButtonText = "Cancel" };
        var buttons = Descendants<CdsButton>(saving).ToList();
        Assert.Equal(2, buttons.Count);
        var cancel = buttons[0];
        var save = buttons[1];
        Assert.Equal("Cancel", cancel.Text);
        Assert.Equal(CdsButtonKind.Secondary, cancel.Kind);
        Assert.False(cancel.IsEnabled);
        Assert.Equal("Save", save.Text);
        Assert.Equal(CdsIconName.Save, save.Icon);
        Assert.True(save.IsEnabled);

        saving.Status = CdsSavingStatus.InProgress;
        Assert.True(cancel.IsEnabled);
        Assert.False(save.IsEnabled);
        Assert.Equal("Saving...", save.Text);
        Assert.Equal(CdsIconName.InProgress, save.Icon);
    }

    [Fact]
    public void Status_changes_are_not_announced_while_not_shown()
    {
        using var heard = CdsAnnouncer.Capture();
        var saving = new CdsSaving { SuccessText = "Saved" };
        saving.Status = CdsSavingStatus.Success;
        Assert.Empty(heard.Announcements);
    }

    static IEnumerable<T> Descendants<T>(Element root) where T : Element
    {
        foreach (var child in ((IElementController)root).LogicalChildren)
        {
            if (child is T match) yield return match;
            foreach (var d in Descendants<T>(child)) yield return d;
        }
    }
}

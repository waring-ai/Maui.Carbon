namespace Maui.Carbon.Tests.Components;

public class RemoveModalTests
{
    [Theory]
    [InlineData("bx1001", "bx1001", true)]
    [InlineData("BX1001", "bx1001", false)]
    [InlineData("bx1001 ", "bx1001", false)]
    [InlineData("", "bx1001", false)]
    [InlineData(null, "bx1001", false)]
    [InlineData(null, null, true)]
    public void Confirmation_matches_the_resource_name_exactly(string? input, string? resource, bool expected) =>
        Assert.Equal(expected, RemoveModalModel.IsConfirmed(input, resource));

    [Fact]
    public void Primary_disabled_wins_over_the_text_confirmation()
    {
        Assert.True(RemoveModalModel.IsPrimaryDisabled(true, false, null, "bx1001"));
        Assert.True(RemoveModalModel.IsPrimaryDisabled(true, true, "bx1001", "bx1001"));
        Assert.False(RemoveModalModel.IsPrimaryDisabled(false, false, null, "bx1001"));
        Assert.True(RemoveModalModel.IsPrimaryDisabled(false, true, "bx100", "bx1001"));
        Assert.False(RemoveModalModel.IsPrimaryDisabled(false, true, "bx1001", "bx1001"));
    }

    [Fact]
    public void Invalid_text_waits_until_the_input_is_left_with_a_mismatch()
    {
        Assert.False(RemoveModalModel.ShowsInvalid(true, false, "bx", "bx1001"));
        Assert.True(RemoveModalModel.ShowsInvalid(true, true, "bx", "bx1001"));
        Assert.False(RemoveModalModel.ShowsInvalid(true, true, "", "bx1001"));
        Assert.False(RemoveModalModel.ShowsInvalid(true, true, "bx1001", "bx1001"));
        Assert.False(RemoveModalModel.ShowsInvalid(false, true, "bx", "bx1001"));
    }

    [Fact]
    public void Constructs_and_takes_every_property()
    {
        var modal = new CdsRemoveModal
        {
            Title = "Confirm delete",
            Label = "Delete bx1001",
            BodyText = "Deleting bx1001 will permanently delete the configuration. This action cannot be undone.",
            IconDescription = "Close",
            ResourceName = "bx1001",
            RequiresTextConfirmation = true,
            InputLabel = "Type bx1001 to confirm",
            InputPlaceholder = "Name of resource",
            InputInvalidText = "A valid value is required",
            PrimaryButtonText = "Delete",
            SecondaryButtonText = "Cancel",
            PrimaryButtonDisabled = false,
            PrimaryDangerDescription = "danger",
            RequestSubmitCommand = new Command(() => { }),
            RequestCloseCommand = new Command(() => { }),
            LauncherElement = new Button(),
            InitialFocusElement = new Entry(),
        };
        modal.Body = new Label { Text = "Custom body" };
        modal.ConfirmationText = "bx";
        modal.IsInline = true;
        modal.IsInline = false;
        modal.IsOpen = true;
        modal.IsOpen = false;
        Assert.Equal("Confirm delete", modal.Header.Title);
        Assert.True(modal.Footer!.IsDanger);
    }

    [Fact]
    public void Typing_the_resource_name_enables_the_danger_button()
    {
        var modal = new CdsRemoveModal { ResourceName = "bx1001", RequiresTextConfirmation = true, PrimaryButtonText = "Delete", SecondaryButtonText = "Cancel" };
        Assert.True(modal.Footer!.PrimaryButtonDisabled);
        modal.ConfirmationInput.Text = "bx100";
        Assert.Equal("bx100", modal.ConfirmationText);
        Assert.True(modal.Footer.PrimaryButtonDisabled);
        modal.ConfirmationInput.Text = "bx1001";
        Assert.False(modal.Footer.PrimaryButtonDisabled);
        modal.PrimaryButtonDisabled = true;
        Assert.True(modal.Footer.PrimaryButtonDisabled);
    }

    [Fact]
    public void Without_confirmation_the_danger_button_is_enabled_and_focus_starts_on_cancel()
    {
        var modal = new CdsRemoveModal { ResourceName = "bx1001", PrimaryButtonText = "Remove", SecondaryButtonText = "Cancel" };
        Assert.False(modal.Footer!.PrimaryButtonDisabled);
        Assert.False(modal.ConfirmationInput.IsVisible);
        var cancel = modal.Footer.InitialFocus(true);
        Assert.NotNull(cancel);
    }

    [Fact]
    public void Danger_description_reaches_the_danger_button()
    {
        var modal = new CdsRemoveModal { PrimaryButtonText = "Delete", SecondaryButtonText = "Cancel", PrimaryDangerDescription = "danger" };
        var primary = (CdsModalButton)modal.Footer!.Buttons.Last();
        Assert.Equal("danger", SemanticProperties.GetHint(primary.HitTarget));
    }

    [Fact]
    public void Submit_raises_the_event_and_command_and_keeps_the_modal_open()
    {
        var page = new VerticalStackLayout();
        _ = new CdsOverlayHost { Body = page };
        var executed = 0;
        var modal = new CdsRemoveModal
        {
            Title = "Confirm removal",
            ResourceName = "bx1001",
            PrimaryButtonText = "Remove",
            SecondaryButtonText = "Cancel",
            RequestSubmitCommand = new Command(() => executed++),
        };
        var submitted = 0;
        modal.RequestSubmit += (_, _) => submitted++;
        page.Add(modal);
        modal.IsOpen = true;
        modal.Modal.Show();
        var primary = (CdsModalButton)modal.Footer!.Buttons.Last();
        ((IButton)primary.HitTarget).Clicked();
        Assert.Equal(1, submitted);
        Assert.Equal(1, executed);
        Assert.True(modal.IsOpen);
        modal.IsOpen = false;
        Assert.False(modal.Modal.IsOpen);
    }

    [Fact]
    public void Cancel_and_escape_ask_to_close_and_reopening_clears_the_confirmation()
    {
        var page = new VerticalStackLayout();
        var host = new CdsOverlayHost { Body = page };
        var modal = new CdsRemoveModal { Title = "Confirm delete", ResourceName = "bx1001", RequiresTextConfirmation = true, PrimaryButtonText = "Delete", SecondaryButtonText = "Cancel" };
        var requests = 0;
        var cancel = true;
        modal.RequestClose += (_, e) =>
        {
            requests++;
            e.Cancel = cancel;
        };
        page.Add(modal);
        modal.IsOpen = true;
        modal.Modal.Show();
        modal.ConfirmationInput.Text = "bx1001";
        host.RouteKey(new CdsKeyEventArgs(CdsKey.Escape, false));
        Assert.Equal(1, requests);
        Assert.True(modal.IsOpen);
        cancel = false;
        var secondary = (CdsModalButton)modal.Footer!.Buttons.First();
        ((IButton)secondary.HitTarget).Clicked();
        Assert.Equal(2, requests);
        Assert.False(modal.IsOpen);
        modal.IsOpen = true;
        Assert.Equal(string.Empty, modal.ConfirmationText);
        Assert.True(modal.Footer.PrimaryButtonDisabled);
        modal.IsOpen = false;
    }

    [Fact]
    public void Inline_modal_shows_its_parts_in_the_page_and_only_raises_close()
    {
        var modal = new CdsRemoveModal { Title = "Confirm delete", PrimaryButtonText = "Delete", SecondaryButtonText = "Cancel", IsInline = true };
        Assert.True(modal.InlineFrame.IsVisible);
        Assert.Same(modal.InlineFrame, modal.Header.Parent?.Parent);
        Assert.False(modal.InputTransparent);
        var requests = 0;
        modal.RequestClose += (_, _) => requests++;
        ((IButton)modal.Header.CloseTarget).Clicked();
        var secondary = (CdsModalButton)modal.Footer!.Buttons.First();
        ((IButton)secondary.HitTarget).Clicked();
        Assert.Equal(2, requests);
        modal.IsOpen = true;
        Assert.False(modal.Modal.IsOpen);
        modal.IsInline = false;
        Assert.False(modal.InlineFrame.IsVisible);
        Assert.Null(modal.Header.Parent);
        Assert.True(modal.InputTransparent);
    }
}

namespace Maui.Carbon.Tests.Components;

public class ModalTests
{
    [Theory]
    [InlineData(CdsModalSize.Md, 500, 1.0)]
    [InlineData(CdsModalSize.Md, 700, 0.84)]
    [InlineData(CdsModalSize.Md, 1100, 0.60)]
    [InlineData(CdsModalSize.Md, 1400, 0.48)]
    [InlineData(CdsModalSize.Xs, 700, 0.48)]
    [InlineData(CdsModalSize.Xs, 1100, 0.32)]
    [InlineData(CdsModalSize.Xs, 1400, 0.24)]
    [InlineData(CdsModalSize.Sm, 700, 0.60)]
    [InlineData(CdsModalSize.Sm, 1100, 0.42)]
    [InlineData(CdsModalSize.Sm, 1400, 0.36)]
    [InlineData(CdsModalSize.Lg, 700, 0.96)]
    [InlineData(CdsModalSize.Lg, 1100, 0.84)]
    [InlineData(CdsModalSize.Lg, 1400, 0.72)]
    public void Width_fraction_follows_the_breakpoints(CdsModalSize size, double viewport, double expected) =>
        Assert.Equal(expected, CdsModalSizing.WidthFraction(size, viewport), 3);

    [Fact]
    public void Breakpoints_are_inclusive_at_the_lower_edge()
    {
        Assert.Equal(0.84, CdsModalSizing.WidthFraction(CdsModalSize.Md, 672), 3);
        Assert.Equal(0.60, CdsModalSizing.WidthFraction(CdsModalSize.Md, 1056), 3);
        Assert.Equal(0.48, CdsModalSizing.WidthFraction(CdsModalSize.Md, 1312), 3);
        Assert.Equal(1.0, CdsModalSizing.WidthFraction(CdsModalSize.Md, 671.9), 3);
    }

    [Theory]
    [InlineData(CdsModalSize.Md, 500, 1.0)]
    [InlineData(CdsModalSize.Md, 800, 0.90)]
    [InlineData(CdsModalSize.Md, 1200, 0.84)]
    [InlineData(CdsModalSize.Xs, 800, 0.90)]
    [InlineData(CdsModalSize.Xs, 1200, 0.48)]
    [InlineData(CdsModalSize.Sm, 1200, 0.72)]
    [InlineData(CdsModalSize.Lg, 1200, 0.96)]
    public void Max_height_fraction(CdsModalSize size, double viewportWidth, double expected) =>
        Assert.Equal(expected, CdsModalSizing.MaxHeightFraction(size, viewportWidth), 3);

    [Fact]
    public void Modal_dimensions_are_fractions_of_the_viewport()
    {
        Assert.Equal(660, CdsModalSizing.ModalWidth(CdsModalSize.Md, 1100), 3);
        Assert.Equal(588, CdsModalSizing.ModalMaxHeight(CdsModalSize.Md, 1100, 700), 3);
    }

    [Fact]
    public void Dialog_width_is_capped_at_48rem_except_lg()
    {
        Assert.Equal(768, CdsModalSizing.DialogWidth(CdsModalSize.Md, 1700), 3);
        Assert.Equal(0.36 * 1700, CdsModalSizing.DialogWidth(CdsModalSize.Sm, 1700), 3);
        Assert.Equal(0.72 * 1700, CdsModalSizing.DialogWidth(CdsModalSize.Lg, 1700), 3);
        Assert.Equal(468, CdsModalSizing.DialogWidth(CdsModalSize.Md, 500), 3);
        Assert.Equal(0, CdsModalSizing.DialogWidth(CdsModalSize.Md, 20), 3);
    }

    [Theory]
    [InlineData(CdsModalSize.Xs, 384)]
    [InlineData(CdsModalSize.Sm, 576)]
    [InlineData(CdsModalSize.Md, 768)]
    [InlineData(CdsModalSize.Lg, 1152)]
    public void Inline_dialog_widths(CdsModalSize size, double expected) =>
        Assert.Equal(expected, CdsModalSizing.DialogInlineWidth(size));

    [Fact]
    public void Dialog_max_height_steps_with_the_viewport()
    {
        Assert.Equal(500, CdsModalSizing.DialogMaxHeight(600, 500), 3);
        Assert.Equal(450, CdsModalSizing.DialogMaxHeight(800, 500), 3);
        Assert.Equal(420, CdsModalSizing.DialogMaxHeight(1200, 500), 3);
    }

    [Fact]
    public void Heading_wraps_at_about_eighty_percent()
    {
        Assert.Equal(0, CdsModalSizing.HeadingEndPadding(300));
        Assert.Equal(0.2 * (720 - 64) - 48, CdsModalSizing.HeadingEndPadding(720), 3);
        Assert.Equal(0, CdsModalSizing.HeadingEndPadding(0));
    }

    [Fact]
    public void Paragraph_end_padding_never_goes_negative()
    {
        Assert.Equal(0, CdsModalSizing.ParagraphEndPadding(100));
        Assert.Equal(0.2 * (800 - 32) - 32, CdsModalSizing.ParagraphEndPadding(800), 3);
    }

    [Fact]
    public void Fade_shows_only_for_overflowing_or_forced_content()
    {
        Assert.False(CdsModalSizing.ShowsFade(200, 300, false));
        Assert.False(CdsModalSizing.ShowsFade(300, 300, false));
        Assert.True(CdsModalSizing.ShowsFade(301, 300, false));
        Assert.True(CdsModalSizing.ShowsFade(100, 300, true));
        Assert.False(CdsModalSizing.ShowsFade(500, 0, false));
    }

    [Theory]
    [InlineData(0, 1, 0)]
    [InlineData(1, 1, 1)]
    [InlineData(2, 0, 1)]
    [InlineData(3, 1, 1)]
    public void Footer_columns_give_one_two_and_three_buttons_their_share(int count, double spacer, double button)
    {
        var (s, b) = CdsModalSizing.FooterColumns(count);
        Assert.Equal(spacer, s);
        Assert.Equal(button, b);
    }

    [Fact]
    public void Footer_button_shares_are_half_whole_and_a_quarter()
    {
        static double Share(int n)
        {
            var (s, b) = CdsModalSizing.FooterColumns(n);
            return b / (s + b * n);
        }
        Assert.Equal(0.5, Share(1), 3);
        Assert.Equal(0.5, Share(2), 3);
        Assert.Equal(0.25, Share(3), 3);
    }

    [Fact]
    public void Close_button_background_follows_state()
    {
        Assert.Null(CdsModalTokens.CloseButtonBackground(CdsInteractionState.None));
        Assert.Equal(CdsColorKeys.LayerHover01, CdsModalTokens.CloseButtonBackground(CdsInteractionState.Hover));
        Assert.Equal(CdsColorKeys.LayerActive01, CdsModalTokens.CloseButtonBackground(CdsInteractionState.Hover | CdsInteractionState.Pressed));
        Assert.Equal(CdsColorKeys.IconPrimary, CdsModalTokens.CloseButtonIcon(CdsInteractionState.Hover));
        Assert.Equal(CdsColorKeys.IconDisabled, CdsModalTokens.CloseButtonIcon(CdsInteractionState.Disabled));
    }

    [Fact]
    public void Danger_footer_uses_the_danger_button()
    {
        Assert.Equal(CdsButtonKind.Danger, CdsModalTokens.PrimaryKind(true));
        Assert.Equal(CdsButtonKind.Primary, CdsModalTokens.PrimaryKind(false));
        Assert.Equal(CdsColorKeys.Layer01, CdsModalTokens.ContainerFill);
        Assert.Equal(CdsColorKeys.BorderSubtle01, CdsModalTokens.ContainerBorder);
    }

    [Fact]
    public void Modal_can_be_created_and_configured()
    {
        var modal = new CdsModal
        {
            Heading = "Create API key",
            Label = "Access",
            Size = CdsModalSize.Sm,
            PrimaryButtonText = "Create key",
            SecondaryButtonText = "Cancel",
            TertiaryButtonText = "Save draft",
            IsDanger = true,
            IsPassive = false,
            PreventCloseOnClickOutside = true,
            PrimaryButtonDisabled = true,
            HasScrollingContent = true,
            HasForm = true,
            IsFullWidth = true,
            Body = new Label { Text = "Body" },
        };
        Assert.False(modal.IsOpen);
        Assert.Equal("Close", modal.CloseButtonLabel);
        Assert.Equal(CdsModalSize.Sm, modal.Size);
    }

    [Fact]
    public void Setting_IsOpen_before_the_modal_is_loaded_does_not_throw()
    {
        var modal = new CdsModal { Heading = "Delete bucket" };
        modal.IsOpen = true;
        Assert.True(modal.IsOpen);
        modal.IsOpen = false;
        Assert.False(modal.IsOpen);
    }

    [Fact]
    public void Header_body_and_footer_parts_can_be_created()
    {
        var header = new CdsModalHeader { Label = "Access", Title = "Create API key", IconDescription = "Dismiss" };
        var body = new CdsModalBody { HasForm = true, HasScrollingContent = true, IsFullWidth = true, Body = new Label() };
        var footer = new CdsModalFooter { PrimaryButtonText = "Create", SecondaryButtonText = "Cancel", TertiaryButtonText = "Draft", IsDanger = true, PrimaryButtonDisabled = true };
        Assert.Equal("Dismiss", header.IconDescription);
        Assert.True(footer.IsVisible);
        Assert.NotNull(footer.Content);
        footer.PrimaryButtonText = null;
        footer.SecondaryButtonText = null;
        footer.TertiaryButtonText = null;
        Assert.False(footer.IsVisible);
        Assert.True(body.HasForm);
    }

    [Fact]
    public void Composed_modal_collects_parts()
    {
        var modal = new CdsComposedModal { Size = CdsModalSize.Lg, IsDanger = true, PreventCloseOnClickOutside = true };
        modal.Parts.Add(new CdsModalHeader { Title = "Delete" });
        modal.Parts.Add(new CdsModalBody());
        modal.Parts.Add(new CdsModalFooter { PrimaryButtonText = "Delete" });
        Assert.Equal(3, modal.Parts.Count);
        modal.IsOpen = true;
        modal.IsOpen = false;
    }

    [Fact]
    public void Footer_loading_replaces_the_primary_label_and_disables_every_button()
    {
        var footer = new CdsModalFooter { PrimaryButtonText = "Create key", SecondaryButtonText = "Cancel", TertiaryButtonText = "Save draft" };
        Assert.Equal(CdsInlineLoadingStatus.Inactive, footer.LoadingStatus);
        Assert.All(footer.Buttons, b => Assert.True(b.IsEnabled));
        Assert.Null(footer.PrimaryLoading);

        footer.LoadingDescription = "Creating key...";
        footer.LoadingIconDescription = "Creating";
        footer.LoadingStatus = CdsInlineLoadingStatus.Active;
        Assert.Equal(3, footer.Buttons.Count());
        Assert.All(footer.Buttons, b => Assert.False(b.IsEnabled));
        var loading = Assert.IsType<CdsInlineLoading>(footer.PrimaryLoading);
        Assert.Equal(CdsInlineLoadingStatus.Active, loading.Status);
        Assert.Equal("Creating key...", loading.Description);
        Assert.Equal("Creating", loading.IconDescription);
        var primary = (CdsModalButton)footer.Buttons.Last();
        Assert.Equal("Creating key...", SemanticProperties.GetDescription(primary.HitTarget));
        Assert.Null(footer.InitialFocus(false));

        footer.LoadingDescription = "Key created";
        footer.LoadingStatus = CdsInlineLoadingStatus.Finished;
        Assert.Equal(CdsInlineLoadingStatus.Finished, footer.PrimaryLoading!.Status);
        Assert.Equal("Key created", footer.PrimaryLoading.Description);

        // A rebuild (new label) keeps the wait.
        footer.PrimaryButtonText = "Create";
        Assert.NotNull(footer.PrimaryLoading);
        Assert.All(footer.Buttons, b => Assert.False(b.IsEnabled));

        footer.LoadingStatus = CdsInlineLoadingStatus.Inactive;
        Assert.Null(footer.PrimaryLoading);
        Assert.All(footer.Buttons, b => Assert.True(b.IsEnabled));
        Assert.Equal("Create", SemanticProperties.GetDescription(((CdsModalButton)footer.Buttons.Last()).HitTarget));
    }

    [Fact]
    public void Footer_loading_and_primary_disabled_combine()
    {
        var footer = new CdsModalFooter { PrimaryButtonText = "Store entry", SecondaryButtonText = "Cancel", PrimaryButtonDisabled = true };
        Assert.False(footer.Buttons.Last().IsEnabled);
        Assert.True(footer.Buttons.First().IsEnabled);
        footer.LoadingStatus = CdsInlineLoadingStatus.Error;
        Assert.All(footer.Buttons, b => Assert.False(b.IsEnabled));
        footer.LoadingStatus = CdsInlineLoadingStatus.Inactive;
        Assert.False(footer.Buttons.Last().IsEnabled);
        footer.PrimaryButtonDisabled = false;
        Assert.True(footer.Buttons.Last().IsEnabled);
    }

    [Fact]
    public void Primary_does_not_submit_while_loading()
    {
        var footer = new CdsModalFooter { PrimaryButtonText = "Create key", LoadingStatus = CdsInlineLoadingStatus.Active };
        var submitted = 0;
        footer.RequestSubmit += (_, _) => submitted++;
        var primary = (CdsModalButton)footer.Buttons.Single();
        ((IButton)primary.HitTarget).Clicked();
        Assert.Equal(0, submitted);
    }

    [Fact]
    public void Modal_passes_its_loading_state_to_the_footer()
    {
        var page = new VerticalStackLayout();
        _ = new CdsOverlayHost { Body = page };
        var modal = new CdsModal { Heading = "Create API key", PrimaryButtonText = "Create key", SecondaryButtonText = "Cancel" };
        page.Add(modal);
        modal.IsOpen = true;
        modal.Show();
        var footer = modal.Surface!.Footer!;
        Assert.Null(footer.PrimaryLoading);

        modal.LoadingDescription = "Creating key...";
        modal.LoadingStatus = CdsInlineLoadingStatus.Active;
        Assert.Equal(CdsInlineLoadingStatus.Active, footer.LoadingStatus);
        Assert.Equal("Creating key...", footer.PrimaryLoading!.Description);
        Assert.All(footer.Buttons, b => Assert.False(b.IsEnabled));
        modal.IsOpen = false;
    }

    [Fact]
    public void Alert_modal_is_described_by_its_body_and_announced_assertively()
    {
        var page = new VerticalStackLayout();
        _ = new CdsOverlayHost { Body = page };
        var body = new VerticalStackLayout
        {
            new Label { Text = "Your session expires in 2 minutes." },
            new CdsCheckbox { Label = "Remember me" },
            new Label { Text = "Hidden", IsVisible = false },
            new CdsText("Unsaved changes are lost."),
        };
        var modal = new CdsModal { Heading = "Session expiring", PrimaryButtonText = "Stay signed in", SecondaryButtonText = "Sign out", IsAlert = true, Body = body };
        page.Add(modal);
        modal.IsOpen = true;
        using (var heard = CdsAnnouncer.Capture())
        {
            modal.Show();
            var said = Assert.Single(heard.Announcements);
            Assert.Equal(CdsAnnouncePriority.Assertive, said.Priority);
            Assert.Equal("Session expiring. Your session expires in 2 minutes. Unsaved changes are lost.", said.Text);
        }
        Assert.Equal("Your session expires in 2 minutes. Unsaved changes are lost.", modal.Surface!.Description);
        modal.IsAlert = false;
        Assert.Null(modal.Surface.Description);
        modal.IsOpen = false;
    }

    [Fact]
    public void A_plain_modal_is_not_announced_as_an_alert()
    {
        var page = new VerticalStackLayout();
        _ = new CdsOverlayHost { Body = page };
        var modal = new CdsModal { Heading = "Create API key", PrimaryButtonText = "Create key", Body = new Label { Text = "Name the key." } };
        page.Add(modal);
        modal.IsOpen = true;
        using var heard = CdsAnnouncer.Capture();
        modal.Show();
        Assert.Empty(heard.Announcements);
        Assert.Null(modal.Surface!.Description);
        modal.IsOpen = false;
    }

    [Theory]
    [InlineData(false, false, true)]   // passive (no footer): a scrim tap closes
    [InlineData(false, true, false)]   // passive, preventCloseOnClickOutside
    [InlineData(true, false, false)]   // transactional: never closes on a scrim tap
    [InlineData(true, true, false)]
    public void Composed_modal_closes_on_a_scrim_tap_only_when_passive(bool hasButtons, bool prevent, bool closes)
    {
        var footer = hasButtons ? new CdsModalFooter { PrimaryButtonText = "Create", SecondaryButtonText = "Cancel" } : null;
        Assert.Equal(closes, CdsComposedModal.ClosesOnScrimTap(footer, prevent));
    }

    [Fact]
    public void Composed_modal_with_an_empty_footer_is_passive()
    {
        Assert.True(CdsComposedModal.ClosesOnScrimTap(new CdsModalFooter(), false));
        Assert.False(CdsComposedModal.ClosesOnScrimTap(new CdsModalFooter { Body = new Label { Text = "Custom" } }, false));
    }

    // ----- loadingStatus: the submit is in flight, the chrome cannot close the modal -----

    sealed class Counter : System.Windows.Input.ICommand
    {
        public int Executed;
        public event EventHandler? CanExecuteChanged { add { } remove { } }
        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) => Executed++;
    }

    static bool Escape(CdsOverlayHost host) => host.RouteKey(new CdsKeyEventArgs(CdsKey.Escape, false));

    [Theory]
    [InlineData(CdsInlineLoadingStatus.Active)]
    [InlineData(CdsInlineLoadingStatus.Finished)]
    [InlineData(CdsInlineLoadingStatus.Error)]
    public void A_loading_modal_cannot_be_closed_from_its_chrome(CdsInlineLoadingStatus status)
    {
        var page = new VerticalStackLayout();
        var host = new CdsOverlayHost { Body = page };
        var close = new Counter();
        var modal = new CdsModal { Heading = "Create API key", PrimaryButtonText = "Create key", SecondaryButtonText = "Cancel", RequestCloseCommand = close };
        var requested = 0;
        modal.RequestClose += (_, _) => requested++;
        page.Add(modal);
        modal.IsOpen = true;
        modal.Show();
        var header = modal.Surface!.Header!;
        Assert.True(header.IsCloseEnabled);
        Assert.True(header.CloseTarget.IsEnabled);

        modal.LoadingStatus = status;
        Assert.False(header.IsCloseEnabled);
        Assert.False(header.CloseTarget.IsEnabled);
        ((IButton)header.CloseTarget).Clicked();
        Assert.True(Escape(host));   // Escape is taken (the page behind gets nothing) but does nothing
        Assert.False(modal.TryClose());
        Assert.True(modal.IsOpen);
        Assert.Equal(0, requested);
        Assert.Equal(0, close.Executed);

        // The wait ends: the chrome closes it again.
        modal.LoadingStatus = CdsInlineLoadingStatus.Inactive;
        Assert.True(header.IsCloseEnabled);
        Assert.True(header.CloseTarget.IsEnabled);
        Assert.True(Escape(host));
        Assert.False(modal.IsOpen);
        Assert.Equal(1, requested);
        Assert.Equal(1, close.Executed);
    }

    [Fact]
    public void A_modal_opened_while_loading_starts_with_the_close_button_disabled()
    {
        var page = new VerticalStackLayout();
        _ = new CdsOverlayHost { Body = page };
        var modal = new CdsModal { Heading = "Create API key", PrimaryButtonText = "Create key", LoadingStatus = CdsInlineLoadingStatus.Active };
        page.Add(modal);
        modal.IsOpen = true;
        modal.Show();
        Assert.False(modal.Surface!.Header!.CloseTarget.IsEnabled);
        // Closing from code still works (LoadingSuccess handlers do it).
        modal.IsOpen = false;
        Assert.Null(modal.Surface);
    }

    [Fact]
    public void Cancelling_RequestClose_keeps_the_modal_open_and_skips_the_command()
    {
        var page = new VerticalStackLayout();
        var host = new CdsOverlayHost { Body = page };
        var close = new Counter();
        var modal = new CdsModal { Heading = "Edit entry", PrimaryButtonText = "Save", SecondaryButtonText = "Cancel", RequestCloseCommand = close };
        var cancel = true;
        modal.RequestClose += (_, e) => e.Cancel = cancel;
        page.Add(modal);
        modal.IsOpen = true;
        modal.Show();
        ((IButton)modal.Surface!.Header!.CloseTarget).Clicked();
        Escape(host);
        var secondary = (CdsModalButton)modal.Surface.Footer!.Buttons.First();
        ((IButton)secondary.HitTarget).Clicked();
        Assert.True(modal.IsOpen);
        Assert.Equal(0, close.Executed);
        cancel = false;
        ((IButton)secondary.HitTarget).Clicked();
        Assert.False(modal.IsOpen);
        Assert.Equal(1, close.Executed);
    }

    [Fact]
    public void A_composed_modal_follows_its_footer_loading_status()
    {
        var page = new VerticalStackLayout();
        var host = new CdsOverlayHost { Body = page };
        var header = new CdsModalHeader { Title = "Add task" };
        var footerClose = new Counter();
        var footer = new CdsModalFooter { PrimaryButtonText = "Add", SecondaryButtonText = "Cancel", RequestCloseCommand = footerClose };
        var footerClosed = 0;
        footer.RequestClose += (_, _) => footerClosed++;
        var modal = new CdsComposedModal();
        modal.Parts.Add(header);
        modal.Parts.Add(new CdsModalBody { Body = new Label { Text = "Describe the task." } });
        modal.Parts.Add(footer);
        var requested = 0;
        modal.RequestClose += (_, _) => requested++;
        page.Add(modal);
        modal.IsOpen = true;
        modal.Show();
        Assert.True(header.CloseTarget.IsEnabled);

        footer.LoadingStatus = CdsInlineLoadingStatus.Active;
        Assert.False(header.IsCloseEnabled);
        Assert.False(header.CloseTarget.IsEnabled);
        ((IButton)header.CloseTarget).Clicked();
        Assert.True(Escape(host));
        footer.RaiseClose();
        Assert.False(modal.TryClose());
        Assert.True(modal.IsOpen);
        Assert.Equal(0, requested);
        Assert.Equal(0, footerClosed);
        Assert.Equal(0, footerClose.Executed);

        footer.LoadingStatus = CdsInlineLoadingStatus.Inactive;
        Assert.True(header.CloseTarget.IsEnabled);
        Assert.True(Escape(host));
        Assert.False(modal.IsOpen);
        Assert.Equal(1, requested);
        // Closing unwired the parts: the header is usable again in a later open.
        Assert.True(header.IsCloseEnabled);
    }

    [Fact]
    public void A_cancelled_composed_close_does_not_run_the_footer_close_command()
    {
        var page = new VerticalStackLayout();
        _ = new CdsOverlayHost { Body = page };
        var footerClose = new Counter();
        var footer = new CdsModalFooter { PrimaryButtonText = "Save", SecondaryButtonText = "Cancel", RequestCloseCommand = footerClose };
        var footerClosed = 0;
        footer.RequestClose += (_, _) => footerClosed++;
        var modal = new CdsComposedModal();
        modal.Parts.Add(new CdsModalHeader { Title = "Edit" });
        modal.Parts.Add(footer);
        var cancel = true;
        modal.RequestClose += (_, e) => e.Cancel = cancel;
        page.Add(modal);
        modal.IsOpen = true;
        modal.Show();
        var secondary = (CdsModalButton)footer.Buttons.First();
        ((IButton)secondary.HitTarget).Clicked();
        Assert.True(modal.IsOpen);
        Assert.Equal(0, footerClosed);
        Assert.Equal(0, footerClose.Executed);

        cancel = false;
        ((IButton)secondary.HitTarget).Clicked();
        Assert.False(modal.IsOpen);
        Assert.Equal(1, footerClosed);
        Assert.Equal(1, footerClose.Executed);

        // A footer on its own still raises and runs on the secondary button.
        var lone = new CdsModalFooter { SecondaryButtonText = "Cancel", RequestCloseCommand = footerClose };
        ((IButton)((CdsModalButton)lone.Buttons.Single()).HitTarget).Clicked();
        Assert.Equal(2, footerClose.Executed);
    }

    [Fact]
    public void A_disabled_close_button_has_no_hover_fill()
    {
        Assert.Null(CdsModalTokens.CloseButtonBackground(CdsInteractionState.Disabled | CdsInteractionState.Hover));
        Assert.Null(CdsModalTokens.CloseButtonBackground(CdsInteractionState.Disabled | CdsInteractionState.Pressed));
    }
}

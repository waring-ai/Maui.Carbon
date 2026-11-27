namespace Maui.Carbon.Tests.Components;

public class InterstitialScreenTests
{
    [Theory]
    [InlineData(-1, 3, 0)]
    [InlineData(0, 3, 0)]
    [InlineData(2, 3, 2)]
    [InlineData(5, 3, 2)]
    [InlineData(4, 0, 0)]
    public void Steps_clamp(int step, int count, int expected) => Assert.Equal(expected, InterstitialScreenModel.ClampStep(step, count));

    [Fact]
    public void A_single_step_shows_only_start_without_arrow()
    {
        var footer = InterstitialScreenModel.Footer(0, 1, hasSkipLabel: true);
        Assert.Equal([CdsInterstitialScreenAction.Start], footer.Actions);
        Assert.False(footer.StartHasIcon);
        Assert.False(InterstitialScreenModel.IsMultiStep(1));
        Assert.False(InterstitialScreenModel.ShowsProgress(1, hidden: false));
    }

    [Fact]
    public void Multi_step_footer_follows_the_step()
    {
        Assert.Equal([CdsInterstitialScreenAction.Skip, CdsInterstitialScreenAction.Next], InterstitialScreenModel.Footer(0, 3, true).Actions);
        Assert.Equal([CdsInterstitialScreenAction.Skip, CdsInterstitialScreenAction.Back, CdsInterstitialScreenAction.Next], InterstitialScreenModel.Footer(1, 3, true).Actions);
        var last = InterstitialScreenModel.Footer(2, 3, true);
        Assert.Equal([CdsInterstitialScreenAction.Skip, CdsInterstitialScreenAction.Back, CdsInterstitialScreenAction.Start], last.Actions);
        Assert.True(last.StartHasIcon);
        Assert.False(InterstitialScreenModel.Footer(2, 3, true, CdsInterstitialScreenAction.Start).StartHasIcon);
        Assert.Equal([CdsInterstitialScreenAction.Next], InterstitialScreenModel.Footer(0, 3, hasSkipLabel: false).Actions);
        Assert.True(InterstitialScreenModel.ShowsProgress(3, hidden: false));
        Assert.False(InterstitialScreenModel.ShowsProgress(3, hidden: true));
    }

    [Fact]
    public void Next_shows_its_arrow_unless_running()
    {
        var footer = InterstitialScreenModel.Footer(0, 3, true);
        Assert.True(InterstitialScreenModel.HasIcon(CdsInterstitialScreenAction.Next, footer, null));
        Assert.False(InterstitialScreenModel.HasIcon(CdsInterstitialScreenAction.Next, footer, CdsInterstitialScreenAction.Next));
        Assert.False(InterstitialScreenModel.HasIcon(CdsInterstitialScreenAction.Skip, footer, null));
        Assert.False(InterstitialScreenModel.HasIcon(CdsInterstitialScreenAction.Back, footer, null));
    }

    [Theory]
    [InlineData(CdsInterstitialScreenAction.Next, 0, 3, 1, false)]
    [InlineData(CdsInterstitialScreenAction.Next, 2, 3, 2, false)]
    [InlineData(CdsInterstitialScreenAction.Back, 1, 3, 0, false)]
    [InlineData(CdsInterstitialScreenAction.Back, 0, 3, 0, false)]
    [InlineData(CdsInterstitialScreenAction.Skip, 1, 3, 1, true)]
    [InlineData(CdsInterstitialScreenAction.Start, 2, 3, 2, true)]
    [InlineData(CdsInterstitialScreenAction.Close, 0, 1, 0, true)]
    public void Navigation(CdsInterstitialScreenAction action, int step, int count, int expected, bool closes)
    {
        var (next, close) = InterstitialScreenModel.Navigate(action, step, count);
        Assert.Equal(expected, next);
        Assert.Equal(closes, close);
    }

    [Fact]
    public void Buttons_map_to_kinds_and_flags()
    {
        Assert.Equal(CdsButtonKind.Ghost, InterstitialScreenModel.KindOf(CdsInterstitialScreenAction.Skip));
        Assert.Equal(CdsButtonKind.Secondary, InterstitialScreenModel.KindOf(CdsInterstitialScreenAction.Back));
        Assert.Equal(CdsButtonKind.Primary, InterstitialScreenModel.KindOf(CdsInterstitialScreenAction.Next));
        Assert.Equal(CdsButtonKind.Primary, InterstitialScreenModel.KindOf(CdsInterstitialScreenAction.Start));
        Assert.Equal(CdsInterstitialScreenButtons.Skip, InterstitialScreenModel.ButtonOf(CdsInterstitialScreenAction.Skip));
        Assert.Equal(CdsInterstitialScreenButtons.Back, InterstitialScreenModel.ButtonOf(CdsInterstitialScreenAction.Back));
        Assert.Equal(CdsInterstitialScreenButtons.Next, InterstitialScreenModel.ButtonOf(CdsInterstitialScreenAction.Next));
        Assert.Equal(CdsInterstitialScreenButtons.Start, InterstitialScreenModel.ButtonOf(CdsInterstitialScreenAction.Start));
        Assert.Equal(CdsInterstitialScreenButtons.None, InterstitialScreenModel.ButtonOf(CdsInterstitialScreenAction.Close));
    }

    [Fact]
    public void Texts()
    {
        Assert.Equal("Step 2 of 5", InterstitialScreenModel.StepAnnouncement(1, 5));
        Assert.Equal("Step 5 of 5", InterstitialScreenModel.StepAnnouncement(9, 5));
        Assert.Equal("Step 3", InterstitialScreenModel.StepTitle(null, 2));
        Assert.Equal("Step 1", InterstitialScreenModel.StepTitle("  ", 0));
        Assert.Equal("Connect data", InterstitialScreenModel.StepTitle("Connect data", 0));
    }

    [Fact]
    public void Footer_widths_from_md_fix_the_actions_at_240()
    {
        var footer = InterstitialScreenModel.Footer(1, 3, true);   // skip, back, next
        var (lead, widths) = InterstitialScreenModel.FooterWidths(1000, footer);
        Assert.Equal(0, lead);
        Assert.Equal([520, 240, 240], widths);

        var noSkip = InterstitialScreenModel.Footer(1, 3, false);   // back, next
        (lead, widths) = InterstitialScreenModel.FooterWidths(1000, noSkip);
        Assert.Equal(520, lead);
        Assert.Equal([240, 240], widths);

        var single = InterstitialScreenModel.Footer(0, 1, true);   // start
        (lead, widths) = InterstitialScreenModel.FooterWidths(800, single);
        Assert.Equal(560, lead);
        Assert.Equal([240], widths);
    }

    [Fact]
    public void Footer_widths_below_md_share_the_row()
    {
        var footer = InterstitialScreenModel.Footer(1, 3, true);
        var (lead, widths) = InterstitialScreenModel.FooterWidths(600, footer);
        Assert.Equal(0, lead);
        Assert.Equal([200, 200, 200], widths);
        Assert.True(InterstitialScreenModel.IsCompact(671));
        Assert.False(InterstitialScreenModel.IsCompact(672));
        Assert.False(InterstitialScreenModel.IsCompact(0));
    }

    [Fact]
    public void Header_padding()
    {
        Assert.Equal(new Thickness(32, 24, 48, 0), InterstitialScreenModel.HeaderPadding(compact: false, modal: true));
        Assert.Equal(new Thickness(32, 24, 32, 0), InterstitialScreenModel.HeaderPadding(compact: false, modal: false));
        Assert.Equal(new Thickness(16, 24, 16, 0), InterstitialScreenModel.HeaderPadding(compact: true, modal: false));
        Assert.Equal(new Thickness(16, 24, 48, 0), InterstitialScreenModel.HeaderPadding(compact: true, modal: true));
    }

    [Fact]
    public void Tokens()
    {
        Assert.Equal(CdsColorKeys.Layer01, CdsInterstitialScreenTokens.HeaderBackground(true));
        Assert.Equal(CdsColorKeys.Background, CdsInterstitialScreenTokens.HeaderBackground(false));
        Assert.Equal(CdsColorKeys.Background, CdsInterstitialScreenTokens.Background);
        Assert.Equal(CdsColorKeys.BorderSubtle01, CdsInterstitialScreenTokens.Divider);
        Assert.Equal(CdsColorKeys.BorderSubtle01, CdsInterstitialScreenTokens.ContainerBorder);
        Assert.Equal(CdsColorKeys.Layer01, CdsInterstitialScreenTokens.ContainerFill);
        Assert.Equal(CdsButtonTokens.Resolve(CdsButtonKind.Ghost, CdsInteractionState.Hover),
            CdsInterstitialScreenTokens.Button(CdsInterstitialScreenAction.Skip, CdsInteractionState.Hover));
        Assert.Equal(CdsColorKeys.ButtonPrimary, CdsInterstitialScreenTokens.Button(CdsInterstitialScreenAction.Next, CdsInteractionState.None).Background);
    }

    [Fact]
    public void Defaults_and_property_setting_do_not_throw()
    {
        var screen = new CdsInterstitialScreen();
        Assert.False(screen.IsOpen);
        Assert.False(screen.IsFullScreen);
        Assert.Equal("Skip", screen.SkipButtonLabel);
        Assert.Equal("Back", screen.PreviousButtonLabel);
        Assert.Equal("Next", screen.NextButtonLabel);
        Assert.Equal("Get started", screen.StartButtonLabel);
        Assert.Equal("Close", screen.CloseIconDescription);
        screen.HeaderTitle = "Welcome to Event Streams";
        screen.HeaderSubtitle = "Three things to know";
        screen.HeaderContent = new Label { Text = "Extra" };
        screen.HideProgressIndicator = true;
        screen.CloseIconDescription = "Dismiss";
        screen.SkipButtonLabel = "";
        screen.PreviousButtonLabel = "Previous";
        screen.NextButtonLabel = "Continue";
        screen.StartButtonLabel = "Start";
        screen.FooterContent = new HorizontalStackLayout();
        screen.DisabledButtons = CdsInterstitialScreenButtons.Next | CdsInterstitialScreenButtons.Skip;
        screen.LauncherElement = new CdsButton();
        screen.ActionHandler = _ => Task.CompletedTask;
        screen.CloseCommand = new Command(() => { });
        screen.IsFullScreen = true;
        screen.IsInline = true;
        screen.CurrentStep = 4;
        Assert.Equal(0, screen.ShownStep);
        screen.Steps.Add(new CdsInterstitialScreenView { StepTitle = "One" });
        screen.Steps.Add(new CdsInterstitialScreenView { StepTitle = "Two" });
        Assert.Equal(1, screen.ShownStep);
        Assert.Equal(2, screen.StepCount);
    }

    static (CdsInterstitialScreen Screen, CdsOverlayHost Host) OpenModal(int steps, Action<CdsInterstitialScreen>? setup = null)
    {
        var page = new VerticalStackLayout();
        var host = new CdsOverlayHost { Body = page };
        var screen = new CdsInterstitialScreen { HeaderTitle = "Welcome to Event Streams", HeaderSubtitle = "Get set up in three steps" };
        for (var i = 0; i < steps; i++)
            screen.Steps.Add(new CdsInterstitialScreenView { StepTitle = $"Step {i + 1}", Content = new Label { Text = $"Body {i + 1}" } });
        setup?.Invoke(screen);
        page.Add(screen);
        screen.IsOpen = true;
        screen.Show();
        return (screen, host);
    }

    [Fact]
    public void Opening_shows_the_first_step_header_and_progress()
    {
        var (screen, host) = OpenModal(3);
        Assert.True(screen.IsShown);
        Assert.Single(host.Overlays);
        Assert.Same(screen.Steps[0], screen.Deck!.Active);
        Assert.Equal(3, screen.Progress!.Steps.Count);
        Assert.Equal("Step 2", screen.Progress.Steps[1].Label);
        Assert.Equal("Welcome to Event Streams", screen.TitleLabel!.Text);
        Assert.NotNull(screen.CloseTarget);
        Assert.NotNull(screen.ButtonFor(CdsInterstitialScreenAction.Skip));
        Assert.NotNull(screen.ButtonFor(CdsInterstitialScreenAction.Next));
        Assert.Null(screen.ButtonFor(CdsInterstitialScreenAction.Back));
        Assert.Null(screen.ButtonFor(CdsInterstitialScreenAction.Start));
        screen.IsOpen = false;
        Assert.False(screen.IsShown);
    }

    [Fact]
    public async Task Next_and_back_move_and_start_closes()
    {
        var (screen, _) = OpenModal(3);
        CdsInterstitialScreenAction? closedBy = null;
        screen.RequestClose += (_, e) => closedBy = e.Action;
        await screen.RunActionAsync(CdsInterstitialScreenAction.Next);
        Assert.Equal(1, screen.CurrentStep);
        Assert.Same(screen.Steps[1], screen.Deck!.Active);
        Assert.Equal(1, screen.Progress!.CurrentIndex);
        Assert.NotNull(screen.ButtonFor(CdsInterstitialScreenAction.Back));
        await screen.RunActionAsync(CdsInterstitialScreenAction.Back);
        Assert.Equal(0, screen.CurrentStep);
        screen.CurrentStep = 2;
        Assert.NotNull(screen.ButtonFor(CdsInterstitialScreenAction.Start));
        Assert.Null(screen.ButtonFor(CdsInterstitialScreenAction.Next));
        await screen.RunActionAsync(CdsInterstitialScreenAction.Start);
        Assert.Equal(CdsInterstitialScreenAction.Start, closedBy);
        Assert.False(screen.IsOpen);
        Assert.Equal(0, screen.CurrentStep);
    }

    [Fact]
    public async Task An_action_handler_can_stop_the_default_and_shows_loading_meanwhile()
    {
        var gate = new TaskCompletionSource();
        var (screen, _) = OpenModal(3, s => s.ActionHandler = async args =>
        {
            await gate.Task;
            args.Cancel = args.Action == CdsInterstitialScreenAction.Next;
        });
        var running = screen.RunActionAsync(CdsInterstitialScreenAction.Next);
        Assert.Equal(CdsInterstitialScreenAction.Next, screen.RunningAction);
        Assert.True(screen.ButtonFor(CdsInterstitialScreenAction.Next)!.IsLoading);
        gate.SetResult();
        await running;
        Assert.Null(screen.RunningAction);
        Assert.False(screen.ButtonFor(CdsInterstitialScreenAction.Next)!.IsLoading);
        Assert.Equal(0, screen.CurrentStep);
    }

    [Fact]
    public async Task Action_requested_can_cancel_skip()
    {
        var (screen, _) = OpenModal(3, s => s.ActionRequested += (_, e) => e.Cancel = e.Action == CdsInterstitialScreenAction.Skip);
        await screen.RunActionAsync(CdsInterstitialScreenAction.Skip);
        Assert.True(screen.IsOpen);
        await screen.RunActionAsync(CdsInterstitialScreenAction.Next);
        Assert.Equal(1, screen.CurrentStep);
    }

    [Fact]
    public void Escape_closes_the_modal_and_runs_the_command()
    {
        var parameter = (object?)null;
        var (screen, host) = OpenModal(1, s => s.CloseCommand = new Command<object>(p => parameter = p));
        Assert.True(host.RouteKey(new CdsKeyEventArgs(CdsKey.Escape)));
        Assert.False(screen.IsOpen);
        Assert.Equal(CdsInterstitialScreenAction.Close, parameter);
    }

    [Fact]
    public void Request_close_can_keep_it_open()
    {
        var (screen, _) = OpenModal(1, s => s.RequestClose += (_, e) => e.Cancel = true);
        Assert.False(screen.TryClose(CdsInterstitialScreenAction.Close));
        Assert.True(screen.IsOpen);
        screen.IsOpen = false;
    }

    [Fact]
    public void A_single_step_has_start_only_and_no_progress()
    {
        var (screen, _) = OpenModal(1);
        Assert.NotNull(screen.ButtonFor(CdsInterstitialScreenAction.Start));
        Assert.Null(screen.ButtonFor(CdsInterstitialScreenAction.Skip));
        Assert.False(screen.ButtonFor(CdsInterstitialScreenAction.Start)!.HasIcon);
        Assert.False(screen.Progress!.Parent is VisualElement { IsVisible: true });
        screen.IsOpen = false;
    }

    [Fact]
    public void Disabled_buttons_and_full_screen_without_close()
    {
        var (screen, _) = OpenModal(2, s =>
        {
            s.IsFullScreen = true;
            s.DisabledButtons = CdsInterstitialScreenButtons.Next;
        });
        Assert.Null(screen.CloseTarget);
        Assert.False(screen.ButtonFor(CdsInterstitialScreenAction.Next)!.IsEnabled);
        Assert.True(screen.ButtonFor(CdsInterstitialScreenAction.Skip)!.IsEnabled);
        screen.DisabledButtons = CdsInterstitialScreenButtons.None;
        Assert.True(screen.ButtonFor(CdsInterstitialScreenAction.Next)!.IsEnabled);
        screen.IsOpen = false;
    }

    [Fact]
    public void Closing_hands_the_steps_back_for_the_next_showing()
    {
        var (screen, _) = OpenModal(2);
        var step = screen.Steps[0];
        screen.IsOpen = false;
        screen.IsOpen = true;
        screen.Show();
        Assert.Same(screen.Deck, step.Parent);
        screen.IsOpen = false;
    }

    [Fact]
    public void Inline_draws_in_place()
    {
        var screen = new CdsInterstitialScreen { IsInline = true, IsFullScreen = true, HeaderTitle = "Welcome" };
        screen.Steps.Add(new CdsInterstitialScreenView { Content = new Label { Text = "Body" } });
        var root = new ContentPage { Content = screen };
        screen.IsOpen = true;
        screen.Show();
        Assert.True(screen.IsShown);
        Assert.True(screen.WantsEscapeKey);
        Assert.True(screen.OnKeyDown(new CdsKeyEventArgs(CdsKey.Escape)));
        Assert.False(screen.IsOpen);
        Assert.False(screen.IsShown);
        GC.KeepAlive(root);
    }
}

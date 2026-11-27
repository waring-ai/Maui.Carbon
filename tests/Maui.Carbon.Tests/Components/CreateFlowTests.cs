namespace Maui.Carbon.Tests.Components;

public class CreateFlowTests
{
    static CreateStepInfo S(bool included = true, bool intro = false) => new(included, intro);

    [Fact]
    public void Navigation_skips_left_out_steps()
    {
        CreateStepInfo[] steps = [S(), S(included: false), S(), S()];
        Assert.Equal(1, CreateFlowModel.FirstIncluded(steps));
        Assert.Equal(4, CreateFlowModel.LastIncluded(steps));
        Assert.Equal(3, CreateFlowModel.Next(steps, 1));
        Assert.Equal(1, CreateFlowModel.Previous(steps, 3));
        Assert.Equal(4, CreateFlowModel.Next(steps, 4));
        Assert.True(CreateFlowModel.IsLast(steps, 4));
        Assert.Equal(3, CreateFlowModel.Initial(steps, 2));
        Assert.Equal(1, CreateFlowModel.Initial(steps, 0));
    }

    [Fact]
    public void Intro_step_is_not_in_the_progress_indicator()
    {
        CreateStepInfo[] steps = [S(intro: true), S(), S(included: false), S()];
        Assert.Equal([2, 4], CreateFlowModel.ProgressSteps(steps));
        Assert.False(CreateFlowModel.ShowsProgress(steps, 1));
        Assert.True(CreateFlowModel.ShowsProgress(steps, 2));
        Assert.Equal(0, CreateFlowModel.ProgressIndex(steps, 2));
        Assert.Equal(1, CreateFlowModel.ProgressIndex(steps, 4));
    }

    static CdsCreateTearsheet Sheet(out CdsOverlayHost host, out CdsCreateStep[] steps)
    {
        var page = new VerticalStackLayout();
        host = new CdsOverlayHost { Body = page };
        var sheet = new CdsCreateTearsheet { Title = "Create topic", SubmitButtonText = "Create topic" };
        steps = [new() { Title = "Topic name" }, new() { Title = "Partitions" }, new() { Title = "Retention" }];
        foreach (var s in steps) sheet.Steps.Add(s);
        page.Add(sheet);
        sheet.IsOpen = true;
        sheet.Show();
        return sheet;
    }

    [Fact]
    public async Task Next_back_and_submit()
    {
        var sheet = Sheet(out var host, out var steps);
        var flow = sheet.Flow;
        Assert.Equal(1, sheet.CurrentStep);
        Assert.Equal(["Cancel", "Back", "Next"], sheet.ActionSet.Shown.Select(b => b.Text));
        Assert.False(flow.BackButton.IsEnabled);
        Assert.True(steps[0].IsVisible);
        Assert.False(steps[1].IsVisible);

        steps[0].NextAsync = () => Task.FromResult(false);
        await flow.ForwardAsync();
        Assert.Equal(1, sheet.CurrentStep);
        steps[0].NextAsync = null;
        await flow.ForwardAsync();
        Assert.Equal(2, sheet.CurrentStep);
        Assert.True(flow.BackButton.IsEnabled);
        await flow.BackAsync();
        Assert.Equal(1, sheet.CurrentStep);

        steps[1].IsIncluded = false;
        await flow.ForwardAsync();
        Assert.Equal(3, sheet.CurrentStep);
        Assert.Equal("Create topic", flow.PrimaryButton.Text);

        steps[2].IsSubmitDisabled = true;
        Assert.False(flow.PrimaryButton.IsEnabled);
        steps[2].IsSubmitDisabled = false;

        var submitted = 0;
        sheet.Submitted += (_, _) => submitted++;
        sheet.SubmitAsync = () => Task.FromResult(true);
        await flow.ForwardAsync();
        Assert.Equal(1, submitted);
        Assert.False(sheet.IsOpen);
        Assert.Empty(host.Overlays);
    }

    [Fact]
    public void Reopening_starts_at_the_initial_step()
    {
        var sheet = Sheet(out _, out _);
        sheet.IsOpen = false;
        sheet.InitialStep = 2;
        sheet.IsOpen = true;
        sheet.Show();
        Assert.Equal(2, sheet.CurrentStep);
        sheet.IsOpen = false;
    }

    [Fact]
    public void Narrow_and_side_panel_create_have_two_buttons()
    {
        var narrow = new CdsCreateTearsheetNarrow { PrimaryButtonText = "Create topic", IsSubmitEnabled = false };
        Assert.Equal(["Create topic", "Cancel"], narrow.Actions.Select(b => b.Text));
        Assert.False(narrow.Actions[0].IsEnabled);

        var panel = new CdsCreateSidePanel { PrimaryButtonText = "Create rule", SecondaryButtonText = "Discard" };
        Assert.True(panel.SlideIn);
        Assert.True(panel.HideCloseButton);
        Assert.False(panel.AnimateTitle);
        Assert.Equal(["Create rule", "Discard"], panel.Actions.Select(b => b.Text));
    }

    [Fact]
    public async Task Full_page_cancel_asks_first()
    {
        var page = new CdsCreateFullPage { Title = "Create cluster" };
        page.Steps.Add(new CdsCreateStep { Title = "Details" });
        page.Steps.Add(new CdsCreateStep { Title = "Review" });
        page.Restart();
        Assert.Equal(1, page.CurrentStep);
        await page.Flow.ForwardAsync();
        Assert.Equal(2, page.CurrentStep);
        page.Flow.CancelRequested!();
        Assert.True(page.Confirmation.IsOpen);
        var clicked = 0;
        page.InfluencerStepClicked += (_, n) => clicked = n;
        Assert.True(page.Flow.Progress.IsClickable);
        page.GoToStep(1);
        Assert.Equal(1, page.CurrentStep);
    }
}

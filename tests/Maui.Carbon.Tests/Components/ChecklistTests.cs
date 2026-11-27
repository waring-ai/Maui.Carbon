using Maui.Carbon.Tests.Foundation;

namespace Maui.Carbon.Tests.Components;

public class ChecklistTests
{
    // ----- model -----

    [Fact]
    public void Chart_needs_a_value_and_a_label_and_zero_is_a_value()
    {
        Assert.True(ChecklistModel.ShowsChart(0, "0% complete"));
        Assert.False(ChecklistModel.ShowsChart(null, "15% complete"));
        Assert.False(ChecklistModel.ShowsChart(0.15, null));
        Assert.False(ChecklistModel.ShowsChart(0.15, ""));
    }

    [Fact]
    public void Header_shows_with_a_title_or_a_chart()
    {
        Assert.True(ChecklistModel.ShowsHeader("Get started", false));
        Assert.True(ChecklistModel.ShowsHeader(null, true));
        Assert.False(ChecklistModel.ShowsHeader("", false));
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(0.15, 54)]
    [InlineData(0.5, 180)]
    [InlineData(1, 360)]
    [InlineData(1.5, 360)]
    [InlineData(-0.2, 0)]
    [InlineData(double.NaN, 0)]
    public void Sweep_is_the_clamped_fraction_of_a_turn(double value, double expected) =>
        Assert.Equal(expected, ChecklistModel.Sweep(value), 6);

    [Fact]
    public void Task_names_carry_the_status()
    {
        Assert.Equal("Connect a data source, Complete", ChecklistModel.TaskName("Connect a data source", "Complete"));
        Assert.Equal("Connect a data source", ChecklistModel.TaskName("Connect a data source", null));
        Assert.Equal("Complete", ChecklistModel.TaskName(null, "Complete"));
    }

    [Fact]
    public void Chart_is_40_with_a_4_ring()
    {
        Assert.Equal(40, ChecklistModel.ChartSize);
        Assert.Equal(4, ChecklistModel.ChartRing);
        Assert.Equal(50, ChecklistModel.HeightAt(0, 100, 0.5));
        Assert.Equal(0, ChecklistModel.HeightAt(100, 0, 2));
    }

    // ----- tokens -----

    [Theory]
    [InlineData(CdsChecklistTaskKind.Unchecked, CdsIconName.CircleDash, CdsColorKeys.IconPrimary, CdsColorKeys.TextPrimary)]
    [InlineData(CdsChecklistTaskKind.Indeterminate, CdsIconName.Incomplete, CdsColorKeys.Interactive, CdsColorKeys.TextPrimary)]
    [InlineData(CdsChecklistTaskKind.Checked, CdsIconName.CheckmarkOutline, CdsColorKeys.Interactive, CdsColorKeys.TextPrimary)]
    [InlineData(CdsChecklistTaskKind.Disabled, CdsIconName.CircleDash, CdsColorKeys.IconDisabled, CdsColorKeys.TextDisabled)]
    [InlineData(CdsChecklistTaskKind.Error, CdsIconName.Warning, CdsColorKeys.SupportError, CdsColorKeys.TextPrimary)]
    public void Each_kind_has_its_glyph_and_colors(CdsChecklistTaskKind kind, CdsIconName icon, string iconColor, string labelColor)
    {
        Assert.Equal(icon, CdsChecklistTokens.Icon(kind));
        Assert.Equal(iconColor, CdsChecklistTokens.IconColor(kind));
        Assert.Equal(labelColor, CdsChecklistTokens.LabelColor(kind));
        Assert.False(string.IsNullOrEmpty(CdsChecklistTokens.StatusText(kind)));
    }

    [Fact]
    public void Links_underline_on_hover_and_press()
    {
        Assert.Equal(new CdsChecklistLinkVisuals(CdsColorKeys.LinkPrimary, false), CdsChecklistTokens.Link(CdsInteractionState.None));
        Assert.Equal(new CdsChecklistLinkVisuals(CdsColorKeys.LinkPrimaryHover, true), CdsChecklistTokens.Link(CdsInteractionState.Hover));
        Assert.Equal(new CdsChecklistLinkVisuals(CdsColorKeys.TextPrimary, true), CdsChecklistTokens.Link(CdsInteractionState.Hover | CdsInteractionState.Pressed));
        Assert.Equal(CdsColorKeys.TextDisabled, CdsChecklistTokens.Link(CdsInteractionState.Disabled).Foreground);
    }

    [Fact]
    public void Chart_and_surface_tokens()
    {
        Assert.Equal(CdsColorKeys.LayerAccent01, CdsChecklistTokens.ChartTrack);
        Assert.Equal(CdsColorKeys.DataVis14, CdsChecklistTokens.ChartProgress);
        Assert.Equal(CdsColorKeys.Layer01, CdsChecklistTokens.Background);
        Assert.Equal(CdsColorKeys.BorderSubtle01, CdsChecklistTokens.Rule);
    }

    // ----- control -----

    static CdsChecklist Sample(out CdsChecklistTask clickable)
    {
        clickable = new CdsChecklistTask("Connect a data source", CdsChecklistTaskKind.Checked);
        clickable.Clicked += (_, _) => { };
        var list = new CdsChecklistTaskList("Set up your workspace");
        list.Tasks.Add(clickable);
        list.Tasks.Add(new CdsChecklistTask("Invite your team", CdsChecklistTaskKind.Indeterminate));
        var checklist = new CdsChecklist { Title = "Get started", ChartValue = 0.15, ChartLabel = "15% complete", ViewAllLabel = "View all (10)" };
        checklist.TaskLists.Add(list);
        return checklist;
    }

    [Fact]
    public void Every_property_can_be_set_without_a_platform()
    {
        var checklist = Sample(out _);
        checklist.ShowToggle = false;
        checklist.ToggleLabel = "Collapse";
        checklist.ToggleDescription = "Collapse the checklist";
        checklist.TogglePlacement = CdsPlacement.Bottom;
        checklist.ViewAllCommand = new Command(() => { });
        checklist.IsOpen = false;
        checklist.ChartValue = null;
        checklist.Title = null;
        checklist.ViewAllLabel = null;
        SemanticProperties.SetDescription(checklist, "Onboarding");
        checklist.TaskLists.Clear();
        checklist.IsEnabled = false;
        Assert.False(checklist.IsOpen);
    }

    [Fact]
    public void Clickable_tasks_are_links_named_with_their_status()
    {
        var checklist = Sample(out var task);
        var activations = 0;
        task.Clicked += (_, _) => activations++;
        var link = ContainerSemanticsAuditTests.Walk(checklist).OfType<ChecklistLink>().First(l => l.Text == "Connect a data source");
        Assert.Equal("Connect a data source, Complete", SemanticProperties.GetDescription(link.Hit));
        link.Hit.RaiseKeyboardActivation();
        Assert.Equal(1, activations);
        // The plain task is text with its status in its name.
        var label = ContainerSemanticsAuditTests.Walk(checklist).OfType<CdsText>().First(t => t.Text == "Invite your team");
        Assert.Equal("Invite your team, In progress", SemanticProperties.GetDescription(label));
    }

    [Fact]
    public void Changing_a_task_rebuilds_its_row()
    {
        var checklist = Sample(out _);
        var task = checklist.TaskLists[0].Tasks[1];
        task.StatusDescription = "Started";
        task.Label = "Invite two teammates";
        Assert.Contains(ContainerSemanticsAuditTests.Walk(checklist).OfType<CdsText>(), t => t.Text == "Invite two teammates" && SemanticProperties.GetDescription(t) == "Invite two teammates, Started");
        task.Command = new Command(() => { });
        Assert.Contains(ContainerSemanticsAuditTests.Walk(checklist).OfType<ChecklistLink>(), l => l.Text == "Invite two teammates");
    }

    [Fact]
    public void The_toggle_collapses_and_reports_expanded_state()
    {
        var checklist = Sample(out _);
        CdsChoiceChangedEventArgs<bool>? seen = null;
        checklist.OpenChanged += (_, e) => seen = e;
        var toggle = checklist.ToggleTarget!;
        Assert.True(toggle.IsExpanded);
        Assert.Equal("Checklist toggle", SemanticProperties.GetDescription(toggle));
        toggle.RaiseKeyboardActivation();
        Assert.False(checklist.IsOpen);
        Assert.False(seen!.NewValue);
        Assert.False(toggle.IsExpanded);

        checklist.ShowToggle = false;
        Assert.Null(checklist.ToggleTarget);
    }

    [Fact]
    public void No_title_and_no_chart_means_no_header_or_toggle()
    {
        var checklist = new CdsChecklist();
        Assert.Null(checklist.ToggleTarget);
        checklist.ChartValue = 0;
        checklist.ChartLabel = "0% complete";
        Assert.NotNull(checklist.ToggleTarget);
    }

    [Fact]
    public void View_all_raises_its_event_and_command()
    {
        var checklist = Sample(out _);
        var clicks = 0;
        var commands = 0;
        checklist.ViewAllClicked += (_, _) => clicks++;
        checklist.ViewAllCommand = new Command(() => commands++);
        var link = ContainerSemanticsAuditTests.Walk(checklist).OfType<ChecklistLink>().First(l => l.Text == "View all (10)");
        link.Hit.RaiseKeyboardActivation();
        Assert.Equal(1, clicks);
        Assert.Equal(1, commands);
    }
}

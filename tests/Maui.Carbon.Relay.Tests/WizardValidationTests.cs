namespace Maui.Carbon.Relay.Tests;

public class WizardValidationTests
{
    static readonly DateTime Today = TestWorld.Now;

    [Fact]
    public void Type_step_requires_type_and_project()
    {
        var draft = new IssueDraft();
        Assert.False(WizardValidation.CanAdvance(draft, 0, Today));
        draft.Type = IssueType.Bug;
        draft.ProjectId = "atl";
        Assert.True(WizardValidation.CanAdvance(draft, 0, Today));
    }

    [Theory]
    [InlineData("", false)]
    [InlineData("Too few", false)]
    [InlineData("Sync stalls on reconnect", true)]
    public void Details_step_validates_the_summary(string title, bool ok)
    {
        var draft = new IssueDraft { Type = IssueType.Task, ProjectId = "atl", Title = title };
        Assert.Equal(ok, WizardValidation.CanAdvance(draft, 1, Today));
    }

    [Fact]
    public void Bug_without_description_warns_but_does_not_block()
    {
        var draft = new IssueDraft { Type = IssueType.Bug, ProjectId = "atl", Title = "Sync stalls on reconnect" };
        var messages = WizardValidation.Validate(draft, 1, Today);
        Assert.Contains(messages, m => m.Field == "description" && !m.IsError);
        Assert.True(WizardValidation.CanAdvance(draft, 1, Today));
    }

    [Fact]
    public void Due_before_start_blocks_and_past_due_warns()
    {
        var draft = new IssueDraft { StartDate = Today.AddDays(5), DueDate = Today.AddDays(2) };
        Assert.False(WizardValidation.CanAdvance(draft, 2, Today));
        draft = new IssueDraft { DueDate = Today.AddDays(-1) };
        Assert.True(WizardValidation.CanAdvance(draft, 2, Today));
        Assert.Contains(WizardValidation.Validate(draft, 2, Today), m => m.Field == "dates" && !m.IsError);
    }

    [Fact]
    public void Reminder_needs_a_due_date()
    {
        var draft = new IssueDraft { ReminderTime = TimeSpan.FromHours(9) };
        Assert.False(WizardValidation.CanAdvance(draft, 2, Today));
    }

    [Fact]
    public void Review_step_runs_every_check()
    {
        var draft = new IssueDraft();
        var fields = WizardValidation.Validate(draft, 3, Today).Select(m => m.Field).ToHashSet();
        Assert.Contains("type", fields);
        Assert.Contains("title", fields);
    }
}

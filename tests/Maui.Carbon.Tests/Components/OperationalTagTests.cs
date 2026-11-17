namespace Maui.Carbon.Tests.Components;

public class OperationalTagTests
{
    [Fact]
    public void Properties_can_be_set_without_a_platform()
    {
        var tag = new CdsOperationalTag("+3", CdsTagType.Blue)
        {
            Size = CdsTagSize.Lg,
            Icon = CdsIconName.Add,
            IsExpanded = true,
            Command = new Command(() => { }),
            CommandParameter = 1,
        };
        tag.IsExpanded = null;
        tag.Type = CdsTagType.HighContrast;
        tag.IsEnabled = false;
        Assert.Equal("+3", tag.Text);
    }

    [Fact]
    public void A_command_that_cannot_execute_disables_the_tag()
    {
        var tag = new CdsOperationalTag("+3") { Command = new Command(() => { }, () => false) };
        Assert.False(tag.IsEnabled);
        Assert.True(tag.Is(CdsInteractionState.Disabled));
    }
}

public class DismissibleTagTests
{
    [Fact]
    public void Defaults_follow_the_readme()
    {
        var tag = new CdsDismissibleTag("Region: us-east", CdsTagType.Blue);
        Assert.Equal("Dismiss", tag.DismissLabel);
        Assert.Equal(CdsTagSize.Md, tag.Size);
        Assert.Equal("Region: us-east", tag.Text);
    }

    [Fact]
    public void Properties_can_be_set_without_a_platform()
    {
        var tag = new CdsDismissibleTag
        {
            Text = "Status: running",
            Type = CdsTagType.Green,
            Size = CdsTagSize.Sm,
            DismissLabel = "Remove filter",
            Icon = CdsIconName.Add,
            IsEnabled = false,
        };
        foreach (var size in Enum.GetValues<CdsTagSize>()) tag.Size = size;
        foreach (var type in Enum.GetValues<CdsTagType>()) tag.Type = type;
    }
}

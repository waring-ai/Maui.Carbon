namespace Maui.Carbon.Tests.Components;

public class FormTests
{
    [Fact]
    public void FormStacksFieldsThirtyTwoApartOrTwentyFourWhenTight()
    {
        var form = new CdsForm();
        Assert.Equal(CdsSpacing.Spacing07, form.Spacing);
        form.IsTight = true;
        Assert.Equal(CdsSpacing.Spacing06, form.Spacing);
        form.IsTight = false;
        Assert.Equal(32, form.Spacing);
    }

    [Fact]
    public void SubmitRaisesTheEventAndRunsTheCommand()
    {
        var form = new CdsForm();
        var raised = 0;
        var ran = 0;
        form.Submitted += (_, _) => raised++;
        form.SubmitCommand = new Command(() => ran++);
        form.Submit();
        Assert.Equal((1, 1), (raised, ran));
    }

    [Fact]
    public void FormItemIsATransparentColumn()
    {
        var item = new CdsFormItem();
        Assert.Equal(0, item.Spacing);
        Assert.Equal(LayoutOptions.Fill, item.HorizontalOptions);
    }

    [Fact]
    public void FormLabelSetsItsProperties()
    {
        var label = new CdsFormLabel { Text = "Cluster name", HasMargin = false, Body = new Label { Text = "?" }, For = new Entry() };
        label.IsEnabled = false;
        Assert.False(label.HasMargin);
    }

    [Fact]
    public void FormGroupDisablesAndRestoresNestedControls()
    {
        var entry = new Entry();
        var already = new Entry { IsEnabled = false };
        var input = new CdsTextInput { Label = "Name" };
        var group = new CdsFormGroup
        {
            Legend = "Location", MessageText = "Pick where the cluster runs", IsInvalid = true,
            Body = new VerticalStackLayout { entry, already, input },
        };
        group.IsEnabled = false;
        Assert.False(entry.IsEnabled);
        Assert.False(input.IsEnabled);
        group.IsEnabled = true;
        Assert.True(entry.IsEnabled);
        Assert.True(input.IsEnabled);
        Assert.False(already.IsEnabled); // it was disabled before the group touched it
    }

    [Fact]
    public void FormGroupDisablesABodyAddedWhileDisabled()
    {
        var entry = new Entry();
        var group = new CdsFormGroup { Legend = "Zones" };
        group.IsEnabled = false;
        group.Body = new VerticalStackLayout { entry };
        Assert.False(entry.IsEnabled);
        group.IsEnabled = true;
        Assert.True(entry.IsEnabled);
    }
}

namespace Maui.Carbon.Tests.Components;

public class FluidFormTests
{
    [Fact]
    public void FormPublishesTheFluidContextToItsSubtree()
    {
        var input = new CdsTextInput { Label = "Name" };
        var form = new CdsFluidForm();
        form.Items.Add(input);
        Assert.True(CdsFluidForm.GetIsFluid(input));
        Assert.True(FluidProbe.ChromeOf(input).IsFluidLayout);
    }

    [Fact]
    public void InputsOutsideAFluidFormStayDefault()
    {
        var input = new CdsTextInput { Label = "Name" };
        _ = new VerticalStackLayout { input };
        Assert.False(CdsFluidForm.GetIsFluid(input));
        Assert.False(FluidProbe.ChromeOf(input).IsFluidLayout);
    }

    [Fact]
    public void MovingAnInputOutOfTheFormRestoresTheDefaultLayout()
    {
        var input = new CdsTextInput { Label = "Name" };
        var form = new CdsFluidForm();
        form.Items.Add(input);
        var chrome = FluidProbe.ChromeOf(input);
        Assert.True(chrome.IsFluidLayout);
        form.Items.Remove(input);
        _ = new VerticalStackLayout { input };
        Assert.False(chrome.IsFluidLayout);
        chrome.Apply(CdsInteractionState.None);
        Assert.Equal(CdsSize.Md.Height(), chrome.Field.HeightRequest);
    }

    [Fact]
    public void EveryBaseInputSwitchesToFluidInsideTheForm()
    {
        var form = new CdsFluidForm();
        View[] inputs =
        [
            new CdsTextInput(), new CdsTextArea(), new CdsNumberInput(), new CdsSearch(), new CdsSelect(),
            new CdsTimePicker(), new CdsDropdown(), new CdsComboBox(), new CdsMultiSelect(), new CdsDatePicker(),
        ];
        foreach (var input in inputs) form.Items.Add(input);
        foreach (var input in inputs) Assert.True(FluidProbe.ChromeOf(input).IsFluidLayout, input.GetType().Name);
    }

    [Fact]
    public void FormSubmitRaisesTheEventAndRunsTheCommand()
    {
        var form = new CdsFluidForm();
        var raised = 0;
        var ran = 0;
        form.Submitted += (_, _) => raised++;
        form.SubmitCommand = new Command(() => ran++);
        form.Submit();
        Assert.Equal((1, 1), (raised, ran));
    }

    [Fact]
    public void FormStacksItsItemsWithoutGaps()
    {
        var form = new CdsFluidForm();
        form.Items.Add(new CdsFluidTextInput());
        form.Items.Add(new CdsFluidTextInput());
        Assert.Equal(2, form.Items.Count);
        var stack = Assert.IsType<VerticalStackLayout>(((ContentView)form.Content).Content);
        Assert.Equal(0, stack.Spacing);
    }

    [Fact]
    public void RowGivesEveryFieldAnEqualColumnWithOnePixelBetween()
    {
        var row = new CdsFluidFormRow();
        var a = new CdsFluidTextInput();
        var b = new CdsFluidSelect();
        var c = new CdsFluidNumberInput();
        row.Add(a);
        row.Add(b);
        row.Add(c);
        Assert.Equal(1, row.ColumnSpacing);
        Assert.Equal(3, row.ColumnDefinitions.Count);
        Assert.All(row.ColumnDefinitions, d => Assert.Equal(GridLength.Star, d.Width));
        Assert.Equal([0, 1, 2], new[] { Grid.GetColumn(a), Grid.GetColumn(b), Grid.GetColumn(c) });
        row.Remove(b);
        Assert.Equal(2, row.ColumnDefinitions.Count);
        Assert.Equal(1, Grid.GetColumn(c));
    }
}

public class FluidChromeTests
{
    [Fact]
    public void FluidFieldIsSixtyFourHighWhateverTheSize()
    {
        var chrome = new CdsFieldChrome { Size = CdsSize.Sm };
        Assert.Equal(32, chrome.Field.HeightRequest);
        chrome.IsFluid = true;
        Assert.True(chrome.IsFluidLayout);
        Assert.Equal(CdsFieldChrome.FluidFieldHeight, chrome.Field.HeightRequest);
        chrome.Size = CdsSize.Lg;
        Assert.Equal(64, chrome.Field.HeightRequest);
        chrome.IsFluid = false;
        chrome.Size = CdsSize.Lg;
        Assert.Equal(48, chrome.Field.HeightRequest);
        Assert.Equal(-1, chrome.Field.MinimumHeightRequest);
    }

    [Fact]
    public void MultiLineFluidFieldGrowsFromASixtyFourMinimum()
    {
        var chrome = new CdsFieldChrome { IsFluid = true, FieldHeight = -1 };
        Assert.Equal(-1, chrome.Field.HeightRequest);
        Assert.Equal(64, chrome.Field.MinimumHeightRequest);
    }

    [Fact]
    public void FluidHidesHelperTextButShowsValidationMessages()
    {
        var chrome = new CdsFieldChrome { IsFluid = true, HelperText = "Helper", InvalidText = "Bad", WarnText = "Careful" };
        chrome.Apply(CdsInteractionState.None);
        Assert.Null(chrome.CurrentMessage);
        chrome.Apply(CdsInteractionState.Invalid);
        Assert.Equal("Bad", chrome.CurrentMessage);
        chrome.Apply(CdsInteractionState.Warn);
        Assert.Equal("Careful", chrome.CurrentMessage);

        chrome.IsFluid = false;
        chrome.Apply(CdsInteractionState.None);
        Assert.Equal("Helper", chrome.CurrentMessage);
    }

    [Fact]
    public void FluidIgnoresInlineAndHideLabel()
    {
        var chrome = new CdsFieldChrome { Label = "Name", HideLabel = true, IsInline = true, IsFluid = true };
        chrome.Apply(CdsInteractionState.None);
        Assert.True(chrome.IsFluidLayout);
        Assert.Equal(64, chrome.Field.HeightRequest);
    }

    [Fact]
    public void ForcedFluidNotifiesTheOwnerOnce()
    {
        var chrome = new CdsFieldChrome();
        var raised = 0;
        chrome.FluidLayoutChanged += (_, _) => raised++;
        chrome.IsFluid = true;
        chrome.IsFluid = true;
        chrome.IsFluid = false;
        Assert.Equal(2, raised);
    }
}

internal static class FluidProbe
{
    internal static CdsFieldChrome ChromeOf(object control)
    {
        var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public;
        var field = control.GetType().GetField("Chrome", flags) ?? throw new InvalidOperationException(control.GetType().Name);
        return (CdsFieldChrome)field.GetValue(control)!;
    }
}

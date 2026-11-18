namespace Maui.Carbon.Tests.Components;

public class FluidInputsTests
{
    public static TheoryData<string> Names => new(
        "TextInput", "TextArea", "NumberInput", "Search", "Select", "TimePicker", "Dropdown", "ComboBox", "MultiSelect", "DatePicker");

    static View Create(string name) => name switch
    {
        "TextInput" => new CdsFluidTextInput { Label = "Cluster name", Placeholder = "prod", Text = "a" },
        "TextArea" => new CdsFluidTextArea { Label = "Description", Rows = 3, EnableCounter = true, MaxCount = 40 },
        "NumberInput" => new CdsFluidNumberInput { Label = "Replicas", Min = 1, Max = 10, Value = 3 },
        "Search" => new CdsFluidSearch { Label = "Search", Placeholder = "Name", Text = "x" },
        "Select" => new CdsFluidSelect { Label = "Region" },
        "TimePicker" => new CdsFluidTimePicker { Label = "Start time", Text = "2:30" },
        "Dropdown" => new CdsFluidDropdown { Label = "Region", ItemsSource = new[] { "a", "b" } },
        "ComboBox" => new CdsFluidComboBox { Label = "Zone", ItemsSource = new[] { "a", "b" } },
        "MultiSelect" => new CdsFluidMultiSelect { Label = "Events", ItemsSource = new[] { "a", "b" } },
        "DatePicker" => new CdsFluidDatePicker { Label = "Date" },
        _ => throw new ArgumentOutOfRangeException(name),
    };

    [Theory]
    [MemberData(nameof(Names))]
    public void FluidControlsConstructAndAreFluidOutsideAForm(string name)
    {
        var control = Create(name);
        Assert.True(FluidProbe.ChromeOf(control).IsFluidLayout);
        Assert.False(CdsFluidForm.GetIsFluid(control));
    }

    [Theory]
    [MemberData(nameof(Names))]
    public void FluidControlsTakeEveryFieldStateWithoutThrowing(string name)
    {
        var control = (VisualElement)Create(name);
        if (control is CdsFormField field)
        {
            field.IsInvalid = true;
            field.InvalidText = "Bad";
            field.IsInvalid = false;
            field.IsWarning = true;
            field.WarnText = "Careful";
            field.IsWarning = false;
            field.IsReadOnly = true;
            field.IsReadOnly = false;
            field.HelperText = "ignored";
            field.Size = CdsSize.Lg;
        }
        control.IsEnabled = false;
        control.IsEnabled = true;
        Assert.True(FluidProbe.ChromeOf(control).IsFluidLayout);
    }

    [Fact]
    public void FluidFieldsAreSixtyFourHigh()
    {
        foreach (var name in new[] { "TextInput", "NumberInput", "Search", "Select", "Dropdown", "ComboBox", "MultiSelect", "DatePicker" })
        {
            var chrome = FluidProbe.ChromeOf(Create(name));
            Assert.Equal(CdsFieldChrome.FluidFieldHeight, chrome.Field.HeightRequest);
        }
        Assert.Equal(-1, FluidProbe.ChromeOf(Create("TextArea")).Field.HeightRequest);
        Assert.Equal(64, FluidProbe.ChromeOf(Create("TextArea")).Field.MinimumHeightRequest);
    }

    [Fact]
    public void FluidInputsKeepTheirBaseTypes()
    {
        Assert.IsAssignableFrom<CdsTextInput>(Create("TextInput"));
        Assert.IsAssignableFrom<CdsTextArea>(Create("TextArea"));
        Assert.IsAssignableFrom<CdsNumberInput>(Create("NumberInput"));
        Assert.IsAssignableFrom<CdsSearch>(Create("Search"));
        Assert.IsAssignableFrom<CdsSelect>(Create("Select"));
        Assert.IsAssignableFrom<CdsTimePicker>(Create("TimePicker"));
        Assert.IsAssignableFrom<CdsDropdown>(Create("Dropdown"));
        Assert.IsAssignableFrom<CdsComboBox>(Create("ComboBox"));
        Assert.IsAssignableFrom<CdsMultiSelect>(Create("MultiSelect"));
        Assert.IsAssignableFrom<CdsDatePicker>(Create("DatePicker"));
    }

    [Fact]
    public void FluidRangeDatePickerAndTimePickerPartsConstruct()
    {
        var range = new CdsFluidDatePicker { Type = CdsDatePickerType.Range, Label = "Start", EndLabel = "End" };
        Assert.True(FluidProbe.ChromeOf(range).IsFluidLayout);
        var time = new CdsFluidTimePicker { Label = "Time", Is24Hour = true, IsInvalid = true };
        Assert.True(FluidProbe.ChromeOf(time).IsFluidLayout);
        time.TimeZones.Add("UTC");
        time.Is24Hour = false;
    }
}

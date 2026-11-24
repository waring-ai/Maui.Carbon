using System.Reflection;
using Microsoft.Maui.Graphics;

namespace Maui.Carbon.Tests.Components;

/// <summary>The fluid-form fixes of the design-conformance review (CA-37 to CA-49).</summary>
public class FluidConformanceTests
{
    const BindingFlags Any = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

    static T Private<T>(object owner, string name)
    {
        for (var type = owner.GetType(); type is not null; type = type.BaseType)
        {
            if (type.GetField(name, Any) is { } field) return (T)field.GetValue(owner)!;
        }
        throw new InvalidOperationException($"{owner.GetType().Name}.{name}");
    }

    // An element holds its parent weakly: keep the themed roots alive for the whole test, or a collected root takes the
    // theme with it and later token changes stop resolving.
    readonly List<ContentView> _roots = [];

    T Themed<T>(T view) where T : View
    {
        _roots.Add(new ContentView { Content = view, Resources = CdsPalettes.CreateThemeDictionary(CdsThemeName.White) });
        return view;
    }

    static Color Token(string key) => CdsPalettes.GetColor(CdsThemeName.White, key);

    // ---- CA-37: read-only fluid fields keep field-01; the picker dividers are views keyed by state ----

    [Fact]
    public void ReadOnlyFluidFieldsKeepTheFieldFillButDefaultOnesStayTransparent()
    {
        Assert.Equal(CdsColorKeys.Field01, CdsFieldChrome.FieldFillKey(fluid: true, readOnly: true, hover: false, flat: false));
        Assert.Null(CdsFieldChrome.FieldFillKey(fluid: false, readOnly: true, hover: false, flat: false));
        Assert.Equal(CdsColorKeys.FieldHover01, CdsFieldChrome.FieldFillKey(fluid: true, readOnly: false, hover: true, flat: false));

        var input = Themed(new CdsFluidTextInput { Label = "Cluster ID", IsReadOnly = true });
        Assert.Equal(Token(CdsColorKeys.Field01), FluidProbe.ChromeOf(input).Field.BackgroundColor);
        var select = Themed(new CdsFluidSelect { Label = "Region", IsReadOnly = true });
        Assert.Equal(Token(CdsColorKeys.Field01), FluidProbe.ChromeOf(select).Field.BackgroundColor);
        var dropdown = Themed(new CdsFluidDropdown { Label = "Region", IsReadOnly = true, ItemsSource = new[] { "a" } });
        Assert.Equal(Token(CdsColorKeys.Field01), FluidProbe.ChromeOf(dropdown).Field.BackgroundColor);
        var plain = Themed(new CdsTextInput { Label = "Cluster ID", IsReadOnly = true });
        Assert.Equal(Colors.Transparent, FluidProbe.ChromeOf(plain).Field.BackgroundColor);
    }

    [Fact]
    public void FluidTimePickerDrawsItsDividersAsViewsKeyedByState()
    {
        var picker = new CdsFluidTimePicker { Label = "Start time" };
        picker.TimeZones.Add("UTC");
        Themed(picker);
        var row = Private<Grid>(picker, "_fluidRow");
        Assert.Equal(0, row.ColumnSpacing);
        Assert.Equal(Token(CdsColorKeys.Field01), row.BackgroundColor);

        var (period, zone) = picker.FluidDividers;
        Assert.Contains(period, row.Children);
        Assert.Contains(zone, row.Children);
        Assert.Equal(1, period.WidthRequest);
        Assert.Equal(Token(CdsColorKeys.BorderStrong01), period.Color);

        picker.IsReadOnly = true;
        Assert.Equal(Token(CdsColorKeys.BorderSubtle01), period.Color);
        Assert.Equal(Token(CdsColorKeys.BorderSubtle01), zone.Color);

        Assert.Equal(CdsColorKeys.BorderDisabled, CdsTimePicker.DividerColorKey(disabled: true, readOnly: false));
        Assert.Equal(CdsColorKeys.BorderSubtle01, CdsTimePicker.DividerColorKey(disabled: false, readOnly: true));
        Assert.Equal(CdsColorKeys.BorderStrong01, CdsTimePicker.DividerColorKey(disabled: false, readOnly: false));
    }

    [Fact]
    public void ReadOnlyFluidRangeDatePickerSplitsItsHalvesWithASubtleLineOnAFieldFill()
    {
        var picker = Themed(new CdsFluidDatePicker { Type = CdsDatePickerType.Range, Label = "Start date", IsReadOnly = true });
        var fields = Private<Grid>(picker, "_fields");
        Assert.Equal(Token(CdsColorKeys.Field01), fields.BackgroundColor);
        Assert.Equal(0, fields.ColumnSpacing);
        Assert.True(picker.RangeDivider.IsVisible);
        Assert.Equal(Token(CdsColorKeys.BorderSubtle01), picker.RangeDivider.Color);
        picker.IsReadOnly = false;
        Assert.Equal(Token(CdsColorKeys.BorderStrong01), picker.RangeDivider.Color);
    }

    // ---- CA-38: pointer opens the calendar; the icon stays in fluid invalid and warning states ----

    [Fact]
    public void FocusingADateFieldOpensTheCalendar()
    {
        var picker = new CdsDatePicker { Label = "Date" };
        picker.OnEntryFocused(0);
        Assert.True(picker.IsOpen);

        var range = new CdsFluidDatePicker { Type = CdsDatePickerType.Range, Label = "Start date" };
        range.OnEntryFocused(1);
        Assert.True(range.IsOpen);
    }

    [Fact]
    public void FocusDoesNotOpenAReadOnlyDisabledOrSimplePicker()
    {
        var readOnly = new CdsDatePicker { IsReadOnly = true };
        readOnly.OnEntryFocused(0);
        Assert.False(readOnly.IsOpen);
        var disabled = new CdsDatePicker { IsEnabled = false };
        disabled.OnEntryFocused(0);
        Assert.False(disabled.IsOpen);
        var simple = new CdsDatePicker { Type = CdsDatePickerType.Simple };
        simple.OnEntryFocused(0);
        Assert.False(simple.IsOpen);
    }

    [Fact]
    public void FluidDatePickerKeepsItsCalendarIconWhenInvalidOrWarning()
    {
        var picker = new CdsFluidDatePicker { Label = "Snapshot date", IsInvalid = true, InvalidText = "Choose a date" };
        Themed(picker);
        Assert.True(picker.CalendarIcons.Start.IsVisible);
        picker.IsInvalid = false;
        picker.IsWarning = true;
        Assert.True(picker.CalendarIcons.Start.IsVisible);

        // The default layout puts the status icon where the calendar icon was.
        var plain = Themed(new CdsDatePicker { Label = "Snapshot date", IsInvalid = true });
        Assert.False(plain.CalendarIcons.Start.IsVisible);
    }

    // ---- CA-39: one picker-level message for a fluid range; input-level errors stay on their field ----

    [Fact]
    public void FluidRangePickerLevelErrorIsOneMessageUnderBothFields()
    {
        var picker = Themed(new CdsFluidDatePicker
        {
            Type = CdsDatePickerType.Range,
            Label = "From",
            EndLabel = "To",
            IsInvalid = true,
            InvalidText = "Choose a period of 92 days or less",
        });
        var start = FluidProbe.ChromeOf(picker);
        var end = Private<CdsFieldChrome>(picker, "EndChrome");
        Assert.Equal("Choose a period of 92 days or less", picker.FluidMessage.Message);
        Assert.Null(start.CurrentMessage);
        Assert.Null(end.CurrentMessage);
        Assert.True(Private<CdsFocusRing>(picker, "_fluidRing").IsActive);
        // The bottom rules go and the split becomes an inset line.
        Assert.Equal(0, start.Rule.Opacity);
        Assert.Equal(0, end.Rule.Opacity);
        Assert.Equal(new Thickness(0, 8), picker.RangeDivider.Margin);
    }

    [Fact]
    public void FluidRangeWarningDrawsARuleUnderTheMessage()
    {
        var picker = Themed(new CdsFluidDatePicker { Type = CdsDatePickerType.Range, IsWarning = true, WarnText = "Long period" });
        Assert.Equal("Long period", picker.FluidMessage.Message);
        Assert.True(picker.FluidMessage.ShowsRule);
        Assert.False(Private<CdsFocusRing>(picker, "_fluidRing").IsActive);
    }

    [Fact]
    public void InputLevelErrorOutlinesOnlyThatEndOfTheRange()
    {
        var picker = Themed(new CdsFluidDatePicker
        {
            Type = CdsDatePickerType.Range,
            Label = "From",
            EndLabel = "To",
            IsEndInvalid = true,
            EndInvalidText = "End date must be after the start date",
        });
        var start = FluidProbe.ChromeOf(picker);
        var end = Private<CdsFieldChrome>(picker, "EndChrome");
        Assert.Null(picker.FluidMessage.Message);
        Assert.Null(start.CurrentMessage);
        Assert.Equal("End date must be after the start date", end.CurrentMessage);
        Assert.True(picker.EndInputInvalid);
        Assert.False(picker.StartInputInvalid);

        picker.IsEndInvalid = false;
        picker.IsStartInvalid = true;
        picker.StartInvalidText = "Enter a start date";
        Assert.Equal("Enter a start date", start.CurrentMessage);
        Assert.Null(end.CurrentMessage);
    }

    [Fact]
    public void EndInvalidIsIgnoredBySinglePickers()
    {
        var picker = new CdsFluidDatePicker { IsEndInvalid = true, EndInvalidText = "x" };
        Assert.False(picker.EndInputInvalid);
        Assert.Null(FluidProbe.ChromeOf(picker).CurrentMessage);
    }

    // ---- CA-40: only a range has a split ----

    [Fact]
    public void SingleAndSimpleFluidDatePickersHaveNoSplitLine()
    {
        foreach (var type in new[] { CdsDatePickerType.Single, CdsDatePickerType.Simple })
        {
            var picker = Themed(new CdsFluidDatePicker { Type = type });
            var fields = Private<Grid>(picker, "_fields");
            Assert.Equal(0, fields.ColumnSpacing);
            Assert.NotEqual(Token(CdsColorKeys.BorderStrong01), fields.BackgroundColor);
            Assert.False(picker.RangeDivider.IsVisible);
        }
        Assert.Equal(0, Private<Grid>(new CdsDatePicker(), "_fields").ColumnSpacing);
        Assert.Equal(1, Private<Grid>(new CdsDatePicker { Type = CdsDatePickerType.Range }, "_fields").ColumnSpacing);
    }

    // ---- CA-41: a long fluid label stays on one line ----

    [Fact]
    public void FluidLabelTruncatesOnOneLineAndTheDefaultLabelWraps()
    {
        var chrome = new CdsFieldChrome { Label = "Fully qualified domain name of the private service endpoint", IsFluid = true };
        var label = Private<CdsText>(chrome, "_label");
        Assert.Equal(1, label.MaxLines);
        Assert.Equal(LineBreakMode.TailTruncation, label.LineBreakMode);
        chrome.IsFluid = false;
        Assert.Equal(-1, label.MaxLines);
        Assert.Equal(LineBreakMode.WordWrap, label.LineBreakMode);
    }

    // ---- CA-42 and CA-43: condensed rows, a 220px cap, content in the top 40px ----

    [Fact]
    public void FluidListsHaveSixtyFourPixelRowsCappedAtTwoHundredTwenty()
    {
        Assert.Equal((64d, 220d, 40d), new CdsFluidDropdown().ListMetrics);
        Assert.Equal((40d, 220d, 40d), new CdsFluidComboBox { IsCondensed = true }.ListMetrics);
        Assert.Equal((64d, 220d, 40d), new CdsFluidMultiSelect().ListMetrics);
        // The default layout ignores IsCondensed: rows match the field and the list shows 5.5 of them.
        Assert.Equal((48d, 264d, 48d), new CdsDropdown { Size = CdsSize.Lg, IsCondensed = true }.ListMetrics);
    }

    [Fact]
    public void TallFluidRowsKeepTheirContentInTheTopBand()
    {
        var entry = new ListBoxEntry("Frankfurt", "Frankfurt", false);
        var tall = new CdsListBoxRow(entry, 64, inline: false, multi: false, isSelected: true, content: null, contentHeight: 40);
        Assert.Equal(new Thickness(16, 0, 16, 24), Private<Grid>(tall, "_root").Padding);
        var plain = new CdsListBoxRow(entry, 40, inline: false, multi: false, isSelected: true, content: null);
        Assert.Equal(new Thickness(16, 0), Private<Grid>(plain, "_root").Padding);

        var list = new CdsListBoxList(inline: false, multi: false, "Regions", 64, 220, 40);
        Assert.Equal(220, list.MaxHeight);
        Assert.Equal(40, list.ContentHeight);
        Assert.Equal(220, new CdsListBoxList(CdsSize.Md, inline: false, multi: false, "Regions").MaxHeight);
    }

    // ---- CA-44: fluid fields fill the height of their cell ----

    [Fact]
    public void FluidFieldsFillATallerCellWithTheFieldFill()
    {
        var input = Themed(new CdsFluidTextInput { Label = "Name" });
        var chrome = FluidProbe.ChromeOf(input);
        Assert.Equal(GridUnitType.Star, Assert.Single(chrome.RowDefinitions).Height.GridUnitType);
        var fill = Private<Grid>(chrome, "_fluidFill");
        Assert.True(fill.IsVisible);
        Assert.Equal(2, Grid.GetRow(fill));
        Assert.Equal(Token(CdsColorKeys.Field01), fill.BackgroundColor);

        var plain = FluidProbe.ChromeOf(new CdsTextInput());
        Assert.All(plain.RowDefinitions, r => Assert.Equal(GridUnitType.Auto, r.Height.GridUnitType));
        Assert.False(Private<Grid>(plain, "_fluidFill").IsVisible);

        var time = new CdsFluidTimePicker();
        var root = Private<Grid>(time, "_root");
        Assert.Equal(GridUnitType.Star, Assert.Single(root.RowDefinitions).Height.GridUnitType);
        Assert.True(Private<Grid>(new CdsFluidDatePicker(), "_fluidFill").IsVisible);
    }

    // ---- CA-45: fluid time picker invalid and warning ----

    [Fact]
    public void FluidTimePickerWarningMovesTheRulesUnderTheMessageAndInsetsTheDividers()
    {
        var picker = Themed(new CdsFluidTimePicker { Label = "Start time", IsWarning = true, WarnText = "Backups also run at 3:00." });
        var time = FluidProbe.ChromeOf(picker);
        var period = FluidProbe.ChromeOf(Private<CdsSelect>(picker, "PeriodSelect"));
        Assert.Equal("Backups also run at 3:00.", picker.FluidMessage.Message);
        Assert.True(picker.FluidMessage.ShowsRule);
        Assert.Equal(0, time.Rule.Opacity);
        Assert.Equal(0, period.Rule.Opacity);
        Assert.Equal(new Thickness(0, 8), picker.FluidDividers.Period.Margin);

        picker.IsWarning = false;
        Assert.Null(picker.FluidMessage.Message);
        Assert.Equal(1, time.Rule.Opacity);
        Assert.Equal(1, period.Rule.Opacity);
        Assert.Equal(new Thickness(0, 0, 0, 1), picker.FluidDividers.Period.Margin);
    }

    [Fact]
    public void FluidTimePickerInvalidHasNoRuleUnderTheMessage()
    {
        var picker = Themed(new CdsFluidTimePicker { Label = "Start time", IsInvalid = true, InvalidText = "Enter a time" });
        Assert.Equal("Enter a time", picker.FluidMessage.Message);
        Assert.False(picker.FluidMessage.ShowsRule);
    }

    // ---- CA-46: disabled fluid number input and search keep a subtle rule ----

    [Fact]
    public void DisabledFluidNumberInputAndSearchKeepASubtleBottomRule()
    {
        var number = Themed(new CdsFluidNumberInput { Label = "Replicas", IsEnabled = false });
        Assert.Equal(Token(CdsColorKeys.BorderSubtle01), FluidProbe.ChromeOf(number).Rule.Color);
        var search = Themed(new CdsFluidSearch { Label = "Search", IsEnabled = false });
        Assert.Equal(Token(CdsColorKeys.BorderSubtle01), FluidProbe.ChromeOf(search).Rule.Color);

        // Other disabled fields, and the default layout, drop the rule.
        var text = Themed(new CdsFluidTextInput { IsEnabled = false });
        Assert.Equal(Colors.Transparent, FluidProbe.ChromeOf(text).Rule.Color);
        var plain = Themed(new CdsNumberInput { IsEnabled = false });
        Assert.Equal(Colors.Transparent, FluidProbe.ChromeOf(plain).Rule.Color);
    }

    // ---- CA-47: read-only fluid value is text-secondary ----

    [Fact]
    public void ReadOnlyFluidTextInputValueIsSecondary()
    {
        Assert.Equal(CdsColorKeys.TextSecondary, CdsTextInput.ValueColorKey(disabled: false, readOnly: true, fluid: true));
        Assert.Equal(CdsColorKeys.TextPrimary, CdsTextInput.ValueColorKey(disabled: false, readOnly: true, fluid: false));
        Assert.Equal(CdsColorKeys.TextDisabled, CdsTextInput.ValueColorKey(disabled: true, readOnly: true, fluid: true));

        var input = Themed(new CdsFluidTextInput { Label = "Cluster ID", Text = "c-42", IsReadOnly = true });
        Assert.Equal(Token(CdsColorKeys.TextSecondary), Private<CdsNativeEntry>(input, "Entry").TextColor);
    }

    // ---- CA-48: fluid password ----

    [Fact]
    public void FluidPasswordInputHasAThirtyTwoPixelToggleTwentySixFromTheTop()
    {
        var input = new CdsFluidPasswordInput { Label = "API key" };
        var chrome = FluidProbe.ChromeOf(input);
        Assert.True(chrome.IsFluidLayout);
        Assert.IsAssignableFrom<CdsPasswordInput>(input);
        var toggle = input.GetVisualTreeDescendants().OfType<CdsFieldIconButton>().Single();
        var box = Assert.IsAssignableFrom<View>(toggle.Content);
        Assert.Equal((32d, 32d), (box.WidthRequest, box.HeightRequest));
        Assert.Equal(LayoutOptions.Start, chrome.Trailing.VerticalOptions);
        Assert.Equal(new Thickness(0, 26, 0, 0), chrome.Trailing.Margin);
        var tooltip = Private<CdsTooltip>(input, "_tooltip");
        Assert.Equal(new Thickness(0, 0, -8, 0), tooltip.Margin);

        // The default password input keeps its field-height toggle.
        var plain = new CdsPasswordInput { Size = CdsSize.Lg };
        var plainBox = (View)plain.GetVisualTreeDescendants().OfType<CdsFieldIconButton>().Single().Content;
        Assert.Equal(48, plainBox.WidthRequest);
    }

    // ---- CA-49: fluid skeletons ----

    [Fact]
    public void FluidSkeletonsAreSixtyFourPixelFieldsWithALabelAndAValueBar()
    {
        View[] skeletons =
        [
            new CdsFluidTextInputSkeleton(), new CdsFluidNumberInputSkeleton(), new CdsFluidSearchSkeleton(),
            new CdsFluidSelectSkeleton(), new CdsFluidDropdownSkeleton(), new CdsFluidComboBoxSkeleton(),
            new CdsFluidMultiSelectSkeleton(),
        ];
        foreach (var skeleton in skeletons)
        {
            var field = Assert.IsType<Grid>(((ContentView)skeleton).Content);
            Assert.Equal(64, field.HeightRequest);
            Assert.Equal(2, field.GetVisualTreeDescendants().OfType<CdsShimmerView>().Count());
            Assert.False(AutomationProperties.GetIsInAccessibleTree(skeleton) ?? true);
        }
        var area = Assert.IsType<Grid>(new CdsFluidTextAreaSkeleton().Content);
        Assert.Equal(128, area.HeightRequest);
    }

    [Fact]
    public void FluidPickerSkeletonsSplitLikeTheirPickers()
    {
        static int Parts(View skeleton) => ((Grid)((ContentView)skeleton).Content).ColumnDefinitions.Count;
        Assert.Equal(1, Parts(new CdsFluidDatePickerSkeleton()));
        Assert.Equal(2, Parts(new CdsFluidDatePickerSkeleton { Range = true }));
        var time = new CdsFluidTimePickerSkeleton();
        Assert.Equal(3, Parts(time));
        var widths = ((Grid)time.Content).ColumnDefinitions.Select(c => c.Width.Value).ToArray();
        Assert.Equal([1d, 1d, 2d], widths);
        time.IsOnlyTwo = true;
        Assert.Equal(2, Parts(time));
    }
}

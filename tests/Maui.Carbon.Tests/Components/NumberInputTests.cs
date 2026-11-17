using System.Globalization;

namespace Maui.Carbon.Tests.Components;

public class NumberInputTests
{
    static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    [Theory]
    [InlineData(null, true, null)]
    [InlineData("", true, null)]
    [InlineData("   ", true, null)]
    [InlineData("5", true, 5.0)]
    [InlineData(" -2.5 ", true, -2.5)]
    [InlineData("1e3", true, 1000.0)]
    [InlineData("abc", false, null)]
    [InlineData("-", false, null)]
    [InlineData("NaN", false, null)]
    [InlineData("Infinity", false, null)]
    public void ParsesText(string? text, bool ok, double? expected)
    {
        Assert.Equal(ok, NumberInputModel.TryParse(text, Inv, out var value));
        Assert.Equal(expected, value);
    }

    [Theory]
    [InlineData(1, 0)]
    [InlineData(0.5, 1)]
    [InlineData(0.25, 2)]
    [InlineData(-3.125, 3)]
    [InlineData(1000000, 0)]
    public void CountsDecimals(double value, int decimals) => Assert.Equal(decimals, NumberInputModel.Decimals(value));

    [Fact]
    public void PrecisionIsTheLargerOfStepAndValue()
    {
        Assert.Equal(0, NumberInputModel.Precision(1, 5));
        Assert.Equal(1, NumberInputModel.Precision(0.1, 5));
        Assert.Equal(2, NumberInputModel.Precision(0.1, 2.25));
        Assert.Equal(1, NumberInputModel.Precision(0.1, null));
    }

    [Fact]
    public void StepsAndRoundsFloatingPointNoise()
    {
        Assert.Equal(0.3, NumberInputModel.Step(0.2, 1, 0.1, null, null));
        Assert.Equal(0.2, NumberInputModel.Step(0.3, -1, 0.1, null, null));
        var v = 0.0;
        for (var i = 0; i < 10; i++) v = NumberInputModel.Step(v, 1, 0.1, null, null);
        Assert.Equal(1.0, v);
        Assert.Equal(1.35, NumberInputModel.Step(1.1, 1, 0.25, null, null));
    }

    [Fact]
    public void EmptyCountsAsZero()
    {
        Assert.Equal(1, NumberInputModel.Step(null, 1, 1, null, null));
        Assert.Equal(-1, NumberInputModel.Step(null, -1, 1, null, null));
        Assert.Equal(1, NumberInputModel.Step(null, -1, 1, 1, 10)); // clamped into the range
    }

    [Fact]
    public void StepsClampToTheRange()
    {
        Assert.Equal(10, NumberInputModel.Step(10, 1, 1, 1, 10));
        Assert.Equal(1, NumberInputModel.Step(1, -1, 1, 1, 10));
        Assert.Equal(10, NumberInputModel.Step(9.5, 1, 1, 1, 10));
        Assert.Equal(10, NumberInputModel.Step(50, -1, 1, 1, 10)); // out of range comes back to the nearest limit
        Assert.Equal(1, NumberInputModel.Step(-4, 1, 1, 1, 10));
    }

    [Fact]
    public void NonPositiveStepFallsBackToOne() => Assert.Equal(3, NumberInputModel.Step(2, 1, 0, null, null));

    [Theory]
    [InlineData(5.0, 1.0, 10.0, 5.0)]
    [InlineData(0.0, 1.0, 10.0, 1.0)]
    [InlineData(11.0, 1.0, 10.0, 10.0)]
    [InlineData(-100.0, null, null, -100.0)]
    [InlineData(7.0, null, 5.0, 5.0)]
    [InlineData(3.0, 5.0, null, 5.0)]
    public void Clamps(double value, double? min, double? max, double expected) =>
        Assert.Equal(expected, NumberInputModel.Clamp(value, min, max));

    [Theory]
    [InlineData("5", 1.0, 10.0, false, "Valid")]
    [InlineData("1", 1.0, 10.0, false, "Valid")]
    [InlineData("10", 1.0, 10.0, false, "Valid")]
    [InlineData("0", 1.0, 10.0, false, "BelowMin")]
    [InlineData("11", 1.0, 10.0, false, "AboveMax")]
    [InlineData("", 1.0, 10.0, false, "Empty")]
    [InlineData("", 1.0, 10.0, true, "Valid")]
    [InlineData("x", 1.0, 10.0, true, "NotANumber")]
    [InlineData("-1e9", null, null, false, "Valid")]
    public void ValidatesAgainstTheRange(string text, double? min, double? max, bool allowEmpty, string expected) =>
        Assert.Equal(expected, NumberInputModel.Validate(text, Inv, min, max, allowEmpty).ToString());

    [Fact]
    public void FormatsWithPrecision()
    {
        Assert.Equal("3", NumberInputModel.Format(3, 1, Inv));
        Assert.Equal("3.0", NumberInputModel.Format(3, 0.1, Inv));
        Assert.Equal("2.5", NumberInputModel.Format(2.5, 1, Inv));
        Assert.Equal("0.30", NumberInputModel.Format(0.3, 0.01, Inv));
        Assert.Equal("", NumberInputModel.Format(null, 1, Inv));
    }

    [Fact]
    public void ControlStepsThroughTheModel()
    {
        var input = new CdsNumberInput { Label = "Worker nodes", Min = 1, Max = 3, Value = 2 };
        var events = new List<CdsNumberChangedEventArgs>();
        input.ValueChanged += (_, e) => events.Add(e);
        input.StepBy(CdsNumberDirection.Up);
        Assert.Equal(3, input.Value);
        input.StepBy(CdsNumberDirection.Up);
        Assert.Equal(3, input.Value); // clamped at Max
        input.StepBy(CdsNumberDirection.Down);
        Assert.Equal(2, input.Value);
        Assert.All(events, e => Assert.NotEqual(CdsNumberDirection.None, e.Direction));
        Assert.Equal(3, events.Count);
    }

    [Fact]
    public void ControlAllowsEmptyAndKeepsPrecision()
    {
        var input = new CdsNumberInput { AllowEmpty = true, Step = 0.1, Value = null };
        Assert.Null(input.Value);
        input.StepBy(CdsNumberDirection.Up);
        Assert.Equal(0.1, input.Value);
        input.StepBy(CdsNumberDirection.Up);
        input.StepBy(CdsNumberDirection.Up);
        Assert.Equal(0.3, input.Value);
    }

    [Fact]
    public void ControlSetsEveryPropertyWithoutThrowing()
    {
        var input = new CdsNumberInput
        {
            Label = "Timeout", HideLabel = true, HelperText = "Seconds", Placeholder = "30", Min = 0, Max = 60, Step = 5, Value = 30,
            AllowEmpty = true, HideSteppers = true, IncrementLabel = "More", DecrementLabel = "Less", IsInvalid = true,
            InvalidText = "No", IsWarning = true, WarnText = "Hmm", IsReadOnly = true, Size = CdsSize.Sm, IsInline = true,
        };
        input.IsEnabled = false;
        Assert.Equal(5, input.Step);
        input.Step = -2;
        Assert.Equal(1, input.Step);
    }
}

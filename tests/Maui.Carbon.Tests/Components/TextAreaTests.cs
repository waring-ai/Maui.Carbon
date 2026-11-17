namespace Maui.Carbon.Tests.Components;

public class TextAreaTests
{
    [Theory]
    [InlineData(null, 0)]
    [InlineData("", 0)]
    [InlineData("one", 1)]
    [InlineData("  one   two\nthree\t ", 3)]
    [InlineData("a-b c", 2)]
    public void CountsWords(string? text, int words) =>
        Assert.Equal(words, TextAreaModel.Count(text, CdsCounterMode.Word));

    [Fact]
    public void CountsCharacters()
    {
        Assert.Equal(0, TextAreaModel.Count(null, CdsCounterMode.Character));
        Assert.Equal(7, TextAreaModel.Count("a b\nc d", CdsCounterMode.Character));
    }

    [Fact]
    public void CounterTextNeedsALimit()
    {
        Assert.Null(TextAreaModel.CounterText("abc", CdsCounterMode.Character, 0));
        Assert.Equal("3/10", TextAreaModel.CounterText("abc", CdsCounterMode.Character, 10));
        Assert.Equal("2/5", TextAreaModel.CounterText("hello world", CdsCounterMode.Word, 5));
        Assert.Equal("0/5", TextAreaModel.CounterText(null, CdsCounterMode.Word, 5));
    }

    [Fact]
    public void LimitsCharacters()
    {
        Assert.Equal("abc", TextAreaModel.Limit("abcdef", CdsCounterMode.Character, 3));
        Assert.Equal("abc", TextAreaModel.Limit("abc", CdsCounterMode.Character, 3));
        Assert.Equal("abcdef", TextAreaModel.Limit("abcdef", CdsCounterMode.Character, 0));
        Assert.Null(TextAreaModel.Limit(null, CdsCounterMode.Character, 3));
    }

    [Fact]
    public void LimitsWordsKeepingSeparatorsInsideTheLimit()
    {
        Assert.Equal("one two", TextAreaModel.Limit("one two three", CdsCounterMode.Word, 2));
        Assert.Equal("one  two", TextAreaModel.Limit("one  two   three four", CdsCounterMode.Word, 2));
        Assert.Equal("one two ", TextAreaModel.Limit("one two ", CdsCounterMode.Word, 2));
        Assert.Equal("one two three", TextAreaModel.Limit("one two three", CdsCounterMode.Word, 3));
        Assert.Equal("one", TextAreaModel.Limit("one", CdsCounterMode.Word, 1));
        Assert.Equal("one two three", TextAreaModel.Limit("one two three", CdsCounterMode.Word, 0));
    }

    [Fact]
    public void LimitNoteNamesTheUnit()
    {
        Assert.Equal("Character limit 300", TextAreaModel.LimitNote(CdsCounterMode.Character, 300));
        Assert.Equal("Word limit 50", TextAreaModel.LimitNote(CdsCounterMode.Word, 50));
        Assert.Null(TextAreaModel.LimitNote(CdsCounterMode.Word, 0));
    }

    [Fact]
    public void RowsSetTheHeight()
    {
        Assert.Equal(80, TextAreaModel.Height(4, 20));
        Assert.Equal(20, TextAreaModel.Height(0, 20));
    }

    [Fact]
    public void ControlSetsEveryPropertyWithoutThrowing()
    {
        var area = new CdsTextArea
        {
            Label = "Justification", HideLabel = true, HelperText = "Say why", Placeholder = "Reason", Rows = 6,
            EnableCounter = true, MaxCount = 300, CounterMode = CdsCounterMode.Word, IsInvalid = true, InvalidText = "Required",
            IsWarning = true, WarnText = "Short", IsReadOnly = true, Size = CdsSize.Lg, IsInline = true, Text = "hello",
        };
        area.IsEnabled = false;
        Assert.Equal("hello", area.Text);
        Assert.Equal(6, area.Rows);
        area.Rows = -3;
        Assert.Equal(1, area.Rows);
    }

    [Fact]
    public void WordLimitCutsTextSetFromCode()
    {
        var area = new CdsTextArea { EnableCounter = true, MaxCount = 2, CounterMode = CdsCounterMode.Word };
        area.Text = "one two three";
        // The text reaches the editor, whose TextChanged cuts it to the limit.
        Assert.Equal("one two", area.Text);
    }
}

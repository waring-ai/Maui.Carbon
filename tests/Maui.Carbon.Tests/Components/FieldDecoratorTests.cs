namespace Maui.Carbon.Tests.Components;

/// <summary>The AI label decorator of the form fields (Carbon's <c>decorator</c> slot and AI presence).</summary>
public class FieldDecoratorTests
{
    static Color Token(string key) => CdsPalettes.GetTokenColor(CdsThemeName.White, key);

    static CdsFieldChrome ChromeOf(object field) => Private.Field<CdsFieldChrome>(field, "Chrome");

    [Fact]
    public void An_ai_label_decorator_is_shown_at_its_mini_size_in_the_trailing_row()
    {
        var label = new CdsAILabel { Size = CdsAILabelSize.Md };
        var input = new CdsTextInput { Label = "Cluster name", Decorator = label };
        var chrome = ChromeOf(input);
        Assert.Same(label, chrome.Decorator);
        Assert.Equal(CdsAILabelSize.Mini, label.Size);
        Assert.Contains(label, chrome.Trailing.Children);
        Assert.True(chrome.IsAiPresence);
    }

    [Fact]
    public void The_decorator_sits_after_the_status_icon_and_before_the_controls_own_parts()
    {
        var select = new CdsSelect { Label = "Region", IsInvalid = true, InvalidText = "Choose a region" };
        var label = new CdsAILabel();
        select.Decorator = label;
        var trailing = ChromeOf(select).Trailing.Children;
        var status = Private.Field<CdsIcon>(ChromeOf(select), "_statusIcon");
        Assert.Equal(trailing.IndexOf(status) + 1, trailing.IndexOf(label));
        // The chevron stays last.
        Assert.Equal(trailing.Count - 1, trailing.IndexOf(Private.Field<Grid>(select, "_chevronHost")));
    }

    [Fact]
    public void Ai_presence_draws_the_aura_and_an_interactive_rule()
    {
        var input = new CdsTextInput { Label = "Summary", Decorator = new CdsAILabel() };
        _ = new CdsTheme { Theme = CdsThemeName.White, Body = input };
        var chrome = ChromeOf(input);
        Assert.Equal(CdsColorKeys.Field01, chrome.AiFillKey);
        Assert.IsType<LinearGradientBrush>(chrome.Field.Background);
        Assert.Equal(Token("ai-border-strong"), chrome.Rule.Color);
    }

    [Fact]
    public void Revert_drops_the_ai_styling_and_reverting_brings_it_back()
    {
        var label = new CdsAILabel();
        var input = new CdsTextInput { Label = "Summary", Decorator = label };
        _ = new CdsTheme { Theme = CdsThemeName.White, Body = input };
        var chrome = ChromeOf(input);

        label.IsRevertActive = true;
        Assert.False(chrome.IsAiPresence);
        Assert.Null(chrome.AiFillKey);
        Assert.True(Brush.IsNullOrEmpty(chrome.Field.Background));
        Assert.Equal(Token("border-strong-01"), chrome.Rule.Color);

        label.IsRevertActive = false;
        Assert.True(chrome.IsAiPresence);
        Assert.Equal(CdsColorKeys.Field01, chrome.AiFillKey);
        Assert.IsType<LinearGradientBrush>(chrome.Field.Background);
    }

    [Fact]
    public void Read_only_and_disabled_fields_show_no_aura()
    {
        var input = new CdsTextInput { Label = "Summary", Decorator = new CdsAILabel(), IsReadOnly = true };
        Assert.Null(ChromeOf(input).AiFillKey);
        input.IsReadOnly = false;
        Assert.Equal(CdsColorKeys.Field01, ChromeOf(input).AiFillKey);
        input.IsEnabled = false;
        Assert.Null(ChromeOf(input).AiFillKey);
    }

    [Fact]
    public void Another_view_is_placed_without_ai_styling_and_removing_it_clears_the_slot()
    {
        var tip = new CdsToggletip();
        var dropdown = new CdsDropdown { Label = "Plan", Decorator = tip };
        var chrome = ChromeOf(dropdown);
        Assert.Contains(tip, chrome.Trailing.Children);
        Assert.False(chrome.IsAiPresence);
        Assert.Null(chrome.AiFillKey);

        dropdown.Decorator = null;
        Assert.DoesNotContain(tip, chrome.Trailing.Children);
        Assert.Null(chrome.Decorator);
    }

    [Fact]
    public void Every_field_built_on_the_chrome_takes_a_decorator()
    {
        View[] fields =
        [
            new CdsTextInput(), new CdsPasswordInput(), new CdsTextArea(), new CdsNumberInput(), new CdsSelect(),
            new CdsDropdown(), new CdsComboBox(), new CdsMultiSelect(), new CdsDatePicker(),
            new CdsFluidTextInput(), new CdsFluidSelect(), new CdsFluidDatePicker(),
        ];
        foreach (var field in fields)
        {
            var label = new CdsAILabel();
            switch (field)
            {
                case CdsTextInput t: t.Decorator = label; break;
                case CdsFormField f: f.Decorator = label; break;
            }
            Assert.True(ChromeOf(field).IsAiPresence, field.GetType().Name);
        }
    }
}

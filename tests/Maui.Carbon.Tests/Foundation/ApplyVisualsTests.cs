namespace Maui.Carbon.Tests.Foundation;

public class ApplyVisualsTests
{
    sealed class QuietControl : CdsControl
    {
        public QuietControl() => SetChrome(new Grid());

        public int Applied { get; private set; }

        protected override void ApplyVisuals() => Applied++;
    }

    [Fact]
    public void A_control_whose_state_never_changes_applies_its_visuals_once_it_is_parented()
    {
        var control = new QuietControl();
        Assert.Equal(0, control.Applied);

        _ = new ContentView { Content = control };
        Assert.Equal(1, control.Applied);
    }

    [Fact]
    public void Re_parenting_does_not_re_apply_visuals_that_were_already_applied()
    {
        var control = new QuietControl();
        var first = new ContentView { Content = control };
        first.Content = null;
        _ = new ContentView { Content = control };
        Assert.Equal(1, control.Applied);
    }

    [Fact]
    public void An_unchecked_multi_select_row_gets_its_checkbox_colors_without_a_state_change()
    {
        var entry = new ListBoxEntry("Cluster created", "Cluster created", false);
        var row = new CdsListBoxRow(entry, 40, inline: false, multi: true, isSelected: false, content: null);
        var root = new ContentView { Resources = CdsPalettes.CreateThemeDictionary(CdsThemeName.White) };
        root.Content = row;

        var glyph = row.GetVisualTreeDescendants().OfType<CdsCheckGlyph>().Single();
        Assert.NotNull(glyph.BorderColor);
        Assert.True(glyph.BorderColor!.Alpha > 0);
    }
}

namespace Maui.Carbon.Tests.Components;

public class ExpandableTileTests
{
    [Fact]
    public void Plain_text_content_is_not_interactive()
    {
        var body = new VerticalStackLayout { new CdsText("TLS certificate"), new CdsText("Expires in 12 days") };
        Assert.False(ExpandableTileModel.ContainsInteractive(body));
        Assert.False(ExpandableTileModel.ContainsInteractive(null));
    }

    [Fact]
    public void A_nested_button_makes_the_content_interactive()
    {
        var body = new VerticalStackLayout { new CdsText("TLS certificate"), new Grid { new Microsoft.Maui.Controls.Button { Text = "Renew" } } };
        Assert.True(ExpandableTileModel.ContainsInteractive(body));
    }

    [Fact]
    public void Carbon_controls_and_native_inputs_are_interactive()
    {
        Assert.True(ExpandableTileModel.ContainsInteractive(new VerticalStackLayout { new CdsCheckbox("Enable") }));
        Assert.True(ExpandableTileModel.ContainsInteractive(new Entry()));
        Assert.True(ExpandableTileModel.ContainsInteractive(new Switch()));
    }

    [Fact]
    public void A_label_with_a_tap_recognizer_is_a_link()
    {
        var link = new Label { Text = "Renew now" };
        Assert.False(ExpandableTileModel.ContainsInteractive(new VerticalStackLayout { link }));
        link.GestureRecognizers.Add(new TapGestureRecognizer());
        Assert.True(ExpandableTileModel.ContainsInteractive(new VerticalStackLayout { link }));
    }

    [Fact]
    public void An_explicit_variant_overrides_detection()
    {
        var button = new Microsoft.Maui.Controls.Button();
        Assert.False(ExpandableTileModel.UseInteractiveVariant(false, button, null));
        Assert.True(ExpandableTileModel.UseInteractiveVariant(true, new Label(), null));
        Assert.True(ExpandableTileModel.UseInteractiveVariant(null, new Label(), button));
        Assert.False(ExpandableTileModel.UseInteractiveVariant(null, new Label(), new Label()));
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(0.5, 40)]
    [InlineData(1, 80)]
    [InlineData(-1, 0)]
    [InlineData(2, 80)]
    public void Height_interpolates_and_clamps_progress(double progress, double expected) =>
        Assert.Equal(expected, ExpandableTileModel.HeightAt(0, 80, progress));

    [Fact]
    public void Height_can_shrink() => Assert.Equal(60, ExpandableTileModel.HeightAt(100, 20, 0.5));

    [Fact]
    public void Expanding_is_faster_than_collapsing()
    {
        Assert.Equal(CdsDuration.Fast02, ExpandableTileModel.Duration(true));
        Assert.Equal(CdsDuration.Moderate01, ExpandableTileModel.Duration(false));
    }

    [Fact]
    public void Description_follows_the_state_and_falls_back_to_the_defaults()
    {
        Assert.Equal("Show details for prod-eu-1", ExpandableTileModel.Description(false, "Show details for prod-eu-1", "Hide details"));
        Assert.Equal("Hide details", ExpandableTileModel.Description(true, "Show details for prod-eu-1", "Hide details"));
        Assert.Equal("Interact to expand Tile", ExpandableTileModel.Description(false, null, null));
        Assert.Equal("Interact to collapse Tile", ExpandableTileModel.Description(true, "", ""));
    }

    [Fact]
    public void Tile_picks_its_variant_from_its_content()
    {
        var plain = new CdsExpandableTile { Body = new CdsText("Certificate"), ExpandedContent = new CdsText("Issued for api.example.com") };
        Assert.False(plain.IsInteractiveVariant);

        var interactive = new CdsExpandableTile { Body = new VerticalStackLayout { new CdsText("Certificate"), new CdsButton("Renew now") } };
        Assert.True(interactive.IsInteractiveVariant);

        plain.IsInteractive = true;
        Assert.True(plain.IsInteractiveVariant);
    }

    [Fact]
    public void Tile_toggles_and_raises_ExpandedChanged_without_a_platform()
    {
        var tile = new CdsExpandableTile
        {
            Body = new CdsText("Certificate"),
            ExpandedContent = new CdsText("Details"),
            CollapsedText = "Show",
            ExpandedText = "Hide",
            CollapsedDescription = "Show certificate details",
            ExpandedDescription = "Hide certificate details",
        };
        CdsChoiceChangedEventArgs<bool>? seen = null;
        tile.ExpandedChanged += (_, e) => seen = e;
        tile.IsExpanded = true;
        Assert.True(seen!.NewValue);
        Assert.False(seen.OldValue);
        tile.IsExpanded = false;
        Assert.False(tile.IsExpanded);
        tile.IsEnabled = false;
    }
}

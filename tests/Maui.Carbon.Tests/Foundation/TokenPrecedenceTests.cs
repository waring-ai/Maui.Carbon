using Microsoft.Maui.Graphics;

namespace Maui.Carbon.Tests.Foundation;

public class TokenPrecedenceTests
{
    static (ContentView Root, BoxView Box) Tree()
    {
        var box = new BoxView();
        var root = new ContentView { Content = box };
        root.Resources = CdsPalettes.CreateThemeDictionary(CdsThemeName.White);
        return (root, box);
    }

    [Fact]
    public void Null_then_key_applies_the_key()
    {
        var (_, box) = Tree();
        CdsTokens.SetColor(box, BoxView.ColorProperty, null);
        Assert.Equal(Colors.Transparent, box.Color);
        CdsTokens.SetColor(box, BoxView.ColorProperty, CdsColorKeys.Layer01);
        Assert.Equal(CdsPalettes.GetColor(CdsThemeName.White, CdsColorKeys.Layer01), box.Color);
    }

    [Fact]
    public void Key_then_null_then_key_round_trips()
    {
        var (_, box) = Tree();
        CdsTokens.SetColor(box, BoxView.ColorProperty, CdsColorKeys.Focus);
        CdsTokens.SetColor(box, BoxView.ColorProperty, null);
        Assert.Equal(Colors.Transparent, box.Color);
        CdsTokens.SetColor(box, BoxView.ColorProperty, CdsColorKeys.Layer01);
        Assert.Equal(CdsPalettes.GetColor(CdsThemeName.White, CdsColorKeys.Layer01), box.Color);
    }

    [Fact]
    public void A_local_initializer_does_not_block_a_token()
    {
        var box = new BoxView { Color = Colors.Transparent };
        var root = new ContentView { Content = box, Resources = CdsPalettes.CreateThemeDictionary(CdsThemeName.White) };
        CdsTokens.SetColor(box, BoxView.ColorProperty, CdsColorKeys.Focus);
        Assert.Equal(CdsPalettes.GetColor(CdsThemeName.White, CdsColorKeys.Focus), box.Color);
        Assert.NotNull(root);
    }

    [Fact]
    public void Theme_switch_updates_a_bound_token()
    {
        var (root, box) = Tree();
        CdsTokens.SetColor(box, BoxView.ColorProperty, CdsColorKeys.Layer01);
        root.Resources = CdsPalettes.CreateThemeDictionary(CdsThemeName.G100);
        Assert.Equal(CdsPalettes.GetColor(CdsThemeName.G100, CdsColorKeys.Layer01), box.Color);
    }

    [Fact]
    public void Null_key_survives_a_theme_change()
    {
        var (root, box) = Tree();
        CdsTokens.SetColor(box, BoxView.ColorProperty, null);
        root.Resources = CdsPalettes.CreateThemeDictionary(CdsThemeName.G100);
        Assert.Equal(Colors.Transparent, box.Color);
    }

    [Fact]
    public void Brush_null_then_key_applies_the_key()
    {
        var (_, box) = Tree();
        CdsTokens.SetBrush(box, VisualElement.BackgroundProperty, null);
        CdsTokens.SetBrush(box, VisualElement.BackgroundProperty, CdsColorKeys.Focus);
        Assert.Equal(CdsPalettes.GetColor(CdsThemeName.White, CdsColorKeys.Focus), (box.Background as SolidColorBrush)?.Color);
    }
}

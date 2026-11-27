namespace Maui.Carbon.Tests.Foundation;

public class DeckTests
{
    [Fact]
    public void Only_the_active_child_counts_as_shown()
    {
        var a = new Grid();
        var b = new Grid();
        var inner = new Label();
        b.Add(inner);
        var deck = new CdsDeck { Children = { a, b } };
        Assert.True(CdsDeck.IsHidden(a));
        Assert.True(CdsDeck.IsHidden(b));

        deck.Active = b;
        Assert.True(CdsDeck.IsHidden(a));
        Assert.True(CdsDeck.IsShown(inner));
        Assert.True(a.IsVisible);

        deck.Active = a;
        Assert.False(CdsDeck.IsShown(inner));
    }

    [Fact]
    public void Removing_the_active_child_clears_active()
    {
        var a = new Grid();
        var deck = new CdsDeck { Children = { a } };
        deck.Active = a;
        deck.Remove(a);
        Assert.Null(deck.Active);
        Assert.False(CdsDeck.IsHidden(a));
    }

    [Fact]
    public void The_active_view_must_be_a_child() =>
        Assert.Throws<ArgumentException>(() => new CdsDeck { Active = new Grid() });

    [Fact]
    public void Only_the_active_child_is_measured()
    {
        var a = new Fixed(50, 20);
        var b = new Fixed(80, 300);
        var deck = new CdsDeck { Children = { a, b } };
        deck.Active = a;
        var size = ((ICrossPlatformLayout)deck).CrossPlatformMeasure(500, 500);
        Assert.Equal(20, size.Height);
        deck.Active = b;
        Assert.Equal(300, ((ICrossPlatformLayout)deck).CrossPlatformMeasure(500, 500).Height);
        Assert.Equal(1, a.Measures);
    }

    sealed class Fixed(double width, double height) : View
    {
        public int Measures { get; private set; }

        protected override Size MeasureOverride(double widthConstraint, double heightConstraint)
        {
            Measures++;
            return new Size(width, height);
        }
    }
}

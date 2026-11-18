namespace Maui.Carbon;

/// <summary>
/// The drawable of a <see cref="GraphicsView"/> that draws itself. Pass <c>Drawable = new CdsWeakDrawable(this)</c>, never
/// <c>Drawable = this</c>: the platform view holds its drawable strongly, and on iOS and Mac Catalyst a platform view in a
/// view hierarchy keeps its managed peer alive. With the view as its own drawable that peer holds the view, and through
/// it the whole page (parents, the XAML name scope, event handlers), which holds the platform views again: a cycle
/// through native code the garbage collector cannot break, so every page that showed the view would leak.
/// </summary>
internal sealed class CdsWeakDrawable(IDrawable owner) : IDrawable
{
    readonly WeakReference<IDrawable> _owner = new(owner);

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        if (_owner.TryGetTarget(out var owner)) owner.Draw(canvas, dirtyRect);
    }
}

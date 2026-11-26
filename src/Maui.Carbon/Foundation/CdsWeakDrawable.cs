namespace Maui.Carbon;

/// <summary>
/// The drawable of a <see cref="GraphicsView"/> that draws itself. Pass <c>Drawable = new CdsWeakDrawable(this)</c>, never
/// <c>Drawable = this</c>: the platform view holds its drawable strongly, and on iOS and Mac Catalyst a platform view in a
/// view hierarchy keeps its managed peer alive. With the view as its own drawable that peer holds the view, and through
/// it the whole page (parents, the XAML name scope, event handlers), which holds the platform views again: a cycle
/// through native code the garbage collector cannot break, so every page that showed the view would leak.
/// <para>
/// Every draw is measured as <see cref="CdsMetrics.DrawDuration"/>, tagged with <paramref name="component"/> (by default
/// the owner's type).
/// </para>
/// </summary>
internal sealed class CdsWeakDrawable(IDrawable owner, string? component = null) : IDrawable
{
    readonly WeakReference<IDrawable> _owner = new(owner);
    readonly string _component = component ?? owner.GetType().Name;

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        if (!_owner.TryGetTarget(out var owner)) return;
        var start = CdsMeter.Start(CdsMeter.Draw);
        try
        {
            owner.Draw(canvas, dirtyRect);
        }
        finally
        {
            CdsMeter.Stop(CdsMeter.Draw, start, _component);
        }
    }
}

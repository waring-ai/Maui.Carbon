namespace Maui.Carbon;

/// <summary>Where a popup sits relative to its anchor (Carbon's <c>align</c>).</summary>
public enum CdsPlacement
{
    /// <summary>Above, centered.</summary>
    Top,
    /// <summary>Above, start edges aligned.</summary>
    TopStart,
    /// <summary>Above, end edges aligned.</summary>
    TopEnd,
    /// <summary>Below, centered.</summary>
    Bottom,
    /// <summary>Below, start edges aligned.</summary>
    BottomStart,
    /// <summary>Below, end edges aligned.</summary>
    BottomEnd,
    /// <summary>To the left, centered.</summary>
    Left,
    /// <summary>To the left, top edges aligned.</summary>
    LeftTop,
    /// <summary>To the left, bottom edges aligned.</summary>
    LeftBottom,
    /// <summary>To the right, centered.</summary>
    Right,
    /// <summary>To the right, top edges aligned.</summary>
    RightTop,
    /// <summary>To the right, bottom edges aligned.</summary>
    RightBottom,
}

/// <summary>The resolved position of a popup.</summary>
/// <param name="Frame">The popup frame in host coordinates.</param>
/// <param name="Placement">The placement used (flipped when the preferred side did not fit).</param>
/// <param name="CaretOffset">Distance along the popup edge from its start to the anchor's center (for a caret).</param>
public readonly record struct CdsPlacementResult(Rect Frame, CdsPlacement Placement, double CaretOffset);

/// <summary>Pure placement math for anchored popups (tested on its own).</summary>
public static class CdsPlacementResolver
{
    /// <summary>Default margin kept between a popup and the host edges (see <see cref="CdsOverlayOptions.EdgeMargin"/>).</summary>
    public const double EdgeMargin = 8;

    /// <summary>
    /// Places a popup of <paramref name="popup"/> size next to <paramref name="anchor"/> inside a host of
    /// <paramref name="host"/> size, flipping to the opposite side when the preferred side does not fit and the
    /// opposite side fits better, then clamping into the host. An anchor entirely outside the host is not clamped.
    /// <paramref name="edgeMargin"/> is the margin kept to the host edges (0 lets a popup sit flush with an edge, as a
    /// tab tip popover does).
    /// </summary>
    public static CdsPlacementResult Resolve(Rect anchor, Size popup, Size host, CdsPlacement preferred, double offset = 0, bool autoFlip = true, double edgeMargin = EdgeMargin)
    {
        // An anchor scrolled out of view takes its popup with it (as a web popover scrolls away with its trigger):
        // clamping it into the host would float it over unrelated content.
        if (!anchor.IntersectsWith(new Rect(0, 0, host.Width, host.Height)))
        {
            var away = Place(anchor, popup, preferred, offset);
            var awayCaret = Side(preferred) is 'T' or 'B' ? anchor.Center.X - away.X : anchor.Center.Y - away.Y;
            return new CdsPlacementResult(away, preferred, awayCaret);
        }

        var placement = preferred;
        if (autoFlip)
        {
            var side = Side(preferred);
            var space = Space(side, anchor, host, edgeMargin);
            var opposite = Space(Opposite(side), anchor, host, edgeMargin);
            var needed = (side is 'T' or 'B' ? popup.Height : popup.Width) + offset;
            if (space < needed && opposite > space) placement = Flip(preferred);
        }

        var frame = Place(anchor, popup, placement, offset);

        // Clamp into the host (cross axis first, then main axis as a last resort).
        var maxX = Math.Max(edgeMargin, host.Width - popup.Width - edgeMargin);
        var maxY = Math.Max(edgeMargin, host.Height - popup.Height - edgeMargin);
        var x = Math.Clamp(frame.X, Math.Min(edgeMargin, maxX), maxX);
        var y = Math.Clamp(frame.Y, Math.Min(edgeMargin, maxY), maxY);
        frame = new Rect(x, y, popup.Width, popup.Height);

        var caret = Side(placement) is 'T' or 'B'
            ? anchor.Center.X - frame.X
            : anchor.Center.Y - frame.Y;
        return new CdsPlacementResult(frame, placement, caret);
    }

    static Rect Place(Rect a, Size p, CdsPlacement placement, double offset) => placement switch
    {
        CdsPlacement.Top => new(a.Center.X - p.Width / 2, a.Top - offset - p.Height, p.Width, p.Height),
        CdsPlacement.TopStart => new(a.Left, a.Top - offset - p.Height, p.Width, p.Height),
        CdsPlacement.TopEnd => new(a.Right - p.Width, a.Top - offset - p.Height, p.Width, p.Height),
        CdsPlacement.Bottom => new(a.Center.X - p.Width / 2, a.Bottom + offset, p.Width, p.Height),
        CdsPlacement.BottomStart => new(a.Left, a.Bottom + offset, p.Width, p.Height),
        CdsPlacement.BottomEnd => new(a.Right - p.Width, a.Bottom + offset, p.Width, p.Height),
        CdsPlacement.Left => new(a.Left - offset - p.Width, a.Center.Y - p.Height / 2, p.Width, p.Height),
        CdsPlacement.LeftTop => new(a.Left - offset - p.Width, a.Top, p.Width, p.Height),
        CdsPlacement.LeftBottom => new(a.Left - offset - p.Width, a.Bottom - p.Height, p.Width, p.Height),
        CdsPlacement.Right => new(a.Right + offset, a.Center.Y - p.Height / 2, p.Width, p.Height),
        CdsPlacement.RightTop => new(a.Right + offset, a.Top, p.Width, p.Height),
        _ => new(a.Right + offset, a.Bottom - p.Height, p.Width, p.Height),
    };

    static char Side(CdsPlacement p) => p switch
    {
        CdsPlacement.Top or CdsPlacement.TopStart or CdsPlacement.TopEnd => 'T',
        CdsPlacement.Bottom or CdsPlacement.BottomStart or CdsPlacement.BottomEnd => 'B',
        CdsPlacement.Left or CdsPlacement.LeftTop or CdsPlacement.LeftBottom => 'L',
        _ => 'R',
    };

    static char Opposite(char side) => side switch { 'T' => 'B', 'B' => 'T', 'L' => 'R', _ => 'L' };

    static double Space(char side, Rect a, Size host, double margin) => side switch
    {
        'T' => a.Top - margin,
        'B' => host.Height - a.Bottom - margin,
        'L' => a.Left - margin,
        _ => host.Width - a.Right - margin,
    };

    /// <summary>The same alignment on the opposite side.</summary>
    public static CdsPlacement Flip(CdsPlacement p) => p switch
    {
        CdsPlacement.Top => CdsPlacement.Bottom,
        CdsPlacement.TopStart => CdsPlacement.BottomStart,
        CdsPlacement.TopEnd => CdsPlacement.BottomEnd,
        CdsPlacement.Bottom => CdsPlacement.Top,
        CdsPlacement.BottomStart => CdsPlacement.TopStart,
        CdsPlacement.BottomEnd => CdsPlacement.TopEnd,
        CdsPlacement.Left => CdsPlacement.Right,
        CdsPlacement.LeftTop => CdsPlacement.RightTop,
        CdsPlacement.LeftBottom => CdsPlacement.RightBottom,
        CdsPlacement.Right => CdsPlacement.Left,
        CdsPlacement.RightTop => CdsPlacement.LeftTop,
        _ => CdsPlacement.LeftBottom,
    };

    /// <summary>True for the placements above or below the anchor.</summary>
    public static bool IsVertical(CdsPlacement p) => Side(p) is 'T' or 'B';
}

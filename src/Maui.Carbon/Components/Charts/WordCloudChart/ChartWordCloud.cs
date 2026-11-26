namespace Maui.Carbon;

/// <summary>A word for the cloud layout: its text, font size and the data it came from.</summary>
internal sealed class CloudWord(string text, int size, int index)
{
    public string Text { get; } = text;
    /// <summary>The font size in pixels (d3-cloud truncates it to an integer).</summary>
    public int Size { get; } = size;
    public int Index { get; } = index;
    /// <summary>The anchor point: the horizontal centre and the alphabetic baseline.</summary>
    public double X;
    public double Y;
    /// <summary>The ink box (with padding) relative to the anchor.</summary>
    public double X0;
    public double X1;
    public double Y0;
    public double Y1;
}

/// <summary>
/// d3-cloud's word placement, deterministic: words in descending size each start near the centre (seeded), then walk an
/// archimedean spiral until they fit inside the box, overlap no placed word and touch the bounds of the words so far.
/// d3-cloud tests collisions on rendered glyph pixels; this tests each word's ink box (its measured width, and cap or
/// x-height above and descenders below the baseline as its letters need), padded by <c>padding</c>.
/// </summary>
internal static class ChartWordCloud
{
    /// <summary>How far outside the placed words' bounds a word may start touching them.</summary>
    const double Gap = 1;

    const string Ascenders = "bdfhijklt0123456789'\"!?/()[]{}|#$%&@";
    const string Descenders = "gjpqy,;()[]{}|";

    /// <summary>The ink box of a word at a size, relative to its centre and baseline: (x0, x1, y0, y1).</summary>
    public static (double X0, double X1, double Y0, double Y1) InkBox(string text, ChartFont font)
    {
        var w = CdsChartTextMetrics.Width(text, font);
        var tall = text.Any(c => char.IsUpper(c) || Ascenders.Contains(c));
        var low = text.Any(c => Descenders.Contains(c));
        var s = font.Size;
        return (-w / 2, w / 2, -(tall ? 0.74 : 0.53) * s, (low ? 0.22 : 0.02) * s);
    }

    /// <summary>Places the words in a <paramref name="width"/> by <paramref name="height"/> box; returns those that fit, in placing order.</summary>
    public static List<CloudWord> Layout(IEnumerable<CloudWord> words, double width, double height, double padding, Func<double> random, Func<CloudWord, ChartFont> font)
    {
        var data = words.Select((w, i) => (w, i)).OrderByDescending(x => x.w.Size).ThenBy(x => x.i).Select(x => x.w).ToList();
        var placed = new List<CloudWord>();
        (double X0, double Y0, double X1, double Y1)? bounds = null;
        foreach (var d in data)
        {
            d.X = (int)(width * (random() + .5)) >> 1;
            d.Y = (int)(height * (random() + .5)) >> 1;
            var (x0, x1, y0, y1) = InkBox(d.Text, font(d));
            if (x1 - x0 <= 0) continue;
            d.X0 = x0 - padding;
            d.X1 = x1 + padding;
            d.Y0 = y0 - padding;
            d.Y1 = y1 + padding;
            if (!Place(d, placed, bounds, width, height, random)) continue;
            placed.Add(d);
            bounds = bounds is { } b
                ? (Math.Min(b.X0, d.X + d.X0), Math.Min(b.Y0, d.Y + d.Y0), Math.Max(b.X1, d.X + d.X1), Math.Max(b.Y1, d.Y + d.Y1))
                : (d.X + d.X0, d.Y + d.Y0, d.X + d.X1, d.Y + d.Y1);
        }
        return placed;
    }

    /// <summary>d3-cloud's archimedean spiral for a box's aspect ratio.</summary>
    public static (double X, double Y) Spiral(double width, double height, double t)
    {
        var e = width / height;
        t *= .1;
        return (e * t * Math.Cos(t), t * Math.Sin(t));
    }

    static bool Place(CloudWord tag, List<CloudWord> placed, (double X0, double Y0, double X1, double Y1)? bounds, double width, double height, Func<double> random)
    {
        var startX = tag.X;
        var startY = tag.Y;
        var maxDelta = Math.Sqrt(width * width + height * height);
        var dt = random() < .5 ? 1 : -1;
        var t = -dt;
        for (var step = 0; step < 200_000; step++)
        {
            t += dt;
            var (sx, sy) = Spiral(width, height, t);
            var dx = Math.Truncate(sx);
            var dy = Math.Truncate(sy);
            if (Math.Min(Math.Abs(dx), Math.Abs(dy)) >= maxDelta) break;
            tag.X = startX + dx;
            tag.Y = startY + dy;
            if (tag.X + tag.X0 < 0 || tag.Y + tag.Y0 < 0 || tag.X + tag.X1 > width || tag.Y + tag.Y1 > height) continue;
            // d3-cloud also requires overlap with the placed words' bounding box, which only pixel sprites can satisfy
            // without colliding; with boxes the spiral from the centre keeps the cloud together on its own.
            if (bounds is { } b && !(tag.X + tag.X1 > b.X0 - 2 * Gap && tag.X + tag.X0 < b.X1 + 2 * Gap && tag.Y + tag.Y1 > b.Y0 - 2 * Gap && tag.Y + tag.Y0 < b.Y1 + 2 * Gap)) continue;
            if (placed.Any(p => Collides(tag, p))) continue;
            return true;
        }
        return false;
    }

    /// <summary>Whether two words' padded ink boxes overlap (each keeps its padding, as d3-cloud's padded sprites do).</summary>
    public static bool Collides(CloudWord a, CloudWord b) =>
        a.X + a.X1 > b.X + b.X0 && a.X + a.X0 < b.X + b.X1 && a.Y + a.Y1 > b.Y + b.Y0 && a.Y + a.Y0 < b.Y + b.Y1;
}

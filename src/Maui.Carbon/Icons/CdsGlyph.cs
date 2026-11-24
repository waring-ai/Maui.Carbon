namespace Maui.Carbon;

/// <summary>One path of a glyph: SVG path data using only absolute <c>M</c>, <c>L</c>, <c>C</c> and <c>Z</c>.</summary>
/// <param name="Data">The path data.</param>
/// <param name="EvenOdd">True to fill with the even-odd rule (SVG <c>fill-rule="evenodd"</c>).</param>
public readonly record struct CdsGlyphPath(string Data, bool EvenOdd = false);

/// <summary>A Carbon icon drawn from one source artboard (16, 20, 24 or 32 units).</summary>
public sealed class CdsGlyph
{
    /// <summary>Creates a glyph.</summary>
    /// <param name="name">Carbon icon stem, e.g. <c>arrow--right</c>.</param>
    /// <param name="artboardSize">Size of the source artboard (16, 20, 24 or 32).</param>
    /// <param name="viewBox">The region of the artboard to draw. 32-unit sources whose art sits in the centre 24 are cropped to (4, 4, 24, 24); <c>add</c> and <c>subtract</c> use (3, 3, 26, 26), and <c>close</c> and the checkbox glyphs keep the full 32 box, matching Carbon's 16px glyphs.</param>
    /// <param name="paths">Filled paths, drawn in the icon color.</param>
    /// <param name="innerPaths">Inner paths (Carbon's <c>data-icon-path="inner-path"</c>), drawn in the inner color.</param>
    public CdsGlyph(string name, int artboardSize, RectF viewBox, IReadOnlyList<CdsGlyphPath> paths, IReadOnlyList<CdsGlyphPath> innerPaths)
    {
        Name = name;
        ArtboardSize = artboardSize;
        ViewBox = viewBox;
        Paths = paths;
        InnerPaths = innerPaths;
    }

    /// <summary>Carbon icon stem, e.g. <c>arrow--right</c>.</summary>
    public string Name { get; }

    /// <summary>Size of the source artboard.</summary>
    public int ArtboardSize { get; }

    /// <summary>The drawn region of the artboard.</summary>
    public RectF ViewBox { get; }

    /// <summary>Paths filled with the icon color.</summary>
    public IReadOnlyList<CdsGlyphPath> Paths { get; }

    /// <summary>Paths filled with the inner color (transparent by default).</summary>
    public IReadOnlyList<CdsGlyphPath> InnerPaths { get; }
}

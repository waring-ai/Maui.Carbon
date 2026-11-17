namespace Maui.Carbon;

/// <summary>The interaction state of one slider thumb.</summary>
public enum CdsSliderThumbState
{
    /// <summary>At rest.</summary>
    Normal,
    /// <summary>Pointer over the thumb (a round thumb scales up 1.4286).</summary>
    Hover,
    /// <summary>Keyboard focus: the thumb becomes <c>interactive</c> with a gap ring.</summary>
    Focus,
    /// <summary>Being dragged: <c>interactive</c> inner ring.</summary>
    Pressed,
}

/// <summary>Color keys of a slider's drawn parts. <c>null</c> means not drawn.</summary>
/// <param name="Track">The 2px track.</param>
/// <param name="Fill">The filled part of the track.</param>
/// <param name="Thumb">The thumb (the bracket glyph of a range handle).</param>
/// <param name="Ring">An inner ring: <c>interactive</c> while pressed.</param>
/// <param name="Gap">The 1px gap ring between thumb edge and focus fill (<c>layer-01</c>).</param>
/// <param name="RangeLabel">Color of the min / max labels.</param>
public readonly record struct CdsSliderVisuals(string Track, string Fill, string Thumb, string? Ring, string? Gap, string RangeLabel);

/// <summary>Maps a slider state to Carbon tokens (from <c>packages/styles/scss/components/slider</c>).</summary>
public static class CdsSliderTokens
{
    /// <summary>
    /// Resolves the colors of the track and the thumb in <paramref name="thumb"/> state. Disabled paints everything
    /// <c>border-disabled</c>; a focused thumb turns the fill <c>border-interactive</c>.
    /// </summary>
    public static CdsSliderVisuals Resolve(CdsInteractionState state, CdsSliderThumbState thumb, bool isRange)
    {
        if (state.HasFlag(CdsInteractionState.Disabled))
            return new(CdsColorKeys.BorderDisabled, CdsColorKeys.BorderDisabled, CdsColorKeys.BorderDisabled, null, null, CdsColorKeys.TextDisabled);

        var fill = thumb == CdsSliderThumbState.Focus ? CdsColorKeys.BorderInteractive : CdsColorKeys.LayerSelectedInverse;
        if (isRange)
        {
            var color = thumb switch
            {
                CdsSliderThumbState.Focus => CdsColorKeys.Interactive,
                CdsSliderThumbState.Hover => CdsColorKeys.TextSecondary,
                _ => CdsColorKeys.LayerSelectedInverse,
            };
            return new(CdsColorKeys.BorderSubtle01, fill, color, null, null, CdsColorKeys.TextPrimary);
        }
        return thumb switch
        {
            CdsSliderThumbState.Focus => new(CdsColorKeys.BorderSubtle01, fill, CdsColorKeys.Interactive, null, CdsColorKeys.Layer01, CdsColorKeys.TextPrimary),
            CdsSliderThumbState.Pressed => new(CdsColorKeys.BorderSubtle01, fill, CdsColorKeys.LayerSelectedInverse, CdsColorKeys.Interactive, null, CdsColorKeys.TextPrimary),
            _ => new(CdsColorKeys.BorderSubtle01, fill, CdsColorKeys.LayerSelectedInverse, null, null, CdsColorKeys.TextPrimary),
        };
    }
}

/// <summary>Draws the slider's track, filled track and thumbs. Colors are bindable so they follow tokens.</summary>
internal sealed class CdsSliderGlyph : CdsGlyphView
{
    /// <summary>Horizontal room on each side of the track for thumbs that overhang the ends.</summary>
    public const float PadX = 16;

    public static readonly BindableProperty TrackColorProperty = BindableProperty.Create(nameof(TrackColor), typeof(Color), typeof(CdsSliderGlyph), null, propertyChanged: Redraw);
    public static readonly BindableProperty FillColorProperty = BindableProperty.Create(nameof(FillColor), typeof(Color), typeof(CdsSliderGlyph), null, propertyChanged: Redraw);
    public static readonly BindableProperty ThumbColorProperty = BindableProperty.Create(nameof(ThumbColor), typeof(Color), typeof(CdsSliderGlyph), null, propertyChanged: Redraw);
    public static readonly BindableProperty RingColorProperty = BindableProperty.Create(nameof(RingColor), typeof(Color), typeof(CdsSliderGlyph), null, propertyChanged: Redraw);
    public static readonly BindableProperty GapColorProperty = BindableProperty.Create(nameof(GapColor), typeof(Color), typeof(CdsSliderGlyph), null, propertyChanged: Redraw);

    double _lower;
    double _upper = 1;
    bool _range;
    bool _showThumbs = true;
    SliderHandle _activeHandle;
    CdsSliderThumbState _activeState;

    public Color? TrackColor { get => (Color?)GetValue(TrackColorProperty); set => SetValue(TrackColorProperty, value); }
    public Color? FillColor { get => (Color?)GetValue(FillColorProperty); set => SetValue(FillColorProperty, value); }
    public Color? ThumbColor { get => (Color?)GetValue(ThumbColorProperty); set => SetValue(ThumbColorProperty, value); }
    public Color? RingColor { get => (Color?)GetValue(RingColorProperty); set => SetValue(RingColorProperty, value); }
    public Color? GapColor { get => (Color?)GetValue(GapColorProperty); set => SetValue(GapColorProperty, value); }

    /// <summary>The track length: the view width minus the padding on both sides.</summary>
    public double TrackLength => Math.Max(0, Width - 2 * PadX);

    public void Set(double lowerFraction, double upperFraction, bool range, bool showThumbs, SliderHandle activeHandle, CdsSliderThumbState activeState)
    {
        _lower = lowerFraction;
        _upper = upperFraction;
        _range = range;
        _showThumbs = showThumbs;
        _activeHandle = activeHandle;
        _activeState = activeState;
        Invalidate();
    }

    public override void Draw(ICanvas canvas, RectF dirtyRect)
    {
        var length = (float)TrackLength;
        if (length <= 0) return;
        var cy = (float)Height / 2;
        var lowerX = PadX + (float)(_range ? _lower : 0) * length;
        var upperX = PadX + (float)(_range ? _upper : _lower) * length;

        if (Visible(TrackColor))
        {
            canvas.FillColor = TrackColor!;
            canvas.FillRectangle(PadX, cy - 1, length, 2);
        }
        if (Visible(FillColor))
        {
            canvas.FillColor = FillColor!;
            canvas.FillRectangle(lowerX, cy - 1, Math.Max(0, upperX - lowerX), 2);
        }
        if (!_showThumbs || !Visible(ThumbColor)) return;

        if (_range)
        {
            DrawHandle(canvas, lowerX, cy, lower: true, framed: _activeHandle == SliderHandle.Lower && _activeState == CdsSliderThumbState.Focus);
            DrawHandle(canvas, upperX, cy, lower: false, framed: _activeHandle == SliderHandle.Upper && _activeState == CdsSliderThumbState.Focus);
        }
        else
        {
            var radius = _activeState == CdsSliderThumbState.Normal ? 7f : 10f;
            canvas.FillColor = ThumbColor!;
            canvas.FillCircle(upperX, cy, radius);
            if (Visible(GapColor))
            {
                canvas.StrokeColor = GapColor!;
                canvas.StrokeSize = 1;
                canvas.DrawCircle(upperX, cy, radius - 2.5f);
            }
            if (Visible(RingColor))
            {
                canvas.StrokeColor = RingColor!;
                canvas.StrokeSize = 2;
                canvas.DrawCircle(upperX, cy, radius - 1);
            }
        }
    }

    // A range handle is a bracket: a 2px bar at the inner edge, short caps and an arrow pointing outward.
    // Its color comes from ThumbColor (text-secondary on hover, interactive when focused, where it also gets a frame).
    void DrawHandle(ICanvas canvas, float x, float cy, bool lower, bool framed)
    {
        var dir = lower ? -1f : 1f;
        canvas.FillColor = ThumbColor!;
        var barX = lower ? x - 2 : x;
        canvas.FillRectangle(barX, cy - 12, 2, 24);
        canvas.FillRectangle(lower ? x - 8 : x, cy - 12, 8, 2);
        canvas.FillRectangle(lower ? x - 8 : x, cy + 10, 8, 2);
        var arrow = new PathF();
        arrow.MoveTo(x + dir * 12, cy);
        arrow.LineTo(x + dir * 6, cy - 4);
        arrow.LineTo(x + dir * 6, cy + 4);
        arrow.Close();
        canvas.FillPath(arrow);
        if (framed)
        {
            canvas.StrokeColor = ThumbColor!;
            canvas.StrokeSize = 1;
            canvas.DrawRectangle(lower ? x - 16 + 0.5f : x + 0.5f, cy - 12 + 0.5f, 15, 23);
        }
    }
}

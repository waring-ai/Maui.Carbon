namespace Maui.Carbon;

/// <summary>The size of a <see cref="CdsToggle"/>.</summary>
public enum CdsToggleSize
{
    /// <summary>48x24 track with an 18px knob (default).</summary>
    Md,
    /// <summary>32x16 track with a 10px knob and a check mark in the on state.</summary>
    Sm,
}

/// <summary>Color keys of a toggle in one state. <c>null</c> means transparent (or not drawn).</summary>
/// <param name="Track">Track fill.</param>
/// <param name="TrackBorder">1px track outline (read-only only).</param>
/// <param name="Knob">Knob fill.</param>
/// <param name="Check">Check mark of the sm on state, or <c>null</c> for none.</param>
/// <param name="StateText">Color of the state text beside the switch.</param>
/// <param name="LabelText">Color of the label above the switch.</param>
public readonly record struct CdsToggleVisuals(string? Track, string? TrackBorder, string Knob, string? Check, string StateText, string LabelText);

/// <summary>Maps a toggle state to Carbon tokens (from <c>packages/styles/scss/components/toggle</c>).</summary>
public static class CdsToggleTokens
{
    /// <summary>
    /// Resolves the colors. Flags that matter: <see cref="CdsInteractionState.Disabled"/> and
    /// <see cref="CdsInteractionState.ReadOnly"/> (outlined track, <c>text-primary</c> knob, no check mark).
    /// </summary>
    public static CdsToggleVisuals Resolve(CdsInteractionState state, bool isToggled)
    {
        if (state.HasFlag(CdsInteractionState.Disabled))
            return new(CdsColorKeys.ButtonDisabled, null, CdsColorKeys.TextOnColorDisabled, CdsColorKeys.ButtonDisabled, CdsColorKeys.TextDisabled, CdsColorKeys.TextDisabled);
        if (state.HasFlag(CdsInteractionState.ReadOnly))
            return new(null, CdsColorKeys.IconDisabled, CdsColorKeys.TextPrimary, null, CdsColorKeys.TextPrimary, CdsColorKeys.TextSecondary);
        return new(isToggled ? CdsColorKeys.SupportSuccess : CdsColorKeys.ToggleOff, null, CdsColorKeys.IconOnColor,
            CdsColorKeys.SupportSuccess, CdsColorKeys.TextPrimary, CdsColorKeys.TextSecondary);
    }
}

/// <summary>The toggle track and knob. The knob position is <see cref="Progress"/> (0 off, 1 on) so it can animate.</summary>
internal sealed class CdsToggleGlyph : CdsGlyphView
{
    public static readonly BindableProperty TrackColorProperty = BindableProperty.Create(nameof(TrackColor), typeof(Color), typeof(CdsToggleGlyph), null, propertyChanged: Redraw);
    public static readonly BindableProperty TrackBorderColorProperty = BindableProperty.Create(nameof(TrackBorderColor), typeof(Color), typeof(CdsToggleGlyph), null, propertyChanged: Redraw);
    public static readonly BindableProperty KnobColorProperty = BindableProperty.Create(nameof(KnobColor), typeof(Color), typeof(CdsToggleGlyph), null, propertyChanged: Redraw);
    public static readonly BindableProperty CheckColorProperty = BindableProperty.Create(nameof(CheckColor), typeof(Color), typeof(CdsToggleGlyph), null, propertyChanged: Redraw);

    double _progress;
    bool _small;

    public CdsToggleGlyph()
    {
        HorizontalOptions = LayoutOptions.Start;
        VerticalOptions = LayoutOptions.Start;
        Apply(false);
    }

    public Color? TrackColor { get => (Color?)GetValue(TrackColorProperty); set => SetValue(TrackColorProperty, value); }
    public Color? TrackBorderColor { get => (Color?)GetValue(TrackBorderColorProperty); set => SetValue(TrackBorderColorProperty, value); }
    public Color? KnobColor { get => (Color?)GetValue(KnobColorProperty); set => SetValue(KnobColorProperty, value); }
    public Color? CheckColor { get => (Color?)GetValue(CheckColorProperty); set => SetValue(CheckColorProperty, value); }

    /// <summary>Knob position: 0 off, 1 on.</summary>
    public double Progress
    {
        get => _progress;
        set
        {
            _progress = Math.Clamp(value, 0, 1);
            Invalidate();
        }
    }

    public double TrackWidth => _small ? 32 : 48;

    public double TrackHeight => _small ? 16 : 24;

    public void Apply(bool small)
    {
        _small = small;
        WidthRequest = TrackWidth;
        HeightRequest = TrackHeight;
        Invalidate();
    }

    public override void Draw(ICanvas canvas, RectF dirtyRect)
    {
        var w = (float)TrackWidth;
        var h = (float)TrackHeight;
        var knob = _small ? 10f : 18f;
        if (Visible(TrackColor))
        {
            canvas.FillColor = TrackColor!;
            canvas.FillRoundedRectangle(0, 0, w, h, h / 2);
        }
        if (Visible(TrackBorderColor))
        {
            canvas.StrokeColor = TrackBorderColor!;
            canvas.StrokeSize = 1;
            canvas.DrawRoundedRectangle(0.5f, 0.5f, w - 1, h - 1, h / 2 - 0.5f);
        }
        var x = 3 + (float)_progress * (w - 6 - knob);
        var y = (h - knob) / 2;
        if (Visible(KnobColor))
        {
            canvas.FillColor = KnobColor!;
            if (_small && _progress >= 1 && Visible(CheckColor))
            {
                // The check (6x5, centered on the knob) is cut out of the knob so the track shows through. The CSS fills
                // it with the track's color (support-success, or button-disabled): the same picture in opaque themes, and
                // still visible where the disabled tokens are translucent (g90, g100), where a drawn check blends into the knob.
                var cx = x + knob / 2;
                var cy = y + knob / 2 + 0.25f;
                var path = new PathF();
                path.AppendEllipse(x, y, knob, knob);
                AddSegment(path, cx - 3, cy, cx - 1, cy + 2.25f);
                AddSegment(path, cx - 1, cy + 2.25f, cx + 3, cy - 2.25f);
                canvas.FillPath(path, WindingMode.EvenOdd);
            }
            else
            {
                canvas.FillCircle(x + knob / 2, y + knob / 2, knob / 2);
            }
        }
    }

    // A closed 1.2px wide quad along a line segment (one subpath, so the even-odd fill cuts it out of the knob).
    static void AddSegment(PathF path, float x1, float y1, float x2, float y2)
    {
        var dx = x2 - x1;
        var dy = y2 - y1;
        var length = MathF.Sqrt(dx * dx + dy * dy);
        var nx = -dy / length * 0.6f;
        var ny = dx / length * 0.6f;
        path.MoveTo(x1 + nx, y1 + ny);
        path.LineTo(x2 + nx, y2 + ny);
        path.LineTo(x2 - nx, y2 - ny);
        path.LineTo(x1 - nx, y1 - ny);
        path.Close();
    }
}

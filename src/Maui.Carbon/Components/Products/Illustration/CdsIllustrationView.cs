using Microsoft.Maui.Graphics;

namespace Maui.Carbon;

/// <summary>
/// Draws a ported illustration (<see cref="IllustrationDrawing"/>) scaled to fit its bounds. It picks the dark variant from
/// <see cref="Theme"/> and the theme zone it sits in, and paints theme shapes with <see cref="ThemeFillKey"/> and
/// <see cref="ThemeStrokeKey"/>. Decorative: it stays out of the accessibility tree unless it gets a description.
/// </summary>
internal sealed class CdsIllustrationView : GraphicsView, IDrawable
{
    static readonly BindableProperty ContextThemeProperty = BindableProperty.Create(
        "ContextTheme", typeof(CdsThemeName), typeof(CdsIllustrationView), CdsThemeName.White,
        propertyChanged: static (b, _, _) => ((CdsIllustrationView)b).OnArtChanged());

    internal static readonly BindableProperty ThemeFillProperty = BindableProperty.Create(
        "ThemeFill", typeof(Color), typeof(CdsIllustrationView), null, propertyChanged: Redraw);

    internal static readonly BindableProperty ThemeStrokeProperty = BindableProperty.Create(
        "ThemeStroke", typeof(Color), typeof(CdsIllustrationView), null, propertyChanged: Redraw);

    IllustrationDrawing? _light;
    IllustrationDrawing? _dark;
    CdsIllustrationTheme _theme;
    IllustrationAlignment _alignment;
    bool _sizesToWidth;
    IllustrationDrawing? _current;

    public CdsIllustrationView()
    {
        Drawable = new CdsWeakDrawable(this, nameof(CdsIllustrationView));
        InputTransparent = true;
        BackgroundColor = Colors.Transparent;
        AutomationProperties.SetIsInAccessibleTree(this, false);
        SetDynamicResource(ContextThemeProperty, CdsContextKeys.ThemeId);
        CdsTokens.SetColor(this, ThemeFillProperty, CdsColorKeys.Background);
        CdsTokens.SetColor(this, ThemeStrokeProperty, CdsColorKeys.BorderInverse);
    }

    /// <summary>The light (or only) drawing.</summary>
    public IllustrationDrawing? Light
    {
        get => _light;
        set
        {
            if (_light == value) return;
            _light = value;
            OnArtChanged();
        }
    }

    /// <summary>The dark drawing, if the artwork has one.</summary>
    public IllustrationDrawing? Dark
    {
        get => _dark;
        set
        {
            if (_dark == value) return;
            _dark = value;
            OnArtChanged();
        }
    }

    /// <summary>Which variant to draw. Default <see cref="CdsIllustrationTheme.Auto"/>: dark in g90 and g100 zones.</summary>
    public CdsIllustrationTheme Theme
    {
        get => _theme;
        set
        {
            if (_theme == value) return;
            _theme = value;
            OnArtChanged();
        }
    }

    /// <summary>Where the artwork sits when the box is not its shape.</summary>
    public IllustrationAlignment Alignment
    {
        get => _alignment;
        set
        {
            if (_alignment == value) return;
            _alignment = value;
            Invalidate();
        }
    }

    /// <summary>Measures as tall as the artwork is at the offered width (SVG <c>inline-size: 100%; block-size: auto</c>).</summary>
    public bool SizesToWidth
    {
        get => _sizesToWidth;
        set
        {
            if (_sizesToWidth == value) return;
            _sizesToWidth = value;
            InvalidateMeasure();
        }
    }

    /// <summary>The token that paints <see cref="IllustrationPaintKind.ThemeFill"/> shapes. Default <c>background</c>.</summary>
    public string ThemeFillKey
    {
        set => CdsTokens.SetColor(this, ThemeFillProperty, value);
    }

    /// <summary>The token that paints <see cref="IllustrationPaintKind.ThemeStroke"/> shapes. Default <c>border-inverse</c>.</summary>
    public string ThemeStrokeKey
    {
        set => CdsTokens.SetColor(this, ThemeStrokeProperty, value);
    }

    /// <summary>The drawing shown now.</summary>
    internal IllustrationDrawing? Current => _current;

    /// <summary>Names the illustration for screen readers; null or empty keeps it decorative.</summary>
    internal void SetDescription(string? description)
    {
        var named = !string.IsNullOrWhiteSpace(description);
        AutomationProperties.SetIsInAccessibleTree(this, named);
        SemanticProperties.SetDescription(this, named ? description : null);
    }

    /// <inheritdoc/>
    protected override Size MeasureOverride(double widthConstraint, double heightConstraint)
    {
        if (_sizesToWidth && _current is { } art && double.IsFinite(widthConstraint) && widthConstraint > 0)
        {
            var width = WidthRequest >= 0 ? Math.Min(WidthRequest, widthConstraint) : widthConstraint;
            var height = IllustrationModel.HeightForWidth(art, width);
            if (double.IsFinite(heightConstraint) && height > heightConstraint)
            {
                height = heightConstraint;
                width = height * art.Width / art.Height;
            }
            return new Size(width, height);
        }
        return base.MeasureOverride(widthConstraint, heightConstraint);
    }

    void OnArtChanged()
    {
        var next = IllustrationModel.Pick(_light, _dark, _theme, (CdsThemeName)GetValue(ContextThemeProperty));
        if (next == _current) return;
        var resized = next is null || _current is null || next.Width != _current.Width || next.Height != _current.Height;
        _current = next;
        if (resized && _sizesToWidth) InvalidateMeasure();
        Invalidate();
    }

    void IDrawable.Draw(ICanvas canvas, RectF dirtyRect)
    {
        if (_current is not { } art) return;
        var bounds = new RectF(0, 0, (float)Width, (float)Height);
        Render(canvas, bounds, art, _alignment, (Color?)GetValue(ThemeFillProperty), (Color?)GetValue(ThemeStrokeProperty));
    }

    /// <summary>Paints <paramref name="art"/> into <paramref name="bounds"/>.</summary>
    internal static void Render(ICanvas canvas, RectF bounds, IllustrationDrawing art, IllustrationAlignment alignment, Color? themeFill, Color? themeStroke)
    {
        var fit = IllustrationModel.Fit(art.Width, art.Height, bounds.Width, bounds.Height, alignment);
        if (fit.Scale <= 0) return;
        canvas.SaveState();
        canvas.Translate(bounds.X + fit.OffsetX, bounds.Y + fit.OffsetY);
        canvas.Scale(fit.Scale, fit.Scale);
        canvas.ClipRectangle(0, 0, art.Width, art.Height);
        for (var i = 0; i < art.Shapes.Count; i++)
        {
            var shape = art.Shapes[i];
            var clip = art.ClipAt(i);
            canvas.SaveState();
            if (clip is not null) canvas.ClipPath(clip, WindingMode.NonZero);
            canvas.Alpha = shape.Opacity;
            var path = art.PathAt(i);
            if (shape.Fill is { } fill && SetFill(canvas, fill, art, themeFill, themeStroke))
                canvas.FillPath(path, WindingMode.NonZero);
            if (shape.Stroke is { } stroke && StrokeColor(stroke, themeFill, themeStroke) is { } strokeColor)
            {
                canvas.StrokeColor = strokeColor;
                canvas.StrokeSize = shape.StrokeWidth;
                canvas.StrokeLineCap = shape.Cap;
                canvas.StrokeLineJoin = shape.Join;
                canvas.MiterLimit = shape.MiterLimit;
                if (shape.Dash is { } dash) canvas.StrokeDashPattern = dash;
                canvas.DrawPath(path);
            }
            canvas.RestoreState();
        }
        canvas.RestoreState();
    }

    static bool SetFill(ICanvas canvas, IllustrationPaint paint, IllustrationDrawing art, Color? themeFill, Color? themeStroke)
    {
        switch (paint.Kind)
        {
            case IllustrationPaintKind.Linear:
                canvas.SetFillPaint(paint.Gradient(art.Width, art.Height), new RectF(0, 0, art.Width, art.Height));
                return true;
            default:
                if (StrokeColor(paint, themeFill, themeStroke) is not { } color) return false;
                canvas.FillColor = color;
                return true;
        }
    }

    static Color? StrokeColor(IllustrationPaint paint, Color? themeFill, Color? themeStroke) => paint.Kind switch
    {
        IllustrationPaintKind.Solid => IllustrationPalette.ToColor(paint.Rgb, paint.Alpha),
        IllustrationPaintKind.ThemeFill => themeFill,
        IllustrationPaintKind.ThemeStroke => themeStroke,
        _ => null,
    };

    static void Redraw(BindableObject b, object? o, object? n) => ((CdsIllustrationView)b).Invalidate();
}

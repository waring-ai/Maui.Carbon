using Microsoft.Maui.Graphics;

namespace Maui.Carbon;

/// <summary>Geometry of the Carbon spinner (pure, tested).</summary>
internal static class SpinnerModel
{
    /// <summary>One rotation takes 690ms.</summary>
    public const uint RotationMilliseconds = 690;

    /// <summary>The ring radius in the 100-unit artboard.</summary>
    public const double Radius = 44;

    /// <summary>Stroke width in artboard units: 10 for the large spinner, 16 for the small one.</summary>
    public static double StrokeWidth(bool small) => small ? 16 : 10;

    /// <summary>Fraction of the ring that is drawn: 81% (large) or 48% (small).</summary>
    public static double ArcFraction(bool small)
    {
        var circumference = 2 * Math.PI * Radius;
        var offset = small ? 143.7596 : 52.5276;
        return (circumference - offset) / circumference;
    }

    /// <summary>Rotation in degrees for an animation phase (0 to 1).</summary>
    public static double Rotation(double phase) => 360 * phase;
}

/// <summary>The spinner: a ring arc in <c>interactive</c> that rotates continuously (static under reduced motion).</summary>
internal sealed class CdsSpinnerView : GraphicsView, IDrawable
{
    const string AnimationName = "cds-spin";

    public static readonly BindableProperty IsSmallProperty = BindableProperty.Create(
        nameof(IsSmall), typeof(bool), typeof(CdsSpinnerView), false,
        propertyChanged: static (b, _, _) => ((CdsSpinnerView)b).ApplySize());

    public static readonly BindableProperty ArcColorProperty = BindableProperty.Create(
        nameof(ArcColor), typeof(Color), typeof(CdsSpinnerView), null, propertyChanged: Redraw);

    public static readonly BindableProperty TrackColorProperty = BindableProperty.Create(
        nameof(TrackColor), typeof(Color), typeof(CdsSpinnerView), null, propertyChanged: Redraw);

    bool _spinning;

    public CdsSpinnerView()
    {
        Drawable = new CdsWeakDrawable(this);
        InputTransparent = true;
        BackgroundColor = Colors.Transparent;
        HorizontalOptions = LayoutOptions.Center;
        VerticalOptions = LayoutOptions.Center;
        CdsTokens.SetColor(this, ArcColorProperty, CdsColorKeys.Interactive);
        ApplySize();
        Loaded += (_, _) => UpdateAnimation();
        Unloaded += (_, _) => CdsLoop.Stop(this, AnimationName);
    }

    public bool IsSmall
    {
        get => (bool)GetValue(IsSmallProperty);
        set => SetValue(IsSmallProperty, value);
    }

    public Color? ArcColor
    {
        get => (Color?)GetValue(ArcColorProperty);
        set => SetValue(ArcColorProperty, value);
    }

    public Color? TrackColor
    {
        get => (Color?)GetValue(TrackColorProperty);
        set => SetValue(TrackColorProperty, value);
    }

    /// <summary>Whether the spinner rotates. Stopping fades the arc out.</summary>
    public bool IsSpinning
    {
        get => _spinning;
        set
        {
            if (_spinning == value) return;
            _spinning = value;
            UpdateAnimation();
        }
    }

    void UpdateAnimation()
    {
        if (_spinning)
        {
            IsVisible = true;
            Opacity = 1;
            if (IsLoaded) CdsLoop.Start(this, AnimationName, phase => Rotation = SpinnerModel.Rotation(phase), 0, SpinnerModel.RotationMilliseconds);
        }
        else
        {
            CdsLoop.Stop(this, AnimationName);
            Rotation = 0;
            if (IsLoaded && !CdsMotion.IsReducedMotion) _ = FadeOutAsync();
            else Opacity = 0;
        }
    }

    async Task FadeOutAsync()
    {
        await this.FadeToAsync(0, CdsDuration.Slow01, CdsEasing.ExitProductive);
        if (!_spinning) Opacity = 0;
    }

    void ApplySize()
    {
        var size = IsSmall ? 16 : 88;
        WidthRequest = size;
        HeightRequest = size;
        Invalidate();
    }

    void IDrawable.Draw(ICanvas canvas, RectF dirtyRect)
    {
        var size = (float)Math.Min(Width > 0 ? Width : WidthRequest, Height > 0 ? Height : HeightRequest);
        if (size <= 0 || ArcColor is not { } arc) return;
        var scale = size / 100f;
        var stroke = (float)SpinnerModel.StrokeWidth(IsSmall);
        var radius = (float)SpinnerModel.Radius;
        canvas.SaveState();
        canvas.Scale(scale, scale);
        canvas.StrokeSize = stroke;
        canvas.StrokeLineCap = LineCap.Butt;
        if (IsSmall && TrackColor is { } track)
        {
            canvas.StrokeColor = track;
            canvas.DrawCircle(50, 50, radius);
        }
        canvas.StrokeColor = arc;
        var sweep = (float)(360 * SpinnerModel.ArcFraction(IsSmall));
        // Start at 3 o'clock and sweep clockwise, as the SVG circle does (a polyline keeps the arc exact on every platform).
        var path = new PathF();
        const float step = 4f;
        for (var a = 0f; a <= sweep + 0.01f; a += step)
        {
            var angle = Math.Min(a, sweep) * MathF.PI / 180;
            var point = new PointF(50 + radius * MathF.Cos(angle), 50 + radius * MathF.Sin(angle));
            if (a == 0) path.MoveTo(point);
            else path.LineTo(point);
        }
        canvas.DrawPath(path);
        canvas.RestoreState();
    }

    static void Redraw(BindableObject b, object? o, object? n) => ((CdsSpinnerView)b).Invalidate();
}

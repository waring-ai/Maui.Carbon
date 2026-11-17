namespace Maui.Carbon;

/// <summary>
/// The small upload spinner of a file row: a <c>layer-accent-01</c> track and a three-quarter <c>icon-primary</c> arc
/// that turns once every 690 ms (with reduced motion it stands still).
/// </summary>
internal sealed class CdsFileSpinner : GraphicsView, IDrawable
{
    static readonly BindableProperty TrackColorProperty = BindableProperty.Create(
        "TrackColor", typeof(Color), typeof(CdsFileSpinner), null,
        propertyChanged: static (b, _, _) => ((CdsFileSpinner)b).Invalidate());

    static readonly BindableProperty ArcColorProperty = BindableProperty.Create(
        "ArcColor", typeof(Color), typeof(CdsFileSpinner), null,
        propertyChanged: static (b, _, _) => ((CdsFileSpinner)b).Invalidate());

    bool _running;

    public CdsFileSpinner()
    {
        Drawable = this;
        WidthRequest = 16;
        HeightRequest = 16;
        InputTransparent = true;
        BackgroundColor = Colors.Transparent;
        VerticalOptions = LayoutOptions.Center;
        HorizontalOptions = LayoutOptions.Center;
        SetDynamicResource(TrackColorProperty, CdsColorKeys.LayerAccent01);
        SetDynamicResource(ArcColorProperty, CdsColorKeys.IconPrimary);
        AutomationProperties.SetIsInAccessibleTree(this, true);
        SemanticProperties.SetDescription(this, "Uploading");
        Loaded += (_, _) => Start();
        Unloaded += (_, _) => _running = false;
    }

    void IDrawable.Draw(ICanvas canvas, RectF dirtyRect)
    {
        if (TrackColor() is not { } track || (Color)GetValue(ArcColorProperty) is not { } arc) return;
        var size = Math.Min(dirtyRect.Width, dirtyRect.Height);
        var stroke = size * 0.14f;
        var inset = stroke / 2;
        var rect = new RectF(dirtyRect.X + inset, dirtyRect.Y + inset, size - stroke, size - stroke);
        canvas.StrokeSize = stroke;
        canvas.StrokeColor = track;
        canvas.DrawEllipse(rect);
        canvas.StrokeColor = arc;
        canvas.StrokeLineCap = LineCap.Butt;
        canvas.DrawArc(rect, 0, 90, true, false);
    }

    Color? TrackColor() => (Color?)GetValue(TrackColorProperty);

    public void Start()
    {
        if (_running) return;
        _running = true;
        _ = SpinAsync();
    }

    async Task SpinAsync()
    {
        try
        {
            while (_running && Handler is not null && IsVisible && !CdsMotion.IsReducedMotion)
                await CdsMotion.AnimateAsync(this, "FileSpin", v => Rotation = v, 0, 360, 690, Easing.Linear);
        }
        finally
        {
            _running = false;
        }
    }
}

namespace Maui.Carbon;

/// <summary>
/// Draws a Carbon icon in a single color (default <c>icon-primary</c>). Icons are decorative unless you set
/// <see cref="Description"/>; status icons always sit next to a text label.
/// </summary>
public class CdsIcon : GraphicsView, IDrawable
{
    /// <summary>Bindable property for <see cref="Icon"/>.</summary>
    public static readonly BindableProperty IconProperty = BindableProperty.Create(
        nameof(Icon), typeof(CdsIconName?), typeof(CdsIcon), null, propertyChanged: Redraw);

    /// <summary>Bindable property for <see cref="Glyph"/>.</summary>
    public static readonly BindableProperty GlyphProperty = BindableProperty.Create(
        nameof(Glyph), typeof(CdsGlyph), typeof(CdsIcon), null, propertyChanged: Redraw);

    /// <summary>Bindable property for <see cref="Size"/>.</summary>
    public static readonly BindableProperty SizeProperty = BindableProperty.Create(
        nameof(Size), typeof(double), typeof(CdsIcon), 16d,
        propertyChanged: static (b, _, n) => ((CdsIcon)b).ApplySize((double)n));

    /// <summary>Bindable property for <see cref="Color"/>.</summary>
    public static readonly BindableProperty ColorProperty = BindableProperty.Create(
        nameof(Color), typeof(Color), typeof(CdsIcon), null, propertyChanged: Redraw);

    /// <summary>Bindable property for <see cref="InnerColor"/>.</summary>
    public static readonly BindableProperty InnerColorProperty = BindableProperty.Create(
        nameof(InnerColor), typeof(Color), typeof(CdsIcon), null, propertyChanged: Redraw);

    /// <summary>Bindable property for <see cref="Description"/>.</summary>
    public static readonly BindableProperty DescriptionProperty = BindableProperty.Create(
        nameof(Description), typeof(string), typeof(CdsIcon), null,
        propertyChanged: static (b, _, n) => ((CdsIcon)b).ApplyDescription((string?)n));

    /// <summary>Creates a 16-unit icon in <c>icon-primary</c>.</summary>
    public CdsIcon()
    {
        Drawable = new CdsWeakDrawable(this);
        InputTransparent = true;
        BackgroundColor = Colors.Transparent;
        HorizontalOptions = LayoutOptions.Center;
        VerticalOptions = LayoutOptions.Center;
        ApplySize(16);
        SetDynamicResource(ColorProperty, CdsColorKeys.IconPrimary);
        ApplyDescription(null);
    }

    /// <summary>Creates an icon.</summary>
    public CdsIcon(CdsIconName icon, double size = 16) : this()
    {
        Icon = icon;
        Size = size;
    }

    /// <summary>The bundled icon to draw.</summary>
    public CdsIconName? Icon
    {
        get => (CdsIconName?)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>A custom glyph; used when <see cref="Icon"/> is not set.</summary>
    public CdsGlyph? Glyph
    {
        get => (CdsGlyph?)GetValue(GlyphProperty);
        set => SetValue(GlyphProperty, value);
    }

    /// <summary>Drawn size (16, 20, 24 or 32). Default 16. Also sets the requested width and height.</summary>
    public double Size
    {
        get => (double)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    /// <summary>Icon color. Defaults to the <c>icon-primary</c> token; bind another key with <c>SetDynamicResource</c>.</summary>
    public Color? Color
    {
        get => (Color?)GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    /// <summary>Color of a filled status glyph's inner mark (the check in <c>checkmark--filled</c>). Default transparent.</summary>
    public Color? InnerColor
    {
        get => (Color?)GetValue(InnerColorProperty);
        set => SetValue(InnerColorProperty, value);
    }

    /// <summary>Accessible name. When set the icon is exposed to screen readers; otherwise it is decorative.</summary>
    public string? Description
    {
        get => (string?)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    void IDrawable.Draw(ICanvas canvas, RectF dirtyRect)
    {
        var size = (float)Size;
        var glyph = Icon is { } name ? CdsIcons.GetGlyph(name, size) : Glyph;
        if (glyph is null || Color is not { } color) return;

        var bounds = new RectF(0, 0, (float)Width, (float)Height);
        var drawn = Math.Min(size, Math.Min(bounds.Width, bounds.Height));
        if (drawn <= 0) return;
        var scale = drawn / glyph.ViewBox.Width;

        canvas.SaveState();
        canvas.Translate(bounds.Center.X - drawn / 2, bounds.Center.Y - drawn / 2);
        canvas.Scale(scale, scale);
        canvas.Translate(-glyph.ViewBox.X, -glyph.ViewBox.Y);
        canvas.FillColor = color;
        foreach (var p in glyph.Paths)
            canvas.FillPath(CdsIcons.GetPath(p.Data), p.EvenOdd ? WindingMode.EvenOdd : WindingMode.NonZero);
        if (InnerColor is { Alpha: > 0 } inner)
        {
            canvas.FillColor = inner;
            foreach (var p in glyph.InnerPaths)
                canvas.FillPath(CdsIcons.GetPath(p.Data), p.EvenOdd ? WindingMode.EvenOdd : WindingMode.NonZero);
        }
        canvas.RestoreState();
    }

    static void Redraw(BindableObject b, object? o, object? n) => ((CdsIcon)b).Invalidate();

    void ApplySize(double size)
    {
        WidthRequest = size;
        HeightRequest = size;
        Invalidate();
    }

    void ApplyDescription(string? description)
    {
        AutomationProperties.SetIsInAccessibleTree(this, !string.IsNullOrEmpty(description));
        SemanticProperties.SetDescription(this, description);
    }
}

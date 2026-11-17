namespace Maui.Carbon;

/// <summary>The kinds a <see cref="CdsShapeIndicator"/> can show: severity and lifecycle.</summary>
public enum CdsShapeIndicatorKind
{
    /// <summary>Failed: <c>status-red</c>, octagon with an x.</summary>
    Failed,
    /// <summary>Critical: <c>status-red</c>, hexagon.</summary>
    Critical,
    /// <summary>High: <c>status-red</c>, triangle.</summary>
    High,
    /// <summary>Medium: <c>status-orange</c> with a <c>status-orange-outline</c> border, diamond.</summary>
    Medium,
    /// <summary>Low: <c>status-yellow</c> with a <c>status-yellow-outline</c> border, inverted triangle.</summary>
    Low,
    /// <summary>Cautious: <c>status-yellow</c> with a <c>status-yellow-outline</c> border, triangle.</summary>
    Cautious,
    /// <summary>Undefined: <c>status-purple</c>, diamond.</summary>
    Undefined,
    /// <summary>Stable: <c>status-green</c>, circle.</summary>
    Stable,
    /// <summary>Informative: <c>status-blue</c>, inverted triangle.</summary>
    Informative,
    /// <summary>Incomplete: <c>status-blue</c>, half-filled circle.</summary>
    Incomplete,
    /// <summary>Draft: <c>status-gray</c>, ring.</summary>
    Draft,
}

/// <summary>Color keys of a shape indicator.</summary>
/// <param name="Fill">The shape color.</param>
/// <param name="Outline">The darker outline that keeps orange and yellow shapes at 3:1 contrast; <c>null</c> for none.</param>
public readonly record struct CdsShapeIndicatorVisuals(string Fill, string? Outline);

/// <summary>Maps <see cref="CdsShapeIndicatorKind"/> to its <c>status-*</c> tokens and shape.</summary>
public static class CdsShapeIndicatorTokens
{
    /// <summary>Resolves the colors of a kind.</summary>
    public static CdsShapeIndicatorVisuals Resolve(CdsShapeIndicatorKind kind) => kind switch
    {
        CdsShapeIndicatorKind.Failed or CdsShapeIndicatorKind.Critical or CdsShapeIndicatorKind.High => new(CdsColorKeys.StatusRed, null),
        CdsShapeIndicatorKind.Medium => new(CdsColorKeys.StatusOrange, CdsColorKeys.StatusOrangeOutline),
        CdsShapeIndicatorKind.Low or CdsShapeIndicatorKind.Cautious => new(CdsColorKeys.StatusYellow, CdsColorKeys.StatusYellowOutline),
        CdsShapeIndicatorKind.Undefined => new(CdsColorKeys.StatusPurple, null),
        CdsShapeIndicatorKind.Stable => new(CdsColorKeys.StatusGreen, null),
        CdsShapeIndicatorKind.Draft => new(CdsColorKeys.StatusGray, null),
        _ => new(CdsColorKeys.StatusBlue, null),
    };

    internal static CdsStatusShape Shape(CdsShapeIndicatorKind kind) => kind switch
    {
        CdsShapeIndicatorKind.Failed => CdsStatusShape.OctagonX,
        CdsShapeIndicatorKind.Critical => CdsStatusShape.Hexagon,
        CdsShapeIndicatorKind.High or CdsShapeIndicatorKind.Cautious => CdsStatusShape.TriangleUp,
        CdsShapeIndicatorKind.Medium or CdsShapeIndicatorKind.Undefined => CdsStatusShape.Diamond,
        CdsShapeIndicatorKind.Low or CdsShapeIndicatorKind.Informative => CdsStatusShape.TriangleDown,
        CdsShapeIndicatorKind.Stable => CdsStatusShape.Circle,
        CdsShapeIndicatorKind.Incomplete => CdsStatusShape.HalfFilled,
        _ => CdsStatusShape.Ring,
    };

    /// <summary>The label type style: helper-text-01 (12) or body-compact-01 (14).</summary>
    public static CdsTypeStyle LabelStyle(int textSize) => textSize >= 14 ? CdsTypeStyle.BodyCompact01 : CdsTypeStyle.HelperText01;
}

/// <summary>
/// A small colored shape plus a label, used for severity and lifecycle where the shape distinguishes levels without
/// relying on hue (dense tables, lists, diagrams). The shape is always 16 and decorative; the label is the accessible
/// text and is required. For health that needs a glyph with more room use <see cref="CdsIconIndicator"/>.
/// </summary>
public class CdsShapeIndicator : CdsControl
{
    /// <summary>Bindable property for <see cref="Kind"/>.</summary>
    public static readonly BindableProperty KindProperty = BindableProperty.Create(
        nameof(Kind), typeof(CdsShapeIndicatorKind), typeof(CdsShapeIndicator), CdsShapeIndicatorKind.Stable, propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="Label"/>.</summary>
    public static readonly BindableProperty LabelProperty = BindableProperty.Create(
        nameof(Label), typeof(string), typeof(CdsShapeIndicator), null, propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="TextSize"/>.</summary>
    public static readonly BindableProperty TextSizeProperty = BindableProperty.Create(
        nameof(TextSize), typeof(int), typeof(CdsShapeIndicator), 12,
        coerceValue: static (_, v) => (int)v >= 14 ? 14 : 12, propertyChanged: OnChanged);

    readonly CdsStatusGlyphView _glyph = new();
    readonly CdsText _text = new() { Tone = CdsTextTone.Secondary, VerticalOptions = LayoutOptions.Center };

    /// <summary>Creates a stable indicator with 12px text.</summary>
    public CdsShapeIndicator()
    {
        var row = new HorizontalStackLayout { Spacing = CdsSpacing.Spacing03 };
        row.Add(_glyph);
        row.Add(_text);
        HorizontalOptions = LayoutOptions.Start;
        SetChrome(row);
        Update();
    }

    /// <summary>Creates an indicator.</summary>
    public CdsShapeIndicator(CdsShapeIndicatorKind kind, string label) : this()
    {
        Kind = kind;
        Label = label;
    }

    /// <summary>The severity or lifecycle state. Pair shapes consistently across a product.</summary>
    public CdsShapeIndicatorKind Kind
    {
        get => (CdsShapeIndicatorKind)GetValue(KindProperty);
        set => SetValue(KindProperty, value);
    }

    /// <summary>The label ("Critical"). Required: it is the accessible text.</summary>
    public string? Label
    {
        get => (string?)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <summary>Label size: 12 (default) or 14 to match 14px row text in tables. The shape is always 16.</summary>
    public int TextSize
    {
        get => (int)GetValue(TextSizeProperty);
        set => SetValue(TextSizeProperty, value);
    }

    /// <inheritdoc/>
    protected override void ApplyVisuals()
    {
        var v = CdsShapeIndicatorTokens.Resolve(Kind);
        SetColor(_glyph, CdsStatusGlyphView.FillProperty, v.Fill);
        SetColor(_glyph, CdsStatusGlyphView.OutlineProperty, v.Outline);
    }

    void Update()
    {
        _glyph.Shape = CdsShapeIndicatorTokens.Shape(Kind);
        _text.TypeStyle = CdsShapeIndicatorTokens.LabelStyle(TextSize);
        _text.Text = Label;
        RefreshVisuals();
    }

    static void OnChanged(BindableObject b, object? o, object? n) => ((CdsShapeIndicator)b).Update();
}

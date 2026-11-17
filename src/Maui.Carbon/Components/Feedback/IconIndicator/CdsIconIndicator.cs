namespace Maui.Carbon;

/// <summary>The states an <see cref="CdsIconIndicator"/> can show. Each has its own shape and symbol and a color.</summary>
public enum CdsIconIndicatorKind
{
    /// <summary>Error or danger: <c>status-red</c>, circle with an x.</summary>
    Failed,
    /// <summary>Serious warning: <c>status-orange</c>, inverted triangle with !.</summary>
    CautionMajor,
    /// <summary>Warning: <c>status-yellow</c>, triangle with !.</summary>
    CautionMinor,
    /// <summary>Undefined or outlier: <c>status-purple</c>, diamond with ?.</summary>
    Undefined,
    /// <summary>Success: <c>status-green</c>, circle with a check.</summary>
    Succeeded,
    /// <summary>Normal: <c>status-blue</c>, outlined circle with a check.</summary>
    Normal,
    /// <summary>In progress: <c>status-blue</c>, half-filled circle.</summary>
    InProgress,
    /// <summary>Incomplete: <c>status-blue</c>, half circle with a dashed outline.</summary>
    Incomplete,
    /// <summary>Not started or draft: <c>status-gray</c>, dashed circle.</summary>
    NotStarted,
    /// <summary>Pending: <c>status-gray</c>, circle with three dots.</summary>
    Pending,
    /// <summary>Unknown: <c>status-gray</c>, circle with ?.</summary>
    Unknown,
    /// <summary>Passive information: <c>status-blue</c>, square with !.</summary>
    Informative,
}

/// <summary>Maps <see cref="CdsIconIndicatorKind"/> to its <c>status-*</c> token.</summary>
public static class CdsIconIndicatorTokens
{
    /// <summary>The color key of a kind.</summary>
    public static string Color(CdsIconIndicatorKind kind) => kind switch
    {
        CdsIconIndicatorKind.Failed => CdsColorKeys.StatusRed,
        CdsIconIndicatorKind.CautionMajor => CdsColorKeys.StatusOrange,
        CdsIconIndicatorKind.CautionMinor => CdsColorKeys.StatusYellow,
        CdsIconIndicatorKind.Undefined => CdsColorKeys.StatusPurple,
        CdsIconIndicatorKind.Succeeded => CdsColorKeys.StatusGreen,
        CdsIconIndicatorKind.NotStarted or CdsIconIndicatorKind.Pending or CdsIconIndicatorKind.Unknown => CdsColorKeys.StatusGray,
        _ => CdsColorKeys.StatusBlue,
    };

    internal static CdsStatusShape Shape(CdsIconIndicatorKind kind) => kind switch
    {
        CdsIconIndicatorKind.Failed => CdsStatusShape.CircleX,
        CdsIconIndicatorKind.CautionMajor => CdsStatusShape.TriangleDownBang,
        CdsIconIndicatorKind.CautionMinor => CdsStatusShape.TriangleUpBang,
        CdsIconIndicatorKind.Undefined => CdsStatusShape.DiamondQuestion,
        CdsIconIndicatorKind.Succeeded => CdsStatusShape.CircleCheck,
        CdsIconIndicatorKind.Normal => CdsStatusShape.RingCheck,
        CdsIconIndicatorKind.InProgress => CdsStatusShape.HalfFilledCircle,
        CdsIconIndicatorKind.Incomplete => CdsStatusShape.HalfCircleDashed,
        CdsIconIndicatorKind.NotStarted => CdsStatusShape.DashedCircle,
        CdsIconIndicatorKind.Pending => CdsStatusShape.CircleDots,
        CdsIconIndicatorKind.Unknown => CdsStatusShape.CircleQuestion,
        _ => CdsStatusShape.SquareBang,
    };

    /// <summary>The label type style: body-compact-01 with the 16px glyph, body-compact-02 with the 20px glyph.</summary>
    public static CdsTypeStyle LabelStyle(double size) => size >= 20 ? CdsTypeStyle.BodyCompact02 : CdsTypeStyle.BodyCompact01;
}

/// <summary>
/// A status glyph followed by a text label. The glyph's shape carries the status, so it stays readable without color.
/// Use it for system health and the state of objects or processes (a status column, a task list); for severity in dense
/// lists use <see cref="CdsShapeIndicator"/>. The glyph is decorative and the label is the accessible text, so the
/// label is required. Avoid more than five or six different indicators in one view.
/// </summary>
public class CdsIconIndicator : CdsControl
{
    /// <summary>Bindable property for <see cref="Kind"/>.</summary>
    public static readonly BindableProperty KindProperty = BindableProperty.Create(
        nameof(Kind), typeof(CdsIconIndicatorKind), typeof(CdsIconIndicator), CdsIconIndicatorKind.Normal, propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="Label"/>.</summary>
    public static readonly BindableProperty LabelProperty = BindableProperty.Create(
        nameof(Label), typeof(string), typeof(CdsIconIndicator), null, propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="Size"/>.</summary>
    public static readonly BindableProperty SizeProperty = BindableProperty.Create(
        nameof(Size), typeof(double), typeof(CdsIconIndicator), 16d,
        coerceValue: static (_, v) => (double)v >= 20 ? 20d : 16d, propertyChanged: OnChanged);

    readonly CdsStatusGlyphView _glyph = new();
    readonly CdsText _text = new() { Tone = CdsTextTone.Secondary, VerticalOptions = LayoutOptions.Center };

    /// <summary>Creates a 16px "normal" indicator.</summary>
    public CdsIconIndicator()
    {
        var row = new HorizontalStackLayout { Spacing = CdsSpacing.Spacing03 };
        row.Add(_glyph);
        row.Add(_text);
        HorizontalOptions = LayoutOptions.Start;
        SetChrome(row);
        Update();
    }

    /// <summary>Creates an indicator.</summary>
    public CdsIconIndicator(CdsIconIndicatorKind kind, string label) : this()
    {
        Kind = kind;
        Label = label;
    }

    /// <summary>The state. Default <see cref="CdsIconIndicatorKind.Normal"/>.</summary>
    public CdsIconIndicatorKind Kind
    {
        get => (CdsIconIndicatorKind)GetValue(KindProperty);
        set => SetValue(KindProperty, value);
    }

    /// <summary>The state in the product's words ("Deploying (3 of 5 pods)"). Required: it is the accessible text.</summary>
    public string? Label
    {
        get => (string?)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <summary>The glyph size: 16 (default) or 20 (other values snap to these).</summary>
    public double Size
    {
        get => (double)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    /// <inheritdoc/>
    protected override void ApplyVisuals()
    {
        SetColor(_glyph, CdsStatusGlyphView.FillProperty, CdsIconIndicatorTokens.Color(Kind));
    }

    void Update()
    {
        _glyph.Shape = CdsIconIndicatorTokens.Shape(Kind);
        _glyph.GlyphSize = Size;
        _text.TypeStyle = CdsIconIndicatorTokens.LabelStyle(Size);
        _text.Text = Label;
        RefreshVisuals();
    }

    static void OnChanged(BindableObject b, object? o, object? n) => ((CdsIconIndicator)b).Update();
}

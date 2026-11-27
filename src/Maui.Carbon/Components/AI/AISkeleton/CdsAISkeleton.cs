using Microsoft.Maui.Controls.Shapes;

namespace Maui.Carbon;

/// <summary>The AI skeleton sweep as pure math: a band slides left to right once every 1250ms with an ease-in-out curve.</summary>
internal static class AiShimmerModel
{
    /// <summary>Loop length in milliseconds.</summary>
    public const uint LoopMilliseconds = 1250;

    /// <summary>The phase shown when motion is reduced (the band rests in the middle).</summary>
    public const double RestPhase = 0.5;

    /// <summary>
    /// The band's translation as a fraction of its own width at a phase from 0 to 1: -1 (fully off the start edge) to +1
    /// (fully off the end edge), eased.
    /// </summary>
    public static double Translation(double phase)
    {
        phase -= Math.Floor(phase);
        return -1 + 2 * ShimmerModel.Ease(phase);
    }

    /// <summary>How wide the band is relative to the block: 100% for text, 200% for placeholders and icons.</summary>
    public static double BandWidthFactor(bool wide) => wide ? 2 : 1;

    /// <summary>Corner radius: 16 for text lines, 8 for placeholders, 2 for icons.</summary>
    public static double Radius(AiSkeletonShape shape) => shape switch
    {
        AiSkeletonShape.Text => 16,
        AiSkeletonShape.Placeholder => 8,
        _ => 2,
    };
}

/// <summary>The shapes of AI skeleton blocks.</summary>
internal enum AiSkeletonShape
{
    Text,
    Placeholder,
    Icon,
}

/// <summary>
/// The shared AI skeleton block: a rounded <c>ai-skeleton-background</c> fill with a transparent to <c>ai-skeleton-element-background</c> (50% opacity) to
/// transparent band sliding across it every 1250ms. Static (the band rests in the middle) when motion is reduced. Decorative.
/// </summary>
internal sealed class CdsAiShimmerView : Border
{
    const string AnimationName = "cds-ai-shimmer";
    readonly BoxView _band = new() { HorizontalOptions = LayoutOptions.Start, Opacity = CdsAiTokens.SkeletonElementOpacity, InputTransparent = true };
    readonly Grid _inner = new() { IsClippedToBounds = true };
    readonly double _factor;

    public CdsAiShimmerView(AiSkeletonShape shape)
    {
        _factor = AiShimmerModel.BandWidthFactor(shape != AiSkeletonShape.Text);
        StrokeThickness = 0;
        Padding = 0;
        StrokeShape = new RoundRectangle { CornerRadius = AiShimmerModel.Radius(shape) };
        CdsTokens.SetColor(this, BackgroundColorProperty, CdsAiTokens.SkeletonBackground);
        CdsColorProxies.Bind(_band, colors =>
        {
            if (colors[0] is not { } color) return;
            _band.Background = new LinearGradientBrush(
                [new GradientStop(Colors.Transparent, 0f), new GradientStop(color, 0.5f), new GradientStop(Colors.Transparent, 1f)],
                new Point(0, 0.5), new Point(1, 0.5));
        }, CdsAiTokens.SkeletonElement);
        _inner.Add(_band);
        Content = _inner;
        InputTransparent = true;
        AutomationProperties.SetIsInAccessibleTree(this, false);
        SizeChanged += (_, _) => Apply(_phase);
        Loaded += (_, _) => CdsLoop.Start(this, AnimationName, Apply, AiShimmerModel.RestPhase, AiShimmerModel.LoopMilliseconds);
        Unloaded += (_, _) => CdsLoop.Stop(this, AnimationName);
    }

    double _phase = AiShimmerModel.RestPhase;

    void Apply(double phase)
    {
        _phase = phase;
        if (Width <= 0) return;
        var bandWidth = Width * _factor;
        _band.WidthRequest = bandWidth;
        _band.TranslationX = AiShimmerModel.Translation(phase) * bandWidth;
    }
}

/// <summary>
/// The AI version of <see cref="CdsSkeletonText"/>: a bar standing in for a line, heading or paragraph of AI-generated text while it
/// loads: 16 high (24 for a heading), fully rounded, <c>ai-skeleton-background</c> with a fast sliding <c>ai-skeleton-element-background</c> shimmer, 8 between
/// lines. Use it only where the pending content is AI-generated (in a container that carries an <see cref="CdsAILabel"/>); use the
/// plain skeletons elsewhere. Match the size of the content it stands in for. Skeletons are invisible to screen readers: mark the
/// loading region busy and announce progress ("Generating summary") yourself.
/// </summary>
public class CdsAISkeletonText : CdsControl
{
    /// <summary>Bindable property for <see cref="IsHeading"/>.</summary>
    public static readonly BindableProperty IsHeadingProperty = BindableProperty.Create(
        nameof(IsHeading), typeof(bool), typeof(CdsAISkeletonText), false, propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="IsParagraph"/>.</summary>
    public static readonly BindableProperty IsParagraphProperty = BindableProperty.Create(
        nameof(IsParagraph), typeof(bool), typeof(CdsAISkeletonText), false, propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="LineCount"/>.</summary>
    public static readonly BindableProperty LineCountProperty = BindableProperty.Create(
        nameof(LineCount), typeof(int), typeof(CdsAISkeletonText), 3,
        coerceValue: static (_, v) => Math.Max(1, (int)v), propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="WidthPercent"/>.</summary>
    public static readonly BindableProperty WidthPercentProperty = BindableProperty.Create(
        nameof(WidthPercent), typeof(double), typeof(CdsAISkeletonText), 100d,
        coerceValue: static (_, v) => Math.Clamp((double)v, 0, 100), propertyChanged: OnChanged);

    readonly VerticalStackLayout _lines = new() { Spacing = CdsSpacing.Spacing03 };

    /// <summary>Creates a full-width single-line skeleton.</summary>
    public CdsAISkeletonText()
    {
        HorizontalOptions = LayoutOptions.Fill;
        AutomationProperties.SetIsInAccessibleTree(this, false);
        SetChrome(_lines);
        Rebuild();
    }

    /// <summary>The taller 24 heading bar.</summary>
    public bool IsHeading
    {
        get => (bool)GetValue(IsHeadingProperty);
        set => SetValue(IsHeadingProperty, value);
    }

    /// <summary>Several lines of varying width (100, 85, 95, 75, 90 percent) instead of one.</summary>
    public bool IsParagraph
    {
        get => (bool)GetValue(IsParagraphProperty);
        set => SetValue(IsParagraphProperty, value);
    }

    /// <summary>Lines of a paragraph. Default 3.</summary>
    public int LineCount
    {
        get => (int)GetValue(LineCountProperty);
        set => SetValue(LineCountProperty, value);
    }

    /// <summary>Width as a percentage of the available width (default 100). In paragraph mode a value other than 100 applies to every line.</summary>
    public double WidthPercent
    {
        get => (double)GetValue(WidthPercentProperty);
        set => SetValue(WidthPercentProperty, value);
    }

    void Rebuild()
    {
        _lines.Children.Clear();
        var height = SkeletonTextModel.LineHeight(IsHeading);
        foreach (var fraction in SkeletonTextModel.LineFractions(IsParagraph, LineCount, WidthPercent))
        {
            var row = new Grid { HeightRequest = height };
            if (fraction < 1)
            {
                row.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(Math.Max(fraction, 0.0001), GridUnitType.Star)));
                row.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(1 - fraction, GridUnitType.Star)));
            }
            row.Add(new CdsAiShimmerView(AiSkeletonShape.Text) { HeightRequest = height }, 0);
            _lines.Children.Add(row);
        }
    }

    static void OnChanged(BindableObject b, object? o, object? n) => ((CdsAISkeletonText)b).Rebuild();
}

/// <summary>
/// The AI version of <see cref="CdsSkeletonPlaceholder"/>: a rounded (8) <c>ai-skeleton-background</c> box with the AI shimmer, 100 x 100 by
/// default; set <c>WidthRequest</c> and <c>HeightRequest</c> to match the content it replaces (a generated chart or image).
/// </summary>
public class CdsAISkeletonPlaceholder : CdsControl
{
    /// <summary>Creates a 100 x 100 placeholder.</summary>
    public CdsAISkeletonPlaceholder()
    {
        WidthRequest = CdsSkeletonPlaceholder.DefaultSize;
        HeightRequest = CdsSkeletonPlaceholder.DefaultSize;
        HorizontalOptions = LayoutOptions.Start;
        VerticalOptions = LayoutOptions.Start;
        AutomationProperties.SetIsInAccessibleTree(this, false);
        SetChrome(new CdsAiShimmerView(AiSkeletonShape.Placeholder));
    }
}

/// <summary>
/// The AI version of <see cref="CdsSkeletonIcon"/>: a 16 x 16 box with a 2px radius and the AI shimmer; resize with <see cref="Size"/>.
/// </summary>
public class CdsAISkeletonIcon : CdsControl
{
    /// <summary>Bindable property for <see cref="Size"/>.</summary>
    public static readonly BindableProperty SizeProperty = BindableProperty.Create(
        nameof(Size), typeof(double), typeof(CdsAISkeletonIcon), 16d,
        coerceValue: static (_, v) => Math.Max(1, (double)v),
        propertyChanged: static (b, _, _) => ((CdsAISkeletonIcon)b).UpdateSize());

    /// <summary>Creates a 16 x 16 icon skeleton.</summary>
    public CdsAISkeletonIcon()
    {
        HorizontalOptions = LayoutOptions.Start;
        VerticalOptions = LayoutOptions.Center;
        AutomationProperties.SetIsInAccessibleTree(this, false);
        SetChrome(new CdsAiShimmerView(AiSkeletonShape.Icon));
        UpdateSize();
    }

    /// <summary>Width and height. Default 16.</summary>
    public double Size
    {
        get => (double)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    void UpdateSize()
    {
        WidthRequest = Size;
        HeightRequest = Size;
    }
}

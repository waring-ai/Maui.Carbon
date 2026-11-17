namespace Maui.Carbon;

/// <summary>Line layout of a text skeleton (pure, tested).</summary>
internal static class SkeletonTextModel
{
    /// <summary>The fixed repeating widths of paragraph lines, in percent (Carbon randomizes them; this keeps output stable).</summary>
    static readonly double[] s_paragraphPattern = [100, 85, 95, 75, 90];

    /// <summary>Bar height: 16, or 24 for headings.</summary>
    public static double LineHeight(bool heading) => heading ? 24 : 16;

    /// <summary>
    /// The width of each line as a fraction of the available width. A single line (or a heading) uses
    /// <paramref name="widthPercent"/>; a paragraph cycles through 100/85/95/75/90 unless a width other than 100 is
    /// set, which then applies to every line.
    /// </summary>
    public static IReadOnlyList<double> LineFractions(bool paragraph, int lineCount, double widthPercent)
    {
        var width = Math.Clamp(widthPercent, 0, 100) / 100;
        if (!paragraph) return [width];
        var count = Math.Max(1, lineCount);
        var fractions = new double[count];
        for (var i = 0; i < count; i++)
            fractions[i] = widthPercent is >= 100 ? s_paragraphPattern[i % s_paragraphPattern.Length] / 100 : width;
        return fractions;
    }
}

/// <summary>
/// An animated bar standing in for a line, heading or paragraph of text while it loads: 16 high (24 for a heading),
/// <c>skeleton-background</c> with a sweeping <c>skeleton-element</c> band, 8 between lines. Size skeletons like the
/// content they replace so nothing shifts when it arrives, show them for a few seconds at most, and do not mix them with
/// spinners in the same region. Skeletons are decorative and invisible to screen readers: mark the loading region as
/// busy and name what is loading yourself, and announce failures with a notification.
/// </summary>
public class CdsSkeletonText : CdsControl
{
    /// <summary>Bindable property for <see cref="IsHeading"/>.</summary>
    public static readonly BindableProperty IsHeadingProperty = BindableProperty.Create(
        nameof(IsHeading), typeof(bool), typeof(CdsSkeletonText), false, propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="IsParagraph"/>.</summary>
    public static readonly BindableProperty IsParagraphProperty = BindableProperty.Create(
        nameof(IsParagraph), typeof(bool), typeof(CdsSkeletonText), false, propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="LineCount"/>.</summary>
    public static readonly BindableProperty LineCountProperty = BindableProperty.Create(
        nameof(LineCount), typeof(int), typeof(CdsSkeletonText), 3,
        coerceValue: static (_, v) => Math.Max(1, (int)v), propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="WidthPercent"/>.</summary>
    public static readonly BindableProperty WidthPercentProperty = BindableProperty.Create(
        nameof(WidthPercent), typeof(double), typeof(CdsSkeletonText), 100d,
        coerceValue: static (_, v) => Math.Clamp((double)v, 0, 100), propertyChanged: OnChanged);

    readonly VerticalStackLayout _lines = new() { Spacing = CdsSpacing.Spacing03 };

    /// <summary>Creates a full-width single-line skeleton.</summary>
    public CdsSkeletonText()
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

    /// <summary>Several lines with varying widths (100, 85, 95, 75, 90 percent) instead of one.</summary>
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

    /// <summary>
    /// Width as a percentage of the available width, 0 to 100 (default 100). In paragraph mode a value other than 100
    /// applies to every line. For an absolute width set <c>WidthRequest</c>.
    /// </summary>
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
            row.Add(new CdsShimmerView { HeightRequest = height }, 0);
            _lines.Children.Add(row);
        }
    }

    static void OnChanged(BindableObject b, object? o, object? n) => ((CdsSkeletonText)b).Rebuild();
}

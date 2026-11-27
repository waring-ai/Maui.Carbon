using Microsoft.Maui.Controls.Shapes;

namespace Maui.Carbon;

/// <summary>
/// The floating surface shared by popovers, tooltips and toggletips: a 2px-radius box on <c>layer-01</c> (or
/// <c>background-inverse</c> when high contrast) with an optional caret (12x6 unless sized otherwise) pointing at the anchor and a
/// <c>shadow</c> drop shadow. Give the content its own padding. Call <see cref="Track"/> with the overlay that shows it.
/// </summary>
public sealed class CdsPopoverSurface : Grid
{
    /// <summary>Default caret size along the edge.</summary>
    public const double CaretWidth = 12;

    /// <summary>Default caret depth away from the edge.</summary>
    public const double CaretHeight = 6;

    // Carbon's icon tooltip (and so the slider thumb tooltip) uses a smaller caret: popover-caret-width 8, -height 4.
    internal const double TooltipCaretWidth = 8;
    internal const double TooltipCaretHeight = 4;

    readonly Border _box = new() { StrokeThickness = 0, Padding = 0, StrokeShape = new RoundRectangle { CornerRadius = 2 } };
    readonly Polygon _caret = new() { IsVisible = false, InputTransparent = true, WidthRequest = CaretWidth, HeightRequest = CaretWidth };
    readonly bool _hasCaret;
    readonly double _caretWidth;
    readonly double _caretHeight;

    /// <summary>Creates a surface.</summary>
    /// <param name="content">The content (with its own padding).</param>
    /// <param name="hasCaret">Draw a caret toward the anchor.</param>
    /// <param name="highContrast">Use <c>background-inverse</c> / <c>text-inverse</c> instead of <c>layer-01</c>.</param>
    /// <param name="dropShadow">Add the Carbon drop shadow.</param>
    /// <param name="border">Add a 1px <c>border-subtle-01</c> outline.</param>
    /// <param name="maxWidth">Maximum content width (Carbon popover 368, tooltip 288).</param>
    /// <param name="caretWidth">Caret size along the edge; default <see cref="CaretWidth"/> (the icon tooltip is 8).</param>
    /// <param name="caretHeight">Caret depth away from the edge; default <see cref="CaretHeight"/> (the icon tooltip is 4).</param>
    public CdsPopoverSurface(View content, bool hasCaret = true, bool highContrast = false, bool dropShadow = true, bool border = false, double maxWidth = 368,
        double caretWidth = CaretWidth, double caretHeight = CaretHeight)
    {
        _hasCaret = hasCaret;
        _caretWidth = caretWidth;
        _caretHeight = caretHeight;
        var fill = highContrast ? CdsColorKeys.BackgroundInverse : CdsColorKeys.Layer01;
        CdsTokens.SetColor(_box, BackgroundColorProperty, fill);
        CdsTokens.SetBrush(_caret, Shape.FillProperty, fill);
        if (border)
        {
            _box.StrokeThickness = 1;
            CdsTokens.SetBrush(_box, Border.StrokeProperty, CdsColorKeys.BorderSubtle01);
        }
        if (dropShadow) CdsTokens.SetShadow(_box, CdsColorKeys.Shadow);
        _box.MaximumWidthRequest = maxWidth;
        _box.Content = content;
        Children.Add(_box);
        Children.Add(_caret);
        IsHighContrast = highContrast;
        if (highContrast) ApplyInverseText(content);
    }

    /// <summary>True when the surface uses the inverse colors.</summary>
    public bool IsHighContrast { get; }

    /// <summary>The surface box (for content that needs to size against it).</summary>
    public Border Box => _box;

    /// <summary>Keeps the caret pointed at the anchor as the overlay moves.</summary>
    public void Track(CdsOverlay overlay)
    {
        overlay.Positioned += (_, _) => UpdateCaret(overlay.ActualPlacement, overlay.CaretOffset);
    }

    /// <summary>Gap between the anchor and a surface without a caret (Carbon: <c>spacing-02</c>).</summary>
    public const double NoCaretGap = CdsSpacing.Spacing02;

    /// <summary>
    /// The overlay offset that puts the content <paramref name="gap"/> away from the anchor, caret included. A surface
    /// without a caret keeps <see cref="NoCaretGap"/> from the anchor whatever <paramref name="gap"/> is.
    /// </summary>
    public double OffsetFor(double gap) => _hasCaret ? Math.Max(0, gap - _caretHeight) : NoCaretGap;

    /// <summary>Places the caret on the edge facing the anchor.</summary>
    public void UpdateCaret(CdsPlacement placement, double caretOffset)
    {
        if (!_hasCaret)
        {
            _caret.IsVisible = false;
            return;
        }
        _caret.IsVisible = true;
        var vertical = CdsPlacementResolver.IsVertical(placement);
        var popupBelow = placement is CdsPlacement.Bottom or CdsPlacement.BottomStart or CdsPlacement.BottomEnd;
        var popupRight = placement is CdsPlacement.Right or CdsPlacement.RightTop or CdsPlacement.RightBottom;

        // The box leaves room for the caret on the anchor side.
        _box.Margin = vertical
            ? new Thickness(0, popupBelow ? _caretHeight : 0, 0, popupBelow ? 0 : _caretHeight)
            : new Thickness(popupRight ? _caretHeight : 0, 0, popupRight ? 0 : _caretHeight, 0);

        var w = _caretWidth;
        var h = _caretHeight;
        _caret.Points = vertical
            ? (popupBelow ? [new(0, h), new(w / 2, 0), new(w, h)] : [new(0, 0), new(w / 2, h), new(w, 0)])
            : (popupRight ? [new(h, 0), new(0, w / 2), new(h, w)] : [new(0, 0), new(h, w / 2), new(0, w)]);
        _caret.WidthRequest = vertical ? w : h;
        _caret.HeightRequest = vertical ? h : w;
        _caret.HorizontalOptions = vertical ? LayoutOptions.Start : (popupRight ? LayoutOptions.Start : LayoutOptions.End);
        _caret.VerticalOptions = vertical ? (popupBelow ? LayoutOptions.Start : LayoutOptions.End) : LayoutOptions.Start;
        var along = Math.Max(4, caretOffset - w / 2);
        _caret.TranslationX = vertical ? along : 0;
        _caret.TranslationY = vertical ? 0 : along;
    }

    static void ApplyInverseText(View content)
    {
        // Labels inside a high-contrast surface default to text-inverse.
        if (content is Label l && !l.IsSet(Label.TextColorProperty)) l.SetDynamicResource(Label.TextColorProperty, CdsColorKeys.TextInverse);
    }
}

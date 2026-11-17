namespace Maui.Carbon;

/// <summary>The skeleton sweep (Carbon's <c>cds--skeleton</c> keyframes, 3s loop), as pure math.</summary>
internal static class ShimmerModel
{
    /// <summary>Loop length in milliseconds.</summary>
    public const uint LoopMilliseconds = 3000;

    /// <summary>
    /// The element's state at a phase (0 to 1): horizontal scale of the band, whether it grows from the start edge
    /// (otherwise the end edge), and its opacity.
    /// </summary>
    public static (double Scale, bool FromStart, double Opacity) At(double phase)
    {
        phase -= Math.Floor(phase);
        double scale;
        bool fromStart;
        if (phase < 0.20) { scale = Ease(phase / 0.20); fromStart = true; }
        else if (phase < 0.28) { scale = 1; fromStart = false; }
        else if (phase < 0.51) { scale = 1 - Ease((phase - 0.28) / 0.23); fromStart = false; }
        else if (phase < 0.58) { scale = 0; fromStart = false; }
        else if (phase < 0.82) { scale = Ease((phase - 0.58) / 0.24); fromStart = false; }
        else if (phase < 0.83) { scale = 1; fromStart = true; }
        else if (phase < 0.96) { scale = 1 - Ease((phase - 0.83) / 0.13); fromStart = true; }
        else { scale = 0; fromStart = true; }

        // Opacity rises from 0.3 to 1 over the first 20% and falls back to 0.3 by the end of the loop.
        var opacity = phase < 0.20 ? 0.3 + 0.7 * Ease(phase / 0.20) : 1 - 0.7 * Ease((phase - 0.20) / 0.80);
        return (scale, fromStart, opacity);
    }

    /// <summary>The CSS <c>ease-in-out</c> curve approximated by a smoothstep.</summary>
    public static double Ease(double t)
    {
        t = Math.Clamp(t, 0, 1);
        return t * t * (3 - 2 * t);
    }
}

/// <summary>
/// The shared skeleton block: a <c>skeleton-background</c> fill with a <c>skeleton-element</c> band that sweeps across
/// it. Static (the band fills the block) when motion is reduced. Decorative: not in the accessibility tree.
/// </summary>
internal sealed class CdsShimmerView : Grid
{
    const string AnimationName = "cds-shimmer";
    readonly BoxView _background = new();
    readonly BoxView _band = new() { AnchorX = 0 };

    public CdsShimmerView()
    {
        CdsTokens.SetColor(_background, BoxView.ColorProperty, CdsColorKeys.SkeletonBackground);
        CdsTokens.SetColor(_band, BoxView.ColorProperty, CdsColorKeys.SkeletonElement);
        Children.Add(_background);
        Children.Add(_band);
        InputTransparent = true;
        CascadeInputTransparent = true;
        IsClippedToBounds = true;
        AutomationProperties.SetIsInAccessibleTree(this, false);
        Loaded += (_, _) => CdsLoop.Start(this, AnimationName, Apply, 0.2, ShimmerModel.LoopMilliseconds);
        Unloaded += (_, _) => CdsLoop.Stop(this, AnimationName);
    }

    void Apply(double phase)
    {
        var (scale, fromStart, opacity) = ShimmerModel.At(phase);
        _band.AnchorX = fromStart ? 0 : 1;
        _band.ScaleX = Math.Max(scale, 0.001);
        _band.Opacity = scale <= 0.001 ? 0 : opacity;
    }
}

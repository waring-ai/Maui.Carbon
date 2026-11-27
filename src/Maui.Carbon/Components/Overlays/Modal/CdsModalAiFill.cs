namespace Maui.Carbon;

/// <summary>The four colors Carbon's AI modal container paints with: the aura, its transparent end, the surface and the inner glow.</summary>
internal readonly record struct CdsModalAiColors(Color AuraStart, Color AuraEnd, Color Surface, Color InnerShadow);

/// <summary>
/// The fill of an AI modal container, taken from Carbon's <c>_modal.scss</c> (the <c>modal--decorator:has(.cds--ai-label)</c> rules).
/// CSS builds it from a gradient with a hard stop plus inset <c>box-shadow</c>s; MAUI has neither inset shadows nor gradient hints, so
/// the same picture is sampled into one vertical <see cref="LinearGradientBrush"/> (bottom to top) from the real tokens.
/// </summary>
/// <remarks>
/// Layers, bottom to top: the surface (<c>layer</c>); <c>ai-popover-gradient('default', offset, 'layer')</c>, which is
/// <c>linear-gradient(to top, layer offset, ai-aura-start offset, 15%, ai-aura-end 50%)</c> (the offset is 0 without a footer and 64px
/// with one); the inner glow <c>inset 0 -80px 70px -65px ai-inner-shadow</c> (with a footer <c>inset 0 -160px 70px -65px</c>); and with a
/// footer the solid band <c>inset 0 -80px 0 -16px layer</c>, which keeps the bottom 64px flat.
/// </remarks>
internal static class CdsModalAiFill
{
    /// <summary>Where the aura gradient hint sits, as a share of the container height (the <c>15%</c> of the gradient).</summary>
    public const double AuraHint = 0.15;

    /// <summary>Where the aura has faded out, as a share of the container height (the <c>50%</c> of the gradient).</summary>
    public const double AuraEndShare = 0.5;

    /// <summary>Vertical offset of the inner glow without a footer (<c>-80px</c>).</summary>
    public const double GlowOffset = 80;

    /// <summary>Vertical offset of the inner glow with a footer (<c>-160px</c>).</summary>
    public const double GlowOffsetWithFooter = 160;

    /// <summary>Spread of the inner glow (<c>-65px</c>): it moves the glow's edge toward the container's edge.</summary>
    public const double GlowSpread = 65;

    /// <summary>Blur of the inner glow (<c>70px</c>); the Gaussian's standard deviation is half of it.</summary>
    public const double GlowBlur = 70;

    /// <summary>Offset of the solid footer band (<c>-80px</c>).</summary>
    public const double BandOffset = 80;

    /// <summary>Spread of the solid footer band (<c>-16px</c>): the band is <see cref="BandOffset"/> - 16 = 64px high.</summary>
    public const double BandSpread = 16;

    /// <summary>Height of the scrolling body's fade (80px) and where its solid part starts (the 48px above the footer are fully faded).</summary>
    public const double FadeHeight = 80;

    /// <summary>The share of the body fade that ramps in (32 of 80px); the rest is fully hidden.</summary>
    public const double FadeRampShare = 32 / FadeHeight;

    const double SampleStep = 6;

    /// <summary>The height of the solid footer band (<c>BandOffset - BandSpread</c>).</summary>
    public static double BandHeight => BandOffset - BandSpread;

    /// <summary>
    /// The color of the container <paramref name="up"/> px above its bottom edge, for a container <paramref name="height"/> high with a
    /// solid footer band of <paramref name="footer"/> px (0 = no footer).
    /// </summary>
    public static Color ColorAt(CdsModalAiColors colors, double height, double footer, double up)
    {
        var c = colors.Surface;
        if (height <= 0) return c;
        var x = up / height;
        var start = footer / height;
        var span = AuraEndShare - start;
        if (x >= start && span > 0)
        {
            var aura = colors.AuraEnd;
            var t = (x - start) / span;
            if (t < 1)
            {
                // A CSS color hint moves the midpoint of the transition to the hint: weight = t ^ (ln 0.5 / ln hint).
                var hint = Math.Clamp((AuraHint - start) / span, 0.001, 0.999);
                aura = Mix(colors.AuraStart, colors.AuraEnd, Math.Pow(t, Math.Log(0.5) / Math.Log(hint)));
            }
            c = CdsAiTreatment.Over(aura, c);
        }

        var edge = (footer > 0 ? GlowOffsetWithFooter : GlowOffset) - GlowSpread; // where the glow's hole ends, above the bottom edge
        var coverage = Phi((edge - up) / (GlowBlur / 2));
        c = CdsAiTreatment.Over(colors.InnerShadow.WithAlpha((float)(colors.InnerShadow.Alpha * coverage)), c);
        if (footer > 0 && up <= footer) c = colors.Surface;
        return c;
    }

    /// <summary>The container fill as a gradient from the bottom edge (offset 0) to the top.</summary>
    public static LinearGradientBrush CreateFill(CdsModalAiColors colors, double height, double footer)
    {
        var stops = new List<GradientStop>();
        if (height <= 0)
        {
            stops.Add(new GradientStop(colors.Surface, 0));
            stops.Add(new GradientStop(colors.Surface, 1));
            return new LinearGradientBrush([.. stops], new Point(0, 1), new Point(0, 0));
        }
        var edge = (footer > 0 ? GlowOffsetWithFooter : GlowOffset) - GlowSpread;
        var top = Math.Min(height, Math.Max(AuraEndShare * height, edge + 3 * GlowBlur / 2));
        var samples = new SortedSet<double> { 0 };
        // The footer band ends at a hard edge.
        if (footer > 0 && footer < top)
        {
            samples.Add(footer);
            samples.Add(footer + 0.01);
        }
        for (var up = SampleStep; up < top; up += SampleStep)
            if (footer <= 0 || Math.Abs(up - footer) > 0.01) samples.Add(up);
        foreach (var up in samples)
            stops.Add(new GradientStop(ColorAt(colors, height, footer, up), (float)(up / height)));
        stops.Add(new GradientStop(colors.Surface, 1));
        return new LinearGradientBrush([.. stops], new Point(0, 1), new Point(0, 0));
    }

    /// <summary>
    /// The scrolling body's bottom fade in an AI modal. Carbon masks the content so the aura shows through (32px ramp, then the 48px
    /// above the footer fully hidden); here the strip paints the container fill that lies under it, transparent at its top. The strip
    /// is <see cref="FadeHeight"/> px high and its bottom edge sits <paramref name="footer"/> px above the container's bottom.
    /// </summary>
    public static LinearGradientBrush CreateFade(CdsModalAiColors colors, double height, double footer)
    {
        const int steps = 10;
        var stops = new List<GradientStop>();
        for (var i = 0; i <= steps; i++)
        {
            var y = i / (double)steps; // 0 at the top of the strip, 1 at its bottom
            var color = ColorAt(colors, height, footer, footer + FadeHeight * (1 - y));
            var alpha = y >= FadeRampShare ? 1 : y / FadeRampShare;
            stops.Add(new GradientStop(color.WithAlpha((float)(color.Alpha * alpha)), (float)y));
        }
        return new LinearGradientBrush([.. stops], new Point(0, 0), new Point(0, 1));
    }

    /// <summary>Interpolates two colors in premultiplied space, as CSS does for gradients.</summary>
    internal static Color Mix(Color from, Color to, double t)
    {
        var a = from.Alpha + (to.Alpha - from.Alpha) * t;
        if (a <= 0) return Colors.Transparent;
        float Channel(float f, float u) => (float)((f * from.Alpha * (1 - t) + u * to.Alpha * t) / a);
        return new Color(Channel(from.Red, to.Red), Channel(from.Green, to.Green), Channel(from.Blue, to.Blue), (float)a);
    }

    /// <summary>The standard normal cumulative distribution (a blurred edge's coverage).</summary>
    internal static double Phi(double x) => 0.5 * (1 + Erf(x / Math.Sqrt(2)));

    // Abramowitz and Stegun 7.1.26, absolute error below 1.5e-7.
    static double Erf(double x)
    {
        var sign = x < 0 ? -1 : 1;
        x = Math.Abs(x);
        var t = 1 / (1 + 0.3275911 * x);
        var y = 1 - (((((1.061405429 * t - 1.453152027) * t) + 1.421413741) * t - 0.284496736) * t + 0.254829592) * t * Math.Exp(-x * x);
        return sign * y;
    }
}

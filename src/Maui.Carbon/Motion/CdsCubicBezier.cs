namespace Maui.Carbon;

/// <summary>CSS <c>cubic-bezier()</c> timing functions as MAUI easings.</summary>
public static class CdsCubicBezier
{
    /// <summary>Creates an easing equivalent to <c>cubic-bezier(x1, y1, x2, y2)</c>.</summary>
    public static Easing ToEasing(double x1, double y1, double x2, double y2) => new(t => Evaluate(x1, y1, x2, y2, t));

    /// <summary>Evaluates the curve's y for progress x (0–1).</summary>
    public static double Evaluate(double x1, double y1, double x2, double y2, double x)
    {
        if (x <= 0) return 0;
        if (x >= 1) return 1;

        // Solve bx(t) = x with Newton–Raphson, falling back to bisection.
        var t = x;
        for (var i = 0; i < 8; i++)
        {
            var err = Bezier(x1, x2, t) - x;
            if (Math.Abs(err) < 1e-6) return Bezier(y1, y2, t);
            var d = Derivative(x1, x2, t);
            if (Math.Abs(d) < 1e-6) break;
            t -= err / d;
        }

        double lo = 0, hi = 1;
        t = x;
        for (var i = 0; i < 40; i++)
        {
            var v = Bezier(x1, x2, t);
            if (Math.Abs(v - x) < 1e-6) break;
            if (v < x) lo = t; else hi = t;
            t = (lo + hi) / 2;
        }
        return Bezier(y1, y2, t);
    }

    static double Bezier(double p1, double p2, double t)
    {
        var u = 1 - t;
        return 3 * u * u * t * p1 + 3 * u * t * t * p2 + t * t * t;
    }

    static double Derivative(double p1, double p2, double t)
    {
        var u = 1 - t;
        return 3 * u * u * p1 + 6 * u * t * (p2 - p1) + 3 * t * t * (1 - p2);
    }
}

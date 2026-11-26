namespace Maui.Carbon;

/// <summary>The statistics of one group of a box plot (Carbon's getBoxplotData).</summary>
internal sealed record ChartBoxplotStats(
    string Group, IReadOnlyList<double> Values, double Q1, double Median, double Q3, double WhiskerMin, double WhiskerMax, IReadOnlyList<double> Outliers)
{
    /// <summary>The interquartile range, Q3 - Q1.</summary>
    public double Iqr => Q3 - Q1;

    /// <summary>d3-array's <c>quantile</c> of sorted values (R-7: linear interpolation between closest ranks); NaN when empty.</summary>
    public static double Quantile(IReadOnlyList<double> sorted, double p)
    {
        var n = sorted.Count;
        if (n == 0 || double.IsNaN(p)) return double.NaN;
        if (p <= 0 || n < 2) return sorted[0];
        if (p >= 1) return sorted[n - 1];
        var i = (n - 1) * p;
        var i0 = (int)Math.Floor(i);
        var v0 = sorted[i0];
        var v1 = sorted[i0 + 1];
        return v0 + (v1 - v0) * (i - i0);
    }

    /// <summary>
    /// Quartiles, 1.5 x IQR fences, outliers (values beyond the fences) and whiskers (the last values inside them; the
    /// quartiles' extent when every value is an outlier). Null when there are no finite values.
    /// </summary>
    public static ChartBoxplotStats? Compute(string group, IEnumerable<double> values)
    {
        var sorted = values.Where(double.IsFinite).Order().ToList();
        if (sorted.Count == 0) return null;
        var q1 = Quantile(sorted, 0.25);
        var q2 = Quantile(sorted, 0.5);
        var q3 = Quantile(sorted, 0.75);
        var fence = (q3 - q1) * 1.5;
        var lo = q1 - fence;
        var hi = q3 + fence;
        var outliers = new List<double>();
        var normal = new List<double>();
        foreach (var v in sorted) (v < lo || v > hi ? outliers : normal).Add(v);
        var min = normal.Count > 0 ? normal[0] : Math.Min(q1, Math.Min(q2, q3));
        var max = normal.Count > 0 ? normal[^1] : Math.Max(q1, Math.Max(q2, q3));
        return new ChartBoxplotStats(group, sorted, q1, q2, q3, min, max, outliers);
    }
}

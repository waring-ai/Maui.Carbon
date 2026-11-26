namespace Maui.Carbon;

/// <summary>One bin of a histogram: [X0, X1) (the last bin includes X1) and the items in it.</summary>
internal sealed class ChartBin<T>
{
    public double X0 { get; set; }

    public double X1 { get; set; }

    public List<T> Items { get; } = [];
}

/// <summary>
/// d3-array's <c>bin</c> as Carbon's getBinConfigurations uses it: a bin count asks for that many nice thresholds over the
/// niced extent (the last bin widened to the common width), an array of thresholds sets the bins and the domain.
/// </summary>
internal static class ChartBins
{
    /// <summary>The result: the bins and the domain they cover.</summary>
    internal sealed record Result<T>(IReadOnlyList<ChartBin<T>> Bins, double Lo, double Hi);

    /// <summary>
    /// Bins <paramref name="items"/> by <paramref name="value"/> (non-finite values are left out). With
    /// <paramref name="thresholds"/> of two or more values the domain is their first to last value and values outside it
    /// are left out; otherwise <paramref name="count"/> nice bins cover the data.
    /// </summary>
    public static Result<T> Compute<T>(IReadOnlyList<T> items, Func<T, double?> value, int count, IReadOnlyList<double>? thresholds = null)
    {
        var data = new List<(double V, T Item)>();
        foreach (var item in items)
            if (value(item) is { } v && double.IsFinite(v)) data.Add((v, item));
        var defined = thresholds is { Count: > 1 };
        if (data.Count == 0)
            return new Result<T>([], defined ? thresholds![0] : 0, defined ? thresholds![^1] : 1);

        double x0, x1;
        List<double> tz;
        double step = double.NaN;
        if (defined)
        {
            x0 = thresholds![0];
            x1 = thresholds[^1];
            tz = thresholds.ToList();
        }
        else
        {
            x0 = data.Min(d => d.V);
            x1 = data.Max(d => d.V);
            if (x0 == x1)
            {
                // d3 makes one zero-width bin; give it a unit width so it can be drawn.
                var only = new ChartBin<T> { X0 = x0, X1 = x0 + 1 };
                only.Items.AddRange(data.Select(d => d.Item));
                return new Result<T>([only], x0, x0 + 1);
            }
            var max = x1;
            var tn = Math.Max(1, count);
            (x0, x1) = Nice(x0, x1, tn);
            tz = ChartTicks.Ticks(x0, x1, tn).ToList();
            if (tz.Count > 0 && tz[0] <= x0) step = ChartTicks.Increment(x0, x1, tn);
            if (tz.Count > 0 && tz[^1] >= x1)
            {
                if (max >= x1)
                {
                    var s = ChartTicks.Increment(x0, x1, tn);
                    if (double.IsFinite(s))
                    {
                        if (s > 0) x1 = (Math.Floor(x1 / s) + 1) * s;
                        else if (s < 0) x1 = (Math.Ceiling(x1 * -s) + 1) / -s;
                    }
                }
                else tz.RemoveAt(tz.Count - 1);
            }
        }

        // Remove the thresholds outside the domain.
        var a = 0;
        var b = tz.Count;
        while (a < b && tz[a] <= x0) a++;
        while (b > a && tz[b - 1] > x1) b--;
        tz = tz.GetRange(a, b - a);
        var m = tz.Count;
        var bins = new List<ChartBin<T>>(m + 1);
        for (var i = 0; i <= m; i++) bins.Add(new ChartBin<T> { X0 = i > 0 ? tz[i - 1] : x0, X1 = i < m ? tz[i] : x1 });

        foreach (var (x, item) in data)
        {
            if (!(x0 <= x && x <= x1)) continue;
            int index;
            if (double.IsFinite(step) && step > 0) index = Math.Min(m, (int)Math.Floor((x - x0) / step));
            else if (double.IsFinite(step) && step < 0)
            {
                var j = (int)Math.Floor((x0 - x) * step);
                index = Math.Min(m, j + (j < m && tz[j] <= x ? 1 : 0));
            }
            else index = BisectRight(tz, x);
            bins[Math.Clamp(index, 0, m)].Items.Add(item);
        }

        if (defined)
        {
            bins[^1].X1 = thresholds![^1];
            // d3 ends with a zero-width bin at the last threshold; count its values in the last real bin instead.
            if (bins.Count > 1 && bins[^1].X0 == bins[^1].X1)
            {
                bins[^2].Items.AddRange(bins[^1].Items);
                bins.RemoveAt(bins.Count - 1);
            }
            return new Result<T>(bins, thresholds[0], thresholds[^1]);
        }
        bins[^1].X1 = bins[^1].X0 + (bins[0].X1 - bins[0].X0);
        return new Result<T>(bins, bins[0].X0, bins[^1].X1);
    }

    /// <summary>d3-array's <c>nice</c>: extends [start, stop] to round tick values (stops when the step repeats or is not finite).</summary>
    public static (double Start, double Stop) Nice(double start, double stop, int count)
    {
        double? prestep = null;
        for (var guard = 0; guard < 64; guard++)
        {
            var step = ChartTicks.Increment(start, stop, count);
            if (step == prestep || step == 0 || !double.IsFinite(step)) break;
            if (step > 0)
            {
                start = Math.Floor(start / step) * step;
                stop = Math.Ceiling(stop / step) * step;
            }
            else
            {
                start = Math.Ceiling(start * step) / step;
                stop = Math.Floor(stop * step) / step;
            }
            prestep = step;
        }
        return (start, stop);
    }

    static int BisectRight(List<double> a, double x)
    {
        int lo = 0, hi = a.Count;
        while (lo < hi)
        {
            var mid = (lo + hi) >>> 1;
            if (x < a[mid]) hi = mid;
            else lo = mid + 1;
        }
        return lo;
    }
}

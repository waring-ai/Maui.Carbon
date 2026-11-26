namespace Maui.Carbon.Tests.Components;

/// <summary>ChartBins against d3-array 3.2.5's bin (expected values printed by d3 for the same inputs).</summary>
public class ChartBinsTests
{
    /// <summary>The histogram preview's seeded generator (Park-Miller and Box-Muller), shared across its three samples.</summary>
    internal sealed class Seeded(long seed)
    {
        long _seed = seed;

        double Next()
        {
            _seed = _seed * 16807 % 2147483647;
            return (_seed - 1) / 2147483646.0;
        }

        public double Normal(double mean, double sd)
        {
            double u = 0, v = 0;
            while (u == 0) u = Next();
            while (v == 0) v = Next();
            return mean + sd * Math.Sqrt(-2 * Math.Log(u)) * Math.Cos(2 * Math.PI * v);
        }
    }

    internal static (List<double> Ages, List<double> Latency, List<double> Orders) PreviewSamples()
    {
        var r = new Seeded(7);
        var ages = Enumerable.Range(0, 400).Select(_ => Math.Floor(Math.Max(19, Math.Min(66, r.Normal(38, 9))) + 0.5)).ToList();
        var latency = new List<double>();
        foreach (var (m, s, n) in new[] { (140.0, 28.0, 260), (175.0, 35.0, 220), (230.0, 45.0, 160) })
            for (var i = 0; i < n; i++) latency.Add(Math.Max(40, r.Normal(m, s)));
        var orders = Enumerable.Range(0, 300).Select(_ => Math.Min(499, Math.Exp(r.Normal(3.6, 0.7)))).ToList();
        return (ages, latency, orders);
    }

    static void Matches((double X0, double X1, int Count)[] expected, ChartBins.Result<double> actual)
    {
        Assert.Equal(expected.Length, actual.Bins.Count);
        for (var i = 0; i < expected.Length; i++)
        {
            Assert.Equal(expected[i].X0, actual.Bins[i].X0, 9);
            Assert.Equal(expected[i].X1, actual.Bins[i].X1, 9);
            Assert.Equal(expected[i].Count, actual.Bins[i].Items.Count);
        }
    }

    static ChartBins.Result<double> Bin(IReadOnlyList<double> values, int count, IReadOnlyList<double>? thresholds = null) =>
        ChartBins.Compute(values, v => v, count, thresholds);

    [Fact]
    public void Preview_samples_match_the_generator()
    {
        var (ages, latency, orders) = PreviewSamples();
        Assert.Equal(new double[] { 66, 41, 31, 38, 31 }, ages.Take(5));
        Assert.Equal(124.75709675049958, latency[0], 9);
        Assert.Equal(62.079358154569896, orders[0], 9);
    }

    [Fact]
    public void Twenty_bins_of_ages_match_d3()
    {
        Matches([(18, 20, 5), (20, 22, 5), (22, 24, 5), (24, 26, 11), (26, 28, 10), (28, 30, 17), (30, 32, 42), (32, 34, 26), (34, 36, 39), (36, 38, 32), (38, 40, 41), (40, 42, 36), (42, 44, 35), (44, 46, 29), (46, 48, 13), (48, 50, 14), (50, 52, 11), (52, 54, 6), (54, 56, 14), (56, 58, 1), (58, 60, 4), (60, 62, 1), (62, 64, 0), (64, 66, 1), (66, 68, 2)], Bin(PreviewSamples().Ages, 20));
    }

    [Fact]
    public void Fifteen_bins_of_latency_match_d3()
    {
        var bins = Bin(PreviewSamples().Latency, 15);
        Matches([(40, 60, 3), (60, 80, 7), (80, 100, 25), (100, 120, 50), (120, 140, 82), (140, 160, 111), (160, 180, 103), (180, 200, 86), (200, 220, 44), (220, 240, 48), (240, 260, 36), (260, 280, 25), (280, 300, 13), (300, 320, 5), (320, 340, 2)], bins);
        Assert.Equal(40, bins.Lo);
        Assert.Equal(340, bins.Hi);
    }

    [Fact]
    public void Thresholds_set_the_bins_and_the_last_zero_width_bin_is_merged()
    {
        // d3: [(0, 25, 95), (25, 50, 115), (50, 75, 46), (75, 100, 28), (100, 150, 7), (150, 200, 5), (200, 300, 4), (300, 500, 0), (500, 500, 0)]; the zero-width [500, 500] bin is folded into the last real bin.
        var bins = Bin(PreviewSamples().Orders, 10, [0, 25, 50, 75, 100, 150, 200, 300, 500]);
        Matches([(0, 25, 95), (25, 50, 115), (50, 75, 46), (75, 100, 28), (100, 150, 7), (150, 200, 5), (200, 300, 4), (300, 500, 0)], bins);
        Assert.Equal(0, bins.Lo);
        Assert.Equal(500, bins.Hi);
    }

    [Fact]
    public void A_value_on_the_last_threshold_counts_in_the_last_bin()
    {
        var bins = Bin([5, 10, 20], 10, [0, 10, 20]);
        Matches([(0, 10, 1), (10, 20, 2)], bins);
    }

    [Fact]
    public void Values_outside_the_thresholds_are_left_out()
    {
        var bins = Bin([-5, 5, 15, 25], 10, [0, 10, 20]);
        Assert.Equal(2, bins.Bins.Sum(b => b.Items.Count));
    }

    [Fact]
    public void Integer_range_extends_the_last_bin_like_d3()
    {
        Matches([(1, 2, 1), (2, 3, 1), (3, 4, 1), (4, 5, 1), (5, 6, 1), (6, 7, 1), (7, 8, 1), (8, 9, 1), (9, 10, 1), (10, 11, 1)], Bin([1, 2, 3, 4, 5, 6, 7, 8, 9, 10], 10));
    }

    [Fact]
    public void Fractional_steps_use_inverse_increments_like_d3()
    {
        Matches([(0, 0.1, 2), (0.1, 0.2, 0), (0.2, 0.3, 0), (0.3, 0.4, 1), (0.4, 0.5, 0), (0.5, 0.6, 1), (0.6, 0.7, 0), (0.7, 0.8, 0), (0.8, 0.9, 0), (0.9, 1, 1)], Bin([0.01, 0.02, 0.37, 0.5, 0.93], 10));
    }

    [Fact]
    public void Negative_values_nice_outwards_like_d3()
    {
        Matches([(-10, -5, 1), (-5, 0, 1), (0, 5, 2), (5, 10, 0), (10, 15, 1)], Bin([-7, -3, 0, 2, 11], 5));
    }

    [Fact]
    public void Identical_values_make_one_unit_bin()
    {
        var bins = Bin([4, 4, 4], 10);
        Matches([(4, 5, 3)], bins);
    }

    [Fact]
    public void No_finite_values_make_no_bins()
    {
        var bins = Bin([double.NaN, double.PositiveInfinity], 10);
        Assert.Empty(bins.Bins);
        Assert.Equal((0d, 1d), (bins.Lo, bins.Hi));
        Assert.Empty(ChartBins.Compute(Array.Empty<double>(), v => v, 10, [0, 10]).Bins);
    }

    [Fact]
    public void Nice_extends_to_round_values_and_stops_on_degenerate_input()
    {
        Assert.Equal((0d, 100d), ChartBins.Nice(3, 97, 10));
        Assert.Equal((5d, 5d), ChartBins.Nice(5, 5, 10));
    }
}

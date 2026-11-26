namespace Maui.Carbon;

/// <summary>A circle of d3's pack layout (x, y, r).</summary>
internal sealed class PackCircle(double x, double y, double r)
{
    public double X = x;
    public double Y = y;
    public double R = r;
}

/// <summary>
/// d3-hierarchy's pack layout (front-chain sibling packing and Welzl's minimal enclosing circle, with d3's seeded LCG so
/// the layout is the same every time), as Carbon's circle pack uses it: <c>pack().size([w, h]).padding(fn)</c>.
/// </summary>
internal static class ChartPack
{
    /// <summary>d3's linear congruential generator (a = 1664525, c = 1013904223, m = 2^32), starting at 1.</summary>
    public static Func<double> Lcg()
    {
        ulong s = 1;
        return () =>
        {
            s = (1664525UL * s + 1013904223UL) % 4294967296UL;
            return s / 4294967296.0;
        };
    }

    /// <summary>
    /// Packs a summed hierarchy into <paramref name="width"/> by <paramref name="height"/>: leaves get radius
    /// sqrt(value), siblings are packed, and <paramref name="padding"/> (of the parent) is kept in pixels after scaling.
    /// </summary>
    public static void Pack(HierarchyNode root, double width, double height, Func<HierarchyNode, double> padding)
    {
        var random = Lcg();
        root.X = width / 2;
        root.Y = height / 2;
        foreach (var n in root.PreOrder())
            if (n.Children is null) n.R = Math.Max(0, Math.Sqrt(Math.Max(0, n.Value)));
        foreach (var n in root.PostOrder()) PackChildren(n, _ => 0, 1, random);
        var k = root.R / Math.Min(width, height);
        foreach (var n in root.PostOrder()) PackChildren(n, padding, k, random);
        var scale = root.R > 0 ? Math.Min(width, height) / (2 * root.R) : 0;
        foreach (var n in root.PreOrder())
        {
            n.R *= scale;
            if (n.Parent is { } p)
            {
                n.X = p.X + scale * n.X;
                n.Y = p.Y + scale * n.Y;
            }
        }
    }

    static void PackChildren(HierarchyNode node, Func<HierarchyNode, double> padding, double k, Func<double> random)
    {
        if (node.Children is not { } children) return;
        var r = padding(node) * k;
        if (double.IsNaN(r)) r = 0;
        if (r != 0) foreach (var c in children) c.R += r;
        var circles = children.Select(c => new PackCircle(0, 0, c.R)).ToList();
        var e = PackSiblings(circles, random);
        for (var i = 0; i < children.Count; i++)
        {
            children[i].X = circles[i].X;
            children[i].Y = circles[i].Y;
        }
        if (r != 0) foreach (var c in children) c.R -= r;
        node.R = e + r;
    }

    sealed class ChainNode(PackCircle c)
    {
        public readonly PackCircle C = c;
        public ChainNode Next = null!;
        public ChainNode Previous = null!;
    }

    /// <summary>d3 <c>packSiblingsRandom</c>: places circles tangent along a front chain; returns the enclosing radius, centered on the origin.</summary>
    public static double PackSiblings(IReadOnlyList<PackCircle> circles, Func<double> random)
    {
        var n = circles.Count;
        if (n == 0) return 0;
        var a0 = circles[0];
        a0.X = 0;
        a0.Y = 0;
        if (n < 2) return a0.R;
        var b0 = circles[1];
        a0.X = -b0.R;
        b0.X = a0.R;
        b0.Y = 0;
        if (n < 3) return a0.R + b0.R;
        Place(b0, a0, circles[2]);
        var a = new ChainNode(a0);
        var b = new ChainNode(b0);
        var c = new ChainNode(circles[2]);
        a.Next = c.Previous = b;
        b.Next = a.Previous = c;
        c.Next = b.Previous = a;
        for (var i = 3; i < n; ++i)
        {
            Place(a.C, b.C, circles[i]);
            c = new ChainNode(circles[i]);
            var j = b.Next;
            var k = a.Previous;
            var sj = b.C.R;
            var sk = a.C.R;
            var restart = false;
            do
            {
                if (sj <= sk)
                {
                    if (Intersects(j.C, c.C))
                    {
                        b = j;
                        a.Next = b;
                        b.Previous = a;
                        --i;
                        restart = true;
                        break;
                    }
                    sj += j.C.R;
                    j = j.Next;
                }
                else
                {
                    if (Intersects(k.C, c.C))
                    {
                        a = k;
                        a.Next = b;
                        b.Previous = a;
                        --i;
                        restart = true;
                        break;
                    }
                    sk += k.C.R;
                    k = k.Previous;
                }
            }
            while (j != k.Next);
            if (restart) continue;
            c.Previous = a;
            c.Next = b;
            a.Next = b.Previous = b = c;
            var aa = Score(a);
            while ((c = c.Next) != b)
            {
                var ca = Score(c);
                if (ca < aa)
                {
                    a = c;
                    aa = ca;
                }
            }
            b = a.Next;
        }
        var chain = new List<PackCircle> { b.C };
        for (var cc = b.Next; cc != b; cc = cc.Next) chain.Add(cc.C);
        var e = Enclose(chain, random);
        foreach (var circle in circles)
        {
            circle.X -= e.X;
            circle.Y -= e.Y;
        }
        return e.R;
    }

    static void Place(PackCircle b, PackCircle a, PackCircle c)
    {
        double dx = b.X - a.X, dy = b.Y - a.Y, d2 = dx * dx + dy * dy;
        if (d2 != 0)
        {
            var a2 = a.R + c.R;
            a2 *= a2;
            var b2 = b.R + c.R;
            b2 *= b2;
            if (a2 > b2)
            {
                var x = (d2 + b2 - a2) / (2 * d2);
                var y = Math.Sqrt(Math.Max(0, b2 / d2 - x * x));
                c.X = b.X - x * dx - y * dy;
                c.Y = b.Y - x * dy + y * dx;
            }
            else
            {
                var x = (d2 + a2 - b2) / (2 * d2);
                var y = Math.Sqrt(Math.Max(0, a2 / d2 - x * x));
                c.X = a.X + x * dx - y * dy;
                c.Y = a.Y + x * dy + y * dx;
            }
        }
        else
        {
            c.X = a.X + c.R;
            c.Y = a.Y;
        }
    }

    static bool Intersects(PackCircle a, PackCircle b)
    {
        var dr = a.R + b.R - 1e-6;
        double dx = b.X - a.X, dy = b.Y - a.Y;
        return dr > 0 && dr * dr > dx * dx + dy * dy;
    }

    static double Score(ChainNode node)
    {
        var a = node.C;
        var b = node.Next.C;
        var ab = a.R + b.R;
        var dx = (a.X * b.R + b.X * a.R) / ab;
        var dy = (a.Y * b.R + b.Y * a.R) / ab;
        return dx * dx + dy * dy;
    }

    /// <summary>d3 <c>packEncloseRandom</c>: the smallest circle enclosing every circle (Welzl's algorithm on a seeded shuffle).</summary>
    public static PackCircle Enclose(IReadOnlyList<PackCircle> input, Func<double> random)
    {
        var circles = input.ToList();
        for (var m = circles.Count; m > 0;)
        {
            var i = (int)(random() * m--);
            (circles[m], circles[i]) = (circles[i], circles[m]);
        }
        var B = new List<PackCircle>();
        PackCircle? e = null;
        for (var i = 0; i < circles.Count;)
        {
            var p = circles[i];
            if (e is not null && EnclosesWeak(e, p)) ++i;
            else
            {
                B = ExtendBasis(B, p);
                e = EncloseBasis(B);
                i = 0;
            }
        }
        return e ?? new PackCircle(0, 0, 0);
    }

    static List<PackCircle> ExtendBasis(List<PackCircle> B, PackCircle p)
    {
        if (EnclosesWeakAll(p, B)) return [p];
        for (var i = 0; i < B.Count; ++i)
            if (EnclosesNot(p, B[i]) && EnclosesWeakAll(EncloseBasis2(B[i], p), B)) return [B[i], p];
        for (var i = 0; i < B.Count - 1; ++i)
            for (var j = i + 1; j < B.Count; ++j)
                if (EnclosesNot(EncloseBasis2(B[i], B[j]), p) && EnclosesNot(EncloseBasis2(B[i], p), B[j])
                    && EnclosesNot(EncloseBasis2(B[j], p), B[i]) && EnclosesWeakAll(EncloseBasis3(B[i], B[j], p), B))
                    return [B[i], B[j], p];
        throw new InvalidOperationException("Circle packing failed to find an enclosing basis.");
    }

    static bool EnclosesNot(PackCircle a, PackCircle b)
    {
        double dr = a.R - b.R, dx = b.X - a.X, dy = b.Y - a.Y;
        return dr < 0 || dr * dr < dx * dx + dy * dy;
    }

    static bool EnclosesWeak(PackCircle a, PackCircle b)
    {
        double dr = a.R - b.R + Math.Max(Math.Max(a.R, b.R), 1) * 1e-9, dx = b.X - a.X, dy = b.Y - a.Y;
        return dr > 0 && dr * dr > dx * dx + dy * dy;
    }

    static bool EnclosesWeakAll(PackCircle a, List<PackCircle> B)
    {
        foreach (var b in B)
            if (!EnclosesWeak(a, b)) return false;
        return true;
    }

    static PackCircle EncloseBasis(List<PackCircle> B) => B.Count switch
    {
        1 => new PackCircle(B[0].X, B[0].Y, B[0].R),
        2 => EncloseBasis2(B[0], B[1]),
        _ => EncloseBasis3(B[0], B[1], B[2]),
    };

    static PackCircle EncloseBasis2(PackCircle a, PackCircle b)
    {
        double x1 = a.X, y1 = a.Y, r1 = a.R, x2 = b.X, y2 = b.Y, r2 = b.R;
        double x21 = x2 - x1, y21 = y2 - y1, r21 = r2 - r1, l = Math.Sqrt(x21 * x21 + y21 * y21);
        return new PackCircle((x1 + x2 + x21 / l * r21) / 2, (y1 + y2 + y21 / l * r21) / 2, (l + r1 + r2) / 2);
    }

    static PackCircle EncloseBasis3(PackCircle a, PackCircle b, PackCircle c)
    {
        double x1 = a.X, y1 = a.Y, r1 = a.R, x2 = b.X, y2 = b.Y, r2 = b.R, x3 = c.X, y3 = c.Y, r3 = c.R;
        double a2 = x1 - x2, a3 = x1 - x3, b2 = y1 - y2, b3 = y1 - y3, c2 = r2 - r1, c3 = r3 - r1;
        var d1 = x1 * x1 + y1 * y1 - r1 * r1;
        var d2 = d1 - x2 * x2 - y2 * y2 + r2 * r2;
        var d3 = d1 - x3 * x3 - y3 * y3 + r3 * r3;
        var ab = a3 * b2 - a2 * b3;
        var xa = (b2 * d3 - b3 * d2) / (ab * 2) - x1;
        var xb = (b3 * c2 - b2 * c3) / ab;
        var ya = (a3 * d2 - a2 * d3) / (ab * 2) - y1;
        var yb = (a2 * c3 - a3 * c2) / ab;
        var A = xb * xb + yb * yb - 1;
        var Bq = 2 * (r1 + xa * xb + ya * yb);
        var Cq = xa * xa + ya * ya - r1 * r1;
        var r = -(Math.Abs(A) > 1e-6 ? (Bq + Math.Sqrt(Bq * Bq - 4 * A * Cq)) / (2 * A) : Cq / Bq);
        return new PackCircle(x1 + xa + xb * r, y1 + ya + yb * r, r);
    }
}

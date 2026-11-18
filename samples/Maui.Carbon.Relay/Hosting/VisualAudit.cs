using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls.Shapes;

namespace Maui.Carbon.Relay.Hosting;

/// <summary>One problem the exercise found, grouped by <see cref="Key"/> across the states it showed up in.</summary>
public sealed record AuditFinding(string Kind, string Key, string Detail, double? Score = null);

/// <summary>
/// Checks a live visual tree for faults a screenshot would only show if someone looked closely:
/// <list type="bullet">
/// <item><b>overflow</b> - a visible element extends past the window's left or right edge outside any horizontal scroller
/// (the outermost such element is reported, not its children).</item>
/// <item><b>contrast</b> - text whose color, composited over the backgrounds behind it, falls below WCAG AA (4.5:1, or
/// 3:1 for large text). Disabled text is skipped.</item>
/// <item><b>collapsed-text</b> - a visible label with text that was laid out with no width or height.</item>
/// <item><b>unsettled</b> - elements whose frame still changes after the screen has gone idle (a layout loop or an
/// animation that resizes).</item>
/// </list>
/// It also remembers every element it sees (weakly), so <see cref="CountAlive"/> can tell which types leak.
/// </summary>
public static class VisualAudit
{
    static readonly ConditionalWeakTable<Element, object> s_seen = [];
    static readonly List<WeakReference<Element>> s_tracked = [];

    public static List<AuditFinding> Audit(Page page)
    {
        var findings = new List<AuditFinding>();
        if (page.Handler is null || page.Width <= 0) return findings;
        var pageBox = new Rect(0, 0, page.Width, page.Height);
        var background = PageBackground(page);
        Walk(page, page, pageBox, background, findings, inHorizontalScroller: false, overflowReported: false, opacity: 1, enabled: true);
        return findings;
    }

    static void Walk(Element element, Page page, Rect pageBox, Color pageBackground, List<AuditFinding> findings,
        bool inHorizontalScroller, bool overflowReported, double opacity, bool enabled)
    {
        Track(element);
        if (element is VisualElement v)
        {
            if (!v.IsVisible || v.Opacity <= 0.01) return;
            // Offscreen by design (closed panels, collapsed side nav slide out).
            if (v.TranslationX != 0 || v.TranslationY != 0) return;
            opacity *= v.Opacity;
            enabled &= v.IsEnabled;
            inHorizontalScroller |= IsHorizontalScroller(v);

            if (v.Handler is not null && v.Width > 0)
            {
                if (!inHorizontalScroller && !overflowReported)
                {
                    var box = new Rect(AbsoluteX(v, page), 0, v.Width, v.Height);
                    var over = Math.Max(box.Right - pageBox.Right, pageBox.Left - box.Left);
                    if (over > 1.5)
                    {
                        overflowReported = true;
                        findings.Add(new AuditFinding("overflow", Signature(v),
                            $"{Describe(v)} spans x {box.Left:0}..{box.Right:0} in a {pageBox.Width:0}px window ({over:0}px out); ancestors: {Chain(v, page)}", over));
                    }
                }
                if (enabled) CheckContrast(v, pageBackground, opacity, findings);
            }
            if (v is Label { Handler: not null } label && HasText(label) && (label.Width <= 0.5 || label.Height <= 0.5) && v.Parent is VisualElement { Width: > 0 })
                findings.Add(new AuditFinding("collapsed-text", Signature(v), $"\"{Snip(TextOf(label))}\" laid out at {label.Width:0.#}x{label.Height:0.#}"));
        }
        if (element is IVisualTreeElement tree)
        {
            foreach (var child in tree.GetVisualChildren())
            {
                if (child is Element e)
                    Walk(e, page, pageBox, pageBackground, findings, inHorizontalScroller, overflowReported, opacity, enabled);
            }
        }
    }

    /// <summary>The element's left edge in page coordinates: frames and translations summed up the parent chain, less
    /// the scroll offset of any scroll view in it.</summary>
    static double AbsoluteX(VisualElement v, Page page)
    {
        double x = 0;
        for (Element? e = v; e is not null && e != page; e = e.Parent)
        {
            if (e is not VisualElement ve) continue;
            x += ve.Frame.X + ve.TranslationX;
            if (e != v && ve is ScrollView sv) x -= sv.ScrollX;
        }
        return x;
    }

    /// <summary>The ancestors' x and width, inner first, to show which one pushed an element out.</summary>
    static string Chain(VisualElement v, Page page)
    {
        var parts = new List<string>();
        for (var e = v.Parent; e is not null && e != page && parts.Count < 7; e = e.Parent)
        {
            if (e is VisualElement ve) parts.Add($"{ve.GetType().Name}@{AbsoluteX(ve, page):0}+{ve.Width:0}");
            // A Grid parent's columns say who took the room.
            if (e is Grid g && g.ColumnDefinitions.Count > 1)
                parts[^1] += $"[cols {string.Join(",", g.ColumnDefinitions.Select(c => $"{c.Width}"))}; children {string.Join(",", g.Children.OfType<VisualElement>().Select(c => $"{c.GetType().Name}:{Grid.GetColumn((BindableObject)c)}={c.Width:0}{(c.IsVisible ? "" : "(hidden)")}"))}]";
        }
        return string.Join(" < ", parts);
    }

    // ---- contrast ----

    static void CheckContrast(VisualElement v, Color pageBackground, double opacity, List<AuditFinding> findings)
    {
        (Color? text, string content, double size, bool bold) = v switch
        {
            Label l when HasText(l) && l.FormattedText is null => (l.TextColor, l.Text, l.FontSize, l.FontAttributes.HasFlag(FontAttributes.Bold)),
            Button b when !string.IsNullOrWhiteSpace(b.Text) => (b.TextColor, b.Text, b.FontSize, b.FontAttributes.HasFlag(FontAttributes.Bold)),
            Entry e when !string.IsNullOrEmpty(e.Text) => (e.TextColor, e.Text, e.FontSize, false),
            Editor e when !string.IsNullOrEmpty(e.Text) => (e.TextColor, e.Text, e.FontSize, false),
            _ => (null, "", 0, false),
        };
        if (content.Length == 0) return;
        var background = EffectiveBackground(v, pageBackground);
        // No TextColor means the platform default: black (the app runs in light mode; Carbon themes the colors).
        var color = text ?? Colors.Black;
        var fg = Blend(color, background, opacity * color.Alpha);
        var ratio = Contrast(fg, background);
        var large = size >= 24 || (bold && size >= 18.66);
        var needed = large ? 3.0 : 4.5;
        if (ratio >= needed) return;
        findings.Add(new AuditFinding("contrast", Signature(v),
            $"\"{Snip(content)}\" {Hex(fg)} on {Hex(background)} = {ratio:0.00}:1 (needs {needed}:1){(text is null ? ", no TextColor set" : "")}", ratio));
    }

    /// <summary>The backgrounds behind an element, composited from the nearest opaque one up.</summary>
    static Color EffectiveBackground(VisualElement v, Color pageBackground)
    {
        var layers = new List<Color>();
        Element? child = null;
        for (Element? e = v; e is not null; child = e, e = e.Parent)
        {
            if (e is not VisualElement ve) continue;
            if (OwnBackground(ve) is { } c)
            {
                layers.Add(c);
                if (c.Alpha >= 0.99) break;
            }
            // Controls often paint a background as a lower sibling layer (a BoxView or Border under the content in
            // the same Grid cell): the nearest one below the child that covers it counts too.
            if (child is VisualElement cv && ve is Layout layout && SiblingBackground(layout, cv) is { } s)
            {
                layers.Add(s);
                if (s.Alpha >= 0.99) break;
            }
        }
        var result = layers.Count > 0 && layers[^1].Alpha >= 0.99 ? layers[^1] : pageBackground;
        for (var i = layers.Count - (layers.Count > 0 && layers[^1].Alpha >= 0.99 ? 2 : 1); i >= 0; i--)
            result = Blend(layers[i], result, layers[i].Alpha);
        return result;
    }

    static Color? SiblingBackground(Layout layout, VisualElement child)
    {
        var index = layout.Children.IndexOf(child);
        for (var i = index - 1; i >= 0; i--)
        {
            if (layout.Children[i] is not VisualElement s || !s.IsVisible || s.Opacity < 0.5) continue;
            var cf = child.Frame;
            // The layer itself, or a layer inside it (an indicator in an indicator layer), offset by its container.
            foreach (var (layer, dx, dy) in Layers(s, 0, 0, 3))
            {
                var f = layer.Frame.Offset(dx, dy);
                if (f.Left <= cf.Left + 1 && f.Top <= cf.Top + 1 && f.Right >= cf.Right - 1 && f.Bottom >= cf.Bottom - 1 && OwnBackground(layer) is { Alpha: >= 0.5f } c)
                    return c;
            }
        }
        return null;
    }

    static IEnumerable<(VisualElement Layer, double Dx, double Dy)> Layers(VisualElement v, double dx, double dy, int depth)
    {
        yield return (v, dx, dy);
        if (depth == 0 || v is not IVisualTreeElement t) yield break;
        foreach (var c in t.GetVisualChildren().OfType<VisualElement>().Where(c => c.IsVisible && c.Opacity >= 0.5))
            foreach (var l in Layers(c, dx + v.Frame.X, dy + v.Frame.Y, depth - 1)) yield return l;
    }

    static Color? OwnBackground(VisualElement v)
    {
        if (v is BoxView { Color: { Alpha: > 0.01f } box }) return box;
        if (v.Background is SolidColorBrush { Color: { Alpha: > 0.01f } brush }) return brush;
        if (v.BackgroundColor is { Alpha: > 0.01f } color) return color;
        if (v is Shape { Fill: SolidColorBrush { Color: { Alpha: > 0.01f } fill } }) return fill;
        return null;
    }

    static Color PageBackground(Page page)
    {
        foreach (var e in Descend(page).Take(4))
        {
            if (e is VisualElement ve && OwnBackground(ve) is { Alpha: >= 0.99f } c) return c;
        }
        return Colors.White;
    }

    static IEnumerable<Element> Descend(Element e)
    {
        yield return e;
        if (e is IVisualTreeElement t)
        {
            foreach (var child in t.GetVisualChildren().OfType<Element>().Take(1))
                foreach (var d in Descend(child)) yield return d;
        }
    }

    static Color Blend(Color top, Color bottom, double alpha)
    {
        alpha = Math.Clamp(alpha, 0, 1);
        return new Color(
            (float)(top.Red * alpha + bottom.Red * (1 - alpha)),
            (float)(top.Green * alpha + bottom.Green * (1 - alpha)),
            (float)(top.Blue * alpha + bottom.Blue * (1 - alpha)));
    }

    static double Contrast(Color a, Color b)
    {
        var la = Luminance(a) + 0.05;
        var lb = Luminance(b) + 0.05;
        return Math.Max(la, lb) / Math.Min(la, lb);
    }

    static double Luminance(Color c)
    {
        static double Channel(float v) => v <= 0.03928 ? v / 12.92 : Math.Pow((v + 0.055) / 1.055, 2.4);
        return 0.2126 * Channel(c.Red) + 0.7152 * Channel(c.Green) + 0.0722 * Channel(c.Blue);
    }

    // ---- layout stability ----

    /// <summary>Every visible element's frame, to compare after the screen has been idle a while.</summary>
    public static Dictionary<VisualElement, Rect> Frames(Page page)
    {
        var frames = new Dictionary<VisualElement, Rect>(ReferenceEqualityComparer.Instance);
        void Visit(Element e)
        {
            if (e is VisualElement v)
            {
                if (!v.IsVisible) return;
                frames[v] = v.Frame;
            }
            if (e is IVisualTreeElement t)
                foreach (var c in t.GetVisualChildren().OfType<Element>()) Visit(c);
        }
        Visit(page);
        return frames;
    }

    public static IEnumerable<AuditFinding> Unsettled(Dictionary<VisualElement, Rect> before, Dictionary<VisualElement, Rect> after)
    {
        foreach (var (v, frame) in after)
        {
            if (!before.TryGetValue(v, out var old)) continue;
            if (Math.Abs(old.Width - frame.Width) > 0.5 || Math.Abs(old.Height - frame.Height) > 0.5 || Math.Abs(old.X - frame.X) > 0.5 || Math.Abs(old.Y - frame.Y) > 0.5)
            {
                // Report the outermost element that moved.
                if (v.Parent is VisualElement p && before.TryGetValue(p, out var pOld) && after.TryGetValue(p, out var pNew) && pOld != pNew) continue;
                yield return new AuditFinding("unsettled", Signature(v), $"{Describe(v)} went from {Fmt(old)} to {Fmt(frame)} while idle");
            }
        }
    }

    // ---- leak tracking ----

    static void Track(Element e)
    {
        if (s_seen.TryGetValue(e, out _)) return;
        s_seen.Add(e, true);
        s_tracked.Add(new WeakReference<Element>(e));
    }

    /// <summary>Registers every element under a root without auditing it.</summary>
    public static void TrackTree(Element root)
    {
        Track(root);
        if (root is IVisualTreeElement t)
            foreach (var c in t.GetVisualChildren().OfType<Element>()) TrackTree(c);
    }

    /// <summary>How many seen elements of each type are still alive (collect garbage first).</summary>
    public static Dictionary<string, int> CountAlive()
    {
        s_tracked.RemoveAll(w => !w.TryGetTarget(out _));
        return s_tracked
            .Select(w => w.TryGetTarget(out var e) ? e.GetType().Name : null)
            .Where(n => n is not null)
            .GroupBy(n => n!)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    /// <summary>
    /// Where the live elements of a type hang: "attached" (still in a window) or the type of the topmost element of a
    /// detached subtree, which is what something outside the visual tree is holding.
    /// </summary>
    public static Dictionary<string, int> Roots(string typeName)
    {
        var roots = new Dictionary<string, int>();
        foreach (var w in s_tracked)
        {
            if (!w.TryGetTarget(out var e) || e.GetType().Name != typeName) continue;
            var top = e;
            while (top.Parent is { } p) top = p;
            var key = top is Application or Window ? "attached"
                : ReferenceEquals(top, e) ? $"orphan ({(e.Handler is null ? "no handler" : "handler connected")}){Sample(e)}"
                : $"detached under {top.GetType().Name}";
            roots[key] = roots.GetValueOrDefault(key) + 1;
        }
        return roots;
    }

    /// <summary>What an orphan shows, to tell which UI it came from.</summary>
    static string Sample(Element e) => e switch
    {
        Label l => $" e.g. \"{Snip(TextOf(l))}\"",
        BoxView b => $" {b.WidthRequest:0}x{b.HeightRequest:0}",
        _ when e.GetType().GetProperty("Icon")?.GetValue(e) is { } icon => $" {icon}",
        _ => "",
    };

    // ---- describing elements ----

    static bool IsHorizontalScroller(VisualElement v) => v switch
    {
        ScrollView s => s.Orientation is ScrollOrientation.Horizontal or ScrollOrientation.Both,
        CarouselView => true,
        ItemsView i when i is StructuredItemsView { ItemsLayout: LinearItemsLayout { Orientation: ItemsLayoutOrientation.Horizontal } } => true,
        _ => false,
    };

    static bool HasText(Label l) => !string.IsNullOrWhiteSpace(TextOf(l));

    static string TextOf(Label l) => l.FormattedText?.ToString() ?? l.Text ?? "";

    /// <summary>
    /// A grouping key: the nearest Relay view, up to three Carbon controls around the element (outer first), and the
    /// element's type. The same fault in every row of a table is one key.
    /// </summary>
    public static string Signature(Element e)
    {
        var carbon = new List<string>();
        string? screen = null;
        for (var p = e.Parent; p is not null && screen is null; p = p.Parent)
        {
            var type = p.GetType();
            if (type.Namespace?.StartsWith("Maui.Carbon.Relay", StringComparison.Ordinal) == true) screen = type.Name;
            else if (type.Name.StartsWith("Cds", StringComparison.Ordinal) && carbon.Count < 3 && (carbon.Count == 0 || carbon[^1] != type.Name)) carbon.Add(type.Name);
        }
        carbon.Reverse();
        return string.Join(" > ", new[] { screen ?? "(detached)" }.Concat(carbon).Append(e.GetType().Name));
    }

    static string Describe(VisualElement v) => v switch
    {
        Label l => $"{v.GetType().Name} \"{Snip(TextOf(l))}\"",
        Button b => $"{v.GetType().Name} \"{Snip(b.Text ?? "")}\"",
        _ => v.GetType().Name,
    };

    static string Snip(string s)
    {
        s = s.ReplaceLineEndings(" ");
        return s.Length > 40 ? s[..37] + "..." : s;
    }

    static string Fmt(Rect r) => $"{r.X:0},{r.Y:0} {r.Width:0}x{r.Height:0}";

    static string Hex(Color c) => c.ToRgbaHex()[..7];
}

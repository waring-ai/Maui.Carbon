namespace Maui.Carbon;

/// <summary>Where a contained list sits.</summary>
public enum CdsContainedListKind
{
    /// <summary>For persistent areas such as cards and sidebars: a semibold header on <c>background</c> with a rule. The default.</summary>
    OnPage,
    /// <summary>For popovers and other elevated layers: a 32px header on <c>layer-01</c> with a small secondary label.</summary>
    Disclosed,
}

/// <summary>Pure helpers of the contained list.</summary>
internal static class CdsContainedListModel
{
    /// <summary>
    /// The text a view shows, for naming a clickable item whose content is a custom body: the texts of its labels (and
    /// spans), in visual-tree order, joined with spaces; <c>null</c> when there is none.
    /// </summary>
    public static string? TextOf(Element? view)
    {
        if (view is null) return null;
        var parts = new List<string>();
        Collect(view, parts);
        return parts.Count == 0 ? null : string.Join(" ", parts);
    }

    static void Collect(Element element, List<string> parts)
    {
        if (CdsDeck.IsHidden(element)) return;
        if (element is Label label)
        {
            var text = label.FormattedText is { } formatted ? string.Concat(formatted.Spans.Select(s => s.Text)) : label.Text;
            if (!string.IsNullOrWhiteSpace(text)) parts.Add(text.Trim());
            return;
        }
        if (element is IVisualTreeElement tree)
            foreach (var child in tree.GetVisualChildren())
                if (child is Element e) Collect(e, parts);
    }
}

/// <summary>Measurements of the contained list (from <c>packages/styles/scss/components/contained-list</c>).</summary>
public static class CdsContainedListMetrics
{
    /// <summary>Height of the header and of a one-line item: sm 32, md 40, lg 48 (default), xl 64. Other sizes use lg.</summary>
    public static double Height(CdsSize size) => size switch
    {
        CdsSize.Xs or CdsSize.Sm => 32,
        CdsSize.Md => 40,
        CdsSize.Xl or CdsSize.Xxl => 64,
        _ => 48,
    };

    /// <summary>Largest height of an action: the list height, but 48 at most (an xl list caps its actions at lg).</summary>
    public static double ActionHeight(CdsSize size) => Math.Min(Height(size), 48);

    /// <summary>Height of the header: the list height on a page, always 32 when disclosed.</summary>
    public static double HeaderHeight(CdsContainedListKind kind, CdsSize size) =>
        kind == CdsContainedListKind.Disclosed ? 32 : Height(size);

    /// <summary>Largest height of the header action: up to xl on a page, 32 when disclosed.</summary>
    public static double HeaderActionHeight(CdsContainedListKind kind, CdsSize size) =>
        kind == CdsContainedListKind.Disclosed ? 32 : Height(size);

    /// <summary>Vertical padding that centers a 20px text line in an item of <paramref name="size"/>.</summary>
    public static double ItemPadding(CdsSize size) => (Height(size) - 20) / 2;

    /// <summary>End padding an item reserves for its action (5rem).</summary>
    public const double ActionReserve = 80;

    /// <summary>Side padding of the header and items (1rem).</summary>
    public const double SidePadding = 16;
}

/// <summary>Maps contained list parts and states to Carbon tokens.</summary>
public static class CdsContainedListTokens
{
    /// <summary>Header fill: <c>background</c> on a page, <c>layer-01</c> when disclosed.</summary>
    public static string HeaderFill(CdsContainedListKind kind) =>
        kind == CdsContainedListKind.Disclosed ? CdsColorKeys.Layer01 : CdsColorKeys.Background;

    /// <summary>Header label color: <c>text-primary</c> on a page, <c>text-secondary</c> when disclosed.</summary>
    public static string HeaderText(CdsContainedListKind kind) =>
        kind == CdsContainedListKind.Disclosed ? CdsColorKeys.TextSecondary : CdsColorKeys.TextPrimary;

    /// <summary>The rule under an on-page header (<c>border-subtle-01</c>); a disclosed header has none.</summary>
    public static string? HeaderRule(CdsContainedListKind kind) =>
        kind == CdsContainedListKind.OnPage ? CdsColorKeys.BorderSubtle01 : null;

    /// <summary>Fill of a clickable item: <c>layer-hover-01</c> on hover, <c>layer-active-01</c> pressed; disabled and plain items have none.</summary>
    public static string? ItemFill(CdsInteractionState state, bool clickable)
    {
        if (!clickable || state.HasFlag(CdsInteractionState.Disabled)) return null;
        if (state.HasFlag(CdsInteractionState.Pressed)) return CdsColorKeys.LayerActive01;
        return state.HasFlag(CdsInteractionState.Hover) ? CdsColorKeys.LayerHover01 : null;
    }

    /// <summary>Item text: <c>text-primary</c>, <c>text-disabled</c> when disabled.</summary>
    public static string ItemText(bool disabled) => disabled ? CdsColorKeys.TextDisabled : CdsColorKeys.TextPrimary;

    /// <summary>
    /// Item icon: <c>icon-primary</c>. Icons are decorative, so a disabled item keeps it; only its text turns
    /// <c>text-disabled</c>.
    /// </summary>
    public static string ItemIcon(bool disabled) => CdsColorKeys.IconPrimary;

    /// <summary>The 1px divider between items.</summary>
    public static string Divider => CdsColorKeys.BorderSubtle01;
}

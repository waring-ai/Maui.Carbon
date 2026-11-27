namespace Maui.Carbon;

/// <summary>
/// Finds where focus goes when a notification that holds focus closes: the next focusable element after it in the visual
/// tree (document order), or the one before it. The search never enters the notification itself and stops at the page.
/// </summary>
internal static class NotificationFocusSearch
{
    /// <summary>The first element after <paramref name="origin"/> in document order that <paramref name="canFocus"/> accepts.</summary>
    public static VisualElement? Next(Element origin, Func<VisualElement, bool> canFocus) => Search(origin, canFocus, forward: true);

    /// <summary>The last element before <paramref name="origin"/> in document order that <paramref name="canFocus"/> accepts.</summary>
    public static VisualElement? Previous(Element origin, Func<VisualElement, bool> canFocus) => Search(origin, canFocus, forward: false);

    static VisualElement? Search(Element origin, Func<VisualElement, bool> canFocus, bool forward)
    {
        for (var current = origin; current.Parent is { } parent && current is not Page; current = parent)
        {
            var siblings = Children(parent);
            var index = siblings.IndexOf(current);
            if (index < 0) continue;
            if (forward)
            {
                for (var i = index + 1; i < siblings.Count; i++)
                    if (Find(siblings[i], canFocus, forward) is { } found) return found;
            }
            else
            {
                for (var i = index - 1; i >= 0; i--)
                    if (Find(siblings[i], canFocus, forward) is { } found) return found;
            }
            if (parent is Page) break;
        }
        return null;
    }

    static VisualElement? Find(Element element, Func<VisualElement, bool> canFocus, bool forward)
    {
        if (CdsDeck.IsHidden(element)) return null;
        if (element is VisualElement visual && canFocus(visual)) return visual;
        var children = Children(element);
        if (forward)
        {
            foreach (var child in children)
                if (Find(child, canFocus, forward) is { } found) return found;
        }
        else
        {
            for (var i = children.Count - 1; i >= 0; i--)
                if (Find(children[i], canFocus, forward) is { } found) return found;
        }
        return null;
    }

    static List<Element> Children(Element element) =>
        ((IVisualTreeElement)element).GetVisualChildren().OfType<Element>().ToList();
}

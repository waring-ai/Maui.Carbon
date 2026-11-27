namespace Maui.Carbon;

/// <summary>Search, filtering, sorting and navigation of Carbon for IBM Products' add-select (pure, unit-tested).</summary>
internal static class AddSelectModel
{
    /// <summary>Every item and its descendants, depth first.</summary>
    public static IEnumerable<CdsAddSelectItem> Flatten(IEnumerable<CdsAddSelectItem> items)
    {
        foreach (var item in items)
        {
            yield return item;
            foreach (var child in Flatten(item.Children)) yield return child;
        }
    }

    public static bool IsHierarchical(IEnumerable<CdsAddSelectItem> items) => items.Any(i => i.Children.Count > 0);

    /// <summary>An attribute of an item: its own fields, then <see cref="CdsAddSelectItem.Attributes"/>.</summary>
    public static string? Attribute(CdsAddSelectItem item, string key) => key switch
    {
        "title" => item.Title,
        "subtitle" => item.Subtitle,
        "value" => item.Value,
        "id" => item.Id,
        _ => item.Attributes.TryGetValue(key, out var v) ? v : null,
    };

    /// <summary>The distinct values of an attribute across every item, in order of first appearance.</summary>
    public static IReadOnlyList<string> FilterOptions(IEnumerable<CdsAddSelectItem> roots, string attribute) =>
        [.. Flatten(roots).Select(i => Attribute(i, attribute)).OfType<string>().Where(v => v.Length > 0).Distinct()];

    /// <summary>Carbon compares the values with their spaces removed.</summary>
    public static int Compare(CdsAddSelectItem a, CdsAddSelectItem b, string attribute, bool descending)
    {
        var x = Attribute(a, attribute)?.Replace(" ", "", StringComparison.Ordinal) ?? "";
        var y = Attribute(b, attribute)?.Replace(" ", "", StringComparison.Ordinal) ?? "";
        var c = string.Compare(x, y, StringComparison.CurrentCulture);
        return descending ? -c : c;
    }

    /// <summary>
    /// The items to list: with a search term or filters, every matching item from the whole tree (title contains the
    /// term, every filter equals), sorted when asked; otherwise the children of the last item in
    /// <paramref name="path"/>, or the top-level items.
    /// </summary>
    public static IReadOnlyList<CdsAddSelectItem> Visible(
        IReadOnlyList<CdsAddSelectItem> roots,
        IReadOnlyList<CdsAddSelectItem> path,
        string? search,
        IReadOnlyDictionary<string, string> filters,
        string? sortAttribute,
        bool descending)
    {
        if (!string.IsNullOrEmpty(search) || filters.Count > 0)
        {
            var term = search ?? "";
            var results = Flatten(roots)
                .Where(i => i.Title.Contains(term, StringComparison.CurrentCultureIgnoreCase))
                .Where(i => filters.All(f => Attribute(i, f.Key) == f.Value))
                .ToList();
            if (!string.IsNullOrEmpty(sortAttribute)) results.Sort((a, b) => Compare(a, b, sortAttribute, descending));
            return results;
        }
        return path.Count > 0 ? [.. path[^1].Children] : roots;
    }
}

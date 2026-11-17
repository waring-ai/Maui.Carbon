using System.Collections;

namespace Maui.Carbon;

/// <summary>One selectable row of the native list.</summary>
/// <param name="Value">The value reported as the selected item.</param>
/// <param name="Text">The row text.</param>
internal readonly record struct SelectEntry(object? Value, string Text);

/// <summary>Pure list-building and selection logic of the select.</summary>
internal static class SelectModel
{
    /// <summary>Flattens declarative options and groups into the rows the native list shows, skipping hidden and disabled items.</summary>
    public static List<SelectEntry> Flatten(IEnumerable<CdsSelectOption> options)
    {
        var result = new List<SelectEntry>();
        foreach (var option in options)
        {
            switch (option)
            {
                case CdsSelectItem item:
                    Add(result, item, groupDisabled: false);
                    break;
                case CdsSelectItemGroup group:
                    foreach (var item in group.Items) Add(result, item, group.IsDisabled);
                    break;
            }
        }
        return result;
    }

    /// <summary>Builds rows from a data source, mapping each item to text with <paramref name="itemToString"/> (default <c>ToString()</c>).</summary>
    public static List<SelectEntry> FromSource(IEnumerable? source, Func<object, string>? itemToString)
    {
        var result = new List<SelectEntry>();
        if (source is null) return result;
        foreach (var item in source)
        {
            if (item is null) continue;
            result.Add(new SelectEntry(item, itemToString?.Invoke(item) ?? item.ToString() ?? string.Empty));
        }
        return result;
    }

    /// <summary>The index of the row whose value equals <paramref name="item"/>, or -1.</summary>
    public static int IndexOf(IReadOnlyList<SelectEntry> entries, object? item)
    {
        if (item is null) return -1;
        for (var i = 0; i < entries.Count; i++)
        {
            if (Equals(entries[i].Value, item)) return i;
        }
        return -1;
    }

    /// <summary>The value at <paramref name="index"/>, or <c>null</c> when it is out of range.</summary>
    public static object? ValueAt(IReadOnlyList<SelectEntry> entries, int index) =>
        index >= 0 && index < entries.Count ? entries[index].Value : null;

    static void Add(List<SelectEntry> result, CdsSelectItem item, bool groupDisabled)
    {
        if (item.IsHidden || item.IsDisabled || groupDisabled) return;
        var text = item.Text ?? item.Value?.ToString() ?? string.Empty;
        result.Add(new SelectEntry(item.Value ?? text, text));
    }
}

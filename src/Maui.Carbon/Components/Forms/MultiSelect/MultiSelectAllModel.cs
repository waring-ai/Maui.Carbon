namespace Maui.Carbon;

/// <summary>
/// The pure rules of the parent "select all" row of a multi-select, copied from Carbon React: the non-filterable variant
/// follows <c>MultiSelect.tsx</c> and <c>internal/Selection.tsx</c>, the filterable one <c>FilterableMultiSelect.tsx</c>.
/// <c>real</c> is always the visible data rows (never the parent row), <c>selected</c> the chosen items in order.
/// </summary>
internal static class MultiSelectAllModel
{
    /// <summary>
    /// Whether the parent row is listed. Filterable: only "if there exist non-disabled filtered items to select"
    /// (FilterableMultiSelect.tsx, <c>selectableRealItems.length &gt; 0</c>). Non-filterable: Carbon's list always contains it.
    /// </summary>
    public static bool IsShown(bool filterable, IReadOnlyList<ListBoxEntry> real) =>
        filterable ? real.Any(r => !r.IsDisabled) : real.Count > 0;

    /// <summary>
    /// The parent's check state: <c>true</c> checked, <c>null</c> mixed, <c>false</c> unchecked.
    /// Non-filterable (MultiSelect.tsx <c>isChecked</c>/<c>isIndeterminate</c>): every selected item counts, disabled or not, against
    /// the number of non-disabled items. Filterable (<c>selectAllStatus</c>): over the non-disabled visible items only.
    /// </summary>
    public static bool? State(bool filterable, IReadOnlyList<ListBoxEntry> real, IReadOnlyList<object?> selected)
    {
        var total = real.Count(r => !r.IsDisabled);
        if (filterable)
        {
            var notSelected = real.Count(r => !r.IsDisabled && !Contains(selected, r.Value));
            if (total > 0 && notSelected == 0) return true;
            return notSelected > 0 && notSelected < total ? null : false;
        }
        var count = selected.Count;
        if (count == total && total > 0) return true;
        return count > 0 && count < total ? null : false;
    }

    /// <summary>
    /// The selection after the parent row was activated. Non-filterable (Selection.tsx): anything selected clears
    /// everything (selected disabled items too), otherwise every non-disabled item is selected. Filterable
    /// (<c>handleSelectAllClick</c>): checked or mixed removes every visible item (disabled ones included, items hidden by the
    /// filter stay); otherwise the non-disabled visible items not yet selected are appended.
    /// </summary>
    public static List<object?> Activate(bool filterable, IReadOnlyList<ListBoxEntry> real, IReadOnlyList<object?> selected)
    {
        if (!filterable)
            return selected.Count > 0 ? [] : [.. real.Where(r => !r.IsDisabled).Select(r => r.Value)];

        if (State(true, real, selected) is not false)
            return [.. selected.Where(s => !real.Any(r => Equals(r.Value, s)))];
        var result = new List<object?>(selected);
        result.AddRange(real.Where(r => !r.IsDisabled && !Contains(selected, r.Value)).Select(r => r.Value));
        return result;
    }

    /// <summary>
    /// Selection.tsx: after an item was added, when <c>filteredItems.length - 1 === newSelectedItems.length + disabledItemCount</c>
    /// the selection becomes every non-disabled item in list order. Returns that selection, or <c>null</c> when the rule does not apply.
    /// </summary>
    public static List<object?>? CollapseOnAdd(IReadOnlyList<ListBoxEntry> real, IReadOnlyList<object?> selected)
    {
        var disabled = real.Count(r => r.IsDisabled);
        if (real.Count != selected.Count + disabled) return null;
        return [.. real.Where(r => !r.IsDisabled).Select(r => r.Value)];
    }

    /// <summary>
    /// Whether the parent row survives the filter text, which Carbon's filterable variant applies to the parent too
    /// (<c>filteredItems.some(isSelectAllItem)</c>); the parent stays listed regardless, but the "all selected" rule above only
    /// applies while it matches.
    /// </summary>
    public static bool MatchesFilter(string label, string? text) =>
        string.IsNullOrEmpty(text) || ListBoxLogic.DefaultFilter(label, text);

    static bool Contains(IReadOnlyList<object?> selected, object? value)
    {
        for (var i = 0; i < selected.Count; i++)
            if (Equals(selected[i], value)) return true;
        return false;
    }
}

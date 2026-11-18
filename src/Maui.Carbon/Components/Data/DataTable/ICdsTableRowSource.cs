namespace Maui.Carbon;

/// <summary>
/// Rows a <see cref="CdsTable"/> asks for on demand (the data table's). The table makes a pool of rows with
/// <see cref="CreateRow"/> and points each at a data row with <see cref="BindRow"/> as the body scrolls, so a row's views
/// must show whatever row it is bound to last.
/// </summary>
internal interface ICdsTableRowSource
{
    /// <summary>The number of data rows.</summary>
    int Count { get; }

    /// <summary>Makes a row for the pool: its cells (one per column), not yet bound to data.</summary>
    CdsTableRow CreateRow();

    /// <summary>Points a pooled row at data row <paramref name="index"/>: texts, selection, expansion and cell state.</summary>
    void BindRow(CdsTableRow row, int index);

    /// <summary>The table no longer uses <paramref name="row"/> (it was rebuilt or the source replaced).</summary>
    void ReleaseRow(CdsTableRow row);

    /// <summary>Whether data row <paramref name="index"/> shows its child row, for rows that are not realized.</summary>
    bool IsExpanded(int index);

    /// <summary>A stable identity of data row <paramref name="index"/>, across sorting and filtering (child heights are remembered by it).</summary>
    object KeyOf(int index);
}

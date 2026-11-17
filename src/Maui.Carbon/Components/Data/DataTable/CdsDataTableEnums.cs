namespace Maui.Carbon;

/// <summary>The sort state of a table column.</summary>
public enum CdsSortDirection
{
    /// <summary>Not sorted (<c>NONE</c>).</summary>
    None,
    /// <summary>Ascending, smallest or A first (<c>ASC</c>).</summary>
    Ascending,
    /// <summary>Descending (<c>DESC</c>).</summary>
    Descending,
}

/// <summary>Horizontal alignment of a table column.</summary>
public enum CdsTableAlignment
{
    /// <summary>Start edge: text and most values.</summary>
    Start,
    /// <summary>End edge: numbers (so digits line up). A sortable end-aligned header puts its label on the end edge with the arrow before it.</summary>
    End,
}

/// <summary>Whether (and how) a <see cref="CdsDataTable"/> lets people select rows.</summary>
public enum CdsDataTableSelection
{
    /// <summary>Rows cannot be selected.</summary>
    None,
    /// <summary>A checkbox per row and an indeterminate select-all checkbox in the header; the batch bar appears while rows are selected.</summary>
    Multi,
    /// <summary>A radio per row; one row at a time.</summary>
    Radio,
}

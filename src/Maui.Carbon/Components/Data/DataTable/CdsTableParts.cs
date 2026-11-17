using System.Collections.ObjectModel;

namespace Maui.Carbon;

/// <summary>
/// A column header of a <see cref="CdsTable"/>: its label, alignment, width and sort state. A sortable header is a button;
/// activating it raises <see cref="Clicked"/> (the table does not reorder rows itself: sort your data, then set
/// <see cref="SortDirection"/>). The active sort header is drawn with <c>layer-selected-hover-01</c> and an arrow
/// (rotated for descending); inactive sortable headers show up-down arrows on hover and focus.
/// </summary>
public class CdsTableHeader : BindableObject
{
    /// <summary>Bindable property for <see cref="Text"/>.</summary>
    public static readonly BindableProperty TextProperty = BindableProperty.Create(
        nameof(Text), typeof(string), typeof(CdsTableHeader), null, propertyChanged: OnStructureChanged);

    /// <summary>Bindable property for <see cref="Align"/>.</summary>
    public static readonly BindableProperty AlignProperty = BindableProperty.Create(
        nameof(Align), typeof(CdsTableAlignment), typeof(CdsTableHeader), CdsTableAlignment.Start, propertyChanged: OnStructureChanged);

    /// <summary>Bindable property for <see cref="Width"/>.</summary>
    public static readonly BindableProperty WidthProperty = BindableProperty.Create(
        nameof(Width), typeof(GridLength), typeof(CdsTableHeader), GridLength.Star, propertyChanged: OnStructureChanged);

    /// <summary>Bindable property for <see cref="IsSortable"/>.</summary>
    public static readonly BindableProperty IsSortableProperty = BindableProperty.Create(
        nameof(IsSortable), typeof(bool?), typeof(CdsTableHeader), null, propertyChanged: OnStructureChanged);

    /// <summary>Bindable property for <see cref="SortDirection"/>.</summary>
    public static readonly BindableProperty SortDirectionProperty = BindableProperty.Create(
        nameof(SortDirection), typeof(CdsSortDirection), typeof(CdsTableHeader), CdsSortDirection.None,
        propertyChanged: static (b, _, _) => ((CdsTableHeader)b).VisualChanged?.Invoke(b, EventArgs.Empty));

    /// <summary>Bindable property for <see cref="Body"/>.</summary>
    public static readonly BindableProperty BodyProperty = BindableProperty.Create(
        nameof(Body), typeof(View), typeof(CdsTableHeader), null, propertyChanged: OnStructureChanged);

    /// <summary>Bindable property for <see cref="Description"/>.</summary>
    public static readonly BindableProperty DescriptionProperty = BindableProperty.Create(
        nameof(Description), typeof(string), typeof(CdsTableHeader), null, propertyChanged: OnStructureChanged);

    internal event EventHandler? Changed;

    internal event EventHandler? VisualChanged;

    /// <summary>Raised when a sortable header is activated (click, tap, Enter or Space).</summary>
    public event EventHandler? Clicked;

    /// <summary>The header label: one or two words, sentence case.</summary>
    public string? Text
    {
        get => (string?)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>Start (default) or end alignment of the header and its cells. Right-align numbers.</summary>
    public CdsTableAlignment Align
    {
        get => (CdsTableAlignment)GetValue(AlignProperty);
        set => SetValue(AlignProperty, value);
    }

    /// <summary>
    /// The column width: a fixed size, <c>Auto</c> or a star share (default <c>*</c>). In a table with
    /// <see cref="CdsTable.StickyHeader"/> the header and the body are separate grids, so <c>Auto</c> columns become <c>*</c> there.
    /// </summary>
    public GridLength Width
    {
        get => (GridLength)GetValue(WidthProperty);
        set => SetValue(WidthProperty, value);
    }

    /// <summary>
    /// Whether the header sorts. <c>null</c> (default) follows <see cref="CdsTable.IsSortable"/>; <c>false</c> opts a column out
    /// when the table is sortable.
    /// </summary>
    public bool? IsSortable
    {
        get => (bool?)GetValue(IsSortableProperty);
        set => SetValue(IsSortableProperty, value);
    }

    /// <summary>The column's current sort: none, ascending or descending. Set it from your own sort handling.</summary>
    public CdsSortDirection SortDirection
    {
        get => (CdsSortDirection)GetValue(SortDirectionProperty);
        set => SetValue(SortDirectionProperty, value);
    }

    /// <summary>A view drawn instead of the label (the select-all checkbox, for example). Such a header is never a sort button.</summary>
    public View? Body
    {
        get => (View?)GetValue(BodyProperty);
        set => SetValue(BodyProperty, value);
    }

    /// <summary>The accessible name when it differs from <see cref="Text"/> (for a header with no visible text, such as "Actions").</summary>
    public string? Description
    {
        get => (string?)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    /// <summary>When true the cell has no padding (columns of controls that bring their own).</summary>
    internal bool IsFlush { get; set; }

    internal void RaiseClicked() => Clicked?.Invoke(this, EventArgs.Empty);

    static void OnStructureChanged(BindableObject b, object? o, object? n) => ((CdsTableHeader)b).Changed?.Invoke(b, EventArgs.Empty);
}

/// <summary>The header row of a <see cref="CdsTable"/>: its <see cref="Headers"/>.</summary>
[ContentProperty(nameof(Headers))]
public class CdsTableHead : BindableObject
{
    readonly ObservableCollection<CdsTableHeader> _headers = [];

    /// <summary>Creates an empty head.</summary>
    public CdsTableHead() => _headers.CollectionChanged += (_, _) => Changed?.Invoke(this, EventArgs.Empty);

    internal event EventHandler? Changed;

    /// <summary>The column headers, in order.</summary>
    public IList<CdsTableHeader> Headers => _headers;

    internal ObservableCollection<CdsTableHeader> HeaderList => _headers;
}

/// <summary>
/// One cell of a <see cref="CdsTableRow"/>: text (drawn in <c>body-compact-01</c>, <c>text-secondary</c>, becoming
/// <c>text-primary</c> when the row is hovered or selected) or any view in <see cref="Body"/>.
/// </summary>
public class CdsTableCell : BindableObject
{
    /// <summary>Bindable property for <see cref="Text"/>.</summary>
    public static readonly BindableProperty TextProperty = BindableProperty.Create(
        nameof(Text), typeof(string), typeof(CdsTableCell), null, propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="Body"/>.</summary>
    public static readonly BindableProperty BodyProperty = BindableProperty.Create(
        nameof(Body), typeof(View), typeof(CdsTableCell), null, propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="Align"/>.</summary>
    public static readonly BindableProperty AlignProperty = BindableProperty.Create(
        nameof(Align), typeof(CdsTableAlignment?), typeof(CdsTableCell), null, propertyChanged: OnChanged);

    /// <summary>Creates an empty cell.</summary>
    public CdsTableCell()
    {
    }

    /// <summary>Creates a text cell.</summary>
    public CdsTableCell(string? text) => Text = text;

    internal event EventHandler? Changed;

    /// <summary>The cell text.</summary>
    public string? Text
    {
        get => (string?)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>A view drawn instead of <see cref="Text"/> (a status indicator, a link, a tag).</summary>
    public View? Body
    {
        get => (View?)GetValue(BodyProperty);
        set => SetValue(BodyProperty, value);
    }

    /// <summary>Overrides the column's alignment; <c>null</c> (default) follows the column header.</summary>
    public CdsTableAlignment? Align
    {
        get => (CdsTableAlignment?)GetValue(AlignProperty);
        set => SetValue(AlignProperty, value);
    }

    /// <summary>When true the cell has no padding (columns of controls that bring their own).</summary>
    internal bool IsFlush { get; set; }

    /// <summary>The cell's view is invisible until the row is hovered or something in it has focus (row menus).</summary>
    internal bool RevealOnHover { get; set; }

    /// <summary>Keeps a reveal-on-hover cell visible (its control has focus or is open).</summary>
    internal bool KeepVisible
    {
        get => _keepVisible;
        set
        {
            if (_keepVisible == value) return;
            _keepVisible = value;
            KeepVisibleChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    bool _keepVisible;

    internal event EventHandler? KeepVisibleChanged;

    static void OnChanged(BindableObject b, object? o, object? n) => ((CdsTableCell)b).Changed?.Invoke(b, EventArgs.Empty);
}

/// <summary>
/// A body row of a <see cref="CdsTable"/>: one <see cref="CdsTableCell"/> per column. Set <see cref="IsSelected"/> for the
/// selected look (<c>layer-selected-01</c>) and, to make the row expandable, <see cref="ExpandedContent"/> with
/// <see cref="IsExpanded"/> (the content sits in a child row indented 56px).
/// </summary>
[ContentProperty(nameof(Cells))]
public class CdsTableRow : BindableObject
{
    /// <summary>Bindable property for <see cref="IsSelected"/>.</summary>
    public static readonly BindableProperty IsSelectedProperty = BindableProperty.Create(
        nameof(IsSelected), typeof(bool), typeof(CdsTableRow), false, propertyChanged: OnVisualChanged);

    /// <summary>Bindable property for <see cref="IsExpanded"/>.</summary>
    public static readonly BindableProperty IsExpandedProperty = BindableProperty.Create(
        nameof(IsExpanded), typeof(bool), typeof(CdsTableRow), false, propertyChanged: OnVisualChanged);

    /// <summary>Bindable property for <see cref="ExpandedContent"/>.</summary>
    public static readonly BindableProperty ExpandedContentProperty = BindableProperty.Create(
        nameof(ExpandedContent), typeof(View), typeof(CdsTableRow), null, propertyChanged: OnVisualChanged);

    readonly ObservableCollection<CdsTableCell> _cells = [];
    bool _isExpandable;

    /// <summary>Creates an empty row.</summary>
    public CdsTableRow() => _cells.CollectionChanged += (_, _) => Changed?.Invoke(this, EventArgs.Empty);

    /// <summary>Creates a row of text cells.</summary>
    public CdsTableRow(params string[] cells) : this()
    {
        foreach (var text in cells) _cells.Add(new CdsTableCell(text));
    }

    internal event EventHandler? Changed;

    internal event EventHandler? VisualChanged;

    /// <summary>The cells, one per column.</summary>
    public IList<CdsTableCell> Cells => _cells;

    /// <summary>The selected look: <c>layer-selected-01</c> fill and <c>text-primary</c> text.</summary>
    public bool IsSelected
    {
        get => (bool)GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    /// <summary>Whether the child row with <see cref="ExpandedContent"/> is shown.</summary>
    public bool IsExpanded
    {
        get => (bool)GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    /// <summary>The content of the child row. Setting it makes the row expandable.</summary>
    public View? ExpandedContent
    {
        get => (View?)GetValue(ExpandedContentProperty);
        set => SetValue(ExpandedContentProperty, value);
    }

    /// <summary>The row reserves a child row even before <see cref="ExpandedContent"/> exists (it is created on first expansion).</summary>
    internal bool IsExpandable
    {
        get => _isExpandable || ExpandedContent is not null;
        set
        {
            if (_isExpandable == value) return;
            _isExpandable = value;
            Changed?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>The source object of a data table row.</summary>
    internal object? Tag { get; set; }

    internal ObservableCollection<CdsTableCell> CellList => _cells;

    static void OnVisualChanged(BindableObject b, object? o, object? n) => ((CdsTableRow)b).VisualChanged?.Invoke(b, EventArgs.Empty);
}

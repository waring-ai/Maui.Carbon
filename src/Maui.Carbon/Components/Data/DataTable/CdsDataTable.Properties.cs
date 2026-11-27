using System.Collections;
using System.Collections.ObjectModel;

namespace Maui.Carbon;

public partial class CdsDataTable
{
    /// <summary>Bindable property for <see cref="ItemsSource"/>.</summary>
    public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(
        nameof(ItemsSource), typeof(IEnumerable), typeof(CdsDataTable), null,
        propertyChanged: static (b, o, n) => ((CdsDataTable)b).OnItemsSourceChanged((IEnumerable?)o, (IEnumerable?)n));

    /// <summary>Bindable property for <see cref="RowKeySelector"/>.</summary>
    public static readonly BindableProperty RowKeySelectorProperty = BindableProperty.Create(
        nameof(RowKeySelector), typeof(Func<object, string>), typeof(CdsDataTable), null, propertyChanged: OnDataChanged);

    /// <summary>Bindable property for <see cref="RowNameSelector"/>.</summary>
    public static readonly BindableProperty RowNameSelectorProperty = BindableProperty.Create(
        nameof(RowNameSelector), typeof(Func<object, string?>), typeof(CdsDataTable), null, propertyChanged: OnDataChanged);

    /// <summary>Bindable property for <see cref="TitleHeadingLevel"/>.</summary>
    public static readonly BindableProperty TitleHeadingLevelProperty = BindableProperty.Create(
        nameof(TitleHeadingLevel), typeof(int), typeof(CdsDataTable), 4,
        coerceValue: static (_, v) => Math.Clamp((int)v, 1, CdsHeadingStyles.MaxLevel),
        propertyChanged: OnChromeChanged);

    /// <summary>Bindable property for <see cref="IsRowDisabled"/>.</summary>
    public static readonly BindableProperty IsRowDisabledProperty = BindableProperty.Create(
        nameof(IsRowDisabled), typeof(Func<object, bool>), typeof(CdsDataTable), null, propertyChanged: OnDataChanged);

    /// <summary>Bindable property for <see cref="Size"/>.</summary>
    public static readonly BindableProperty SizeProperty = BindableProperty.Create(
        nameof(Size), typeof(CdsSize), typeof(CdsDataTable), CdsSize.Lg, propertyChanged: OnLayoutChanged);

    /// <summary>Bindable property for <see cref="UseZebraStyles"/>.</summary>
    public static readonly BindableProperty UseZebraStylesProperty = BindableProperty.Create(
        nameof(UseZebraStyles), typeof(bool), typeof(CdsDataTable), false, propertyChanged: OnLayoutChanged);

    /// <summary>Bindable property for <see cref="IsSortable"/>.</summary>
    public static readonly BindableProperty IsSortableProperty = BindableProperty.Create(
        nameof(IsSortable), typeof(bool), typeof(CdsDataTable), false, propertyChanged: OnLayoutChanged);

    /// <summary>Bindable property for <see cref="Selection"/>.</summary>
    public static readonly BindableProperty SelectionProperty = BindableProperty.Create(
        nameof(Selection), typeof(CdsDataTableSelection), typeof(CdsDataTable), CdsDataTableSelection.None,
        propertyChanged: static (b, _, _) => ((CdsDataTable)b).OnSelectionModeChanged());

    /// <summary>Bindable property for <see cref="IsExpandable"/>.</summary>
    public static readonly BindableProperty IsExpandableProperty = BindableProperty.Create(
        nameof(IsExpandable), typeof(bool), typeof(CdsDataTable), false, propertyChanged: OnLayoutChanged);

    /// <summary>Bindable property for <see cref="ExpandedRowTemplate"/>.</summary>
    public static readonly BindableProperty ExpandedRowTemplateProperty = BindableProperty.Create(
        nameof(ExpandedRowTemplate), typeof(DataTemplate), typeof(CdsDataTable), null, propertyChanged: OnLayoutChanged);

    /// <summary>Bindable property for <see cref="Title"/>.</summary>
    public static readonly BindableProperty TitleProperty = BindableProperty.Create(
        nameof(Title), typeof(string), typeof(CdsDataTable), null, propertyChanged: OnChromeChanged);

    /// <summary>Bindable property for <see cref="Description"/>.</summary>
    public static readonly BindableProperty DescriptionProperty = BindableProperty.Create(
        nameof(Description), typeof(string), typeof(CdsDataTable), null, propertyChanged: OnChromeChanged);

    /// <summary>Bindable property for <see cref="Label"/>.</summary>
    public static readonly BindableProperty LabelProperty = BindableProperty.Create(
        nameof(Label), typeof(string), typeof(CdsDataTable), null, propertyChanged: OnChromeChanged);

    /// <summary>Bindable property for <see cref="Decorator"/>.</summary>
    public static readonly BindableProperty DecoratorProperty = BindableProperty.Create(
        nameof(Decorator), typeof(View), typeof(CdsDataTable), null,
        propertyChanged: static (b, o, n) => ((CdsDataTable)b).OnDecoratorChanged((View?)o, (View?)n));

    /// <summary>Bindable property for <see cref="IsAiEnabled"/>.</summary>
    public static readonly BindableProperty IsAiEnabledProperty = BindableProperty.Create(
        nameof(IsAiEnabled), typeof(bool), typeof(CdsDataTable), false, propertyChanged: OnChromeChanged);

    /// <summary>Bindable property for <see cref="RowDecoratorTemplate"/>.</summary>
    public static readonly BindableProperty RowDecoratorTemplateProperty = BindableProperty.Create(
        nameof(RowDecoratorTemplate), typeof(DataTemplate), typeof(CdsDataTable), null, propertyChanged: OnLayoutChanged);

    /// <summary>Bindable property for <see cref="IsSearchable"/>.</summary>
    public static readonly BindableProperty IsSearchableProperty = BindableProperty.Create(
        nameof(IsSearchable), typeof(bool), typeof(CdsDataTable), false, propertyChanged: OnToolbarChanged);

    /// <summary>Bindable property for <see cref="SearchPlaceholder"/>.</summary>
    public static readonly BindableProperty SearchPlaceholderProperty = BindableProperty.Create(
        nameof(SearchPlaceholder), typeof(string), typeof(CdsDataTable), "Search", propertyChanged: OnToolbarChanged);

    /// <summary>Bindable property for <see cref="IsPersistentSearch"/>.</summary>
    public static readonly BindableProperty IsPersistentSearchProperty = BindableProperty.Create(
        nameof(IsPersistentSearch), typeof(bool), typeof(CdsDataTable), false, propertyChanged: OnToolbarChanged);

    /// <summary>Bindable property for <see cref="ToolbarContent"/>.</summary>
    public static readonly BindableProperty ToolbarContentProperty = BindableProperty.Create(
        nameof(ToolbarContent), typeof(View), typeof(CdsDataTable), null, propertyChanged: OnToolbarChanged);

    /// <summary>Bindable property for <see cref="RowActionsProvider"/>.</summary>
    public static readonly BindableProperty RowActionsProviderProperty = BindableProperty.Create(
        nameof(RowActionsProvider), typeof(Func<object, IEnumerable<CdsDataTableRowAction>>), typeof(CdsDataTable), null, propertyChanged: OnLayoutChanged);

    /// <summary>Bindable property for <see cref="RowActionsHeader"/>.</summary>
    public static readonly BindableProperty RowActionsHeaderProperty = BindableProperty.Create(
        nameof(RowActionsHeader), typeof(string), typeof(CdsDataTable), "Actions", propertyChanged: OnLayoutChanged);

    /// <summary>Bindable property for <see cref="OverflowMenuOnHover"/>.</summary>
    public static readonly BindableProperty OverflowMenuOnHoverProperty = BindableProperty.Create(
        nameof(OverflowMenuOnHover), typeof(bool), typeof(CdsDataTable), true, propertyChanged: OnLayoutChanged);

    /// <summary>Bindable property for <see cref="StickyHeader"/>.</summary>
    public static readonly BindableProperty StickyHeaderProperty = BindableProperty.Create(
        nameof(StickyHeader), typeof(bool), typeof(CdsDataTable), false, propertyChanged: OnLayoutChanged);

    /// <summary>Bindable property for <see cref="VirtualizeInScrollView"/>.</summary>
    public static readonly BindableProperty VirtualizeInScrollViewProperty = BindableProperty.Create(
        nameof(VirtualizeInScrollView), typeof(bool), typeof(CdsDataTable), true, propertyChanged: OnLayoutChanged);

    static readonly BindablePropertyKey VisibleRowCountPropertyKey = BindableProperty.CreateReadOnly(
        nameof(VisibleRowCount), typeof(int), typeof(CdsDataTable), 0);

    /// <summary>Read-only bindable property for <see cref="VisibleRowCount"/>.</summary>
    public static readonly BindableProperty VisibleRowCountProperty = VisibleRowCountPropertyKey.BindableProperty;

    static readonly BindablePropertyKey TotalRowCountPropertyKey = BindableProperty.CreateReadOnly(
        nameof(TotalRowCount), typeof(int), typeof(CdsDataTable), 0);

    /// <summary>Read-only bindable property for <see cref="TotalRowCount"/>.</summary>
    public static readonly BindableProperty TotalRowCountProperty = TotalRowCountPropertyKey.BindableProperty;

    /// <summary>Bindable property for <see cref="EmptyState"/>.</summary>
    public static readonly BindableProperty EmptyStateProperty = BindableProperty.Create(
        nameof(EmptyState), typeof(CdsDataTableEmptyState), typeof(CdsDataTable), null,
        propertyChanged: static (b, o, n) => ((CdsDataTable)b).OnEmptyStateChanged((CdsDataTableEmptyState?)o, (CdsDataTableEmptyState?)n));

    /// <summary>Bindable property for <see cref="EmptyContent"/>.</summary>
    public static readonly BindableProperty EmptyContentProperty = BindableProperty.Create(
        nameof(EmptyContent), typeof(View), typeof(CdsDataTable), null,
        propertyChanged: static (b, _, _) => ((CdsDataTable)b).UpdateEmpty());

    /// <summary>Bindable property for <see cref="SelectedRows"/>.</summary>
    public static readonly BindableProperty SelectedRowsProperty = BindableProperty.Create(
        nameof(SelectedRows), typeof(IList), typeof(CdsDataTable), null, BindingMode.TwoWay,
        propertyChanged: static (b, _, _) => ((CdsDataTable)b).OnSelectedRowsPropertyChanged());

    /// <summary>Bindable property for <see cref="ExpandedRows"/>.</summary>
    public static readonly BindableProperty ExpandedRowsProperty = BindableProperty.Create(
        nameof(ExpandedRows), typeof(IList), typeof(CdsDataTable), null, BindingMode.TwoWay,
        propertyChanged: static (b, _, _) => ((CdsDataTable)b).OnExpandedRowsPropertyChanged());

    /// <summary>Bindable property for <see cref="SortKey"/>.</summary>
    public static readonly BindableProperty SortKeyProperty = BindableProperty.Create(
        nameof(SortKey), typeof(string), typeof(CdsDataTable), null, BindingMode.TwoWay,
        propertyChanged: static (b, _, _) => ((CdsDataTable)b).OnSortPropertyChanged());

    /// <summary>Bindable property for <see cref="SortDirection"/>.</summary>
    public static readonly BindableProperty SortDirectionProperty = BindableProperty.Create(
        nameof(SortDirection), typeof(CdsSortDirection), typeof(CdsDataTable), CdsSortDirection.None, BindingMode.TwoWay,
        propertyChanged: static (b, _, _) => ((CdsDataTable)b).OnSortPropertyChanged());

    /// <summary>Bindable property for <see cref="SearchText"/>.</summary>
    public static readonly BindableProperty SearchTextProperty = BindableProperty.Create(
        nameof(SearchText), typeof(string), typeof(CdsDataTable), null, BindingMode.TwoWay,
        propertyChanged: static (b, _, _) => ((CdsDataTable)b).OnSearchTextPropertyChanged());

    /// <summary>
    /// The data: any objects, one per row. Observe an <see cref="System.Collections.Specialized.INotifyCollectionChanged"/>
    /// source and the table rebuilds when it changes. Keep to about 200 rows and page longer data with a
    /// <see cref="CdsPagination"/> under the table: rows are real views, not virtualized.
    /// </summary>
    public IEnumerable? ItemsSource
    {
        get => (IEnumerable?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    /// <summary>
    /// Returns a row's stable id (<c>row.id</c>). Selection, expansion and sorting follow it, so give every row a unique
    /// value; without one a row's id is its position in <see cref="ItemsSource"/>.
    /// </summary>
    public Func<object, string>? RowKeySelector
    {
        get => (Func<object, string>?)GetValue(RowKeySelectorProperty);
        set => SetValue(RowKeySelectorProperty, value);
    }

    /// <summary>
    /// Returns a row's accessible name, which the row's controls include so a screen reader can tell the rows apart: the
    /// selection checkbox or radio is "Select row, &lt;name&gt;" and the row menu "Options for &lt;name&gt;". Default (or when it
    /// returns null or empty): the row's first non-empty cell text.
    /// </summary>
    public Func<object, string?>? RowNameSelector
    {
        get => (Func<object, string?>?)GetValue(RowNameSelectorProperty);
        set => SetValue(RowNameSelectorProperty, value);
    }

    /// <summary>
    /// The heading level screen readers report for <see cref="Title"/> (1 to 6). Default 4, as in Carbon, where the table
    /// title and the <see cref="CdsPageHeader"/> title are both <c>h4</c>, so a table never outranks the page title. Set it
    /// to fit the page's outline (one below the heading of the section the table is in). The look stays <c>heading-03</c>.
    /// </summary>
    public int TitleHeadingLevel
    {
        get => (int)GetValue(TitleHeadingLevelProperty);
        set => SetValue(TitleHeadingLevelProperty, value);
    }

    /// <summary>A predicate for rows that cannot be selected (<c>row.disabled</c>): their checkbox or radio is disabled and select-all skips them.</summary>
    public Func<object, bool>? IsRowDisabled
    {
        get => (Func<object, bool>?)GetValue(IsRowDisabledProperty);
        set => SetValue(IsRowDisabledProperty, value);
    }

    /// <summary>Row height: xs 24, sm 32, md 40, lg 48 (default), xl 64. Match the toolbar and the pagination.</summary>
    public CdsSize Size
    {
        get => (CdsSize)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    /// <summary>Stripes every second row with <c>layer-accent-01</c>.</summary>
    public bool UseZebraStyles
    {
        get => (bool)GetValue(UseZebraStylesProperty);
        set => SetValue(UseZebraStylesProperty, value);
    }

    /// <summary>Makes every column sortable; set a header's <see cref="CdsDataTableHeader.IsSortable"/> to <c>false</c> to opt it out.</summary>
    public bool IsSortable
    {
        get => (bool)GetValue(IsSortableProperty);
        set => SetValue(IsSortableProperty, value);
    }

    /// <summary>None, <see cref="CdsDataTableSelection.Multi"/> (checkboxes, select-all, batch bar) or <see cref="CdsDataTableSelection.Radio"/> (one row).</summary>
    public CdsDataTableSelection Selection
    {
        get => (CdsDataTableSelection)GetValue(SelectionProperty);
        set => SetValue(SelectionProperty, value);
    }

    /// <summary>Adds an expand chevron column; needs <see cref="ExpandedRowTemplate"/>.</summary>
    public bool IsExpandable
    {
        get => (bool)GetValue(IsExpandableProperty);
        set => SetValue(IsExpandableProperty, value);
    }

    /// <summary>Builds the content of an expanded child row; its binding context is the row object. It is created when a row first expands.</summary>
    public DataTemplate? ExpandedRowTemplate
    {
        get => (DataTemplate?)GetValue(ExpandedRowTemplateProperty);
        set => SetValue(ExpandedRowTemplateProperty, value);
    }

    /// <summary>The title above the toolbar; it also names the table. Sentence case, says what the rows have in common.</summary>
    public string? Title
    {
        get => (string?)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>The description under the title; it describes the table.</summary>
    public string? Description
    {
        get => (string?)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    /// <summary>The accessible name of the table when there is no <see cref="Title"/> (<c>aria-label</c>).</summary>
    public string? Label
    {
        get => (string?)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <summary>
    /// A view at the end of the title block, beside the title and description (Carbon's <c>decorator</c> on <c>TableContainer</c>):
    /// usually a <see cref="CdsAILabel"/> that explains what the AI did to the table. It is placed as it is (the label keeps its size)
    /// and, with no title or description, sits alone at the end. It does not change the table's styling: Carbon keeps that in
    /// <see cref="IsAiEnabled"/>.
    /// </summary>
    public View? Decorator
    {
        get => (View?)GetValue(DecoratorProperty);
        set => SetValue(DecoratorProperty, value);
    }

    /// <summary>
    /// Whether the entire table has AI generated content (Carbon's <c>aiEnabled</c>): a 1px <c>ai-border-start</c> to
    /// <c>ai-border-end</c> gradient border around the table and an <c>ai-aura-start</c> glow rising from the bottom of the body. It is
    /// independent of <see cref="Decorator"/>: set both, with a <see cref="CdsAILabel"/> as the decorator, so the label explains the styling.
    /// Use it when AI is present in all aspects of the table; for a column or a row use the decorators of
    /// <see cref="CdsDataTableHeader.Decorator"/> and <see cref="RowDecoratorTemplate"/>.
    /// </summary>
    public bool IsAiEnabled
    {
        get => (bool)GetValue(IsAiEnabledProperty);
        set => SetValue(IsAiEnabledProperty, value);
    }

    /// <summary>
    /// Builds the AI label of each row (Carbon's <c>TableDecoratorRow</c>) in a 16 wide column before every other column; the
    /// view's binding context is the row object. A row whose view is a shown <see cref="CdsAILabel"/> is an AI row: an
    /// <c>ai-aura-start-sm</c> gradient across the row, a 1px <c>ai-border-strong</c> line on its start edge and the AI hover and
    /// selected fills; bind <c>IsVisible</c> to hide the label on a row that is not AI. The AI styling stays while a label shows its
    /// revert button (<see cref="CdsAILabel.IsRevertActive"/>), as in Carbon. Rows are re-bound as the table scrolls, so bind everything
    /// the label shows to the row object; the label is named with the row ("AI, Show information, cluster-01"). Without a template the
    /// table has no AI rows and builds nothing for them.
    /// </summary>
    public DataTemplate? RowDecoratorTemplate
    {
        get => (DataTemplate?)GetValue(RowDecoratorTemplateProperty);
        set => SetValue(RowDecoratorTemplateProperty, value);
    }

    /// <summary>Adds the toolbar search (<c>search</c>): a 48 magnifier that expands to a field; Escape clears it.</summary>
    public bool IsSearchable
    {
        get => (bool)GetValue(IsSearchableProperty);
        set => SetValue(IsSearchableProperty, value);
    }

    /// <summary>The search hint and its (hidden) accessible name. Default "Search"; say what is searched ("Filter clusters").</summary>
    public string? SearchPlaceholder
    {
        get => (string?)GetValue(SearchPlaceholderProperty);
        set => SetValue(SearchPlaceholderProperty, value);
    }

    /// <summary>Keeps the search field open instead of collapsing it to the magnifier.</summary>
    public bool IsPersistentSearch
    {
        get => (bool)GetValue(IsPersistentSearchProperty);
        set => SetValue(IsPersistentSearchProperty, value);
    }

    /// <summary>
    /// Buttons at the end of the toolbar (put several in a <c>HorizontalStackLayout</c>): at most five global actions, the
    /// primary one last. Use <c>Size="Lg"</c> buttons to fill the 48 high toolbar (<c>Sm</c> for xs and sm tables).
    /// </summary>
    public View? ToolbarContent
    {
        get => (View?)GetValue(ToolbarContentProperty);
        set => SetValue(ToolbarContentProperty, value);
    }

    /// <summary>The batch bar actions (multi selection): label, icon and a command that receives the selected rows.</summary>
    public IList<CdsDataTableBatchAction> BatchActions => _batchActions;

    /// <summary>The row menu actions: a last column with an overflow menu per row.</summary>
    public IList<CdsDataTableRowAction> RowActions => _rowActions;

    /// <summary>Chooses the row menu actions per row, instead of <see cref="RowActions"/> (<c>rowActions</c> as a function).</summary>
    public Func<object, IEnumerable<CdsDataTableRowAction>>? RowActionsProvider
    {
        get => (Func<object, IEnumerable<CdsDataTableRowAction>>?)GetValue(RowActionsProviderProperty);
        set => SetValue(RowActionsProviderProperty, value);
    }

    /// <summary>The hidden header text of the menu column (<c>rowActionsHeader</c>), read by screen readers. Default "Actions".</summary>
    public string? RowActionsHeader
    {
        get => (string?)GetValue(RowActionsHeaderProperty);
        set => SetValue(RowActionsHeaderProperty, value);
    }

    /// <summary>
    /// Shows a row's menu trigger only while the row is hovered or the trigger has focus (default). Touch platforms always show it;
    /// set <c>false</c> to keep the triggers visible everywhere.
    /// </summary>
    public bool OverflowMenuOnHover
    {
        get => (bool)GetValue(OverflowMenuOnHoverProperty);
        set => SetValue(OverflowMenuOnHoverProperty, value);
    }

    /// <summary>
    /// Pins the header while the body scrolls inside a 300 high area. Header and body are separate grids then, so give
    /// columns fixed or star widths (<c>Auto</c> columns become star columns). The body is virtualized: only the rows in
    /// view are built, rows are one line high (longer text is truncated), and rows are re-bound to other data as it scrolls.
    /// </summary>
    public bool StickyHeader
    {
        get => (bool)GetValue(StickyHeaderProperty);
        set => SetValue(StickyHeaderProperty, value);
    }

    /// <summary>
    /// In a vertical <see cref="ScrollView"/> (and without <see cref="StickyHeader"/>), builds only the rows in or near the
    /// scroll view's viewport, and re-binds them as the page scrolls. Default <c>true</c>. Rows then keep the row height
    /// (cell text truncates instead of wrapping) and rows out of view are not in the accessibility tree; set <c>false</c>
    /// to build every row. A table with an <c>Auto</c> column always builds every row (the column is as wide as its widest
    /// cell).
    /// </summary>
    public bool VirtualizeInScrollView
    {
        get => (bool)GetValue(VirtualizeInScrollViewProperty);
        set => SetValue(VirtualizeInScrollViewProperty, value);
    }

    /// <summary>What shows under the header when no rows match: a title, one sentence and at most one action.</summary>
    public CdsDataTableEmptyState? EmptyState
    {
        get => (CdsDataTableEmptyState?)GetValue(EmptyStateProperty);
        set => SetValue(EmptyStateProperty, value);
    }

    /// <summary>
    /// The number of rows that pass the search: what the table shows, across every page of a virtualized table. Read-only
    /// and bindable (<see cref="VisibleRowCountProperty"/>, raises <c>PropertyChanged</c>), updated whenever the rows,
    /// <see cref="SearchText"/> or anything else changes the visible set, so a page can show and announce "Showing
    /// <see cref="VisibleRowCount"/> of <see cref="TotalRowCount"/>" while a search filters the table.
    /// </summary>
    public int VisibleRowCount => (int)GetValue(VisibleRowCountProperty);

    /// <summary>
    /// The number of rows in <see cref="ItemsSource"/> (null items are skipped), whatever the search hides. Read-only and
    /// bindable (<see cref="TotalRowCountProperty"/>, raises <c>PropertyChanged</c>); updated when the source or its
    /// collection changes.
    /// </summary>
    public int TotalRowCount => (int)GetValue(TotalRowCountProperty);

    /// <summary>Your own empty-state view; it replaces <see cref="EmptyState"/>.</summary>
    public View? EmptyContent
    {
        get => (View?)GetValue(EmptyContentProperty);
        set => SetValue(EmptyContentProperty, value);
    }

    /// <summary>
    /// The selected row objects (<c>selectedIds</c>), two-way. The user's changes assign a new list; assign your own to select
    /// rows from code. Rows that are not in <see cref="ItemsSource"/> are ignored.
    /// </summary>
    public IList SelectedRows
    {
        get => (IList?)GetValue(SelectedRowsProperty) ?? Array.Empty<object>();
        set => SetValue(SelectedRowsProperty, value);
    }

    /// <summary>The expanded row objects (<c>expandedIds</c>), two-way, with the same rules as <see cref="SelectedRows"/>.</summary>
    public IList ExpandedRows
    {
        get => (IList?)GetValue(ExpandedRowsProperty) ?? Array.Empty<object>();
        set => SetValue(ExpandedRowsProperty, value);
    }

    /// <summary>The key of the sorted column (two-way). Pair it with <see cref="SortDirection"/>.</summary>
    public string? SortKey
    {
        get => (string?)GetValue(SortKeyProperty);
        set => SetValue(SortKeyProperty, value);
    }

    /// <summary>The sort direction (two-way); <see cref="CdsSortDirection.None"/> shows the rows in source order.</summary>
    public CdsSortDirection SortDirection
    {
        get => (CdsSortDirection)GetValue(SortDirectionProperty);
        set => SetValue(SortDirectionProperty, value);
    }

    /// <summary>The search text (two-way): rows with a cell containing it (ignoring case) stay.</summary>
    public string? SearchText
    {
        get => (string?)GetValue(SearchTextProperty);
        set => SetValue(SearchTextProperty, value);
    }

    /// <summary>The columns, in order (the content property).</summary>
    public IList<CdsDataTableHeader> Headers => _headers;

    static void OnDataChanged(BindableObject b, object? o, object? n) => ((CdsDataTable)b).Reload(structure: false);

    static void OnLayoutChanged(BindableObject b, object? o, object? n) => ((CdsDataTable)b).Render();

    static void OnChromeChanged(BindableObject b, object? o, object? n) => ((CdsDataTable)b).UpdateChrome();

    static void OnToolbarChanged(BindableObject b, object? o, object? n) => ((CdsDataTable)b).UpdateToolbar();

    readonly CdsItemCollection<CdsDataTableHeader> _headers = [];
    readonly CdsItemCollection<CdsDataTableBatchAction> _batchActions = [];
    readonly CdsItemCollection<CdsDataTableRowAction> _rowActions = [];
}

using System.Collections;
using System.Collections.Specialized;

namespace Maui.Carbon;

/// <summary>
/// Structured rows and columns that people scan, sort, search, select and act on. It is declarative: define
/// <see cref="Headers"/> (<see cref="CdsDataTableHeader"/>) and set <see cref="ItemsSource"/>, and the table builds the
/// title, toolbar, header, selection, expansion and row menus (on <see cref="CdsTable"/>, so the styling is shared).
/// <list type="bullet">
/// <item>Sorting cycles none, ascending, descending per column (<see cref="IsSortable"/>); numbers sort numerically, text by the current culture, and the sort is stable.</item>
/// <item><see cref="Selection"/> adds checkboxes with an indeterminate select-all (<see cref="CdsDataTableSelection.Multi"/>) or radios; while rows are selected the batch bar of <see cref="BatchActions"/> replaces the toolbar.</item>
/// <item><see cref="IsExpandable"/> with <see cref="ExpandedRowTemplate"/> adds a chevron column and a child row; <see cref="RowActions"/> adds an overflow menu per row; <see cref="IsSearchable"/> adds the toolbar search; <see cref="StickyHeader"/> pins the header above a 300 high scrolling body.</item>
/// <item><see cref="SelectedRows"/>, <see cref="ExpandedRows"/>, <see cref="SortKey"/> with <see cref="SortDirection"/> and <see cref="SearchText"/> are two-way, so you can control or persist them.</item>
/// </list>
/// With <see cref="StickyHeader"/> the table owns its scrolling and is virtualized: only the rows in view are built, and they
/// are re-bound to other rows as the body scrolls, so bind everything a <see cref="CdsDataTableHeader.CellTemplate"/> or
/// <see cref="ExpandedRowTemplate"/> shows to its item. Without it every row is a real view: keep to about 200 and put a
/// <see cref="CdsPagination"/> directly under the table for more. Use
/// <see cref="CdsDataTableSkeleton"/> while the first rows load, and a <see cref="CdsStructuredList"/> for a handful of read-only values.
/// </summary>
[ContentProperty(nameof(Headers))]
public partial class CdsDataTable : CdsControl, ICdsTableRowSource
{
    readonly DataTableModel _model = new();
    readonly CdsTable _table = new() { VerticalOptions = LayoutOptions.Start };
    readonly VerticalStackLayout _root = new() { Spacing = 0 };
    readonly VerticalStackLayout _titleBlock = new() { Spacing = 0, Padding = new Thickness(CdsSpacing.Spacing05, CdsSpacing.Spacing05, CdsSpacing.Spacing05, CdsSpacing.Spacing06) };
    readonly CdsText _title = new() { TypeStyle = CdsTypeStyle.Heading03 };
    readonly CdsText _description = new() { TypeStyle = CdsTypeStyle.BodyCompact01, Tone = CdsTextTone.Secondary, MaximumWidthRequest = 640, HorizontalOptions = LayoutOptions.Start };
    readonly Grid _toolbar = new();
    readonly Grid _toolbarRow = new() { ColumnSpacing = 0 };
    readonly ContentView _toolbarContentHost = new();
    readonly CdsSearch _search = new();
    readonly Grid _batchBar = new() { IsVisible = false, ColumnSpacing = 0 };
    readonly CdsText _batchText = new() { TypeStyle = CdsTypeStyle.BodyCompact01, Tone = CdsTextTone.OnColor, VerticalOptions = LayoutOptions.Center };
    readonly HorizontalStackLayout _batchActionsHost = new() { Spacing = 0 };
    readonly Grid _empty = new() { IsVisible = false, Padding = new Thickness(CdsSpacing.Spacing05, CdsSpacing.Spacing08) };
    readonly CdsCheckbox _headerCheck = new() { Label = "Select all rows", HideLabel = true };
    readonly Dictionary<string, RowRef> _refs = [];
    readonly Dictionary<CdsTableRow, RowRef> _rowRefs = [];
    readonly Stack<CdsTableRow> _spareRows = new();
    readonly List<(CdsTableHeader Header, string Key)> _dataHeaders = [];
    bool _rendered;
    readonly Dictionary<object, string> _itemKeys = [];
    INotifyCollectionChanged? _observed;
    bool _syncing;
    bool _batchShown;

    /// <summary>Creates an empty table.</summary>
    public CdsDataTable()
    {
        HorizontalOptions = LayoutOptions.Fill;
        VerticalOptions = LayoutOptions.Start;

        // Title block.
        CdsTokens.SetColor(_titleBlock, BackgroundColorProperty, CdsTableTokens.Background);
        _titleBlock.Add(_title);
        _titleBlock.Add(_description);
        _description.Margin = new Thickness(0, CdsSpacing.Spacing02, 0, 0);

        // Toolbar: [spacer][search][content], with the batch bar over it.
        CdsTokens.SetColor(_toolbar, BackgroundColorProperty, CdsTableTokens.Toolbar);
        _toolbarRow.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        _toolbarRow.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        _toolbarRow.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        _toolbarRow.Add(_search, 1, 0);
        _toolbarRow.Add(_toolbarContentHost, 2, 0);
        CdsSemantics.SetGroupName(_toolbarRow, "data table toolbar");
        _toolbar.Add(_toolbarRow);
        _toolbar.Add(_batchBar);
        _search.TextChanged += (_, e) => OnToolbarSearchChanged(e.NewTextValue);
        _search.ExpandedChanged += (_, _) => UpdateToolbarColumns();

        // Batch bar: [count][actions][cancel].
        CdsTokens.SetColor(_batchBar, BackgroundColorProperty, CdsTableTokens.BatchBar);
        _batchBar.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        _batchBar.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        _batchBar.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        _batchText.Margin = new Thickness(CdsSpacing.Spacing05, 0);
        _batchBar.Add(_batchText, 0, 0);
        _batchBar.Add(_batchActionsHost, 1, 0);

        // Empty state block.
        CdsTokens.SetColor(_empty, BackgroundColorProperty, CdsTableTokens.Background);

        _root.Add(_titleBlock);
        _root.Add(_toolbar);
        _root.Add(_table);
        _root.Add(_empty);
        SetChrome(_root);

        _headerCheck.Clicked += (_, _) => OnSelectAllActivated();
        _headers.CollectionChanged += OnHeadersChanged;
        _batchActions.CollectionChanged += (_, e) =>
        {
            Attach(e);
            RebuildBatchBar();
            UpdateToolbar();
        };
        _rowActions.CollectionChanged += (_, e) =>
        {
            Attach(e);
            Render();
        };
        Loaded += (_, _) => UpdateAttachedPagination();

        UpdateChrome();
        UpdateToolbar();
        Reload(structure: true);
    }

    /// <summary>Raised after the user changed the selection (<c>onSelectionChange</c>), with the selected row objects.</summary>
    public event EventHandler<CdsDataTableRowsEventArgs>? SelectionChanged;

    /// <summary>Raised after the user expanded or collapsed a row (<c>onExpandChange</c>), with the expanded row objects.</summary>
    public event EventHandler<CdsDataTableRowsEventArgs>? ExpandedChanged;

    /// <summary>Raised after the user changed the sort (<c>onSortChange</c>).</summary>
    public event EventHandler<CdsDataTableSortEventArgs>? SortChanged;

    /// <summary>Raised when the search text changes (<c>onSearchChange</c>).</summary>
    public event EventHandler<TextChangedEventArgs>? SearchChanged;

    /// <summary>The number of selected rows (what the batch bar counts, including rows hidden by the search).</summary>
    public int SelectedCount => _model.SelectedCount;

    /// <summary>Clears the selection (what the batch bar's Cancel does).</summary>
    public void ClearSelection()
    {
        if (!_model.ClearSelection()) return;
        OnSelectionChanged();
    }

    /// <summary>The underlying table, for tests and advanced styling.</summary>
    internal CdsTable Table => _table;

    internal CdsSearch SearchField => _search;

    // ----- data -----

    void OnItemsSourceChanged(IEnumerable? oldSource, IEnumerable? newSource)
    {
        if (_observed is not null) _observed.CollectionChanged -= OnSourceChanged;
        _observed = newSource as INotifyCollectionChanged;
        if (_observed is not null) _observed.CollectionChanged += OnSourceChanged;
        Reload(structure: false);
    }

    void OnSourceChanged(object? sender, NotifyCollectionChangedEventArgs e) => Reload(structure: false);

    void OnHeadersChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        Attach(e);
        Reload(structure: true);
    }

    void Attach(NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
        {
            foreach (var item in e.OldItems)
            {
                switch (item)
                {
                    case CdsDataTableHeader h: h.Changed -= OnPartChanged; break;
                    case CdsDataTableBatchAction b: b.Changed -= OnBatchActionChanged; break;
                    case CdsDataTableRowAction r: r.Changed -= OnPartChanged; break;
                }
            }
        }
        if (e.NewItems is not null)
        {
            foreach (var item in e.NewItems)
            {
                if (item is BindableObject part) SetInheritedBindingContext(part, BindingContext);
                switch (item)
                {
                    case CdsDataTableHeader h: h.Changed += OnPartChanged; break;
                    case CdsDataTableBatchAction b: b.Changed += OnBatchActionChanged; break;
                    case CdsDataTableRowAction r: r.Changed += OnPartChanged; break;
                }
            }
        }
    }

    void OnPartChanged(object? sender, EventArgs e) => Reload(structure: true);

    void OnBatchActionChanged(object? sender, EventArgs e) => RebuildBatchBar();

    /// <summary>Re-reads the rows from <see cref="ItemsSource"/> and redraws (the whole table when the columns may have changed).</summary>
    internal void Reload(bool structure = false)
    {
        var selectionChanged = _model.SetRows(BuildData());
        _model.SetColumns(_headers.Select((h, i) => ColumnKey(h, i)).ToList());
        _model.SetSort(SortKey, SortDirection);
        _model.SearchText = SearchText ?? string.Empty;
        ApplyPropertiesToModel();
        if (structure) Render();
        else RefreshRows();
        if (selectionChanged || _model.SelectedCount != SelectedRows.Count) PublishSelection(raise: false);
    }

    static string ColumnKey(CdsDataTableHeader header, int index) => header.Key ?? index.ToString(System.Globalization.CultureInfo.InvariantCulture);

    List<DataTableRowData> BuildData()
    {
        var list = new List<DataTableRowData>();
        _itemKeys.Clear();
        if (ItemsSource is null) return list;
        var used = new HashSet<string>();
        var index = 0;
        foreach (var item in ItemsSource)
        {
            if (item is null) continue;
            var key = RowKeySelector?.Invoke(item) ?? $"row-{index}";
            if (!used.Add(key))
            {
                var n = 2;
                while (!used.Add($"{key}#{n}")) n++;
                key = $"{key}#{n}";
            }
            index++;
            var values = new object?[_headers.Count];
            var texts = new string[_headers.Count];
            for (var c = 0; c < _headers.Count; c++)
            {
                values[c] = _headers[c].ValueOf(item);
                texts[c] = _headers[c].TextOf(values[c]);
            }
            list.Add(new DataTableRowData(key, item, values, texts, IsRowDisabled?.Invoke(item) ?? false));
            _itemKeys.TryAdd(item, key);
        }
        return list;
    }

    /// <summary>Pushes the two-way properties (selection, expansion) into the model after the rows changed.</summary>
    void ApplyPropertiesToModel()
    {
        // Merge: rows that stay selected keep their state, and rows named by the properties join (order of binding does not matter).
        _model.ReplaceSelection(_model.SelectedKeys.Concat(KeysOf(SelectedRows)).Distinct().ToList());
        _model.ReplaceExpanded(_model.ExpandedKeys.Concat(KeysOf(ExpandedRows)).Distinct().ToList());
        EnforceSelectionMode();
    }

    /// <summary>No selection column means nothing is selected; radio keeps at most one row.</summary>
    bool EnforceSelectionMode()
    {
        if (Selection == CdsDataTableSelection.None) return _model.ClearSelection();
        if (Selection == CdsDataTableSelection.Radio && _model.SelectedCount > 1)
        {
            var first = _model.SelectedRows.First().Key;
            return _model.SelectOnly(first);
        }
        return false;
    }

    void OnSelectionModeChanged()
    {
        ApplyPropertiesToModel();
        Render();
        PublishSelection(raise: false);
    }

    IEnumerable<string> KeysOf(IList items)
    {
        foreach (var item in items)
            if (item is not null && _itemKeys.TryGetValue(item, out var key)) yield return key;
    }

    List<object> ItemsOf(IEnumerable<DataTableRowData> rows) => rows.Select(r => r.Item!).ToList();

    // ----- render -----

    CdsSize EffectiveSize => Size > CdsSize.Xl ? CdsSize.Xl : Size;

    /// <summary>
    /// Builds the table's structure (head, columns, size, the rows' views) from scratch. With a sticky header the table
    /// asks for rows on demand (only those in view are realized); otherwise every visible row is a row of the table.
    /// </summary>
    void Render()
    {
        var size = EffectiveSize;
        var selection = Selection;
        var rowActions = HasRowActions;
        _refs.Clear();
        _rowRefs.Clear();
        _spareRows.Clear();

        _table.BatchUpdate(() =>
        {
            _table.Size = size;
            _table.UseZebraStyles = UseZebraStyles;
            _table.StickyHeader = StickyHeader;
            _table.IsSortable = false;
            _table.ChildInset = CdsTableTokens.ChildInset(selection);
            _table.Label = Title ?? Label;
            _table.Head = BuildHead(selection, rowActions);
            _table.Rows.Clear();
            _table.RowSource = StickyHeader ? this : null;
            if (!StickyHeader)
            {
                var visible = _model.VisibleRows;
                for (var i = 0; i < visible.Count; i++)
                {
                    var row = CreateRow();
                    BindRow(row, i);
                    _table.Rows.Add(row);
                }
            }
        });
        _rendered = true;
        AfterRows();
        UpdateChrome();
        UpdateToolbar();
    }

    /// <summary>
    /// The rows changed (sorted, searched, reloaded) but not the columns: re-binds the existing rows to the visible data,
    /// adding or removing rows at the end, instead of building every row's views again.
    /// </summary>
    void RefreshRows()
    {
        if (!_rendered)
        {
            Render();
            return;
        }
        foreach (var (header, key) in _dataHeaders)
            header.SortDirection = _model.SortKey == key ? _model.SortDirection : CdsSortDirection.None;

        _table.BatchUpdate(() =>
        {
            if (_table.RowSource is not null)
            {
                _table.InvalidateRows();
                return;
            }
            var visible = _model.VisibleRows;
            var rows = _table.Rows;
            // Rows taken off the end are kept and re-added (in the same order) when the list grows again, so the table
            // shows their views again instead of building new ones.
            while (rows.Count > visible.Count)
            {
                var last = rows[^1];
                rows.RemoveAt(rows.Count - 1);
                UnmapRow(last);
                _spareRows.Push(last);
            }
            for (var i = 0; i < rows.Count; i++) BindRow(rows[i], i);
            for (var i = rows.Count; i < visible.Count; i++)
            {
                var row = _spareRows.Count > 0 ? _spareRows.Pop() : CreateRow();
                BindRow(row, i);
                rows.Add(row);
            }
        });
        AfterRows();
    }

    void AfterRows()
    {
        PublishRowCounts();
        SyncAll();
        UpdateEmpty();
        UpdateAttachedPagination();
    }

    bool HasRowActions => RowActions.Count > 0 || RowActionsProvider is not null;

    bool CanExpand => IsExpandable && ExpandedRowTemplate is not null;

    CdsTableHead BuildHead(CdsDataTableSelection selection, bool rowActions)
    {
        var head = new CdsTableHead();
        _dataHeaders.Clear();
        if (CanExpand)
            head.HeaderList.Add(new CdsTableHeader { Width = new GridLength(CdsTableTokens.ExpandColumnWidth), IsSortable = false, Description = "Expand", Body = new BoxView { WidthRequest = 1, HeightRequest = 1, Color = Colors.Transparent, InputTransparent = true }, IsFlush = true });
        if (selection == CdsDataTableSelection.Multi)
        {
            _headerCheck.Margin = new Thickness(CdsTableTokens.CellPaddingX, 0, 0, 0);
            _headerCheck.HorizontalOptions = LayoutOptions.Start;
            head.HeaderList.Add(new CdsTableHeader { Width = new GridLength(CdsTableTokens.CheckboxColumnWidth), IsSortable = false, Body = _headerCheck, Description = "Select all rows", IsFlush = true });
        }
        else if (selection == CdsDataTableSelection.Radio)
        {
            head.HeaderList.Add(new CdsTableHeader { Width = new GridLength(CdsTableTokens.RadioColumnWidth), IsSortable = false, Body = new BoxView { WidthRequest = 1, HeightRequest = 1, Color = Colors.Transparent, InputTransparent = true }, Description = "Select row", IsFlush = true });
        }

        for (var i = 0; i < _headers.Count; i++)
        {
            var source = _headers[i];
            var key = ColumnKey(source, i);
            var header = new CdsTableHeader
            {
                Text = source.Header,
                Align = source.Align,
                Width = source.Width,
                IsSortable = source.IsSortable ?? IsSortable,
                SortDirection = _model.SortKey == key ? _model.SortDirection : CdsSortDirection.None,
            };
            header.Clicked += (_, _) => OnHeaderActivated(key);
            head.HeaderList.Add(header);
            _dataHeaders.Add((header, key));
        }

        if (rowActions)
            head.HeaderList.Add(new CdsTableHeader { Width = new GridLength(CdsTableTokens.MenuColumnWidth), IsSortable = false, Description = RowActionsHeader, Text = null, Body = new BoxView { WidthRequest = 1, HeightRequest = 1, Color = Colors.Transparent, InputTransparent = true }, IsFlush = true });
        return head;
    }

    // ----- rows (the table's row source) -----

    int ICdsTableRowSource.Count => _model.VisibleRows.Count;

    CdsTableRow ICdsTableRowSource.CreateRow() => CreateRow();

    void ICdsTableRowSource.BindRow(CdsTableRow row, int index) => BindRow(row, index);

    void ICdsTableRowSource.ReleaseRow(CdsTableRow row) => ReleaseRow(row);

    bool ICdsTableRowSource.IsExpanded(int index) => _model.IsExpanded(_model.VisibleRows[index].Key);

    object ICdsTableRowSource.KeyOf(int index) => _model.VisibleRows[index].Key;

    /// <summary>
    /// Makes a row's cells and controls, not yet showing any data. Its controls act on whatever data row it is bound to
    /// when they are used, so the same row can be re-bound as the table scrolls or sorts.
    /// </summary>
    CdsTableRow CreateRow()
    {
        var size = EffectiveSize;
        var selection = Selection;
        var row = new CdsTableRow { IsExpandable = CanExpand };
        var reference = new RowRef { Row = row };
        _rowRefs[row] = reference;
        var topAligned = size >= CdsSize.Xl;

        if (CanExpand) row.CellList.Add(new CdsTableCell { Body = BuildExpandButton(reference), IsFlush = true });

        if (selection == CdsDataTableSelection.Multi)
        {
            var check = new CdsCheckbox { Label = "Select row", HideLabel = true, HorizontalOptions = LayoutOptions.Start };
            check.Margin = new Thickness(CdsTableTokens.CellPaddingX, topAligned ? CdsSpacing.Spacing04 : 0, 0, 0);
            check.VerticalOptions = topAligned ? LayoutOptions.Start : LayoutOptions.Center;
            check.Clicked += (_, _) =>
            {
                if (reference.Data is { } data) OnRowCheckActivated(data.Key);
            };
            reference.Check = check;
            row.CellList.Add(new CdsTableCell { Body = check, IsFlush = true });
        }
        else if (selection == CdsDataTableSelection.Radio)
        {
            var radio = new CdsRadioButton { Label = "Select row", HideLabel = true, HorizontalOptions = LayoutOptions.Start };
            radio.Margin = new Thickness(CdsTableTokens.CellPaddingX, topAligned ? CdsSpacing.Spacing04 : 0, 0, 0);
            radio.VerticalOptions = topAligned ? LayoutOptions.Start : LayoutOptions.Center;
            radio.CheckedChanged += (_, e) =>
            {
                if (!_syncing && e.Value && reference.Data is { } data) OnRowRadioChosen(data.Key);
            };
            reference.Radio = radio;
            row.CellList.Add(new CdsTableCell { Body = radio, IsFlush = true });
        }

        for (var c = 0; c < _headers.Count; c++)
        {
            CdsTableCell cell;
            View? view = null;
            if (_headers[c].CellTemplate is { } template)
            {
                view = (View)template.CreateContent();
                cell = new CdsTableCell { Body = view };
            }
            else
            {
                cell = new CdsTableCell();
            }
            reference.DataCells.Add(cell);
            reference.TemplateViews.Add(view);
            row.CellList.Add(cell);
        }

        if (HasRowActions) row.CellList.Add(BuildMenuCell(reference, size));
        return row;
    }

    /// <summary>Points a row at visible data row <paramref name="index"/>: texts, template contexts, menus, selection and expansion.</summary>
    void BindRow(CdsTableRow row, int index)
    {
        if (!_rowRefs.TryGetValue(row, out var r)) return;
        var data = _model.VisibleRows[index];
        if (r.Data is { } old && _refs.TryGetValue(old.Key, out var current) && current == r) _refs.Remove(old.Key);
        r.Data = data;
        _refs[data.Key] = r;
        row.Tag = data.Item;

        var wasSyncing = _syncing;
        _syncing = true;
        try
        {
            for (var c = 0; c < r.DataCells.Count; c++)
            {
                if (r.TemplateViews[c] is { } view)
                {
                    if (!ReferenceEquals(view.BindingContext, data.Item)) view.BindingContext = data.Item;
                }
                else
                {
                    r.DataCells[c].Text = c < data.Texts.Count ? data.Texts[c] : null;
                }
            }
            var selected = _model.IsSelected(data.Key);
            row.IsSelected = selected;
            // Every row control is named by its row: a screen reader has no table context to tell the rows apart.
            var selectName = $"Select row, {RowName(data)}";
            if (r.Check is { } check)
            {
                check.Label = selectName;
                check.IsEnabled = !data.IsDisabled;
                if (check.IsChecked != selected) check.IsChecked = selected;
            }
            if (r.Radio is { } radio)
            {
                radio.Label = selectName;
                radio.IsEnabled = !data.IsDisabled;
                if (radio.IsChecked != selected) radio.IsChecked = selected;
            }
            if (r.Menu is { } menu)
            {
                menu.IconDescription = $"Options for {RowName(data)}";
                if (RowActionsProvider is { } provider && data.Item is not null) FillMenu(menu, r, provider(data.Item));
            }
            SyncExpansion(data.Key, r, animate: false);
        }
        finally
        {
            _syncing = wasSyncing;
        }
    }

    void ReleaseRow(CdsTableRow row)
    {
        UnmapRow(row);
        _rowRefs.Remove(row);
    }

    /// <summary>The row no longer shows its data row (it is spare or released).</summary>
    void UnmapRow(CdsTableRow row)
    {
        if (_rowRefs.TryGetValue(row, out var r) && r.Data is { } data && _refs.TryGetValue(data.Key, out var current) && current == r)
            _refs.Remove(data.Key);
    }

    /// <summary>The row's accessible name: <see cref="RowNameSelector"/>, else its first non-empty cell text, else its key.</summary>
    string RowName(DataTableRowData data)
    {
        if (data.Item is not null && RowNameSelector?.Invoke(data.Item) is { Length: > 0 } name) return name;
        foreach (var text in data.Texts)
            if (!string.IsNullOrEmpty(text)) return text;
        return data.Key;
    }

    CdsTableCell BuildMenuCell(RowRef reference, CdsSize size)
    {
        var menu = new CdsOverflowMenu
        {
            Size = CdsTableTokens.MenuTriggerSize(size),
            IsFlipped = true,
            HorizontalOptions = LayoutOptions.End,
            VerticalOptions = size >= CdsSize.Xl ? LayoutOptions.Start : LayoutOptions.Center,
        };
        if (size >= CdsSize.Xl) menu.Margin = new Thickness(0, CdsSpacing.Spacing03, 0, 0);
        if (RowActionsProvider is null) FillMenu(menu, reference, RowActions);
        var cell = new CdsTableCell { Body = menu, IsFlush = true, RevealOnHover = OverflowMenuOnHover };
        void Keep() => cell.KeepVisible = menu.Is(CdsInteractionState.Focused) || menu.IsOpen;
        menu.StateChanged += (_, _) => Keep();
        menu.Opened += (_, _) => Keep();
        menu.Closed += (_, _) => Keep();
        reference.Menu = menu;
        return cell;
    }

    /// <summary>Fills a row menu; each item acts on the row's data at the time it is chosen.</summary>
    static void FillMenu(CdsOverflowMenu menu, RowRef reference, IEnumerable<CdsDataTableRowAction> actions)
    {
        menu.Items.Clear();
        foreach (var action in actions)
        {
            // The item's command acts on the row bound at the time; the menu reads its CanExecute when it opens, so an action
            // whose command cannot run for this row is shown disabled instead of doing nothing.
            var item = new CdsMenuItem(action.Label ?? string.Empty, action.Icon)
            {
                Kind = action.Kind,
                IsEnabled = action.IsEnabled,
                Command = new RowActionCommand(action, reference),
            };
            menu.Items.Add(item);
        }
    }

    View BuildExpandButton(RowRef reference)
    {
        var icon = new CdsIcon(CdsIconName.ChevronRight, 16) { HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center, InputTransparent = true };
        CdsTokens.SetColor(icon, CdsIcon.ColorProperty, CdsTableTokens.ExpandIcon);
        var ring = new CdsFocusRing(CdsFocusRingStyle.Inset);
        var hit = new CdsHitTarget { Role = CdsAccessibilityRole.Expander, IsExpanded = false };
        SemanticProperties.SetDescription(hit, "Expand current row");
        hit.Clicked += (_, _) =>
        {
            if (reference.Data is { } data) OnExpandActivated(data.Key);
        };
        hit.Focused += (_, _) => ring.IsActive = hit.IsFocusFromKeyboard;
        hit.Unfocused += (_, _) => ring.IsActive = false;
        var host = new Grid { HorizontalOptions = LayoutOptions.Fill, VerticalOptions = LayoutOptions.Fill };
        host.Add(icon);
        host.Add(ring);
        host.Add(hit);
        reference.ExpandIcon = icon;
        reference.ExpandHit = hit;
        return host;
    }

    // ----- state sync -----

    /// <summary>Brings every row's selection and expansion visuals, the select-all checkbox and the batch bar in line with the model.</summary>
    void SyncAll()
    {
        _syncing = true;
        try
        {
            _table.BatchUpdate(SyncRows);
        }
        finally
        {
            _syncing = false;
        }
        UpdateBatchBar();
    }

    void SyncRows()
    {
        foreach (var (key, r) in _refs)
        {
            var selected = _model.IsSelected(key);
            r.Row.IsSelected = selected;
            if (r.Check is not null && r.Check.IsChecked != selected) r.Check.IsChecked = selected;
            if (r.Radio is not null && r.Radio.IsChecked != selected) r.Radio.IsChecked = selected;
            SyncExpansion(key, r);
        }
        var all = _model.SelectAllState;
        _headerCheck.IsChecked = all == true;
        _headerCheck.IsIndeterminate = all is null;
    }

    void SyncExpansion(string key, RowRef r, bool animate = true)
    {
        var expanded = _model.IsExpanded(key);
        var item = r.Data?.Item;
        if (r.Row.ExpandedContent is { } existing)
        {
            // A re-bound row keeps its child content and shows the new row's data in it.
            if (!ReferenceEquals(existing.BindingContext, item)) existing.BindingContext = item;
        }
        else if (expanded && ExpandedRowTemplate is { } template && item is not null)
        {
            var content = (View)template.CreateContent();
            content.BindingContext = item;
            r.Row.ExpandedContent = content;
        }
        r.Row.IsExpanded = expanded;
        if (r.ExpandHit is not null)
        {
            r.ExpandHit.IsExpanded = expanded;
            SemanticProperties.SetDescription(r.ExpandHit, expanded ? "Collapse current row" : "Expand current row");
        }
        if (r.ExpandIcon is { } icon)
        {
            var target = expanded ? 90d : 0d;
            if (!animate)
            {
                icon.AbortAnimation("cds-expand");
                icon.Rotation = target;
            }
            else if (Math.Abs(icon.Rotation - target) > 0.1)
            {
                _ = CdsMotion.AnimateAsync(icon, "cds-expand", v => icon.Rotation = v, icon.Rotation, target, CdsDuration.Moderate01, CdsEasing.StandardProductive);
            }
        }
    }

    // ----- user actions -----

    void OnHeaderActivated(string key)
    {
        _model.CycleSort(key);
        PublishSort();
        RefreshRows();
        SortChanged?.Invoke(this, new CdsDataTableSortEventArgs(_model.SortKey, _model.SortDirection));
        CdsAnnouncer.Announce(_model.SortKey is null
            ? "Sorting cleared"
            : $"Sorted by {HeaderText(_model.SortKey)}, {(_model.SortDirection == CdsSortDirection.Ascending ? "ascending" : "descending")}");
    }

    string HeaderText(string key)
    {
        for (var i = 0; i < _headers.Count; i++)
            if (ColumnKey(_headers[i], i) == key) return _headers[i].Header ?? key;
        return key;
    }

    void PublishSort()
    {
        _syncing = true;
        try
        {
            SortKey = _model.SortKey;
            SortDirection = _model.SortDirection;
        }
        finally
        {
            _syncing = false;
        }
    }

    void OnSortPropertyChanged()
    {
        if (_syncing) return;
        _model.SetSort(SortKey, SortDirection);
        RefreshRows();
    }

    void OnSearchTextPropertyChanged()
    {
        if (_syncing) return;
        var text = SearchText ?? string.Empty;
        if (_search.Text != text) _search.Text = text;
        if (_search.IsExpandable && text.Length > 0) _search.IsExpanded = true;
        _model.SearchText = text;
        RefreshRows();
    }

    void OnToolbarSearchChanged(string? text)
    {
        text ??= string.Empty;
        if (_model.SearchText == text) return;
        _syncing = true;
        try
        {
            SearchText = text;
        }
        finally
        {
            _syncing = false;
        }
        _model.SearchText = text;
        RefreshRows();
        SearchChanged?.Invoke(this, new TextChangedEventArgs(string.Empty, text));
    }

    void OnSelectAllActivated()
    {
        _model.ToggleSelectAll();
        OnSelectionChanged();
    }

    void OnRowCheckActivated(string key)
    {
        _model.Toggle(key);
        OnSelectionChanged();
    }

    void OnRowRadioChosen(string key)
    {
        if (_model.SelectOnly(key)) OnSelectionChanged();
    }

    void OnSelectionChanged()
    {
        SyncAll();
        PublishSelection(raise: true);
        if (Selection == CdsDataTableSelection.Multi) CdsAnnouncer.Announce(DataTableModel.BatchText(_model.SelectedCount));
    }

    void PublishSelection(bool raise)
    {
        var rows = ItemsOf(_model.SelectedRows);
        _syncing = true;
        try
        {
            SetValue(SelectedRowsProperty, rows);
        }
        finally
        {
            _syncing = false;
        }
        if (raise) SelectionChanged?.Invoke(this, new CdsDataTableRowsEventArgs(rows));
    }

    void OnSelectedRowsPropertyChanged()
    {
        if (_syncing) return;
        var changed = _model.ReplaceSelection(KeysOf(SelectedRows));
        if (EnforceSelectionMode()) PublishSelection(raise: false);
        if (changed) SyncAll();
    }

    void OnExpandActivated(string key)
    {
        _model.ToggleExpanded(key);
        if (_refs.TryGetValue(key, out var r)) SyncExpansion(key, r);
        RefreshVisuals();
        var rows = ItemsOf(_model.Rows.Where(row => _model.IsExpanded(row.Key)));
        _syncing = true;
        try
        {
            SetValue(ExpandedRowsProperty, rows);
        }
        finally
        {
            _syncing = false;
        }
        ExpandedChanged?.Invoke(this, new CdsDataTableRowsEventArgs(rows));
    }

    void OnExpandedRowsPropertyChanged()
    {
        if (_syncing) return;
        if (!_model.ReplaceExpanded(KeysOf(ExpandedRows))) return;
        foreach (var (key, r) in _refs) SyncExpansion(key, r);
        _table.InvalidateRows(); // rows out of view may have opened or closed: the rows below them move
        RefreshVisuals();
    }

    // ----- chrome -----

    void UpdateChrome()
    {
        _title.Text = Title;
        SemanticProperties.SetHeadingLevel(_title, CdsHeadingStyles.ToSemantic(TitleHeadingLevel));
        _description.Text = Description;
        _title.IsVisible = !string.IsNullOrEmpty(Title);
        _description.IsVisible = !string.IsNullOrEmpty(Description);
        _titleBlock.IsVisible = _title.IsVisible || _description.IsVisible;
        _table.Label = Title ?? Label;
        // Shown as text above the table; also the table body's group hint. Never a Hint on the table itself: that makes
        // the whole table one VoiceOver element.
        _table.AccessibleHint = Description;
    }

    bool BatchAvailable => Selection == CdsDataTableSelection.Multi && _batchActions.Count > 0;

    void UpdateToolbar()
    {
        var height = CdsTableTokens.ToolbarHeight(EffectiveSize);
        var small = height < CdsSizing.Large;
        _search.IsVisible = IsSearchable;
        _search.Size = small ? CdsSize.Sm : CdsSize.Lg;
        _search.Label = SearchPlaceholder;
        _search.Placeholder = SearchPlaceholder;
        _search.IsExpandable = !IsPersistentSearch;
        _toolbarContentHost.Content = ToolbarContent;
        _toolbar.HeightRequest = height;
        _batchBar.HeightRequest = height;
        _toolbar.IsVisible = IsSearchable || ToolbarContent is not null || BatchAvailable;
        UpdateToolbarColumns();
        UpdateBatchBar();
    }

    void UpdateToolbarColumns()
    {
        // Collapsed search sits at the end beside the buttons; expanded (or persistent) it takes the free space.
        var wide = IsSearchable && (IsPersistentSearch || _search.IsExpanded);
        _toolbarRow.ColumnDefinitions[0].Width = wide ? GridLength.Auto : GridLength.Star;
        _toolbarRow.ColumnDefinitions[1].Width = wide ? GridLength.Star : GridLength.Auto;
    }

    void RebuildBatchBar()
    {
        _batchActionsHost.Children.Clear();
        while (_batchBar.Children.Count > 2) _batchBar.Children.RemoveAt(2);
        var height = CdsTableTokens.ToolbarHeight(EffectiveSize);
        foreach (var action in _batchActions)
        {
            var button = new CdsBatchBarButton(action.Label ?? string.Empty, action.Icon, height);
            var captured = action;
            button.Clicked += (_, _) => captured.Invoke(ItemsOf(_model.SelectedRows));
            // The button is disabled while the action's command cannot run for the selected rows.
            if (action.Command is not null) button.SetAvailability(new BatchActionCommand(action, this));
            _batchActionsHost.Add(button);
        }
        var divider = new BoxView { WidthRequest = 1, HeightRequest = CdsSpacing.Spacing05, VerticalOptions = LayoutOptions.Center, InputTransparent = true };
        CdsTokens.SetColor(divider, BoxView.ColorProperty, CdsTableTokens.BatchText);
        var cancel = new CdsBatchBarButton("Cancel", null, height);
        cancel.Clicked += (_, _) => ClearSelection();
        var cancelHost = new Grid();
        cancelHost.Add(cancel);
        cancelHost.Add(divider);
        divider.HorizontalOptions = LayoutOptions.Start;
        _batchBar.Add(cancelHost, 2, 0);
    }

    void UpdateBatchBar()
    {
        var show = BatchAvailable && _model.SelectedCount > 0;
        _batchText.Text = DataTableModel.BatchText(_model.SelectedCount);
        if (_batchActionsHost.Children.Count == 0 && _batchActions.Count > 0) RebuildBatchBar();
        // CanExecute takes the selected rows, so it is asked again whenever the selection changes.
        foreach (var child in _batchActionsHost.Children)
            if (child is CdsBatchBarButton button) button.RefreshAvailability();
        if (show == _batchShown) return;
        _batchShown = show;
        _batchBar.IsVisible = show;
        _toolbarRow.IsVisible = !show;
        if (show)
        {
            _batchBar.Opacity = 0;
            _ = CdsMotion.AnimateAsync(_batchBar, "cds-batch", v => _batchBar.Opacity = v, 0, 1, CdsDuration.Fast02, CdsEasing.StandardProductive);
        }
    }

    void OnEmptyStateChanged(CdsDataTableEmptyState? oldState, CdsDataTableEmptyState? newState)
    {
        if (oldState is not null) oldState.Changed -= OnEmptyChanged;
        if (newState is not null)
        {
            newState.Changed += OnEmptyChanged;
            SetInheritedBindingContext(newState, BindingContext);
        }
        UpdateEmpty();
    }

    void OnEmptyChanged(object? sender, EventArgs e) => UpdateEmpty();

    void UpdateEmpty()
    {
        var show = _model.VisibleRows.Count == 0 && (EmptyContent is not null || EmptyState is not null);
        _empty.IsVisible = show;
        _empty.Children.Clear();
        if (!show) return;
        if (EmptyContent is { } content)
        {
            if (content.Parent is Layout old) old.Remove(content);
            _empty.Add(content);
            return;
        }
        var state = EmptyState!;
        var stack = new VerticalStackLayout { Spacing = 0, HorizontalOptions = LayoutOptions.Center };
        if (!string.IsNullOrEmpty(state.Title))
            stack.Add(new CdsText(state.Title, CdsTypeStyle.HeadingCompact02) { HorizontalTextAlignment = TextAlignment.Center });
        if (!string.IsNullOrEmpty(state.Description))
            stack.Add(new CdsText(state.Description, CdsTypeStyle.BodyCompact01, CdsTextTone.Secondary)
            {
                HorizontalTextAlignment = TextAlignment.Center,
                MaximumWidthRequest = 288,
                Margin = new Thickness(0, CdsSpacing.Spacing03, 0, 0),
            });
        if (!string.IsNullOrEmpty(state.ActionText))
        {
            var action = new CdsButton(state.ActionText, CdsButtonKind.Tertiary) { HorizontalOptions = LayoutOptions.Center, Margin = new Thickness(0, CdsSpacing.Spacing06, 0, 0) };
            // The button runs the command itself, so it draws disabled (and its hit target is disabled) while
            // CanExecute(ActionCommandParameter) is false, and follows CanExecuteChanged, as every CdsButton does.
            action.Clicked += (_, _) => state.RaiseActionInvoked();
            action.Command = state.ActionCommand;
            action.CommandParameter = state.ActionCommandParameter;
            stack.Add(action);
        }
        _empty.Add(stack);
    }

    /// <summary>The empty-state action button, while it is shown (for tests).</summary>
    internal CdsButton? EmptyActionButton =>
        _empty.Children.OfType<VerticalStackLayout>().SelectMany(s => s.Children).OfType<CdsButton>().FirstOrDefault();

    void PublishRowCounts()
    {
        if (VisibleRowCount != _model.VisibleRows.Count) SetValue(VisibleRowCountPropertyKey, _model.VisibleRows.Count);
        if (TotalRowCount != _model.Rows.Count) SetValue(TotalRowCountPropertyKey, _model.Rows.Count);
    }

    /// <inheritdoc/>
    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        // The parts declared inside the table are not in the visual tree: they get its context, as logical children do,
        // so a binding on a header, batch or row action or the empty state resolves. A context set on a part is kept.
        foreach (var header in _headers) SetInheritedBindingContext(header, BindingContext);
        foreach (var action in _batchActions) SetInheritedBindingContext(action, BindingContext);
        foreach (var action in _rowActions) SetInheritedBindingContext(action, BindingContext);
        if (EmptyState is { } state) SetInheritedBindingContext(state, BindingContext);
    }

    /// <summary>Drops the top border of a <see cref="CdsPagination"/> placed directly under the table.</summary>
    void UpdateAttachedPagination()
    {
        if (Parent is not Layout layout) return;
        var index = layout.Children.IndexOf(this);
        if (index >= 0 && index + 1 < layout.Children.Count && layout.Children[index + 1] is CdsPagination pagination)
            pagination.RefreshAttachment();
    }

    /// <inheritdoc/>
    protected override void OnParentSet()
    {
        base.OnParentSet();
        UpdateAttachedPagination();
    }

    /// <summary>The selected row objects, as the batch actions receive them.</summary>
    internal IReadOnlyList<object> SelectedRowItems => ItemsOf(_model.SelectedRows);

    /// <summary>
    /// Gates a batch button on its action's command: <c>CanExecute</c> with the selected rows, re-asked when the command
    /// raises <c>CanExecuteChanged</c> (the button subscribes weakly while it is on screen). Running is the button's click.
    /// </summary>
    sealed class BatchActionCommand(CdsDataTableBatchAction action, CdsDataTable table) : System.Windows.Input.ICommand
    {
        readonly WeakReference<CdsDataTable> _table = new(table);

        public event EventHandler? CanExecuteChanged
        {
            add { if (action.Command is { } command) command.CanExecuteChanged += value; }
            remove { if (action.Command is { } command) command.CanExecuteChanged -= value; }
        }

        public bool CanExecute(object? parameter) =>
            action.Command is not { } command || !_table.TryGetTarget(out var t) || command.CanExecute(t.SelectedRowItems);

        public void Execute(object? parameter)
        {
        }
    }

    /// <summary>
    /// The command of a row menu item: it can run when the action's command can run for the row bound now, and running it
    /// invokes the action with that row.
    /// </summary>
    sealed class RowActionCommand(CdsDataTableRowAction action, RowRef reference) : System.Windows.Input.ICommand
    {
        // The menu asks CanExecute each time it opens; nothing listens between openings.
        public event EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }

        public bool CanExecute(object? parameter) =>
            reference.Data?.Item is { } row && (action.Command?.CanExecute(row) ?? true);

        public void Execute(object? parameter)
        {
            if (reference.Data?.Item is { } row) action.Invoke(row);
        }
    }

    sealed class RowRef
    {
        /// <summary>The data row this table row shows now (rows are re-bound when sorted, searched or scrolled).</summary>
        public DataTableRowData? Data;
        public readonly List<CdsTableCell> DataCells = [];
        public readonly List<View?> TemplateViews = [];
        public CdsTableRow Row = null!;
        public CdsCheckbox? Check;
        public CdsRadioButton? Radio;
        public CdsHitTarget? ExpandHit;
        public CdsIcon? ExpandIcon;
        public CdsOverflowMenu? Menu;
    }
}

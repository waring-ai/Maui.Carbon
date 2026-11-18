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
/// Rows are real views: keep to about 200 and put a <see cref="CdsPagination"/> directly under the table for more. Use
/// <see cref="CdsDataTableSkeleton"/> while the first rows load, and a <see cref="CdsStructuredList"/> for a handful of read-only values.
/// </summary>
[ContentProperty(nameof(Headers))]
public partial class CdsDataTable : CdsControl
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
        SemanticProperties.SetHeadingLevel(_title, SemanticHeadingLevel.Level2);
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
        SemanticProperties.SetDescription(_toolbarRow, "data table toolbar");
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
        Reload();
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

    /// <summary>The number of rows that pass the search.</summary>
    public int VisibleRowCount => _model.VisibleRows.Count;

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
        Reload();
    }

    void OnSourceChanged(object? sender, NotifyCollectionChangedEventArgs e) => Reload();

    void OnHeadersChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        Attach(e);
        Reload();
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
                switch (item)
                {
                    case CdsDataTableHeader h: h.Changed += OnPartChanged; break;
                    case CdsDataTableBatchAction b: b.Changed += OnBatchActionChanged; break;
                    case CdsDataTableRowAction r: r.Changed += OnPartChanged; break;
                }
            }
        }
    }

    void OnPartChanged(object? sender, EventArgs e) => Reload();

    void OnBatchActionChanged(object? sender, EventArgs e) => RebuildBatchBar();

    /// <summary>Re-reads the rows from <see cref="ItemsSource"/> and redraws.</summary>
    void Reload()
    {
        var selectionChanged = _model.SetRows(BuildData());
        _model.SetColumns(_headers.Select((h, i) => ColumnKey(h, i)).ToList());
        _model.SetSort(SortKey, SortDirection);
        _model.SearchText = SearchText ?? string.Empty;
        ApplyPropertiesToModel();
        Render();
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

    void Render()
    {
        var size = EffectiveSize;
        var selection = Selection;
        var rowActions = HasRowActions;
        _refs.Clear();

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
            foreach (var data in _model.VisibleRows) _table.Rows.Add(BuildRow(data, size, selection, rowActions));
        });
        SyncAll();
        UpdateChrome();
        UpdateToolbar();
        UpdateEmpty();
        UpdateAttachedPagination();
    }

    bool HasRowActions => RowActions.Count > 0 || RowActionsProvider is not null;

    bool CanExpand => IsExpandable && ExpandedRowTemplate is not null;

    CdsTableHead BuildHead(CdsDataTableSelection selection, bool rowActions)
    {
        var head = new CdsTableHead();
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
        }

        if (rowActions)
            head.HeaderList.Add(new CdsTableHeader { Width = new GridLength(CdsTableTokens.MenuColumnWidth), IsSortable = false, Description = RowActionsHeader, Text = null, Body = new BoxView { WidthRequest = 1, HeightRequest = 1, Color = Colors.Transparent, InputTransparent = true }, IsFlush = true });
        return head;
    }

    CdsTableRow BuildRow(DataTableRowData data, CdsSize size, CdsDataTableSelection selection, bool rowActions)
    {
        var row = new CdsTableRow { Tag = data.Item, IsExpandable = CanExpand };
        var reference = new RowRef { Data = data, Row = row };
        _refs[data.Key] = reference;
        var topAligned = size >= CdsSize.Xl;

        if (CanExpand) row.CellList.Add(new CdsTableCell { Body = BuildExpandButton(data, reference), IsFlush = true });

        if (selection == CdsDataTableSelection.Multi)
        {
            var check = new CdsCheckbox { Label = "Select row", HideLabel = true, IsEnabled = !data.IsDisabled, HorizontalOptions = LayoutOptions.Start };
            check.Margin = new Thickness(CdsTableTokens.CellPaddingX, topAligned ? CdsSpacing.Spacing04 : 0, 0, 0);
            check.VerticalOptions = topAligned ? LayoutOptions.Start : LayoutOptions.Center;
            check.Clicked += (_, _) => OnRowCheckActivated(data.Key);
            reference.Check = check;
            row.CellList.Add(new CdsTableCell { Body = check, IsFlush = true });
        }
        else if (selection == CdsDataTableSelection.Radio)
        {
            var radio = new CdsRadioButton { Label = "Select row", HideLabel = true, IsEnabled = !data.IsDisabled, HorizontalOptions = LayoutOptions.Start };
            radio.Margin = new Thickness(CdsTableTokens.CellPaddingX, topAligned ? CdsSpacing.Spacing04 : 0, 0, 0);
            radio.VerticalOptions = topAligned ? LayoutOptions.Start : LayoutOptions.Center;
            radio.CheckedChanged += (_, e) =>
            {
                if (!_syncing && e.Value) OnRowRadioChosen(data.Key);
            };
            reference.Radio = radio;
            row.CellList.Add(new CdsTableCell { Body = radio, IsFlush = true });
        }

        for (var c = 0; c < _headers.Count; c++)
        {
            var header = _headers[c];
            if (header.CellTemplate is { } template && data.Item is not null)
            {
                var view = (View)template.CreateContent();
                view.BindingContext = data.Item;
                row.CellList.Add(new CdsTableCell { Body = view });
            }
            else
            {
                row.CellList.Add(new CdsTableCell(c < data.Texts.Count ? data.Texts[c] : null));
            }
        }

        if (rowActions) row.CellList.Add(BuildMenuCell(data, reference, size));
        return row;
    }

    string FirstCellText(DataTableRowData data)
    {
        foreach (var text in data.Texts)
            if (!string.IsNullOrEmpty(text)) return text;
        return data.Key;
    }

    CdsTableCell BuildMenuCell(DataTableRowData data, RowRef reference, CdsSize size)
    {
        var actions = RowActionsProvider is { } provider && data.Item is not null ? provider(data.Item).ToList() : RowActions.ToList();
        var menu = new CdsOverflowMenu
        {
            Size = CdsTableTokens.MenuTriggerSize(size),
            IsFlipped = true,
            IconDescription = $"Options for {FirstCellText(data)}",
            HorizontalOptions = LayoutOptions.End,
            VerticalOptions = size >= CdsSize.Xl ? LayoutOptions.Start : LayoutOptions.Center,
        };
        if (size >= CdsSize.Xl) menu.Margin = new Thickness(0, CdsSpacing.Spacing03, 0, 0);
        foreach (var action in actions)
        {
            var item = new CdsMenuItem(action.Label ?? string.Empty, action.Icon) { Kind = action.Kind, IsEnabled = action.IsEnabled };
            var captured = action;
            item.Clicked += (_, _) =>
            {
                if (data.Item is not null) captured.Invoke(data.Item);
            };
            menu.Items.Add(item);
        }
        var cell = new CdsTableCell { Body = menu, IsFlush = true, RevealOnHover = OverflowMenuOnHover };
        void Keep() => cell.KeepVisible = menu.Is(CdsInteractionState.Focused) || menu.IsOpen;
        menu.StateChanged += (_, _) => Keep();
        menu.Opened += (_, _) => Keep();
        menu.Closed += (_, _) => Keep();
        reference.Menu = menu;
        return cell;
    }

    View BuildExpandButton(DataTableRowData data, RowRef reference)
    {
        var icon = new CdsIcon(CdsIconName.ChevronRight, 16) { HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center, InputTransparent = true };
        CdsTokens.SetColor(icon, CdsIcon.ColorProperty, CdsTableTokens.ExpandIcon);
        var ring = new CdsFocusRing(CdsFocusRingStyle.Inset);
        var hit = new CdsHitTarget { Role = CdsAccessibilityRole.Expander, IsExpanded = false };
        SemanticProperties.SetDescription(hit, "Expand current row");
        hit.Clicked += (_, _) => OnExpandActivated(data.Key);
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

    void SyncExpansion(string key, RowRef r)
    {
        var expanded = _model.IsExpanded(key);
        if (expanded && r.Row.ExpandedContent is null && ExpandedRowTemplate is { } template && r.Data.Item is not null)
        {
            var content = (View)template.CreateContent();
            content.BindingContext = r.Data.Item;
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
            if (Math.Abs(icon.Rotation - target) > 0.1)
                _ = CdsMotion.AnimateAsync(icon, "cds-expand", v => icon.Rotation = v, icon.Rotation, target, CdsDuration.Moderate01, CdsEasing.StandardProductive);
        }
    }

    // ----- user actions -----

    void OnHeaderActivated(string key)
    {
        _model.CycleSort(key);
        PublishSort();
        Render();
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
        Render();
    }

    void OnSearchTextPropertyChanged()
    {
        if (_syncing) return;
        var text = SearchText ?? string.Empty;
        if (_search.Text != text) _search.Text = text;
        if (_search.IsExpandable && text.Length > 0) _search.IsExpanded = true;
        _model.SearchText = text;
        Render();
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
        Render();
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
        RefreshVisuals();
    }

    // ----- chrome -----

    void UpdateChrome()
    {
        _title.Text = Title;
        _description.Text = Description;
        _title.IsVisible = !string.IsNullOrEmpty(Title);
        _description.IsVisible = !string.IsNullOrEmpty(Description);
        _titleBlock.IsVisible = _title.IsVisible || _description.IsVisible;
        _table.Label = Title ?? Label;
        SemanticProperties.SetHint(_table, Description);
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
        if (newState is not null) newState.Changed += OnEmptyChanged;
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
            action.Clicked += (_, _) => state.InvokeAction();
            stack.Add(action);
        }
        _empty.Add(stack);
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

    sealed class RowRef
    {
        public DataTableRowData Data = null!;
        public CdsTableRow Row = null!;
        public CdsCheckbox? Check;
        public CdsRadioButton? Radio;
        public CdsHitTarget? ExpandHit;
        public CdsIcon? ExpandIcon;
        public CdsOverflowMenu? Menu;
    }
}

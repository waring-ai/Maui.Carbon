using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Maui.Carbon;

/// <summary>
/// Simple tabular information in rows and cells without table chrome; the selectable variant lets people pick one row.
/// Define the header with <see cref="Columns"/> and the body with <see cref="Rows"/> (each a
/// <see cref="CdsStructuredListRow"/> of text or views). The background is <c>layer-01</c>, rows are separated by 1px
/// <c>border-subtle-01</c> rules, header cells are semibold <c>text-primary</c> and body cells <c>text-secondary</c>.
/// <see cref="IsSelectable"/> adds a radio per row: rows hover to <c>layer-hover-01</c> and select to
/// <c>layer-selected-01</c>; Up and Down move the selection. <see cref="IsCondensed"/> uses 8px cell padding and
/// <see cref="IsFlush"/> removes the start padding so text lines up with content above. For sorting, searching or
/// many columns use a data table.
/// </summary>
[ContentProperty(nameof(Rows))]
public class CdsStructuredList : CdsControl
{
    /// <summary>Bindable property for <see cref="Label"/>.</summary>
    public static readonly BindableProperty LabelProperty = BindableProperty.Create(
        nameof(Label), typeof(string), typeof(CdsStructuredList), "Structured list section",
        propertyChanged: static (b, _, _) => ((CdsStructuredList)b).Rebuild());

    /// <summary>Bindable property for <see cref="IsSelectable"/>.</summary>
    public static readonly BindableProperty IsSelectableProperty = BindableProperty.Create(
        nameof(IsSelectable), typeof(bool), typeof(CdsStructuredList), false,
        propertyChanged: static (b, _, _) => ((CdsStructuredList)b).Rebuild());

    /// <summary>Bindable property for <see cref="IsCondensed"/>.</summary>
    public static readonly BindableProperty IsCondensedProperty = BindableProperty.Create(
        nameof(IsCondensed), typeof(bool), typeof(CdsStructuredList), false,
        propertyChanged: static (b, _, _) => ((CdsStructuredList)b).Rebuild());

    /// <summary>Bindable property for <see cref="IsFlush"/>.</summary>
    public static readonly BindableProperty IsFlushProperty = BindableProperty.Create(
        nameof(IsFlush), typeof(bool), typeof(CdsStructuredList), false,
        propertyChanged: static (b, _, _) => ((CdsStructuredList)b).Rebuild());

    /// <summary>Bindable property for <see cref="IsNoWrap"/>.</summary>
    public static readonly BindableProperty IsNoWrapProperty = BindableProperty.Create(
        nameof(IsNoWrap), typeof(bool), typeof(CdsStructuredList), false,
        propertyChanged: static (b, _, _) => ((CdsStructuredList)b).Rebuild());

    /// <summary>Bindable property for <see cref="SelectedIndex"/>.</summary>
    public static readonly BindableProperty SelectedIndexProperty = BindableProperty.Create(
        nameof(SelectedIndex), typeof(int), typeof(CdsStructuredList), -1, BindingMode.TwoWay,
        coerceValue: static (b, v) => Math.Max(-1, (int)v),
        propertyChanged: static (b, o, n) => ((CdsStructuredList)b).OnSelectedIndexChanged((int)o, (int)n));

    /// <summary>Bindable property for <see cref="SelectedItem"/>.</summary>
    public static readonly BindableProperty SelectedItemProperty = BindableProperty.Create(
        nameof(SelectedItem), typeof(CdsStructuredListRow), typeof(CdsStructuredList), null, BindingMode.TwoWay,
        propertyChanged: static (b, _, n) => ((CdsStructuredList)b).OnSelectedItemChanged((CdsStructuredListRow?)n));

    readonly CdsItemCollection<CdsStructuredListColumn> _columns = [];
    readonly CdsItemCollection<CdsStructuredListRow> _rows = [];
    readonly Grid _grid = new() { RowSpacing = 0, ColumnSpacing = 0 };
    readonly List<RowVisual> _visuals = [];
    readonly List<BoxView> _rules = [];
    readonly List<ContentView> _wrappers = [];
    readonly CdsCoalescedAction _rebuild;
    // One tab stop for the rows, like a native radio group: Tab enters on the selected row (else the first enabled one) and
    // the arrow keys move the selection.
    readonly CdsRovingFocus _roving = new(CdsRovingOrientation.Both, wraps: true) { RemembersFocus = false, HandlesHomeEnd = false };
    bool _syncing;

    /// <summary>Creates an empty list.</summary>
    public CdsStructuredList()
    {
        _rebuild = new CdsCoalescedAction(this, RebuildNow);
        HorizontalOptions = LayoutOptions.Fill;
        VerticalOptions = LayoutOptions.Start;
        CdsTokens.SetColor(_grid, BackgroundColorProperty, CdsStructuredListTokens.Background);
        _columns.CollectionChanged += OnColumnsChanged;
        _rows.CollectionChanged += OnRowsChanged;
        _roving.PreferredIndex = () => SelectedIndex;
        _roving.Navigated += (_, index) => Select(index);
        SetChrome(_grid);
    }

    /// <summary>Raised after the selection changed (<c>onSelectionChange</c>).</summary>
    public event EventHandler? SelectionChanged;

    /// <summary>The columns: header labels and widths.</summary>
    public IList<CdsStructuredListColumn> Columns => _columns;

    /// <summary>The body rows.</summary>
    public IList<CdsStructuredListRow> Rows => _rows;

    /// <summary>The accessible name of the list (<c>aria-label</c>). Default <c>Structured list section</c>; always set a real one.</summary>
    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <summary>Adds a radio per row so people can pick exactly one (<c>selection</c>). Use a data table for multiple selection.</summary>
    public bool IsSelectable
    {
        get => (bool)GetValue(IsSelectableProperty);
        set => SetValue(IsSelectableProperty, value);
    }

    /// <summary>8px cell padding instead of the default 16 / 24.</summary>
    public bool IsCondensed
    {
        get => (bool)GetValue(IsCondensedProperty);
        set => SetValue(IsCondensedProperty, value);
    }

    /// <summary>No start padding, so text lines up with content above. Ignored when <see cref="IsSelectable"/>.</summary>
    public bool IsFlush
    {
        get => (bool)GetValue(IsFlushProperty);
        set => SetValue(IsFlushProperty, value);
    }

    /// <summary>Keep every cell on one line (<c>noWrap</c>); a column can also set <see cref="CdsStructuredListColumn.IsNoWrap"/>.</summary>
    public bool IsNoWrap
    {
        get => (bool)GetValue(IsNoWrapProperty);
        set => SetValue(IsNoWrapProperty, value);
    }

    /// <summary>The index of the selected row, or -1 (two-way).</summary>
    public int SelectedIndex
    {
        get => (int)GetValue(SelectedIndexProperty);
        set => SetValue(SelectedIndexProperty, value);
    }

    /// <summary>The selected row, or <c>null</c> (two-way).</summary>
    public CdsStructuredListRow? SelectedItem
    {
        get => (CdsStructuredListRow?)GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    /// <inheritdoc/>
    protected internal override bool WantsNavigationKeys => IsSelectable;

    /// <inheritdoc/>
    protected internal override bool OnKeyDown(CdsKeyEventArgs e)
    {
        if (!IsSelectable) return false;
        _rebuild.Flush();
        return _roving.HandleKey(e);
    }

    /// <summary>The row hit targets in order (for tests); empty unless the list is selectable.</summary>
    internal IReadOnlyList<CdsHitTarget> RowTargets => _roving.Items;

    /// <inheritdoc/>
    protected override void ApplyVisuals()
    {
        // The visuals still describe the old rows; the rebuild re-applies them.
        if (_rebuild.IsPending) return;
        for (var i = 0; i < _visuals.Count && i < _rows.Count; i++)
        {
            var v = _visuals[i];
            var disabled = IsSelectable && !_rows[i].IsEnabled;
            var selected = IsSelectable && i == SelectedIndex;
            var hover = IsSelectable && v.Hover && !disabled;
            SetColor(v.Fill, BoxView.ColorProperty, CdsStructuredListTokens.RowFill(selected, hover, disabled));
            var active = selected || hover;
            foreach (var label in v.Labels)
                SetColor(label, CdsText.TextColorProperty, CdsStructuredListTokens.CellText(false, active, disabled));
            if (v.Icon is not null)
            {
                v.Icon.Icon = selected ? CdsIconName.RadioButtonChecked : CdsIconName.RadioButton;
                SetColor(v.Icon, CdsIcon.ColorProperty, CdsStructuredListTokens.Icon(disabled));
            }
            if (v.Ring is not null) v.Ring.IsActive = v.Hit is { IsFocused: true } hit && hit.IsFocusFromKeyboard;
            if (v.Hit is not null)
            {
                v.Hit.IsChecked = selected;
                v.Hit.IsEnabled = !disabled;
            }
        }
        for (var i = 0; i < _rules.Count; i++)
        {
            var above = i > 0 && i - 1 < _visuals.Count ? _visuals[i - 1] : null;
            var row = i < _visuals.Count ? _visuals[i] : null;
            var aboveDisabled = IsSelectable && i > 0 && !_rows[i - 1].IsEnabled;
            var rowDisabled = IsSelectable && row is not null && !_rows[i].IsEnabled;
            var rowSelected = IsSelectable && row is not null && i == SelectedIndex;
            var aboveSelected = IsSelectable && above is not null && i - 1 == SelectedIndex;
            var rowHover = IsSelectable && row is { Hover: true } && !rowDisabled;
            var aboveHover = IsSelectable && above is { Hover: true } && !aboveDisabled && !aboveSelected;
            SetColor(_rules[i], BoxView.ColorProperty, CdsStructuredListTokens.Rule(rowSelected, rowHover && !rowSelected, aboveSelected, aboveHover));
        }
    }

    void OnColumnsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
            foreach (CdsStructuredListColumn c in e.OldItems) c.Changed -= OnPartChanged;
        if (e.NewItems is not null)
            foreach (CdsStructuredListColumn c in e.NewItems) c.Changed += OnPartChanged;
        Rebuild();
    }

    void OnRowsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
            foreach (CdsStructuredListRow r in e.OldItems) r.Changed -= OnPartChanged;
        if (e.NewItems is not null)
            foreach (CdsStructuredListRow r in e.NewItems) r.Changed += OnPartChanged;
        Rebuild();
        if (SelectedIndex >= _rows.Count) SelectedIndex = -1;
    }

    void OnPartChanged(object? sender, EventArgs e) => Rebuild();

    void OnSelectedIndexChanged(int oldIndex, int index)
    {
        if (!_syncing)
        {
            _syncing = true;
            SelectedItem = index >= 0 && index < _rows.Count ? _rows[index] : null;
            _syncing = false;
        }
        _roving.Refresh();
        RefreshVisuals();
        SelectionChanged?.Invoke(this, EventArgs.Empty);
        if (IsSelectable && index >= 0 && index < _rows.Count)
            CdsAnnouncer.Announce(_rows[index].Label);
    }

    void OnSelectedItemChanged(CdsStructuredListRow? row)
    {
        if (_syncing) return;
        _syncing = true;
        SelectedIndex = row is null ? -1 : _rows.IndexOf(row);
        _syncing = false;
    }

    void Select(int index)
    {
        if (index < 0 || index >= _rows.Count || !_rows[index].IsEnabled) return;
        SelectedIndex = index;
    }

    /// <summary>Rebuilds the grid; on screen, once after the current batch of changes.</summary>
    void Rebuild() => _rebuild.Request();

    void RebuildNow()
    {
        _rebuild.Cancel();
        // Custom cells are the consumer's views and come back in the new grid: take them out of their old wrappers first, or
        // they would be added while still a child there.
        foreach (var wrapper in _wrappers) wrapper.Content = null;
        _wrappers.Clear();
        _grid.Children.Clear();
        _grid.RowDefinitions.Clear();
        _grid.ColumnDefinitions.Clear();
        _visuals.Clear();
        _rules.Clear();
        _roving.Clear();

        var selection = IsSelectable;
        var offset = selection ? 1 : 0;
        var columnCount = Math.Max(_columns.Count, _rows.Count == 0 ? 0 : _rows.Max(r => r.Cells.Count));
        if (columnCount == 0 && !selection) return;

        if (selection) _grid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(CdsStructuredListModel.IconColumnWidth)));
        for (var c = 0; c < columnCount; c++)
            _grid.ColumnDefinitions.Add(new ColumnDefinition(c < _columns.Count ? _columns[c].Width : GridLength.Star));
        var span = Math.Max(1, _grid.ColumnDefinitions.Count);

        // Header row.
        var row = 0;
        var hasHeader = _columns.Any(c => !string.IsNullOrEmpty(c.Header));
        if (hasHeader)
        {
            _grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            var min = new BoxView { HeightRequest = CdsStructuredListModel.HeaderHeight, Color = Colors.Transparent, InputTransparent = true };
            AutomationProperties.SetIsInAccessibleTree(min, false);
            _grid.Add(min, 0, row);
            Grid.SetColumnSpan(min, span);
            if (selection)
            {
                var spacer = new ContentView { Padding = CdsStructuredListModel.IconCellPadding(true, IsCondensed) };
                _grid.Add(spacer, 0, row);
            }
            for (var c = 0; c < columnCount; c++)
            {
                // Header text wraps like body text ("Long text wraps inside its cell"); no-wrap columns stay on one line.
                var headerNoWrap = IsNoWrap || (c < _columns.Count && _columns[c].IsNoWrap);
                var label = new CdsText(c < _columns.Count ? _columns[c].Header ?? string.Empty : string.Empty, CdsTypeStyle.HeadingCompact01)
                {
                    Padding = CdsStructuredListModel.CellPadding(true, IsCondensed, IsFlush && !selection, selection, c == 0),
                    LineBreakMode = headerNoWrap ? LineBreakMode.NoWrap : LineBreakMode.WordWrap,
                };
                SemanticProperties.SetHeadingLevel(label, SemanticHeadingLevel.None);
                _grid.Add(label, c + offset, row);
            }
            row++;
        }

        // Body rows: [background] [cells] [focus ring and hit target].
        for (var r = 0; r < _rows.Count; r++)
        {
            _grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            var source = _rows[r];
            var visual = new RowVisual();
            _visuals.Add(visual);

            visual.Fill = new BoxView { InputTransparent = true };
            AutomationProperties.SetIsInAccessibleTree(visual.Fill, false);
            _grid.Add(visual.Fill, 0, row);
            Grid.SetColumnSpan(visual.Fill, span);

            var rule = new BoxView { HeightRequest = 1, VerticalOptions = LayoutOptions.Start, InputTransparent = true };
            AutomationProperties.SetIsInAccessibleTree(rule, false);
            _rules.Add(rule);
            _grid.Add(rule, 0, row);
            Grid.SetColumnSpan(rule, span);

            var disabled = selection && !source.IsEnabled;
            if (selection)
            {
                visual.Icon = new CdsIcon(CdsIconName.RadioButton, 16) { HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Start, InputTransparent = true };
                var iconCell = new ContentView { Padding = CdsStructuredListModel.IconCellPadding(false, IsCondensed), InputTransparent = true, Content = visual.Icon };
                visual.Icon.Margin = new Thickness(0, 2, 0, 0);
                _grid.Add(iconCell, 0, row);
            }

            for (var c = 0; c < columnCount; c++)
            {
                var cell = c < source.Cells.Count ? source.Cells[c] : null;
                var noWrap = IsNoWrap || (c < _columns.Count && _columns[c].IsNoWrap);
                var padding = CdsStructuredListModel.CellPadding(false, IsCondensed, IsFlush && !selection, selection, c == 0);
                View view;
                if (cell is View custom)
                {
                    var wrapper = new ContentView { Padding = padding, Content = custom };
                    _wrappers.Add(wrapper);
                    view = wrapper;
                }
                else
                {
                    var label = new CdsText(cell?.ToString() ?? string.Empty, CdsTypeStyle.Body01, CdsTextTone.Secondary)
                    {
                        Padding = padding,
                        LineBreakMode = noWrap ? LineBreakMode.NoWrap : LineBreakMode.WordWrap,
                        InputTransparent = selection,
                    };
                    visual.Labels.Add(label);
                    view = label;
                }
                if (selection) view.InputTransparent = true;
                _grid.Add(view, c + offset, row);
            }

            if (selection)
            {
                var index = r;
                var host = new Grid();
                visual.Ring = new CdsFocusRing(CdsFocusRingStyle.Inset);
                visual.Hit = new CdsHitTarget { Role = CdsAccessibilityRole.Radio, IsEnabled = !disabled };
                SemanticProperties.SetDescription(visual.Hit, source.Label);
                visual.Hit.Clicked += (_, _) => Select(index);
                visual.Hit.Focused += (_, _) => { SetState(CdsInteractionState.Focused, true); CdsFocusTracker.SetFocused(this); RefreshVisuals(); };
                visual.Hit.Unfocused += (_, _) => { SetState(CdsInteractionState.Focused | CdsInteractionState.FocusVisible, false); CdsFocusTracker.ClearFocused(this); RefreshVisuals(); };
                host.Add(visual.Ring);
                host.Add(visual.Hit);
                var pointer = new PointerGestureRecognizer();
                pointer.PointerEntered += (_, _) => { visual.Hover = true; RefreshVisuals(); };
                pointer.PointerExited += (_, _) => { visual.Hover = false; RefreshVisuals(); };
                host.GestureRecognizers.Add(pointer);
                _grid.Add(host, 0, row);
                Grid.SetColumnSpan(host, span);
            }
            row++;
        }

        // Closing rule under the last row.
        var closing = new BoxView { HeightRequest = 1, VerticalOptions = LayoutOptions.End, InputTransparent = true };
        AutomationProperties.SetIsInAccessibleTree(closing, false);
        _rules.Add(closing);
        if (_rows.Count > 0)
        {
            _grid.Add(closing, 0, row - 1);
            Grid.SetColumnSpan(closing, span);
        }
        else
        {
            _rules.Remove(closing);
        }

        CdsSemantics.SetGroupName(_grid, Label);
        _roving.SetItems(_visuals.Where(v => v.Hit is not null).Select(v => v.Hit!));
        RefreshVisuals();
    }

    sealed class RowVisual
    {
        public BoxView Fill = null!;
        public readonly List<CdsText> Labels = [];
        public CdsIcon? Icon;
        public CdsFocusRing? Ring;
        public CdsHitTarget? Hit;
        public bool Hover;
    }
}

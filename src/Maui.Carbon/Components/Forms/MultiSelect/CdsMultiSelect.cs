using System.Collections;
using System.Collections.Specialized;

namespace Maui.Carbon;

/// <summary>
/// A field that opens a checkbox list and holds several selected items, summarised by a count tag with a clear button;
/// <see cref="IsFilterable"/> turns the field into a text input that narrows the list. Clicking anywhere on a row toggles it
/// and the list stays open. Use it for several values (channels, roles, filter criteria); for one value use a
/// <see cref="CdsDropdown"/> or <see cref="CdsComboBox"/>, and for fewer than about five options a checkbox group.
/// <para>
/// Keyboard: Down, Enter or Space open; arrows move; Enter or Space toggle the highlighted row (Space types when
/// filterable); Home and End jump (not when filterable); Escape or Tab close; Delete on the closed field clears every
/// selection. The tag's x is not a tab stop.
/// </para>
/// </summary>
public partial class CdsMultiSelect : CdsListBox
{
    /// <summary>The native text entry used when <see cref="IsFilterable"/>.</summary>
    protected readonly CdsNativeEntry Entry = new() { IsVisible = false };

    /// <summary>The field text shown beside the count tag: <see cref="CdsListBox.Placeholder"/>.</summary>
    protected readonly CdsText ValueText = new() { LineBreakMode = LineBreakMode.TailTruncation, MaxLines = 1, VerticalOptions = LayoutOptions.Center };

    readonly SelectionSet _selection = new();
    readonly CdsListBoxSelectionTag _tag = new() { IsVisible = false, Margin = new Thickness(0, 0, CdsSpacing.Spacing03, 0) };
    readonly CdsListBoxIconButton _clearText = new(CdsIconName.Close, 13) { IsVisible = false, Margin = new Thickness(0, 0, 5, 0) };
    readonly CdsHitTarget _hit;
    List<object?> _snapshot = [];
    INotifyCollectionChanged? _observed;
    bool _syncing;
    bool _filterSyncing;

    /// <summary>Creates a medium multi-select.</summary>
    public CdsMultiSelect()
    {
        _hit = CreateTriggerHit();
        AutomationProperties.SetExcludedWithChildren(ValueText, true);
        ValueText.InputTransparent = true;

        _tag.Description = "Clear all selected items";
        _tag.Cleared += (_, _) => ClearSelection();
        _tag.BodyTapped += (_, _) => ToggleFromPointer();
        _clearText.Description = "Clear filter";
        _clearText.Clicked += (_, _) =>
        {
            Entry.Text = string.Empty;
            Entry.Focus();
        };
        Chrome.Trailing.Insert(Chrome.Trailing.Count - 1, _clearText);

        Entry.TextChanged += OnFilterTextChanged;
        Entry.Completed += (_, _) => OnEntryCompleted();

        var input = new Grid { ColumnSpacing = 0 };
        input.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        input.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        input.Add(_tag, 0);
        input.Add(ValueText, 1);
        input.Add(_hit, 1);
        input.Add(Entry, 1);
        BuildField(input, _hit);
        UpdateChrome();
    }

    /// <summary>Raised when the selection changes (by the user or in code).</summary>
    public event EventHandler<CdsListBoxSelectionChangedEventArgs>? SelectionChanged;

    /// <inheritdoc/>
    protected override bool HasTextField => IsFilterable;

    /// <inheritdoc/>
    protected override bool IsMultiple => true;

    /// <inheritdoc/>
    protected override string? AccessibleValue => _selection.Count == 0 ? null : $"{_selection.Count} selected";

    /// <summary>The number of selected items.</summary>
    public int SelectedCount => _selection.Count;

    /// <summary>Clears every selection (what the tag's x and Delete do).</summary>
    public void ClearSelection()
    {
        if (!CanOpen) return;
        if (_selection.Clear()) Commit();
    }

    // ---- rows ----

    /// <inheritdoc/>
    internal override IReadOnlyList<ListBoxEntry> BuildRows()
    {
        IReadOnlyList<ListBoxEntry> rows = AllRows;
        var text = IsFilterable ? Entry.Text : null;
        if (!string.IsNullOrEmpty(text)) rows = ListBoxLogic.Filter(rows, text, null, ShouldFilterItem);
        if (SortSelectedFirst && SelectionFeedback != CdsMultiSelectFeedback.Fixed)
        {
            Func<object?, bool> first = SelectionFeedback == CdsMultiSelectFeedback.Top
                ? _selection.Contains
                : v => v is not null && _snapshot.Exists(s => Equals(s, v));
            rows = ListBoxLogic.SelectedFirst(rows, first);
        }
        return rows;
    }

    /// <inheritdoc/>
    internal override bool IsRowChosen(ListBoxEntry row) => _selection.Contains(row.Value);

    /// <inheritdoc/>
    internal override void ChooseRow(int index)
    {
        _selection.Toggle(VisibleRows[index].Value);
        Commit();
        if (SelectionFeedback == CdsMultiSelectFeedback.Top && SortSelectedFirst) RefreshRows(keepHighlight: true);
    }

    /// <inheritdoc/>
    protected override void OnListOpening() => _snapshot = [.. _selection.Items];

    /// <inheritdoc/>
    protected override void OnListClosed()
    {
        if (IsFilterable && !string.IsNullOrEmpty(Entry.Text)) SetFilterText(string.Empty);
    }

    /// <inheritdoc/>
    protected override void OnItemsChanged() => ReconcileSelection();

    // ---- selection ----

    void OnSelectedItemsChanged(IList? previous, IList? current)
    {
        if (previous is INotifyCollectionChanged old) old.CollectionChanged -= OnSelectedCollectionChanged;
        _observed = current as INotifyCollectionChanged;
        if (_observed is not null) _observed.CollectionChanged += OnSelectedCollectionChanged;
        if (_syncing) return;
        if (_selection.Replace(current?.Cast<object?>())) AfterSelectionChanged(raise: true);
    }

    void OnSelectedCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (_syncing) return;
        if (_selection.Replace(SelectedItems?.Cast<object?>())) AfterSelectionChanged(raise: true);
    }

    void ReconcileSelection()
    {
        // Items that left the list are no longer selected.
        var present = _selection.Items.Where(i => AllRows.Any(r => Equals(r.Value, i))).ToList();
        if (present.Count != _selection.Count && _selection.Replace(present)) Commit();
    }

    /// <summary>Writes the selection back to <see cref="SelectedItems"/> and notifies.</summary>
    void Commit()
    {
        _syncing = true;
        try
        {
            var target = SelectedItems;
            if (target is { IsReadOnly: false, IsFixedSize: false })
            {
                target.Clear();
                foreach (var item in _selection.Items) target.Add(item);
            }
            else
            {
                SelectedItems = new List<object?>(_selection.Items);
            }
        }
        finally
        {
            _syncing = false;
        }
        AfterSelectionChanged(raise: true);
    }

    void AfterSelectionChanged(bool raise)
    {
        UpdateTag();
        UpdateAccessibleName();
        RefreshOpenList();
        if (!raise) return;
        var items = new List<object?>(_selection.Items);
        SelectionChanged?.Invoke(this, new CdsListBoxSelectionChangedEventArgs(items.Count > 0 ? items[0] : null, items));
        if (Handler is not null)
            CdsAnnouncer.Announce(items.Count == 0 ? "No items selected" : $"{items.Count} items selected");
    }

    void UpdateTag()
    {
        _tag.IsVisible = _selection.Count > 0;
        _tag.Count = _selection.Count;
        RefreshVisuals();
    }

    // ---- filterable ----

    void OnFilterableChanged()
    {
        var filterable = IsFilterable;
        Entry.IsVisible = filterable;
        ValueText.IsVisible = !filterable;
        _hit.IsVisible = !filterable;
        BuildField(Chrome.Input!, filterable ? Entry : _hit);
        UpdateChrome();
    }

    void SetFilterText(string text)
    {
        _filterSyncing = true;
        Entry.Text = text;
        _filterSyncing = false;
        _clearText.IsVisible = IsFilterable && text.Length > 0;
    }

    void OnFilterTextChanged(object? sender, TextChangedEventArgs e)
    {
        _clearText.IsVisible = IsFilterable && !string.IsNullOrEmpty(e.NewTextValue);
        if (_filterSyncing || !CanOpen) return;
        RefilterFromTyping();
    }

    void OnEntryCompleted()
    {
        if (!CanOpen) return;
        if (IsOpen && Model.HighlightedRow is not null) ChooseOpenRow();
        else if (!IsOpen) OpenFromKeyboard();
    }

    void ChooseOpenRow()
    {
        ChooseRow(Model.Highlighted);
        RefreshOpenList();
    }

    /// <inheritdoc/>
    protected override bool OnClosedKey(CdsKeyEventArgs e)
    {
        if (e.Key != CdsKey.Delete || IsFilterable || _selection.Count == 0) return false;
        ClearSelection();
        return true;
    }

    // ---- visuals ----

    /// <inheritdoc/>
    protected override void OnChromeUpdated()
    {
        ValueText.Text = Placeholder;
        Entry.Placeholder = Placeholder;
        Entry.IsReadOnly = IsReadOnly;
        UpdateAccessibleName();
    }

    /// <inheritdoc/>
    protected override void ApplyTrigger(bool disabled, bool readOnly)
    {
        SetColor(ValueText, Microsoft.Maui.Controls.Label.TextColorProperty, CdsListBoxTokens.FieldText(disabled));
        SetColor(Entry, Microsoft.Maui.Controls.Entry.TextColorProperty, CdsListBoxTokens.FieldText(disabled));
        SetColor(Entry, Microsoft.Maui.Controls.Entry.PlaceholderColorProperty, disabled ? CdsColorKeys.TextDisabled : CdsColorKeys.TextPlaceholder);
        Entry.IsEnabled = IsEnabled;
        _tag.Muted = disabled || readOnly;
        _tag.IsEnabled = IsEnabled;
        _clearText.Dimmed = disabled || readOnly;
    }
}

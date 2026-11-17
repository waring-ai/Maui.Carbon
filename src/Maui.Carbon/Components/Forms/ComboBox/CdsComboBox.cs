namespace Maui.Carbon;

/// <summary>
/// A text field with a list of options that filters as you type, for choosing one value from a long list. Choosing an option
/// fills the field and shows a clear button; <see cref="AllowCustomValue"/> keeps text that matches nothing. Use a
/// <see cref="CdsDropdown"/> for short fixed lists and a <see cref="CdsMultiSelect"/> with
/// <see cref="CdsMultiSelect.IsFilterable"/> for several values. There is no inline variant.
/// <para>
/// Keyboard: typing filters and opens, the first match is highlighted; Down and Up open and move; Enter chooses the
/// highlighted row (or opens the list when it is closed); Escape closes, or clears the text when already closed; Tab closes.
/// Space types a space and never selects. The clear and chevron buttons are not tab stops.
/// </para>
/// </summary>
public partial class CdsComboBox : CdsListBox
{
    /// <summary>The native text entry inside the field.</summary>
    protected readonly CdsNativeEntry Entry = new();

    readonly CdsListBoxIconButton _clear = new(CdsIconName.Close, 13) { IsVisible = false, Margin = new Thickness(0, 0, 5, 0) };
    bool _syncing;
    bool _customCommit;

    /// <summary>Creates a medium combo box.</summary>
    public CdsComboBox()
    {
        _clear.Description = "Clear selected item";
        _clear.Clicked += (_, _) => Clear();
        Chrome.Trailing.Insert(Chrome.Trailing.Count - 1, _clear);

        Entry.TextChanged += OnEntryTextChanged;
        Entry.Completed += (_, _) => OnEntryCompleted();
        Entry.Unfocused += (_, _) => CommitOrRevert();
        BuildField(Entry, Entry);
        UpdateChrome();
    }

    /// <summary>Raised when the choice changes: an item was chosen, or the field was cleared or committed as a custom value.</summary>
    public event EventHandler<CdsListBoxSelectionChangedEventArgs>? SelectionChanged;

    /// <summary>Raised when the text changes (typing, clearing, or a choice).</summary>
    public event EventHandler<TextChangedEventArgs>? InputTextChanged;

    /// <inheritdoc/>
    protected override bool HasTextField => true;

    /// <inheritdoc/>
    protected override bool AllowsInline => false;

    /// <inheritdoc/>
    protected internal override bool WantsEscapeKey => !IsOpen && CanOpen && !string.IsNullOrEmpty(InputText);

    string? SelectedText => SelectedItem is null ? null : TextOf(SelectedItem);

    /// <summary>Empties the field: clears the text and the choice, raises <see cref="SelectionChanged"/> and refocuses the input.</summary>
    public void Clear()
    {
        if (!CanOpen) return;
        var had = SelectedItem is not null;
        InputText = string.Empty;
        SelectedItem = null;
        if (!had) RaiseSelectionChanged(null, string.Empty);
        RefreshRows(keepHighlight: false);
        Entry.Focus();
    }

    /// <inheritdoc/>
    internal override IReadOnlyList<ListBoxEntry> BuildRows() =>
        ListBoxLogic.Filter(AllRows, InputText, SelectedText, ShouldFilterItem);

    /// <inheritdoc/>
    internal override bool IsRowChosen(ListBoxEntry row) => SelectedItem is not null && Equals(row.Value, SelectedItem);

    /// <inheritdoc/>
    internal override void ChooseRow(int index)
    {
        var item = VisibleRows[index].Value;
        var same = Equals(item, SelectedItem);
        SetText(TextOf(item));
        SelectedItem = item;
        if (same) RaiseSelectionChanged(item, TextOf(item));
    }

    /// <inheritdoc/>
    internal override object? HighlightOnOpen => SelectedItem;

    void OnSelectedItemChanged(object? item)
    {
        if (!_customCommit) SetText(item is null ? string.Empty : TextOf(item));
        UpdateAccessibleName();
        RefreshOpenList();
        RaiseSelectionChanged(item, item is null ? InputText : TextOf(item));
    }

    void RaiseSelectionChanged(object? item, string? text) =>
        SelectionChanged?.Invoke(this, new CdsListBoxSelectionChangedEventArgs(item, item is null ? [] : [item], text));

    void OnInputTextChanged(string previous, string current)
    {
        if (!_syncing && Entry.Text != current)
        {
            _syncing = true;
            Entry.Text = current;
            _syncing = false;
        }
        _clear.IsVisible = current.Length > 0;
        InputTextChanged?.Invoke(this, new TextChangedEventArgs(previous, current));
    }

    void SetText(string text)
    {
        InputText = text;
        if (Entry.Text != text)
        {
            _syncing = true;
            Entry.Text = text;
            _syncing = false;
        }
    }

    void OnEntryTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (_syncing) return;
        _syncing = true;
        InputText = e.NewTextValue ?? string.Empty;
        _syncing = false;
        if (!CanOpen) return;
        // Typing filters the list, opens it, and highlights the first match.
        RefreshRows(keepHighlight: false);
        if (!IsOpen) IsOpen = true;
        HighlightFirstRow();
    }

    void OnEntryCompleted()
    {
        if (!CanOpen) return;
        if (IsOpen && Model.HighlightedRow is not null)
        {
            ChooseRow(Model.Highlighted);
            IsOpen = false;
        }
        else if (AllowCustomValue && InputText.Length > 0)
        {
            CommitCustom();
            IsOpen = false;
        }
        else if (!IsOpen)
        {
            OpenFromKeyboard();
        }
    }

    void CommitCustom()
    {
        var had = SelectedItem is not null;
        _customCommit = true;
        try
        {
            SelectedItem = null;
        }
        finally
        {
            _customCommit = false;
        }
        if (!had) RaiseSelectionChanged(null, InputText);
    }

    void CommitOrRevert()
    {
        if (string.Equals(InputText, SelectedText ?? string.Empty, StringComparison.Ordinal)) return;
        if (AllowCustomValue && InputText.Length > 0) CommitCustom();
        else if (AllowCustomValue) Clear();
        else SetText(SelectedText ?? string.Empty);
        RefreshRows(keepHighlight: false);
    }

    /// <inheritdoc/>
    protected override void OnListClosed() => RefreshRows(keepHighlight: false);

    /// <inheritdoc/>
    protected override bool OnClosedKey(CdsKeyEventArgs e)
    {
        if (e.Key != CdsKey.Escape || InputText.Length == 0) return false;
        Clear();
        return true;
    }

    /// <inheritdoc/>
    protected override string? AccessibleValue => null;

    /// <inheritdoc/>
    protected override void OnChromeUpdated()
    {
        Entry.Placeholder = Placeholder;
        Entry.IsReadOnly = IsReadOnly;
        UpdateAccessibleName();
    }

    /// <inheritdoc/>
    protected override void ApplyTrigger(bool disabled, bool readOnly)
    {
        Entry.IsEnabled = IsEnabled;
        SetColor(Entry, Microsoft.Maui.Controls.Entry.TextColorProperty, CdsListBoxTokens.FieldText(disabled));
        SetColor(Entry, Microsoft.Maui.Controls.Entry.PlaceholderColorProperty, disabled ? CdsColorKeys.TextDisabled : CdsColorKeys.TextPlaceholder);
        _clear.Dimmed = disabled || readOnly;
        _clear.IsEnabled = IsEnabled;
    }
}

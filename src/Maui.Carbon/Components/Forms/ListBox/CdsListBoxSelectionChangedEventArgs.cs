namespace Maui.Carbon;

/// <summary>Data for the <c>SelectionChanged</c> event of <see cref="CdsDropdown"/>, <see cref="CdsComboBox"/> and <see cref="CdsMultiSelect"/>.</summary>
public sealed class CdsListBoxSelectionChangedEventArgs : EventArgs
{
    /// <summary>Creates event data.</summary>
    public CdsListBoxSelectionChangedEventArgs(object? selectedItem, IReadOnlyList<object?> selectedItems, string? inputText = null)
    {
        SelectedItem = selectedItem;
        SelectedItems = selectedItems;
        InputText = inputText;
    }

    /// <summary>The chosen item of a dropdown or combo box (<c>null</c> when cleared); the first selected item of a multi-select.</summary>
    public object? SelectedItem { get; }

    /// <summary>Every selected item: the chosen item alone for a dropdown or combo box, the whole selection for a multi-select.</summary>
    public IReadOnlyList<object?> SelectedItems { get; }

    /// <summary>For a combo box with <c>AllowCustomValue</c>, the typed text when no item was chosen; otherwise the text of the choice.</summary>
    public string? InputText { get; }
}

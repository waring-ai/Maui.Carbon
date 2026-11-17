namespace Maui.Carbon;

public partial class CdsComboBox
{
    /// <summary>Bindable property for <see cref="SelectedItem"/>.</summary>
    public static readonly BindableProperty SelectedItemProperty = BindableProperty.Create(
        nameof(SelectedItem), typeof(object), typeof(CdsComboBox), null, BindingMode.TwoWay,
        propertyChanged: static (b, _, n) => ((CdsComboBox)b).OnSelectedItemChanged(n));

    /// <summary>Bindable property for <see cref="InputText"/>.</summary>
    public static readonly BindableProperty InputTextProperty = BindableProperty.Create(
        nameof(InputText), typeof(string), typeof(CdsComboBox), string.Empty, BindingMode.TwoWay,
        coerceValue: static (_, v) => v ?? string.Empty,
        propertyChanged: static (b, o, n) => ((CdsComboBox)b).OnInputTextChanged((string)o, (string)n));

    /// <summary>Bindable property for <see cref="ShouldFilterItem"/>.</summary>
    public static readonly BindableProperty ShouldFilterItemProperty = BindableProperty.Create(
        nameof(ShouldFilterItem), typeof(Func<object?, string, bool>), typeof(CdsComboBox), null,
        propertyChanged: static (b, _, _) => ((CdsComboBox)b).RefreshRows(keepHighlight: false));

    /// <summary>Bindable property for <see cref="AllowCustomValue"/>.</summary>
    public static readonly BindableProperty AllowCustomValueProperty = BindableProperty.Create(
        nameof(AllowCustomValue), typeof(bool), typeof(CdsComboBox), false);

    /// <summary>The chosen item (two-way), or <c>null</c> when nothing is chosen or a custom value was entered.</summary>
    public object? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    /// <summary>The text in the field (two-way). Setting it filters the list; choosing an item replaces it with the item's text.</summary>
    public string InputText
    {
        get => (string)GetValue(InputTextProperty);
        set => SetValue(InputTextProperty, value);
    }

    /// <summary>
    /// Replaces the default filter, a case-insensitive "contains" match on <see cref="CdsListBox.ItemToString"/> text. Called
    /// with the item and the typed text; return <c>true</c> to keep the row. The full list shows when the text equals the
    /// chosen item's text.
    /// </summary>
    public Func<object?, string, bool>? ShouldFilterItem
    {
        get => (Func<object?, string, bool>?)GetValue(ShouldFilterItemProperty);
        set => SetValue(ShouldFilterItemProperty, value);
    }

    /// <summary>
    /// Keeps typed text that matches no item: Enter or leaving the field then clears <see cref="SelectedItem"/> and raises
    /// <see cref="CdsComboBox.SelectionChanged"/> with the text in <see cref="CdsListBoxSelectionChangedEventArgs.InputText"/>.
    /// Without it, the text reverts to the chosen item (or empties) when the field loses focus.
    /// </summary>
    public bool AllowCustomValue
    {
        get => (bool)GetValue(AllowCustomValueProperty);
        set => SetValue(AllowCustomValueProperty, value);
    }
}

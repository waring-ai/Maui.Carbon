namespace Maui.Carbon;

public partial class CdsDropdown
{
    /// <summary>Bindable property for <see cref="SelectedItem"/>.</summary>
    public static readonly BindableProperty SelectedItemProperty = BindableProperty.Create(
        nameof(SelectedItem), typeof(object), typeof(CdsDropdown), null, BindingMode.TwoWay,
        propertyChanged: static (b, o, n) => ((CdsDropdown)b).OnSelectedItemChanged(o, n));

    /// <summary>The chosen item (two-way): an object from <see cref="CdsListBox.ItemsSource"/>, or <c>null</c> while nothing is chosen.</summary>
    public object? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }
}

using System.Collections;

namespace Maui.Carbon;

public partial class CdsSelect
{
    /// <summary>Bindable property for <see cref="ItemsSource"/>.</summary>
    public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(
        nameof(ItemsSource), typeof(IEnumerable), typeof(CdsSelect), null,
        propertyChanged: static (b, _, n) => ((CdsSelect)b).OnItemsSourceChanged((IEnumerable?)n));

    /// <summary>Bindable property for <see cref="ItemToString"/>.</summary>
    public static readonly BindableProperty ItemToStringProperty = BindableProperty.Create(
        nameof(ItemToString), typeof(Func<object, string>), typeof(CdsSelect), null,
        propertyChanged: static (b, _, _) => ((CdsSelect)b).Rebuild());

    /// <summary>Bindable property for <see cref="SelectedItem"/>.</summary>
    public static readonly BindableProperty SelectedItemProperty = BindableProperty.Create(
        nameof(SelectedItem), typeof(object), typeof(CdsSelect), null, BindingMode.TwoWay,
        propertyChanged: static (b, _, n) => ((CdsSelect)b).OnSelectedItemPropertyChanged(n));

    /// <summary>Bindable property for <see cref="SelectedIndex"/>.</summary>
    public static readonly BindableProperty SelectedIndexProperty = BindableProperty.Create(
        nameof(SelectedIndex), typeof(int), typeof(CdsSelect), -1, BindingMode.TwoWay,
        propertyChanged: static (b, _, n) => ((CdsSelect)b).OnSelectedIndexPropertyChanged((int)n));

    /// <summary>Bindable property for <see cref="Placeholder"/>.</summary>
    public static readonly BindableProperty PlaceholderProperty = BindableProperty.Create(
        nameof(Placeholder), typeof(string), typeof(CdsSelect), null,
        propertyChanged: static (b, _, _) => ((CdsSelect)b).UpdateChrome());

    /// <summary>The data the list is built from; it replaces <see cref="CdsSelect.Items"/> while set.</summary>
    public IEnumerable? ItemsSource
    {
        get => (IEnumerable?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    /// <summary>Maps a data item to its row text (Carbon's <c>itemToString</c>). Default <c>ToString()</c>.</summary>
    public Func<object, string>? ItemToString
    {
        get => (Func<object, string>?)GetValue(ItemToStringProperty);
        set => SetValue(ItemToStringProperty, value);
    }

    /// <summary>The chosen item (two-way): a data item, or a <see cref="CdsSelectItem.Value"/>.</summary>
    public object? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    /// <summary>The index of the chosen row in the list as shown (two-way); -1 when nothing is chosen.</summary>
    public int SelectedIndex
    {
        get => (int)GetValue(SelectedIndexProperty);
        set => SetValue(SelectedIndexProperty, value);
    }

    /// <summary>Text shown while nothing is chosen. Prefer preselecting the common default; use this only when a choice is required.</summary>
    public string? Placeholder
    {
        get => (string?)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }
}

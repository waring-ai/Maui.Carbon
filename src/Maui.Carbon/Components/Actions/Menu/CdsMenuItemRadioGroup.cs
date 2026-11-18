using System.Collections.ObjectModel;

namespace Maui.Carbon;

/// <summary>
/// A group of mutually exclusive choices (a sort order). Each entry of <see cref="Items"/> becomes a row with a leading
/// checkmark when it is the <see cref="SelectedItem"/>. Choosing a row selects it and closes the menu.
/// </summary>
[ContentProperty(nameof(Items))]
public class CdsMenuItemRadioGroup : CdsMenuItemBase
{
    /// <summary>Bindable property for <see cref="Label"/>.</summary>
    public static readonly BindableProperty LabelProperty = BindableProperty.Create(
        nameof(Label), typeof(string), typeof(CdsMenuItemRadioGroup), null);

    /// <summary>Bindable property for <see cref="SelectedItem"/>.</summary>
    public static readonly BindableProperty SelectedItemProperty = BindableProperty.Create(
        nameof(SelectedItem), typeof(object), typeof(CdsMenuItemRadioGroup), null, BindingMode.TwoWay,
        propertyChanged: static (b, o, n) =>
            ((CdsMenuItemRadioGroup)b).SelectedItemChanged?.Invoke(b, new CdsMenuSelectionChangedEventArgs(o, n)));

    /// <summary>Raised when <see cref="SelectedItem"/> changes.</summary>
    public event EventHandler<CdsMenuSelectionChangedEventArgs>? SelectedItemChanged;

    /// <summary>The accessible name of the group.</summary>
    public string? Label
    {
        get => (string?)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <summary>The choices: strings, or objects shown with <see cref="ItemToString"/>.</summary>
    public IList<object> Items { get; } = new CdsItemCollection<object>();

    /// <summary>The selected choice. Two-way.</summary>
    public object? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    /// <summary>Converts a choice to its label (Carbon's <c>itemToString</c>). Default: the string itself, else <c>ToString()</c>.</summary>
    public Func<object, string>? ItemToString { get; set; }

    internal string GetLabel(object item) => ItemToString?.Invoke(item) ?? item as string ?? item.ToString() ?? string.Empty;
}

/// <summary>Arguments of the selection events of menu items.</summary>
public sealed class CdsMenuSelectionChangedEventArgs : EventArgs
{
    /// <summary>Creates the arguments.</summary>
    public CdsMenuSelectionChangedEventArgs(object? oldValue, object? newValue)
    {
        OldValue = oldValue;
        NewValue = newValue;
    }

    /// <summary>The previous selection.</summary>
    public object? OldValue { get; }

    /// <summary>The new selection.</summary>
    public object? NewValue { get; }
}

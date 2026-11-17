namespace Maui.Carbon;

/// <summary>
/// A menu item that toggles <see cref="IsSelected"/> when chosen and shows a 16px checkmark in a leading column
/// while selected. Choosing it closes the menu. Use it for settings experienced users flip often.
/// </summary>
public class CdsMenuItemSelectable : CdsMenuItemBase
{
    /// <summary>Bindable property for <see cref="Text"/>.</summary>
    public static readonly BindableProperty TextProperty = BindableProperty.Create(
        nameof(Text), typeof(string), typeof(CdsMenuItemSelectable), null);

    /// <summary>Bindable property for <see cref="Icon"/>.</summary>
    public static readonly BindableProperty IconProperty = BindableProperty.Create(
        nameof(Icon), typeof(CdsIconName?), typeof(CdsMenuItemSelectable), null);

    /// <summary>Bindable property for <see cref="Shortcut"/>.</summary>
    public static readonly BindableProperty ShortcutProperty = BindableProperty.Create(
        nameof(Shortcut), typeof(string), typeof(CdsMenuItemSelectable), null);

    /// <summary>Bindable property for <see cref="IsEnabled"/>.</summary>
    public static readonly BindableProperty IsEnabledProperty = BindableProperty.Create(
        nameof(IsEnabled), typeof(bool), typeof(CdsMenuItemSelectable), true);

    /// <summary>Bindable property for <see cref="IsSelected"/>.</summary>
    public static readonly BindableProperty IsSelectedProperty = BindableProperty.Create(
        nameof(IsSelected), typeof(bool), typeof(CdsMenuItemSelectable), false, BindingMode.TwoWay,
        propertyChanged: static (b, _, n) =>
            ((CdsMenuItemSelectable)b).SelectedChanged?.Invoke(b, new CdsMenuSelectionChangedEventArgs(!(bool)n, (bool)n)));

    /// <summary>Raised when <see cref="IsSelected"/> changes.</summary>
    public event EventHandler<CdsMenuSelectionChangedEventArgs>? SelectedChanged;

    /// <summary>The label.</summary>
    public string? Text
    {
        get => (string?)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>A 16px icon in its own column, after the checkmark column.</summary>
    public CdsIconName? Icon
    {
        get => (CdsIconName?)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>Shortcut text shown at the end of the row.</summary>
    public string? Shortcut
    {
        get => (string?)GetValue(ShortcutProperty);
        set => SetValue(ShortcutProperty, value);
    }

    /// <summary>Disabled items are skipped by the keyboard and cannot be toggled.</summary>
    public bool IsEnabled
    {
        get => (bool)GetValue(IsEnabledProperty);
        set => SetValue(IsEnabledProperty, value);
    }

    /// <summary>Whether the item is checked. Two-way.</summary>
    public bool IsSelected
    {
        get => (bool)GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }
}

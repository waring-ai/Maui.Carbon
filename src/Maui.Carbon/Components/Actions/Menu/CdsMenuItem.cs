using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Maui.Carbon;

/// <summary>
/// An action in a <see cref="CdsMenu"/>: label, optional leading icon and trailing shortcut text. Give it nested
/// <see cref="Items"/> to make it a submenu (one level at most). Choosing it closes the menu, then runs
/// <see cref="Clicked"/> and <see cref="Command"/>.
/// </summary>
[ContentProperty(nameof(Items))]
public class CdsMenuItem : CdsMenuItemBase
{
    /// <summary>Bindable property for <see cref="Text"/>.</summary>
    public static readonly BindableProperty TextProperty = BindableProperty.Create(
        nameof(Text), typeof(string), typeof(CdsMenuItem), null);

    /// <summary>Bindable property for <see cref="Icon"/>.</summary>
    public static readonly BindableProperty IconProperty = BindableProperty.Create(
        nameof(Icon), typeof(CdsIconName?), typeof(CdsMenuItem), null);

    /// <summary>Bindable property for <see cref="Shortcut"/>.</summary>
    public static readonly BindableProperty ShortcutProperty = BindableProperty.Create(
        nameof(Shortcut), typeof(string), typeof(CdsMenuItem), null);

    /// <summary>Bindable property for <see cref="Kind"/>.</summary>
    public static readonly BindableProperty KindProperty = BindableProperty.Create(
        nameof(Kind), typeof(CdsMenuItemKind), typeof(CdsMenuItem), CdsMenuItemKind.Default);

    /// <summary>Bindable property for <see cref="IsEnabled"/>.</summary>
    public static readonly BindableProperty IsEnabledProperty = BindableProperty.Create(
        nameof(IsEnabled), typeof(bool), typeof(CdsMenuItem), true);

    /// <summary>Bindable property for <see cref="Command"/>.</summary>
    public static readonly BindableProperty CommandProperty = BindableProperty.Create(
        nameof(Command), typeof(ICommand), typeof(CdsMenuItem), null);

    /// <summary>Bindable property for <see cref="CommandParameter"/>.</summary>
    public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(
        nameof(CommandParameter), typeof(object), typeof(CdsMenuItem), null);

    readonly ObservableCollection<CdsMenuItemBase> _items;

    /// <summary>Creates an empty item.</summary>
    public CdsMenuItem() => _items = CreateChildren<CdsMenuItemBase>();

    /// <summary>Creates an item with a label.</summary>
    public CdsMenuItem(string text, CdsIconName? icon = null) : this()
    {
        Text = text;
        Icon = icon;
    }

    /// <summary>Raised when the item is chosen (after the menu closed), before <see cref="Command"/> runs.</summary>
    public event EventHandler? Clicked;

    /// <summary>The label: sentence case, verb + noun ("Download logs"). Long labels truncate with an ellipsis.</summary>
    public string? Text
    {
        get => (string?)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>A 16px icon in its own leading column. When any item of a menu has one, all items reserve the column.</summary>
    public CdsIconName? Icon
    {
        get => (CdsIconName?)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>Shortcut text shown at the end of the row ("Ctrl+D").</summary>
    public string? Shortcut
    {
        get => (string?)GetValue(ShortcutProperty);
        set => SetValue(ShortcutProperty, value);
    }

    /// <summary><see cref="CdsMenuItemKind.Danger"/> for destructive actions only.</summary>
    public CdsMenuItemKind Kind
    {
        get => (CdsMenuItemKind)GetValue(KindProperty);
        set => SetValue(KindProperty, value);
    }

    /// <summary>Disabled items are skipped by the keyboard and cannot be chosen. Hide actions a user can never perform instead.</summary>
    public bool IsEnabled
    {
        get => (bool)GetValue(IsEnabledProperty);
        set => SetValue(IsEnabledProperty, value);
    }

    /// <summary>Executed when the item is chosen; <c>CanExecute</c> also gates <see cref="IsEnabled"/>.</summary>
    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    /// <summary>Parameter for <see cref="Command"/>.</summary>
    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    /// <summary>Nested items. When not empty the item opens a submenu (Right arrow or hover) instead of running an action.</summary>
    public IList<CdsMenuItemBase> Items => _items;

    /// <summary>True when the item can be chosen: enabled and its command, if any, can execute.</summary>
    internal bool IsActionable => IsEnabled && (Command?.CanExecute(CommandParameter) ?? true);

    internal override IEnumerable<BindableObject> ChildItems() => _items;

    /// <summary>Raises <see cref="Clicked"/> and runs the command.</summary>
    internal void Invoke()
    {
        Clicked?.Invoke(this, EventArgs.Empty);
        if (Command?.CanExecute(CommandParameter) == true) Command.Execute(CommandParameter);
    }
}

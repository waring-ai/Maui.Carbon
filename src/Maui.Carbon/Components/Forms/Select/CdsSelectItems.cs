namespace Maui.Carbon;

/// <summary>The base of the declarative children of a <see cref="CdsSelect"/>: <see cref="CdsSelectItem"/> and <see cref="CdsSelectItemGroup"/>.</summary>
public abstract class CdsSelectOption : BindableObject
{
    /// <summary>Raised when any property of the option changes, so the select rebuilds its list.</summary>
    internal event EventHandler? Changed;

    /// <inheritdoc/>
    protected override void OnPropertyChanged(string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
        Changed?.Invoke(this, EventArgs.Empty);
    }
}

/// <summary>One option of a <see cref="CdsSelect"/> (Carbon's <c>SelectItem</c>).</summary>
public sealed class CdsSelectItem : CdsSelectOption
{
    /// <summary>Bindable property for <see cref="Value"/>.</summary>
    public static readonly BindableProperty ValueProperty = BindableProperty.Create(
        nameof(Value), typeof(object), typeof(CdsSelectItem), null);

    /// <summary>Bindable property for <see cref="Text"/>.</summary>
    public static readonly BindableProperty TextProperty = BindableProperty.Create(
        nameof(Text), typeof(string), typeof(CdsSelectItem), null);

    /// <summary>Bindable property for <see cref="IsDisabled"/>.</summary>
    public static readonly BindableProperty IsDisabledProperty = BindableProperty.Create(
        nameof(IsDisabled), typeof(bool), typeof(CdsSelectItem), false);

    /// <summary>Bindable property for <see cref="IsHidden"/>.</summary>
    public static readonly BindableProperty IsHiddenProperty = BindableProperty.Create(
        nameof(IsHidden), typeof(bool), typeof(CdsSelectItem), false);

    /// <summary>The value reported as the select's <see cref="CdsSelect.SelectedItem"/>; defaults to <see cref="Text"/>.</summary>
    public object? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    /// <summary>The text shown in the list.</summary>
    public string? Text
    {
        get => (string?)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>Disabled items are left out of the list: the platform's native list cannot show a disabled row.</summary>
    public bool IsDisabled
    {
        get => (bool)GetValue(IsDisabledProperty);
        set => SetValue(IsDisabledProperty, value);
    }

    /// <summary>Hidden items are left out of the list.</summary>
    public bool IsHidden
    {
        get => (bool)GetValue(IsHiddenProperty);
        set => SetValue(IsHiddenProperty, value);
    }
}

/// <summary>
/// A labelled group of options (Carbon's <c>SelectItemGroup</c>). The platform's native list cannot show group headings,
/// so a <see cref="CdsSelect"/> flattens the group: its items appear in order, without a heading.
/// </summary>
[ContentProperty(nameof(Items))]
public sealed class CdsSelectItemGroup : CdsSelectOption
{
    /// <summary>Bindable property for <see cref="Label"/>.</summary>
    public static readonly BindableProperty LabelProperty = BindableProperty.Create(
        nameof(Label), typeof(string), typeof(CdsSelectItemGroup), null);

    /// <summary>Bindable property for <see cref="IsDisabled"/>.</summary>
    public static readonly BindableProperty IsDisabledProperty = BindableProperty.Create(
        nameof(IsDisabled), typeof(bool), typeof(CdsSelectItemGroup), false);

    /// <summary>Creates a group.</summary>
    public CdsSelectItemGroup()
    {
        Items.CollectionChanged += (_, _) => OnPropertyChanged(nameof(Items));
    }

    /// <summary>The group heading (kept for the API; the native list flattens groups).</summary>
    public string? Label
    {
        get => (string?)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <summary>Disables every item of the group.</summary>
    public bool IsDisabled
    {
        get => (bool)GetValue(IsDisabledProperty);
        set => SetValue(IsDisabledProperty, value);
    }

    /// <summary>The items of the group.</summary>
    public System.Collections.ObjectModel.ObservableCollection<CdsSelectItem> Items { get; } = new CdsItemCollection<CdsSelectItem>();
}

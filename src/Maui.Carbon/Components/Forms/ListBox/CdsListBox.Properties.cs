using System.Collections;

namespace Maui.Carbon;

public abstract partial class CdsListBox
{
    /// <summary>Bindable property for <see cref="ItemsSource"/>.</summary>
    public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(
        nameof(ItemsSource), typeof(IEnumerable), typeof(CdsListBox), null,
        propertyChanged: static (b, _, n) => ((CdsListBox)b).OnItemsSourceChanged((IEnumerable?)n));

    /// <summary>Bindable property for <see cref="ItemToString"/>.</summary>
    public static readonly BindableProperty ItemToStringProperty = BindableProperty.Create(
        nameof(ItemToString), typeof(Func<object?, string>), typeof(CdsListBox), null,
        propertyChanged: static (b, _, _) => ((CdsListBox)b).RebuildItems());

    /// <summary>Bindable property for <see cref="ItemTemplate"/>.</summary>
    public static readonly BindableProperty ItemTemplateProperty = BindableProperty.Create(
        nameof(ItemTemplate), typeof(DataTemplate), typeof(CdsListBox), null,
        propertyChanged: static (b, _, _) => ((CdsListBox)b).RefreshRows(keepHighlight: false));

    /// <summary>Bindable property for <see cref="IsItemDisabled"/>.</summary>
    public static readonly BindableProperty IsItemDisabledProperty = BindableProperty.Create(
        nameof(IsItemDisabled), typeof(Func<object?, bool>), typeof(CdsListBox), null,
        propertyChanged: static (b, _, _) => ((CdsListBox)b).RebuildItems());

    /// <summary>Bindable property for <see cref="Placeholder"/>.</summary>
    public static readonly BindableProperty PlaceholderProperty = BindableProperty.Create(
        nameof(Placeholder), typeof(string), typeof(CdsListBox), null,
        propertyChanged: static (b, _, _) => ((CdsListBox)b).UpdateChrome());

    /// <summary>Bindable property for <see cref="Direction"/>.</summary>
    public static readonly BindableProperty DirectionProperty = BindableProperty.Create(
        nameof(Direction), typeof(CdsListBoxDirection), typeof(CdsListBox), CdsListBoxDirection.Bottom);

    /// <summary>Bindable property for <see cref="Type"/>.</summary>
    public static readonly BindableProperty TypeProperty = BindableProperty.Create(
        nameof(Type), typeof(CdsListBoxType), typeof(CdsListBox), CdsListBoxType.Default,
        propertyChanged: static (b, _, _) => ((CdsListBox)b).UpdateChrome());

    /// <summary>Bindable property for <see cref="IsOpen"/>.</summary>
    public static readonly BindableProperty IsOpenProperty = BindableProperty.Create(
        nameof(IsOpen), typeof(bool), typeof(CdsListBox), false, BindingMode.TwoWay,
        coerceValue: static (b, v) => (bool)v && ((CdsListBox)b).CanOpen,
        propertyChanged: static (b, _, _) => ((CdsListBox)b).OnIsOpenChanged());

    /// <summary>
    /// The data the list is built from: strings, or objects shown through <see cref="ItemToString"/>. An
    /// <see cref="System.Collections.Specialized.INotifyCollectionChanged"/> source updates the list live.
    /// </summary>
    public IEnumerable? ItemsSource
    {
        get => (IEnumerable?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    /// <summary>Maps an item to its row text (Carbon's <c>itemToString</c>). Default <c>ToString()</c>; <c>null</c> items map to "".</summary>
    public Func<object?, string>? ItemToString
    {
        get => (Func<object?, string>?)GetValue(ItemToStringProperty);
        set => SetValue(ItemToStringProperty, value);
    }

    /// <summary>
    /// Renders a rich row for each item (Carbon's <c>itemToElement</c>); the template's binding context is the item. The field
    /// itself still shows <see cref="ItemToString"/> text. Keep rows to one line.
    /// </summary>
    public DataTemplate? ItemTemplate
    {
        get => (DataTemplate?)GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    /// <summary>Tells which items are disabled (Carbon's <c>disabled: true</c> on an item). Disabled rows are skipped by the keyboard and cannot be chosen.</summary>
    public Func<object?, bool>? IsItemDisabled
    {
        get => (Func<object?, bool>?)GetValue(IsItemDisabledProperty);
        set => SetValue(IsItemDisabledProperty, value);
    }

    /// <summary>
    /// Text shown in the field while nothing is chosen (Carbon's <c>label</c> for a dropdown or multi-select, <c>placeholder</c>
    /// for a combo box). Say what to do ("Choose a region"); it never replaces <see cref="CdsFormField.Label"/>.
    /// </summary>
    public string? Placeholder
    {
        get => (string?)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    /// <summary>Which way the list opens. It still flips when there is no room. Default <see cref="CdsListBoxDirection.Bottom"/>.</summary>
    public CdsListBoxDirection Direction
    {
        get => (CdsListBoxDirection)GetValue(DirectionProperty);
        set => SetValue(DirectionProperty, value);
    }

    /// <summary>
    /// <see cref="CdsListBoxType.Inline"/> drops the fill and underline and puts the label to the start (toolbars, filter bars);
    /// ignored by the combo box, which has no inline variant (as in Carbon).
    /// </summary>
    public CdsListBoxType Type
    {
        get => (CdsListBoxType)GetValue(TypeProperty);
        set => SetValue(TypeProperty, value);
    }

    /// <summary>Whether the list is open (two-way). Setting it before the control loads opens the list once it does.</summary>
    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }
}

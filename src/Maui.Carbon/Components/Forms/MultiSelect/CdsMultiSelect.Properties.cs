using System.Collections;

namespace Maui.Carbon;

public partial class CdsMultiSelect
{
    /// <summary>Bindable property for <see cref="SelectedItems"/>.</summary>
    public static readonly BindableProperty SelectedItemsProperty = BindableProperty.Create(
        nameof(SelectedItems), typeof(IList), typeof(CdsMultiSelect), null, BindingMode.TwoWay,
        propertyChanged: static (b, o, n) => ((CdsMultiSelect)b).OnSelectedItemsChanged((IList?)o, (IList?)n));

    /// <summary>Bindable property for <see cref="IsFilterable"/>.</summary>
    public static readonly BindableProperty IsFilterableProperty = BindableProperty.Create(
        nameof(IsFilterable), typeof(bool), typeof(CdsMultiSelect), false,
        propertyChanged: static (b, _, _) => ((CdsMultiSelect)b).OnFilterableChanged());

    /// <summary>Bindable property for <see cref="SelectionFeedback"/>.</summary>
    public static readonly BindableProperty SelectionFeedbackProperty = BindableProperty.Create(
        nameof(SelectionFeedback), typeof(CdsMultiSelectFeedback), typeof(CdsMultiSelect), CdsMultiSelectFeedback.TopAfterReopen,
        propertyChanged: static (b, _, _) => ((CdsMultiSelect)b).RefreshRows(keepHighlight: true));

    /// <summary>Bindable property for <see cref="SortSelectedFirst"/>.</summary>
    public static readonly BindableProperty SortSelectedFirstProperty = BindableProperty.Create(
        nameof(SortSelectedFirst), typeof(bool), typeof(CdsMultiSelect), true,
        propertyChanged: static (b, _, _) => ((CdsMultiSelect)b).RefreshRows(keepHighlight: true));

    /// <summary>Bindable property for <see cref="ShouldFilterItem"/>.</summary>
    public static readonly BindableProperty ShouldFilterItemProperty = BindableProperty.Create(
        nameof(ShouldFilterItem), typeof(Func<object?, string, bool>), typeof(CdsMultiSelect), null,
        propertyChanged: static (b, _, _) => ((CdsMultiSelect)b).RefreshRows(keepHighlight: false));

    /// <summary>
    /// The selected items (two-way). A list the application owns is updated in place when it allows it (an
    /// <c>ObservableCollection</c>); otherwise <see cref="CdsMultiSelect"/> assigns a new list.
    /// </summary>
    public IList? SelectedItems
    {
        get => (IList?)GetValue(SelectedItemsProperty);
        set => SetValue(SelectedItemsProperty, value);
    }

    /// <summary>Turns the field into a text input that narrows the list as you type; the list stays open while choosing. Not for filterable lists in forms with <see cref="CdsListBox.Type"/> inline.</summary>
    public bool IsFilterable
    {
        get => (bool)GetValue(IsFilterableProperty);
        set => SetValue(IsFilterableProperty, value);
    }

    /// <summary>Where selected rows sit in the open list. Default <see cref="CdsMultiSelectFeedback.TopAfterReopen"/>.</summary>
    public CdsMultiSelectFeedback SelectionFeedback
    {
        get => (CdsMultiSelectFeedback)GetValue(SelectionFeedbackProperty);
        set => SetValue(SelectionFeedbackProperty, value);
    }

    /// <summary>
    /// Lists selected rows first (per <see cref="SelectionFeedback"/>), keeping their order and the order of the rest. Turn it
    /// off to keep the order of the items whatever the feedback. Default <c>true</c>.
    /// </summary>
    public bool SortSelectedFirst
    {
        get => (bool)GetValue(SortSelectedFirstProperty);
        set => SetValue(SortSelectedFirstProperty, value);
    }

    /// <summary>Replaces the default filter of <see cref="IsFilterable"/> fields, a case-insensitive "contains" match on the row text. Called with the item and the typed text.</summary>
    public Func<object?, string, bool>? ShouldFilterItem
    {
        get => (Func<object?, string, bool>?)GetValue(ShouldFilterItemProperty);
        set => SetValue(ShouldFilterItemProperty, value);
    }
}

using System.Collections.ObjectModel;

namespace Maui.Carbon;

/// <summary>
/// Items that belong together. Carbon does not draw the label: it is the accessible name of the group (screen
/// readers announce it), so give each group one. Use a <see cref="CdsMenuItemDivider"/> for a visible break.
/// </summary>
[ContentProperty(nameof(Items))]
public class CdsMenuItemGroup : CdsMenuItemBase
{
    /// <summary>Bindable property for <see cref="Label"/>.</summary>
    public static readonly BindableProperty LabelProperty = BindableProperty.Create(
        nameof(Label), typeof(string), typeof(CdsMenuItemGroup), null);

    readonly CdsItemCollection<CdsMenuItemBase> _items;

    /// <summary>Creates an empty group.</summary>
    public CdsMenuItemGroup() => _items = CreateChildren<CdsMenuItemBase>();

    /// <summary>The accessible name of the group.</summary>
    public string? Label
    {
        get => (string?)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <summary>The items of the group.</summary>
    public IList<CdsMenuItemBase> Items => _items;

    internal override IEnumerable<BindableObject> ChildItems() => _items;
}

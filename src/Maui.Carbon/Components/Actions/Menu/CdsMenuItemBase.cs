using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Maui.Carbon;

/// <summary>Base of everything that can be placed in a <see cref="CdsMenu"/>.</summary>
public abstract class CdsMenuItemBase : BindableObject
{
    internal CdsMenuItemBase()
    {
    }

    /// <summary>Creates a collection that hands this item's binding context down to its children.</summary>
    internal CdsItemCollection<T> CreateChildren<T>() where T : BindableObject
    {
        var list = new CdsItemCollection<T>();
        list.CollectionChanged += (_, e) =>
        {
            if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems is not null)
                foreach (T child in e.NewItems) SetInherited(child, BindingContext);
        };
        return list;
    }

    internal static void SetInherited(BindableObject child, object? context) =>
        child.BindingContext = context;

    /// <inheritdoc/>
    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        foreach (var child in ChildItems()) SetInherited(child, BindingContext);
    }

    /// <summary>The nested items whose binding context follows this item's.</summary>
    internal virtual IEnumerable<BindableObject> ChildItems() => [];
}

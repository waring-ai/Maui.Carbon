using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Maui.Carbon;

/// <summary>
/// The child collections of Carbon controls (<c>Items</c>, <c>Rows</c>, <c>Steps</c>…). <see cref="Collection{T}.Clear"/>
/// raises one <c>Remove</c> that lists every cleared item instead of a <c>Reset</c>: a Reset carries no <c>OldItems</c>, so
/// a control that takes its old children out of its layout and drops their event handlers from <c>OldItems</c> would keep
/// showing (and keep alive) everything that was cleared. One event, so a control that rebuilds on a change rebuilds once.
/// </summary>
internal sealed class CdsItemCollection<T> : ObservableCollection<T>
{
    protected override void ClearItems()
    {
        CheckReentrancy();
        if (Count == 0) return;
        var removed = Items.ToList();
        Items.Clear();
        OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
        OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removed, 0));
    }
}

namespace Maui.Carbon;

/// <summary>
/// A handler on another object's event that does not keep the subscriber alive: the source holds this small proxy, the proxy
/// holds the subscriber weakly. Use it when the source can outlive the subscriber (a view model's command, the shell), so a
/// view removed from the page can still be collected. Once the subscriber is gone the proxy removes itself the next time the
/// event is raised; <see cref="Dispose"/> removes it at once.
/// </summary>
internal sealed class CdsWeakSubscription<TSubscriber> : IDisposable where TSubscriber : class
{
    readonly WeakReference<TSubscriber> _subscriber;
    readonly Action<TSubscriber> _onRaised;
    readonly EventHandler _handler;
    Action<EventHandler>? _remove;

    /// <param name="subscriber">The object to notify.</param>
    /// <param name="onRaised">Called with the subscriber; must not capture it (use a <c>static</c> lambda), or the proxy keeps it alive.</param>
    /// <param name="add">Adds the handler to the source's event.</param>
    /// <param name="remove">Removes it again.</param>
    public CdsWeakSubscription(TSubscriber subscriber, Action<TSubscriber> onRaised, Action<EventHandler> add, Action<EventHandler> remove)
    {
        _subscriber = new WeakReference<TSubscriber>(subscriber);
        _onRaised = onRaised;
        _handler = OnRaised;
        _remove = remove;
        add(_handler);
    }

    /// <summary>Removes the handler from the source.</summary>
    public void Dispose()
    {
        var remove = _remove;
        _remove = null;
        remove?.Invoke(_handler);
    }

    void OnRaised(object? sender, EventArgs e)
    {
        if (_remove is null) return;
        if (_subscriber.TryGetTarget(out var subscriber)) _onRaised(subscriber);
        else Dispose();
    }
}

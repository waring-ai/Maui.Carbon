namespace Maui.Carbon;

/// <summary>
/// Runs a control's rebuild once per batch of changes while it is on screen. Filling a shown list item by item would
/// otherwise rebuild it once per item (N items cost N rebuilds of up to N items). Off screen (no handler, as while XAML
/// builds the page) the rebuild runs at once, so the control is complete before it is shown. Every run is measured as
/// <see cref="CdsMetrics.RebuildDuration"/>, tagged with the owner's type.
/// </summary>
internal sealed class CdsCoalescedAction(VisualElement owner, Action action)
{
    bool _pending;

    /// <summary>Whether a run is queued.</summary>
    public bool IsPending => _pending;

    /// <summary>Runs the action now (off screen) or queues one run after the current changes (on screen).</summary>
    public void Request()
    {
        if (_pending) return;
        if (owner.Handler is not null && owner.Dispatcher is { } dispatcher)
        {
            _pending = true;
            dispatcher.Dispatch(Flush);
            return;
        }
        Run();
    }

    /// <summary>Drops a queued run (call it when the action runs by other means).</summary>
    public void Cancel() => _pending = false;

    /// <summary>Runs a queued run now.</summary>
    public void Flush()
    {
        if (!_pending) return;
        _pending = false;
        Run();
    }

    void Run()
    {
        var start = CdsMeter.Start(CdsMeter.Rebuild);
        try
        {
            action();
        }
        finally
        {
            CdsMeter.Stop(CdsMeter.Rebuild, start, owner);
        }
    }
}

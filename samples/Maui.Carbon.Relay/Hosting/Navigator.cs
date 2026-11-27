using Maui.Carbon.Relay.Abstractions;
using Microsoft.Extensions.Logging;

namespace Maui.Carbon.Relay.Hosting;

/// <summary>
/// Navigation inside <see cref="CdsUIShell"/>: the shell has no navigation of its own (its links only raise
/// <c>Clicked</c>), so this changes what its <see cref="CdsContainer.Body"/> shows. A route maps to a view and a view model
/// from DI. Top-level routes reset the back stack; detail routes push onto it. The view model is told when its screen is
/// shown (<see cref="IActivatable.ActivateAsync"/>, with the parameter) and left.
/// <para>
/// The body is a <see cref="CdsDeck"/> of screens with one active. Screens mapped with <c>keepAttached</c> (the singleton
/// top-level ones) stay in it, hidden while not shown: taking a large screen out of the window and putting it back costs
/// MAUI hundreds of milliseconds each time (every element re-propagates the window and resources, and re-creates its native
/// gestures and subviews), and so does hiding it with <c>IsVisible</c> (MAUI pushes the visibility to every descendant);
/// the deck hides only the screen's own native view. Other screens leave the deck when they leave the back stack, with their
/// handlers disconnected so they can be collected: a removed subtree otherwise keeps its native views and the Loaded and
/// window hooks MAUI put on them, which on iOS and Mac Catalyst keep the whole screen alive.
/// </para>
/// </summary>
public sealed class Navigator(IServiceProvider services, ILogger<Navigator> logger) : INavigator
{
    readonly Dictionary<string, (Type View, Type ViewModel, bool Keep)> _routes = [];
    readonly Stack<Entry> _stack = new();
    readonly CdsDeck _deck = new();
    readonly HashSet<View> _kept = [];
    CdsUIShell? _shell;
    Entry? _pending;

    sealed record Entry(string Route, object? Parameter, View View, IActivatable ViewModel);

    public string? CurrentRoute => _stack.TryPeek(out var top) ? top.Route : null;

    public bool CanGoBack => _stack.Count > 1;

    /// <summary>The view shown in the shell body.</summary>
    public View? CurrentView => _stack.TryPeek(out var top) ? top.View : null;

    public event EventHandler<string>? Navigated;

    /// <summary>
    /// Registers a route. Register views and view models in DI too (singletons keep their state between visits). Pass
    /// <paramref name="keepAttached"/> for a singleton view: it then stays in the window, hidden, between visits.
    /// </summary>
    public Navigator Map<TView, TViewModel>(string route, bool keepAttached = false)
        where TView : View
        where TViewModel : class, IActivatable
    {
        _routes[route] = (typeof(TView), typeof(TViewModel), keepAttached);
        return this;
    }

    /// <summary>Connects the shell whose body this navigator drives, and shows whatever was requested before.</summary>
    public void Attach(CdsUIShell shell)
    {
        _shell = shell;
        shell.Body = _deck;
    }

    /// <summary>Forgets the stack (sign out) so the next sign-in starts fresh.</summary>
    public void Reset()
    {
        while (_stack.TryPop(out var e)) e.ViewModel.Deactivate();
        var screens = _deck.Children.OfType<View>().ToList();
        _deck.Children.Clear();
        foreach (var screen in screens) screen.DisconnectHandlers();
        _kept.Clear();
    }

    public async Task NavigateAsync(string route, object? parameter = null)
    {
        if (!_routes.TryGetValue(route, out var target)) throw new ArgumentException($"Unknown route '{route}'.", nameof(route));
        if (_stack.TryPeek(out var current))
        {
            if (current.Route == route && Equals(current.Parameter, parameter)) return;
            current.ViewModel.Deactivate();
        }
        if (!Routes.IsDetail(route))
        {
            _stack.Clear();
        }
        else if (_stack.Any(e => e.Route == route))
        {
            // Opening a detail from a detail of the same kind replaces it rather than growing the stack.
            while (_stack.TryPeek(out var top) && top.Route != route) _stack.Pop();
            _stack.TryPop(out _);
        }

        var view = (View)services.GetRequiredService(target.View);
        var viewModel = (IActivatable)services.GetRequiredService(target.ViewModel);
        view.BindingContext = viewModel;
        if (target.Keep) _kept.Add(view);
        var entry = new Entry(route, parameter, view, viewModel);
        _stack.Push(entry);
        await ShowAsync(entry);
    }

    public async Task GoBackAsync()
    {
        if (_stack.Count <= 1) return;
        _stack.Pop().ViewModel.Deactivate();
        await ShowAsync(_stack.Peek());
    }

    async Task ShowAsync(Entry entry)
    {
        _pending = entry;
        Present(entry.View);
        Navigated?.Invoke(this, entry.Route);
        logger.LogInformation("Showing {Route} {Parameter}", entry.Route, entry.Parameter);
        try
        {
            await entry.ViewModel.ActivateAsync(entry.Parameter);
        }
        catch (Exception ex)
        {
            // Callers often fire and forget navigation, which would hide this until the task is collected.
            logger.LogError(ex, "Loading {Route} {Parameter} failed", entry.Route, entry.Parameter);
            throw;
        }
        if (ReferenceEquals(_pending, entry)) _pending = null;
    }

    /// <summary>Shows <paramref name="view"/> in the deck, hides the rest, and drops screens nothing needs any more.</summary>
    void Present(View view)
    {
        if (view.Parent != _deck) _deck.Add(view);
        _deck.Active = view;
        foreach (var child in _deck.Children.OfType<View>().ToList())
        {
            if (ReferenceEquals(child, view) || _kept.Contains(child) || _stack.Any(e => ReferenceEquals(e.View, child))) continue;
            _deck.Remove(child);
            child.DisconnectHandlers();
        }
    }
}

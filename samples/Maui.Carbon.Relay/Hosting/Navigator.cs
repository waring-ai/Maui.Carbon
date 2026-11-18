using Maui.Carbon.Relay.Abstractions;
using Microsoft.Extensions.Logging;

namespace Maui.Carbon.Relay.Hosting;

/// <summary>
/// Navigation inside <see cref="CdsUIShell"/>: the shell has no navigation of its own (its links only raise
/// <c>Clicked</c>), so this swaps <see cref="CdsContainer.Body"/>. A route maps to a view and a view model from DI.
/// Top-level routes reset the back stack; detail routes push onto it. The view model is told when its screen is shown
/// (<see cref="IActivatable.ActivateAsync"/>, with the parameter) and left.
/// </summary>
public sealed class Navigator(IServiceProvider services, ILogger<Navigator> logger) : INavigator
{
    readonly Dictionary<string, (Type View, Type ViewModel)> _routes = [];
    readonly Stack<Entry> _stack = new();
    CdsUIShell? _shell;
    Entry? _pending;

    sealed record Entry(string Route, object? Parameter, View View, IActivatable ViewModel);

    public string? CurrentRoute => _stack.TryPeek(out var top) ? top.Route : null;

    public bool CanGoBack => _stack.Count > 1;

    public event EventHandler<string>? Navigated;

    /// <summary>Registers a route. Register views and view models in DI too (singletons keep their state between visits).</summary>
    public Navigator Map<TView, TViewModel>(string route)
        where TView : View
        where TViewModel : class, IActivatable
    {
        _routes[route] = (typeof(TView), typeof(TViewModel));
        return this;
    }

    /// <summary>Connects the shell whose body this navigator drives, and shows whatever was requested before.</summary>
    public void Attach(CdsUIShell shell)
    {
        _shell = shell;
        if (_stack.TryPeek(out var top)) shell.Body = top.View;
    }

    /// <summary>Forgets the stack (sign out) so the next sign-in starts fresh.</summary>
    public void Reset()
    {
        while (_stack.TryPop(out var e)) e.ViewModel.Deactivate();
        if (_shell is not null) _shell.Body = null;
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
        if (_shell is not null) _shell.Body = entry.View;
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
}

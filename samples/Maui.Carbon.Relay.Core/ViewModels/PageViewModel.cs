using CommunityToolkit.Mvvm.ComponentModel;
using Maui.Carbon.Relay.Abstractions;

namespace Maui.Carbon.Relay.ViewModels;

/// <summary>
/// Base for screen view models. The navigator calls <see cref="ActivateAsync"/> when the screen is shown and
/// <see cref="Deactivate"/> when it is left; <see cref="Lifetime"/> is cancelled on leave, so loads started for a screen
/// stop when the user moves on. Subscribe to service events in <see cref="OnActivatedAsync"/> and unsubscribe in
/// <see cref="OnDeactivated"/>.
/// </summary>
public abstract partial class PageViewModel : ObservableObject, IActivatable
{
    CancellationTokenSource? _lifetime;

    /// <summary>True while the screen's first load runs (views show skeletons).</summary>
    [ObservableProperty]
    public partial bool IsLoading { get; set; }

    /// <summary>The screen title (page header).</summary>
    [ObservableProperty]
    public partial string Title { get; set; } = "";

    protected CancellationToken Lifetime => _lifetime?.Token ?? CancellationToken.None;

    public async Task ActivateAsync(object? parameter)
    {
        _lifetime?.Cancel();
        _lifetime = new CancellationTokenSource();
        try
        {
            await OnActivatedAsync(parameter, _lifetime.Token);
        }
        catch (OperationCanceledException)
        {
            // Left the screen before the load finished.
        }
    }

    public void Deactivate()
    {
        _lifetime?.Cancel();
        _lifetime = null;
        OnDeactivated();
    }

    protected virtual Task OnActivatedAsync(object? parameter, CancellationToken ct) => Task.CompletedTask;

    protected virtual void OnDeactivated()
    {
    }
}

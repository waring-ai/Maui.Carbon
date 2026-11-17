namespace Maui.Carbon;

/// <summary>
/// Runs an endless animation (spinner rotation, indeterminate sweep, skeleton shimmer) that respects Carbon's reduced
/// motion: when reduced, the callback is invoked once with the rest value and nothing animates.
/// </summary>
internal static class CdsLoop
{
    /// <summary>Starts a looping animation on <paramref name="owner"/> (a no-op without a platform handler).</summary>
    /// <param name="owner">The element that owns the animation (and its lifetime).</param>
    /// <param name="name">Animation name; one loop per name and owner.</param>
    /// <param name="callback">Receives the phase from 0 to 1 on each frame.</param>
    /// <param name="restPhase">The phase shown when motion is reduced.</param>
    /// <param name="duration">Length of one loop in milliseconds.</param>
    /// <returns><c>true</c> when an animation is running.</returns>
    public static bool Start(VisualElement owner, string name, Action<double> callback, double restPhase, uint duration)
    {
        owner.AbortAnimation(name);
        if (CdsMotion.IsReducedMotion || owner.Handler is null)
        {
            callback(restPhase);
            return false;
        }
        new Animation(callback, 0, 1, Easing.Linear).Commit(owner, name, 16, duration, Easing.Linear, repeat: static () => true);
        return true;
    }

    /// <summary>Stops a loop started with <see cref="Start"/>.</summary>
    public static void Stop(VisualElement owner, string name) => owner.AbortAnimation(name);
}

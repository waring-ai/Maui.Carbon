namespace Maui.Carbon;

/// <summary>Carbon motion: durations (<see cref="CdsDuration"/>), easings (<see cref="CdsEasing"/>) and reduced motion.</summary>
public static partial class CdsMotion
{
    /// <summary>Forces reduced motion on (<c>true</c>) or off (<c>false</c>); <c>null</c> follows the OS.</summary>
    public static bool? ReducedMotionOverride { get; set; }

    /// <summary>True when animations should jump to their end state.</summary>
    public static bool IsReducedMotion =>
        ReducedMotionOverride ?? (CdsOptions.Current.RespectReducedMotion && PlatformPrefersReducedMotion());

    /// <summary>Runs an animation callback over <paramref name="duration"/> unless motion is reduced, then sets the final value.</summary>
    public static Task AnimateAsync(VisualElement owner, string name, Action<double> callback, double from, double to, uint duration, Easing easing)
    {
        if (IsReducedMotion || owner.Handler is null)
        {
            callback(to);
            return Task.CompletedTask;
        }
        var tcs = new TaskCompletionSource();
        owner.AbortAnimation(name);
        new Animation(callback, from, to, easing).Commit(owner, name, 16, duration, finished: (_, _) => tcs.TrySetResult());
        return tcs.Task;
    }

    static bool PlatformPrefersReducedMotion()
    {
        var reduced = false;
        PlatformPrefersReducedMotion(ref reduced);
        return reduced;
    }

    static partial void PlatformPrefersReducedMotion(ref bool reduced);
}

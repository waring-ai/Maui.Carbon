using System.Runtime.CompilerServices;
using IAnimationManager = Microsoft.Maui.Animations.IAnimationManager;

namespace Maui.Carbon;

/// <summary>
/// Runs an endless animation (spinner rotation, indeterminate sweep, skeleton shimmer) that respects Carbon's reduced
/// motion: when reduced, the callback is invoked once with the rest value and nothing animates.
/// <para>
/// Every loop of a window runs on one shared frame clock (one animation in MAUI's animation manager), not one MAUI
/// animation each: a screen of skeletons and spinners costs one tick per frame plus a callback per shown loop. A loop
/// whose owner or an ancestor is hidden skips its frames; it keeps running until <see cref="Stop"/> (owners stop on
/// <c>Unloaded</c>).
/// </para>
/// </summary>
internal static class CdsLoop
{
    static readonly ConditionalWeakTable<VisualElement, Dictionary<string, Loop>> Running = new();
    static readonly ConditionalWeakTable<IAnimationManager, Clock> Clocks = new();

    /// <summary>Starts a looping animation on <paramref name="owner"/> (a no-op without a platform handler).</summary>
    /// <param name="owner">The element that owns the animation (and its lifetime).</param>
    /// <param name="name">Animation name; one loop per name and owner.</param>
    /// <param name="callback">Receives the phase from 0 to 1 on each frame.</param>
    /// <param name="restPhase">The phase shown when motion is reduced.</param>
    /// <param name="duration">Length of one loop in milliseconds.</param>
    /// <returns><c>true</c> when an animation is running.</returns>
    public static bool Start(VisualElement owner, string name, Action<double> callback, double restPhase, uint duration) =>
        Start(owner, name, callback, restPhase, duration,
            owner.Handler?.MauiContext?.Services.GetService(typeof(IAnimationManager)) as IAnimationManager);

    /// <summary>Starts a loop on the clock of <paramref name="manager"/> (the window's animation manager).</summary>
    internal static bool Start(VisualElement owner, string name, Action<double> callback, double restPhase, uint duration, IAnimationManager? manager)
    {
        Stop(owner, name);
        if (CdsMotion.IsReducedMotion || manager is null || !manager.Ticker.SystemEnabled)
        {
            callback(restPhase);
            return false;
        }

        // The manager drops an animation it force-finishes (system animations turned off); start a fresh clock then.
        if (!Clocks.TryGetValue(manager, out var clock) || clock.HasFinished)
        {
            clock = new Clock(manager);
            Clocks.AddOrUpdate(manager, clock);
        }

        var loop = new Loop(owner, callback, Math.Max(duration, 1u), clock);
        Running.GetOrCreateValue(owner)[name] = loop;
        clock.Add(loop);
        if (IsShown(owner)) callback(0);
        return true;
    }

    /// <summary>Whether the element and all its ancestors are visible.</summary>
    internal static bool IsShown(Element element)
    {
        for (Element? e = element; e is not null; e = e.Parent)
            if (e is VisualElement { IsVisible: false }) return false;
        return true;
    }

    /// <summary>Stops a loop started with <see cref="Start(VisualElement, string, Action{double}, double, uint)"/>.</summary>
    public static void Stop(VisualElement owner, string name)
    {
        if (!Running.TryGetValue(owner, out var loops) || !loops.Remove(name, out var loop)) return;
        loop.Clock.Remove(loop);
    }

    /// <summary>Whether <paramref name="owner"/> has a running loop named <paramref name="name"/>.</summary>
    internal static bool IsRunning(VisualElement owner, string name) =>
        Running.TryGetValue(owner, out var loops) && loops.ContainsKey(name);

    /// <summary>One loop: its own phase over its own duration, advanced by the shared clock.</summary>
    sealed class Loop(VisualElement owner, Action<double> callback, double duration, Clock clock)
    {
        double _elapsed;

        public Clock Clock => clock;

        public bool IsStopped { get; set; }

        public void Advance(double milliseconds)
        {
            _elapsed = (_elapsed + milliseconds) % duration;
            if (IsShown(owner)) callback(_elapsed / duration);
        }
    }

    /// <summary>
    /// The frame clock of one animation manager: a single never-finishing animation that advances every loop on it. It
    /// is in the manager only while it has loops, so an idle window's ticker stops.
    /// </summary>
    sealed class Clock(IAnimationManager manager) : Microsoft.Maui.Animations.Animation
    {
        readonly List<Loop> _loops = [];
        Loop[] _snapshot = [];
        bool _changed;

        public void Add(Loop loop)
        {
            _loops.Add(loop);
            _changed = true;
            if (_loops.Count == 1) Commit(manager);
        }

        public void Remove(Loop loop)
        {
            loop.IsStopped = true;
            if (!_loops.Remove(loop)) return;
            _changed = true;
            if (_loops.Count == 0) manager.Remove(this);
        }

        protected override void OnTick(double millisecondsSinceLastUpdate)
        {
            // A callback may start or stop loops; iterate a snapshot and skip the ones stopped meanwhile.
            if (_changed)
            {
                _snapshot = [.. _loops];
                _changed = false;
            }
            foreach (var loop in _snapshot)
                if (!loop.IsStopped) loop.Advance(millisecondsSinceLastUpdate);
        }
    }
}

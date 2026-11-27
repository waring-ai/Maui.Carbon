using System.Runtime.CompilerServices;
using IAnimationManager = Microsoft.Maui.Animations.IAnimationManager;

namespace Maui.Carbon;

/// <summary>
/// Runs an endless animation (spinner rotation, indeterminate sweep, skeleton shimmer) that respects Carbon's reduced
/// motion: when reduced, the callback is invoked once with the rest value and nothing animates.
/// <para>
/// Every loop of a window runs on one shared frame clock (one animation in MAUI's animation manager), not one MAUI
/// animation each: a screen of skeletons and spinners costs one tick per frame plus a callback per shown loop. A loop
/// whose owner or an ancestor is hidden is parked: it leaves the clock and listens to the visibility of its owner's
/// ancestors, and rejoins (where it left off) when they are all shown again. A screen kept attached but hidden, or a
/// loading skeleton hidden once its data arrived, costs nothing per frame, and with every loop parked the clock leaves the
/// manager so the window's ticker stops. A loop runs until <see cref="Stop"/> (owners stop on <c>Unloaded</c>).
/// </para>
/// </summary>
internal static class CdsLoop
{
    static readonly ConditionalWeakTable<VisualElement, Dictionary<string, Loop>> Running = new();
    static readonly ConditionalWeakTable<IAnimationManager, Clock> Clocks = new();

    static CdsLoop() => CdsMeter.ObserveActiveLoops(CountRunning);

    // For CdsMetrics.ActiveLoops (loops on a clock, not parked): read from the listener's thread, so a count taken
    // mid-change may be off by one.
    static int CountRunning()
    {
        var count = 0;
        foreach (var (_, clock) in Clocks)
            if (!clock.HasFinished) count += clock.Count;
        return count;
    }

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

        var loop = new Loop(owner, callback, Math.Max(duration, 1u), manager);
        Running.GetOrCreateValue(owner)[name] = loop;
        if (IsShown(owner))
        {
            loop.Resume();
            callback(0);
        }
        else
        {
            loop.Park();
        }
        return true;
    }

    // The manager drops an animation it force-finishes (system animations turned off); start a fresh clock then.
    static Clock ClockFor(IAnimationManager manager)
    {
        if (!Clocks.TryGetValue(manager, out var clock) || clock.HasFinished)
        {
            clock = new Clock(manager);
            Clocks.AddOrUpdate(manager, clock);
        }
        return clock;
    }

    /// <summary>Whether the element and all its ancestors are visible (an inactive <see cref="CdsDeck"/> child is hidden).</summary>
    internal static bool IsShown(Element element) => CdsDeck.IsShown(element);

    /// <summary>Stops a loop started with <see cref="Start(VisualElement, string, Action{double}, double, uint)"/>.</summary>
    public static void Stop(VisualElement owner, string name)
    {
        if (!Running.TryGetValue(owner, out var loops) || !loops.Remove(name, out var loop)) return;
        loop.Stop();
    }

    /// <summary>Whether <paramref name="owner"/> has a loop named <paramref name="name"/> (on the clock or parked).</summary>
    internal static bool IsRunning(VisualElement owner, string name) =>
        Running.TryGetValue(owner, out var loops) && loops.ContainsKey(name);

    /// <summary>Whether <paramref name="owner"/>'s loop named <paramref name="name"/> is parked (hidden, off the clock).</summary>
    internal static bool IsParked(VisualElement owner, string name) =>
        Running.TryGetValue(owner, out var loops) && loops.TryGetValue(name, out var loop) && loop.IsParked;

    /// <summary>One loop: its own phase over its own duration, advanced by the shared clock while its owner is shown.</summary>
    sealed class Loop(VisualElement owner, Action<double> callback, double duration, IAnimationManager manager)
    {
        // The owner and its ancestors while parked: a change to any one's visibility or parent may show the owner again.
        readonly List<Element> _watched = [];
        double _elapsed;
        Clock? _clock;

        public bool IsStopped { get; private set; }

        public bool IsParked => _clock is null && !IsStopped;

        /// <summary>Advances the phase; false when the owner turned out to be hidden (the clock then parks the loop).</summary>
        public bool Advance(double milliseconds)
        {
            if (!IsShown(owner)) return false;
            _elapsed = (_elapsed + milliseconds) % duration;
            callback(_elapsed / duration);
            return true;
        }

        public void Resume()
        {
            Unwatch();
            _clock = ClockFor(manager);
            _clock.Add(this);
        }

        /// <summary>Leaves the clock and waits for the owner to be shown again.</summary>
        public void Park()
        {
            _clock?.Remove(this);
            _clock = null;
            Watch();
        }

        public void Stop()
        {
            IsStopped = true;
            _clock?.Remove(this);
            _clock = null;
            Unwatch();
        }

        void Watch()
        {
            Unwatch();
            for (Element? e = owner; e is not null; e = e.Parent)
            {
                e.PropertyChanged += OnWatchedChanged;
                _watched.Add(e);
            }
        }

        void Unwatch()
        {
            foreach (var e in _watched) e.PropertyChanged -= OnWatchedChanged;
            _watched.Clear();
        }

        void OnWatchedChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == VisualElement.IsVisibleProperty.PropertyName || e.PropertyName == CdsDeck.ActiveProperty.PropertyName)
            {
                if (IsShown(owner)) Resume();
            }
            else if (e.PropertyName == nameof(Element.Parent))
            {
                // Moved: the ancestors changed, and the new ones may all be visible.
                if (IsShown(owner)) Resume();
                else Watch();
            }
        }
    }

    /// <summary>
    /// The frame clock of one animation manager: a single never-finishing animation that advances every shown loop on it.
    /// It is in the manager only while it has loops, so an idle window's ticker stops.
    /// </summary>
    sealed class Clock(IAnimationManager manager) : Microsoft.Maui.Animations.Animation
    {
        readonly List<Loop> _loops = [];
        Loop[] _snapshot = [];
        bool _changed;

        public int Count => _loops.Count;

        public void Add(Loop loop)
        {
            if (_loops.Contains(loop)) return;
            _loops.Add(loop);
            _changed = true;
            if (_loops.Count == 1) Commit(manager);
        }

        public void Remove(Loop loop)
        {
            if (!_loops.Remove(loop)) return;
            _changed = true;
            if (_loops.Count == 0) manager.Remove(this);
        }

        protected override void OnTick(double millisecondsSinceLastUpdate)
        {
            var start = CdsMeter.Start(CdsMeter.LoopTick);
            // A callback may start, stop or park loops; iterate a snapshot and skip the ones stopped or parked meanwhile.
            if (_changed)
            {
                _snapshot = [.. _loops];
                _changed = false;
            }
            foreach (var loop in _snapshot)
                if (!loop.IsStopped && !loop.IsParked && !loop.Advance(millisecondsSinceLastUpdate)) loop.Park();
            CdsMeter.Stop(CdsMeter.LoopTick, start);
        }
    }
}

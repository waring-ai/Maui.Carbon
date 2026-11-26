using System.Diagnostics.Metrics;
using Microsoft.Maui.Animations;

namespace Maui.Carbon.Tests.Foundation;

// Runs with the loop clock tests: the loop test needs motion on, and one of those forces reduced motion.
[Collection(FocusTrackerCollection.Name)]
public class MetricsTests
{
    /// <summary>Listens to the Carbon meter and keeps what it records.</summary>
    sealed class Recorder : IDisposable
    {
        readonly MeterListener _listener = new();
        readonly List<(string Instrument, double Value, string? Component)> _measurements = [];

        public Recorder()
        {
            _listener.InstrumentPublished = (instrument, listener) =>
            {
                if (instrument.Meter.Name == CdsMetrics.MeterName) listener.EnableMeasurementEvents(instrument);
            };
            _listener.SetMeasurementEventCallback<double>((i, v, tags, _) => Add(i, v, tags));
            _listener.SetMeasurementEventCallback<int>((i, v, tags, _) => Add(i, v, tags));
            _listener.Start();
        }

        void Add(Instrument instrument, double value, ReadOnlySpan<KeyValuePair<string, object?>> tags)
        {
            string? component = null;
            foreach (var tag in tags)
                if (tag.Key is CdsMetrics.ComponentTag or CdsMetrics.ThemeTag) component = tag.Value as string;
            lock (_measurements) _measurements.Add((instrument.Name, value, component));
        }

        public void Observe() => _listener.RecordObservableInstruments();

        public List<double> Of(string instrument, string? component = null)
        {
            lock (_measurements)
                return _measurements.Where(m => m.Instrument == instrument && (component is null || m.Component == component)).Select(m => m.Value).ToList();
        }

        public void Dispose() => _listener.Dispose();
    }

    sealed class ProbeDrawable : IDrawable
    {
        public int Draws { get; private set; }
        public void Draw(ICanvas canvas, RectF dirtyRect) => Draws++;
    }

    sealed class MetricsProbe : CdsControl
    {
        public MetricsProbe()
        {
            Hit = CreateHitTarget();
            SetChrome(new Grid { Children = { Hit } });
        }

        public CdsHitTarget Hit { get; }
        public int Activations { get; private set; }
        public int Applied { get; private set; }

        public void Refresh() => RefreshVisuals();

        protected override void ApplyVisuals() => Applied++;

        protected override void OnActivated() => Activations++;
    }

    [Fact]
    public void Draws_are_measured_under_the_owner_or_the_given_component()
    {
        using var recorder = new Recorder();
        var owner = new ProbeDrawable();
        new CdsWeakDrawable(owner).Draw(null!, RectF.Zero);
        new CdsWeakDrawable(owner, "MetricsProbeChart").Draw(null!, RectF.Zero);
        new CdsWeakDrawable(owner, "MetricsProbeChart").Draw(null!, RectF.Zero);

        Assert.Equal(3, owner.Draws);
        Assert.Single(recorder.Of(CdsMetrics.DrawDuration, nameof(ProbeDrawable)));
        Assert.Equal(2, recorder.Of(CdsMetrics.DrawDuration, "MetricsProbeChart").Count);
        Assert.All(recorder.Of(CdsMetrics.DrawDuration, "MetricsProbeChart"), s => Assert.True(s >= 0));
    }

    [Fact]
    public void Visuals_layout_and_activation_are_measured_per_control_type()
    {
        using var recorder = new Recorder();
        var probe = new MetricsProbe();
        _ = new ContentView { Content = probe };
        probe.Refresh();
        ((IView)probe).Measure(200, 100);
        ((IView)probe).Arrange(new Rect(0, 0, 200, 100));
        probe.Hit.RaiseKeyboardActivation();

        const string name = nameof(MetricsProbe);
        Assert.Equal(probe.Applied, recorder.Of(CdsMetrics.VisualsDuration, name).Count);
        Assert.True(probe.Applied >= 2);
        Assert.NotEmpty(recorder.Of(CdsMetrics.MeasureDuration, name));
        Assert.NotEmpty(recorder.Of(CdsMetrics.ArrangeDuration, name));
        Assert.Equal(1, probe.Activations);
        Assert.Single(recorder.Of(CdsMetrics.ActivationDuration, name));
    }

    [Fact]
    public void Coalesced_rebuilds_are_measured_under_their_owner()
    {
        using var recorder = new Recorder();
        var runs = 0;
        var rebuild = new CdsCoalescedAction(new MetricsProbe(), () => runs++);
        rebuild.Request();
        rebuild.Request();

        Assert.Equal(2, runs);
        Assert.Equal(2, recorder.Of(CdsMetrics.RebuildDuration, nameof(MetricsProbe)).Count);
    }

    [Fact]
    public void Loop_frames_and_running_loops_are_reported()
    {
        using var recorder = new Recorder();
        var manager = new FakeAnimationManager();
        var owner = new BoxView();
        Assert.True(CdsLoop.Start(owner, "metrics", _ => { }, 0, 400, manager));
        manager.Frame(16);
        manager.Frame(16);
        recorder.Observe();

        Assert.Equal(2, recorder.Of(CdsMetrics.LoopTickDuration).Count);
        Assert.True(recorder.Of(CdsMetrics.ActiveLoops).Single() >= 1);
        CdsLoop.Stop(owner, "metrics");
    }

    [Fact]
    public void Nothing_is_timed_without_a_listener()
    {
        Assert.False(CdsMeter.Draw.Enabled);
        Assert.Equal(0, CdsMeter.Start(CdsMeter.Draw));
        using (new Recorder())
        {
            Assert.True(CdsMeter.Draw.Enabled);
            Assert.NotEqual(0, CdsMeter.Start(CdsMeter.Draw));
        }
        Assert.False(CdsMeter.Draw.Enabled);
    }

    sealed class FakeTicker : ITicker
    {
        public bool IsRunning { get; set; }
        public bool SystemEnabled { get; set; } = true;
        public int MaxFps { get; set; } = 60;
        public Action? Fire { get; set; }
        public void Start() => IsRunning = true;
        public void Stop() => IsRunning = false;
    }

    sealed class FakeAnimationManager : IAnimationManager
    {
        readonly List<Microsoft.Maui.Animations.Animation> _animations = [];
        public ITicker Ticker { get; } = new FakeTicker();
        public double SpeedModifier { get; set; } = 1;
        public bool AutoStartTicker { get; set; } = true;
        public void Add(Microsoft.Maui.Animations.Animation animation)
        {
            if (!_animations.Contains(animation)) _animations.Add(animation);
        }
        public void Remove(Microsoft.Maui.Animations.Animation animation) => _animations.Remove(animation);
        public void Frame(double milliseconds)
        {
            foreach (var animation in _animations.ToArray()) animation.Tick(milliseconds);
        }
    }
}

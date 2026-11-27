namespace Maui.Carbon;

/// <summary>
/// Drives the steps, progress indicator and buttons of a create flow (<see cref="CdsCreateTearsheet"/>,
/// <see cref="CdsCreateFullPage"/>), as Carbon for IBM Products' <c>useCreateComponentStepChange</c>: Cancel (ghost),
/// Back (secondary, from the second included step) and Next, which submits on the last included step.
/// </summary>
internal sealed class CreateFlowController
{
    readonly VisualElement _owner;
    readonly CdsItemCollection<CdsCreateStep> _steps = [];
    readonly VerticalStackLayout _host = new() { Spacing = 0 };
    readonly CdsProgressIndicator _progress = new() { IsVertical = true, SpaceEqually = true };
    readonly CdsButton _back = new("Back", CdsButtonKind.Secondary);
    readonly CdsButton _cancel = new("Cancel", CdsButtonKind.Ghost);
    readonly CdsButton _primary = new("Next", CdsButtonKind.Primary);
    readonly IList<CdsButton> _actions;
    bool _busy;

    public CreateFlowController(VisualElement owner, IList<CdsButton> actions)
    {
        _owner = owner;
        _actions = actions;
        _steps.CollectionChanged += (_, e) =>
        {
            if (e.OldItems is not null)
                foreach (CdsCreateStep s in e.OldItems)
                {
                    s.FlowChanged -= OnStepChanged;
                    _host.Children.Remove(s);
                }
            if (e.NewItems is not null)
                foreach (CdsCreateStep s in e.NewItems) s.FlowChanged += OnStepChanged;
            _host.Children.Clear();
            foreach (var s in _steps) _host.Children.Add(s);
            if (Current == 0 || Current > _steps.Count) Current = CreateFlowModel.FirstIncluded(Infos);
            Refresh();
        };
        _actions.Add(_cancel);
        _actions.Add(_primary);
        _cancel.Clicked += (_, _) => CancelRequested?.Invoke();
        _primary.Clicked += async (_, _) => await ForwardAsync();
        _back.Clicked += async (_, _) => await BackAsync();
    }

    public IList<CdsCreateStep> Steps => _steps;

    /// <summary>The view that holds the steps (only the current one shows).</summary>
    public View StepsHost => _host;

    public CdsProgressIndicator Progress => _progress;

    /// <summary>The current step, 1-based (0 when there are no steps).</summary>
    public int Current { get; private set; }

    public bool IsBusy => _busy;

    public string BackText { get => _back.Text ?? ""; set => _back.Text = value; }

    public string CancelText { get => _cancel.Text ?? ""; set => _cancel.Text = value; }

    public string NextText { get; set; } = "Next";

    public string SubmitText { get; set; } = "Create";

    /// <summary>Runs on submit from the last step; return false to stay open.</summary>
    public Func<Task<bool>>? Submit { get; set; }

    public Action? CancelRequested { get; set; }

    public Action? Submitted { get; set; }

    public Action? StepChanged { get; set; }

    internal CdsButton PrimaryButton => _primary;

    internal CdsButton BackButton => _back;

    IReadOnlyList<CreateStepInfo> Infos => [.. _steps.Select(s => s.Info)];

    CdsCreateStep? CurrentStep => Current >= 1 && Current <= _steps.Count ? _steps[Current - 1] : null;

    void OnStepChanged(object? sender, EventArgs e) => Refresh();

    /// <summary>Goes to the opening step (<paramref name="initialStep"/>-th included step, or the first).</summary>
    public void Reset(int initialStep)
    {
        _busy = false;
        Current = CreateFlowModel.Initial(Infos, initialStep);
        Refresh(entered: true);
    }

    public void Refresh(bool entered = false)
    {
        var infos = Infos;
        for (var i = 0; i < _steps.Count; i++) _steps[i].IsVisible = i + 1 == Current;

        // Progress: the included, non-intro steps.
        var listed = CreateFlowModel.ProgressSteps(infos);
        _progress.Steps.Clear();
        foreach (var n in listed)
        {
            var s = _steps[n - 1];
            _progress.Steps.Add(new CdsProgressStep { Label = s.Title, SecondaryLabel = s.SecondaryLabel, IsInvalid = s.IsInvalid });
        }
        _progress.CurrentIndex = CreateFlowModel.ProgressIndex(infos, Current);
        _progress.IsVisible = CreateFlowModel.ShowsProgress(infos, Current) && listed.Count > 0;

        // Buttons: Back appears once there is more than one step.
        var hasBack = _steps.Count > 1;
        if (hasBack && !_actions.Contains(_back)) _actions.Insert(1, _back);
        else if (!hasBack && _actions.Contains(_back)) _actions.Remove(_back);
        var step = CurrentStep;
        _back.IsEnabled = !_busy && Current != CreateFlowModel.FirstIncluded(infos) && step?.IsBackDisabled != true;
        _primary.Text = CreateFlowModel.IsLast(infos, Current) ? SubmitText : NextText;
        _primary.IsEnabled = !_busy && step?.IsSubmitDisabled != true;

        if (!entered || step is null) return;
        Animate(step);
        step.RaiseShown();
        StepChanged?.Invoke();
    }

    static void Animate(View step)
    {
        // Carbon: the step fades in and drops 12 over slow-01 with standard easing.
        if (CdsMotion.IsReducedMotion || step.Handler is null) return;
        step.Opacity = 0;
        step.TranslationY = -CdsSpacing.Spacing04;
        _ = step.FadeToAsync(1, CdsDuration.Slow01, CdsEasing.StandardProductive);
        _ = step.TranslateToAsync(0, 0, CdsDuration.Slow01, CdsEasing.StandardProductive);
    }

    static async Task<bool> RunAsync(Func<Task<bool>>? action, string what)
    {
        if (action is null) return true;
        try
        {
            return await action();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Create flow {what} error: {ex.Message}");
            return false;
        }
    }

    internal async Task ForwardAsync()
    {
        var step = CurrentStep;
        if (_busy || step is null) return;
        _busy = true;
        Refresh();
        var moved = false;
        try
        {
            if (!await RunAsync(step.NextAsync, "next")) return;
            if (CreateFlowModel.IsLast(Infos, Current))
            {
                if (await RunAsync(Submit, "submit")) Submitted?.Invoke();
                return;
            }
            Current = CreateFlowModel.Next(Infos, Current);
            moved = true;
        }
        finally
        {
            _busy = false;
            Refresh(entered: moved);
        }
    }

    internal async Task BackAsync()
    {
        var step = CurrentStep;
        if (_busy || step is null) return;
        _busy = true;
        Refresh();
        var moved = false;
        try
        {
            if (!await RunAsync(step.PreviousAsync, "previous")) return;
            var previous = CreateFlowModel.Previous(Infos, Current);
            moved = previous != Current;
            Current = previous;
        }
        finally
        {
            _busy = false;
            Refresh(entered: moved);
        }
    }

    /// <summary>Jumps to a step (from a clickable progress indicator in a full-page flow).</summary>
    internal void GoTo(int step)
    {
        if (_busy || step < 1 || step > _steps.Count || !_steps[step - 1].IsIncluded || step == Current) return;
        Current = step;
        Refresh(entered: true);
    }
}

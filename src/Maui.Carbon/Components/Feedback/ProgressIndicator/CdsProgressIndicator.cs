using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Maui.Carbon;

/// <summary>Event data of <see cref="CdsProgressIndicator.StepRequested"/>. Set <see cref="Cancel"/> to stay on the current step.</summary>
public sealed class CdsProgressStepRequestedEventArgs : EventArgs
{
    internal CdsProgressStepRequestedEventArgs(int index, CdsProgressStep step)
    {
        Index = index;
        Step = step;
    }

    /// <summary>The zero-based index of the clicked step.</summary>
    public int Index { get; }

    /// <summary>The clicked step.</summary>
    public CdsProgressStep Step { get; }

    /// <summary>True to refuse the move (for example to only allow going back to earlier steps).</summary>
    public bool Cancel { get; set; }
}

/// <summary>
/// Shows the steps of a multi-step flow and where the person is in it, horizontally or vertically. Steps before
/// <see cref="CurrentIndex"/> are complete, that step is current, later ones are incomplete. Use it for linear flows of
/// three or more steps done in order, beside Back and Next buttons. Prefer vertical when there is room; horizontal
/// suits modals and narrow headers. Set <see cref="IsClickable"/> to make steps tabbable and clickable (the current
/// and disabled steps never are). For how much of a task has run use <see cref="CdsProgressBar"/>.
/// </summary>
[ContentProperty(nameof(Steps))]
public class CdsProgressIndicator : CdsControl
{
    /// <summary>Bindable property for <see cref="CurrentIndex"/>.</summary>
    public static readonly BindableProperty CurrentIndexProperty = BindableProperty.Create(
        nameof(CurrentIndex), typeof(int), typeof(CdsProgressIndicator), 0, BindingMode.TwoWay,
        propertyChanged: static (b, o, n) => ((CdsProgressIndicator)b).OnCurrentIndexChanged((int)o, (int)n));

    /// <summary>Bindable property for <see cref="IsVertical"/>.</summary>
    public static readonly BindableProperty IsVerticalProperty = BindableProperty.Create(
        nameof(IsVertical), typeof(bool), typeof(CdsProgressIndicator), false, propertyChanged: OnLayoutChanged);

    /// <summary>Bindable property for <see cref="SpaceEqually"/>.</summary>
    public static readonly BindableProperty SpaceEquallyProperty = BindableProperty.Create(
        nameof(SpaceEqually), typeof(bool), typeof(CdsProgressIndicator), false, propertyChanged: OnLayoutChanged);

    /// <summary>Bindable property for <see cref="IsClickable"/>.</summary>
    public static readonly BindableProperty IsClickableProperty = BindableProperty.Create(
        nameof(IsClickable), typeof(bool), typeof(CdsProgressIndicator), false, propertyChanged: OnLayoutChanged);

    readonly CdsItemCollection<CdsProgressStep> _steps = [];
    readonly List<CdsProgressStepView> _views = [];
    readonly ContentView _host = new();

    /// <summary>Creates an empty horizontal indicator.</summary>
    public CdsProgressIndicator()
    {
        _steps.CollectionChanged += OnStepsChanged;
        HorizontalOptions = LayoutOptions.Start;
        SetChrome(_host);
        Rebuild();
    }

    /// <summary>Raised after <see cref="CurrentIndex"/> changed (by a click or by code).</summary>
    public event EventHandler<ValueChangedEventArgs>? CurrentIndexChanged;

    /// <summary>Raised when a step is clicked, before <see cref="CurrentIndex"/> moves; set <c>Cancel</c> to refuse.</summary>
    public event EventHandler<CdsProgressStepRequestedEventArgs>? StepRequested;

    /// <summary>The steps, in order.</summary>
    public IList<CdsProgressStep> Steps => _steps;

    /// <summary>The zero-based current step. Two-way. Earlier steps become complete, later ones incomplete.</summary>
    public int CurrentIndex
    {
        get => (int)GetValue(CurrentIndexProperty);
        set => SetValue(CurrentIndexProperty, value);
    }

    /// <summary>Lay the steps out in a column (default is a row).</summary>
    public bool IsVertical
    {
        get => (bool)GetValue(IsVerticalProperty);
        set => SetValue(IsVerticalProperty, value);
    }

    /// <summary>Horizontal only: the steps share the width equally (each at least 128).</summary>
    public bool SpaceEqually
    {
        get => (bool)GetValue(SpaceEquallyProperty);
        set => SetValue(SpaceEquallyProperty, value);
    }

    /// <summary>Make steps clickable and tabbable (except the current and disabled ones), like Carbon's <c>onChange</c>.</summary>
    public bool IsClickable
    {
        get => (bool)GetValue(IsClickableProperty);
        set => SetValue(IsClickableProperty, value);
    }

    void OnStepsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null) foreach (CdsProgressStep step in e.OldItems) step.Changed -= OnStepChanged;
        if (e.NewItems is not null) foreach (CdsProgressStep step in e.NewItems) step.Changed += OnStepChanged;
        Rebuild();
    }

    void OnStepChanged(object? sender, EventArgs e) => UpdateSteps();

    void OnCurrentIndexChanged(int oldIndex, int newIndex)
    {
        UpdateSteps();
        CurrentIndexChanged?.Invoke(this, new ValueChangedEventArgs(oldIndex, newIndex));
    }

    static void OnLayoutChanged(BindableObject b, object? o, object? n) => ((CdsProgressIndicator)b).Rebuild();

    void Rebuild()
    {
        _views.Clear();
        foreach (var step in _steps)
        {
            var view = new CdsProgressStepView();
            var captured = step;
            view.Clicked += (_, _) => OnStepClicked(captured);
            _views.Add(view);
        }

        if (IsVertical)
        {
            var stack = new VerticalStackLayout { Spacing = 0 };
            foreach (var v in _views) stack.Add(v);
            _host.Content = stack;
        }
        else
        {
            var grid = new Grid { ColumnSpacing = 0 };
            foreach (var v in _views)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition(SpaceEqually ? GridLength.Star : new GridLength(128)));
                v.MinimumWidthRequest = SpaceEqually ? 128 : 112;
                grid.Add(v, grid.ColumnDefinitions.Count - 1);
            }
            _host.Content = grid;
        }
        // Equal steps share the width offered; the app's alignment (and any WidthRequest) is left alone.
        SetChrome(_host, fillsWidth: !IsVertical && SpaceEqually);
        UpdateSteps();
    }

    void UpdateSteps()
    {
        var current = CurrentIndex; // beyond the last step every step is complete; below zero none is
        for (var i = 0; i < _views.Count; i++)
        {
            var step = _steps[i];
            var state = ProgressIndicatorModel.Resolve(i, current, step.IsInvalid, step.IsDisabled, step.IsComplete);
            var reached = ProgressIndicatorModel.LineReached(i, current, step.IsComplete);
            var clickable = ProgressIndicatorModel.IsClickable(IsClickable, i, current, step.IsDisabled);
            _views[i].IsEnabled = !step.IsDisabled;
            _views[i].Update(step, state, reached, IsVertical, SpaceEqually, clickable);
        }
    }

    void OnStepClicked(CdsProgressStep step)
    {
        var index = _steps.IndexOf(step);
        if (index < 0 || !ProgressIndicatorModel.IsClickable(IsClickable, index, CurrentIndex, step.IsDisabled)) return;
        var args = new CdsProgressStepRequestedEventArgs(index, step);
        StepRequested?.Invoke(this, args);
        if (!args.Cancel) CurrentIndex = index;
    }
}

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Microsoft.Maui.Controls.Shapes;

namespace Maui.Carbon;

/// <summary>
/// A segmented control that switches between two to five alternate views of the same content; one
/// <see cref="CdsSwitch"/> is always selected. Segments share the width equally (icon-only segments are squares of the control
/// height). Sizes are sm 32, md 40 (default) and lg 48. High contrast (default) fills the selected segment with
/// <c>layer-selected-inverse</c>; <see cref="IsLowContrast"/> draws it on the next layer for cards and dialogs. Place it above the
/// content it controls, aligned left or right, never centered. It is a single Tab stop; Left and Right move between
/// enabled segments (wrapping) and select in <see cref="CdsActivation.Automatic"/> mode, Enter or Space select in
/// <see cref="CdsActivation.Manual"/> mode. For distinct sections use <see cref="CdsTabs"/>, for on/off use a toggle.
/// </summary>
[ContentProperty(nameof(Items))]
public class CdsContentSwitcher : CdsControl
{
    /// <summary>Bindable property for <see cref="SelectedIndex"/>.</summary>
    public static readonly BindableProperty SelectedIndexProperty = BindableProperty.Create(
        nameof(SelectedIndex), typeof(int), typeof(CdsContentSwitcher), 0, BindingMode.TwoWay,
        coerceValue: static (_, v) => Math.Max(0, (int)v),
        propertyChanged: static (b, _, _) => ((CdsContentSwitcher)b).ApplySelection());

    /// <summary>Bindable property for <see cref="Size"/>.</summary>
    public static readonly BindableProperty SizeProperty = BindableProperty.Create(
        nameof(Size), typeof(CdsSize), typeof(CdsContentSwitcher), CdsSize.Md,
        propertyChanged: static (b, _, _) => ((CdsContentSwitcher)b).Rebuild());

    /// <summary>Bindable property for <see cref="IsLowContrast"/>.</summary>
    public static readonly BindableProperty IsLowContrastProperty = BindableProperty.Create(
        nameof(IsLowContrast), typeof(bool), typeof(CdsContentSwitcher), false,
        propertyChanged: static (b, _, _) => ((CdsContentSwitcher)b).Rebuild());

    /// <summary>Bindable property for <see cref="IsIconOnly"/>.</summary>
    public static readonly BindableProperty IsIconOnlyProperty = BindableProperty.Create(
        nameof(IsIconOnly), typeof(bool), typeof(CdsContentSwitcher), false,
        propertyChanged: static (b, _, _) => ((CdsContentSwitcher)b).Rebuild());

    /// <summary>Bindable property for <see cref="Activation"/>.</summary>
    public static readonly BindableProperty ActivationProperty = BindableProperty.Create(
        nameof(Activation), typeof(CdsActivation), typeof(CdsContentSwitcher), CdsActivation.Automatic);

    /// <summary>Bindable property for <see cref="Label"/>.</summary>
    public static readonly BindableProperty LabelProperty = BindableProperty.Create(
        nameof(Label), typeof(string), typeof(CdsContentSwitcher), null,
        propertyChanged: static (b, _, _) => ((CdsContentSwitcher)b).Rebuild());

    readonly ObservableCollection<CdsSwitch> _items = [];
    readonly List<CdsSwitchView> _views = [];
    readonly Border _frame = new() { StrokeThickness = 1, StrokeShape = new RoundRectangle { CornerRadius = 4 }, Padding = 0 };
    readonly CdsEqualWidthRow _grid = new();
    bool _rebuilding;
    int _applied = -2;
    int _focusIndex = -1;

    /// <summary>Creates an empty medium, high-contrast switcher.</summary>
    public CdsContentSwitcher()
    {
        _frame.Content = _grid;
        SetChrome(_frame);
        _items.CollectionChanged += OnItemsChanged;
        Rebuild();
    }

    /// <summary>Raised when the selected segment changes (by the user or by <see cref="SelectedIndex"/>).</summary>
    public event EventHandler<CdsSwitchSelectionChangedEventArgs>? SelectionChanged;

    /// <summary>The segments, in order.</summary>
    public IList<CdsSwitch> Items => _items;

    /// <summary>The index of the selected segment (two-way, clamped to the segments). The first is selected by default; the user cannot select a disabled segment, but the property can.</summary>
    public int SelectedIndex
    {
        get => (int)GetValue(SelectedIndexProperty);
        set => SetValue(SelectedIndexProperty, value);
    }

    /// <summary>Height: <see cref="CdsSize.Sm"/> 32, <see cref="CdsSize.Md"/> 40 (default) or <see cref="CdsSize.Lg"/> 48.</summary>
    public CdsSize Size
    {
        get => (CdsSize)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    /// <summary>No outline, a <c>layer-01</c> track and the selected segment on the next layer. Use it in cards and dialogs. Default <c>false</c>.</summary>
    public bool IsLowContrast
    {
        get => (bool)GetValue(IsLowContrastProperty);
        set => SetValue(IsLowContrastProperty, value);
    }

    /// <summary>Every segment shows its <see cref="CdsSwitch.Icon"/> instead of a label; <see cref="CdsSwitch.Text"/> is the name and tooltip.</summary>
    public bool IsIconOnly
    {
        get => (bool)GetValue(IsIconOnlyProperty);
        set => SetValue(IsIconOnlyProperty, value);
    }

    /// <summary>Arrow keys select (<see cref="CdsActivation.Automatic"/>, default) or only move focus (<see cref="CdsActivation.Manual"/>).</summary>
    public CdsActivation Activation
    {
        get => (CdsActivation)GetValue(ActivationProperty);
        set => SetValue(ActivationProperty, value);
    }

    /// <summary>Names the group for screen readers (not drawn).</summary>
    public string? Label
    {
        get => (string?)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <inheritdoc/>
    protected internal override bool WantsNavigationKeys => true;

    /// <inheritdoc/>
    protected internal override bool OnKeyDown(CdsKeyEventArgs e)
    {
        if (_views.Count == 0 || e.Key is not (CdsKey.Left or CdsKey.Right)) return false;
        var current = _focusIndex >= 0 ? _focusIndex : Math.Max(0, _applied);
        var target = TabsModel.Target(e.Key, Enabled(), current);
        if (target < 0) return false;
        e.Handled = true;
        FocusSegment(target);
        if (Activation == CdsActivation.Automatic) OnSegmentActivated(target);
        return true;
    }

    internal void OnSegmentActivated(int index)
    {
        if (index < 0 || index >= _items.Count || !_items[index].IsEnabled) return;
        if (SelectedIndex != index) SelectedIndex = index;
        else ApplySelection();
    }

    IReadOnlyList<bool> Enabled() => _items.Select(i => i.IsEnabled).ToArray();

    void FocusSegment(int index)
    {
        if (index < 0 || index >= _views.Count) return;
        _focusIndex = index;
        _views[index].Focus();
    }

    void ApplySelection()
    {
        if (_rebuilding) return;
        var coerced = _items.Count == 0 ? -1 : Math.Clamp(SelectedIndex, 0, _items.Count - 1);
        if (coerced >= 0 && coerced != SelectedIndex)
        {
            SelectedIndex = coerced;
            return;
        }
        var previous = _applied;
        _applied = coerced;
        for (var i = 0; i < _views.Count; i++) _views[i].Update(_items[i], i == _applied);
        UpdateDividers();
        if (previous != -2 && previous != _applied && _applied >= 0)
        {
            var item = _items[_applied];
            SelectionChanged?.Invoke(this, new CdsSwitchSelectionChangedEventArgs(_applied, item.Name, item.Text));
        }
    }

    void OnItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
            foreach (CdsSwitch item in e.OldItems)
            {
                item.PropertyChanged -= OnItemPropertyChanged;
                item.Parent = null;
            }
        if (e.NewItems is not null)
            foreach (CdsSwitch item in e.NewItems)
            {
                item.Parent = this;
                item.PropertyChanged += OnItemPropertyChanged;
            }
        Rebuild();
    }

    void OnItemPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e) => Rebuild();

    void Rebuild()
    {
        _rebuilding = true;
        try
        {
            _grid.Clear();
            _views.Clear();
            var height = CdsContentSwitcherTokens.Height(Size);
            for (var i = 0; i < _items.Count; i++)
            {
                var view = new CdsSwitchView(this, i);
                view.StateChanged += OnSegmentStateChanged;
                _views.Add(view);
                _grid.Add(view);
            }
            _grid.FixedCellWidth = IsIconOnly ? height : -1;
            HorizontalOptions = IsIconOnly ? LayoutOptions.Start : LayoutOptions.Fill;
            _frame.HeightRequest = height;
            SemanticProperties.SetDescription(_frame, Label);
            var coerced = _items.Count == 0 ? -1 : Math.Clamp(SelectedIndex, 0, _items.Count - 1);
            if (coerced >= 0 && coerced != SelectedIndex) SelectedIndex = coerced;
            _applied = coerced;
        }
        finally
        {
            _rebuilding = false;
        }
        for (var i = 0; i < _views.Count; i++) _views[i].Update(_items[i], i == _applied);
        UpdateDividers();
        RefreshVisuals();
    }

    void OnSegmentStateChanged(object? sender, EventArgs e)
    {
        if (sender is CdsSwitchView view)
        {
            if (view.Is(CdsInteractionState.Focused)) _focusIndex = view.Index;
            else if (_focusIndex == view.Index) _focusIndex = -1;
        }
        UpdateDividers();
    }

    void UpdateDividers()
    {
        for (var i = 0; i < _views.Count; i++)
        {
            var self = _views[i];
            var previous = i > 0 ? _views[i - 1] : null;
            var hidden = self.HidesDividers || (previous?.HidesDividers ?? false);
            var previousSelectedDisabled = previous is { IsSwitchSelected: true } p && p.Is(CdsInteractionState.Disabled);
            self.SetDivider(CdsContentSwitcherTokens.Divider(i == 0, hidden, self.Is(CdsInteractionState.Disabled), previousSelectedDisabled));
        }
        RefreshVisuals();
    }

    /// <inheritdoc/>
    protected override void ApplyVisuals()
    {
        var outline = CdsContentSwitcherTokens.Outline(IsLowContrast, _items.Any(i => !i.IsEnabled));
        SetBrush(_frame, Border.StrokeProperty, outline);
        SetColor(_frame, BackgroundColorProperty, IsLowContrast ? CdsColorKeys.Layer01 : null);
    }
}

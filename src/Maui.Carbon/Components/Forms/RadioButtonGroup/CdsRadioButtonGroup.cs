using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Maui.Carbon;

/// <summary>
/// A set of <see cref="CdsRadioButton"/> items for choosing exactly one of a few visible options. The group owns the
/// selection (<see cref="SelectedValue"/> / <see cref="SelectedItem"/>), the legend and the helper, invalid and
/// warning messages. Arrow keys move the focus and select within the group (wrapping, skipping disabled radios). MAUI cannot take a
/// native button out of the Tab order, so every enabled radio stays a tab stop (Carbon's web radios are one tab stop). Prefer <see cref="CdsChoiceOrientation.Vertical"/> for scanning.
/// </summary>
[ContentProperty(nameof(Items))]
public class CdsRadioButtonGroup : CdsControl
{
    /// <summary>Bindable property for <see cref="Legend"/>.</summary>
    public static readonly BindableProperty LegendProperty = BindableProperty.Create(
        nameof(Legend), typeof(string), typeof(CdsRadioButtonGroup), null,
        propertyChanged: static (b, _, _) => ((CdsRadioButtonGroup)b).UpdateChrome());

    /// <summary>Bindable property for <see cref="SelectedValue"/>.</summary>
    public static readonly BindableProperty SelectedValueProperty = BindableProperty.Create(
        nameof(SelectedValue), typeof(object), typeof(CdsRadioButtonGroup), null, BindingMode.TwoWay,
        propertyChanged: static (b, o, n) => ((CdsRadioButtonGroup)b).OnSelectedValueChanged(o, n));

    /// <summary>Bindable property for <see cref="SelectedItem"/>.</summary>
    public static readonly BindableProperty SelectedItemProperty = BindableProperty.Create(
        nameof(SelectedItem), typeof(CdsRadioButton), typeof(CdsRadioButtonGroup), null, BindingMode.TwoWay,
        propertyChanged: static (b, _, n) => ((CdsRadioButtonGroup)b).OnSelectedItemSet((CdsRadioButton?)n));

    /// <summary>Bindable property for <see cref="Orientation"/>.</summary>
    public static readonly BindableProperty OrientationProperty = BindableProperty.Create(
        nameof(Orientation), typeof(CdsChoiceOrientation), typeof(CdsRadioButtonGroup), CdsChoiceOrientation.Horizontal,
        propertyChanged: static (b, _, _) => ((CdsRadioButtonGroup)b).RebuildItems());

    /// <summary>Bindable property for <see cref="LabelPosition"/>.</summary>
    public static readonly BindableProperty LabelPositionProperty = BindableProperty.Create(
        nameof(LabelPosition), typeof(CdsRadioLabelPosition), typeof(CdsRadioButtonGroup), CdsRadioLabelPosition.Right,
        propertyChanged: static (b, _, _) => ((CdsRadioButtonGroup)b).RefreshVisuals());

    /// <summary>Bindable property for <see cref="HelperText"/>.</summary>
    public static readonly BindableProperty HelperTextProperty = BindableProperty.Create(
        nameof(HelperText), typeof(string), typeof(CdsRadioButtonGroup), null,
        propertyChanged: static (b, _, _) => ((CdsRadioButtonGroup)b).RefreshVisuals());

    /// <summary>Bindable property for <see cref="IsInvalid"/>.</summary>
    public static readonly BindableProperty IsInvalidProperty = BindableProperty.Create(
        nameof(IsInvalid), typeof(bool), typeof(CdsRadioButtonGroup), false,
        propertyChanged: static (b, _, _) => ((CdsRadioButtonGroup)b).RefreshVisuals());

    /// <summary>Bindable property for <see cref="InvalidText"/>.</summary>
    public static readonly BindableProperty InvalidTextProperty = BindableProperty.Create(
        nameof(InvalidText), typeof(string), typeof(CdsRadioButtonGroup), null,
        propertyChanged: static (b, _, _) => ((CdsRadioButtonGroup)b).RefreshVisuals());

    /// <summary>Bindable property for <see cref="IsWarning"/>.</summary>
    public static readonly BindableProperty IsWarningProperty = BindableProperty.Create(
        nameof(IsWarning), typeof(bool), typeof(CdsRadioButtonGroup), false,
        propertyChanged: static (b, _, _) => ((CdsRadioButtonGroup)b).RefreshVisuals());

    /// <summary>Bindable property for <see cref="WarnText"/>.</summary>
    public static readonly BindableProperty WarnTextProperty = BindableProperty.Create(
        nameof(WarnText), typeof(string), typeof(CdsRadioButtonGroup), null,
        propertyChanged: static (b, _, _) => ((CdsRadioButtonGroup)b).RefreshVisuals());

    /// <summary>Bindable property for <see cref="IsReadOnly"/>.</summary>
    public static readonly BindableProperty IsReadOnlyProperty = BindableProperty.Create(
        nameof(IsReadOnly), typeof(bool), typeof(CdsRadioButtonGroup), false,
        propertyChanged: static (b, _, _) => ((CdsRadioButtonGroup)b).RefreshVisuals());

    readonly Grid _root = new() { ColumnSpacing = 0, RowSpacing = 0 };
    readonly CdsText _legend = new() { TypeStyle = CdsTypeStyle.Label01, Tone = CdsTextTone.Secondary, Margin = new Thickness(0, 0, 0, CdsSpacing.Spacing03) };
    readonly ContentView _itemsHost = new();
    readonly CdsFormMessage _message = new() { Margin = new Thickness(0, 6, 0, 0) };
    readonly CdsItemCollection<CdsRadioButton> _items = [];
    bool _syncing;

    /// <summary>Creates an empty group.</summary>
    public CdsRadioButtonGroup()
    {
        _root.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        _root.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        _root.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        _root.Add(_legend, 0, 0);
        _root.Add(_itemsHost, 0, 1);
        _root.Add(_message, 0, 2);
        _items.CollectionChanged += OnItemsChanged;
        SetChrome(_root);
        UpdateChrome();
    }

    /// <summary>Raised when the selection changes (by the user or in code).</summary>
    public event EventHandler<CdsChoiceChangedEventArgs<object>>? SelectionChanged;

    /// <summary>The radio buttons of the group (the content property: nest <see cref="CdsRadioButton"/> elements in XAML).</summary>
    public IList<CdsRadioButton> Items => _items;

    /// <summary>The group's name, shown above the radios in <c>label-01</c>. Always provide one.</summary>
    public string? Legend
    {
        get => (string?)GetValue(LegendProperty);
        set => SetValue(LegendProperty, value);
    }

    /// <summary>The <see cref="CdsRadioButton.Value"/> of the selected radio, or <c>null</c> (two-way). Nothing is preselected unless you set it.</summary>
    public object? SelectedValue
    {
        get => GetValue(SelectedValueProperty);
        set => SetValue(SelectedValueProperty, value);
    }

    /// <summary>The selected radio button, or <c>null</c> (two-way). Setting it selects that radio.</summary>
    public CdsRadioButton? SelectedItem
    {
        get => (CdsRadioButton?)GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    /// <summary>Horizontal (default, 16px apart) or vertical (stacked; long labels wrap under themselves).</summary>
    public CdsChoiceOrientation Orientation
    {
        get => (CdsChoiceOrientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    /// <summary>Label right of the circle (default) or left of it, for every radio in the group.</summary>
    public CdsRadioLabelPosition LabelPosition
    {
        get => (CdsRadioLabelPosition)GetValue(LabelPositionProperty);
        set => SetValue(LabelPositionProperty, value);
    }

    /// <summary>Helper text under the group; hidden while the group is invalid, warning or disabled.</summary>
    public string? HelperText
    {
        get => (string?)GetValue(HelperTextProperty);
        set => SetValue(HelperTextProperty, value);
    }

    /// <summary>Error state: rings turn <c>support-error</c> and <see cref="InvalidText"/> shows. Ignored while disabled or read-only.</summary>
    public bool IsInvalid
    {
        get => (bool)GetValue(IsInvalidProperty);
        set => SetValue(IsInvalidProperty, value);
    }

    /// <summary>What is wrong and how to fix it.</summary>
    public string? InvalidText
    {
        get => (string?)GetValue(InvalidTextProperty);
        set => SetValue(InvalidTextProperty, value);
    }

    /// <summary>Warning state: <see cref="WarnText"/> shows with a warning triangle. Ignored when invalid, disabled or read-only.</summary>
    public bool IsWarning
    {
        get => (bool)GetValue(IsWarningProperty);
        set => SetValue(IsWarningProperty, value);
    }

    /// <summary>The warning message.</summary>
    public string? WarnText
    {
        get => (string?)GetValue(WarnTextProperty);
        set => SetValue(WarnTextProperty, value);
    }

    /// <summary>Read-only: the selection cannot change; rings use <c>icon-disabled</c>. Radios stay focusable.</summary>
    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    /// <inheritdoc/>
    protected internal override bool WantsNavigationKeys => true;

    /// <inheritdoc/>
    protected internal override bool OnKeyDown(CdsKeyEventArgs e)
    {
        var direction = RadioGroupModel.Direction(e.Key);
        if (direction == 0 || _items.Count == 0) return false;

        var enabled = _items.Select(r => !r.IsEffectivelyDisabled).ToList();
        var current = _items.ToList().FindIndex(r => r.Is(CdsInteractionState.Focused));
        if (current < 0) current = SelectedIndex();
        var next = RadioGroupModel.NextEnabled(enabled, current, direction);
        if (next < 0) return false;

        var target = _items[next];
        target.FocusRadio();
        if (!IsReadOnly && !target.IsReadOnly) SelectedValue = target.Value;
        e.Handled = true;
        return true;
    }

    /// <inheritdoc/>
    protected override void ApplyVisuals()
    {
        var state = GroupState();
        SetColor(_legend, Microsoft.Maui.Controls.Label.TextColorProperty,
            state.HasFlag(CdsInteractionState.Disabled) ? CdsColorKeys.TextDisabled : CdsColorKeys.TextSecondary);
        _message.Apply(CdsFormMessageResolver.Resolve(state, HelperText, InvalidText, WarnText, hideHelperWhenBlocked: true));
        var inherited = state & (CdsInteractionState.Disabled | CdsInteractionState.ReadOnly | CdsInteractionState.Invalid);
        foreach (var item in _items) item.SetInherited(inherited, LabelPosition);
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
        if (propertyName == IsEnabledProperty.PropertyName) RefreshVisuals();
    }

    CdsInteractionState GroupState()
    {
        var state = State;
        if (IsInvalid) state |= CdsInteractionState.Invalid;
        if (IsWarning) state |= CdsInteractionState.Warn;
        if (IsReadOnly) state |= CdsInteractionState.ReadOnly;
        return state;
    }

    int SelectedIndex() => RadioGroupModel.IndexOfValue(_items.Select(r => r.Value).ToList(), SelectedValue);

    void UpdateChrome()
    {
        _legend.Text = Legend;
        _legend.IsVisible = !string.IsNullOrEmpty(Legend);
        SemanticProperties.SetDescription(_root, Legend);
        RebuildItems();
        RefreshVisuals();
    }

    void OnItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
            foreach (CdsRadioButton item in e.OldItems) Unhook(item);
        if (e.NewItems is not null)
            foreach (CdsRadioButton item in e.NewItems) Hook(item);
        RebuildItems();
        SyncChecked();
    }

    void Hook(CdsRadioButton radio)
    {
        radio.UserSelected += OnRadioSelected;
        radio.CheckedPropertyChanged += OnRadioCheckedChanged;
        radio.ValueChanged += OnRadioValueChanged;
    }

    void Unhook(CdsRadioButton radio)
    {
        radio.UserSelected -= OnRadioSelected;
        radio.CheckedPropertyChanged -= OnRadioCheckedChanged;
        radio.ValueChanged -= OnRadioValueChanged;
    }

    void OnRadioSelected(object? sender, EventArgs e)
    {
        if (sender is CdsRadioButton radio) SelectedValue = radio.Value;
    }

    void OnRadioCheckedChanged(object? sender, EventArgs e)
    {
        if (_syncing || sender is not CdsRadioButton radio) return;
        if (radio.IsChecked) SelectedValue = radio.Value;
        else if (Equals(SelectedValue, radio.Value)) SelectedValue = null;
    }

    void OnRadioValueChanged(object? sender, EventArgs e) => SyncChecked();

    void OnSelectedValueChanged(object? oldValue, object? newValue)
    {
        SyncChecked();
        SelectionChanged?.Invoke(this, new CdsChoiceChangedEventArgs<object>(oldValue, newValue));
    }

    void OnSelectedItemSet(CdsRadioButton? item)
    {
        if (_syncing) return;
        SelectedValue = item?.Value;
    }

    // Brings each radio's IsChecked and SelectedItem in line with SelectedValue.
    void SyncChecked()
    {
        if (_syncing) return;
        _syncing = true;
        try
        {
            CdsRadioButton? selected = null;
            foreach (var radio in _items)
            {
                var on = SelectedValue is not null && Equals(radio.Value, SelectedValue) && selected is null;
                if (on) selected = radio;
                if (radio.IsChecked != on) radio.IsChecked = on;
            }
            if (!ReferenceEquals(SelectedItem, selected)) SelectedItem = selected;
        }
        finally
        {
            _syncing = false;
        }
    }

    void RebuildItems()
    {
        // Detach first: a view can only have one parent.
        if (_itemsHost.Content is Layout old)
        {
            foreach (var child in old.Children.OfType<CdsChoiceCell>()) child.Content = null;
            old.Clear();
        }
        if (Orientation == CdsChoiceOrientation.Horizontal)
        {
            var row = new FlexLayout { Wrap = Microsoft.Maui.Layouts.FlexWrap.Wrap, Direction = Microsoft.Maui.Layouts.FlexDirection.Row, Margin = new Thickness(0, 0, 0, -6) };
            foreach (var item in _items)
                row.Add(new CdsChoiceCell { Content = item, Padding = new Thickness(0, 0, CdsSpacing.Spacing05, 6) });
            _itemsHost.Content = row;
        }
        else
        {
            var column = new VerticalStackLayout { Spacing = 6 };
            foreach (var item in _items) column.Add(item);
            _itemsHost.Content = column;
        }
        RefreshVisuals();
    }
}

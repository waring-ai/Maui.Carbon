using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Maui.Carbon;

/// <summary>
/// Groups <see cref="CdsCheckbox"/> items under one legend and owns the group's helper, invalid and warning messages.
/// Boxes stack 8px apart, or sit in a wrapping row 16px apart when <see cref="Orientation"/> is horizontal. Prefer
/// vertical, and leave 32px before the next form component.
/// </summary>
[ContentProperty(nameof(Items))]
public partial class CdsCheckboxGroup : CdsControl
{
    readonly Grid _root = new() { RowSpacing = CdsSpacing.Spacing03, ColumnSpacing = 0 };
    readonly CdsText _legend = new() { TypeStyle = CdsTypeStyle.Label01, Tone = CdsTextTone.Secondary };
    readonly ContentView _itemsHost = new();
    readonly CdsFormMessage _message = new();
    readonly CdsItemCollection<CdsCheckbox> _items = [];

    /// <summary>Creates an empty group.</summary>
    public CdsCheckboxGroup()
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

    /// <summary>The checkboxes of the group (the content property: nest <see cref="CdsCheckbox"/> elements in XAML).</summary>
    public IList<CdsCheckbox> Items => _items;

    /// <inheritdoc/>
    protected override void ApplyVisuals()
    {
        var state = GroupState();
        var disabled = state.HasFlag(CdsInteractionState.Disabled);
        SetColor(_legend, Microsoft.Maui.Controls.Label.TextColorProperty, disabled ? CdsColorKeys.TextDisabled : CdsColorKeys.TextSecondary);
        _message.Apply(CdsFormMessageResolver.Resolve(state, HelperText, InvalidText, WarnText));
        foreach (var item in _items) item.SetInherited(InheritedFor(state));
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

    // Disabled and read-only cascade to every box; invalid turns every box border red.
    static CdsInteractionState InheritedFor(CdsInteractionState state) =>
        state & (CdsInteractionState.Disabled | CdsInteractionState.ReadOnly | CdsInteractionState.Invalid);

    void UpdateChrome()
    {
        _legend.Text = Legend;
        _legend.IsVisible = !string.IsNullOrEmpty(Legend);
        CdsSemantics.SetGroupName(_root, Legend);
        RebuildItems();
        RefreshVisuals();
    }

    void OnItemsChanged(object? sender, NotifyCollectionChangedEventArgs e) => RebuildItems();

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
            var row = new FlexLayout { Wrap = Microsoft.Maui.Layouts.FlexWrap.Wrap, Direction = Microsoft.Maui.Layouts.FlexDirection.Row, Margin = new Thickness(0, 0, 0, -CdsSpacing.Spacing03) };
            foreach (var item in _items)
                row.Add(new CdsChoiceCell { Content = item, Padding = new Thickness(0, 0, CdsSpacing.Spacing05, CdsSpacing.Spacing03) });
            _itemsHost.Content = row;
        }
        else
        {
            var column = new VerticalStackLayout { Spacing = CdsSpacing.Spacing03 };
            foreach (var item in _items) column.Add(item);
            _itemsHost.Content = column;
        }
        RefreshVisuals();
    }
}

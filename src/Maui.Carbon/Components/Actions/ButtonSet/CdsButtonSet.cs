using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Maui.Carbon;

/// <summary>
/// Groups two or three related <see cref="CdsButton"/>s in a row (or a column with <see cref="IsStacked"/>) with equal
/// widths and no gap, separated by a 1px <c>button-separator</c> hairline. Each button is at most 12.25rem wide (20rem when
/// expressive); use one size per set and one high-emphasis (primary or danger) button. Order: in dialogs and side panels the
/// primary goes last; in in-page forms first; when stacked, first. <see cref="IsFluid"/> is the modal-footer form: right-aligned,
/// filling its container, sorted by kind and stacking itself when narrow.
/// </summary>
[ContentProperty(nameof(Items))]
public class CdsButtonSet : CdsControl
{
    /// <summary>Bindable property for <see cref="IsStacked"/>.</summary>
    public static readonly BindableProperty IsStackedProperty = BindableProperty.Create(
        nameof(IsStacked), typeof(bool), typeof(CdsButtonSet), false, propertyChanged: OnLayoutChanged);

    /// <summary>Bindable property for <see cref="IsFluid"/>.</summary>
    public static readonly BindableProperty IsFluidProperty = BindableProperty.Create(
        nameof(IsFluid), typeof(bool), typeof(CdsButtonSet), false, propertyChanged: OnLayoutChanged);

    /// <summary>Bindable property for <see cref="Label"/>.</summary>
    public static readonly BindableProperty LabelProperty = BindableProperty.Create(
        nameof(Label), typeof(string), typeof(CdsButtonSet), null,
        propertyChanged: static (b, _, n) =>
        {
            var set = (CdsButtonSet)b;
            CdsSemantics.SetGroupName(set._grid, (string?)n);
            CdsSemantics.SetGroupName(set._row, (string?)n);
        });

    readonly CdsItemCollection<CdsButton> _items = [];
    readonly Grid _grid = new() { ColumnSpacing = 0, RowSpacing = 0 };
    readonly CdsEqualWidthRow _row = new();
    readonly List<(BoxView Line, int Index)> _separators = [];
    bool _fluidStacked;

    /// <summary>Creates an empty button set.</summary>
    public CdsButtonSet()
    {
        HorizontalOptions = LayoutOptions.Start;
        _items.CollectionChanged += OnItemsChanged;
        _grid.SizeChanged += (_, _) => OnGridSizeChanged();
        SetChrome(_grid);
        Rebuild();
    }

    /// <summary>The buttons, in visual order (the content property).</summary>
    public IList<CdsButton> Items => _items;

    /// <summary>Lay the buttons out in a column (narrow columns and side panels). The primary comes first.</summary>
    public bool IsStacked
    {
        get => (bool)GetValue(IsStackedProperty);
        set => SetValue(IsStackedProperty, value);
    }

    /// <summary>
    /// Fluid layout for modal, tearsheet and side-panel footers: fills the container, right-aligned buttons of 25% (11-14.5rem), ghost
    /// buttons grow, buttons are ordered ghost, danger ghost, tertiary, secondary, danger, primary, and the set stacks (primary
    /// first) when the container is under 11rem per button.
    /// </summary>
    public bool IsFluid
    {
        get => (bool)GetValue(IsFluidProperty);
        set => SetValue(IsFluidProperty, value);
    }

    /// <summary>The accessible name of the group, for pages with several sets.</summary>
    public string? Label
    {
        get => (string?)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    bool Stacked => IsStacked || (IsFluid && _fluidStacked);

    void OnItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
            foreach (CdsButton b in e.OldItems) b.StateChanged -= OnButtonStateChanged;
        if (e.NewItems is not null)
            foreach (CdsButton b in e.NewItems) b.StateChanged += OnButtonStateChanged;
        Rebuild();
    }

    void OnButtonStateChanged(object? sender, EventArgs e) => UpdateSeparators();

    void OnGridSizeChanged()
    {
        if (!IsFluid || _grid.Width <= 0) return;
        var stack = ButtonSetModel.ShouldStack(_grid.Width, _items.Count);
        if (stack != _fluidStacked)
        {
            _fluidStacked = stack;
            Rebuild();
        }
        else if (!stack)
        {
            ApplyFluidWidths();
        }
    }

    List<CdsButton> Ordered()
    {
        var buttons = _items.ToList();
        if (!IsFluid) return buttons;
        return ButtonSetModel.FluidOrder(buttons.Select(b => b.Kind).ToArray()).Select(i => buttons[i]).ToList();
    }

    void Rebuild()
    {
        _row.Clear();
        _grid.Children.Clear();
        _grid.ColumnDefinitions.Clear();
        _grid.RowDefinitions.Clear();
        _separators.Clear();

        var buttons = Ordered();
        var stacked = Stacked;
        var expressive = buttons.Any(b => b.IsExpressive);

        if (!IsFluid && !stacked)
        {
            BuildRow(buttons, expressive);
            return;
        }

        if (Content != _grid) SetChrome(_grid);
        if (IsFluid)
        {
            HorizontalOptions = LayoutOptions.Fill;
            _grid.HorizontalOptions = LayoutOptions.Fill;
            _grid.MaximumWidthRequest = double.PositiveInfinity;
        }
        else
        {
            HorizontalOptions = LayoutOptions.Start;
            _grid.HorizontalOptions = LayoutOptions.Fill;
            _grid.MaximumWidthRequest = ButtonSetModel.MaxSetWidth(buttons.Count, stacked, expressive);
        }

        // Stacked sets list the primary first.
        if (stacked && IsFluid) buttons.Reverse();

        for (var i = 0; i < buttons.Count; i++)
        {
            var button = buttons[i];
            button.HorizontalOptions = LayoutOptions.Fill;
            if (stacked)
            {
                _grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
                _grid.Add(button, 0, i);
            }
            else
            {
                _grid.ColumnDefinitions.Add(new ColumnDefinition(IsFluid ? GridLength.Auto : GridLength.Star));
                _grid.Add(button, i, 0);
            }
        }
        if (stacked) _grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

        // Separators sit on the leading edge of every button after the first.
        for (var i = 1; i < buttons.Count; i++)
        {
            var line = new BoxView { InputTransparent = true };
            AutomationProperties.SetIsInAccessibleTree(line, false);
            if (stacked)
            {
                line.HeightRequest = 1;
                line.VerticalOptions = LayoutOptions.Start;
                line.HorizontalOptions = LayoutOptions.Fill;
                line.Margin = new Thickness(0, -1, 0, 0);
                _grid.Add(line, 0, i);
            }
            else
            {
                line.WidthRequest = 1;
                line.HorizontalOptions = LayoutOptions.Start;
                line.VerticalOptions = LayoutOptions.Fill;
                line.Margin = new Thickness(-1, 0, 0, 0);
                _grid.Add(line, i, 0);
            }
            _separators.Add((line, i));
        }

        _orderedForSeparators = buttons;
        if (IsFluid && !stacked) ApplyFluidWidths();
        UpdateSeparators();
    }

    List<CdsButton> _orderedForSeparators = [];

    /// <summary>A plain horizontal set: equal cells as wide as the widest label needs, each at most the Carbon maximum.</summary>
    void BuildRow(List<CdsButton> buttons, bool expressive)
    {
        if (Content != _row) SetChrome(_row);
        HorizontalOptions = LayoutOptions.Start;
        _row.MaxCellWidth = expressive ? ButtonSetModel.MaxExpressiveButtonWidth : ButtonSetModel.MaxButtonWidth;
        for (var i = 0; i < buttons.Count; i++)
        {
            buttons[i].HorizontalOptions = LayoutOptions.Fill;
            _row.Add(buttons[i]);
        }
        for (var i = 1; i < buttons.Count; i++)
        {
            var line = new BoxView { InputTransparent = true };
            AutomationProperties.SetIsInAccessibleTree(line, false);
            CdsEqualWidthRow.SetSeparatorFor(line, i);
            _row.Add(line);
            _separators.Add((line, i));
        }
        _orderedForSeparators = buttons;
        UpdateSeparators();
    }

    void ApplyFluidWidths()
    {
        var buttons = _orderedForSeparators;
        if (buttons.Count == 0 || _grid.Width <= 0) return;
        var widths = ButtonSetModel.FluidWidths(_grid.Width, buttons.Select(b => b.Kind is CdsButtonKind.Ghost or CdsButtonKind.DangerGhost).ToArray(), out var offset);
        for (var i = 0; i < buttons.Count && i < _grid.ColumnDefinitions.Count; i++)
            _grid.ColumnDefinitions[i].Width = new GridLength(widths[i]);
        _grid.Padding = new Thickness(offset, 0, 0, 0);
    }

    void UpdateSeparators()
    {
        var buttons = _orderedForSeparators;
        var stacked = Stacked;
        foreach (var (line, i) in _separators)
        {
            if (i >= buttons.Count) continue;
            var current = buttons[i];
            var previous = buttons[i - 1];
            line.IsVisible = ButtonSetModel.ShowSeparator(i,
                previous.Is(CdsInteractionState.FocusVisible), current.Is(CdsInteractionState.FocusVisible));
            SetColor(line, BoxView.ColorProperty, CdsButtonSetTokens.Separator(stacked, !current.IsEnabled));
        }
    }

    static void OnLayoutChanged(BindableObject b, object? o, object? n)
    {
        var set = (CdsButtonSet)b;
        set._fluidStacked = false;
        set.Rebuild();
    }
}

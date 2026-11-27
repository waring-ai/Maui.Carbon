namespace Maui.Carbon;

/// <summary>
/// Groups <see cref="CdsRadioTile"/> items so exactly one can be chosen. The tiles can sit in any layout you put in
/// <see cref="CdsContainer.Body"/> (a grid of cards): the group finds every radio tile inside it. It owns the legend and
/// <see cref="SelectedValue"/>. Like native radios the group is one tab stop: Tab lands on the chosen tile (the first
/// enabled one when nothing is chosen) and leaves the group in one step; the arrow keys move the focus and choose within
/// the group (wrapping, skipping disabled and hidden tiles), and Space chooses the focused tile.
/// </summary>
public class CdsTileGroup : CdsContainer
{
    /// <summary>Bindable property for <see cref="Legend"/>.</summary>
    public static readonly BindableProperty LegendProperty = BindableProperty.Create(
        nameof(Legend), typeof(string), typeof(CdsTileGroup), null,
        propertyChanged: static (b, _, _) => ((CdsTileGroup)b).UpdateChrome());

    /// <summary>Bindable property for <see cref="SelectedValue"/>.</summary>
    public static readonly BindableProperty SelectedValueProperty = BindableProperty.Create(
        nameof(SelectedValue), typeof(object), typeof(CdsTileGroup), null, BindingMode.TwoWay,
        propertyChanged: static (b, o, n) => ((CdsTileGroup)b).OnSelectedValueChanged(o, n));

    /// <summary>Bindable property for <see cref="IsReadOnly"/>.</summary>
    public static readonly BindableProperty IsReadOnlyProperty = BindableProperty.Create(
        nameof(IsReadOnly), typeof(bool), typeof(CdsTileGroup), false);

    readonly Grid _root = new() { RowSpacing = CdsSpacing.Spacing03, ColumnSpacing = 0 };
    readonly CdsText _legend = new() { TypeStyle = CdsTypeStyle.Label01, Tone = CdsTextTone.Secondary };
    readonly ContentView _bodyHost = new();
    readonly List<CdsRadioTile> _tiles = [];
    // Radio-group keyboard model: the stop returns to the chosen tile when focus leaves, any arrow moves (tiles can wrap
    // in a grid of cards), and there is no Home or End, as with native radios.
    readonly CdsRovingFocus _roving = new(CdsRovingOrientation.Both, wraps: true) { RemembersFocus = false, HandlesHomeEnd = false };
    bool _syncing;

    /// <summary>Creates an empty group.</summary>
    public CdsTileGroup()
    {
        _root.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        _root.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        _root.Add(_legend, 0, 0);
        _root.Add(_bodyHost, 0, 1);
        _roving.CanFocus = i => i >= 0 && i < _tiles.Count && _tiles[i].CanTakeFocus;
        _roving.PreferredIndex = () => _tiles.FindIndex(t => t.IsChecked);
        _roving.Navigated += OnNavigated;
        SetChrome(_root);
        UpdateChrome();
    }

    /// <summary>Raised when the chosen tile changes (by the user or in code).</summary>
    public event EventHandler<CdsChoiceChangedEventArgs<object>>? SelectionChanged;

    /// <summary>The group's name, shown above the tiles in <c>label-01</c>. Always provide one; it can be short ("Plan").</summary>
    public string? Legend
    {
        get => (string?)GetValue(LegendProperty);
        set => SetValue(LegendProperty, value);
    }

    /// <summary>The <see cref="CdsRadioTile.Value"/> of the chosen tile, or <c>null</c> (two-way).</summary>
    public object? SelectedValue
    {
        get => GetValue(SelectedValueProperty);
        set => SetValue(SelectedValueProperty, value);
    }

    /// <summary>Read-only: the choice cannot change; tiles stay focusable.</summary>
    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    /// <inheritdoc/>
    protected internal override bool WantsNavigationKeys => true;

    /// <inheritdoc/>
    protected internal override bool OnKeyDown(CdsKeyEventArgs e) =>
        // An AI label on a tile takes its own keys: an arrow pressed on it must not move the group's choice.
        _tiles.Count > 0 && !CdsFocusTracker.FocusChain().Any(c => c is CdsAILabel) && _roving.HandleKey(e);

    /// <summary>The index of the tile that has the group's one tab stop, or -1 when no tile can take focus.</summary>
    internal int TabStopIndex => _roving.TabStopIndex;

    // An arrow moved the focus: the focused tile is chosen (selection follows focus, as with native radios).
    void OnNavigated(object? sender, int index)
    {
        if (!IsReadOnly && index >= 0 && index < _tiles.Count) SelectedValue = _tiles[index].Value;
    }

    /// <inheritdoc/>
    protected override void OnBodyChanged(View? oldBody, View? newBody)
    {
        if (oldBody is not null) oldBody.DescendantAdded -= OnDescendantsChanged;
        if (oldBody is not null) oldBody.DescendantRemoved -= OnDescendantsChanged;
        _bodyHost.Content = newBody;
        if (newBody is not null)
        {
            newBody.DescendantAdded += OnDescendantsChanged;
            newBody.DescendantRemoved += OnDescendantsChanged;
        }
        Rescan();
    }

    /// <inheritdoc/>
    protected override void ApplyVisuals()
    {
        var disabled = Is(CdsInteractionState.Disabled);
        SetColor(_legend, Microsoft.Maui.Controls.Label.TextColorProperty, disabled ? CdsColorKeys.TextDisabled : CdsColorKeys.TextSecondary);
        foreach (var tile in _tiles) tile.SetInheritedDisabled(disabled);
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
        if (propertyName == IsEnabledProperty.PropertyName) RefreshVisuals();
    }

    void UpdateChrome()
    {
        _legend.Text = Legend;
        _legend.IsVisible = !string.IsNullOrEmpty(Legend);
        CdsSemantics.SetGroupName(_root, Legend);
        RefreshVisuals();
    }

    void OnDescendantsChanged(object? sender, ElementEventArgs e) => Rescan();

    // Finds every radio tile in the body and wires the group to it.
    void Rescan()
    {
        var found = CdsTileText.Descendants(Body).OfType<CdsRadioTile>().ToList();
        if (Body is CdsRadioTile single) found.Insert(0, single);
        foreach (var tile in _tiles.Except(found)) Unhook(tile);
        foreach (var tile in found.Except(_tiles)) Hook(tile);
        _tiles.Clear();
        _tiles.AddRange(found);
        SyncChecked();
        RefreshVisuals();
        _roving.SetItems(_tiles.Select(t => t.Hit));
    }

    void Hook(CdsRadioTile tile)
    {
        tile.UserSelected += OnTileSelected;
        tile.CheckedPropertyChanged += OnTileCheckedChanged;
        tile.ValueChanged += OnTileValueChanged;
        tile.PropertyChanged += OnTilePropertyChanged;
    }

    void Unhook(CdsRadioTile tile)
    {
        tile.UserSelected -= OnTileSelected;
        tile.CheckedPropertyChanged -= OnTileCheckedChanged;
        tile.ValueChanged -= OnTileValueChanged;
        tile.PropertyChanged -= OnTilePropertyChanged;
        tile.Hit.IsTabStop = true;
    }

    // A hidden tile cannot take the tab stop (its hit target's own visibility does not change).
    void OnTilePropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == IsVisibleProperty.PropertyName) _roving.Refresh();
    }

    void OnTileSelected(object? sender, EventArgs e)
    {
        if (sender is CdsRadioTile tile && !IsReadOnly) SelectedValue = tile.Value;
    }

    void OnTileCheckedChanged(object? sender, EventArgs e)
    {
        if (_syncing || sender is not CdsRadioTile tile) return;
        if (IsReadOnly)
        {
            SyncChecked();
            return;
        }
        if (tile.IsChecked) SelectedValue = tile.Value;
        else if (Equals(SelectedValue, tile.Value)) SelectedValue = null;
    }

    void OnTileValueChanged(object? sender, EventArgs e) => SyncChecked();

    void OnSelectedValueChanged(object? oldValue, object? newValue)
    {
        SyncChecked();
        SelectionChanged?.Invoke(this, new CdsChoiceChangedEventArgs<object>(oldValue, newValue));
    }

    // Brings each tile's IsChecked in line with SelectedValue (only the first tile with that value is chosen).
    void SyncChecked()
    {
        if (_syncing) return;
        _syncing = true;
        try
        {
            var chosen = false;
            foreach (var tile in _tiles)
            {
                var on = !chosen && SelectedValue is not null && Equals(tile.Value, SelectedValue);
                chosen |= on;
                if (tile.IsChecked != on) tile.IsChecked = on;
            }
        }
        finally
        {
            _syncing = false;
        }
        _roving.Refresh();
    }
}

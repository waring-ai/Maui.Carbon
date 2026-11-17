namespace Maui.Carbon;

/// <summary>
/// Groups <see cref="CdsRadioTile"/> items so exactly one can be chosen. The tiles can sit in any layout you put in
/// <see cref="CdsContainer.Body"/> (a grid of cards): the group finds every radio tile inside it. It owns the legend and
/// <see cref="SelectedValue"/>; arrow keys move the focus and choose within the group (wrapping, skipping disabled tiles).
/// MAUI cannot take a native button out of the Tab order, so every enabled tile stays a tab stop.
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
    bool _syncing;

    /// <summary>Creates an empty group.</summary>
    public CdsTileGroup()
    {
        _root.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        _root.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        _root.Add(_legend, 0, 0);
        _root.Add(_bodyHost, 0, 1);
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
    protected internal override bool OnKeyDown(CdsKeyEventArgs e)
    {
        var direction = RadioGroupModel.Direction(e.Key);
        if (direction == 0 || _tiles.Count == 0) return false;
        var enabled = _tiles.Select(t => !t.IsEffectivelyDisabled).ToList();
        var current = _tiles.FindIndex(t => t.Is(CdsInteractionState.Focused));
        if (current < 0) current = RadioGroupModel.IndexOfValue(_tiles.Select(t => t.Value).ToList(), SelectedValue);
        var next = RadioGroupModel.NextEnabled(enabled, current, direction);
        if (next < 0) return false;
        _tiles[next].FocusTile();
        if (!IsReadOnly) SelectedValue = _tiles[next].Value;
        e.Handled = true;
        return true;
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
        SemanticProperties.SetDescription(_root, Legend);
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
    }

    void Hook(CdsRadioTile tile)
    {
        tile.UserSelected += OnTileSelected;
        tile.CheckedPropertyChanged += OnTileCheckedChanged;
        tile.ValueChanged += OnTileValueChanged;
    }

    void Unhook(CdsRadioTile tile)
    {
        tile.UserSelected -= OnTileSelected;
        tile.CheckedPropertyChanged -= OnTileCheckedChanged;
        tile.ValueChanged -= OnTileValueChanged;
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
    }
}

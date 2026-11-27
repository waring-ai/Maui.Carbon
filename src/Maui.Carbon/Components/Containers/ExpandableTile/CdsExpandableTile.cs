namespace Maui.Carbon;

/// <summary>
/// A tile that shows a summary (<see cref="CdsContainer.Body"/>) and reveals more content
/// (<see cref="ExpandedContent"/>) when expanded. When neither fold has interactive content the whole tile is one button
/// with a non-interactive chevron; when one does, the tile is the interactive variant: no hover, and a separate 48px
/// chevron button toggles it. The height animates over <c>duration-moderate-01</c> (<c>duration-fast-02</c> when
/// expanding), the below-the-fold content fades, and the chevron turns 180 degrees; all of it jumps with reduced motion.
/// Keep content clear of the 48px chevron corner.
/// </summary>
public partial class CdsExpandableTile : CdsTileBase
{
    readonly ContentView _expandedHost = new() { Padding = new Thickness(CdsSpacing.Spacing05, 0, CdsSpacing.Spacing05, CdsSpacing.Spacing05), IsVisible = false, Opacity = 0 };
    readonly Grid _chevronBox = new() { HeightRequest = 48, MinimumWidthRequest = 48, HorizontalOptions = LayoutOptions.End, VerticalOptions = LayoutOptions.End };
    readonly HorizontalStackLayout _chevronRow = new() { Spacing = 0, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
    readonly CdsText _chevronText = new() { TypeStyle = CdsTypeStyle.Label01, Tone = CdsTextTone.Secondary, VerticalOptions = LayoutOptions.Center, Margin = new Thickness(0, 0, CdsSpacing.Spacing03, 0) };
    readonly CdsIcon _chevronIcon = new(CdsIconName.ChevronDown, 16) { AnchorX = 0.5, AnchorY = 0.5 };
    readonly CdsFocusRing _chevronRing = new(CdsFocusRingStyle.Inset);
    readonly CdsHitTarget _tileHit;
    readonly CdsHitTarget _chevronHit;
    bool _interactive;
    bool _chevronHover;
    int _version;

    /// <summary>Creates a collapsed tile.</summary>
    public CdsExpandableTile()
    {
        // Collapsed height is the above-the-fold content plus padding: never stretch to a taller neighbour.
        VerticalOptions = LayoutOptions.Start;
        Surface.MinimumHeightRequest = 0;
        Stack.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        // The second row takes any extra height, so the chevron box (spanning both rows) stays at the bottom end.
        Stack.RowDefinitions.Add(new RowDefinition(GridLength.Star));
        Grid.SetRow(_expandedHost, 1);
        Stack.Insert(1, _expandedHost);
        Grid.SetRowSpan(Ring, 2);

        _chevronRow.Add(_chevronText);
        _chevronRow.Add(_chevronIcon);
        _chevronRow.InputTransparent = true;
        AutomationProperties.SetExcludedWithChildren(_chevronRow, true);
        _chevronBox.Add(_chevronRow);
        _chevronBox.Add(_chevronRing);

        _tileHit = CreateHitTarget(CdsAccessibilityRole.Expander);
        _chevronHit = CreateHitTarget(CdsAccessibilityRole.Expander);
        _chevronBox.Add(_chevronHit);
        var pointer = new PointerGestureRecognizer();
        pointer.PointerEntered += (_, _) =>
        {
            _chevronHover = true;
            RefreshVisuals();
        };
        pointer.PointerExited += (_, _) =>
        {
            _chevronHover = false;
            RefreshVisuals();
        };
        _chevronBox.GestureRecognizers.Add(pointer);

        Grid.SetRowSpan(_chevronBox, 2);
        Grid.SetRowSpan(_tileHit, 2);
        Stack.Add(_chevronBox);
        Stack.Add(_tileHit);
        Loaded += (_, _) => UpdateVariant();
        UpdateVariant();
    }

    /// <summary>Raised when <see cref="IsExpanded"/> changes (by the user or in code).</summary>
    public event EventHandler<CdsChoiceChangedEventArgs<bool>>? ExpandedChanged;

    /// <summary>Whether the tile is currently the interactive variant (only the chevron toggles it).</summary>
    public bool IsInteractiveVariant => _interactive;

    /// <inheritdoc/>
    protected override CdsAILabelSize? DecoratorSize => CdsAILabelSize.Xs;

    /// <inheritdoc/>
    protected override void OnDecoratorReplaced() => UpdateVariant();

    /// <inheritdoc/>
    protected override CdsTileKind TileKind => _interactive ? CdsTileKind.ExpandableInteractive : CdsTileKind.Expandable;

    /// <inheritdoc/>
    protected override CdsInteractionState TokenState =>
        _interactive ? (_chevronHover ? State | CdsInteractionState.Hover : State & ~CdsInteractionState.Hover) : State;

    /// <inheritdoc/>
    protected override void OnActivated() => IsExpanded = !IsExpanded;

    /// <inheritdoc/>
    protected override void OnBodyReplaced() => UpdateVariant();

    /// <inheritdoc/>
    protected override void ApplyTileVisuals(CdsTileVisuals visuals)
    {
        SetColor(_chevronIcon, CdsIcon.ColorProperty, visuals.Icon);
        SetColor(_chevronBox, BackgroundColorProperty, visuals.ChevronBackground);
        var visible = Is(CdsInteractionState.FocusVisible);
        _chevronRing.IsActive = visible && _interactive;
        Ring.IsActive = visible && !_interactive;
        var enabled = !Is(CdsInteractionState.Disabled);
        _tileHit.IsEnabled = enabled;
        _chevronHit.IsEnabled = enabled;
    }

    void OnExpandedContentChanged(View? content)
    {
        _expandedHost.Content = content;
        UpdateVariant();
    }

    void UpdateVariant()
    {
        _interactive = ExpandableTileModel.UseInteractiveVariant(IsInteractive, Body, ExpandedContent, Decorator is not null);
        _tileHit.IsVisible = !_interactive;
        _chevronHit.IsVisible = _interactive;
        // Non-interactive: the tile is one button and its content is just text. Interactive: content stays reachable.
        BodyHost.InputTransparent = !_interactive;
        _expandedHost.InputTransparent = !_interactive;
        AutomationProperties.SetExcludedWithChildren(BodyHost, !_interactive);
        AutomationProperties.SetExcludedWithChildren(_expandedHost, !_interactive);
        UpdateToggle();
    }

    void UpdateToggle()
    {
        var description = ExpandableTileModel.Description(IsExpanded, CollapsedDescription, ExpandedDescription);
        foreach (var hit in new[] { _tileHit, _chevronHit })
        {
            SemanticProperties.SetDescription(hit, description);
            hit.IsExpanded = IsExpanded;
        }
        var text = _interactive ? null : IsExpanded ? ExpandedText : CollapsedText;
        _chevronText.Text = text;
        _chevronText.IsVisible = !string.IsNullOrEmpty(text);
        _chevronRow.Padding = _chevronText.IsVisible ? new Thickness(CdsSpacing.Spacing05, 0) : new Thickness(0);
        _chevronBox.WidthRequest = _chevronText.IsVisible ? -1 : 48;
        // A labelled chevron is wide: reserve its 48px row under the expanded content so the label never covers text.
        _expandedHost.Padding = new Thickness(CdsSpacing.Spacing05, 0, CdsSpacing.Spacing05, ExpandableTileModel.ExpandedBottomPadding(_chevronText.IsVisible));
        RefreshVisuals();
    }

    void OnExpandedChanged(bool oldValue, bool newValue)
    {
        UpdateToggle();
        var version = ++_version;
        if (!IsLoaded || Handler is null || Width <= 0 || CdsMotion.IsReducedMotion) ApplyExpansion();
        else _ = AnimateExpansionAsync(newValue, version);
        ExpandedChanged?.Invoke(this, new CdsChoiceChangedEventArgs<bool>(oldValue, newValue));
    }

    // The settled state: content shown or hidden, natural height, chevron up or down.
    void ApplyExpansion()
    {
        _expandedHost.IsVisible = IsExpanded;
        _expandedHost.Opacity = IsExpanded ? 1 : 0;
        Surface.HeightRequest = -1;
        _chevronIcon.Rotation = IsExpanded ? 180 : 0;
    }

    async Task AnimateExpansionAsync(bool expanding, int version)
    {
        var from = Surface.Height > 0 ? Surface.Height : Surface.Measure(Width, double.PositiveInfinity).Height;
        double to;
        if (expanding)
        {
            _expandedHost.Opacity = 0;
            _expandedHost.IsVisible = true;
            Surface.HeightRequest = -1;
            to = Surface.Measure(Width, double.PositiveInfinity).Height;
        }
        else
        {
            to = BodyHost.Measure(Width, double.PositiveInfinity).Height + Surface.StrokeThickness * 2;
        }
        Surface.HeightRequest = from;
        var duration = ExpandableTileModel.Duration(expanding);
        var rotationFrom = _chevronIcon.Rotation;
        await Task.WhenAll(
            CdsMotion.AnimateAsync(this, "CdsTileHeight", p => Surface.HeightRequest = ExpandableTileModel.HeightAt(from, to, p), 0, 1, duration, CdsEasing.StandardProductive),
            CdsMotion.AnimateAsync(this, "CdsTileFade", p => _expandedHost.Opacity = p, expanding ? 0 : 1, expanding ? 1 : 0, CdsDuration.Fast02, CdsEasing.StandardProductive),
            CdsMotion.AnimateAsync(this, "CdsTileChevron", p => _chevronIcon.Rotation = p, rotationFrom, expanding ? 180 : 0, CdsDuration.Fast02, CdsEasing.StandardProductive));
        // A newer toggle took over while this one was running.
        if (version == _version) ApplyExpansion();
    }
}

namespace Maui.Carbon;

/// <summary>
/// An icon-only button (three dots) that opens a <see cref="CdsMenu"/> of secondary actions for a row, card or toolbar.
/// Put <see cref="CdsMenuItem"/>s (and dividers) directly inside it. Name the trigger after the object
/// (<see cref="IconDescription"/> "Actions for ci-deploy"); size it to its row; use <see cref="IsFlipped"/> when it sits at
/// the end of a row so the menu stays inside. While the menu is open the trigger takes the <c>layer-01</c> fill and the menu
/// shadow so it reads as part of the menu. Keep the menu to about 12 items, destructive items last after a divider.
/// </summary>
[ContentProperty(nameof(Items))]
public class CdsOverflowMenu : CdsControl
{
    /// <summary>Bindable property for <see cref="IconDescription"/>.</summary>
    public static readonly BindableProperty IconDescriptionProperty = BindableProperty.Create(
        nameof(IconDescription), typeof(string), typeof(CdsOverflowMenu), "Options", propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="Icon"/>.</summary>
    public static readonly BindableProperty IconProperty = BindableProperty.Create(
        nameof(Icon), typeof(CdsIconName), typeof(CdsOverflowMenu), CdsIconName.OverflowMenuVertical, propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="Size"/>.</summary>
    public static readonly BindableProperty SizeProperty = BindableProperty.Create(
        nameof(Size), typeof(CdsSize), typeof(CdsOverflowMenu), CdsSize.Md, propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="IsFlipped"/>.</summary>
    public static readonly BindableProperty IsFlippedProperty = BindableProperty.Create(
        nameof(IsFlipped), typeof(bool), typeof(CdsOverflowMenu), false);

    /// <summary>Bindable property for <see cref="Direction"/>.</summary>
    public static readonly BindableProperty DirectionProperty = BindableProperty.Create(
        nameof(Direction), typeof(CdsOverflowMenuDirection), typeof(CdsOverflowMenu), CdsOverflowMenuDirection.Bottom);

    /// <summary>Bindable property for <see cref="IsOpen"/>.</summary>
    public static readonly BindableProperty IsOpenProperty = BindableProperty.Create(
        nameof(IsOpen), typeof(bool), typeof(CdsOverflowMenu), false, BindingMode.TwoWay,
        propertyChanged: static (b, _, n) => ((CdsOverflowMenu)b).OnIsOpenChanged((bool)n));

    readonly CdsActionFace _face = new();
    readonly CdsMenu _menu = new();
    bool _openPending;

    /// <summary>Creates a vertical-dots trigger of size md.</summary>
    public CdsOverflowMenu()
    {
        HorizontalOptions = LayoutOptions.Start;
        VerticalOptions = LayoutOptions.Start;
        _face.Mode = CdsActionFaceMode.IconOnly;
        _face.RingStyle = CdsFocusRingStyle.Inset;
        _face.Resolver = CdsOverflowMenuTokens.Resolve;
        _face.Expandable = true;
        _face.ShadowWhenOpen = true;
        _face.Activated += (_, _) => Toggle();
        _menu.Opened += (_, _) =>
        {
            _face.IsOpen = true;
            Opened?.Invoke(this, EventArgs.Empty);
        };
        _menu.Closed += (_, _) =>
        {
            _face.IsOpen = false;
            SetValue(IsOpenProperty, false);
            Closed?.Invoke(this, EventArgs.Empty);
        };
        SetChrome(_face);
        TrackFocus(_face.Hit, () => _face.Hit.IsFocusFromKeyboard);
        // Tabbing away (or any focus move off the trigger) closes the menu, so its keys stop acting on it.
        _face.Hit.Unfocused += (_, _) => _menu.CloseForFocusLoss();
        Loaded += (_, _) =>
        {
            if (_openPending) Dispatcher.Dispatch(() => OnIsOpenChanged(true));
        };
        Unloaded += (_, _) => _menu.Close();
        Update();
    }

    /// <summary>Raised when the menu opens.</summary>
    public event EventHandler? Opened;

    /// <summary>Raised when the menu closes.</summary>
    public event EventHandler? Closed;

    /// <summary>The accessible name and tooltip of the trigger, also the menu's name. Default "Options"; make it unique per row.</summary>
    public string? IconDescription
    {
        get => (string?)GetValue(IconDescriptionProperty);
        set => SetValue(IconDescriptionProperty, value);
    }

    /// <summary>The trigger glyph. Default <see cref="CdsIconName.OverflowMenuVertical"/>; <see cref="CdsIconName.OverflowMenuHorizontal"/> is the other standard one.</summary>
    public CdsIconName Icon
    {
        get => (CdsIconName)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>Trigger square and menu row height: xs 24, sm 32, md 40 (default), lg 48. Match the row it sits in.</summary>
    public CdsSize Size
    {
        get => (CdsSize)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    /// <summary>Align the menu's end edge to the trigger's end edge instead of its start edge.</summary>
    public bool IsFlipped
    {
        get => (bool)GetValue(IsFlippedProperty);
        set => SetValue(IsFlippedProperty, value);
    }

    /// <summary>Open below (default) or above the trigger. The menu still flips when it does not fit.</summary>
    public CdsOverflowMenuDirection Direction
    {
        get => (CdsOverflowMenuDirection)GetValue(DirectionProperty);
        set => SetValue(DirectionProperty, value);
    }

    /// <summary>Whether the menu is open. Two-way; setting it before the control is on screen opens the menu once it loads.</summary>
    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    /// <summary>The menu items (the content property).</summary>
    public IList<CdsMenuItemBase> Items => _menu.Items;

    /// <summary>The underlying menu, for reuse and advanced settings.</summary>
    public CdsMenu Menu => _menu;

    void Toggle()
    {
        if (_menu.IsOpen) _menu.Close();
        else SetValue(IsOpenProperty, true);
    }

    void OnIsOpenChanged(bool open)
    {
        if (!open)
        {
            _openPending = false;
            _menu.Close();
            return;
        }
        if (_menu.IsOpen) return;
        if (!_face.IsLoaded || !IsEnabled)
        {
            _openPending = true;
            return;
        }
        _openPending = false;
        _menu.Size = Size;
        _menu.Label = IconDescription;
        _menu.Open(_face, CdsOverflowMenuTokens.Placement(Direction, IsFlipped));
    }

    void Update()
    {
        _face.ButtonHeight = CdsOverflowMenuTokens.Side(Size);
        _face.Icon = Icon;
        _face.AccessibleName = IconDescription;
        if (!string.IsNullOrEmpty(IconDescription)) ToolTipProperties.SetText(_face.Hit, IconDescription);
        else _face.Hit.ClearValue(ToolTipProperties.TextProperty);
        RefreshVisuals();
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
        if (propertyName == IsEnabledProperty.PropertyName && _face is not null) _face.IsEnabled = IsEnabled;
    }

    static void OnChanged(BindableObject b, object? o, object? n) => ((CdsOverflowMenu)b).Update();
}

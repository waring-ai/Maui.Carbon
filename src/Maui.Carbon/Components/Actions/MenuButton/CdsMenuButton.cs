namespace Maui.Carbon;

/// <summary>
/// A button with a chevron that opens a <see cref="CdsMenu"/> of related actions beneath it: for several actions of equal
/// weight that act on the whole page or a large object. Label it with the verb's object or "Actions" (never "Menu"), keep it to
/// fewer than five items and put destructive items last after a divider. When one action dominates use a
/// <see cref="CdsComboButton"/>; for row or card actions an <see cref="CdsOverflowMenu"/>. The chevron turns 180 degrees while open.
/// </summary>
[ContentProperty(nameof(Items))]
public class CdsMenuButton : CdsControl
{
    /// <summary>Bindable property for <see cref="Label"/>.</summary>
    public static readonly BindableProperty LabelProperty = BindableProperty.Create(
        nameof(Label), typeof(string), typeof(CdsMenuButton), null, propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="Kind"/>.</summary>
    public static readonly BindableProperty KindProperty = BindableProperty.Create(
        nameof(Kind), typeof(CdsMenuButtonKind), typeof(CdsMenuButton), CdsMenuButtonKind.Primary, propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="Size"/>.</summary>
    public static readonly BindableProperty SizeProperty = BindableProperty.Create(
        nameof(Size), typeof(CdsSize), typeof(CdsMenuButton), CdsSize.Lg, propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="MenuAlignment"/>.</summary>
    public static readonly BindableProperty MenuAlignmentProperty = BindableProperty.Create(
        nameof(MenuAlignment), typeof(CdsMenuAlignment), typeof(CdsMenuButton), CdsMenuAlignment.Bottom);

    /// <summary>Bindable property for <see cref="HasMenuBorder"/>.</summary>
    public static readonly BindableProperty HasMenuBorderProperty = BindableProperty.Create(
        nameof(HasMenuBorder), typeof(bool), typeof(CdsMenuButton), false);

    /// <summary>Bindable property for <see cref="IsOpen"/>.</summary>
    public static readonly BindableProperty IsOpenProperty = BindableProperty.Create(
        nameof(IsOpen), typeof(bool), typeof(CdsMenuButton), false, BindingMode.TwoWay,
        propertyChanged: static (b, _, n) => ((CdsMenuButton)b).OnIsOpenChanged((bool)n));

    readonly CdsActionFace _face = new();
    readonly CdsMenu _menu = new();
    bool _openPending;

    /// <summary>Creates a large primary menu button.</summary>
    public CdsMenuButton()
    {
        HorizontalOptions = LayoutOptions.Start;
        VerticalOptions = LayoutOptions.Start;
        _face.Icon = CdsIconName.ChevronDown;
        _face.RotateIconWhenOpen = true;
        _face.Expandable = true;
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

    /// <summary>The button text and the menu's accessible name: the verb's object or "Actions", never "Menu".</summary>
    public string? Label
    {
        get => (string?)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <summary>Emphasis: primary (default), tertiary or ghost; secondary also renders.</summary>
    public CdsMenuButtonKind Kind
    {
        get => (CdsMenuButtonKind)GetValue(KindProperty);
        set => SetValue(KindProperty, value);
    }

    /// <summary>Trigger height and menu row height: xs 24, sm 32, md 40, lg 48 (default).</summary>
    public CdsSize Size
    {
        get => (CdsSize)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    /// <summary>
    /// Where the menu opens. <see cref="CdsMenuAlignment.Bottom"/> (default) and <see cref="CdsMenuAlignment.Top"/> make it at least as
    /// wide as the trigger; the <c>Start</c>/<c>End</c> variants use the menu's own width aligned to that edge.
    /// </summary>
    public CdsMenuAlignment MenuAlignment
    {
        get => (CdsMenuAlignment)GetValue(MenuAlignmentProperty);
        set => SetValue(MenuAlignmentProperty, value);
    }

    /// <summary>Draws a 1px outline around the menu.</summary>
    public bool HasMenuBorder
    {
        get => (bool)GetValue(HasMenuBorderProperty);
        set => SetValue(HasMenuBorderProperty, value);
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
        _menu.Label = Label;
        _menu.HasBorder = HasMenuBorder;
        _menu.OpenCore(_face, MenuAlignment.ToPlacement(), MenuAlignment.FillsTrigger() ? _face.Width : 0);
    }

    void Update()
    {
        var kind = Kind;
        var ghost = kind == CdsMenuButtonKind.Ghost;
        _face.Mode = ghost ? CdsActionFaceMode.Inline : CdsActionFaceMode.Pinned;
        _face.ButtonHeight = CdsMenuButtonTokens.Height(Size);
        _face.MinWidth = CdsMenuButtonTokens.MinimumWidth(kind);
        _face.Text = Label;
        _face.AccessibleName = Label;
        _face.RingStyle = CdsButtonTokens.FocusRing(CdsMenuButtonTokens.ToButtonKind(kind));
        _face.Resolver = state => CdsMenuButtonTokens.Resolve(kind, state);
        RefreshVisuals();
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
        if (propertyName == IsEnabledProperty.PropertyName && _face is not null) _face.IsEnabled = IsEnabled;
    }

    static void OnChanged(BindableObject b, object? o, object? n) => ((CdsMenuButton)b).Update();
}

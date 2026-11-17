using System.Windows.Input;

namespace Maui.Carbon;

/// <summary>
/// A split button: a primary action beside a chevron button that opens a <see cref="CdsMenu"/> of alternatives. Use it when one
/// action is used far more than its siblings (Deploy, with "Deploy to staging" in the menu); the primary action must not repeat
/// in the menu, and <see cref="CdsMenuItemKind.Danger"/> is for destructive alternatives only. Both halves are primary buttons of
/// the same size, separated by a 1px gap; disabling the control disables both. There are two tab stops: the action, then the chevron.
/// </summary>
[ContentProperty(nameof(Items))]
public class CdsComboButton : CdsControl
{
    /// <summary>Bindable property for <see cref="Label"/>.</summary>
    public static readonly BindableProperty LabelProperty = BindableProperty.Create(
        nameof(Label), typeof(string), typeof(CdsComboButton), null, propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="Size"/>.</summary>
    public static readonly BindableProperty SizeProperty = BindableProperty.Create(
        nameof(Size), typeof(CdsSize), typeof(CdsComboButton), CdsSize.Lg, propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="MenuAlignment"/>.</summary>
    public static readonly BindableProperty MenuAlignmentProperty = BindableProperty.Create(
        nameof(MenuAlignment), typeof(CdsMenuAlignment), typeof(CdsComboButton), CdsMenuAlignment.Bottom);

    /// <summary>Bindable property for <see cref="TriggerDescription"/>.</summary>
    public static readonly BindableProperty TriggerDescriptionProperty = BindableProperty.Create(
        nameof(TriggerDescription), typeof(string), typeof(CdsComboButton), "Additional actions", propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="IsOpen"/>.</summary>
    public static readonly BindableProperty IsOpenProperty = BindableProperty.Create(
        nameof(IsOpen), typeof(bool), typeof(CdsComboButton), false, BindingMode.TwoWay,
        propertyChanged: static (b, _, n) => ((CdsComboButton)b).OnIsOpenChanged((bool)n));

    /// <summary>Bindable property for <see cref="Command"/>.</summary>
    public static readonly BindableProperty CommandProperty = BindableProperty.Create(
        nameof(Command), typeof(ICommand), typeof(CdsComboButton), null, propertyChanged: OnCommandChanged);

    /// <summary>Bindable property for <see cref="CommandParameter"/>.</summary>
    public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(
        nameof(CommandParameter), typeof(object), typeof(CdsComboButton), null, propertyChanged: OnCommandChanged);

    readonly HorizontalStackLayout _row = new() { Spacing = CdsComboButtonTokens.Gap };
    readonly CdsActionFace _primary = new();
    readonly CdsActionFace _trigger = new();
    readonly CdsMenu _menu = new();
    bool _openPending;

    /// <summary>Creates a large combo button.</summary>
    public CdsComboButton()
    {
        HorizontalOptions = LayoutOptions.Start;
        VerticalOptions = LayoutOptions.Start;

        _primary.Mode = CdsActionFaceMode.Pinned;
        _primary.TruncateLabel = true;
        _primary.RingStyle = CdsFocusRingStyle.Button;
        _primary.Resolver = CdsComboButtonTokens.Resolve;
        _primary.MaximumWidthRequest = CdsComboButtonTokens.PrimaryMaxWidth;
        _primary.Activated += (_, _) => OnPrimaryActivated();

        _trigger.Mode = CdsActionFaceMode.IconOnly;
        _trigger.Icon = CdsIconName.ChevronDown;
        _trigger.RotateIconWhenOpen = true;
        _trigger.Expandable = true;
        _trigger.RingStyle = CdsFocusRingStyle.Button;
        _trigger.Resolver = CdsComboButtonTokens.Resolve;
        _trigger.Activated += (_, _) => Toggle();

        _row.Add(_primary);
        _row.Add(_trigger);

        _menu.Opened += (_, _) =>
        {
            _trigger.IsOpen = true;
            Opened?.Invoke(this, EventArgs.Empty);
        };
        _menu.Closed += (_, _) =>
        {
            _trigger.IsOpen = false;
            SetValue(IsOpenProperty, false);
            Closed?.Invoke(this, EventArgs.Empty);
        };
        SetChrome(_row);
        TrackFocus(_primary.Hit, () => _primary.Hit.IsFocusFromKeyboard);
        Loaded += (_, _) =>
        {
            if (_openPending) Dispatcher.Dispatch(() => OnIsOpenChanged(true));
        };
        Unloaded += (_, _) => _menu.Close();
        Update();
    }

    /// <summary>Raised when the primary action is activated, before <see cref="Command"/> runs.</summary>
    public event EventHandler? Clicked;

    /// <summary>Raised when the menu opens.</summary>
    public event EventHandler? Opened;

    /// <summary>Raised when the menu closes.</summary>
    public event EventHandler? Closed;

    /// <summary>The primary action's label (verb + noun, "Deploy"). Truncates with an ellipsis past 239px.</summary>
    public string? Label
    {
        get => (string?)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <summary>Height of both halves, width of the chevron half and menu row height: sm 32, md 40, lg 48 (default); xs 24 also renders.</summary>
    public CdsSize Size
    {
        get => (CdsSize)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    /// <summary>Where the menu opens relative to the two halves together. <c>Bottom</c> (default) and <c>Top</c> make it at least as wide as both.</summary>
    public CdsMenuAlignment MenuAlignment
    {
        get => (CdsMenuAlignment)GetValue(MenuAlignmentProperty);
        set => SetValue(MenuAlignmentProperty, value);
    }

    /// <summary>Accessible name, tooltip and menu name of the chevron half. Default "Additional actions".</summary>
    public string? TriggerDescription
    {
        get => (string?)GetValue(TriggerDescriptionProperty);
        set => SetValue(TriggerDescriptionProperty, value);
    }

    /// <summary>Whether the menu is open. Two-way; setting it before the control is on screen opens the menu once it loads.</summary>
    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    /// <summary>Executed with the primary action; <c>CanExecute</c> drives <c>IsEnabled</c> of the whole control.</summary>
    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    /// <summary>Parameter for <see cref="Command"/>.</summary>
    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    /// <summary>The alternative actions (the content property).</summary>
    public IList<CdsMenuItemBase> Items => _menu.Items;

    /// <summary>The underlying menu, for reuse and advanced settings.</summary>
    public CdsMenu Menu => _menu;

    void OnPrimaryActivated()
    {
        Clicked?.Invoke(this, EventArgs.Empty);
        ExecuteCommand();
    }

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
        if (!_trigger.IsLoaded || !IsEnabled)
        {
            _openPending = true;
            return;
        }
        _openPending = false;
        _menu.Size = Size;
        _menu.Label = TriggerDescription;
        _menu.OpenCore(_row, MenuAlignment.ToPlacement(), MenuAlignment.FillsTrigger() ? _row.Width : 0, returnFocusTo: _trigger);
    }

    void Update()
    {
        _primary.ButtonHeight = CdsComboButtonTokens.Side(Size);
        _primary.MinWidth = CdsComboButtonTokens.PrimaryMinimumWidth(Size);
        _primary.Text = Label;
        _primary.AccessibleName = Label;
        _trigger.ButtonHeight = CdsComboButtonTokens.Side(Size);
        _trigger.AccessibleName = TriggerDescription;
        if (!string.IsNullOrEmpty(TriggerDescription)) ToolTipProperties.SetText(_trigger.Hit, TriggerDescription);
        else _trigger.Hit.ClearValue(ToolTipProperties.TextProperty);
        RefreshVisuals();
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
        if (propertyName != IsEnabledProperty.PropertyName || _primary is null || _trigger is null) return;
        _primary.IsEnabled = IsEnabled;
        _trigger.IsEnabled = IsEnabled;
    }

    static void OnChanged(BindableObject b, object? o, object? n) => ((CdsComboButton)b).Update();

    static void OnCommandChanged(BindableObject b, object? o, object? n)
    {
        var button = (CdsComboButton)b;
        button.SetCommand(button.Command, button.CommandParameter);
    }
}

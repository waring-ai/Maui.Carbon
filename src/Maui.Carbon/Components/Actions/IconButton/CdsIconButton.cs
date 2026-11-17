using System.Windows.Input;

namespace Maui.Carbon;

/// <summary>
/// A square, icon-only button that always carries a tooltip label. Prefer it over an icon-only <see cref="CdsButton"/>
/// so the action is named for sighted users too. <see cref="Label"/> is the tooltip text and the accessible name.
/// Primary and ghost are the common kinds (ghost in toolbars). <see cref="IsSelected"/> makes it a toggle and
/// <see cref="BadgeCount"/> adds a count or dot badge (ghost, lg only by Carbon's rules). For destructive actions use a
/// <see cref="CdsButton"/> with a visible label; to copy a value use <see cref="CdsCopyButton"/>.
/// </summary>
public class CdsIconButton : CdsControl
{
    /// <summary>Bindable property for <see cref="Label"/>.</summary>
    public static readonly BindableProperty LabelProperty = BindableProperty.Create(
        nameof(Label), typeof(string), typeof(CdsIconButton), null, propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="Icon"/>.</summary>
    public static readonly BindableProperty IconProperty = BindableProperty.Create(
        nameof(Icon), typeof(CdsIconName?), typeof(CdsIconButton), null, propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="Kind"/>.</summary>
    public static readonly BindableProperty KindProperty = BindableProperty.Create(
        nameof(Kind), typeof(CdsIconButtonKind), typeof(CdsIconButton), CdsIconButtonKind.Primary, propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="Size"/>.</summary>
    public static readonly BindableProperty SizeProperty = BindableProperty.Create(
        nameof(Size), typeof(CdsSize), typeof(CdsIconButton), CdsSize.Lg, propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="TooltipPlacement"/>.</summary>
    public static readonly BindableProperty TooltipPlacementProperty = BindableProperty.Create(
        nameof(TooltipPlacement), typeof(CdsPlacement), typeof(CdsIconButton), CdsPlacement.Top, propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="IsSelected"/>.</summary>
    public static readonly BindableProperty IsSelectedProperty = BindableProperty.Create(
        nameof(IsSelected), typeof(bool?), typeof(CdsIconButton), null, BindingMode.TwoWay, propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="BadgeCount"/>.</summary>
    public static readonly BindableProperty BadgeCountProperty = BindableProperty.Create(
        nameof(BadgeCount), typeof(int?), typeof(CdsIconButton), null, propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="IsHighContrast"/>.</summary>
    public static readonly BindableProperty IsHighContrastProperty = BindableProperty.Create(
        nameof(IsHighContrast), typeof(bool), typeof(CdsIconButton), true, propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="HasDropShadow"/>.</summary>
    public static readonly BindableProperty HasDropShadowProperty = BindableProperty.Create(
        nameof(HasDropShadow), typeof(bool), typeof(CdsIconButton), false, propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="EnterDelay"/>.</summary>
    public static readonly BindableProperty EnterDelayProperty = BindableProperty.Create(
        nameof(EnterDelay), typeof(int), typeof(CdsIconButton), 100, propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="LeaveDelay"/>.</summary>
    public static readonly BindableProperty LeaveDelayProperty = BindableProperty.Create(
        nameof(LeaveDelay), typeof(int), typeof(CdsIconButton), 300, propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="Command"/>.</summary>
    public static readonly BindableProperty CommandProperty = BindableProperty.Create(
        nameof(Command), typeof(ICommand), typeof(CdsIconButton), null, propertyChanged: OnCommandChanged);

    /// <summary>Bindable property for <see cref="CommandParameter"/>.</summary>
    public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(
        nameof(CommandParameter), typeof(object), typeof(CdsIconButton), null, propertyChanged: OnCommandChanged);

    readonly CdsActionFace _face = new();
    readonly CdsIconTooltip _tooltip;

    /// <summary>Creates a large primary icon button.</summary>
    public CdsIconButton()
    {
        HorizontalOptions = LayoutOptions.Start;
        VerticalOptions = LayoutOptions.Start;
        _tooltip = new CdsIconTooltip(this, _face);
        _face.Activated += (_, _) => OnFaceActivated();
        _face.StateChanged += (_, _) =>
        {
            _tooltip.StateChanged(_face);
            CopyFaceState();
        };
        SetChrome(_face);
        TrackFocus(_face.Hit, () => _face.Hit.IsFocusFromKeyboard);
        Update();
    }

    /// <summary>Creates an icon button.</summary>
    public CdsIconButton(CdsIconName icon, string label, CdsIconButtonKind kind = CdsIconButtonKind.Primary) : this()
    {
        Icon = icon;
        Label = label;
        Kind = kind;
    }

    /// <summary>Raised on activation (click, tap, Enter or Space), before <see cref="Command"/> runs.</summary>
    public event EventHandler? Clicked;

    /// <summary>The tooltip text and accessible name: one or two words naming the action ("Download logs"), never the icon.</summary>
    public string? Label
    {
        get => (string?)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <summary>The 16px outline icon. Use the same icon for the same action across the product.</summary>
    public CdsIconName? Icon
    {
        get => (CdsIconName?)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>Visual emphasis. Default <see cref="CdsIconButtonKind.Primary"/>; ghost in toolbars.</summary>
    public CdsIconButtonKind Kind
    {
        get => (CdsIconButtonKind)GetValue(KindProperty);
        set => SetValue(KindProperty, value);
    }

    /// <summary>Square side: xs 24, sm 32, md 40, lg 48 (default). Larger values clamp to lg.</summary>
    public CdsSize Size
    {
        get => (CdsSize)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    /// <summary>Where the tooltip sits. Default <see cref="CdsPlacement.Top"/>.</summary>
    public CdsPlacement TooltipPlacement
    {
        get => (CdsPlacement)GetValue(TooltipPlacementProperty);
        set => SetValue(TooltipPlacementProperty, value);
    }

    /// <summary>
    /// <c>true</c>/<c>false</c> makes the button a toggle (selected fill <c>background-selected</c>, reported as pressed
    /// to screen readers) and flips on activation; <c>null</c> (default) is a plain button.
    /// </summary>
    public bool? IsSelected
    {
        get => (bool?)GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    /// <summary>
    /// Badge: <c>null</c> none, <c>0</c> a dot, a positive number its count. Carbon allows it only on ghost buttons at
    /// size lg; it is hidden while disabled.
    /// </summary>
    public int? BadgeCount
    {
        get => (int?)GetValue(BadgeCountProperty);
        set => SetValue(BadgeCountProperty, value);
    }

    /// <summary><c>background-inverse</c> tooltip (default) or a <c>layer-01</c> one.</summary>
    public bool IsHighContrast
    {
        get => (bool)GetValue(IsHighContrastProperty);
        set => SetValue(IsHighContrastProperty, value);
    }

    /// <summary>Adds the Carbon drop shadow to the tooltip (useful with <see cref="IsHighContrast"/> off).</summary>
    public bool HasDropShadow
    {
        get => (bool)GetValue(HasDropShadowProperty);
        set => SetValue(HasDropShadowProperty, value);
    }

    /// <summary>Milliseconds of hover before the tooltip opens. Default 100.</summary>
    public int EnterDelay
    {
        get => (int)GetValue(EnterDelayProperty);
        set => SetValue(EnterDelayProperty, value);
    }

    /// <summary>Milliseconds after the pointer leaves before the tooltip closes. Default 300.</summary>
    public int LeaveDelay
    {
        get => (int)GetValue(LeaveDelayProperty);
        set => SetValue(LeaveDelayProperty, value);
    }

    /// <summary>Executed on activation; <c>CanExecute</c> drives <c>IsEnabled</c>.</summary>
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

    /// <summary>True while the tooltip is shown.</summary>
    public bool IsTooltipOpen => _tooltip.IsOpen;

    void OnFaceActivated()
    {
        if (IsSelected is { } selected) IsSelected = !selected;
        Clicked?.Invoke(this, EventArgs.Empty);
        ExecuteCommand();
    }

    void CopyFaceState()
    {
        // The wrapper mirrors hover and focus so consumers can observe State.
        const CdsInteractionState mirrored = CdsInteractionState.Hover | CdsInteractionState.Pressed;
        SetState(mirrored, false);
        SetState(_face.State & mirrored, true);
    }

    void Update()
    {
        var kind = Kind;
        var selected = IsSelected;
        _face.Mode = CdsActionFaceMode.IconOnly;
        _face.ButtonHeight = CdsIconButtonTokens.Side(Size);
        _face.Icon = Icon;
        _face.AccessibleName = Label;
        _face.HitRole = selected is not null ? CdsAccessibilityRole.ToggleButton : CdsAccessibilityRole.Button;
        _face.IsSelected = selected == true;
        _face.BadgeCount = CdsIconButtonTokens.BadgeText(BadgeCount) is null ? null : BadgeCount;
        _face.RingStyle = selected == true && kind != CdsIconButtonKind.Ghost ? CdsFocusRingStyle.Inset : CdsIconButtonTokens.FocusRing(kind);
        _face.Resolver = state => CdsIconButtonTokens.Resolve(kind, state);

        _tooltip.Text = Label;
        _tooltip.Placement = TooltipPlacement;
        _tooltip.IsHighContrast = IsHighContrast;
        _tooltip.HasDropShadow = HasDropShadow;
        _tooltip.EnterDelay = EnterDelay;
        _tooltip.LeaveDelay = LeaveDelay;
        RefreshVisuals();
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
        if (propertyName == IsEnabledProperty.PropertyName && _face is not null) _face.IsEnabled = IsEnabled;
    }

    static void OnChanged(BindableObject b, object? o, object? n) => ((CdsIconButton)b).Update();

    static void OnCommandChanged(BindableObject b, object? o, object? n)
    {
        var button = (CdsIconButton)b;
        button.SetCommand(button.Command, button.CommandParameter);
    }
}

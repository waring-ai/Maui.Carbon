using Microsoft.Maui.Controls.Shapes;

namespace Maui.Carbon;

/// <summary>
/// A 48 x 48 icon button in the header's global bar (search, notifications, user, switcher). The 20px icon is
/// <c>icon-secondary</c> going to <c>icon-primary</c>; <see cref="Label"/> is its accessible name and tooltip (it
/// always names the action: "Notifications", "Open switcher"). While the action's panel is open set
/// <see cref="CdsShellItem.IsActive"/>: the button fills with <c>layer-01</c> and gets <c>border-subtle-00</c> side borders so it flows
/// into the <see cref="CdsHeaderPanel"/>. Focus is a 1px <c>focus</c> border.
/// </summary>
public class CdsHeaderGlobalAction : CdsShellItem
{
    /// <summary>Bindable property for <see cref="Label"/>.</summary>
    public static readonly BindableProperty LabelProperty = BindableProperty.Create(
        nameof(Label), typeof(string), typeof(CdsHeaderGlobalAction), null, propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="Icon"/>.</summary>
    public static readonly BindableProperty IconProperty = BindableProperty.Create(
        nameof(Icon), typeof(CdsIconName?), typeof(CdsHeaderGlobalAction), null, propertyChanged: OnChanged);

    readonly Grid _root = new() { WidthRequest = UIShellModel.HeaderHeight, HeightRequest = UIShellModel.HeaderHeight };
    readonly Border _box = new() { StrokeThickness = 1, StrokeShape = new Rectangle(), InputTransparent = true };
    readonly BoxView _left = new() { WidthRequest = 1, HorizontalOptions = LayoutOptions.Start, InputTransparent = true, IsVisible = false };
    readonly BoxView _right = new() { WidthRequest = 1, HorizontalOptions = LayoutOptions.End, InputTransparent = true, IsVisible = false };
    readonly CdsIcon _icon = new() { Size = CdsSizing.IconSize02, InputTransparent = true };
    readonly CdsHitTarget _hit;

    /// <summary>Creates an action.</summary>
    public CdsHeaderGlobalAction()
    {
        HorizontalOptions = LayoutOptions.Start;
        VerticalOptions = LayoutOptions.Fill;
        AutomationProperties.SetExcludedWithChildren(_box, true);
        AutomationProperties.SetExcludedWithChildren(_icon, true);
        _hit = CreateHitTarget(CdsAccessibilityRole.Button);
        _root.Add(_box);
        _root.Add(_left);
        _root.Add(_right);
        _root.Add(_icon);
        _root.Add(_hit);
        SetChrome(_root);
        Update();
    }

    /// <summary>Creates an action with an icon and a label.</summary>
    public CdsHeaderGlobalAction(CdsIconName icon, string label) : this()
    {
        Icon = icon;
        Label = label;
    }

    /// <summary>The accessible name and tooltip ("Notifications"). Required: the action has no visible text.</summary>
    public string? Label
    {
        get => (string?)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <summary>The 20px icon.</summary>
    public CdsIconName? Icon
    {
        get => (CdsIconName?)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <inheritdoc/>
    protected override void ApplyVisuals()
    {
        var v = CdsUIShellTokens.HeaderAction(State, IsActive);
        SetColor(_box, BackgroundColorProperty, v.Background);
        SetBrush(_box, Border.StrokeProperty, CdsUIShellTokens.HeaderActionBorder(State));
        SetColor(_icon, CdsIcon.ColorProperty, v.Icon);
        SetColor(_left, BoxView.ColorProperty, v.SideBorder);
        SetColor(_right, BoxView.ColorProperty, v.SideBorder);
        _left.IsVisible = _right.IsVisible = v.SideBorder is not null && !Is(CdsInteractionState.FocusVisible);
    }

    void Update()
    {
        _icon.Icon = Icon;
        SemanticProperties.SetDescription(_hit, Label);
        if (!string.IsNullOrEmpty(Label)) ToolTipProperties.SetText(_hit, Label);
        _hit.IsExpanded = IsActive ? true : null;
        RefreshVisuals();
    }

    /// <inheritdoc/>
    protected override void OnActiveChanged()
    {
        _hit.IsExpanded = IsActive ? true : null;
        base.OnActiveChanged();
    }

    static void OnChanged(BindableObject b, object? o, object? n) => ((CdsHeaderGlobalAction)b).Update();
}

/// <summary>
/// The right-aligned strip of <see cref="CdsHeaderGlobalAction"/>s at the end of the header. Utilities sit flush with no
/// gaps and the switcher is always last.
/// </summary>
public class CdsHeaderGlobalBar : HorizontalStackLayout
{
    /// <summary>Creates the bar.</summary>
    public CdsHeaderGlobalBar()
    {
        Spacing = 0;
        HorizontalOptions = LayoutOptions.End;
        VerticalOptions = LayoutOptions.Fill;
    }
}

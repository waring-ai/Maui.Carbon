using Microsoft.Maui.Controls.Shapes;

namespace Maui.Carbon;

/// <summary>
/// The hamburger at the start of the header that expands and collapses the side nav. Inside a <see cref="CdsUIShell"/> it
/// drives the shell's <see cref="CdsSideNav"/> and mirrors its state in <see cref="CdsShellItem.IsActive"/> (the button
/// shows the close icon while the side nav is open); on its own it just toggles <see cref="CdsShellItem.IsActive"/>. It is
/// hidden from the lg breakpoint (1056) up unless <see cref="IsCollapsible"/>. It looks and behaves like a
/// <see cref="CdsHeaderGlobalAction"/>: 48 x 48, 20px icon, accessible name from <see cref="Label"/>.
/// </summary>
public class CdsHeaderMenuButton : CdsShellItem
{
    /// <summary>Bindable property for <see cref="IsCollapsible"/>.</summary>
    public static readonly BindableProperty IsCollapsibleProperty = BindableProperty.Create(
        nameof(IsCollapsible), typeof(bool), typeof(CdsHeaderMenuButton), false,
        propertyChanged: static (b, _, _) => ((CdsHeaderMenuButton)b).Sync());

    /// <summary>Bindable property for <see cref="Label"/>.</summary>
    public static readonly BindableProperty LabelProperty = BindableProperty.Create(
        nameof(Label), typeof(string), typeof(CdsHeaderMenuButton), null,
        propertyChanged: static (b, _, _) => ((CdsHeaderMenuButton)b).Sync());

    readonly Grid _root = new() { WidthRequest = UIShellModel.HeaderHeight, HeightRequest = UIShellModel.HeaderHeight };
    readonly Border _box = new() { StrokeThickness = 1, StrokeShape = new Rectangle(), InputTransparent = true };
    readonly BoxView _left = new() { WidthRequest = 1, HorizontalOptions = LayoutOptions.Start, InputTransparent = true, IsVisible = false };
    readonly BoxView _right = new() { WidthRequest = 1, HorizontalOptions = LayoutOptions.End, InputTransparent = true, IsVisible = false };
    readonly CdsIcon _icon = new(CdsIconName.Menu, CdsSizing.IconSize02) { InputTransparent = true };
    readonly CdsHitTarget _hit;
    readonly CdsBreakpointWatcher _watcher;

    /// <summary>Creates a hamburger button.</summary>
    public CdsHeaderMenuButton()
    {
        HorizontalOptions = LayoutOptions.Start;
        VerticalOptions = LayoutOptions.Fill;
        AutomationProperties.SetExcludedWithChildren(_box, true);
        AutomationProperties.SetExcludedWithChildren(_icon, true);
        _hit = CreateHitTarget(CdsAccessibilityRole.ToggleButton);
        _root.Add(_box);
        _root.Add(_left);
        _root.Add(_right);
        _root.Add(_icon);
        _root.Add(_hit);
        SetChrome(_root);
        _watcher = new CdsBreakpointWatcher(this, Sync);
        Sync();
    }

    /// <summary>Keeps the button on screen from the lg breakpoint up (the side nav can then be collapsed to give room).</summary>
    public bool IsCollapsible
    {
        get => (bool)GetValue(IsCollapsibleProperty);
        set => SetValue(IsCollapsibleProperty, value);
    }

    /// <summary>The accessible name and tooltip. By default "Open menu" while the side nav is closed and "Close menu" while it is open.</summary>
    public string? Label
    {
        get => (string?)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <summary>True while the button is shown (below lg, or collapsible).</summary>
    public bool IsShown => UIShellModel.MenuButtonVisible(_watcher.Current, IsCollapsible);

    /// <inheritdoc/>
    protected override void OnItemActivated()
    {
        if (_watcher.Shell?.SideNav is CdsSideNav nav) nav.IsExpanded = !nav.IsExpanded;
        else IsActive = !IsActive;
    }

    /// <inheritdoc/>
    protected override void ApplyVisuals()
    {
        var v = CdsUIShellTokens.HeaderAction(State, IsActive);
        SetBrush(_box, BackgroundProperty, v.Background);
        SetBrush(_box, Border.StrokeProperty, CdsUIShellTokens.HeaderActionBorder(State));
        SetColor(_icon, CdsIcon.ColorProperty, v.Icon);
        // A null key would set a local Transparent value that a later dynamic resource cannot override, so keep a key and hide the view.
        SetColor(_left, BoxView.ColorProperty, v.SideBorder ?? CdsColorKeys.BorderSubtle01);
        SetColor(_right, BoxView.ColorProperty, v.SideBorder ?? CdsColorKeys.BorderSubtle01);
        _left.IsVisible = _right.IsVisible = v.SideBorder is not null && !Is(CdsInteractionState.FocusVisible);
    }

    void Sync()
    {
        if (_watcher is null) return;
        if (_watcher.Shell?.SideNav is CdsSideNav nav && IsActive != nav.IsExpanded) IsActive = nav.IsExpanded;
        _root.IsVisible = IsShown;
        _icon.Icon = IsActive ? CdsIconName.Close : CdsIconName.Menu;
        var name = Label ?? (IsActive ? "Close menu" : "Open menu");
        SemanticProperties.SetDescription(_hit, name);
        ToolTipProperties.SetText(_hit, name);
        _hit.IsChecked = IsActive;
        RefreshVisuals();
    }

    /// <inheritdoc/>
    protected override void OnActiveChanged()
    {
        Sync();
        base.OnActiveChanged();
    }
}

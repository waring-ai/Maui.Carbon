namespace Maui.Carbon;

/// <summary>Where a header link is drawn.</summary>
internal enum CdsShellPresentation
{
    /// <summary>In the header bar: 48 high, underline for the current link.</summary>
    Bar,
    /// <summary>A row of an open header menu: 48 high on <c>layer-01</c>, start bar for the current row.</summary>
    Dropdown,
    /// <summary>A row of the side nav (below lg): 32 high, start bar for the current row.</summary>
    SideNav,
}

/// <summary>
/// A link in the header: a child of <see cref="CdsHeaderNavigation"/> (a 48 high bar item, <c>text-secondary</c>, current link
/// <c>text-primary</c> with a 3px <c>border-interactive</c> underline), of a <see cref="CdsHeaderMenu"/> (a 48 high row of the
/// dropdown, current row <c>layer-selected-01</c> with a 3px start bar) or of <see cref="CdsHeaderSideNavItems"/> (a side nav row).
/// Header links are unique nouns describing the destination ("Billing"), never verbs, and never open new tabs. Mark the
/// current one with <see cref="CdsShellItem.IsActive"/>; handle <see cref="CdsShellItem.Clicked"/> to navigate.
/// </summary>
public class CdsHeaderMenuItem : CdsShellItem
{
    /// <summary>Bindable property for <see cref="Text"/>.</summary>
    public static readonly BindableProperty TextProperty = BindableProperty.Create(
        nameof(Text), typeof(string), typeof(CdsHeaderMenuItem), null,
        propertyChanged: static (b, _, _) => ((CdsHeaderMenuItem)b).Update());

    /// <summary>Bindable property for <see cref="Href"/>.</summary>
    public static readonly BindableProperty HrefProperty = BindableProperty.Create(
        nameof(Href), typeof(string), typeof(CdsHeaderMenuItem), null);

    readonly Grid _root = new();
    readonly BoxView _fill = new() { InputTransparent = true };
    readonly CdsText _label = new() { LineBreakMode = LineBreakMode.TailTruncation, MaxLines = 1, VerticalOptions = LayoutOptions.Center, InputTransparent = true };
    readonly BoxView _bar = new() { InputTransparent = true, IsVisible = false };
    readonly CdsFocusRing _ring = new(CdsFocusRingStyle.Inset);
    readonly CdsHitTarget _hit;
    CdsShellPresentation _presentation = CdsShellPresentation.Bar;
    bool _highlighted;
    bool _inMenu;

    /// <summary>Creates an empty link.</summary>
    public CdsHeaderMenuItem()
    {
        HorizontalOptions = LayoutOptions.Start;
        VerticalOptions = LayoutOptions.Fill;
        AutomationProperties.SetExcludedWithChildren(_label, true);
        _hit = CreateHitTarget(CdsAccessibilityRole.Link);
        _root.Add(_fill);
        _root.Add(_label);
        _root.Add(_bar);
        _root.Add(_ring);
        _root.Add(_hit);
        SetChrome(_root);
        Update();
    }

    /// <summary>Creates a link.</summary>
    public CdsHeaderMenuItem(string text, bool isActive = false) : this()
    {
        Text = text;
        IsActive = isActive;
    }

    /// <summary>The link text. One or two words.</summary>
    public string? Text
    {
        get => (string?)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>An absolute URI opened in the system browser on activation (optional; use <see cref="CdsShellItem.Clicked"/> for in-app navigation).</summary>
    public string? Href
    {
        get => (string?)GetValue(HrefProperty);
        set => SetValue(HrefProperty, value);
    }

    internal CdsShellPresentation Presentation
    {
        get => _presentation;
        set
        {
            if (_presentation == value) return;
            _presentation = value;
            Update();
        }
    }

    /// <summary>The item sits inside a menu group of the side nav (indented 68, regular weight).</summary>
    internal bool InSideNavMenu
    {
        get => _inMenu;
        set
        {
            _inMenu = value;
            Update();
        }
    }

    /// <summary>The hosting menu's virtual focus is on this row.</summary>
    internal bool IsHighlighted
    {
        get => _highlighted;
        set
        {
            if (_highlighted == value) return;
            _highlighted = value;
            RefreshVisuals();
        }
    }

    /// <summary>The highlight came from the keyboard: show the inset focus ring on the highlighted row.</summary>
    internal bool ShowHighlightRing { get; set; }

    /// <inheritdoc/>
    protected override void OnActiveChanged()
    {
        _hit.CurrentKind = IsActive ? CdsCurrentKind.Page : CdsCurrentKind.None;
        base.OnActiveChanged();
    }

    /// <summary>Whether Tab can focus the item (a dropdown row keeps focus on its trigger).</summary>
    internal void SetTabStop(bool value) => _hit.IsTabStop = value;

    /// <inheritdoc/>
    protected override void OnItemActivated()
    {
        if (!string.IsNullOrEmpty(Href) && Uri.TryCreate(Href, UriKind.Absolute, out var uri))
            _ = Launcher.Default.OpenAsync(uri);
    }

    /// <inheritdoc/>
    protected override void ApplyVisuals()
    {
        var dropdown = _presentation == CdsShellPresentation.Dropdown;
        var v = _presentation switch
        {
            CdsShellPresentation.Dropdown => CdsUIShellTokens.HeaderMenuRow(State, IsActive, _highlighted),
            CdsShellPresentation.SideNav => CdsUIShellTokens.SideNavRow(State, false) with { Bar = IsActive ? CdsColorKeys.BorderInteractive : null, Foreground = IsActive ? CdsColorKeys.TextPrimary : CdsUIShellTokens.SideNavRow(State, false).Foreground },
            _ => CdsUIShellTokens.HeaderMenuItem(State, IsActive),
        };
        SetColor(_fill, BoxView.ColorProperty, v.Background ?? CdsColorKeys.Layer01);
        _fill.IsVisible = v.Background is not null;
        SetColor(_label, Label.TextColorProperty, v.Foreground);
        SetColor(_bar, BoxView.ColorProperty, v.Bar ?? CdsColorKeys.BorderInteractive);
        _bar.IsVisible = v.Bar is not null;
        _ring.IsActive = Is(CdsInteractionState.FocusVisible) || (dropdown && _highlighted && ShowHighlightRing);
    }

    void Update()
    {
        _label.Text = Text;
        SemanticProperties.SetDescription(_hit, Text);
        _hit.CurrentKind = IsActive ? CdsCurrentKind.Page : CdsCurrentKind.None;
        switch (_presentation)
        {
            case CdsShellPresentation.Bar:
                _root.HeightRequest = -1;
                _label.TypeStyle = CdsTypeStyle.BodyCompact01;
                _label.Margin = new Thickness(CdsSpacing.Spacing05, 0);
                _bar.HeightRequest = 3;
                _bar.WidthRequest = -1;
                _bar.HorizontalOptions = LayoutOptions.Fill;
                _bar.VerticalOptions = LayoutOptions.End;
                break;
            case CdsShellPresentation.Dropdown:
                _root.HeightRequest = UIShellModel.HeaderMenuItemHeight;
                _label.TypeStyle = CdsTypeStyle.BodyCompact01;
                _label.Margin = new Thickness(CdsSpacing.Spacing05, 0);
                _bar.HeightRequest = -1;
                _bar.WidthRequest = 3;
                _bar.HorizontalOptions = LayoutOptions.Start;
                _bar.VerticalOptions = LayoutOptions.Fill;
                break;
            default:
                _root.HeightRequest = UIShellModel.SideNavRowHeight;
                _label.TypeStyle = _inMenu ? CdsTypeStyle.BodyCompact01 : CdsTypeStyle.HeadingCompact01;
                _label.Margin = new Thickness(_inMenu ? 68 : CdsSpacing.Spacing05, 0, CdsSpacing.Spacing05, 0);
                _bar.HeightRequest = -1;
                _bar.WidthRequest = 3;
                _bar.HorizontalOptions = LayoutOptions.Start;
                _bar.VerticalOptions = LayoutOptions.Fill;
                break;
        }
        HorizontalOptions = _presentation == CdsShellPresentation.Bar ? LayoutOptions.Start : LayoutOptions.Fill;
        RefreshVisuals();
    }
}

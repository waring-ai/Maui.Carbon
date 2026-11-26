using Microsoft.Maui.Controls.Shapes;

namespace Maui.Carbon;

/// <summary>
/// Triggers an action. Use one <see cref="CdsButtonKind.Primary"/> button per view, <c>Secondary</c> or <c>Tertiary</c>
/// for supporting actions, <c>Ghost</c> for low emphasis and the danger kinds for destructive steps. Labels are
/// left-aligned; the icon is pinned to the end edge. Buttons are square and at most 320 wide, except that
/// <c>HorizontalOptions="Fill"</c> makes a full-width button (login forms, modal footers) with no maximum.
/// </summary>
public partial class CdsButton : CdsControl
{
    const double MaxWidth = 320;

    readonly Grid _root = new();
    readonly Border _box = new() { StrokeThickness = 1, StrokeShape = new Rectangle(), Padding = 0 };
    readonly Grid _layout = new() { ColumnSpacing = 0 };
    readonly CdsText _label = new() { LineBreakMode = LineBreakMode.WordWrap };
    readonly CdsIcon _icon = new() { IsVisible = false };
    readonly CdsHitTarget _hit;
    readonly CdsButtonOutline _outline = new();
    CdsFocusRing _ring;

    /// <summary>Creates a large primary button.</summary>
    public CdsButton()
    {
        _layout.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        _layout.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        _layout.Add(_label, 0);
        _layout.Add(_icon, 1);
        _box.Content = _layout;
        _box.InputTransparent = true;
        AutomationProperties.SetExcludedWithChildren(_box, true);

        _hit = CreateHitTarget();
        _ring = new CdsFocusRing(CdsFocusRingStyle.Button);
        _root.Add(_box);
        _root.Add(_outline);
        _root.Add(_ring);
        _root.Add(_hit);
        HorizontalOptions = LayoutOptions.Start;
        UpdateMaxWidth();

        SetChrome(_root);
        UpdateLayout();
    }

    /// <summary>Creates a button with a label.</summary>
    public CdsButton(string text, CdsButtonKind kind = CdsButtonKind.Primary) : this()
    {
        Text = text;
        Kind = kind;
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
        if (propertyName == HorizontalOptionsProperty.PropertyName) UpdateMaxWidth();
    }

    // A button is at most 20rem wide (inline-size: max-content; max-inline-size: 20rem). An app that fills a button asks for a
    // full-width one (Carbon's inline-size: 100%; max-inline-size: none in login forms, modal footers and fluid sets): the
    // cap would stop it short and MAUI would centre it in the space.
    void UpdateMaxWidth() =>
        _root.MaximumWidthRequest = HorizontalOptions.Alignment == LayoutAlignment.Fill ? double.PositiveInfinity : MaxWidth;

    /// <summary>Raised on activation (click, tap, Enter or Space), before <see cref="Command"/> runs.</summary>
    public event EventHandler? Clicked;

    /// <inheritdoc/>
    protected override void OnActivated()
    {
        if (IsSelected is { } selected) IsSelected = !selected;
        Clicked?.Invoke(this, EventArgs.Empty);
        ExecuteCommand();
        if (!string.IsNullOrEmpty(Href) && Uri.TryCreate(Href, UriKind.Absolute, out var uri))
            _ = Launcher.Default.OpenAsync(uri);
    }

    /// <inheritdoc/>
    protected override void ApplyVisuals()
    {
        var state = State;
        if (IsSelected == true) state |= CdsInteractionState.Selected;
        var v = CdsButtonTokens.Resolve(Kind, state);
        // The fill goes on the root, which covers the whole box: on Mac Catalyst a Border paints its background and stroke
        // half a point inside its frame, so a filled face drew 1pt smaller than its layout box (a 2pt gap in combo buttons).
        SetColor(_root, BackgroundColorProperty, v.Background);
        // The outline is drawn by hairlines on the box edge, for the same reason; the Border keeps its 1px (transparent)
        // stroke so the label and icon keep their inset.
        SetBrush(_box, Border.StrokeProperty, null);
        SetColor(_outline, CdsButtonOutline.ColorProperty, v.Border);
        SetColor(_label, Label.TextColorProperty, v.Foreground);
        SetColor(_icon, CdsIcon.ColorProperty, v.Icon);
        _ring.IsActive = Is(CdsInteractionState.FocusVisible);
    }

    void UpdateLayout()
    {
        var height = Size.Height();
        var iconOnly = IsIconOnly && Icon is not null;
        var ghost = Kind is CdsButtonKind.Ghost or CdsButtonKind.DangerGhost;
        var lineHeight = IsExpressive ? 22 : 18;
        var iconSize = IsExpressive ? 20 : 16;

        _label.TypeStyle = IsExpressive ? CdsTypeStyle.BodyCompact02 : CdsTypeStyle.BodyCompact01;
        _label.Text = Text;
        _label.IsVisible = !iconOnly;
        _icon.Icon = Icon;
        _icon.Size = iconSize;
        _icon.IsVisible = Icon is not null;

        // Carbon pads the label so it is centered up to lg and top-aligned for xl/2xl.
        var padTop = Math.Min((height - lineHeight) / 2 - 1, 14);
        var iconTop = Math.Min((height - iconSize) / 2 - 1, 14) + 1;

        _root.MinimumHeightRequest = height;
        if (iconOnly)
        {
            _root.WidthRequest = height;
            _root.HeightRequest = height;
            Grid.SetColumn(_icon, 0);
            Grid.SetColumnSpan(_icon, 2);
            _icon.HorizontalOptions = LayoutOptions.Center;
            _icon.VerticalOptions = LayoutOptions.Center;
            _icon.Margin = 0;
        }
        else
        {
            _root.WidthRequest = -1;
            _root.HeightRequest = -1;
            _label.VerticalOptions = LayoutOptions.Start;
            if (ghost)
            {
                // Ghost: icon sits 8 after the label, 15 end padding.
                _label.Margin = new Thickness(15, padTop, Icon is null ? 15 : 0, padTop);
                Grid.SetColumn(_icon, 1);
                Grid.SetColumnSpan(_icon, 1);
                _icon.HorizontalOptions = LayoutOptions.Start;
                _icon.Margin = new Thickness(8, iconTop - 1, 15, 0);
            }
            else
            {
                // Others: 63 end padding reserves the icon column; the icon is pinned 16 from the end edge.
                _label.Margin = new Thickness(15, padTop, 63, padTop);
                Grid.SetColumn(_icon, 0);
                Grid.SetColumnSpan(_icon, 2);
                _icon.HorizontalOptions = LayoutOptions.End;
                _icon.Margin = new Thickness(0, iconTop - 1, 16 - 1, 0);
            }
            _icon.VerticalOptions = LayoutOptions.Start;
        }

        // Focus ring variant depends on the kind.
        var ringStyle = CdsButtonTokens.FocusRing(Kind);
        if (_root.Children.IndexOf(_ring) is var index and >= 0 && _ringStyle != ringStyle)
        {
            _root.Children.RemoveAt(index);
            _ring = new CdsFocusRing(ringStyle);
            _root.Children.Insert(index, _ring);
            _ringStyle = ringStyle;
        }

        // Accessibility: icon-only buttons are named by their description; toggles report pressed state.
        var name = iconOnly ? IconDescription : Text;
        SemanticProperties.SetDescription(_hit, name);
        if (iconOnly && !string.IsNullOrEmpty(IconDescription)) ToolTipProperties.SetText(_hit, IconDescription);
        else _hit.ClearValue(ToolTipProperties.TextProperty);
        _hit.Role = !string.IsNullOrEmpty(Href) ? CdsAccessibilityRole.Link
            : IsSelected is not null ? CdsAccessibilityRole.ToggleButton
            : CdsAccessibilityRole.Button;
        _hit.IsChecked = IsSelected == true;

        RefreshVisuals();
    }

    CdsFocusRingStyle _ringStyle = CdsFocusRingStyle.Button;
}

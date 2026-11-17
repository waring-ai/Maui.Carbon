using Microsoft.Maui.Controls.Shapes;

namespace Maui.Carbon;

/// <summary>
/// A 24px icon button inside a list-box field: the combo box clear (x) button. It is not a tab stop (the field handles the
/// keyboard) and has no hover fill in Carbon; it dims when the field is disabled or read only.
/// </summary>
internal sealed class CdsListBoxIconButton : CdsControl
{
    readonly Grid _root = new() { WidthRequest = CdsListBoxTokens.ButtonSize, HeightRequest = CdsListBoxTokens.ButtonSize, VerticalOptions = LayoutOptions.Center };
    readonly CdsIcon _icon;
    readonly CdsHitTarget _hit;
    bool _dimmed;

    public CdsListBoxIconButton(CdsIconName icon, double iconSize)
    {
        _icon = new CdsIcon(icon, iconSize);
        AutomationProperties.SetExcludedWithChildren(_icon, true);
        _hit = CreateHitTarget();
        _hit.IsTabStop = false;
        _root.Add(_icon);
        _root.Add(_hit);
        SetChrome(_root);
        ApplyVisuals();
    }

    /// <summary>Raised when the button is activated.</summary>
    public event EventHandler? Clicked;

    /// <summary>The accessible name.</summary>
    public string? Description
    {
        set => SemanticProperties.SetDescription(_hit, value);
    }

    /// <summary>Dims the icon and ignores presses (disabled and read-only fields).</summary>
    public bool Dimmed
    {
        get => _dimmed;
        set
        {
            _dimmed = value;
            _hit.InputTransparent = value;
            RefreshVisuals();
        }
    }

    /// <inheritdoc/>
    protected override void OnActivated() => Clicked?.Invoke(this, EventArgs.Empty);

    /// <inheritdoc/>
    protected override void ApplyVisuals() =>
        SetColor(_icon, CdsIcon.ColorProperty, CdsListBoxTokens.FieldIcon(Is(CdsInteractionState.Disabled), _dimmed));
}

/// <summary>
/// The multi-select's selection tag: a high-contrast pill holding the count and an x that clears every selection
/// (<c>background-inverse</c> / <c>text-inverse</c>, x hover <c>button-secondary-hover</c>). The x is not a tab stop; Delete on
/// the field clears from the keyboard.
/// </summary>
internal sealed class CdsListBoxSelectionTag : CdsControl
{
    readonly Border _pill = new() { StrokeThickness = 0, StrokeShape = new RoundRectangle { CornerRadius = CdsRadius.Tag }, MinimumWidthRequest = 32, HeightRequest = 24, VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Start };
    readonly CdsText _count = new(string.Empty, CdsTypeStyle.Label01) { VerticalOptions = LayoutOptions.Center };
    readonly Border _closeCircle = new() { StrokeThickness = 0, StrokeShape = new RoundRectangle { CornerRadius = 12 }, WidthRequest = 24, HeightRequest = 24, VerticalOptions = LayoutOptions.Center };
    readonly CdsIcon _closeIcon = new(CdsIconName.Close, 13);
    readonly CdsHitTarget _closeHit;
    bool _closeHover;
    bool _disabled;

    public CdsListBoxSelectionTag()
    {
        var row = new Grid { ColumnSpacing = CdsSpacing.Spacing02, Padding = new Thickness(CdsSpacing.Spacing03, 0, 0, 0) };
        row.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        row.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        row.Add(_count, 0);

        var close = new Grid { WidthRequest = 24, HeightRequest = 24, VerticalOptions = LayoutOptions.Center };
        _closeCircle.InputTransparent = true;
        AutomationProperties.SetExcludedWithChildren(_closeCircle, true);
        AutomationProperties.SetExcludedWithChildren(_closeIcon, true);
        _closeHit = CreateHitTarget();
        _closeHit.IsTabStop = false;
        var pointer = new PointerGestureRecognizer();
        pointer.PointerEntered += (_, _) => SetCloseHover(true);
        pointer.PointerExited += (_, _) => SetCloseHover(false);
        close.GestureRecognizers.Add(pointer);
        close.Add(_closeCircle);
        close.Add(_closeIcon);
        close.Add(_closeHit);
        row.Add(close, 1);

        // A tap on the count (anywhere but the x) belongs to the field: it toggles the list.
        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => BodyTapped?.Invoke(this, EventArgs.Empty);
        _pill.GestureRecognizers.Add(tap);

        _pill.Content = row;
        SetChrome(_pill);
        ApplyVisuals();
    }

    /// <summary>Raised when the x is activated.</summary>
    public event EventHandler? Cleared;

    /// <summary>Raised when the pill (not the x) is tapped.</summary>
    public event EventHandler? BodyTapped;

    /// <summary>The number shown.</summary>
    public int Count
    {
        set => _count.Text = value.ToString(System.Globalization.CultureInfo.CurrentCulture);
    }

    /// <summary>The accessible name of the x.</summary>
    public string? Description
    {
        set => SemanticProperties.SetDescription(_closeHit, value);
    }

    /// <summary>Disabled and read-only fields show a muted tag and ignore the x.</summary>
    public bool Muted
    {
        get => _disabled;
        set
        {
            _disabled = value;
            _closeHit.InputTransparent = value;
            RefreshVisuals();
        }
    }

    /// <inheritdoc/>
    protected override void OnActivated() => Cleared?.Invoke(this, EventArgs.Empty);

    void SetCloseHover(bool on)
    {
        _closeHover = on;
        RefreshVisuals();
    }

    /// <inheritdoc/>
    protected override void ApplyVisuals()
    {
        var v = CdsListBoxTokens.ResolveTag(_disabled || Is(CdsInteractionState.Disabled));
        SetColor(_pill, BackgroundColorProperty, v.Background);
        SetColor(_count, Label.TextColorProperty, v.Text);
        SetColor(_closeIcon, CdsIcon.ColorProperty, v.Icon);
        SetColor(_closeCircle, BackgroundColorProperty, _closeHover ? v.CloseHover : null);
    }
}

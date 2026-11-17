using Microsoft.Maui.Controls.Shapes;

namespace Maui.Carbon;

/// <summary>
/// A tag with a close button, used for applied filters or chosen values the person can remove. The tag does not hide
/// itself: handle <see cref="Dismissed"/> and remove it from your own state, then move focus somewhere sensible.
/// Only the close button is focusable. Its accessible name is <see cref="DismissLabel"/> followed by the tag text
/// ("Remove filter Region: us-east"), so a row of tags stays distinguishable. For labels that must always stay use
/// <see cref="CdsTag"/>.
/// </summary>
public class CdsDismissibleTag : CdsTag
{
    /// <summary>Bindable property for <see cref="DismissLabel"/>.</summary>
    public static readonly BindableProperty DismissLabelProperty = BindableProperty.Create(
        nameof(DismissLabel), typeof(string), typeof(CdsDismissibleTag), "Dismiss",
        propertyChanged: static (b, _, _) => ((CdsDismissibleTag)b).UpdateClose());

    readonly Grid _closeRoot = new();
    readonly Border _hoverCircle = new() { StrokeThickness = 0, InputTransparent = true };
    readonly Border _focusCircle = new() { StrokeThickness = 1, InputTransparent = true, IsVisible = false, BackgroundColor = Colors.Transparent };
    readonly CdsIcon _closeIcon = new(CdsIconName.Close, 16) { InputTransparent = true };
    CdsHitTarget? _closeHit;
    CdsIcon _iconView = null!;
    CdsText _labelView = null!;
    bool _closeHover;

    /// <summary>Creates a medium gray dismissible tag.</summary>
    public CdsDismissibleTag()
    {
        UpdateClose();
    }

    /// <summary>Creates a dismissible tag.</summary>
    public CdsDismissibleTag(string text, CdsTagType type = CdsTagType.Gray) : this()
    {
        Text = text;
        Type = type;
    }

    /// <summary>Raised when the close button is activated (click, tap, Enter or Space). Remove the tag from your state here.</summary>
    public event EventHandler? Dismissed;

    /// <summary>The close button's label and tooltip, said before the tag text. Default "Dismiss"; "Remove filter" reads better for filters.</summary>
    public string DismissLabel
    {
        get => (string)GetValue(DismissLabelProperty);
        set => SetValue(DismissLabelProperty, value);
    }

    /// <inheritdoc/>
    protected override bool HasTrailingContent => true;

    /// <inheritdoc/>
    protected override View BuildChrome(Border pill)
    {
        var grid = CdsTagParts.Rebuild(pill, Row, out _iconView, out _labelView);
        AutomationProperties.SetExcludedWithChildren(_hoverCircle, true);

        _closeHit = CreateHitTarget();
        _closeRoot.Add(_hoverCircle);
        _closeRoot.Add(_closeIcon);
        _closeRoot.Add(_focusCircle);
        _closeRoot.Add(_closeHit);
        _closeRoot.VerticalOptions = LayoutOptions.Center;
        var pointer = new PointerGestureRecognizer();
        pointer.PointerEntered += (_, _) => SetCloseHover(true);
        pointer.PointerExited += (_, _) => SetCloseHover(false);
        _closeRoot.GestureRecognizers.Add(pointer);
        grid.Add(_closeRoot, 2);
        return pill;
    }

    /// <inheritdoc/>
    protected override void OnActivated() => Dismissed?.Invoke(this, EventArgs.Empty);

    /// <inheritdoc/>
    protected override void ApplyVisuals()
    {
        if (_iconView is null) return;
        var v = CurrentVisuals;
        SetColor(Pill, BackgroundColorProperty, v.Background);
        SetBrush(Pill, Border.StrokeProperty, v.Border);
        Pill.StrokeThickness = v.Border is null ? 0 : 1;
        SetColor(_labelView, Label.TextColorProperty, v.Foreground);
        SetColor(_iconView, CdsIcon.ColorProperty, v.Foreground);

        var close = CdsInteractiveTagTokens.Close(Type, _closeHover, Is(CdsInteractionState.Disabled));
        SetColor(_hoverCircle, BackgroundColorProperty, close.Hover);
        SetColor(_closeIcon, CdsIcon.ColorProperty, close.Icon);
        SetBrush(_focusCircle, Border.StrokeProperty, close.Focus);
        _focusCircle.IsVisible = Is(CdsInteractionState.FocusVisible);
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
        if (propertyName is nameof(Text) or nameof(Size)) UpdateClose();
    }

    void SetCloseHover(bool hover)
    {
        if (_closeHover == hover) return;
        _closeHover = hover;
        RefreshVisuals();
    }

    void UpdateClose()
    {
        if (_closeHit is null) return;
        var height = CdsTagTokens.Height(Size);
        _closeRoot.WidthRequest = height;
        _closeRoot.HeightRequest = height;
        _closeRoot.Margin = new Thickness(Size == CdsTagSize.Sm ? 5 : 2, 0, 0, 0);
        var circle = new RoundRectangle { CornerRadius = CdsRadius.Circle(height) };
        _hoverCircle.StrokeShape = circle;
        _focusCircle.StrokeShape = new RoundRectangle { CornerRadius = CdsRadius.Circle(height) };
        var name = string.IsNullOrEmpty(Text) ? DismissLabel : $"{DismissLabel} {Text}";
        SemanticProperties.SetDescription(_closeHit, name);
        ToolTipProperties.SetText(_closeHit, DismissLabel);
    }
}

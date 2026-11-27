using Microsoft.Maui.Controls.Shapes;

namespace Maui.Carbon;

/// <summary>The Carbon focus-ring variants.</summary>
public enum CdsFocusRingStyle
{
    /// <summary>2px <c>focus</c> outline inside the bounds (fields, tabs, tiles, menu rows).</summary>
    Inset,
    /// <summary>2px <c>focus</c> outline 1px outside the bounds (checkboxes, toggles, tags).</summary>
    Offset,
    /// <summary>Filled buttons: a 2px <c>focus</c> ring with a 1px <c>background</c> inner ring.</summary>
    Button,
}

/// <summary>
/// Draws the Carbon focus indicator. Place it in the same grid cell as the control's visuals, above them, and set
/// <see cref="IsActive"/> from <see cref="CdsInteractionState.FocusVisible"/> (or Focused for fields).
/// <para>
/// The outline's views are created the first time the ring shows: every focusable control has a ring, most are never
/// focused from the keyboard, and in a table each row has several.
/// </para>
/// </summary>
public sealed class CdsFocusRing : Grid
{
    readonly CdsFocusRingStyle _style;
    readonly double _cornerRadius;
    string _colorKey = CdsColorKeys.Focus;
    Border? _outer;

    /// <summary>Creates a focus ring.</summary>
    /// <param name="style">The variant.</param>
    /// <param name="cornerRadius">Corner radius of the control (0 for Carbon's square controls).</param>
    public CdsFocusRing(CdsFocusRingStyle style = CdsFocusRingStyle.Inset, double cornerRadius = 0)
    {
        _style = style;
        _cornerRadius = cornerRadius;
        InputTransparent = true;
        IsVisible = false;
        AutomationProperties.SetIsInAccessibleTree(this, false);
        if (style == CdsFocusRingStyle.Offset) Margin = new Thickness(-3);
    }

    /// <summary>Shows or hides the ring.</summary>
    public bool IsActive
    {
        get => IsVisible;
        set => IsVisible = value;
    }

    /// <summary>Whether the outline's views exist yet (for tests).</summary>
    internal bool IsDrawn => _outer is not null;

    /// <summary>Changes the ring color (for example <c>support-error</c> for an invalid field that is not focused).</summary>
    public void SetColorKey(string key)
    {
        _colorKey = key;
        if (_outer is not null) CdsTokens.SetBrush(_outer, Border.StrokeProperty, key);
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
        // Also when a control sets IsVisible itself rather than IsActive.
        if (propertyName == IsVisibleProperty.PropertyName && IsVisible) Draw();
    }

    void Draw()
    {
        if (_outer is not null) return;
        var outerRadius = _style == CdsFocusRingStyle.Offset ? _cornerRadius + 3 : _cornerRadius;
        _outer = new Border
        {
            StrokeThickness = 2,
            BackgroundColor = Colors.Transparent,
            StrokeShape = new RoundRectangle { CornerRadius = outerRadius },
            InputTransparent = true,
        };
        CdsTokens.SetBrush(_outer, Border.StrokeProperty, _colorKey);
        Children.Add(_outer);

        if (_style == CdsFocusRingStyle.Button)
        {
            var inner = new Border
            {
                StrokeThickness = 1,
                Margin = new Thickness(2),
                BackgroundColor = Colors.Transparent,
                StrokeShape = new RoundRectangle { CornerRadius = Math.Max(0, _cornerRadius - 2) },
                InputTransparent = true,
            };
            CdsTokens.SetBrush(inner, Border.StrokeProperty, CdsColorKeys.Background);
            Children.Add(inner);
        }
    }
}

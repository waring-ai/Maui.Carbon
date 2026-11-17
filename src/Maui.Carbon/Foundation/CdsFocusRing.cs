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
/// </summary>
public sealed class CdsFocusRing : Grid
{
    readonly Border _outer;
    readonly Border? _inner;

    /// <summary>Creates a focus ring.</summary>
    /// <param name="style">The variant.</param>
    /// <param name="cornerRadius">Corner radius of the control (0 for Carbon's square controls).</param>
    public CdsFocusRing(CdsFocusRingStyle style = CdsFocusRingStyle.Inset, double cornerRadius = 0)
    {
        InputTransparent = true;
        IsVisible = false;
        AutomationProperties.SetIsInAccessibleTree(this, false);

        var outerRadius = style == CdsFocusRingStyle.Offset ? cornerRadius + 3 : cornerRadius;
        _outer = new Border
        {
            StrokeThickness = 2,
            BackgroundColor = Colors.Transparent,
            StrokeShape = new RoundRectangle { CornerRadius = outerRadius },
            InputTransparent = true,
        };
        CdsTokens.SetBrush(_outer, Border.StrokeProperty, CdsColorKeys.Focus);
        if (style == CdsFocusRingStyle.Offset) Margin = new Thickness(-3);
        Children.Add(_outer);

        if (style == CdsFocusRingStyle.Button)
        {
            _inner = new Border
            {
                StrokeThickness = 1,
                Margin = new Thickness(2),
                BackgroundColor = Colors.Transparent,
                StrokeShape = new RoundRectangle { CornerRadius = Math.Max(0, cornerRadius - 2) },
                InputTransparent = true,
            };
            CdsTokens.SetBrush(_inner, Border.StrokeProperty, CdsColorKeys.Background);
            Children.Add(_inner);
        }
    }

    /// <summary>Shows or hides the ring.</summary>
    public bool IsActive
    {
        get => IsVisible;
        set => IsVisible = value;
    }

    /// <summary>Changes the ring color (for example <c>support-error</c> for an invalid field that is not focused).</summary>
    public void SetColorKey(string key) => CdsTokens.SetBrush(_outer, Border.StrokeProperty, key);
}

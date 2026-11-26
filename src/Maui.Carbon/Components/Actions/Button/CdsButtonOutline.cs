namespace Maui.Carbon;

/// <summary>
/// The 1px outline of a button face, drawn as four hairlines on the edges of its box (Carbon's
/// <c>border: 1px solid</c> with <c>box-sizing: border-box</c>). A <see cref="Border"/> stroke cannot be used: on Mac
/// Catalyst it is painted half a point inside its frame, so the outline sat inside the 48pt box. Place it in the same
/// grid cell as the face, above the fill and below the focus ring.
/// </summary>
internal sealed class CdsButtonOutline : Grid
{
    /// <summary>Bindable property for <see cref="Color"/>.</summary>
    public static readonly BindableProperty ColorProperty = BindableProperty.Create(
        nameof(Color), typeof(Color), typeof(CdsButtonOutline), null,
        propertyChanged: static (b, _, n) => ((CdsButtonOutline)b).OnColorChanged(n as Color));

    readonly BoxView[] _edges =
    [
        new() { HeightRequest = 1, VerticalOptions = LayoutOptions.Start },
        new() { HeightRequest = 1, VerticalOptions = LayoutOptions.End },
        new() { WidthRequest = 1, HorizontalOptions = LayoutOptions.Start },
        new() { WidthRequest = 1, HorizontalOptions = LayoutOptions.End },
    ];

    /// <summary>Creates an outline with no color (hidden).</summary>
    public CdsButtonOutline()
    {
        InputTransparent = true;
        IsVisible = false;
        AutomationProperties.SetExcludedWithChildren(this, true);
        foreach (var edge in _edges)
        {
            edge.InputTransparent = true;
            Children.Add(edge);
        }
    }

    /// <summary>The outline color; <c>null</c> or transparent hides it.</summary>
    public Color? Color
    {
        get => (Color?)GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    /// <summary>The four edges: top, bottom, start, end (for tests).</summary>
    internal IReadOnlyList<BoxView> Edges => _edges;

    void OnColorChanged(Color? color)
    {
        IsVisible = color is { Alpha: > 0 };
        foreach (var edge in _edges) edge.Color = color ?? Colors.Transparent;
    }
}

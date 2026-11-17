namespace Maui.Carbon;

/// <summary>
/// Pure logic of <see cref="CdsExpandableTile"/>: whether its content is interactive (which decides the variant) and
/// the height math of the expand and collapse animation.
/// </summary>
internal static class ExpandableTileModel
{
    /// <summary>
    /// True when the view tree contains something a person can interact with: a native button, field or switch, a Carbon
    /// control, or a label that reacts to taps (a link). Such content makes the tile the interactive variant, where only the
    /// chevron toggles it.
    /// </summary>
    public static bool ContainsInteractive(Element? root)
    {
        if (root is null) return false;
        if (IsInteractive(root)) return true;
        return CdsTileText.Descendants(root).Any(IsInteractive);
    }

    /// <summary>Whether one element is itself interactive.</summary>
    public static bool IsInteractive(Element element) => element switch
    {
        Microsoft.Maui.Controls.Button or ImageButton or Entry or Editor or Picker or DatePicker or TimePicker
            or Switch or Microsoft.Maui.Controls.CheckBox or Microsoft.Maui.Controls.Slider or Stepper or SearchBar
            or Microsoft.Maui.Controls.RadioButton or CdsControl => true,
        Label label => label.GestureRecognizers.OfType<TapGestureRecognizer>().Any(),
        _ => false,
    };

    /// <summary>The variant for content, honoring an explicit <paramref name="forced"/> override.</summary>
    public static bool UseInteractiveVariant(bool? forced, Element? above, Element? below) =>
        forced ?? (ContainsInteractive(above) || ContainsInteractive(below));

    /// <summary>The height of the tile at animation progress <paramref name="progress"/> (0 to 1) between two heights.</summary>
    public static double HeightAt(double from, double to, double progress) =>
        from + (to - from) * Math.Min(Math.Max(progress, 0), 1);

    /// <summary>The duration of the height animation: <c>duration-fast-02</c> when expanding, <c>duration-moderate-01</c> when collapsing.</summary>
    public static uint Duration(bool expanding) => expanding ? CdsDuration.Fast02 : CdsDuration.Moderate01;

    /// <summary>The accessible name of the toggle: the description for the state the tile is in.</summary>
    public static string Description(bool expanded, string? collapsedDescription, string? expandedDescription) =>
        expanded
            ? string.IsNullOrEmpty(expandedDescription) ? "Interact to collapse Tile" : expandedDescription
            : string.IsNullOrEmpty(collapsedDescription) ? "Interact to expand Tile" : collapsedDescription;
}

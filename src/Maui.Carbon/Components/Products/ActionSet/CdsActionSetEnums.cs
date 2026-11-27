namespace Maui.Carbon;

/// <summary>
/// The width class of a <see cref="CdsActionSet"/>: the size of the panel or page it closes. It decides whether the
/// buttons stack and how wide each one is.
/// </summary>
public enum CdsActionSetSize
{
    /// <summary>Narrow containers (side panels up to 320 wide): the buttons always stack, primary on top.</summary>
    Sm,
    /// <summary>Medium containers (480): one or two buttons in a row, three stack.</summary>
    Md,
    /// <summary>Large containers (640): up to three buttons in a row.</summary>
    Lg,
    /// <summary>Extra large containers (1040): buttons a quarter of the width, at most 232.</summary>
    Xl,
    /// <summary>The widest containers (1280, full-page flows): as <see cref="Xl"/>.</summary>
    Xxl,
}

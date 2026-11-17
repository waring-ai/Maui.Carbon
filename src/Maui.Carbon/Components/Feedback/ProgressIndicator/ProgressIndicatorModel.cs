namespace Maui.Carbon;

/// <summary>How a step of a <see cref="CdsProgressIndicator"/> is drawn.</summary>
public enum CdsProgressStepState
{
    /// <summary>Done: <c>checkmark--outline</c> in <c>interactive</c>.</summary>
    Complete,
    /// <summary>The step the person is on: <c>incomplete</c> in <c>interactive</c>.</summary>
    Current,
    /// <summary>Not reached yet: <c>circle-dash</c> in <c>icon-primary</c>.</summary>
    Incomplete,
    /// <summary>Needs fixing: <c>warning</c> in <c>support-error</c>.</summary>
    Invalid,
    /// <summary>Cannot be used: <c>circle-dash</c> in <c>icon-disabled</c> with a <c>text-disabled</c> label.</summary>
    Disabled,
}

/// <summary>Colors and icon of a step state.</summary>
/// <param name="Icon">The 16px icon.</param>
/// <param name="IconColor">The icon color key.</param>
/// <param name="LabelColor">The label color key.</param>
public readonly record struct CdsProgressStepVisuals(CdsIconName Icon, string IconColor, string LabelColor);

/// <summary>Maps step states to tokens.</summary>
public static class CdsProgressStepTokens
{
    /// <summary>Resolves a state: icon, icon color and label color.</summary>
    public static CdsProgressStepVisuals Resolve(CdsProgressStepState state) => state switch
    {
        CdsProgressStepState.Complete => new(CdsIconName.CheckmarkOutline, CdsColorKeys.Interactive, CdsColorKeys.TextPrimary),
        CdsProgressStepState.Current => new(CdsIconName.Incomplete, CdsColorKeys.Interactive, CdsColorKeys.TextPrimary),
        CdsProgressStepState.Invalid => new(CdsIconName.Warning, CdsColorKeys.SupportError, CdsColorKeys.TextPrimary),
        CdsProgressStepState.Disabled => new(CdsIconName.CircleDash, CdsColorKeys.IconDisabled, CdsColorKeys.TextDisabled),
        _ => new(CdsIconName.CircleDash, CdsColorKeys.IconPrimary, CdsColorKeys.TextPrimary),
    };

    /// <summary>The connector color: <c>interactive</c> for complete and current steps, <c>border-subtle-01</c> for the rest.</summary>
    public static string LineColor(bool reached) => reached ? CdsColorKeys.Interactive : CdsColorKeys.BorderSubtle01;

    /// <summary>The visually hidden status word read after the label.</summary>
    public static string StatusWord(CdsProgressStepState state) => state switch
    {
        CdsProgressStepState.Complete => "Complete",
        CdsProgressStepState.Current => "Current",
        CdsProgressStepState.Invalid => "Invalid",
        CdsProgressStepState.Disabled => "Disabled",
        _ => "Incomplete",
    };
}

/// <summary>The state logic of a progress indicator (pure, tested).</summary>
internal static class ProgressIndicatorModel
{
    /// <summary>
    /// The widest a label may be: 160 in a vertical indicator; horizontally unlimited, so the label takes what is left of the
    /// step (128, or its share when spaced equally) beside the icon and only then truncates.
    /// </summary>
    public static double LabelMaxWidth(bool vertical) => vertical ? 160 : double.PositiveInfinity;

    /// <summary>The state that follows from the position alone: earlier steps are complete, the current one is current.</summary>
    public static CdsProgressStepState Underlying(int index, int currentIndex, bool forceComplete) =>
        index == currentIndex ? CdsProgressStepState.Current
        : index < currentIndex || forceComplete ? CdsProgressStepState.Complete
        : CdsProgressStepState.Incomplete;

    /// <summary>The drawn state: disabled wins over invalid, which wins over the position.</summary>
    public static CdsProgressStepState Resolve(int index, int currentIndex, bool invalid, bool disabled, bool forceComplete)
    {
        if (disabled) return CdsProgressStepState.Disabled;
        if (invalid) return CdsProgressStepState.Invalid;
        return Underlying(index, currentIndex, forceComplete);
    }

    /// <summary>Whether the connector is drawn in <c>interactive</c>: the step is complete or current (by position, even when invalid or disabled).</summary>
    public static bool LineReached(int index, int currentIndex, bool forceComplete) =>
        Underlying(index, currentIndex, forceComplete) != CdsProgressStepState.Incomplete;

    /// <summary>Whether a step can be clicked: clicking is on, it is not the current step and not disabled.</summary>
    public static bool IsClickable(bool clickable, int index, int currentIndex, bool disabled) =>
        clickable && !disabled && index != currentIndex;
}

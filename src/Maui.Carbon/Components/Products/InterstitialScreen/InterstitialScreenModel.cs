namespace Maui.Carbon;

/// <summary>Which footer buttons an interstitial screen shows on a step, and whether start carries the arrow.</summary>
internal readonly record struct InterstitialFooterState(bool Skip, bool Back, bool Next, bool Start, bool StartHasIcon)
{
    /// <summary>The shown buttons, start to end (skip, back, next, start).</summary>
    public IEnumerable<CdsInterstitialScreenAction> Actions
    {
        get
        {
            if (Skip) yield return CdsInterstitialScreenAction.Skip;
            if (Back) yield return CdsInterstitialScreenAction.Back;
            if (Next) yield return CdsInterstitialScreenAction.Next;
            if (Start) yield return CdsInterstitialScreenAction.Start;
        }
    }
}

/// <summary>
/// Pure rules of <see cref="CdsInterstitialScreen"/>, from <c>InterstitialScreen*.tsx</c> and <c>_interstitial-screen.scss</c>:
/// step clamping and navigation, which footer buttons show, the footer widths and the header padding.
/// </summary>
internal static class InterstitialScreenModel
{
    /// <summary>The width of the back, next and start buttons from the md breakpoint (<c>spacing-11</c> × 3 = 240).</summary>
    public const double ActionButtonWidth = CdsSpacing.Spacing11 * 3;

    /// <summary>More than one step: the progress indicator, skip, back and next appear.</summary>
    public static bool IsMultiStep(int stepCount) => stepCount > 1;

    /// <summary>Keeps a step index inside <c>[0, stepCount - 1]</c> (0 when there are no steps).</summary>
    public static int ClampStep(int step, int stepCount) => stepCount <= 0 ? 0 : Math.Clamp(step, 0, stepCount - 1);

    /// <summary>
    /// The footer on a step: skip on every step of a multi-step screen (unless its label is empty), back after the first
    /// step, next before the last, start on the last (or only) step. Start shows the arrow on the last of several steps,
    /// except while its own action runs (the inline loading takes its place).
    /// </summary>
    public static InterstitialFooterState Footer(int step, int stepCount, bool hasSkipLabel, CdsInterstitialScreenAction? running = null)
    {
        var multi = IsMultiStep(stepCount);
        var last = Math.Max(stepCount - 1, 0);
        var current = ClampStep(step, stepCount);
        var onLast = current == last;
        return new(
            Skip: multi && hasSkipLabel,
            Back: multi && current > 0,
            Next: multi && current < last,
            Start: !multi || onLast,
            StartHasIcon: multi && onLast && running != CdsInterstitialScreenAction.Start);
    }

    /// <summary>Whether the button of an action shows the arrow (next always does, unless it is running; start per <see cref="Footer"/>).</summary>
    public static bool HasIcon(CdsInterstitialScreenAction action, InterstitialFooterState footer, CdsInterstitialScreenAction? running) => action switch
    {
        CdsInterstitialScreenAction.Next => running != CdsInterstitialScreenAction.Next,
        CdsInterstitialScreenAction.Start => footer.StartHasIcon,
        _ => false,
    };

    /// <summary>
    /// What an action does once the app let it through: next and back move one step (clamped); every other action closes
    /// the screen.
    /// </summary>
    public static (int Step, bool Close) Navigate(CdsInterstitialScreenAction action, int step, int stepCount) => action switch
    {
        CdsInterstitialScreenAction.Next => (ClampStep(step + 1, stepCount), false),
        CdsInterstitialScreenAction.Back => (ClampStep(step - 1, stepCount), false),
        _ => (ClampStep(step, stepCount), true),
    };

    /// <summary>The flag of an action's button (<see cref="CdsInterstitialScreenButtons.None"/> for close).</summary>
    public static CdsInterstitialScreenButtons ButtonOf(CdsInterstitialScreenAction action) => action switch
    {
        CdsInterstitialScreenAction.Skip => CdsInterstitialScreenButtons.Skip,
        CdsInterstitialScreenAction.Back => CdsInterstitialScreenButtons.Back,
        CdsInterstitialScreenAction.Next => CdsInterstitialScreenButtons.Next,
        CdsInterstitialScreenAction.Start => CdsInterstitialScreenButtons.Start,
        _ => CdsInterstitialScreenButtons.None,
    };

    /// <summary>The kind of an action's button: ghost skip, secondary back, primary next and start.</summary>
    public static CdsButtonKind KindOf(CdsInterstitialScreenAction action) => action switch
    {
        CdsInterstitialScreenAction.Skip => CdsButtonKind.Ghost,
        CdsInterstitialScreenAction.Back => CdsButtonKind.Secondary,
        _ => CdsButtonKind.Primary,
    };

    /// <summary>The live text read when the step changes (Carbon's visually hidden "Step 2 of 5").</summary>
    public static string StepAnnouncement(int step, int stepCount) => $"Step {ClampStep(step, stepCount) + 1} of {stepCount}";

    /// <summary>The label of a step in the progress indicator: its title, else "Step n".</summary>
    public static string StepTitle(string? title, int index) => string.IsNullOrWhiteSpace(title) ? $"Step {index + 1}" : title;

    /// <summary>The progress indicator shows for several steps unless hidden.</summary>
    public static bool ShowsProgress(int stepCount, bool hidden) => IsMultiStep(stepCount) && !hidden;

    /// <summary>Below the md breakpoint (42rem) the footer buttons share the width and the header padding tightens.</summary>
    public static bool IsCompact(double width) => width > 0 && width < CdsModalSizing.BreakpointMd;

    /// <summary>
    /// The header padding: 24 on top; 32 at the start (16 when compact); at the end the 48 of the close button in the
    /// modal variant, else the same as the start.
    /// </summary>
    public static Thickness HeaderPadding(bool compact, bool modal)
    {
        var side = compact ? CdsSpacing.Spacing05 : CdsSpacing.Spacing07;
        return new Thickness(side, CdsSpacing.Spacing06, modal ? CdsModalSizing.CloseButtonSize : side, 0);
    }

    /// <summary>
    /// The footer columns for a width: below md every button takes an equal share; from md back, next and start are 240
    /// each (shrunk equally if the row is too narrow) and skip takes the rest, at the start. Returns each shown button's
    /// width, in order, and the empty space before the first one.
    /// </summary>
    public static (double Lead, double[] Widths) FooterWidths(double width, InterstitialFooterState footer)
    {
        var actions = footer.Actions.ToArray();
        if (actions.Length == 0 || width <= 0) return (Math.Max(width, 0), new double[actions.Length]);
        if (IsCompact(width))
        {
            var share = width / actions.Length;
            return (0, actions.Select(_ => share).ToArray());
        }
        var fixedCount = actions.Count(a => a != CdsInterstitialScreenAction.Skip);
        var fixedWidth = fixedCount == 0 ? 0 : Math.Min(ActionButtonWidth, width / (fixedCount + (footer.Skip ? 1 : 0)));
        var rest = Math.Max(0, width - fixedWidth * fixedCount);
        var widths = actions.Select(a => a == CdsInterstitialScreenAction.Skip ? rest : fixedWidth).ToArray();
        return (footer.Skip ? 0 : rest, widths);
    }
}

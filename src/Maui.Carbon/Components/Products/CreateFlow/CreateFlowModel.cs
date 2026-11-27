namespace Maui.Carbon;

/// <summary>What the create-flow engine needs to know about a step.</summary>
internal readonly record struct CreateStepInfo(bool Included, bool Intro);

/// <summary>
/// Step navigation of Carbon for IBM Products' create flows (CreateTearsheet, CreateFullPage): steps are numbered from 1,
/// steps left out with <c>includeStep={false}</c> are skipped, and an intro step has no entry in the progress indicator.
/// Pure, so it is unit-tested.
/// </summary>
internal static class CreateFlowModel
{
    /// <summary>The first included step (1-based), or 0 when none is.</summary>
    public static int FirstIncluded(IReadOnlyList<CreateStepInfo> steps)
    {
        for (var i = 0; i < steps.Count; i++) if (steps[i].Included) return i + 1;
        return 0;
    }

    /// <summary>The last included step (1-based), or 0 when none is.</summary>
    public static int LastIncluded(IReadOnlyList<CreateStepInfo> steps)
    {
        for (var i = steps.Count - 1; i >= 0; i--) if (steps[i].Included) return i + 1;
        return 0;
    }

    /// <summary>The next included step after <paramref name="current"/>, or <paramref name="current"/> when it is the last.</summary>
    public static int Next(IReadOnlyList<CreateStepInfo> steps, int current)
    {
        for (var i = current; i < steps.Count; i++) if (steps[i].Included) return i + 1;
        return current;
    }

    /// <summary>The previous included step before <paramref name="current"/>, or <paramref name="current"/> when it is the first.</summary>
    public static int Previous(IReadOnlyList<CreateStepInfo> steps, int current)
    {
        for (var i = current - 2; i >= 0; i--) if (steps[i].Included) return i + 1;
        return current;
    }

    /// <summary>
    /// The step a flow opens on: the <paramref name="initialStep"/>-th included step when one is asked for (Carbon
    /// counts the steps that are left out), else the first included step.
    /// </summary>
    public static int Initial(IReadOnlyList<CreateStepInfo> steps, int initialStep)
    {
        if (initialStep <= 0) return FirstIncluded(steps);
        var seen = 0;
        for (var i = 0; i < steps.Count; i++)
        {
            if (!steps[i].Included) continue;
            if (++seen == initialStep) return i + 1;
        }
        return LastIncluded(steps);
    }

    /// <summary>The progress indicator lists the included steps that are not intro steps.</summary>
    public static IReadOnlyList<int> ProgressSteps(IReadOnlyList<CreateStepInfo> steps) =>
        [.. Enumerable.Range(0, steps.Count).Where(i => steps[i].Included && !steps[i].Intro).Select(i => i + 1)];

    /// <summary>The current step's index in the progress indicator: the number of listed steps before it.</summary>
    public static int ProgressIndex(IReadOnlyList<CreateStepInfo> steps, int current) =>
        ProgressSteps(steps).Count(s => s < current);

    /// <summary>The progress indicator is hidden while an intro first step is shown.</summary>
    public static bool ShowsProgress(IReadOnlyList<CreateStepInfo> steps, int current) =>
        !(current == 1 && steps.Count > 0 && steps[0].Intro);

    /// <summary>On the last included step the primary button submits instead of moving on.</summary>
    public static bool IsLast(IReadOnlyList<CreateStepInfo> steps, int current) => current >= LastIncluded(steps);
}

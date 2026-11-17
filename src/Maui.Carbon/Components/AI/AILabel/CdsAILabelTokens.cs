namespace Maui.Carbon;

/// <summary>The sizes of an <see cref="CdsAILabel"/> button (16, 20, 24, 32, 40, 48 and 64).</summary>
public enum CdsAILabelSize
{
    /// <summary>16: on tiles and other containers.</summary>
    Mini,
    /// <summary>20.</summary>
    TwoXs,
    /// <summary>24 (default): beside fields and table headers.</summary>
    Xs,
    /// <summary>32.</summary>
    Sm,
    /// <summary>40.</summary>
    Md,
    /// <summary>48.</summary>
    Lg,
    /// <summary>64: only next to large type.</summary>
    Xl,
}

/// <summary>The look of an <see cref="CdsAILabel"/> button.</summary>
public enum CdsAILabelKind
{
    /// <summary>A square with a 1px <c>border-inverse</c> outline around "AI".</summary>
    Default,
    /// <summary>A small square before "AI" with no box; add text to get the bordered "inline with content" look.</summary>
    Inline,
}

/// <summary>Color keys of an AI label button in one state. <c>null</c> means transparent.</summary>
/// <param name="Background">Fill.</param>
/// <param name="Border">1px border.</param>
/// <param name="Text">"AI" text color.</param>
/// <param name="SecondaryText">Text after "AI" (inline with content).</param>
/// <param name="Dot">The inline square before "AI".</param>
/// <param name="InnerRing">The inset 1px focus ring, or <c>null</c>.</param>
public readonly record struct CdsAILabelVisuals(string? Background, string? Border, string Text, string SecondaryText, string? Dot, string? InnerRing);

/// <summary>Sizes and state tokens of the AI label (from <c>packages/styles/scss/components/slug</c>).</summary>
public static class CdsAILabelTokens
{
    /// <summary>Side of the square button: 16, 20, 24, 32, 40, 48, 64.</summary>
    public static double Side(CdsAILabelSize size) => size switch
    {
        CdsAILabelSize.Mini => 16,
        CdsAILabelSize.TwoXs => 20,
        CdsAILabelSize.Xs => CdsSizing.XSmall,
        CdsAILabelSize.Sm => CdsSizing.Small,
        CdsAILabelSize.Md => CdsSizing.Medium,
        CdsAILabelSize.Lg => CdsSizing.Large,
        _ => CdsSizing.XLarge,
    };

    /// <summary>Font size of "AI" in the default kind: 9 (mini), 12 (2xs, xs), 16 (sm, md, lg), 20 (xl).</summary>
    public static double FontSize(CdsAILabelSize size) => size switch
    {
        CdsAILabelSize.Mini => 9,
        CdsAILabelSize.TwoXs or CdsAILabelSize.Xs => 12,
        CdsAILabelSize.Xl => 20,
        _ => 16,
    };

    /// <summary>Font size of the inline kind: 12 at sm and below, 14 at md, 16 at lg and xl.</summary>
    public static double InlineFontSize(CdsAILabelSize size) => size switch
    {
        CdsAILabelSize.Mini or CdsAILabelSize.TwoXs or CdsAILabelSize.Xs or CdsAILabelSize.Sm => 12,
        CdsAILabelSize.Md => 14,
        _ => 16,
    };

    /// <summary>The inline square: 4 (8 at lg and xl).</summary>
    public static double InlineDot(CdsAILabelSize size) => size >= CdsAILabelSize.Lg ? 8 : 4;

    /// <summary>The gap between the inline square and "AI": 8 (12 at lg and xl).</summary>
    public static double InlineGap(CdsAILabelSize size) => size >= CdsAILabelSize.Lg ? 12 : 8;

    /// <summary>The invisible hit area side: mini and 2xs get 24, others their own size.</summary>
    public static double HitArea(CdsAILabelSize size) => size <= CdsAILabelSize.TwoXs ? CdsSizing.XSmall : Side(size);

    /// <summary>Size of the revert button that replaces the label: the matching icon button size (xs for mini to xs, then sm, md, lg).</summary>
    public static CdsSize RevertSize(CdsAILabelSize size) => size switch
    {
        CdsAILabelSize.Mini or CdsAILabelSize.TwoXs or CdsAILabelSize.Xs => CdsSize.Xs,
        CdsAILabelSize.Sm => CdsSize.Sm,
        CdsAILabelSize.Md => CdsSize.Md,
        _ => CdsSize.Lg,
    };

    /// <summary>Resolves the colors of the button for a kind and state.</summary>
    public static CdsAILabelVisuals Resolve(CdsAILabelKind kind, bool hasText, CdsInteractionState state)
    {
        var hover = state.HasFlag(CdsInteractionState.Hover) || state.HasFlag(CdsInteractionState.Pressed);
        var focus = state.HasFlag(CdsInteractionState.FocusVisible);
        if (kind == CdsAILabelKind.Default)
        {
            var border = focus ? CdsColorKeys.Focus : CdsColorKeys.BorderInverse;
            var inner = focus ? CdsColorKeys.Focus : null;
            return hover
                ? new(CdsColorKeys.BorderInverse, border, CdsColorKeys.TextInverse, CdsColorKeys.TextInverse, null, inner)
                : new(null, border, CdsColorKeys.TextPrimary, CdsColorKeys.TextPrimary, null, inner);
        }

        // Inline: no box; with text it carries the border-inverse outline.
        string? edge = hasText ? CdsColorKeys.BorderInverse : null;
        if (hover) edge = CdsColorKeys.IconSecondary;
        if (focus) edge = CdsColorKeys.Focus;
        var inlineInner = focus && hasText ? CdsColorKeys.Focus : null;
        return hover
            ? new(null, edge, CdsColorKeys.TextSecondary, CdsColorKeys.TextSecondary, CdsColorKeys.IconSecondary, inlineInner)
            : new(null, edge, CdsColorKeys.TextPrimary, CdsColorKeys.TextPrimary, CdsColorKeys.IconPrimary, inlineInner);
    }

    /// <summary>The accessible name: "AI Show information" (or the inline text label when there is one).</summary>
    public static string AccessibleName(string aiText, string? label, string? textLabel, CdsAILabelKind kind) =>
        $"{aiText} {(kind == CdsAILabelKind.Inline && !string.IsNullOrEmpty(textLabel) ? textLabel : label)}".Trim();
}

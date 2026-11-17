namespace Maui.Carbon;

/// <summary>The kinds of a <see cref="CdsChatButton"/> (the same colors as the matching <see cref="CdsButtonKind"/>).</summary>
public enum CdsChatButtonKind
{
    /// <summary>Filled with <c>button-primary</c>: one per composer (Send).</summary>
    Primary,
    /// <summary>Filled with <c>button-secondary</c>.</summary>
    Secondary,
    /// <summary>Outlined.</summary>
    Tertiary,
    /// <summary>No fill or border.</summary>
    Ghost,
    /// <summary>Filled with <c>button-danger-primary</c>.</summary>
    Danger,
}

/// <summary>Sizes and state tokens of the chat button (from <c>packages/styles/scss/components/chat-button</c>).</summary>
public static class CdsChatButtonTokens
{
    /// <summary>Keeps sm, md and lg; any other size falls back to lg (as Carbon does).</summary>
    public static CdsSize Coerce(CdsSize size) => size is CdsSize.Sm or CdsSize.Md ? size : CdsSize.Lg;

    /// <summary>Pill radius: 24 at lg, 20 at md, 16 at sm.</summary>
    public static double Radius(CdsSize size) => Coerce(size) switch
    {
        CdsSize.Sm => 16,
        CdsSize.Md => 20,
        _ => 24,
    };

    /// <summary>Height: 32, 40 or 48.</summary>
    public static double Height(CdsSize size) => Coerce(size).Height();

    /// <summary>The button kind whose colors a chat kind uses.</summary>
    public static CdsButtonKind ToButtonKind(CdsChatButtonKind kind) => kind switch
    {
        CdsChatButtonKind.Secondary => CdsButtonKind.Secondary,
        CdsChatButtonKind.Tertiary => CdsButtonKind.Tertiary,
        CdsChatButtonKind.Ghost => CdsButtonKind.Ghost,
        CdsChatButtonKind.Danger => CdsButtonKind.Danger,
        _ => CdsButtonKind.Primary,
    };

    /// <summary>A quick action is always a small ghost button.</summary>
    public static (CdsChatButtonKind Kind, CdsSize Size) Effective(CdsChatButtonKind kind, CdsSize size, bool isQuickAction) =>
        isQuickAction ? (CdsChatButtonKind.Ghost, CdsSize.Sm) : (kind, Coerce(size));

    /// <summary>The end padding of the label: 15 without an icon (the label looks centered), 63 with one (the icon column).</summary>
    public static double LabelEndPadding(bool hasIcon, bool isGhostLayout) => hasIcon && !isGhostLayout ? 63 : 15;

    /// <summary>The focus ring: quick actions a plain inset ring, others as <see cref="CdsButtonTokens.FocusRing"/>.</summary>
    public static CdsFocusRingStyle FocusRing(CdsChatButtonKind kind, bool isQuickAction) =>
        isQuickAction ? CdsFocusRingStyle.Inset : CdsButtonTokens.FocusRing(ToButtonKind(kind));

    /// <summary>
    /// Resolves the colors. Regular chat buttons use <see cref="CdsButtonTokens"/>. Quick actions: a 1px <c>link-primary</c> border and text on
    /// a transparent fill; hover and pressed drop the border and fill with <c>background-hover</c> / <c>background-active</c> with
    /// <c>link-primary-hover</c> text; focus is a <c>focus</c> border; disabled is <c>button-disabled</c>; selected is no border,
    /// <c>background-selected</c> and <c>text-secondary</c> (also when disabled).
    /// </summary>
    public static CdsButtonVisuals Resolve(CdsChatButtonKind kind, CdsInteractionState state, bool isQuickAction, bool isSelected)
    {
        if (!isQuickAction) return CdsButtonTokens.Resolve(ToButtonKind(kind), state);

        var disabled = state.HasFlag(CdsInteractionState.Disabled);
        var pressed = state.HasFlag(CdsInteractionState.Pressed);
        var hover = state.HasFlag(CdsInteractionState.Hover);
        var focus = state.HasFlag(CdsInteractionState.FocusVisible);

        if (isSelected)
            return new(CdsColorKeys.BackgroundSelected, focus && !disabled ? CdsColorKeys.Focus : null, CdsColorKeys.TextSecondary, CdsColorKeys.TextSecondary);
        if (disabled)
            return new(null, CdsColorKeys.ButtonDisabled, CdsColorKeys.ButtonDisabled, CdsColorKeys.ButtonDisabled);
        if (pressed)
            return new(CdsColorKeys.BackgroundActive, focus ? CdsColorKeys.Focus : null, CdsColorKeys.LinkPrimaryHover, CdsColorKeys.LinkPrimaryHover);
        if (hover)
            return new(CdsColorKeys.BackgroundHover, focus ? CdsColorKeys.Focus : null, CdsColorKeys.LinkPrimaryHover, CdsColorKeys.LinkPrimaryHover);
        return new(null, focus ? CdsColorKeys.Focus : CdsColorKeys.LinkPrimary, CdsColorKeys.LinkPrimary, CdsColorKeys.LinkPrimary);
    }
}

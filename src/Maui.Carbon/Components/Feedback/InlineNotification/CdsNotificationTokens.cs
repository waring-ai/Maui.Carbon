namespace Maui.Carbon;

/// <summary>Color keys of a notification for one kind and contrast style.</summary>
/// <param name="Background">Fill.</param>
/// <param name="Accent">3px start edge and status icon color.</param>
/// <param name="Outline">1px outline in the accent color (low-contrast inline notifications, callouts); <c>null</c> for none.</param>
/// <param name="Text">Title, subtitle and caption.</param>
/// <param name="CloseIcon">Close icon color.</param>
/// <param name="CloseHover">Close button hover fill.</param>
/// <param name="Focus">Focus ring color.</param>
/// <param name="ActionText">Ghost action text color.</param>
/// <param name="ActionHover">Ghost action hover fill.</param>
public readonly record struct CdsNotificationVisuals(
    string Background, string Accent, string? Outline, string Text, string CloseIcon, string CloseHover,
    string Focus, string ActionText, string ActionHover);

/// <summary>Color keys of a notification action button.</summary>
/// <param name="Background">Fill (<c>null</c> is transparent).</param>
/// <param name="Border">1px outline (<c>null</c> for none).</param>
/// <param name="Text">Label color.</param>
public readonly record struct CdsNotificationActionVisuals(string? Background, string? Border, string Text);

/// <summary>
/// Maps notification kinds and contrast styles to Carbon tokens. Shared by <see cref="CdsInlineNotification"/>,
/// <see cref="CdsActionableNotification"/>, <see cref="CdsToastNotification"/> and <see cref="CdsCallout"/>.
/// High contrast is <c>background-inverse</c> with <c>support-*-inverse</c> accents; low contrast uses
/// <c>notification-background-*</c> with <c>support-*</c> accents.
/// </summary>
public static class CdsNotificationTokens
{
    /// <summary>Resolves the colors of a kind in a contrast style.</summary>
    public static CdsNotificationVisuals Resolve(CdsNotificationKind kind, bool lowContrast)
    {
        var accent = Accent(kind, lowContrast);
        return lowContrast
            ? new(Background(kind), accent, accent, CdsColorKeys.TextPrimary, CdsColorKeys.IconPrimary,
                CdsColorKeys.BackgroundHover, CdsColorKeys.Focus, CdsColorKeys.LinkPrimary, CdsColorKeys.NotificationActionHover)
            : new(CdsColorKeys.BackgroundInverse, accent, null, CdsColorKeys.TextInverse, CdsColorKeys.IconInverse,
                CdsColorKeys.BackgroundInverseHover, CdsColorKeys.FocusInverse, CdsColorKeys.LinkInverse, CdsColorKeys.BackgroundInverseHover);
    }

    /// <summary>The accent (edge and icon) key of a kind.</summary>
    public static string Accent(CdsNotificationKind kind, bool lowContrast) => kind switch
    {
        CdsNotificationKind.Error => lowContrast ? CdsColorKeys.SupportError : CdsColorKeys.SupportErrorInverse,
        CdsNotificationKind.Success => lowContrast ? CdsColorKeys.SupportSuccess : CdsColorKeys.SupportSuccessInverse,
        CdsNotificationKind.Warning or CdsNotificationKind.WarningAlt =>
            lowContrast ? CdsColorKeys.SupportWarning : CdsColorKeys.SupportWarningInverse,
        _ => lowContrast ? CdsColorKeys.SupportInfo : CdsColorKeys.SupportInfoInverse,
    };

    /// <summary>The <c>notification-background-*</c> key of a kind (low contrast fill).</summary>
    public static string Background(CdsNotificationKind kind) => kind switch
    {
        CdsNotificationKind.Error => CdsColorKeys.NotificationBackgroundError,
        CdsNotificationKind.Success => CdsColorKeys.NotificationBackgroundSuccess,
        CdsNotificationKind.Warning or CdsNotificationKind.WarningAlt => CdsColorKeys.NotificationBackgroundWarning,
        _ => CdsColorKeys.NotificationBackgroundInfo,
    };

    /// <summary>The 20px status icon of a kind.</summary>
    public static CdsIconName Icon(CdsNotificationKind kind) => kind switch
    {
        CdsNotificationKind.Error => CdsIconName.ErrorFilled,
        CdsNotificationKind.Success => CdsIconName.CheckmarkFilled,
        CdsNotificationKind.Warning => CdsIconName.WarningFilled,
        CdsNotificationKind.WarningAlt => CdsIconName.WarningAltFilled,
        CdsNotificationKind.InfoSquare => CdsIconName.InformationSquareFilled,
        _ => CdsIconName.InformationFilled,
    };

    /// <summary>The default accessible name of the status icon ("error icon").</summary>
    public static string DefaultIconDescription(CdsNotificationKind kind) => kind switch
    {
        CdsNotificationKind.Error => "error icon",
        CdsNotificationKind.Success => "success icon",
        CdsNotificationKind.Warning => "warning icon",
        CdsNotificationKind.WarningAlt => "warning alt icon",
        CdsNotificationKind.InfoSquare => "info square icon",
        _ => "info icon",
    };

    /// <summary>
    /// Resolves an action button. Ghost: link text, <c>background-inverse-hover</c> (high contrast) or
    /// <c>notification-action-hover</c> (low contrast) on hover and press. Tertiary: the
    /// <c>notification-action-tertiary-inverse*</c> tokens on high contrast, the tertiary button tokens on low contrast.
    /// </summary>
    public static CdsNotificationActionVisuals ResolveAction(bool lowContrast, bool tertiary, CdsInteractionState state)
    {
        var hover = (state & CdsInteractionState.Hover) != 0;
        var pressed = (state & CdsInteractionState.Pressed) != 0;
        var disabled = (state & CdsInteractionState.Disabled) != 0;

        if (!tertiary)
        {
            if (disabled) return new(null, null, CdsColorKeys.TextDisabled);
            var v = Resolve(CdsNotificationKind.Info, lowContrast);
            return new(hover || pressed ? v.ActionHover : null, null, v.ActionText);
        }
        if (lowContrast)
        {
            if (disabled) return new(null, CdsColorKeys.BorderDisabled, CdsColorKeys.TextDisabled);
            if (pressed) return new(CdsColorKeys.ButtonTertiaryActive, CdsColorKeys.ButtonTertiaryActive, CdsColorKeys.TextInverse);
            if (hover) return new(CdsColorKeys.ButtonTertiaryHover, CdsColorKeys.ButtonTertiaryHover, CdsColorKeys.TextInverse);
            return new(null, CdsColorKeys.ButtonTertiary, CdsColorKeys.ButtonTertiary);
        }
        if (disabled) return new(null, null, CdsColorKeys.NotificationActionTertiaryInverseTextOnColorDisabled);
        if (pressed) return new(CdsColorKeys.NotificationActionTertiaryInverseActive, null, CdsColorKeys.NotificationActionTertiaryInverseText);
        if (hover) return new(CdsColorKeys.NotificationActionTertiaryInverseHover, CdsColorKeys.NotificationActionTertiaryInverse, CdsColorKeys.NotificationActionTertiaryInverseText);
        return new(null, CdsColorKeys.NotificationActionTertiaryInverse, CdsColorKeys.NotificationActionTertiaryInverse);
    }
}

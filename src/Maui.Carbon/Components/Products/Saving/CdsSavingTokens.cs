namespace Maui.Carbon;

/// <summary>What a <see cref="CdsSaving"/> shows for a status (pure, tested).</summary>
/// <param name="ButtonIcon">The save button's icon (manual): <c>save</c>, <c>in-progress</c>, <c>checkmark--outline</c> or <c>error--outline</c>.</param>
/// <param name="IsSaveEnabled">Whether the save button is enabled (manual): not while a save is in progress.</param>
/// <param name="IsCancelEnabled">Whether the cancel button is enabled (manual): only while a save is in progress.</param>
/// <param name="ShowsErrorIcon">Whether the status line starts with the <c>error--filled</c> icon (auto, on failure).</param>
public readonly record struct CdsSavingVisuals(CdsIconName ButtonIcon, bool IsSaveEnabled, bool IsCancelEnabled, bool ShowsErrorIcon);

/// <summary>Maps a <see cref="CdsSavingStatus"/> to what <see cref="CdsSaving"/> shows and the tokens it uses.</summary>
public static class CdsSavingTokens
{
    /// <summary>The auto status line's error icon: <c>button-danger-primary</c>.</summary>
    public const string ErrorIconColor = CdsColorKeys.ButtonDangerPrimary;

    /// <summary>The auto status line's text: <c>text-primary</c>, <c>body-compact-01</c>.</summary>
    public const string TextColor = CdsColorKeys.TextPrimary;

    /// <summary>Space after the auto error icon: 16 (<c>spacing-05</c>).</summary>
    public const double ErrorIconGap = CdsSpacing.Spacing05;

    /// <summary>Resolves a status.</summary>
    public static CdsSavingVisuals Resolve(CdsSavingStatus status) => status switch
    {
        CdsSavingStatus.InProgress => new(CdsIconName.InProgress, false, true, false),
        CdsSavingStatus.Success => new(CdsIconName.CheckmarkOutline, true, false, false),
        CdsSavingStatus.Fail => new(CdsIconName.ErrorOutline, true, false, true),
        _ => new(CdsIconName.Save, true, false, false),
    };

    /// <summary>The text of a status from the four texts.</summary>
    public static string? Text(CdsSavingStatus status, string? defaultText, string? inProgressText, string? successText, string? failText) => status switch
    {
        CdsSavingStatus.InProgress => inProgressText,
        CdsSavingStatus.Success => successText,
        CdsSavingStatus.Fail => failText,
        _ => defaultText,
    };
}

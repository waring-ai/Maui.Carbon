namespace Maui.Carbon;

/// <summary>Maps the parts of the Carbon for IBM Products modals to Carbon tokens.</summary>
public static class CdsProductModalTokens
{
    /// <summary>The check of a success message (export, API key): <c>button-primary</c>.</summary>
    public static string SuccessIcon => CdsColorKeys.ButtonPrimary;

    /// <summary>The icon of an error message (export, API key): <c>button-danger-primary</c>.</summary>
    public static string ErrorIcon => CdsColorKeys.ButtonDangerPrimary;

    /// <summary>The information icon before the API key download or success body: <c>icon-primary</c>.</summary>
    public static string InformationIcon => CdsColorKeys.IconPrimary;

    /// <summary>The 1px rule under the create modal's header: <c>layer-accent-01</c>.</summary>
    public static string CreateHeaderRule => CdsColorKeys.LayerAccent01;

    /// <summary>The 1px divider between the about modal's links: <c>border-strong-01</c>.</summary>
    public static string AboutLinkDivider => CdsColorKeys.BorderStrong01;

    /// <summary>
    /// The fill of the about modal's additional-info footer: <c>layer-01</c> inside a <see cref="CdsLayer"/>, which
    /// resolves to Carbon's <c>layer-02</c> one level above the modal.
    /// </summary>
    public static string AboutFooterFill => CdsColorKeys.Layer01;

    /// <summary>The color of a status message's icon: success, error, or the information icon otherwise.</summary>
    public static string MessageIcon(bool success, bool error) => error ? ErrorIcon : success ? SuccessIcon : InformationIcon;
}

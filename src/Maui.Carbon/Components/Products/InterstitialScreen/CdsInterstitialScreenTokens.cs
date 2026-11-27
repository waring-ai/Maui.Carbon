namespace Maui.Carbon;

/// <summary>Maps the parts of a <see cref="CdsInterstitialScreen"/> to Carbon tokens.</summary>
public static class CdsInterstitialScreenTokens
{
    /// <summary>The header fill: <c>layer-01</c> when it has a title, subtitle or content, else <c>background</c>.</summary>
    public static string HeaderBackground(bool hasTitle) => hasTitle ? CdsColorKeys.Layer01 : CdsColorKeys.Background;

    /// <summary>The fill of the body and the footer: <c>background</c>.</summary>
    public static string Background => CdsColorKeys.Background;

    /// <summary>The 1px rule under a titled header and above the footer: <c>border-subtle-01</c>.</summary>
    public static string Divider => CdsColorKeys.BorderSubtle01;

    /// <summary>The 1px border of the modal container: <c>border-subtle-01</c>.</summary>
    public static string ContainerBorder => CdsColorKeys.BorderSubtle01;

    /// <summary>The modal container fill: <c>layer-01</c>.</summary>
    public static string ContainerFill => CdsColorKeys.Layer01;

    /// <summary>The colors of a footer button: those of the Carbon button of the action's kind.</summary>
    public static CdsButtonVisuals Button(CdsInterstitialScreenAction action, CdsInteractionState state) =>
        CdsButtonTokens.Resolve(InterstitialScreenModel.KindOf(action), state);
}

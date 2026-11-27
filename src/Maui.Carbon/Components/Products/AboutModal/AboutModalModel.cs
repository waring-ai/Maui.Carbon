namespace Maui.Carbon;

/// <summary>The about modal's vertical rhythm (pure, from <c>_about-modal.scss</c>).</summary>
internal static class AboutModalModel
{
    /// <summary>
    /// The space above the links, the legal text and the copyright: links 24 under the version; legal text 24 (16 right
    /// after the links); copyright 16 (8 right after the links).
    /// </summary>
    public static (double Links, double Legal, double Copyright) Spacing(bool hasLinks, bool hasLegal) => (
        CdsSpacing.Spacing06,
        hasLinks ? CdsSpacing.Spacing05 : CdsSpacing.Spacing06,
        hasLinks && !hasLegal ? CdsSpacing.Spacing03 : CdsSpacing.Spacing05);
}

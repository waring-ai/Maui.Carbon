namespace Maui.Carbon;

/// <summary>
/// A narrow tearsheet (Carbon for IBM Products' <c>TearsheetNarrow</c>): a tall modal surface as wide as a small modal
/// that rises from the bottom of the page, for a focused task with more content than a modal holds, such as a short
/// form. Its content sits on the next layer. It has no influencer, header actions or navigation; use
/// <see cref="CdsTearsheet"/> for those.
/// </summary>
public class CdsTearsheetNarrow : CdsTearsheetShell
{
    /// <summary>Creates a closed narrow tearsheet.</summary>
    public CdsTearsheetNarrow()
    {
    }

    private protected override bool IsWide => false;
}

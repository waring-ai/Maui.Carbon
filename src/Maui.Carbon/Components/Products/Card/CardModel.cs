namespace Maui.Carbon;

/// <summary>What a card is asked to show (the props the layout depends on).</summary>
internal readonly record struct CardInputs(
    bool Productive,
    int ActionIconCount,
    int OverflowCount,
    bool HasPrimaryButton,
    bool HasSecondaryButton,
    CdsCardPlacement ActionsPlacement,
    CdsCardPlacement PrimaryPlacement,
    CdsCardPlacement SecondaryPlacement,
    bool HasClick,
    CdsCardClickZone ClickZone,
    bool HasLabel,
    bool HasTitle,
    bool HasDescription,
    CdsCardTitleSize TitleSize = CdsCardTitleSize.Default,
    bool HasDecorator = false);

/// <summary>Where a card puts its parts, decided from <see cref="CardInputs"/> the way <c>Card.tsx</c> decides it.</summary>
internal readonly record struct CardLayout(
    bool HasHeaderActions,
    bool HasFooterActions,
    bool HasFooterButton,
    bool HasBottomBar,
    bool PrimaryInHeader,
    bool SecondaryInHeader,
    bool PrimaryInFooter,
    bool SecondaryInFooter,
    bool UsesOverflow,
    CdsCardClickZone? ClickZone,
    bool LabelOnly,
    double HeaderEndPadding,
    double HeaderBottomPadding,
    double HeaderActionsTopMargin);

/// <summary>The pure layout rules of the shared card (Carbon for IBM Products' internal <c>Card</c>).</summary>
internal static class CardModel
{
    /// <summary>Decides which parts the card shows and where.</summary>
    public static CardLayout Layout(CardInputs i)
    {
        var overflow = i.Productive ? i.OverflowCount : 0;
        // actions are the icons (or the overflow menu that replaces them); a top primary button also counts.
        var hasActions = i.ActionIconCount > 0 || overflow > 0 || (i.HasPrimaryButton && i.PrimaryPlacement == CdsCardPlacement.Top);
        var headerActions = hasActions && i.ActionsPlacement == CdsCardPlacement.Top;
        var footerActions = hasActions && i.ActionsPlacement == CdsCardPlacement.Bottom;
        var footerButton = i.HasSecondaryButton || (i.HasPrimaryButton && i.PrimaryPlacement == CdsCardPlacement.Bottom);
        CdsCardClickZone? zone = !i.HasClick ? null : i.Productive ? i.ClickZone : CdsCardClickZone.One;
        var labelOnly = i.HasLabel && !i.HasTitle && !i.HasDescription;
        var largeOrLabel = i.HasTitle && (i.HasLabel || (i.Productive && i.TitleSize == CdsCardTitleSize.Large));
        return new CardLayout(
            HasHeaderActions: headerActions,
            HasFooterActions: footerActions,
            HasFooterButton: footerButton,
            HasBottomBar: footerActions || footerButton,
            PrimaryInHeader: headerActions && i.HasPrimaryButton && i.PrimaryPlacement == CdsCardPlacement.Top,
            SecondaryInHeader: headerActions && i.HasSecondaryButton && i.SecondaryPlacement == CdsCardPlacement.Top,
            PrimaryInFooter: i.HasPrimaryButton && i.PrimaryPlacement == CdsCardPlacement.Bottom,
            SecondaryInFooter: i.HasSecondaryButton && i.SecondaryPlacement == CdsCardPlacement.Bottom,
            UsesOverflow: overflow > 0,
            ClickZone: zone,
            LabelOnly: labelOnly,
            HeaderEndPadding: HeaderEndPadding(headerActions, i.HasDecorator, largeOrLabel),
            HeaderBottomPadding: labelOnly ? CdsSpacing.Spacing03 : CdsSpacing.Spacing05,
            HeaderActionsTopMargin: i.Productive && i.TitleSize == CdsCardTitleSize.Default && !i.HasLabel ? -CdsSpacing.Spacing01 : 0);
    }

    /// <summary>
    /// The end padding of the header row: 8 with header actions; with a decorator 32, 40 with actions too, and 48 when a title
    /// sits under a label or is large (the later rule in the stylesheet wins).
    /// </summary>
    public static double HeaderEndPadding(bool hasHeaderActions, bool hasDecorator, bool largeTileOrLabel)
    {
        if (!hasDecorator) return hasHeaderActions ? CdsSpacing.Spacing03 : 0;
        if (largeTileOrLabel) return CdsSpacing.Spacing09;
        return hasHeaderActions ? CdsSpacing.Spacing08 : CdsSpacing.Spacing07;
    }

    /// <summary>
    /// Whether the header ghost button at <paramref name="index"/> of <paramref name="count"/> header actions draws its
    /// separator: every one does except the first child of several (<c>:first-child:not(:last-child)</c>).
    /// </summary>
    public static bool ShowsSeparator(int index, int count) => !(index == 0 && count > 1);

    /// <summary>The kind of a bottom-bar button: always ghost on productive cards, the chosen kind on expressive ones.</summary>
    public static CdsButtonKind FooterKind(bool productive, bool primary, CdsCardPrimaryButtonKind primaryKind, CdsCardSecondaryButtonKind secondaryKind)
    {
        if (productive) return CdsButtonKind.Ghost;
        if (primary) return primaryKind == CdsCardPrimaryButtonKind.Ghost ? CdsButtonKind.Ghost : CdsButtonKind.Primary;
        return secondaryKind == CdsCardSecondaryButtonKind.Ghost ? CdsButtonKind.Ghost : CdsButtonKind.Secondary;
    }

    /// <summary>Icon button size: sm (32) in the header; md (40) in a productive bottom bar; sm on expressive cards.</summary>
    public static CdsSize ActionSize(bool productive, CdsCardPlacement placement) =>
        productive && placement == CdsCardPlacement.Bottom ? CdsSize.Md : CdsSize.Sm;

    /// <summary>
    /// The column widths of a productive bottom bar with <paramref name="count"/> parts (<c>justify-content: space-between</c>,
    /// or <c>flex-end</c> without a button): parts take their own width and equal stars share the space between them.
    /// </summary>
    public static IReadOnlyList<GridLength> FooterColumns(int count, bool hasButton)
    {
        if (count <= 0) return [];
        if (!hasButton || count == 1) return hasButton ? [GridLength.Auto, GridLength.Star] : [GridLength.Star, GridLength.Auto];
        var columns = new List<GridLength>();
        for (var i = 0; i < count; i++)
        {
            if (i > 0) columns.Add(GridLength.Star);
            columns.Add(GridLength.Auto);
        }
        return columns;
    }

    /// <summary>The column of part <paramref name="index"/> in <see cref="FooterColumns"/>.</summary>
    public static int FooterColumn(int index, int count, bool hasButton) =>
        count == 1 || !hasButton ? (hasButton ? 0 : 1) : index * 2;

    /// <summary>The accessible name of the click target: label and title (or the body text for a body-only zone).</summary>
    public static string? AccessibleName(CdsCardClickZone? zone, string? label, string? title, string? bodyText)
    {
        if (zone == CdsCardClickZone.Three && !string.IsNullOrWhiteSpace(bodyText)) return bodyText;
        var parts = new[] { label, title }.Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
        return parts.Length == 0 ? (string.IsNullOrWhiteSpace(bodyText) ? null : bodyText) : string.Join(", ", parts);
    }
}

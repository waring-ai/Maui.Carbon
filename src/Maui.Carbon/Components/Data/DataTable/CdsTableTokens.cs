namespace Maui.Carbon;

/// <summary>
/// Maps table states to Carbon tokens and holds the table measurements (from
/// <c>packages/styles/scss/components/data-table</c>). Pure and unit-tested.
/// </summary>
public static class CdsTableTokens
{
    /// <summary>Body fill: <c>layer-01</c>.</summary>
    public static string Background => CdsColorKeys.Layer01;

    /// <summary>
    /// Header cell fill: <c>layer-accent-01</c>; <c>layer-selected-hover-01</c> for the active sort header and for a sortable header
    /// under the pointer.
    /// </summary>
    public static string HeaderFill(bool sortable, bool hover, bool active) =>
        active || sortable && hover ? CdsColorKeys.LayerSelectedHover01 : CdsColorKeys.LayerAccent01;

    /// <summary>Header text: <c>text-primary</c>.</summary>
    public static string HeaderText => CdsColorKeys.TextPrimary;

    /// <summary>
    /// Body row fill. Selected is <c>layer-selected-01</c> (<c>layer-selected-hover-01</c> on hover), hover is
    /// <c>layer-hover-01</c>, an even zebra row is <c>layer-accent-01</c>, anything else <c>layer-01</c>.
    /// </summary>
    public static string RowFill(bool selected, bool hover, bool zebraStripe)
    {
        if (selected) return hover ? CdsColorKeys.LayerSelectedHover01 : CdsColorKeys.LayerSelected01;
        if (hover) return CdsColorKeys.LayerHover01;
        return zebraStripe ? CdsColorKeys.LayerAccent01 : CdsColorKeys.Layer01;
    }

    /// <summary>
    /// The 1px rule under a row: <c>border-subtle-01</c>; it takes the fill's color on hover so it disappears into the row,
    /// <c>layer-active-01</c> under a selected row, and in zebra tables the neighbouring stripe's color
    /// (<paramref name="zebra"/> true; <paramref name="zebraStripe"/> is the row's own stripe).
    /// </summary>
    public static string RowRule(bool selected, bool hover, bool zebra, bool zebraStripe)
    {
        if (hover) return selected ? CdsColorKeys.LayerSelectedHover01 : CdsColorKeys.LayerHover01;
        if (selected) return CdsColorKeys.LayerActive01;
        if (zebra) return zebraStripe ? CdsColorKeys.LayerAccent01 : CdsColorKeys.Layer01;
        return CdsColorKeys.BorderSubtle01;
    }

    /// <summary>Cell text: <c>text-secondary</c>, <c>text-primary</c> when the row is hovered or selected.</summary>
    public static string CellText(bool active) => active ? CdsColorKeys.TextPrimary : CdsColorKeys.TextSecondary;

    /// <summary>Sort arrow and expand chevron colors.</summary>
    public static string SortIcon => CdsColorKeys.IconPrimary;

    /// <summary>The expand chevron: <c>layer-selected-inverse</c>.</summary>
    public static string ExpandIcon => CdsColorKeys.LayerSelectedInverse;

    /// <summary>The batch action bar fill: <c>background-brand</c>.</summary>
    public static string BatchBar => CdsColorKeys.BackgroundBrand;

    /// <summary>The batch bar text and icons: <c>text-on-color</c>.</summary>
    public static string BatchText => CdsColorKeys.TextOnColor;

    /// <summary>
    /// Fill of a batch bar button while it is drawn (hovered, pressed or disabled): <c>button-primary-hover</c>, pressed
    /// <c>button-primary-active</c>, and <c>button-disabled</c> while its command cannot run (its text and icon then use the
    /// <c>-on-color-disabled</c> tokens).
    /// </summary>
    public static string BatchButtonFill(bool pressed, bool disabled) =>
        disabled ? CdsColorKeys.ButtonDisabled : pressed ? CdsColorKeys.ButtonPrimaryActive : CdsColorKeys.ButtonPrimaryHover;

    /// <summary>The toolbar fill: <c>layer-01</c>.</summary>
    public static string Toolbar => CdsColorKeys.Layer01;

    /// <summary>
    /// Whether a row's overflow menu trigger is drawn. It is always shown on touch platforms or when
    /// <paramref name="hoverOnly"/> is off; otherwise only while the row is hovered, the trigger has focus or its menu is open.
    /// </summary>
    public static bool IsOverflowRevealed(bool hoverOnly, bool touchPlatform, bool rowHover, bool focusedOrOpen) =>
        !hoverOnly || touchPlatform || rowHover || focusedOrOpen;

    /// <summary>Whether the platform is touch first (Android, iOS) so hover-only affordances must stay visible.</summary>
    public static bool IsTouchPlatform
    {
        get
        {
            try
            {
                var platform = DeviceInfo.Platform;
                return platform == DevicePlatform.Android || platform == DevicePlatform.iOS;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }

    /// <summary>Row height: xs 24, sm 32, md 40, lg 48 (default), xl 64. Larger sizes clamp to xl.</summary>
    public static double RowHeight(CdsSize size) => (size > CdsSize.Xl ? CdsSize.Xl : size).Height();

    /// <summary>The toolbar height: 32 in xs and sm tables, 48 otherwise.</summary>
    public static double ToolbarHeight(CdsSize size) => size <= CdsSize.Sm ? CdsSizing.Small : CdsSizing.Large;

    /// <summary>The checkbox column width: 40.</summary>
    public const double CheckboxColumnWidth = 40;

    /// <summary>The radio column width: 48.</summary>
    public const double RadioColumnWidth = 48;

    /// <summary>The expand column width: 40.</summary>
    public const double ExpandColumnWidth = 40;

    /// <summary>The overflow menu column width: 52.</summary>
    public const double MenuColumnWidth = 52;

    /// <summary>
    /// The width a star or auto column is assumed to need at least when deciding whether the table scrolls sideways
    /// (fixed columns count at their width).
    /// </summary>
    public const double MinimumFlexibleColumnWidth = 96;

    /// <summary>Horizontal cell padding: 16.</summary>
    public const double CellPaddingX = CdsSpacing.Spacing05;

    /// <summary>The default height of a sticky header table's scrolling body: 300.</summary>
    public const double StickyBodyHeight = 300;

    /// <summary>The inset of an expanded child row's content: 56, or 88 when the table has a selection column.</summary>
    public static double ChildInset(CdsDataTableSelection selection) => selection == CdsDataTableSelection.None ? 56 : 88;

    /// <summary>Vertical cell padding (top, bottom): xs 2, sm and md 7 and 6, lg 0 (centered), xl 16 (top aligned).</summary>
    public static (double Top, double Bottom) CellPaddingY(CdsSize size) => size switch
    {
        CdsSize.Xs => (2, 2),
        CdsSize.Sm or CdsSize.Md => (7, 6),
        CdsSize.Lg => (0, 0),
        _ => (CdsSpacing.Spacing05, CdsSpacing.Spacing05),
    };

    /// <summary>The overflow menu trigger size for a table size: the row height, with xl using lg.</summary>
    public static CdsSize MenuTriggerSize(CdsSize size) => size >= CdsSize.Xl ? CdsSize.Lg : size;
}

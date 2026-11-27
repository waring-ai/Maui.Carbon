namespace Maui.Carbon;

/// <summary>
/// A card for task-focused content in dashboards and resource lists (Carbon for IBM Products' <c>ProductiveCard</c>): a
/// <c>heading-compact-02</c> title (or <c>heading-03</c>), an optional label and description, the
/// <see cref="CdsContainer.Body"/>, and small ghost actions. Icon actions sit in the header by default (32px ghost icon
/// buttons) and give way to an overflow menu when <see cref="OverflowActions"/> has items. The bottom bar, under a
/// <c>border-subtle-01</c> rule, holds ghost buttons at the start and actions at the end. A clickable card picks its
/// target with <see cref="ClickZone"/>; its buttons stay separate targets.
/// </summary>
public partial class CdsProductiveCard : CdsCardBase
{
    readonly CdsItemCollection<CdsMenuItemBase> _overflowActions = [];

    /// <summary>Creates an empty productive card.</summary>
    public CdsProductiveCard() : base(productive: true)
    {
        _overflowActions.CollectionChanged += (_, _) => RequestRebuild();
        Rebuild();
    }

    /// <summary>
    /// Items of an overflow menu that replaces the icon actions (Carbon's <c>overflowActions</c>): use it for more than two
    /// actions. The menu sits where <see cref="ActionsPlacement"/> puts the actions.
    /// </summary>
    public IList<CdsMenuItemBase> OverflowActions => _overflowActions;

    private protected override CardOptions Options => new(
        ActionsPlacement, PrimaryButtonPlacement, SecondaryButtonPlacement, ClickZone, TitleSize,
        CdsCardPrimaryButtonKind.Ghost, CdsCardSecondaryButtonKind.Ghost, null, CdsCardMediaPosition.Top, null,
        _overflowActions, OverflowDescription);
}

public partial class CdsProductiveCard
{
    /// <summary>Bindable property for <see cref="ActionsPlacement"/>.</summary>
    public static readonly BindableProperty ActionsPlacementProperty = BindableProperty.Create(
        nameof(ActionsPlacement), typeof(CdsCardPlacement), typeof(CdsProductiveCard), CdsCardPlacement.Top, propertyChanged: OnLayoutChanged);

    /// <summary>Bindable property for <see cref="ClickZone"/>.</summary>
    public static readonly BindableProperty ClickZoneProperty = BindableProperty.Create(
        nameof(ClickZone), typeof(CdsCardClickZone), typeof(CdsProductiveCard), CdsCardClickZone.One, propertyChanged: OnLayoutChanged);

    /// <summary>Bindable property for <see cref="TitleSize"/>.</summary>
    public static readonly BindableProperty TitleSizeProperty = BindableProperty.Create(
        nameof(TitleSize), typeof(CdsCardTitleSize), typeof(CdsProductiveCard), CdsCardTitleSize.Default, propertyChanged: OnLayoutChanged);

    /// <summary>Bindable property for <see cref="PrimaryButtonPlacement"/>.</summary>
    public static readonly BindableProperty PrimaryButtonPlacementProperty = BindableProperty.Create(
        nameof(PrimaryButtonPlacement), typeof(CdsCardPlacement), typeof(CdsProductiveCard), CdsCardPlacement.Bottom, propertyChanged: OnLayoutChanged);

    /// <summary>Bindable property for <see cref="SecondaryButtonPlacement"/>.</summary>
    public static readonly BindableProperty SecondaryButtonPlacementProperty = BindableProperty.Create(
        nameof(SecondaryButtonPlacement), typeof(CdsCardPlacement), typeof(CdsProductiveCard), CdsCardPlacement.Bottom, propertyChanged: OnLayoutChanged);

    /// <summary>Bindable property for <see cref="OverflowDescription"/>.</summary>
    public static readonly BindableProperty OverflowDescriptionProperty = BindableProperty.Create(
        nameof(OverflowDescription), typeof(string), typeof(CdsProductiveCard), "Options", propertyChanged: OnLayoutChanged);

    /// <summary>Where the icon actions (or the overflow menu) sit: in the header (default, 32px buttons) or in the bottom bar (40px).</summary>
    public CdsCardPlacement ActionsPlacement
    {
        get => (CdsCardPlacement)GetValue(ActionsPlacementProperty);
        set => SetValue(ActionsPlacementProperty, value);
    }

    /// <summary>Which part of a clickable card is the target: the whole card (default), the header and body, or the body. Make the card clickable with <see cref="CdsCardBase.Clicked"/> or <see cref="CdsCardBase.Command"/>.</summary>
    public CdsCardClickZone ClickZone
    {
        get => (CdsCardClickZone)GetValue(ClickZoneProperty);
        set => SetValue(ClickZoneProperty, value);
    }

    /// <summary>Title size: <c>heading-compact-02</c> (default) or <c>heading-03</c>.</summary>
    public CdsCardTitleSize TitleSize
    {
        get => (CdsCardTitleSize)GetValue(TitleSizeProperty);
        set => SetValue(TitleSizeProperty, value);
    }

    /// <summary>Where the primary button sits: a ghost button in the bottom bar (default) or a small ghost button in the header.</summary>
    public CdsCardPlacement PrimaryButtonPlacement
    {
        get => (CdsCardPlacement)GetValue(PrimaryButtonPlacementProperty);
        set => SetValue(PrimaryButtonPlacementProperty, value);
    }

    /// <summary>Where the secondary button sits. In the header it shows only beside other header actions.</summary>
    public CdsCardPlacement SecondaryButtonPlacement
    {
        get => (CdsCardPlacement)GetValue(SecondaryButtonPlacementProperty);
        set => SetValue(SecondaryButtonPlacementProperty, value);
    }

    /// <summary>Accessible name and tooltip of the overflow menu trigger (Carbon's <c>overflowAriaLabel</c>, formerly <c>iconDescription</c>). Default "Options".</summary>
    public string? OverflowDescription
    {
        get => (string?)GetValue(OverflowDescriptionProperty);
        set => SetValue(OverflowDescriptionProperty, value);
    }

    static void OnLayoutChanged(BindableObject b, object? o, object? n) => ((CdsProductiveCard)b).RequestRebuild();
}

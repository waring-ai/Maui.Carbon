namespace Maui.Carbon;

/// <summary>
/// A card for marketing-like, browsable content such as catalog entries and getting-started links (Carbon for IBM
/// Products' <c>ExpressiveCard</c>): optional media (top or start) or a 32px pictogram, a label, a <c>heading-03</c>
/// title, a description, the <see cref="CdsContainer.Body"/>, and a bottom bar with the secondary and primary buttons
/// and ghost icon actions flush at the end edge. Handling <see cref="CdsCardBase.Clicked"/> makes the whole card one
/// target with a <c>border-tile-01</c> border and <c>layer-hover-01</c> on hover; keep it to one action then.
/// </summary>
public partial class CdsExpressiveCard : CdsCardBase
{
    /// <summary>Creates an empty expressive card.</summary>
    public CdsExpressiveCard() : base(productive: false) => Rebuild();

    private protected override CardOptions Options => new(
        CdsCardPlacement.Bottom, CdsCardPlacement.Bottom, CdsCardPlacement.Bottom, CdsCardClickZone.One, CdsCardTitleSize.Default,
        PrimaryButtonKind, SecondaryButtonKind, Media, MediaPosition, Pictogram, null, null);
}

public partial class CdsExpressiveCard
{
    /// <summary>Bindable property for <see cref="Media"/>.</summary>
    public static readonly BindableProperty MediaProperty = BindableProperty.Create(
        nameof(Media), typeof(View), typeof(CdsExpressiveCard), null, propertyChanged: OnLayoutChanged);

    /// <summary>Bindable property for <see cref="MediaPosition"/>.</summary>
    public static readonly BindableProperty MediaPositionProperty = BindableProperty.Create(
        nameof(MediaPosition), typeof(CdsCardMediaPosition), typeof(CdsExpressiveCard), CdsCardMediaPosition.Top, propertyChanged: OnLayoutChanged);

    /// <summary>Bindable property for <see cref="Pictogram"/>.</summary>
    public static readonly BindableProperty PictogramProperty = BindableProperty.Create(
        nameof(Pictogram), typeof(CdsIconName?), typeof(CdsExpressiveCard), null, propertyChanged: OnLayoutChanged);

    /// <summary>Bindable property for <see cref="PrimaryButtonKind"/>.</summary>
    public static readonly BindableProperty PrimaryButtonKindProperty = BindableProperty.Create(
        nameof(PrimaryButtonKind), typeof(CdsCardPrimaryButtonKind), typeof(CdsExpressiveCard), CdsCardPrimaryButtonKind.Primary, propertyChanged: OnLayoutChanged);

    /// <summary>Bindable property for <see cref="SecondaryButtonKind"/>.</summary>
    public static readonly BindableProperty SecondaryButtonKindProperty = BindableProperty.Create(
        nameof(SecondaryButtonKind), typeof(CdsCardSecondaryButtonKind), typeof(CdsExpressiveCard), CdsCardSecondaryButtonKind.Secondary, propertyChanged: OnLayoutChanged);

    /// <summary>Optional media (an image or an <see cref="CdsAspectRatio"/> box) above or beside the content.</summary>
    public View? Media
    {
        get => (View?)GetValue(MediaProperty);
        set => SetValue(MediaProperty, value);
    }

    /// <summary>Where the media sits: above the content (default) or at the start edge beside it.</summary>
    public CdsCardMediaPosition MediaPosition
    {
        get => (CdsCardMediaPosition)GetValue(MediaPositionProperty);
        set => SetValue(MediaPositionProperty, value);
    }

    /// <summary>An optional 32px icon at the top of the card, 16 from the top and start edges.</summary>
    public CdsIconName? Pictogram
    {
        get => (CdsIconName?)GetValue(PictogramProperty);
        set => SetValue(PictogramProperty, value);
    }

    /// <summary>The primary button's kind: primary (default) or ghost.</summary>
    public CdsCardPrimaryButtonKind PrimaryButtonKind
    {
        get => (CdsCardPrimaryButtonKind)GetValue(PrimaryButtonKindProperty);
        set => SetValue(PrimaryButtonKindProperty, value);
    }

    /// <summary>The secondary button's kind: secondary (default) or ghost.</summary>
    public CdsCardSecondaryButtonKind SecondaryButtonKind
    {
        get => (CdsCardSecondaryButtonKind)GetValue(SecondaryButtonKindProperty);
        set => SetValue(SecondaryButtonKindProperty, value);
    }

    static void OnLayoutChanged(BindableObject b, object? o, object? n) => ((CdsExpressiveCard)b).RequestRebuild();
}

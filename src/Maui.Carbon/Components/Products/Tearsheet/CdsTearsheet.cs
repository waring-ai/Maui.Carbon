namespace Maui.Carbon;

/// <summary>
/// A wide tearsheet (Carbon for IBM Products' <c>Tearsheet</c>): a tall modal surface that rises from the bottom of the
/// page, nearly as wide as the viewport, for a substantial task that still needs a sense of the page underneath: a
/// complex form, a multi-step flow, a review of many records. It has a header (<see cref="CdsTearsheetShell.Label"/>,
/// title, description, <see cref="HeaderActions"/>, <see cref="HeaderNavigation"/>), an optional
/// <see cref="Influencer"/> column, the main content on the page background, and up to four actions 80 high. Use
/// <see cref="CdsTearsheetNarrow"/> for a short task, a side panel when the page must stay visible, a modal for a quick
/// decision. Up to three tearsheets can stack.
/// </summary>
public class CdsTearsheet : CdsTearsheetShell
{
    /// <summary>Bindable property for <see cref="HeaderActions"/>.</summary>
    public static readonly BindableProperty HeaderActionsProperty = BindableProperty.Create(
        nameof(HeaderActions), typeof(View), typeof(CdsTearsheet), null,
        propertyChanged: static (b, _, _) => ((CdsTearsheet)b).OnPartsChanged());

    /// <summary>Bindable property for <see cref="HeaderNavigation"/>.</summary>
    public static readonly BindableProperty HeaderNavigationProperty = BindableProperty.Create(
        nameof(HeaderNavigation), typeof(View), typeof(CdsTearsheet), null,
        propertyChanged: static (b, _, _) => ((CdsTearsheet)b).OnPartsChanged());

    /// <summary>Bindable property for <see cref="Influencer"/>.</summary>
    public static readonly BindableProperty InfluencerProperty = BindableProperty.Create(
        nameof(Influencer), typeof(View), typeof(CdsTearsheet), null,
        propertyChanged: static (b, _, _) => ((CdsTearsheet)b).OnPartsChanged());

    /// <summary>Bindable property for <see cref="InfluencerPosition"/>.</summary>
    public static readonly BindableProperty InfluencerPositionProperty = BindableProperty.Create(
        nameof(InfluencerPosition), typeof(CdsTearsheetInfluencerPosition), typeof(CdsTearsheet), CdsTearsheetInfluencerPosition.Start,
        propertyChanged: static (b, _, _) => ((CdsTearsheet)b).OnPartsChanged());

    /// <summary>Bindable property for <see cref="InfluencerWidth"/>.</summary>
    public static readonly BindableProperty InfluencerWidthProperty = BindableProperty.Create(
        nameof(InfluencerWidth), typeof(CdsTearsheetInfluencerWidth), typeof(CdsTearsheet), CdsTearsheetInfluencerWidth.Narrow,
        propertyChanged: static (b, _, _) => ((CdsTearsheet)b).OnPartsChanged());

    /// <summary>Creates a closed wide tearsheet.</summary>
    public CdsTearsheet()
    {
    }

    /// <summary>Controls at the top end of the header (a <see cref="CdsButtonSet"/> or a dropdown) that act on the whole tearsheet.</summary>
    public View? HeaderActions
    {
        get => (View?)GetValue(HeaderActionsProperty);
        set => SetValue(HeaderActionsProperty, value);
    }

    /// <summary>Navigation under the title (Carbon: <c>navigation</c>), usually <see cref="CdsTabs"/> that switch the main content.</summary>
    public View? HeaderNavigation
    {
        get => (View?)GetValue(HeaderNavigationProperty);
        set => SetValue(HeaderNavigationProperty, value);
    }

    /// <summary>A side column that steers the main content: the steps of a flow, a table of contents, a preview. It scrolls on its own.</summary>
    public View? Influencer
    {
        get => (View?)GetValue(InfluencerProperty);
        set => SetValue(InfluencerProperty, value);
    }

    /// <summary>Where the influencer sits. Default before the content.</summary>
    public CdsTearsheetInfluencerPosition InfluencerPosition
    {
        get => (CdsTearsheetInfluencerPosition)GetValue(InfluencerPositionProperty);
        set => SetValue(InfluencerPositionProperty, value);
    }

    /// <summary>The influencer width: 256 (default) or 320.</summary>
    public CdsTearsheetInfluencerWidth InfluencerWidth
    {
        get => (CdsTearsheetInfluencerWidth)GetValue(InfluencerWidthProperty);
        set => SetValue(InfluencerWidthProperty, value);
    }

    private protected override bool IsWide => true;

    private protected override View? HeaderActionsView => HeaderActions;

    private protected override View? NavigationView => HeaderNavigation;

    private protected override View? InfluencerView => Influencer;

    private protected override CdsTearsheetInfluencerPosition InfluencerSide => InfluencerPosition;

    private protected override CdsTearsheetInfluencerWidth InfluencerSize => InfluencerWidth;
}

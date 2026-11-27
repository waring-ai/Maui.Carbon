namespace Maui.Carbon;

/// <summary>
/// Choose several items from a long or nested list in a wide tearsheet (Carbon for IBM Products' <c>MultiAddSelect</c>):
/// search, optional global filters and sorting, a list with checkboxes, navigation into child items, an optional
/// per-item <see cref="Modifier"/> (a role for each added user), details in a meta panel, and a sidebar that lists what
/// is selected. Use it to add many resources at once (users to a group).
/// </summary>
public class CdsMultiAddSelect : CdsTearsheetShell
{
    /// <summary>Bindable property for <see cref="GlobalSearchLabel"/>.</summary>
    public static readonly BindableProperty GlobalSearchLabelProperty = BindableProperty.Create(
        nameof(GlobalSearchLabel), typeof(string), typeof(CdsMultiAddSelect), "Search",
        propertyChanged: static (b, _, _) => ((CdsMultiAddSelect)b).SyncTexts());

    /// <summary>The search field label.</summary>
    public string GlobalSearchLabel
    {
        get => (string)GetValue(GlobalSearchLabelProperty);
        set => SetValue(GlobalSearchLabelProperty, value);
    }

    /// <summary>Bindable property for <see cref="GlobalSearchPlaceholder"/>.</summary>
    public static readonly BindableProperty GlobalSearchPlaceholderProperty = BindableProperty.Create(
        nameof(GlobalSearchPlaceholder), typeof(string), typeof(CdsMultiAddSelect), null,
        propertyChanged: static (b, _, _) => ((CdsMultiAddSelect)b).SyncTexts());

    /// <summary>The search field placeholder ("Find items").</summary>
    public string? GlobalSearchPlaceholder
    {
        get => (string?)GetValue(GlobalSearchPlaceholderProperty);
        set => SetValue(GlobalSearchPlaceholderProperty, value);
    }

    /// <summary>Bindable property for <see cref="ItemsLabel"/>.</summary>
    public static readonly BindableProperty ItemsLabelProperty = BindableProperty.Create(
        nameof(ItemsLabel), typeof(string), typeof(CdsMultiAddSelect), "Items",
        propertyChanged: static (b, _, _) => ((CdsMultiAddSelect)b).SyncTexts());

    /// <summary>The label over the list, and the first breadcrumb of nested items.</summary>
    public string ItemsLabel
    {
        get => (string)GetValue(ItemsLabelProperty);
        set => SetValue(ItemsLabelProperty, value);
    }

    /// <summary>Bindable property for <see cref="SearchResultsLabel"/>.</summary>
    public static readonly BindableProperty SearchResultsLabelProperty = BindableProperty.Create(
        nameof(SearchResultsLabel), typeof(string), typeof(CdsMultiAddSelect), "Search results",
        propertyChanged: static (b, _, _) => ((CdsMultiAddSelect)b).SyncTexts());

    /// <summary>The label over the list while searching.</summary>
    public string SearchResultsLabel
    {
        get => (string)GetValue(SearchResultsLabelProperty);
        set => SetValue(SearchResultsLabelProperty, value);
    }

    /// <summary>Bindable property for <see cref="NoResultsTitle"/>.</summary>
    public static readonly BindableProperty NoResultsTitleProperty = BindableProperty.Create(
        nameof(NoResultsTitle), typeof(string), typeof(CdsMultiAddSelect), "No results",
        propertyChanged: static (b, _, _) => ((CdsMultiAddSelect)b).SyncTexts());

    /// <summary>The heading when nothing matches.</summary>
    public string NoResultsTitle
    {
        get => (string)GetValue(NoResultsTitleProperty);
        set => SetValue(NoResultsTitleProperty, value);
    }

    /// <summary>Bindable property for <see cref="NoResultsDescription"/>.</summary>
    public static readonly BindableProperty NoResultsDescriptionProperty = BindableProperty.Create(
        nameof(NoResultsDescription), typeof(string), typeof(CdsMultiAddSelect), "Try a different search.",
        propertyChanged: static (b, _, _) => ((CdsMultiAddSelect)b).SyncTexts());

    /// <summary>The text when nothing matches.</summary>
    public string? NoResultsDescription
    {
        get => (string?)GetValue(NoResultsDescriptionProperty);
        set => SetValue(NoResultsDescriptionProperty, value);
    }

    /// <summary>Bindable property for <see cref="NavIconDescription"/>.</summary>
    public static readonly BindableProperty NavIconDescriptionProperty = BindableProperty.Create(
        nameof(NavIconDescription), typeof(string), typeof(CdsMultiAddSelect), "View children",
        propertyChanged: static (b, _, _) => ((CdsMultiAddSelect)b).SyncTexts());

    /// <summary>The accessible name of the button that opens an item's children.</summary>
    public string NavIconDescription
    {
        get => (string)GetValue(NavIconDescriptionProperty);
        set => SetValue(NavIconDescriptionProperty, value);
    }

    /// <summary>Bindable property for <see cref="CloseButtonText"/>.</summary>
    public static readonly BindableProperty CloseButtonTextProperty = BindableProperty.Create(
        nameof(CloseButtonText), typeof(string), typeof(CdsMultiAddSelect), "Cancel",
        propertyChanged: static (b, _, _) => ((CdsMultiAddSelect)b).SyncButtons());

    /// <summary>The secondary button label (Carbon: <c>onCloseButtonText</c>); it closes without choosing.</summary>
    public string CloseButtonText
    {
        get => (string)GetValue(CloseButtonTextProperty);
        set => SetValue(CloseButtonTextProperty, value);
    }

    /// <summary>Bindable property for <see cref="SubmitButtonText"/>.</summary>
    public static readonly BindableProperty SubmitButtonTextProperty = BindableProperty.Create(
        nameof(SubmitButtonText), typeof(string), typeof(CdsMultiAddSelect), "Add",
        propertyChanged: static (b, _, _) => ((CdsMultiAddSelect)b).SyncButtons());

    /// <summary>The primary button label (Carbon: <c>onSubmitButtonText</c>), enabled once something is chosen.</summary>
    public string SubmitButtonText
    {
        get => (string)GetValue(SubmitButtonTextProperty);
        set => SetValue(SubmitButtonTextProperty, value);
    }

    /// <summary>Bindable property for <see cref="InfluencerTitle"/>.</summary>
    public static readonly BindableProperty InfluencerTitleProperty = BindableProperty.Create(
        nameof(InfluencerTitle), typeof(string), typeof(CdsMultiAddSelect), "Selected items",
        propertyChanged: static (b, _, _) => ((CdsMultiAddSelect)b).SyncTexts());

    /// <summary>The heading of the selection sidebar.</summary>
    public string InfluencerTitle
    {
        get => (string)GetValue(InfluencerTitleProperty);
        set => SetValue(InfluencerTitleProperty, value);
    }

    /// <summary>Bindable property for <see cref="NoSelectionTitle"/>.</summary>
    public static readonly BindableProperty NoSelectionTitleProperty = BindableProperty.Create(
        nameof(NoSelectionTitle), typeof(string), typeof(CdsMultiAddSelect), "No items selected",
        propertyChanged: static (b, _, _) => ((CdsMultiAddSelect)b).SyncTexts());

    /// <summary>The sidebar heading before anything is chosen.</summary>
    public string NoSelectionTitle
    {
        get => (string)GetValue(NoSelectionTitleProperty);
        set => SetValue(NoSelectionTitleProperty, value);
    }

    /// <summary>Bindable property for <see cref="NoSelectionDescription"/>.</summary>
    public static readonly BindableProperty NoSelectionDescriptionProperty = BindableProperty.Create(
        nameof(NoSelectionDescription), typeof(string), typeof(CdsMultiAddSelect), null,
        propertyChanged: static (b, _, _) => ((CdsMultiAddSelect)b).SyncTexts());

    /// <summary>The sidebar text before anything is chosen.</summary>
    public string? NoSelectionDescription
    {
        get => (string?)GetValue(NoSelectionDescriptionProperty);
        set => SetValue(NoSelectionDescriptionProperty, value);
    }

    /// <summary>Bindable property for <see cref="MetaIconDescription"/>.</summary>
    public static readonly BindableProperty MetaIconDescriptionProperty = BindableProperty.Create(
        nameof(MetaIconDescription), typeof(string), typeof(CdsMultiAddSelect), "View details",
        propertyChanged: static (b, _, _) => ((CdsMultiAddSelect)b).SyncTexts());

    /// <summary>The accessible name of an item's details button.</summary>
    public string MetaIconDescription
    {
        get => (string)GetValue(MetaIconDescriptionProperty);
        set => SetValue(MetaIconDescriptionProperty, value);
    }

    /// <summary>Bindable property for <see cref="MetaPanelTitle"/>.</summary>
    public static readonly BindableProperty MetaPanelTitleProperty = BindableProperty.Create(
        nameof(MetaPanelTitle), typeof(string), typeof(CdsMultiAddSelect), "Details",
        propertyChanged: static (b, _, _) => ((CdsMultiAddSelect)b).SyncTexts());

    /// <summary>The heading of the details panel.</summary>
    public string MetaPanelTitle
    {
        get => (string)GetValue(MetaPanelTitleProperty);
        set => SetValue(MetaPanelTitleProperty, value);
    }

    /// <summary>Bindable property for <see cref="GlobalFiltersLabel"/>.</summary>
    public static readonly BindableProperty GlobalFiltersLabelProperty = BindableProperty.Create(
        nameof(GlobalFiltersLabel), typeof(string), typeof(CdsMultiAddSelect), "Filters",
        propertyChanged: static (b, _, _) => ((CdsMultiAddSelect)b).SyncTexts());

    /// <summary>The heading of the filter panel.</summary>
    public string GlobalFiltersLabel
    {
        get => (string)GetValue(GlobalFiltersLabelProperty);
        set => SetValue(GlobalFiltersLabelProperty, value);
    }

    /// <summary>Bindable property for <see cref="GlobalFiltersIconDescription"/>.</summary>
    public static readonly BindableProperty GlobalFiltersIconDescriptionProperty = BindableProperty.Create(
        nameof(GlobalFiltersIconDescription), typeof(string), typeof(CdsMultiAddSelect), "Filter",
        propertyChanged: static (b, _, _) => ((CdsMultiAddSelect)b).SyncTexts());

    /// <summary>The accessible name of the filter button.</summary>
    public string GlobalFiltersIconDescription
    {
        get => (string)GetValue(GlobalFiltersIconDescriptionProperty);
        set => SetValue(GlobalFiltersIconDescriptionProperty, value);
    }

    /// <summary>Bindable property for <see cref="GlobalFiltersPlaceholderText"/>.</summary>
    public static readonly BindableProperty GlobalFiltersPlaceholderTextProperty = BindableProperty.Create(
        nameof(GlobalFiltersPlaceholderText), typeof(string), typeof(CdsMultiAddSelect), "Choose an option",
        propertyChanged: static (b, _, _) => ((CdsMultiAddSelect)b).SyncTexts());

    /// <summary>The placeholder of each filter dropdown.</summary>
    public string? GlobalFiltersPlaceholderText
    {
        get => (string?)GetValue(GlobalFiltersPlaceholderTextProperty);
        set => SetValue(GlobalFiltersPlaceholderTextProperty, value);
    }

    /// <summary>Bindable property for <see cref="GlobalFiltersPrimaryButtonText"/>.</summary>
    public static readonly BindableProperty GlobalFiltersPrimaryButtonTextProperty = BindableProperty.Create(
        nameof(GlobalFiltersPrimaryButtonText), typeof(string), typeof(CdsMultiAddSelect), "Apply",
        propertyChanged: static (b, _, _) => ((CdsMultiAddSelect)b).SyncTexts());

    /// <summary>The button that applies the filters.</summary>
    public string GlobalFiltersPrimaryButtonText
    {
        get => (string)GetValue(GlobalFiltersPrimaryButtonTextProperty);
        set => SetValue(GlobalFiltersPrimaryButtonTextProperty, value);
    }

    /// <summary>Bindable property for <see cref="GlobalFiltersSecondaryButtonText"/>.</summary>
    public static readonly BindableProperty GlobalFiltersSecondaryButtonTextProperty = BindableProperty.Create(
        nameof(GlobalFiltersSecondaryButtonText), typeof(string), typeof(CdsMultiAddSelect), "Reset",
        propertyChanged: static (b, _, _) => ((CdsMultiAddSelect)b).SyncTexts());

    /// <summary>The button that resets the filter dropdowns.</summary>
    public string GlobalFiltersSecondaryButtonText
    {
        get => (string)GetValue(GlobalFiltersSecondaryButtonTextProperty);
        set => SetValue(GlobalFiltersSecondaryButtonTextProperty, value);
    }

    /// <summary>Bindable property for <see cref="ClearFiltersText"/>.</summary>
    public static readonly BindableProperty ClearFiltersTextProperty = BindableProperty.Create(
        nameof(ClearFiltersText), typeof(string), typeof(CdsMultiAddSelect), "Clear filters",
        propertyChanged: static (b, _, _) => ((CdsMultiAddSelect)b).SyncTexts());

    /// <summary>The button that removes every applied filter.</summary>
    public string ClearFiltersText
    {
        get => (string)GetValue(ClearFiltersTextProperty);
        set => SetValue(ClearFiltersTextProperty, value);
    }

    /// <summary>Bindable property for <see cref="SortByLabel"/>.</summary>
    public static readonly BindableProperty SortByLabelProperty = BindableProperty.Create(
        nameof(SortByLabel), typeof(string), typeof(CdsMultiAddSelect), "Sort by",
        propertyChanged: static (b, _, _) => ((CdsMultiAddSelect)b).SyncTexts());

    /// <summary>The accessible name of the sort menu (shown with search results or filters).</summary>
    public string SortByLabel
    {
        get => (string)GetValue(SortByLabelProperty);
        set => SetValue(SortByLabelProperty, value);
    }

    /// <summary>Bindable property for <see cref="Modifier"/>.</summary>
    public static readonly BindableProperty ModifierProperty = BindableProperty.Create(
        nameof(Modifier), typeof(CdsAddSelectModifier), typeof(CdsMultiAddSelect), null,
        propertyChanged: static (b, _, _) => ((CdsMultiAddSelect)b).SyncTexts());

    /// <summary>A choice to make for each selected item (a role), shown as an inline dropdown on selected rows.</summary>
    public CdsAddSelectModifier? Modifier
    {
        get => (CdsAddSelectModifier?)GetValue(ModifierProperty);
        set => SetValue(ModifierProperty, value);
    }


    readonly AddSelectBody _body;
    readonly CdsButton _close = new("Cancel", CdsButtonKind.Secondary);
    readonly CdsButton _submit = new("Add", CdsButtonKind.Primary) { IsEnabled = false };

    /// <summary>The entries, at the top level; nest more under <see cref="CdsAddSelectItem.Children"/>.</summary>
    public IList<CdsAddSelectItem> Items { get; } = new CdsItemCollection<CdsAddSelectItem>();

    /// <summary>Raised when the person submits: the chosen items and modifiers. The add-select then closes and resets.</summary>
    public event EventHandler<CdsAddSelectSubmittedEventArgs>? Submitted;

    /// <summary>The body (for tests).</summary>
    internal AddSelectBody AddSelect => _body;

    /// <summary>The submit button (for tests).</summary>
    internal CdsButton SubmitButton => _submit;

    internal void Submit()
    {
        if (!_submit.IsEnabled) return;
        var args = new CdsAddSelectSubmittedEventArgs(_body.SelectedItems, new Dictionary<string, object?>(_body.SelectedItems.ToDictionary(i => i.Id, i => _body.Modifiers.TryGetValue(i.Id, out var m) ? m : null)));
        Submitted?.Invoke(this, args);
        _body.Reset();
        IsOpen = false;
    }

    void SyncButtons()
    {
        _close.Text = CloseButtonText;
        _submit.Text = SubmitButtonText;
    }

    private protected override void OnOpening()
    {
        _body.Items = [.. Items];
        SyncTexts();
        _body.Reset();
    }

    readonly AddSelectSidebar _sidebar;

    /// <summary>The filters offered in the filter panel, one dropdown per attribute.</summary>
    public IList<CdsAddSelectFilter> GlobalFilters { get; } = new CdsItemCollection<CdsAddSelectFilter>();

    /// <summary>The attributes the results can be sorted by (each ascending and descending).</summary>
    public IList<string> GlobalSortBy { get; } = new CdsItemCollection<string>();

    /// <summary>Creates a closed multi add-select.</summary>
    public CdsMultiAddSelect()
    {
        _body = new AddSelectBody(multi: true, new AddSelectTexts());
        _sidebar = new AddSelectSidebar(_body);
        _body.StateChanged += (_, _) => _submit.IsEnabled = _body.SelectedItems.Count > 0;
        Actions.Add(_close);
        Actions.Add(_submit);
        _close.Clicked += (_, _) =>
        {
            if (TryClose()) _body.Reset();
        };
        _submit.Clicked += (_, _) => Submit();
        Body = _body;
        SyncButtons();
    }

    private protected override bool IsWide => true;

    private protected override View? InfluencerView => _sidebar;

    private protected override CdsTearsheetInfluencerPosition InfluencerSide => CdsTearsheetInfluencerPosition.End;

    // Carbon: the add-select sidebar is 22.5rem wide.
    private protected override double? InfluencerWidthOverride => 360;

    void SyncTexts()
    {
        if (_body is null || _sidebar is null) return;
        _body.SetTexts(new AddSelectTexts
        {
            SearchLabel = GlobalSearchLabel,
            SearchPlaceholder = GlobalSearchPlaceholder,
            ItemsLabel = ItemsLabel,
            SearchResultsLabel = SearchResultsLabel,
            NoResultsTitle = NoResultsTitle,
            NoResultsDescription = NoResultsDescription,
            NavIconDescription = NavIconDescription,
            MetaIconDescription = MetaIconDescription,
            FiltersLabel = GlobalFiltersLabel,
            FiltersIconDescription = GlobalFiltersIconDescription,
            FiltersPlaceholder = GlobalFiltersPlaceholderText,
            FiltersApplyText = GlobalFiltersPrimaryButtonText,
            FiltersResetText = GlobalFiltersSecondaryButtonText,
            ClearFiltersText = ClearFiltersText,
            SortByLabel = SortByLabel,
        });
        _body.GlobalFilters = [.. GlobalFilters];
        _body.SortBy = [.. GlobalSortBy];
        _body.Modifier = Modifier;
        _sidebar.InfluencerTitle = InfluencerTitle;
        _sidebar.NoSelectionTitle = NoSelectionTitle;
        _sidebar.NoSelectionDescription = NoSelectionDescription;
        _sidebar.MetaPanelTitle = MetaPanelTitle;
        _sidebar.CloseIconDescription = CloseButtonLabel;
        _sidebar.Modifier = Modifier;
        _sidebar.Rebuild();
    }
}

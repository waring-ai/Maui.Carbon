namespace Maui.Carbon;

/// <summary>
/// Choose one item from a long or nested list in a narrow tearsheet (Carbon for IBM Products' <c>SingleAddSelect</c>):
/// a search over every item, a list with radio buttons, navigation into child items with a breadcrumb, and Add and
/// Cancel buttons. Use it to add one resource to something (a cluster to a project). For a short list use a dropdown;
/// to add several items use <see cref="CdsMultiAddSelect"/>.
/// </summary>
public class CdsSingleAddSelect : CdsTearsheetShell
{
    /// <summary>Bindable property for <see cref="GlobalSearchLabel"/>.</summary>
    public static readonly BindableProperty GlobalSearchLabelProperty = BindableProperty.Create(
        nameof(GlobalSearchLabel), typeof(string), typeof(CdsSingleAddSelect), "Search",
        propertyChanged: static (b, _, _) => ((CdsSingleAddSelect)b).SyncTexts());

    /// <summary>The search field label.</summary>
    public string GlobalSearchLabel
    {
        get => (string)GetValue(GlobalSearchLabelProperty);
        set => SetValue(GlobalSearchLabelProperty, value);
    }

    /// <summary>Bindable property for <see cref="GlobalSearchPlaceholder"/>.</summary>
    public static readonly BindableProperty GlobalSearchPlaceholderProperty = BindableProperty.Create(
        nameof(GlobalSearchPlaceholder), typeof(string), typeof(CdsSingleAddSelect), null,
        propertyChanged: static (b, _, _) => ((CdsSingleAddSelect)b).SyncTexts());

    /// <summary>The search field placeholder ("Find items").</summary>
    public string? GlobalSearchPlaceholder
    {
        get => (string?)GetValue(GlobalSearchPlaceholderProperty);
        set => SetValue(GlobalSearchPlaceholderProperty, value);
    }

    /// <summary>Bindable property for <see cref="ItemsLabel"/>.</summary>
    public static readonly BindableProperty ItemsLabelProperty = BindableProperty.Create(
        nameof(ItemsLabel), typeof(string), typeof(CdsSingleAddSelect), "Items",
        propertyChanged: static (b, _, _) => ((CdsSingleAddSelect)b).SyncTexts());

    /// <summary>The label over the list, and the first breadcrumb of nested items.</summary>
    public string ItemsLabel
    {
        get => (string)GetValue(ItemsLabelProperty);
        set => SetValue(ItemsLabelProperty, value);
    }

    /// <summary>Bindable property for <see cref="SearchResultsLabel"/>.</summary>
    public static readonly BindableProperty SearchResultsLabelProperty = BindableProperty.Create(
        nameof(SearchResultsLabel), typeof(string), typeof(CdsSingleAddSelect), "Search results",
        propertyChanged: static (b, _, _) => ((CdsSingleAddSelect)b).SyncTexts());

    /// <summary>The label over the list while searching.</summary>
    public string SearchResultsLabel
    {
        get => (string)GetValue(SearchResultsLabelProperty);
        set => SetValue(SearchResultsLabelProperty, value);
    }

    /// <summary>Bindable property for <see cref="NoResultsTitle"/>.</summary>
    public static readonly BindableProperty NoResultsTitleProperty = BindableProperty.Create(
        nameof(NoResultsTitle), typeof(string), typeof(CdsSingleAddSelect), "No results",
        propertyChanged: static (b, _, _) => ((CdsSingleAddSelect)b).SyncTexts());

    /// <summary>The heading when nothing matches.</summary>
    public string NoResultsTitle
    {
        get => (string)GetValue(NoResultsTitleProperty);
        set => SetValue(NoResultsTitleProperty, value);
    }

    /// <summary>Bindable property for <see cref="NoResultsDescription"/>.</summary>
    public static readonly BindableProperty NoResultsDescriptionProperty = BindableProperty.Create(
        nameof(NoResultsDescription), typeof(string), typeof(CdsSingleAddSelect), "Try a different search.",
        propertyChanged: static (b, _, _) => ((CdsSingleAddSelect)b).SyncTexts());

    /// <summary>The text when nothing matches.</summary>
    public string? NoResultsDescription
    {
        get => (string?)GetValue(NoResultsDescriptionProperty);
        set => SetValue(NoResultsDescriptionProperty, value);
    }

    /// <summary>Bindable property for <see cref="NavIconDescription"/>.</summary>
    public static readonly BindableProperty NavIconDescriptionProperty = BindableProperty.Create(
        nameof(NavIconDescription), typeof(string), typeof(CdsSingleAddSelect), "View children",
        propertyChanged: static (b, _, _) => ((CdsSingleAddSelect)b).SyncTexts());

    /// <summary>The accessible name of the button that opens an item's children.</summary>
    public string NavIconDescription
    {
        get => (string)GetValue(NavIconDescriptionProperty);
        set => SetValue(NavIconDescriptionProperty, value);
    }

    /// <summary>Bindable property for <see cref="CloseButtonText"/>.</summary>
    public static readonly BindableProperty CloseButtonTextProperty = BindableProperty.Create(
        nameof(CloseButtonText), typeof(string), typeof(CdsSingleAddSelect), "Cancel",
        propertyChanged: static (b, _, _) => ((CdsSingleAddSelect)b).SyncButtons());

    /// <summary>The secondary button label (Carbon: <c>onCloseButtonText</c>); it closes without choosing.</summary>
    public string CloseButtonText
    {
        get => (string)GetValue(CloseButtonTextProperty);
        set => SetValue(CloseButtonTextProperty, value);
    }

    /// <summary>Bindable property for <see cref="SubmitButtonText"/>.</summary>
    public static readonly BindableProperty SubmitButtonTextProperty = BindableProperty.Create(
        nameof(SubmitButtonText), typeof(string), typeof(CdsSingleAddSelect), "Add",
        propertyChanged: static (b, _, _) => ((CdsSingleAddSelect)b).SyncButtons());

    /// <summary>The primary button label (Carbon: <c>onSubmitButtonText</c>), enabled once something is chosen.</summary>
    public string SubmitButtonText
    {
        get => (string)GetValue(SubmitButtonTextProperty);
        set => SetValue(SubmitButtonTextProperty, value);
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

    /// <summary>Creates a closed single add-select.</summary>
    public CdsSingleAddSelect()
    {
        _body = new AddSelectBody(multi: false, new AddSelectTexts());
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

    private protected override bool IsWide => false;

    void SyncTexts()
    {
        if (_body is null) return;
        var t = new AddSelectTexts
        {
            SearchLabel = GlobalSearchLabel,
            SearchPlaceholder = GlobalSearchPlaceholder,
            ItemsLabel = ItemsLabel,
            SearchResultsLabel = SearchResultsLabel,
            NoResultsTitle = NoResultsTitle,
            NoResultsDescription = NoResultsDescription,
            NavIconDescription = NavIconDescription,
        };
        _body.SetTexts(t);
    }
}

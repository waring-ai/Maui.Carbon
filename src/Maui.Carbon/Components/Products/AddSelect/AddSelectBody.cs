namespace Maui.Carbon;

/// <summary>Labels the add-select body reads from its owner.</summary>
internal sealed class AddSelectTexts
{
    public string SearchLabel = "Search";
    public string? SearchPlaceholder;
    public string ItemsLabel = "Items";
    public string SearchResultsLabel = "Search results";
    public string NoResultsTitle = "No results";
    public string? NoResultsDescription;
    public string NavIconDescription = "View children";
    public string MetaIconDescription = "View details";
    public string FiltersLabel = "Filters";
    public string FiltersIconDescription = "Filter";
    public string? FiltersPlaceholder = "Choose an option";
    public string FiltersApplyText = "Apply";
    public string FiltersResetText = "Reset";
    public string ClearFiltersText = "Clear filters";
    public string SortByLabel = "Sort by";
}

/// <summary>
/// The body of an add-select (Carbon for IBM Products' <c>AddSelectBody</c>): search with optional global filters,
/// a breadcrumb or results label with a count, an optional sort menu, and the list of items with radio buttons or
/// checkboxes, navigation into children, a meta button and per-item modifiers.
/// </summary>
internal sealed class AddSelectBody : ContentView
{
    readonly bool _multi;
    AddSelectTexts _texts;
    readonly CdsCoalescedAction _rebuild;
    readonly VerticalStackLayout _root = new() { Spacing = 0 };
    readonly CdsSearch _search = new();
    readonly HashSet<string> _multiSelection = [];
    readonly List<CdsAddSelectItem> _path = [];
    readonly Dictionary<string, string> _filters = [];
    readonly Dictionary<string, object?> _modifiers = [];
    CdsOverlay? _filterPopup;

    public AddSelectBody(bool multi, AddSelectTexts texts)
    {
        _multi = multi;
        _texts = texts;
        _rebuild = new CdsCoalescedAction(this, Rebuild);
        _search.TextChanged += (_, _) => { Search = _search.Text; _rebuild.Request(); };
        Content = _root;
        Rebuild();
    }

    public IList<CdsAddSelectItem> Items { get; set; } = [];

    public void SetTexts(AddSelectTexts texts)
    {
        _texts = texts;
        _rebuild.Request();
    }

    public IList<CdsAddSelectFilter> GlobalFilters { get; set; } = [];

    public IList<string> SortBy { get; set; } = [];

    public CdsAddSelectModifier? Modifier { get; set; }

    public string? Search { get; private set; }

    public string? SingleSelection { get; private set; }

    public IReadOnlyCollection<string> MultiSelection => _multiSelection;

    public IReadOnlyDictionary<string, string> AppliedFilters => _filters;

    public string? SortAttribute { get; private set; }

    public bool SortDescending { get; private set; }

    public CdsAddSelectItem? MetaItem { get; private set; }

    public IReadOnlyList<CdsAddSelectItem> Path => _path;

    /// <summary>Raised when the selection, modifiers or meta item change.</summary>
    public event EventHandler? StateChanged;

    public IReadOnlyList<CdsAddSelectItem> VisibleItems =>
        AddSelectModel.Visible([.. Items], _path, Search, _filters, SortAttribute, SortDescending);

    public IReadOnlyList<CdsAddSelectItem> SelectedItems
    {
        get
        {
            var all = AddSelectModel.Flatten(Items).ToList();
            if (!_multi) return [.. all.Where(i => i.Id == SingleSelection)];
            // In the order they were chosen.
            return [.. _multiSelection.Select(id => all.FirstOrDefault(i => i.Id == id)).OfType<CdsAddSelectItem>()];
        }
    }

    public IReadOnlyDictionary<string, object?> Modifiers => _modifiers;

    public void Reset()
    {
        SingleSelection = null;
        _multiSelection.Clear();
        _search.Text = null;
        Search = null;
        _filters.Clear();
        _path.Clear();
        MetaItem = null;
        SortAttribute = null;
        SortDescending = false;
        _modifiers.Clear();
        foreach (var item in AddSelectModel.Flatten(Items)) _modifiers[item.Id] = item.ModifierValue;
        _filterPopup?.Close();
        Rebuild();
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    internal void Toggle(CdsAddSelectItem item)
    {
        if (_multi)
        {
            if (!_multiSelection.Remove(item.Id)) _multiSelection.Add(item.Id);
        }
        else
        {
            SingleSelection = item.Id;
        }
        _rebuild.Request();
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    internal void Navigate(CdsAddSelectItem item)
    {
        _path.Add(item);
        _rebuild.Request();
    }

    internal void NavigateTo(int depth)
    {
        if (depth < _path.Count) _path.RemoveRange(depth, _path.Count - depth);
        _rebuild.Request();
    }

    internal void ShowMeta(CdsAddSelectItem? item)
    {
        MetaItem = item;
        _rebuild.Request();
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    internal void SetModifier(CdsAddSelectItem item, object? value)
    {
        _modifiers[item.Id] = value;
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    internal void ApplyFilters(IReadOnlyDictionary<string, string> filters)
    {
        _filters.Clear();
        foreach (var f in filters) _filters[f.Key] = f.Value;
        _rebuild.Request();
    }

    internal void SetSort(string attribute, bool descending)
    {
        SortAttribute = attribute;
        SortDescending = descending;
        _rebuild.Request();
    }

    internal bool IsSelected(CdsAddSelectItem item) => _multi ? _multiSelection.Contains(item.Id) : SingleSelection == item.Id;

    void Rebuild()
    {
        _rebuild.Cancel();
        _root.Children.Clear();
        var filtering = !string.IsNullOrEmpty(Search) || _filters.Count > 0;
        var visible = VisibleItems;

        // Header: search (and the filter toggle), applied filters, then the breadcrumb or label with a count and sort.
        var header = new VerticalStackLayout { Spacing = 0, Padding = new Thickness(CdsSpacing.Spacing05, CdsSpacing.Spacing05, CdsSpacing.Spacing05, 0) };
        _search.Label = _texts.SearchLabel;
        _search.Placeholder = _texts.SearchPlaceholder;
        var searchRow = new Grid { ColumnDefinitions = [new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto)] };
        if (_search.Parent is Layout old) old.Remove(_search);
        searchRow.Add(_search, 0);
        var filterOptions = _multi ? GlobalFilters.Where(f => AddSelectModel.FilterOptions(Items, f.Id).Count > 0).ToList() : [];
        if (filterOptions.Count > 0)
        {
            var toggle = new CdsIconButton(CdsIconName.Filter, _texts.FiltersIconDescription, CdsIconButtonKind.Ghost) { Size = CdsSize.Md, VerticalOptions = LayoutOptions.End, TooltipPlacement = CdsPlacement.Left };
            toggle.Clicked += (_, _) => ToggleFilters(searchRow, filterOptions);
            searchRow.Add(toggle, 1);
        }
        header.Add(searchRow);
        if (_filters.Count > 0)
        {
            var applied = new HorizontalStackLayout { Spacing = CdsSpacing.Spacing03, Padding = new Thickness(0, CdsSpacing.Spacing03) };
            foreach (var f in _filters.ToList())
            {
                var tag = new CdsDismissibleTag { Text = $"{f.Key}: {f.Value}", Size = CdsTagSize.Sm, VerticalOptions = LayoutOptions.Center };
                tag.Dismissed += (_, _) => { _filters.Remove(f.Key); _rebuild.Request(); };
                applied.Add(tag);
            }
            var clear = new CdsButton(_texts.ClearFiltersText, CdsButtonKind.Ghost) { Size = CdsSize.Sm };
            clear.Clicked += (_, _) => { _filters.Clear(); _rebuild.Request(); };
            applied.Add(clear);
            header.Add(applied);
        }

        var sub = new Grid { ColumnDefinitions = [new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto)], Margin = new Thickness(0, CdsSpacing.Spacing05, 0, CdsSpacing.Spacing03) };
        var left = new HorizontalStackLayout { Spacing = CdsSpacing.Spacing03, VerticalOptions = LayoutOptions.Center };
        var hierarchical = AddSelectModel.IsHierarchical(Items);
        if (hierarchical && !filtering)
        {
            var crumb = new CdsBreadcrumb { Size = CdsBreadcrumbSize.Sm, NoTrailingSlash = true };
            var root = new CdsBreadcrumbItem { Text = _texts.ItemsLabel, IsCurrentPage = _path.Count == 0 };
            root.Clicked += (_, _) => NavigateTo(0);
            crumb.Items.Add(root);
            for (var i = 0; i < _path.Count; i++)
            {
                var depth = i + 1;
                var item = new CdsBreadcrumbItem { Text = _path[i].Title, IsCurrentPage = depth == _path.Count };
                item.Clicked += (_, _) => NavigateTo(depth);
                crumb.Items.Add(item);
            }
            left.Add(crumb);
        }
        else
        {
            left.Add(new CdsText { Text = filtering && !string.IsNullOrEmpty(Search) ? _texts.SearchResultsLabel : _texts.ItemsLabel, TypeStyle = CdsTypeStyle.HeadingCompact01, VerticalOptions = LayoutOptions.Center });
        }
        // The count shows for flat lists and for search results.
        if (!string.IsNullOrEmpty(Search) || !(hierarchical && _multi))
            left.Add(new CdsTag { Text = visible.Count.ToString(System.Globalization.CultureInfo.CurrentCulture), Type = CdsTagType.Gray, Size = CdsTagSize.Sm, VerticalOptions = LayoutOptions.Center });
        sub.Add(left, 0);
        if (filtering && visible.Count > 0 && SortBy.Count > 0)
        {
            var sort = new CdsOverflowMenu { Icon = CdsIconName.ArrowsVertical, IconDescription = _texts.SortByLabel };
            foreach (var attribute in SortBy)
                foreach (var desc in new[] { false, true })
                {
                    var option = new CdsMenuItem { Text = $"{attribute} {(desc ? "desc" : "asc")}" };
                    option.Clicked += (_, _) => SetSort(attribute, desc);
                    sort.Items.Add(option);
                }
            sub.Add(sort, 1);
        }
        header.Add(sub);
        _root.Add(header);

        if (visible.Count == 0)
        {
            // Carbon: a NotFoundEmptyState in the body, padded 16.
            _root.Add(new CdsNotFoundEmptyState { Title = _texts.NoResultsTitle, Subtitle = _texts.NoResultsDescription, Margin = new Thickness(CdsSpacing.Spacing05) });
            return;
        }

        var list = new VerticalStackLayout { Spacing = 0 };
        CdsSemantics.SetGroup(list, filtering ? _texts.SearchResultsLabel : _texts.ItemsLabel, CdsSemanticGroupKind.List);
        for (var i = 0; i < visible.Count; i++)
            list.Add(new AddSelectRow(this, visible[i], _multi, first: i == 0, _texts, Modifier));
        _root.Add(list);
    }

    void ToggleFilters(View anchor, IReadOnlyList<CdsAddSelectFilter> filters)
    {
        if (_filterPopup is { IsOpen: true })
        {
            _filterPopup.Close();
            return;
        }
        var pending = new Dictionary<string, string>(_filters);
        var grid = new Grid { ColumnDefinitions = [new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star)], ColumnSpacing = CdsSpacing.Spacing07, RowSpacing = CdsSpacing.Spacing05 };
        var reset = new CdsButton(_texts.FiltersResetText, CdsButtonKind.Secondary) { HorizontalOptions = LayoutOptions.Fill };
        var apply = new CdsButton(_texts.FiltersApplyText, CdsButtonKind.Primary) { HorizontalOptions = LayoutOptions.Fill };
        var dropdowns = new List<CdsDropdown>();
        void Sync()
        {
            reset.IsEnabled = pending.Count > 0;
            apply.IsEnabled = pending.Count > 0 || _filters.Count > 0;
        }
        for (var i = 0; i < filters.Count; i++)
        {
            var f = filters[i];
            var dropdown = new CdsDropdown
            {
                Label = f.Label,
                Placeholder = _texts.FiltersPlaceholder,
                ItemsSource = AddSelectModel.FilterOptions(Items, f.Id).ToList(),
                SelectedItem = pending.TryGetValue(f.Id, out var current) ? current : null,
            };
            dropdown.SelectionChanged += (_, _) =>
            {
                if (dropdown.SelectedItem is string s) pending[f.Id] = s;
                else pending.Remove(f.Id);
                Sync();
            };
            dropdowns.Add(dropdown);
            grid.Add(dropdown, i % 2, i / 2);
            if (grid.RowDefinitions.Count <= i / 2) grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        }
        reset.Clicked += (_, _) =>
        {
            pending.Clear();
            foreach (var d in dropdowns) d.SelectedItem = null;
            Sync();
        };
        apply.Clicked += (_, _) =>
        {
            _filterPopup?.Close();
            ApplyFilters(pending);
        };
        Sync();
        var buttons = new Grid { ColumnDefinitions = [new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star)] };
        buttons.Add(reset, 0);
        buttons.Add(apply, 1);
        var content = new VerticalStackLayout { Spacing = 0, WidthRequest = Math.Min(640, Math.Max(320, Width - 2 * CdsSpacing.Spacing05)) };
        var inner = new VerticalStackLayout { Spacing = CdsSpacing.Spacing05, Padding = new Thickness(CdsSpacing.Spacing05) };
        inner.Add(new CdsText { Text = _texts.FiltersLabel, TypeStyle = CdsTypeStyle.Body01 });
        inner.Add(grid);
        content.Add(inner);
        content.Add(buttons);
        CdsTokens.SetColor(content, BackgroundColorProperty, CdsColorKeys.Layer01);
        CdsTokens.SetShadow(content, CdsColorKeys.Shadow, 6, 6);
        CdsSemantics.SetGroup(content, _texts.FiltersLabel, CdsSemanticGroupKind.Group);
        _filterPopup = CdsOverlayService.Open(content, new CdsOverlayOptions { Anchor = anchor, Placement = CdsPlacement.BottomEnd, LightDismiss = true }, this);
    }
}

/// <summary>One row of an add-select list.</summary>
internal sealed class AddSelectRow : CdsControl
{
    readonly AddSelectBody _owner;
    readonly CdsAddSelectItem _item;
    readonly Grid _root = new() { HeightRequest = CdsSpacing.Spacing09 };
    readonly BoxView _fill = new() { InputTransparent = true };
    readonly BoxView _metaEdge = new() { WidthRequest = 2, HorizontalOptions = LayoutOptions.Start, InputTransparent = true };
    readonly CdsIcon _control;
    readonly CdsText _title;
    readonly CdsFocusRing _ring = new(CdsFocusRingStyle.Inset);
    readonly bool _multi;

    public AddSelectRow(AddSelectBody owner, CdsAddSelectItem item, bool multi, bool first, AddSelectTexts texts, CdsAddSelectModifier? modifier)
    {
        _owner = owner;
        _item = item;
        _multi = multi;
        var selected = owner.IsSelected(item);
        var hit = CreateHitTarget(multi ? CdsAccessibilityRole.Checkbox : CdsAccessibilityRole.Radio);
        hit.IsChecked = selected;
        SemanticProperties.SetDescription(hit, string.IsNullOrEmpty(item.Subtitle) ? item.Title : $"{item.Title}, {item.Subtitle}");

        _control = new CdsIcon(multi ? (selected ? CdsIconName.CheckboxCheckedFilled : CdsIconName.Checkbox) : (selected ? CdsIconName.RadioButtonChecked : CdsIconName.RadioButton), 20) { VerticalOptions = LayoutOptions.Center, InputTransparent = true };
        _title = new CdsText { Text = item.Title, TypeStyle = CdsTypeStyle.BodyCompact01, LineBreakMode = LineBreakMode.TailTruncation, MaxLines = 1 };
        var texts2 = new VerticalStackLayout { Spacing = 0, VerticalOptions = LayoutOptions.Center, InputTransparent = true };
        texts2.Add(_title);
        if (!string.IsNullOrEmpty(item.Subtitle))
            texts2.Add(new CdsText { Text = item.Subtitle, TypeStyle = CdsTypeStyle.Label01, Tone = CdsTextTone.Secondary, LineBreakMode = LineBreakMode.TailTruncation, MaxLines = 1 });
        var label = new HorizontalStackLayout { Spacing = 0, Margin = new Thickness(CdsSpacing.Spacing05, 0, 0, 0), VerticalOptions = LayoutOptions.Center, InputTransparent = true };
        if (item.Icon is { } glyph)
        {
            var icon = new CdsIcon(glyph, 16) { Margin = new Thickness(0, 0, CdsSpacing.Spacing03, 0), VerticalOptions = LayoutOptions.Center };
            CdsTokens.SetColor(icon, CdsIcon.ColorProperty, CdsColorKeys.IconPrimary);
            label.Add(icon);
        }
        label.Add(texts2);

        var start = new HorizontalStackLayout { Spacing = 0, VerticalOptions = LayoutOptions.Center, InputTransparent = true };
        start.Add(_control);
        start.Add(label);

        var end = new HorizontalStackLayout { Spacing = 0, HorizontalOptions = LayoutOptions.End, VerticalOptions = LayoutOptions.Center };
        if (multi && modifier is { Options.Count: > 0 })
        {
            if (modifier.IsMultiSelect)
            {
                var ms = new CdsMultiSelect { Label = modifier.Title, Placeholder = modifier.Label, ItemsSource = modifier.Options.ToList(), Type = CdsListBoxType.Inline, IsEnabled = selected, HideLabel = string.IsNullOrEmpty(modifier.Title) };
                if (owner.Modifiers.TryGetValue(item.Id, out var current) && current is IEnumerable<string> values)
                    ms.SelectedItems = values.ToList();
                ms.SelectionChanged += (_, _) => owner.SetModifier(item, ms.SelectedItems?.OfType<string>().ToList() ?? []);
                end.Add(new CdsLayer { Body = ms });
            }
            else
            {
                var dd = new CdsDropdown { Label = modifier.Title, Placeholder = modifier.Label, ItemsSource = modifier.Options.ToList(), Type = CdsListBoxType.Inline, IsEnabled = selected, HideLabel = string.IsNullOrEmpty(modifier.Title) };
                if (owner.Modifiers.TryGetValue(item.Id, out var current)) dd.SelectedItem = current;
                dd.SelectionChanged += (_, _) => owner.SetModifier(item, dd.SelectedItem);
                end.Add(new CdsLayer { Body = dd });
            }
        }
        if (item.Children.Count > 0)
        {
            var nav = new CdsIconButton(CdsIconName.ChevronRight, texts.NavIconDescription, CdsIconButtonKind.Ghost) { Size = CdsSize.Sm, TooltipPlacement = CdsPlacement.Left };
            nav.Clicked += (_, _) => owner.Navigate(item);
            end.Add(nav);
        }
        if (item.Meta.Count > 0)
        {
            var meta = new CdsIconButton(CdsIconName.View, texts.MetaIconDescription, CdsIconButtonKind.Ghost) { Size = CdsSize.Sm, TooltipPlacement = CdsPlacement.Left };
            meta.Clicked += (_, _) => owner.ShowMeta(item);
            end.Add(meta);
        }

        var cells = new Grid { ColumnDefinitions = [new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto)], Padding = new Thickness(CdsSpacing.Spacing05, 0), InputTransparent = true, CascadeInputTransparent = false };
        cells.Add(start, 0);
        cells.Add(end, 1);

        var bottom = new BoxView { HeightRequest = 1, VerticalOptions = LayoutOptions.End, InputTransparent = true };
        CdsTokens.SetColor(bottom, BoxView.ColorProperty, CdsColorKeys.LayerAccent01);
        AutomationProperties.SetExcludedWithChildren(_fill, true);
        _root.Add(_fill);
        _root.Add(hit);
        _root.Add(_ring);
        _root.Add(cells);
        _root.Add(bottom);
        _root.Add(_metaEdge);
        if (first)
        {
            var top = new BoxView { HeightRequest = 1, VerticalOptions = LayoutOptions.Start, InputTransparent = true };
            CdsTokens.SetColor(top, BoxView.ColorProperty, CdsColorKeys.BorderSubtle01);
            _root.Add(top);
        }
        TrackHover();
        SetChrome(_root, fillsWidth: true);
        RefreshVisuals();
    }

    protected override void OnActivated() => _owner.Toggle(_item);

    protected internal override bool OnKeyDown(CdsKeyEventArgs e)
    {
        if (e.Key == CdsKey.Right && _item.Children.Count > 0)
        {
            _owner.Navigate(_item);
            return true;
        }
        return false;
    }

    protected override void ApplyVisuals()
    {
        var selected = _owner.IsSelected(_item);
        var inMeta = ReferenceEquals(_owner.MetaItem, _item);
        var hover = Is(CdsInteractionState.Hover);
        var fill = inMeta ? CdsColorKeys.LayerHover01 : hover ? CdsColorKeys.LayerHover01 : selected ? CdsColorKeys.LayerSelected01 : null;
        SetColor(_fill, BoxView.ColorProperty, fill);
        SetColor(_metaEdge, BoxView.ColorProperty, inMeta ? CdsColorKeys.Interactive : null);
        SetColor(_title, Label.TextColorProperty, hover ? CdsColorKeys.Interactive : CdsColorKeys.TextPrimary);
        SetColor(_control, CdsIcon.ColorProperty, CdsColorKeys.IconPrimary);
        _ring.IsActive = Is(CdsInteractionState.FocusVisible);
    }
}

/// <summary>The multi add-select sidebar: the selected items, or the meta panel of one item.</summary>
internal sealed class AddSelectSidebar : ContentView
{
    readonly AddSelectBody _body;

    public AddSelectSidebar(AddSelectBody body)
    {
        _body = body;
        body.StateChanged += (_, _) => Rebuild();
        Rebuild();
    }

    public string InfluencerTitle { get; set; } = "Selected items";

    public string NoSelectionTitle { get; set; } = "No items selected";

    public string? NoSelectionDescription { get; set; }

    public string MetaPanelTitle { get; set; } = "Details";

    public string CloseIconDescription { get; set; } = "Close";

    public CdsAddSelectModifier? Modifier { get; set; }

    public void Rebuild()
    {
        if (_body.MetaItem is { } metaItem)
        {
            var panel = new VerticalStackLayout { Spacing = 0, Padding = new Thickness(CdsSpacing.Spacing05) };
            var head = new Grid { ColumnDefinitions = [new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto)], Margin = new Thickness(0, 0, 0, CdsSpacing.Spacing05) };
            var title = new CdsText { Text = MetaPanelTitle, TypeStyle = CdsTypeStyle.Heading03, VerticalOptions = LayoutOptions.Center };
            SemanticProperties.SetHeadingLevel(title, SemanticHeadingLevel.Level3);
            head.Add(title, 0);
            var close = new CdsIconButton(CdsIconName.Close, CloseIconDescription, CdsIconButtonKind.Ghost) { Size = CdsSize.Sm, TooltipPlacement = CdsPlacement.Left };
            close.Clicked += (_, _) => _body.ShowMeta(null);
            head.Add(close, 1);
            panel.Add(head);
            foreach (var entry in metaItem.Meta)
            {
                var block = new VerticalStackLayout { Spacing = 0, Margin = new Thickness(0, 0, 0, CdsSpacing.Spacing05) };
                block.Add(new CdsText { Text = entry.Title, TypeStyle = CdsTypeStyle.HeadingCompact01 });
                block.Add(new CdsText { Text = entry.Value, TypeStyle = CdsTypeStyle.BodyCompact01, LineBreakMode = LineBreakMode.WordWrap });
                panel.Add(block);
            }
            Content = panel;
            return;
        }

        var selected = _body.SelectedItems;
        var root = new VerticalStackLayout { Spacing = 0 };
        var header = new HorizontalStackLayout { Spacing = CdsSpacing.Spacing03, Padding = new Thickness(CdsSpacing.Spacing05, CdsSpacing.Spacing07, CdsSpacing.Spacing05, CdsSpacing.Spacing03) };
        var heading = new CdsText { Text = InfluencerTitle, TypeStyle = CdsTypeStyle.HeadingCompact02, VerticalOptions = LayoutOptions.Center };
        SemanticProperties.SetHeadingLevel(heading, SemanticHeadingLevel.Level3);
        header.Add(heading);
        header.Add(new CdsTag { Text = selected.Count.ToString(System.Globalization.CultureInfo.CurrentCulture), Type = CdsTagType.Gray, Size = CdsTagSize.Sm, VerticalOptions = LayoutOptions.Center });
        var headerBox = new Grid();
        headerBox.Add(header);
        var rule = new BoxView { HeightRequest = 1, VerticalOptions = LayoutOptions.End, InputTransparent = true };
        CdsTokens.SetColor(rule, BoxView.ColorProperty, CdsColorKeys.LayerAccent01);
        headerBox.Add(rule);
        root.Add(headerBox);

        if (selected.Count == 0)
        {
            // Carbon: a small NoDataEmptyState, 48 from the top, at most 16rem wide.
            root.Add(new CdsNoDataEmptyState
            {
                Title = NoSelectionTitle,
                Subtitle = NoSelectionDescription,
                Size = CdsEmptyStateSize.Sm,
                MaximumWidthRequest = 256,
                HorizontalOptions = LayoutOptions.Center,
                Margin = new Thickness(CdsSpacing.Spacing05, CdsSpacing.Spacing09, CdsSpacing.Spacing05, 0),
            });
            Content = root;
            return;
        }

        var accordion = new CdsAccordion();
        foreach (var item in selected)
        {
            var title = item.Title;
            if (Modifier is not null && _body.Modifiers.TryGetValue(item.Id, out var mod) && mod is not null)
                title += " · " + (mod is IEnumerable<string> list ? string.Join(", ", list) : mod.ToString());
            var details = new VerticalStackLayout { Spacing = 0 };
            void Add(string key, string? value)
            {
                if (string.IsNullOrEmpty(value)) return;
                details.Add(new CdsText { Text = key, TypeStyle = CdsTypeStyle.Label01, Tone = CdsTextTone.Secondary, Margin = new Thickness(0, 0, 0, CdsSpacing.Spacing03) });
                details.Add(new CdsText { Text = value, TypeStyle = CdsTypeStyle.Body01, LineBreakMode = LineBreakMode.WordWrap, Margin = new Thickness(0, 0, 0, CdsSpacing.Spacing05) });
            }
            Add("title", item.Title);
            Add("subtitle", item.Subtitle);
            Add("value", item.Value);
            foreach (var a in item.Attributes) Add(a.Key, a.Value);
            accordion.Items.Add(new CdsAccordionItem { Title = title, Body = details });
        }
        root.Add(accordion);
        Content = root;
    }
}

using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Maui.Carbon;

/// <summary>
/// The top-of-page header (Carbon v11.99 preview): <c>layer-01</c> with a 1px <c>border-subtle-01</c> bottom border, holding, in order, a
/// <see cref="CdsPageHeaderBreadcrumbBar"/>, a <see cref="CdsPageHeaderContent"/> (title, icon, contextual and page actions, subtitle
/// and body) and a <see cref="CdsPageHeaderTabBar"/> with tags. Use the parts the page needs: a title alone for top-level pages,
/// the breadcrumb bar for nested pages, the tab bar when the page has sections. One per page, at the top of the main content
/// under the shell, never inside a card or dialog (use <see cref="CdsHeading"/> there). The parts align to the 16-column grid by
/// reading the page breakpoint's margin.
/// <para>
/// Left out versus Carbon: collapsing page actions into an overflow menu when space runs out, the "+N" tag overflow popover, and the
/// definition tooltip on a clipped title (a native tooltip with the full title is used).
/// </para>
/// </summary>
[ContentProperty(nameof(Items))]
public class CdsPageHeader : CdsControl
{
    readonly ObservableCollection<View> _items = [];
    readonly VerticalStackLayout _stack = new() { Spacing = 0 };

    /// <summary>Creates an empty page header.</summary>
    public CdsPageHeader()
    {
        HorizontalOptions = LayoutOptions.Fill;
        VerticalOptions = LayoutOptions.Start;
        var fill = new BoxView { InputTransparent = true };
        CdsTokens.SetColor(fill, BoxView.ColorProperty, CdsColorKeys.Layer01);
        var rule = new BoxView { HeightRequest = 1, VerticalOptions = LayoutOptions.End, InputTransparent = true };
        CdsTokens.SetColor(rule, BoxView.ColorProperty, CdsColorKeys.BorderSubtle01);
        var root = new Grid();
        root.Add(fill);
        root.Add(_stack);
        root.Add(rule);
        SetChrome(root);
        _items.CollectionChanged += OnItemsChanged;
    }

    /// <summary>The parts, top to bottom.</summary>
    public IList<View> Items => _items;

    void OnItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null) foreach (View v in e.OldItems) _stack.Remove(v);
        if (e.NewItems is not null) foreach (View v in e.NewItems) _stack.Add(v);
    }
}

/// <summary>
/// The 40 high bar at the top of the page header: a breadcrumb (usually small), an optional 16px <see cref="Icon"/> before it, and
/// on the end <see cref="ContentActions"/> (for example a medium button) and <see cref="PageActions"/> (usually medium ghost icon
/// buttons). It has its own 1px <c>border-subtle-01</c> bottom border (<see cref="HasBorder"/>; turn it off when nothing follows
/// the bar). Use 40 high (md) buttons here.
/// </summary>
[ContentProperty(nameof(Breadcrumb))]
public class CdsPageHeaderBreadcrumbBar : CdsControl
{
    /// <summary>Bindable property for <see cref="Breadcrumb"/>.</summary>
    public static readonly BindableProperty BreadcrumbProperty = BindableProperty.Create(
        nameof(Breadcrumb), typeof(View), typeof(CdsPageHeaderBreadcrumbBar), null,
        propertyChanged: static (b, o, n) => ((CdsPageHeaderBreadcrumbBar)b).Place((View?)o, (View?)n, ((CdsPageHeaderBreadcrumbBar)b)._crumbHost));

    /// <summary>Bindable property for <see cref="Icon"/>.</summary>
    public static readonly BindableProperty IconProperty = BindableProperty.Create(
        nameof(Icon), typeof(CdsIconName?), typeof(CdsPageHeaderBreadcrumbBar), null,
        propertyChanged: static (b, _, _) => ((CdsPageHeaderBreadcrumbBar)b).Update());

    /// <summary>Bindable property for <see cref="ContentActions"/>.</summary>
    public static readonly BindableProperty ContentActionsProperty = BindableProperty.Create(
        nameof(ContentActions), typeof(View), typeof(CdsPageHeaderBreadcrumbBar), null,
        propertyChanged: static (b, o, n) => ((CdsPageHeaderBreadcrumbBar)b).Place((View?)o, (View?)n, ((CdsPageHeaderBreadcrumbBar)b)._contentActionsHost));

    /// <summary>Bindable property for <see cref="PageActions"/>.</summary>
    public static readonly BindableProperty PageActionsProperty = BindableProperty.Create(
        nameof(PageActions), typeof(View), typeof(CdsPageHeaderBreadcrumbBar), null,
        propertyChanged: static (b, o, n) => ((CdsPageHeaderBreadcrumbBar)b).Place((View?)o, (View?)n, ((CdsPageHeaderBreadcrumbBar)b)._pageActionsHost));

    /// <summary>Bindable property for <see cref="HasBorder"/>.</summary>
    public static readonly BindableProperty HasBorderProperty = BindableProperty.Create(
        nameof(HasBorder), typeof(bool), typeof(CdsPageHeaderBreadcrumbBar), true,
        propertyChanged: static (b, _, _) => ((CdsPageHeaderBreadcrumbBar)b).Update());

    /// <summary>Bindable property for <see cref="ContentActionsFlush"/>.</summary>
    public static readonly BindableProperty ContentActionsFlushProperty = BindableProperty.Create(
        nameof(ContentActionsFlush), typeof(bool), typeof(CdsPageHeaderBreadcrumbBar), false,
        propertyChanged: static (b, _, _) => ((CdsPageHeaderBreadcrumbBar)b).Update());

    /// <summary>Bindable property for <see cref="PageActionsFlush"/>.</summary>
    public static readonly BindableProperty PageActionsFlushProperty = BindableProperty.Create(
        nameof(PageActionsFlush), typeof(bool), typeof(CdsPageHeaderBreadcrumbBar), false,
        propertyChanged: static (b, _, _) => ((CdsPageHeaderBreadcrumbBar)b).Update());

    readonly Grid _row = new() { HeightRequest = PageHeaderModel.BreadcrumbBarHeight, ColumnSpacing = 0 };
    readonly CdsIcon _icon = new() { IsVisible = false, VerticalOptions = LayoutOptions.Center, Margin = new Thickness(0, 0, CdsSpacing.Spacing03, 0) };
    readonly ContentView _crumbHost = new() { VerticalOptions = LayoutOptions.Center };
    readonly ContentView _contentActionsHost = new() { VerticalOptions = LayoutOptions.Center, Margin = new Thickness(0, 0, CdsSpacing.Spacing04, 0) };
    readonly ContentView _pageActionsHost = new() { VerticalOptions = LayoutOptions.Center };
    readonly BoxView _rule = new() { HeightRequest = 1, VerticalOptions = LayoutOptions.End, InputTransparent = true };
    readonly CdsBreakpointWatcher _watcher;

    /// <summary>Creates an empty bar.</summary>
    public CdsPageHeaderBreadcrumbBar()
    {
        HorizontalOptions = LayoutOptions.Fill;
        for (var i = 0; i < 5; i++)
            _row.ColumnDefinitions.Add(new ColumnDefinition(i == 2 ? GridLength.Star : GridLength.Auto));
        _row.Add(_icon, 0);
        _row.Add(_crumbHost, 1);
        _row.Add(_contentActionsHost, 3);
        _row.Add(_pageActionsHost, 4);
        CdsTokens.SetColor(_rule, BoxView.ColorProperty, CdsColorKeys.BorderSubtle01);
        var root = new Grid();
        root.Add(_row);
        root.Add(_rule);
        SetChrome(root);
        _watcher = new CdsBreakpointWatcher(this, Update);
        Update();
    }

    /// <summary>The breadcrumb (usually <see cref="CdsBreadcrumbSize.Sm"/>).</summary>
    public View? Breadcrumb
    {
        get => (View?)GetValue(BreadcrumbProperty);
        set => SetValue(BreadcrumbProperty, value);
    }

    /// <summary>A 16px icon before the breadcrumb.</summary>
    public CdsIconName? Icon
    {
        get => (CdsIconName?)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>Content actions at the end, such as a medium button.</summary>
    public View? ContentActions
    {
        get => (View?)GetValue(ContentActionsProperty);
        set => SetValue(ContentActionsProperty, value);
    }

    /// <summary>Page actions at the end, usually medium ghost icon buttons.</summary>
    public View? PageActions
    {
        get => (View?)GetValue(PageActionsProperty);
        set => SetValue(PageActionsProperty, value);
    }

    /// <summary>The bar's own 1px bottom border. Default <c>true</c>; turn it off when nothing follows the bar.</summary>
    public bool HasBorder
    {
        get => (bool)GetValue(HasBorderProperty);
        set => SetValue(HasBorderProperty, value);
    }

    /// <summary>Content actions touch the end edge (no end padding).</summary>
    public bool ContentActionsFlush
    {
        get => (bool)GetValue(ContentActionsFlushProperty);
        set => SetValue(ContentActionsFlushProperty, value);
    }

    /// <summary>Page actions touch the end edge (no end padding).</summary>
    public bool PageActionsFlush
    {
        get => (bool)GetValue(PageActionsFlushProperty);
        set => SetValue(PageActionsFlushProperty, value);
    }

    void Place(View? oldView, View? newView, ContentView host) => host.Content = newView;

    void Update()
    {
        if (_watcher is null) return;
        var bp = _watcher.Current;
        _icon.Icon = Icon;
        _icon.IsVisible = Icon is not null;
        _rule.IsVisible = HasBorder;
        var flush = PageActions is null ? ContentActionsFlush : PageActionsFlush;
        _row.Padding = new Thickness(PageHeaderModel.Inset(bp), 0, PageHeaderModel.BarEndInset(bp, flush), 0);
        _contentActionsHost.Margin = new Thickness(0, 0, PageActions is null ? 0 : CdsSpacing.Spacing04, 0);
    }
}

/// <summary>
/// The page actions of <see cref="CdsPageHeaderContent"/>: views (buttons) in a row that right-aligns in the title row (left-aligned
/// when the actions stack under the title on small screens). Wrap even a single button in it. Use at most one primary action and
/// ghost icon-only buttons (with a label) for the rest.
/// </summary>
public class CdsPageHeaderActions : HorizontalStackLayout
{
    /// <summary>Creates an empty action row.</summary>
    public CdsPageHeaderActions()
    {
        Spacing = 0;
        HorizontalOptions = LayoutOptions.End;
    }
}

/// <summary>
/// The body of the page header content: a <see cref="Subtitle"/> in heading-03 (20) and body text in body-01 (14), both up to 640 wide.
/// </summary>
[ContentProperty(nameof(Text))]
public class CdsPageHeaderContentText : CdsControl
{
    /// <summary>Bindable property for <see cref="Subtitle"/>.</summary>
    public static readonly BindableProperty SubtitleProperty = BindableProperty.Create(
        nameof(Subtitle), typeof(string), typeof(CdsPageHeaderContentText), null,
        propertyChanged: static (b, _, _) => ((CdsPageHeaderContentText)b).Update());

    /// <summary>Bindable property for <see cref="Text"/>.</summary>
    public static readonly BindableProperty TextProperty = BindableProperty.Create(
        nameof(Text), typeof(string), typeof(CdsPageHeaderContentText), null,
        propertyChanged: static (b, _, _) => ((CdsPageHeaderContentText)b).Update());

    readonly CdsText _subtitle = new("", CdsTypeStyle.Heading03) { LineBreakMode = LineBreakMode.WordWrap, Margin = new Thickness(0, 0, 0, CdsSpacing.Spacing03) };
    readonly CdsText _body = new("", CdsTypeStyle.Body01) { LineBreakMode = LineBreakMode.WordWrap, Margin = new Thickness(0, CdsSpacing.Spacing03, 0, 0) };

    /// <summary>Creates empty text.</summary>
    public CdsPageHeaderContentText()
    {
        HorizontalOptions = LayoutOptions.Start;
        var stack = new VerticalStackLayout { Spacing = 0, MaximumWidthRequest = PageHeaderModel.TextMaxWidth };
        stack.Add(_subtitle);
        stack.Add(_body);
        SetChrome(stack);
        Update();
    }

    /// <summary>The subtitle (a short description of the object: "Checkout service, US storefront").</summary>
    public string? Subtitle
    {
        get => (string?)GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }

    /// <summary>The body text.</summary>
    public string? Text
    {
        get => (string?)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    void Update()
    {
        _subtitle.Text = Subtitle;
        _subtitle.IsVisible = !string.IsNullOrEmpty(Subtitle);
        _body.Text = Text;
        _body.IsVisible = !string.IsNullOrEmpty(Text);
        if (!_subtitle.IsVisible) _body.Margin = new Thickness(0);
        else _body.Margin = new Thickness(0, CdsSpacing.Spacing03, 0, 0);
    }
}

/// <summary>
/// The title area of the page header: 24 of vertical padding, the <see cref="Title"/> in heading-04 (28, clamped to two lines and
/// 640 wide; one line when <see cref="ContextualActions"/> such as a status tag share the row), an optional 32px
/// <see cref="Icon"/>, and <see cref="PageActions"/> (a <see cref="CdsPageHeaderActions"/>) right-aligned with a 16 gap (below 672 they
/// stack under the title). The body (usually a <see cref="CdsPageHeaderContentText"/>) follows. The title is the name of the
/// page or object in sentence case and is exposed as a level 4 heading; a clipped title gets a tooltip with its full text.
/// </summary>
[ContentProperty(nameof(Body))]
public class CdsPageHeaderContent : CdsContainer
{
    /// <summary>Bindable property for <see cref="Title"/>.</summary>
    public static readonly BindableProperty TitleProperty = BindableProperty.Create(
        nameof(Title), typeof(string), typeof(CdsPageHeaderContent), null,
        propertyChanged: static (b, _, _) => ((CdsPageHeaderContent)b).Update());

    /// <summary>Bindable property for <see cref="Icon"/>.</summary>
    public static readonly BindableProperty IconProperty = BindableProperty.Create(
        nameof(Icon), typeof(CdsIconName?), typeof(CdsPageHeaderContent), null,
        propertyChanged: static (b, _, _) => ((CdsPageHeaderContent)b).Update());

    /// <summary>Bindable property for <see cref="ContextualActions"/>.</summary>
    public static readonly BindableProperty ContextualActionsProperty = BindableProperty.Create(
        nameof(ContextualActions), typeof(View), typeof(CdsPageHeaderContent), null,
        propertyChanged: static (b, o, n) =>
        {
            var content = (CdsPageHeaderContent)b;
            content._contextualHost.Content = (View?)n;
            content.Update();
        });

    /// <summary>Bindable property for <see cref="PageActions"/>.</summary>
    public static readonly BindableProperty PageActionsProperty = BindableProperty.Create(
        nameof(PageActions), typeof(View), typeof(CdsPageHeaderContent), null,
        propertyChanged: static (b, o, n) =>
        {
            var content = (CdsPageHeaderContent)b;
            content._actionsHost.Content = (View?)n;
            content.Update();
        });

    readonly VerticalStackLayout _stack = new() { Spacing = 0 };
    readonly Grid _titleRow = new() { ColumnSpacing = CdsSpacing.Spacing05, RowSpacing = CdsSpacing.Spacing05, MinimumHeightRequest = CdsSizing.Medium, Margin = new Thickness(0, 0, 0, CdsSpacing.Spacing05) };
    readonly HorizontalStackLayout _start = new() { Spacing = CdsSpacing.Spacing05 };
    readonly CdsIcon _icon = new() { Size = 32, IsVisible = false, VerticalOptions = LayoutOptions.Start };
    readonly CdsText _title = new("", CdsTypeStyle.Heading04) { LineBreakMode = LineBreakMode.TailTruncation, MaxLines = 2, MaximumWidthRequest = PageHeaderModel.TextMaxWidth, VerticalOptions = LayoutOptions.Center };
    readonly ContentView _contextualHost = new() { VerticalOptions = LayoutOptions.Center };
    readonly ContentView _actionsHost = new() { VerticalOptions = LayoutOptions.Start };
    readonly ContentView _bodyHost = new();
    readonly CdsBreakpointWatcher _watcher;

    /// <summary>Creates empty content.</summary>
    public CdsPageHeaderContent()
    {
        HorizontalOptions = LayoutOptions.Fill;
        _icon.Margin = new Thickness(0, 0, 0, 0);
        _start.Add(_icon);
        _start.Add(_title);
        _start.Add(_contextualHost);
        _titleRow.Add(_start);
        _titleRow.Add(_actionsHost);
        _stack.Add(_titleRow);
        _stack.Add(_bodyHost);
        SemanticProperties.SetHeadingLevel(_title, SemanticHeadingLevel.Level4);
        SetChrome(_stack);
        _watcher = new CdsBreakpointWatcher(this, Update);
        Update();
    }

    /// <summary>The page or object name (required). Sentence case; a resource name stays as typed.</summary>
    public string? Title
    {
        get => (string?)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>A 32px icon before the title.</summary>
    public CdsIconName? Icon
    {
        get => (CdsIconName?)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>Content after the title, such as a large status <see cref="CdsTag"/>. It limits the title to one line.</summary>
    public View? ContextualActions
    {
        get => (View?)GetValue(ContextualActionsProperty);
        set => SetValue(ContextualActionsProperty, value);
    }

    /// <summary>The page actions (a <see cref="CdsPageHeaderActions"/> of medium buttons), right-aligned.</summary>
    public View? PageActions
    {
        get => (View?)GetValue(PageActionsProperty);
        set => SetValue(PageActionsProperty, value);
    }

    /// <inheritdoc/>
    protected override void OnBodyChanged(View? oldBody, View? newBody) => _bodyHost.Content = newBody;

    void Update()
    {
        if (_watcher is null) return;
        var bp = _watcher.Current;
        _title.Text = Title;
        ToolTipProperties.SetText(_title, Title ?? string.Empty);
        _title.MaxLines = PageHeaderModel.TitleMaxLines(ContextualActions is not null);
        _icon.Icon = Icon;
        _icon.IsVisible = Icon is not null;
        _contextualHost.IsVisible = ContextualActions is not null;

        var inset = PageHeaderModel.Inset(bp);
        _stack.Padding = new Thickness(inset, CdsSpacing.Spacing06, inset, CdsSpacing.Spacing06);

        _titleRow.ColumnDefinitions.Clear();
        _titleRow.RowDefinitions.Clear();
        if (PageHeaderModel.StacksActions(bp))
        {
            _titleRow.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            _titleRow.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            Grid.SetRow(_start, 0);
            Grid.SetColumn(_start, 0);
            Grid.SetRow(_actionsHost, 1);
            Grid.SetColumn(_actionsHost, 0);
            _actionsHost.HorizontalOptions = LayoutOptions.Start;
        }
        else
        {
            _titleRow.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
            _titleRow.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            Grid.SetRow(_start, 0);
            Grid.SetColumn(_start, 0);
            Grid.SetRow(_actionsHost, 0);
            Grid.SetColumn(_actionsHost, 1);
            _actionsHost.HorizontalOptions = LayoutOptions.End;
        }
        _actionsHost.IsVisible = PageActions is not null;
    }
}

/// <summary>
/// The tab bar of the page header: a <see cref="CdsTabs"/> strip (set <see cref="Items"/>; the tab panels belong below the header, so
/// do not set <see cref="CdsTab.Body"/>) pulled 16 to the start so the labels line up with the title, and on the end the
/// <see cref="Tags"/> (64 after the tabs). Switch your panels from <see cref="SelectionChanged"/>. The strip is a tab list named
/// <see cref="Label"/> ("Page tabs") with the usual keys: Left and Right move and select (skipping disabled tabs), Home and End jump.
/// </summary>
[ContentProperty(nameof(Items))]
public class CdsPageHeaderTabBar : CdsControl
{
    /// <summary>Bindable property for <see cref="SelectedIndex"/>.</summary>
    public static readonly BindableProperty SelectedIndexProperty = BindableProperty.Create(
        nameof(SelectedIndex), typeof(int), typeof(CdsPageHeaderTabBar), 0, BindingMode.TwoWay,
        propertyChanged: static (b, _, n) => ((CdsPageHeaderTabBar)b)._tabs.SelectedIndex = (int)n);

    /// <summary>Bindable property for <see cref="Label"/>.</summary>
    public static readonly BindableProperty LabelProperty = BindableProperty.Create(
        nameof(Label), typeof(string), typeof(CdsPageHeaderTabBar), "Page tabs",
        propertyChanged: static (b, _, n) => ((CdsPageHeaderTabBar)b)._tabs.Label = (string)n);

    readonly CdsTabs _tabs = new() { PanelPadding = new Thickness(0), HorizontalOptions = LayoutOptions.Start };
    readonly ObservableCollection<CdsTag> _tags = [];
    readonly HorizontalStackLayout _tagRow = new() { Spacing = CdsSpacing.Spacing03, HorizontalOptions = LayoutOptions.End, VerticalOptions = LayoutOptions.Center };
    readonly Grid _row = new() { ColumnSpacing = PageHeaderModel.TabTagGap };
    readonly CdsBreakpointWatcher _watcher;

    /// <summary>Creates an empty tab bar.</summary>
    public CdsPageHeaderTabBar()
    {
        HorizontalOptions = LayoutOptions.Fill;
        _tabs.Label = Label;
        _row.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        _row.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        _row.Add(_tabs, 0);
        _row.Add(_tagRow, 1);
        SetChrome(_row);
        _tabs.SelectionChanged += (_, e) =>
        {
            if (SelectedIndex != e.NewIndex) SetValue(SelectedIndexProperty, e.NewIndex);
            SelectionChanged?.Invoke(this, e);
        };
        _tags.CollectionChanged += OnTagsChanged;
        _watcher = new CdsBreakpointWatcher(this, Update);
        Update();
    }

    /// <summary>Raised when the selected tab changes.</summary>
    public event EventHandler<CdsTabsSelectionChangedEventArgs>? SelectionChanged;

    /// <summary>The tabs, in order.</summary>
    public IList<CdsTab> Items => _tabs.Items;

    /// <summary>Tags aligned to the end of the tab row (for example a status or environment tag).</summary>
    public IList<CdsTag> Tags => _tags;

    /// <summary>The selected tab (two-way). Default 0.</summary>
    public int SelectedIndex
    {
        get => (int)GetValue(SelectedIndexProperty);
        set => SetValue(SelectedIndexProperty, value);
    }

    /// <summary>The accessible name of the tab list. Default "Page tabs".</summary>
    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    void OnTagsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null) foreach (CdsTag t in e.OldItems) _tagRow.Remove(t);
        if (e.NewItems is not null) foreach (CdsTag t in e.NewItems) _tagRow.Add(t);
    }

    void Update()
    {
        if (_watcher is null) return;
        var inset = PageHeaderModel.Inset(_watcher.Current);
        // The strip pulls 16 to the start: its first label lines up with the title.
        _row.Padding = new Thickness(inset + PageHeaderModel.TabBarPull, 0, inset, 0);
    }
}

/// <summary>
/// A hero image for landing pages: a box in a 2 x 1 ratio (3 x 2 below the lg breakpoint) that clips its content (an
/// <c>Image</c> with <c>Aspect="AspectFill"</c>). Put it in a grid column beside the column that holds the breadcrumb bar and content.
/// </summary>
[ContentProperty(nameof(Image))]
public class CdsPageHeaderHeroImage : CdsControl
{
    /// <summary>Bindable property for <see cref="Image"/>.</summary>
    public static readonly BindableProperty ImageProperty = BindableProperty.Create(
        nameof(Image), typeof(View), typeof(CdsPageHeaderHeroImage), null,
        propertyChanged: static (b, o, n) =>
        {
            var hero = (CdsPageHeaderHeroImage)b;
            if (o is View old) hero._box.Remove(old);
            if (n is View view) hero._box.Add(view);
        });

    readonly CdsAspectRatio _box = new() { CustomRatio = 2 };
    readonly CdsBreakpointWatcher _watcher;

    /// <summary>Creates an empty hero image box.</summary>
    public CdsPageHeaderHeroImage()
    {
        HorizontalOptions = LayoutOptions.Fill;
        SetChrome(_box);
        _watcher = new CdsBreakpointWatcher(this, Update);
        Update();
    }

    /// <summary>The image (or any view) that fills the box.</summary>
    public View? Image
    {
        get => (View?)GetValue(ImageProperty);
        set => SetValue(ImageProperty, value);
    }

    void Update()
    {
        if (_watcher is null) return;
        _box.CustomRatio = PageHeaderModel.HeroRatio(_watcher.Current);
    }
}

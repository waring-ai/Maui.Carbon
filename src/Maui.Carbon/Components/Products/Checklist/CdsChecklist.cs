using System.Collections.Specialized;

namespace Maui.Carbon;

/// <summary>
/// An onboarding checklist that tracks a person's progress through a set of tasks (Carbon for IBM Products'
/// <c>Checklist</c>). The header shows an optional 40px progress ring with its <see cref="ChartLabel"/>, the
/// <c>heading-compact-02</c> <see cref="Title"/> and a ghost chevron toggle that collapses the list. The body holds
/// one or more <see cref="TaskLists"/>, each an optional section label over its tasks, every task a status glyph and a
/// label (a link when the task has an action). <see cref="ViewAllLabel"/> adds a "View all" link at the bottom.
/// </summary>
[ContentProperty(nameof(TaskLists))]
public partial class CdsChecklist : CdsControl
{
    readonly Grid _root = new() { RowSpacing = 0 };
    readonly Grid _header = new() { ColumnSpacing = CdsSpacing.Spacing03, RowSpacing = 0, Padding = CdsSpacing.Spacing05, IsVisible = false };
    readonly BoxView _headerRule = new() { HeightRequest = 1, InputTransparent = true, IsVisible = false };
    readonly VerticalStackLayout _titles = new() { Spacing = 0, VerticalOptions = LayoutOptions.Center };
    readonly CdsText _title = new() { TypeStyle = CdsTypeStyle.HeadingCompact02, IsVisible = false };
    readonly CdsText _chartLabel = new() { TypeStyle = CdsTypeStyle.Label01, IsVisible = false };
    readonly Grid _contentClip = new() { IsClippedToBounds = true };
    readonly VerticalStackLayout _content = new() { Spacing = 0 };
    readonly VerticalStackLayout _body = new() { Spacing = 0 };
    readonly CdsItemCollection<CdsChecklistTaskList> _taskLists = [];
    readonly CdsCoalescedAction _rebuild;
    ChecklistChart? _chart;
    CdsActionFace? _toggle;
    CdsIconTooltip? _toggleTooltip;
    Grid? _footer;
    ChecklistLink? _viewAll;
    int _version;

    /// <summary>Creates an open, empty checklist.</summary>
    public CdsChecklist()
    {
        _rebuild = new CdsCoalescedAction(this, RebuildBody);
        _header.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        _header.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        _header.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        _titles.Add(_title);
        _titles.Add(_chartLabel);
        _header.Add(_titles, 1);
        SemanticProperties.SetHeadingLevel(_title, SemanticHeadingLevel.Level2);

        _content.Add(_body);
        _contentClip.Add(_content);
        _root.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        _root.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        _root.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        _root.Add(_header, 0, 0);
        _root.Add(_headerRule, 0, 1);
        _root.Add(_contentClip, 0, 2);
        _taskLists.CollectionChanged += OnTaskListsChanged;
        SetChrome(_root);
        UpdateHeader();
        UpdateFooter();
        UpdateGroupName();
        RebuildBody();
        ApplyOpen();
    }

    /// <summary>Raised when <see cref="IsOpen"/> changes (Carbon's <c>onToggle</c>).</summary>
    public event EventHandler<CdsChoiceChangedEventArgs<bool>>? OpenChanged;

    /// <summary>Raised when the "View all" link is activated (Carbon's <c>onClickViewAll</c>), before <see cref="ViewAllCommand"/> runs.</summary>
    public event EventHandler? ViewAllClicked;

    /// <summary>The task lists, in order (Carbon's <c>taskLists</c>). Each list has an optional title and its tasks.</summary>
    public IList<CdsChecklistTaskList> TaskLists => _taskLists;

    /// <summary>The collapse toggle, while the header shows it (for tests).</summary>
    internal CdsHitTarget? ToggleTarget => _toggle is { IsVisible: true } t && _header.IsVisible ? t.Hit : null;

    /// <inheritdoc/>
    protected override void ApplyVisuals()
    {
        SetColor(_header, BackgroundColorProperty, CdsChecklistTokens.Background);
        SetColor(_content, BackgroundColorProperty, CdsChecklistTokens.Background);
        SetColor(_headerRule, BoxView.ColorProperty, CdsChecklistTokens.Rule);
        SetColor(_title, Label.TextColorProperty, CdsColorKeys.TextPrimary);
        SetColor(_chartLabel, Label.TextColorProperty, CdsColorKeys.TextSecondary);
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
        if (propertyName == SemanticProperties.DescriptionProperty.PropertyName) UpdateGroupName();
    }

    // The checklist is a named region ("Checklist" unless the app names it with SemanticProperties.Description).
    void UpdateGroupName()
    {
        var own = SemanticProperties.GetDescription(this);
        CdsSemantics.SetGroup(_root, string.IsNullOrWhiteSpace(own) ? "Checklist" : null);
    }

    void OnTaskListsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
            foreach (CdsChecklistTaskList list in e.OldItems)
            {
                list.Changed -= OnListChanged;
                list.PropertyChanged -= OnListPropertyChanged;
            }
        if (e.NewItems is not null)
            foreach (CdsChecklistTaskList list in e.NewItems)
            {
                list.Changed += OnListChanged;
                list.PropertyChanged += OnListPropertyChanged;
            }
        _rebuild.Request();
    }

    void OnListChanged(object? sender, EventArgs e) => _rebuild.Request();

    void OnListPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e) => _rebuild.Request();

    void UpdateHeader()
    {
        var showsChart = ChecklistModel.ShowsChart(ChartValue, ChartLabel);
        var showsHeader = ChecklistModel.ShowsHeader(Title, showsChart);
        _header.IsVisible = showsHeader;
        _headerRule.IsVisible = showsHeader;

        if (showsChart && _chart is null)
        {
            _chart = new ChecklistChart();
            _header.Insert(0, _chart);
            Grid.SetColumn(_chart, 0);
        }
        if (_chart is not null)
        {
            _chart.IsVisible = showsChart;
            _chart.Value = ChartValue ?? 0;
            SemanticProperties.SetDescription(_chart, ChartLabel);
        }
        _header.ColumnDefinitions[0].Width = showsChart ? GridLength.Auto : new GridLength(0);
        _header.ColumnSpacing = showsChart ? CdsSpacing.Spacing03 : 0;

        _title.Text = Title;
        _title.IsVisible = !string.IsNullOrEmpty(Title);
        _chartLabel.Text = ChartLabel;
        _chartLabel.IsVisible = showsChart;
        _chartLabel.Margin = new Thickness(0, _title.IsVisible ? CdsSpacing.Spacing01 : 0, 0, 0);

        if (ShowToggle && _toggle is null)
        {
            _toggle = new CdsActionFace
            {
                Mode = CdsActionFaceMode.IconOnly,
                ButtonHeight = CdsSize.Sm.Height(),
                Icon = CdsIconName.ChevronDown,
                RotateIconWhenOpen = true,
                Expandable = true,
                RingStyle = CdsFocusRingStyle.Inset,
                Resolver = static state => CdsIconButtonTokens.Resolve(CdsIconButtonKind.Ghost, state),
                VerticalOptions = LayoutOptions.Start,
                // margin-block-start: -8; margin-inline: auto -8 (pulled into the header's padding).
                Margin = new Thickness(0, -CdsSpacing.Spacing03, -CdsSpacing.Spacing03, 0),
            };
            _toggle.Activated += (_, _) => IsOpen = !IsOpen;
            _toggleTooltip = new CdsIconTooltip(this, _toggle);
            _toggle.StateChanged += (_, _) => _toggleTooltip.StateChanged(_toggle);
            _header.Add(_toggle, 2);
        }
        if (_toggle is not null)
        {
            _toggle.IsVisible = ShowToggle;
            _toggle.IsOpen = IsOpen;
            _toggle.AccessibleName = string.IsNullOrEmpty(ToggleDescription) ? "Checklist toggle" : ToggleDescription;
            _toggleTooltip!.Text = string.IsNullOrEmpty(ToggleLabel) ? "Toggle" : ToggleLabel;
            _toggleTooltip.Placement = TogglePlacement;
        }
        RefreshVisuals();
    }

    void UpdateFooter()
    {
        var show = !string.IsNullOrEmpty(ViewAllLabel);
        if (show && _footer is null)
        {
            _footer = new Grid { RowSpacing = 0 };
            _footer.RowDefinitions.Add(new RowDefinition(new GridLength(1)));
            _footer.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            var rule = new BoxView { HeightRequest = 1, InputTransparent = true };
            CdsTokens.SetColor(rule, BoxView.ColorProperty, CdsChecklistTokens.Rule);
            _viewAll = new ChecklistLink();
            _viewAll.Activated += (_, _) =>
            {
                ViewAllClicked?.Invoke(this, EventArgs.Empty);
                if (ViewAllCommand?.CanExecute(null) == true) ViewAllCommand.Execute(null);
            };
            _viewAll.Margin = new Thickness(CdsSpacing.Spacing05, CdsSpacing.Spacing03);
            _footer.Add(rule, 0, 0);
            _footer.Add(_viewAll, 0, 1);
            _content.Add(_footer);
        }
        if (_footer is not null) _footer.IsVisible = show;
        if (_viewAll is not null) _viewAll.Text = ViewAllLabel;
    }

    void RebuildBody()
    {
        _body.Clear();
        var first = true;
        foreach (var list in _taskLists)
        {
            if (!first)
            {
                var rule = new BoxView { HeightRequest = 1, InputTransparent = true };
                CdsTokens.SetColor(rule, BoxView.ColorProperty, CdsChecklistTokens.Rule);
                _body.Add(rule);
            }
            first = false;
            var group = new VerticalStackLayout { Spacing = 0, Padding = CdsSpacing.Spacing05 };
            if (!string.IsNullOrEmpty(list.Title))
            {
                var title = new CdsText(list.Title) { MaxLines = 2, LineBreakMode = LineBreakMode.TailTruncation, Margin = new Thickness(0, 0, 0, CdsSpacing.Spacing03) };
                SemanticProperties.SetHeadingLevel(title, SemanticHeadingLevel.Level3);
                ToolTipProperties.SetText(title, list.Title);
                group.Add(title);
            }
            var tasks = new VerticalStackLayout { Spacing = CdsSpacing.Spacing03 };
            foreach (var task in list.Tasks) tasks.Add(BuildTask(task));
            CdsSemantics.SetGroup(tasks, list.Title, CdsSemanticGroupKind.List);
            group.Add(tasks);
            _body.Add(group);
        }
    }

    static View BuildTask(CdsChecklistTask task)
    {
        var row = new Grid { ColumnSpacing = 0 };
        row.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        row.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        var icon = new CdsIcon(CdsChecklistTokens.Icon(task.Kind), 16)
        {
            VerticalOptions = LayoutOptions.Start,
            Margin = new Thickness(0, 1, CdsSpacing.Spacing03, 0),
        };
        CdsTokens.SetColor(icon, CdsIcon.ColorProperty, CdsChecklistTokens.IconColor(task.Kind));
        row.Add(icon, 0);
        var status = string.IsNullOrEmpty(task.StatusDescription) ? CdsChecklistTokens.StatusText(task.Kind) : task.StatusDescription;
        var name = ChecklistModel.TaskName(task.Label, status);
        if (task.IsClickable)
        {
            var link = new ChecklistLink { Text = task.Label, AccessibleName = name };
            link.Activated += (_, _) => task.Activate();
            row.Add(link, 1);
        }
        else
        {
            var label = new CdsText(task.Label ?? string.Empty) { MaxLines = 2, LineBreakMode = LineBreakMode.TailTruncation };
            CdsTokens.SetColor(label, Label.TextColorProperty, CdsChecklistTokens.LabelColor(task.Kind));
            if (!string.IsNullOrEmpty(task.Label)) ToolTipProperties.SetText(label, task.Label);
            SemanticProperties.SetDescription(label, name);
            row.Add(label, 1);
        }
        return row;
    }

    void OnOpenChanged(bool oldValue, bool newValue)
    {
        UpdateHeader();
        var version = ++_version;
        if (!IsLoaded || Handler is null || Width <= 0 || CdsMotion.IsReducedMotion) ApplyOpen();
        else _ = AnimateAsync(newValue, version);
        OpenChanged?.Invoke(this, new CdsChoiceChangedEventArgs<bool>(oldValue, newValue));
    }

    // Settled: the list at its natural height, or hidden (block-size 0) when closed.
    void ApplyOpen()
    {
        _contentClip.IsVisible = IsOpen;
        _contentClip.HeightRequest = -1;
        _content.TranslationY = 0;
    }

    // The list slides up out of view (translate 0 to -100%) as its height goes to 0, over duration-fast-02.
    async Task AnimateAsync(bool opening, int version)
    {
        var full = _content.Measure(Width, double.PositiveInfinity).Height;
        if (opening)
        {
            _contentClip.HeightRequest = 0;
            _contentClip.IsVisible = true;
        }
        var from = opening ? 0 : (_contentClip.Height > 0 ? _contentClip.Height : full);
        var to = opening ? full : 0;
        await CdsMotion.AnimateAsync(this, "CdsChecklistContent", p =>
        {
            var height = ChecklistModel.HeightAt(from, to, p);
            _contentClip.HeightRequest = height;
            _content.TranslationY = height - full;
        }, 0, 1, CdsDuration.Fast02, CdsEasing.StandardProductive);
        if (version == _version) ApplyOpen();
    }
}

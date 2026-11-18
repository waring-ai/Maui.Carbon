using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Maui.Carbon;

/// <summary>Event data of <see cref="CdsTreeView"/> node events.</summary>
public sealed class CdsTreeNodeEventArgs : EventArgs
{
    /// <summary>Creates the event data.</summary>
    public CdsTreeNodeEventArgs(CdsTreeNode node, IReadOnlyList<CdsTreeNode> selected)
    {
        Node = node;
        SelectedNodes = selected;
    }

    /// <summary>The node concerned.</summary>
    public CdsTreeNode Node { get; }

    /// <summary>The selected nodes after the event.</summary>
    public IReadOnlyList<CdsTreeNode> SelectedNodes { get; }
}

/// <summary>
/// A hierarchical list of expandable <see cref="CdsTreeNode"/>s for navigating nested data such as folders, resource groups
/// or documentation sections. Rows are <c>layer-01</c> with <c>text-secondary</c>; hover is <c>layer-hover-01</c>, selected
/// <c>layer-selected-01</c>, and the active node adds a 4px <c>interactive</c> bar at the start edge. Clicking the chevron only
/// expands or collapses; clicking anywhere else on the row selects (and activates) it; with <see cref="IsMultiselect"/> a click
/// toggles the node in the selection. Indentation follows depth (16 per level, 24 when nodes have icons). Only one node is in the
/// tab order; Up and Down move focus through the visible nodes, Home and End jump to the ends, Right expands a closed parent and then
/// moves to its first child, Left collapses an open parent or moves to the parent, and Space or Enter selects. Always give the tree a
/// <see cref="Label"/> (<see cref="HideLabel"/> hides it visually). Use an accordion for one level, and the UI shell side nav for
/// product navigation.
/// </summary>
[ContentProperty(nameof(Nodes))]
public class CdsTreeView : CdsControl
{
    /// <summary>Bindable property for <see cref="Label"/>.</summary>
    public static readonly BindableProperty LabelProperty = BindableProperty.Create(
        nameof(Label), typeof(string), typeof(CdsTreeView), null, propertyChanged: static (b, _, _) => ((CdsTreeView)b).UpdateLabel());

    /// <summary>Bindable property for <see cref="HideLabel"/>.</summary>
    public static readonly BindableProperty HideLabelProperty = BindableProperty.Create(
        nameof(HideLabel), typeof(bool), typeof(CdsTreeView), false, propertyChanged: static (b, _, _) => ((CdsTreeView)b).UpdateLabel());

    /// <summary>Bindable property for <see cref="Size"/>.</summary>
    public static readonly BindableProperty SizeProperty = BindableProperty.Create(
        nameof(Size), typeof(CdsTreeViewSize), typeof(CdsTreeView), CdsTreeViewSize.Sm, propertyChanged: static (b, _, _) => ((CdsTreeView)b).Rebuild());

    /// <summary>Bindable property for <see cref="IsMultiselect"/>.</summary>
    public static readonly BindableProperty IsMultiselectProperty = BindableProperty.Create(
        nameof(IsMultiselect), typeof(bool), typeof(CdsTreeView), false, propertyChanged: static (b, _, _) => ((CdsTreeView)b).OnMultiselectChanged());

    /// <summary>Bindable property for <see cref="ActiveNode"/>.</summary>
    public static readonly BindableProperty ActiveNodeProperty = BindableProperty.Create(
        nameof(ActiveNode), typeof(CdsTreeNode), typeof(CdsTreeView), null, BindingMode.TwoWay,
        propertyChanged: static (b, _, _) => ((CdsTreeView)b).OnActiveChanged());

    /// <summary>Bindable property for <see cref="SelectedNode"/>.</summary>
    public static readonly BindableProperty SelectedNodeProperty = BindableProperty.Create(
        nameof(SelectedNode), typeof(CdsTreeNode), typeof(CdsTreeView), null, BindingMode.TwoWay,
        propertyChanged: static (b, _, n) => ((CdsTreeView)b).OnSelectedNodeChanged((CdsTreeNode?)n));

    readonly CdsItemCollection<CdsTreeNode> _nodes = [];
    readonly VerticalStackLayout _root = new() { Spacing = 0 };
    readonly CdsText _label = new() { TypeStyle = CdsTypeStyle.Label01, Tone = CdsTextTone.Secondary, Margin = new Thickness(0, 0, 0, CdsSpacing.Spacing03) };
    readonly VerticalStackLayout _rowsHost = new() { Spacing = 0 };
    readonly List<CdsTreeNode> _selected = [];
    readonly List<RowVisual> _visuals = [];
    List<TreeRow> _rows = [];
    CdsTreeNode? _tabStop;
    int _hover = -1;
    bool _syncing;

    /// <summary>Creates an empty tree.</summary>
    public CdsTreeView()
    {
        HorizontalOptions = LayoutOptions.Fill;
        VerticalOptions = LayoutOptions.Start;
        _root.Add(_label);
        _root.Add(_rowsHost);
        SemanticProperties.SetHeadingLevel(_label, SemanticHeadingLevel.None);
        _nodes.CollectionChanged += OnNodesChanged;
        SetChrome(_root);
        UpdateLabel();
        Rebuild();
    }

    /// <summary>Raised after the selection changed (<c>onSelect</c>).</summary>
    public event EventHandler<CdsTreeNodeEventArgs>? SelectionChanged;

    /// <summary>Raised when a node became the active node (<c>onActivate</c>).</summary>
    public event EventHandler<CdsTreeNodeEventArgs>? NodeActivated;

    /// <summary>Raised when a node expanded or collapsed (<c>onToggle</c>).</summary>
    public event EventHandler<CdsTreeNodeEventArgs>? NodeToggled;

    /// <summary>The root nodes (the content property).</summary>
    public IList<CdsTreeNode> Nodes => _nodes;

    /// <summary>The accessible name of the tree, shown above it unless <see cref="HideLabel"/>. Always set one.</summary>
    public string? Label
    {
        get => (string?)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <summary>Hides the label visually; it stays the accessible name.</summary>
    public bool HideLabel
    {
        get => (bool)GetValue(HideLabelProperty);
        set => SetValue(HideLabelProperty, value);
    }

    /// <summary>Row height: <see cref="CdsTreeViewSize.Xs"/> 24 or <see cref="CdsTreeViewSize.Sm"/> 32 (default).</summary>
    public CdsTreeViewSize Size
    {
        get => (CdsTreeViewSize)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    /// <summary>A click or Space toggles a node in the selection instead of replacing it (the web version uses Ctrl or Cmd with the click).</summary>
    public bool IsMultiselect
    {
        get => (bool)GetValue(IsMultiselectProperty);
        set => SetValue(IsMultiselectProperty, value);
    }

    /// <summary>The current node (<c>active</c>, two-way): the one that gets the 4px bar. Clicking a node makes it active.</summary>
    public CdsTreeNode? ActiveNode
    {
        get => (CdsTreeNode?)GetValue(ActiveNodeProperty);
        set => SetValue(ActiveNodeProperty, value);
    }

    /// <summary>The selected node (two-way); with <see cref="IsMultiselect"/> the first of <see cref="SelectedNodes"/>. Setting it selects just that node.</summary>
    public CdsTreeNode? SelectedNode
    {
        get => (CdsTreeNode?)GetValue(SelectedNodeProperty);
        set => SetValue(SelectedNodeProperty, value);
    }

    /// <summary>The selected nodes (<c>selected</c>).</summary>
    public IReadOnlyList<CdsTreeNode> SelectedNodes => _selected;

    /// <inheritdoc/>
    protected internal override bool WantsNavigationKeys => true;

    /// <summary>Finds a node by <see cref="CdsTreeNode.Id"/> anywhere in the tree.</summary>
    public CdsTreeNode? FindNode(string id)
    {
        foreach (var node in Descendants(_nodes))
            if (node.Id == id) return node;
        return null;
    }

    /// <summary>Replaces the selection with the given nodes (more than one only with <see cref="IsMultiselect"/>).</summary>
    public void Select(params CdsTreeNode[] nodes)
    {
        var next = nodes.Where(n => !n.IsDisabled).Take(IsMultiselect ? int.MaxValue : 1).ToList();
        if (next.SequenceEqual(_selected)) return;
        _selected.Clear();
        _selected.AddRange(next);
        AfterSelectionChanged(next.FirstOrDefault());
    }

    /// <summary>Expands every parent node.</summary>
    public void ExpandAll() => SetAllExpanded(true);

    /// <summary>Collapses every parent node.</summary>
    public void CollapseAll() => SetAllExpanded(false);

    void SetAllExpanded(bool expanded)
    {
        foreach (var node in Descendants(_nodes).Where(n => n.IsParent)) node.IsExpanded = expanded;
    }

    static IEnumerable<CdsTreeNode> Descendants(IEnumerable<CdsTreeNode> nodes)
    {
        foreach (var node in nodes)
        {
            yield return node;
            foreach (var child in Descendants(node.Children)) yield return child;
        }
    }

    // ----- keyboard -----

    /// <inheritdoc/>
    protected internal override bool OnKeyDown(CdsKeyEventArgs e)
    {
        var current = _visuals.FindIndex(v => v.Hit.IsFocused);
        var result = TreeViewModel.HandleKey(_rows, current, e.Key);
        if (!result.Handled) return false;
        if (result.ToggleIndex >= 0 && _rows[result.ToggleIndex].Node is CdsTreeNode toggled)
        {
            _tabStop = toggled;
            toggled.IsExpanded = result.Expand; // rebuilds the rows and restores focus to this node
        }
        if (result.FocusIndex >= 0) FocusRow(result.FocusIndex);
        if (result.Select && _rows.Count > 0)
        {
            var index = current >= 0 ? current : result.FocusIndex;
            if (index >= 0 && index < _rows.Count && _rows[index].Node is CdsTreeNode node) Choose(node);
        }
        return true;
    }

    void FocusRow(int index)
    {
        if (index < 0 || index >= _visuals.Count) return;
        _tabStop = (CdsTreeNode)_rows[index].Node;
        ApplyTabStops();
        _visuals[index].Hit.Focus();
    }

    void ApplyTabStops()
    {
        var stop = _tabStop is not null && TreeViewModel.IndexOf(_rows, _tabStop) is var i and >= 0 && !_tabStop.IsDisabled
            ? i
            : TreeViewModel.TabStopIndex(_rows, ActiveNode, n => _selected.Contains((CdsTreeNode)n));
        for (var r = 0; r < _visuals.Count; r++) _visuals[r].Hit.IsTabStop = r == stop;
    }

    // ----- selection -----

    void Choose(CdsTreeNode node)
    {
        if (node.IsDisabled) return;
        if (IsMultiselect)
        {
            if (!_selected.Remove(node)) _selected.Add(node);
        }
        else
        {
            _selected.Clear();
            _selected.Add(node);
        }
        AfterSelectionChanged(node);
    }

    void AfterSelectionChanged(CdsTreeNode? node)
    {
        _syncing = true;
        try
        {
            if (node is not null && !ReferenceEquals(ActiveNode, node))
            {
                ActiveNode = node;
                NodeActivated?.Invoke(this, new CdsTreeNodeEventArgs(node, _selected.ToList()));
            }
            SelectedNode = _selected.FirstOrDefault();
        }
        finally
        {
            _syncing = false;
        }
        RefreshVisuals();
        if (node is not null)
        {
            SelectionChanged?.Invoke(this, new CdsTreeNodeEventArgs(node, _selected.ToList()));
            CdsAnnouncer.Announce(node.Label);
        }
    }

    void OnActiveChanged()
    {
        if (_syncing) return;
        RefreshVisuals();
    }

    void OnSelectedNodeChanged(CdsTreeNode? node)
    {
        if (_syncing) return;
        _selected.Clear();
        if (node is not null && !node.IsDisabled) _selected.Add(node);
        RefreshVisuals();
    }

    void OnMultiselectChanged()
    {
        if (!IsMultiselect && _selected.Count > 1) _selected.RemoveRange(1, _selected.Count - 1);
        RefreshVisuals();
    }

    // ----- structure -----

    void OnNodesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
        {
            foreach (CdsTreeNode n in e.OldItems)
            {
                n.Changed -= OnNodeChanged;
                n.ExpandedBubble -= OnNodeToggled;
            }
        }
        if (e.NewItems is not null)
        {
            foreach (CdsTreeNode n in e.NewItems)
            {
                n.Changed += OnNodeChanged;
                n.ExpandedBubble += OnNodeToggled;
            }
        }
        Rebuild();
    }

    void OnNodeChanged(object? sender, EventArgs e) => Rebuild();

    void OnNodeToggled(object? sender, EventArgs e)
    {
        if (sender is CdsTreeNode node) NodeToggled?.Invoke(this, new CdsTreeNodeEventArgs(node, _selected.ToList()));
    }

    void UpdateLabel()
    {
        _label.Text = Label;
        _label.IsVisible = !HideLabel && !string.IsNullOrEmpty(Label);
        SemanticProperties.SetDescription(_rowsHost, Label);
    }

    void Rebuild()
    {
        var focused = _visuals.FindIndex(v => v.Hit.IsFocused);
        var focusNode = focused >= 0 && focused < _rows.Count ? _rows[focused].Node as CdsTreeNode : null;
        _rows = TreeViewModel.Flatten(_nodes);
        _rowsHost.Children.Clear();
        _visuals.Clear();
        _hover = -1;

        var withIcons = _rows.Any(r => ((CdsTreeNode)r.Node).Icon is not null);
        var height = TreeViewModel.RowHeight(Size);
        for (var i = 0; i < _rows.Count; i++)
        {
            var visual = BuildRow(i, _rows[i], withIcons, height);
            _visuals.Add(visual);
            _rowsHost.Add(visual.Host);
        }
        // Selection and active node refer to nodes: forget those that left the tree.
        var all = Descendants(_nodes).ToHashSet();
        _selected.RemoveAll(n => !all.Contains(n));
        ApplyTabStops();
        RefreshVisuals();
        if (focusNode is not null && TreeViewModel.IndexOf(_rows, focusNode) is var index and >= 0)
            Dispatcher.Dispatch(() => FocusRow(index));
    }

    RowVisual BuildRow(int index, TreeRow row, bool withIcons, double height)
    {
        var node = (CdsTreeNode)row.Node;
        var visual = new RowVisual { Node = node, Row = row };
        var host = new Grid { MinimumHeightRequest = height, ColumnSpacing = 0 };
        visual.Host = host;

        // [indent][toggle 24][gap 4][icon 16 + 8][text *]
        var indent = TreeViewModel.ToggleX(row.Depth, withIcons) - 4;
        host.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(indent)));
        host.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(TreeViewModel.ToggleSlot)));
        host.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(CdsSpacing.Spacing02)));
        host.ColumnDefinitions.Add(new ColumnDefinition(withIcons ? new GridLength(16 + CdsSpacing.Spacing03) : new GridLength(0)));
        host.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

        visual.Fill = new BoxView { InputTransparent = true };
        AutomationProperties.SetIsInAccessibleTree(visual.Fill, false);
        host.Add(visual.Fill);
        Grid.SetColumnSpan(visual.Fill, 5);

        visual.ActiveBar = new BoxView { WidthRequest = CdsTreeViewTokens.ActiveBarWidth, HorizontalOptions = LayoutOptions.Start, InputTransparent = true, IsVisible = false };
        AutomationProperties.SetIsInAccessibleTree(visual.ActiveBar, false);
        host.Add(visual.ActiveBar);
        Grid.SetColumnSpan(visual.ActiveBar, 5);

        if (row.HasChildren)
        {
            visual.Chevron = new CdsIcon(CdsIconName.ChevronDown, 16)
            {
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                InputTransparent = true,
                Rotation = CdsTreeViewTokens.ChevronRotation(node.IsExpanded),
            };
            host.Add(visual.Chevron, 1, 0);
        }
        if (withIcons)
        {
            visual.NodeIcon = new CdsIcon(node.Icon ?? CdsIconName.Document, 16)
            {
                VerticalOptions = LayoutOptions.Center,
                IsVisible = node.Icon is not null,
                InputTransparent = true,
            };
            host.Add(visual.NodeIcon, 3, 0);
        }
        visual.Text = new CdsText(node.Label ?? string.Empty, CdsTypeStyle.BodyCompact01, CdsTextTone.Secondary)
        {
            LineBreakMode = LineBreakMode.TailTruncation,
            VerticalOptions = LayoutOptions.Center,
            Margin = new Thickness(CdsSpacing.Spacing02, 0, CdsSpacing.Spacing05, 0),
            InputTransparent = true,
        };
        host.Add(visual.Text, 4, 0);
        if (node.Label is { } tip) ToolTipProperties.SetText(visual.Text, tip);

        visual.Ring = new CdsFocusRing(CdsFocusRingStyle.Inset);
        host.Add(visual.Ring);
        Grid.SetColumnSpan(visual.Ring, 5);

        visual.Hit = new CdsHitTarget { Role = CdsAccessibilityRole.Option, IsEnabled = !node.IsDisabled };
        SemanticProperties.SetDescription(visual.Hit, node.Label);
        visual.Hit.IsExpanded = row.HasChildren ? node.IsExpanded : null;
        visual.Hit.Clicked += (_, _) => Choose(node);
        visual.Hit.Focused += (_, _) =>
        {
            SetState(CdsInteractionState.Focused, true);
            CdsFocusTracker.SetFocused(this);
            _tabStop = node;
            ApplyTabStops();
            RefreshVisuals();
        };
        visual.Hit.Unfocused += (_, _) =>
        {
            SetState(CdsInteractionState.Focused | CdsInteractionState.FocusVisible, false);
            CdsFocusTracker.ClearFocused(this);
            RefreshVisuals();
        };
        host.Add(visual.Hit);
        Grid.SetColumnSpan(visual.Hit, 5);

        if (row.HasChildren)
        {
            visual.Toggle = new CdsHitTarget { Role = CdsAccessibilityRole.Expander, IsTabStop = false, IsEnabled = !node.IsDisabled, IsExpanded = node.IsExpanded };
            SemanticProperties.SetDescription(visual.Toggle, node.IsExpanded ? $"Collapse {node.Label}" : $"Expand {node.Label}");
            visual.Toggle.Clicked += (_, _) => node.IsExpanded = !node.IsExpanded;
            host.Add(visual.Toggle, 1, 0);
        }

        var pointer = new PointerGestureRecognizer();
        pointer.PointerEntered += (_, _) => { _hover = index; RefreshVisuals(); };
        pointer.PointerExited += (_, _) => { if (_hover == index) _hover = -1; RefreshVisuals(); };
        host.GestureRecognizers.Add(pointer);
        return visual;
    }

    /// <inheritdoc/>
    protected override void ApplyVisuals()
    {
        for (var i = 0; i < _visuals.Count; i++)
        {
            var v = _visuals[i];
            var node = v.Node;
            var disabled = node.IsDisabled;
            var selected = _selected.Contains(node);
            var hover = _hover == i && !disabled;
            SetColor(v.Fill, BoxView.ColorProperty, CdsTreeViewTokens.Fill(selected, hover, disabled));
            SetColor(v.Text, CdsText.TextColorProperty, CdsTreeViewTokens.Text(selected, hover, disabled));
            var iconKey = CdsTreeViewTokens.Icon(selected, hover, disabled);
            if (v.Chevron is not null) SetColor(v.Chevron, CdsIcon.ColorProperty, iconKey);
            if (v.NodeIcon is not null) SetColor(v.NodeIcon, CdsIcon.ColorProperty, iconKey);
            var active = ReferenceEquals(ActiveNode, node);
            v.ActiveBar.IsVisible = active;
            SetColor(v.ActiveBar, BoxView.ColorProperty, CdsTreeViewTokens.ActiveBar);
            v.Ring.IsActive = v.Hit.IsFocused && v.Hit.IsFocusFromKeyboard;
            v.Hit.IsSelected = selected;
        }
    }

    sealed class RowVisual
    {
        public CdsTreeNode Node = null!;
        public TreeRow Row;
        public Grid Host = null!;
        public BoxView Fill = null!;
        public BoxView ActiveBar = null!;
        public CdsIcon? Chevron;
        public CdsIcon? NodeIcon;
        public CdsText Text = null!;
        public CdsFocusRing Ring = null!;
        public CdsHitTarget Hit = null!;
        public CdsHitTarget? Toggle;
    }
}

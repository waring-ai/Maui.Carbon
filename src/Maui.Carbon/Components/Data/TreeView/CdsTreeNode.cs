using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Maui.Carbon;

/// <summary>
/// A node of a <see cref="CdsTreeView"/>: a <see cref="Label"/> (a branch label summarizes what it contains, a leaf label
/// names the item), an optional <see cref="Icon"/> (use icons on every node or on none: folders for parents, documents for
/// leaves), <see cref="IsExpanded"/>, <see cref="IsDisabled"/> and nested <see cref="Children"/> (a node with children is a parent).
/// </summary>
[ContentProperty(nameof(Children))]
public class CdsTreeNode : BindableObject, ITreeNodeData
{
    /// <summary>Bindable property for <see cref="Label"/>.</summary>
    public static readonly BindableProperty LabelProperty = BindableProperty.Create(
        nameof(Label), typeof(string), typeof(CdsTreeNode), null, propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="Id"/>.</summary>
    public static readonly BindableProperty IdProperty = BindableProperty.Create(
        nameof(Id), typeof(string), typeof(CdsTreeNode), null);

    /// <summary>Bindable property for <see cref="Value"/>.</summary>
    public static readonly BindableProperty ValueProperty = BindableProperty.Create(
        nameof(Value), typeof(object), typeof(CdsTreeNode), null);

    /// <summary>Bindable property for <see cref="Icon"/>.</summary>
    public static readonly BindableProperty IconProperty = BindableProperty.Create(
        nameof(Icon), typeof(CdsIconName?), typeof(CdsTreeNode), null, propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="IsDisabled"/>.</summary>
    public static readonly BindableProperty IsDisabledProperty = BindableProperty.Create(
        nameof(IsDisabled), typeof(bool), typeof(CdsTreeNode), false, propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="IsExpanded"/>.</summary>
    public static readonly BindableProperty IsExpandedProperty = BindableProperty.Create(
        nameof(IsExpanded), typeof(bool), typeof(CdsTreeNode), false, BindingMode.TwoWay,
        propertyChanged: static (b, _, n) => ((CdsTreeNode)b).OnExpandedChanged((bool)n));

    readonly ObservableCollection<CdsTreeNode> _children = [];

    /// <summary>Creates an empty node.</summary>
    public CdsTreeNode() => _children.CollectionChanged += OnChildrenChanged;

    /// <summary>Creates a node with a label and an optional icon.</summary>
    public CdsTreeNode(string label, CdsIconName? icon = null) : this()
    {
        Label = label;
        Icon = icon;
    }

    internal event EventHandler? Changed;

    internal event EventHandler? ExpandedBubble;

    /// <summary>Raised when the node expanded or collapsed (<c>onToggle</c>).</summary>
    public event EventHandler? Toggled;

    /// <summary>The node text.</summary>
    public string? Label
    {
        get => (string?)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <summary>An identifier, unique in the tree (the web version's <c>id</c>), used by <see cref="CdsTreeView.FindNode"/> and your own code.</summary>
    public string? Id
    {
        get => (string?)GetValue(IdProperty);
        set => SetValue(IdProperty, value);
    }

    /// <summary>Your data for the node (<c>value</c>).</summary>
    public object? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    /// <summary>The icon before the label (<c>renderIcon</c>), for example <see cref="CdsIconName.Folder"/> or <see cref="CdsIconName.Document"/>.</summary>
    public CdsIconName? Icon
    {
        get => (CdsIconName?)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>A disabled node cannot be selected or expanded and is skipped by the keyboard (<c>disabled</c>).</summary>
    public bool IsDisabled
    {
        get => (bool)GetValue(IsDisabledProperty);
        set => SetValue(IsDisabledProperty, value);
    }

    /// <summary>Whether the children are shown (two-way). Only meaningful for a parent.</summary>
    public bool IsExpanded
    {
        get => (bool)GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    /// <summary>The nested nodes (the content property). A node with children is a parent with a chevron.</summary>
    public IList<CdsTreeNode> Children => _children;

    /// <summary>The parent node, or <c>null</c> for a root node.</summary>
    public CdsTreeNode? ParentNode { get; private set; }

    /// <summary>Whether the node has children.</summary>
    public bool IsParent => _children.Count > 0;

    bool ITreeNodeData.IsExpanded => IsExpanded;

    bool ITreeNodeData.IsDisabled => IsDisabled;

    IReadOnlyList<ITreeNodeData> ITreeNodeData.ChildNodes => _children.Cast<ITreeNodeData>().ToList();

    internal void Raise() => Changed?.Invoke(this, EventArgs.Empty);

    void OnChildrenChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
        {
            foreach (CdsTreeNode child in e.OldItems)
            {
                child.Changed -= OnChildChanged;
                child.ExpandedBubble -= OnChildExpanded;
                child.ParentNode = null;
            }
        }
        if (e.NewItems is not null)
        {
            foreach (CdsTreeNode child in e.NewItems)
            {
                child.Changed += OnChildChanged;
                child.ExpandedBubble += OnChildExpanded;
                child.ParentNode = this;
            }
        }
        Raise();
    }

    void OnChildChanged(object? sender, EventArgs e) => Changed?.Invoke(sender, e);

    void OnChildExpanded(object? sender, EventArgs e) => ExpandedBubble?.Invoke(sender, e);

    void OnExpandedChanged(bool expanded)
    {
        Raise();
        Toggled?.Invoke(this, EventArgs.Empty);
        ExpandedBubble?.Invoke(this, EventArgs.Empty);
    }

    static void OnChanged(BindableObject b, object? o, object? n) => ((CdsTreeNode)b).Raise();
}

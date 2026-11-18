using System.Collections.ObjectModel;

namespace Maui.Carbon;

/// <summary>A column of a <see cref="CdsStructuredList"/>: its header label and width.</summary>
public class CdsStructuredListColumn : BindableObject
{
    /// <summary>Bindable property for <see cref="Header"/>.</summary>
    public static readonly BindableProperty HeaderProperty = BindableProperty.Create(
        nameof(Header), typeof(string), typeof(CdsStructuredListColumn), null,
        propertyChanged: static (b, _, _) => ((CdsStructuredListColumn)b).Changed?.Invoke(b, EventArgs.Empty));

    /// <summary>Bindable property for <see cref="Width"/>.</summary>
    public static readonly BindableProperty WidthProperty = BindableProperty.Create(
        nameof(Width), typeof(GridLength), typeof(CdsStructuredListColumn), GridLength.Star,
        propertyChanged: static (b, _, _) => ((CdsStructuredListColumn)b).Changed?.Invoke(b, EventArgs.Empty));

    /// <summary>Bindable property for <see cref="IsNoWrap"/>.</summary>
    public static readonly BindableProperty IsNoWrapProperty = BindableProperty.Create(
        nameof(IsNoWrap), typeof(bool), typeof(CdsStructuredListColumn), false,
        propertyChanged: static (b, _, _) => ((CdsStructuredListColumn)b).Changed?.Invoke(b, EventArgs.Empty));

    internal event EventHandler? Changed;

    /// <summary>The header label: one or two words, sentence case.</summary>
    public string? Header
    {
        get => (string?)GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    /// <summary>The column width: <c>Auto</c>, a fixed size or a star share (default <c>*</c>).</summary>
    public GridLength Width
    {
        get => (GridLength)GetValue(WidthProperty);
        set => SetValue(WidthProperty, value);
    }

    /// <summary>Keep the cells of this column on one line (<c>noWrap</c>).</summary>
    public bool IsNoWrap
    {
        get => (bool)GetValue(IsNoWrapProperty);
        set => SetValue(IsNoWrapProperty, value);
    }
}

/// <summary>
/// A body row of a <see cref="CdsStructuredList"/>. <see cref="Cells"/> holds one entry per column: a string (drawn as
/// <c>body-01</c> text) or any view. In a selectable list the whole row is the click target, so do not put interactive
/// controls in it.
/// </summary>
[ContentProperty(nameof(Cells))]
public class CdsStructuredListRow : BindableObject
{
    /// <summary>Bindable property for <see cref="Id"/>.</summary>
    public static readonly BindableProperty IdProperty = BindableProperty.Create(
        nameof(Id), typeof(string), typeof(CdsStructuredListRow), null);

    /// <summary>Bindable property for <see cref="Label"/>.</summary>
    public static readonly BindableProperty LabelProperty = BindableProperty.Create(
        nameof(Label), typeof(string), typeof(CdsStructuredListRow), "Select row",
        propertyChanged: static (b, _, _) => ((CdsStructuredListRow)b).Changed?.Invoke(b, EventArgs.Empty));

    /// <summary>Bindable property for <see cref="IsEnabled"/>.</summary>
    public static readonly BindableProperty IsEnabledProperty = BindableProperty.Create(
        nameof(IsEnabled), typeof(bool), typeof(CdsStructuredListRow), true,
        propertyChanged: static (b, _, _) => ((CdsStructuredListRow)b).Changed?.Invoke(b, EventArgs.Empty));

    readonly CdsItemCollection<object?> _cells = [];

    /// <summary>Creates an empty row.</summary>
    public CdsStructuredListRow()
    {
        _cells.CollectionChanged += (_, _) => Changed?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Creates a row of text cells.</summary>
    public CdsStructuredListRow(params string[] cells) : this()
    {
        foreach (var cell in cells) _cells.Add(cell);
    }

    internal event EventHandler? Changed;

    /// <summary>The cells, one per column: strings or views.</summary>
    public IList<object?> Cells => _cells;

    /// <summary>An identifier for the row (the web version's row <c>id</c>).</summary>
    public string? Id
    {
        get => (string?)GetValue(IdProperty);
        set => SetValue(IdProperty, value);
    }

    /// <summary>The accessible name of the row's radio. Default <c>Select row</c>; make it specific ("bx2.4x16 flavor").</summary>
    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <summary>A disabled row cannot be selected and is drawn dimmed (only in a selectable list).</summary>
    public bool IsEnabled
    {
        get => (bool)GetValue(IsEnabledProperty);
        set => SetValue(IsEnabledProperty, value);
    }
}

using System.Windows.Input;

namespace Maui.Carbon;

/// <summary>
/// A column of a <see cref="CdsDataTable"/>: <see cref="Key"/> and <see cref="Header"/> text, alignment, sorting, width and
/// where its values come from. A cell shows <see cref="ValueSelector"/>'s result as text, or the view of
/// <see cref="CellTemplate"/> (bound to the row object) when set. Without either, rows that are string-keyed dictionaries
/// are read by <see cref="Key"/>.
/// </summary>
public class CdsDataTableHeader : BindableObject
{
    /// <summary>Bindable property for <see cref="Key"/>.</summary>
    public static readonly BindableProperty KeyProperty = BindableProperty.Create(
        nameof(Key), typeof(string), typeof(CdsDataTableHeader), null, propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="Header"/>.</summary>
    public static readonly BindableProperty HeaderProperty = BindableProperty.Create(
        nameof(Header), typeof(string), typeof(CdsDataTableHeader), null, propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="Align"/>.</summary>
    public static readonly BindableProperty AlignProperty = BindableProperty.Create(
        nameof(Align), typeof(CdsTableAlignment), typeof(CdsDataTableHeader), CdsTableAlignment.Start, propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="IsSortable"/>.</summary>
    public static readonly BindableProperty IsSortableProperty = BindableProperty.Create(
        nameof(IsSortable), typeof(bool?), typeof(CdsDataTableHeader), null, propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="Width"/>.</summary>
    public static readonly BindableProperty WidthProperty = BindableProperty.Create(
        nameof(Width), typeof(GridLength), typeof(CdsDataTableHeader), GridLength.Star, propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="CellTemplate"/>.</summary>
    public static readonly BindableProperty CellTemplateProperty = BindableProperty.Create(
        nameof(CellTemplate), typeof(DataTemplate), typeof(CdsDataTableHeader), null, propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="ValueSelector"/>.</summary>
    public static readonly BindableProperty ValueSelectorProperty = BindableProperty.Create(
        nameof(ValueSelector), typeof(Func<object, object?>), typeof(CdsDataTableHeader), null, propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="StringFormat"/>.</summary>
    public static readonly BindableProperty StringFormatProperty = BindableProperty.Create(
        nameof(StringFormat), typeof(string), typeof(CdsDataTableHeader), null, propertyChanged: OnChanged);

    internal event EventHandler? Changed;

    /// <summary>The column id: sort and search results and <see cref="CdsDataTable.SortKey"/> refer to it. Unique per table.</summary>
    public string? Key
    {
        get => (string?)GetValue(KeyProperty);
        set => SetValue(KeyProperty, value);
    }

    /// <summary>The header text: one or two words, sentence case.</summary>
    public string? Header
    {
        get => (string?)GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    /// <summary>Start (default) or end alignment. Right-align numbers.</summary>
    public CdsTableAlignment Align
    {
        get => (CdsTableAlignment)GetValue(AlignProperty);
        set => SetValue(AlignProperty, value);
    }

    /// <summary>
    /// Whether the column sorts: <c>null</c> (default) follows <see cref="CdsDataTable.IsSortable"/>; <c>false</c> opts out of a
    /// sortable table, <c>true</c> opts in.
    /// </summary>
    public bool? IsSortable
    {
        get => (bool?)GetValue(IsSortableProperty);
        set => SetValue(IsSortableProperty, value);
    }

    /// <summary>The column width: a fixed size, <c>Auto</c> or a star share (default <c>*</c>).</summary>
    public GridLength Width
    {
        get => (GridLength)GetValue(WidthProperty);
        set => SetValue(WidthProperty, value);
    }

    /// <summary>A template for the cell's view (<c>renderCell</c>); its binding context is the row object. Such a column is searched by its <see cref="ValueSelector"/> text, if any.</summary>
    public DataTemplate? CellTemplate
    {
        get => (DataTemplate?)GetValue(CellTemplateProperty);
        set => SetValue(CellTemplateProperty, value);
    }

    /// <summary>
    /// Reads the cell value from a row object. The value is what the column sorts by (numbers sort numerically) and, as
    /// text, what the search matches and the cell shows. Declared in code: <c>ValueSelector = row =&gt; ((Cluster)row).Nodes</c>.
    /// </summary>
    public Func<object, object?>? ValueSelector
    {
        get => (Func<object, object?>?)GetValue(ValueSelectorProperty);
        set => SetValue(ValueSelectorProperty, value);
    }

    /// <summary>A format for the text of a formattable value, for example <c>N0</c> or <c>yyyy-MM-dd</c>.</summary>
    public string? StringFormat
    {
        get => (string?)GetValue(StringFormatProperty);
        set => SetValue(StringFormatProperty, value);
    }

    /// <summary>The value of this column in a row.</summary>
    internal object? ValueOf(object item)
    {
        if (ValueSelector is { } selector) return selector(item);
        if (Key is { } key)
        {
            if (item is System.Collections.IDictionary dictionary && dictionary.Contains(key)) return dictionary[key];
            if (item is IReadOnlyDictionary<string, object?> readOnly && readOnly.TryGetValue(key, out var value)) return value;
        }
        return null;
    }

    /// <summary>The display text of a value.</summary>
    internal string TextOf(object? value) => value switch
    {
        null => string.Empty,
        IFormattable formattable when !string.IsNullOrEmpty(StringFormat) => formattable.ToString(StringFormat, System.Globalization.CultureInfo.CurrentCulture),
        _ => value.ToString() ?? string.Empty,
    };

    static void OnChanged(BindableObject b, object? o, object? n) => ((CdsDataTableHeader)b).Changed?.Invoke(b, EventArgs.Empty);
}

/// <summary>Event data carrying the rows an action or a state change concerns.</summary>
public sealed class CdsDataTableRowsEventArgs : EventArgs
{
    /// <summary>Creates the event data.</summary>
    public CdsDataTableRowsEventArgs(IReadOnlyList<object> rows) => Rows = rows;

    /// <summary>The row objects.</summary>
    public IReadOnlyList<object> Rows { get; }
}

/// <summary>Event data carrying one row.</summary>
public sealed class CdsDataTableRowEventArgs : EventArgs
{
    /// <summary>Creates the event data.</summary>
    public CdsDataTableRowEventArgs(object row) => Row = row;

    /// <summary>The row object.</summary>
    public object Row { get; }
}

/// <summary>Event data of <see cref="CdsDataTable.SortChanged"/>.</summary>
public sealed class CdsDataTableSortEventArgs : EventArgs
{
    /// <summary>Creates the event data.</summary>
    public CdsDataTableSortEventArgs(string? key, CdsSortDirection direction)
    {
        Key = key;
        Direction = direction;
    }

    /// <summary>The key of the sorted column, or <c>null</c> when the table is unsorted.</summary>
    public string? Key { get; }

    /// <summary>The direction.</summary>
    public CdsSortDirection Direction { get; }
}

/// <summary>
/// An action of the batch bar that replaces the toolbar while rows are selected (multi selection only): a label, an
/// optional icon and a command or event that receives the selected row objects. Keep to a few verbs ("Restart", "Delete").
/// </summary>
public class CdsDataTableBatchAction : BindableObject
{
    /// <summary>Bindable property for <see cref="Label"/>.</summary>
    public static readonly BindableProperty LabelProperty = BindableProperty.Create(
        nameof(Label), typeof(string), typeof(CdsDataTableBatchAction), null, propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="Icon"/>.</summary>
    public static readonly BindableProperty IconProperty = BindableProperty.Create(
        nameof(Icon), typeof(CdsIconName?), typeof(CdsDataTableBatchAction), null, propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="Command"/>.</summary>
    public static readonly BindableProperty CommandProperty = BindableProperty.Create(
        nameof(Command), typeof(ICommand), typeof(CdsDataTableBatchAction), null, propertyChanged: OnChanged);

    internal event EventHandler? Changed;

    /// <summary>Raised when the action is chosen, with the selected rows.</summary>
    public event EventHandler<CdsDataTableRowsEventArgs>? Invoked;

    /// <summary>The button text.</summary>
    public string? Label
    {
        get => (string?)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <summary>An icon after the label.</summary>
    public CdsIconName? Icon
    {
        get => (CdsIconName?)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>
    /// A command run with an <c>IReadOnlyList&lt;object&gt;</c> of the selected rows as its parameter. While its
    /// <c>CanExecute</c> is false for the selected rows the batch button is disabled.
    /// </summary>
    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    internal void Invoke(IReadOnlyList<object> rows)
    {
        Invoked?.Invoke(this, new CdsDataTableRowsEventArgs(rows));
        if (Command?.CanExecute(rows) == true) Command.Execute(rows);
    }

    static void OnChanged(BindableObject b, object? o, object? n) => ((CdsDataTableBatchAction)b).Changed?.Invoke(b, EventArgs.Empty);
}

/// <summary>
/// An action in a row's overflow menu (<see cref="CdsDataTable.RowActions"/>): a label, an optional icon, a kind (use
/// <see cref="CdsMenuItemKind.Danger"/> for destructive actions and put them last) and a command or event that receives the row.
/// </summary>
public class CdsDataTableRowAction : BindableObject
{
    /// <summary>Bindable property for <see cref="Label"/>.</summary>
    public static readonly BindableProperty LabelProperty = BindableProperty.Create(
        nameof(Label), typeof(string), typeof(CdsDataTableRowAction), null, propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="Icon"/>.</summary>
    public static readonly BindableProperty IconProperty = BindableProperty.Create(
        nameof(Icon), typeof(CdsIconName?), typeof(CdsDataTableRowAction), null, propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="Kind"/>.</summary>
    public static readonly BindableProperty KindProperty = BindableProperty.Create(
        nameof(Kind), typeof(CdsMenuItemKind), typeof(CdsDataTableRowAction), CdsMenuItemKind.Default, propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="IsEnabled"/>.</summary>
    public static readonly BindableProperty IsEnabledProperty = BindableProperty.Create(
        nameof(IsEnabled), typeof(bool), typeof(CdsDataTableRowAction), true, propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="Command"/>.</summary>
    public static readonly BindableProperty CommandProperty = BindableProperty.Create(
        nameof(Command), typeof(ICommand), typeof(CdsDataTableRowAction), null);

    internal event EventHandler? Changed;

    /// <summary>Raised when the action is chosen, with the row.</summary>
    public event EventHandler<CdsDataTableRowEventArgs>? Invoked;

    /// <summary>The menu item text: a verb ("View details", "Delete").</summary>
    public string? Label
    {
        get => (string?)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <summary>An icon in the menu item.</summary>
    public CdsIconName? Icon
    {
        get => (CdsIconName?)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>Default or <see cref="CdsMenuItemKind.Danger"/> (destructive; last in the menu).</summary>
    public CdsMenuItemKind Kind
    {
        get => (CdsMenuItemKind)GetValue(KindProperty);
        set => SetValue(KindProperty, value);
    }

    /// <summary>Whether the action can be chosen.</summary>
    public bool IsEnabled
    {
        get => (bool)GetValue(IsEnabledProperty);
        set => SetValue(IsEnabledProperty, value);
    }

    /// <summary>
    /// A command run with the row object as its parameter. While its <c>CanExecute</c> is false for a row, the item is
    /// disabled in that row's menu (checked each time the menu opens).
    /// </summary>
    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    internal void Invoke(object row)
    {
        Invoked?.Invoke(this, new CdsDataTableRowEventArgs(row));
        if (Command?.CanExecute(row) == true) Command.Execute(row);
    }

    static void OnChanged(BindableObject b, object? o, object? n) => ((CdsDataTableRowAction)b).Changed?.Invoke(b, EventArgs.Empty);
}

/// <summary>
/// What a <see cref="CdsDataTable"/> shows under its header when no rows match (or there are none): a centered block on
/// <c>layer-01</c> with a short title, one sentence that says what to do and at most one action.
/// </summary>
public class CdsDataTableEmptyState : BindableObject
{
    /// <summary>Bindable property for <see cref="Title"/>.</summary>
    public static readonly BindableProperty TitleProperty = BindableProperty.Create(
        nameof(Title), typeof(string), typeof(CdsDataTableEmptyState), null, propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="Description"/>.</summary>
    public static readonly BindableProperty DescriptionProperty = BindableProperty.Create(
        nameof(Description), typeof(string), typeof(CdsDataTableEmptyState), null, propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="ActionText"/>.</summary>
    public static readonly BindableProperty ActionTextProperty = BindableProperty.Create(
        nameof(ActionText), typeof(string), typeof(CdsDataTableEmptyState), null, propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="ActionCommand"/>.</summary>
    public static readonly BindableProperty ActionCommandProperty = BindableProperty.Create(
        nameof(ActionCommand), typeof(ICommand), typeof(CdsDataTableEmptyState), null);

    internal event EventHandler? Changed;

    /// <summary>Raised when the action button is activated.</summary>
    public event EventHandler? ActionInvoked;

    /// <summary>The title: what is missing ("No clusters yet").</summary>
    public string? Title
    {
        get => (string?)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>One sentence that says what to do next.</summary>
    public string? Description
    {
        get => (string?)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    /// <summary>The label of the single action button; no button when empty.</summary>
    public string? ActionText
    {
        get => (string?)GetValue(ActionTextProperty);
        set => SetValue(ActionTextProperty, value);
    }

    /// <summary>The command run by the action button.</summary>
    public ICommand? ActionCommand
    {
        get => (ICommand?)GetValue(ActionCommandProperty);
        set => SetValue(ActionCommandProperty, value);
    }

    internal void InvokeAction()
    {
        ActionInvoked?.Invoke(this, EventArgs.Empty);
        if (ActionCommand?.CanExecute(null) == true) ActionCommand.Execute(null);
    }

    static void OnChanged(BindableObject b, object? o, object? n) => ((CdsDataTableEmptyState)b).Changed?.Invoke(b, EventArgs.Empty);
}

/// <summary>A flat text-and-icon button on the batch bar (<c>text-on-color</c> on <c>background-brand</c>).</summary>
internal sealed class CdsBatchBarButton : CdsControl
{
    readonly Grid _root = new() { ColumnSpacing = 0 };
    readonly BoxView _fill = new() { InputTransparent = true };
    readonly CdsText _label = new() { TypeStyle = CdsTypeStyle.BodyCompact01, VerticalOptions = LayoutOptions.Center, InputTransparent = true };
    readonly CdsIcon _icon = new() { IsVisible = false, VerticalOptions = LayoutOptions.Center, InputTransparent = true };
    readonly CdsFocusRing _ring = new(CdsFocusRingStyle.Inset);
    readonly CdsHitTarget _hit;

    public CdsBatchBarButton(string text, CdsIconName? icon, double height)
    {
        _root.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        _root.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        _label.Text = text;
        _label.Margin = new Thickness(CdsSpacing.Spacing05, 0, icon is null ? CdsSpacing.Spacing05 : 0, 0);
        _icon.Margin = new Thickness(CdsSpacing.Spacing03, 0, CdsSpacing.Spacing05, 0);
        if (icon is { } name)
        {
            _icon.Icon = name;
            _icon.IsVisible = true;
        }
        _root.HeightRequest = height;
        AutomationProperties.SetExcludedWithChildren(_label, true);
        AutomationProperties.SetExcludedWithChildren(_icon, true);
        _ring.SetColorKey(CdsColorKeys.Layer01);
        _hit = CreateHitTarget();
        SemanticProperties.SetDescription(_hit, text);
        _root.Add(_fill, 0, 0);
        Grid.SetColumnSpan(_fill, 2);
        _root.Add(_label, 0, 0);
        _root.Add(_icon, 1, 0);
        _root.Add(_ring, 0, 0);
        Grid.SetColumnSpan(_ring, 2);
        _root.Add(_hit, 0, 0);
        Grid.SetColumnSpan(_hit, 2);
        SetChrome(_root);
        HorizontalOptions = LayoutOptions.Start;
    }

    public event EventHandler? Clicked;

    /// <summary>Makes the button follow <paramref name="gate"/>'s <c>CanExecute</c> (disabled while it cannot run).</summary>
    public void SetAvailability(ICommand? gate) => SetCommand(gate, null);

    /// <summary>Asks the gate's <c>CanExecute</c> again (its answer depends on the selected rows).</summary>
    public void RefreshAvailability() => RefreshIsEnabledProperty();

    protected override void OnActivated() => Clicked?.Invoke(this, EventArgs.Empty);

    protected override void OnPropertyChanged(string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
        if (propertyName == IsEnabledProperty.PropertyName) RefreshVisuals();
    }

    protected override void ApplyVisuals()
    {
        var disabled = !IsEnabled;
        var pressed = Is(CdsInteractionState.Pressed);
        var hover = Is(CdsInteractionState.Hover);
        SetColor(_fill, BoxView.ColorProperty, CdsTableTokens.BatchButtonFill(pressed, disabled));
        _fill.IsVisible = disabled || pressed || hover;
        SetColor(_label, CdsText.TextColorProperty, disabled ? CdsColorKeys.TextOnColorDisabled : CdsTableTokens.BatchText);
        SetColor(_icon, CdsIcon.ColorProperty, disabled ? CdsColorKeys.IconOnColorDisabled : CdsColorKeys.IconOnColor);
        _ring.IsActive = Is(CdsInteractionState.FocusVisible);
    }
}

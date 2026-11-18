using Maui.Carbon.Relay.ViewModels;

namespace Maui.Carbon.Relay.Presentation;

/// <summary>
/// Data table columns for <see cref="IssueRow"/> lists. POCO rows need a <see cref="CdsDataTableHeader.ValueSelector"/>
/// (sorting and search read it) and a cell template for anything richer than text. Keys match <c>IssueQuery.Sort</c>.
/// </summary>
public static class IssueColumns
{
    public const string Key = "key";
    public const string Title = "title";
    public const string Type = "type";
    public const string Status = "status";
    public const string Priority = "priority";
    public const string Assignee = "assignee";
    public const string Points = "points";
    public const string Due = "due";
    public const string Updated = "updated";
    public const string Project = "project";

    static IssueRow R(object row) => (IssueRow)row;

    /// <summary>Adds the given columns (all of them by default) and the row key.</summary>
    public static void Add(CdsDataTable table, Action<IssueRow>? openIssue = null, params string[] columns)
    {
        table.RowKeySelector = r => R(r).Key;
        if (columns.Length == 0) columns = [Key, Title, Status, Priority, Assignee, Points, Updated];
        foreach (var column in columns) table.Headers.Add(Create(column, openIssue));
    }

    public static CdsDataTableHeader Create(string column, Action<IssueRow>? openIssue = null) => column switch
    {
        Key => new CdsDataTableHeader { Key = Key, Header = "Key", Width = new GridLength(96), ValueSelector = r => R(r).Issue.Id, CellTemplate = KeyTemplate(openIssue) },
        Title => new CdsDataTableHeader { Key = Title, Header = "Summary", Width = GridLength.Star, ValueSelector = r => R(r).Title, CellTemplate = TitleTemplate() },
        Type => new CdsDataTableHeader { Key = Type, Header = "Type", Width = new GridLength(110), ValueSelector = r => R(r).TypeText, CellTemplate = TypeTemplate() },
        Status => new CdsDataTableHeader { Key = Status, Header = "Status", Width = new GridLength(124), ValueSelector = r => (int)R(r).Status, CellTemplate = StatusTemplate() },
        Priority => new CdsDataTableHeader { Key = Priority, Header = "Priority", Width = new GridLength(104), ValueSelector = r => -(int)R(r).Priority, CellTemplate = PriorityTemplate() },
        Assignee => new CdsDataTableHeader { Key = Assignee, Header = "Assignee", Width = new GridLength(136), ValueSelector = r => R(r).AssigneeName },
        Points => new CdsDataTableHeader { Key = Points, Header = "Points", Width = new GridLength(72), Align = CdsTableAlignment.End, ValueSelector = r => R(r).Issue.StoryPoints ?? -1, CellTemplate = PointsTemplate() },
        Due => new CdsDataTableHeader { Key = Due, Header = "Due", Width = new GridLength(120), ValueSelector = r => R(r).Issue.DueDate ?? DateTime.MaxValue, CellTemplate = DueTemplate() },
        Updated => new CdsDataTableHeader { Key = Updated, Header = "Updated", Width = new GridLength(112), ValueSelector = r => R(r).Issue.Updated, CellTemplate = UpdatedTemplate() },
        Project => new CdsDataTableHeader { Key = Project, Header = "Project", Width = new GridLength(160), ValueSelector = r => R(r).ProjectName },
        _ => throw new ArgumentOutOfRangeException(nameof(column), column, "Unknown issue column."),
    };

    /// <summary>The key as a link that opens the issue.</summary>
    public static DataTemplate KeyTemplate(Action<IssueRow>? openIssue) => new(() =>
    {
        var link = new CdsLink { VerticalOptions = LayoutOptions.Center };
        link.SetBinding(CdsLink.TextProperty, static (IssueRow r) => r.Key, mode: BindingMode.OneTime);
        if (openIssue is not null) link.Clicked += (s, _) => openIssue(R(((BindableObject)s!).BindingContext));
        return link;
    });

    /// <summary>The summary, after a type icon (the icon's description names the type for screen readers).</summary>
    public static DataTemplate TitleTemplate() => new(() =>
    {
        var grid = new Grid { ColumnDefinitions = [new(GridLength.Auto), new(GridLength.Star)], ColumnSpacing = 8, VerticalOptions = LayoutOptions.Center };
        var icon = new CdsIcon { Size = 16, VerticalOptions = LayoutOptions.Center };
        icon.SetBinding(CdsIcon.IconProperty, static (IssueRow r) => r.Type, mode: BindingMode.OneTime, converter: Converters.TypeIcon);
        icon.SetBinding(CdsIcon.DescriptionProperty, static (IssueRow r) => r.TypeText, mode: BindingMode.OneTime);
        var text = new CdsText { TypeStyle = CdsTypeStyle.BodyCompact01, MaxLines = 1, LineBreakMode = LineBreakMode.TailTruncation, VerticalOptions = LayoutOptions.Center };
        text.SetBinding(Label.TextProperty, static (IssueRow r) => r.Title, mode: BindingMode.OneTime);
        grid.Add(icon);
        grid.Add(text, 1);
        return grid;
    });

    /// <summary>The type as a colored tag with its icon.</summary>
    public static DataTemplate TypeTemplate() => new(() =>
    {
        var tag = new CdsTag { Size = CdsTagSize.Sm, HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Center };
        tag.SetBinding(CdsTag.TextProperty, static (IssueRow r) => r.TypeText, mode: BindingMode.OneTime);
        tag.SetBinding(CdsTag.TypeProperty, static (IssueRow r) => r.Type, mode: BindingMode.OneTime, converter: Converters.TypeTag);
        tag.SetBinding(CdsTag.IconProperty, static (IssueRow r) => r.Type, mode: BindingMode.OneTime, converter: Converters.TypeIcon);
        return tag;
    });

    /// <summary>The status as an icon indicator (icon plus word, never color alone).</summary>
    public static DataTemplate StatusTemplate() => new(() =>
    {
        var indicator = new CdsIconIndicator { Size = 16, VerticalOptions = LayoutOptions.Center };
        indicator.SetBinding(CdsIconIndicator.KindProperty, static (IssueRow r) => r.Status, mode: BindingMode.OneTime, converter: Converters.StatusIndicator);
        indicator.SetBinding(CdsIconIndicator.LabelProperty, static (IssueRow r) => r.StatusText, mode: BindingMode.OneTime);
        return indicator;
    });

    public static DataTemplate PriorityTemplate() => new(() =>
    {
        var shape = new CdsShapeIndicator { VerticalOptions = LayoutOptions.Center };
        shape.SetBinding(CdsShapeIndicator.KindProperty, static (IssueRow r) => r.Priority, mode: BindingMode.OneTime, converter: Converters.PriorityShape);
        shape.SetBinding(CdsShapeIndicator.LabelProperty, static (IssueRow r) => r.PriorityText, mode: BindingMode.OneTime);
        return shape;
    });

    public static DataTemplate PointsTemplate() => new(() =>
    {
        var text = new CdsText { TypeStyle = CdsTypeStyle.BodyCompact01, HorizontalOptions = LayoutOptions.End, VerticalOptions = LayoutOptions.Center };
        text.SetBinding(Label.TextProperty, static (IssueRow r) => r.PointsText, mode: BindingMode.OneTime);
        return text;
    });

    /// <summary>The due date; overdue dates are red with a warning icon.</summary>
    public static DataTemplate DueTemplate() => new(() =>
    {
        var row = new HorizontalStackLayout { Spacing = 4, VerticalOptions = LayoutOptions.Center };
        var icon = new CdsIcon(CdsIconName.WarningFilled, 16) { VerticalOptions = LayoutOptions.Center, Description = "Overdue" };
        CdsTokens.SetColor(icon, CdsIcon.ColorProperty, CdsColorKeys.SupportError);
        icon.SetBinding(VisualElement.IsVisibleProperty, static (IssueRow r) => r.IsOverdue, mode: BindingMode.OneTime);
        var text = new CdsText { TypeStyle = CdsTypeStyle.BodyCompact01, VerticalOptions = LayoutOptions.Center };
        text.SetBinding(Label.TextProperty, static (IssueRow r) => r.DueText, mode: BindingMode.OneTime);
        row.Add(icon);
        row.Add(text);
        return row;
    });

    public static DataTemplate UpdatedTemplate() => new(() =>
    {
        var text = new CdsText { TypeStyle = CdsTypeStyle.BodyCompact01, Tone = CdsTextTone.Secondary, VerticalOptions = LayoutOptions.Center };
        text.SetBinding(Label.TextProperty, static (IssueRow r) => r.UpdatedText, mode: BindingMode.OneTime);
        return text;
    });
}

using Maui.Carbon.Relay.ViewModels;
using Microsoft.Maui.Controls.Shapes;

namespace Maui.Carbon.Relay.Presentation;

/// <summary>Data table columns for <see cref="TeamMemberRow"/> (the team screen), plus the initials avatar it uses.</summary>
public static class TeamColumns
{
    static TeamMemberRow R(object row) => (TeamMemberRow)row;

    /// <summary>
    /// Adds the person (avatar, name and job title), role, location and open issue columns and keys rows by person id.
    /// The job title is the second line of the person cell so the table fits beside the detail panel.
    /// </summary>
    public static void Add(CdsDataTable table)
    {
        table.RowKeySelector = r => R(r).Id;
        table.Headers.Add(new CdsDataTableHeader { Key = "name", Header = "Name and title", Width = GridLength.Star, ValueSelector = r => R(r).Name, CellTemplate = NameTemplate() });
        table.Headers.Add(new CdsDataTableHeader { Key = "role", Header = "Role", Width = new GridLength(188), ValueSelector = r => R(r).RoleText, CellTemplate = RoleTemplate() });
        table.Headers.Add(new CdsDataTableHeader { Key = "location", Header = "Location", Width = new GridLength(116), ValueSelector = r => R(r).Location });
        table.Headers.Add(new CdsDataTableHeader { Key = "open", Header = "Open issues", Width = new GridLength(124), Align = CdsTableAlignment.End, ValueSelector = r => R(r).OpenIssues, CellTemplate = OpenTemplate() });
    }

    /// <summary>A round avatar with the person's initials, on <c>layer-accent</c>.</summary>
    public static View Avatar(double size)
    {
        var text = new CdsText
        {
            TypeStyle = size >= 40 ? CdsTypeStyle.HeadingCompact02 : CdsTypeStyle.Label01,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
        };
        text.SetBinding(Label.TextProperty, static (TeamMemberRow r) => r.Initials);
        var circle = new Border
        {
            StrokeShape = new Ellipse(),
            StrokeThickness = 0,
            WidthRequest = size,
            HeightRequest = size,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Start,
            Content = text,
        };
        CdsTokens.SetColor(circle, VisualElement.BackgroundColorProperty, CdsColorKeys.LayerAccent01);
        AutomationProperties.SetIsInAccessibleTree(circle, false);
        return circle;
    }

    static DataTemplate NameTemplate() => new(() =>
    {
        var row = new Grid { ColumnDefinitions = [new(GridLength.Auto), new(GridLength.Star)], ColumnSpacing = 12, VerticalOptions = LayoutOptions.Center };
        var lines = new VerticalStackLayout { Spacing = 2, VerticalOptions = LayoutOptions.Center };
        var name = new CdsText { TypeStyle = CdsTypeStyle.HeadingCompact01, MaxLines = 1, LineBreakMode = LineBreakMode.TailTruncation };
        name.SetBinding(Label.TextProperty, static (TeamMemberRow r) => r.Name, mode: BindingMode.OneTime);
        var title = new CdsText { TypeStyle = CdsTypeStyle.Label01, Tone = CdsTextTone.Secondary, MaxLines = 1, LineBreakMode = LineBreakMode.TailTruncation };
        title.SetBinding(Label.TextProperty, static (TeamMemberRow r) => r.JobTitle, mode: BindingMode.OneTime);
        lines.Add(name);
        lines.Add(title);
        row.Add(Avatar(32));
        row.Add(lines, 1);
        return row;
    });

    static DataTemplate RoleTemplate() => new(() =>
    {
        var tag = new CdsTag { Size = CdsTagSize.Sm, HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Center };
        tag.SetBinding(CdsTag.TextProperty, static (TeamMemberRow r) => r.RoleText, mode: BindingMode.OneTime);
        tag.SetBinding(CdsTag.TypeProperty, static (TeamMemberRow r) => r.Role, mode: BindingMode.OneTime, converter: Converters.RoleTag);
        return tag;
    });

    static DataTemplate OpenTemplate() => new(() =>
    {
        var text = new CdsText { TypeStyle = CdsTypeStyle.BodyCompact01, HorizontalOptions = LayoutOptions.End, VerticalOptions = LayoutOptions.Center };
        text.SetBinding(Label.TextProperty, static (TeamMemberRow r) => r.OpenIssuesText, mode: BindingMode.OneTime);
        return text;
    });
}

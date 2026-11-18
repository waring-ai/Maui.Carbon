namespace Maui.Carbon.Gallery.Pages.Patterns.Data;

/// <summary>
/// An empty state block: an optional 32px icon, a heading-03 title, body-01 text and a row of actions, left-aligned.
/// Carbon has no empty-state component, so the pattern pages compose it from tokens, <see cref="CdsIcon"/>,
/// <see cref="CdsText"/>, <see cref="CdsButton"/> and <see cref="CdsLink"/>.
/// </summary>
public sealed class K1EmptyState : ContentView
{
    /// <summary>Builds an empty state; <paramref name="padding"/> is the space around the block.</summary>
    public K1EmptyState(CdsIconName? icon, string title, string body, IEnumerable<View> actions, Thickness padding)
    {
        var stack = new VerticalStackLayout { Spacing = 0, HorizontalOptions = LayoutOptions.Start };
        if (icon is { } name)
        {
            var glyph = new CdsIcon(name, 32) { HorizontalOptions = LayoutOptions.Start, Margin = new Thickness(0, 0, 0, CdsSpacing.Spacing06) };
            CdsTokens.SetColor(glyph, CdsIcon.ColorProperty, CdsColorKeys.IconSecondary);
            stack.Add(glyph);
        }

        var heading = new CdsText(title, CdsTypeStyle.Heading03) { MaximumWidthRequest = 480, HorizontalOptions = LayoutOptions.Start };
        SemanticProperties.SetHeadingLevel(heading, SemanticHeadingLevel.Level2);
        stack.Add(heading);
        stack.Add(new CdsText(body, CdsTypeStyle.Body01, CdsTextTone.Secondary)
        {
            MaximumWidthRequest = 420,
            HorizontalOptions = LayoutOptions.Start,
            Margin = new Thickness(0, CdsSpacing.Spacing03, 0, 0),
        });

        var row = new HorizontalStackLayout { Spacing = CdsSpacing.Spacing06, Margin = new Thickness(0, CdsSpacing.Spacing06, 0, 0) };
        foreach (var action in actions) row.Add(action);
        if (row.Children.Count > 0) stack.Add(row);

        Padding = padding;
        Content = stack;
    }
}

namespace Maui.Carbon;

/// <summary>
/// An in-context message about the state of the page or of a task, shown near what it is about: square, 48 minimum
/// height, 3px colored start edge, status icon, a semibold title and a subtitle, and an optional close button. It
/// never dismisses itself. For a message with an action use <see cref="CdsActionableNotification"/>; for transient
/// confirmation <see cref="CdsToastNotification"/>; for guidance that cannot be dismissed <see cref="CdsCallout"/>.
/// Never stack more than three.
/// </summary>
public class CdsInlineNotification : CdsDismissibleNotification
{
    const double MaxWidth = 608;
    const double MinWidth = 288;

    readonly Grid _content = new() { ColumnSpacing = CdsSpacing.Spacing05, Margin = new Thickness(3, 0, 0, 0) };
    readonly Label _text;
    readonly Span _title;
    readonly Span _subtitle;

    /// <summary>Creates an info inline notification in the high-contrast style.</summary>
    public CdsInlineNotification()
    {
        _text = CdsNotificationText.CreateFlow(out _title, out _subtitle);
        RegisterText(_text);

        _content.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        _content.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        StatusIcon.Margin = new Thickness(0, 14, 0, 0);
        var texts = new VerticalStackLayout { Padding = new Thickness(0, 15), Spacing = 0, Children = { _text, BodyHost } };
        _content.Add(StatusIcon, 0);
        _content.Add(texts, 1);

        Surface.MinimumHeightRequest = CdsSizing.Large;
        MinimumWidthRequest = MinWidth;
        SetMaxContentWidth(MaxWidth);
        HorizontalOptions = LayoutOptions.Fill;
        SetBody(_content);
    }

    /// <summary>Creates an inline notification with a kind, title and subtitle.</summary>
    public CdsInlineNotification(CdsNotificationKind kind, string title, string? subtitle = null) : this()
    {
        Kind = kind;
        Title = title;
        Subtitle = subtitle;
    }

    /// <inheritdoc/>
    protected override double OutlineOpacity => IsLowContrast ? 1 : 0;

    /// <inheritdoc/>
    protected override void UpdateContent()
    {
        CdsNotificationText.SetFlow(_title, _subtitle, Title, Subtitle);
        _content.Padding = new Thickness(CdsSpacing.Spacing05, 0, HideCloseButton ? CdsSpacing.Spacing05 : CloseSpace, 0);
    }
}

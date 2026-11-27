namespace Maui.Carbon;

/// <summary>
/// One notification in a <see cref="CdsNotificationsPanel"/>: its status, time, title, description and an optional link.
/// Properties are bindable, so a change shows in an open panel.
/// </summary>
public class CdsNotificationsPanelItem : BindableObject
{
    /// <summary>Bindable property for <see cref="Kind"/>.</summary>
    public static readonly BindableProperty KindProperty = BindableProperty.Create(nameof(Kind), typeof(CdsNotificationKind?), typeof(CdsNotificationsPanelItem), null);

    /// <summary>Bindable property for <see cref="Timestamp"/>.</summary>
    public static readonly BindableProperty TimestampProperty = BindableProperty.Create(nameof(Timestamp), typeof(DateTimeOffset), typeof(CdsNotificationsPanelItem), default(DateTimeOffset));

    /// <summary>Bindable property for <see cref="Title"/>.</summary>
    public static readonly BindableProperty TitleProperty = BindableProperty.Create(nameof(Title), typeof(string), typeof(CdsNotificationsPanelItem), null);

    /// <summary>Bindable property for <see cref="Description"/>.</summary>
    public static readonly BindableProperty DescriptionProperty = BindableProperty.Create(nameof(Description), typeof(string), typeof(CdsNotificationsPanelItem), null);

    /// <summary>Bindable property for <see cref="LinkText"/>.</summary>
    public static readonly BindableProperty LinkTextProperty = BindableProperty.Create(nameof(LinkText), typeof(string), typeof(CdsNotificationsPanelItem), null);

    /// <summary>Bindable property for <see cref="LinkUrl"/>.</summary>
    public static readonly BindableProperty LinkUrlProperty = BindableProperty.Create(nameof(LinkUrl), typeof(string), typeof(CdsNotificationsPanelItem), null);

    /// <summary>Bindable property for <see cref="IsUnread"/>.</summary>
    public static readonly BindableProperty IsUnreadProperty = BindableProperty.Create(nameof(IsUnread), typeof(bool), typeof(CdsNotificationsPanelItem), false);

    /// <summary>An optional identifier of your own (Carbon: <c>id</c>).</summary>
    public object? Id { get; set; }

    /// <summary>The status: error, warning, success or informational; shown as a colored icon before the title. Null shows no icon.</summary>
    public CdsNotificationKind? Kind
    {
        get => (CdsNotificationKind?)GetValue(KindProperty);
        set => SetValue(KindProperty, value);
    }

    /// <summary>When it happened. Sorts the list (newest first), puts it under Today, Yesterday or Previous, and shows as relative time.</summary>
    public DateTimeOffset Timestamp
    {
        get => (DateTimeOffset)GetValue(TimestampProperty);
        set => SetValue(TimestampProperty, value);
    }

    /// <summary>What happened, in a few words ("Deployment failed").</summary>
    public string? Title
    {
        get => (string?)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>One or two sentences of detail. Text longer than 88 characters is cut to two lines with a Read more button.</summary>
    public string? Description
    {
        get => (string?)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    /// <summary>The text of a link under the description ("View logs"). Shown with <see cref="LinkUrl"/>.</summary>
    public string? LinkText
    {
        get => (string?)GetValue(LinkTextProperty);
        set => SetValue(LinkTextProperty, value);
    }

    /// <summary>Where the link goes; opened with the system launcher.</summary>
    public string? LinkUrl
    {
        get => (string?)GetValue(LinkUrlProperty);
        set => SetValue(LinkUrlProperty, value);
    }

    /// <summary>Shows the title in semibold until the person has seen it.</summary>
    public bool IsUnread
    {
        get => (bool)GetValue(IsUnreadProperty);
        set => SetValue(IsUnreadProperty, value);
    }
}

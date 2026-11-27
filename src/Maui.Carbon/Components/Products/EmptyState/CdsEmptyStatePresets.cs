namespace Maui.Carbon;

/// <summary>
/// An empty state with the error illustration (Carbon for IBM Products <c>ErrorEmptyState</c>), in light and dark
/// variants (<see cref="CdsEmptyState.IllustrationTheme"/>). Use it when something went wrong while loading or saving: say what failed and offer a retry.
/// </summary>
public class CdsErrorEmptyState : CdsEmptyState
{
    /// <summary>Creates the preset.</summary>
    public CdsErrorEmptyState() : base(IllustrationArt.ErrorLight, IllustrationArt.ErrorDark)
    {
    }
}

/// <summary>
/// An empty state with the no data illustration (Carbon for IBM Products <c>NoDataEmptyState</c>), in light and dark
/// variants (<see cref="CdsEmptyState.IllustrationTheme"/>). Use it when there is nothing here yet (a first use, or a list with no items): say how to add the first item.
/// </summary>
public class CdsNoDataEmptyState : CdsEmptyState
{
    /// <summary>Creates the preset.</summary>
    public CdsNoDataEmptyState() : base(IllustrationArt.NoDataLight, IllustrationArt.NoDataDark)
    {
    }
}

/// <summary>
/// An empty state with the no tags illustration (Carbon for IBM Products <c>NoTagsEmptyState</c>), in light and dark
/// variants (<see cref="CdsEmptyState.IllustrationTheme"/>). Use it when an item has no tags yet: say how to add one.
/// </summary>
public class CdsNoTagsEmptyState : CdsEmptyState
{
    /// <summary>Creates the preset.</summary>
    public CdsNoTagsEmptyState() : base(IllustrationArt.NoTagsLight, IllustrationArt.NoTagsDark)
    {
    }
}

/// <summary>
/// An empty state with the not found illustration (Carbon for IBM Products <c>NotFoundEmptyState</c>), in light and dark
/// variants (<see cref="CdsEmptyState.IllustrationTheme"/>). Use it when a search or filter found nothing: name the query and offer to clear it.
/// </summary>
public class CdsNotFoundEmptyState : CdsEmptyState
{
    /// <summary>Creates the preset.</summary>
    public CdsNotFoundEmptyState() : base(IllustrationArt.NotFoundLight, IllustrationArt.NotFoundDark)
    {
    }
}

/// <summary>
/// An empty state with the notifications illustration (Carbon for IBM Products <c>NotificationsEmptyState</c>), in light and dark
/// variants (<see cref="CdsEmptyState.IllustrationTheme"/>). Use it when there are no notifications: say that people are up to date.
/// </summary>
public class CdsNotificationsEmptyState : CdsEmptyState
{
    /// <summary>Creates the preset.</summary>
    public CdsNotificationsEmptyState() : base(IllustrationArt.NotificationsLight, IllustrationArt.NotificationsDark)
    {
    }
}

/// <summary>
/// An empty state with the unauthorized illustration (Carbon for IBM Products <c>UnauthorizedEmptyState</c>), in light and dark
/// variants (<see cref="CdsEmptyState.IllustrationTheme"/>). Use it when people lack permission to see the content: name the missing access and who grants it.
/// </summary>
public class CdsUnauthorizedEmptyState : CdsEmptyState
{
    /// <summary>Creates the preset.</summary>
    public CdsUnauthorizedEmptyState() : base(IllustrationArt.UnauthorizedLight, IllustrationArt.UnauthorizedDark)
    {
    }
}

namespace Maui.Carbon;

/// <summary>What to show in a toast opened with <see cref="CdsToast.Show"/>.</summary>
public sealed class CdsToastOptions
{
    /// <summary>The status. Default <see cref="CdsNotificationKind.Info"/>.</summary>
    public CdsNotificationKind Kind { get; set; } = CdsNotificationKind.Info;

    /// <summary>The outcome without a period ("Deployment complete").</summary>
    public string? Title { get; set; }

    /// <summary>One or two short sentences; do not repeat the title.</summary>
    public string? Subtitle { get; set; }

    /// <summary>Optional small text under the subtitle, such as a timestamp.</summary>
    public string? Caption { get; set; }

    /// <summary>
    /// Milliseconds until it closes itself. 0 never closes. Only use it for low-severity success messages that can be
    /// found elsewhere (about 5000); never auto-dismiss errors.
    /// </summary>
    public int Timeout { get; set; }

    /// <summary>Use the low-contrast style. Pick one contrast style for all toasts in a product.</summary>
    public bool IsLowContrast { get; set; } = true;

    /// <summary>Hide the close button (for a toast that closes itself).</summary>
    public bool HideCloseButton { get; set; }

    /// <summary>How it is announced. Default <see cref="CdsNotificationRole.Status"/>; use <c>Alert</c> only for urgent errors.</summary>
    public CdsNotificationRole Role { get; set; } = CdsNotificationRole.Status;

    /// <summary>Accessible name of the status icon (default "&lt;kind&gt; icon").</summary>
    public string? StatusIconDescription { get; set; }

    /// <summary>Accessible name of the close button (default "Close notification").</summary>
    public string? CloseButtonDescription { get; set; }

    /// <summary>Called when the toast is about to close (button, Escape or timeout); set <c>Cancel</c> to keep it.</summary>
    public Action<CdsNotificationCloseEventArgs>? CloseRequested { get; set; }

    /// <summary>Called after the toast closed.</summary>
    public Action? Closed { get; set; }
}

/// <summary>
/// Shows toasts in the top-right toast region of a page: newest on top of the column, 8px apart, through
/// <see cref="CdsOverlayService"/>. A toast never takes the keyboard: Escape and the arrow keys keep going to a modal or
/// menu that is open under it.
/// </summary>
public static class CdsToast
{
    /// <summary>Opens a toast on the page that contains <paramref name="context"/>.</summary>
    /// <param name="context">Any element on the target page (or <c>null</c> for the current page).</param>
    /// <param name="options">Kind, text and timeout.</param>
    /// <returns>The overlay; <see cref="CdsOverlay.Close"/> removes the toast.</returns>
    public static CdsOverlay Show(Element? context, CdsToastOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        var toast = Create(options);
        var overlay = CdsOverlayService.Open(toast,
            new CdsOverlayOptions { Anchor = null, LightDismiss = false, CloseOnEscape = false, ReturnFocus = false, Animate = false }, context);
        toast.Closed += (_, _) => overlay.Close();
        overlay.Closed += (_, _) => options.Closed?.Invoke();
        return overlay;
    }

    /// <summary>Builds the toast view for <paramref name="options"/> (without showing it).</summary>
    public static CdsToastNotification Create(CdsToastOptions options)
    {
        var toast = new CdsToastNotification
        {
            Kind = options.Kind,
            Title = options.Title,
            Subtitle = options.Subtitle,
            Caption = options.Caption,
            Timeout = options.Timeout,
            IsLowContrast = options.IsLowContrast,
            HideCloseButton = options.HideCloseButton,
            Role = options.Role,
            StatusIconDescription = options.StatusIconDescription,
        };
        if (options.CloseButtonDescription is { } description) toast.CloseButtonDescription = description;
        if (options.CloseRequested is { } requested) toast.CloseRequested += (_, e) => requested(e);
        return toast;
    }
}

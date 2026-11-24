namespace Maui.Carbon;

/// <summary>
/// A transient message that floats above the page to report the result of an action: status icon, title, subtitle,
/// optional caption and a close button, 288 wide (352 from the max breakpoint, 1584 up), with a <c>shadow</c>. It can close itself after
/// <see cref="Timeout"/> milliseconds; only auto-dismiss low-severity messages that can be found elsewhere, never errors.
/// Show it with <see cref="CdsToast.Show"/> so it lands in the page's top-right toast region.
/// It holds no action: a toast that times out must never hold something the person needs.
/// </summary>
public class CdsToastNotification : CdsDismissibleNotification
{
    /// <summary>Bindable property for <see cref="Caption"/>.</summary>
    public static readonly BindableProperty CaptionProperty = BindableProperty.Create(
        nameof(Caption), typeof(string), typeof(CdsToastNotification), null,
        propertyChanged: static (b, _, _) => ((CdsToastNotification)b).Refresh());

    /// <summary>Bindable property for <see cref="Timeout"/>.</summary>
    public static readonly BindableProperty TimeoutProperty = BindableProperty.Create(
        nameof(Timeout), typeof(int), typeof(CdsToastNotification), 0,
        coerceValue: static (_, v) => Math.Max(0, (int)v),
        propertyChanged: static (b, _, _) => ((CdsToastNotification)b).RestartTimer());

    /// <summary>The width of a toast below the max breakpoint (1584); from there it is <see cref="WideToastWidth"/>.</summary>
    public const double ToastWidth = NotificationWidthModel.Narrow;

    /// <summary>The width of a toast from the max breakpoint (1584 up).</summary>
    public const double WideToastWidth = 352;

    readonly Grid _content = new() { ColumnSpacing = CdsSpacing.Spacing05, Margin = new Thickness(3, 0, 0, 0) };
    readonly CdsText _title = CdsNotificationText.CreateLine(CdsTypeStyle.HeadingCompact01);
    readonly CdsText _subtitle = CdsNotificationText.CreateLine(CdsTypeStyle.BodyCompact01);
    readonly CdsText _caption = CdsNotificationText.CreateLine(CdsTypeStyle.BodyCompact01);
    IDispatcherTimer? _timer;

    /// <summary>Creates an error toast in the high-contrast style.</summary>
    public CdsToastNotification()
    {
        Kind = CdsNotificationKind.Error;
        RegisterText(_title);
        RegisterText(_subtitle);
        RegisterText(_caption);
        _caption.Margin = new Thickness(0, CdsSpacing.Spacing03, 0, 0);

        _content.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        _content.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        StatusIcon.Margin = new Thickness(0, CdsSpacing.Spacing05, 0, 0);
        var texts = new VerticalStackLayout
        {
            Padding = new Thickness(0, CdsSpacing.Spacing05),
            Spacing = 0,
            Children = { _title, _subtitle, _caption, BodyHost },
        };
        _content.Add(StatusIcon, 0);
        _content.Add(texts, 1);

        WidthRequest = NotificationWidthModel.Toast(ViewportWidth);
        HorizontalOptions = LayoutOptions.Start;
        CdsTokens.SetShadow(Surface, CdsColorKeys.Shadow, 2, 6);
        SetBody(_content);

        Loaded += (_, _) => RestartTimer();
        Unloaded += (_, _) => StopTimer();
    }

    /// <summary>Creates a toast with a kind, title and subtitle.</summary>
    public CdsToastNotification(CdsNotificationKind kind, string title, string? subtitle = null) : this()
    {
        Kind = kind;
        Title = title;
        Subtitle = subtitle;
    }

    /// <summary>Small supporting text under the subtitle, such as a timestamp ("2 minutes ago"). Use one format everywhere.</summary>
    public string? Caption
    {
        get => (string?)GetValue(CaptionProperty);
        set => SetValue(CaptionProperty, value);
    }

    /// <summary>
    /// Milliseconds until the toast closes itself (through <see cref="CdsDismissibleNotification.Close"/>, so
    /// <c>CloseRequested</c> can cancel it). 0, the default, never closes. About 5000 for success messages.
    /// </summary>
    public int Timeout
    {
        get => (int)GetValue(TimeoutProperty);
        set => SetValue(TimeoutProperty, value);
    }

    /// <inheritdoc/>
    protected override void UpdateContent()
    {
        _title.Text = Title;
        _title.IsVisible = !string.IsNullOrEmpty(Title);
        _subtitle.Text = Subtitle;
        _subtitle.IsVisible = !string.IsNullOrEmpty(Subtitle);
        _caption.Text = Caption;
        _caption.IsVisible = !string.IsNullOrEmpty(Caption);
        _content.Padding = new Thickness(13, 0, CloseSpace + CdsSpacing.Spacing05, 0);
    }

    /// <inheritdoc/>
    private protected override void OnViewportWidthChanged() => WidthRequest = NotificationWidthModel.Toast(ViewportWidth);

    void RestartTimer()
    {
        StopTimer();
        if (Timeout <= 0 || !IsLoaded || Dispatcher is not { } dispatcher) return;
        _timer = dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromMilliseconds(Timeout);
        _timer.IsRepeating = false;
        _timer.Tick += OnTimeout;
        _timer.Start();
    }

    void StopTimer()
    {
        if (_timer is null) return;
        _timer.Stop();
        _timer.Tick -= OnTimeout;
        _timer = null;
    }

    void OnTimeout(object? sender, EventArgs e)
    {
        StopTimer();
        Close();
    }
}

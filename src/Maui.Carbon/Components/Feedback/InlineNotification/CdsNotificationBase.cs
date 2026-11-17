namespace Maui.Carbon;

/// <summary>
/// Shared chrome of the notification family: a filled frame with a 3px colored start edge, an optional 1px outline,
/// the 20px status icon and the title and subtitle. Colors come from <see cref="CdsNotificationTokens"/>.
/// Use <see cref="CdsInlineNotification"/>, <see cref="CdsActionableNotification"/>, <see cref="CdsToastNotification"/>
/// or <see cref="CdsCallout"/>.
/// </summary>
[ContentProperty(nameof(Body))]
public abstract class CdsNotificationBase : CdsControl
{
    /// <summary>Bindable property for <see cref="Kind"/>.</summary>
    public static readonly BindableProperty KindProperty = BindableProperty.Create(
        nameof(Kind), typeof(CdsNotificationKind), typeof(CdsNotificationBase), CdsNotificationKind.Info, propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="IsLowContrast"/>.</summary>
    public static readonly BindableProperty IsLowContrastProperty = BindableProperty.Create(
        nameof(IsLowContrast), typeof(bool), typeof(CdsNotificationBase), false, propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="Title"/>.</summary>
    public static readonly BindableProperty TitleProperty = BindableProperty.Create(
        nameof(Title), typeof(string), typeof(CdsNotificationBase), null, propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="Subtitle"/>.</summary>
    public static readonly BindableProperty SubtitleProperty = BindableProperty.Create(
        nameof(Subtitle), typeof(string), typeof(CdsNotificationBase), null, propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="StatusIconDescription"/>.</summary>
    public static readonly BindableProperty StatusIconDescriptionProperty = BindableProperty.Create(
        nameof(StatusIconDescription), typeof(string), typeof(CdsNotificationBase), null, propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="Body"/>.</summary>
    public static readonly BindableProperty BodyProperty = BindableProperty.Create(
        nameof(Body), typeof(View), typeof(CdsNotificationBase), null, propertyChanged: OnBodyChanged);

    readonly BoxView _edge = new() { WidthRequest = 3, HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Fill, InputTransparent = true };
    readonly Border _outline = new()
    {
        StrokeThickness = 1,
        BackgroundColor = Colors.Transparent,
        StrokeShape = new Microsoft.Maui.Controls.Shapes.Rectangle(),
        InputTransparent = true,
        IsVisible = false,
    };
    readonly List<Label> _texts = [];
    readonly List<View> _topLayer = [];
    bool _built;

    /// <summary>Creates the shared chrome. The derived constructor builds its layout and calls <see cref="SetBody"/>.</summary>
    protected CdsNotificationBase()
    {
        Surface = new Grid { BackgroundColor = Colors.Transparent };
        StatusIcon = new CdsIcon(CdsIconName.InformationFilled, 20)
        {
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.Start,
        };
        BodyHost = new ContentView { IsVisible = false };
        AutomationProperties.SetExcludedWithChildren(_edge, true);
        AutomationProperties.SetExcludedWithChildren(_outline, true);
        Loaded += (_, _) => Announce();
    }

    /// <summary>The status: icon and accent colors. Default <see cref="CdsNotificationKind.Info"/>.</summary>
    public CdsNotificationKind Kind
    {
        get => (CdsNotificationKind)GetValue(KindProperty);
        set => SetValue(KindProperty, value);
    }

    /// <summary>
    /// Low contrast (<c>notification-background-*</c> fill, <c>text-primary</c>, <c>support-*</c> accents) instead of the
    /// inverse style. Low contrast is the safer default; do not mix both styles on one surface.
    /// </summary>
    public bool IsLowContrast
    {
        get => (bool)GetValue(IsLowContrastProperty);
        set => SetValue(IsLowContrastProperty, value);
    }

    /// <summary>The outcome without a period ("Deployment failed"). Semibold.</summary>
    public string? Title
    {
        get => (string?)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>The reason and next step in one or two short sentences; do not repeat the title.</summary>
    public string? Subtitle
    {
        get => (string?)GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }

    /// <summary>Accessible name of the status icon, so the kind is not conveyed by color alone. Default "&lt;kind&gt; icon".</summary>
    public string? StatusIconDescription
    {
        get => (string?)GetValue(StatusIconDescriptionProperty);
        set => SetValue(StatusIconDescriptionProperty, value);
    }

    /// <summary>Extra content after the subtitle, such as an inline link. Keep it to text and links.</summary>
    public View? Body
    {
        get => (View?)GetValue(BodyProperty);
        set => SetValue(BodyProperty, value);
    }

    /// <summary>The colors in effect for <see cref="Kind"/> and <see cref="IsLowContrast"/>.</summary>
    protected CdsNotificationVisuals Visuals => CdsNotificationTokens.Resolve(Kind, IsLowContrast);

    /// <summary>The root layout the chrome is drawn into.</summary>
    protected Grid Surface { get; }

    /// <summary>The 20px status icon (the derived layout positions it).</summary>
    protected CdsIcon StatusIcon { get; }

    /// <summary>Hosts <see cref="Body"/> (the derived layout places it after the subtitle).</summary>
    protected ContentView BodyHost { get; }

    /// <summary>Opacity of the 1px accent outline: 0 for none, 1 for a full outline, 0.4 for Carbon's faint outline.</summary>
    protected virtual double OutlineOpacity => 0;

    /// <summary>How the notification is announced when it appears.</summary>
    protected virtual CdsNotificationRole AnnouncedRole => CdsNotificationRole.None;

    /// <summary>Called when a property that affects layout or content changed. The derived class updates labels and spacing.</summary>
    protected abstract void UpdateContent();

    /// <summary>Registers a label whose color follows the notification's text token.</summary>
    protected void RegisterText(Label label) => _texts.Add(label);

    /// <summary>Adds a view above the content (the close button) in the frame.</summary>
    protected void AddTopLayer(View view) => _topLayer.Add(view);

    /// <summary>Completes the chrome with the derived layout.</summary>
    protected void SetBody(View content)
    {
        Surface.Add(_outline);
        Surface.Add(_edge);
        Surface.Add(content);
        foreach (var v in _topLayer) Surface.Add(v);
        SetChrome(Surface);
        _built = true;
        Refresh();
    }

    /// <summary>The text announced to screen readers: the title and subtitle as one sentence.</summary>
    protected string AnnouncementText
    {
        get
        {
            var parts = new[] { Title, Subtitle }.Where(p => !string.IsNullOrWhiteSpace(p)).Select(p => p!.TrimEnd('.', ' '));
            return string.Join(". ", parts);
        }
    }

    /// <summary>Re-applies content and visuals.</summary>
    protected void Refresh()
    {
        if (!_built) return;
        StatusIcon.Icon = CdsNotificationTokens.Icon(Kind);
        StatusIcon.Description = StatusIconDescription ?? CdsNotificationTokens.DefaultIconDescription(Kind);
        BodyHost.Content = Body;
        BodyHost.IsVisible = Body is not null;
        UpdateContent();
        RefreshVisuals();
    }

    /// <summary>Announces the notification through the platform screen reader (called when it is shown).</summary>
    protected virtual void Announce()
    {
        if (AnnouncedRole == CdsNotificationRole.None) return;
        var text = AnnouncementText;
        if (text.Length == 0) return;
        try
        {
            SemanticScreenReader.Announce(text);
        }
        catch (Exception)
        {
            // No screen reader service on this platform (tests, unsupported hosts): nothing to announce.
        }
    }

    /// <inheritdoc/>
    protected override void ApplyVisuals()
    {
        var v = Visuals;
        SetColor(Surface, BackgroundColorProperty, v.Background);
        SetColor(_edge, BoxView.ColorProperty, v.Accent);
        SetBrush(_outline, Border.StrokeProperty, v.Outline);
        _outline.Opacity = OutlineOpacity;
        _outline.IsVisible = v.Outline is not null && OutlineOpacity > 0;
        SetColor(StatusIcon, CdsIcon.ColorProperty, v.Accent);
        foreach (var text in _texts) SetColor(text, Label.TextColorProperty, v.Text);
    }

    static void OnChanged(BindableObject b, object? o, object? n) => ((CdsNotificationBase)b).Refresh();

    static void OnBodyChanged(BindableObject b, object? o, object? n) => ((CdsNotificationBase)b).Refresh();
}

/// <summary>Builds the "bold title then subtitle" flow label inline and callout notifications use.</summary>
internal static class CdsNotificationText
{
    public static Label CreateFlow(out Span title, out Span subtitle)
    {
        title = new Span();
        subtitle = new Span();
        CdsTypography.Apply(title, CdsTypeStyle.HeadingCompact01);
        CdsTypography.Apply(subtitle, CdsTypeStyle.BodyCompact01);
        var label = new Label { LineBreakMode = LineBreakMode.WordWrap, VerticalOptions = LayoutOptions.Center };
        label.FormattedText = new FormattedString { Spans = { title, subtitle } };
        return label;
    }

    public static void SetFlow(Span title, Span subtitle, string? titleText, string? subtitleText)
    {
        var hasTitle = !string.IsNullOrEmpty(titleText);
        title.Text = hasTitle ? titleText + (string.IsNullOrEmpty(subtitleText) ? "" : " ") : "";
        subtitle.Text = subtitleText ?? "";
    }

    public static CdsText CreateLine(CdsTypeStyle style) => new() { TypeStyle = style, LineBreakMode = LineBreakMode.WordWrap };
}

/// <summary>A notification that the person can close (inline, actionable and toast): close button, events and Escape.</summary>
public abstract class CdsDismissibleNotification : CdsNotificationBase
{
    /// <summary>Bindable property for <see cref="HideCloseButton"/>.</summary>
    public static readonly BindableProperty HideCloseButtonProperty = BindableProperty.Create(
        nameof(HideCloseButton), typeof(bool), typeof(CdsDismissibleNotification), false, propertyChanged: OnCloseChanged);

    /// <summary>Bindable property for <see cref="CloseButtonDescription"/>.</summary>
    public static readonly BindableProperty CloseButtonDescriptionProperty = BindableProperty.Create(
        nameof(CloseButtonDescription), typeof(string), typeof(CdsDismissibleNotification), "Close notification", propertyChanged: OnCloseChanged);

    /// <summary>Bindable property for <see cref="Role"/>.</summary>
    public static readonly BindableProperty RoleProperty = BindableProperty.Create(
        nameof(Role), typeof(CdsNotificationRole), typeof(CdsDismissibleNotification), CdsNotificationRole.Status);

    /// <summary>Bindable property for <see cref="CloseOnEscape"/>.</summary>
    public static readonly BindableProperty CloseOnEscapeProperty = BindableProperty.Create(
        nameof(CloseOnEscape), typeof(bool), typeof(CdsDismissibleNotification), true);

    readonly CdsNotificationCloseButton _close = new();

    /// <summary>Creates the notification and its close button.</summary>
    protected CdsDismissibleNotification()
    {
        _close.Clicked += (_, _) =>
        {
            CloseButtonClicked?.Invoke(this, EventArgs.Empty);
            Close();
        };
        AddTopLayer(_close);
    }

    /// <summary>Raised on every click of the close button (before <see cref="CloseRequested"/>).</summary>
    public event EventHandler? CloseButtonClicked;

    /// <summary>Raised when the notification is about to close (close button, Escape, timeout). Set <c>Cancel</c> to keep it open.</summary>
    public event EventHandler<CdsNotificationCloseEventArgs>? CloseRequested;

    /// <summary>Raised after the notification closed and hid itself.</summary>
    public event EventHandler? Closed;

    /// <summary>
    /// Hides the close button, for a message the person must read or act on, or a toast that closes itself.
    /// Inline notifications never dismiss themselves.
    /// </summary>
    public bool HideCloseButton
    {
        get => (bool)GetValue(HideCloseButtonProperty);
        set => SetValue(HideCloseButtonProperty, value);
    }

    /// <summary>Accessible name of the close button. Default "Close notification".</summary>
    public string CloseButtonDescription
    {
        get => (string)GetValue(CloseButtonDescriptionProperty);
        set => SetValue(CloseButtonDescriptionProperty, value);
    }

    /// <summary>
    /// How the notification is announced when it appears: <c>Status</c> (polite, default) or <c>Alert</c> for urgent
    /// errors. Render the notification when the event happens so it is announced.
    /// </summary>
    public CdsNotificationRole Role
    {
        get => (CdsNotificationRole)GetValue(RoleProperty);
        set => SetValue(RoleProperty, value);
    }

    /// <summary>Escape closes the notification while focus is inside it. Default <c>true</c>.</summary>
    public bool CloseOnEscape
    {
        get => (bool)GetValue(CloseOnEscapeProperty);
        set => SetValue(CloseOnEscapeProperty, value);
    }

    /// <summary>The close button's slot width: 48 when shown, 0 when hidden.</summary>
    protected double CloseSpace => HideCloseButton ? 0 : CdsSizing.Large;

    /// <inheritdoc/>
    protected override CdsNotificationRole AnnouncedRole => Role;

    /// <summary>Closes the notification: raises <see cref="CloseRequested"/>, then hides it and raises <see cref="Closed"/> unless canceled.</summary>
    /// <returns><c>true</c> when it closed.</returns>
    public bool Close()
    {
        var args = new CdsNotificationCloseEventArgs();
        CloseRequested?.Invoke(this, args);
        if (args.Cancel) return false;
        IsVisible = false;
        Closed?.Invoke(this, EventArgs.Empty);
        return true;
    }

    /// <inheritdoc/>
    protected override void ApplyVisuals()
    {
        base.ApplyVisuals();
        _close.Visuals = Visuals;
        _close.IsVisible = !HideCloseButton;
        _close.Description = CloseButtonDescription;
    }

    /// <inheritdoc/>
    protected internal override bool OnKeyDown(CdsKeyEventArgs e)
    {
        if (e.Key != CdsKey.Escape || !CloseOnEscape || !IsVisible) return false;
        return Close();
    }

    static void OnCloseChanged(BindableObject b, object? o, object? n) => ((CdsDismissibleNotification)b).Refresh();
}

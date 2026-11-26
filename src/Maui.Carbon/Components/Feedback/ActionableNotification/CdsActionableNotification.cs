using System.Windows.Input;

namespace Maui.Carbon;

/// <summary>
/// A notification with one action button, in a toast layout (stacked, 288 wide, tertiary action under the text) or an
/// inline layout (<see cref="IsInline"/>: full width up to 288 below the md breakpoint, then 608, 736 from lg and 832 from
/// max; the ghost action shares the row with the text from md, 672 up, and sits under it below).
/// It stays until dismissed, even in the toast layout. One action only, labelled with one or two words. By default it
/// is announced as an alert dialog: when it appears focus moves to the action, and Tab and Shift+Tab wrap between the
/// action and the close button until it closes. Set <see cref="CdsDismissibleNotification.Role"/> to
/// <see cref="CdsNotificationRole.Status"/> or <see cref="HasFocus"/> to <c>false</c> to leave focus where it is.
/// </summary>
public class CdsActionableNotification : CdsDismissibleNotification
{
    /// <summary>Bindable property for <see cref="Caption"/>.</summary>
    public static readonly BindableProperty CaptionProperty = BindableProperty.Create(
        nameof(Caption), typeof(string), typeof(CdsActionableNotification), null, propertyChanged: OnLayoutChanged);

    /// <summary>Bindable property for <see cref="ActionButtonLabel"/>.</summary>
    public static readonly BindableProperty ActionButtonLabelProperty = BindableProperty.Create(
        nameof(ActionButtonLabel), typeof(string), typeof(CdsActionableNotification), null, propertyChanged: OnLayoutChanged);

    /// <summary>Bindable property for <see cref="IsInline"/>.</summary>
    public static readonly BindableProperty IsInlineProperty = BindableProperty.Create(
        nameof(IsInline), typeof(bool), typeof(CdsActionableNotification), false, propertyChanged: OnLayoutChanged);

    /// <summary>Bindable property for <see cref="HasFocus"/>.</summary>
    public static readonly BindableProperty HasFocusProperty = BindableProperty.Create(
        nameof(HasFocus), typeof(bool), typeof(CdsActionableNotification), true);

    /// <summary>Bindable property for <see cref="ActionCommand"/>.</summary>
    public static readonly BindableProperty ActionCommandProperty = BindableProperty.Create(
        nameof(ActionCommand), typeof(ICommand), typeof(CdsActionableNotification), null);

    /// <summary>Bindable property for <see cref="ActionCommandParameter"/>.</summary>
    public static readonly BindableProperty ActionCommandParameterProperty = BindableProperty.Create(
        nameof(ActionCommandParameter), typeof(object), typeof(CdsActionableNotification), null);

    // How long after an item of the notification lost focus a sentinel that gains focus counts as Tab from inside.
    const long FocusHandOverMilliseconds = 500;

    readonly ActionableNotificationLayout _content;
    readonly ContentView _startSentinel;
    readonly ContentView _endSentinel;
    readonly VerticalStackLayout _texts = new() { Spacing = 0 };
    readonly Label _flow;
    readonly Span _flowTitle;
    readonly Span _flowSubtitle;
    readonly CdsText _title = CdsNotificationText.CreateLine(CdsTypeStyle.HeadingCompact01);
    readonly CdsText _subtitle = CdsNotificationText.CreateLine(CdsTypeStyle.BodyCompact01);
    readonly CdsText _caption = CdsNotificationText.CreateLine(CdsTypeStyle.BodyCompact01);
    readonly CdsNotificationAction _action = new();
    CdsControl? _leftItem;
    long _leftItemAt = long.MinValue;

    /// <summary>Creates an error notification in the toast layout.</summary>
    public CdsActionableNotification()
    {
        Kind = CdsNotificationKind.Error;
        Role = CdsNotificationRole.AlertDialog;
        // A constructor default only (CONVENTIONS: never reassigned on a rebuild). The toast layout is 288 wide and must not
        // be centered; the inline layout fills the offered width through its width box under any alignment.
        HorizontalOptions = LayoutOptions.Start;
        _flow = CdsNotificationText.CreateFlow(out _flowTitle, out _flowSubtitle);
        RegisterText(_flow);
        RegisterText(_title);
        RegisterText(_subtitle);
        RegisterText(_caption);
        _caption.Margin = new Thickness(0, CdsSpacing.Spacing05 + CdsSpacing.Spacing03, 0, 0);

        _texts.Children.Add(_flow);
        _texts.Children.Add(_title);
        _texts.Children.Add(_subtitle);
        _texts.Children.Add(_caption);
        _texts.Children.Add(BodyHost);

        _action.VerticalOptions = LayoutOptions.Start;
        _action.Margin = new Thickness(0);
        _content = new ActionableNotificationLayout(StatusIcon, _texts, _action) { Margin = new Thickness(3, 0, 0, 0) };
        // Carbon decides by the viewport (672 up: one row); the decision is made while measuring, never after arranging.
        _content.SingleRow = width =>
            IsInline && NotificationWidthModel.IsSingleRow(ViewportWidth, width + _content.Margin.HorizontalThickness);

        _action.Clicked += (_, _) =>
        {
            ActionClicked?.Invoke(this, EventArgs.Empty);
            if (ActionCommand?.CanExecute(ActionCommandParameter) == true) ActionCommand.Execute(ActionCommandParameter);
        };
        _action.FocusLost += OnItemFocusLost;
        CloseButton.FocusLost += OnItemFocusLost;

        // Focus sentinels (the web's focus-trap spans): Tab from the last item or Shift+Tab from the first lands on one
        // and is sent back into the notification. One leads the tab order (top start), one ends it (bottom end).
        _startSentinel = CreateSentinel(LayoutOptions.Start, LayoutOptions.Start, start: true);
        _endSentinel = CreateSentinel(LayoutOptions.End, LayoutOptions.End, start: false);

        SetBody(_content);
        Surface.Children.Insert(0, _startSentinel);
        Surface.Children.Add(_endSentinel);
    }

    /// <summary>Creates an actionable notification with a kind, title, subtitle and action label.</summary>
    public CdsActionableNotification(CdsNotificationKind kind, string title, string? subtitle, string actionLabel) : this()
    {
        Kind = kind;
        Title = title;
        Subtitle = subtitle;
        ActionButtonLabel = actionLabel;
    }

    /// <summary>Raised when the action button is activated.</summary>
    public event EventHandler? ActionClicked;

    /// <summary>Small supporting text under the subtitle (toast layout), such as a timestamp.</summary>
    public string? Caption
    {
        get => (string?)GetValue(CaptionProperty);
        set => SetValue(CaptionProperty, value);
    }

    /// <summary>The action's label: one or two words, usually leading to a related page or flow ("View logs"). No label, no button.</summary>
    public string? ActionButtonLabel
    {
        get => (string?)GetValue(ActionButtonLabelProperty);
        set => SetValue(ActionButtonLabelProperty, value);
    }

    /// <summary>
    /// Inline layout (full width, title and subtitle share a line, ghost action) instead of the 288 wide toast layout
    /// (tertiary action under the text). Default <c>false</c>.
    /// </summary>
    public bool IsInline
    {
        get => (bool)GetValue(IsInlineProperty);
        set => SetValue(IsInlineProperty, value);
    }

    /// <summary>With the <see cref="CdsNotificationRole.AlertDialog"/> role, move focus to the action when shown. Default <c>true</c>.</summary>
    public bool HasFocus
    {
        get => (bool)GetValue(HasFocusProperty);
        set => SetValue(HasFocusProperty, value);
    }

    /// <summary>Command run when the action is activated.</summary>
    public ICommand? ActionCommand
    {
        get => (ICommand?)GetValue(ActionCommandProperty);
        set => SetValue(ActionCommandProperty, value);
    }

    /// <summary>Parameter passed to <see cref="ActionCommand"/>.</summary>
    public object? ActionCommandParameter
    {
        get => GetValue(ActionCommandParameterProperty);
        set => SetValue(ActionCommandParameterProperty, value);
    }

    /// <inheritdoc/>
    protected override double OutlineOpacity => IsInline && IsLowContrast ? 0.4 : 0;

    /// <summary>Whether Tab wraps inside the notification: the alert-dialog role with an action or close button to wrap between.</summary>
    internal bool WrapsFocus => Role == CdsNotificationRole.AlertDialog && (_action.IsVisible || !HideCloseButton);

    /// <summary>Whether focus moves to the action when the notification appears.</summary>
    internal bool TakesFocusWhenShown => HasFocus && Role == CdsNotificationRole.AlertDialog && _action.IsVisible;

    /// <summary>Whether the action shares the row with the text (decided by the last measure).</summary>
    internal bool IsSingleRow => _content.IsSingleRow;

    /// <summary>The layout of the icon, text and action (tests).</summary>
    internal ActionableNotificationLayout ContentLayout => _content;

    /// <summary>The action button (tests).</summary>
    internal CdsControl ActionButton => _action;

    /// <summary>The focus sentinels (tests).</summary>
    internal CdsHitTarget StartSentinel => (CdsHitTarget)_startSentinel.Content;

    /// <inheritdoc cref="StartSentinel"/>
    internal CdsHitTarget EndSentinel => (CdsHitTarget)_endSentinel.Content;

    /// <summary>
    /// The item a focus sentinel sends focus to: when focus comes from inside the notification (Tab from the last item or
    /// Shift+Tab from the first), the other item, so focus wraps; when it comes from outside, the nearest item.
    /// </summary>
    internal CdsControl? SentinelTarget(bool start)
    {
        var hasAction = _action.IsVisible;
        var hasClose = !HideCloseButton;
        CdsControl? from =
            _action.Is(CdsInteractionState.Focused) ? _action
            : CloseButton.Is(CdsInteractionState.Focused) ? CloseButton
            : Environment.TickCount64 - _leftItemAt <= FocusHandOverMilliseconds ? _leftItem
            : null;
        var cameFrom = ReferenceEquals(from, _action) ? NotificationFocusItem.Action
            : ReferenceEquals(from, CloseButton) ? NotificationFocusItem.Close
            : NotificationFocusItem.None;
        return NotificationFocusModel.SentinelTarget(start, cameFrom, hasAction, hasClose) switch
        {
            NotificationFocusItem.Action => _action,
            NotificationFocusItem.Close => CloseButton,
            _ => null,
        };
    }

    /// <inheritdoc/>
    private protected override void OnShown()
    {
        base.OnShown();
        if (TakesFocusWhenShown) FocusSoon(_action);
    }

    /// <inheritdoc/>
    private protected override void OnViewportWidthChanged() => Refresh();

    ContentView CreateSentinel(LayoutOptions horizontal, LayoutOptions vertical, bool start)
    {
        var hit = new CdsHitTarget();
        AutomationProperties.SetIsInAccessibleTree(hit, false);
        hit.Focused += (_, _) =>
        {
            if (WrapsFocus && SentinelTarget(start) is { } target) FocusSoon(target);
        };
        // 1 x 1: a zero-sized view cannot take keyboard focus. The hit target draws nothing.
        return new ContentView
        {
            WidthRequest = 1,
            HeightRequest = 1,
            HorizontalOptions = horizontal,
            VerticalOptions = vertical,
            IsVisible = false,
            Content = hit,
        };
    }

    void OnItemFocusLost(object? sender, EventArgs e)
    {
        _leftItem = sender as CdsControl;
        _leftItemAt = Environment.TickCount64;
    }

    // Focus moves after the current focus change or layout pass, when the target is laid out and visible.
    void FocusSoon(CdsControl target)
    {
        if (Handler is not null && Dispatcher is { } dispatcher) dispatcher.Dispatch(() => target.Focus());
        else target.Focus();
    }

    /// <inheritdoc/>
    protected override void UpdateContent()
    {
        var inline = IsInline;
        var hasAction = !string.IsNullOrEmpty(ActionButtonLabel);

        // Texts: title and subtitle flow together inline, stack in the toast layout.
        CdsNotificationText.SetFlow(_flowTitle, _flowSubtitle, Title, Subtitle);
        _flow.IsVisible = inline;
        _title.Text = Title;
        _title.IsVisible = !inline && !string.IsNullOrEmpty(Title);
        _subtitle.Text = Subtitle;
        _subtitle.IsVisible = !inline && !string.IsNullOrEmpty(Subtitle);
        _caption.Text = Caption;
        _caption.IsVisible = !inline && !string.IsNullOrEmpty(Caption);

        _action.IsVisible = hasAction;
        _action.Text = ActionButtonLabel;
        _action.Tertiary = !inline;
        _action.LowContrast = IsLowContrast;
        _action.Visuals = Visuals;

        MinimumWidthRequest = NotificationWidthModel.Narrow;
        if (inline)
        {
            WidthRequest = -1;
            MaximumWidthRequest = double.PositiveInfinity;
            // The notification's width box fills the offered width up to the cap whatever the app's alignment is.
            SetMaxContentWidth(NotificationWidthModel.Inline(ViewportWidth));
            StatusIcon.Margin = new Thickness(0, 14, 0, 0);
            _texts.Padding = new Thickness(0, 15);
            // One row: details 13 in, the action 8 above and below, then the 48 close slot (8 when the close button is
            // hidden). Stacked: the close button floats at the top end over a 48 slot, the action sits under the text.
            _content.Configure(
                singleRowPadding: new Thickness(13, 0, HideCloseButton ? CdsSpacing.Spacing03 : CloseSpace, 0),
                stackedPadding: new Thickness(13, 0, CloseSpace, 0),
                singleRowActionInsets: new Thickness(0, CdsSpacing.Spacing03),
                stackedActionInsets: new Thickness(20, 0, 0, CdsSpacing.Spacing03));
        }
        else
        {
            WidthRequest = CdsToastNotification.ToastWidth;
            MaximumWidthRequest = CdsToastNotification.ToastWidth;
            SetMaxContentWidth(double.PositiveInfinity);
            StatusIcon.Margin = new Thickness(0, CdsSpacing.Spacing05, 0, 0);
            _texts.Padding = new Thickness(0, 15, 0, hasAction ? CdsSpacing.Spacing03 : CdsSpacing.Spacing05);
            var stacked = new Thickness(13, 0, CloseSpace + CdsSpacing.Spacing03, 0);
            _content.Configure(stacked, stacked, new Thickness(0), new Thickness(34, 0, 0, CdsSpacing.Spacing05));
        }
        var wraps = WrapsFocus;
        _startSentinel.IsVisible = wraps;
        _endSentinel.IsVisible = wraps;
        CdsTokens.SetShadow(Surface, inline ? null : CdsColorKeys.Shadow, 2, 6);
    }

    static void OnLayoutChanged(BindableObject b, object? o, object? n) => ((CdsActionableNotification)b).Refresh();
}

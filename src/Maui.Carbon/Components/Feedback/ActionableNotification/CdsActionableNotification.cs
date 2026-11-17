using System.Windows.Input;

namespace Maui.Carbon;

/// <summary>
/// A notification with one action button, in a toast layout (stacked, 288 wide, tertiary action under the text) or an
/// inline layout (<see cref="IsInline"/>: full width up to 608, ghost action on the same row from about 600 wide).
/// It stays until dismissed, even in the toast layout. One action only, labelled with one or two words. By default it
/// is announced as an alert dialog and focus moves to the action; set <see cref="CdsDismissibleNotification.Role"/> to
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

    const double MaxInlineWidth = 608;
    const double WideThreshold = 576;

    readonly Grid _content = new() { Margin = new Thickness(3, 0, 0, 0), ColumnSpacing = CdsSpacing.Spacing05 };
    readonly VerticalStackLayout _texts = new() { Spacing = 0 };
    readonly Label _flow;
    readonly Span _flowTitle;
    readonly Span _flowSubtitle;
    readonly CdsText _title = CdsNotificationText.CreateLine(CdsTypeStyle.HeadingCompact01);
    readonly CdsText _subtitle = CdsNotificationText.CreateLine(CdsTypeStyle.BodyCompact01);
    readonly CdsText _caption = CdsNotificationText.CreateLine(CdsTypeStyle.BodyCompact01);
    readonly CdsNotificationAction _action = new();
    bool _wide;

    /// <summary>Creates an error notification in the toast layout.</summary>
    public CdsActionableNotification()
    {
        Kind = CdsNotificationKind.Error;
        Role = CdsNotificationRole.AlertDialog;
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

        _content.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        _content.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        _content.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        _content.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        _content.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        _content.Add(StatusIcon);
        _content.Add(_texts);
        _content.Add(_action);

        _action.Clicked += (_, _) =>
        {
            ActionClicked?.Invoke(this, EventArgs.Empty);
            if (ActionCommand?.CanExecute(ActionCommandParameter) == true) ActionCommand.Execute(ActionCommandParameter);
        };
        SizeChanged += (_, _) =>
        {
            var wide = IsInline && Width >= WideThreshold;
            if (wide == _wide) return;
            _wide = wide;
            Refresh();
        };
        Loaded += (_, _) =>
        {
            if (HasFocus && Role == CdsNotificationRole.AlertDialog && _action.IsVisible) _action.Focus();
        };

        SetBody(_content);
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

        MinimumWidthRequest = 288;
        if (inline)
        {
            WidthRequest = -1;
            MaximumWidthRequest = double.PositiveInfinity;
            SetMaxContentWidth(MaxInlineWidth);
            HorizontalOptions = LayoutOptions.Fill;
            StatusIcon.Margin = new Thickness(0, 14, 0, 0);
            _texts.Padding = new Thickness(0, 15);
            _content.Padding = new Thickness(13, 0, _wide ? (HideCloseButton ? CdsSpacing.Spacing03 : CloseSpace) : CloseSpace, 0);
            Grid.SetRow(StatusIcon, 0);
            Grid.SetColumn(StatusIcon, 0);
            Grid.SetRow(_texts, 0);
            Grid.SetColumn(_texts, 1);
            Grid.SetColumnSpan(_texts, _wide ? 1 : 2);
            if (_wide)
            {
                Grid.SetRow(_action, 0);
                Grid.SetColumn(_action, 2);
                Grid.SetColumnSpan(_action, 1);
                _action.Margin = new Thickness(0, CdsSpacing.Spacing03, 0, CdsSpacing.Spacing03);
                _action.VerticalOptions = LayoutOptions.Center;
            }
            else
            {
                Grid.SetRow(_action, 1);
                Grid.SetColumn(_action, 0);
                Grid.SetColumnSpan(_action, 3);
                _action.Margin = new Thickness(20, 0, 0, CdsSpacing.Spacing03);
                _action.VerticalOptions = LayoutOptions.Start;
            }
        }
        else
        {
            WidthRequest = CdsToastNotification.ToastWidth;
            MaximumWidthRequest = CdsToastNotification.ToastWidth;
            SetMaxContentWidth(double.PositiveInfinity);
            HorizontalOptions = LayoutOptions.Start;
            StatusIcon.Margin = new Thickness(0, CdsSpacing.Spacing05, 0, 0);
            _texts.Padding = new Thickness(0, 15, 0, hasAction ? CdsSpacing.Spacing03 : CdsSpacing.Spacing05);
            _content.Padding = new Thickness(13, 0, CloseSpace + CdsSpacing.Spacing03, 0);
            Grid.SetRow(StatusIcon, 0);
            Grid.SetColumn(StatusIcon, 0);
            Grid.SetRow(_texts, 0);
            Grid.SetColumn(_texts, 1);
            Grid.SetColumnSpan(_texts, 2);
            Grid.SetRow(_action, 1);
            Grid.SetColumn(_action, 0);
            Grid.SetColumnSpan(_action, 3);
            _action.Margin = new Thickness(34, 0, 0, CdsSpacing.Spacing05);
            _action.VerticalOptions = LayoutOptions.Start;
        }
        CdsTokens.SetShadow(Surface, inline ? null : CdsColorKeys.Shadow, 2, 6);
    }

    static void OnLayoutChanged(BindableObject b, object? o, object? n) => ((CdsActionableNotification)b).Refresh();
}

using System.Windows.Input;

namespace Maui.Carbon;

/// <summary>
/// A persistent, non-dismissible message placed in page content to highlight information the person should know before
/// acting (Carbon's former StaticNotification). Low contrast by default; it has no close button and no live-region
/// role, so place it right next to the control or data it is about, before the control it warns about. Do not stack
/// callouts or put many on one page.
/// </summary>
public class CdsCallout : CdsNotificationBase
{
    /// <summary>Bindable property for <see cref="ActionButtonLabel"/>.</summary>
    public static readonly BindableProperty ActionButtonLabelProperty = BindableProperty.Create(
        nameof(ActionButtonLabel), typeof(string), typeof(CdsCallout), null,
        propertyChanged: static (b, _, _) => ((CdsCallout)b).Refresh());

    /// <summary>Bindable property for <see cref="ActionCommand"/>.</summary>
    public static readonly BindableProperty ActionCommandProperty = BindableProperty.Create(
        nameof(ActionCommand), typeof(ICommand), typeof(CdsCallout), null);

    /// <summary>Bindable property for <see cref="ActionCommandParameter"/>.</summary>
    public static readonly BindableProperty ActionCommandParameterProperty = BindableProperty.Create(
        nameof(ActionCommandParameter), typeof(object), typeof(CdsCallout), null);

    readonly Grid _content = new() { Margin = new Thickness(3, 0, 0, 0), ColumnSpacing = CdsSpacing.Spacing05 };
    readonly Label _text;
    readonly Span _title;
    readonly Span _subtitle;
    readonly CdsNotificationAction _action = new();

    /// <summary>Creates a low-contrast info callout.</summary>
    public CdsCallout()
    {
        IsLowContrast = true;
        _text = CdsNotificationText.CreateFlow(out _title, out _subtitle);
        RegisterText(_text);

        _content.Padding = new Thickness(CdsSpacing.Spacing05, 0);
        _content.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        _content.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        _content.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        StatusIcon.Margin = new Thickness(0, 14, 0, 0);
        var texts = new VerticalStackLayout { Padding = new Thickness(0, 15), Spacing = 0, Children = { _text, BodyHost } };
        _action.Margin = new Thickness(0, CdsSpacing.Spacing03);
        _action.VerticalOptions = LayoutOptions.Start;
        _content.Add(StatusIcon, 0);
        _content.Add(texts, 1);
        _content.Add(_action, 2);
        _action.Clicked += (_, _) =>
        {
            ActionClicked?.Invoke(this, EventArgs.Empty);
            if (ActionCommand?.CanExecute(ActionCommandParameter) == true) ActionCommand.Execute(ActionCommandParameter);
        };

        Surface.MinimumHeightRequest = CdsSizing.Large;
        MinimumWidthRequest = 288;
        MaximumWidthRequest = 608;
        HorizontalOptions = LayoutOptions.Fill;
        SetBody(_content);
    }

    /// <summary>Creates a callout with a kind, title and subtitle.</summary>
    public CdsCallout(CdsNotificationKind kind, string? title, string subtitle) : this()
    {
        Kind = kind;
        Title = title;
        Subtitle = subtitle;
    }

    /// <summary>Raised when the optional action button is activated.</summary>
    public event EventHandler? ActionClicked;

    /// <summary>
    /// Label of an optional small ghost button at the end. Carbon prefers an action on the page near the control it
    /// belongs to, so use this sparingly.
    /// </summary>
    public string? ActionButtonLabel
    {
        get => (string?)GetValue(ActionButtonLabelProperty);
        set => SetValue(ActionButtonLabelProperty, value);
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
    protected override double OutlineOpacity => IsLowContrast ? 0.4 : 0;

    /// <inheritdoc/>
    protected override void UpdateContent()
    {
        CdsNotificationText.SetFlow(_title, _subtitle, Title, Subtitle);
        _action.IsVisible = !string.IsNullOrEmpty(ActionButtonLabel);
        _action.Text = ActionButtonLabel;
        _action.DescribedBy = Title;
        _action.Tertiary = false;
        _action.LowContrast = IsLowContrast;
        _action.Visuals = Visuals;
    }
}

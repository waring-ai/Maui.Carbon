using System.Windows.Input;

namespace Maui.Carbon;

/// <summary>
/// An inline link for use inside formatted text (<c>Label.FormattedText</c>): <c>link-primary</c> (or <c>link-visited</c>) text,
/// always underlined, taking the surrounding paragraph's size, that opens <see cref="Href"/> or runs
/// <see cref="Command"/> when tapped. A span is not a view: it cannot take keyboard focus or be named for screen readers on its
/// own. Put the formatted text in a <see cref="CdsParagraph"/>, which gives every link span a Tab stop and an accessible link
/// name; in a plain label the link is pointer-only. A standalone <see cref="CdsLink"/> with <see cref="CdsLink.IsInline"/> also
/// works when the link can sit on its own line.
/// </summary>
public class CdsLinkSpan : Span
{
    /// <summary>Bindable property for <see cref="Href"/>.</summary>
    public static readonly BindableProperty HrefProperty = BindableProperty.Create(
        nameof(Href), typeof(string), typeof(CdsLinkSpan), null);

    /// <summary>Bindable property for <see cref="IsVisited"/>.</summary>
    public static readonly BindableProperty IsVisitedProperty = BindableProperty.Create(
        nameof(IsVisited), typeof(bool), typeof(CdsLinkSpan), false,
        propertyChanged: static (b, _, _) => ((CdsLinkSpan)b).ApplyColor());

    /// <summary>Bindable property for <see cref="IsInverse"/>.</summary>
    public static readonly BindableProperty IsInverseProperty = BindableProperty.Create(
        nameof(IsInverse), typeof(bool), typeof(CdsLinkSpan), false,
        propertyChanged: static (b, _, _) => ((CdsLinkSpan)b).ApplyColor());

    /// <summary>Bindable property for <see cref="IsEnabled"/>.</summary>
    public static readonly BindableProperty IsEnabledProperty = BindableProperty.Create(
        nameof(IsEnabled), typeof(bool), typeof(CdsLinkSpan), true,
        propertyChanged: static (b, _, _) => ((CdsLinkSpan)b).ApplyColor());

    /// <summary>Bindable property for <see cref="Command"/>.</summary>
    public static readonly BindableProperty CommandProperty = BindableProperty.Create(
        nameof(Command), typeof(ICommand), typeof(CdsLinkSpan), null);

    /// <summary>Bindable property for <see cref="CommandParameter"/>.</summary>
    public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(
        nameof(CommandParameter), typeof(object), typeof(CdsLinkSpan), null);

    bool _showsFocus;

    /// <summary>Creates an underlined link span.</summary>
    public CdsLinkSpan()
    {
        TextDecorations = TextDecorations.Underline;
        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => Activate();
        GestureRecognizers.Add(tap);
        ApplyColor();
    }

    /// <summary>Raised when the span is tapped, before <see cref="Command"/> and <see cref="Href"/>.</summary>
    public event EventHandler? Clicked;

    /// <summary>An absolute address opened with the system launcher when tapped.</summary>
    public string? Href
    {
        get => (string?)GetValue(HrefProperty);
        set => SetValue(HrefProperty, value);
    }

    /// <summary>Opts in to the <c>link-visited</c> color.</summary>
    public bool IsVisited
    {
        get => (bool)GetValue(IsVisitedProperty);
        set => SetValue(IsVisitedProperty, value);
    }

    /// <summary>Uses the <c>link-inverse</c> colors for text on inverse surfaces.</summary>
    public bool IsInverse
    {
        get => (bool)GetValue(IsInverseProperty);
        set => SetValue(IsInverseProperty, value);
    }

    /// <summary>When <c>false</c> the span is <c>text-disabled</c>, not underlined and ignores taps. Default <c>true</c>.</summary>
    public bool IsEnabled
    {
        get => (bool)GetValue(IsEnabledProperty);
        set => SetValue(IsEnabledProperty, value);
    }

    /// <summary>Run when tapped.</summary>
    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    /// <summary>The parameter passed to <see cref="Command"/>.</summary>
    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    /// <summary>
    /// Whether the span shows the keyboard focus of its <see cref="CdsParagraph"/> link: a <c>highlight</c> background (a span
    /// cannot draw Carbon's 1px focus outline).
    /// </summary>
    internal bool ShowsFocus
    {
        get => _showsFocus;
        set
        {
            if (_showsFocus == value) return;
            _showsFocus = value;
            ApplyColor();
        }
    }

    /// <summary>Follows the link as a tap does: <see cref="Clicked"/>, <see cref="Command"/>, then <see cref="Href"/>.</summary>
    internal void Activate()
    {
        if (!IsEnabled) return;
        Clicked?.Invoke(this, EventArgs.Empty);
        if (Command?.CanExecute(CommandParameter) == true) Command.Execute(CommandParameter);
        if (!string.IsNullOrEmpty(Href) && Uri.TryCreate(Href, UriKind.Absolute, out var uri))
            _ = Launcher.Default.OpenAsync(uri);
    }

    void ApplyColor()
    {
        var state = IsEnabled ? CdsInteractionState.None : CdsInteractionState.Disabled;
        var v = CdsLinkTokens.Resolve(state, IsVisited, isInline: true, IsInverse);
        TextDecorations = v.Underline ? TextDecorations.Underline : TextDecorations.None;
        CdsTokens.SetColor(this, TextColorProperty, v.Foreground);
        CdsTokens.SetColor(this, BackgroundColorProperty, _showsFocus && IsEnabled ? CdsColorKeys.Highlight : null);
    }
}

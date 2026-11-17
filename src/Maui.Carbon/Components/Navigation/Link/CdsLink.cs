using System.Windows.Input;

namespace Maui.Carbon;

/// <summary>
/// Navigates to another page, site, section or address: <c>link-primary</c> text with an underline on hover,
/// <c>link-primary-hover</c> on hover and <c>text-primary</c> while pressed. Name the destination in the text (never "click here")
/// and use a button for actions. <see cref="Href"/> is opened with the system launcher; <see cref="Clicked"/> and
/// <see cref="Command"/> run first. <see cref="IsInline"/> underlines the link and hides the icon. Put an
/// <see cref="Icon"/> after the text: <see cref="CdsIconName.ArrowRight"/> for internal and <see cref="CdsIconName.Launch"/> for
/// external destinations. For a link in the middle of running text use <see cref="CdsLinkSpan"/> inside a formatted label.
/// </summary>
public class CdsLink : CdsControl
{
    /// <summary>Bindable property for <see cref="Text"/>.</summary>
    public static readonly BindableProperty TextProperty = BindableProperty.Create(
        nameof(Text), typeof(string), typeof(CdsLink), null, propertyChanged: OnLayoutChanged);

    /// <summary>Bindable property for <see cref="Href"/>.</summary>
    public static readonly BindableProperty HrefProperty = BindableProperty.Create(
        nameof(Href), typeof(string), typeof(CdsLink), null);

    /// <summary>Bindable property for <see cref="Size"/>.</summary>
    public static readonly BindableProperty SizeProperty = BindableProperty.Create(
        nameof(Size), typeof(CdsLinkSize), typeof(CdsLink), CdsLinkSize.Md, propertyChanged: OnLayoutChanged);

    /// <summary>Bindable property for <see cref="IsInline"/>.</summary>
    public static readonly BindableProperty IsInlineProperty = BindableProperty.Create(
        nameof(IsInline), typeof(bool), typeof(CdsLink), false, propertyChanged: OnLayoutChanged);

    /// <summary>Bindable property for <see cref="Icon"/>.</summary>
    public static readonly BindableProperty IconProperty = BindableProperty.Create(
        nameof(Icon), typeof(CdsIconName?), typeof(CdsLink), null, propertyChanged: OnLayoutChanged);

    /// <summary>Bindable property for <see cref="IsVisited"/>.</summary>
    public static readonly BindableProperty IsVisitedProperty = BindableProperty.Create(
        nameof(IsVisited), typeof(bool), typeof(CdsLink), false, propertyChanged: OnLayoutChanged);

    /// <summary>Bindable property for <see cref="IsInverse"/>.</summary>
    public static readonly BindableProperty IsInverseProperty = BindableProperty.Create(
        nameof(IsInverse), typeof(bool), typeof(CdsLink), false, propertyChanged: OnLayoutChanged);

    /// <summary>Bindable property for <see cref="Command"/>.</summary>
    public static readonly BindableProperty CommandProperty = BindableProperty.Create(
        nameof(Command), typeof(ICommand), typeof(CdsLink), null, propertyChanged: OnCommandChanged);

    /// <summary>Bindable property for <see cref="CommandParameter"/>.</summary>
    public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(
        nameof(CommandParameter), typeof(object), typeof(CdsLink), null, propertyChanged: OnCommandChanged);

    readonly Grid _root = new() { HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Start };
    readonly CdsText _label = new() { LineBreakMode = LineBreakMode.WordWrap, VerticalOptions = LayoutOptions.Center };
    readonly CdsIcon _icon = new() { IsVisible = false, HorizontalOptions = LayoutOptions.Start };
    readonly Border _focus = new()
    {
        StrokeThickness = 1,
        BackgroundColor = Colors.Transparent,
        InputTransparent = true,
        IsVisible = false,
        Margin = new Thickness(-1),
    };
    readonly CdsHitTarget _hit;

    /// <summary>Creates a link.</summary>
    public CdsLink()
    {
        _root.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        _root.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        _root.Add(_label, 0);
        _root.Add(_icon, 1);
        _label.InputTransparent = true;
        _icon.InputTransparent = true;
        AutomationProperties.SetExcludedWithChildren(_label, true);
        CdsTokens.SetBrush(_focus, Border.StrokeProperty, CdsColorKeys.Focus);
        Grid.SetColumnSpan(_focus, 2);
        _root.Add(_focus);

        _hit = CreateHitTarget(CdsAccessibilityRole.Link);
        Grid.SetColumnSpan(_hit, 2);
        _root.Add(_hit);

        HorizontalOptions = LayoutOptions.Start;
        VerticalOptions = LayoutOptions.Start;
        SetChrome(_root);
        UpdateLayout();
    }

    /// <summary>Creates a link with text and a destination.</summary>
    public CdsLink(string text, string? href = null) : this()
    {
        Text = text;
        Href = href;
    }

    /// <summary>Raised on activation (click, tap or Enter), before <see cref="Command"/> and <see cref="Href"/>.</summary>
    public event EventHandler? Clicked;

    /// <summary>The link text: meaningful words naming the destination.</summary>
    public string? Text
    {
        get => (string?)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>An absolute address opened with the system launcher on activation.</summary>
    public string? Href
    {
        get => (string?)GetValue(HrefProperty);
        set => SetValue(HrefProperty, value);
    }

    /// <summary>Text size. Default <see cref="CdsLinkSize.Md"/> (14/18).</summary>
    public CdsLinkSize Size
    {
        get => (CdsLinkSize)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    /// <summary>Underlined at all times and never shows an icon. Default <c>false</c>.</summary>
    public bool IsInline
    {
        get => (bool)GetValue(IsInlineProperty);
        set => SetValue(IsInlineProperty, value);
    }

    /// <summary>A 16 icon 8 after the text (<c>ArrowRight</c>, <c>Launch</c>); ignored when <see cref="IsInline"/>.</summary>
    public CdsIconName? Icon
    {
        get => (CdsIconName?)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>Opts in to the <c>link-visited</c> color for a destination the user has been to. Use sparingly.</summary>
    public bool IsVisited
    {
        get => (bool)GetValue(IsVisitedProperty);
        set => SetValue(IsVisitedProperty, value);
    }

    /// <summary>Uses the <c>link-inverse</c> colors for links on inverse surfaces (high-contrast notifications, toggletips).</summary>
    public bool IsInverse
    {
        get => (bool)GetValue(IsInverseProperty);
        set => SetValue(IsInverseProperty, value);
    }

    /// <summary>Run on activation; <c>IsEnabled</c> follows <c>CanExecute</c>.</summary>
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

    /// <inheritdoc/>
    protected override void OnActivated()
    {
        Clicked?.Invoke(this, EventArgs.Empty);
        ExecuteCommand();
        if (!string.IsNullOrEmpty(Href) && Uri.TryCreate(Href, UriKind.Absolute, out var uri))
            _ = Launcher.Default.OpenAsync(uri);
    }

    /// <inheritdoc/>
    protected override void ApplyVisuals()
    {
        var v = CdsLinkTokens.Resolve(State, IsVisited, IsInline, IsInverse);
        SetColor(_label, Label.TextColorProperty, v.Foreground);
        SetColor(_icon, CdsIcon.ColorProperty, v.Foreground);
        _label.TextDecorations = v.Underline ? TextDecorations.Underline : TextDecorations.None;
        SetBrush(_focus, Border.StrokeProperty, IsInverse ? CdsColorKeys.FocusInverse : CdsColorKeys.Focus);
        _focus.IsVisible = Is(CdsInteractionState.FocusVisible);
    }

    void UpdateLayout()
    {
        _label.TypeStyle = CdsLinkTokens.TypeStyle(Size);
        _label.Text = Text;
        var showIcon = Icon is not null && !IsInline;
        _icon.Icon = Icon;
        _icon.Size = CdsLinkTokens.IconSize;
        _icon.IsVisible = showIcon;
        _icon.Margin = new Thickness(CdsLinkTokens.IconGap, 0, 0, 0);
        SemanticProperties.SetDescription(_hit, Text);
        RefreshVisuals();
    }

    static void OnLayoutChanged(BindableObject b, object? o, object? n) => ((CdsLink)b).UpdateLayout();

    static void OnCommandChanged(BindableObject b, object? o, object? n)
    {
        var link = (CdsLink)b;
        link.SetCommand(link.Command, link.CommandParameter);
    }
}

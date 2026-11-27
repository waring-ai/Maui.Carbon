using System.Windows.Input;

namespace Maui.Carbon;

/// <summary>
/// One icon action of a card (Carbon for IBM Products' <c>actionIcons</c> entry): an icon-only ghost button named by
/// <see cref="IconDescription"/>. With <see cref="Href"/> it opens that URI. Declare them in a card's <c>ActionIcons</c>.
/// </summary>
public class CdsCardAction : BindableObject
{
    /// <summary>Bindable property for <see cref="Icon"/>.</summary>
    public static readonly BindableProperty IconProperty = BindableProperty.Create(
        nameof(Icon), typeof(CdsIconName?), typeof(CdsCardAction), null);

    /// <summary>Bindable property for <see cref="IconDescription"/>.</summary>
    public static readonly BindableProperty IconDescriptionProperty = BindableProperty.Create(
        nameof(IconDescription), typeof(string), typeof(CdsCardAction), null);

    /// <summary>Bindable property for <see cref="Href"/>.</summary>
    public static readonly BindableProperty HrefProperty = BindableProperty.Create(
        nameof(Href), typeof(string), typeof(CdsCardAction), null);

    /// <summary>Bindable property for <see cref="Command"/>.</summary>
    public static readonly BindableProperty CommandProperty = BindableProperty.Create(
        nameof(Command), typeof(ICommand), typeof(CdsCardAction), null);

    /// <summary>Bindable property for <see cref="CommandParameter"/>.</summary>
    public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(
        nameof(CommandParameter), typeof(object), typeof(CdsCardAction), null);

    /// <summary>Creates an empty action.</summary>
    public CdsCardAction()
    {
    }

    /// <summary>Creates an action with an icon and its accessible name.</summary>
    public CdsCardAction(CdsIconName icon, string iconDescription)
    {
        Icon = icon;
        IconDescription = iconDescription;
    }

    /// <summary>Raised on activation, before <see cref="Command"/> runs.</summary>
    public event EventHandler? Clicked;

    /// <summary>The 16px icon (<c>Edit</c>, <c>TrashCan</c>, <c>ArrowRight</c>).</summary>
    public CdsIconName? Icon
    {
        get => (CdsIconName?)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>Accessible name and tooltip of the icon button, naming the action ("Edit", "Delete"). Required.</summary>
    public string? IconDescription
    {
        get => (string?)GetValue(IconDescriptionProperty);
        set => SetValue(IconDescriptionProperty, value);
    }

    /// <summary>An absolute URI opened on activation: the action becomes a link.</summary>
    public string? Href
    {
        get => (string?)GetValue(HrefProperty);
        set => SetValue(HrefProperty, value);
    }

    /// <summary>Executed on activation.</summary>
    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    /// <summary>Parameter for <see cref="Command"/>.</summary>
    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    /// <summary>Raises <see cref="Clicked"/>, runs the command and, when <paramref name="openHref"/>, opens <see cref="Href"/>.</summary>
    internal void Activate(bool openHref)
    {
        Clicked?.Invoke(this, EventArgs.Empty);
        if (Command?.CanExecute(CommandParameter) == true) Command.Execute(CommandParameter);
        if (openHref && !string.IsNullOrEmpty(Href) && Uri.TryCreate(Href, UriKind.Absolute, out var uri))
            _ = Launcher.Default.OpenAsync(uri);
    }
}

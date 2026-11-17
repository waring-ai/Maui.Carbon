using System.Windows.Input;

namespace Maui.Carbon;

public partial class CdsButton
{
    /// <summary>Bindable property for <see cref="Text"/>.</summary>
    public static readonly BindableProperty TextProperty = BindableProperty.Create(
        nameof(Text), typeof(string), typeof(CdsButton), null, propertyChanged: OnLayoutPropertyChanged);

    /// <summary>Bindable property for <see cref="Kind"/>.</summary>
    public static readonly BindableProperty KindProperty = BindableProperty.Create(
        nameof(Kind), typeof(CdsButtonKind), typeof(CdsButton), CdsButtonKind.Primary, propertyChanged: OnLayoutPropertyChanged);

    /// <summary>Bindable property for <see cref="Size"/>.</summary>
    public static readonly BindableProperty SizeProperty = BindableProperty.Create(
        nameof(Size), typeof(CdsSize), typeof(CdsButton), CdsSize.Lg, propertyChanged: OnLayoutPropertyChanged);

    /// <summary>Bindable property for <see cref="IsExpressive"/>.</summary>
    public static readonly BindableProperty IsExpressiveProperty = BindableProperty.Create(
        nameof(IsExpressive), typeof(bool), typeof(CdsButton), false, propertyChanged: OnLayoutPropertyChanged);

    /// <summary>Bindable property for <see cref="Icon"/>.</summary>
    public static readonly BindableProperty IconProperty = BindableProperty.Create(
        nameof(Icon), typeof(CdsIconName?), typeof(CdsButton), null, propertyChanged: OnLayoutPropertyChanged);

    /// <summary>Bindable property for <see cref="IsIconOnly"/>.</summary>
    public static readonly BindableProperty IsIconOnlyProperty = BindableProperty.Create(
        nameof(IsIconOnly), typeof(bool), typeof(CdsButton), false, propertyChanged: OnLayoutPropertyChanged);

    /// <summary>Bindable property for <see cref="IconDescription"/>.</summary>
    public static readonly BindableProperty IconDescriptionProperty = BindableProperty.Create(
        nameof(IconDescription), typeof(string), typeof(CdsButton), null, propertyChanged: OnLayoutPropertyChanged);

    /// <summary>Bindable property for <see cref="IsSelected"/>.</summary>
    public static readonly BindableProperty IsSelectedProperty = BindableProperty.Create(
        nameof(IsSelected), typeof(bool?), typeof(CdsButton), null, BindingMode.TwoWay, propertyChanged: OnLayoutPropertyChanged);

    /// <summary>Bindable property for <see cref="Href"/>.</summary>
    public static readonly BindableProperty HrefProperty = BindableProperty.Create(
        nameof(Href), typeof(string), typeof(CdsButton), null, propertyChanged: OnLayoutPropertyChanged);

    /// <summary>Bindable property for <see cref="Command"/>.</summary>
    public static readonly BindableProperty CommandProperty = BindableProperty.Create(
        nameof(Command), typeof(ICommand), typeof(CdsButton), null, propertyChanged: OnCommandChanged);

    /// <summary>Bindable property for <see cref="CommandParameter"/>.</summary>
    public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(
        nameof(CommandParameter), typeof(object), typeof(CdsButton), null, propertyChanged: OnCommandChanged);

    /// <summary>The label: sentence case, verb + noun ("Create cluster").</summary>
    public string? Text
    {
        get => (string?)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>Visual emphasis. Default <see cref="CdsButtonKind.Primary"/>; one primary per view.</summary>
    public CdsButtonKind Kind
    {
        get => (CdsButtonKind)GetValue(KindProperty);
        set => SetValue(KindProperty, value);
    }

    /// <summary>Height: xs 24, sm 32, md 40, lg 48 (default), xl 64, 2xl 80. Match neighbouring fields.</summary>
    public CdsSize Size
    {
        get => (CdsSize)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    /// <summary>Expressive style: 16px label and 20px icon. With <see cref="CdsSize.Lg"/> it is the "large expressive" button.</summary>
    public bool IsExpressive
    {
        get => (bool)GetValue(IsExpressiveProperty);
        set => SetValue(IsExpressiveProperty, value);
    }

    /// <summary>Icon pinned 16 from the end edge (8 after the label for ghosts). 16px, 20px when expressive.</summary>
    public CdsIconName? Icon
    {
        get => (CdsIconName?)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>Square icon-only button. Requires <see cref="IconDescription"/>. Never for danger kinds.</summary>
    public bool IsIconOnly
    {
        get => (bool)GetValue(IsIconOnlyProperty);
        set => SetValue(IsIconOnlyProperty, value);
    }

    /// <summary>Accessible name and tooltip of an icon-only button.</summary>
    public string? IconDescription
    {
        get => (string?)GetValue(IconDescriptionProperty);
        set => SetValue(IconDescriptionProperty, value);
    }

    /// <summary>
    /// Ghost icon-only buttons only: <c>true</c>/<c>false</c> makes it a toggle button (selected fill when true);
    /// <c>null</c> (default) is a plain button. Toggled on activation.
    /// </summary>
    public bool? IsSelected
    {
        get => (bool?)GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    /// <summary>Opens this URI on activation (a link styled as a button: downloads, external consoles).</summary>
    public string? Href
    {
        get => (string?)GetValue(HrefProperty);
        set => SetValue(HrefProperty, value);
    }

    /// <summary>Executed on activation; <c>CanExecute</c> drives <c>IsEnabled</c>.</summary>
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

    static void OnLayoutPropertyChanged(BindableObject b, object? o, object? n) => ((CdsButton)b).UpdateLayout();

    static void OnCommandChanged(BindableObject b, object? o, object? n)
    {
        var button = (CdsButton)b;
        button.SetCommand(button.Command, button.CommandParameter);
    }
}

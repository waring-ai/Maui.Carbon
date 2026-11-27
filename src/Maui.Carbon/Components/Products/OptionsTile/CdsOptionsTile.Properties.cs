namespace Maui.Carbon;

public partial class CdsOptionsTile
{
    /// <summary>Bindable property for <see cref="Title"/>.</summary>
    public static readonly BindableProperty TitleProperty = BindableProperty.Create(
        nameof(Title), typeof(string), typeof(CdsOptionsTile), null, propertyChanged: OnLayoutChanged);

    /// <summary>Bindable property for <see cref="Summary"/>.</summary>
    public static readonly BindableProperty SummaryProperty = BindableProperty.Create(
        nameof(Summary), typeof(string), typeof(CdsOptionsTile), null, propertyChanged: OnLayoutChanged);

    /// <summary>Bindable property for <see cref="Size"/>.</summary>
    public static readonly BindableProperty SizeProperty = BindableProperty.Create(
        nameof(Size), typeof(CdsSize), typeof(CdsOptionsTile), CdsSize.Lg, propertyChanged: OnLayoutChanged);

    /// <summary>Bindable property for <see cref="IsToggled"/>.</summary>
    public static readonly BindableProperty IsToggledProperty = BindableProperty.Create(
        nameof(IsToggled), typeof(bool?), typeof(CdsOptionsTile), null, BindingMode.TwoWay, propertyChanged: OnLayoutChanged);

    /// <summary>Bindable property for <see cref="IsOpen"/>.</summary>
    public static readonly BindableProperty IsOpenProperty = BindableProperty.Create(
        nameof(IsOpen), typeof(bool), typeof(CdsOptionsTile), false, BindingMode.TwoWay, propertyChanged: static (b, o, n) => ((CdsOptionsTile)b).OnOpenChanged((bool)o, (bool)n));

    /// <summary>Bindable property for <see cref="IsInvalid"/>.</summary>
    public static readonly BindableProperty IsInvalidProperty = BindableProperty.Create(
        nameof(IsInvalid), typeof(bool), typeof(CdsOptionsTile), false, propertyChanged: OnLayoutChanged);

    /// <summary>Bindable property for <see cref="InvalidText"/>.</summary>
    public static readonly BindableProperty InvalidTextProperty = BindableProperty.Create(
        nameof(InvalidText), typeof(string), typeof(CdsOptionsTile), null, propertyChanged: OnLayoutChanged);

    /// <summary>Bindable property for <see cref="IsWarning"/>.</summary>
    public static readonly BindableProperty IsWarningProperty = BindableProperty.Create(
        nameof(IsWarning), typeof(bool), typeof(CdsOptionsTile), false, propertyChanged: OnLayoutChanged);

    /// <summary>Bindable property for <see cref="WarnText"/>.</summary>
    public static readonly BindableProperty WarnTextProperty = BindableProperty.Create(
        nameof(WarnText), typeof(string), typeof(CdsOptionsTile), null, propertyChanged: OnLayoutChanged);

    /// <summary>Bindable property for <see cref="IsLocked"/>.</summary>
    public static readonly BindableProperty IsLockedProperty = BindableProperty.Create(
        nameof(IsLocked), typeof(bool), typeof(CdsOptionsTile), false, propertyChanged: OnLayoutChanged);

    /// <summary>Bindable property for <see cref="LockedText"/>.</summary>
    public static readonly BindableProperty LockedTextProperty = BindableProperty.Create(
        nameof(LockedText), typeof(string), typeof(CdsOptionsTile), null, propertyChanged: OnLayoutChanged);

    /// <summary>The title (<c>heading-compact-01</c>, <c>text-primary</c>); it also names the header and the toggle. Required, never interactive.</summary>
    public string? Title
    {
        get => (string?)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>A short summary of the current settings (<c>helper-text-01</c>, <c>text-secondary</c>, one line with an ellipsis), such as "English | Locale: English". Hidden while the tile is open.</summary>
    public string? Summary
    {
        get => (string?)GetValue(SummaryProperty);
        set => SetValue(SummaryProperty, value);
    }

    /// <summary>Header height: <see cref="CdsSize.Lg"/> 48 (default) or <see cref="CdsSize.Xl"/> 64; other sizes read as the nearest.</summary>
    public CdsSize Size
    {
        get => (CdsSize)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    /// <summary>The group toggle (Carbon's <c>enabled</c>): <c>null</c> (default) shows no toggle; <c>true</c>/<c>false</c> shows a small toggle at the end in that state. Two-way.</summary>
    public bool? IsToggled
    {
        get => (bool?)GetValue(IsToggledProperty);
        set => SetValue(IsToggledProperty, value);
    }

    /// <summary>Whether the tile is open (two-way). Only a tile with a body opens.</summary>
    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    /// <summary>Invalid state: <c>WarningFilled</c> and <see cref="InvalidText"/> in <c>support-error</c> replace the summary.</summary>
    public bool IsInvalid
    {
        get => (bool)GetValue(IsInvalidProperty);
        set => SetValue(IsInvalidProperty, value);
    }

    /// <summary>Why the settings are invalid ("Your system does not support this configuration").</summary>
    public string? InvalidText
    {
        get => (string?)GetValue(InvalidTextProperty);
        set => SetValue(InvalidTextProperty, value);
    }

    /// <summary>Warning state (Carbon's <c>warn</c>): <c>WarningAltFilled</c> and <see cref="WarnText"/> replace the summary.</summary>
    public bool IsWarning
    {
        get => (bool)GetValue(IsWarningProperty);
        set => SetValue(IsWarningProperty, value);
    }

    /// <summary>The warning ("A restart is required to apply these settings").</summary>
    public string? WarnText
    {
        get => (string?)GetValue(WarnTextProperty);
        set => SetValue(WarnTextProperty, value);
    }

    /// <summary>Locked state: the settings are managed elsewhere. The toggle is disabled and <see cref="LockedText"/> shows (in the summary of a static tile, above the content of an open one).</summary>
    public bool IsLocked
    {
        get => (bool)GetValue(IsLockedProperty);
        set => SetValue(IsLockedProperty, value);
    }

    /// <summary>Why the tile is locked ("This option is managed by your administrator").</summary>
    public string? LockedText
    {
        get => (string?)GetValue(LockedTextProperty);
        set => SetValue(LockedTextProperty, value);
    }

    static void OnLayoutChanged(BindableObject b, object? o, object? n) => ((CdsOptionsTile)b).UpdateChrome();
}

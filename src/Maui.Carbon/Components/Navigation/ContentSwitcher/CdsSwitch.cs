namespace Maui.Carbon;

/// <summary>
/// One segment of a <see cref="CdsContentSwitcher"/>. Labels are two or three word nouns, not verbs (they must not read as
/// buttons). Long labels truncate and keep the full text as a tooltip. In an icon-only switcher the
/// <see cref="Text"/> is the accessible name and tooltip and <see cref="Icon"/> is drawn.
/// </summary>
[ContentProperty(nameof(Text))]
public class CdsSwitch : Element
{
    /// <summary>Bindable property for <see cref="Text"/>.</summary>
    public static readonly BindableProperty TextProperty = BindableProperty.Create(
        nameof(Text), typeof(string), typeof(CdsSwitch), null);

    /// <summary>Bindable property for <see cref="Name"/>.</summary>
    public static readonly BindableProperty NameProperty = BindableProperty.Create(
        nameof(Name), typeof(string), typeof(CdsSwitch), null);

    /// <summary>Bindable property for <see cref="Icon"/>.</summary>
    public static readonly BindableProperty IconProperty = BindableProperty.Create(
        nameof(Icon), typeof(CdsIconName?), typeof(CdsSwitch), null);

    /// <summary>Bindable property for <see cref="IsEnabled"/>.</summary>
    public static readonly BindableProperty IsEnabledProperty = BindableProperty.Create(
        nameof(IsEnabled), typeof(bool), typeof(CdsSwitch), true);

    /// <summary>The label (and the accessible name and tooltip of an icon-only segment).</summary>
    public string? Text
    {
        get => (string?)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>A value returned in <see cref="CdsContentSwitcher.SelectionChanged"/> (Carbon's <c>name</c>).</summary>
    public string? Name
    {
        get => (string?)GetValue(NameProperty);
        set => SetValue(NameProperty, value);
    }

    /// <summary>The 16 icon of an icon-only switcher.</summary>
    public CdsIconName? Icon
    {
        get => (CdsIconName?)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>Disabled segments cannot be selected and are skipped by the arrow keys. Default <c>true</c>.</summary>
    public bool IsEnabled
    {
        get => (bool)GetValue(IsEnabledProperty);
        set => SetValue(IsEnabledProperty, value);
    }
}

/// <summary>Arguments of <see cref="CdsContentSwitcher.SelectionChanged"/>.</summary>
public sealed class CdsSwitchSelectionChangedEventArgs(int index, string? name, string? text) : EventArgs
{
    /// <summary>The newly selected index.</summary>
    public int Index { get; } = index;

    /// <summary>The <see cref="CdsSwitch.Name"/> of the selected segment.</summary>
    public string? Name { get; } = name;

    /// <summary>The <see cref="CdsSwitch.Text"/> of the selected segment.</summary>
    public string? Text { get; } = text;
}

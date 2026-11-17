namespace Maui.Carbon;

/// <summary>
/// One tab of a <see cref="CdsTabs"/>: a label (one or two words, sentence case) with an optional icon and secondary
/// label, and the <see cref="Body"/> that is shown below the tab row while the tab is selected. Disable a tab with
/// <see cref="IsEnabled"/>. Other tabs' bodies are not in the visual tree (they are kept alive, not shown).
/// </summary>
[ContentProperty(nameof(Body))]
public class CdsTab : Element
{
    /// <summary>Bindable property for <see cref="Text"/>.</summary>
    public static readonly BindableProperty TextProperty = BindableProperty.Create(
        nameof(Text), typeof(string), typeof(CdsTab), null);

    /// <summary>Bindable property for <see cref="SecondaryLabel"/>.</summary>
    public static readonly BindableProperty SecondaryLabelProperty = BindableProperty.Create(
        nameof(SecondaryLabel), typeof(string), typeof(CdsTab), null);

    /// <summary>Bindable property for <see cref="Icon"/>.</summary>
    public static readonly BindableProperty IconProperty = BindableProperty.Create(
        nameof(Icon), typeof(CdsIconName?), typeof(CdsTab), null);

    /// <summary>Bindable property for <see cref="IsEnabled"/>.</summary>
    public static readonly BindableProperty IsEnabledProperty = BindableProperty.Create(
        nameof(IsEnabled), typeof(bool), typeof(CdsTab), true);

    /// <summary>Bindable property for <see cref="Body"/>.</summary>
    public static readonly BindableProperty BodyProperty = BindableProperty.Create(
        nameof(Body), typeof(View), typeof(CdsTab), null);

    /// <summary>The tab label. For icon-only tabs it is the accessible name and tooltip.</summary>
    public string? Text
    {
        get => (string?)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>A second line under the label. Only contained tabs show it (and become 64 high).</summary>
    public string? SecondaryLabel
    {
        get => (string?)GetValue(SecondaryLabelProperty);
        set => SetValue(SecondaryLabelProperty, value);
    }

    /// <summary>A 16 icon after the label, or the whole tab when <see cref="CdsTabs.IsIconOnly"/>. Do not mix tabs with and without icons.</summary>
    public CdsIconName? Icon
    {
        get => (CdsIconName?)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>Disabled tabs are skipped by the arrow keys and cannot be selected. Default <c>true</c>.</summary>
    public bool IsEnabled
    {
        get => (bool)GetValue(IsEnabledProperty);
        set => SetValue(IsEnabledProperty, value);
    }

    /// <summary>The content shown while this tab is selected.</summary>
    public View? Body
    {
        get => (View?)GetValue(BodyProperty);
        set => SetValue(BodyProperty, value);
    }
}

/// <summary>Arguments of <see cref="CdsTabs.SelectionChanged"/>.</summary>
public sealed class CdsTabsSelectionChangedEventArgs(int oldIndex, int newIndex) : EventArgs
{
    /// <summary>The previously selected index (-1 for none).</summary>
    public int OldIndex { get; } = oldIndex;

    /// <summary>The newly selected index (-1 for none).</summary>
    public int NewIndex { get; } = newIndex;
}

/// <summary>Arguments of <see cref="CdsTabs.TabCloseRequested"/>.</summary>
public sealed class CdsTabCloseRequestedEventArgs(int index, CdsTab tab) : EventArgs
{
    /// <summary>The index of the tab whose close icon was used.</summary>
    public int Index { get; } = index;

    /// <summary>The tab. Remove it from <see cref="CdsTabs.Items"/> to close it.</summary>
    public CdsTab Tab { get; } = tab;
}

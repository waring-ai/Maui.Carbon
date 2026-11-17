namespace Maui.Carbon;

/// <summary>
/// A region of a page that is one heading level deeper than its parent: every <see cref="CdsHeading"/> inside (and every
/// nested section) reads the level from the <see cref="CdsContextKeys.HeadingLevel"/> context. Sections nest up to level 6.
/// Wrap reusable components in a section so their headings stay correct wherever the component is placed. Set
/// <see cref="Level"/> to force a level instead of parent plus one.
/// </summary>
[ContentProperty(nameof(Body))]
public class CdsSection : ContentView
{
    /// <summary>Bindable property for <see cref="Level"/>.</summary>
    public static readonly BindableProperty LevelProperty = BindableProperty.Create(
        nameof(Level), typeof(int?), typeof(CdsSection), null,
        propertyChanged: static (b, _, _) => ((CdsSection)b).Recompute());

    /// <summary>Bindable property for <see cref="Body"/>.</summary>
    public static readonly BindableProperty BodyProperty = BindableProperty.Create(
        nameof(Body), typeof(View), typeof(CdsSection), null,
        propertyChanged: static (b, _, n) => ((CdsSection)b)._scope.Content = (View?)n);

    // Read from ancestors. The section itself never holds this key: the override lives on _scope.
    static readonly BindableProperty ParentLevelProperty = BindableProperty.Create(
        "ParentLevel", typeof(int), typeof(CdsSection), 1,
        propertyChanged: static (b, _, _) => ((CdsSection)b).Recompute());

    readonly CdsScopeView _scope = new();
    int _applied;

    /// <summary>Creates a section one level below its context.</summary>
    public CdsSection()
    {
        Content = _scope;
        SetDynamicResource(ParentLevelProperty, CdsContextKeys.HeadingLevel);
        Recompute();
    }

    /// <summary>Forces the heading level (1 to 6) of this subtree instead of the parent's level plus one.</summary>
    public int? Level
    {
        get => (int?)GetValue(LevelProperty);
        set => SetValue(LevelProperty, value);
    }

    /// <summary>The section's content.</summary>
    public View? Body
    {
        get => (View?)GetValue(BodyProperty);
        set => SetValue(BodyProperty, value);
    }

    /// <summary>The level the headings directly inside this section have.</summary>
    public int EffectiveLevel => CdsSectionModel.LevelFor((int)GetValue(ParentLevelProperty), Level);

    void Recompute()
    {
        var level = EffectiveLevel;
        if (_applied == level) return;
        _applied = level;
        _scope.Resources = new ResourceDictionary { [CdsContextKeys.HeadingLevel] = level };
    }
}

/// <summary>Pure heading-level arithmetic for <see cref="CdsSection"/>.</summary>
public static class CdsSectionModel
{
    /// <summary>The level inside a section: the forced level (clamped to 1 to 6), else the parent's plus one capped at 6.</summary>
    public static int LevelFor(int parentLevel, int? forced) =>
        forced is { } f
            ? Math.Clamp(f, 1, CdsHeadingStyles.MaxLevel)
            : Math.Clamp(parentLevel + 1, 1, CdsHeadingStyles.MaxLevel);
}

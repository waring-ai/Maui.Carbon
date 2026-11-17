namespace Maui.Carbon;

/// <summary>Maps a heading level to the Carbon productive heading style (README: heading-05 for page titles down to heading-compact-01).</summary>
public static class CdsHeadingStyles
{
    /// <summary>The deepest heading level.</summary>
    public const int MaxLevel = 6;

    /// <summary>
    /// The type style for a heading level: 1 is <c>heading-05</c> (page title, 32), 2 <c>heading-04</c> (28),
    /// 3 <c>heading-03</c> (20), 4 <c>heading-02</c> (16 semibold), 5 <c>heading-01</c> (14 semibold) and
    /// 6 <c>heading-compact-01</c>. Levels outside 1 to 6 are clamped.
    /// </summary>
    public static CdsTypeStyle ForLevel(int level) => Math.Clamp(level, 1, MaxLevel) switch
    {
        1 => CdsTypeStyle.Heading05,
        2 => CdsTypeStyle.Heading04,
        3 => CdsTypeStyle.Heading03,
        4 => CdsTypeStyle.Heading02,
        5 => CdsTypeStyle.Heading01,
        _ => CdsTypeStyle.HeadingCompact01,
    };

    /// <summary>The screen reader heading level for a level, clamped to 1 to 6.</summary>
    public static SemanticHeadingLevel ToSemantic(int level) => Math.Clamp(level, 1, MaxLevel) switch
    {
        1 => SemanticHeadingLevel.Level1,
        2 => SemanticHeadingLevel.Level2,
        3 => SemanticHeadingLevel.Level3,
        4 => SemanticHeadingLevel.Level4,
        5 => SemanticHeadingLevel.Level5,
        _ => SemanticHeadingLevel.Level6,
    };
}

/// <summary>
/// A heading that knows its place in the document: outside any <see cref="CdsSection"/> it is level 1, and every
/// enclosing section adds one (up to 6). It picks the Carbon productive heading style for its level
/// (<see cref="CdsHeadingStyles.ForLevel"/>), uses <c>text-primary</c>, and reports its level to screen readers
/// (<c>SemanticProperties.HeadingLevel</c>), so reusable components keep a correct outline wherever they are placed.
/// Set <see cref="CdsText.TypeStyle"/> to change the look without changing the structure; do not skip levels for size.
/// </summary>
public class CdsHeading : CdsText
{
    static readonly BindableProperty ContextLevelProperty = BindableProperty.Create(
        "ContextLevel", typeof(int), typeof(CdsHeading), 1,
        propertyChanged: static (b, _, _) => ((CdsHeading)b).ApplyLevel());

    bool _applying;
    bool _explicitStyle;

    /// <summary>Creates a heading in its context's level.</summary>
    public CdsHeading()
    {
        SetDynamicResource(ContextLevelProperty, CdsContextKeys.HeadingLevel);
        ApplyLevel();
    }

    /// <summary>Creates a heading with text.</summary>
    public CdsHeading(string text) : this()
    {
        Text = text;
    }

    /// <summary>The heading level (1 to 6) in effect where the heading sits.</summary>
    public int Level => Math.Clamp((int)GetValue(ContextLevelProperty), 1, CdsHeadingStyles.MaxLevel);

    /// <inheritdoc/>
    protected override void OnPropertyChanged(string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
        if (propertyName == TypeStyleProperty.PropertyName && !_applying) _explicitStyle = true;
    }

    void ApplyLevel()
    {
        SemanticProperties.SetHeadingLevel(this, CdsHeadingStyles.ToSemantic(Level));
        if (_explicitStyle) return;
        _applying = true;
        try
        {
            TypeStyle = CdsHeadingStyles.ForLevel(Level);
        }
        finally
        {
            _applying = false;
        }
    }
}

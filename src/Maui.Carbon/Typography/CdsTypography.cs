namespace Maui.Carbon;

/// <summary>Applies Carbon type styles to MAUI text elements. Colors are not part of a type style.</summary>
public static class CdsTypography
{
    /// <summary>Key of the keyed Label style for a type style (<c>{StaticResource CdsType.BodyCompact01}</c>).</summary>
    public static string StyleKey(CdsTypeStyle style) => "CdsType." + style;

    /// <summary>Applies a type style to a label.</summary>
    public static void Apply(Label label, CdsTypeStyle style)
    {
        var spec = CdsTypeSpecs.Get(style);
        label.FontFamily = spec.FontFamily;
        label.FontSize = spec.FontSize;
        label.CharacterSpacing = spec.CharacterSpacing;
        label.LineHeight = spec.MauiLineHeight;
    }

    /// <summary>Applies a type style to a span.</summary>
    public static void Apply(Span span, CdsTypeStyle style)
    {
        var spec = CdsTypeSpecs.Get(style);
        span.FontFamily = spec.FontFamily;
        span.FontSize = spec.FontSize;
        span.CharacterSpacing = spec.CharacterSpacing;
        span.LineHeight = spec.MauiLineHeight;
    }

    /// <summary>Applies a type style to an entry.</summary>
    public static void Apply(Entry entry, CdsTypeStyle style)
    {
        var spec = CdsTypeSpecs.Get(style);
        entry.FontFamily = spec.FontFamily;
        entry.FontSize = spec.FontSize;
        entry.CharacterSpacing = spec.CharacterSpacing;
    }

    /// <summary>Applies a type style to an editor.</summary>
    public static void Apply(Editor editor, CdsTypeStyle style)
    {
        var spec = CdsTypeSpecs.Get(style);
        editor.FontFamily = spec.FontFamily;
        editor.FontSize = spec.FontSize;
        editor.CharacterSpacing = spec.CharacterSpacing;
    }

    /// <summary>Builds the keyed Label style for a type style.</summary>
    internal static Style CreateLabelStyle(CdsTypeStyle style)
    {
        var spec = CdsTypeSpecs.Get(style);
        var s = new Style(typeof(Label));
        s.Setters.Add(new Setter { Property = Label.FontFamilyProperty, Value = spec.FontFamily });
        s.Setters.Add(new Setter { Property = Label.FontSizeProperty, Value = spec.FontSize });
        s.Setters.Add(new Setter { Property = Label.CharacterSpacingProperty, Value = spec.CharacterSpacing });
        s.Setters.Add(new Setter { Property = Label.LineHeightProperty, Value = spec.MauiLineHeight });
        return s;
    }
}

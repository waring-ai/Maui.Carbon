using System.Globalization;
using System.Security;
using System.Text;

namespace Maui.Carbon.Generator;

/// <summary>Emits the token-derived C# files.</summary>
public static class TokenEmitter
{
    /// <summary>Theme id in tokens.json → CdsThemeName member.</summary>
    public static readonly IReadOnlyDictionary<string, string> ThemeMembers = new Dictionary<string, string>
    {
        ["white"] = "White",
        ["g10"] = "G10",
        ["g90"] = "G90",
        ["g100"] = "G100",
    };

    /// <summary>Returns (file name, body) pairs.</summary>
    public static IEnumerable<(string Name, string Body)> Emit(CarbonTokens tokens)
    {
        yield return ("CdsColorKeys.g.cs", ColorKeys(tokens));
        yield return ("CdsPaletteData.g.cs", Palettes(tokens));
        yield return ("CdsTypeStyle.g.cs", TypeStyles(tokens));
        yield return ("CdsScales.g.cs", Scales(tokens));
    }

    /// <summary>Converts a kebab-case token name to PascalCase (<c>layer-hover-01</c> → <c>LayerHover01</c>).</summary>
    public static string Pascal(string kebab)
    {
        var sb = new StringBuilder();
        foreach (var part in kebab.Split('-', StringSplitOptions.RemoveEmptyEntries))
            sb.Append(char.ToUpperInvariant(part[0])).Append(part.AsSpan(1));
        return sb.ToString();
    }

    /// <summary>The resource key used for a color token (<c>layer-01</c> → <c>CdsLayer01</c>).</summary>
    public static string ColorKey(string token) => "Cds" + Pascal(token);

    static string Doc(string text) => SecurityElement.Escape(text.ReplaceLineEndings(" ")) ?? "";

    static string F(double d) => d.ToString("0.#####", CultureInfo.InvariantCulture);

    static string ColorKeys(CarbonTokens t)
    {
        var sb = new StringBuilder();
        sb.AppendLine("namespace Maui.Carbon;");
        sb.AppendLine();
        sb.AppendLine("/// <summary>");
        sb.AppendLine("/// Resource keys for the Carbon color tokens. Colors change with the theme, so always reference them");
        sb.AppendLine("/// dynamically: <c>{DynamicResource CdsLayer01}</c> in XAML or <c>SetDynamicResource(prop, CdsColorKeys.Layer01)</c> in C#.");
        sb.AppendLine("/// Inside a <see cref=\"CdsLayer\"/> the <c>-01</c> layer keys resolve to the enclosing layer's values.");
        sb.AppendLine("/// </summary>");
        sb.AppendLine("public static partial class CdsColorKeys");
        sb.AppendLine("{");
        var seen = new HashSet<string>();
        foreach (var c in t.Colors)
        {
            var member = Pascal(c.Name);
            if (!seen.Add(member)) throw new InvalidOperationException($"Duplicate color member {member}.");
            sb.AppendLine($"    /// <summary><c>{c.Name}</c>: {Doc(c.Usage)}</summary>");
            sb.AppendLine($"    public const string {member} = \"{ColorKey(c.Name)}\";");
            sb.AppendLine();
        }
        sb.AppendLine("    /// <summary>Every color key, in token order.</summary>");
        sb.AppendLine("    public static IReadOnlyList<string> All { get; } =");
        sb.AppendLine("    [");
        foreach (var c in t.Colors) sb.AppendLine($"        {Pascal(c.Name)},");
        sb.AppendLine("    ];");
        sb.AppendLine();
        sb.AppendLine("    /// <summary>Carbon token name (<c>layer-01</c>) for every key, in token order.</summary>");
        sb.AppendLine("    public static IReadOnlyList<string> TokenNames { get; } =");
        sb.AppendLine("    [");
        foreach (var c in t.Colors) sb.AppendLine($"        \"{c.Name}\",");
        sb.AppendLine("    ];");
        sb.AppendLine("}");
        return sb.ToString();
    }

    static string Palettes(CarbonTokens t)
    {
        var sb = new StringBuilder();
        sb.AppendLine("using Microsoft.Maui.Graphics;");
        sb.AppendLine();
        sb.AppendLine("namespace Maui.Carbon;");
        sb.AppendLine();
        sb.AppendLine("/// <summary>Color values per theme, index-aligned with <see cref=\"CdsColorKeys.All\"/>.</summary>");
        sb.AppendLine("internal static partial class CdsPaletteData");
        sb.AppendLine("{");
        var fallbacks = t.Colors.Where(c => c.FallbackThemes.Count > 0).ToList();
        sb.AppendLine("    /// <summary>Tokens that have no value in some themes and inherit the first theme's value there.</summary>");
        sb.AppendLine("    internal static readonly string[] InheritedTokens =");
        sb.AppendLine("    [");
        foreach (var c in fallbacks) sb.AppendLine($"        \"{c.Name}\", // {string.Join(", ", c.FallbackThemes)}");
        sb.AppendLine("    ];");
        sb.AppendLine();
        sb.AppendLine("    internal static Color[] Get(CdsThemeName theme) => theme switch");
        sb.AppendLine("    {");
        foreach (var th in t.Themes) sb.AppendLine($"        CdsThemeName.{ThemeMembers[th]} => {ThemeMembers[th]},");
        sb.AppendLine("        _ => throw new ArgumentOutOfRangeException(nameof(theme)),");
        sb.AppendLine("    };");
        foreach (var th in t.Themes)
        {
            sb.AppendLine();
            sb.AppendLine($"    static readonly Color[] {ThemeMembers[th]} =");
            sb.AppendLine("    [");
            foreach (var c in t.Colors)
                sb.AppendLine($"        Color.FromUint({c.Values[th].ToUintLiteral()}), // {c.Name}");
            sb.AppendLine("    ];");
        }
        sb.AppendLine("}");
        return sb.ToString();
    }

    /// <summary>Font alias for a family/weight pair; must match <c>CdsFonts</c>.</summary>
    public static string FontAlias(string family, int weight) => (family, weight) switch
    {
        ("sans", 300) => "CdsFonts.SansLight",
        ("sans", 400) => "CdsFonts.Sans",
        ("sans", 600) => "CdsFonts.SansSemiBold",
        ("mono", 400) => "CdsFonts.Mono",
        ("serif", 300) => "CdsFonts.SerifLight",
        ("serif", 400) => "CdsFonts.Serif",
        _ => throw new InvalidOperationException($"No bundled font for {family} {weight}."),
    };

    static string TypeStyles(CarbonTokens t)
    {
        var sb = new StringBuilder();
        sb.AppendLine("namespace Maui.Carbon;");
        sb.AppendLine();
        sb.AppendLine("/// <summary>The Carbon type styles. Apply with <see cref=\"CdsText.TypeStyle\"/> or <see cref=\"CdsTypography.Apply(Label, CdsTypeStyle)\"/>.</summary>");
        sb.AppendLine("public enum CdsTypeStyle");
        sb.AppendLine("{");
        foreach (var s in t.TypeStyles)
        {
            var lh = s.FontSizePx * s.LineHeight;
            sb.AppendLine($"    /// <summary><c>{s.Name}</c> ({s.Group}): {F(s.FontSizePx)}px/{F(Math.Round(lh, 2))}px, weight {s.FontWeight}. {Doc(s.Usage)}</summary>");
            sb.AppendLine($"    {Pascal(s.Name)},");
        }
        sb.AppendLine("}");
        sb.AppendLine();
        sb.AppendLine("internal static partial class CdsTypeSpecs");
        sb.AppendLine("{");
        sb.AppendLine("    internal static CdsTypeSpec Get(CdsTypeStyle style) => style switch");
        sb.AppendLine("    {");
        foreach (var s in t.TypeStyles)
            sb.AppendLine($"        CdsTypeStyle.{Pascal(s.Name)} => new(\"{s.Name}\", {FontAlias(s.Family, s.FontWeight)}, {s.FontWeight}, {F(s.FontSizePx)}, {F(s.LineHeight)}, {F(s.LetterSpacingPx)}),");
        sb.AppendLine("        _ => throw new ArgumentOutOfRangeException(nameof(style)),");
        sb.AppendLine("    };");
        sb.AppendLine("}");
        return sb.ToString();
    }

    static readonly IReadOnlyDictionary<string, string> SizeNames = new Dictionary<string, string>
    {
        ["size-xsmall"] = "XSmall",
        ["size-small"] = "Small",
        ["size-medium"] = "Medium",
        ["size-large"] = "Large",
        ["size-xlarge"] = "XLarge",
        ["size-2xlarge"] = "X2Large",
    };

    static string Scales(CarbonTokens t)
    {
        var sb = new StringBuilder();
        sb.AppendLine("using Microsoft.Maui;");
        sb.AppendLine();
        sb.AppendLine("namespace Maui.Carbon;");
        sb.AppendLine();

        void Class(string name, string summary, string family, Func<ScalarToken, string> member, Func<ScalarToken, string> value, string type)
        {
            sb.AppendLine($"/// <summary>{summary}</summary>");
            sb.AppendLine($"public static partial class {name}");
            sb.AppendLine("{");
            foreach (var s in t.Scalars[family])
            {
                sb.AppendLine($"    /// <summary><c>{s.Name}</c> ({s.Value}): {Doc(s.Usage)}</summary>");
                sb.AppendLine($"    public const {type} {member(s)} = {value(s)};");
            }
            sb.AppendLine("}");
            sb.AppendLine();
        }

        static string Strip(string name, string prefix) => Pascal(name.StartsWith(prefix, StringComparison.Ordinal) ? name[prefix.Length..] : name);

        Class("CdsSpacing", "Carbon spacing scale in device-independent units (rem × 16). Never use off-scale values.",
            "spacing", s => Pascal(s.Name), s => F(CarbonTokens.Length(s.Value)), "double");
        Class("CdsSizing", "Carbon size scale: control heights, containers and icon sizes, in device-independent units.",
            "size", s => SizeNames.TryGetValue(s.Name, out var n) ? n : Pascal(s.Name), s => F(CarbonTokens.Length(s.Value)), "double");

        // radius-circle is 50%: not a constant; CdsRadius.Circle(height) is hand-written.
        sb.AppendLine("/// <summary>Carbon corner radii. The system is square; radius is only for tags, toggles and checkboxes.</summary>");
        sb.AppendLine("public static partial class CdsRadius");
        sb.AppendLine("{");
        foreach (var s in t.Scalars["radius"].Where(s => !s.Value.EndsWith('%')))
        {
            sb.AppendLine($"    /// <summary><c>{s.Name}</c> ({s.Value}): {Doc(s.Usage)}</summary>");
            sb.AppendLine($"    public const double {Strip(s.Name, "radius-")} = {F(CarbonTokens.Length(s.Value))};");
        }
        sb.AppendLine("}");
        sb.AppendLine();

        Class("CdsBreakpoints", "Carbon 2x grid breakpoints (minimum widths) in device-independent units.",
            "breakpoint", s => Strip(s.Name, "breakpoint-"), s => F(CarbonTokens.Length(s.Value)), "double");
        Class("CdsFluidSpacing", "Fluid spacing between expressive page sections, as a fraction of the viewport width.",
            "fluidSpacing", s => Strip(s.Name, "fluid-"), s => F(CarbonTokens.Length(s.Value)), "double");
        Class("CdsDuration", "Carbon motion durations in milliseconds.",
            "duration", s => Strip(s.Name, "duration-"), s => ((uint)CarbonTokens.Length(s.Value)).ToString(CultureInfo.InvariantCulture), "uint");

        sb.AppendLine("/// <summary>Carbon easing curves (productive for task UI, expressive for significant moments).</summary>");
        sb.AppendLine("public static partial class CdsEasing");
        sb.AppendLine("{");
        foreach (var s in t.Scalars["ease"])
        {
            var args = s.Value[(s.Value.IndexOf('(') + 1)..s.Value.LastIndexOf(')')]
                .Split(',', StringSplitOptions.TrimEntries)
                .Select(a => F(double.Parse(a, CultureInfo.InvariantCulture)));
            sb.AppendLine($"    /// <summary><c>{s.Name}</c> ({s.Value}): {Doc(s.Usage)}</summary>");
            sb.AppendLine($"    public static readonly Easing {Strip(s.Name, "ease-")} = CdsCubicBezier.ToEasing({string.Join(", ", args)});");
        }
        sb.AppendLine("}");
        return sb.ToString();
    }
}

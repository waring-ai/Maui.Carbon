using System.Globalization;
using System.Text.Json;

namespace Maui.Carbon.Generator;

/// <summary>A parsed ARGB color.</summary>
public readonly record struct Argb(byte A, byte R, byte G, byte B)
{
    /// <summary>The value as a <c>0xAARRGGBB</c> literal.</summary>
    public string ToUintLiteral() => $"0x{A:X2}{R:X2}{G:X2}{B:X2}";

    /// <summary>Parses <c>#rgb</c>, <c>#rrggbb</c>, <c>#rrggbbaa</c>, <c>rgb()</c> and <c>rgba()</c>.</summary>
    public static Argb Parse(string value)
    {
        var s = value.Trim();
        if (s.StartsWith('#'))
        {
            var hex = s[1..];
            if (hex.Length is 3 or 4) hex = string.Concat(hex.Select(c => $"{c}{c}"));
            byte At(int i) => byte.Parse(hex.AsSpan(i, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            return hex.Length switch
            {
                6 => new Argb(255, At(0), At(2), At(4)),
                8 => new Argb(At(6), At(0), At(2), At(4)),
                _ => throw new FormatException($"Unsupported hex color '{value}'."),
            };
        }
        if (s.StartsWith("rgb", StringComparison.OrdinalIgnoreCase))
        {
            var open = s.IndexOf('(');
            var parts = s[(open + 1)..s.LastIndexOf(')')].Split(',', StringSplitOptions.TrimEntries);
            byte Channel(string p) => (byte)Math.Clamp((int)Math.Round(double.Parse(p, CultureInfo.InvariantCulture)), 0, 255);
            var alpha = parts.Length > 3 ? double.Parse(parts[3], CultureInfo.InvariantCulture) : 1.0;
            return new Argb((byte)Math.Clamp((int)Math.Round(alpha * 255), 0, 255), Channel(parts[0]), Channel(parts[1]), Channel(parts[2]));
        }
        throw new FormatException($"Unsupported color '{value}'.");
    }
}

/// <summary>One color token with a resolved value per theme.</summary>
public sealed record ColorToken(string Name, string Usage, IReadOnlyDictionary<string, Argb> Values, IReadOnlyList<string> FallbackThemes);

/// <summary>A scalar token (spacing, size, radius, breakpoint, duration, easing...).</summary>
public sealed record ScalarToken(string Name, string Value, string Usage);

/// <summary>One type style.</summary>
public sealed record TypeStyleToken(string Name, string Group, string Family, double FontSizePx, double LineHeight, int FontWeight, double LetterSpacingPx, string Usage);

/// <summary>The subset of design/carbon/tokens.json the generator understands.</summary>
public sealed class CarbonTokens
{
    /// <summary>Theme ids in file order; the first is the primary theme.</summary>
    public required IReadOnlyList<string> Themes { get; init; }
    /// <summary>Color tokens in file order.</summary>
    public required IReadOnlyList<ColorToken> Colors { get; init; }
    /// <summary>Type styles in file order.</summary>
    public required IReadOnlyList<TypeStyleToken> TypeStyles { get; init; }
    /// <summary>Scalar families keyed by family name (spacing, size, radius, breakpoint, fluidSpacing, duration, ease).</summary>
    public required IReadOnlyDictionary<string, IReadOnlyList<ScalarToken>> Scalars { get; init; }

    /// <summary>Loads and validates tokens.json.</summary>
    public static CarbonTokens Load(string path)
    {
        using var doc = JsonDocument.Parse(File.ReadAllBytes(path));
        var root = doc.RootElement;
        var color = root.GetProperty("color");
        var themes = color.GetProperty("themes").EnumerateArray().Select(t => t.GetProperty("id").GetString()!).ToList();

        var colors = new List<ColorToken>();
        foreach (var t in color.GetProperty("tokens").EnumerateArray())
        {
            var name = t.GetProperty("name").GetString()!;
            var usage = t.TryGetProperty("usage", out var u) ? u.GetString() ?? "" : "";
            var raw = new Dictionary<string, string>();
            var v = t.GetProperty("value");
            if (v.ValueKind == JsonValueKind.String)
                foreach (var th in themes) raw[th] = v.GetString()!;
            else
                foreach (var p in v.EnumerateObject()) raw[p.Name] = p.Value.GetString()!;

            var values = new Dictionary<string, Argb>();
            var fallbacks = new List<string>();
            foreach (var th in themes)
            {
                if (!raw.TryGetValue(th, out var s))
                {
                    // Format rule: a theme without a value inherits the first theme's value.
                    s = raw[themes[0]];
                    fallbacks.Add(th);
                }
                if (s.StartsWith('{')) throw new FormatException($"Alias values are not supported yet ({name}).");
                values[th] = Argb.Parse(s);
            }
            colors.Add(new ColorToken(name, usage, values, fallbacks));
        }

        var type = root.GetProperty("type");
        var styles = new List<TypeStyleToken>();
        foreach (var g in type.GetProperty("groups").EnumerateArray())
        {
            var group = g.GetProperty("name").GetString()!;
            var family = g.GetProperty("family").GetString()!;
            foreach (var s in g.GetProperty("styles").EnumerateArray())
            {
                styles.Add(new TypeStyleToken(
                    s.GetProperty("name").GetString()!,
                    group,
                    s.TryGetProperty("family", out var f) ? f.GetString()! : family,
                    Length(s.GetProperty("fontSize")),
                    s.GetProperty("lineHeight").GetDouble(),
                    s.GetProperty("fontWeight").GetInt32(),
                    s.TryGetProperty("letterSpacing", out var ls) ? Length(ls) : 0,
                    s.TryGetProperty("usage", out var su) ? su.GetString() ?? "" : ""));
            }
        }

        var scalars = new Dictionary<string, IReadOnlyList<ScalarToken>>();
        foreach (var family in new[] { "spacing", "size", "radius", "breakpoint", "fluidSpacing", "duration", "ease" })
        {
            scalars[family] = root.GetProperty(family).GetProperty("tokens").EnumerateArray()
                .Select(t => new ScalarToken(
                    t.GetProperty("name").GetString()!,
                    t.GetProperty("value").ValueKind == JsonValueKind.String ? t.GetProperty("value").GetString()! : t.GetProperty("value").GetRawText(),
                    t.TryGetProperty("usage", out var u) ? u.GetString() ?? "" : ""))
                .ToList();
        }

        return new CarbonTokens { Themes = themes, Colors = colors, TypeStyles = styles, Scalars = scalars };
    }

    /// <summary>Converts a CSS length (<c>rem</c>, <c>px</c> or a bare number) to px at a 16px root.</summary>
    public static double Length(JsonElement e) => e.ValueKind == JsonValueKind.Number ? e.GetDouble() : Length(e.GetString()!);

    /// <inheritdoc cref="Length(JsonElement)"/>
    public static double Length(string s)
    {
        s = s.Trim();
        if (s.EndsWith("rem", StringComparison.Ordinal)) return double.Parse(s[..^3], CultureInfo.InvariantCulture) * 16;
        if (s.EndsWith("px", StringComparison.Ordinal)) return double.Parse(s[..^2], CultureInfo.InvariantCulture);
        if (s.EndsWith("ms", StringComparison.Ordinal)) return double.Parse(s[..^2], CultureInfo.InvariantCulture);
        if (s.EndsWith("vw", StringComparison.Ordinal)) return double.Parse(s[..^2], CultureInfo.InvariantCulture) / 100;
        if (s.EndsWith('%')) return double.Parse(s[..^1], CultureInfo.InvariantCulture) / 100;
        return double.Parse(s, CultureInfo.InvariantCulture);
    }
}

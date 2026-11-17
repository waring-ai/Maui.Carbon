namespace Maui.Carbon;

/// <summary>What the text area's counter counts.</summary>
public enum CdsCounterMode
{
    /// <summary>Characters (default). The native editor stops at the limit.</summary>
    Character,

    /// <summary>Words: runs of non-whitespace. Words past the limit are cut off.</summary>
    Word,
}

/// <summary>Pure counting and limiting logic of the text area.</summary>
internal static class TextAreaModel
{
    /// <summary>Counts characters or words.</summary>
    public static int Count(string? text, CdsCounterMode mode)
    {
        if (string.IsNullOrEmpty(text)) return 0;
        if (mode == CdsCounterMode.Character) return text.Length;
        var words = 0;
        var inWord = false;
        foreach (var c in text)
        {
            if (char.IsWhiteSpace(c)) inWord = false;
            else if (!inWord)
            {
                inWord = true;
                words++;
            }
        }
        return words;
    }

    /// <summary>"n/max", or <c>null</c> when there is no limit.</summary>
    public static string? CounterText(string? text, CdsCounterMode mode, int max) =>
        max > 0 ? $"{Count(text, mode)}/{max}" : null;

    /// <summary>Cuts <paramref name="text"/> to at most <paramref name="max"/> characters or words (<paramref name="max"/> &lt;= 0 = no limit).</summary>
    public static string? Limit(string? text, CdsCounterMode mode, int max)
    {
        if (text is null || max <= 0) return text;
        if (mode == CdsCounterMode.Character) return text.Length <= max ? text : text[..max];
        var words = 0;
        var inWord = false;
        for (var i = 0; i < text.Length; i++)
        {
            if (char.IsWhiteSpace(text[i]))
            {
                inWord = false;
                continue;
            }
            if (inWord) continue;
            inWord = true;
            if (++words > max) return text[..i].TrimEnd(); // drop the extra word and the gap before it
        }
        return text;
    }

    /// <summary>The note screen readers hear: "Character limit 300" or "Word limit 50".</summary>
    public static string? LimitNote(CdsCounterMode mode, int max) =>
        max > 0 ? (mode == CdsCounterMode.Word ? $"Word limit {max}" : $"Character limit {max}") : null;

    /// <summary>The editor height in device units for a number of rows of <paramref name="lineHeight"/>.</summary>
    public static double Height(int rows, double lineHeight) => Math.Max(1, rows) * lineHeight;
}

using Maui.Carbon.Relay.Models;

namespace Maui.Carbon.Relay.Logic;

/// <summary>What the projects screen filters by. Empty <see cref="Categories"/> means every category.</summary>
public sealed record ProjectFilter
{
    public string SearchText { get; init; } = "";

    public IReadOnlySet<ProjectCategory> Categories { get; init; } = new HashSet<ProjectCategory>();

    public bool StarredOnly { get; init; }

    public bool ShowArchived { get; init; }
}

/// <summary>Filtering for the projects grid and the rules for a new project's key.</summary>
public static class ProjectsQuery
{
    /// <summary>The projects that match, starred first, then by name. Archived projects show only when asked for.</summary>
    public static IEnumerable<Project> Filter(IEnumerable<Project> projects, ProjectFilter filter)
    {
        var text = filter.SearchText.Trim();
        return projects
            .Where(p => filter.ShowArchived || !p.IsArchived)
            .Where(p => !filter.StarredOnly || p.IsStarred)
            .Where(p => filter.Categories.Count == 0 || filter.Categories.Contains(p.Category))
            .Where(p => text.Length == 0
                || p.Name.Contains(text, StringComparison.OrdinalIgnoreCase)
                || p.Key.Contains(text, StringComparison.OrdinalIgnoreCase)
                || p.Description.Contains(text, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(p => p.IsStarred)
            .ThenBy(p => p.IsArchived)
            .ThenBy(p => p.Name, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>True when <paramref name="key"/> has the shape of a project key: 2 to 5 capital letters A–Z.</summary>
    public static bool IsWellFormedKey(string key) => key.Length is >= 2 and <= 5 && key.All(char.IsAsciiLetterUpper);

    /// <summary>
    /// A key suggested from a project name: the initials of a multi-word name ("Mobile release train" → "MRT"), or the
    /// first three letters of a single word ("Payments" → "PAY"). When that key is taken, more letters are added (up to
    /// five) until <paramref name="isAvailable"/> accepts one. Empty when the name has too few letters.
    /// </summary>
    public static string SuggestKey(string name, Func<string, bool> isAvailable)
    {
        var words = name.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(w => new string(w.Where(char.IsAsciiLetter).ToArray()).ToUpperInvariant())
            .Where(w => w.Length > 0)
            .ToList();
        if (words.Count == 0) return "";

        var letters = string.Concat(words);
        if (letters.Length < 2) return "";

        var initials = string.Concat(words.Select(w => w[0]));
        var candidates = new List<string>();
        if (words.Count > 1)
        {
            var baseKey = initials.Length > 5 ? initials[..5] : initials;
            if (baseKey.Length >= 2) candidates.Add(baseKey);
            // Initials plus the next letters of the last word ("AM" → "AMO", "AMOB").
            var last = words[^1];
            for (var extra = 1; extra < last.Length && baseKey.Length + extra <= 5; extra++)
                candidates.Add(baseKey + last[1..(1 + extra)]);
        }
        for (var length = 3; length <= Math.Min(5, letters.Length); length++) candidates.Add(letters[..length]);
        if (letters.Length == 2) candidates.Add(letters);

        foreach (var candidate in candidates.Distinct())
            if (IsWellFormedKey(candidate) && isAvailable(candidate)) return candidate;
        return candidates.FirstOrDefault(IsWellFormedKey) ?? "";
    }
}

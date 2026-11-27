namespace Maui.Carbon;

/// <summary>Pure logic of <see cref="CdsUserAvatar"/>: initials and content precedence.</summary>
internal static class UserAvatarModel
{
    /// <summary>
    /// Initials as Carbon for IBM Products forms them: one word gives its first two letters ("Thomas" → "TH", "TW" →
    /// "TW"); several give the first letter of the first and of the last word ("thomas j. watson" → "TW"). Upper case.
    /// Runs of spaces are ignored. <c>null</c> for a name with no letters to show.
    /// </summary>
    public static string? Initials(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return null;
        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 0) return null;
        var first = parts[0];
        if (parts.Length == 1) return first[..Math.Min(2, first.Length)].ToUpperInvariant();
        return string.Concat(char.ToUpperInvariant(first[0]), char.ToUpperInvariant(parts[^1][0]));
    }

    /// <summary>What to draw: the image, else the icon, else initials, else the default <c>user</c> icon.</summary>
    public static CdsUserAvatarContent Content(bool hasImage, bool hasIcon, string? name) =>
        hasImage ? CdsUserAvatarContent.Image
        : hasIcon ? CdsUserAvatarContent.Icon
        : Initials(name) is not null ? CdsUserAvatarContent.Initials
        : CdsUserAvatarContent.DefaultIcon;
}

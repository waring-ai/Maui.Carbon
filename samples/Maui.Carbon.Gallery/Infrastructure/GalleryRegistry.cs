using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Maui.Carbon.Gallery.Infrastructure;

/// <summary>A discovered gallery page.</summary>
public sealed record GalleryEntry(GalleryPageAttribute Info, [property: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type PageType);

/// <summary>Finds every <see cref="GalleryPageAttribute"/> page in this assembly.</summary>
public static class GalleryRegistry
{
    static IReadOnlyList<GalleryEntry>? s_entries;

    public static IReadOnlyList<GalleryEntry> Entries => s_entries ??= Discover();

    public static GalleryPageAttribute? InfoFor(Type pageType) => pageType.GetCustomAttribute<GalleryPageAttribute>();

    [UnconditionalSuppressMessage("Trimming", "IL2072", Justification = "The gallery assembly is not trimmed (TrimMode=partial).")]
    static IReadOnlyList<GalleryEntry> Discover() =>
        typeof(GalleryRegistry).Assembly.GetTypes()
            .Where(t => !t.IsAbstract && typeof(Page).IsAssignableFrom(t))
            .Select(t => (Type: t, Info: t.GetCustomAttribute<GalleryPageAttribute>()))
            .Where(x => x.Info is not null)
            .Select(x => new GalleryEntry(x.Info!, x.Type))
            .OrderBy(e => e.Info.Category)
            .ThenBy(e => e.Info.Order)
            .ThenBy(e => e.Info.Title, StringComparer.OrdinalIgnoreCase)
            .ToList();
}

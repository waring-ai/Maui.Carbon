using Maui.Carbon.Generator;

namespace Maui.Carbon.Tests.Generator;

/// <summary>
/// Whether the Carbon design snapshot (design/carbon) is present. It is gitignored, so a fresh clone (CI) has none; the
/// tests that read it skip there and run wherever the snapshot has been put in place.
/// </summary>
public static class DesignSnapshot
{
    public const string Missing = "design/carbon is not in the repository (it is gitignored); put the Carbon design snapshot there to run this test.";

    public static bool Present => Directory.Exists(Path.Combine(RepoPaths.FindRoot(AppContext.BaseDirectory), "design", "carbon"));
}

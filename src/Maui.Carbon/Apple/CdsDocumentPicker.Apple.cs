using Foundation;
using UIKit;
using UniformTypeIdentifiers;

namespace Maui.Carbon;

/// <summary>
/// The iOS and Mac Catalyst side of <see cref="CdsFileUploader"/>'s file access: the document picker in open-in-place
/// mode and the files of a drop, each returned with its original path and its security-scoped access held.
/// </summary>
/// <remarks>
/// MAUI's FilePicker (Essentials 11) also opens files in place (UIDocumentPickerViewController with asCopy false, so
/// nothing is copied), but it stops the security-scoped access right after bookmarking the URL and starts it again only
/// for the stream from FileResult.OpenReadAsync. A sandboxed app then cannot open FileResult.FullPath itself, which a
/// path-based reader (an archive streamed in place) needs. Here the access stays open until the uploader lets the file go.
/// </remarks>
internal static class CdsDocumentPicker
{
    /// <summary>
    /// Shows the document picker for <paramref name="types"/> (identifiers, or ".ext" for an extension's dynamic
    /// type) and returns the chosen files in place; empty when cancelled or when there is no window to present from.
    /// </summary>
    public static async Task<IReadOnlyList<CdsFileGrant>> PickInPlaceAsync(IReadOnlyList<string> types, bool multiple)
    {
        UTType[] contentTypes = [.. types.Select(ToType).OfType<UTType>()];
        if (contentTypes.Length == 0) contentTypes = [UTTypes.Item];

        UIViewController? presenter;
        try
        {
            presenter = Microsoft.Maui.ApplicationModel.Platform.GetCurrentUIViewController();
        }
        catch (Exception ex) when (ex is InvalidOperationException or NullReferenceException)
        {
            presenter = null;
        }
        if (presenter is null) return [];

        // asCopy false opens in place: the picker hands back the original file's URL, never a copy in tmp.
        using var picker = new UIDocumentPickerViewController(contentTypes, asCopy: false) { AllowsMultipleSelection = multiple };
        var picked = new TaskCompletionSource<NSUrl[]?>(TaskCreationOptions.RunContinuationsAsynchronously);
        picker.DidPickDocumentAtUrls += (_, e) => picked.TrySetResult(e.Urls);
        picker.WasCancelled += (_, _) => picked.TrySetResult(null);
        presenter.PresentViewController(picker, true, null);

        var urls = await picked.Task;
        if (urls is not { Length: > 0 }) return [];
        List<CdsFileGrant> grants = [.. urls.Select(Grant).OfType<CdsFileGrant>()];
        await CoordinateReadingAsync(urls);
        return grants;
    }

    /// <summary>
    /// Loads one dropped item as a file: a file URL (Finder on Mac Catalyst) is the original file; a file representation
    /// (the Files app, other apps) is opened in place when the source allows it, and copied to a temporary folder when it
    /// only offers a copy (UIKit deletes its own copy as soon as the load returns). Call it while the drop is performed.
    /// </summary>
    public static Task<CdsFileGrant?> LoadDroppedFileAsync(NSItemProvider provider)
    {
        if (provider.CanLoadObject(typeof(NSUrl))) return LoadFileUrlAsync(provider);

        var type = provider.GetRegisteredTypeIdentifiers(NSItemProviderFileOptions.OpenInPlace).FirstOrDefault()
            ?? provider.RegisteredTypeIdentifiers.FirstOrDefault();
        if (type is null) return Task.FromResult<CdsFileGrant?>(null);

        var loaded = new TaskCompletionSource<CdsFileGrant?>(TaskCreationOptions.RunContinuationsAsynchronously);
        provider.LoadInPlaceFileRepresentation(type, (url, isInPlace, error) =>
        {
            try
            {
                loaded.TrySetResult(error is not null || url is null ? null : isInPlace ? Grant(url) : KeepCopy(url));
            }
            catch (Exception)
            {
                loaded.TrySetResult(null);
            }
        });
        return loaded.Task;
    }

    static async Task<CdsFileGrant?> LoadFileUrlAsync(NSItemProvider provider) =>
        await provider.LoadObjectAsync<NSUrl>() is { } url ? Grant(url) : null;

    // Starts the URL's security-scoped access. It returns false for a URL that has no scope (inside the app's container,
    // or any file for a Mac app without the sandbox): then there is nothing to hold or release.
    static CdsFileGrant? Grant(NSUrl url)
    {
        if (!url.IsFileUrl || url.Path is not { Length: > 0 } path) return null;
        return new CdsFileGrant(path, url.StartAccessingSecurityScopedResource() ? new CdsSecurityScope(url) : null);
    }

    static CdsFileGrant? KeepCopy(NSUrl url)
    {
        if (url.Path is not { Length: > 0 } source) return null;
        var folder = Path.Combine(Path.GetTempPath(), "Maui.Carbon.Drops", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        var target = Path.Combine(folder, Path.GetFileName(source));
        File.Copy(source, target);
        return new CdsFileGrant(target, null);
    }

    // As MAUI's picker does: a coordinated read lets a file provider (iCloud Drive) bring the file down before it is
    // opened. Reading "without changes" never copies the file.
    static async Task CoordinateReadingAsync(NSUrl[] urls)
    {
        NSFileAccessIntent[] intents = [.. urls.Select(u => NSFileAccessIntent.CreateReadingIntent(u, NSFileCoordinatorReadingOptions.WithoutChanges))];
        using var coordinator = new NSFileCoordinator();
        var done = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        coordinator.CoordinateAccess(intents, new NSOperationQueue(), _ => done.TrySetResult());
        await done.Task;
    }

    static UTType? ToType(string entry) =>
        entry.StartsWith('.') ? UTType.CreateFromExtension(entry.TrimStart('.')) : UTType.CreateFromIdentifier(entry);
}

/// <summary>Uniform types and MIME types from the system's type database, for the accept list on iOS and Mac Catalyst.</summary>
internal static class CdsAppleFileTypes
{
    /// <summary>The identifier of a declared type for ".ext" ("public.zip-archive"); <c>null</c> when the system only has a dynamic type.</summary>
    public static string? UniformTypeOfExtension(string extension) =>
        UTType.CreateFromExtension(extension.TrimStart('.')) is { Dynamic: false } type ? type.Identifier : null;

    /// <summary>The identifier of a declared type for a MIME type; <c>null</c> when the system does not know it.</summary>
    public static string? UniformTypeOfMime(string mimeType) =>
        UTType.CreateFromMimeType(mimeType) is { Dynamic: false } type ? type.Identifier : null;

    /// <summary>The preferred MIME type of ".ext", or <c>null</c>.</summary>
    public static string? MimeOfExtension(string extension) =>
        UTType.CreateFromExtension(extension.TrimStart('.'))?.PreferredMimeType;
}

/// <summary>Holds a URL's security-scoped access until disposed.</summary>
internal sealed class CdsSecurityScope(NSUrl url) : IDisposable
{
    NSUrl? _url = url;

    /// <summary>Stops the access (once).</summary>
    public void Dispose() => Interlocked.Exchange(ref _url, null)?.StopAccessingSecurityScopedResource();
}

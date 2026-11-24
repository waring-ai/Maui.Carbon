namespace Maui.Carbon.Tests;

/// <summary>
/// Tests that read or set the process-wide <see cref="CdsFocusTracker"/> join this collection, so they never run in
/// parallel with each other or with the rest of the suite.
/// </summary>
[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class FocusTrackerCollection
{
    /// <summary>The collection name for <c>[Collection(FocusTrackerCollection.Name)]</c>.</summary>
    public const string Name = "FocusTracker";
}

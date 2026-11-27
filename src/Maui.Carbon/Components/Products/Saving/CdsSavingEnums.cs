namespace Maui.Carbon;

/// <summary>How a <see cref="CdsSaving"/> presents the save status.</summary>
public enum CdsSavingType
{
    /// <summary>Auto-save: a status line only ("Saving...", "Saved", "Failed to save").</summary>
    Auto,
    /// <summary>Manual save: a cancel button and a save button that carries the status.</summary>
    Manual,
}

/// <summary>The save status: untouched, in progress, then success or failure.</summary>
public enum CdsSavingStatus
{
    /// <summary>Nothing saved yet (the untouched state).</summary>
    Default,
    /// <summary>A save is in progress.</summary>
    InProgress,
    /// <summary>The last save succeeded.</summary>
    Success,
    /// <summary>The last save failed.</summary>
    Fail,
}

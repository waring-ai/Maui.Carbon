namespace Maui.Carbon;

/// <summary>What the person did in a <see cref="CdsInterstitialScreen"/> (Carbon's <c>ActionType</c>).</summary>
public enum CdsInterstitialScreenAction
{
    /// <summary>The close button of the modal variant, or Escape.</summary>
    Close,
    /// <summary>The start button on the last (or only) step.</summary>
    Start,
    /// <summary>The skip button of a multi-step screen.</summary>
    Skip,
    /// <summary>The back button (every step after the first).</summary>
    Back,
    /// <summary>The next button (every step before the last).</summary>
    Next,
}

/// <summary>The footer buttons of a <see cref="CdsInterstitialScreen"/>, as flags (Carbon's <c>disableButtonConfig</c>).</summary>
[Flags]
public enum CdsInterstitialScreenButtons
{
    /// <summary>No button.</summary>
    None = 0,
    /// <summary>The skip button.</summary>
    Skip = 1,
    /// <summary>The back button.</summary>
    Back = 2,
    /// <summary>The next button.</summary>
    Next = 4,
    /// <summary>The start button.</summary>
    Start = 8,
}

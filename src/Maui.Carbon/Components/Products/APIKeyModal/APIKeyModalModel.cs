using System.Text.Encodings.Web;
using System.Text.Json;

namespace Maui.Carbon;

/// <summary>What the API key modal's state is made of (pure inputs of <see cref="APIKeyModalModel"/>).</summary>
internal readonly record struct APIKeyModalInputs(
    string? ApiKey,
    bool Loading,
    bool Editing,
    bool EditSuccess,
    int StepCount,
    int CurrentStep,
    bool? StepValid,
    bool NameRequired,
    string? Name,
    bool HasVisibilityToggle,
    bool HasDownloadLink,
    bool CopyError,
    bool Error);

/// <summary>What the primary button does.</summary>
internal enum APIKeyPrimaryAction
{
    /// <summary>Go to the next custom step.</summary>
    NextStep,

    /// <summary>Copy the generated key.</summary>
    Copy,

    /// <summary>Save the edited name.</summary>
    Edit,

    /// <summary>Generate the key.</summary>
    Generate,
}

/// <summary>The API key modal's state machine (pure, after <c>APIKeyModal.tsx</c>).</summary>
internal static class APIKeyModalModel
{
    /// <summary>The key is shown and can be copied: there is one and no request runs.</summary>
    public static bool KeyLoaded(in APIKeyModalInputs s) => !string.IsNullOrEmpty(s.ApiKey) && !s.Loading;

    public static bool HasSteps(in APIKeyModalInputs s) => s.StepCount > 0;

    public static bool HasNextStep(in APIKeyModalInputs s) => HasSteps(s) && s.CurrentStep < s.StepCount - 1;

    public static bool HasPreviousStep(in APIKeyModalInputs s) => HasSteps(s) && s.CurrentStep != 0;

    /// <summary>Next on every step but the last, then Copy once the key is loaded, else Save (editing) or Generate.</summary>
    public static APIKeyPrimaryAction PrimaryAction(in APIKeyModalInputs s) =>
        HasNextStep(s) ? APIKeyPrimaryAction.NextStep
        : KeyLoaded(s) ? APIKeyPrimaryAction.Copy
        : s.Editing ? APIKeyPrimaryAction.Edit
        : APIKeyPrimaryAction.Generate;

    /// <summary>The secondary button goes back a step while there is one and no key is shown; otherwise it closes.</summary>
    public static bool SecondaryGoesBack(in APIKeyModalInputs s) => HasPreviousStep(s) && !KeyLoaded(s);

    /// <summary>Disabled while loading, on a custom step that says it is invalid, or without a required name.</summary>
    public static bool IsPrimaryDisabled(in APIKeyModalInputs s)
    {
        if (s.Loading) return true;
        if (HasSteps(s) && s.StepValid is { } valid) return !valid;
        if (!HasSteps(s) && s.NameRequired && string.IsNullOrEmpty(s.Name)) return true;
        return false;
    }

    /// <summary>The custom step's content replaces the generated body until the key is loaded.</summary>
    public static bool ShowsStepContent(in APIKeyModalInputs s) => HasSteps(s) && !KeyLoaded(s);

    /// <summary>The key field shows (read-only) once there is a key, except while editing.</summary>
    public static bool ShowsKey(in APIKeyModalInputs s) => !ShowsStepContent(s) && !s.Editing && !string.IsNullOrEmpty(s.ApiKey);

    /// <summary>The name field shows while editing, or before the key is loaded when a name is required.</summary>
    public static bool ShowsNameInput(in APIKeyModalInputs s) => !ShowsStepContent(s) && (s.Editing || (!KeyLoaded(s) && s.NameRequired));

    /// <summary>The error line shows for a failed copy or a failed request.</summary>
    public static bool ShowsError(in APIKeyModalInputs s) => !ShowsStepContent(s) && (s.CopyError || s.Error);

    /// <summary>The information line (download link or success body) shows once the key is loaded.</summary>
    public static bool ShowsInformation(in APIKeyModalInputs s) => !ShowsStepContent(s) && KeyLoaded(s);

    /// <summary>The success line shows after a saved edit or with the loaded key, when there is a message.</summary>
    public static bool ShowsSuccess(in APIKeyModalInputs s, string? message) =>
        !ShowsStepContent(s) && !string.IsNullOrEmpty(message) && (s.EditSuccess || KeyLoaded(s));

    /// <summary>The title: the custom step's while steps show, else the generate title.</summary>
    public static string? Title(in APIKeyModalInputs s, string? generateTitle, string? stepTitle) =>
        s.Editing && s.EditSuccess ? generateTitle
        : KeyLoaded(s) ? generateTitle
        : HasSteps(s) ? stepTitle
        : generateTitle;

    /// <summary>The success message: the edit one after a saved edit, the generate one with the loaded key.</summary>
    public static string? SuccessMessage(in APIKeyModalInputs s, string? editSuccessMessage, string? generateSuccessMessage) =>
        s.Editing && s.EditSuccess ? editSuccessMessage
        : KeyLoaded(s) ? generateSuccessMessage
        : null;

    /// <summary>The step after a move by <paramref name="delta"/>, kept inside the steps.</summary>
    public static int MoveStep(int current, int delta, int count) => count <= 0 ? 0 : Math.Clamp(current + delta, 0, count - 1);
}

/// <summary>The API key file (pure, after <c>APIKeyDownloader.tsx</c>).</summary>
internal static class APIKeyDownloaderModel
{
    /// <summary>"apikey.json": the name (default <c>apikey</c>) and the type's extension.</summary>
    public static string FileName(string? name, CdsAPIKeyFileType type) =>
        $"{(string.IsNullOrWhiteSpace(name) ? "apikey" : name)}.{Extension(type)}";

    /// <summary>The extension of a file type, without the dot.</summary>
    public static string Extension(CdsAPIKeyFileType type) => type == CdsAPIKeyFileType.Txt ? "txt" : "json";

    /// <summary>The MIME type of a file type.</summary>
    public static string ContentType(CdsAPIKeyFileType type) => type == CdsAPIKeyFileType.Txt ? "text/plain" : "application/json";

    /// <summary>The file content: the key itself, or <c>{"apiKey":"..."}</c>.</summary>
    public static string Content(string? apiKey, CdsAPIKeyFileType type)
    {
        var key = apiKey ?? string.Empty;
        if (type == CdsAPIKeyFileType.Txt) return key;
        return "{\"apiKey\":\"" + JsonEncodedText.Encode(key, JavaScriptEncoder.UnsafeRelaxedJsonEscaping) + "\"}";
    }
}

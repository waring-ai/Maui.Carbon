using System.Text.RegularExpressions;
using Maui.Carbon.Gallery.Infrastructure;
using Maui.Carbon.Gallery.Pages.Patterns.Data;

namespace Maui.Carbon.Gallery.Pages.Patterns;

[GalleryPage("Login pattern", GalleryCategory.Patterns, Order = 74, Spec = "PatternLogin",
    Summary = "A two-step login page: the user ID first (with remember ID, create account and an alternative login), then the password, with inline validation and one generic error after a failed attempt.")]
public partial class PatternLoginPage : GalleryPage
{
    static readonly Regex s_email = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

    public PatternLoginPage()
    {
        InitializeComponent();
        // Validate on blur, clear the error once the value is valid.
        K2Data.OnBlur(FirstId, () => ValidateId(FirstId));
        K2Data.OnBlur(FailedId, () => ValidateId(FailedId));
        FirstId.TextChanged += (_, _) => ClearIfValid(FirstId);
        FailedId.TextChanged += (_, _) => ClearIfValid(FailedId);
        K2Data.OnBlur(PasswordInput, () => ValidatePassword());
        PasswordInput.TextChanged += (_, _) =>
        {
            if (PasswordInput.IsInvalid && !string.IsNullOrEmpty(PasswordInput.Text)) PasswordInput.IsInvalid = false;
        };
    }

    static bool ValidateId(CdsTextInput input)
    {
        var text = (input.Text ?? "").Trim();
        string? error = text.Length == 0
            ? "Enter your email address."
            : s_email.IsMatch(text) ? null : "Enter an email address in the format name@example.com.";
        input.IsInvalid = error is not null;
        input.InvalidText = error;
        return error is null;
    }

    static void ClearIfValid(CdsTextInput input)
    {
        if (input.IsInvalid && s_email.IsMatch((input.Text ?? "").Trim())) input.IsInvalid = false;
    }

    bool ValidatePassword()
    {
        var empty = string.IsNullOrEmpty(PasswordInput.Text);
        PasswordInput.IsInvalid = empty;
        PasswordInput.InvalidText = empty ? "Enter your password." : null;
        return !empty;
    }

    void OnFirstContinue(object? sender, EventArgs e) => Continue(FirstId);

    void OnFailedContinue(object? sender, EventArgs e) => Continue(FailedId);

    void Continue(CdsTextInput input)
    {
        if (!ValidateId(input))
        {
            input.Focus();
            Status.Text = "Fix the user ID to continue";
            return;
        }
        Status.Text = $"Continue to the password step for {input.Text!.Trim()}";
    }

    void OnNotYouClicked(object? sender, EventArgs e) => Status.Text = "Back to step 1 to enter a different user ID";

    void OnLogInClicked(object? sender, EventArgs e)
    {
        if (!ValidatePassword())
        {
            PasswordInput.Focus();
            Status.Text = "Enter your password to log in";
            return;
        }
        // Simulated failure: go back to step 1, keep the ID, clear the password, show one generic error.
        PasswordInput.Text = "";
        Status.Text = "Your user ID or password is incorrect. Back to step 1 with the ID kept (see the third screen)";
    }
}

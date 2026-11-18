using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Maui.Carbon.Relay.Abstractions;
using Maui.Carbon.Relay.Services;

namespace Maui.Carbon.Relay.ViewModels;

/// <summary>Two-step sign-in (email, then password), as in Carbon's login pattern.</summary>
public sealed partial class LoginViewModel(IAuthService auth, ISessionHost session) : ObservableObject
{
    public const string DemoEmail = "maya.chen@relay.dev";

    /// <summary>0 = email step, 1 = password step.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEmailStep), nameof(IsPasswordStep))]
    public partial int Step { get; set; }

    public bool IsEmailStep => Step == 0;

    public bool IsPasswordStep => Step == 1;

    [ObservableProperty]
    public partial string Email { get; set; } = DemoEmail;

    [ObservableProperty]
    public partial string Password { get; set; } = "";

    [ObservableProperty]
    public partial bool RememberMe { get; set; } = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEmailInvalid))]
    public partial string? EmailError { get; set; }

    public bool IsEmailInvalid => EmailError is not null;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsPasswordInvalid))]
    public partial string? PasswordError { get; set; }

    public bool IsPasswordInvalid => PasswordError is not null;

    /// <summary>A form-level error (shown as an inline notification).</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasFormError))]
    public partial string? FormError { get; set; }

    public bool HasFormError => FormError is not null;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ContinueCommand), nameof(SignInCommand))]
    public partial bool IsBusy { get; set; }

    bool NotBusy => !IsBusy;

    [RelayCommand(CanExecute = nameof(NotBusy))]
    async Task ContinueAsync()
    {
        IsBusy = true;
        EmailError = null;
        FormError = null;
        try
        {
            var result = await auth.CheckEmailAsync(Email);
            if (result.Succeeded) Step = 1;
            else EmailError = result.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(NotBusy))]
    async Task SignInAsync()
    {
        IsBusy = true;
        PasswordError = null;
        FormError = null;
        try
        {
            var result = await auth.SignInAsync(Email, Password, RememberMe);
            if (result.Succeeded)
            {
                Password = "";
                Step = 0;
                session.ShowApp();
            }
            else if (result.Field == "password") PasswordError = result.Message;
            else FormError = result.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    void ChangeEmail()
    {
        Step = 0;
        Password = "";
        PasswordError = null;
        FormError = null;
    }
}

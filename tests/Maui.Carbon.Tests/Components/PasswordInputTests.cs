namespace Maui.Carbon.Tests.Components;

public class PasswordInputTests
{
    [Fact]
    public void ToggleSwitchesVisibilityAndLabels()
    {
        var input = new CdsPasswordInput { Label = "Password" };
        Assert.False(input.IsPasswordVisible);
        Assert.Equal("Show password", input.ShowPasswordLabel);
        Assert.Equal("Hide password", input.HidePasswordLabel);
        input.IsPasswordVisible = true;
        Assert.True(input.IsPasswordVisible);
    }

    [Fact]
    public void ControlSetsEveryPropertyWithoutThrowing()
    {
        var input = new CdsPasswordInput
        {
            Label = "API key", HelperText = "At least 12 characters", Size = CdsSize.Lg, IsInvalid = true, InvalidText = "Too short",
            IsWarning = true, WarnText = "Weak", IsReadOnly = true, ShowPasswordLabel = "Reveal", HidePasswordLabel = "Conceal",
            IsPasswordVisible = true, Text = "secret",
        };
        input.IsEnabled = false;
        Assert.Equal("secret", input.Text);
    }
}

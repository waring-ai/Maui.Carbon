namespace Maui.Carbon.Relay.Views;

public partial class AccountPanel : ContentView
{
    public AccountPanel()
    {
        InitializeComponent();
        CdsTokens.SetColor(Avatar, Border.BackgroundColorProperty, CdsColorKeys.BackgroundBrand);
    }
}

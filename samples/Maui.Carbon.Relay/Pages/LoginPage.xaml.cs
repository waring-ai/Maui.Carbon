using Maui.Carbon.Relay.ViewModels;

namespace Maui.Carbon.Relay.Pages;

public partial class LoginPage : ContentPage
{
    public LoginPage(LoginViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
        CdsTokens.SetColor(Rule1, BoxView.ColorProperty, CdsColorKeys.BorderSubtle01);
        CdsTokens.SetColor(Rule2, BoxView.ColorProperty, CdsColorKeys.BorderSubtle01);
        CdsTokens.SetColor(Brand, VisualElement.BackgroundColorProperty, CdsColorKeys.Background);
        CdsTokens.SetColor(Root, VisualElement.BackgroundColorProperty, CdsColorKeys.Background);
        // The brand panel needs the lg breakpoint (1056) to sit beside the form.
        Root.SizeChanged += (_, _) =>
        {
            var wide = Root.Width >= CdsBreakpoints.Lg;
            BrandZone.IsVisible = wide;
            Root.ColumnDefinitions[1].Width = wide ? new GridLength(1.6, GridUnitType.Star) : new GridLength(0);
        };
    }
}

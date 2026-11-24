using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Automation.Peers;
using WAutomationProperties = Microsoft.UI.Xaml.Automation.AutomationProperties;

namespace Maui.Carbon;

public static partial class CdsSemantics
{
    static readonly BindableProperty AppliedProperty = BindableProperty.CreateAttached(
        "CdsGroupApplied", typeof(bool), typeof(CdsSemantics), false);

    // MAUI's layout and content panels have automation peers that become named Pane elements; their children stay in the
    // UI Automation tree, so the name is all Narrator needs.
    static partial void ApplyPlatform(VisualElement element, string? name, string? hint, CdsSemanticGroupKind kind)
    {
        if (element.Handler?.PlatformView is not FrameworkElement view) return;
        if (name is null && hint is null)
        {
            if (!(bool)element.GetValue(AppliedProperty)) return;
            element.SetValue(AppliedProperty, false);
            view.ClearValue(WAutomationProperties.NameProperty);
            view.ClearValue(WAutomationProperties.HelpTextProperty);
            view.ClearValue(WAutomationProperties.LandmarkTypeProperty);
            view.ClearValue(WAutomationProperties.LocalizedLandmarkTypeProperty);
            return;
        }
        element.SetValue(AppliedProperty, true);
        WAutomationProperties.SetName(view, name ?? string.Empty);
        WAutomationProperties.SetHelpText(view, hint ?? string.Empty);
        switch (kind)
        {
            case CdsSemanticGroupKind.Navigation:
                WAutomationProperties.SetLandmarkType(view, AutomationLandmarkType.Navigation);
                break;
            case CdsSemanticGroupKind.Banner:
                WAutomationProperties.SetLandmarkType(view, AutomationLandmarkType.Custom);
                WAutomationProperties.SetLocalizedLandmarkType(view, "banner");
                break;
            default:
                view.ClearValue(WAutomationProperties.LandmarkTypeProperty);
                view.ClearValue(WAutomationProperties.LocalizedLandmarkTypeProperty);
                break;
        }
    }
}

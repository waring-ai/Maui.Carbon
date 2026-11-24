using System.Runtime.InteropServices;
using ObjCRuntime;
using UIKit;

namespace Maui.Carbon;

public static partial class CdsSemantics
{
    static readonly BindableProperty AppliedProperty = BindableProperty.CreateAttached(
        "CdsGroupApplied", typeof(bool), typeof(CdsSemantics), false);

    static partial void ApplyPlatform(VisualElement element, string? name, string? hint, CdsSemanticGroupKind kind)
    {
        if (element.Handler?.PlatformView is not UIView view) return;
        if (name is null && hint is null)
        {
            if (!(bool)element.GetValue(AppliedProperty)) return;
            element.SetValue(AppliedProperty, false);
            view.AccessibilityLabel = null;
            view.AccessibilityHint = null;
            view.ShouldGroupAccessibilityChildren = false;
            SetContainerType(view, UIAccessibilityContainerType.None);
            return;
        }
        element.SetValue(AppliedProperty, true);
        // A UIKit accessibility element is a leaf: VoiceOver would skip every subview. A container with a label and a
        // container type is announced when VoiceOver enters it, and its children stay reachable.
        view.IsAccessibilityElement = false;
        view.AccessibilityLabel = name;
        view.AccessibilityHint = hint;
        view.ShouldGroupAccessibilityChildren = true;
        SetContainerType(view, kind switch
        {
            CdsSemanticGroupKind.Navigation or CdsSemanticGroupKind.Banner => UIAccessibilityContainerType.Landmark,
            CdsSemanticGroupKind.List => UIAccessibilityContainerType.List,
            _ => UIAccessibilityContainerType.SemanticGroup,
        });
    }

    // accessibilityContainerType is declared on the informal UIAccessibilityContainer protocol, which the UIView binding
    // does not implement, so the setter is sent directly.
    static void SetContainerType(UIView view, UIAccessibilityContainerType type) =>
        SendNint(view.Handle, Selector.GetHandle("setAccessibilityContainerType:"), (nint)(long)type);

    [DllImport(Constants.ObjectiveCLibrary, EntryPoint = "objc_msgSend")]
    static extern void SendNint(IntPtr receiver, IntPtr selector, nint value);
}

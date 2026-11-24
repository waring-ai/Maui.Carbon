using AndroidX.Core.View;
using AndroidX.Core.View.Accessibility;
using Microsoft.Maui.Platform;
using AView = Android.Views.View;

namespace Maui.Carbon;

public static partial class CdsSemantics
{
    // TalkBack: a content description would turn the group into a focus stop of its own that speaks the name instead of
    // its children's text. The container title (Android 14+) is Android's fieldset: TalkBack announces it when focus
    // first enters the group, and the group itself is never focused.
    static partial void ApplyPlatform(VisualElement element, string? name, string? hint, CdsSemanticGroupKind kind)
    {
        if (element.Handler?.PlatformView is not AView view) return;
        var existing = Find(ViewCompat.GetAccessibilityDelegate(view));
        if (existing is not null)
        {
            existing.Name = name;
            existing.Hint = hint;
            return;
        }
        if (name is null && hint is null) return;
        ViewCompat.SetAccessibilityDelegate(view, new CdsGroupAccessibilityDelegate(ViewCompat.GetAccessibilityDelegate(view)) { Name = name, Hint = hint });
    }

    // MAUI wraps an existing delegate when it installs its own, so look through the chain.
    static CdsGroupAccessibilityDelegate? Find(AccessibilityDelegateCompat? current)
    {
        for (var i = 0; current is not null && i < 8; i++)
        {
            if (current is CdsGroupAccessibilityDelegate mine) return mine;
            current = (current as AccessibilityDelegateCompatWrapper)?.WrappedDelegate;
        }
        return null;
    }
}

/// <summary>Adds the Carbon group name to a view's accessibility node, keeping any delegate it already had.</summary>
internal sealed class CdsGroupAccessibilityDelegate(AccessibilityDelegateCompat? original) : AccessibilityDelegateCompatWrapper(original)
{
    public string? Name { get; set; }

    public string? Hint { get; set; }

    public override void OnInitializeAccessibilityNodeInfo(AView? host, AccessibilityNodeInfoCompat? info)
    {
        base.OnInitializeAccessibilityNodeInfo(host, info);
        if (info is null) return;
        if (Name is not null) info.ContainerTitle = Name;
        if (Hint is not null) info.HintText = Hint;
    }
}

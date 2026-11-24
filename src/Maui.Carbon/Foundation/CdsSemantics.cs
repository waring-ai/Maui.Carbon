using Microsoft.Maui.Handlers;

namespace Maui.Carbon;

/// <summary>What kind of group <see cref="CdsSemantics.GroupNameProperty"/> names (the web's region or landmark role).</summary>
public enum CdsSemanticGroupKind
{
    /// <summary>A plain group of related controls (<c>fieldset</c>, <c>role="group"</c>, a toolbar, a dialog).</summary>
    Group,
    /// <summary>A navigation landmark (<c>nav</c>): side nav, header navigation, breadcrumb, pagination.</summary>
    Navigation,
    /// <summary>The banner landmark (<c>header</c>) of the UI shell.</summary>
    Banner,
    /// <summary>A list of items (<c>role="list"</c>, <c>listbox</c>, <c>menu</c>, <c>tablist</c>).</summary>
    List,
}

/// <summary>
/// Names a group of controls for assistive technology without hiding its members.
/// <para>
/// Never put <c>SemanticProperties.Description</c> or <c>Hint</c> on a layout, border, scroll view or control that has
/// interactive descendants: on iOS and Mac Catalyst MAUI then makes the container one accessibility element, and VoiceOver
/// can no longer reach the controls inside. Set <see cref="GroupNameProperty"/> (and optionally
/// <see cref="GroupHintProperty"/> and <see cref="GroupKindProperty"/>) instead:
/// </para>
/// <list type="bullet">
/// <item>iOS and Mac Catalyst: the platform view keeps <c>IsAccessibilityElement = false</c>, gets the name as its
/// <c>AccessibilityLabel</c> and becomes an accessibility container (<c>SemanticGroup</c>, <c>Landmark</c> or <c>List</c>),
/// so VoiceOver announces the name when it moves into the group and every child stays reachable.</item>
/// <item>Android: the view's accessibility node gets the name as its container title (Android 14+; TalkBack announces it
/// when focus enters the group). The group gets no content description, so it does not become a TalkBack stop of its own
/// and the children keep their own focus. On older versions the name is not announced; the children stay reachable.</item>
/// <item>Windows: <c>AutomationProperties.Name</c> / <c>HelpText</c> (and a landmark type) on the panel, whose children
/// stay in the UI Automation tree.</item>
/// </list>
/// <para>Usage: <c>CdsSemantics.SetGroupName(_root, Legend);</c> or, in XAML,
/// <c>cds:CdsSemantics.GroupName="Theme"</c>. Leaf controls (hit targets, entries) keep using
/// <c>SemanticProperties.Description</c>.</para>
/// </summary>
public static partial class CdsSemantics
{
    /// <summary>Attached property: the accessible name of a group. <c>null</c> or empty removes the group naming.</summary>
    public static readonly BindableProperty GroupNameProperty = BindableProperty.CreateAttached(
        "GroupName", typeof(string), typeof(CdsSemantics), null, propertyChanged: OnGroupChanged);

    /// <summary>
    /// Attached property: extra information about a group (a form group's message, a table's description). Read by
    /// Narrator and kept in the Android node; VoiceOver does not read hints of containers, so keep the same text visible
    /// (or in the members' hints) as well.
    /// </summary>
    public static readonly BindableProperty GroupHintProperty = BindableProperty.CreateAttached(
        "GroupHint", typeof(string), typeof(CdsSemantics), null, propertyChanged: OnGroupChanged);

    /// <summary>Attached property: what kind of group it is. Default <see cref="CdsSemanticGroupKind.Group"/>.</summary>
    public static readonly BindableProperty GroupKindProperty = BindableProperty.CreateAttached(
        "GroupKind", typeof(CdsSemanticGroupKind), typeof(CdsSemantics), CdsSemanticGroupKind.Group, propertyChanged: OnGroupChanged);

    // Set once per view that ever had group semantics: HandlerChanged is subscribed, and the platform side knows to
    // clear what it applied when the name goes away.
    static readonly BindableProperty TrackedProperty = BindableProperty.CreateAttached(
        "CdsGroupTracked", typeof(bool), typeof(CdsSemantics), false);

    static bool s_mapperRegistered;

    /// <summary>Gets the accessible group name of <paramref name="view"/>.</summary>
    public static string? GetGroupName(BindableObject view) => (string?)view.GetValue(GroupNameProperty);

    /// <summary>Names <paramref name="view"/> as a group without hiding its children (see <see cref="CdsSemantics"/>).</summary>
    public static void SetGroupName(BindableObject view, string? value) => view.SetValue(GroupNameProperty, value);

    /// <summary>Gets the accessible group hint of <paramref name="view"/>.</summary>
    public static string? GetGroupHint(BindableObject view) => (string?)view.GetValue(GroupHintProperty);

    /// <summary>Sets extra information about the group <paramref name="view"/> (see <see cref="GroupHintProperty"/>).</summary>
    public static void SetGroupHint(BindableObject view, string? value) => view.SetValue(GroupHintProperty, value);

    /// <summary>Gets the group kind of <paramref name="view"/>.</summary>
    public static CdsSemanticGroupKind GetGroupKind(BindableObject view) => (CdsSemanticGroupKind)view.GetValue(GroupKindProperty);

    /// <summary>Sets the group kind of <paramref name="view"/> (landmark, list or plain group).</summary>
    public static void SetGroupKind(BindableObject view, CdsSemanticGroupKind value) => view.SetValue(GroupKindProperty, value);

    /// <summary>Names <paramref name="view"/> as a group of <paramref name="kind"/> in one call.</summary>
    public static void SetGroup(BindableObject view, string? name, CdsSemanticGroupKind kind = CdsSemanticGroupKind.Group, string? hint = null)
    {
        SetGroupKind(view, kind);
        SetGroupHint(view, hint);
        SetGroupName(view, name);
    }

    /// <summary>True when <paramref name="view"/> carries a group name or hint.</summary>
    internal static bool HasGroup(BindableObject view) =>
        !string.IsNullOrWhiteSpace(GetGroupName(view)) || !string.IsNullOrWhiteSpace(GetGroupHint(view));

    /// <summary>
    /// Re-applies group semantics after MAUI maps <c>SemanticProperties</c> (which resets the platform label of a layout).
    /// Called once from <c>UseCarbon</c>; without it the group is still applied when the handler connects and on changes.
    /// </summary>
    internal static void RegisterMapper()
    {
        if (s_mapperRegistered) return;
        s_mapperRegistered = true;
        ViewHandler.ViewMapper.AppendToMapping("Semantics", static (_, view) =>
        {
            if (view is VisualElement element && (bool)element.GetValue(TrackedProperty)) Apply(element);
        });
    }

    static void OnGroupChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is not VisualElement element) return;
        if (!(bool)element.GetValue(TrackedProperty))
        {
            if (!HasGroup(element)) return;
            element.SetValue(TrackedProperty, true);
            element.HandlerChanged += static (sender, _) => Apply((VisualElement)sender!);
        }
        Apply(element);
    }

    static void Apply(VisualElement element)
    {
        if (element.Handler is null) return;
        var name = GetGroupName(element);
        var hint = GetGroupHint(element);
        ApplyPlatform(element, string.IsNullOrWhiteSpace(name) ? null : name, string.IsNullOrWhiteSpace(hint) ? null : hint, GetGroupKind(element));
    }

    /// <summary>Applies (or, with both texts <c>null</c>, removes) the group semantics on the platform view.</summary>
    static partial void ApplyPlatform(VisualElement element, string? name, string? hint, CdsSemanticGroupKind kind);
}

using System.Collections;
using System.Reflection;

namespace Maui.Carbon.Tests.Foundation;

/// <summary>
/// Description or Hint on a container makes it one VoiceOver element on iOS and Mac Catalyst and hides its children
/// (MAUI's SemanticExtensions.UpdateSemantics). Containers are named with CdsSemantics.GroupName instead. These tests build
/// every public Carbon view with a parameterless constructor, give it names and a few items, and walk the tree.
/// </summary>
public class ContainerSemanticsAuditTests
{
    // Properties that carry a control's name or a group's message: setting them exercises the naming code.
    static readonly string[] TextProperties =
        ["Label", "Legend", "Title", "Description", "MenuLabel", "Heading", "HelperText", "MessageText", "Text", "Subtitle", "Placeholder"];

    [Fact]
    public void No_container_with_interactive_descendants_has_a_description_or_hint()
    {
        var violations = new List<string>();
        var skipped = new List<string>();
        var checkedCount = 0;
        foreach (var type in AuditedTypes())
        {
            View view;
            try
            {
                view = (View)Activator.CreateInstance(type)!;
                Populate(view);
            }
            catch (Exception e)
            {
                skipped.Add($"{type.Name}: {(e is TargetInvocationException { InnerException: { } inner } ? inner : e).GetType().Name}");
                continue;
            }
            checkedCount++;
            violations.AddRange(Violations(view, type.Name));
        }
        TestContext.Current.TestOutputHelper?.WriteLine($"Checked {checkedCount} types; skipped (throw headlessly): {string.Join(", ", skipped)}");
        Assert.True(checkedCount > 60, $"only {checkedCount} types could be built");
        Assert.True(violations.Count == 0, "Containers named with SemanticProperties (use CdsSemantics.SetGroupName):\n" + string.Join("\n", violations));
    }

    [Fact]
    public void Overlay_surfaces_are_named_as_groups()
    {
        var list = new CdsListBoxList(CdsSize.Md, inline: false, multi: false, "Regions");
        Assert.Equal("Regions", CdsSemantics.GetGroupName(list.Surface));
        Assert.Equal(CdsSemanticGroupKind.List, CdsSemantics.GetGroupKind(list.Surface));
        Assert.Null(SemanticProperties.GetDescription(list.Surface));

        var calendar = new CdsCalendarView();
        Assert.Empty(Violations(calendar, nameof(CdsCalendarView)));
        Assert.Contains(Walk(calendar), e => CdsSemantics.GetGroupName(e) == "Choose date");

        var menu = new CdsBreadcrumbMenu([new CdsBreadcrumbItem { Text = "Projects" }], _ => { });
        Assert.Equal("Breadcrumb levels", CdsSemantics.GetGroupName(menu));
        Assert.Empty(Violations(menu, nameof(CdsBreadcrumbMenu)));
    }

    [Fact]
    public void Modal_surface_name_is_a_group_name()
    {
        var footer = new CdsModalFooter { PrimaryButtonText = "Delete", SecondaryButtonText = "Cancel" };
        var surface = new CdsModalSurface(CdsModalMode.Modal, CdsModalSize.Md, false, new CdsModalHeader { Title = "Delete bucket" }, new CdsModalBody(), footer);
        surface.SetName("Delete bucket");
        Assert.Contains(Walk(surface), e => CdsSemantics.GetGroupName(e) == "Delete bucket");
        Assert.Empty(Violations(surface, nameof(CdsModalSurface)));
    }

    [Fact]
    public void Data_table_description_moves_to_the_body_group_hint()
    {
        var table = new CdsDataTable
        {
            Title = "Guests",
            Description = "Virtual machines and containers",
            Selection = CdsDataTableSelection.Multi,
            RowKeySelector = r => (string)r,
            ItemsSource = new[] { "vm-1", "vm-2" },
        };
        table.Headers.Add(new CdsDataTableHeader { Key = "name", Header = "Name", ValueSelector = r => r });
        var tree = Walk(table).ToList();
        Assert.Contains(tree, e => CdsSemantics.GetGroupName(e) == "Guests" && CdsSemantics.GetGroupHint(e) == "Virtual machines and containers");
        Assert.DoesNotContain(tree, e => e is CdsTable && SemanticProperties.GetHint(e) is not null);
        var violations = Violations(table, nameof(CdsDataTable)).ToList();
        Assert.True(violations.Count == 0, string.Join("\n", violations));
    }

    [Fact]
    public void Form_group_message_becomes_the_group_hint()
    {
        var group = new CdsFormGroup { Legend = "Contact", MessageText = "Give at least one way to reach you", Body = new CdsTextInput { Label = "Email" } };
        var tree = Walk(group).ToList();
        Assert.Contains(tree, e => CdsSemantics.GetGroupName(e) == "Contact" && CdsSemantics.GetGroupHint(e) == "Give at least one way to reach you");
        Assert.Empty(Violations(group, nameof(CdsFormGroup)));
    }

    [Fact]
    public void Shell_landmarks_are_named_groups_of_the_right_kind()
    {
        var nav = new CdsSideNav { Label = "Main navigation" };
        Assert.Equal("Main navigation", CdsSemantics.GetGroupName(nav));
        Assert.Equal(CdsSemanticGroupKind.Navigation, CdsSemantics.GetGroupKind(nav));
        Assert.Null(SemanticProperties.GetDescription(nav));

        var header = new CdsHeader { Label = "Waring" };
        Assert.Equal(CdsSemanticGroupKind.Banner, CdsSemantics.GetGroupKind(header));
        Assert.Null(SemanticProperties.GetDescription(header));
    }

    [Fact]
    public void The_audit_catches_a_named_container()
    {
        var grid = new Grid { new CdsHitTarget() };
        SemanticProperties.SetDescription(grid, "Theme");
        Assert.Single(Violations(grid, "test"));

        var hidden = new Grid { new CdsHitTarget { IsVisible = false } };
        SemanticProperties.SetHint(hidden, "Leaf");
        Assert.Empty(Violations(hidden, "test"));

        var named = new Grid { new CdsHitTarget() };
        CdsSemantics.SetGroupName(named, "Theme");
        Assert.Empty(Violations(named, "test"));
    }

    [Fact]
    public void Named_groups_keep_their_name_off_semantic_properties()
    {
        var radios = new CdsRadioButtonGroup { Legend = "Theme" };
        radios.Items.Add(new CdsRadioButton { Label = "Light", Value = "light" });
        radios.Items.Add(new CdsRadioButton { Label = "Dark", Value = "dark" });
        Assert.Contains(Walk(radios), e => CdsSemantics.GetGroupName(e) == "Theme");
        Assert.Empty(Violations(radios, nameof(CdsRadioButtonGroup)));

        var tabs = new CdsTabs { Label = "Settings sections" };
        tabs.Items.Add(new CdsTab { Text = "General" });
        tabs.Items.Add(new CdsTab { Text = "Advanced" });
        Assert.Contains(Walk(tabs), e => CdsSemantics.GetGroupName(e) == "Settings sections" && CdsSemantics.GetGroupKind(e) == CdsSemanticGroupKind.List);
        Assert.Empty(Violations(tabs, nameof(CdsTabs)));
    }

    // ----- helpers (area agents: reuse these when you add a container) -----

    static IEnumerable<Type> AuditedTypes() =>
        typeof(CdsControl).Assembly.GetExportedTypes()
            .Where(t => !t.IsAbstract && !t.ContainsGenericParameters && typeof(View).IsAssignableFrom(t) && t.GetConstructor(Type.EmptyTypes) is not null)
            .OrderBy(t => t.FullName, StringComparer.Ordinal);

    static void Populate(View view)
    {
        SetTexts(view, "Name");
        foreach (var property in view.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (property.GetIndexParameters().Length != 0 || !property.CanRead) continue;
            var listType = property.PropertyType.GetInterfaces().Append(property.PropertyType)
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IList<>));
            if (listType is null || property.PropertyType.IsArray) continue;
            var itemType = listType.GetGenericArguments()[0];
            if (itemType.IsAbstract || !typeof(Element).IsAssignableFrom(itemType) || itemType.GetConstructor(Type.EmptyTypes) is null) continue;
            if (property.GetValue(view) is not IList list || list.IsReadOnly) continue;
            try
            {
                for (var i = 0; i < 2; i++)
                {
                    var item = (Element)Activator.CreateInstance(itemType)!;
                    SetTexts(item, $"Item {i + 1}");
                    list.Add(item);
                }
            }
            catch (Exception)
            {
                // Some item types need more than a name; the control is still checked with what was added.
            }
        }
    }

    static void SetTexts(object target, string text)
    {
        foreach (var name in TextProperties)
        {
            var property = target.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            if (property is not { CanWrite: true } || property.PropertyType != typeof(string) || property.SetMethod?.IsPublic != true) continue;
            try
            {
                property.SetValue(target, text);
            }
            catch (Exception)
            {
                // A property that rejects this text is irrelevant here.
            }
        }
    }

    /// <summary>Every container (layout, content view, border, scroll view) that has a visible interactive descendant and a Description or Hint.</summary>
    internal static IEnumerable<string> Violations(Element root, string owner)
    {
        foreach (var element in Walk(root))
        {
            if (element is not (Layout or ContentView or Border or ScrollView)) continue;
            var description = SemanticProperties.GetDescription(element);
            var hint = SemanticProperties.GetHint(element);
            if (string.IsNullOrWhiteSpace(description) && string.IsNullOrWhiteSpace(hint)) continue;
            if (!HasVisibleInteractiveDescendant(element)) continue;
            yield return $"{owner}: {element.GetType().Name} has Description='{description}' Hint='{hint}'";
        }
    }

    static bool IsInteractive(Element e) =>
        e is CdsHitTarget or InputView or Button or ImageButton or Picker or DatePicker or TimePicker or Slider or Stepper or Switch or CheckBox or RadioButton;

    static bool HasVisibleInteractiveDescendant(Element container)
    {
        foreach (var child in Children(container))
        {
            if (child is VisualElement { IsVisible: false }) continue;
            if (IsInteractive(child) || HasVisibleInteractiveDescendant(child)) return true;
        }
        return false;
    }

    internal static IEnumerable<Element> Walk(Element root)
    {
        var seen = new HashSet<Element>(ReferenceEqualityComparer.Instance);
        var stack = new Stack<Element>();
        stack.Push(root);
        while (stack.Count > 0)
        {
            var e = stack.Pop();
            if (!seen.Add(e)) continue;
            yield return e;
            foreach (var c in Children(e)) stack.Push(c);
        }
    }

    static IEnumerable<Element> Children(Element e)
    {
        if (e is IVisualTreeElement tree)
            foreach (var c in tree.GetVisualChildren())
                if (c is Element child) yield return child;
        switch (e)
        {
            case ContentView { Content: { } content }: yield return content; break;
            case Border { Content: { } content }: yield return content; break;
            case ScrollView { Content: { } content }: yield return content; break;
            case Layout layout:
                foreach (var c in layout.Children)
                    if (c is Element child) yield return child;
                break;
        }
    }
}

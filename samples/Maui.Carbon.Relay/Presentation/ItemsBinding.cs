using System.Collections;
using System.Collections.Specialized;

namespace Maui.Carbon.Relay.Presentation;

/// <summary>
/// Data-binds the declarative item collections of Carbon controls (<c>CdsContainedList.Items</c>,
/// <c>CdsSwitcher.Items</c>, <c>CdsStructuredList.Rows</c>, <c>CdsAccordion.Items</c>…), which take child views rather
/// than an <c>ItemsSource</c>. Each source item is rendered with <see cref="TemplateProperty"/> (its binding context is
/// the item) and added to the collection named by <see cref="CollectionProperty"/> (default <c>Items</c>). Observable
/// sources stay in sync.
/// <code>
/// &lt;cds:CdsContainedList p:ItemsBinding.Source="{Binding Activity}"&gt;
///     &lt;p:ItemsBinding.Template&gt;&lt;DataTemplate x:DataType="vm:ActivityRow"&gt;&lt;cds:CdsContainedListItem Text="{Binding Text}" /&gt;&lt;/DataTemplate&gt;&lt;/p:ItemsBinding.Template&gt;
/// &lt;/cds:CdsContainedList&gt;
/// </code>
/// Set the template before the source (XAML attribute order does not matter: either change re-renders).
/// </summary>
public static class ItemsBinding
{
    public static readonly BindableProperty SourceProperty = BindableProperty.CreateAttached(
        "Source", typeof(IEnumerable), typeof(ItemsBinding), null, propertyChanged: OnSourceChanged);

    public static readonly BindableProperty TemplateProperty = BindableProperty.CreateAttached(
        "Template", typeof(DataTemplate), typeof(ItemsBinding), null, propertyChanged: (b, _, _) => Render(b));

    public static readonly BindableProperty CollectionProperty = BindableProperty.CreateAttached(
        "Collection", typeof(string), typeof(ItemsBinding), "Items", propertyChanged: (b, _, _) => Render(b));

    static readonly BindableProperty SubscriptionProperty = BindableProperty.CreateAttached(
        "Subscription", typeof(Subscription), typeof(ItemsBinding), null);

    public static IEnumerable? GetSource(BindableObject target) => (IEnumerable?)target.GetValue(SourceProperty);

    public static void SetSource(BindableObject target, IEnumerable? value) => target.SetValue(SourceProperty, value);

    public static DataTemplate? GetTemplate(BindableObject target) => (DataTemplate?)target.GetValue(TemplateProperty);

    public static void SetTemplate(BindableObject target, DataTemplate? value) => target.SetValue(TemplateProperty, value);

    public static string GetCollection(BindableObject target) => (string)target.GetValue(CollectionProperty);

    public static void SetCollection(BindableObject target, string value) => target.SetValue(CollectionProperty, value);

    sealed class Subscription(BindableObject target, INotifyCollectionChanged source)
    {
        public void Attach() => source.CollectionChanged += OnChanged;

        public void Detach() => source.CollectionChanged -= OnChanged;

        void OnChanged(object? sender, NotifyCollectionChangedEventArgs e) => Render(target);
    }

    static void OnSourceChanged(BindableObject target, object? oldValue, object? newValue)
    {
        (target.GetValue(SubscriptionProperty) as Subscription)?.Detach();
        target.SetValue(SubscriptionProperty, null);
        if (newValue is INotifyCollectionChanged observable)
        {
            var subscription = new Subscription(target, observable);
            subscription.Attach();
            target.SetValue(SubscriptionProperty, subscription);
        }
        Render(target);
    }

    // Named cases keep the getters alive under trimming (the library is trimmable); reflection covers the rest.
    static object? Collection(BindableObject target, string name) => (target, name) switch
    {
        (CdsContainedList c, "Items") => c.Items,
        (CdsSwitcher s, "Items") => s.Items,
        (CdsStructuredList l, "Rows") => l.Rows,
        (CdsAccordion a, "Items") => a.Items,
        (CdsList l, "Items") => l.Items,
        (CdsCheckboxGroup g, "Items") => g.Items,
        (CdsRadioButtonGroup g, "Items") => g.Items,
        (CdsBreadcrumb b, "Items") => b.Items,
        (CdsOverflowMenu m, "Items") => m.Items,
        (CdsMenuButton m, "Items") => m.Items,
        (CdsTreeView t, "Nodes") => t.Nodes,
        (Layout l, "Children") => l.Children,
        _ => target.GetType().GetProperty(name)?.GetValue(target),
    };

    static void Render(BindableObject target)
    {
        if (GetTemplate(target) is not { } template) return;
        if (Collection(target, GetCollection(target)) is not IList items) return;
        items.Clear();
        if (GetSource(target) is not { } source) return;
        foreach (var item in source)
        {
            var selected = template is DataTemplateSelector selector ? selector.SelectTemplate(item, target) : template;
            if (selected.CreateContent() is not BindableObject view) continue;
            view.BindingContext = item;
            items.Add(view);
        }
    }
}

namespace Maui.Carbon;

/// <summary>
/// One entry of a <see cref="CdsSingleAddSelect"/> or <see cref="CdsMultiAddSelect"/>: a title and optional subtitle,
/// icon, details (<see cref="Meta"/>), attributes to filter and sort by, and child entries to navigate into.
/// </summary>
public class CdsAddSelectItem
{
    /// <summary>A unique identifier.</summary>
    public string Id { get; set; } = "";

    /// <summary>The name shown in the list and searched.</summary>
    public string Title { get; set; } = "";

    /// <summary>The value the app receives when the item is chosen.</summary>
    public string Value { get; set; } = "";

    /// <summary>A second line under the title (an email address, a type).</summary>
    public string? Subtitle { get; set; }

    /// <summary>An icon before the title.</summary>
    public CdsIconName? Icon { get; set; }

    /// <summary>
    /// The item's attributes by name ("role" = "Admin"): what global filters, sorting and the multi-select sidebar
    /// read. <c>title</c>, <c>subtitle</c>, <c>value</c> and <c>id</c> are always available.
    /// </summary>
    public IDictionary<string, string> Attributes { get; } = new Dictionary<string, string>();

    /// <summary>Details shown in the meta panel, opened from the item's view button.</summary>
    public IList<CdsAddSelectMeta> Meta { get; } = [];

    /// <summary>Child entries the person can navigate into (a folder, a group).</summary>
    public IList<CdsAddSelectItem> Children { get; } = [];

    /// <summary>The initial value of the modifier (<see cref="CdsMultiAddSelect.Modifier"/>) for this item.</summary>
    public object? ModifierValue { get; set; }
}

/// <summary>A labelled detail of a <see cref="CdsAddSelectItem"/>, shown in the meta panel.</summary>
public class CdsAddSelectMeta
{
    /// <summary>The detail's label ("Owner").</summary>
    public string Title { get; set; } = "";

    /// <summary>The detail's value.</summary>
    public string? Value { get; set; }
}

/// <summary>A filter the multi add-select offers in its filter panel: the values of one attribute of the items.</summary>
public class CdsAddSelectFilter
{
    /// <summary>The attribute filtered on (a key of <see cref="CdsAddSelectItem.Attributes"/>).</summary>
    public string Id { get; set; } = "";

    /// <summary>The dropdown label.</summary>
    public string Label { get; set; } = "";
}

/// <summary>
/// A per-item choice in the multi add-select (a role for each added user): a dropdown, or a multi-select, on every
/// selected row.
/// </summary>
public class CdsAddSelectModifier
{
    /// <summary>The name the chosen value is reported under.</summary>
    public string Id { get; set; } = "";

    /// <summary>The dropdown label.</summary>
    public string? Title { get; set; }

    /// <summary>The placeholder before a value is chosen.</summary>
    public string? Label { get; set; }

    /// <summary>The values to choose from.</summary>
    public IList<string> Options { get; } = [];

    /// <summary>Lets the person choose several values.</summary>
    public bool IsMultiSelect { get; set; }
}

/// <summary>What an add-select submitted.</summary>
public sealed class CdsAddSelectSubmittedEventArgs(IReadOnlyList<CdsAddSelectItem> items, IReadOnlyDictionary<string, object?> modifiers) : EventArgs
{
    /// <summary>The chosen items (one for a single add-select).</summary>
    public IReadOnlyList<CdsAddSelectItem> Items { get; } = items;

    /// <summary>The modifier value chosen for each item, by item id (empty without a modifier).</summary>
    public IReadOnlyDictionary<string, object?> Modifiers { get; } = modifiers;
}

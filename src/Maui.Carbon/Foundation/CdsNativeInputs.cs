using Microsoft.Maui.Handlers;

namespace Maui.Carbon;

/// <summary>An Entry with every bit of native chrome removed (used inside Carbon field chrome).</summary>
public sealed class CdsNativeEntry : Entry
{
    /// <summary>Creates a chrome-less entry in <c>body-compact-01</c> and <c>text-primary</c>.</summary>
    public CdsNativeEntry()
    {
        BackgroundColor = Colors.Transparent;
        VerticalOptions = LayoutOptions.Center;
        CdsTypography.Apply(this, CdsTypeStyle.BodyCompact01);
        this.SetDynamicResource(TextColorProperty, CdsColorKeys.TextPrimary);
        this.SetDynamicResource(PlaceholderColorProperty, CdsColorKeys.TextPlaceholder);
    }
}

/// <summary>An Editor with every bit of native chrome removed (used inside Carbon field chrome).</summary>
public sealed class CdsNativeEditor : Editor
{
    /// <summary>Creates a chrome-less editor in <c>body-01</c> and <c>text-primary</c>.</summary>
    public CdsNativeEditor()
    {
        BackgroundColor = Colors.Transparent;
        CdsTypography.Apply(this, CdsTypeStyle.Body01);
        this.SetDynamicResource(TextColorProperty, CdsColorKeys.TextPrimary);
        this.SetDynamicResource(PlaceholderColorProperty, CdsColorKeys.TextPlaceholder);
    }
}

/// <summary>A Picker with native chrome removed (the platform's own list for Carbon's <c>Select</c>).</summary>
public sealed class CdsNativePicker : Picker
{
    /// <summary>Creates a chrome-less picker in <c>body-compact-01</c> and <c>text-primary</c>.</summary>
    public CdsNativePicker()
    {
        BackgroundColor = Colors.Transparent;
        VerticalOptions = LayoutOptions.Center;
        var spec = CdsTypeSpec.Of(CdsTypeStyle.BodyCompact01);
        FontFamily = spec.FontFamily;
        FontSize = spec.FontSize;
        CharacterSpacing = spec.CharacterSpacing;
        this.SetDynamicResource(TextColorProperty, CdsColorKeys.TextPrimary);
        this.SetDynamicResource(TitleColorProperty, CdsColorKeys.TextPlaceholder);
    }
}

/// <summary>Entry handler that strips platform borders, underlines, padding and focus visuals.</summary>
public partial class CdsNativeEntryHandler : EntryHandler
{
    /// <summary>The mapper.</summary>
    public static readonly IPropertyMapper<IEntry, CdsNativeEntryHandler> NativeMapper =
        new PropertyMapper<IEntry, CdsNativeEntryHandler>(EntryHandler.Mapper)
        {
            [nameof(IView.Background)] = static (h, _) => h.Strip(),
        };

    /// <summary>Creates the handler.</summary>
    public CdsNativeEntryHandler() : base(NativeMapper, CommandMapper)
    {
    }

    partial void Strip();
}

/// <summary>Editor handler that strips platform borders, insets and focus visuals.</summary>
public partial class CdsNativeEditorHandler : EditorHandler
{
    /// <summary>The mapper.</summary>
    public static readonly IPropertyMapper<IEditor, CdsNativeEditorHandler> NativeMapper =
        new PropertyMapper<IEditor, CdsNativeEditorHandler>(EditorHandler.Mapper)
        {
            [nameof(IView.Background)] = static (h, _) => h.Strip(),
        };

    /// <summary>Creates the handler.</summary>
    public CdsNativeEditorHandler() : base(NativeMapper, CommandMapper)
    {
    }

    partial void Strip();
}

/// <summary>Picker handler that strips platform borders and focus visuals.</summary>
public partial class CdsNativePickerHandler : PickerHandler
{
    /// <summary>The mapper.</summary>
    public static readonly IPropertyMapper<IPicker, CdsNativePickerHandler> NativeMapper =
        new PropertyMapper<IPicker, CdsNativePickerHandler>(PickerHandler.Mapper)
        {
            [nameof(IView.Background)] = static (h, _) => h.Strip(),
        };

    /// <summary>Creates the handler.</summary>
    public CdsNativePickerHandler() : base(NativeMapper, CommandMapper)
    {
    }

    partial void Strip();
}

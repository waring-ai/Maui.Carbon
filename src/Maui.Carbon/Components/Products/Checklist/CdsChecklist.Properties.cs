using System.Windows.Input;

namespace Maui.Carbon;

public partial class CdsChecklist
{
    /// <summary>Bindable property for <see cref="Title"/>.</summary>
    public static readonly BindableProperty TitleProperty = BindableProperty.Create(
        nameof(Title), typeof(string), typeof(CdsChecklist), null, propertyChanged: OnHeaderChanged);

    /// <summary>Bindable property for <see cref="ChartValue"/>.</summary>
    public static readonly BindableProperty ChartValueProperty = BindableProperty.Create(
        nameof(ChartValue), typeof(double?), typeof(CdsChecklist), null, propertyChanged: OnHeaderChanged);

    /// <summary>Bindable property for <see cref="ChartLabel"/>.</summary>
    public static readonly BindableProperty ChartLabelProperty = BindableProperty.Create(
        nameof(ChartLabel), typeof(string), typeof(CdsChecklist), null, propertyChanged: OnHeaderChanged);

    /// <summary>Bindable property for <see cref="IsOpen"/>.</summary>
    public static readonly BindableProperty IsOpenProperty = BindableProperty.Create(
        nameof(IsOpen), typeof(bool), typeof(CdsChecklist), true, BindingMode.TwoWay, propertyChanged: static (b, o, n) => ((CdsChecklist)b).OnOpenChanged((bool)o, (bool)n));

    /// <summary>Bindable property for <see cref="ShowToggle"/>.</summary>
    public static readonly BindableProperty ShowToggleProperty = BindableProperty.Create(
        nameof(ShowToggle), typeof(bool), typeof(CdsChecklist), true, propertyChanged: OnHeaderChanged);

    /// <summary>Bindable property for <see cref="ToggleLabel"/>.</summary>
    public static readonly BindableProperty ToggleLabelProperty = BindableProperty.Create(
        nameof(ToggleLabel), typeof(string), typeof(CdsChecklist), "Toggle", propertyChanged: OnHeaderChanged);

    /// <summary>Bindable property for <see cref="ToggleDescription"/>.</summary>
    public static readonly BindableProperty ToggleDescriptionProperty = BindableProperty.Create(
        nameof(ToggleDescription), typeof(string), typeof(CdsChecklist), "Checklist toggle", propertyChanged: OnHeaderChanged);

    /// <summary>Bindable property for <see cref="TogglePlacement"/>.</summary>
    public static readonly BindableProperty TogglePlacementProperty = BindableProperty.Create(
        nameof(TogglePlacement), typeof(CdsPlacement), typeof(CdsChecklist), CdsPlacement.Top, propertyChanged: OnHeaderChanged);

    /// <summary>Bindable property for <see cref="ViewAllLabel"/>.</summary>
    public static readonly BindableProperty ViewAllLabelProperty = BindableProperty.Create(
        nameof(ViewAllLabel), typeof(string), typeof(CdsChecklist), null, propertyChanged: OnFooterChanged);

    /// <summary>Bindable property for <see cref="ViewAllCommand"/>.</summary>
    public static readonly BindableProperty ViewAllCommandProperty = BindableProperty.Create(
        nameof(ViewAllCommand), typeof(ICommand), typeof(CdsChecklist), null, propertyChanged: OnFooterChanged);

    /// <summary>The title (<c>heading-compact-02</c>). With no title and no chart there is no header, and so no toggle.</summary>
    public string? Title
    {
        get => (string?)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>Progress from 0 to 1 for the ring (15% is 0.15). The ring shows only with <see cref="ChartLabel"/> too; 0 is a valid value.</summary>
    public double? ChartValue
    {
        get => (double?)GetValue(ChartValueProperty);
        set => SetValue(ChartValueProperty, value);
    }

    /// <summary>Text beside the ring (<c>label-01</c>, <c>text-secondary</c>), such as "15% complete"; it also names the ring.</summary>
    public string? ChartLabel
    {
        get => (string?)GetValue(ChartLabelProperty);
        set => SetValue(ChartLabelProperty, value);
    }

    /// <summary>Whether the task list is shown (two-way; default <c>true</c>). The toggle flips it.</summary>
    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    /// <summary>Whether the header shows the open/close toggle (Carbon's <c>enableToggle</c>). Default <c>true</c>.</summary>
    public bool ShowToggle
    {
        get => (bool)GetValue(ShowToggleProperty);
        set => SetValue(ShowToggleProperty, value);
    }

    /// <summary>The toggle's tooltip text (Carbon's <c>toggleLabel</c>). Default "Toggle".</summary>
    public string? ToggleLabel
    {
        get => (string?)GetValue(ToggleLabelProperty);
        set => SetValue(ToggleLabelProperty, value);
    }

    /// <summary>The toggle's accessible name (Carbon's <c>checklistToggleAriaLabel</c>). Default "Checklist toggle".</summary>
    public string? ToggleDescription
    {
        get => (string?)GetValue(ToggleDescriptionProperty);
        set => SetValue(ToggleDescriptionProperty, value);
    }

    /// <summary>Where the toggle's tooltip opens (Carbon's <c>toggleLabelAlign</c>). Default <see cref="CdsPlacement.Top"/>.</summary>
    public CdsPlacement TogglePlacement
    {
        get => (CdsPlacement)GetValue(TogglePlacementProperty);
        set => SetValue(TogglePlacementProperty, value);
    }

    /// <summary>Shows a "View all" link at the bottom with this text, such as "View all (10)".</summary>
    public string? ViewAllLabel
    {
        get => (string?)GetValue(ViewAllLabelProperty);
        set => SetValue(ViewAllLabelProperty, value);
    }

    /// <summary>Executed when the "View all" link is activated.</summary>
    public ICommand? ViewAllCommand
    {
        get => (ICommand?)GetValue(ViewAllCommandProperty);
        set => SetValue(ViewAllCommandProperty, value);
    }

    static void OnHeaderChanged(BindableObject b, object? o, object? n) => ((CdsChecklist)b).UpdateHeader();

    static void OnFooterChanged(BindableObject b, object? o, object? n) => ((CdsChecklist)b).UpdateFooter();
}

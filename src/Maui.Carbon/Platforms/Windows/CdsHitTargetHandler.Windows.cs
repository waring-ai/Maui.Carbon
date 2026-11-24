using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using WAutomationProperties = Microsoft.UI.Xaml.Automation.AutomationProperties;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Automation.Provider;
using Microsoft.UI.Xaml.Markup;
using WButton = Microsoft.UI.Xaml.Controls.Button;
using WValueProvider = Microsoft.UI.Xaml.Automation.Provider.IValueProvider;
using WControlTemplate = Microsoft.UI.Xaml.Controls.ControlTemplate;
using WSolidColorBrush = Microsoft.UI.Xaml.Media.SolidColorBrush;

namespace Maui.Carbon;

public partial class CdsHitTargetHandler
{
    static WControlTemplate? s_template;

    /// <inheritdoc/>
    protected override WButton CreatePlatformView() => new CdsWindowsHitTargetView();

    /// <inheritdoc/>
    protected override void ConnectHandler(WButton platformView)
    {
        base.ConnectHandler(platformView);
        if (platformView is CdsWindowsHitTargetView view && VirtualView is CdsHitTarget target)
            view.Owner = new WeakReference<CdsHitTarget>(target);
    }

    partial void StripChrome()
    {
        var b = PlatformView;
        b.Template = s_template ??= (WControlTemplate)XamlReader.Load(
            "<ControlTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' TargetType='Button'>" +
            "<Grid Background='Transparent'/></ControlTemplate>");
        b.UseSystemFocusVisuals = false;
        b.Background = new WSolidColorBrush(Microsoft.UI.Colors.Transparent);
        b.BorderThickness = new Microsoft.UI.Xaml.Thickness(0);
        b.Padding = new Microsoft.UI.Xaml.Thickness(0);
        b.MinHeight = 0;
        b.MinWidth = 0;
    }

    partial void UpdateTabStop(CdsHitTarget target) => PlatformView.IsTabStop = target.IsTabStop;

    partial void UpdateAccessibility(CdsHitTarget target)
    {
        if (PlatformView is not CdsWindowsHitTargetView view) return;
        var oldChecked = view.CheckedState;
        var oldExpanded = view.ExpandedState;
        var oldSelected = view.SelectedState;
        view.Role = target.Role;
        view.CheckedState = target.IsChecked;
        view.SelectedState = target.IsSelected;
        view.ExpandedState = target.IsExpanded;
        view.ReadOnlyState = target.IsReadOnly;
        // aria-current has no UIA property of its own; Narrator reads the item status after the name and role.
        if (CdsHitTargetStates.CurrentWord(target.CurrentKind) is { } current) WAutomationProperties.SetItemStatus(view, current);
        else view.ClearValue(WAutomationProperties.ItemStatusProperty);

        // Tell UI Automation clients about state changes of a peer that already exists.
        if (FrameworkElementAutomationPeer.FromElement(view) is not CdsHitTargetAutomationPeer peer) return;
        if (oldChecked != view.CheckedState)
            peer.RaisePropertyChangedEvent(TogglePatternIdentifiers.ToggleStateProperty, CdsHitTargetAutomationPeer.ToToggleState(oldChecked), peer.ToggleState);
        if (oldExpanded != view.ExpandedState)
            peer.RaisePropertyChangedEvent(ExpandCollapsePatternIdentifiers.ExpandCollapseStateProperty, CdsHitTargetAutomationPeer.ToExpandCollapseState(oldExpanded), peer.ExpandCollapseState);
        if (oldSelected != view.SelectedState)
            peer.RaisePropertyChangedEvent(SelectionItemPatternIdentifiers.IsSelectedProperty, oldSelected, view.SelectedState);
    }
}

/// <summary>A template-less WinUI button that reports keyboard focus and the Carbon role to UI Automation.</summary>
internal sealed partial class CdsWindowsHitTargetView : WButton
{
    internal WeakReference<CdsHitTarget>? Owner { get; set; }
    internal CdsAccessibilityRole Role { get; set; }
    internal bool? CheckedState { get; set; }
    internal bool SelectedState { get; set; }
    internal bool? ExpandedState { get; set; }
    internal bool ReadOnlyState { get; set; }

    internal CdsHitTarget? Target => Owner is not null && Owner.TryGetTarget(out var t) ? t : null;

    protected override void OnGotFocus(RoutedEventArgs e)
    {
        if (Target is { } t) t.IsFocusFromKeyboard = FocusState == FocusState.Keyboard;
        base.OnGotFocus(e);
    }

    protected override AutomationPeer OnCreateAutomationPeer() => new CdsHitTargetAutomationPeer(this);
}

internal sealed partial class CdsHitTargetAutomationPeer : ButtonAutomationPeer, IToggleProvider, IExpandCollapseProvider, ISelectionItemProvider, WValueProvider
{
    readonly CdsWindowsHitTargetView _owner;

    public CdsHitTargetAutomationPeer(CdsWindowsHitTargetView owner) : base(owner) => _owner = owner;

    bool IsToggle => _owner.Role is CdsAccessibilityRole.Checkbox or CdsAccessibilityRole.Switch or CdsAccessibilityRole.ToggleButton;

    bool IsSelectionItem => _owner.Role is CdsAccessibilityRole.Tab or CdsAccessibilityRole.Option or CdsAccessibilityRole.Radio;

    // Anything with an expanded state is a disclosure (a header action that opens a panel, a menu button).
    bool IsExpandable => _owner.ExpandedState is not null || _owner.Role is CdsAccessibilityRole.ComboBox or CdsAccessibilityRole.Expander;

    protected override AutomationControlType GetAutomationControlTypeCore() => _owner.Role switch
    {
        CdsAccessibilityRole.Checkbox => AutomationControlType.CheckBox,
        CdsAccessibilityRole.Radio => AutomationControlType.RadioButton,
        CdsAccessibilityRole.Tab => AutomationControlType.TabItem,
        CdsAccessibilityRole.MenuItem => AutomationControlType.MenuItem,
        CdsAccessibilityRole.Option => AutomationControlType.ListItem,
        CdsAccessibilityRole.ComboBox => AutomationControlType.ComboBox,
        CdsAccessibilityRole.Link => AutomationControlType.Hyperlink,
        CdsAccessibilityRole.Adjustable => AutomationControlType.Slider,
        _ => AutomationControlType.Button,
    };

    protected override object? GetPatternCore(PatternInterface patternInterface)
    {
        if (patternInterface == PatternInterface.Toggle && IsToggle) return this;
        if (patternInterface == PatternInterface.ExpandCollapse && IsExpandable) return this;
        if (patternInterface == PatternInterface.SelectionItem && IsSelectionItem) return this;
        // Read-only (aria-readonly): UIA carries it on the Value pattern, which Narrator reads as "read only".
        if (patternInterface == PatternInterface.Value && _owner.ReadOnlyState) return this;
        return base.GetPatternCore(patternInterface);
    }

    public ToggleState ToggleState => ToToggleState(_owner.CheckedState);

    internal static ToggleState ToToggleState(bool? isChecked) => isChecked switch
    {
        true => ToggleState.On,
        false => ToggleState.Off,
        null => ToggleState.Indeterminate,
    };

    internal static ExpandCollapseState ToExpandCollapseState(bool? isExpanded) =>
        isExpanded == true ? ExpandCollapseState.Expanded : ExpandCollapseState.Collapsed;

    public void Toggle() => _owner.Target?.RaiseKeyboardActivation();

    public ExpandCollapseState ExpandCollapseState => ToExpandCollapseState(_owner.ExpandedState);

    public void Expand()
    {
        if (_owner.ExpandedState != true) _owner.Target?.RaiseKeyboardActivation();
    }

    public void Collapse()
    {
        if (_owner.ExpandedState == true) _owner.Target?.RaiseKeyboardActivation();
    }

    public bool IsSelected => _owner.Role == CdsAccessibilityRole.Radio ? _owner.CheckedState == true : _owner.SelectedState;

    public IRawElementProviderSimple? SelectionContainer => null;

    public void AddToSelection() => Select();

    public void RemoveFromSelection()
    {
    }

    public void Select()
    {
        if (!IsSelected) _owner.Target?.RaiseKeyboardActivation();
    }

    bool WValueProvider.IsReadOnly => true;

    string WValueProvider.Value => CdsHitTargetStates.CheckedWord(_owner.Role, _owner.CheckedState) ?? string.Empty;

    void WValueProvider.SetValue(string value)
    {
        // Read-only: the value cannot be changed.
    }
}

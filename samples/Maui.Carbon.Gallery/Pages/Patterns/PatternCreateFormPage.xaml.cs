using System.Text.RegularExpressions;
using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Patterns;

[GalleryPage("Create form", GalleryCategory.Patterns, Order = 10, Spec = "PatternCreateForm",
    Summary = "A full-page create flow for one resource: breadcrumb, page title, grouped fields, a server error summary, inline field errors and warnings, a cost summary, and the submit row.")]
public partial class PatternCreateFormPage : GalleryPage
{
    const string NameError = "Use lowercase letters, numbers and hyphens, starting with a letter. For example, prod-eu-01.";

    static readonly Regex s_name = new("^[a-z][a-z0-9-]*$", RegexOptions.Compiled);

    static readonly Dictionary<string, string> s_regions = new()
    {
        ["us-south"] = "Dallas",
        ["us-east"] = "Washington DC",
        ["eu-de"] = "Frankfurt",
        ["eu-gb"] = "London",
        ["jp-tok"] = "Tokyo",
    };

    static readonly Dictionary<string, (string Label, double Price)> s_plans = new()
    {
        ["free"] = ("Free", 0),
        ["standard"] = ("Standard", 0.42),
        ["dedicated"] = ("Dedicated", 1.18),
    };

    readonly List<string> _errors = [];
    readonly bool _ready;

    public PatternCreateFormPage()
    {
        InitializeComponent();

        CdsTokens.SetColor(Rule, BoxView.ColorProperty, CdsColorKeys.BorderSubtle01);

        VersionDropdown.ItemsSource = new[] { "1.31.2 (default)", "1.30.6", "1.29.10" };
        VersionDropdown.SelectedItem = "1.31.2 (default)";

        // The page opens after a failed submit: the name is invalid and the summary is showing.
        NameInput.IsInvalid = true;
        NameInput.InvalidText = NameError;

        // Validate a field when it loses focus.
        var nameFocused = false;
        NameInput.StateChanged += (_, _) =>
        {
            var focused = NameInput.Is(CdsInteractionState.Focused);
            if (nameFocused && !focused) ValidateName();
            nameFocused = focused;
        };

        UpdateCost();
        UpdateNodesWarning();
        _ready = true;
    }

    string Name => (NameInput.Text ?? string.Empty).Trim();

    string Plan => PlanGroup.SelectedValue as string ?? "standard";

    int Nodes => (int)Math.Round(NodesInput.Value ?? 1);

    bool ValidateName()
    {
        var ok = s_name.IsMatch(Name);
        NameInput.IsInvalid = !ok;
        NameInput.InvalidText = string.IsNullOrEmpty(Name) ? "Enter a cluster name. For example, prod-eu-01." : NameError;
        RefreshSummary();
        return ok;
    }

    bool ValidateNodes()
    {
        var value = NodesInput.Value;
        var ok = value is >= 1 and <= 50;
        NodesInput.IsInvalid = !ok;
        RefreshSummary();
        return ok;
    }

    void RefreshSummary()
    {
        _errors.Clear();
        if (NameInput.IsInvalid) _errors.Add($"Cluster name: {NameInput.InvalidText}");
        if (NodesInput.IsInvalid) _errors.Add("Worker nodes: enter a number from 1 to 50.");

        // Remove the summary once every field is fixed; show it again only after a failed submit.
        if (_errors.Count == 0)
        {
            ErrorSummary.IsVisible = false;
            return;
        }

        if (!ErrorSummary.IsVisible) return;
        ErrorSummary.Title = _errors.Count == 1 ? "Fix 1 error before you continue." : $"Fix {_errors.Count} errors before you continue.";
        ErrorSummary.Subtitle = _errors[0];
    }

    void UpdateNodesWarning() => NodesInput.IsWarning = !NodesInput.IsInvalid && Nodes < 3;

    void UpdateCost()
    {
        var plan = s_plans[Plan];
        var region = RegionSelect.SelectedItem as string ?? "eu-de";
        var monthly = (int)Math.Round(plan.Price * Nodes * 730);
        PlanValue.Text = plan.Label;
        RegionValue.Text = s_regions.GetValueOrDefault(region, region);
        NodesValue.Text = Nodes.ToString();
        AutoscaleValue.Text = AutoscaleToggle.IsToggled ? "On" : "Off";
        TotalValue.Text = $"${monthly:N0}";
    }

    void OnNameTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (!_ready) return;
        // Typing a valid name clears the error right away; invalid input waits for blur or submit.
        if (NameInput.IsInvalid && s_name.IsMatch(Name)) ValidateName();
    }

    void OnFieldChanged(object? sender, EventArgs e)
    {
        if (_ready) UpdateCost();
    }

    void OnPlanChanged(object? sender, CdsChoiceChangedEventArgs<object> e)
    {
        if (_ready) UpdateCost();
    }

    void OnNodesChanged(object? sender, CdsNumberChangedEventArgs e)
    {
        if (!_ready) return;
        ValidateNodes();
        UpdateNodesWarning();
        UpdateCost();
    }

    void OnAutoscaleToggled(object? sender, ToggledEventArgs e)
    {
        if (_ready) UpdateCost();
    }

    void OnCreateClicked(object? sender, EventArgs e) => Form.Submit();

    void OnFormSubmitted(object? sender, EventArgs e)
    {
        var nameOk = ValidateName();
        var nodesOk = ValidateNodes();
        if (!nameOk || !nodesOk)
        {
            ErrorSummary.IsVisible = true;
            RefreshSummary();
            Status.Text = "Not submitted: fix the errors above.";
            if (!nameOk) NameInput.Focus();
            else NodesInput.Focus();
            return;
        }

        ErrorSummary.IsVisible = false;
        Status.Text = $"Create request sent for {Name} ({s_plans[Plan].Label}, {Nodes} nodes).";
    }

    void OnCancelClicked(object? sender, EventArgs e) => Status.Text = "Cancelled. Nothing was created.";
}

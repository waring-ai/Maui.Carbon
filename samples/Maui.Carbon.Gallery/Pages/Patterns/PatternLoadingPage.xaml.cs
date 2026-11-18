using Maui.Carbon.Gallery.Infrastructure;

namespace Maui.Carbon.Gallery.Pages.Patterns;

[GalleryPage("Loading", GalleryCategory.Patterns, Order = 60, Spec = "PatternLoading",
    Summary = "Which loading feedback to use and where: skeletons for the first page load, inline loading in place of the button that started an action, a progress bar for measurable work, and a blocking overlay for an action that must finish.")]
public partial class PatternLoadingPage : GalleryPage
{
    const double ButtonWidth = 196;

    bool _uploading;
    bool _creating;
    CdsButton? _createButton;
    CdsButton? _cancelButton;
    CdsInlineLoading? _createLoading;

    public PatternLoadingPage()
    {
        InitializeComponent();

        CdsTokens.SetBrush(RegionFrame, Border.StrokeProperty, CdsColorKeys.BorderSubtle01);
        CdsTokens.SetColor(Scrim, BackgroundColorProperty, CdsColorKeys.Overlay);

        SkeletonHost.Content = BuildSkeleton();
        LoadedHost.Content = BuildLoaded();

        ButtonRows.Add(BuildButtonRow("Before submitting", interactive: true));
        ButtonRows.Add(BuildButtonRow("Status Active: Cancel is disabled", CdsInlineLoadingStatus.Active, "Creating cluster..."));
        ButtonRows.Add(BuildButtonRow("Status Finished", CdsInlineLoadingStatus.Finished, "Cluster created"));
        ButtonRows.Add(BuildButtonRow("Status Error", CdsInlineLoadingStatus.Error, "Creation failed"));
    }

    // ----- skeleton and loaded panels (same layout, same sizes) -----

    static BoxView Divider()
    {
        var line = new BoxView { WidthRequest = 1 };
        CdsTokens.SetColor(line, BoxView.ColorProperty, CdsColorKeys.BorderSubtle01);
        return line;
    }

    static Grid Stats(params View[] stats)
    {
        var grid = new Grid { HeightRequest = 64, ColumnSpacing = CdsSpacing.Spacing05 };
        for (var i = 0; i < stats.Length; i++)
        {
            if (i > 0)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
                grid.Add(Divider(), grid.ColumnDefinitions.Count - 1, 0);
            }

            grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            grid.Add(stats[i], grid.ColumnDefinitions.Count - 1, 0);
        }

        return grid;
    }

    static View Stat(string label, string value) => new VerticalStackLayout
    {
        Spacing = CdsSpacing.Spacing02,
        Padding = new Thickness(0, CdsSpacing.Spacing04, 0, 0),
        Children =
        {
            new CdsText(label, CdsTypeStyle.Label01, CdsTextTone.Secondary),
            new CdsText(value, CdsTypeStyle.Heading03),
        },
    };

    static View SkeletonStat(string label) => new VerticalStackLayout
    {
        Spacing = CdsSpacing.Spacing02,
        Padding = new Thickness(0, CdsSpacing.Spacing04, 0, 0),
        Children =
        {
            new CdsText(label, CdsTypeStyle.Label01, CdsTextTone.Secondary),
            new CdsSkeletonText { IsHeading = true, WidthPercent = 60 },
        },
    };

    static View BuildSkeleton()
    {
        var table = new CdsDataTableSkeleton { ShowHeader = false, ShowToolbar = false, ColumnCount = 4, RowCount = 3, Label = "Nodes (loading)" };
        foreach (var header in new[] { "Node", "Zone", "CPU", "State" }) table.Headers.Add(header);

        var progress = new VerticalStackLayout { Spacing = CdsSpacing.Spacing03 };
        progress.Add(new CdsSkeletonText { WidthPercent = 30 });
        progress.Add(new CdsSkeletonPlaceholder { HeightRequest = 8 });
        progress.Add(new CdsSkeletonText { WidthPercent = 40 });

        var stack = new VerticalStackLayout { Spacing = CdsSpacing.Spacing05 };
        stack.Add(new CdsSkeletonText { IsHeading = true, WidthPercent = 45 });
        stack.Add(new CdsSkeletonText { IsParagraph = true, LineCount = 2 });
        stack.Add(Stats(SkeletonStat("Worker nodes"), SkeletonStat("Pods"), SkeletonStat("CPU usage")));
        stack.Add(progress);
        stack.Add(table);
        SemanticProperties.SetDescription(stack, "Loading cluster details");
        return new CdsLayer { Body = stack };
    }

    static View BuildLoaded()
    {
        var table = new CdsDataTable { Label = "Nodes", Size = CdsSize.Lg };
        table.Headers.Add(new CdsDataTableHeader { Key = "name", Header = "Node", Width = new GridLength(140) });
        table.Headers.Add(new CdsDataTableHeader { Key = "zone", Header = "Zone", Width = new GridLength(100) });
        table.Headers.Add(new CdsDataTableHeader { Key = "cpu", Header = "CPU", Width = new GridLength(70), Align = CdsTableAlignment.End });
        table.Headers.Add(new CdsDataTableHeader { Key = "state", Header = "State", Width = GridLength.Star });
        table.ItemsSource = new List<Dictionary<string, object?>>
        {
            new() { ["name"] = "kube-pool1-a1", ["zone"] = "eu-de-1", ["cpu"] = "38%", ["state"] = "Ready" },
            new() { ["name"] = "kube-pool1-b2", ["zone"] = "eu-de-2", ["cpu"] = "52%", ["state"] = "Ready" },
            new() { ["name"] = "kube-pool1-c3", ["zone"] = "eu-de-3", ["cpu"] = "47%", ["state"] = "Ready" },
        };

        var stack = new VerticalStackLayout { Spacing = CdsSpacing.Spacing05 };
        stack.Add(new CdsText("prod-eu-01", CdsTypeStyle.Heading03));
        stack.Add(new CdsText("Kubernetes 1.31.2 in Frankfurt. Created Nov 3, 2026 by the platform team.", CdsTypeStyle.Body01, CdsTextTone.Secondary));
        stack.Add(Stats(Stat("Worker nodes", "12"), Stat("Pods", "184"), Stat("CPU usage", "46%")));
        stack.Add(new CdsProgressBar { Label = "Pod capacity", Value = 184, Max = 330, HelperText = "184 of 330 pods scheduled" });
        stack.Add(table);
        return new CdsLayer { Body = stack };
    }

    async void OnReloadClicked(object? sender, EventArgs e)
    {
        LoadedHost.Content = BuildSkeleton();
        await Task.Delay(2000);
        LoadedHost.Content = BuildLoaded();
    }

    // ----- inline loading in place of the primary button -----

    View BuildButtonRow(string caption, CdsInlineLoadingStatus? status = null, string? description = null, bool interactive = false)
    {
        var cancel = new CdsButton { Text = "Cancel", Kind = CdsButtonKind.Secondary, WidthRequest = ButtonWidth, IsEnabled = status != CdsInlineLoadingStatus.Active };
        var slot = new Grid { WidthRequest = ButtonWidth, HeightRequest = CdsSizing.Large };

        var loading = new CdsInlineLoading
        {
            Status = status ?? CdsInlineLoadingStatus.Active,
            Description = description ?? string.Empty,
            SuccessDelay = 600000,
            IsVisible = status is not null,
            VerticalOptions = LayoutOptions.Center,
            Margin = new Thickness(CdsSpacing.Spacing05, 0, 0, 0),
        };
        var create = new CdsButton { Text = "Create cluster", WidthRequest = ButtonWidth, IsVisible = status is null };
        slot.Add(create);
        slot.Add(loading);

        if (interactive)
        {
            _createButton = create;
            _cancelButton = cancel;
            _createLoading = loading;
            create.Clicked += OnCreateClicked;
            cancel.Clicked += (_, _) => Status.Text = "Creation cancelled.";
        }

        var buttons = new HorizontalStackLayout { Spacing = 0, VerticalOptions = LayoutOptions.Center };
        buttons.Add(cancel);
        buttons.Add(slot);

        var row = new Grid { ColumnDefinitions = [new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto)], ColumnSpacing = CdsSpacing.Spacing05 };
        row.Add(new CdsText(caption, CdsTypeStyle.BodyCompact01, CdsTextTone.Secondary) { VerticalOptions = LayoutOptions.Center }, 0, 0);
        row.Add(buttons, 1, 0);
        return new CdsTile { Body = row };
    }

    async void OnCreateClicked(object? sender, EventArgs e)
    {
        if (_creating || _createButton is null || _cancelButton is null || _createLoading is null) return;
        _creating = true;

        _createButton.IsVisible = false;
        _cancelButton.IsEnabled = false;
        _createLoading.Description = "Creating cluster...";
        _createLoading.Status = CdsInlineLoadingStatus.Active;
        _createLoading.IsVisible = true;
        Status.Text = "Creating cluster...";

        await Task.Delay(2500);
        _createLoading.Description = "Cluster created";
        _createLoading.Status = CdsInlineLoadingStatus.Finished;
        Status.Text = "Cluster created.";

        await Task.Delay(2000);
        _createLoading.IsVisible = false;
        _createButton.IsVisible = true;
        _cancelButton.IsEnabled = true;
        _creating = false;
    }

    // ----- progress bar -----

    async void OnUploadAgainClicked(object? sender, EventArgs e)
    {
        if (_uploading) return;
        _uploading = true;

        const int total = 640;
        UploadBar.Status = CdsProgressStatus.Active;
        for (var done = 0; done <= total; done += 40)
        {
            UploadBar.Value = done;
            var left = (total - done) / 40;
            UploadBar.HelperText = done < total ? $"{done} MB of {total} MB, about {Math.Max(1, left)} seconds left" : $"{total} MB of {total} MB";
            await Task.Delay(250);
        }

        UploadBar.Status = CdsProgressStatus.Finished;
        UploadBar.HelperText = "Upload complete. 640 MB added to bucket backups-eu-de.";
        _uploading = false;
    }

    // ----- blocking overlay -----

    async void OnRestoreClicked(object? sender, EventArgs e)
    {
        if (PageLoading.IsActive) return;
        Status.Text = "Restoring snapshot...";
        PageLoading.IsActive = true;
        await Task.Delay(3000);
        PageLoading.IsActive = false;
        Status.Text = "Snapshot restored.";
    }
}

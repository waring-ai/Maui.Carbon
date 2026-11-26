using System.Text;

namespace Maui.Carbon;

// The toolbar (Carbon Charts' toolbar component): icon buttons, the overflow menu, "Show as table", fullscreen and exports.
public abstract partial class CdsChart
{
    /// <summary>Carbon Charts' "Show as table" icon (rows with bullets), which the bundled icon set lacks.</summary>
    internal static readonly CdsGlyph TableGlyph = new("chart--table", 32, new RectF(4, 4, 24, 24),
        [new CdsGlyphPath("M4 6h18v2H4zm0 6h18v2H4zm0 6h18v2H4zm0 6h18v2H4zM26 6h2v2h-2zm0 6h2v2h-2zm0 6h2v2h-2zm0 6h2v2h-2z")], []);

    internal bool ToolbarShown => IsToolbarEnabled ?? DefaultToolbarEnabled;

    void BuildToolbar()
    {
        _title.Text = Title;
        _title.IsVisible = !string.IsNullOrEmpty(Title);
        _toolbar.Clear();
        var shown = ToolbarShown && ToolbarControls != CdsChartToolbarControls.None;
        _toolbar.IsVisible = shown;
        _header.IsVisible = shown || !string.IsNullOrEmpty(Title);
        // Carbon: 15px under a header with a toolbar, 24px under a title alone.
        _headerSpacer.HeightRequest = !_header.IsVisible ? 0 : shown ? CdsSpacing.Spacing05 : CdsSpacing.Spacing06;
        if (!shown) return;
        CdsSemantics.SetGroup(_toolbar, "Chart toolbar");

        var controls = ToolbarControls;
        var icons = Math.Max(0, ToolbarIconCount - 1);
        var buttons = new List<(string Label, CdsIconName? Icon, CdsGlyph? Glyph, Func<Task> Action)>();
        var overflow = new List<(string Label, Func<Task> Action)>();
        if (controls.HasFlag(CdsChartToolbarControls.ShowAsTable)) Place("Show as table", null, TableGlyph, () => { OpenTable(); return Task.CompletedTask; });
        if (controls.HasFlag(CdsChartToolbarControls.Fullscreen))
            Place(IsFullscreen ? "Exit fullscreen" : "Make fullscreen", IsFullscreen ? CdsIconName.Minimize : CdsIconName.Maximize, null, () => { ToggleFullscreen(); return Task.CompletedTask; });
        if (controls.HasFlag(CdsChartToolbarControls.ExportCsv)) overflow.Add(("Export to CSV", () => ExportAsync(CdsChartExportFormat.Csv)));
        if (controls.HasFlag(CdsChartToolbarControls.ExportPng)) overflow.Add(("Export to PNG", () => ExportAsync(CdsChartExportFormat.Png)));
        if (controls.HasFlag(CdsChartToolbarControls.ExportJpg)) overflow.Add(("Export to JPG", () => ExportAsync(CdsChartExportFormat.Jpg)));

        foreach (var b in buttons)
        {
            var button = new CdsIconButton { Kind = CdsIconButtonKind.Ghost, Size = CdsSize.Sm, Label = b.Label, Icon = b.Icon, TooltipPlacement = CdsPlacement.Bottom };
            if (b.Glyph is not null) button.Glyph = b.Glyph;
            var action = b.Action;
            button.Clicked += async (_, _) => await action();
            _toolbar.Add(button);
        }
        if (overflow.Count > 0)
        {
            var menu = new CdsOverflowMenu { Size = CdsSize.Sm, IsFlipped = true, IconDescription = "More options" };
            foreach (var o in overflow)
            {
                var item = new CdsMenuItem { Text = o.Label };
                var action = o.Action;
                item.Clicked += async (_, _) => await action();
                menu.Items.Add(item);
            }
            _toolbar.Add(menu);
        }

        void Place(string label, CdsIconName? icon, CdsGlyph? glyph, Func<Task> action)
        {
            if (buttons.Count < icons) buttons.Add((label, icon, glyph, action));
            else overflow.Add((label, action));
        }
    }

    /// <summary>Opens "Show as table": every value in a data table, with "Download as CSV".</summary>
    public void OpenTable()
    {
        var table = RenderTable();
        var grid = new CdsDataTable { Size = CdsSize.Sm, ItemsSource = table.Rows };
        for (var i = 0; i < table.Headers.Count; i++)
        {
            var index = i;
            grid.Headers.Add(new CdsDataTableHeader { Key = "c" + i, Header = table.Headers[i], ValueSelector = row => ((string[])row)[index] });
        }
        _tableModal.Label = Title;
        _tableModal.Heading = "Tabular representation";
        _tableModal.PrimaryButtonText = "Download as CSV";
        _tableModal.Body = grid;
        _tableModal.IsOpen = true;
    }

    /// <summary>Makes the chart fill the page, or returns it ("Make fullscreen" / "Exit fullscreen"). Needs a loaded chart.</summary>
    public void ToggleFullscreen()
    {
        if (_fullscreen is { } open)
        {
            open.Close();
            return;
        }
        if (!IsLoaded) return;
        var host = new Grid { Padding = new Thickness(CdsSpacing.Spacing07), HorizontalOptions = LayoutOptions.Fill, VerticalOptions = LayoutOptions.Fill };
        CdsTokens.SetColor(host, BackgroundColorProperty, CdsColorKeys.Background);
        SetChrome(new ContentView());
        host.Add(_root);
        try
        {
            _fullscreen = CdsOverlayService.Open(host, new CdsOverlayOptions { IsModal = true, LightDismiss = false, CloseOnEscape = true, Animate = false });
        }
        catch
        {
            host.Remove(_root);
            SetChrome(_root);
            throw;
        }
        _fullscreen.Closed += (_, _) =>
        {
            _fullscreen = null;
            host.Remove(_root);
            SetChrome(_root);
            BuildToolbar();
            InvalidateMeasure();
            RequestRebuild();
        };
        BuildToolbar();
        RequestRebuild();
    }

    /// <summary>Exports the table as CSV or the chart as an image, raising <see cref="Exporting"/>; unhandled exports open the share sheet.</summary>
    public async Task ExportAsync(CdsChartExportFormat format)
    {
        var name = (string.IsNullOrWhiteSpace(ExportFileName) ? "myChart" : ExportFileName) + format switch
        {
            CdsChartExportFormat.Png => ".png",
            CdsChartExportFormat.Jpg => ".jpg",
            _ => ".csv",
        };
        byte[] bytes;
        if (format == CdsChartExportFormat.Csv)
        {
            bytes = Encoding.UTF8.GetBytes(RenderTable().ToCsv());
        }
        else
        {
            var capture = await CaptureAsync(format);
            if (capture is null) return;
            bytes = capture;
        }
        var args = new CdsChartExportingEventArgs(format, name, bytes);
        Exporting?.Invoke(this, args);
        if (args.Handled) return;
        try
        {
            var path = Path.Combine(FileSystem.CacheDirectory, name);
            await File.WriteAllBytesAsync(path, bytes);
            await Share.Default.RequestAsync(new ShareFileRequest(Title ?? name, new ShareFile(path)));
        }
        catch (Exception)
        {
            // No share sheet here (unit tests, or a platform without one): the Exporting event is the way to save.
        }
    }

    async Task<byte[]?> CaptureAsync(CdsChartExportFormat format)
    {
        // The export shows the title, legend and graph: not the toolbar, a tooltip or the focus ring.
        var toolbarOpacity = _toolbar.Opacity;
        var tooltip = _tooltip.IsVisible;
        var ring = _ring.IsVisible;
        _toolbar.Opacity = 0;
        _tooltip.IsVisible = false;
        _ring.IsVisible = false;
        try
        {
            var shot = await _root.CaptureAsync();
            if (shot is null) return null;
            await using var stream = await shot.OpenReadAsync(format == CdsChartExportFormat.Jpg ? ScreenshotFormat.Jpeg : ScreenshotFormat.Png, 95);
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            return ms.ToArray();
        }
        catch (Exception)
        {
            return null;
        }
        finally
        {
            _toolbar.Opacity = toolbarOpacity;
            _tooltip.IsVisible = tooltip;
            _ring.IsVisible = ring;
        }
    }
}

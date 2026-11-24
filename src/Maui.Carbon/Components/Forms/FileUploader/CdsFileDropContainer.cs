using Microsoft.Maui.Controls.Shapes;

namespace Maui.Carbon;

/// <summary>A file handed to the uploader by the picker or a drop: its full path and the platform access held for it.</summary>
internal sealed record CdsFileGrant(string Path, IDisposable? Access);

/// <summary>
/// The dashed drop target of a file uploader: 96 high, a 1px dashed <c>border-strong-01</c> box with <c>link-primary</c>
/// text. It is a real button (click, Enter or Space open the picker); dragging files over it draws a 2px <c>focus</c>
/// outline. Dropped files are collected best-effort per platform: Mac Catalyst and iOS hand over the original file (its
/// security-scoped access held until the uploader lets the file go), or a copy when the source only offers one; Windows
/// hands over storage-item paths; Android offers no files.
/// </summary>
internal sealed class CdsFileDropContainer : CdsControl
{
    readonly Grid _root = new() { HeightRequest = 96 };
    readonly Border _box = new() { StrokeThickness = 1, StrokeShape = new Rectangle(), StrokeDashArray = [3, 3], Padding = new Thickness(CdsSpacing.Spacing05), InputTransparent = true };
    readonly CdsText _label = new() { LineBreakMode = LineBreakMode.WordWrap, VerticalOptions = LayoutOptions.Center, HorizontalTextAlignment = TextAlignment.Center };
    readonly CdsFocusRing _ring = new(CdsFocusRingStyle.Inset);
    readonly CdsHitTarget _hit;
    bool _dragOver;

    public CdsFileDropContainer()
    {
        _box.Content = _label;
        AutomationProperties.SetExcludedWithChildren(_box, true);
        _hit = CreateHitTarget();
        _root.Add(_box);
        _root.Add(_ring);
        _root.Add(_hit);

        var drop = new DropGestureRecognizer { AllowDrop = true };
        drop.DragOver += (_, e) =>
        {
            if (!IsEnabled) return;
            e.AcceptedOperation = DataPackageOperation.Copy;
            SetDragOver(true);
        };
        drop.DragLeave += (_, _) => SetDragOver(false);
        drop.Drop += async (_, e) =>
        {
            SetDragOver(false);
            if (!IsEnabled) return;
            // Handled before the first await: otherwise MAUI goes on to read the drop as its own text or image package,
            // which a drop from another app does not have (it throws and logs a warning).
            e.Handled = true;
            var files = await CollectFilesAsync(e);
            if (files.Count > 0) FilesDropped?.Invoke(this, files);
        };
        _root.GestureRecognizers.Add(drop);

        SetChrome(_root);
        ApplyVisuals();
    }

    /// <summary>Raised when the container is activated: open the picker.</summary>
    public event EventHandler? Activated;

    /// <summary>Raised with the files dropped on the container.</summary>
    public event EventHandler<IReadOnlyList<CdsFileGrant>>? FilesDropped;

    public string? Text
    {
        get => _label.Text;
        set
        {
            _label.Text = value;
            SemanticProperties.SetDescription(_hit, value);
        }
    }

    protected override void OnActivated() => Activated?.Invoke(this, EventArgs.Empty);

    protected override void ApplyVisuals()
    {
        var disabled = Is(CdsInteractionState.Disabled);
        SetBrush(_box, Border.StrokeProperty, disabled ? CdsColorKeys.ButtonDisabled : CdsColorKeys.BorderStrong01);
        SetColor(_label, Microsoft.Maui.Controls.Label.TextColorProperty,
            disabled ? CdsColorKeys.TextDisabled : Is(CdsInteractionState.Pressed) ? CdsColorKeys.TextPrimary : CdsColorKeys.LinkPrimary);
        _label.TextDecorations = !disabled && (Is(CdsInteractionState.Hover) || Is(CdsInteractionState.FocusVisible) || Is(CdsInteractionState.Pressed))
            ? TextDecorations.Underline : TextDecorations.None;
        // Focus and drag-over both draw the 2px focus outline.
        _ring.IsActive = !disabled && (_dragOver || Is(CdsInteractionState.FocusVisible));
    }

    void SetDragOver(bool value)
    {
        if (_dragOver == value) return;
        _dragOver = value;
        RefreshVisuals();
    }

    static async Task<IReadOnlyList<CdsFileGrant>> CollectFilesAsync(DropEventArgs e)
    {
        var files = new List<CdsFileGrant>();
#if WINDOWS
        try
        {
            var view = e.PlatformArgs?.DragEventArgs?.DataView;
            if (view is not null && view.Contains(global::Windows.ApplicationModel.DataTransfer.StandardDataFormats.StorageItems))
            {
                var items = await view.GetStorageItemsAsync();
                foreach (var item in items)
                {
                    if (!string.IsNullOrEmpty(item.Path)) files.Add(new CdsFileGrant(item.Path, null));
                }
            }
        }
        catch (Exception)
        {
            // Best effort: a drop the platform will not hand over is ignored.
        }
#elif IOS || MACCATALYST
        try
        {
            if (e.PlatformArgs?.DropSession is { } session)
            {
                // Every item is requested now, while UIKit performs the drop: it serves item data only to requests made
                // then, so loading one item after another would lose all but the first.
                var loads = session.Items.Select(item => CdsDocumentPicker.LoadDroppedFileAsync(item.ItemProvider)).ToArray();
                foreach (var file in await Task.WhenAll(loads))
                {
                    if (file is not null) files.Add(file);
                }
            }
        }
        catch (Exception)
        {
            // Best effort: a drop the platform will not hand over is ignored.
        }
#else
        // Android and plain .NET: no cross-platform way to read dropped files.
        await Task.CompletedTask;
        _ = e;
#endif
        return files;
    }
}

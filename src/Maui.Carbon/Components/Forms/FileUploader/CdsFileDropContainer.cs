using Microsoft.Maui.Controls.Shapes;

namespace Maui.Carbon;

/// <summary>
/// The dashed drop target of a file uploader: 96 high, a 1px dashed <c>border-strong-01</c> box with <c>link-primary</c>
/// text. It is a real button (click, Enter or Space open the picker); dragging files over it draws a 2px <c>focus</c>
/// outline. Dropped files are collected best-effort per platform (Mac Catalyst, iOS and Windows hand over file paths).
/// </summary>
internal sealed class CdsFileDropContainer : CdsControl
{
    readonly Grid _root = new() { HeightRequest = 96 };
    readonly Border _box = new() { StrokeThickness = 1, StrokeShape = new Rectangle(), StrokeDashArray = [3, 3], Padding = new Thickness(CdsSpacing.Spacing05), InputTransparent = true };
    readonly CdsText _label = new() { LineBreakMode = LineBreakMode.WordWrap, VerticalOptions = LayoutOptions.Start };
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
            var paths = await CollectPathsAsync(e);
            if (paths.Count > 0) FilesDropped?.Invoke(this, paths);
        };
        _root.GestureRecognizers.Add(drop);

        SetChrome(_root);
        ApplyVisuals();
    }

    /// <summary>Raised when the container is activated: open the picker.</summary>
    public event EventHandler? Activated;

    /// <summary>Raised with the paths of files dropped on the container.</summary>
    public event EventHandler<IReadOnlyList<string>>? FilesDropped;

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

    static async Task<IReadOnlyList<string>> CollectPathsAsync(DropEventArgs e)
    {
        var paths = new List<string>();
#if WINDOWS
        try
        {
            var view = e.PlatformArgs?.DragEventArgs?.DataView;
            if (view is not null && view.Contains(global::Windows.ApplicationModel.DataTransfer.StandardDataFormats.StorageItems))
            {
                var items = await view.GetStorageItemsAsync();
                foreach (var item in items)
                {
                    if (!string.IsNullOrEmpty(item.Path)) paths.Add(item.Path);
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
            var session = e.PlatformArgs?.DropSession;
            if (session is not null)
            {
                foreach (var item in session.Items)
                {
                    var tcs = new TaskCompletionSource<string?>();
                    item.ItemProvider.LoadItem("public.file-url", null, (data, _) =>
                    {
                        var url = data switch
                        {
                            Foundation.NSUrl u => u,
                            Foundation.NSData d => Foundation.NSUrl.FromString(new Foundation.NSString(d, Foundation.NSStringEncoding.UTF8)),
                            _ => null,
                        };
                        tcs.TrySetResult(url?.Path);
                    });
                    var path = await tcs.Task;
                    if (!string.IsNullOrEmpty(path)) paths.Add(path);
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
        return paths;
    }
}

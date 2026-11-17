namespace Maui.Carbon;

/// <summary>
/// The overflow list of a breadcrumb: a <c>layer-01</c> surface with a shadow and one 32 high row per collapsed level.
/// Internal; opened with <see cref="CdsOverlayService"/> and driven with virtual focus (focus stays on the "…" trigger).
/// </summary>
internal sealed class CdsBreadcrumbMenu : Border
{
    readonly List<Row> _rows = [];
    readonly Action<CdsBreadcrumbItem> _choose;
    int _highlighted = -1;

    public CdsBreadcrumbMenu(IReadOnlyList<CdsBreadcrumbItem> items, Action<CdsBreadcrumbItem> choose)
    {
        _choose = choose;
        StrokeThickness = 0;
        Padding = 0;
        MinimumWidthRequest = 160;
        MaximumWidthRequest = 288;
        CdsTokens.SetColor(this, BackgroundColorProperty, CdsColorKeys.Layer01);
        CdsTokens.SetShadow(this, CdsColorKeys.Shadow);
        SemanticProperties.SetDescription(this, "Breadcrumb levels");

        var stack = new VerticalStackLayout { Spacing = 0 };
        for (var i = 0; i < items.Count; i++)
        {
            var row = new Row(items[i], i, this);
            _rows.Add(row);
            stack.Add(row);
        }
        Content = stack;
    }

    /// <summary>Keys while the menu is open: Up and Down move the highlight (wrapping), Home and End jump, Enter and Space choose.</summary>
    public bool HandleKey(CdsKeyEventArgs e)
    {
        var enabled = _rows.Select(r => r.Item.IsEnabled).ToArray();
        switch (e.Key)
        {
            case CdsKey.Down or CdsKey.Up or CdsKey.Home or CdsKey.End:
                var target = TabsModel.Target(e.Key, enabled, _highlighted, vertical: true);
                if (target < 0) return false;
                SetHighlight(target);
                return true;
            case CdsKey.Enter or CdsKey.Space when _highlighted >= 0:
                Choose(_highlighted);
                return true;
            default:
                return false;
        }
    }

    void SetHighlight(int index)
    {
        _highlighted = index;
        for (var i = 0; i < _rows.Count; i++) _rows[i].IsHighlighted = i == index;
    }

    void Choose(int index) => _choose(_rows[index].Item);

    sealed class Row : CdsControl
    {
        readonly CdsBreadcrumbMenu _menu;
        readonly Grid _root = new() { HeightRequest = CdsSizing.Small };
        readonly BoxView _fill = new() { InputTransparent = true };
        readonly CdsText _label = new() { LineBreakMode = LineBreakMode.TailTruncation, MaxLines = 1, VerticalOptions = LayoutOptions.Center, InputTransparent = true, Margin = new Thickness(CdsSpacing.Spacing05, 0) };
        readonly CdsHitTarget _hit;
        readonly int _index;
        bool _highlighted;

        public Row(CdsBreadcrumbItem item, int index, CdsBreadcrumbMenu menu)
        {
            Item = item;
            _index = index;
            _menu = menu;
            _label.Text = item.Text;
            AutomationProperties.SetExcludedWithChildren(_label, true);
            _hit = CreateHitTarget(CdsAccessibilityRole.MenuItem);
            SemanticProperties.SetDescription(_hit, item.Text);
            IsEnabled = item.IsEnabled;
            _root.Add(_fill);
            _root.Add(_label);
            _root.Add(_hit);
            SetChrome(_root);
        }

        public CdsBreadcrumbItem Item { get; }

        public bool IsHighlighted
        {
            get => _highlighted;
            set
            {
                _highlighted = value;
                RefreshVisuals();
            }
        }

        protected override void OnActivated() => _menu.Choose(_index);

        protected override void ApplyVisuals()
        {
            var active = _highlighted || Is(CdsInteractionState.Hover) || Is(CdsInteractionState.Pressed);
            var disabled = Is(CdsInteractionState.Disabled);
            SetColor(_fill, BoxView.ColorProperty, active && !disabled ? CdsColorKeys.LayerHover01 : null);
            SetColor(_label, Label.TextColorProperty, disabled ? CdsColorKeys.TextDisabled : active ? CdsColorKeys.TextPrimary : CdsColorKeys.TextSecondary);
        }
    }
}

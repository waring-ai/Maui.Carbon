using Microsoft.Maui.Controls.Shapes;
using MauiImage = Microsoft.Maui.Controls.Image;

namespace Maui.Carbon;

/// <summary>
/// A circle that stands for a person (Carbon for IBM Products' UserAvatar): a photo, an icon, or the initials of
/// <see cref="Name"/> on one of twelve background colors (<see cref="AvatarColor"/>), with an optional hover and focus
/// tooltip. Precedence: <see cref="Image"/>, then <see cref="Icon"/>, then initials, then the default <c>user</c> icon.
/// Use it for owners, authors and assignees; give the people in one view different colors. A photo needs
/// <see cref="ImageDescription"/>; with <see cref="TooltipText"/> the avatar becomes a focusable trigger named by it.
/// </summary>
public class CdsUserAvatar : CdsControl
{
    /// <summary>Bindable property for <see cref="Name"/>.</summary>
    public static readonly BindableProperty NameProperty = BindableProperty.Create(
        nameof(Name), typeof(string), typeof(CdsUserAvatar), null, propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="Image"/>.</summary>
    public static readonly BindableProperty ImageProperty = BindableProperty.Create(
        nameof(Image), typeof(ImageSource), typeof(CdsUserAvatar), null, propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="ImageDescription"/>.</summary>
    public static readonly BindableProperty ImageDescriptionProperty = BindableProperty.Create(
        nameof(ImageDescription), typeof(string), typeof(CdsUserAvatar), null, propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="Icon"/>.</summary>
    public static readonly BindableProperty IconProperty = BindableProperty.Create(
        nameof(Icon), typeof(CdsIconName?), typeof(CdsUserAvatar), null, propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="Size"/>.</summary>
    public static readonly BindableProperty SizeProperty = BindableProperty.Create(
        nameof(Size), typeof(CdsSize), typeof(CdsUserAvatar), CdsSize.Md, propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="AvatarColor"/>.</summary>
    public static readonly BindableProperty AvatarColorProperty = BindableProperty.Create(
        nameof(AvatarColor), typeof(CdsUserAvatarColor), typeof(CdsUserAvatar), CdsUserAvatarColor.Order1Cyan,
        propertyChanged: static (b, _, _) => ((CdsUserAvatar)b).RefreshVisuals());

    /// <summary>Bindable property for <see cref="TooltipText"/>.</summary>
    public static readonly BindableProperty TooltipTextProperty = BindableProperty.Create(
        nameof(TooltipText), typeof(string), typeof(CdsUserAvatar), null, propertyChanged: OnChanged);

    /// <summary>Bindable property for <see cref="TooltipPlacement"/>.</summary>
    public static readonly BindableProperty TooltipPlacementProperty = BindableProperty.Create(
        nameof(TooltipPlacement), typeof(CdsPlacement), typeof(CdsUserAvatar), CdsPlacement.Bottom, propertyChanged: OnChanged);

    readonly ContentView _host = new() { HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Start };
    readonly Border _circle = new() { StrokeThickness = 0, Padding = 0, HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Start };
    readonly Grid _inner = new();
    readonly CdsText _initials = new()
    {
        HorizontalOptions = LayoutOptions.Center,
        VerticalOptions = LayoutOptions.Center,
        HorizontalTextAlignment = TextAlignment.Center,
        VerticalTextAlignment = TextAlignment.Center,
        LineBreakMode = LineBreakMode.NoWrap,
        MaxLines = 1,
    };
    readonly CdsIcon _icon = new();
    MauiImage? _photo;
    CdsUserAvatarTrigger? _trigger;
    CdsTooltip? _tooltip;

    /// <summary>Creates a medium avatar showing the default <c>user</c> icon.</summary>
    public CdsUserAvatar()
    {
        HorizontalOptions = LayoutOptions.Start;
        VerticalOptions = LayoutOptions.Center;
        _inner.Add(_initials);
        _inner.Add(_icon);
        _circle.Content = _inner;
        _host.Content = _circle;
        SetChrome(_host);
        Update();
    }

    /// <summary>Creates an avatar with the initials of a name.</summary>
    public CdsUserAvatar(string name, CdsUserAvatarColor color = CdsUserAvatarColor.Order1Cyan) : this()
    {
        Name = name;
        AvatarColor = color;
    }

    /// <summary>The person's full name or initials; the avatar shows the initials ("Thomas J. Watson" → "TW").</summary>
    public string? Name
    {
        get => (string?)GetValue(NameProperty);
        set => SetValue(NameProperty, value);
    }

    /// <summary>A photo, clipped to the circle. Takes precedence over the icon and the initials; set <see cref="ImageDescription"/>.</summary>
    public ImageSource? Image
    {
        get => (ImageSource?)GetValue(ImageProperty);
        set => SetValue(ImageProperty, value);
    }

    /// <summary>The photo's description for screen readers ("Avatar of Thomas J. Watson"). Required with <see cref="Image"/>.</summary>
    public string? ImageDescription
    {
        get => (string?)GetValue(ImageDescriptionProperty);
        set => SetValue(ImageDescriptionProperty, value);
    }

    /// <summary>An icon in place of the initials (<c>Group</c> for a team, <c>User</c> for an unknown person).</summary>
    public CdsIconName? Icon
    {
        get => (CdsIconName?)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>Diameter: <see cref="CdsSize.Sm"/> 24, Md 32 (default), Lg 48, Xl 64 (Xs draws 24, Xxl 64).</summary>
    public CdsSize Size
    {
        get => (CdsSize)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    /// <summary>The background color (<c>backgroundColor</c>). Default <see cref="CdsUserAvatarColor.Order1Cyan"/>.</summary>
    public CdsUserAvatarColor AvatarColor
    {
        get => (CdsUserAvatarColor)GetValue(AvatarColorProperty);
        set => SetValue(AvatarColorProperty, value);
    }

    /// <summary>
    /// The person's display name, shown on hover and keyboard focus ("TW, Thomas J. Watson user profile"). Makes the
    /// avatar a focusable trigger whose accessible name is this text.
    /// </summary>
    public string? TooltipText
    {
        get => (string?)GetValue(TooltipTextProperty);
        set => SetValue(TooltipTextProperty, value);
    }

    /// <summary>Where the tooltip opens (<c>tooltipAlignment</c>). Default <see cref="CdsPlacement.Bottom"/>; flips when it does not fit.</summary>
    public CdsPlacement TooltipPlacement
    {
        get => (CdsPlacement)GetValue(TooltipPlacementProperty);
        set => SetValue(TooltipPlacementProperty, value);
    }

    /// <summary>What the avatar draws now.</summary>
    public CdsUserAvatarContent CurrentContent => UserAvatarModel.Content(Image is not null, Icon is not null, Name);

    /// <summary>The initials shown now ("TW"), or <c>null</c> when the avatar shows a photo or an icon.</summary>
    public string? CurrentInitials => CurrentContent == CdsUserAvatarContent.Initials ? UserAvatarModel.Initials(Name) : null;

    /// <inheritdoc/>
    protected override void ApplyVisuals()
    {
        SetColor(_circle, BackgroundColorProperty, CdsUserAvatarTokens.Background(AvatarColor, ContextTheme.IsDark()));
        SetColor(_initials, Label.TextColorProperty, CdsUserAvatarTokens.Foreground);
        SetColor(_icon, CdsIcon.ColorProperty, CdsUserAvatarTokens.IconColor);
    }

    /// <inheritdoc/>
    protected override void OnContextChanged() => RefreshVisuals();

    void Update()
    {
        var diameter = CdsUserAvatarTokens.Diameter(Size);
        _circle.WidthRequest = diameter;
        _circle.HeightRequest = diameter;
        _circle.StrokeShape = new RoundRectangle { CornerRadius = CdsRadius.Circle(diameter) };

        var content = CurrentContent;
        _initials.IsVisible = content == CdsUserAvatarContent.Initials;
        _initials.Text = content == CdsUserAvatarContent.Initials ? UserAvatarModel.Initials(Name) : null;
        _initials.TypeStyle = CdsUserAvatarTokens.TypeStyle(Size);

        _icon.IsVisible = content is CdsUserAvatarContent.Icon or CdsUserAvatarContent.DefaultIcon;
        _icon.Icon = content == CdsUserAvatarContent.Icon ? Icon : CdsIconName.User;
        _icon.Size = CdsUserAvatarTokens.IconSize(Size);

        if (content == CdsUserAvatarContent.Image)
        {
            if (_photo is null)
            {
                _photo = new MauiImage { Aspect = Aspect.AspectFill, HorizontalOptions = LayoutOptions.Fill, VerticalOptions = LayoutOptions.Fill };
                _inner.Add(_photo);
            }
            _photo.Source = Image;
            _photo.IsVisible = true;
            AutomationProperties.SetIsInAccessibleTree(_photo, !string.IsNullOrEmpty(ImageDescription));
            SemanticProperties.SetDescription(_photo, ImageDescription);
        }
        else if (_photo is not null)
        {
            _photo.Source = null;
            _photo.IsVisible = false;
        }

        UpdateTooltip(diameter);
        RefreshVisuals();
    }

    void UpdateTooltip(double diameter)
    {
        var text = TooltipText;
        if (string.IsNullOrEmpty(text))
        {
            if (_trigger is null || _host.Content == _circle) return;
            _host.Content = null;
            _trigger.Detach(_circle);
            _host.Content = _circle;
            return;
        }

        if (_trigger is null)
        {
            _trigger = new CdsUserAvatarTrigger();
            _tooltip = new CdsTooltip { Body = _trigger };
        }
        _trigger.SetDiameter(diameter);
        if (_host.Content != _tooltip)
        {
            _host.Content = null;
            _trigger.Attach(_circle);
            _host.Content = _tooltip;
        }
        _tooltip!.Placement = TooltipPlacement;
        _tooltip.Text = text;
    }

    static void OnChanged(BindableObject b, object? o, object? n) => ((CdsUserAvatar)b).Update();
}

/// <summary>The focusable trigger an avatar with a tooltip sits in: the face, a round focus ring and a hit target.</summary>
internal sealed class CdsUserAvatarTrigger : CdsControl
{
    readonly Grid _grid = new() { HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Start };
    readonly CdsHitTarget _hit;
    CdsFocusRing _ring = new(CdsFocusRingStyle.Offset);
    double _diameter = -1;

    public CdsUserAvatarTrigger()
    {
        HorizontalOptions = LayoutOptions.Start;
        VerticalOptions = LayoutOptions.Start;
        _hit = CreateHitTarget();
        _grid.Add(_ring);
        _grid.Add(_hit);
        SetChrome(_grid);
    }

    /// <summary>The face currently shown.</summary>
    public View? Face { get; private set; }

    public void Attach(View face)
    {
        Face = face;
        face.InputTransparent = true;
        AutomationProperties.SetExcludedWithChildren(face, true);
        _grid.Insert(0, face);
    }

    public void Detach(View face)
    {
        _grid.Remove(face);
        face.InputTransparent = false;
        AutomationProperties.SetExcludedWithChildren(face, false);
        Face = null;
    }

    /// <summary>Recreates the ring for a new circle size (the ring's radius is fixed when it is built).</summary>
    public void SetDiameter(double diameter)
    {
        if (diameter == _diameter) return;
        _diameter = diameter;
        var index = _grid.IndexOf(_ring);
        _grid.Remove(_ring);
        _ring = new CdsFocusRing(CdsFocusRingStyle.Offset, CdsRadius.Circle(diameter));
        _grid.Insert(Math.Max(0, index), _ring);
        RefreshVisuals();
    }

    protected override void ApplyVisuals() => _ring.IsActive = Is(CdsInteractionState.FocusVisible);
}

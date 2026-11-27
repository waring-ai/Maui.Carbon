namespace Maui.Carbon;

public partial class CdsAboutModal
{
    /// <summary>Bindable property for <see cref="Title"/>.</summary>
    public static readonly BindableProperty TitleProperty = BindableProperty.Create(
        nameof(Title), typeof(string), typeof(CdsAboutModal), null,
        propertyChanged: static (b, _, _) => ((CdsAboutModal)b).Sync());

    /// <summary>Bindable property for <see cref="Version"/>.</summary>
    public static readonly BindableProperty VersionProperty = BindableProperty.Create(
        nameof(Version), typeof(string), typeof(CdsAboutModal), null,
        propertyChanged: static (b, _, _) => ((CdsAboutModal)b).Sync());

    /// <summary>Bindable property for <see cref="Logo"/>.</summary>
    public static readonly BindableProperty LogoProperty = BindableProperty.Create(
        nameof(Logo), typeof(View), typeof(CdsAboutModal), null,
        propertyChanged: static (b, _, _) => ((CdsAboutModal)b).Sync());

    /// <summary>Bindable property for <see cref="LegalText"/>.</summary>
    public static readonly BindableProperty LegalTextProperty = BindableProperty.Create(
        nameof(LegalText), typeof(string), typeof(CdsAboutModal), null,
        propertyChanged: static (b, _, _) => ((CdsAboutModal)b).Sync());

    /// <summary>Bindable property for <see cref="CopyrightText"/>.</summary>
    public static readonly BindableProperty CopyrightTextProperty = BindableProperty.Create(
        nameof(CopyrightText), typeof(string), typeof(CdsAboutModal), null,
        propertyChanged: static (b, _, _) => ((CdsAboutModal)b).Sync());

    /// <summary>Bindable property for <see cref="AdditionalInfo"/>.</summary>
    public static readonly BindableProperty AdditionalInfoProperty = BindableProperty.Create(
        nameof(AdditionalInfo), typeof(View), typeof(CdsAboutModal), null,
        propertyChanged: static (b, _, _) => ((CdsAboutModal)b).Sync());

    /// <summary>Bindable property for <see cref="CloseIconDescription"/>.</summary>
    public static readonly BindableProperty CloseIconDescriptionProperty = BindableProperty.Create(
        nameof(CloseIconDescription), typeof(string), typeof(CdsAboutModal), "Close",
        propertyChanged: static (b, _, _) => ((CdsAboutModal)b).Sync());

    /// <summary>The product name (<c>title</c>), shown in <c>heading-04</c> under the logo: "IBM Product name". It also names the dialog unless <c>SemanticProperties.Description</c> is set (<c>modalAriaLabel</c>).</summary>
    public string? Title
    {
        get => (string?)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>The product version (<c>version</c>), in <c>text-secondary</c>: "Version 1.3.41".</summary>
    public string? Version
    {
        get => (string?)GetValue(VersionProperty);
        set => SetValue(VersionProperty, value);
    }

    /// <summary>The product logo (<c>logo</c>), above the product name; about 96 wide (an <see cref="Image"/> with <c>WidthRequest="96"</c>).</summary>
    public View? Logo
    {
        get => (View?)GetValue(LogoProperty);
        set => SetValue(LogoProperty, value);
    }

    /// <summary>Disclaimers or legal information (<c>content</c>), in <c>label-01</c> <c>text-secondary</c>.</summary>
    public string? LegalText
    {
        get => (string?)GetValue(LegalTextProperty);
        set => SetValue(LegalTextProperty, value);
    }

    /// <summary>The copyright line (<c>copyrightText</c>) from the first release year to the current one: "Copyright © 2018, 2026 IBM Corp."</summary>
    public string? CopyrightText
    {
        get => (string?)GetValue(CopyrightTextProperty);
        set => SetValue(CopyrightTextProperty, value);
    }

    /// <summary>Extra detail in a <c>layer-02</c> footer (<c>additionalInfo</c>), such as the logos of technologies the product is built on (legally required for some products) or tabs of third-party notices. Without it the modal is passive and a tap outside closes it.</summary>
    public View? AdditionalInfo
    {
        get => (View?)GetValue(AdditionalInfoProperty);
        set => SetValue(AdditionalInfoProperty, value);
    }

    /// <summary>Accessible name of the close button (<c>closeIconDescription</c>). Default <c>Close</c>.</summary>
    public string CloseIconDescription
    {
        get => (string)GetValue(CloseIconDescriptionProperty);
        set => SetValue(CloseIconDescriptionProperty, value);
    }
}

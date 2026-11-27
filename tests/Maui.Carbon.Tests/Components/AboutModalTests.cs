namespace Maui.Carbon.Tests.Components;

public class AboutModalTests
{
    [Fact]
    public void Spacing_follows_what_comes_before()
    {
        Assert.Equal((24d, 24d, 16d), AboutModalModel.Spacing(false, false));
        Assert.Equal((24d, 24d, 16d), AboutModalModel.Spacing(false, true));
        Assert.Equal((24d, 16d, 16d), AboutModalModel.Spacing(true, true));
        Assert.Equal((24d, 16d, 8d), AboutModalModel.Spacing(true, false));
    }

    [Fact]
    public void Constructs_and_takes_every_property()
    {
        var modal = new CdsAboutModal
        {
            Title = "IBM Product name",
            Version = "Version 1.3.41",
            Logo = new BoxView { WidthRequest = 96, HeightRequest = 96 },
            LegalText = "This Web site contains proprietary notices and copyright information.",
            CopyrightText = "Copyright © 2020, 2026 IBM Corp.",
            AdditionalInfo = new CdsText("Powered by"),
            CloseIconDescription = "Close",
        };
        modal.Links.Add(new CdsLink("Link action"));
        modal.Links.Add(new CdsLink("Link action"));
        modal.IsInline = true;
        Assert.Equal("IBM Product name", modal.Header.Title);
        Assert.NotNull(modal.Header.Logo);
        Assert.True(modal.Footer!.HasContent);
        Assert.True(modal.Header.ShowsCloseButton);
    }

    [Fact]
    public void Without_additional_info_the_modal_is_passive()
    {
        var modal = new CdsAboutModal { Title = "IBM Product name", Version = "Version 1.3.41" };
        Assert.False(modal.Footer!.HasContent);
        Assert.True(CdsComposedModal.ClosesOnScrimTap(modal.Footer, modal.Modal.PreventCloseOnClickOutside));
        modal.AdditionalInfo = new CdsText("Powered by");
        Assert.True(modal.Footer.HasContent);
        Assert.False(CdsComposedModal.ClosesOnScrimTap(modal.Footer, modal.Modal.PreventCloseOnClickOutside));
        modal.AdditionalInfo = null;
        Assert.False(modal.Footer.HasContent);
    }

    [Fact]
    public void Links_are_split_by_dividers()
    {
        var modal = new CdsAboutModal();
        modal.Links.Add(new CdsLink("Release notes"));
        modal.Links.Add(new CdsLink("Support"));
        modal.Links.Add(new CdsLink("Privacy"));
        var row = modal.Links[0].Parent as HorizontalStackLayout;
        Assert.NotNull(row);
        Assert.Equal(5, row!.Count);
        Assert.IsType<BoxView>(row[1]);
        modal.Links.Clear();
        Assert.Empty(row);
    }

    [Fact]
    public void Focus_starts_on_the_close_button_and_close_asks_to_close()
    {
        var page = new VerticalStackLayout();
        _ = new CdsOverlayHost { Body = page };
        var modal = new CdsAboutModal { Title = "IBM Product name" };
        var requests = 0;
        modal.RequestClose += (_, _) => requests++;
        page.Add(modal);
        modal.IsOpen = true;
        Assert.Same(modal.Header.CloseTarget, modal.Modal.InitialFocusElement);
        modal.Modal.Show();
        ((IButton)modal.Header.CloseTarget).Clicked();
        Assert.Equal(1, requests);
        Assert.False(modal.IsOpen);
    }
}

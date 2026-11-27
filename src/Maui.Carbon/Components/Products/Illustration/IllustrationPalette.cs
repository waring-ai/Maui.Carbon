using Microsoft.Maui.Graphics;

namespace Maui.Carbon;

/// <summary>
/// The fixed artwork colors of the ported Carbon for IBM Products illustrations, as 0xRRGGBB. These are illustration
/// colors, not UI tokens: they come from the SVG sources of @carbon/ibm-products 2.104.0-rc.0 (commit
/// e5915cc933cc7b00b6a66cf2f616ea2772f294c1): <c>EmptyStates/assets/*Illustration.jsx</c>,
/// <c>HTTPErrors/assets/HTTPErrorSvg*.jsx</c> and the fills in <c>ibm-products-styles/.../HTTPErrors/_http-errors.scss</c>.
/// Values that match the Carbon palette carry its name; the others (gradient steps, the cable's blue ramp) carry their
/// value. This file is the one place in the library with literal colors, because the artwork is not themeable: the
/// empty state illustrations ship a light and a dark variant instead. The FullPageError drawings use theme tokens.
/// </summary>
internal static class IllustrationPalette
{
    internal const uint Black = 0x000000;
    internal const uint Art002B50 = 0x002B50;
    internal const uint Art003D73 = 0x003D73;
    internal const uint Art0058A1 = 0x0058A1;
    internal const uint Art0062FF = 0x0062FF;
    internal const uint Art0565FF = 0x0565FF;
    internal const uint Art07192B = 0x07192B;
    internal const uint Art0A69FF = 0x0A69FF;
    internal const uint Art106CFF = 0x106CFF;
    internal const uint Art156FFF = 0x156FFF;
    internal const uint Gray100 = 0x161616;
    internal const uint Art171717 = 0x171717;
    internal const uint Art1A72FF = 0x1A72FF;
    internal const uint Art1F76FF = 0x1F76FF;
    internal const uint Art2479FF = 0x2479FF;
    internal const uint Gray90 = 0x262626;
    internal const uint Art282828 = 0x282828;
    internal const uint Art297CFF = 0x297CFF;
    internal const uint Art2B2828 = 0x2B2828;
    internal const uint Art2F7FFF = 0x2F7FFF;
    internal const uint Art333333 = 0x333333;
    internal const uint Art3483FF = 0x3483FF;
    internal const uint Art373D42 = 0x373D42;
    internal const uint Gray80 = 0x393939;
    internal const uint Art3986FF = 0x3986FF;
    internal const uint Art3E89FF = 0x3E89FF;
    internal const uint Art3F3F3F = 0x3F3F3F;
    internal const uint Art404040 = 0x404040;
    internal const uint Art413D3D = 0x413D3D;
    internal const uint Art438CFF = 0x438CFF;
    internal const uint Art4890FF = 0x4890FF;
    internal const uint Art4A4A4A = 0x4A4A4A;
    internal const uint Art4D4D4D = 0x4D4D4D;
    internal const uint Art4E93FF = 0x4E93FF;
    internal const uint Art51565C = 0x51565C;
    internal const uint Gray70 = 0x525252;
    internal const uint Art5396FF = 0x5396FF;
    internal const uint Art545454 = 0x545454;
    internal const uint Art565656 = 0x565656;
    internal const uint Art575757 = 0x575757;
    internal const uint Art5899FF = 0x5899FF;
    internal const uint Art595655 = 0x595655;
    internal const uint Art5C5C5C = 0x5C5C5C;
    internal const uint Art5D9DFF = 0x5D9DFF;
    internal const uint Art62A0FF = 0x62A0FF;
    internal const uint Art636363 = 0x636363;
    internal const uint Art686868 = 0x686868;
    internal const uint Art68A3FF = 0x68A3FF;
    internal const uint CoolGray60 = 0x697077;
    internal const uint Art6C6C6C = 0x6C6C6C;
    internal const uint Art6DA6FF = 0x6DA6FF;
    internal const uint Art6E6E6E = 0x6E6E6E;
    internal const uint Art6EA6FF = 0x6EA6FF;
    internal const uint Gray60 = 0x6F6F6F;
    internal const uint WarmGray60 = 0x726E6E;
    internal const uint Art72AAFF = 0x72AAFF;
    internal const uint Art77ADFF = 0x77ADFF;
    internal const uint Art787878 = 0x787878;
    internal const uint Art7CB0FF = 0x7CB0FF;
    internal const uint Art818181 = 0x818181;
    internal const uint Art81B3FF = 0x81B3FF;
    internal const uint Art87B7FF = 0x87B7FF;
    internal const uint Art8C8C8C = 0x8C8C8C;
    internal const uint Art8CBAFF = 0x8CBAFF;
    internal const uint Gray50 = 0x8D8D8D;
    internal const uint Art908B8B = 0x908B8B;
    internal const uint Art91BDFF = 0x91BDFF;
    internal const uint Art96C0FF = 0x96C0FF;
    internal const uint ArtA3A3A3 = 0xA3A3A3;
    internal const uint ArtA4A4A4 = 0xA4A4A4;
    internal const uint ArtA4A4A5 = 0xA4A4A5;
    internal const uint Gray40 = 0xA8A8A8;
    internal const uint ArtABABAB = 0xABABAB;
    internal const uint ArtBDBDBD = 0xBDBDBD;
    internal const uint ArtBEBEBE = 0xBEBEBE;
    internal const uint ArtC4C4C4 = 0xC4C4C4;
    internal const uint Gray30 = 0xC6C6C6;
    internal const uint ArtD0D0D0 = 0xD0D0D0;
    internal const uint ArtD2D2D2 = 0xD2D2D2;
    internal const uint ArtD3D3D3 = 0xD3D3D3;
    internal const uint ArtD6D6D6 = 0xD6D6D6;
    internal const uint ArtD7D7D7 = 0xD7D7D7;
    internal const uint ArtD8D8D8 = 0xD8D8D8;
    internal const uint ArtDCDCDC = 0xDCDCDC;
    internal const uint ArtDFDFDF = 0xDFDFDF;
    internal const uint ArtE0DBDA = 0xE0DBDA;
    internal const uint Gray20 = 0xE0E0E0;
    internal const uint ArtE1E1E1 = 0xE1E1E1;
    internal const uint ArtE2E2E2 = 0xE2E2E2;
    internal const uint ArtE4E4E4 = 0xE4E4E4;
    internal const uint ArtE5E5E5 = 0xE5E5E5;
    internal const uint ArtE6E6E6 = 0xE6E6E6;
    internal const uint ArtE7E7E7 = 0xE7E7E7;
    internal const uint ArtEAEAEA = 0xEAEAEA;
    internal const uint ArtEBEBEB = 0xEBEBEB;
    internal const uint ArtECECEC = 0xECECEC;
    internal const uint ArtEDEDED = 0xEDEDED;
    internal const uint ArtEEEEEE = 0xEEEEEE;
    internal const uint ArtF1F1F1 = 0xF1F1F1;
    internal const uint ArtF2F2F2 = 0xF2F2F2;
    internal const uint ArtF3F3F3 = 0xF3F3F3;
    internal const uint Gray10 = 0xF4F4F4;
    internal const uint ArtF6F6F6 = 0xF6F6F6;
    internal const uint ArtF7F7F7 = 0xF7F7F7;
    internal const uint ArtF8F8F8 = 0xF8F8F8;
    internal const uint ArtFAFAFA = 0xFAFAFA;
    internal const uint ArtFBFBFB = 0xFBFBFB;
    internal const uint ArtFCFCFC = 0xFCFCFC;
    internal const uint ArtFDFDFD = 0xFDFDFD;
    internal const uint ArtFEFEFE = 0xFEFEFE;
    internal const uint White = 0xFFFFFF;

    /// <summary>The color for a 0xRRGGBB value with an opacity (0 to 1).</summary>
    internal static Color ToColor(uint rgb, float alpha = 1) =>
        new(((rgb >> 16) & 0xFF) / 255f, ((rgb >> 8) & 0xFF) / 255f, (rgb & 0xFF) / 255f, Math.Clamp(alpha, 0, 1));
}

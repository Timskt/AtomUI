using AtomUI.Theme.TokenSystem;
using AtomUI.Theme;

namespace AtomUI.Controls.DesignTokens
{
    public static class IconTokenKey
    {
        public static readonly TokenResourceKey FallbackColor = new TokenResourceKey("AtomUI.Controls.Icon.FallbackColor");
        public static readonly TokenResourceKey SecondaryFillColor = new TokenResourceKey("AtomUI.Controls.Icon.SecondaryFillColor");
        public static readonly TokenResourceKey SecondaryStrokeColor = new TokenResourceKey("AtomUI.Controls.Icon.SecondaryStrokeColor");
        public static readonly TokenResourceKey StrokeLineCap = new TokenResourceKey("AtomUI.Controls.Icon.StrokeLineCap");
        public static readonly TokenResourceKey StrokeLineJoin = new TokenResourceKey("AtomUI.Controls.Icon.StrokeLineJoin");
        public static readonly TokenResourceKey StrokeWidth = new TokenResourceKey("AtomUI.Controls.Icon.StrokeWidth");
    }

    public enum IconTokenKind
    {
        FallbackColor,
        SecondaryFillColor,
        SecondaryStrokeColor,
        StrokeLineCap,
        StrokeLineJoin,
        StrokeWidth
    }

    public class IconTokenResourceExtension : TokenResourceExtension<IconTokenKind>
    {
        public IconTokenResourceExtension()
        {
        }

        public IconTokenResourceExtension(IconTokenKind kind) : base(kind)
        {
        }
    }
}
using AtomUI.Media;
using AtomUI.Theme.Styling;
using AtomUI.Theme.TokenSystem;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal class ButtonToken : AbstractControlDesignToken
{
    public const string ID = "Button";

    /// <summary>
    /// 文字字重
    /// </summary>
    public double FontWeight { get; set; }

    /// <summary>
    /// 默认按钮阴影
    /// </summary>
    public BoxShadows DefaultShadow { get; set; }

    /// <summary>
    /// 默认按钮文本颜色
    /// </summary>
    public Color DefaultColor { get; set; }

    /// <summary>
    /// 默认按钮背景色
    /// </summary>
    public Color DefaultBg { get; set; }

    /// <summary>
    /// 默认按钮边框颜色
    /// </summary>
    public Color DefaultBorderColor { get; set; }

    /// <summary>
    /// 默认按钮激活态文字颜色
    /// </summary>
    public Color DefaultActiveColor { get; set; }

    /// <summary>
    /// 只有图标的按钮图标尺寸
    /// </summary>
    public double OnlyIconSize { get; set; }
    
    /// <summary>
    /// 按钮内容字体大小
    /// </summary>
    public double ContentFontSize { get; set; } = double.NaN;

    /// <summary>
    /// 大号按钮内容字体大小
    /// </summary>
    public double ContentFontSizeLG { get; set; } = double.NaN;

    /// <summary>
    /// 小号按钮内容字体大小
    /// </summary>
    public double ContentFontSizeSM { get; set; } = double.NaN;

    /// <summary>
    /// 按钮内容字体行高
    /// </summary>
    public double ContentLineHeight { get; set; } = double.NaN;

    /// <summary>
    /// 大号按钮内容字体行高
    /// </summary>
    public double ContentLineHeightLG { get; set; } = double.NaN;

    /// <summary>
    /// 小号按钮内容字体行高
    /// </summary>
    public double ContentLineHeightSM { get; set; } = double.NaN;
    
    /// <summary>
    /// 按钮的下拉弹出菜单跟按钮之间的间距大小
    /// </summary>
    public double GutterToFlyout { get; set; }

    public ButtonToken()
        : base(ID)
    {
    }
}
using AtomUI.Controls.Converters;
using Avalonia.Styling;

namespace AtomUI.Desktop.Controls.Themes;

public class ButtonTheme : ControlTheme
{
    public static readonly ButtonIconVisibleConverter IconVisibleConverter = new();
    public static readonly IReadOnlyList<double> DashedStyle = [4, 2];
}
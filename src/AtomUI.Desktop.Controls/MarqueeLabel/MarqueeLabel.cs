using AtomUI.Controls;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme;
using AtomUI.Theme.Styling;
using Avalonia.Styling;

namespace AtomUI.Desktop.Controls;

public class MarqueeLabel : AbstractMarqueeLabel
{
    public MarqueeLabel()
    {
        this.RegisterTokenResourceScope(MarqueeLabelToken.ScopeProvider);
        ConfigureInstanceStyles();
    }

    private void ConfigureInstanceStyles()
    {
        var style = new Style();
        style.Add(CycleSpaceProperty, MarqueeLabelTokenKind.CycleSpace);
        style.Add(MoveSpeedProperty, MarqueeLabelTokenKind.DefaultSpeed);
        Styles.Add(style);
    }
}
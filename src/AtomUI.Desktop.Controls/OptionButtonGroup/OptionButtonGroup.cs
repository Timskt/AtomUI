using AtomUI.Controls.Commons;
using AtomUI.Theme;

namespace AtomUI.Desktop.Controls;

public class OptionButtonGroup : AbstractOptionButtonGroup
{
    public OptionButtonGroup()
    {
        this.RegisterTokenResourceScope(OptionButtonToken.ScopeProvider);
    }
}
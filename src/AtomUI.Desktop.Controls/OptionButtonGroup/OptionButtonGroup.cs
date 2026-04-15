using AtomUI.Controls;
using AtomUI.Theme;

namespace AtomUI.Desktop.Controls;

public class OptionButtonGroup : AbstractOptionButtonGroup
{
    public OptionButtonGroup()
    {
        this.RegisterTokenResourceScope(OptionButtonToken.ScopeProvider);
    }
}
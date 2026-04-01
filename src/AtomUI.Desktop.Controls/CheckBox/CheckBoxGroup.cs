using AtomUI.Controls;
using AtomUI.Theme;

namespace AtomUI.Desktop.Controls;

public class CheckBoxGroup : AbstractCheckBoxGroup
{
    public CheckBoxGroup()
    {
        this.RegisterTokenResourceScope(CheckBoxToken.ScopeProvider);
    }
}
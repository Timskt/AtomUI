using AtomUI.Controls;
using AtomUI.Theme;

namespace AtomUI.Desktop.Controls;

public class HyperLinkButton : AbstractHyperLinkButton
{
    public HyperLinkButton()
    {
        this.RegisterTokenResourceScope(ButtonToken.ScopeProvider);
    }
}
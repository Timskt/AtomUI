using AtomUI.Controls;
using AtomUI.Theme;

namespace AtomUI.Desktop.Controls;

public class Empty : AbstractEmpty
{
    public Empty()
    {
        this.RegisterTokenResourceScope(EmptyToken.ScopeProvider);
    }
}
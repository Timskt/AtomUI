using AtomUI.Controls;
using AtomUI.Theme;

namespace AtomUI.Desktop.Controls;

public class Tag : AbstractTag
{
    public Tag()
    {
        this.RegisterTokenResourceScope(TagToken.ScopeProvider);
    }
}
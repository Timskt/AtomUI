using AtomUI.Controls;
using AtomUI.Theme;

namespace AtomUI.Desktop.Controls;

public class Avatar : AbstractAvatar
{
    public Avatar()
    {
        this.RegisterTokenResourceScope(AvatarToken.ScopeProvider);
    }
}
using AtomUI.Controls;
using AtomUI.Theme;
using AtomUI.Utils;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

public class ArrowDecoratedBox : AbstractArrowDecoratedBox
{
    public ArrowDecoratedBox()
    {
        this.RegisterTokenResourceScope(ArrowDecoratedBoxToken.ScopeProvider);
    }
}
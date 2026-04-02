using AtomUI.Controls.Commons;
using AtomUI.Theme;

namespace AtomUI.Desktop.Controls;

public class Segmented : AbstractSegmented
{
    public Segmented()
    {
        this.RegisterTokenResourceScope(SegmentedToken.ScopeProvider);
    }
}
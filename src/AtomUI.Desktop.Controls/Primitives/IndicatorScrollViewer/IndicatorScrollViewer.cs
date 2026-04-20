using AtomUI.Theme;

namespace AtomUI.Desktop.Controls.Primitives;

internal class IndicatorScrollViewer : ScrollViewer
{
    public IndicatorScrollViewer()
    {
        this.RegisterTokenResourceScope(IndicatorScrollViewerToken.ScopeProvider);
    }
}
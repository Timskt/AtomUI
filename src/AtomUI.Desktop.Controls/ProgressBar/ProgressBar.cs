using AtomUI.Controls;
using AtomUI.Theme;

namespace AtomUI.Desktop.Controls;

public class ProgressBar : AbstractGeneralProgressBar
{
    public ProgressBar()
    {
        this.RegisterTokenResourceScope(ProgressBarToken.ScopeProvider);
    }
}
using AtomUI.Controls;
using AtomUI.Theme;

namespace AtomUI.Desktop.Controls;

public class StepsProgressBar : AbstractGeneralStepsProgressBar
{
    public StepsProgressBar()
    {
        this.RegisterTokenResourceScope(ProgressBarToken.ScopeProvider);
    }
}
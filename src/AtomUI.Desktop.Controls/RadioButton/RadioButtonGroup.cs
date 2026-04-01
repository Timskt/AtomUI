using AtomUI.Theme;

namespace AtomUI.Desktop.Controls;

public class RadioButtonGroup : AbstractRadioButtonGroup
{
    public RadioButtonGroup()
    {
        this.RegisterTokenResourceScope(RadioButtonToken.ScopeProvider);
    }
}
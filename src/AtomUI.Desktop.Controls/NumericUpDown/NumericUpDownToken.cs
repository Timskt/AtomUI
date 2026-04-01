using AtomUI.Theme;
using AtomUI.Theme.TokenSystem;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal class NumericUpDownToken : ButtonSpinnerToken
{
    public new const string ID = "NumericUpDown";
    public new static readonly ControlTokenResourceScopeProvider ScopeProvider = new(ID);
    
    public NumericUpDownToken()
        : base(ID)
    {
    }
}
using AtomUI.Controls;
using AtomUI.Theme;
using AtomUI.Utils;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

public class ArrowDecoratedBox : AbstractArrowDecoratedBox, 
                                 IControlSharedTokenResourcesHost
{
    #region 内部属性定义
    
    Control IControlSharedTokenResourcesHost.HostControl => this;
    string IControlSharedTokenResourcesHost.TokenId => ArrowDecoratedBoxToken.ID;
        
    #endregion
    
    public ArrowDecoratedBox()
    {
        this.RegisterResources();
    }
}
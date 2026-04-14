using AtomUIGallery.ShowCases;
using AtomUIGallery.Workspace.Views;
using Avalonia;

namespace AtomUIGallery;

public partial class BaseGalleryApplication : Application
{
    protected WorkspaceWindow CreateWorkspaceWindow()
    {
        return new WorkspaceWindow();
    }

    /// <summary>
    /// 注册所有 ShowCase 的 View ↔ ViewModel 绑定到 ReactiveUI 的 AppLocator。
    /// 遵循 ReactiveUI 23.x + Avalonia 最佳实践，在框架初始化完成回调中、主窗口创建之前调用，
    /// 确保路由能够正确解析视图。
    /// </summary>
    protected virtual void RegisterShowCases()
    {
        ShowCaseRegister.Register();
    }
}
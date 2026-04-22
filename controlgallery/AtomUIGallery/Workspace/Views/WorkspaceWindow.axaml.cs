using System.Reactive;
using System.Reactive.Disposables.Fluent;
using AtomUI.Desktop.Controls;
using AtomUIGallery.Workspace.ViewModels;
using Avalonia.Interactivity;
using ReactiveUI;

namespace AtomUIGallery.Workspace.Views;

internal enum WindowMenuItemKind
{
    FullScreen,
    Pin,
    Minimize,
    Maximize,
    Move,
    Resize,
    DarkMode,
    Compact,
    Motion,
    WaveSpirit,
    LanguageZhCN,
    LanguageEnUS,
}

public partial class WorkspaceWindow : ReactiveWindow<WorkspaceWindowViewModel>
{
    public const string LanguageId = nameof(WorkspaceWindow);

    public WorkspaceWindow()
    {
        ViewModel = new WorkspaceWindowViewModel();
        InitializeComponent();
        AddHandler(MenuItem.IsCheckStateChangedEvent, HandleMenuItemCheckChanged);

        this.WhenActivated(disposables =>
        {
            this.OneWayBind(ViewModel, vm => vm.Router, v => v.RoutedViewHost.Router)
                .DisposeWith(disposables);
            
            ShowCaseNavigation.ViewModel = ViewModel!.CaseNavigation;
        });
    }

    public override void Show()
    {
        base.Show();
        Height = double.NaN;
        Width  = double.NaN;
    }

    private void HandleMenuItemCheckChanged(object? sender, RoutedEventArgs e)
    {
        if (ViewModel is null) return;

        if (e.Source is MenuItem menuItem && menuItem.Tag is WindowMenuItemKind kind)
        {
            // 窗口 UI 属性：纯 View 层行为，直接操作窗口属性
            if (kind == WindowMenuItemKind.FullScreen)
            {
                IsFullScreenCaptionButtonEnabled = menuItem.IsChecked;
            }
            else if (kind == WindowMenuItemKind.Pin)
            {
                IsPinCaptionButtonEnabled = menuItem.IsChecked;
            }
            else if (kind == WindowMenuItemKind.Minimize)
            {
                CanMinimize = menuItem.IsChecked;
            }
            else if (kind == WindowMenuItemKind.Maximize)
            {
                CanMaximize = menuItem.IsChecked;
            }
            else if (kind == WindowMenuItemKind.Move)
            {
                IsMoveEnabled = menuItem.IsChecked;
            }
            else if (kind == WindowMenuItemKind.Resize)
            {
                CanResize = menuItem.IsChecked;
            }
            // ✅ 应用级别的操作：通过 ViewModel 命令执行，关注点分离
            else if (kind == WindowMenuItemKind.DarkMode)
            {
                ViewModel.ToggleDarkModeCommand.Execute(menuItem.IsChecked)
                         .Subscribe();
            }
            else if (kind == WindowMenuItemKind.Compact)
            {
                ViewModel.ToggleCompactModeCommand.Execute(menuItem.IsChecked)
                         .Subscribe();
            }
            else if (kind == WindowMenuItemKind.Motion)
            {
                // View 层：同步联动 WaveSpirit 复选框状态（纯 UI 行为）
                if (menuItem.Parent is MenuItem themeMenuItem)
                {
                    foreach (var item in themeMenuItem.Items)
                    {
                        if (item is MenuItem themeMenuChildItem &&
                            themeMenuChildItem.Tag is WindowMenuItemKind childKind &&
                            childKind == WindowMenuItemKind.WaveSpirit)
                        {
                            if (!menuItem.IsChecked)
                            {
                                themeMenuChildItem.IsChecked = false;
                            }
                        }
                    }
                }

                // ViewModel 命令：更新应用运动状态
                ViewModel.ToggleMotionCommand.Execute(menuItem.IsChecked)
                         .Subscribe();
            }
            else if (kind == WindowMenuItemKind.WaveSpirit)
            {
                ViewModel.ToggleWaveSpiritCommand.Execute(menuItem.IsChecked)
                         .Subscribe();
            }
            else if (kind == WindowMenuItemKind.LanguageZhCN)
            {
                ViewModel.SwitchToZhCNCommand.Execute(Unit.Default)
                         .Subscribe();
            }
            else if (kind == WindowMenuItemKind.LanguageEnUS)
            {
                ViewModel.SwitchToEnUSCommand.Execute(Unit.Default)
                         .Subscribe();
            }
        }
    }
}

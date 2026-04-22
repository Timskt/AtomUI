# AtomUI Gallery — 编码规范摘要

> **文档版本**：2026-04-15

---

## 1. 命名约定

| 元素 | 规范 | 示例 |
|---|---|---|
| 类名 | PascalCase | `ButtonShowCase`, `CaseNavigationViewModel` |
| 接口名 | `I` 前缀 + PascalCase | `IScreen`, `IRoutableViewModel` |
| 公共属性 | PascalCase | `HostScreen`, `ButtonSizeType` |
| 私有字段 | `_` 前缀 + camelCase | `_viewModel`, `_showCaseViewModelFactories` |
| 常量 | PascalCase（`const string`） | `public static EntityKey ID = "Button"` |
| 方法 | PascalCase | `NavigateTo()`, `HandleMenuItemCheckChanged()` |
| 事件处理方法 | `Handle` + 描述 | `HandleBasicModalButtonClick`, `HandleNavMenuItemClick` |
| XAML 模板部件 | `PART_` 前缀 | `PART_MainPanel`, `PART_RootBorder` |
| 静态属性 | PascalCase + `Property` 后缀 | `TitleProperty`, `DescriptionProperty` |
| ViewModel ID | PascalCase 字符串 | `"Button"`, `"AboutUs"`, `"CustomizeTheme"` |

---

## 2. 命名空间约定

| 命名空间 | 用途 |
|---|---|
| `AtomUIGallery` | 根命名空间 |
| `AtomUIGallery.Desktop` | 桌面启动项目 |
| `AtomUIGallery.ShowCases.ViewModels` | **所有** ShowCase ViewModel（平铺，不按分类子命名空间） |
| `AtomUIGallery.ShowCases.Views` | **所有** ShowCase View（平铺，不按分类子命名空间） |
| `AtomUIGallery.Workspace.ViewModels` | 工作区 ViewModel |
| `AtomUIGallery.Workspace.Views` | 工作区 View |
| `AtomUIGallery.Controls` | Gallery 自定义控件 |
| `AtomUIGallery.Models` | 数据模型 |
| `AtomUIGallery.Utils` | 工具类 |
| `AtomUIGallery.Workspace.Localization.*Lang` | 本地化资源 |
| `AtomUIGallery.Localization` | 生成的本地化基础设施 |
| `AtomUIGallery.ShowCases.ShowCaseControls` | ShowCase 专用控件 |
| `AtomUIGallery.Icons.Desktop` | 图标项目 |

> **重要**：所有 ShowCase ViewModel 使用**扁平命名空间** `AtomUIGallery.ShowCases.ViewModels`，不管文件系统中按 `General/`、`Layout/` 等子目录组织。View 同理使用 `AtomUIGallery.ShowCases.Views`。

---

## 3. 文件组织方式

### 3.1 ViewModel 文件

- 目录按分类组织：`ViewModels/General/`、`ViewModels/Layout/` 等
- 但命名空间始终为 `AtomUIGallery.ShowCases.ViewModels`
- 文件名 = 类名（如 `ButtonViewModel.cs`）
- 每个文件通常只包含一个类（`OsInfoViewModel.cs` 是例外，包含辅助类）

### 3.2 View 文件

- 目录按分类组织：`Views/General/`、`Views/DataEntry/` 等
- 命名空间始终为 `AtomUIGallery.ShowCases.Views`
- 成对出现：`.axaml` + `.axaml.cs`（代码后置）
- ⚠️ 存在**非标准文件名**（历史遗留）：
  - `FormShowCase.axmal.cs`（注意 `.axmal`，不是 `.axaml`）
  - `RateShowCase.axmal.cs`（同上）
  - `TourShowCase.axmal.cs`（同上）
  - `ResultShowCase.cs.axaml`（注意 `.cs.axaml`，不是 `.axaml`）
  - `CustomizeThemeShowCase.cs`（直接 `.cs`，不是 `.axaml.cs`）

### 3.3 控件文件

- 控件类 + 主题文件成对：`XxxControl.cs` + `XxxControlTheme.axaml`
- 主题提供器：`XxxThemesProvider.cs` + `XxxThemesProvider.axaml`

---

## 4. 代码组织模式

### 4.1 ViewModel 标准结构

```csharp
// 1. using 声明
using AtomUI.Controls;
using ReactiveUI;

// 2. 文件级命名空间
namespace AtomUIGallery.ShowCases.ViewModels;

// 3. 类定义（实现接口）
public class XxxViewModel : ReactiveObject, IRoutableViewModel
{
    // 4. 静态 ID
    public static EntityKey ID = "Xxx";
    
    // 5. IRoutableViewModel 实现
    public IScreen HostScreen { get; }
    public string UrlPathSegment { get; } = ID.ToString();
    
    // 6. 响应式属性（private 字段 + public 属性 + RaiseAndSetIfChanged）
    private SizeType _sizeType;
    public SizeType SizeType
    {
        get => _sizeType;
        set => this.RaiseAndSetIfChanged(ref _sizeType, value);
    }
    
    // 7. 构造函数
    public XxxViewModel(IScreen screen)
    {
        HostScreen = screen;
    }
}
```

### 4.2 View 标准结构

```csharp
// 1. using 声明
using AtomUIGallery.ShowCases.ViewModels;
using ReactiveUI;
using ReactiveUI.Avalonia;

// 2. 文件级命名空间
namespace AtomUIGallery.ShowCases.Views;

// 3. partial 类继承 ReactiveUserControl<TViewModel>
public partial class XxxShowCase : ReactiveUserControl<XxxViewModel>
{
    // 4. 构造函数：WhenActivated + InitializeComponent
    public XxxShowCase()
    {
        this.WhenActivated(disposables =>
        {
            // 5. 激活时的逻辑（事件订阅、初始化等）
        });
        InitializeComponent();
    }
    
    // 6. 事件处理方法
    public void HandleXxxClick(object? sender, RoutedEventArgs e) { ... }
}
```

### 4.3 本地化资源标准结构

```csharp
using AtomUI.Theme.Language;
using AtomUIGallery.Localization;
using AtomUIGallery.Workspace.Views;

namespace AtomUIGallery.Workspace.Localization.XxxLang;

[LanguageProvider(LanguageCode.en_US, Xxx.LanguageId)]
internal class en_US : LanguageProvider
{
    public const string Key1 = "English Text";
    public const string Key2 = "Another Text";
    
    protected override Type GetResourceKindType() => typeof(XxxLangResourceKind);
}
```

---

## 5. AXAML 编码规范

### 5.1 命名空间前缀

| 前缀 | URI | 用途 |
|---|---|---|
| `atom` | `https://atomui.net` 或 `using:AtomUI.Desktop.Controls` | AtomUI 控件 |
| `antdicons` | `https://atomui.net/icons/antdesign` | Ant Design 图标 |
| `gallery` | `https://atomui.net/oss-controls/gallery` | Gallery 控件/模型/本地化 |
| `rxui` | `http://reactiveui.net` | ReactiveUI 控件（RoutedViewHost） |
| `viewmodels` | `using:AtomUIGallery.ShowCases.ViewModels` | ViewModel 引用（x:Static） |

### 5.2 Token 资源引用

- 共享 Token：`{atom:SharedTokenResource ColorBorder}`
- 控件 Token：`{atom:TokenResource XxxProperty}`

### 5.3 本地化资源引用

- `{gallery:CaseNavigationLangResource <Key>}`
- `{gallery:WorkspaceWindowLangResource <Key>}`

### 5.4 静态引用

- ViewModel ID：`ItemKey="{x:Static viewmodels:ButtonViewModel.ID}"`
- 枚举：`Tag="{x:Static workspaceviews:WindowMenuItemKind.DarkMode}"`
- 模板部件：`Name="PART_MainPanel"`

---

## 6. 设计模式

| 模式 | 应用场景 |
|---|---|
| **MVVM** | 全局架构模式（ReactiveUI） |
| **路由导航** | `IScreen` + `RoutingState` + `RoutedViewHost` |
| **工厂模式** | `CaseNavigationViewModel._showCaseViewModelFactories`（`Dictionary<EntityKey, Func<IRoutableViewModel>>`） |
| **观察者模式** | `ReactiveObject.RaiseAndSetIfChanged` + `WhenActivated` |
| **模板方法** | `ShowCasePanel.OnApplyTemplate()` 中的布局算法 |
| **Provider 模式** | `LanguageProvider`、`IconProvider`、`ControlThemesProvider` — AtomUI 大量使用的注册机制 |
| **Dispose 模式** | `WhenActivated` 中通过 `CompositeDisposable` 管理事件订阅生命周期 |
| **Source Generator** | 语言资源枚举、图标提供者、XAML 名称访问器 — 自动代码生成 |
| **Strategy** | Dark/Light/Compact 主题模式切换 |

---

## 7. 事件处理模式

ShowCase View 中的事件处理有**两种模式**：

### 模式 A：Code-Behind 事件处理（当前主流）

```csharp
// 在 AXAML 中绑定：<Button Click="HandleLoadingBtnClick"/>
public void HandleLoadingBtnClick(object? sender, RoutedEventArgs args)
{
    if (sender is Button button)
    {
        button.IsLoading = true;
        Dispatcher.UIThread.InvokeAsync(async () =>
        {
            await Task.Delay(TimeSpan.FromSeconds(3));
            button.IsLoading = false;
        });
    }
}
```

### 模式 B：ViewModel 属性绑定

```csharp
// ViewModel 中定义响应式属性
private SizeType _buttonSizeType;
public SizeType ButtonSizeType
{
    get => _buttonSizeType;
    set => this.RaiseAndSetIfChanged(ref _buttonSizeType, value);
}

// View 中通过 DataContext 设置
_viewModel.ButtonSizeType = SizeType.Large;
```

> **当前状态**：大多数 ShowCase 使用模式 A（Code-Behind 直接操作控件），少数使用模式 B。Gallery 是演示项目，Code-Behind 操作控件是合理的选择。

---

## 8. ReactiveUI 使用规范

### 8.1 属性变更通知

使用 `RaiseAndSetIfChanged` 而非 `INotifyPropertyChanged`：
```csharp
// ✅ 正确
private SizeType _sizeType;
public SizeType SizeType
{
    get => _sizeType;
    set => this.RaiseAndSetIfChanged(ref _sizeType, value);
}
```

### 8.2 WhenActivated + DisposeWith

在 View 构造函数中注册 `WhenActivated`，通过 `DisposeWith` 或 `Disposable.Create` 管理订阅生命周期：

```csharp
this.WhenActivated(disposables =>
{
    // 方式 1：DisposeWith
    this.WhenAnyValue(x => x.ViewModel!.SearchText)
        .Subscribe(text => FilterIcons(text))
        .DisposeWith(disposables);

    // 方式 2：Disposable.Create（用于事件订阅）
    MyButton.Click += HandleClick;
    disposables.Add(Disposable.Create(() => MyButton.Click -= HandleClick));
});
```

### 8.3 路由规范

- 每个 ViewModel 必须实现 `IRoutableViewModel`
- `HostScreen` 引用传入的 `IScreen` 实例
- `UrlPathSegment` 使用 `ID.ToString()` 作为路由标识

---

## 9. AXAML ShowCase 根元素模板

```xml
<!-- ShowCase View 的 AXAML 根元素标准结构 -->
<rxui:ReactiveUserControl
    xmlns="https://github.com/avaloniaui"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:rxui="http://reactiveui.net"
    xmlns:atom="https://atomui.net"
    xmlns:gallery="https://atomui.net/oss-controls/gallery"
    xmlns:vm="using:AtomUIGallery.ShowCases.ViewModels"
    x:TypeArguments="vm:XxxViewModel"
    x:Class="AtomUIGallery.ShowCases.Views.XxxShowCase">

    <gallery:ShowCasePanel>
        <gallery:ShowCaseItem Title="基本用法" Description="...">
            <!-- 控件演示内容 -->
        </gallery:ShowCaseItem>
    </gallery:ShowCasePanel>
</rxui:ReactiveUserControl>
```

---

## 7. 异常处理

- **全局异常**：`Program.Main()` 包裹 try-catch，异常写入 `AppCrashLogs/CrashLog_*.log`
- **局部异常**：极少使用 try-catch，主要在 `SystemInfoProvider`（硬件信息获取）和 `LinuxDistributionDetector`（文件系统读取）中
- **空安全**：全局启用 `<Nullable>enable</Nullable>`，广泛使用 `?.` 和 `is` 模式匹配

---

## 8. 编译器设置

| 设置 | 值 |
|---|---|
| `<Nullable>` | `enable` |
| `<EmitCompilerGeneratedFiles>` | `true`（输出到 `GeneratedFiles/`） |
| `<TargetFrameworks>` | `$(AtomUITargetFrameworks)` — Debug: `net10.0` / Release: `net10.0;net8.0` |
| Avalonia 名称生成器 | 自动为 `x:Name` 生成强类型字段 |
| AtomUI Generator | 生成 `LanguageResourceConst`、`LanguageProviderPool`、`TokenResourceConst` |



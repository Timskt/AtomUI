# 06 - 技术栈

## 编程语言

| 语言 | 版本 | 用途 |
|------|------|------|
| **C#** | 12.0 (net8.0 默认) | 所有源码 |
| **XAML** | Avalonia AXAML | UI 模板、样式、主题定义 |
| **SVG** | — | 图标源文件 (Ant Design) |

## 框架与运行时

| 技术 | 版本 | 说明 |
|------|------|------|
| **.NET** | 8.0 | 目标框架，所有项目统一 |
| **Avalonia UI** | 11.2.3 (最低) / 11.3.0 (当前) | 跨平台 UI 框架，AtomUI 的底层技术 |
| **Avalonia.Desktop** | — | 桌面平台支持 |
| **Avalonia.Themes.Fluent** | — | Gallery 默认主题（被 AtomUI 主题覆盖） |

### Avalonia 核心依赖项

| NuGet 包 | 版本 | 用途 |
|----------|------|------|
| `Avalonia` | 11.3.0 | 核心框架 |
| `Avalonia.Desktop` | 11.3.0 | 桌面宿主 |
| `Avalonia.Themes.Fluent` | 11.3.0 | Fluent 主题基础 |
| `Avalonia.Fonts.Inter` | 11.3.0 | Inter 字体 |
| `Avalonia.Diagnostics` | 11.3.0 | 调试工具 (Debug only) |
| `Avalonia.ReactiveUI` | 11.3.0 | ReactiveUI 集成 |
| `Avalonia.Controls.DataGrid` | 11.3.0 | DataGrid 控件 |
| `Avalonia.Svg` | 11.3.0 | SVG 渲染 |
| `Avalonia.Svg.Skia` | 11.3.0 | SVG Skia 渲染 |

## 构建工具

| 工具 | 版本/说明 |
|------|-----------|
| **MSBuild** | .NET 8.0 SDK 内置 |
| **dotnet CLI** | 8.0+ |
| **Source Generators** | Roslyn 增量生成器 |
| **Directory.Build.props** | 全局构建属性 |
| **Directory.Build.targets** | 全局构建目标 |
| **Directory.Packages.props** | 中央包版本管理 (CPM) |

### 构建配置文件

| 文件 | 作用 |
|------|------|
| `Directory.Build.props` | 全局属性：TargetFramework、LangVersion、Nullable、ImplicitUsings |
| `Directory.Build.targets` | 全局构建目标 |
| `Directory.Packages.props` | NuGet 包版本集中管理 |
| `build/Version.props` | 版本号定义 |
| `build/Common.props` | 通用构建属性 |
| `build/Output.props` | 输出路径配置 |
| `build/Output.App.props` | 应用输出配置 |
| `build/PackageMetaInfo.props` | NuGet 包元信息 |
| `global.json` | SDK 版本锁定 |

### 关键构建属性

```xml
<!-- Directory.Build.props -->
<TargetFramework>net8.0</TargetFramework>
<LangVersion>latest</LangVersion>
<Nullable>enable</Nullable>
<ImplicitUsings>enable</ImplicitUsings>
<AvaloniaUseCompiledBindingsByDefault>true</AvaloniaUseCompiledBindingsByDefault>
```

## UI 技术

### XAML 标记扩展

| 标记扩展 | 命名空间 | 用途 |
|----------|----------|------|
| `ControlTokenResource` | `atom` | 引用控件 Token 资源 |
| `AntDesignIcon` | `atom` | 引用 Ant Design 图标 |
| `DynamicResource` | Avalonia | 动态资源引用 |
| `StaticResource` | Avalonia | 静态资源引用 |
| `Binding` | Avalonia | 数据绑定 |
| `TemplateBinding` | Avalonia | 模板绑定 |
| `x:Static` | XAML | 静态成员引用 |

### XAML 命名空间

```xml
<!-- AtomUI 控件命名空间 -->
xmlns:atom="https://atomui.net"

<!-- Avalonia 命名空间 -->
xmlns="https://github.com/avaloniaui"

<!-- XAML 标准命名空间 -->
xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
```

### 样式系统

- **样式选择器**: Avalonia CSS-like 选择器 (`atom|Button:primary:hover`)
- **PseudoClass**: 自定义伪类 (`:danger`, `:loading`, `:small`, `:large`)
- **ControlTheme**: Avalonia ControlTheme 机制
- **ResourceDictionary**: 层级资源字典，Token 值注入

## 测试框架

| 框架 | 项目 | 说明 |
|------|------|------|
| **xUnit** | AtomUI.Base.Tests | 单元测试 |
| **xUnit** | AtomUI.Generator.Tests | 生成器测试 |
| **Avalonia.Headless** | AtomUI.TestBase | 无头 UI 测试基类 |

### 测试项目结构

```
tests/
├── AtomUI.Base.Tests/          # 基础单元测试
│   └── AtomUI.Base.Tests.csproj
├── AtomUI.Generator.Tests/     # Source Generator 测试
│   └── AtomUI.Generator.Tests.csproj
└── AtomUI.TestBase/            # 测试基础设施
    └── AtomUI.TestBase.csproj
```

## 外部依赖

### NuGet 包依赖（完整清单）

| 包名 | 版本 | 使用项目 | 用途 |
|------|------|----------|------|
| `Avalonia` | 11.3.0 | Core, Controls, Desktop | UI 框架核心 |
| `Avalonia.Desktop` | 11.3.0 | Gallery.Desktop | 桌面宿主 |
| `Avalonia.Themes.Fluent` | 11.3.0 | Gallery | 基础主题 |
| `Avalonia.Fonts.Inter` | 11.3.0 | Gallery | 字体 |
| `Avalonia.Diagnostics` | 11.3.0 | Gallery (Debug) | 调试 |
| `Avalonia.ReactiveUI` | 11.3.0 | Gallery | 响应式扩展 |
| `Avalonia.Controls.DataGrid` | 11.3.0 | Desktop.Controls.DataGrid | 数据网格 |
| `Avalonia.Svg` | 11.3.0 | Icons | SVG 支持 |
| `Avalonia.Svg.Skia` | 11.3.0 | Icons | SVG Skia 渲染 |
| `Microsoft.CodeAnalysis.CSharp` | 4.8.0 | SourceGenerators | Roslyn 编译器 API |
| `Microsoft.CodeAnalysis.Analyzers` | — | SourceGenerators | 分析器基础设施 |
| `QRCoder` | 1.4.3 | Desktop.Controls | 二维码生成 |
| `SkiaSharp` | — | Core (间接) | 图形渲染 |
| `MicroCom.CodeGen.MSBuild` | — | Native | COM 互操作生成 |
| `MonoMac` | — | Native | macOS 原生绑定 |

### 项目间引用（内部依赖）

```
AtomUI.Desktop.Controls
  → AtomUI.Controls
  → AtomUI.Controls.Shared
  → AtomUI.Core

AtomUI.Controls
  → AtomUI.Core

AtomUI.Controls.Shared
  → AtomUI.Core

AtomUI.Desktop.Controls.ColorPicker
  → AtomUI.Desktop.Controls
  → AtomUI.Controls

AtomUI.Desktop.Controls.DataGrid
  → AtomUI.Desktop.Controls

AtomUI.Icons.AntDesign
  → AtomUI.Icons
  → AtomUI.Core

AtomUI.Icons
  → AtomUI.Core

AtomUI.Gallery
  → AtomUI.Desktop.Controls
  → AtomUI.Icons.AntDesign

AtomUI.Gallery.Desktop
  → AtomUI.Gallery
```

## 字体系统

| 字体 | 用途 | 来源项目 |
|------|------|----------|
| **AlibabaSans** | 默认 UI 字体 | AtomUI.Fonts.AlibabaSans |
| **Inter** | 备选 UI 字体 | Avalonia.Fonts.Inter |
| **Ant Design 图标字体** | 图标渲染 | AtomUI.Icons.AntDesign |

## 平台原生互操作 (AtomUI.Native)

### 原生 API 调用方式

| 平台 | 调用方式 | 原生库 | 关键 API |
|------|----------|--------|----------|
| **Windows** | P/Invoke (`DllImport`) | `user32.dll` | `GetWindowLongW`, `SetWindowLongW` |
| **macOS** | P/Invoke (`DllImport`) | `/usr/lib/libobjc.A.dylib` | `objc_getClass`, `sel_registerName`, `objc_msgSend` |
| **Linux** | P/Invoke (`DllImport`) | `libxcb.so` | `xcb_connect`, `xcb_shape_rectangles_checked` |

### 原生功能清单

| 功能 | 用途 | 使用控件 |
|------|------|----------|
| `SetWindowIgnoreMouseEvents` | 窗口鼠标穿透（FloatButton） | FloatButton |
| `IsWindowIgnoreMouseEvents` | 查询鼠标穿透状态 | FloatButton |
| `SetMacOSOptionButtonsPosition` | macOS 红绿灯按钮位置 | Window |
| `GetMacOSOptionsSize` | macOS 红绿灯按钮尺寸 | Window |
| `SetMacOSWindowClosable` | macOS 窗口可关闭控制 | Window |

### 平台安全机制

- 所有平台特定方法标记 `[SupportedOSPlatform("...")]` 属性
- 运行时通过 `OperatingSystem.IsWindows()`/`IsMacOS()`/`IsLinux()` 分发
- 设计时通过 `Design.IsDesignMode` 跳过原生调用
- `InternalsVisibleTo` 限制访问：仅 `AtomUI.Core` 和 `AtomUI.Desktop.Controls` 可见

## 平台支持

| 平台 | 支持状态 | 说明 |
|------|----------|------|
| **Windows** | ✅ 完全支持 | x64, x86, ARM64 |
| **macOS** | ✅ 完全支持 | x64, ARM64 (Apple Silicon) |
| **Linux** | ✅ 完全支持 | x64, ARM64 (X11/XCB) |
| **iOS** | 🔮 计划中 | Avalonia 支持，AtomUI 未适配（InternalsVisibleTo 已预留 Mobile.Controls） |
| **Android** | 🔮 计划中 | Avalonia 支持，AtomUI 未适配 |
| **WebAssembly** | 🔮 计划中 | Avalonia 支持，AtomUI 未适配 |

## 版本管理

| 工具 | 说明 |
|------|------|
| **Git** | 版本控制 |
| **SemVer** | 语义化版本 |
| **NuGet** | 包发布 |
| **GitHub/Gitee** | 远程仓库 |

### 版本号规则

- 格式: `Major.Minor.Patch[-Prerelease]`
- 定义在 `build/Version.props`
- NuGet 包元信息在 `build/PackageMetaInfo.props`
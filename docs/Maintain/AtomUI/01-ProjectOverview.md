# 01 - 项目概览

## 项目简介

**AtomUI** 是一个基于 Avalonia UI 框架的跨平台桌面控件库，视觉设计严格遵循 **Ant Design 5.0** 规范。项目采用 **Design Token** 驱动的动态主题架构，支持亮色/暗色主题切换、控件尺寸变体、样式变体等高级特性。

## 核心特性

- 🎨 **Ant Design 5.0 视觉规范** — 77+ 控件，完整覆盖 Ant Design 组件
- 🌓 **亮色/暗色主题** — 基于 Design Token 的动态主题切换
- 📐 **SizeType 变体** — Small / Middle / Large 三级尺寸
- 🎭 **StyleVariant 变体** — Filled / Outlined / Borderless 等风格
- 🔤 **800+ Ant Design 图标** — Filled / Outlined / Twotone 三种变体
- ⚡ **Source Generator** — 编译时生成 Token 枚举和资源映射
- 🖥️ **跨平台** — Windows / macOS / Linux
- 🌐 **国际化** — ILanguageProvider 接口（部分实现）

## 目录结构

```
AtomUI/
├── AtomUI.sln                          # 解决方案文件
├── Directory.Build.props               # 全局构建属性
├── Directory.Build.targets             # 全局构建目标
├── Directory.Packages.props            # NuGet 中央包管理
├── global.json                         # SDK 版本锁定
├── build/                              # 构建配置
│   ├── Version.props                   # 版本号
│   ├── Common.props                    # 通用属性
│   ├── Output.props                    # 输出路径
│   ├── Output.App.props                # 应用输出
│   └── PackageMetaInfo.props           # NuGet 元信息
├── src/                                # 源码
│   ├── AtomUI.Core/                    # 核心层：Theme/Token/Palette/Motion
│   ├── AtomUI.Controls/                # 控件抽象层：属性/事件定义
│   ├── AtomUI.Controls.Shared/         # 共享控件：IconPresenter/WaveSpirit
│   ├── AtomUI.Desktop.Controls/        # 桌面控件实现：77+ 控件
│   ├── AtomUI.Desktop.Controls.ColorPicker/  # 颜色选择器
│   ├── AtomUI.Desktop.Controls.DataGrid/     # 数据网格
│   ├── AtomUI.SourceGenerators/        # Token Source Generator
│   ├── AtomUI.Icons/                   # 图标抽象层
│   ├── AtomUI.Icons.Shared/            # 图标共享代码
│   ├── AtomUI.Icons.AntDesign/         # Ant Design 图标包
│   ├── AtomUI.Icons.AntDesign.Generator/  # 图标 Source Generator
│   ├── AtomUI.Fonts.AlibabaSans/       # 阿里巴巴普惠体
│   ├── AtomUI.Gallery/                 # Gallery Demo (共享)
│   └── AtomUI.Gallery.Desktop/         # Gallery Demo (桌面启动)
├── tests/                              # 测试
│   ├── AtomUI.Base.Tests/              # 基础单元测试
│   ├── AtomUI.Generator.Tests/         # Generator 测试
│   └── AtomUI.TestBase/                # 测试基础设施
├── .referenceprojects/                 # 参考项目
│   ├── Avalonia/                       # Avalonia 源码参考
│   └── avalonia-docs/                  # Avalonia 文档参考
└── docs/                               # 文档
    ├── Maintain/AtomUI/                # 维护文档（本目录）
    └── PromptWords/                    # AI 提示词
```

## 项目统计

| 指标 | 数值 |
|------|------|
| 源码项目数 | 13 |
| 测试项目数 | 3 |
| 控件数量 | 77+ |
| Token 类数量 | 65+ |
| 图标数量 | 800+ |
| 目标框架 | .NET 8.0 |
| Avalonia 版本 | 11.2.3 ~ 11.3.0 |
| C# 版本 | 12.0 (latest) |

## 快速开始

### 构建项目

```bash
# 还原依赖
dotnet restore

# 构建全部
dotnet build

# 运行 Gallery
dotnet run --project src/AtomUI.Gallery.Desktop
```

### 在项目中使用 AtomUI

```xml
<!-- 添加 NuGet 引用 -->
<PackageReference Include="AtomUI.Desktop.Controls" Version="*" />
<PackageReference Include="AtomUI.Icons.AntDesign" Version="*" />
```

```csharp
// App.axaml.cs 中初始化主题
public override void OnFrameworkInitializationCompleted()
{
    base.OnFrameworkInitializationCompleted();
    
    // 构建 AtomUI 主题
    ThemeBuilder.BuildTheme();
}
```

```xml
<!-- XAML 中使用控件 -->
<Window xmlns:atom="https://atomui.net">
    <atom:Button ButtonType="Primary" SizeType="Large">
        点击我
    </atom:Button>
</Window>
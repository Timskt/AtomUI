# AtomUI 项目维护文档

> 本文档旨在为 AI 辅助迭代提供全面、准确的项目知识库。所有内容基于源码分析，随项目演进需同步更新。

## 📚 文档索引

| 序号 | 文档 | 内容概要 |
|------|------|----------|
| 01 | [项目概览](01-ProjectOverview.md) | 项目定位、目标、目录结构、构建配置 |
| 02 | [架构设计](02-Architecture.md) | 模块划分、依赖关系、数据流、架构图 |
| 03 | [主题系统](03-ThemeSystem.md) | Design Token 体系、全局/控件 Token、主题变体、Source Generator |
| 04 | [控件体系](04-ControlSystem.md) | 77 个控件清单、继承层次、实现模式、添加控件流程 |
| 05 | [生成器系统](05-GeneratorSystem.md) | Source Generator、Icon Generator、Token Generator |
| 06 | [技术栈](06-TechStack.md) | 语言/框架/构建/测试/依赖全清单 |
| 07 | [编码规范](07-CodingConventions.md) | 命名约定、代码组织、设计模式、异常处理 |
| 08 | [风险与注意事项](08-RisksAndNotes.md) | 技术债、性能瓶颈、硬编码、迭代注意事项 |
| 09 | [Avalonia内部机制](09-AvaloniaInternals.md) | Avalonia API、PseudoClasses、ControlTheme、模板系统、绑定机制 |

## 🔑 关键概念速查

### 项目本质
AtomUI 是基于 **Avalonia UI** 框架的 **Ant Design** 风格控件库，采用 **Design Token** 驱动的主题系统，支持亮色/暗色主题切换。

### 核心架构分层
```
AtomUI.Controls          → 控件抽象定义（属性、事件、接口）
AtomUI.Controls.Shared   → 共享基础设施（IconPresenter、WaveSpirit、AddOnDecoratedBox 等）
AtomUI.Core              → 核心系统（Theme、Token、Palette、MediaQuery、Motion）
AtomUI.Desktop.Controls  → 控件完整实现（逻辑、模板、Token、主题）
AtomUI.Native            → 平台原生互操作（Win/Mac/Linux P/Invoke）
AtomUI.Fonts.AlibabaSans → 阿里巴巴普惠体字体包
AtomUI.Icons             → 图标系统（Ant Design 图标包）
```

### 主题系统核心流程
```
GlobalToken → ControlDesignToken.CalculateTokenValues() → [TokenResourceKey] 标记的属性
    → Source Generator 生成 TokenKind 枚举 + TokenResourceConst 映射
    → XAML 中通过 {atom:ControlTokenResource Key} 标记扩展引用
```

### 添加新控件核心步骤
1. 定义枚举 → 2. 创建控件 .cs → 3. 创建 Token .cs → 4. 创建 ThemeConstants → 5. 创建 Theme .axaml → 6. 注册到 Provider → 7. 编译触发 Generator → 8. Gallery 展示

## ⚠️ 迭代前必读

1. **Token 系统是编译时生成的**：修改 Token 属性后必须重新编译，Source Generator 才会更新 `*.g.cs` 文件
2. **控件跨项目拆分**：`AtomUI.Controls` 定义属性，`AtomUI.Desktop.Controls` 实现逻辑，使用 `partial class`
3. **主题文件聚合**：新主题文件必须在 `DesktopControlThemesProvider.axaml` 中注册
4. **PseudoClass 约定**：自定义伪类格式为 `:xxx`，在静态构造函数中通过 `PseudoClasses.Set()` 更新
5. **Avalonia 兼容性**：基于 Avalonia 11.x API，不可随意升级 Avalonia 版本

## 📊 项目规模统计

| 指标 | 数值 |
|------|------|
| 源码项目数 | 15+ |
| 控件数量 | 77 |
| 抽象基类数 | 30+ |
| Design Token 属性 | 500+ |
| 图标数量 | 800+ (Ant Design) |
| 目标框架 | .NET 8.0 |
| 最低 Avalonia 版本 | 11.2.3 |
请协助我将 **AtomUI Gallery** 项目从旧版 ReactiveUI 规范升级到最新标准。当前已升级包版本：`ReactiveUI.Avalonia` 11.4.12、`ReactiveUI` 23.1.8。需要按照最新 ReactiveUI 规范对整个 Gallery 项目进行代码改造。

**在执行任何修改之前，请先完成以下准备工作：**

1. **确认资料可访问性**  
   请检查以下路径是否存在且可读。如果任何路径不可访问，请立即告知我，我将提供具体文件内容。
    - ReactiveUI 源码：`.referenceprojects/ReactiveUI`
    - ReactiveUI.Avalonia 源码：`.referenceprojects/ReactiveUI.Avalonia`
    - ReactiveUI 官方例子：`.referenceprojects/ReactiveUI.Avalonia` （即与源码同一目录下的示例）
    - ReactiveUI 官网文档（非常重要）：`.referenceprojects/reactiveui-website/reactiveui/docs`
    - 项目文档（包含 Gallery 维护文档）：`docs` 目录，尤其是 `docs/Gallery` 下的所有文档

2. **学习并提取规范**  
   请认真阅读上述资料，特别关注：
    - ReactiveUI 23.1.8 的 `WhenActivated`、`ViewModelActivator`、`ActivationForViewFetcher` 等激活机制的变化
    - `RoutableViewModel` 和路由相关的最新用法
    - 自动视图模型绑定（`ViewModelViewHost`、`ViewLocator`）的最佳实践
    - 在 Avalonia 11.x 下使用 `ReactiveUI.Avalonia` 的完整配置方式（包括 `UseRoutedViewHost`、`UseViewLocator` 等）
    - 官网文档中关于“升级指南”或“从旧版本迁移”的章节

3. **分析当前 Gallery 项目**  
   扫描 `Gallery` 项目的全部代码（重点关注 ViewModels 和 Views），找出不符合上述新规范的地方，包括但不限于：
    - 过时的 `WhenActivated` 使用方式
    - 未使用 `ViewModelActivator` 或 `ActivationForViewFetcher` 导致视图激活事件失效
    - 路由和导航实现方式与新版 API 不兼容
    - 视图定位器（`ViewLocator`）未正确注册或实现
    - 任何使用了已被标记为 `[Obsolete]` 的 ReactiveUI API

4. **执行改造**  
   按照新规范逐模块修改代码。每个模块修改完成后，请简要说明修改内容和依据（引用官方文档或源码中的具体位置）。  
   改造顺序建议：
    - 全局配置（App.xaml.cs、Bootstrapper、ViewLocator 等）
    - 基础 ViewModel 基类或公共逻辑
    - 路由相关的 ViewModel 和 View
    - 其余功能页面

5. **输出要求**  
   完成改造后，请提供：
    - 所有修改文件的列表及修改摘要
    - 更新后的核心代码片段（如新的 ViewModel 基类、ViewLocator 实现、导航服务等）
    - 一份简短的**升级验证清单**，用于人工测试关键功能是否正常（如页面切换、激活/失活事件、命令绑定等）

**重要提醒**：
- 请严格以 ReactiveUI 官方文档和最新示例为准，不要依赖旧有经验或个人推测。
- 如果文档或源码中存在矛盾，请优先参考官方文档并向我说明。
- 改造过程中请不要修改项目的目标框架或无关的第三方库版本。
- 如果某些改造可能破坏现有功能，请先提出方案再执行。

请现在开始准备工作，确认资料可访问性后，我会等待你的分析结果和改造计划。
> 请结合 AtomUI 的项目文档（位于 `docs/` 目录下），对 AtomUI 代码进行全面的**性能问题**与**内存泄漏**分析。  
> 技术背景：AtomUI 是基于 **Avalonia** 的 .NET 控件库，目标框架为 **.NET 8 / .NET 10**。
>
> 需要分析的代码范围：
> - `src/` 下的所有项目
> - `controlgallery/` 下的所有项目
>
> 请输出一份**详细的分析报告**，并针对每个问题给出**具体的修复方案**。  
> 报告应至少包含：问题描述、代码位置（文件 + 行号/方法名）、复现条件、影响评估、修复建议与示例代码。
>
> 分析时请重点关注 **Avalonia / .NET 环境下常见性能与内存问题**，例如：
> - 事件订阅未取消（如 `PropertyChanged`、`PointerPressed` 等）
> - 弱引用（`WeakReference`）使用不当或缺失
> - 绑定（`Binding`）泄漏，尤其是没有使用 `x:CompileBindings` 或未清理的 `INotifyPropertyChanged`
> - 控件未从逻辑树/视觉树中正确分离导致根引用残留
> - `Dispatcher.UIThread` 或 `Timer` 未及时停止
> - 动画（`Animation`、`Transition`）完成后未释放资源
> - 大对象堆（LOH）分配频繁，或在控件模板中创建过多临时对象
> - 异步操作（`Task`、`async/await`）缺少取消支持（`CancellationToken`）
> - 内存中缓存未限制大小或未使用 `WeakCache`
> - 在控件生命周期中未实现 `IDisposable` 或未正确调用 `Dispose`
> - 任何其他我没有想到，但是会导致问题的代码
>
> 若 `docs/` 中有架构说明或设计约束，请对照验证其实现是否引入上述风险。
> 请将你发现的所有问题整理成文档，保存到 docs/AtomUI_issues.md 中

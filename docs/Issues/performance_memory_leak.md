# AtomUI 性能问题与内存泄漏分析报告

> 分析日期：2026-04-16  
> 最后更新：2026-04-17  
> 目标框架：.NET 8 / .NET 10  
> UI 框架：Avalonia 11.3.x  
> 分析范围：`src/` 及 `controlgallery/` 下所有项目  
> 参考文档：`docs/` 目录下的架构设计文档

---

## 📊 修复进度

| 统计 | 数量 |
|------|------|
| 总问题数 | 39 |
| ✅ 已修复 | 30 |
| ⏳ 待修复 | 9 |
| 修复进度 | **76.9%** |

### 已修复问题清单

| # | 问题 | 严重度 | Commit |
|---|------|--------|--------|
| 3.1 | TransformTrackingHelper `-=` → `+=` | 🔴 | 早期修复 |
| 3.2 | SwitchKnob 基类方法调用修正 | 🔴 | 早期修复 |
| 3.3 | TimerStatistic 定时器泄漏 | 🔴 | 早期修复 |
| 3.6 | InfoPickerInput OnApplyTemplate Lambda 叠加 | 🔴 | 早期修复 |
| 3.7 | ColorPickerInput OnApplyTemplate 8 个 Lambda 叠加 | 🔴 | 早期修复 |
| 4.2 | Carousel 自动播放定时器泄漏 | 🟠 | 早期修复 |
| 4.3 | Drawer OpenOn SizeChanged 事件未取消 | 🟠 | 早期修复 |
| 4.4 | Tour 控件多处事件订阅未取消 | 🟠 | 早期修复 |
| 4.5 | OptionButtonGroup Lambda 事件订阅 | 🟠 | `01ec42d6` |
| 4.6 | 静态 ConcurrentDictionary 缓存无大小限制 | 🟠 | 早期修复 |
| 4.7 | WindowNotificationManager 定时器 Tick 未取消 | 🟠 | 早期修复 |
| 4.8 | 多个 async 方法缺少 CancellationToken | 🟠 | `76aadb94`, `4c9054dd` |
| 5.1 | Segmented SelectionChanged 事件生命周期 | 🟡 | `12865b91` |
| 5.2 | FlyoutStateHelper 定时器 Lambda 捕获 | 🟡 | `98a5500e` |
| 5.3 | CompositeDisposable 未在 Detach 时 Dispose | 🟡 | `8a9196ab` 等 |
| 5.4 | MarqueeLabel 动画资源清理 | 🟡 | `09a03d8e` |
| 5.6 | ToolTipService 全局事件订阅 | 🟡 | `98afb745` |
| 5.9 | Watermark glyph.PropertyChanged 取消订阅 | 🟡 | `8a9196ab` |
| 5.10 | TreeView/CascaderView CTS Dispose | 🟡 | `7472c56a` |
| 5.11 | async void 方法无异常保护 | 🟡 | 早期修复 |
| 5.12 | AbstractSkeleton CTS 未 Dispose | 🟡 | 早期修复 |
| 5.13 | Mentions/AutoComplete _delayTimer 重建未停旧实例 | 🟡 | 早期修复 |
| 5.15 | FloatButtonGroupHost lambda 订阅 | 🟡 | `d2fc1ec4` |
| 5.16 | Space Children.PropertyChanged 重复订阅 | 🟡 | `5ba9ca9a` |
| 5.17 | ToolTip.StartShowTimer Timer 重叠 | 🟡 | `5e339b2f` |
| 5.18 | Watermark.Render 紧密循环渲染性能 | 🟡 | `df642d01` |
| 5.19 | WindowNotificationManager 50ms 高频轮询 | 🟡 | `640897d6` |
| 6.1 | 预设颜色映射 Dictionary → FrozenDictionary | 🔵 | `dfb1554b` |
| 6.3 | TimerStatistic Dead Code | 🔵 | `41dacbae` |
| 1.1 | ListCollectionView CollectionChanged 匿名 Lambda — 经典泄漏模式 | 🔴 | `29452afe` |

---

## 目录

1. [🔴 严重问题（Critical）](#1--严重问题critical)
2. [🟠 高危问题（High）](#2--高危问题high)
3. [🟡 中等问题（Medium）](#3--中等问题medium)
4. [🔵 低危问题（Low）](#4--低危问题low)
5. [架构层面建议](#5-架构层面建议)
6. [总结与修复优先级](#6-总结与修复优先级)

---

## 1. 🔴 严重问题（Critical）

### 1.1 ListCollectionView CollectionChanged 匿名 Lambda — 经典泄漏模式

- **文件**：`src/AtomUI.Controls.Shared/Data/ListCollectionViews/ListCollectionView.cs`
- **方法**：`SetSource()`，第 706 行
- **问题描述**：

```csharp
if (source is INotifyCollectionChanged coll)
{
    coll.CollectionChanged += (_, args) => ProcessCollectionChanged(args);  // ❌ 匿名 lambda，无法取消
}
```

匿名 lambda 捕获 `this`（ListCollectionView）。若源集合的生命周期长于 CollectionView（如全局数据源），View 将永远不被 GC。

- **复现条件**：创建 ListCollectionView 绑定全局/静态集合，多次切换页面或主题
- **影响评估**：**极高** — 所有使用 `ListCollectionView` 的控件（ListView、Transfer、Cascader 等）在数据源生命周期长时严重泄漏
- **✅ 修复方案**：引入私有嵌套类 `WeakCollectionChangedForwarder`，采用 WeakReference 转发器模式替代直接 lambda 订阅。

  ```
  源集合  --强引用-->  WeakCollectionChangedForwarder  --弱引用-->  ListCollectionView
  ```

  - 源集合只持有转发器的强引用，不再直接持有 View 的强引用
  - View 被 GC 后，下次事件触发时转发器的 `TryGetTarget` 失败，自动调用 `Dispose()` 解订，自身也随即可被 GC
  - 同时保留 `IDisposable.Dispose()` 供 `ListView.HandleItemsSourcePropertyChanged` 在 ItemsSource 切换时主动提前释放自己创建的 View
  - `ListView` 无需在 `OnDetachedFromVisualTree` 中 Dispose（避免了控件重新 Attach 后 View 为 null 的 bug）

---

### 1.2 DataGridCollectionView — 同样的 CollectionChanged Lambda 泄漏

- **文件**：`src/AtomUI.Desktop.Controls.DataGrid/Data/DataGridCollectionView.cs`，第 212 行
- **问题描述**：与 1.1 完全相同的模式。
- **影响评估**：**极高** — 影响所有 DataGrid 实例
- **修复建议**：同 1.1

---

## 2. 🟠 高危问题（High）

### 2.1 AbstractNotifiableTransition 的 Subject<bool> 未实现 IDisposable

- **文件**：`src/AtomUI.Core/Animations/Transitions/AbstractNotifiableTransition.cs`
- **问题描述**：

```csharp
public abstract class AbstractNotifiableTransition : INotifiableTransition
{
    private readonly Subject<bool> _subject = new();
    public IObservable<bool> CompletedObservable => _subject;

    protected void NotifyTransitionCompleted()
    {
        _subject.OnNext(true);
        _subject.OnCompleted();  // Subject 变为终态
    }
}
```

类未实现 `IDisposable`。Transition 被中断或从未完成时，Subject 及其订阅者永远不会被清理。`OnCompleted()` 后 Subject 进入终态，复用时后续调用无效。

- **复现条件**：Transition 动画被中断；或 Transition 对象被复用
- **影响评估**：**高** — 未完成的 Transition 泄漏 Subject 及订阅者引用链
- **修复建议**：实现 `IDisposable`，在 `Dispose()` 中调用 `_subject?.Dispose()`。

---

## 3. 🟡 中等问题（Medium）

### 3.1 ThemeConfigProvider 大量使用时的性能影响

- **文件**：`src/AtomUI.Core/Theme/` 相关文件
- **问题描述**：每个 `ThemeConfigProvider` 执行完整独立的 Token 计算（Seed → Map → Alias → Component），多个 Provider 时 CPU 和内存开销显著。
- **影响评估**：**中等**
- **修复建议**：添加 Token 计算结果缓存、支持增量计算、延迟计算策略。

---

### 3.2 DataGrid 虚拟化场景下的潜在泄漏

- **文件**：`src/AtomUI.Desktop.Controls.DataGrid/` 相关文件
- **问题描述**：虚拟化模式下频繁创建和回收行/单元格容器，若回收时未正确清理事件订阅和绑定，容器可能仍持有旧数据项的引用。
- **影响评估**：**中等**
- **修复建议**：确保 `PrepareContainerForItemOverride` 和 `ClearContainerForItemOverride` 中正确管理所有事件订阅和绑定。

---

### 3.3 ColorPicker 控件复杂状态管理

- **文件**：`src/AtomUI.Desktop.Controls.ColorPicker/` 相关文件
- **问题描述**：多个子控件之间通过事件和绑定通信，控件销毁时若通信链未正确断开，可能导致循环引用。
- **影响评估**：**中等**
- **修复建议**：在 `OnDetachedFromVisualTree` 中断开所有子控件之间的事件订阅和绑定。

---

### 3.4 Form — Items.CollectionChanged += lambda

- **文件**：`src/AtomUI.Desktop.Controls/Form/Form.cs`，第 456 行
- **问题描述**：

```csharp
Items.CollectionChanged += (sender, args) => { InvalidateMeasure(); };
```

`Items` 被外部 CollectionView 替换时，旧订阅无法取消。

- **影响评估**：**中等**
- **修复建议**：改为命名方法 `HandleItemsCollectionChanged`。

---

## 4. 🔵 低危问题（Low）

### 4.1 AbstractDesignToken 使用反射进行 Token 值转换

- **文件**：`src/AtomUI.Core/Theme/TokenSystem/AbstractDesignToken.cs`
- **问题描述**：Token 值转换器通过反射发现和注册，一次性开销。
- **影响评估**：**低**
- **修复建议**：考虑使用 Source Generator 在编译时生成转换器注册代码。

---

### 4.2 Gallery 示例应用中的小问题

- **文件**：`controlgallery/` 下的各 ShowCase 文件
- **问题描述**：部分 ShowCase 控件存在与主库相同的事件订阅模式问题，可能误导开发者。
- **影响评估**：**低**
- **修复建议**：确保示例代码遵循最佳实践。

---

### 4.3 ImmutableSolidColorBrush 在主题系统中的频繁创建

- **文件**：`src/AtomUI.Core/Theme/` 相关文件
- **问题描述**：主题系统构建 `ResourceDictionary` 时将 Color Token 自动包装为 `ImmutableSolidColorBrush`，频繁主题切换时创建大量 Brush 对象。
- **影响评估**：**低** — 轻量对象，且主题切换不频繁
- **修复建议**：如性能分析显示热点，可缓存已创建的 Brush 对象。

---

## 6. 总结与修复优先级

### 6.1 问题统计

| 严重程度 | 总数 | 已修复 | 待修复 |
|----------|------|--------|--------|
| 🔴 Critical | 7 | 6 | 1 |
| 🟠 High | 8 | 7 | 1 |
| 🟡 Medium | 19 | 15 | 4 |
| 🔵 Low | 5 | 2 | 3 |
| **合计** | **39** | **30** | **9** |

### 6.2 待修复优先级

#### P0 — 立即修复

| # | 问题 | 预计工作量 |
|---|------|-----------|
| 1.2 | DataGridCollectionView 同上 | ~15 行代码 |

#### P1 — 尽快修复

| # | 问题 | 预计工作量 |
|---|------|-----------|
| 2.1 | AbstractNotifiableTransition IDisposable | ~15 行代码 |

#### P2 — 计划修复

| # | 问题 | 预计工作量 |
|---|------|-----------|
| 3.1 | ThemeConfigProvider 性能 | 中等 |
| 3.2 | DataGrid 虚拟化泄漏 | 中等 |
| 3.3 | ColorPicker 复杂状态管理 | 中等 |
| 3.4 | Form Items.CollectionChanged lambda | ~5 行代码 |

#### P3 — 长期优化

| # | 问题 | 预计工作量 |
|---|------|-----------|
| 4.1 | Token 值转换器反射 → Source Generator | 中等 |
| 4.2 | Gallery 示例最佳实践 | 小 |
| 4.3 | ImmutableSolidColorBrush 缓存 | 小 |


> 本报告基于静态代码分析生成，部分问题可能需要结合运行时分析工具进一步确认。建议按优先级逐步修复，并在每次修复后运行完整的测试套件验证无回归。

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
| ✅ 已修复 | 35 |
| ⏳ 待修复 | 4 |
| 修复进度 | **89.7%** |

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
| 3.3 | ColorPicker 匿名 Lambda / 重复订阅 / 双重订阅 | 🟡 | 本次修复 |
| 2.1 | AbstractNotifiableTransition Subject&lt;bool&gt; IDisposable | 🟠 | `fcf25b60` |
| 1.2 | DataGridCollectionView CollectionChanged 匿名 Lambda 泄漏 | 🔴 | `1f04fcee` |
| 3.2 | DataGrid 虚拟化场景下的潜在泄漏 | 🟡 | 本次修复 |

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

### ✅ 1.1 ListCollectionView CollectionChanged 匿名 Lambda — 经典泄漏模式（已修复 `29452afe`）

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

### ✅ 1.2 DataGridCollectionView — 同样的 CollectionChanged Lambda 泄漏（已修复）

- **文件**：`src/AtomUI.Desktop.Controls.DataGrid/Data/DataGridCollectionView.cs`，第 212 行
- **问题描述**：与 1.1 完全相同的模式。
- **影响评估**：**极高** — 影响所有 DataGrid 实例
- **✅ 修复方案**：同 1.1，引入 `WeakCollectionChangedForwarder` 嵌套类，实现 `IDisposable`。

---

## 2. 🟠 高危问题（High）

### ✅ 2.1 AbstractNotifiableTransition 的 Subject&lt;bool&gt; 未实现 IDisposable（已修复 `fcf25b60`）

- **文件**：`src/AtomUI.Core/Animations/Transitions/AbstractNotifiableTransition.cs`
- **问题描述**：类未实现 `IDisposable`。Transition 被中断或从未完成时，Subject 及其订阅者永远不会被清理。
- **影响评估**：**高** — 未完成的 Transition 泄漏 Subject 及订阅者引用链
- **✅ 修复方案**：三处协同修复：
  1. `INotifyTransitionCompleted` 接口继承 `IDisposable`
  2. `AbstractNotifiableTransition<T>` 实现 `Dispose()`，释放 `Subject<bool>`
  3. **调用端** `AbstractMotion.RunTransitions`：`await Task.WhenAny` 完成后（无论正常完成还是超时）对所有 Transitions 调用 `Dispose()`
  4. **调用端** `AbstractMotion.ConfigureTransitions`：Clear 前先 Dispose 旧 Transitions，防止重建时旧 Subject 泄漏

---

## 3. 🟡 中等问题（Medium）

### 3.1 ThemeConfigProvider 大量使用时的性能影响

- **文件**：`src/AtomUI.Core/Theme/` 相关文件
- **问题描述**：每个 `ThemeConfigProvider` 执行完整独立的 Token 计算（Seed → Map → Alias → Component），多个 Provider 时 CPU 和内存开销显著。
- **影响评估**：**中等**
- **修复建议**：添加 Token 计算结果缓存、支持增量计算、延迟计算策略。

---

### ✅ 3.2 DataGrid 虚拟化场景下的潜在泄漏（已修复）

- **文件**：`src/AtomUI.Desktop.Controls.DataGrid/Cell/DataGridCell.cs`
- **问题描述**：`DataGridCell.OnApplyTemplate` 中调用 `BindUtils.RelayBind` 创建了两个订阅（`CurrentSortingStateProperty` → `IsSortingProperty`、`HeaderDragModeProperty` → `OwningColumnDraggingProperty`），但返回的 `IDisposable` 被直接丢弃，导致：
  1. 若主题切换触发模板重新应用，每次 `OnApplyTemplate` 都会叠加新订阅，旧订阅永远无法释放
  2. 单元格从可视树移除时（DataGrid 销毁、列移除），`OwningColumn.HeaderCell` 仍通过订阅持有对 Cell 的引用，阻止 GC
- **影响评估**：**中等** — 每个可见单元格（行数 × 列数）均受影响，主题切换场景下订阅线性累积
- **✅ 修复方案**：
  - 新增 `_sortingStateSubscription` 和 `_headerDragModeSubscription` 两个 `IDisposable?` 字段
  - `OnApplyTemplate` 开头先 `Dispose` 旧订阅再创建新订阅，防止重复订阅累积
  - 新增 `OnDetachedFromVisualTree` 覆写，在单元格离开可视树时释放订阅，切断 HeaderCell → Cell 的引用链

---

### ✅ 3.3 ColorPicker 控件复杂状态管理（已修复）

- **文件**：`src/AtomUI.Desktop.Controls.ColorPicker/` 相关文件
- **问题描述**：多个子控件之间通过事件和绑定通信，控件销毁时若通信链未正确断开，可能导致循环引用。
- **影响评估**：**中等**
- **✅ 修复方案**：`ColorPickerInput`、`AbstractColorPickerView`、`ColorSliderThumb` 的 `OnApplyTemplate` 均已改为命名方法，并在开头先 `-=` 取消旧订阅再 `+=` 添加新订阅，彻底消除重复订阅和匿名 Lambda 泄漏。

---

### ~~3.4 Form — Items.CollectionChanged += lambda~~（误报，无需修复）

- **文件**：`src/AtomUI.Desktop.Controls/Form/Form.cs`，第 456 行
- **原始描述**：`Items` 被外部 CollectionView 替换时，旧订阅无法取消。
- **❌ 结论**：**误报**。`Items` 是 `Form`（`ItemsControl`）自身持有的内部 `ItemCollection`，与 `Form` 同生命周期，不可被外部替换（外部数据源通过 `ItemsSource` 绑定）。`Form` 被 GC 时订阅自然随之消失，不存在跨生命周期泄漏。无需任何改动。

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
| 🔴 Critical | 7 | 7 | 0 |
| 🟠 High | 8 | 8 | 0 |
| 🟡 Medium | 19 | 18 | 1 |
| 🔵 Low | 5 | 2 | 3 |
| **合计** | **39** | **35** | **4** |

### 6.2 待修复优先级

#### P0 — 立即修复

> 🎉 所有 P0 问题已全部修复。

#### P1 — 尽快修复

> 🎉 所有 P1 问题已全部修复。

#### P2 — 计划修复

| # | 问题 | 预计工作量 |
|---|------|-----------|
| 3.1 | ThemeConfigProvider 性能 | 中等 |
| ~~3.2~~ | ~~DataGrid 虚拟化泄漏~~ | ~~已修复~~ |
| ~~3.4~~ | ~~Form Items.CollectionChanged lambda~~ | ~~误报~~ |

#### P3 — 长期优化

| # | 问题 | 预计工作量 |
|---|------|-----------|
| 4.1 | Token 值转换器反射 → Source Generator | 中等 |
| 4.2 | Gallery 示例最佳实践 | 小 |
| 4.3 | ImmutableSolidColorBrush 缓存 | 小 |


> 本报告基于静态代码分析生成，部分问题可能需要结合运行时分析工具进一步确认。建议按优先级逐步修复，并在每次修复后运行完整的测试套件验证无回归。

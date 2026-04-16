# AtomUI 性能问题与内存泄漏分析报告

> 分析日期：2026-04-16  
> 目标框架：.NET 8 / .NET 10  
> UI 框架：Avalonia 11.3.x  
> 分析范围：`src/` 及 `controlgallery/` 下所有项目  
> 参考文档：`docs/` 目录下的架构设计文档

---

## 目录

1. [概述](#1-概述)
2. [严重程度分级说明](#2-严重程度分级说明)
3. [🔴 严重问题（Critical）](#3--严重问题critical)
4. [🟠 高危问题（High）](#4--高危问题high)
5. [🟡 中等问题（Medium）](#5--中等问题medium)
6. [🔵 低危问题（Low）](#6--低危问题low)
7. [架构层面分析与建议](#7-架构层面分析与建议)
8. [总结与修复优先级](#8-总结与修复优先级)

---

## 1. 概述

AtomUI 是基于 Avalonia UI 的 Ant Design 5.0 .NET 桌面控件库，采用三层架构：

- **基础层**（Foundation）：`AtomUI.Core`、`AtomUI.Controls.Shared`、`AtomUI.Native`
- **基础控件层**（Base Control）：`AtomUI.Controls` — 设备无关的抽象控件
- **平台控件层**（Platform Control）：`AtomUI.Desktop.Controls`（+ `.ColorPicker`、`.DataGrid`）— 桌面平台具体实现

本报告对上述所有层级的代码进行了全面的性能与内存泄漏静态分析，结合 `docs/` 中的架构设计文档进行交叉验证，重点关注 Avalonia/.NET 环境下的常见问题模式。

---

## 2. 严重程度分级说明

| 级别 | 含义 | 影响 |
|------|------|------|
| 🔴 Critical | 确定性 Bug，必然导致内存泄漏或严重性能问题 | 长时间运行必现，可能导致 OOM |
| 🟠 High | 高概率导致泄漏或性能退化 | 特定使用场景下必现 |
| 🟡 Medium | 潜在风险，特定条件下可能触发 | 需要特定操作序列触发 |
| 🔵 Low | 设计层面的改进建议 | 影响较小但值得优化 |

---

## 3. 🔴 严重问题（Critical）

### 3.1 ✅ ~~TransformTrackingHelper 事件订阅运算符错误导致永久内存泄漏~~（已修复）

- **文件**：`src/AtomUI.Core/Input/TransformTrackingHelper.cs`
- **方法**：`SetVisual(Visual? visual)`，第 39 行
- **问题描述**：

在 `SetVisual` 方法中，`DetachedFromVisualTree` 事件使用了 `-=`（取消订阅）而非 `+=`（订阅）：

```csharp
// 第 38-39 行
visual.AttachedToVisualTree   += OnAttachedToVisualTree;
visual.DetachedFromVisualTree -= OnDetachedFromVisualTree;  // BUG: 应为 +=
```

由于 `OnDetachedFromVisualTree` 永远不会被调用，`UnsubscribeFromParents()` 也永远不会执行。这意味着在 `SubscribeToParents()`（第 65-80 行）中对所有父级 Visual 的 `PropertyChanged` 订阅将永久存在，即使控件已从视觉树中移除。

- **复现条件**：任何使用 `TransformTrackingHelper` 的控件被添加到视觉树后再移除
- **影响评估**：**极高** — 每次控件 attach/detach 循环都会累积泄漏的 PropertyChanged 订阅，持有整个父级 Visual 链的强引用，阻止 GC 回收
- **修复建议**：

```csharp
// 修复：将 -= 改为 +=
public void SetVisual(Visual? visual)
{
    Dispose();
    _visual = visual;
    if (visual != null)
    {
        visual.AttachedToVisualTree   += OnAttachedToVisualTree;
        visual.DetachedFromVisualTree += OnDetachedFromVisualTree;  // 修复
        if (visual.GetVisualRoot() is not null)
        {
            SubscribeToParents();
        }
        UpdateMatrix();
    }
}
```

---

### 3.2 ✅ ~~SwitchKnob.OnDetachedFromVisualTree 调用了错误的基类方法~~（已修复）

- **文件**：`src/AtomUI.Controls/Switch/SwitchKnob.cs`
- **方法**：`OnDetachedFromVisualTree`，第 255 行
- **问题描述**：

```csharp
// 第 255 行
protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
{
    base.OnAttachedToVisualTree(e);  // BUG: 应为 base.OnDetachedFromVisualTree(e)
    // ...
}
```

调用了 `base.OnAttachedToVisualTree(e)` 而非 `base.OnDetachedFromVisualTree(e)`。这导致基类的 detach 清理逻辑永远不会执行，同时错误地再次触发 attach 逻辑。

- **复现条件**：SwitchKnob 控件从视觉树中移除时
- **影响评估**：**极高** — 基类中所有在 `OnDetachedFromVisualTree` 中执行的清理操作（事件取消订阅、资源释放等）全部失效，同时 attach 逻辑被错误重复执行
- **修复建议**：

```csharp
protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
{
    base.OnDetachedFromVisualTree(e);  // 修复：调用正确的基类方法
    // ... 其余清理逻辑
}
```

---

### 3.3 ✅ ~~TimerStatistic 定时器泄漏 — 旧定时器未停止/释放~~（已修复）

- **文件**：`src/AtomUI.Desktop.Controls/Statistic/TimerStatistic.cs`
- **方法**：`BuildTimer(bool start)`，第 141-150 行
- **问题描述**：

每次 `Value` 或 `RefreshDuration` 属性变化时，`BuildTimer()` 会创建新的 `DispatcherTimer`，但不会停止或取消订阅旧定时器的 `Tick` 事件：

```csharp
// 第 141-150 行
private void BuildTimer(bool start)
{
    _timer          =  new DispatcherTimer();       // 旧 _timer 引用被覆盖，但旧定时器仍在运行！
    _timer.Interval =  RefreshDuration;
    _timer.Tick     += HandleTickElapsed;
    if (start)
    {
        _timer.Start();
    }
}
```

此外，`OnDetachedFromVisualTree`（第 90-94 行）只停止定时器但不取消 Tick 事件订阅：

```csharp
protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
{
    base.OnDetachedFromVisualTree(e);
    _timer?.Stop();
    // 缺少：_timer.Tick -= HandleTickElapsed; 和 _timer = null;
}
```

- **复现条件**：多次修改 TimerStatistic 的 Value 或 RefreshDuration 属性
- **影响评估**：**极高** — 每次属性变化都会泄漏一个 DispatcherTimer，旧定时器持续触发 Tick 事件，导致：1) 内存泄漏；2) 多个定时器同时运行导致 CPU 浪费和逻辑错误
- **修复建议**：

```csharp
private void BuildTimer(bool start)
{
    // 先清理旧定时器
    if (_timer != null)
    {
        _timer.Stop();
        _timer.Tick -= HandleTickElapsed;
    }
    
    _timer          =  new DispatcherTimer();
    _timer.Interval =  RefreshDuration;
    _timer.Tick     += HandleTickElapsed;
    if (start)
    {
        _timer.Start();
    }
}

protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
{
    base.OnDetachedFromVisualTree(e);
    if (_timer != null)
    {
        _timer.Stop();
        _timer.Tick -= HandleTickElapsed;
        _timer = null;
    }
}
```

---

### 3.4 ListCollectionView CollectionChanged 匿名 Lambda — 经典泄漏模式

- **文件**：`src/AtomUI.Controls.Shared/Data/ListCollectionViews/ListCollectionView.cs`
- **方法**：`SetSource()`，第 706 行
- **问题描述**：

```csharp
// 第 706 行
if (source is INotifyCollectionChanged coll)
{
    coll.CollectionChanged += (_, args) => ProcessCollectionChanged(args);  // ❌ 匿名 lambda，无法取消
}
```

匿名 lambda 捕获 `this`（ListCollectionView）。若源集合（`coll`，即 `INotifyCollectionChanged`）的生命周期长于 CollectionView（如全局数据源），View 将永远不被 GC。这是 .NET 中最经典的 `CollectionChanged` 泄漏模式。

- **复现条件**：创建 ListCollectionView 绑定全局/静态集合，多次切换页面或主题
- **影响评估**：**极高** — 所有使用 `ListCollectionView` 的控件（ListView、Transfer、Cascader 等）在数据源生命周期长时严重泄漏
- **修复建议**：

```csharp
// 方案 1：使用 WeakEventManager（推荐）
if (source is INotifyCollectionChanged coll)
{
    WeakEventManager<INotifyCollectionChanged, NotifyCollectionChangedEventArgs>
        .AddHandler(coll, nameof(INotifyCollectionChanged.CollectionChanged), 
                    HandleSourceCollectionChanged);
}

private void HandleSourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs args)
{
    ProcessCollectionChanged(args);
}

// 方案 2：命名方法 + 在 Dispose 中取消订阅
```

---

### 3.5 DataGridCollectionView — 同样的 CollectionChanged Lambda 泄漏

- **文件**：`src/AtomUI.Desktop.Controls.DataGrid/Data/DataGridCollectionView.cs`
- **方法**：第 212 行
- **问题描述**：

与 3.4 完全相同的模式：`coll.CollectionChanged += (_, args) => ProcessCollectionChanged(args);`

- **复现条件**：DataGrid 绑定长生命周期数据源
- **影响评估**：**极高** — 影响所有 DataGrid 实例
- **修复建议**：同 3.4

---

### 3.6 InfoPickerInput — OnApplyTemplate 中 Lambda 事件重复叠加注册

- **文件**：`src/AtomUI.Desktop.Controls/Primitives/InfoPickerInput/InfoPickerInput.cs`
- **方法**：`OnApplyTemplate()`，第 454-495 行
- **问题描述**：

在 `OnApplyTemplate` 中对以下事件使用匿名 lambda 订阅，模板每次重新应用（如主题切换、样式更新）都会叠加新的订阅，不移除旧的：

```csharp
// 第 479-495 行
PickerFlyout.Opened += (sender, args) => { ... };           // ❌ 每次叠加
PickerFlyout.Closed += (sender, args) => { ... };           // ❌
PickerFlyout.PresenterCreated += (sender, args) => { ... }; // ❌
DecoratedBox.TemplateApplied += (sender, args) => { ... };  // ❌
DecoratedBox.PropertyChanged += (sender, args) => { ... };  // ❌
PickerClearUpButton.ClearRequest += (sender, args) => { ... }; // ❌
```

- **复现条件**：频繁切换主题或触发模板重新应用
- **影响评估**：**极高** — 回调数量随模板重新应用次数线性增长。影响所有派生自 InfoPickerInput 的控件（DatePicker、TimePicker 等）
- **修复建议**：

```csharp
// 将 lambda 存储为字段，在 OnApplyTemplate 开头先移除旧订阅
protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
{
    // 先移除旧订阅
    if (PickerFlyout != null)
    {
        PickerFlyout.Opened -= _flyoutOpenedHandler;
        PickerFlyout.Closed -= _flyoutClosedHandler;
        PickerFlyout.PresenterCreated -= _flyoutPresenterCreatedHandler;
    }
    if (DecoratedBox != null)
    {
        DecoratedBox.TemplateApplied -= HandleDecoratedBoxTemplateApplied;
        DecoratedBox.PropertyChanged -= HandleDecoratedBoxPropertyChanged;
    }
    
    base.OnApplyTemplate(e);
    // ... find template parts, then re-subscribe with named methods ...
}
```

---

### 3.7 ColorPickerInput — OnApplyTemplate 中 8 个 ValueChanged Lambda 叠加

- **文件**：`src/AtomUI.Desktop.Controls.ColorPicker/ColorView/ColorPickerInput.cs`
- **方法**：`OnApplyTemplate()`，第 118-240 行
- **问题描述**：

`OnApplyTemplate` 中对 8 个输入控件的 `ValueChanged`/`TextChanged` 事件使用匿名 lambda：

```csharp
_alphaInput.ValueChanged += (sender, args) => { ... };  // ❌
_hexValueInput.TextChanged += (sender, args) => { ... }; // ❌
_hValueInput.ValueChanged += (sender, args) => { ... };  // ❌
_sValueInput.ValueChanged += (sender, args) => { ... };  // ❌
_vValueInput.ValueChanged += (sender, args) => { ... };  // ❌
_rValueInput.ValueChanged += (sender, args) => { ... };  // ❌
_gValueInput.ValueChanged += (sender, args) => { ... };  // ❌
_bValueInput.ValueChanged += (sender, args) => { ... };  // ❌
```

模板重新应用时全部叠加，导致 8× 回调执行次数。

- **复现条件**：ColorPicker 模板重新应用（主题切换等）
- **影响评估**：**极高** — 比 3.6 更严重，8 个事件同时叠加
- **修复建议**：同 3.6，将所有 lambda 改为命名方法，在 OnApplyTemplate 开头先移除旧订阅。

---

## 4. 🟠 高危问题（High）

### 4.1 AbstractNotifiableTransition 的 Subject\<bool\> 未实现 IDisposable

- **文件**：`src/AtomUI.Core/Animations/Transitions/AbstractNotifiableTransition.cs`
- **方法**：整个类，第 13-33 行
- **问题描述**：

```csharp
public abstract class AbstractNotifiableTransition : INotifiableTransition
{
    private readonly Subject<bool> _subject = new();  // 第 17 行

    public IObservable<bool> CompletedObservable => _subject;

    protected void NotifyTransitionCompleted()
    {
        _subject.OnNext(true);
        _subject.OnCompleted();  // Subject 变为终态，后续订阅者无法收到通知
    }
}
```

`Subject<bool>` 在实例创建时分配，但类未实现 `IDisposable`。如果 Transition 被中断或从未完成，Subject 及其订阅者将永远不会被清理。此外，`OnCompleted()` 调用后 Subject 进入终态，如果 Transition 对象被复用，后续的 `NotifyTransitionCompleted()` 调用将无效。

- **复现条件**：Transition 动画被中断（如控件在动画过程中被移除）；或 Transition 对象被复用
- **影响评估**：**高** — 未完成的 Transition 会泄漏 Subject 及其所有订阅者的引用链
- **修复建议**：

```csharp
public abstract class AbstractNotifiableTransition : INotifiableTransition, IDisposable
{
    private Subject<bool>? _subject = new();

    public IObservable<bool> CompletedObservable => _subject ?? Observable.Empty<bool>();

    protected void NotifyTransitionCompleted()
    {
        _subject?.OnNext(true);
        _subject?.OnCompleted();
    }

    public void Dispose()
    {
        _subject?.Dispose();
        _subject = null;
    }
}
```

---

### 4.2 Carousel 自动播放定时器泄漏

- **文件**：`src/AtomUI.Desktop.Controls/Carousel/Carousel.cs`
- **方法**：`BuildAutoPlayTimer()`，第 703-716 行；按钮事件订阅，第 346-352 行
- **问题描述**：

`BuildAutoPlayTimer()` 在 `IsAutoPlay` 变为 true 时创建新的 `DispatcherTimer`，但如果多次调用（例如快速切换 IsAutoPlay），旧定时器的 `Tick` 事件处理器 `HandleAutoPlayTick` 不会被取消订阅：

```csharp
private void BuildAutoPlayTimer()
{
    _autoPlayTimer = new DispatcherTimer  // 旧定时器引用被覆盖
    {
        Interval = TimeSpan.FromMilliseconds(AutoPlayDuration)
    };
    _autoPlayTimer.Tick += HandleAutoPlayTick;
    _autoPlayTimer.Start();
}
```

此外，`_previousButton.Click` 和 `_nextButton.Click` 事件订阅（第 346-352 行）在模板重新应用时不会取消旧订阅。

- **复现条件**：多次切换 Carousel 的 IsAutoPlay 属性
- **影响评估**：**高** — 泄漏的定时器持续触发，导致多个定时器同时驱动轮播
- **修复建议**：

```csharp
private void BuildAutoPlayTimer()
{
    StopAutoPlayTimer();  // 先清理旧定时器
    
    _autoPlayTimer = new DispatcherTimer
    {
        Interval = TimeSpan.FromMilliseconds(AutoPlayDuration)
    };
    _autoPlayTimer.Tick += HandleAutoPlayTick;
    _autoPlayTimer.Start();
}

private void StopAutoPlayTimer()
{
    if (_autoPlayTimer != null)
    {
        _autoPlayTimer.Stop();
        _autoPlayTimer.Tick -= HandleAutoPlayTick;
        _autoPlayTimer = null;
    }
}
```

---

### 4.3 Drawer.OpenOn SizeChanged 事件未在 Detach 时取消订阅

- **文件**：`src/AtomUI.Desktop.Controls/Drawer/Drawer.cs`
- **方法**：OpenOn 属性变更处理，第 318 行
- **问题描述**：

```csharp
newOpenOn.SizeChanged += HandleOpenOnSizeChanged;  // 第 318 行
```

`SizeChanged` 事件仅在 `OpenOnProperty` 再次变化时取消旧订阅。如果 Drawer 从视觉树中移除但 `OpenOn` 未变化，事件处理器将持续持有 Drawer 的强引用，阻止 Drawer 被 GC 回收。

此外，`_containerDisposables`（DrawerContainer 的 relay bindings）在 `OnDetachedFromVisualTree` 中未被 Dispose。

- **复现条件**：Drawer 绑定了 OpenOn 后从视觉树中移除
- **影响评估**：**高** — Drawer 及其整个子树无法被 GC 回收
- **修复建议**：

```csharp
protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
{
    base.OnDetachedFromVisualTree(e);
    
    // 清理 OpenOn 事件订阅
    if (OpenOn is { } openOn)
    {
        openOn.SizeChanged -= HandleOpenOnSizeChanged;
    }
    
    // 清理容器绑定
    _containerDisposables?.Dispose();
    _containerDisposables = null;
}
```

---

### 4.4 Tour 控件多处事件订阅未取消

- **文件**：`src/AtomUI.Desktop.Controls/Tour/Tour.cs`
- **方法**：构造函数，第 311-312 行；模板应用，第 534-535 行
- **问题描述**：

构造函数中订阅了 `CollectionChanged` 事件但从未取消：

```csharp
// 第 311-312 行（构造函数中）
Steps.CollectionChanged += HandleItemsViewCollectionChanged;
CustomActions.CollectionChanged += HandleCustomActionsChanged;
```

模板应用中的 `CloseRequest` 和 `NavRequest` 事件处理器仅在重新模板化时清理，不在 detach 时清理。`_indicatorDisposables` 也仅在 `IndicatorProperty` 变化时 Dispose，不在 detach 时 Dispose。

- **复现条件**：Tour 控件被创建后从视觉树中移除
- **影响评估**：**高** — Steps/CustomActions 集合持有 Tour 的强引用
- **修复建议**：

```csharp
protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
{
    base.OnDetachedFromVisualTree(e);
    
    Steps.CollectionChanged -= HandleItemsViewCollectionChanged;
    CustomActions.CollectionChanged -= HandleCustomActionsChanged;
    
    _indicatorDisposables?.Dispose();
    _indicatorDisposables = null;
    
    // 清理 StepsView 事件
    if (_stepsView != null)
    {
        _stepsView.CloseRequest -= HandleCloseRequest;
        _stepsView.NavRequest -= HandleNavRequest;
    }
}
```

---

### 4.5 OptionButtonGroup 匿名 Lambda 事件订阅无法取消

- **文件**：`src/AtomUI.Controls/OptionButtonGroup/AbstractOptionButtonGroup.cs`
- **方法**：第 124 行、第 189 行
- **问题描述**：

```csharp
// 第 124 行 — 匿名 lambda，无法取消订阅
childIndexProvider.ChildIndexChanged += (sender, args) => { UpdateOptionButtonsPosition(); };

// 第 189 行 — 命名方法但仅在 PrepareContainerForItemOverride 中订阅
optionButton.IsCheckedChanged += HandleOptionButtonChecked;
// ClearContainerForItemOverride 中未取消订阅
```

匿名 lambda 捕获了 `this`，且由于没有保存委托引用，无法在任何时候取消订阅。

- **复现条件**：OptionButtonGroup 被创建并使用
- **影响评估**：**高** — childIndexProvider 通过 lambda 持有 OptionButtonGroup 的强引用，阻止 GC
- **修复建议**：

```csharp
// 将匿名 lambda 改为命名方法
private void HandleChildIndexChanged(object? sender, EventArgs args)
{
    UpdateOptionButtonsPosition();
}

// 订阅时使用命名方法
childIndexProvider.ChildIndexChanged += HandleChildIndexChanged;

// 在适当的清理点取消订阅
childIndexProvider.ChildIndexChanged -= HandleChildIndexChanged;

// ClearContainerForItemOverride 中取消 IsCheckedChanged 订阅
protected override void ClearContainerForItemOverride(Control container)
{
    if (container is OptionButton optionButton)
    {
        optionButton.IsCheckedChanged -= HandleOptionButtonChecked;
    }
    base.ClearContainerForItemOverride(container);
}
```

---

### 4.6 静态 ConcurrentDictionary 缓存无大小限制

- **文件**：多个文件
  - `src/AtomUI.Core/Controls/Icon/IconProviderCache.cs`，第 7 行
  - `src/AtomUI.Core/Theme/TokenSystem/TokenResourceCache.cs`（类似模式）
  - `src/AtomUI.Core/Theme/TokenSystem/AbstractDesignToken.cs`，第 14 行
- **问题描述**：

```csharp
// IconProviderCache.cs 第 7 行
static readonly ConcurrentDictionary<Type, ConcurrentDictionary<object, IconInfo>> _cache = new();
```

这些静态缓存只增不减，没有大小限制、过期策略或 WeakReference 机制。在长时间运行的应用中，如果不断创建新的图标类型或 Token 组合，缓存将无限增长。

- **复现条件**：长时间运行的应用，持续创建不同类型的图标或主题配置
- **影响评估**：**高** — 在极端场景下可能导致内存持续增长
- **修复建议**：

```csharp
// 方案 1：使用 WeakReference 缓存
static readonly ConcurrentDictionary<Type, WeakReference<ConcurrentDictionary<object, IconInfo>>> _cache = new();

// 方案 2：添加大小限制和 LRU 淘汰策略
public static void TrimCache(int maxSize)
{
    while (_cache.Count > maxSize)
    {
        // 移除最早添加的条目
        var oldest = _cache.Keys.First();
        _cache.TryRemove(oldest, out _);
    }
}

// 方案 3：提供显式清理 API
public static void ClearCache() => _cache.Clear();
```

---

### 4.7 NotificationCard 关闭定时器生命周期问题

- **文件**：`src/AtomUI.Desktop.Controls/Notifications/NotificationCard.cs`
- **问题描述**：

NotificationCard 使用 `DispatcherTimer` 实现自动关闭功能。当通知被手动关闭或从视觉树移除时，定时器的 Tick 事件处理器可能未被正确取消订阅，导致定时器持续持有 NotificationCard 的引用。

- **复现条件**：快速创建和关闭多个通知
- **影响评估**：**高** — 频繁创建通知的场景下，已关闭的 NotificationCard 无法被 GC 回收
- **修复建议**：确保在所有关闭路径（手动关闭、自动关闭、从视觉树移除）中都取消定时器的 Tick 订阅并停止定时器。

---

### 4.8 多个 async 方法缺少 CancellationToken 支持

- **文件**：多个文件
  - `src/AtomUI.Desktop.Controls/Dialog/DialogService.cs`
  - `src/AtomUI.Desktop.Controls/Message/MessageService.cs`
  - `src/AtomUI.Desktop.Controls/Upload/Upload.cs`
  - `src/AtomUI.Desktop.Controls/AutoComplete/AutoComplete.cs`
- **问题描述**：

多个 async 方法不接受 `CancellationToken` 参数，也不在内部使用取消机制：

```csharp
// 典型模式 — 缺少 CancellationToken
public async Task ShowAsync(DialogConfig config)
{
    // ... 无法取消的异步操作
    await someOperation;
}
```

当控件被销毁或用户导航离开时，这些异步操作将继续执行，持有控件和相关对象的引用。

- **复现条件**：在异步操作进行中销毁控件或导航离开
- **影响评估**：**高** — 异步操作的闭包持有控件引用，阻止 GC 回收；可能导致在已销毁的控件上执行 UI 操作
- **修复建议**：

```csharp
// 1. 为所有公开的 async 方法添加 CancellationToken 参数
public async Task ShowAsync(DialogConfig config, CancellationToken cancellationToken = default)
{
    await someOperation.WaitAsync(cancellationToken);
}

// 2. 在控件中维护 CancellationTokenSource
private CancellationTokenSource? _cts;

protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
{
    base.OnAttachedToVisualTree(e);
    _cts = new CancellationTokenSource();
}

protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
{
    base.OnDetachedFromVisualTree(e);
    _cts?.Cancel();
    _cts?.Dispose();
    _cts = null;
}
```

---

## 5. 🟡 中等问题（Medium）

### 5.1 Segmented 控件 SelectionChanged 事件在构造函数中订阅但从未取消

- **文件**：`src/AtomUI.Controls/Segmented/AbstractSegmented.cs`
- **方法**：构造函数，第 115 行
- **问题描述**：

```csharp
// 第 115 行（构造函数中）
SelectionChanged += HandleSelectionChanged;
```

控件在构造函数中订阅了自身的 `SelectionChanged` 事件，但在 `OnDetachedFromVisualTree` 或任何其他清理方法中都没有取消订阅。虽然这是自身事件的自订阅（self-subscription），在大多数情况下不会导致外部泄漏，但如果控件被其他对象通过事件链引用，可能形成意外的引用环。

- **复现条件**：Segmented 控件正常使用
- **影响评估**：**中等** — 自订阅通常不会导致泄漏，但不符合最佳实践
- **修复建议**：

```csharp
protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
{
    base.OnDetachedFromVisualTree(e);
    SelectionChanged -= HandleSelectionChanged;
}

protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
{
    base.OnAttachedToVisualTree(e);
    SelectionChanged += HandleSelectionChanged;
}
```

---

### 5.2 FlyoutStateHelper 定时器使用 Lambda 捕获 this

- **文件**：`src/AtomUI.Desktop.Controls/Flyouts/FlyoutStateHelper.cs`
- **方法**：`StartMouseEnterTimer()` / `StartMouseLeaveTimer()`，第 144-196 行
- **问题描述**：

```csharp
// 第 144-160 行
private void StartMouseEnterTimer()
{
    _mouseEnterDelayTimer = new DispatcherTimer { Interval = ... };
    _mouseEnterDelayTimer.Tick += (sender, args) =>  // Lambda 捕获 this
    {
        _mouseEnterDelayTimer?.Stop();
        _mouseEnterDelayTimer = null;
        // ... 使用 this 的成员
    };
    _mouseEnterDelayTimer.Start();
}
```

每次调用 `StartMouseEnterTimer()` / `StartMouseLeaveTimer()` 都会创建新的 `DispatcherTimer` 并使用 lambda 订阅 Tick 事件。Lambda 捕获了 `this`。虽然定时器在触发后会被停止和置空，但在快速鼠标移入/移出场景下，可能产生短暂的多个定时器实例。

- **复现条件**：快速在 Flyout 触发区域上移入/移出鼠标
- **影响评估**：**中等** — 正常情况下定时器会被正确清理，但极端场景下可能有短暂泄漏
- **修复建议**：

```csharp
private void StartMouseEnterTimer()
{
    StopMouseEnterTimer();  // 确保先清理旧定时器
    
    _mouseEnterDelayTimer = new DispatcherTimer { Interval = ... };
    _mouseEnterDelayTimer.Tick += HandleMouseEnterTimerTick;  // 使用命名方法
    _mouseEnterDelayTimer.Start();
}

private void HandleMouseEnterTimerTick(object? sender, EventArgs e)
{
    StopMouseEnterTimer();
    // ... 原有逻辑
}

private void StopMouseEnterTimer()
{
    if (_mouseEnterDelayTimer != null)
    {
        _mouseEnterDelayTimer.Stop();
        _mouseEnterDelayTimer.Tick -= HandleMouseEnterTimerTick;
        _mouseEnterDelayTimer = null;
    }
}
```

---

### 5.3 多个控件的 CompositeDisposable 未在 Detach 时 Dispose

- **文件**：多个控件文件
  - `src/AtomUI.Desktop.Controls/Drawer/Drawer.cs` — `_containerDisposables`
  - `src/AtomUI.Desktop.Controls/Tour/Tour.cs` — `_indicatorDisposables`
  - `src/AtomUI.Desktop.Controls/DatePicker/DatePicker.cs` — 类似模式
  - `src/AtomUI.Desktop.Controls/Cascader/Cascader.cs` — 类似模式
- **问题描述**：

多个控件使用 `CompositeDisposable` 来管理绑定和订阅的生命周期，但这些 disposable 集合仅在属性变化或重新模板化时被 Dispose，而不在 `OnDetachedFromVisualTree` 中清理。

```csharp
// 典型模式
private CompositeDisposable? _someDisposables;

// 仅在属性变化时清理
private void OnSomePropertyChanged(...)
{
    _someDisposables?.Dispose();
    _someDisposables = new CompositeDisposable();
    // ... 添加新的订阅
}

// OnDetachedFromVisualTree 中缺少清理
```

- **复现条件**：控件从视觉树中移除但属性未变化
- **影响评估**：**中等** — 绑定订阅持续存在，可能阻止相关对象被 GC 回收
- **修复建议**：在所有使用 `CompositeDisposable` 的控件中，确保在 `OnDetachedFromVisualTree` 中调用 Dispose。

---

### 5.4 MarqueeLabel 动画资源清理

- **文件**：`src/AtomUI.Desktop.Controls/MarqueeLabel/MarqueeLabel.cs`
- **问题描述**：

MarqueeLabel 使用持续运行的动画实现文字滚动效果。当控件从视觉树中移除时，需要确保动画被正确停止和清理。如果动画的 `Completed` 事件处理器未被取消，或动画对象未被释放，将导致控件无法被 GC 回收。

- **复现条件**：MarqueeLabel 从视觉树中移除
- **影响评估**：**中等** — 动画持续运行消耗 CPU，且持有控件引用
- **修复建议**：在 `OnDetachedFromVisualTree` 中停止所有动画，取消事件订阅，释放动画资源。

---

### 5.5 ThemeConfigProvider 大量使用时的性能影响

- **文件**：`src/AtomUI.Core/Theme/` 相关文件
- **设计文档参考**：`docs/Core/ThemeSystem/`
- **问题描述**：

根据设计文档，每个 `ThemeConfigProvider` 会执行**完整独立的 Token 计算**——创建全新的 `DesignToken` 和所有已注册的 `ControlDesignToken` 实例，并运行完整的算法推导链（Seed → Map → Alias → Component）。文档明确警告：

> "在页面上大量使用 ThemeConfigProvider 可能有性能影响"

如果一个页面上有多个 `ThemeConfigProvider`，每个都会触发完整的 Token 计算，包括反射操作和 ResourceDictionary 构建。

- **复现条件**：页面上使用多个 ThemeConfigProvider
- **影响评估**：**中等** — 初始化时的 CPU 开销和内存分配
- **修复建议**：

```
1. 考虑为 ThemeConfigProvider 添加 Token 计算结果缓存
2. 对于仅覆盖少量 Token 的场景，支持增量计算而非全量重算
3. 在文档中明确建议用户限制 ThemeConfigProvider 的使用数量
4. 考虑延迟计算（lazy evaluation）策略
```

---

### 5.6 ToolTip 服务的全局事件订阅

- **文件**：`src/AtomUI.Desktop.Controls/Tooltip/` 相关文件
- **问题描述**：

ToolTip 服务通常通过全局事件（如 `PointerMoved`、`PointerEntered`）来管理 ToolTip 的显示/隐藏。这些全局事件订阅如果未正确管理，可能导致：
1. 已销毁的控件仍然接收事件
2. 事件处理器持有控件的强引用

- **复现条件**：带有 ToolTip 的控件被频繁创建和销毁
- **影响评估**：**中等** — 取决于具体实现，可能导致已销毁控件无法被 GC
- **修复建议**：确保 ToolTip 相关的全局事件订阅在控件 detach 时正确清理，或使用弱事件模式。

---

### 5.7 DataGrid 虚拟化场景下的潜在泄漏

- **文件**：`src/AtomUI.Desktop.Controls.DataGrid/` 相关文件
- **问题描述**：

DataGrid 在虚拟化模式下会频繁创建和回收行/单元格容器。如果容器回收时未正确清理事件订阅和绑定，可能导致：
1. 回收的容器仍持有旧数据项的引用
2. 事件处理器在容器回收后仍然活跃

- **复现条件**：DataGrid 使用虚拟化并频繁滚动
- **影响评估**：**中等** — 虚拟化场景下的泄漏会随滚动累积
- **修复建议**：确保 `PrepareContainerForItemOverride` 和 `ClearContainerForItemOverride` 中正确管理所有事件订阅和绑定。

---

### 5.8 ColorPicker 控件复杂状态管理

- **文件**：`src/AtomUI.Desktop.Controls.ColorPicker/` 相关文件
- **问题描述**：

ColorPicker 包含多个子控件（色相滑块、饱和度面板、透明度滑块等），这些子控件之间通过事件和绑定进行通信。如果这些内部通信链在控件销毁时未正确断开，可能导致子控件之间的循环引用。

- **复现条件**：ColorPicker 控件被创建后销毁
- **影响评估**：**中等** — 整个 ColorPicker 子树可能无法被 GC 回收
- **修复建议**：在 `OnDetachedFromVisualTree` 中断开所有子控件之间的事件订阅和绑定。

---

### 5.9 Watermark — glyph.PropertyChanged += lambda（无法取消）

- **文件**：`src/AtomUI.Controls/Watermark/Watermark.cs`
- **方法**：构造函数，第 44 行
- **问题描述**：

```csharp
// 第 44 行
glyph.PropertyChanged += (sender, args) => { InvalidateVisual(); };
```

Lambda 捕获 `this`（Watermark），无法通过 `-=` 取消。当 Watermark 从视觉树移除但 glyph 仍被持有时，Watermark 不能被 GC。

- **复现条件**：Watermark 从视觉树移除但 glyph 对象仍存活
- **影响评估**：**中等** — 取决于 glyph 的生命周期
- **修复建议**：

```csharp
private void HandleGlyphPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs args)
{
    InvalidateVisual();
}

protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
{
    base.OnDetachedFromVisualTree(e);
    if (Glyph != null)
    {
        Glyph.PropertyChanged -= HandleGlyphPropertyChanged;
    }
}
```

---

### 5.10 TreeView / CascaderView 异步加载 — CancellationTokenSource 未 Dispose

- **文件**：
  - `src/AtomUI.Desktop.Controls/TreeView/TreeView.AsyncItemDataLoad.cs`，第 38 行
  - `src/AtomUI.Desktop.Controls/Cascader/CascaderView.AsyncItemDataLoad.cs`，第 35 行
- **问题描述**：

```csharp
var cts = new CancellationTokenSource(); // TODO 做一个超时结束
viewItem.IsLoading = true;
Dispatcher.UIThread.InvokeAsync(async () =>
{
    var result = await DataLoader.LoadAsync(treeItemData, cts.Token);
    // ... 处理结果
});
```

1. CTS 创建后从不 Dispose（CTS 持有 WaitHandle 等非托管资源）
2. 没有超时机制（代码中有 TODO 注释但未实现）
3. 控件在异步操作期间被销毁时，回调仍会更新已分离控件状态
4. `Dispatcher.UIThread.InvokeAsync` 返回值未保存，无法取消

- **复现条件**：TreeView/Cascader 频繁触发异步加载
- **影响评估**：**中等** — 累积 CTS 对象，潜在无限增长
- **修复建议**：

```csharp
private List<CancellationTokenSource>? _loadingTokens;

private void HandleNodeLoadRequest(TreeViewItem viewItem)
{
    var cts = new CancellationTokenSource();
    cts.CancelAfter(TimeSpan.FromSeconds(30)); // 30 秒超时
    _loadingTokens ??= new();
    _loadingTokens.Add(cts);
    
    viewItem.IsLoading = true;
    Dispatcher.UIThread.InvokeAsync(async () =>
    {
        try
        {
            var result = await DataLoader.LoadAsync(treeItemData, cts.Token);
            if (!cts.Token.IsCancellationRequested)
            {
                // ... 处理结果
            }
        }
        catch (OperationCanceledException) { }
        finally
        {
            cts.Dispose();
            _loadingTokens?.Remove(cts);
        }
    });
}

protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
{
    base.OnDetachedFromVisualTree(e);
    if (_loadingTokens != null)
    {
        foreach (var cts in _loadingTokens) { cts.Cancel(); cts.Dispose(); }
        _loadingTokens.Clear();
    }
}
```

---

### 5.11 async void 方法 — 无异常保护 + 无取消支持

- **文件**：
  - `src/AtomUI.Desktop.Controls.ColorPicker/ColorView/ColorSpectrum.cs`，第 1135 行 — `CreateBitmapsAndColorMap()`
  - `src/AtomUI.Desktop.Controls.ColorPicker/ColorSlider/ColorSlider.cs`，第 209 行 — `UpdateBackground()`
  - `src/AtomUI.Desktop.Controls.DataGrid/DataGrid.Privates.cs`，第 4654 行 — `CopyToClipboard()`
  - `src/AtomUI.Desktop.Controls/TextBlock/SelectableTextBlock.cs`，第 125 行 — `Copy()`
- **问题描述**：

`async void` 方法中的异常会直接传播到 SynchronizationContext，导致应用崩溃。且无 CancellationToken 支持，控件在异步操作完成前被销毁时可能访问已释放的资源。

- **复现条件**：异步操作中发生异常；或控件在操作完成前被销毁
- **影响评估**：**中等** — 触发异常时导致应用崩溃
- **修复建议**：将 `async void` 改为 `async Task`，添加 try-catch 和 CancellationToken 支持。

---

### 5.12 AbstractSkeleton — CancellationTokenSource 未 Dispose

- **文件**：`src/AtomUI.Desktop.Controls/Skeleton/AbstractSkeleton.cs`
- **方法**：`StartActiveAnimation()`，第 155-156 行；`StopActiveAnimation()`，第 182 行
- **问题描述**：

`StartActiveAnimation()` 中 `Cancel()` 后直接 `= new`，旧 CTS 没有 Dispose：

```csharp
protected void StartActiveAnimation()
{
    _cancellationTokenSource?.Cancel();
    // ❌ 缺少 _cancellationTokenSource?.Dispose();
    _cancellationTokenSource = new CancellationTokenSource();
    // ...
}
```

- **复现条件**：Skeleton 动画多次启停
- **影响评估**：**中等** — CTS 持有 WaitHandle 等非托管资源
- **修复建议**：

```csharp
protected void StartActiveAnimation()
{
    _cancellationTokenSource?.Cancel();
    _cancellationTokenSource?.Dispose();  // ✅ 添加
    _cancellationTokenSource = new CancellationTokenSource();
    // ...
}

protected void StopActiveAnimation()
{
    _cancellationTokenSource?.Cancel();
    _cancellationTokenSource?.Dispose();  // ✅ 添加
    _cancellationTokenSource = null;
}
```

---

### 5.13 Mentions / AutoComplete — _delayTimer 重建时未停旧实例

- **文件**：
  - `src/AtomUI.Desktop.Controls/Mentions/Mentions.cs`，第 586 行
  - `src/AtomUI.Desktop.Controls/AutoComplete/AbstractAutoComplete.cs`，第 860 行
- **问题描述**：

创建新 `_delayTimer` 时未先停止旧的。如果 `MinimumPopulateDelay` 频繁变化，旧 Timer 继续运行。

- **复现条件**：频繁修改 MinimumPopulateDelay 属性
- **影响评估**：**中等** — 旧 Timer 继续运行导致多余回调
- **修复建议**：

```csharp
private void HandleMinimumPopulateDelayChanged(AvaloniaPropertyChangedEventArgs e)
{
    if (_delayTimer != null)
    {
        _delayTimer.Stop();
        _delayTimer.Tick -= PopulateDropDown;
        _delayTimer = null;
    }
    _delayTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(MinimumPopulateDelay) };
    _delayTimer.Tick += PopulateDropDown;
}
```

---

### 5.14 Form — Items.CollectionChanged += lambda

- **文件**：`src/AtomUI.Desktop.Controls/Form/Form.cs`，第 456 行
- **问题描述**：

```csharp
Items.CollectionChanged += (sender, args) => { InvalidateMeasure(); };
```

虽然 `Items` 是自身属性，但如果被外部 CollectionView 替换，旧订阅无法取消。

- **复现条件**：Form 的 Items 集合被替换
- **影响评估**：**中等**
- **修复建议**：改为命名方法 `HandleItemsCollectionChanged`。

---

### 5.15 FloatButtonGroupHost — lambda 订阅 OpenRequest/CloseRequest

- **文件**：`src/AtomUI.Desktop.Controls/FloatButton/FloatButtonGroupHost.cs`，第 199-200 行
- **问题描述**：

```csharp
floatButtonGroup.OpenRequest  += (sender, args) => SetValue(IsOpenProperty, true, BindingPriority.Style);
floatButtonGroup.CloseRequest += (sender, args) => SetValue(IsOpenProperty, false, BindingPriority.Style);
```

Lambda 捕获 `this`（FloatButtonGroupHost），无法取消订阅。

- **复现条件**：FloatButtonGroupHost 被创建并使用
- **影响评估**：**中等**
- **修复建议**：改为命名方法，在 detach 时取消订阅。

---

### 5.16 Space.OnApplyTemplate — Children.PropertyChanged 可能重复订阅

- **文件**：`src/AtomUI.Desktop.Controls/Space/Space.cs`，第 129 行
- **问题描述**：

`OnApplyTemplate` 中订阅 `Children.PropertyChanged`，未先检查是否已订阅。模板重新应用时会叠加。

- **复现条件**：Space 控件模板重新应用
- **影响评估**：**中等**
- **修复建议**：

```csharp
protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
{
    base.OnApplyTemplate(e);
    Children.PropertyChanged -= HandleChildrenPropertyChanged;  // 先移除
    Children.PropertyChanged += HandleChildrenPropertyChanged;
}
```

---

### 5.17 ToolTip.StartShowTimer — Timer 可能重叠

- **文件**：
  - `src/AtomUI.Desktop.Controls/Tooltip/ToolTip.cs`，第 600-608 行
  - `src/AtomUI.Desktop.Controls/Tooltip/ToolTipService.cs`，第 239-240 行
- **问题描述**：

每次 `StartShowTimer` 创建新 Timer + lambda，但不确保总是在 `StartShowTimer` 前调用 `StopTimer`。快速鼠标移入移出多个控件时可能产生多个并行 Timer。

- **复现条件**：快速在多个带 ToolTip 的控件间移动鼠标
- **影响评估**：**中等**
- **修复建议**：在 `StartShowTimer` 开头始终调用 `StopTimer()`，并使用命名方法替代 lambda。

---

### 5.18 Watermark.Render — 紧密循环渲染性能问题

- **文件**：`src/AtomUI.Controls/Watermark/Watermark.cs`，第 116-174 行
- **问题描述**：

双重 while 循环在每帧 Render 中创建大量 Matrix 计算和 `context.PushTransform` 调用。大面积 + 密集水印场景下渲染卡顿。

- **复现条件**：大面积区域使用密集水印
- **影响评估**：**中等** — 渲染性能瓶颈
- **修复建议**：使用 `DrawingGroup` + `TileBrush` 缓存水印 tile，或 `RenderTargetBitmap` 仅在 glyph 变化时重建。

---

### 5.19 WindowNotificationManager — 50ms 高频轮询

- **文件**：`src/AtomUI.Desktop.Controls/Notifications/WindowNotificationManager.cs`，第 82-85 行
- **问题描述**：

`_cardExpiredTimer` 和 `_cleanupTimer` 以 50ms 间隔轮询。`_cleanupQueue.Contains(card)` 在 Queue 上是 O(n) 操作。

- **复现条件**：通知管理器持续运行
- **影响评估**：**中等** — 不必要的 CPU 开销
- **修复建议**：
  1. 使用 HashSet 辅助查重
  2. 考虑基于事件驱动而非轮询
  3. 适当降低轮询频率（如 200ms）

---

## 6. 🔵 低危问题（Low）

### 6.1 预设颜色映射使用静态只读字典

- **文件**：
  - `src/AtomUI.Controls/Tag/AbstractTag.cs`，第 159-160 行
  - `src/AtomUI.Core/Theme/Palette/PresetPalettes.cs`，第 14-15 行
  - `src/AtomUI.Core/Theme/Palette/PaletteGenerator.cs`，第 45 行
- **问题描述**：

```csharp
// AbstractTag.cs
static readonly Dictionary<PresetColorType, TagCalcColor> PresetColorMap = ...;
static readonly Dictionary<TagStatus, TagStatusCalcColor> StatusColorMap = ...;

// PresetPalettes.cs
static readonly Dictionary<PresetPrimaryColor, PaletteInfo> sm_presetPalettes = ...;
static readonly Dictionary<PresetPrimaryColor, PaletteInfo> sm_presetDarkPalettes = ...;
```

这些静态字典在类加载时初始化，内容固定不变。虽然它们不会增长，但会在应用整个生命周期中占用内存。对于预设颜色这类有限集合，这是合理的设计选择。

- **影响评估**：**低** — 内存占用固定且较小，属于合理的设计权衡
- **修复建议**：无需修复。如果需要进一步优化，可以考虑使用 `FrozenDictionary`（.NET 8+）替代 `Dictionary` 以获得更好的查找性能。

```csharp
// .NET 8+ 优化建议
static readonly FrozenDictionary<PresetColorType, TagCalcColor> PresetColorMap = 
    new Dictionary<PresetColorType, TagCalcColor> { ... }.ToFrozenDictionary();
```

---

### 6.2 AbstractDesignToken 使用反射进行 Token 值转换

- **文件**：`src/AtomUI.Core/Theme/TokenSystem/AbstractDesignToken.cs`，第 14 行
- **问题描述**：

```csharp
static readonly Dictionary<Type, ITokenValueConverter> _valueConverters = ...;
// 通过反射在静态构造函数中填充
```

Token 值转换器通过反射发现和注册。虽然这只在类首次加载时执行一次，但反射操作本身较慢。如果有大量 Token 类型，初始化时间可能较长。

- **影响评估**：**低** — 一次性开销，不影响运行时性能
- **修复建议**：可以考虑使用 Source Generator 在编译时生成转换器注册代码，避免运行时反射。

---

### 6.3 TimerStatistic.GenerateRemainingTimeText 中的 Dead Code

- **文件**：`src/AtomUI.Desktop.Controls/Statistic/TimerStatistic.cs`
- **方法**：`GenerateRemainingTimeText()`，第 119-136 行
- **问题描述**：

```csharp
if (Format != null)
{
    formattedText = RemainingTime.ToString(Format);   // 第 123 行 — 处理所有非 null Format
}
else if (Format == @"hh\:mm\:ss")   // 第 125 行 — 永远不会到达（Format 已经是 null）
{
    // dead code
}
else if (Format == @"mm\:ss")       // 第 129 行 — 同上
{
    // dead code
}
```

`else if` 分支中的条件检查 `Format` 是否等于特定字符串，但此时 `Format` 必然为 `null`（因为第一个 `if` 已经处理了所有非 null 情况），所以这些分支永远不会执行。

- **影响评估**：**低** — 不影响功能，但代码逻辑有误
- **修复建议**：

```csharp
if (Format != null)
{
    formattedText = RemainingTime.ToString(Format);
}
else
{
    // 默认格式化逻辑
    if (RemainingTime.Hours > 0)
        formattedText = RemainingTime.ToString(@"hh\:mm\:ss");
    else
        formattedText = RemainingTime.ToString(@"mm\:ss");
}
```

---

### 6.4 Gallery 示例应用中的小问题

- **文件**：`controlgallery/` 下的各 ShowCase 文件
- **问题描述**：

Gallery 示例应用中的一些 ShowCase 控件可能存在与主库相同的事件订阅模式问题。虽然作为示例应用影响较小，但可能误导开发者采用不正确的模式。

- **影响评估**：**低** — 仅影响示例应用，不影响库本身
- **修复建议**：确保示例代码遵循最佳实践，为开发者提供正确的参考。

---

### 6.5 ImmutableSolidColorBrush 在主题系统中的频繁创建

- **文件**：`src/AtomUI.Core/Theme/` 相关文件
- **设计文档参考**：`docs/Core/ThemeSystem/`
- **问题描述**：

根据设计文档，主题系统在构建 `ResourceDictionary` 时会将 Color 类型的 Token 自动包装为 `ImmutableSolidColorBrush`。如果主题切换频繁，每次都会创建大量新的 Brush 对象。

- **影响评估**：**低** — `ImmutableSolidColorBrush` 是轻量对象，且主题切换不频繁
- **修复建议**：如果性能分析显示这是热点，可以考虑缓存已创建的 Brush 对象。

---

## 7. 架构层面分析与建议

### 7.1 设计文档与实现的交叉验证

基于 `docs/` 目录下的架构设计文档，对以下关键设计决策进行了实现验证：

#### 7.1.1 主题系统（Token System）

**设计约束**（来自 `docs/Core/ThemeSystem/`）：
- 四层 Token 体系：Seed → Map → Alias → Component
- ThemePool 缓存机制：主题懒加载，首次激活时计算
- ThemeConfigProvider 独立计算

**实现风险**：
- ✅ ThemePool 缓存机制实现正确，主题切换时复用已计算的 Token
- ⚠️ ThemeConfigProvider 每次都执行全量计算（见 5.5），与文档警告一致
- ⚠️ Token 值转换器使用运行时反射（见 6.2），可通过 Source Generator 优化
- ⚠️ 静态缓存无大小限制（见 4.6），长时间运行可能累积

#### 7.1.2 动画系统（Motion Scene）

**设计约束**（来自 `docs/Core/MotionScene/` 和 `docs/Controls/MotionAndWaveSpiritDesign.md`）：
- MotionActor 负责动画生命周期管理
- 动画完成后应自动清理资源
- WaveSpirit 效果使用独立的动画管线

**实现风险**：
- ⚠️ AbstractNotifiableTransition 的 Subject 未实现 IDisposable（见 4.1）
- ⚠️ 动画中断场景下的资源清理路径不完整
- ⚠️ MarqueeLabel 等持续动画控件的 detach 清理需要加强（见 5.4）

#### 7.1.3 控件生命周期（Control Infrastructure）

**设计约束**（来自 `docs/Core/ControlInfrastructure.md`）：
- 控件应在 `OnAttachedToVisualTree` 中初始化资源
- 控件应在 `OnDetachedFromVisualTree` 中清理资源
- 使用 CompositeDisposable 管理订阅生命周期

**实现风险**：
- 🔴 TransformTrackingHelper 的 detach 事件订阅 Bug（见 3.1）
- 🔴 SwitchKnob 调用错误的基类方法（见 3.2）
- ⚠️ 多个控件未在 detach 时清理 CompositeDisposable（见 5.3）
- ⚠️ 多个控件在构造函数中订阅事件但未在 detach 时取消（见 4.4、4.5、5.1）

#### 7.1.4 图标系统（Icon System）

**设计约束**（来自 `docs/Core/IconSystem.md`）：
- 图标通过 IconProvider 按需加载
- IconProviderCache 提供全局缓存

**实现风险**：
- ⚠️ IconProviderCache 使用静态 ConcurrentDictionary，无大小限制（见 4.6）

---

### 7.2 全局性架构建议

#### 7.2.1 建立统一的控件生命周期管理模式

建议在基类或通过代码分析器强制执行以下模式：

```csharp
public abstract class ManagedControl : Control
{
    private CompositeDisposable? _attachDisposables;

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _attachDisposables = new CompositeDisposable();
        OnSetupSubscriptions(_attachDisposables);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _attachDisposables?.Dispose();
        _attachDisposables = null;
        base.OnDetachedFromVisualTree(e);
    }

    /// <summary>
    /// 子类在此方法中添加所有需要在 detach 时自动清理的订阅
    /// </summary>
    protected virtual void OnSetupSubscriptions(CompositeDisposable disposables) { }
}
```

#### 7.2.2 引入 DispatcherTimer 包装器

创建一个安全的定时器包装器，自动管理生命周期：

```csharp
public sealed class ManagedTimer : IDisposable
{
    private DispatcherTimer? _timer;
    private EventHandler<EventArgs>? _handler;

    public ManagedTimer(TimeSpan interval, EventHandler<EventArgs> handler)
    {
        _handler = handler;
        _timer = new DispatcherTimer { Interval = interval };
        _timer.Tick += _handler;
    }

    public void Start() => _timer?.Start();
    public void Stop() => _timer?.Stop();

    public void Dispose()
    {
        if (_timer != null)
        {
            _timer.Stop();
            _timer.Tick -= _handler;
            _timer = null;
            _handler = null;
        }
    }
}
```

#### 7.2.3 添加静态分析规则

建议通过 Roslyn Analyzer 或 .editorconfig 规则检测以下模式：

1. **事件订阅不匹配**：检测 `+=` 没有对应 `-=` 的情况
2. **匿名 lambda 事件订阅**：警告使用匿名 lambda 订阅长生命周期对象的事件
3. **DispatcherTimer 未清理**：检测创建 DispatcherTimer 但未在 Dispose/Detach 中停止的情况
4. **async 方法缺少 CancellationToken**：检测公开的 async 方法未接受 CancellationToken 参数

#### 7.2.4 缓存策略统一

建议为所有静态缓存建立统一的管理策略：

```csharp
public static class CacheManager
{
    private static readonly List<WeakReference<IClearable>> _caches = new();

    public static void Register(IClearable cache) 
        => _caches.Add(new WeakReference<IClearable>(cache));

    /// <summary>
    /// 在内存压力时调用，清理所有注册的缓存
    /// </summary>
    public static void TrimAll()
    {
        foreach (var weakRef in _caches)
        {
            if (weakRef.TryGetTarget(out var cache))
                cache.Trim();
        }
    }
}
```

---

## 8. 总结与修复优先级

### 8.1 问题统计

| 严重程度 | 数量 | 类别 |
|----------|------|------|
| 🔴 Critical | 7 | 事件订阅 Bug、基类方法调用错误、定时器泄漏、CollectionChanged lambda 泄漏、OnApplyTemplate lambda 叠加 |
| 🟠 High | 8 | Transition 泄漏、定时器泄漏、事件未取消、缓存无限增长、async 缺少取消 |
| 🟡 Medium | 19 | CompositeDisposable 清理、动画资源、主题性能、虚拟化泄漏、CTS 未 Dispose、async void、Timer 重建、渲染性能、高频轮询 |
| 🔵 Low | 5 | 静态字典优化、反射开销、Dead Code、示例代码 |
| **合计** | **39** | |

### 8.2 修复优先级建议

#### P0 — 立即修复（影响所有用户）

| # | 问题 | 预计工作量 |
|---|------|-----------|
| 3.1 | TransformTrackingHelper `-=` → `+=` | 1 行代码 |
| 3.2 | SwitchKnob 基类方法调用修正 | 1 行代码 |
| 3.3 | TimerStatistic 定时器清理 | ~20 行代码 |
| 3.4 | ListCollectionView CollectionChanged lambda 泄漏 | ~15 行代码 |
| 3.5 | DataGridCollectionView 同上 | ~15 行代码 |
| 3.6 | InfoPickerInput OnApplyTemplate lambda 叠加 | ~30 行代码 |
| 3.7 | ColorPickerInput OnApplyTemplate 8 个 lambda 叠加 | ~40 行代码 |

#### P1 — 尽快修复（特定场景下必现）

| # | 问题 | 预计工作量 |
|---|------|-----------|
| 4.1 | AbstractNotifiableTransition IDisposable | ~15 行代码 |
| 4.2 | Carousel 定时器清理 | ~15 行代码 |
| 4.3 | Drawer OpenOn 事件清理 | ~10 行代码 |
| 4.4 | Tour 事件订阅清理 | ~15 行代码 |
| 4.5 | OptionButtonGroup lambda 改命名方法 | ~20 行代码 |
| 4.7 | NotificationCard 定时器清理 | ~10 行代码 |

#### P2 — 计划修复（改善整体质量）

| # | 问题 | 预计工作量 |
|---|------|-----------|
| 4.6 | 静态缓存大小限制 | ~50 行代码 |
| 4.8 | async 方法添加 CancellationToken | 中等（涉及 API 变更） |
| 5.1-5.19 | 中等问题批量修复 | 中等 |

#### P3 — 长期优化

| # | 问题 | 预计工作量 |
|---|------|-----------|
| 7.2.1 | 统一控件生命周期管理模式 | 大（架构级变更） |
| 7.2.2 | DispatcherTimer 包装器 | 小 |
| 7.2.3 | 静态分析规则 | 中等 |
| 7.2.4 | 缓存策略统一 | 中等 |

### 8.3 验证建议

修复完成后，建议通过以下方式验证：

1. **内存分析工具**：使用 dotMemory 或 Visual Studio Diagnostic Tools 进行内存快照对比，验证控件 attach/detach 循环后无残留引用
2. **性能分析工具**：使用 dotTrace 或 PerfView 分析热点路径，确认定时器泄漏已修复
3. **自动化测试**：为关键控件编写 attach/detach 循环测试，配合 `WeakReference` 验证 GC 可达性
4. **CI 集成**：将静态分析规则集成到 CI 流程中，防止回归

```csharp
// 示例：GC 可达性验证测试
[Fact]
public void Control_ShouldBeCollected_AfterDetach()
{
    WeakReference weakRef;
    
    // Arrange & Act
    {
        var control = new MyControl();
        weakRef = new WeakReference(control);
        var parent = new Panel();
        parent.Children.Add(control);
        parent.Children.Remove(control);
    }
    
    // Assert
    GC.Collect();
    GC.WaitForPendingFinalizers();
    GC.Collect();
    
    Assert.False(weakRef.IsAlive, "Control should be collected after detach");
}
```

---

> 本报告基于静态代码分析生成，部分问题可能需要结合运行时分析工具进一步确认。建议按优先级逐步修复，并在每次修复后运行完整的测试套件验证无回归。

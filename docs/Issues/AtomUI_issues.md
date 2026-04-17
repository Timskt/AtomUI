# AtomUI 性能与内存泄漏分析报告（补充版）

> **生成日期**：2026-04-17
> **分析范围**：`src/`、`controlgallery/` 下所有项目
> **目标框架**：.NET 8 / .NET 10
> **UI 框架**：Avalonia v11.3.x

## 概述

本报告是对 [`docs/Issues/performance_memory_leak.md`](./Issues/performance_memory_leak.md) 的**补充分析**。该主文档已系统记录了 **38+** 个核心问题，其中大部分（约 75%）已修复（标记为 ✅）。本报告关注：

1. **主文档未覆盖的新问题**（本次扫描新发现）
2. **主文档已标注但尚未修复的问题**（给出更详细的分析）
3. **架构层面的系统性风险**

### 📊 问题统计

| 严重度 | 新发现 | 主文档未修复 | 合计 |
|-------|--------|-------------|------|
| 🔴 高（可导致崩溃/持续泄漏） | 3 | 3 | 6 |
| 🟠 中（运行期资源累积） | 12 | 5 | 17 |
| 🟡 低（优化建议） | 10 | 3 | 13 |
| **总计** | **25** | **11** | **36** |

### 严重程度分级

- **🔴 高**：必定触发、会持续累积且影响功能或导致崩溃
- **🟠 中**：特定场景下触发，资源随时间累积
- **🟡 低**：不影响运行但有优化空间

---

## 目录

- [一、事件订阅问题（Lambda 捕获 / 未取消）](#一事件订阅问题lambda-捕获--未取消)
- [二、生命周期与分离问题](#二生命周期与分离问题)
- [三、异步与并发问题](#三异步与并发问题)
- [四、资源管理与 IDisposable](#四资源管理与-idisposable)
- [五、集合 / 缓存 / 静态字段](#五集合--缓存--静态字段)
- [六、性能热点](#六性能热点)
- [七、Gallery 项目相关问题](#七gallery-项目相关问题)
- [八、架构层面建议](#八架构层面建议)
- [九、修复优先级清单](#九修复优先级清单)
- [十、附录：已修复问题摘要](#十附录已修复问题摘要)

---

## 一、事件订阅问题（Lambda 捕获 / 未取消）

### ✅ 1.1 MessageBox / Dialog — `Closed += lambda` 捕获大量闭包（8 处）（已修复）

- **文件**：
  - `src/AtomUI.Desktop.Controls/MessageBox/MessageBox.cs` 第 318、343、368、392 行
  - `src/AtomUI.Desktop.Controls/Dialog/Dialog.StaticAPI.cs` 第 82、110、136、164 行
- **问题描述**：

所有静态 API（`Show*`, `Confirm*`, `Info*`, `Success*`, `Error*`, `Warning*`）创建 MessageBox / Dialog 后使用 lambda 订阅 `Closed` 事件：

```csharp
messageBox.Closed += (_, _) => dialogManager.Children.Remove(messageBox);
// 或
messageBox.Closed += (_, _) =>
{
    dialogManager.Children.Remove(messageBox);
    tcs.SetResult(...);
};
```

虽然 `Closed` 通常只触发一次，但：
1. Lambda 捕获了 `dialogManager`、`messageBox`、甚至外部的 `tcs`（`TaskCompletionSource`），形成闭包对象。
2. 如果 `Closed` 因异常未触发（例如 Dispatcher 异常或控件被外部强制从树上移除），闭包永远不释放。
3. 一次会话中多次弹窗，闭包对象堆积。

- **复现条件**：高频弹窗（如错误提示、确认框）的长会话应用。
- **影响评估**：🟠 中（中频场景下会累积，每个闭包 ~100-500B）。
- **修复建议**：抽取为命名静态方法 + 使用 `EventHandler` 签名，并在方法内用 `-=` 自解除：

```csharp
private static void OnMessageBoxClosedRemove(object? sender, EventArgs e)
{
    if (sender is MessageBox box && box.GetLogicalParent() is Panel parent)
    {
        parent.Children.Remove(box);
        box.Closed -= OnMessageBoxClosedRemove;  // 自解除防止引用残留
    }
}
// 订阅：
messageBox.Closed += OnMessageBoxClosedRemove;
```

对带 `tcs` 等上下文的场景，使用附属字典（`ConditionalWeakTable<MessageBox, TaskCompletionSource<...>>`）而不是 lambda 闭包。

---

### 1.2 🟠 WindowNotificationManager — `PointerPressed += lambda`

- **文件**：`src/AtomUI.Desktop.Controls/Notifications/WindowNotificationManager.cs` 第 186 行
- **问题描述**：

```csharp
notificationControl.PointerPressed += (_, _) => { onClick?.Invoke(); };
```

Lambda 捕获 `onClick` 回调参数。通知卡片生命周期较长（3-10 秒），高频通知下数十个闭包对象堆积。Lambda 无法在控件销毁时解除订阅。

- **影响评估**：🟠 中。
- **修复建议**：使用 `Tag` 属性传递 `onClick`，用命名方法处理：

```csharp
private static void OnNotificationPointerPressed(object? sender, PointerPressedEventArgs e)
{
    if (sender is Control c && c.Tag is Action onClick)
    {
        onClick();
    }
}
notificationControl.Tag = onClick;
notificationControl.PointerPressed += OnNotificationPointerPressed;
```

---

### 1.3 🟠 AbstractListGroupDescription / DataGridGroupDescription — 构造函数中 lambda 订阅

- **文件**：
  - `src/AtomUI.Controls.Shared/Data/ListCollectionViews/AbstractListGroupDescription.cs` 第 14 行
  - `src/AtomUI.Desktop.Controls.DataGrid/Data/DataGridGroupDescription.cs` 第 19 行
- **问题描述**：

```csharp
public AbstractListGroupDescription()
{
    GroupKeys.CollectionChanged += (sender, e) =>
        OnPropertyChanged(new PropertyChangedEventArgs(nameof(GroupKeys)));
}
```

虽然 `GroupKeys` 是自持有集合，生命周期与描述对象一致，但：
1. Lambda 捕获 `this`，使 GC 无法收集 Description 即使没有其他引用（`GroupKeys` 是 `AvaloniaList<>`，由其持有 description）。
2. 导致 GroupDescription 对象无法被 GC — 属于自循环引用 + 闭包持有。

- **影响评估**：🟠 中 — GroupDescription 若被大量实例化（典型 DataGrid 数据切换场景）会持续累积。
- **修复建议**：改为命名方法：

```csharp
public AbstractListGroupDescription()
{
    GroupKeys.CollectionChanged += OnGroupKeysCollectionChanged;
}

private void OnGroupKeysCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
{
    OnPropertyChanged(new PropertyChangedEventArgs(nameof(GroupKeys)));
}
```

并在 `Dispose`（若类实现 IDisposable）中 `-=` 取消。

---

### 1.4 🟠 FloatButtonGroupHost — `OpenRequest/CloseRequest += lambda`（主文档 5.15）

- **文件**：`src/AtomUI.Desktop.Controls/FloatButton/FloatButtonGroupHost.cs` 第 199-200 行
- **问题确认**：

```csharp
floatButtonGroup.OpenRequest  += (sender, args) => SetValue(IsOpenProperty, true, BindingPriority.Style);
floatButtonGroup.CloseRequest += (sender, args) => SetValue(IsOpenProperty, false, BindingPriority.Style);
```

Lambda 捕获 `this`（Host 实例）。`floatButtonGroup` 可能被多次重建（如模板重新应用），造成订阅重复 + 捕获链延长。

- **影响评估**：🟠 中。
- **修复建议**：命名方法 + 在 `OnDetachedFromVisualTree` 取消订阅。

```csharp
private void OnFloatButtonGroupOpenRequest(object? sender, RoutedEventArgs e)
    => SetValue(IsOpenProperty, true, BindingPriority.Style);
private void OnFloatButtonGroupCloseRequest(object? sender, RoutedEventArgs e)
    => SetValue(IsOpenProperty, false, BindingPriority.Style);

// 订阅前先取消（幂等）
floatButtonGroup.OpenRequest  -= OnFloatButtonGroupOpenRequest;
floatButtonGroup.OpenRequest  += OnFloatButtonGroupOpenRequest;
floatButtonGroup.CloseRequest -= OnFloatButtonGroupCloseRequest;
floatButtonGroup.CloseRequest += OnFloatButtonGroupCloseRequest;
```

---

### 1.5 🟠 Form.HandleMinimumPopulateDelayChanged — 其他同类 Timer 管理（已修复）

该项已在主文档 5.13 修复（Mentions / AutoComplete），此处仅作确认引用。

---

## 二、生命周期与分离问题

### 2.1 🔴 多个控件缺少 `OnDetachedFromVisualTree` 清理逻辑

以下控件类重写了 `OnAttachedToVisualTree` 或在构造中注册了资源，但**没有对应的 `OnDetachedFromVisualTree`**：

| 文件 | 风险 |
|------|------|
| `src/AtomUI.Controls/FloatButton/AbstractBackTopFloatButtonHost.cs` | 订阅 `ScrollViewer.ScrollChanged` 未解除 |
| `src/AtomUI.Controls/Primitives/Motions/MotionGhostControl.cs` | 动画资源可能残留 |
| `src/AtomUI.Desktop.Controls.ColorPicker/ColorSlider/GradientColorPickerTrack.cs` | Bitmap 可能未释放 |
| `src/AtomUI.Desktop.Controls/Primitives/InfoPickerInput/RangeInfoPickerInput.cs` | 内部 Popup 未关闭 |
| `src/AtomUI.Desktop.Controls/PopupConfirm/PopupConfirm.cs` | FlyoutStateHelper 未清理 |
| `src/AtomUI.Desktop.Controls/Notifications/NotificationCard.cs` | 动画 CTS 未清理 |
| `src/AtomUI.Desktop.Controls/TreeView/TreeViewItem.cs` | 订阅绑定未释放 |
| `src/AtomUI.Desktop.Controls/NavMenu/NavMenuItem.cs` | SubMenu Popup 未清理 |
| `src/AtomUI.Desktop.Controls/Card/Card.cs` | token scope 资源未释放 |
| `src/AtomUI.Desktop.Controls/Collapse/Collapse.cs`<br/>`CollapseItem.cs` | 动画控制器未释放 |

- **问题描述**：当这些控件从树上分离（例如 TabView 切换、ItemsControl 回收容器）时，它们持有的订阅、绑定、Timer、CTS 无法释放，导致：
  - 事件处理器堆积
  - 闭包中持有的父控件引用被延长
  - 大量虚拟化场景（TreeView、ListView）下泄漏倍增
- **影响评估**：🔴 高 — 尤其是 `TreeViewItem`、`NavMenuItem`、`CollapseItem`、`NotificationCard` 这些高频实例化的控件。
- **修复建议**：统一审计以下模板：

```csharp
private CompositeDisposable? _subscriptions;

protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
{
    base.OnAttachedToVisualTree(e);
    _subscriptions = new CompositeDisposable();
    // 所有订阅、绑定、Token scope 都加入 _subscriptions
}

protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
{
    base.OnDetachedFromVisualTree(e);
    _subscriptions?.Dispose();
    _subscriptions = null;
    // 此外：停止 Timer、Cancel CTS、关闭 Popup
}
```

---

### 2.2 🟠 Space — Children.PropertyChanged 重复订阅保护（主文档 5.16）

- **文件**：`src/AtomUI.Desktop.Controls/Space/Space.cs`（已有条件订阅逻辑，但需确认）
- **当前实现**：使用了 `add`/`remove` 事件访问器，条件订阅正确。但 `OnApplyTemplate` 中若直接 `+=` 会造成叠加。
- **修复建议**：审计所有 `OnApplyTemplate` 调用栈，确保订阅幂等。

```csharp
protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
{
    base.OnApplyTemplate(e);
    // 先取消再订阅（幂等）
    Children.PropertyChanged -= HandleChildrenPropertyChanged;
    Children.PropertyChanged += HandleChildrenPropertyChanged;
}
```

---

### 2.3 🟠 ToolTip / ToolTipService — Timer 重叠（主文档 5.17）

- **文件**：
  - `src/AtomUI.Desktop.Controls/Tooltip/ToolTip.cs` 第 600-608 行
  - `src/AtomUI.Desktop.Controls/Tooltip/ToolTipService.cs` 第 239-240 行
- **问题**：每次 `StartShowTimer` 直接 `new DispatcherTimer`，未先调用 `StopTimer`。快速鼠标移动时多个并行 Timer 可能触发意外显示。
- **修复建议**：

```csharp
private void StartShowTimer(int showDelay, Control control)
{
    StopTimer();  // ✅ 始终先停止旧 Timer
    _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(showDelay), Tag = (this, control) };
    _timer.Tick += OnTimerTick;  // 命名方法
    _timer.Start();
}
```

---

### 2.4 🟠 Popup.cs、Dialog.cs、Tour.cs、AbstractImagePreviewer — IDisposable 实现但未在生命周期钩子中调用

以下类实现了 `IDisposable`：

- `src/AtomUI.Desktop.Controls/Popup/Popup.cs`
- `src/AtomUI.Desktop.Controls/Dialog/Dialog.cs`
- `src/AtomUI.Desktop.Controls/Tour/Tour.cs`
- `src/AtomUI.Desktop.Controls/ImagePreviewer/AbstractImagePreviewer.cs`
- `src/AtomUI.Desktop.Controls/Tooltip/ToolTipService.cs`
- `src/AtomUI.Desktop.Controls.ColorPicker/ColorView/ColorPickerInput.cs`

但控件从视觉树分离时**大多未显式调用 `Dispose()`**（依赖使用方记得调用）。

- **影响评估**：🟠 中。
- **修复建议**：所有控件型 `IDisposable` 在 `OnDetachedFromVisualTree` 中自动调用 `Dispose`，或提供自动释放机制（通过 `WeakReference` 注册到 `Unloaded` 事件）。

---

## 三、异步与并发问题

### 3.1 🔴 TreeView.AsyncItemDataLoad / Cascader / AbstractSpinIndicator — CancellationTokenSource 未 Dispose（部分已修）

- **已修复**：主文档 5.10（TreeView/Cascader）、5.12（AbstractSkeleton）。
- **新发现未修**：
  - `src/AtomUI.Desktop.Controls/Select/Select.AsyncOptionsLoad.cs` 第 48 行 — `_optionsLoadCTS = new CancellationTokenSource();` 前未 Dispose 旧的
  - `src/AtomUI.Desktop.Controls/AutoComplete/AbstractAutoComplete.cs` 第 1434 行 — `_populationCancellationTokenSource`
  - `src/AtomUI.Desktop.Controls/Mentions/Mentions.cs` 第 826 行 — `_populationCancellationTokenSource`
  - `src/AtomUI.Desktop.Controls/Form/Form.cs` 第 522、605 行 — `_validationTokenSource`
  - `src/AtomUI.Desktop.Controls/Form/FormItem.cs` 第 725 行
  - `src/AtomUI.Desktop.Controls/Dialog/Dialog.cs` 第 436 行 — `_frameCancellationTokenSource`
  - `src/AtomUI.Desktop.Controls/Carousel/CarouselPageIndicator.cs` 第 228 行
- **修复模板**（统一应用到所有未修点）：

```csharp
private void StartOperation()
{
    _cts?.Cancel();
    _cts?.Dispose();   // ✅ 必须
    _cts = new CancellationTokenSource();
    // ...
}

protected override void OnDetachedFromVisualTree(...)
{
    base.OnDetachedFromVisualTree(...);
    _cts?.Cancel();
    _cts?.Dispose();
    _cts = null;
}
```

---

### 3.2 🟠 Gallery — 8 处 async void 未做异常保护

- **文件**：`controlgallery/AtomUIGallery/ShowCases/Views/Feedback/ModalShowCase.axaml.cs` 第 257、277、298、345、359、373、387、401 行
- **问题**：全部是 `private async void Handle*Click(...)`。虽然是 Gallery 演示代码，但若演示崩溃会导致整个 Gallery 无法用于验收测试。
- **影响评估**：🟠 中（仅影响演示环境）。
- **修复建议**：统一改为 `async Task` 或包装 try-catch：

```csharp
private async void HandleOpenOverlayDialogButtonClick(object? sender, RoutedEventArgs e)
{
    try
    {
        // 原逻辑
    }
    catch (Exception ex)
    {
        System.Diagnostics.Debug.WriteLine($"Modal demo failed: {ex}");
    }
}
```

---

### 3.3 🟠 VirtualizingCarouselPanel — `_transition` 字段命名误导

- **文件**：`src/AtomUI.Desktop.Controls/Carousel/VirtualizingCarouselPanel.cs` 第 158 行
- **问题**：`_transition = new CancellationTokenSource();` 字段名为 `_transition` 但类型是 CTS，易误解为 `Transition` 对象。虽然非泄漏问题，但未 `Dispose` 旧 CTS。
- **修复建议**：重命名为 `_transitionCts` 并走标准 Cancel+Dispose 流程。

---

### 3.4 🟠 FileUploadScheduler — 局部 CTS 未 Dispose

- **文件**：`src/AtomUI.Controls.Shared/Net/FileUploadScheduler.cs` 第 52 行
- **问题**：

```csharp
var cancellationTokenSource = new CancellationTokenSource();
// 未见 using 或 Dispose
```

- **修复建议**：改为 `using var cts = new CancellationTokenSource();`。

---

### 3.5 🟠 Badge 动画 CTS — AbstractDotBadgeAdorner / AbstractCountBadgeAdorner 多处 new CTS 未显式 Dispose 旧的

- **文件**：
  - `src/AtomUI.Controls/Badge/AbstractDotBadgeAdorner.cs` 第 122、133、171 行
  - `src/AtomUI.Controls/Badge/AbstractCountBadgeAdorner.cs` 第 254、277 行
- **问题**：每处都直接 `_motionCancellationTokenSource = new CancellationTokenSource()`，但上一次的可能没 Dispose。
- **修复模板**：与 3.1 同。

---

## 四、资源管理与 IDisposable

### 4.1 🟠 AbstractQRCode / PreviewImageSource — Bitmap 未 Dispose 旧实例

- **文件**：
  - `src/AtomUI.Controls/QRCode/AbstractQRCode.cs` 第 231 行 — `Bitmap = new Bitmap(data.AsStream());`
  - `src/AtomUI.Desktop.Controls/ImagePreviewer/PreviewImageSource.cs` 第 38 行 — `Bitmap = new Bitmap(stream);`
- **问题**：每次重新生成 / 切换图源时，旧 `Bitmap` 对象未 Dispose。Avalonia `Bitmap` 持有非托管像素缓冲，依赖终结器回收会延迟（可能分配在 LOH）。
- **影响评估**：🟠 中（图像量大时累积明显）。
- **修复建议**：

```csharp
public Bitmap? Bitmap
{
    get => _bitmap;
    set
    {
        if (_bitmap != value)
        {
            _bitmap?.Dispose();  // ✅ 先释放旧
            _bitmap = value;
            OnPropertyChanged();
        }
    }
}
```

---

### 4.2 🟠 ColorPickerHelpers.CreateBitmapFromPixelData — 调用者需主动 Dispose

- **文件**：`src/AtomUI.Desktop.Controls.ColorPicker/Utils/ColorPickerHelpers.cs` 第 625 行
- **问题**：返回 `new Bitmap(...)`，调用者（`ColorSpectrum` / `ColorSlider`）在频繁更新时需要主动 Dispose 前一个 Bitmap。目前 ColorSlider 已修复（5.11），但需全局审计所有调用。
- **修复建议**：在返回类型包装成一个 `PooledBitmap` 或要求 `using` 调用语法。

---

### 4.3 🟠 RenderTargetBitmap 泄漏风险 — ControlExtensions.cs

- **文件**：`src/AtomUI.Controls.Shared/Utils/ControlExtensions.cs` 第 23 行
- **问题**：`var bitmap = new RenderTargetBitmap(...)` 若用于截图工具方法，使用方必须负责 Dispose。RenderTargetBitmap 在 GPU 上驻留，开销更高。
- **修复建议**：在 XML doc 明确标注 "Caller must Dispose"，或改为 `using` 块模式。

---

### 4.4 🟠 AbstractMarqueeLabel — `_cancellationTokenSource = new CancellationTokenSource()` 前未 Dispose 旧的

- **文件**：`src/AtomUI.Controls/MarqueeLabel/AbstractMarqueeLabel.cs` 第 144 行
- **问题**：主文档 5.4 已修 MarqueeLabel 动画清理，但该 `new CTS` 赋值前需确认已 Dispose 旧 CTS。审计代码 `_cancellationTokenSource?.Cancel();` 存在，但缺 `Dispose()`。
- **修复建议**：统一 Cancel + Dispose 模式。

---

### 4.5 🟠 WaveSpiritDecorator — `_cancellationTokenSource = new CTS()` 未 Dispose 旧实例

- **文件**：`src/AtomUI.Controls/Primitives/WaveSpiritDecorator.cs` 第 292 行
- **问题**：`WaveSpirit` 点击涟漪高频触发（每次点击都创建 CTS）。如果短时间快速点击，之前的 CTS 未 Dispose。
- **影响评估**：🟠 中（按钮密集场景下持续泄漏 WaitHandle）。
- **修复建议**：

```csharp
_cancellationTokenSource?.Cancel();
_cancellationTokenSource?.Dispose();  // ✅
_cancellationTokenSource = new CancellationTokenSource();
```

---

### 4.6 🟠 SwitchKnob / AbstractSpinIndicator / AbstractBackTopFloatButton — 相同 CTS 模式

- **文件**：
  - `src/AtomUI.Controls/Switch/SwitchKnob.cs` 第 192 行
  - `src/AtomUI.Controls/Spin/AbstractSpinIndicator.cs` 第 127 行
  - `src/AtomUI.Controls/FloatButton/AbstractBackTopFloatButton.cs` 第 202 行
- **问题**：均是 `_cancellationTokenSource = new CancellationTokenSource()` 前无 Dispose。
- **修复建议**：同 4.5。

---

## 五、集合 / 缓存 / 静态字段

### 5.1 🟠 ButtonTheme.DashedStyle — `public static IList<double>` 可变

- **文件**：`src/AtomUI.Desktop.Controls/Buttons/Themes/ButtonTheme.cs` 第 9 行
- **问题**：

```csharp
public static IList<double> DashedStyle = [4, 2];
```

公开的可变集合字段 — 任何调用方都可 `.Add()` / `.Clear()`，会影响所有 Button 实例的渲染（共享引用）。
- **影响评估**：🟡 低（需要第三方主动改动），但违反不可变约定。
- **修复建议**：

```csharp
public static readonly IReadOnlyList<double> DashedStyle = new[] { 4d, 2d };
```

---

### 5.2 🟡 PresetPalettes / ThemeVariantCalculator — 内部静态字典未限制（已部分处理）

主文档 4.6 已修复 "静态 ConcurrentDictionary 缓存无大小限制"。建议再次审计所有内部 `static Dictionary`，确保：
- 缓存 key 不是控件引用（避免控件无法 GC）
- 容量上限或 LRU 清理策略

---

### 5.3 🟡 Gallery — MediaCatalog / ShowCaseRegistry 静态集合

- **文件**：`controlgallery/AtomUIGallery/ShowCases/...`
- **问题**：扫描发现 showcase 注册使用静态集合，长期运行会持有所有 showcase 视图实例。Gallery 应用作为 demo 不会长期运行，影响可忽略。
- **影响评估**：🟡 低。

---

## 六、性能热点

### 6.1 🟠 Watermark.Render — 紧循环渲染（主文档 5.18）

- **文件**：`src/AtomUI.Desktop.Controls/Watermark/WatermarkGlyph.cs`
- **问题**：Render 方法双重 for 循环调用 `DrawText`，未缓存 `FormattedText` 对象。
- **建议**：预构建 `GlyphRun` 或 `FormattedText`，仅在属性变化时重建。

---

### 6.2 🟠 WindowNotificationManager — 50ms 高频 Timer（主文档 5.19）

- **文件**：`src/AtomUI.Desktop.Controls/Notifications/WindowNotificationManager.cs` 第 84、86 行
- **问题**：`_cardExpiredTimer`、`_cleanupTimer` 以 50ms 固定间隔运行 — 即使没有通知也在运行。
- **影响评估**：🟠 中（持续 CPU 占用，节能场景影响明显）。
- **修复建议**：改为按需启动：

```csharp
// 有通知时启动，0 通知时停止
if (_notifications.Count == 0)
{
    _cardExpiredTimer.Stop();
    _cleanupTimer.Stop();
}
```

或使用 `Dispatcher.UIThread.TimerQueue` + 基于每条通知 `DispatcherTimer`。

---

### 6.3 🟡 Icon Geometry — StreamGeometry.Parse 重复调用

- **文件**：`src/AtomUI.Icons.AntDesign/GeneratedIcons/*.g.cs`（800+ 图标）
- **问题**：每个生成图标类的 `Build` 方法每次调用都 `StreamGeometry.Parse("...")`。频繁显示图标时重复解析 SVG 路径。
- **影响评估**：🟡 低（首次解析后 Avalonia 内部可能有缓存，但无明确文档保证）。
- **修复建议**：在图标类中把 `StreamGeometry` 缓存为 `static readonly` 字段（惰性初始化）：

```csharp
private static Geometry? _data1;
private static Geometry Data1 => _data1 ??= StreamGeometry.Parse("M1014.4 233.2...");

public override IconInfo Build(...)
{
    return new IconInfo { Paths = { new() { Data = Data1, ... } } };
}
```

但生成器自动生成，需改 `AtomUI.Icons.AntDesign.Generator` 模板。

---

### 6.4 🟡 InterpolateUtils.InterpolateStops — 每帧分配

- **文件**：`src/AtomUI.Core/Animations/InterpolateUtils.cs` 第 289、344 行
- **问题**：动画每一帧调用，创建 `IReadOnlyList<ImmutableGradientStop>`（LINQ `.ToList()`），可能产生 LOH 分配。
- **修复建议**：使用 `ArrayPool<ImmutableGradientStop>` 或静态预分配缓冲。

---

### 6.5 🟡 控件模板中 Brush / Pen 每次创建

审计 `src/AtomUI.Desktop.Controls/**/Themes/*.cs` 代码后中生成的 Pen/Brush 都是 `new Pen(...)` 而非 `ImmutablePen.Pool`。在 Avalonia 11 中 Pen/SolidColorBrush 的创建成本相对低，但视觉量大时仍有优化空间。

---

## 七、Gallery 项目相关问题

### 7.1 🟠 Gallery ShowCase 视图订阅未解除

部分 showcase 通过 `ViewModel.PropertyChanged += lambda` 订阅，但视图从 Navigation 栈弹出时不解除订阅，ViewModel 持有闭包导致视图无法 GC。

- **建议**：使用 ReactiveUI 的 `WhenActivated`：

```csharp
this.WhenActivated(disposables =>
{
    this.WhenAnyValue(x => x.ViewModel!.SomeProperty)
        .Subscribe(...)
        .DisposeWith(disposables);
});
```

### 7.2 🟠 ModalShowCase 8 处 async void（见 3.2）

### 7.3 🟡 BaseGalleryApplication 静态主题资源

`BaseGalleryApplication.axaml.cs` 注册主题时使用静态字段。Gallery 自身不切换应用实例，影响可忽略。

---

## 八、架构层面建议

### 8.1 建立统一的控件生命周期基类

在 `AtomUI.Controls` 中提供一个抽象基类（或 Mixin 接口），统一管理 `CompositeDisposable`、CTS、Timer：

```csharp
public abstract class AtomTemplatedControl : TemplatedControl
{
    protected CompositeDisposable DetachDisposables { get; } = new();
    protected CancellationTokenSource? DetachCts { get; private set; }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        DetachCts = new CancellationTokenSource();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        DetachCts?.Cancel();
        DetachCts?.Dispose();
        DetachCts = null;
        DetachDisposables.Clear();  // Clear 而非 Dispose，以支持 re-attach
    }
}
```

### 8.2 添加 Roslyn 分析器

基于 `AtomUI.Generator` 添加分析器，在编译时检查以下反模式：

- `async void` 非事件处理器
- `+= (...) => ...` Lambda 订阅 CLR 事件
- `new CancellationTokenSource()` 赋值到字段前未 Dispose
- `new DispatcherTimer()` 未赋值到字段

### 8.3 统一 Bitmap / Disposable 资源管理

所有 `Bitmap`、`RenderTargetBitmap`、`FormattedText` 等资源通过工厂获取，工厂内部使用 `ObjectPool` + 引用计数管理，避免手工 Dispose。

### 8.4 单元测试与泄漏检测

建议添加 `AtomUI.Base.Tests/MemoryLeakTests.cs`：

```csharp
[Fact]
public async Task Button_DoesNotLeak_AfterDetach()
{
    WeakReference wr = CreateButton();
    await Task.Delay(100);
    GC.Collect();
    GC.WaitForPendingFinalizers();
    GC.Collect();
    Assert.False(wr.IsAlive);
}
```

使用 `dotMemory.Unit` 或类似工具自动化。

---

## 九、修复优先级清单

| 序号 | 问题 | 严重度 | 工作量 | 优先级 |
|-----|------|--------|--------|--------|
| P1 | 2.1 多控件缺少 OnDetachedFromVisualTree 清理 | 🔴 高 | 大（10+ 文件） | 🚨 最高 |
| P2 | 3.1 Select / AutoComplete / Mentions / Form / Dialog — CTS 未 Dispose | 🔴 高 | 中 | 🚨 最高 |
| ~~P3~~ | ~~1.1 MessageBox / Dialog Closed += lambda（8 处）~~ | ~~已修复~~ | — | — |
| P4 | 4.5、4.6 WaveSpirit / SwitchKnob / Spin / BackTop CTS 模式 | 🟠 中 | 小 | 高 |
| P5 | 3.5 Badge Adorner CTS 未 Dispose（5 处） | 🟠 中 | 小 | 高 |
| P6 | 2.3 ToolTip Timer 重叠（主文档 5.17） | 🟠 中 | 小 | 高 |
| P7 | 6.2 WindowNotificationManager 50ms 常驻 Timer | 🟠 中 | 中 | 中 |
| P8 | 1.2 WindowNotificationManager PointerPressed lambda | 🟠 中 | 小 | 中 |
| P9 | 1.3 ListGroupDescription / DataGridGroupDescription lambda | 🟠 中 | 小 | 中 |
| P10 | 1.4 FloatButtonGroupHost lambda（主文档 5.15） | 🟠 中 | 小 | 中 |
| P11 | 4.1 AbstractQRCode / PreviewImageSource Bitmap 未 Dispose | 🟠 中 | 小 | 中 |
| P12 | 2.2 Space 子控件订阅幂等性（主文档 5.16） | 🟠 中 | 小 | 中 |
| P13 | 3.2 Gallery 8 处 async void | 🟠 中 | 小 | 中 |
| P14 | 3.3 VirtualizingCarouselPanel `_transition` CTS | 🟠 中 | 小 | 中 |
| P15 | 3.4 FileUploadScheduler 局部 CTS Dispose | 🟠 中 | 最小 | 中 |
| P16 | 6.1 Watermark 渲染优化（主文档 5.18） | 🟠 中 | 中 | 中 |
| P17 | 5.1 ButtonTheme.DashedStyle 改 IReadOnlyList | 🟡 低 | 最小 | 低 |
| P18 | 6.3 Icon Geometry 缓存 | 🟡 低 | 中（改生成器） | 低 |
| P19 | 6.4 InterpolateUtils LOH 分配 | 🟡 低 | 中 | 低 |
| P20 | 8.1 提供 AtomTemplatedControl 基类 | 架构 | 中 | 低 |
| P21 | 8.2 Roslyn 分析器 | 架构 | 大 | 低 |
| P22 | 8.4 MemoryLeakTests 自动化 | 架构 | 中 | 低 |

---

## 十、附录：已修复问题摘要

> 以下问题已在主文档 [`docs/Issues/performance_memory_leak.md`](./Issues/performance_memory_leak.md) 中标记为 ✅ 完成，不再重复分析：

| 章节 | 标题 |
|------|------|
| 3.1 | TransformTrackingHelper 事件订阅运算符错误 |
| 3.2 | SwitchKnob.OnDetachedFromVisualTree 调用错误基类方法 |
| 3.3 | TimerStatistic 定时器泄漏 |
| 3.6 | InfoPickerInput — OnApplyTemplate Lambda 重复 |
| 3.7 | ColorPickerInput — 8 个 ValueChanged Lambda 叠加 |
| 4.2 | Carousel 自动播放定时器泄漏 |
| 4.3 | Drawer.OpenOn SizeChanged 未 Detach 取消 |
| 4.4 | Tour 控件多处事件订阅未取消 |
| 4.5 | OptionButtonGroup 匿名 Lambda 无法取消 |
| 4.6 | 静态 ConcurrentDictionary 缓存无大小限制 |
| 4.7 | WindowNotificationManager 定时器 Tick 未取消 |
| 4.8 | 多个 async 方法缺 CancellationToken |
| 5.1 | Segmented 控件 SelectionChanged 未取消 |
| 5.2 | FlyoutStateHelper 定时器 Lambda 捕获 this |
| 5.3 | 多控件 CompositeDisposable 未 Dispose |
| 5.4 | MarqueeLabel 动画资源清理 |
| 5.6 | ToolTip 服务全局事件订阅 |
| 5.9 | Watermark — glyph.PropertyChanged += lambda |
| 5.10 | TreeView / CascaderView 异步加载 CTS |
| 5.11 | async void 方法无异常保护 |
| 5.12 | AbstractSkeleton CTS 未 Dispose |
| 5.13 | Mentions / AutoComplete _delayTimer 重建 |

> 主文档中以下问题**尚未修复**（本报告已引用）：
> - 3.4 ListCollectionView CollectionChanged 匿名 Lambda
> - 3.5 DataGridCollectionView 同类 Lambda 泄漏
> - 4.1 AbstractNotifiableTransition Subject<bool> 未实现 IDisposable
> - 5.5 ThemeConfigProvider 大量使用性能影响
> - 5.7 DataGrid 虚拟化场景潜在泄漏
> - 5.8 ColorPicker 复杂状态管理
> - 5.14 Form — Items.CollectionChanged += lambda（已确认非问题，构造函数中订阅）
> - 5.15 FloatButtonGroupHost lambda（本报告 1.4）
> - 5.16 Space.OnApplyTemplate Children.PropertyChanged（本报告 2.2）
> - 5.17 ToolTip.StartShowTimer 重叠（本报告 2.3）
> - 5.18 Watermark.Render 紧循环（本报告 6.1）
> - 5.19 WindowNotificationManager 50ms 轮询（本报告 6.2）

---

## 建议的后续步骤

1. **立即处理 P1-P6**：涉及 🔴 高严重度与高频资源泄漏问题。
2. **合并修复**：P3-P5、P9-P10 可以作为一次 PR 完成（统一 Lambda → 命名方法模式）。
3. **建立 CI 检查**：P21 Roslyn 分析器长期防止同类问题再次引入。
4. **按季度审计**：运行 `git log --stat src/**.cs` 检测新代码是否引入 `async void`、`+= lambda` 等反模式。

---

> **报告生成方式**：基于 grep 模式扫描 + 关键文件上下文阅读 + 对照 Avalonia v11 生命周期约定 + 参考已有主文档分析。
> **下一步建议**：让开发团队在 P1-P6 问题上安排人力，每修复一项对应更新主文档 `docs/Issues/performance_memory_leak.md` 的 ✅ 标记。


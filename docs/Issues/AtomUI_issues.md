# AtomUI 性能与内存泄漏分析报告（补充版）

> **生成日期**：2026-04-17
> **分析范围**：`src/`、`controlgallery/` 下所有项目
> **目标框架**：.NET 8 / .NET 10
> **UI 框架**：Avalonia v11.3.x

## 概述

本报告是对 [`docs/Issues/performance_memory_leak.md`](./performance_memory_leak.md) 的**补充分析**，记录尚未修复的问题。

### 严重程度分级

- **🔴 高**：必定触发、会持续累积且影响功能或导致崩溃
- **🟠 中**：特定场景下触发，资源随时间累积
- **🟡 低**：不影响运行但有优化空间

---

## 目录

- [二、生命周期与分离问题](#二生命周期与分离问题)
- [三、异步与并发问题](#三异步与并发问题)
- [四、资源管理与 IDisposable](#四资源管理与-idisposable)
- [五、集合 / 缓存 / 静态字段](#五集合--缓存--静态字段)
- [六、性能热点](#六性能热点)
- [七、Gallery 项目相关问题](#七gallery-项目相关问题)
- [八、架构层面建议](#八架构层面建议)
- [九、修复优先级清单](#九修复优先级清单)

---

## 二、生命周期与分离问题

### 2.1 🔴 多个控件缺少 `OnDetachedFromVisualTree` 清理逻辑（部分未修复）

以下控件已修复部分，但仍有以下文件未处理：

| 文件 | 风险 |
|------|------|
| `src/AtomUI.Desktop.Controls/PopupConfirm/PopupConfirm.cs` | FlyoutStateHelper 未清理 |
| `src/AtomUI.Desktop.Controls/Notifications/NotificationCard.cs` | 动画 CTS 未清理 |
| `src/AtomUI.Desktop.Controls/NavMenu/NavMenuItem.cs` | SubMenu Popup 未清理 |
| `src/AtomUI.Desktop.Controls/Collapse/Collapse.cs` | 动画控制器未释放 |
| `src/AtomUI.Desktop.Controls/Card/Card.cs` | token scope 资源未释放 |

- **影响评估**：🔴 高 — `NavMenuItem`、`NotificationCard` 高频实例化，泄漏倍增。
- **修复建议**：

```csharp
protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
{
    base.OnDetachedFromVisualTree(e);
    // 停止 Timer、Cancel CTS、关闭 Popup、解除事件订阅
}
```

---

### 2.2 🟠 Space — Children.PropertyChanged 重复订阅保护（主文档 5.16）

- **文件**：`src/AtomUI.Desktop.Controls/Space/Space.cs`
- **当前实现**：使用了 `add`/`remove` 事件访问器，条件订阅正确。但 `OnApplyTemplate` 中若直接 `+=` 会造成叠加。
- **修复建议**：审计所有 `OnApplyTemplate` 调用栈，确保订阅幂等。

```csharp
protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
{
    base.OnApplyTemplate(e);
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
    StopTimer();
    _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(showDelay), Tag = (this, control) };
    _timer.Tick += OnTimerTick;
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
- **修复建议**：所有控件型 `IDisposable` 在 `OnDetachedFromVisualTree` 中自动调用 `Dispose`。

---

## 三、异步与并发问题

### 3.1 🔴 Select / AutoComplete / Mentions / Form / Dialog — CancellationTokenSource 未 Dispose

- **新发现未修**：
  - `src/AtomUI.Desktop.Controls/Select/Select.AsyncOptionsLoad.cs` 第 48 行 — `_optionsLoadCTS = new CancellationTokenSource();` 前未 Dispose 旧的
  - `src/AtomUI.Desktop.Controls/AutoComplete/AbstractAutoComplete.cs` 第 1434 行 — `_populationCancellationTokenSource`
  - `src/AtomUI.Desktop.Controls/Mentions/Mentions.cs` 第 826 行 — `_populationCancellationTokenSource`
  - `src/AtomUI.Desktop.Controls/Form/Form.cs` 第 522、605 行 — `_validationTokenSource`
  - `src/AtomUI.Desktop.Controls/Form/FormItem.cs` 第 725 行
  - `src/AtomUI.Desktop.Controls/Dialog/Dialog.cs` 第 436 行 — `_frameCancellationTokenSource`
  - `src/AtomUI.Desktop.Controls/Carousel/CarouselPageIndicator.cs` 第 228 行
- **修复模板**：

```csharp
private void StartOperation()
{
    _cts?.Cancel();
    _cts?.Dispose();
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

### 3.2 🟠 Gallery — 8 处 async void 未做异常保护

- **文件**：`controlgallery/AtomUIGallery/ShowCases/Views/Feedback/ModalShowCase.axaml.cs` 第 257、277、298、345、359、373、387、401 行
- **问题**：全部是 `private async void Handle*Click(...)`。若演示崩溃会导致整个 Gallery 无法用于验收测试。
- **影响评估**：🟠 中（仅影响演示环境）。
- **修复建议**：统一包装 try-catch：

```csharp
private async void HandleOpenOverlayDialogButtonClick(object? sender, RoutedEventArgs e)
{
    try { /* 原逻辑 */ }
    catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Modal demo failed: {ex}"); }
}
```

---

### 3.3 🟠 VirtualizingCarouselPanel — `_transition` 字段命名误导

- **文件**：`src/AtomUI.Desktop.Controls/Carousel/VirtualizingCarouselPanel.cs` 第 158 行
- **问题**：`_transition = new CancellationTokenSource();` 字段名为 `_transition` 但类型是 CTS，未 Dispose 旧 CTS。
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
  - `src/AtomUI.Controls/QRCode/AbstractQRCode.cs` 第 231 行
  - `src/AtomUI.Desktop.Controls/ImagePreviewer/PreviewImageSource.cs` 第 38 行
- **问题**：每次重新生成 / 切换图源时，旧 `Bitmap` 对象未 Dispose。Avalonia `Bitmap` 持有非托管像素缓冲，依赖终结器回收会延迟。
- **影响评估**：🟠 中。
- **修复建议**：

```csharp
public Bitmap? Bitmap
{
    get => _bitmap;
    set
    {
        if (_bitmap != value)
        {
            _bitmap?.Dispose();
            _bitmap = value;
            OnPropertyChanged();
        }
    }
}
```

---

### 4.2 🟠 ColorPickerHelpers.CreateBitmapFromPixelData — 调用者需主动 Dispose

- **文件**：`src/AtomUI.Desktop.Controls.ColorPicker/Utils/ColorPickerHelpers.cs` 第 625 行
- **问题**：返回 `new Bitmap(...)`，调用者（`ColorSpectrum` / `ColorSlider`）在频繁更新时需要主动 Dispose 前一个 Bitmap。需全局审计所有调用。
- **修复建议**：在 XML doc 明确标注 "Caller must Dispose"，或包装成 `PooledBitmap`。

---

### 4.3 🟠 RenderTargetBitmap 泄漏风险 — ControlExtensions.cs

- **文件**：`src/AtomUI.Controls.Shared/Utils/ControlExtensions.cs` 第 23 行
- **问题**：`var bitmap = new RenderTargetBitmap(...)` 使用方必须负责 Dispose。RenderTargetBitmap 在 GPU 上驻留，开销更高。
- **修复建议**：在 XML doc 明确标注 "Caller must Dispose"，或改为 `using` 块模式。

---

### 4.4 🟠 AbstractMarqueeLabel — `_cancellationTokenSource` 前未 Dispose 旧的

- **文件**：`src/AtomUI.Controls/MarqueeLabel/AbstractMarqueeLabel.cs` 第 144 行
- **问题**：`_cancellationTokenSource?.Cancel();` 存在，但缺 `Dispose()`。
- **修复建议**：统一 Cancel + Dispose 模式。

---

### 4.5 🟠 WaveSpiritDecorator — `_cancellationTokenSource = new CTS()` 未 Dispose 旧实例

- **文件**：`src/AtomUI.Controls/Primitives/WaveSpiritDecorator.cs` 第 292 行
- **问题**：`WaveSpirit` 点击涟漪高频触发（每次点击都创建 CTS），之前的 CTS 未 Dispose。
- **影响评估**：🟠 中（按钮密集场景下持续泄漏 WaitHandle）。
- **修复建议**：

```csharp
_cancellationTokenSource?.Cancel();
_cancellationTokenSource?.Dispose();
_cancellationTokenSource = new CancellationTokenSource();
```

---

### 4.6 🟠 SwitchKnob / AbstractSpinIndicator — 相同 CTS 模式

- **文件**：
  - `src/AtomUI.Controls/Switch/SwitchKnob.cs` 第 192 行
  - `src/AtomUI.Controls/Spin/AbstractSpinIndicator.cs` 第 127 行
- **问题**：均是 `_cancellationTokenSource = new CancellationTokenSource()` 前无 Dispose。
- **修复建议**：同 4.5。

---

## 五、集合 / 缓存 / 静态字段

### 5.1 🟡 ButtonTheme.DashedStyle — `public static IList<double>` 可变

- **文件**：`src/AtomUI.Desktop.Controls/Buttons/Themes/ButtonTheme.cs` 第 9 行
- **问题**：

```csharp
public static IList<double> DashedStyle = [4, 2];
```

公开的可变集合字段，任何调用方都可 `.Add()` / `.Clear()`，会影响所有 Button 实例的渲染。
- **影响评估**：🟡 低。
- **修复建议**：

```csharp
public static readonly IReadOnlyList<double> DashedStyle = new[] { 4d, 2d };
```

---

### 5.2 🟡 PresetPalettes / ThemeVariantCalculator — 内部静态字典未限制

主文档 4.6 已修复 "静态 ConcurrentDictionary 缓存无大小限制"。建议再次审计所有内部 `static Dictionary`，确保：
- 缓存 key 不是控件引用（避免控件无法 GC）
- 容量上限或 LRU 清理策略

---

### 5.3 🟡 Gallery — MediaCatalog / ShowCaseRegistry 静态集合

- **文件**：`controlgallery/AtomUIGallery/ShowCases/...`
- **问题**：showcase 注册使用静态集合，长期运行会持有所有 showcase 视图实例。Gallery 作为 demo 不会长期运行，影响可忽略。
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
- **问题**：`_cardExpiredTimer`、`_cleanupTimer` 以 50ms 固定间隔运行，即使没有通知也在运行。
- **影响评估**：🟠 中（持续 CPU 占用，节能场景影响明显）。
- **修复建议**：改为按需启动：

```csharp
if (_notifications.Count == 0)
{
    _cardExpiredTimer.Stop();
    _cleanupTimer.Stop();
}
```

---

### 6.3 🟡 Icon Geometry — StreamGeometry.Parse 重复调用

- **文件**：`src/AtomUI.Icons.AntDesign/GeneratedIcons/*.g.cs`（800+ 图标）
- **问题**：每个生成图标类的 `Build` 方法每次调用都 `StreamGeometry.Parse("...")`，频繁显示图标时重复解析 SVG 路径。
- **影响评估**：🟡 低。
- **修复建议**：在图标类中把 `StreamGeometry` 缓存为 `static readonly` 字段（惰性初始化），需改 `AtomUI.Icons.AntDesign.Generator` 模板。

---

### 6.4 🟡 InterpolateUtils.InterpolateStops — 每帧分配

- **文件**：`src/AtomUI.Core/Animations/InterpolateUtils.cs` 第 289、344 行
- **问题**：动画每一帧调用，创建 `IReadOnlyList<ImmutableGradientStop>`（LINQ `.ToList()`），可能产生 LOH 分配。
- **修复建议**：使用 `ArrayPool<ImmutableGradientStop>` 或静态预分配缓冲。

---

### 6.5 🟡 控件模板中 Brush / Pen 每次创建

审计 `src/AtomUI.Desktop.Controls/**/Themes/*.cs` 中生成的 Pen/Brush 都是 `new Pen(...)` 而非 `ImmutablePen.Pool`。在 Avalonia 11 中创建成本相对低，但视觉量大时仍有优化空间。

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

在 `AtomUI.Controls` 中提供一个抽象基类，统一管理 `CompositeDisposable`、CTS、Timer：

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
        DetachDisposables.Clear();
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

---

## 九、修复优先级清单

| 序号 | 问题 | 严重度 | 工作量 | 优先级 |
|-----|------|--------|--------|--------|
| P1 | 2.1 剩余控件缺少 OnDetachedFromVisualTree（NavMenuItem / NotificationCard / PopupConfirm / Collapse / Card token scope） | 🔴 高 | 中 | 🚨 最高 |
| P2 | 3.1 Select / AutoComplete / Mentions / Form / Dialog — CTS 未 Dispose | 🔴 高 | 中 | 🚨 最高 |
| P3 | 4.5、4.6 WaveSpirit / SwitchKnob / Spin CTS 模式 | 🟠 中 | 小 | 高 |
| P4 | 3.5 Badge Adorner CTS 未 Dispose（5 处） | 🟠 中 | 小 | 高 |
| P5 | 2.3 ToolTip Timer 重叠（主文档 5.17） | 🟠 中 | 小 | 高 |
| P6 | 6.2 WindowNotificationManager 50ms 常驻 Timer | 🟠 中 | 中 | 中 |
| P7 | 4.1 AbstractQRCode / PreviewImageSource Bitmap 未 Dispose | 🟠 中 | 小 | 中 |
| P8 | 2.2 Space 子控件订阅幂等性（主文档 5.16） | 🟠 中 | 小 | 中 |
| P9 | 3.2 Gallery 8 处 async void | 🟠 中 | 小 | 中 |
| P10 | 3.3 VirtualizingCarouselPanel `_transition` CTS | 🟠 中 | 小 | 中 |
| P11 | 3.4 FileUploadScheduler 局部 CTS Dispose | 🟠 中 | 最小 | 中 |
| P12 | 6.1 Watermark 渲染优化（主文档 5.18） | 🟠 中 | 中 | 中 |
| P13 | 5.1 ButtonTheme.DashedStyle 改 IReadOnlyList | 🟡 低 | 最小 | 低 |
| P14 | 6.3 Icon Geometry 缓存 | 🟡 低 | 中（改生成器） | 低 |
| P15 | 6.4 InterpolateUtils LOH 分配 | 🟡 低 | 中 | 低 |
| P16 | 8.1 提供 AtomTemplatedControl 基类 | 架构 | 中 | 低 |
| P17 | 8.2 Roslyn 分析器 | 架构 | 大 | 低 |
| P18 | 8.4 MemoryLeakTests 自动化 | 架构 | 中 | 低 |

---

## 建议的后续步骤

1. **立即处理 P1-P5**：涉及 🔴 高严重度与高频资源泄漏问题。
2. **建立 CI 检查**：P17 Roslyn 分析器长期防止同类问题再次引入。
3. **按季度审计**：运行 `git log --stat src/**.cs` 检测新代码是否引入 `async void`、`+= lambda` 等反模式。

---

> **报告生成方式**：基于 grep 模式扫描 + 关键文件上下文阅读 + 对照 Avalonia v11 生命周期约定 + 参考已有主文档分析。

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

## 四、资源管理与 IDisposable

### 4.2 🟠 ColorPickerHelpers.CreateBitmapFromPixelData — 调用者需主动 Dispose

- **文件**：`src/AtomUI.Desktop.Controls.ColorPicker/Utils/ColorPickerHelpers.cs`
- **问题**：返回 `new Bitmap(...)`，调用者在频繁更新时需要主动 Dispose 前一个 Bitmap。XML doc 中无 "Caller must Dispose" 标注，且 `<returns>` 标签描述有误（写的是 `WriteableBitmap`）。
- **修复建议**：在 XML doc 明确标注 "Caller must Dispose"，修正 `<returns>` 描述。

---

### 4.3 🟠 RenderTargetBitmap 泄漏风险 — ControlExtensions.cs

- **文件**：`src/AtomUI.Controls.Shared/Utils/ControlExtensions.cs` 第 23 行
- **问题**：`CaptureCurrentBitmap` 返回 `RenderTargetBitmap`（实现 `IDisposable`），完全没有 XML doc，调用者不知道需要 Dispose。RenderTargetBitmap 在 GPU 上驻留，开销更高。
- **修复建议**：添加 XML doc 明确标注 "Caller must Dispose"，或改为 `using` 块模式。

---

### 4.4 🟠 AbstractMarqueeLabel — `_cancellationTokenSource` 前未 Dispose 旧的

- **文件**：`src/AtomUI.Controls/MarqueeLabel/AbstractMarqueeLabel.cs` 第 144 行
- **问题**：`_cancellationTokenSource?.Cancel();` 存在，但缺 `Dispose()`。
- **修复建议**：统一 Cancel + Dispose 模式。

---

### 4.5 🟠 WaveSpiritDecorator — `_cancellationTokenSource = new CTS()` 未 Dispose 旧实例

- **文件**：`src/AtomUI.Controls/Primitives/WaveSpiritDecorator.cs` 第 292 行
- **问题**：`WaveSpirit` 点击涟漪高频触发（每次点击都创建 CTS），之前的 CTS 未 Dispose。`OnDetachedFromVisualTree` 和 `OnPropertyChanged` 中也只 Cancel 不 Dispose。
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
- **问题**：均是 `_cancellationTokenSource = new CancellationTokenSource()` 前无 Dispose，`OnDetachedFromVisualTree` 中也只 Cancel 不 Dispose。
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
| P3 | 4.5、4.6 WaveSpirit / SwitchKnob / Spin CTS 模式 | 🟠 中 | 小 | 高 |
| P9 | 3.2 Gallery 8 处 async void | 🟠 中 | 小 | 中 |
| P13 | 5.1 ButtonTheme.DashedStyle 改 IReadOnlyList | 🟡 低 | 最小 | 低 |
| P14 | 6.3 Icon Geometry 缓存 | 🟡 低 | 中（改生成器） | 低 |
| P15 | 6.4 InterpolateUtils LOH 分配 | 🟡 低 | 中 | 低 |
| P16 | 8.1 提供 AtomTemplatedControl 基类 | 架构 | 中 | 低 |
| P17 | 8.2 Roslyn 分析器 | 架构 | 大 | 低 |
| P18 | 8.4 MemoryLeakTests 自动化 | 架构 | 中 | 低 |

---

## 建议的后续步骤

1. **立即处理 P3**：WaveSpirit / SwitchKnob / Spin CTS 高频泄漏。
2. **建立 CI 检查**：P17 Roslyn 分析器长期防止同类问题再次引入。
3. **按季度审计**：运行 `git log --stat src/**.cs` 检测新代码是否引入 `async void`、`+= lambda` 等反模式。

---

> **报告生成方式**：基于 grep 模式扫描 + 关键文件上下文阅读 + 对照 Avalonia v11 生命周期约定 + 参考已有主文档分析。

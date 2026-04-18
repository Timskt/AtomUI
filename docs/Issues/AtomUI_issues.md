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
- [四、集合 / 缓存 / 静态字段](#四集合--缓存--静态字段)
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

## 四、集合 / 缓存 / 静态字段

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

### 7.3 🟡 BaseGalleryApplication 静态主题资源

`BaseGalleryApplication.axaml.cs` 注册主题时使用静态字段。Gallery 自身不切换应用实例，影响可忽略。

---

## 九、修复优先级清单

| 序号 | 问题 | 严重度 | 工作量 | 优先级 |
|-----|------|--------|--------|--------|
| P16 | 8.1 提供 AtomTemplatedControl 基类 | 架构 | 中 | 低 |
| P17 | 8.2 Roslyn 分析器 | 架构 | 大 | 低 |
| P18 | 8.4 MemoryLeakTests 自动化 | 架构 | 中 | 低 |

---

## 建议的后续步骤

1. **建立 CI 检查**：P17 Roslyn 分析器长期防止同类问题再次引入。
2. **按季度审计**：运行 `git log --stat src/**.cs` 检测新代码是否引入 `async void`、`+= lambda` 等反模式。

---

> **报告生成方式**：基于 grep 模式扫描 + 关键文件上下文阅读 + 对照 Avalonia v11 生命周期约定 + 参考已有主文档分析。

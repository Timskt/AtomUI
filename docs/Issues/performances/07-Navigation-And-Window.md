# 07 · 导航 / 窗口 族性能分析

> **覆盖控件**：`Breadcrumb` · `Chrome` · `Window` · `Tour` · `Form` · `Upload` · `Slider`

---

## 问题严重度总览

| # | 问题 | 严重度 | 影响面 |
|---|---|---|---|
| **UP-D1** | `Upload` 目录下 4+ 文件统一 "OnLoaded new Transitions + OnUnloaded = null" 模式 | 🟠 中 | 大量文件列表 |
| **BRD-D1** | `BreadcrumbItem.cs:154,172` Transitions 抖动 | 🟡 低 | 面包屑每项 |
| **TOU-B1** | `Tour.cs:367,732` 属性变化多路径 + `new Thickness()` / `Inflate` | 🟡 低 | Tour 步进期 |

---

## 1. UP-D1：Upload 控件系列 Transitions 抖动

**命中文件**：

| 文件 | 行号 |
|---|---|
| `src/AtomUI.Desktop.Controls/Upload/AbstractUploadListItem.cs` | 158, 163, 180 |
| `src/AtomUI.Desktop.Controls/Upload/AbstractUploadPictureContent.cs` | 111, 133 |
| `src/AtomUI.Desktop.Controls/Upload/UploadDefaultDropArea.cs` | 151, 164 |
| `src/AtomUI.Desktop.Controls/Upload/UploadTriggerContent.cs` | 164, 183 |

模式：`Transitions = new Transitions() { ... }` in OnLoaded, `Transitions = null` in OnUnloaded。

### 根因

- 与基线 A-4 / BTN-D1 / NAV-D1 完全同源。
- Upload 组件在批量上传场景（几十~几百个文件列表项）时放大明显。
- 尤其 `AbstractUploadListItem.cs:158` 明确 `Transitions = new Transitions();` 后续再 Add。

### 解决方案

- 引入 `UploadTransitions` 静态工厂，在 `OnApplyTemplate` 时一次性创建并赋值；`OnUnloaded` 不再清空（`Transitions` 在控件未在可视树时不消耗动画调度器资源，不必 null）。
- 若必须释放（为了解绑 `TransitionsProperty` 订阅），至少改为"可变字段复用"而非每次 `new`。

### 预估收益

- 100 文件批量上传：减少 300+ `Transitions`/`ITransition` 分配与释放。

---

## 2. BRD-D1：BreadcrumbItem Transitions

**文件**：`src/AtomUI.Desktop.Controls/Breadcrumb/BreadcrumbItem.cs:154,172`

同 UP-D1；面包屑通常 3–6 项，影响有限，仅作风格统一。

---

## 3. TOU-B1：Tour 流程属性变化

**文件**：`src/AtomUI.Desktop.Controls/Tour/Tour.cs:367,732`

```csharp
367: protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change) { ... }
698: TargetClipBounds = targetBounds.Inflate(new Thickness(GapOffsetX, GapOffsetY));;
732: LayoutHelper.MeasureChild(_popup.Child, Size.Infinity, new Thickness());
```

### 评估

- `Tour` 步进属于人机交互低频事件；`new Thickness()` 每次 1 个小 struct 分配，不构成问题。
- `OnPropertyChanged` 未发现多路径触发同一重建的模式；仅建议：Tour 步进属性多个一起变时，考虑 deferred rebuild 模式（参照 BTN-B1）。

---

## 4. Form / Window / Chrome

**评估**：
- `Form` 控件主要在表单校验路径，单次表单提交校验量不大，未见热路径问题。
- `Window` 扩展：包括自定义标题栏、状态按钮、拖拽行为；`Chrome` 提供自绘窗口边框与按钮。grep 未命中 Style / Transitions / LINQ 反模式。
- `Slider`：模板化控件，滚动与定位在 Avalonia 原生 Track 上运行，未发现独立热点。

---

## 无重大独立问题的控件

| 控件 | 说明 |
|---|---|
| `Breadcrumb` | 单实例数量少，BRD-D1 影响有限 |
| `Chrome` | 窗口级单例 |
| `Window` | 窗口级单例 |
| `Tour` | 低频使用 |
| `Form` | 校验热路径已较精简 |
| `Slider` | 原生扩展，无独立热点 |



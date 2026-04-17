# 02 · 按钮 / 切换 族控件性能分析

> **覆盖控件**：`Buttons` (Button / IconButton / HyperLinkButton / DropdownButton / SplitButton) · `OptionButtonGroup` · `RadioButton` · `CheckBox` · `Switch` · `Segmented` · `FloatButton` · `ToggleIconButton`

按钮/切换是 UI 中基数最大的一类控件——一个典型页面数十到数百个按钮是常态。本族问题虽然单点不算致命，但**被放大后影响显著**。

---

## 问题严重度总览

| # | 问题 | 严重度 | 影响面 |
|---|---|---|---|
| **BTN-D1** | `Button.ConfigureTransitions` 在多属性变化时反复 `new Transitions()` | 🟠 高 | 所有 Button 子类 / SplitButton / DropdownButton |
| **BTN-B1** | Button 的多个外观属性变化各自触发一次 `ConfigureTransitions(true)` / `ConfigureWaveSpiritType` | 🟡 中 | 同上 |
| **TOG-D1** | 各类 ToggleIconButton / CheckBox / RadioButton 的 Transitions 通常无需根据属性重建，但继承链上仍受 BTN-D1 影响 | 🟡 中 | 同上 |

---

## 1. BTN-D1：Button 每属性变化重建 Transitions

**文件**：`src/AtomUI.Desktop.Controls/Buttons/Button.cs:359-389`

```csharp
private void ConfigureTransitions(bool force)
{
    if (IsMotionEnabled)
    {
        if (force || Transitions == null)
        {
            var transitions = new Transitions();
            transitions.Add(TransitionUtils.CreateTransition<SolidColorBrushTransition>(BackgroundProperty));
            if (ButtonType == ButtonType.Primary)
            {
                if (IsGhost)
                {
                    transitions.Add(TransitionUtils.CreateTransition<SolidColorBrushTransition>(BorderBrushProperty));
                    transitions.Add(TransitionUtils.CreateTransition<SolidColorBrushTransition>(ForegroundProperty));
                }
            }
            else if (ButtonType == ButtonType.Default || ButtonType == ButtonType.Dashed)
            {
                transitions.Add(TransitionUtils.CreateTransition<SolidColorBrushTransition>(BorderBrushProperty));
                transitions.Add(TransitionUtils.CreateTransition<SolidColorBrushTransition>(ForegroundProperty));
            }
            else if (ButtonType == ButtonType.Link)
            {
                transitions.Add(TransitionUtils.CreateTransition<SolidColorBrushTransition>(ForegroundProperty));
            }
            Transitions = transitions;
        }
    }
}
```

调用点（可能每变化都传 `force:true`）：`ButtonType` / `IsGhost` / `IsMotionEnabled` 的 `OnPropertyChanged` 分支。

### 根因

1. `Transitions` 本身和每个 `ITransition` 都是 Avalonia 动画引擎的状态容器，新建开销可观（每次创建 3–5 个小对象）。
2. `ButtonType`、`IsGhost` 在用户操作下常常不变——但初始化阶段每个属性都会各自触发一次 `OnPropertyChanged`，导致单个 Button **初始化期可能重建 3–4 次 Transitions**。
3. 页面上 50+ 按钮时，启动即产生 150+ 次多余分配。

### 解决方案

**方案 A（推荐）**：按 `(ButtonType, IsGhost)` 组合静态缓存 Transitions 模板。
```csharp
private static readonly Dictionary<(ButtonType, bool), ImmutableList<ITransition>> s_transitionsCache = ...;
```
每个实例 `Transitions` 直接赋值**共享 Transitions 实例**（Avalonia 支持多控件共享同一 `Transitions` 集合对象吗？需要确认；若不支持，缓存"配方"再按配方新建，但这样仍要分配——所以真正的收益点是下一条）。

**方案 B（更实用）**：合并属性变化触发。引入 `_pendingTransitionsRebuild` 字段，在 `OnPropertyChanged` 只标记，`OnApplyTemplate` / `OnMeasure` 前集中执行一次。

**方案 C**：将 "过渡" 放到 `ControlTheme` AXAML 层用 `<Setter Property="Transitions">` 按 pseudo-class 或 selector 配置，避免代码路径动态构建。

### 预估收益

- 方案 B 在 50 按钮页启动期减少 ~100 次分配，节省 ~50KB GC。
- 方案 C 彻底移除代码路径——每个 Button 初始化少一个 `Transitions` 对象生命周期。

---

## 2. BTN-B1：外观属性变化级联触发多次重配

**位置**：`Button.cs` 的 `OnPropertyChanged`（推测，基于上文 `ConfigureTransitions(true)` 的调用点）同时会调用 `ConfigureWaveSpiritType()` / 其他 setup。

### 根因

- 多属性变化（主题切换时一次性改 `ButtonType`、`Shape`、`IsGhost`）→ 每个变化都单独触发一次 `ConfigureTransitions(true)` + `ConfigureWaveSpiritType` + Pseudo-class 更新。

### 解决方案

- 合并进"状态机"：引入 `ScheduleRebuild()` + `Dispatcher.UIThread.Post(DispatcherPriority.Loaded)`，本帧内的所有属性变化只触发一次重建。

### 预估收益

- 主题切换 / 动态改样式时减少 **N–1** 次重复工作（N = 批变更数）。

---

## 3. TOG-D1：Toggle 族 Transitions 继承自 Button

**文件**：`ToggleIconButton`、`CheckBox`、`RadioButton`、`Switch`、`Segmented` 底层 ToggleButton 均可能走类似 Button 的 Transitions 构造——未发现各自独立的 `new Transitions()` 热点，但**继承链上 Button 基类问题全部生效**。

### 方案

- 修复 BTN-D1 后自动受益，无需单独动作。

---

## 无重大独立问题的控件

| 控件 | 说明 |
|---|---|
| `OptionButtonGroup` | 内部是 ItemsControl + RadioButton，本身无热路径问题 |
| `RadioButton` / `CheckBox` | 纯 ToggleButton 扩展；Transitions 走基类路径 |
| `Switch` | 模板化，未发现 `new Style` / `new Transitions` 反模式 |
| `Segmented` | 自定义 ItemsControl，Measure/Arrange 未见冗余 invalidate |
| `FloatButton` | 单控件 + Popup，实例数量少，即使有小分配也可忽略 |
| `IconButton` / `HyperLinkButton` | 继承 Button，问题同 BTN-D1 |
| `DropdownButton` / `SplitButton` | 组合控件；若内含 Button 实例即继承 BTN-D1 |



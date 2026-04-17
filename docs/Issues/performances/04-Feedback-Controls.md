# 04 · 反馈控件 族性能分析

> **覆盖控件**：`Alert` · `Message` · `Notifications` · `Dialog` · `Drawer` · `MessageBox` · `Popup` · `Flyouts` · `Tooltip` · `PopupConfirm` · `ProgressBar` · `Spin` · `Skeleton` · `Result` · `Empty`

反馈控件多为"按需打开→关闭"模式，生命周期短，但**频繁打开关闭**（如通知流、Tooltip 跟随鼠标）会把隐藏问题放大。

---

## 问题严重度总览

| # | 问题 | 严重度 | 影响面 |
|---|---|---|---|
| **SPI-A1** | `SpinIndicator` 每实例构造 3 Style + 3 `Selectors.Or` | 🟠 高 | 页面级 Spin 大量使用 |
| **WNM-E1** | `WindowNotificationManager.RemoveExcessNotifications` `OfType.Where.ToList` | 🟠 中 | 高频通知流 |
| **DLG-E1** | `DialogLayer` `GetVisualDescendants().OfType<DialogLayerManager>().FirstOrDefault()` 每次打开 | 🟠 中 | Dialog / MessageBox 每次打开 |
| **POP-A1** | `Popup.cs:208` 每实例 `new Style()` + `Styles.Add` | 🟡 中 | 所有 Popup 驱动的控件 |
| **RES-A1** | `Result.cs:23-39` 每实例多个 Style | 🟡 中 | 每个 Result / Status 页面 |
| **NOT-H1** | `NotificationCard.cs:360 InvalidateVisual()` | 🟡 低 | 通知关闭动画期 |

---

## 1. SPI-A1：SpinIndicator 每实例 Style + Selectors.Or

**文件**：`src/AtomUI.Desktop.Controls/Spin/SpinIndicator.cs:19-59`

```csharp
private void ConfigureInstanceStyles()
{
    {
        var middleStyle = new Style(x => x.PropertyEquals(SizeTypeProperty, ...));
        var iconStyle = new Style(x => Selectors.Or(
            x.Nesting().Descendant().OfType<Icon>()....,
            x.Nesting().Descendant().OfType<PathIcon>()....
        ));
        ...
        Styles.Add(middleStyle);
    }
    // smallStyle / largeStyle 各重复一次
}
```

### 根因

- 与基线 A-1 完全同构：3 个 Style × (1 外层 + 1 Or 内层 + 2 分支) ≈ 每实例 15–20 选择器节点。
- `Spin` 在页面级加载态大量出现（列表/卡片加载蒙层），页面可能有 10+ Spin 实例 → 150–200 选择器节点 per 页面。

### 解决方案

- 把这 3 个 Style 提升到 `SpinIndicatorTheme.axaml` 或 ControlTheme 的 `<Style Selector="^[SizeType=...]">` 中 —— **AXAML 层的 ControlTheme 只构造一次**，所有实例共享。
- 如果确实需要 code-behind，参照基线 A-1 建议：在静态初始化器里构建一次 `Styles`，然后 `Styles.Add(s_sharedStyles)` —— 但 Avalonia 的 `Styles` 所有权模型可能不支持共享，**AXAML 方案最稳**。

### 预估收益

- 10 个 Spin 的页面节省 ~100 个选择器节点 + 30 Style 实例的初始化成本与 GC。

---

## 2. WNM-E1：Notification 超限剔除

**文件**：`src/AtomUI.Desktop.Controls/Notifications/WindowNotificationManager.cs:208-221`

```csharp
var visibleNotifications = _items!.OfType<NotificationCard>().Where(n => !n.IsClosing).ToList();
var excessCount = visibleNotifications.Count - MaxItems;
if (excessCount > 0)
{
    for (int i = 0; i < excessCount; i++)
        visibleNotifications[i].Close();
}
```

### 根因

- 每次 `Show()` 都执行一次 LINQ 三连击。
- 实际上 `_items` 数量通常很小（`MaxItems` 默认 3–5），真实消耗不大，但每秒多次通知时仍可见。

### 解决方案

```csharp
int visibleCount = 0;
int closeNeed;
foreach (var item in _items!)
{
    if (item is NotificationCard c && !c.IsClosing)
        visibleCount++;
}
closeNeed = visibleCount - MaxItems;
if (closeNeed <= 0) return;

foreach (var item in _items!)
{
    if (item is NotificationCard c && !c.IsClosing)
    {
        c.Close();
        if (--closeNeed == 0) break;
    }
}
```

### 预估收益

- 零分配、单次遍历。

---

## 3. DLG-E1：Dialog 层查找

**文件**：`src/AtomUI.Desktop.Controls/Primitives/DialogLayer.cs:75`

```csharp
var layers = tl.GetVisualDescendants().OfType<DialogLayerManager>().FirstOrDefault();
```

### 根因

- 每次打开 Dialog / MessageBox 时遍历 TopLevel 可视树。
- TopLevel 可视树通常不小（应用主 UI 全量）——Dialog 打开慢的一大贡献源。

### 解决方案

- 由 `DialogLayerManager` 在 `OnAttachedToVisualTree` 时把自己注册到 `TopLevel` 的 `AttachedProperty` / 字段；查找改为 O(1)：
  ```csharp
  DialogLayerManager? manager = DialogLayerManager.GetForTopLevel(tl);
  ```

### 预估收益

- Dialog 打开时间缩短 1–5ms（取决于可视树深度）。

---

## 4. POP-A1：Popup 每实例 Style

**文件**：`src/AtomUI.Desktop.Controls/Popup/Popup.cs:208`

```csharp
var style = new Style();
// ... Setter 赋值 ...
Styles.Add(style);
```

### 根因 / 方案

- 单 Style 不算致命，但 Popup 被大量控件（Tooltip / Flyout / PopupConfirm / Menu / Select / AutoComplete）内部驱动，数量倍增。
- 迁移到 `PopupTheme.axaml` ControlTheme。

### 预估收益

- 1 个 Popup 省 1 Style 对象；按 50 个驱动源 ≈ 50 Style 对象。

---

## 5. RES-A1：Result 每实例 Style

**文件**：`src/AtomUI.Desktop.Controls/Result/Result.cs:23-39`

```csharp
var iconStyle = new Style(x => x.Is<AbstractResult>().Descendant().Name("PART_StatusIconPresenter").Child());
// ... 同类多个 style ...
Styles.Add(iconStyle);
Styles.Add(infoStyle);
```

### 方案

- 同 SPI-A1，迁移到 ControlTheme AXAML。
- `Result` 通常单页单实例，优先级较低。

---

## 6. NOT-H1：`NotificationCard.InvalidateVisual()`

**文件**：`src/AtomUI.Desktop.Controls/Notifications/NotificationCard.cs:360`

单次显式 `InvalidateVisual`，位于关闭动画开始处——合理，不算问题。仅记录供回归时留意。

---

## 无重大独立问题的控件

| 控件 | 说明 |
|---|---|
| `Alert` | `AlertToken` 中多次 `new Thickness(...)` 属于 **Token 初始化期一次性** 分配，不在热路径 |
| `Message` | 依赖 `WindowNotificationManager` 基础设施，受 WNM-E1 影响 |
| `Drawer` / `Dialog` / `MessageBox` | 除 DLG-E1 外未见独立热点；`Dialog.cs` 的 `Subscribe` 有配套 `Dispose` |
| `Flyouts` / `Tooltip` / `PopupConfirm` | 依赖 `Popup`，受 POP-A1 影响 |
| `ProgressBar` | 模板 + 动画，无热路径问题 |
| `Skeleton` | 纯 shimmer 动画，无分配热点 |
| `Empty` | 静态模板，无动态构建 |
| `Spin`（容器） | 问题集中在 `SpinIndicator`，容器本身无独立问题 |



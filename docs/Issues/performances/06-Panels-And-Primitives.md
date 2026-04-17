# 06 · 面板 / Primitives 族性能分析

> **覆盖控件**：`BoxPanel` · `FlexPanel` · `Grid` (Row / Col) · `Space` · `Splitter` · `SplitView` · `ScrollViewer` · `AdornerLayer` · `Primitives` (除 AddOnDecoratedBox 外)
> **前置文档**：[《AddOnDecoratedBox 与 TreeView 性能深度分析报告》](../AddOnDecoratedBox_TreeView_Performance.md)

Primitives 层是所有高级控件的基础；这里的任何 N 次放大都会对整个库产生系统影响。

---

## 问题严重度总览

| # | 问题 | 严重度 | 影响面 |
|---|---|---|---|
| **SPC-A1** | `Space` 构造函数 3 SizeType Style | 🟠 高 | Space 大量用作容器 |
| **GRID-E1** | `Grid/Row.cs` `GetOrderedChildren` 每次 Measure 构造 + `OrderBy.ThenBy.ToList` | 🟡 中 | 每次 Row 重排 |
| **GRID-H1** | `Row.cs:79-80` `InvalidateMeasure` + `InvalidateArrange` 成对调用 | 🟡 低 | 响应式断点变化 |
| **GRID-B1** | `Col.cs:143-144` `parent.InvalidateMeasure()` + `InvalidateArrange()` 成对调用 | 🟡 低 | Col 属性变化 |
| **SPL-E1** | `SplitterPanel.cs:112,140` `Children.ToList()` / `_trackedPanels.ToList()` | 🟡 中 | 拖拽期 |
| **CAN-E1** | `CandidateList.cs:218` ScrollViewer 查找（详见 `01-Inputs-And-Editors.md`） | 🟡 中 | 下拉 |
| **AOB-\*** | 详见基线文档 | 🔴 | AddOnDecoratedBox |

---

## 1. SPC-A1：Space 每实例 3 Style

**文件**：`src/AtomUI.Desktop.Controls/Space/Space.cs:162-192`

```csharp
public Space()
{
    this.RegisterTokenResourceScope(SpaceToken.ScopeProvider);
    ConfigureInstanceStyle();  // ← 每实例执行
    Children.CollectionChanged += HandleChildrenChanged;
}

private void ConfigureInstanceStyle()
{
    {
        var middleStyle = new Style(x =>
            x.PropertyEquals(SizeTypeProperty, CustomizableSizeType.Middle));
        middleStyle.Add(ItemSpacingProperty, SpaceTokenKind.GapMiddleSize);
        middleStyle.Add(LineSpacingProperty, SpaceTokenKind.GapMiddleSize);
        Styles.Add(middleStyle);
    }
    // smallStyle / largeStyle 各一遍
}
```

### 根因

- 每 Space 实例都持有 3 个 Style + 6 个 Setter + 3 个 PropertyEquals selector。
- `Space` 是最基础的布局容器，几乎每个页面用十几次；典型页面 10–20 个 Space → 30–60 个冗余 Style。

### 解决方案

- 把 3 个 Style 迁移到 `SpaceTheme.axaml` ControlTheme：
  ```xml
  <ControlTheme TargetType="atom:Space" x:Key="{x:Type atom:Space}">
      <Style Selector="^[SizeType=Middle]">
          <Setter Property="ItemSpacing" Value="{atom:TokenResource SpaceGapMiddleSize}"/>
          <Setter Property="LineSpacing" Value="{atom:TokenResource SpaceGapMiddleSize}"/>
      </Style>
      <!-- Small / Large 同 -->
  </ControlTheme>
  ```
- 删除 `ConfigureInstanceStyle()`。

### 预估收益

- 启动期每页减少数十个 Style 对象；Space 构造提速 ~30%。

---

## 2. GRID-E1：Row 每次 Measure 构造子项信息列表

**文件**：`src/AtomUI.Desktop.Controls/Grid/Row.cs:234-255`

```csharp
private List<RowChildInfo> GetOrderedChildren(MediaBreakPoint breakPoint)
{
    var children = new List<RowChildInfo>(Children.Count);
    for (var i = 0; i < Children.Count; i++)
    {
        if (Children[i] is not Control child || !child.IsVisible) continue;
        var layout = child is Col col
            ? col.ResolveLayout(breakPoint)
            : new GridColLayout(0, 0, 0, 0, 0);  // ← struct 构造
        children.Add(new RowChildInfo(child, layout, i));
    }
    return children
        .OrderBy(info => info.Layout.Order)
        .ThenBy(info => info.Index)
        .ToList();  // ← 再一次分配
}
```

### 根因

- 每次 `MeasureOverride` 至少调用一次 → 每次都分配新 `List<RowChildInfo>` + `OrderBy`/`ThenBy` 的内部排序数组 + 再 `ToList` 一份。
- 一个栅格页面 10 行 → 每次窗口大小变化都 10 次重排。

### 解决方案

1. 缓存复用：
   ```csharp
   private readonly List<RowChildInfo> _orderedCache = new();
   private void RebuildOrderedChildren(MediaBreakPoint bp)
   {
       _orderedCache.Clear();
       for (...) _orderedCache.Add(...);
       _orderedCache.Sort((a, b) => 
           a.Layout.Order != b.Layout.Order
               ? a.Layout.Order.CompareTo(b.Layout.Order)
               : a.Index.CompareTo(b.Index));
   }
   ```
2. 当 `Children` 或 `BreakPoint` 未变时跳过重建。

### 预估收益

- 尺寸变动期每 Measure 节省 2 次数组分配；排序算法改用 `List<T>.Sort` 零额外分配。

---

## 3. GRID-H1 / GRID-B1：成对 invalidate

**Row.cs:79-80**：
```csharp
InvalidateMeasure();
InvalidateArrange();
```

**Col.cs:143-144**：
```csharp
parent.InvalidateMeasure();
parent.InvalidateArrange();
```

### 根因

- `InvalidateMeasure()` **会**隐式 invalidate arrange。重复调用无害但冗余。

### 解决方案

- 只保留 `InvalidateMeasure()`。

### 预估收益

- 代码洁净；对布局管理器省一次队列入队。

---

## 4. SPL-E1：SplitterPanel 拖拽期 `.ToList()`

**文件**：`src/AtomUI.Desktop.Controls/Splitter/SplitterPanel.cs:112,140`

```csharp
112: var panels = Children.ToList();
140: foreach (var panel in _trackedPanels.ToList())
```

### 根因

- 拖拽期 Measure/Arrange 频繁触发；每次拖拽事件都分配副本。
- `_trackedPanels.ToList()` 是经典的"迭代期修改集合"防御——但可以改为倒序 for 循环。

### 解决方案

1. `Children.ToList()`：若只读迭代，改为 `for (var i = 0; i < Children.Count; i++)`。
2. `_trackedPanels.ToList()`：若要删除，改为 `for (var i = _trackedPanels.Count - 1; i >= 0; i--)`。

### 预估收益

- 拖拽期每帧省 1–2 次数组分配。

---

## 5. AdornerLayer / ScrollViewer / BoxPanel / FlexPanel / SplitView

**评估**：
- `AdornerLayer` 是 Avalonia 原生扩展，AtomUI 部分未见热点。
- `ScrollViewer` 扩展主要是模板 + 少量行为，未发现 LINQ / new Style 热路径。
- `BoxPanel` / `FlexPanel` 未命中任何 grep 模式，视为健康。
- `SplitView` 模板化，未发现问题。

---

## 6. Primitives 其他成员

- `AddOnDecoratedBox` / `AddOnDecoratedInnerBox`：**详见基线文档 A-1 ~ A-4**。
- `CandidateList`：CAN-E1（已在 `01-Inputs-And-Editors.md` 描述）。
- `DialogLayer`：DLG-E1（详见 `04-Feedback-Controls.md`）。
- `Popup` / `PopupBuddyLayer`：POP-A1 + `new Thickness` 在拖拽位置纠正期调用（`PopupBuddyLayer.cs:454-466`），属于手势热路径的合理使用，未放大到问题级别。

---

## 无重大独立问题的控件

| 控件 | 说明 |
|---|---|
| `BoxPanel` | 简单布局 Panel，无热点 |
| `FlexPanel` | Flex 布局，Measure/Arrange 未见冗余 |
| `SplitView` | 模板 + 固定 2 区域 |
| `ScrollViewer` (AtomUI 扩展) | 模板扩展为主 |
| `AdornerLayer` | 低频使用 |



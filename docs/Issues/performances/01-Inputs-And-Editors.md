# 01 · 输入 / 编辑器 族控件性能分析

> **覆盖控件**：`Input` (LineEdit / SearchEdit / TextArea) · `NumericUpDown` · `Mentions` · `AutoComplete` · `Cascader` · `Select` · `TreeSelect` · `ComboBox` · `DatePicker` · `TimePicker`
> **前置文档**：[《AddOnDecoratedBox 与 TreeView 性能深度分析报告》](../AddOnDecoratedBox_TreeView_Performance.md)

本族绝大多数控件直接或间接继承自 `AddOnDecoratedBox`，**基线文档中的 A-1 / A-2 / A-3 / A-4 问题对它们全部生效，本文只补充其余独立问题。**

---

## 问题严重度总览

| # | 问题 | 严重度 | 影响面 |
|---|---|---|---|
| **AOB-E1** | `GetVisualAncestors().OfType<Control>()` 在多个控件的 `OnAttachedToVisualTree` / 初始化路径重复出现 | 🟠 高 | Select / AutoComplete / Mentions / Cascader / TreeSelect |
| **SEL-E1** | `Select/Select.cs` 5 处 `SelectedOptions?.ToList()` / `.Cast<>.ToList()` | 🟠 高 | Select 多选场景 |
| **CAN-E1** | `CandidateList` / `SelectCandidateList` 通过 `GetLogicalDescendants().OfType<ScrollViewer>().FirstOrDefault()` 查找 `ScrollViewer` | 🟡 中 | 所有下拉候选列表 |
| **CAS-D1** | `CascaderViewItem` OnLoaded/OnUnloaded 反复新建/清空 Transitions | 🟡 中 | 大量级联面板 item |
| **CAS-M1** | `Cascader` 级联节点访问走 `Menu/MenuItem` 的 `ITreeNode.Children => .ToList()` 同构问题 | 🔴 极严重 | 详见 M-1（`03-Data-Containers.md`） |
| **SEL-C1** | `ComboBox`/`Select`/`Mentions` 候选下拉列表未使用虚拟化 | 🟠 高 | 大候选数据集 |

---

## 1. AOB-E1：祖先查找反复调用 `GetVisualAncestors`

**命中文件**：

| 文件 | 行号 |
|---|---|
| `src/AtomUI.Desktop.Controls/AutoComplete/AbstractAutoComplete.cs` | 1366 |
| `src/AtomUI.Desktop.Controls/Select/AbstractSelect.cs` | 703 |
| `src/AtomUI.Desktop.Controls/Mentions/Mentions.cs` | 1081 |
| `src/AtomUI.Desktop.Controls/Cascader/Cascader.cs` | 478 |
| `src/AtomUI.Desktop.Controls/TreeSelect/TreeSelect.cs` | 506 |

典型模式：
```csharp
foreach (var parent in this.GetVisualAncestors().OfType<Control>())
{
    // 查找特定祖先（TopLevel / Form / Popup 等）
}
```

### 根因

- `GetVisualAncestors()` 返回 `IEnumerable<Visual>`，底层每步走 `VisualParent`，每个被迭代的节点都要走一次 `is Control` 类型检查（`OfType` 产生 `WhereSelectEnumerable`），热路径里 LINQ 分配迭代器。
- 这些路径在 `OnAttachedToVisualTree`、`OnOpened`、`InitializeUI` 中被多次调用；在深层嵌套树上（比如放在 `TabControl` + `Drawer` + `Card` 里），每次切换 Tab 都要遍历 10+ 层。

### 解决方案

1. 若目标是 `TopLevel`，直接用 `TopLevel.GetTopLevel(this)`，O(1)。
2. 若目标是某具体祖先（如 `Form`、`CompactSpaceAware`），实现通用扩展方法 `TryFindAncestor<T>(out T?)` 采用 `while (parent is not null) { if (parent is T t) ... }` 纯 `VisualParent` 循环，不分配迭代器。
3. 将结果缓存在字段（如 `_owningForm`），在 `OnDetachedFromVisualTree` 清空。

### 预估收益

- 单次调用节省约 **50–150 ns + 2 个迭代器对象**，放在 Tab 切换、弹窗打开等高频场景每次省 **数 μs + 零碎 GC**；对触发率 10+/s 的场景可观察。

---

## 2. SEL-E1：`Select` 多选集合频繁 `.ToList()`

**文件**：`src/AtomUI.Desktop.Controls/Select/Select.cs`

```csharp
284:            _candidateList.SelectedItems = SelectedOptions?.ToList();
550:                SelectedOptions = _candidateList.SelectedItems?.Cast<ISelectOption>().ToList();
596:                SelectedOptions = newSelectedSet.ToList();
669:                    _candidateList.SelectedItems = SelectedOptions?.ToList();
691:                _candidateList.SelectedItems = SelectedOptions?.ToList();
```

### 根因

- `SelectedOptions` 是 `IList<ISelectOption>`，已经是列表；`.ToList()` 又拷贝一份。
- `550`: `Cast<>().ToList()` — 如果候选列表本来就是同类型，`Cast` 分配迭代器 + `ToList` 分配新数组。
- 这些路径在"键入过滤 → 勾选 → 删除 tag"的闭环里都会命中。

### 解决方案

1. 区分"需要独立副本"（写入依赖）与"只是传递"（读）——后者直接赋值或用 `IReadOnlyList<T>`。
2. 用 `ArraySegment<T>` / `List<T>` 原地复用，避免重复 `ToList`。
3. 把 `Cast<ISelectOption>().ToList()` 改为预分配容量的 `List<ISelectOption>(source.Count)` + 手动循环，避免 `Cast` 迭代器对象。

### 预估收益

- 每次多选交互减少 **2–5 次短命对象**；大候选集（>100）下减少百字节级 GC。

---

## 3. CAN-E1：`CandidateList` / `SelectCandidateList` 模板内 ScrollViewer 查找

**文件**：
- `src/AtomUI.Desktop.Controls/Primitives/CandidateList/CandidateList.cs:218`
- `src/AtomUI.Desktop.Controls/Select/SelectCandidateList.cs:157`

```csharp
var sv = this.GetLogicalDescendants().OfType<ScrollViewer>().FirstOrDefault();
```

### 根因

- `ScrollViewer` 是模板固定部件，应该在 `OnApplyTemplate` 通过 `e.NameScope.Find<ScrollViewer>("PART_...")` 一次性拿到并缓存为字段。
- 目前每次需要滚动时都递归逻辑树 + 迭代器查找。

### 解决方案

- 给模板 `ScrollViewer` 标上 `x:Name="PART_ScrollViewer"`（若还没有）。
- 改为：
  ```csharp
  private ScrollViewer? _scrollViewer;
  protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
  {
      base.OnApplyTemplate(e);
      _scrollViewer = e.NameScope.Find<ScrollViewer>("PART_ScrollViewer");
  }
  ```

### 预估收益

- 首屏后每次滚动/选中节省 **1 次逻辑树递归**（候选列表项越多越明显）。

---

## 4. CAS-D1：`CascaderViewItem` Transitions 生命周期

**文件**：`src/AtomUI.Desktop.Controls/Cascader/CascaderViewItem.cs:373,387`

```csharp
373:        Transitions = null;   // OnUnloaded
387:        Transitions = null;   // 另一分支
// 同时在某处 new Transitions(...)
```

### 根因

- 与基线 A-4 同源：每个可见 CascaderViewItem 进出视图时都分配/释放 `Transitions`。
- Cascader 级联面板本身已使用虚拟化（`CascaderViewLevelListTheme.axaml` 使用 `VirtualizingStackPanel`），所以每次滚动都触发 Realize/Recycle，命中率高。

### 解决方案

- 参考基线文档 A-4 修复方案：改为**单例 Transitions**（整控件静态只读 `Transitions` 实例）在 `OnApplyTemplate` 时赋值。若涉及不同状态的 transition 差异，按状态分桶静态缓存。

### 预估收益

- 虚拟化滚动时每个 Realize 节省 **1 次对象分配**；滚动 1000 项节省 **1000 次 GC alloc**。

---

## 5. CAS-M1：级联树节点访问触发 `Items.OfType<>().ToList()`

**位置**：`src/AtomUI.Desktop.Controls/Menu/MenuItem.cs:53`（Cascader 基于 Menu/CandidateList 组合）

详见 [03-Data-Containers · M-1](./03-Data-Containers.md#1-m-1menumenuitemitreenodechildren-同构-t-1-的致命问题)。Cascader 在构建路径/回显选中链路时会访问这个 Children。

---

## 6. SEL-J1：下拉候选列表虚拟化缺失（除 Cascader 外）

**命中文件**：`Select/SelectCandidateList`、`AutoComplete` 候选、`Mentions` 候选、`TreeSelect`（树展开时）、`ComboBox`。
全仓 `VirtualizingStackPanel` 仅 `CascaderViewLevelListTheme.axaml` 出现。

### 根因

- 大候选数据集（1000+）场景下，这些候选列表为每个 item 物化整套模板 + Transition + 状态 Style；加上前述 A-1 / A-4 / CAS-D1 问题，单次打开就可能分配数万对象。

### 解决方案

1. 候选 ListBox 的 `ItemsPanel` 模板改为 `<VirtualizingStackPanel Orientation="Vertical"/>`。
2. 确保候选 ItemContainer 支持 Recycle（`ItemsControl` 默认虚拟化友好，但要避免 `ContentControl` 包裹里的 `Transitions = null` 反模式）。
3. TreeSelect 参考基线 T-4/T-5 修复后的方案。

### 预估收益

- 1000 项候选：打开从 ~200ms 降到 <30ms；内存占用降一个数量级。

---

## 无重大独立问题的控件

| 控件 | 说明 |
|---|---|
| `NumericUpDown` | 组合 `ButtonSpinner` + `LineEdit`，性能瓶颈全部落在基线文档的 `AddOnDecoratedBox` 问题上 |
| `DatePicker` / `TimePicker` | 结构为 `AddOnDecoratedBox` + `Calendar` Flyout；`Calendar` 自身见 `05-Display-And-Layout.md` |
| `ComboBox` | 基于 `AddOnDecoratedBox` 与 `CandidateList`；受 AOB-*、CAN-E1、SEL-J1 影响 |
| `AutoComplete` | 同上 + AOB-E1 |
| `Mentions` | 同上 + AOB-E1 |
| `TreeSelect` | 基于 `AddOnDecoratedBox` + `TreeView`；受基线 T-* 全套影响 |



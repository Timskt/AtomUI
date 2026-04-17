# 03 · 数据容器 族控件性能分析

> **覆盖控件**：`ListBox` · `ListView` · `TreeView` · `Transfer` · `Menu` · `NavMenu` · `TabControl` · `Collapse` · `Expander` · `Pagination`
> **前置文档**：[《AddOnDecoratedBox 与 TreeView 性能深度分析报告》](../AddOnDecoratedBox_TreeView_Performance.md)

`TreeView` / `TreeViewItem` 的 T-1 ~ T-7 详见基线文档，本文不重复。本文重点：

1. 将 `TreeView` T-1 的同构问题扩展到 **Menu / MenuItem / NavMenu / Cascader** ——这是跨控件的**系统性**问题。
2. `TabControl` / `ListView` / `NavMenu` 各自的独立问题。

---

## 问题严重度总览

| # | 问题 | 严重度 | 影响面 |
|---|---|---|---|
| **M-1** | `MenuItem.ITreeNode<IMenuItemData>.Children => Items.OfType<IMenuItemData>().ToList()` —— TreeView T-1 的同构翻版 | 🔴 极严重 | Menu / NavMenu / Cascader 级联访问 |
| **NAV-E1** | `NavMenu.TraverNavMenuPathAsync` 双方法各一次 `Items.ToList()` + 循环内再次访问 `navMenuItem.Items` | 🟠 高 | 路由恢复、initial selection |
| **NAV-D1** | `BaseNavMenuItemHeader` OnLoaded 新建 Transitions、OnUnloaded 置空，每个可见 item 都经历 | 🟠 高 | 大菜单 |
| **TAB-E1** | `TabControlScrollViewer.MenuFlyout.Items.ToList()` | 🟠 中 | 溢出 Tab 菜单 |
| **LVI-D1** | `ListViewItem` OnLoaded/OnUnloaded 反复新建/清空 Transitions | 🟡 中 | 大 ListView |
| **LVW-H1** | `ListView.cs:451` 再次 `InvalidateMeasure()` 触发，需检查上游链路是否有重复 | 🟡 低 | 集合变动 |

---

## 1. M-1：`MenuItem.ITreeNode.Children` 同构 T-1 的致命问题

**文件**：`src/AtomUI.Desktop.Controls/Menu/MenuItem.cs:53`

```csharp
IList<IMenuItemData> ITreeNode<IMenuItemData>.Children => Items.OfType<IMenuItemData>().ToList();
```

### 根因

- 与基线 T-1 **完全同构**：每次访问属性都做 `OfType` + `ToList`。
- `Menu`、`NavMenu`、`Cascader` 在键盘导航、路径匹配、级联过滤、打开/关闭传播时**反复访问这个 `Children`**。
- 对一个 500 节点的菜单，级联过滤一次可能触发 O(N²) 次 `ToList`，放大系数更甚于 TreeView（因为菜单交互频率比树高）。

### 解决方案

1. **零分配懒枚举**（推荐）：
   ```csharp
   IList<IMenuItemData> ITreeNode<IMenuItemData>.Children
   {
       get
       {
           // 惰性包装器，避免每次分配 List
           return new MenuChildrenView(Items);
       }
   }
   ```
   实现一个 `MenuChildrenView : IList<IMenuItemData>`，内部持有 `ItemCollection` 引用，在 indexer / foreach 时按需转换。
2. **字段缓存** + `Items.CollectionChanged` 维护：添加 `private List<IMenuItemData>? _cachedChildren;` 在集合变化时置 `null`，首次访问时重建。
3. **接口签名改造**：若 `ITreeNode<T>.Children` 的调用方全部只做 `foreach`，把签名改为 `IEnumerable<T>` —— `Items.OfType<IMenuItemData>()` 零分配（除迭代器）。

### 预估收益

- 深层菜单路径匹配从 O(depth × fanout × ToList) 降为 O(depth × fanout)；
- 500 节点菜单首次打开 GC 分配降约 **10x**。

---

## 2. NAV-E1：`NavMenu.TraverNavMenuPathAsync` 路径遍历

**文件**：`src/AtomUI.Desktop.Controls/NavMenu/NavMenu.cs:476,531`

```csharp
476: IList items = Items.ToList();   // 方法 A
...
498:     items = navMenuItem.Items;   // 内层再次赋值（但未 ToList — 一致性问题）
...
531: IList items = Items.ToList();   // 方法 B 同样
```

### 根因

- 入口 `.ToList()` 把顶层菜单复制一份到 `IList`；循环内层改成直接引用 `navMenuItem.Items`（没有 ToList）——**行为不一致**且入口拷贝毫无必要。
- 该函数在路由恢复、初始选中传播时执行，最坏情况每次路由变化触发。

### 解决方案

- 直接使用 `Items`（它本身是 `ItemCollection`，支持索引访问），删除 `.ToList()`：
  ```csharp
  var items = (IReadOnlyList<object>)Items;  // 或 ItemCollection 本身可迭代
  ```
- 统一方法 A/B 的声明类型，避免将 `Items` 和 `navMenuItem.Items` 在一个变量中混用带来的阅读成本。

### 预估收益

- 每次路径查找少 1 次顶层复制；对 1000 项大菜单节省明显。

---

## 3. NAV-D1：NavMenuItemHeader Transitions 生命周期

**文件**：`src/AtomUI.Desktop.Controls/NavMenu/Header/BaseNavMenuItemHeader.cs:148-175`

```csharp
148:        var transitions = new Transitions() { ... };  // OnLoaded
159:        Transitions = null;
...
175:        Transitions = null;  // OnUnloaded
```

### 根因

- 横向 NavMenu 滚动/动态子菜单开合时反复 Load / Unload。
- 每个可见 `NavMenuItemHeader` 实例都构造 2+ 个 `ITransition`。

### 解决方案

- 与 BTN-D1 同思路：静态缓存 Transitions 模板；或在 AXAML 的 `ControlTheme` 中用 `<Setter Property="Transitions">` 绑定到共享资源。

### 预估收益

- NavMenu 50 个 item 滚入屏幕节省 50 × (Transitions + 2 ITransition) = 150+ 对象。

---

## 4. TAB-E1：`TabControlScrollViewer` 溢出菜单复制

**文件**：`src/AtomUI.Desktop.Controls/TabControl/TabControlScrollViewer.cs:67`

```csharp
var oldItems = MenuFlyout.Items.ToList();
```

### 根因

- 每次打开溢出 Tab 菜单都把 `MenuFlyout.Items` 复制；如果只是为了 `Clear`+重建，可以直接用 `for (var i = Items.Count - 1; i >= 0; i--) Items.RemoveAt(i);` 或直接 `Items.Clear()` 再添加新项。

### 解决方案

- 分析是否真需要"旧项的快照"；若只是清空→直接 `Clear`；若是 diff，用复用 `List` 字段。

### 预估收益

- 10+ Tab 场景下每次打开溢出菜单节省一次数组分配。

---

## 5. LVI-D1：`ListViewItem` Transitions 抖动

**文件**：`src/AtomUI.Desktop.Controls/ListView/ListViewItem.cs:169,211`

```csharp
169:     Transitions = null;   // OnUnloaded
211:     Transitions = null;
```

### 根因 / 方案

- 同 BTN-D1。在 ListView 虚拟化时影响最大。
- 建议使用控件级静态 `Transitions` 模板复用。

---

## 6. LVW-H1：`ListView.cs:451` InvalidateMeasure

**文件**：`src/AtomUI.Desktop.Controls/ListView/ListView.cs:451`

单次调用，未见级联；仅作记录，确保上层不会在同一帧多次触发。

---

## 7. Transfer

`Transfer` 由两个 `TreeView` 组合而成，全部基线 T-* 问题生效。独立问题未发现。

---

## 无重大独立问题的控件

| 控件 | 说明 |
|---|---|
| `ListBox` | Avalonia 原生 ListBox 扩展，虚拟化默认开启；未发现独立热点 |
| `Collapse` / `Expander` | 模板化 + Motion；动画属于一次性，不在热路径 |
| `Pagination` | 固定数量页按钮，无大数据场景 |
| `TabControl` 其他部分 | 除 TAB-E1 外未见独立问题；但若使用 Card 型 Tab 需检查 Tab 项的 Transitions 生命周期 |



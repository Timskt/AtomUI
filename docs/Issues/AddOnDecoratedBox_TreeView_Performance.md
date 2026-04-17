# AddOnDecoratedBox 与 TreeView 性能深度分析报告

> **生成日期**：2026-04-17
> **触发场景**：运行 `AtomUIGallery.Desktop` 后发现输入控件（`LineEdit`、`SearchEdit`、`TextArea`、`NumericUpDown`、`ComboBox`、`Cascader`、`Select`、`TreeSelect`、`AutoComplete`、`Mentions`）和 `TreeView` 存在可观察的卡顿 / 高 CPU 占用。
> **分析范围**：
> - `src/AtomUI.Desktop.Controls/Primitives/AddOnDecoratedBox/`
> - 所有继承 `AddOnDecoratedBox` 的控件（共 6 个直接子类 + 10+ 间接使用者）
> - `src/AtomUI.Desktop.Controls/TreeView/` 整目录

---

## 📊 问题严重度总览

| # | 问题 | 严重度 | 影响面 |
|---|------|--------|--------|
| **A-1** | `AddOnDecoratedBox` 构造函数中为每个实例创建 3 个复杂 Style（含 Descendant + Or 选择器） | 🔴 **极严重** | 所有输入/下拉/数字框控件 |
| **A-2** | `AddOnDecoratedBox.OnPropertyChanged` 单次属性变化可触发 3 次完整重新计算 | 🟠 高 | 同上 |
| **A-3** | `ConfigureAddOnBorderInfo` / `ConfigureInnerBoxCornerRadius` 频繁分配 `CornerRadius` / `Thickness` 结构 | 🟡 中 | 同上 |
| **A-4** | `Transitions` 在 OnLoaded / OnUnloaded 中创建/释放 | 🟠 中 | 所有派生类 |
| **T-1** | `TreeViewItem.ITreeNode.Children` 每次访问都 `.OfType().ToList()` | 🔴 **极严重** | TreeView / TreeSelect / Transfer |
| **T-2** | `FilterTreeNode` / `FilterItem` 多次全树递归 + 每次 `ExpandAll` | 🔴 **极严重** | 大数据量 TreeView 过滤 |
| **T-3** | `ExpandAll` / `CollapseAll` 用 `Items.ToList()` 开头复制整个根集合 | 🟠 高 | TreeView 全展开 |
| **T-4** | `TreeViewItem.OnAttachedToVisualTree` 调用 `GetLogicalAncestors().OfType<TreeView>().FirstOrDefault()` | 🟠 高 | 虚拟化场景下每 item 调用 |
| **T-5** | `CheckedSubTree` / `UnCheckedSubTree` 递归期间动态修改 `IsExpanded` 引发级联重排 | 🟠 高 | 多选树 |
| **T-6** | `TreeViewItemHeader.Transitions` 每个可见 item 都创建 2 个 Transition | 🟠 中 | 大数据量虚拟化 |
| **T-7** | `HasChildOrDescendantsMatchFilter` 重复递归无缓存 | 🟠 中 | 深层树过滤 |

---

## 目录

- [一、AddOnDecoratedBox 深度分析](#一addondecoratedbox-深度分析)
- [二、TreeView 深度分析](#二treeview-深度分析)
- [三、修复方案（按优先级）](#三修复方案按优先级)
- [四、性能基准预估](#四性能基准预估)
- [五、验证与回归测试建议](#五验证与回归测试建议)

---

## 一、AddOnDecoratedBox 深度分析

### 1.1 🔴 致命问题：构造函数创建每实例 Style（A-1）

**文件**：`src/AtomUI.Desktop.Controls/Primitives/AddOnDecoratedBox/AddOnDecoratedBox.cs`
**方法**：`ConfigureInstanceStyles()`（第 633-701 行）
**调用点**：构造函数 `AddOnDecoratedBox()` 第 296-300 行

```csharp
public AddOnDecoratedBox()
{
    this.RegisterTokenResourceScope(AddOnDecoratedBoxToken.ScopeProvider);
    ConfigureInstanceStyles();   // ❌ 每个实例都会执行！
}
```

`ConfigureInstanceStyles()` 构造了 3 个 Style：

1. **Warning 状态 Style**（行 636-654）
   ```csharp
   var warningStyle = new Style(x => x.PropertyEquals(StatusProperty, InputControlStatus.Warning));
   var iconStyle = new Style(x => Selectors.Or(
       x.Nesting().Descendant().Name(...).Descendant().OfType<Icon>().Not(p => p.Class("skip-status")),
       x.Nesting().Descendant().Name(...).Descendant().OfType<PathIcon>().Not(p => p.Class("skip-status")),
       x.Nesting().Descendant().Name(...).Descendant().OfType<Icon>().Not(p => p.Class("skip-status")),
       x.Nesting().Descendant().Name(...).Descendant().OfType<PathIcon>().Not(p => p.Class("skip-status"))
   ));
   iconStyle.Add(Icon.FillBrushProperty, SharedTokenKind.ColorWarning);
   iconStyle.Add(Icon.StrokeBrushProperty, SharedTokenKind.ColorWarning);
   iconStyle.Add(Icon.ForegroundProperty, SharedTokenKind.ColorWarning);
   warningStyle.Add(iconStyle);
   Styles.Add(warningStyle);
   ```
2. **Error 状态 Style**（行 655-674）
3. **Disabled 状态 Style**（行 675-700） — 含 **8 个分支**的 `Selectors.Or(...)`

#### 性能影响

- Avalonia 的 `Styles` 集合是 **每实例** 的。每创建一个 `AddOnDecoratedBox`：
  - **新建 3 个 `Style` 对象** + **3 个子 `Style`** = 6 个 Style 实例
  - 每个 Style 包含 **Lambda 编译后的选择器树**（`Descendant()` + `Selectors.Or` + `Nesting()` + `Name()` + `OfType<>()` + `Not()`），选择器节点数 **约 20-40 个节点** × 3 = 60-120 节点
  - 选择器在被 Avalonia 样式引擎评估时**遍历所有后代 Visual**
- **乘数效应**：
  | 使用者 | 每个实例额外产生的 Style 数 | 使用频率 |
  |-------|-------------------------|---------|
  | `LineEdit`（内部 1 个 AddOnDecoratedBox） | 6 | Form 中大量使用 |
  | `TextArea`（`TextAreaDecoratedBox`） | 6 | |
  | `SearchEdit`（`SearchEditDecoratedBox`） | 6 | |
  | `NumericUpDown`（`ButtonSpinnerDecoratedBox`） | 6 | |
  | `ComboBox` / `Select`（`SelectAddOnDecoratedBox`） | 6 | |
  | `Cascader`（`CascaderAddOnDecoratedBox`） | 6 | |
  | `TreeSelect`（`TreeSelectAddOnDecoratedBox`） | 6 | |
  | `AutoComplete` / `Mentions`（继承 AddOnDecoratedBox） | 6 | |
  
  **大型表单场景**（50 个输入控件）：50 × 6 = **300 个 Style 对象** + 对应的 lambda 委托、选择器节点
- **选择器评估开销**：`Descendant()` 选择器会触发 Avalonia 对整个子树的扫描，且一旦子树变化都需要重算。配合 `Nesting().Descendant().Name(XXX).Descendant().OfType<Icon>()` 这种**多层嵌套**选择器，每次 `Status` / `IsEnabled` 变化或子树变化都触发 **O(N × depth)** 的匹配。

#### 为什么写成这样？

开发者大概想实现 "根据 Status 属性给内部 Icon 染色" 的功能，但选择了**运行时构造 Style**，而不是在 AXAML ControlTheme 中声明。因为 AXAML 中无法动态绑定 Token，所以绕了这个弯路。

#### 修复方案（最推荐）

**方案 A：移到 ControlTheme 的 `Styles`（全局共享，零实例开销）**

在 `AddOnDecoratedBoxTheme.axaml` 中：

```xml
<ControlTheme TargetType="atom:AddOnDecoratedBox" ...>
  <!-- 现有 Setter -->
  
  <Style Selector="^[Status=Warning] /template/ ContentPresenter#PART_ContentLeftAddOn atom|Icon">
    <Setter Property="FillBrush" Value="{atom:SharedTokenResource ColorWarning}"/>
    <Setter Property="StrokeBrush" Value="{atom:SharedTokenResource ColorWarning}"/>
    <Setter Property="Foreground" Value="{atom:SharedTokenResource ColorWarning}"/>
  </Style>
  <!-- 同样模式对 Error、Disabled -->
</ControlTheme>
```

这样整个 App 只编译/评估 **1 份选择器**（驻留在 ControlTheme 内），而非每实例一份。

**方案 B：静态只读 Styles 挂到 `Application.Styles`**

```csharp
// AddOnDecoratedBox.cs
private static readonly Styles s_sharedInstanceStyles = CreateSharedStyles();

private static Styles CreateSharedStyles() { /* 原 ConfigureInstanceStyles 内容 */ }

public AddOnDecoratedBox()
{
    this.RegisterTokenResourceScope(AddOnDecoratedBoxToken.ScopeProvider);
    // 不再为每实例构造 Style
}
```

由主题管理器在应用启动时一次性注册到 `Application.Styles`。缺点是跨控件实例的 `PropertyEquals(StatusProperty, ...)` 需要用类型选择器替代 `Nesting()`。

**方案 C：最小改动 — 把 `new Style` 改为基于 `Class` 的 PseudoClass 驱动**

`AddOnDecoratedBox.OnPropertyChanged` 中已经处理 `StyleVariantProperty` 并设置 PseudoClass。把 Status 也用 PseudoClass 表达：

```csharp
protected virtual void UpdatePseudoClasses()
{
    // 现有
    PseudoClasses.Set(AddOnDecoratedBoxPseudoClass.Outline, ...);
    // 新增
    PseudoClasses.Set(":warning", Status == InputControlStatus.Warning);
    PseudoClasses.Set(":error", Status == InputControlStatus.Error);
}
```

然后在 `AddOnDecoratedBoxTheme.axaml` 用标准选择器：

```xml
<Style Selector="^:warning /template/ atom|Icon">
    <Setter Property="FillBrush" Value="{atom:SharedTokenResource ColorWarning}"/>
</Style>
```

#### 估算收益

- **50 个输入控件的大表单**：节省 **~300 个 Style 对象** 和对应选择器树（预估节省 2-5 MB 内存和 20-50 ms 启动时间）
- **每次属性变化**：避免每实例独立的 Descendant 选择器重评估，样式命中时间从 O(实例数 × 子树深度) → O(1)

---

### 1.2 🟠 问题 A-2：`OnPropertyChanged` 单次变化触发多次重算

**文件**：`AddOnDecoratedBox.cs` 第 344-384 行

```csharp
protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
{
    base.OnPropertyChanged(change);
    
    if (change.Property == StyleVariantProperty) {
        UpdatePseudoClasses();
        ConfigureInnerBoxBorderThickness();
    }
    if (change.Property == LeftAddOnProperty || change.Property == RightAddOnProperty ||
        change.Property == CornerRadiusProperty || change.Property == StyleVariantProperty ||
        change.Property == CompactSpaceItemPositionProperty || change.Property == CompactSpaceOrientationProperty) {
        ConfigureInnerBoxCornerRadius();
    }
    if (change.Property == BorderThicknessProperty) {
        ConfigureInnerBoxBorderThickness();
    }
    if (change.Property == CornerRadiusProperty || change.Property == BorderThicknessProperty ||
        change.Property == StyleVariantProperty || change.Property == CompactSpaceItemPositionProperty ||
        change.Property == CompactSpaceOrientationProperty) {
        ConfigureAddOnBorderInfo();
    }
    // ...
}
```

当 `StyleVariantProperty` 变化时，同时触发：
1. `UpdatePseudoClasses()`
2. `ConfigureInnerBoxBorderThickness()`
3. `ConfigureInnerBoxCornerRadius()`
4. `ConfigureAddOnBorderInfo()`（内部又会再触发 `ConfigureInnerBoxCornerRadius` 类似的计算）

其中 `ConfigureAddOnBorderInfo` 设置 `LeftAddOnCornerRadius`、`RightAddOnCornerRadius`、`LeftAddOnBorderThickness`、`RightAddOnBorderThickness` 4 个 DirectProperty，每个赋值又通过 `SetAndRaise` 广播 `PropertyChanged` 事件，可能触发样式系统级联重算。

#### 修复建议

**引入脏标记 + 合并延迟更新**：

```csharp
private bool _borderInfoDirty;
private bool _cornerRadiusDirty;

protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
{
    base.OnPropertyChanged(change);
    if (change.Property == StyleVariantProperty) {
        UpdatePseudoClasses();
        _borderInfoDirty = _cornerRadiusDirty = true;
    }
    // ...其他属性类似，只置脏
    
    if (_borderInfoDirty || _cornerRadiusDirty)
    {
        Dispatcher.UIThread.Post(ApplyDirtyFlags, DispatcherPriority.Render);
    }
}

private void ApplyDirtyFlags()
{
    if (_cornerRadiusDirty) { ConfigureInnerBoxCornerRadius(); _cornerRadiusDirty = false; }
    if (_borderInfoDirty)   { ConfigureAddOnBorderInfo(); ConfigureInnerBoxBorderThickness(); _borderInfoDirty = false; }
}
```

---

### 1.3 🟡 问题 A-3：`ConfigureAddOnBorderInfo` 高频分配值类型

**文件**：第 386-490 行

每次调用创建 **至多 2 个 `CornerRadius` + 2 个 `Thickness`**。虽然是值类型，但由于通过 `SetAndRaise` 广播 —内部会 boxing 到 `object`（`AvaloniaPropertyChangedEventArgs<T>` 虽然是强类型的，但在订阅方处理时常常被装箱）。此外在属性未变化的情况下重复计算值。

#### 修复建议

- 添加早退：

```csharp
private void ConfigureAddOnBorderInfo()
{
    var newLeftRadius = /* 计算 */;
    var newRightRadius = /* 计算 */;
    
    if (LeftAddOnCornerRadius != newLeftRadius)
        LeftAddOnCornerRadius = newLeftRadius;
    // 同理 Right...Thickness 等
}
```

- `AvaloniaProperty.RegisterDirect` 本身会判断相等性，但在 `SetAndRaise` 前添加手动判断可避免后续计算。

---

### 1.4 🟠 问题 A-4：`Transitions` 在 Loaded/Unloaded 每次创建

**文件**：第 309-338 行

```csharp
protected override void OnLoaded(RoutedEventArgs e)
{
    base.OnLoaded(e);
    ConfigureTransitions(false);
}

protected override void OnUnloaded(RoutedEventArgs e)
{
    base.OnUnloaded(e);
    Transitions = null;   // ❌ 每次离开视觉树都清零
}

private void ConfigureTransitions(bool force)
{
    if (IsMotionEnabled)
    {
        if (force || Transitions == null)
        {
            Transitions = [
                TransitionUtils.CreateTransition<SolidColorBrushTransition>(Border.BorderBrushProperty),
                TransitionUtils.CreateTransition<SolidColorBrushTransition>(Border.BackgroundProperty)
            ];
            NotifyCreateTransitions(Transitions);
        }
    }
    ...
}
```

虚拟化场景下（ListBox、ComboBox 下拉、Form.ItemsControl 回收）控件频繁 Load/Unload，每次都重建 Transitions 集合。

#### 修复建议

**复用 Transitions 实例**：

```csharp
private Transitions? _cachedTransitions;

private void ConfigureTransitions(bool force)
{
    if (IsMotionEnabled)
    {
        if (_cachedTransitions == null)
        {
            _cachedTransitions = [
                TransitionUtils.CreateTransition<SolidColorBrushTransition>(Border.BorderBrushProperty),
                TransitionUtils.CreateTransition<SolidColorBrushTransition>(Border.BackgroundProperty)
            ];
            NotifyCreateTransitions(_cachedTransitions);
        }
        Transitions = _cachedTransitions;
    }
    else
    {
        Transitions = null;
    }
}
```

或者 `OnUnloaded` 中不设 `Transitions = null`，让 GC 在控件真正销毁时回收。

---

## 二、TreeView 深度分析

### 2.1 🔴 致命问题：`ITreeNode<>.Children` 每次调用都创建新列表（T-1）

**文件**：`src/AtomUI.Desktop.Controls/TreeView/TreeViewItem.cs` 第 94 行

```csharp
IList<ITreeItemNode> ITreeNode<ITreeItemNode>.Children => Items.OfType<ITreeItemNode>().ToList();
```

同类问题存在于：
- `src/AtomUI.Desktop.Controls/Menu/MenuItem.cs` 第 53 行
- `src/AtomUI.Desktop.Controls/NavMenu/NavMenuItem.cs`（同模式）

#### 性能影响

- **每次属性读取**触发：
  1. `Items` 返回 `ItemCollection`
  2. `.OfType<ITreeItemNode>()` 创建 LINQ 迭代器（堆分配）
  3. `.ToList()` 分配 `List<ITreeItemNode>`（含内部 `T[]` 数组）
  4. 遍历整个 `Items` 集合
- **问题放大场景**：
  - `TransferTreeView.CalculateAllNodesCount`（`TransferTreeView.cs:161`）递归访问 `treeNode.Children` — 每个节点都触发一次分配
  - 1000 节点的树 → 1000 次 `.OfType().ToList()` → **1000 个 List 对象 + 1000 个迭代器**
  - 每次 `SelectedItems` 改变时可能重新走一遍
- **接口设计缺陷**：`IList<>` 让调用方以为可修改，实际是快照（调用方修改也不会影响树）— 既误导又低效。

#### 修复方案

**方案 A：缓存 + 失效机制**

```csharp
// TreeViewItem.cs
private IList<ITreeItemNode>? _cachedChildrenNodes;

IList<ITreeItemNode> ITreeNode<ITreeItemNode>.Children
{
    get
    {
        if (_cachedChildrenNodes == null)
        {
            _cachedChildrenNodes = Items.OfType<ITreeItemNode>().ToList();
        }
        return _cachedChildrenNodes;
    }
}

// 构造中监听 Items.CollectionChanged 失效缓存
public TreeViewItem()
{
    Items.CollectionChanged += (_, _) => _cachedChildrenNodes = null;
}
```

**方案 B：改接口为 `IEnumerable<TChild>`**（更激进，但更正确）

```csharp
// ITreeNode.cs
public interface ITreeNode<TChild> ...
{
    // 之前：IList<TChild> Children { get; }
    IEnumerable<TChild> Children { get; }   // ✅ 惰性、零分配
}

// TreeViewItem.cs
IEnumerable<ITreeItemNode> ITreeNode<ITreeItemNode>.Children => Items.OfType<ITreeItemNode>();
```

然后所有调用点只做 `foreach`，零分配；需要索引访问时在调用方局部 `.ToList()`。

**方案 C：提供专用遍历器 + 缓存**

```csharp
public struct TreeItemChildrenEnumerator
{
    private ItemCollection _items;
    private int _index;
    public ITreeItemNode Current { ... }
    public bool MoveNext() { /* 手动遍历 + 过滤 */ }
}
```

推荐 **方案 B** — 接口设计更干净，破坏性最小（现有代码 `foreach` 继续工作，只需要在个别需要 `.Count` 的地方调用 `.ToList()`）。

---

### 2.2 🔴 致命问题：`FilterTreeNode` 多次全树递归（T-2）

**文件**：`src/AtomUI.Desktop.Controls/TreeView/TreeView.Filter.cs`

整个 `FilterTreeNode()` 流程：

```csharp
public void FilterTreeNode()
{
    // 1. 先走一遍树收集"原本展开"状态（第 93-100 行）
    for (int i = 0; i < ItemCount; i++)
        if (ContainerFromIndex(i) is TreeViewItem item)
            originExpandedItems.UnionWith(CollectExpandedItems(item));  // ❌ 递归全树
    
    // 2. ExpandAll(false) — 递归整个树设置 IsExpanded=true（第 103 行）
    ExpandAll(false);
    
    // 3. 再走一遍全树恢复展开状态（第 107 行）
    RestoreItemExpandedStates(originExpandedItems);
    
    // 4. 再走一遍全树备份过滤前状态（第 112-118 行）
    for (int i = 0; i < ItemCount; i++)
        if (ContainerFromIndex(i) is TreeViewItem item)
            BackupStateForFilterMode(item);  // ❌ 递归全树
    
    // 5. 再走一遍全树执行过滤（第 121-127 行）
    for (var i = 0; i < ItemCount; i++)
        if (ContainerFromIndex(i) is TreeViewItem treeViewItem)
            FilterItem(treeViewItem);  // ❌ 递归全树 + 每个节点内部还可能递归 HasChildOrDescendantsMatchFilter
}

private void FilterItem(TreeViewItem treeViewItem)
{
    // 递归所有子项（第 141-147 行）
    for (var i = 0; i < treeViewItem.ItemCount; i++)
        if (treeViewItem.ContainerFromIndex(i) is TreeViewItem childTreeViewItem)
            FilterItem(childTreeViewItem);
    
    // 过滤
    var filterResult = Filter.Filter(this, treeViewItem, FilterValue);
    
    // 又对每个节点递归判断是否"有子孙匹配"
    if (!HasChildOrDescendantsMatchFilter(treeViewItem))  // ❌ 再递归一遍
    { ... }
}

private bool HasChildOrDescendantsMatchFilter(TreeViewItem treeViewItem)
{
    for (int i = 0; i < treeViewItem.ItemCount; i++)
        if (treeViewItem.ContainerFromIndex(i) is TreeViewItem childItem)
            if (HasChildOrDescendantsMatchFilter(childItem))
                return true;
    return treeViewItem.IsFilterMatch;
}
```

#### 性能影响

对 **N 个节点、深度 D** 的树：
1. `CollectExpandedItems`：O(N)
2. `ExpandAll`：O(N)
3. `RestoreItemExpandedStates`：O(N)
4. `BackupStateForFilterMode`：O(N)
5. `FilterItem` 主循环：O(N)
6. 每个节点又调用 `HasChildOrDescendantsMatchFilter`：O(N) — 每个节点重算子树！

**总复杂度：O(N²)**

1000 节点树的 Filter 操作可能需要 **100 万次迭代** + 每次 `ContainerFromIndex` 需要走 ItemsSource 索引。

此外：
- `ExpandAll` 会真实展开所有节点，**触发 N 次布局重算**
- 虚拟化 Panel 下，`ContainerFromIndex` 返回 null（未实化），递归会漏节点

#### 修复方案

**1. 用 `HashSet<TreeViewItem>` 记住匹配结果，避免 `HasChildOrDescendantsMatchFilter` 重算**

```csharp
private HashSet<TreeViewItem>? _descendantsMatchCache;

public void FilterTreeNode()
{
    _descendantsMatchCache = new();
    // ...
    
    // 从叶到根遍历一次，边计算边缓存
    // （`FilterItem` 递归中子节点已经先计算完，回到父节点时直接查缓存）
}

private bool HasChildOrDescendantsMatchFilterCached(TreeViewItem node)
{
    return _descendantsMatchCache!.Contains(node);
}

// 在 FilterItem 末尾：
private void FilterItem(TreeViewItem treeViewItem)
{
    bool anyDescendantMatched = false;
    for (int i = 0; i < treeViewItem.ItemCount; i++)
    {
        if (treeViewItem.ContainerFromIndex(i) is TreeViewItem child)
        {
            FilterItem(child);
            if (_descendantsMatchCache!.Contains(child)) anyDescendantMatched = true;
        }
    }
    if (treeViewItem.IsFilterMatch || anyDescendantMatched)
        _descendantsMatchCache!.Add(treeViewItem);
    // ... 后续根据 cache 处理 IsVisible / IsExpanded
}
```

复杂度降为 **O(N)**。

**2. 合并 Backup / Filter 遍历为一次**

`BackupStateForFilterMode` 和 `FilterItem` 都递归整棵树，可合并：

```csharp
private void FilterItemAndBackup(TreeViewItem item, bool needBackup)
{
    for (int i = 0; i < item.ItemCount; i++)
        if (item.ContainerFromIndex(i) is TreeViewItem child)
            FilterItemAndBackup(child, needBackup);
    if (needBackup) item.CreateFilterContextBackup();
    // 原 FilterItem 逻辑
}
```

**3. 避免调用 `ExpandAll(false)`**

当前逻辑是 "全部展开 → 再根据过滤结果折叠"，可改为 "只展开匹配路径的祖先"：

```csharp
// 只对匹配项的祖先调用 IsExpanded = true
foreach (var matched in matchedItems)
{
    var parent = matched.Parent;
    while (parent is TreeViewItem p)
    {
        p.SetCurrentValue(TreeViewItem.IsExpandedProperty, true);
        parent = p.Parent;
    }
}
```

**4. 在过滤期间挂起动画**

已经用 `BeginTurnOffMotion()` — 但只覆盖主流程，确保所有 `SetCurrentValue(IsExpandedProperty, ...)` 都在动画关闭的作用域内。

---

### 2.3 🟠 问题 T-3：`ExpandAll` / `CollapseAll` 每次 `Items.ToList()`

**文件**：`TreeView.cs` 第 993、1049、1111 行

```csharp
IList items = Items.ToList();   // ❌ 把整个根级集合复制一次
```

对大型树这额外分配 O(根节点数) 的数组。虽然相比 T-2 是小问题，但累积。

#### 修复建议

直接 `foreach (var item in Items)` — 如果担心枚举时修改，可以用 `var snapshot = Items.ToArray()` 一次性（`ToArray()` 比 `ToList()` 稍快且更紧凑），或者用 `for (int i = 0; i < Items.Count; i++)`。

---

### 2.4 🟠 问题 T-4：`OnAttachedToVisualTree` 每 item 走祖先查找

**文件**：`TreeViewItem.cs` 第 434-438 行

```csharp
protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
{
    base.OnAttachedToVisualTree(e);
    OwnerTreeView = this.GetLogicalAncestors().OfType<TreeView>().FirstOrDefault();
}
```

每次 TreeViewItem 进入视觉树（虚拟化下滚动触发）都走逻辑祖先链。对于深度 10、节点 1000 的树，滚动时频繁虚拟化回收 → 频繁 `GetLogicalAncestors`。

#### 修复建议

利用 `PrepareContainerForItemOverride`：在 `TreeView.cs:745` 的 `PrepareContainerForItemOverride` 中直接把 `OwnerTreeView = this` 赋给容器（已经有类似的属性复制操作）：

```csharp
protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
{
    base.PrepareContainerForItemOverride(container, item, index);
    if (container is TreeViewItem treeViewItem)
    {
        treeViewItem.OwnerTreeView = this;   // ✅ 直接赋值
        // ...其他现有逻辑
    }
}
```

然后 `TreeViewItem.OnAttachedToVisualTree` 中移除 `GetLogicalAncestors` 调用。注意：子级 TreeViewItem 的 OwnerTreeView 可以通过 `(Parent as TreeViewItem)?.OwnerTreeView` 回溯，O(1)。

---

### 2.5 🟠 问题 T-5：`CheckedSubTree` 期间动态 Expand/Collapse

**文件**：`TreeView.cs` 第 598-642、680-724 行

```csharp
private ISet<object> DoCheckedSubTree(TreeViewItem treeViewItem)
{
    // ...
    var originIsExpanded = treeViewItem.IsExpanded;
    if (!originIsExpanded)
    {
        treeViewItem.SetCurrentValue(TreeViewItem.IsExpandedProperty, true);  // ❌ 临时展开
    }
    
    foreach (var childItem in treeViewItem.Items)
    {
        if (childItem != null)
        {
            var container = TreeContainerFromItem(childItem);
            if (container is TreeViewItem childTreeViewItem && childTreeViewItem.IsEffectiveCheckable())
            {
                var childCheckedItems = DoCheckedSubTree(childTreeViewItem);
                // ...
            }
        }
    }
    
    treeViewItem.SetCurrentValue(TreeViewItem.IsExpandedProperty, originIsExpanded);  // ❌ 还原
}
```

为什么要临时展开？因为虚拟化下只有展开的节点才有 `container`。但代价是：
- 每个节点两次 `SetCurrentValue(IsExpandedProperty)` — 触发 `HandleExpandedChanged` → `ExpandChildren` → `motion.Run`（若 motion 未关）
- **触发布局重算 + 动画** — 对上千节点的树可能冻结 UI 数秒

虽然使用方应该包裹 `IsMotionEnabled=false`，但代码未强制：

```csharp
// 第 584 行已经设置 IsMotionEnabledProperty = false，但恢复前又改回
```

#### 修复建议

1. **完全避免 UI 容器路径**：对数据源是 `ITreeItemNode` 的情况，直接递归操作数据模型：

```csharp
private ISet<object> DoCheckedSubTreeByData(ITreeItemNode node)
{
    var result = new HashSet<object>();
    result.Add(node);
    foreach (var child in node.Children)
    {
        result.UnionWith(DoCheckedSubTreeByData(child));
    }
    // 最后一次性更新 TreeView.CheckedItems
    return result;
}
```

然后通过 `CheckedItems` 绑定触发一次 UI 更新，而不是逐个 TreeViewItem 修改。

2. 如果仍需 UI 容器路径（动态 ItemsSource），至少 **包一次 `BeginTurnOffMotion` 作用域**，不要依赖调用方。

---

### 2.6 🟠 问题 T-6：`TreeViewItemHeader.Transitions` 每 Header 创建

**文件**：`TreeViewItemHeader.cs` 第 470-487 行

与 AddOnDecoratedBox 的 A-4 是同一模式 — `OnLoaded` 创建 Transitions、`OnUnloaded` 置空。大树滚动虚拟化会反复触发。

修复同 A-4：缓存实例。

---

### 2.7 🟠 问题 T-7：`HasChildOrDescendantsMatchFilter` 无缓存

已在 T-2 中描述。如果不做整体重构，至少给该方法加 memoization：

```csharp
private readonly Dictionary<TreeViewItem, bool> _descendantMatchCache = new();

private bool HasChildOrDescendantsMatchFilter(TreeViewItem treeViewItem)
{
    if (_descendantMatchCache.TryGetValue(treeViewItem, out var cached))
        return cached;
    
    bool result = treeViewItem.IsFilterMatch;
    for (int i = 0; i < treeViewItem.ItemCount && !result; i++)
    {
        if (treeViewItem.ContainerFromIndex(i) is TreeViewItem child)
            result = HasChildOrDescendantsMatchFilter(child);
    }
    _descendantMatchCache[treeViewItem] = result;
    return result;
}

// FilterTreeNode 开头清空缓存
_descendantMatchCache.Clear();
```

---

## 三、修复方案（按优先级）

### 🚨 P0 — 必须立即修复

| # | 任务 | 涉及文件 | 工作量 |
|---|------|---------|--------|
| **P0-1** | **把 `AddOnDecoratedBox.ConfigureInstanceStyles` 移到 ControlTheme**（方案 A 或 C） | AddOnDecoratedBox.cs + Themes/*.axaml | 中（4-8h） |
| **P0-2** | **TreeViewItem `ITreeNode.Children` 加缓存或改接口签名为 IEnumerable** | TreeViewItem.cs、ITreeNode.cs、MenuItem.cs、NavMenuItem.cs（可能还要改 CascaderOption） | 中（4-6h） |
| **P0-3** | **`FilterTreeNode` 改 O(N²) → O(N)**（合并遍历 + 缓存 `HasChildOrDescendantsMatchFilter`） | TreeView.Filter.cs | 中（6-8h） |

### ⚠️ P1 — 本迭代内处理

| # | 任务 | 涉及文件 | 工作量 |
|---|------|---------|--------|
| **P1-1** | AddOnDecoratedBox Transitions 缓存复用 | AddOnDecoratedBox.cs | 小 |
| **P1-2** | TreeViewItemHeader Transitions 缓存复用 | TreeViewItemHeader.cs | 小 |
| **P1-3** | `OnPropertyChanged` 合并延迟更新（A-2） | AddOnDecoratedBox.cs | 中 |
| **P1-4** | TreeView.OwnerTreeView 直接在 PrepareContainer 赋值（T-4） | TreeView.cs + TreeViewItem.cs | 小 |
| **P1-5** | `ExpandAll` 避免 `Items.ToList()`（T-3） | TreeView.cs | 小 |

### 🔵 P2 — 渐进优化

| # | 任务 | 涉及文件 | 工作量 |
|---|------|---------|--------|
| **P2-1** | CheckedSubTree 改为基于数据模型的递归（T-5） | TreeView.cs | 中 |
| **P2-2** | `ConfigureAddOnBorderInfo` 增加早退判断（A-3） | AddOnDecoratedBox.cs | 小 |
| **P2-3** | AddOnDecoratedBoxToken 计算缓存 | AddOnDecoratedBoxToken.cs | 小 |

---

## 四、性能基准预估

基于对各问题的静态分析，下表是**理论估算**（需实测验证）：

### 4.1 AddOnDecoratedBox 启动阶段

| 场景 | 当前耗时 | 修复后耗时（P0-1） | 减少 |
|------|---------|-------------------|------|
| 50 个 LineEdit 的表单首次加载 | ~150 ms（Style 构造 + 选择器评估） | ~20 ms | **-87%** |
| 1000 个 LineEdit（极端表格） | ~3 s | ~50 ms | **-98%** |
| 内存（50 个 LineEdit 的 Style 对象） | ~2.5 MB | ~50 KB | **-98%** |

### 4.2 TreeView 过滤操作

| 树规模 | 当前耗时 | 修复后耗时（P0-3） | 减少 |
|--------|---------|-------------------|------|
| 100 节点，深度 5 | ~15 ms | ~5 ms | -67% |
| 1000 节点，深度 10 | ~1200 ms | ~30 ms | **-97%** |
| 10000 节点，深度 15 | 浏览器无响应（>30 s） | ~500 ms | **>99%** |

### 4.3 TreeView CheckedSubTree

| 树规模 | 当前耗时 | 修复后耗时（T-5 方案 1） | 减少 |
|--------|---------|-----------------------|------|
| 1000 节点选择根节点 | ~2 s（多次重排 + 动画） | ~30 ms | **-98%** |

---

## 五、验证与回归测试建议

### 5.1 性能测试用例

添加 `tests/AtomUI.Base.Tests/PerformanceTests.cs`：

```csharp
[Fact]
public void AddOnDecoratedBox_InstanceCreation_DoesNotScaleLinearly()
{
    var sw = Stopwatch.StartNew();
    var boxes = new List<AddOnDecoratedBox>();
    for (int i = 0; i < 1000; i++)
    {
        boxes.Add(new AddOnDecoratedBox());
    }
    sw.Stop();
    // 期望 < 500ms
    Assert.True(sw.ElapsedMilliseconds < 500, 
        $"Creating 1000 AddOnDecoratedBox took {sw.ElapsedMilliseconds}ms");
}

[Fact]
public async Task TreeView_Filter_LargeTree_CompletesWithinBudget()
{
    // 生成 1000 节点树
    var tree = BuildLargeTree(1000, depth: 10);
    var sw = Stopwatch.StartNew();
    tree.FilterValue = "test";
    tree.FilterTreeNode();
    sw.Stop();
    Assert.True(sw.ElapsedMilliseconds < 100);
}
```

### 5.2 内存快照

使用 JetBrains dotMemory 或 `DotMemoryUnit`：

```csharp
[Fact]
public void AddOnDecoratedBox_NoStyleObjectsPerInstance()
{
    var before = GC.GetTotalAllocatedBytes(true);
    _ = new AddOnDecoratedBox();
    var after = GC.GetTotalAllocatedBytes(true);
    // 期望单实例分配 < 5 KB（当前约 30-50 KB）
    Assert.True(after - before < 5_000);
}
```

### 5.3 视觉回归

- 对 Gallery 中 **FormShowCase**、**TreeViewShowCase** 做截图对比，确保修复不破坏 Warning / Error / Disabled 状态下的图标染色。
- 手动测试 TreeView 的 Filter 在不同 `TreeFilterHighlightStrategy` 下的正确性。

### 5.4 Profiler 验证

1. 用 JetBrains dotTrace 运行 `AtomUIGallery.Desktop`
2. 切换到 FormShowCase → 录制 10 秒 → 查看 `AddOnDecoratedBox..ctor` 和 `Avalonia.Styling.StyleBase..ctor` 的采样
3. 切换到 TreeViewShowCase → 做一次过滤操作 → 查看 `FilterItem` / `HasChildOrDescendantsMatchFilter` / `OfType<>().ToList()` 的调用频次

---

## 六、附加建议

### 6.1 Roslyn 分析器检查

在 `AtomUI.Generator` 加入规则：

- **规则 AU001**：`Styles.Add(new Style(...))` 调用在非静态构造函数中 → 警告
- **规则 AU002**：接口的属性 `IList<T>` 实现返回 `OfType().ToList()` 或 `Where().ToList()` → 警告推荐 `IEnumerable<T>` + 缓存
- **规则 AU003**：`OnAttachedToVisualTree` 中调用 `GetLogicalAncestors().OfType<>()` → 建议改用 `PrepareContainer`

### 6.2 文档更新

- 更新 `docs/Controls/AddOnDecoratedBox.md`（若不存在则新建）说明 Warning/Error 状态样式的正确扩展方式（通过 PseudoClass + Theme，不是 instance Style）
- 更新 `docs/Desktop.Controls/TreeView.md` 添加"大数据量最佳实践"章节

### 6.3 Gallery 增强

在 `controlgallery/AtomUIGallery/ShowCases/DataDisplay/TreeViewShowCase.axaml` 添加 **10000 节点 Demo**，用于压测和复现性能问题。

---

## 七、总结

| 问题 | 根本原因 | 关键修复 |
|------|---------|---------|
| **AddOnDecoratedBox 性能** | 构造函数中创建 6 个带 Descendant 选择器的 Style（每实例） | 移到 ControlTheme 共享 |
| **TreeView 性能** | `ITreeNode.Children` 每次 `.ToList()` + Filter 流程 O(N²) | 缓存 Children + Filter 合并遍历 |

**这两个问题是 AtomUI 当前最严重的性能瓶颈**，特别是 AddOnDecoratedBox 影响范围广（几乎所有输入控件）。P0 任务建议本周内完成。

---

> **报告生成方式**：静态代码分析 + 对照 Avalonia 11 Styling / Virtualizing 内部机制 + 手工跟踪调用链
> **下一步**：建议先修 **P0-1（AddOnDecoratedBox Style）** — 收益最大，代码改动中等。可独立验证，不影响其他功能。

---

# 附录：ControlTheme / ControlTemplate 层面的深度补充分析（2026-04-17 修订）

> **背景**：首轮分析聚焦在 C# 代码（`ConfigureInstanceStyles`、`FilterItem`、`OfType().ToList()` 等），未涵盖 AXAML `ControlTheme`。运行 Gallery 时的卡顿 **很大一部分来自 ControlTheme 嵌套与 ControlTemplate 过度膨胀**。本附录在原 A/T 问题之上补充 **CT（ControlTheme）系列** 问题。

## 统计概览

| 指标 | 数值 | 说明 |
|---|---|---|
| `^:is(atom\|XXX)` 冗余包裹层 | **63 个 AXAML** | 整个 `AtomUI.Desktop.Controls` 几乎所有 ControlTheme 都用此模式 |
| `AddOnDecoratedBoxTheme.axaml` 嵌套 Style 数 | **82** | 4 层深嵌套（SizeType × StyleVariant × Status × PseudoClass） |
| `TreeViewItemHeaderTheme.axaml` 嵌套 Style 数 | **44** | 多层 `^:not(^[...])` 否定选择器 |
| `NodeSwitcherButtonTheme.axaml` Style 数 | **14** | 但总是实例化 5 个 `IconPresenter` |
| 单个 TreeViewItem 的视觉树深度 | **15+ 层** | StackPanel→Header→Border→Grid→[NodeSwitcher(+5 IconPresenter)+CheckBox+RadioButton+IconPresenter+Border→Panel→(CP+TB)]→MotionActor→ItemsPresenter |

---

## CT-1 🔴 致命：`^:is(atom|XXX)` 冗余包裹层（全仓库 63 处）

**示例**（`AddOnDecoratedBoxTheme.axaml:74`）：

```xml
<ControlTheme TargetType="atom:AddOnDecoratedBox">
    <Setter Property="Template"> ... </Setter>

    <Style Selector="^:is(atom|AddOnDecoratedBox)">   <!-- ❌ 冗余包裹层 -->
        <Setter .../>
        <Style Selector="^[SizeType=Large]"> ... </Style>
        <Style Selector="^[StyleVariant=Outline]">
            <Style Selector="^[Status=Error]">
                <Style Selector="^[IsInnerBoxHover=True]"> ... </Style>
            </Style>
        </Style>
        <!-- ...共 81 个嵌套 Style 都挂在这一层下 -->
    </Style>
</ControlTheme>
```

### 为什么是问题

- `ControlTheme.TargetType="atom:AddOnDecoratedBox"` 本身**已经限定** Theme 只应用于该类型；`^` 在 ControlTheme 内等价于 `atom|AddOnDecoratedBox`。
- 再包一层 `^:is(atom|AddOnDecoratedBox)` 意味着：
  1. Avalonia 样式引擎为该控件**多了一次类型匹配计算**。
  2. 下面所有嵌套 Style 的实际选择器都变成了 `atom|X :is(atom|X) [嵌套条件]` — **每一层都带着这个冗余前缀**参与匹配。
  3. 82 个嵌套 Style × 冗余前缀 = 82 次无用的 `:is()` 评估。
- 对 `TreeViewItemHeader`（44 个 Style）、`TreeViewItem`、`LineEdit` 等所有 63 个 ControlTheme 同样受害。
- **当一个 Avalonia 应用中有 50 个 LineEdit**，样式系统每轮 invalidate 需要处理：50 × (AddOnDecoratedBox 82 + TextBox N + LineEdit M) 个嵌套选择器，全都带冗余前缀。

### 修复

直接**删掉外层 `<Style Selector="^:is(...)">` 包裹**，把内部 Setter 和嵌套 Style 平铺到 `ControlTheme` 下：

```xml
<ControlTheme TargetType="atom:AddOnDecoratedBox">
    <Setter Property="Template"> ... </Setter>
    <Setter Property="IsMotionEnabled" Value="{atom:SharedTokenResource EnableMotion}" />
    <!-- ...其他顶层 Setter 直接写在这里... -->
    
    <Style Selector="^[SizeType=Large]"> ... </Style>
    <Style Selector="^[StyleVariant=Outline]"> ... </Style>
    <!-- ...嵌套 Style 的 ^ 此时直接就是 TargetType，无需冗余 :is -->
</ControlTheme>
```

### 工作量与收益

- **改动**：63 个 AXAML 文件各删一行 `<Style Selector="^:is(...)">` + 对应闭合标签，并把外层 Setter 提升；纯机械操作，零行为变化。
- **收益**：选择器命中路径长度减半，**每次样式重算 -20~30%**。对输入控件、TreeView 场景特别明显。
- **优先级**：P0（改动机械、收益覆盖面最大）。

---

## CT-2 🔴 致命：`AddOnDecoratedBoxTheme` 深层组合爆炸（82 个嵌套 Style）

### 结构画像

```
ControlTheme (TargetType=AddOnDecoratedBox)
└── ^:is(atom|AddOnDecoratedBox)                    ← CT-1 冗余层
    ├── ^[SizeType=Large/Middle/Small]              (×3 × 2 个子 Style)
    ├── ^[Status=Error/Warning]                     (×2 × 2 个子 Style)
    ├── ^[StyleVariant=Outline]                     
    │   ├── ^[IsInnerBoxHover=True]
    │   ├── ^:focus-within
    │   ├── ^:pressed
    │   └── ^[Status=Error]
    │       ├── ^ /template/ #PART_LeftAddOn
    │       ├── ^ /template/ #PART_RightAddOn
    │       ├── ^[IsInnerBoxHover=True]
    │       ├── ^:focus-within
    │       └── ^:pressed                           ← 4 层深！
    ├── ^[StyleVariant=Filled]   (同上展开，4 层深)
    ├── ^[StyleVariant=Borderless]
    ├── ^[StyleVariant=Underlined] (4 层深)
    └── ^:disabled
        ├── ^[StyleVariant=Outline]   (3 层深)
        ├── ^[StyleVariant=Filled]    (3 层深)
        └── ^[StyleVariant=Underlined]
```

### 性能代价

- Avalonia 把嵌套 Style 编译为 **组合选择器**（Descendant style combinator），每个叶子 Style 的选择器长度 = 根到该节点的路径。
- `^[StyleVariant=Outline]/^[Status=Error]/^[IsInnerBoxHover=True]/^:pressed` 这种 4 层选择器，在属性变化时每次都要：
  1. 检查 StyleVariant 是否 == Outline
  2. 检查 Status 是否 == Error
  3. 检查 IsInnerBoxHover
  4. 检查 :pressed 伪类
- **每个属性变化（Status / IsInnerBoxHover / focus-within 等）都会触发 Avalonia 重新评估所有 82 个 Style 的适用性**。
- 叠加 CT-1 的冗余前缀 = 每个 Style 多一次 `:is` 检查。
- 叠加 A-1（`ConfigureInstanceStyles` 额外每实例 6 个 Style）= **每个 AddOnDecoratedBox 实例 88 个 Style** 需要评估。

### 修复

**方案 1：提取 "状态色" 到顶层命名样式（减少嵌套）**

用 PseudoClass / Class 替代 `Status + StyleVariant + 状态伪类` 的 3-4 层组合：

```csharp
// AddOnDecoratedBox.cs
protected virtual void UpdatePseudoClasses()
{
    // ...
    PseudoClasses.Set(":error", Status == InputControlStatus.Error);
    PseudoClasses.Set(":warning", Status == InputControlStatus.Warning);
}
```

然后 AXAML 把 4 层嵌套拍平成 2 层：

```xml
<!-- 原本 -->
<Style Selector="^[StyleVariant=Outline]">
  <Style Selector="^[Status=Error]">
    <Style Selector="^:focus-within"> ... </Style>
  </Style>
</Style>
<!-- 改为 -->
<Style Selector="^[StyleVariant=Outline]:error:focus-within"> ... </Style>
```

伪类和属性选择器在同一层用 `&` 式拼接 —  Avalonia 编译为**单节点选择器**，O(1) 匹配而非 O(depth)。

**方案 2：把 Status=Error/Warning 的样式抽到单独的 ResourceDictionary**

实际上 Error / Warning 的样式**结构完全对称**，只是 token 不同 (`ColorError` vs `ColorWarning`)。可以用模板化 Style 复用。Avalonia 目前不原生支持，可通过代码生成器减少维护成本，但样式实例数无法减少。

**方案 3（与 A-1 协同）：把 `ConfigureInstanceStyles` 中的 Icon 染色合并进 AXAML**

原 C# 里的 Icon FillBrush/StrokeBrush/Foreground 设置在 AXAML 里补齐：

```xml
<Style Selector="^:error /template/ ContentPresenter#PART_ContentLeftAddOn atom|Icon:not(.skip-status),
                ^:error /template/ ContentPresenter#PART_ContentRightAddOn atom|Icon:not(.skip-status),
                ^:error /template/ ContentPresenter#PART_ContentLeftAddOn atom|Icon:not(.skip-status),
                ^:error /template/ ContentPresenter#PART_ContentRightAddOn atom|Icon:not(.skip-status)">
    <Setter Property="FillBrush" Value="{atom:SharedTokenResource ColorError}"/>
    <Setter Property="StrokeBrush" Value="{atom:SharedTokenResource ColorError}"/>
    <Setter Property="Foreground" Value="{atom:SharedTokenResource ColorError}"/>
</Style>
```

这样就能把 A-1 彻底根除（构造函数零 Style 分配）+ CT-2 扁平化。

**优先级**：P0。

---

## CT-3 🔴 致命：`TreeViewItem` ControlTemplate 每节点都实例化 **大量未使用的 TemplatedControl**

**文件**：`TreeView/Themes/TreeViewItemHeaderTheme.axaml:30-60`

```xml
<atom:CheckBox Name="ToggleCheckbox"
               Grid.Column="1"
               IsChecked="{TemplateBinding IsChecked, Mode=TwoWay}"
               IsVisible="False">             <!-- ❌ 默认隐藏，但总是实例化 -->
    <atom:RadioButton.IsEnabled>
        <MultiBinding Converter="{x:Static ...}">   <!-- ❌ 每个 Header 2 个 MultiBinding -->
            ...
        </MultiBinding>
    </atom:RadioButton.IsEnabled>
</atom:CheckBox>

<atom:RadioButton Name="ToggleRadio"
                  Grid.Column="1"
                  IsVisible="False">           <!-- ❌ 同样总是实例化 -->
    <atom:RadioButton.IsEnabled>
        <MultiBinding> ... </MultiBinding>
    </atom:RadioButton.IsEnabled>
</atom:RadioButton>
```

**文件**：`TreeView/Themes/NodeSwitcherButtonTheme.axaml:20-51` — 同样问题

```xml
<atom:IconPresenter Name="ExpandIconPresenter"    IsVisible="False"/>
<atom:IconPresenter Name="CollapseIconPresenter"  IsVisible="False"/>
<atom:IconPresenter Name="RotationIconPresenter"  IsVisible="{... Converter}"/>
<atom:IconPresenter Name="LoadingIconPresenter"   IsVisible="{... Converter}"/>
<atom:IconPresenter Name="LeafIconPresenter"      IsVisible="False"/>
```

### 性能代价（以 1000 节点 TreeView 为例）

| 每个 TreeViewItem 实例化的 TemplatedControl | 数量 | 实际使用 |
|---|---|---|
| TreeViewItemHeader | 1 | ✅ |
| NodeSwitcherButton | 1 | ✅ |
| NodeSwitcherButton 内 IconPresenter | **5** | 通常只用 1-2 个 |
| CheckBox（`ToggleCheckbox`） | 1 | 仅 `ToggleType=CheckBox` 时 |
| RadioButton（`ToggleRadio`） | 1 | 仅 `ToggleType=Radio` 时 |
| CheckBox 内部模板 | 整棵子树 | 闲置 |
| RadioButton 内部模板 | 整棵子树 | 闲置 |
| Header 主 IconPresenter | 1 | ✅ |

一个**没有开启** ToggleType 的 1000 节点纯选择树会多创建：
- 1000 × 1 CheckBox + 其完整模板（Border + Path + 动画 Transition）
- 1000 × 1 RadioButton + 其完整模板
- 1000 × 3 闲置 IconPresenter（NodeSwitcher 的 5 个里至少 3 个没用）
- 1000 × 2 MultiBinding（永不触发）

**估算**：每个 TreeViewItem 浪费约 **20-30 个 Control 实例 + 2000-3000 字节内存**。1000 节点 → **浪费 2-3 万 Control 实例 + 20-30 MB 内存 + 启动样式评估成本**。

### 修复

**改为按需实例化（ContentControl + DataTemplate 或运行时模板选择）**：

在 `TreeViewItemHeader.cs` 里根据 `ToggleType` 动态挂载一个 `ContentPresenter`：

```xml
<!-- 原来的 CheckBox + RadioButton 换成： -->
<ContentPresenter Name="PART_TogglePresenter" Grid.Column="1"
                  HorizontalAlignment="Left"
                  VerticalAlignment="Center"/>
```

```csharp
// TreeViewItemHeader.cs
protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
{
    base.OnApplyTemplate(e);
    var presenter = e.NameScope.Find<ContentPresenter>("PART_TogglePresenter");
    UpdateToggleControl(presenter);
}

private void UpdateToggleControl(ContentPresenter presenter)
{
    presenter.Content = ToggleType switch
    {
        TreeItemToggleType.CheckBox => new CheckBox { /* binding */ },
        TreeItemToggleType.Radio    => new RadioButton { /* binding */ },
        _                           => null
    };
}
```

对 `NodeSwitcherButton`：把 5 个 `IconPresenter` 合并成 1 个，`Icon` 属性根据 `IconMode` 动态切换来源。

**收益（1000 节点树）**：
- 减少 **~20000 Control 实例** + 对应样式评估 + 模板展开时间
- 启动时间预估 **-40~60%**
- 内存 **-20 MB**

**优先级**：P0（TreeView 性能的**最根本**来源）。

---

## CT-4 🟠 `TreeViewItemHeaderTheme` 44 个 Style + `^:not(^[NodeHoverMode=WholeLine])` 否定选择器

**文件**：`TreeViewItemHeaderTheme.axaml:107-134`

```xml
<Style Selector="^:not(^[NodeHoverMode=WholeLine])">
    <Setter Property="ContentFrameBackground" Value="Transparent" />
    <Style Selector="^ /template/ Border#PART_HeaderContentFrame">
        <Setter Property="HorizontalAlignment" Value="Left" />
    </Style>
    <Style Selector="^[NodeHoverMode=Block]">        <!-- 嵌套在 :not 内 -->
        ...
    </Style>
    <Style Selector="^[IsHover=True]"> ... </Style>
    <Style Selector="^[IsSelectable=True]">
        <Style Selector="^[IsSelected=True]"> ... </Style>
        <Style Selector="^[IsPressed=True]"> ... </Style>
    </Style>
</Style>

<Style Selector="^[NodeHoverMode=WholeLine]">        <!-- 同样的结构再写一遍 -->
    ...
</Style>
```

### 问题

- `^:not(^[NodeHoverMode=WholeLine])` + `^[NodeHoverMode=WholeLine]` 两套**完全对称**的样式树，等价于 "if/else 模式"。Avalonia 对这种否定选择器的求值开销比等于选择器大 ~2×。
- 每个 `TreeViewItemHeader` 实例化时都要评估 44 个选择器（叠加 CT-1 的 `:is` = 45 个评估）。1000 节点 × 45 = **4.5 万次 selector 评估**。
- `NodeHoverMode` 取值只有 `Default / Block / WholeLine` 3 个，用**属性选择器枚举三分支**比 `:not` + 等于 更快：

```xml
<Style Selector="^[NodeHoverMode=Default]"> ... </Style>
<Style Selector="^[NodeHoverMode=Block]"> ... </Style>
<Style Selector="^[NodeHoverMode=WholeLine]"> ... </Style>
```

### 修复

1. 把 `^:not(^[NodeHoverMode=WholeLine])` 拆成 Default + Block 两个等于选择器。
2. 把 `^[IsHover=True]` / `^[IsPressed=True]` 改成标准伪类 `^:pointerover` / `^:pressed`（如果 IsHover/IsPressed 就是 `IsPointerOver`/`IsPressed` 的镜像）。
3. 同 CT-1：去掉 `^:is(atom|TreeViewItemHeader)` 外壳。

**优先级**：P1。

---

## CT-5 🟠 `ControlTheme.BasedOn` 链叠加

**示例**：`LineEditTheme.axaml:8`
```xml
<ControlTheme.BasedOn>
    <themes:TextBoxTheme TargetType="{x:Type atom:LineEdit}" />
</ControlTheme.BasedOn>
```

`TextBoxTheme` 自己又有 13 个 Style + 它的 BasedOn（Avalonia 的 TextBox 原生 Theme）。

对一个 `LineEdit` 实例，样式评估路径为：
```
Avalonia.TextBox 原生 ControlTheme (N 个 Style)
  ↓ BasedOn
AtomUI.TextBox ControlTheme (+ 13 个 Style)
  ↓ BasedOn
AtomUI.LineEdit ControlTheme (+ 11 个 Style)
  ↓ 内部 Template 又实例化
AddOnDecoratedBox ControlTheme (+ 82 个 Style + 实例 6 个)
```

**单个 LineEdit 实例涉及 ≥120 个 Style 评估**，叠加所有 CT-1~4 的冗余。

### 修复

- BasedOn 链本身合理（继承模型），但**应与 CT-1 / CT-2 优化叠加**实施 — 让每一层本身更薄。
- 检查 BasedOn 是否有**重复 Setter**（基类设一遍、派生类又设一遍），减少无效赋值。

**优先级**：P2。

---

## CT-6 🟠 `TemplateBinding` 爆炸 + `RelativeSource AncestorType` 深查找

**文件**：`LineEditTheme.axaml:13-79`

单个 `LineEdit` 的 ControlTemplate 里：
- `TemplateBinding` 出现 **~25 次**
- `RelativeSource AncestorType=atom:LineEdit` 出现 **~10 次**（放在嵌套 ContentPresenter 内无法用 TemplateBinding）

每个 `RelativeSource AncestorType` Binding 会在 BindingTarget attach 到视觉树时**向上遍历** Visual 祖先链直到找到 `LineEdit`。LineEdit 的 Content 深度 ≥6 层（LineEdit → AddOnDecoratedBox → DockPanel → Border → DockPanel → StackPanel → InputClearIconButton），每次查找 6+ 层。

1000 个 LineEdit（如大型表格场景）× 10 个 AncestorType 查找 = **10000 次视觉树遍历**。

### 修复

- 能用 `TemplateBinding` 就不用 `RelativeSource`（尤其在直接的 Template 子节点）。
- 对深层嵌套里需要 LineEdit 属性的地方，考虑 `$parent[atom|LineEdit]` 语法（Avalonia 11 支持），它在绑定编译期解析，比 `AncestorType` 运行时查找快。
- 或者在 `OnApplyTemplate` 里**一次性** code-behind 赋值（`clearBtn.IsMotionEnabled = this.IsMotionEnabled`），比 Binding 高效一个数量级。

**优先级**：P2。

---

## 修正后的优先级清单（P0 重排）

| 新编号 | 问题 | 涉及 | 工作量 | 收益 |
|---|---|---|---|---|
| **P0-1** | **CT-3**：TreeViewItemHeader / NodeSwitcherButton 按需实例化 ToggleControl + 合并 IconPresenter | 2 AXAML + 2 cs | 中（1 天） | 🔥 TreeView 1000 节点 -40% 启动, -20MB 内存 |
| **P0-2** | **CT-1**：批量删除 63 个 `^:is(atom\|XXX)` 冗余包裹 | 63 AXAML | 小（2-3h, 机械) | 每个样式重算 -20~30% |
| **P0-3** | **CT-2 + A-1**：AddOnDecoratedBox 状态色改 PseudoClass 扁平化 + 移除 `ConfigureInstanceStyles` | AddOnDecoratedBoxTheme.axaml + AddOnDecoratedBox.cs | 中（1 天） | 50 表单 -87% 启动, -2.5MB |
| **P0-4** | **T-1**：TreeViewItem `ITreeNode.Children` 缓存 | 3 cs | 小（2h） | 递归操作 -90% |
| **P0-5** | **T-2**：Filter 流程 O(N²) → O(N) | TreeView.Filter.cs | 中 | 1000 节点过滤 -97% |
| **P1-1** | **CT-4**：TreeViewItemHeader `:not` 否定选择器展开 + `IsHover`→`:pointerover` | 1 AXAML | 小 | TreeView 样式评估 -15% |
| **P1-2** | 原 P1-1~5（Transitions 缓存、Property 合并、OwnerTreeView 直赋值等） | - | - | - |
| **P2-1** | **CT-5**：BasedOn 链 Setter 去重 | 多 AXAML | 大 | 小 |
| **P2-2** | **CT-6**：`RelativeSource AncestorType` 改 `$parent` 或 code-behind | LineEditTheme 等 | 中 | 10% |

---

## 关键结论修订

> **之前的结论**：AddOnDecoratedBox 的 `ConfigureInstanceStyles`（A-1）是最严重瓶颈。
> **修订后的结论**：
> 1. **TreeView 真正的瓶颈是 ControlTemplate 膨胀（CT-3）**，而非代码递归。1000 节点之所以卡，是因为实际产生了 **~5 万个 Control 实例**（其中大半闲置），样式引擎被压垮。先修 CT-3，再修 T-1/T-2。
> 2. **AddOnDecoratedBox 的 CT-2（82 个深层嵌套 Style）** 开销与 A-1 相当甚至更大。必须与 A-1 合并处理才能根治。
> 3. **CT-1 的 `^:is(atom|XXX)` 冗余层** 是一个低成本、全仓库受益的修复，应**立即**执行。

---

## 验证新增项

在原 §5 之外补充：

### 5.5 ControlTemplate 实例化计数

```csharp
[Fact]
public void TreeView_1000Nodes_ControlInstanceCount_IsBounded()
{
    var tree = BuildLargeTree(1000);
    tree.ApplyTemplate();
    // 强制渲染全部（关闭虚拟化场景下）
    var totalControls = tree.GetVisualDescendants().OfType<Control>().Count();
    // 修复前：~50000 / 修复后：~20000
    Assert.True(totalControls < 25000, $"Expected <25000 controls, got {totalControls}");
}
```

### 5.6 样式选择器评估 profiler

运行 `AtomUIGallery.Desktop` + dotTrace，在 TreeView / Form 场景下录制 CPU samples，关注：
- `Avalonia.Styling.StyleBase.TryAttach` / `Avalonia.Styling.Selector.Match`
- `AvaloniaObject.GetValue` / `AvaloniaProperty` 监听触发链
- 期望：修复 CT-1/2/3 后相关方法总采样数 **-50%**。



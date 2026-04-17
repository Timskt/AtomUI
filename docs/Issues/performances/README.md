# AtomUI Desktop 控件性能分析报告

> **生成日期**：2026-04-17
> **分析范围**：`src/AtomUI.Desktop.Controls/` 下全部 ~80 个控件
> **基线文档**：[《AddOnDecoratedBox 与 TreeView 性能深度分析报告》](../AddOnDecoratedBox_TreeView_Performance.md)（已就 `AddOnDecoratedBox`、`TreeView`、`TreeViewItem` 做了深入分析，本系列只引用不重复）
>
> 本报告沿用基线文档的问题分类体系（A–K）、严重度 emoji、证据粒度与「根因 → 方案 → 收益」三段式。

---

## 文档导航

| 文档 | 覆盖控件族 |
|---|---|
| [01-Inputs-And-Editors](./01-Inputs-And-Editors.md) | Input / NumericUpDown / Mentions / AutoComplete / Cascader / Select / TreeSelect / ComboBox / DatePicker / TimePicker |
| [02-Buttons-And-Toggles](./02-Buttons-And-Toggles.md) | Buttons / OptionButtonGroup / RadioButton / CheckBox / Switch / Segmented / FloatButton / ToggleIconButton |
| [03-Data-Containers](./03-Data-Containers.md) | ListBox / ListView / TreeView / Transfer / Menu / NavMenu / TabControl / Collapse / Expander / Pagination |
| [04-Feedback-Controls](./04-Feedback-Controls.md) | Alert / Message / Notifications / Dialog / Drawer / MessageBox / Popup / Flyouts / Tooltip / PopupConfirm / ProgressBar / Spin / Skeleton / Result / Empty |
| [05-Display-And-Layout](./05-Display-And-Layout.md) | Avatar / Badge / Tag / Card / Descriptions / Statistic / Timeline / Steps / QRCode / Rate / Calendar / Carousel / ImagePreviewer / MarqueeLabel / Separator / TextBlock / GroupBox / HeaderedContentControl |
| [06-Panels-And-Primitives](./06-Panels-And-Primitives.md) | BoxPanel / FlexPanel / Grid / Space / Splitter / SplitView / ScrollViewer / AdornerLayer / Primitives |
| [07-Navigation-And-Window](./07-Navigation-And-Window.md) | Breadcrumb / Chrome / Window / Tour / Form / Upload / Slider |

---

## 问题分类体系

| 代码 | 分类 | 描述 | 典型模式 |
|---|---|---|---|
| **A** | 每实例 Style / 选择器爆炸 | 构造函数里 `new Style(...)` 并 `Styles.Add`，每个实例都持有自己的选择器树 | `ConfigureInstanceStyles()` + `Selectors.Or(...)` |
| **B** | 属性变更回调重复计算 | `OnPropertyChanged` 对一次变化触发多次完整重计算 / invalidate | 多个 `if` 分支都调用同一 invalidate |
| **C** | 值类型频繁分配 | 热路径中重复 `new Thickness(...)` / `new CornerRadius(...)` / `new Rect(...)` | 每次 Measure/Arrange 重建 |
| **D** | Transitions 生命周期抖动 | `OnLoaded` 新建、`OnUnloaded` 置空，`OnPropertyChanged` 又重建 | `Transitions = new Transitions()` / `Transitions = null` 成对出现 |
| **E** | LINQ 在热路径 | `OfType<>().ToList()`、`FirstOrDefault()`、`GetLogical/VisualAncestors` 用于每帧或每 item 访问 | 树节点子集合计算、祖先查找、集合投影 |
| **F** | 递归 / 全树遍历无缓存 | 过滤、全展开、选中传播、链路查找每次都从根开始递归 | `FilterTreeNode` / `ExpandAll` / `CheckedSubTree` |
| **G** | 事件订阅未解绑 | `Subscribe` / `AddHandler` 未 `Dispose` / `Remove` | 闭包持有 control，跨 TopLevel 生命周期 |
| **H** | Layout 冗余 invalidate | `InvalidateMeasure` / `InvalidateArrange` 级联触发 | 集合变化一次触发多次 |
| **I** | Binding / Resource 热路径 | `SharedTokenResource` / `TokenResource` 在非模板代码路径每次访问都走 Resource 查找 | Getter/Setter 中 `FindResource` |
| **J** | 虚拟化缺失 / 不当 | 列表/树/菜单下拉未启用 `VirtualizingStackPanel` | `ItemsPanel` 显式指定非虚拟化 Panel |
| **K** | Motion / WaveSpirit 过度创建 | 每次状态切换重建动画对象或 MotionActor | `IMotionAwareControl` 未复用 |

---

## 全局严重度 Top 20（跨控件族聚合）

> 评分公式：`严重度(🔴=4 / 🟠=3 / 🟡=2 / 🟢=1) × 影响面(基础/底座=3, 常用=2, 少用=1)`
> 影响面以"多少其他控件直接继承或组合"为依据。

| # | 问题 ID | 文件 / 位置 | 严重度 | 影响面 | 分数 | 简述 |
|---|---|---|---|---|---|---|
| 1 | **AOB-A1 / T-1 / T-2** | `AddOnDecoratedBox.cs` / `TreeView*.cs` | 🔴 | 3 | 12 | 见基线文档（每实例 3 Style + 每次 `OfType().ToList()` + 过滤全树递归） |
| 2 | **M-1** | `Menu/MenuItem.cs:53` | 🔴 | 3 | 12 | `ITreeNode<IMenuItemData>.Children => Items.OfType<IMenuItemData>().ToList()` 与 TreeView T-1 完全同构，`Menu/NavMenu/Cascader 级联` 热路径频繁访问 |
| 3 | **BTN-D1** | `Buttons/Button.cs:359-389` | 🟠 | 3 | 9 | `ConfigureTransitions(force)` 在 `IsMotionEnabled / ButtonType / IsGhost / Shape` 任一变化时都重建 `Transitions`，Button 是使用最广的基础控件 |
| 4 | **NAV-D1** | `NavMenu/Header/BaseNavMenuItemHeader.cs:148-175` | 🟠 | 3 | 9 | 每个可见 NavMenuItemHeader 在 OnLoaded 新建 Transitions，OnUnloaded 置空——对水平菜单 + 动态子菜单场景引发 GC 抖动 |
| 5 | **SEL-E1** | `Select/Select.cs:284,550,596,669,691` | 🟠 | 2 | 6 | 5 处 `SelectedOptions?.ToList()` / `.Cast<>.ToList()`，在多选 Select 高频交互下每次都复制整个集合 |
| 6 | **NAV-E1** | `NavMenu/NavMenu.cs:476,531` | 🟠 | 2 | 6 | `TraverNavMenuPathAsync` 两处 `Items.ToList()`，且在循环内层又赋值 `items = navMenuItem.Items`，大菜单路由恢复时 O(depth × fanout) 复制 |
| 7 | **SPC-A1** | `Space/Space.cs:169-192` | 🟠 | 2 | 6 | 每个 `Space` 实例构造 3 个 SizeType Style，Space 被大量页面作为外层容器使用 |
| 8 | **AOB-E1** | `AutoComplete/Select/Cascader/Mentions/TreeSelect` 各自的 `GetVisualAncestors().OfType<Control>()` | 🟠 | 2 | 6 | 多处在 `OnAttachedToVisualTree` 调用，Avalonia 的 Visual 树遍历成本不低；方案：改为一次 `TopLevel.GetTopLevel(this)` 或缓存直系祖先 |
| 9 | **SPI-A1** | `Spin/SpinIndicator.cs:19-59` | 🟠 | 2 | 6 | 每个 SpinIndicator 实例 3 个 Style + 3 次 `Selectors.Or`；Spin 在页面级加载态遍布 |
| 10 | **WNM-E1** | `Notifications/WindowNotificationManager.cs:210` | 🟠 | 2 | 6 | 每次 `Show` 都 `OfType<NotificationCard>().Where(!IsClosing).ToList()`，高频弹通知场景下放大 |
| 11 | **DLG-E1** | `Primitives/DialogLayer.cs:75` | 🟠 | 2 | 6 | 每次对话框打开都 `GetVisualDescendants().OfType<DialogLayerManager>().FirstOrDefault()`，MessageBox/Dialog 高频打开时可观察 |
| 12 | **TAB-E1** | `TabControl/TabControlScrollViewer.cs:67` | 🟠 | 2 | 6 | `MenuFlyout.Items.ToList()` 每次打开 Tab 溢出菜单都复制整个集合 |
| 13 | **UP-D1** | `Upload/*.cs`（4 个文件） | 🟠 | 1 | 3 | Upload 列表项全部采用 "OnLoaded new Transitions + OnUnloaded = null" 模式，大量文件列表场景抖动 |
| 14 | **GRID-C1/E1** | `Grid/Row.cs:234-255, 79-80` | 🟡 | 2 | 4 | 每次 Measure 都构造 `List<RowChildInfo>` + `OrderBy.ThenBy.ToList`，MediaBreak 切换再次全部重排 |
| 15 | **CAN-E1** | `Primitives/CandidateList/CandidateList.cs:218` 与 `SelectCandidateList.cs:157` | 🟡 | 2 | 4 | `GetLogicalDescendants().OfType<ScrollViewer>().FirstOrDefault()` 每次打开候选列表都跑一次；应在 OnApplyTemplate 缓存 |
| 16 | **SPL-E1** | `Splitter/SplitterPanel.cs:112,140` | 🟡 | 2 | 4 | `Children.ToList()` + `_trackedPanels.ToList()` 在拖拽期每次 Measure 触发 |
| 17 | **AVG-E1** | `Avatar/AvatarGroup.cs:139` & `Space/CompactSpace.cs:125,147,440` | 🟡 | 2 | 4 | 集合变更事件上 `OfType<Control>().ToList()`，大头像组/紧凑布局切换时放大 |
| 18 | **BRD-D1** | `Breadcrumb/BreadcrumbItem.cs` / `Card/CardActionPanel.cs` / `ListView/ListViewItem.cs` / `Cascader/CascaderViewItem.cs` / `TextBlock/HyperLinkTextBlock.cs` | 🟡 | 2 | 4 | 统一的 "OnLoaded new Transitions + OnUnloaded = null" 模式，在虚拟化场景每次 Realize 都执行 |
| 19 | **MRK-A1** | `MarqueeLabel/MarqueeLabel.cs:17-23` / `TextBlock/TextBlock.cs:21-23` / `SelectableTextBlock.cs:116-120` / `Result/Result.cs:23-39` / `Popup/Popup.cs:208` | 🟡 | 2 | 4 | 单 Style `Styles.Add` 模式——虽然只有 1 个，但每实例仍要分配 Style 对象；可提升到 `StyleInclude` / Theme |
| 20 | **IMG-H1** | `ImagePreviewer/ImageViewer.cs:571,708` | 🟡 | 1 | 2 | Zoom/Pan 手势期 `InvalidateArrange()` 双处触发，期间应合并为单次 |

---

## 使用指南

- **阅读顺序建议**：先读基线文档 → README Top 20 → 重点族文档。
- **实施建议**：
  - 🔴 / 🟠 项建议排入本季度修复。
  - 🟡 项建议在碰到相关控件改动时顺手清理。
  - 🟢 项记录在案，避免在未来 PR 中复现同类模式。
- **回归验证**：修复每一类问题后，至少在 `AtomUIGallery.Desktop` 对应 ShowCase 运行 dotTrace/PerfView 对比 CPU 与 GC 分配。

---

## 与基线文档的关系

- `AddOnDecoratedBox`（A-1/2/3/4）与 `TreeView/TreeViewItem`（T-1..T-7）**不在本系列重复**，请直接查阅基线文档。
- 本系列发现的 **Menu/MenuItem T-1 同构问题（M-1）** 单独列出，因为它跨越 `Menu`、`NavMenu`、`Cascader` 三大控件族。



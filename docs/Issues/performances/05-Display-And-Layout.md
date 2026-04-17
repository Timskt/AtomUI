# 05 · 展示 / 布局元素 族性能分析

> **覆盖控件**：`Avatar` · `Badge` · `Tag` · `Card` · `Descriptions` · `Statistic` · `Timeline` · `Steps` · `QRCode` · `Rate` · `Calendar` · `Carousel` · `ImagePreviewer` · `MarqueeLabel` · `Separator` · `TextBlock` · `GroupBox` · `HeaderedContentControl`

---

## 问题严重度总览

| # | 问题 | 严重度 | 影响面 |
|---|---|---|---|
| **AVG-E1** | `AvatarGroup.cs:139` / `Space/CompactSpace.cs:125,147,440` 集合变更事件 `OfType<Control>().ToList()` | 🟡 中 | 大头像组 / 动态 CompactSpace |
| **IMG-H1** | `ImageViewer.cs:571,708` Zoom/Pan 手势期 `InvalidateArrange()` 双处触发 | 🟡 低 | 图片预览手势期 |
| **CAR-H1** | `VirtualizingCarouselPanel.cs:44,271` InvalidateMeasure 两处 | 🟡 低 | Carousel 滚动 |
| **CAL-C1** | `CalendarToken.cs` 中 `new Thickness` —— Token 初始化一次性，无问题 | 🟢 低 | 一次性 |
| **MRK-A1** | `MarqueeLabel.cs:17-23` / `TextBlock.cs:21-23` / `SelectableTextBlock.cs:116-120` 每实例 `new Style` | 🟡 中 | 文本控件大量实例 |
| **CRD-D1** | `CardActionPanel.cs:116,129` OnLoaded/OnUnloaded `Transitions = null` 抖动 | 🟡 低 | 卡片动作区 |
| **HLT-D1** | `HyperLinkTextBlock.cs:179,362` Transitions 抖动 | 🟡 低 | 链接文本 |

---

## 1. AVG-E1 / CompactSpace 集合变动 LINQ 拷贝

**命中**：

| 文件 | 行号 | 模式 |
|---|---|---|
| `src/AtomUI.Desktop.Controls/Avatar/AvatarGroup.cs` | 139 | `e.NewItems!.OfType<Control>().ToList()` |
| `src/AtomUI.Desktop.Controls/Space/CompactSpace.cs` | 125, 147, 440 | 同类 |
| `src/AtomUI.Desktop.Controls/Space/Space.cs` | 215, 231 | 同类 |

### 根因

- `CollectionChanged` 事件每次变动都 `OfType.ToList` 一份，然后 `InsertRange` / `RemoveAll`。
- 大多数 `InsertRange` / `RemoveAll` 也能直接接受 `IEnumerable` —— 多一个 `ToList` 是防御性复制（担心在 `InsertRange` 内部迭代期 `NewItems` 被改）。

### 解决方案

1. 检查 Avalonia `VisualChildren.InsertRange(int, IEnumerable<Visual>)` 的契约：是否允许传 LINQ 惰性序列？若允许，移除 `ToList()`。
2. 若不允许，至少合并成一次投影 + 一次 `ToList`（目前 `NewItems.OfType<Control>().ToList()` + `NewItems.OfType<Visual>()` 两次投影）。
3. `CompactSpace.cs:440` 的 `children.Where(...).ToList()` 在测量期被调用——此处改为 `for` 循环 + 预分配 `List`。

### 预估收益

- 频繁集合变动场景（如动态头像组）节省分配。

---

## 2. IMG-H1 / CAR-H1：图像/轮播 InvalidateArrange

**ImageViewer**：`ImagePreviewer/ImageViewer.cs:571,708`
**VirtualizingCarouselPanel**：`Carousel/VirtualizingCarouselPanel.cs:44,271`

### 根因

- 手势连续期（pinch / pan）每帧触发 `InvalidateArrange`——Avalonia 的布局系统会合并帧末才重新 Arrange，单帧多次调用不造成多次实际 Arrange，但每次调用仍触发一次 `ILayoutManager` 入队。
- 记录为可改进点：若同一帧已排队过，可以跳过。

### 解决方案

- 引入 `_arrangeQueued` 标记：
  ```csharp
  if (!_arrangeQueued)
  {
      _arrangeQueued = true;
      InvalidateArrange();
  }
  // Arrange override 开始时清标记
  ```

### 预估收益

- 快速手势期少量 CPU 省略。

---

## 3. MRK-A1：文本控件每实例单 Style

**命中**：

| 文件 | 行号 |
|---|---|
| `src/AtomUI.Desktop.Controls/MarqueeLabel/MarqueeLabel.cs` | 17–23 |
| `src/AtomUI.Desktop.Controls/TextBlock/TextBlock.cs` | 21–23 |
| `src/AtomUI.Desktop.Controls/TextBlock/SelectableTextBlock.cs` | 116–120 |

```csharp
var styles = new Style();
// setters
Styles.Add(styles);
```

### 根因

- 每 `TextBlock` 实例都持有一份 Style，AtomUI 中任何静态文本都用这类 TextBlock——页面动辄数百实例。
- 虽然 Style 简单，每实例 1 个对象仍是 N 倍放大。

### 解决方案

- 将 Style 迁移到 `TextBlockTheme.axaml` / `MarqueeLabelTheme.axaml` ControlTheme——所有实例共享。

### 预估收益

- 200 文本页省 200 个 Style 对象 + 初始化开销。

---

## 4. CRD-D1 / HLT-D1：Card / HyperLink Transitions 抖动

同 BTN-D1 / NAV-D1 / CAS-D1：OnLoaded 新建，OnUnloaded 置 null。方案同前。

---

## 5. CalendarToken 的 `new Thickness`

**文件**：`Calendar/CalendarToken.cs:126`

```csharp
HeaderMargin = new Thickness(0, 0, 0, SharedToken.UniformlyMarginXS);
```

### 评估

- 在 `CalculateTokenValues` 中一次性执行；非热路径。
- 无需处理——仅作为 Token 计算最佳实践参考（已经符合从 `SharedToken` 派生的规范）。

---

## 无重大独立问题的控件

| 控件 | 说明 |
|---|---|
| `Avatar` | 单体控件无热点；群组问题在 AVG-E1 |
| `Badge` | 纯装饰控件，模板化 |
| `Tag` | `TagToken` 一次性 `new Thickness`；控件自身无 code-behind 热点 |
| `Descriptions` | Grid-based，无额外热点 |
| `Statistic` | 含动画数字但动画短暂 |
| `Timeline` / `Steps` | 顺序列表，通常规模小 |
| `QRCode` | 绘制一次性，无热路径 |
| `Rate` | 固定星数，单次构造 |
| `Calendar` | 日期网格每月重建一次；建议关注是否每次 `IsVisible` 变化都重建（未发现明确问题） |
| `Carousel` | 使用虚拟化 Panel；CAR-H1 为次要项 |
| `ImagePreviewer` | 除 IMG-H1 外，`Subscribe` 均配对 `Dispose`；实现较稳健 |
| `Separator` | 纯装饰 |
| `GroupBox` / `HeaderedContentControl` | 纯容器模板 |



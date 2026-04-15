# AtomUI 控件 API 设计审计报告

> **审计范围**: `src/AtomUI.Controls` + `src/AtomUI.Desktop.Controls`
> **对标**: Avalonia 控件 API 惯例、Ant Design 5.0 规范、项目自身一致性
> **日期**: 2026-04-15

---

## 总览

本报告按 **严重度** 分级，每个问题列出：涉及控件/文件、现状、建议修改、影响评估。

| 严重度 | 含义 |
|--------|------|
| 🔴 **高** | 命名错误/拼写错误，或违反 .NET/Avalonia 明确惯例，对外部用户造成困惑 |
| 🟡 **中** | 项目内部不一致，不影响编译但破坏 API 整体一致性和可预测性 |
| 🔵 **低** | 风格偏好/可改可不改，但统一后能提升开发体验 |

---

## 🔴 高严重度

### 1. `Abstract` 前缀类未声明为 `abstract`

**问题**: 项目规范要求 `Abstract` 前缀表示抽象基类，但以下 15 个类使用了 `Abstract` 前缀却**未**声明 `abstract` 关键字：

| 文件 | 类名 |
|------|------|
| `AtomUI.Controls/Buttons/AbstractHyperLinkButton.cs` | `public class AbstractHyperLinkButton` |
| `AtomUI.Controls/Buttons/AbstractToggleIconButton.cs` | `public class AbstractToggleIconButton` |
| `AtomUI.Controls/CheckBox/AbstractCheckBox.cs` | `public class AbstractCheckBox` |
| `AtomUI.Controls/CheckBox/AbstractCheckBoxGroup.cs` | `public class AbstractCheckBoxGroup` |
| `AtomUI.Controls/OptionButtonGroup/AbstractOptionButton.cs` | `public class AbstractOptionButton` |
| `AtomUI.Controls/OptionButtonGroup/AbstractOptionButtonGroup.cs` | `public class AbstractOptionButtonGroup` |
| `AtomUI.Controls/RadioButton/AbstractRadioButton.cs` | `public class AbstractRadioButton` |
| `AtomUI.Controls/RadioButton/AbstractRadioButtonGroup.cs` | `public class AbstractRadioButtonGroup` |
| `AtomUI.Controls/ProgressBar/AbstractGeneralProgressBar.cs` | `public class AbstractGeneralProgressBar` |
| `AtomUI.Controls/ProgressBar/AbstractGeneralDashboardProgress.cs` | `public class AbstractGeneralDashboardProgress` |
| `AtomUI.Controls/ProgressBar/AbstractGeneralCircleProgress.cs` | `public class AbstractGeneralCircleProgress` |
| `AtomUI.Controls/ProgressBar/AbstractGeneralStepsProgressBar.cs` | `public class AbstractGeneralStepsProgressBar` |
| `AtomUI.Controls/QRCode/AbstractQRCode.cs` | `public class AbstractQRCode` |
| `AtomUI.Controls/Rate/AbstractRate.cs` | `public class AbstractRate` |
| `AtomUI.Controls/Switch/AbstractToggleSwitch.cs` | `public class AbstractToggleSwitch` |

另外，`AtomUI.Desktop.Controls` 中也有同样问题：

| 文件 | 类名 |
|------|------|
| `AtomUI.Desktop.Controls/AutoComplete/AbstractAutoComplete.cs` | `public class AbstractAutoComplete` |
| `AtomUI.Desktop.Controls/Select/AbstractSelect.cs` | `public class AbstractSelect` |
| `AtomUI.Desktop.Controls/ImagePreviewer/AbstractImagePreviewer.cs` | `public class AbstractImagePreviewer` |

**建议**: 将所有 `Abstract` 前缀类加上 `abstract` 关键字，使语义和实现一致。如果某些类确实不需要是抽象类（即没有需要子类实现的抽象成员），则应考虑去掉 `Abstract` 前缀。

---

### 2. 拼写错误：`IsMessageMarqueEnabled` (Alert)

**文件**: `AtomUI.Desktop.Controls/Alert/Alert.cs`

```csharp
// 当前 (错误拼写 — 缺少末尾 'e')
public static readonly StyledProperty<bool> IsMessageMarqueEnabledProperty =
    AvaloniaProperty.Register<Alert, bool>(nameof(IsMessageMarqueEnabled));

// 应为
public static readonly StyledProperty<bool> IsMessageMarqueeEnabledProperty =
    AvaloniaProperty.Register<Alert, bool>(nameof(IsMessageMarqueeEnabled));
```

**说明**: "Marquee" 是标准英文拼写。项目中 `MarqueeLabel` 控件本身拼写正确，但 Alert 中的属性缺少末尾 `e`。

**影响**: 🔴 公共 API 拼写错误，发布后难以修改。

---

### 3. Bool 属性 `Bordered` 缺少 `Is` 前缀 (Tag)

**文件**: `AtomUI.Controls/Tag/AbstractTag.cs`

```csharp
// 当前 (与项目惯例不一致)
public static readonly StyledProperty<bool> BorderedProperty =
    AvaloniaProperty.Register<AbstractTag, bool>(nameof(Bordered), true);

// 同类控件 (遵循惯例)
// QRCode:
public static readonly StyledProperty<bool> IsBorderedProperty = ...
// Descriptions:
public static readonly StyledProperty<bool> IsBorderedProperty = ...
```

**说明**: 项目中所有布尔属性统一使用 `Is` 前缀（`IsClosable`、`IsMotionEnabled`、`IsShowIcon` 等）。`Bordered` 是唯一的例外。QRCode 和 Descriptions 控件已正确使用 `IsBordered`。

**建议**: 重命名为 `IsBordered` / `IsBorderedProperty`。

---

### 4. Bool 属性 `ShowZero` 缺少 `Is` 前缀 (CountBadge)

**文件**: `AtomUI.Controls/Badge/AbstractCountBadge.cs`

```csharp
// 当前
public static readonly StyledProperty<bool> ShowZeroProperty =
    AvaloniaProperty.Register<AbstractCountBadge, bool>(nameof(ShowZero));

// 应为 (与项目中 IsShow* 惯例一致)
public static readonly StyledProperty<bool> IsShowZeroProperty = ...
```

**说明**: 项目中大量使用 `IsShow*` 模式（`IsShowIcon`、`IsShowArrow`、`IsShowDescription` 等约 60+ 个），`ShowZero` 是少数不带 `Is` 前缀的例外。

---

## 🟡 中严重度

### 5. `IsAllowClear` vs `IsClearable` 命名分裂

**现状**: 清除功能的布尔属性存在两种命名：

| 命名 | 使用控件 |
|------|---------|
| `IsAllowClear` | Select, AutoComplete, Cascader, TreeSelect, NumericUpDown, TimePicker, DatePicker (7 个控件) |
| `IsClearable` | SelectHandle (1 个控件) |

**建议**: 统一为 `IsAllowClear`（多数派且更符合 Ant Design `allowClear` 命名）。将 `SelectHandle.IsClearable` 重命名为 `IsAllowClear`。

---

### 6. `IsWaveSpiritEnabled` vs `IsWaveAnimationEnabled` 命名分裂

**现状**: 波纹动画的布尔属性存在两种命名：

| 命名 | 使用控件 |
|------|---------|
| `IsWaveSpiritEnabled` | Button、CheckBox、RadioButton、OptionButton 等 (主流，10+ 控件) |
| `IsWaveAnimationEnabled` | RadioIndicator、Slider (2 个控件) |

**建议**: 统一为 `IsWaveSpiritEnabled`（主流且与 `IWaveSpiritAwareControl` 接口一致）。

---

### 7. Badge 相关属性命名前缀不一致

**问题**: 在 `AbstractCountBadge` (Badge 自身) 和嵌入 Badge 的复合控件之间，属性命名前缀策略不统一：

| 属性 | AbstractCountBadge (Badge 自身) | 复合控件 (FloatButton 等) |
|------|------|------|
| 颜色 | `BadgeColor` | `BadgeColor` ✅ |
| 偏移 | `Offset` | `BadgeOffset` ❌ |
| 数量 | `Count` | `BadgeCount` ❌ |
| 溢出数 | `OverflowCount` | `BadgeOverflowCount` ❌ |
| 可见性 | `BadgeIsVisible` | — |

**说明**: 复合控件为避免属性冲突给 Badge 属性加了 `Badge` 前缀，这是合理的。但 Badge 控件自身的 `BadgeColor` 和 `BadgeIsVisible` 也带了 `Badge` 前缀——当你已经在使用 `CountBadge` 时，`BadgeColor` 就是冗余的（等同于 `CountBadge.CountBadgeColor`）。

**建议**:
- Badge 自身控件：`BadgeColor` → `Color`，`BadgeIsVisible` → `IsVisible`（或使用 Avalonia 内置的 `IsVisible`）
- 复合控件继续保留 `Badge` 前缀以消歧

---

### 8. `CountBadgeSize` 枚举与通用 `SizeType` 不协调

**文件**: `AtomUI.Controls/Badge/AbstractCountBadge.cs`

```csharp
// 当前 — 自定义枚举 + 属性名为 Size
public enum CountBadgeSize { Default, Small }
public static readonly StyledProperty<CountBadgeSize> SizeProperty = ...
```

**问题**:
1. 属性名是 `Size` 而非 `SizeType`，与全局 `ISizeTypeAware.SizeType` 惯例不一致
2. 枚举值 `Default` / `Small` 与全局 `SizeType` 枚举 (`Small`/`Middle`/`Large`) 不对应

**建议**: 如果 Badge 确实只支持两种尺寸，可保留自定义枚举但重命名属性为 `BadgeSize` 以消歧。或考虑映射到 `SizeType`（`Default` → `Middle`）。

---

### 9. Alert `CloseRequest` 使用 CLR 事件而非 `RoutedEvent`

**文件对比**:

```csharp
// Tag (正确 — 使用 RoutedEvent)
public static readonly RoutedEvent<RoutedEventArgs> ClosedEvent =
    RoutedEvent.Register<AbstractTag, RoutedEventArgs>(nameof(Closed), RoutingStrategies.Bubble);

// Alert (不一致 — 使用 CLR 事件)
public event EventHandler? CloseRequest;
```

**说明**: Avalonia 控件的标准做法是使用 `RoutedEvent` 以支持事件冒泡和样式系统集成。Tag 的 `Closed` 事件正确使用了 `RoutedEvent`，而 Alert 的 `CloseRequest` 使用普通 CLR 事件，功能受限：
- 无法通过 XAML `EventSetter` 绑定
- 不支持事件冒泡
- 事件名应为过去式 `Closed`（表示已关闭）或 `Closing`（表示正在关闭，可取消）

**建议**: 将 `Alert.CloseRequest` 改为 `RoutedEvent`，命名为 `Closing`（可取消）或 `Closed`（不可取消），与 Tag 保持一致。

---

### 10. `WaveSpiritDecorator.WaveType` 命名不规范

**文件**: `AtomUI.Controls/Primitives/WaveSpiritDecorator.cs`

```csharp
// 当前
public static readonly StyledProperty<WaveSpiritType> WaveTypeProperty = ...

// 应为 (属性名应匹配类型名)
public static readonly StyledProperty<WaveSpiritType> WaveSpiritTypeProperty = ...
```

**说明**: 属性类型是 `WaveSpiritType`，但属性名却用了缩写 `WaveType`。Avalonia 惯例是属性名与类型名保持语义一致。

---

### 11. `PresetImage` 属性名与类型名 `PresetEmptyImage` 不匹配

**文件**: `AtomUI.Controls/Empty/AbstractEmpty.cs`

```csharp
// 当前 — 属性名是 PresetImage，但类型是 PresetEmptyImage
public static readonly StyledProperty<PresetEmptyImage?> PresetImageProperty = ...
public PresetEmptyImage? PresetImage { get; set; }
```

**问题**: 属性名 `PresetImage` 过于宽泛，且与类型名 `PresetEmptyImage` 不匹配。在 XAML 中使用时 `PresetImage="Default"` 容易让人以为可以设置任意预设图片。

**建议**: 重命名为 `PresetEmptyImage`，使属性名与类型名一致，语义也更精确。

---

### 12. `AbstractHyperLinkButton` 命名与 Avalonia `HyperlinkButton` 不一致

**文件**: `AtomUI.Controls/Buttons/AbstractHyperLinkButton.cs`

**说明**: Avalonia 中使用 `Hyperlink`（一个词），而 AtomUI 使用 `HyperLink`（驼峰拼写）。虽然这是 AtomUI 自己的命名空间，但与 Avalonia 生态的习惯用法不一致。

另外，`NavigateUri` 属性来自 Avalonia 的 `HyperlinkButton`，但 AtomUI 同时还定义了 `Href` 概念（在 Ant Design 中使用），没有说明两者的关系。

**建议**: 这属于风格偏好，可保持现状，但应在文档中注明命名差异。

---

## 🔵 低严重度

### 13. `CardStyleVariant` vs `InputControlStyleVariant` — 变体枚举未统一

**现状**:

| 控件 | StyleVariant 类型 |
|------|-------------------|
| Input 系控件 (TextBox, Select, AutoComplete...) | `InputControlStyleVariant` (Outlined/Filled/Borderless) |
| Card | `CardStyleVariant` (Default/Outline/Borderless) |

**说明**: Card 的变体值（`Default`/`Outline`/`Borderless`）与输入控件的变体值（`Outlined`/`Filled`/`Borderless`）虽有差异，但 `Outline` vs `Outlined` 的不一致容易引起混淆。

**建议**: 对齐命名 — Card 的 `Outline` 改为 `Outlined`，或在文档中明确区分。

---

### 14. `TagText` 属性名冗余

**文件**: `AtomUI.Controls/Tag/AbstractTag.cs`

```csharp
// 当前 — Tag.TagText 冗余 (类似 Button.ButtonText)
public static readonly StyledProperty<string?> TagTextProperty = ...

// 更好的命名 (参考 Avalonia 的 ContentControl.Content)
public static readonly StyledProperty<string?> TextProperty = ...
```

**说明**: 在 `Tag` 控件上下文中，`TagText` 中的 `Tag` 前缀是冗余的。Avalonia 和 WPF 惯例中，`TextBlock.Text`、`Button.Content` 都不加控件名前缀。Ant Design React 中对应的 prop 也只是 children/text。

**建议**: 重命名为 `Text`。

---

### 15. `RibbonColor` vs `BadgeColor` vs `TagColor` — 颜色属性命名缺乏统一规则

**现状**:

| 控件 | 颜色属性名 | 类型 |
|------|-----------|------|
| `AbstractTag` | `TagColor` | `string?` |
| `AbstractCountBadge` | `BadgeColor` | `string?` |
| `AbstractDotBadge` | `BadgeColor` (内部) + `BadgeDotColor` (IBrush?) | 混合 |
| `AbstractRibbonBadge` | `RibbonColor` | `string?` |
| `Alert` | `Type` (enum) | `AlertType` |

**问题**:
1. 颜色类型不统一：有的用 `string?`（接受 preset 名或 hex），有的用 `IBrush?`
2. 命名前缀策略不一致：`TagColor`（有前缀）vs `BadgeColor`（有前缀）vs `Color`（无前缀）

**建议**: 建立统一约定 — 当属性接受预设颜色名/hex 字符串时统一命名为 `Color`（在各自控件上下文中不需要前缀）。类型统一为 `string?`。

---

### 16. `ImagePath` vs `ImageSource` vs `PresetImage` — Empty 控件多图源 API 混乱

**文件**: `AtomUI.Controls/Empty/AbstractEmpty.cs`

```csharp
public static readonly StyledProperty<PresetEmptyImage?> PresetImageProperty = ...
public static readonly StyledProperty<string?> ImagePathProperty = ...
public static readonly StyledProperty<string?> ImageSourceProperty = ...
```

**问题**: 三个互斥属性，运行时通过 `CheckImageSource()` 抛出异常来保证互斥。这不是一个好的 API 设计。

**建议**: 考虑使用联合类型或将图片源统一为一个属性 + 一个枚举类型来标识来源，或至少提供更清晰的文档说明三者的区别和使用场景：
- `PresetImage` — 内置预设图片
- `ImagePath` — SVG 文件路径
- `ImageSource` — 内联 SVG 字符串

---

### 17. `AbstractPagination` 位于 Desktop.Controls 而非 AtomUI.Controls

**文件**: `AtomUI.Desktop.Controls/Pagination/AbstractPagination.cs`

**说明**: 根据项目架构规范，`Abstract` 前缀的基类应位于 `AtomUI.Controls`（Base Control Layer），而 `AbstractPagination` 和 `SimplePagination` 都在 `AtomUI.Desktop.Controls` 中。

**建议**: 如果 Pagination 未来需要 Mobile 版本，应将 `AbstractPagination` 移至 `AtomUI.Controls`。

---

### 18. `AbstractImagePreviewer` 位于 Desktop.Controls 而非 AtomUI.Controls

**文件**: `AtomUI.Desktop.Controls/ImagePreviewer/AbstractImagePreviewer.cs`

与上一条同理。

---

## 汇总

| 严重度 | 数量 | 已修复 | 待修复 | 核心关键词 |
|--------|------|--------|--------|-----------|
| 🔴 高 | 4 | 4 | 0 | Abstract 非 abstract ✅、拼写错误 ✅、Bool 缺 Is 前缀 ✅ |
| 🟡 中 | 8 | 0 | 8 | 命名分裂、事件类型不一致、属性名不匹配类型名 |
| 🔵 低 | 6 | 0 | 6 | 枚举命名对齐、属性冗余前缀、架构层级 |
| **合计** | **18** | **4** | **14** | |

**修复进度**: 🟢 4/18 (22.2%)

---

## 建议优先级与修复状态

### 1. **立即修复** (🔴 高严重度)

- ✅ **[已完成]** 为所有 `Abstract` 前缀类加上 `abstract` 关键字  
  *Commit: 2a3a87c5 (2026-04-15)*

- ✅ **[已完成]** `IsMessageMarqueEnabled` → `IsMessageMarqueeEnabled` (拼写修复)  
  *Severity: Critical, File: Alert.cs*  
  *Changes: Alert.cs, AlertTheme.axaml, AlertShowCase.axaml, docs/*

- ✅ **[已完成]** `Bordered` → `IsBordered` (Tag)  
  *Severity: High, File: AbstractTag.cs*  
  *Changes: AbstractTag.cs, SelectTagTheme.axaml, TagShowCase.axaml, CaseNavigation.axaml, docs/*

- ✅ **[已完成]** `ShowZero` → `IsShowZero` (CountBadge)  
  *Severity: High, File: AbstractCountBadge.cs*  
  *Changes: AbstractCountBadge.cs, BadgeShowCase.axaml, docs/*

### 2. **近期统一** (🟡 中严重度)

- ⏳ `IsClearable` → `IsAllowClear` (SelectHandle, 7 vs 1 controls)
- ⏳ `IsWaveAnimationEnabled` → `IsWaveSpiritEnabled` (RadioIndicator, Slider)
- ⏳ `Alert.CloseRequest` → `RoutedEvent` (Event pattern consistency)
- ⏳ `WaveType` → `WaveSpiritType` (Property naming convention)
- ⏳ `PresetImage` → `PresetEmptyImage` (Property vs type name alignment)
- ⏳ Badge 属性命名前缀整理 (BadgeColor, BadgeIsVisible)
- ⏳ `CountBadgeSize` 与 `SizeType` 对齐
- ⏳ HyperLinkButton 驼峰拼写（仅文档注明）

### 3. **版本迭代中逐步调整** (🔵 低严重度)

- ⏳ `TagText` → `Text` (Redundant prefix)
- ⏳ `CardStyleVariant.Outline` → `Outlined` (Enum value alignment)
- ⏳ 颜色属性命名统一 (TagColor, BadgeColor, RibbonColor)
- ⏳ `ImagePath` vs `ImageSource` vs `PresetImage` API 设计优化
- ⏳ `AbstractPagination` 位置整理 (Desktop → Base Layer)
- ⏳ `AbstractImagePreviewer` 位置整理 (Desktop → Base Layer)


# AddOnDecoratedBox 性能分析

## 背景

`LineEditTheme.axaml` 模板中仅增加了一层 `AddOnDecoratedBox` 包装，但性能下降非常明显。以下是根因分析。

---

## 1. 构造函数逐实例创建 Style 对象（最关键）

**文件**: `AddOnDecoratedBox.cs:296-300, 633-701`

每个 `AddOnDecoratedBox` 实例在构造时调用 `ConfigureInstanceStyles()`，创建 3 组复杂的 Style：

- Warning 状态样式（4 分支 `Selectors.Or`）
- Error 状态样式（4 分支 `Selectors.Or`）
- Disabled 状态样式（8 分支 `Selectors.Or`）

每个分支都使用了 `Descendant()` 选择器，Avalonia 需要扫描整个子树来匹配。一个表单如果有 50 个输入控件，就会产生 **50 × 6 = 300 个 Style 对象**，每个都带有深度嵌套的选择器树和 lambda 委托。

**这是最大的性能杀手。**

---

## 2. 单次属性变更触发多轮级联重算

**文件**: `AddOnDecoratedBox.cs:344-384`

当 `StyleVariant` 变化时，会依次触发：

1. `UpdatePseudoClasses()` — 设置 PseudoClass
2. `ConfigureInnerBoxBorderThickness()` — `SetCurrentValue` 触发 PropertyChanged
3. `ConfigureInnerBoxCornerRadius()` — 再次 `SetCurrentValue`
4. `ConfigureAddOnBorderInfo()` — 又 2~4 次 `SetAndRaise`

一次属性变更 → 4~8 次布局失效。而 TemplateBinding 从 LineEdit 传入的属性（StyleVariant、Status、SizeType 等）每个都会走这条路径。

---

## 3. 主题文件 82 个嵌套 Style + 冗余 `:is()` 包装

**文件**: `AddOnDecoratedBoxTheme.axaml`

所有样式被包在 `^:is(atom|AddOnDecoratedBox)` 下，这层是冗余的（`ControlTheme.TargetType` 已经限定了类型）。82 个嵌套样式在每次属性变更时都要重新评估选择器匹配，加上第 1 点的实例样式，每个实例需要评估 **88 个 Style**。

---

## 4. Transitions 在 Load/Unload 时反复创建销毁

**文件**: `AddOnDecoratedBox.cs:309-338`

`OnLoaded` 创建 Transitions，`OnUnloaded` 置 null。在虚拟化场景（下拉列表、滚动列表）中控件频繁 Load/Unload，每次都重建 Transitions 集合。

---

## 建议修复方向

| 优先级 | 修复项 | 预期收益 |
|--------|--------|----------|
| P0 | 将 `ConfigureInstanceStyles()` 的逻辑移到 AXAML ControlTheme 中，用 PseudoClass 驱动，消除逐实例 Style 创建 | 消除 N×6 个 Style 对象 |
| P0 | 移除主题中冗余的 `^:is(atom\|AddOnDecoratedBox)` 包装层，扁平化选择器嵌套 | 减少选择器匹配开销 |
| P1 | 合并 `OnPropertyChanged` 中的多个 Configure 方法，用 dirty flag + 延迟批量更新替代即时多次 SetAndRaise | 单次属性变更只触发一次布局 |
| P1 | 缓存 Transitions 实例，避免 Load/Unload 时反复创建 | 减少虚拟化场景开销 |
| P2 | `ConfigureAddOnBorderInfo` 中增加值未变化时的 early-exit 检查 | 避免无意义的布局失效 |

---

## 根因总结

看起来只加了一层控件，但这一层控件的构造函数和属性变更回调做了太多重量级操作，尤其是逐实例创建带 `Descendant()` 选择器的 Style 对象，这在 Avalonia 的样式系统中代价非常高。

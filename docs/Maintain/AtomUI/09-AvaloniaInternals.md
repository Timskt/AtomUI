# 09 - Avalonia 内部机制参考

> 本文档记录 AtomUI 依赖的 Avalonia UI 框架内部机制，供 AI 迭代时参考。基于 Avalonia 11.x 源码分析。

## Avalonia 控件继承层次

```
Animatable
  └── Visual
      └── Layoutable
          └── InputElement
              └── Interactive
                  └── Control
                      ├── ContentControl
                      │   ├── Button
                      │   ├── HeaderedContentControl
                      │   │   ├── Expander
                      │   │   ├── GroupBox
                      │   │   └── TabItem
                      │   ├── ToggleButton
                      │   │   ├── CheckBox
                      │   │   └── RadioButton
                      │   ├── RepeatButton
                      │   └── UserControl
                      ├── ItemsControl
                      │   ├── ListBox
                      │   ├── Menu
                      │   ├── TabControl
                      │   ├── TreeView
                      │   └── ComboBox
                      ├── Panel
                      │   ├── StackPanel
                      │   ├── WrapPanel
                      │   ├── DockPanel
                      │   ├── Grid
                      │   └── Canvas
                      ├── RangeBase
                      │   ├── ProgressBar
                      │   ├── Slider
                      │   └── ScrollBar
                      ├── TemplatedControl
                      ├── SelectingItemsControl
                      └── HeaderedSelectingItemsControl
```

## StyledProperty 系统

### 属性注册

```csharp
// Avalonia 属性注册模式
public static readonly StyledProperty<string> TextProperty =
    AvaloniaProperty.Register<MyControl, string>(nameof(Text), defaultValue: "");

// DirectProperty（不参与样式系统，性能更好）
public static readonly DirectProperty<MyControl, string> InternalProperty =
    AvaloniaProperty.RegisterDirect<MyControl, string>(nameof(Internal), o => o._internal);
```

### 属性变更订阅

```csharp
// 静态构造函数中订阅
static MyControl()
{
    TextProperty.Changed.AddClassHandler<MyControl>((x, e) => x.OnTextChanged(e));
}

// 实例回调
protected virtual void OnTextChanged(AvaloniaPropertyChangedEventArgs<string> e)
{
    // 处理变更
}
```

### 关键区别: StyledProperty vs DirectProperty

| 特性 | StyledProperty | DirectProperty |
|------|---------------|---------------|
| 样式可设置 | ✅ | ❌ |
| 继承值 | ✅ 可选 | ❌ |
| 绑定默认模式 | OneWay | OneWay |
| 性能 | 较低 | 较高 |
| 适用场景 | 公开 API、可样式化 | 内部状态、高性能 |

## PseudoClasses 机制

### 内置伪类

| 伪类 | 触发条件 |
|------|----------|
| `:pointerover` | 鼠标悬停 |
| `:focus` | 获得焦点 |
| `:pressed` | 按下 |
| `:disabled` | 禁用 |
| `:selected` | 选中 |
| `:checked` | 勾选 |
| `:focus-within` | 子元素获焦 |
| `:valid` / `:invalid` | 数据验证 |

### 自定义伪类

```csharp
// 在控件中定义伪类字段
private readonly PseudoClasses _sizeTypePseudoClasses = new();

// 更新伪类
PseudoClasses.Set(":small", SizeType == ControlSizeType.Small);
PseudoClasses.Set(":large", SizeType == ControlSizeType.Large);

// XAML 中使用
// atom|Button:small { ... }
// atom|Button:large:hover { ... }
```

### AtomUI 自定义伪类清单

| 伪类 | 使用控件 | 含义 |
|------|----------|------|
| `:small` | 多数控件 | 小尺寸变体 |
| `:middle` | 多数控件 | 中尺寸变体 |
| `:large` | 多数控件 | 大尺寸变体 |
| `:primary` | Button, Tag | 主要样式 |
| `:secondary` | Button | 次要样式 |
| `:outlined` | Button, Tag | 描边样式 |
| `:dashed` | Button | 虚线样式 |
| `:link` | Button | 链接样式 |
| `:text` | Button | 文本样式 |
| `:danger` | Button, Input | 危险样式 |
| `:success` | Input, Tag | 成功样式 |
| `:warning` | Input, Tag | 警告样式 |
| `:info` | Tag | 信息样式 |
| `:loading` | Button, Spin | 加载状态 |
| `:circle` | Button | 圆形按钮 |
| `:round` | Button | 圆角按钮 |
| `:ghost` | Button | 幽灵按钮 |
| `:block` | Button | 块级按钮 |
| `:readonly` | Input | 只读状态 |
| `:bordered` | Table | 带边框 |
| `:fixed` | Table | 固定列 |
| `:open` | Collapse, Dropdown | 展开状态 |
| `:active` | Menu, Tab | 激活状态 |
| `:inline` | Form | 行内布局 |
| `:horizontal` | Steps, Space | 水平方向 |
| `:vertical` | Steps, Space | 垂直方向 |
| `:closable` | Tag | 可关闭 |
| `:checked` | Checkbox, Switch | 选中状态 |
| `:indeterminate` | Checkbox | 不确定状态 |
| `:hasfeedback` | Form | 有反馈 |
| `:rtl` | 全局 | 从右到左 |

## ControlTheme 系统

### 主题定义 (AXAML)

```xml
<ControlTheme x:Key="MyButtonTheme" TargetType="atom:Button">
    <Setter Property="Background" Value="{atom:ControlTokenResource Key=ButtonTokenColorBgContainer}" />
    <Setter Property="CornerRadius" Value="{atom:ControlTokenResource Key=ButtonTokenBorderRadius}" />
</ControlTheme>

<!-- 派生主题 -->
<ControlTheme x:Key="MyPrimaryButtonTheme" BasedOn="{StaticResource MyButtonTheme}" TargetType="atom:Button">
    <Setter Property="Background" Value="Blue" />
</ControlTheme>
```

### 主题选择器语法

```css
/* 类型选择器 */
atom|Button { }

/* 伪类选择器 */
atom|Button:primary:hover { }

/* 类选择器 */
.my-class { }

/* 名称选择器 */
#myControl { }

/* 子代选择器 */
atom|Button > atom|TextBlock { }

/* 后代选择器 */
atom|Button atom|TextBlock { }

/* 模板子代选择器 (^) — 穿越模板边界 */
atom|Button ^ atom|TextBlock { }

/* 逻辑非选择器 */
atom|Button:not(:disabled) { }

/* 多重伪类 */
atom|Input:focus:danger { }
```

## 模板系统

### ControlTemplate + TemplateBinding

```xml
<ControlTheme TargetType="atom:MyControl">
    <Setter Property="Template">
        <ControlTemplate>
            <Border Background="{TemplateBinding Background}"
                    CornerRadius="{TemplateBinding CornerRadius}">
                <ContentPresenter Content="{TemplateBinding Content}" />
            </Border>
        </ControlTemplate>
    </Setter>
</ControlTheme>
```

### OnApplyTemplate 回调

```csharp
// 获取模板中的命名控件
protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
{
    base.OnApplyTemplate(e);
    
    _border = e.GetTemplateChild<Border>("PART_Border");
    _contentPresenter = e.GetTemplateChild<ContentPresenter>("PART_ContentPresenter");
    _iconPresenter = e.GetTemplateChild<IconPresenter>("PART_IconPresenter");
}
```

### PART 命名约定

AtomUI 控件模板中的命名元素统一使用 `PART_` 前缀：

| PART 名 | 典型用途 |
|---------|---------|
| `PART_Root` | 根容器 Border |
| `PART_ContentPresenter` | 内容呈现器 |
| `PART_IconPresenter` | 图标呈现器 |
| `PART_Header` | 头部内容 |
| `PART_Input` | 输入框 |
| `PART_ScrollViewer` | 滚动查看器 |
| `PART_Indicator` | 指示器 |
| `PART_Popup` | 弹出层 |

## Flyout/Popup 系统

### FlyoutBase 层次

```
FlyoutBase (抽象)
  ├── Flyout (标准弹出)
  ├── MenuFlyout (菜单弹出)
  ├── PopupFlyout
  └── ToolTip
```

### 使用模式

```csharp
// 在控件中创建 Flyout
var flyout = new Flyout
{
    Content = new TextBlock { Text = "Hello" }
};
FlyoutBase.SetAttachedFlyout(this, flyout);

// 显示 Flyout
FlyoutBase.ShowAttachedFlyout(this);
```

## 数据绑定

### 绑定模式

| 模式 | 说明 | XAML 语法 |
|------|------|-----------|
| OneWay | 源→目标（默认 StyledProperty） | `{Binding Path}` |
| TwoWay | 源↔目标（默认 DirectProperty） | `{Binding Path, Mode=TwoWay}` |
| OneTime | 仅初始化 | `{Binding Path, Mode=OneTime}` |
| OneWayToSource | 目标→源 | `{Binding Path, Mode=OneWayToSource}` |

### CompiledBinding (AtomUI 默认启用)

```xml
<!-- 编译绑定（默认启用，性能更好） -->
<TextBlock Text="{Binding UserName}" />

<!-- 反射绑定（回退方案） -->
<TextBlock Text="{Binding UserName, x:DataType=viewmodels:MyVM}" />
```

> **重要**: `Directory.Build.props` 中 `AvaloniaUseCompiledBindingsByDefault=true`，所有绑定默认为编译绑定。

## 视觉树与逻辑树

### 生命周期事件

| 事件/方法 | 触发时机 | 用途 |
|-----------|----------|------|
| `OnInitialized` | 控件初始化完成 | 初始设置 |
| `OnAttachedToLogicalTree` | 加入逻辑树 | 资源查找 |
| `OnDetachedFromLogicalTree` | 离开逻辑树 | 清理 |
| `OnAttachedToVisualTree` | 加入视觉树 | 渲染前准备 |
| `OnDetachedFromVisualTree` | 离开视觉树 | 释放资源 |
| `OnLoaded` | 完全加载（布局+渲染完成） | 安全访问模板子元素 |
| `OnUnloaded` | 卸载 | 清理 |

### 推荐的资源清理模式

```csharp
protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
{
    base.OnDetachedFromVisualTree(e);
    // 取消动画
    // 释放 Timer
    // 断开事件订阅
}
```

## Avalonia 资源系统

### ResourceDictionary 层级

```
Application.Resources (全局)
  └── ThemeVariant.Resources (主题变体)
      └── Control.Resources (控件级)
          └── ControlTheme.Setters (主题 Setter)
```

### 资源查找顺序

1. 控件自身 `Resources`
2. 逻辑树向上查找父控件 `Resources`
3. `Application.Current.Resources`
4. 当前 `ThemeVariant` 资源

### DynamicResource vs StaticResource

| 特性 | DynamicResource | StaticResource |
|------|----------------|---------------|
| 运行时更新 | ✅ | ❌ |
| 主题切换响应 | ✅ | ❌ |
| 性能 | 较低 | 较高 |
| 适用场景 | Token 值、主题色 | 不可变常量 |

> **AtomUI 中 Token 资源全部使用 DynamicResource 语义**（通过 `ControlTokenResource` 标记扩展实现），确保主题切换时自动更新。

## Avalonia 11.x 关键 API 变更

### 相对于 Avalonia 10.x 的破坏性变更

| 变更 | 说明 |
|------|------|
| `Style` → `ControlTheme` | 样式系统重构 |
| `Selector` 语法变更 | CSS-like 选择器替代 WPF 样式选择器 |
| `AvaloniaObject.Bind` 签名变更 | 绑定 API 更新 |
| `Window` → `TopLevel` | 顶层抽象统一 |
| `PseudoClasses` API | 新增 `Set(name, value)` 方法 |
| `CompiledBinding` | 默认启用编译绑定 |
| `ThemeVariant` | 替代旧的 `Theme` 概念 |

### AtomUI 依赖的关键 Avalonia 内部 API

| API | 命名空间 | 用途 | 风险等级 |
|-----|----------|------|----------|
| `PseudoClasses.Set()` | `Avalonia.Controls` | 伪类更新 | 🟢 稳定 |
| `AvaloniaProperty.Register` | `Avalonia` | 属性注册 | 🟢 稳定 |
| `TemplateAppliedEventArgs` | `Avalonia.Controls.Templates` | 模板应用 | 🟢 稳定 |
| `ThemeVariant` | `Avalonia.Styling` | 主题变体 | 🟢 稳定 |
| `ResourceDictionary` | `Avalonia.Controls` | 资源管理 | 🟢 稳定 |
| `ControlTheme` | `Avalonia.Styling` | 控件主题 | 🟢 稳定 |
| `FlyoutBase` | `Avalonia.Controls` | 弹出层 | 🟡 可能变更 |
| `AdornerLayer` | `Avalonia.Controls.Primitives` | 装饰层 | 🟡 可能变更 |
| `VisualTreeAttachmentEventArgs` | `Avalonia.VisualTree` | 视觉树事件 | 🟢 稳定 |
| `Dispatcher.UIThread` | `Avalonia.Threading` | UI 线程调度 | 🟢 稳定 |

## Avalonia 文档参考路径

> 以下路径相对于 `.referenceprojects/avalonia-docs/`

| 文档路径 | 内容 |
|----------|------|
| `docs/concepts/` | 核心概念 |
| `docs/controls/` | 内置控件文档 |
| `docs/custom-controls/` | 自定义控件开发 |
| `docs/styling/` | 样式系统 |
| `docs/data-binding/` | 数据绑定 |
| `docs/properties/` | 属性系统 |
| `docs/xaml/` | XAML 参考 |
| `docs/fundamentals/` | 基础知识 |
| `docs/graphics-animation/` | 动画 |
| `docs/layout/` | 布局系统 |
| `docs/events/` | 事件系统 |
| `docs/input-interaction/` | 输入交互 |

## Ant Design 参考路径

> 以下路径相对于 `.referenceprojects/ant-design/`

AtomUI 的设计规范来源于 Ant Design，控件 Token 值和交互行为应与 Ant Design 保持一致。
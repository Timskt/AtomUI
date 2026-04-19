# Transitions 重构计划：从 C# 代码迁移到 XAML 主题

## 背景

当前大量控件通过 C# 代码方式管理 `Transitions`，存在以下问题：
- 每个控件都有重复的 `ConfigureTransitions(bool force)` 方法
- 需要在 `OnLoaded`/`OnUnloaded` 生命周期中手动管理
- 需要在 `OnPropertyChanged` 中监听 `IsMotionEnabledProperty` 变化

## 重构模式

### 改动前（C# 代码方式）

```csharp
// C# 文件中
private void ConfigureTransitions(bool force)
{
    if (IsMotionEnabled)
    {
        if (force || Transitions == null)
        {
            var transitions = new Transitions();
            transitions.Add(TransitionUtils.CreateTransition<SolidColorBrushTransition>(BackgroundProperty));
            transitions.Add(TransitionUtils.CreateTransition<SolidColorBrushTransition>(ForegroundProperty));
            Transitions = transitions;
        }
    }
    else
    {
        Transitions = null;
    }
}

protected override void OnLoaded(RoutedEventArgs e)
{
    base.OnLoaded(e);
    ConfigureTransitions(false);
}

protected override void OnUnloaded(RoutedEventArgs e)
{
    base.OnUnloaded(e);
    Transitions = null;
}

protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
{
    base.OnPropertyChanged(change);
    if (IsLoaded)
    {
        if (change.Property == IsMotionEnabledProperty)
        {
            ConfigureTransitions(true);
        }
    }
}
```

### 改动后（XAML 主题方式）

```xml
<!-- Theme AXAML 文件中 -->
<Style Selector="^[IsMotionEnabled=True]">
    <Setter Property="Transitions">
        <Transitions>
            <atom:SolidColorBrushTransition Property="Background" Duration="{atom:SharedTokenResourceValue Kind=MotionDurationMid}"/>
            <atom:SolidColorBrushTransition Property="Foreground" Duration="{atom:SharedTokenResourceValue Kind=MotionDurationMid}"/>
        </Transitions>
    </Setter>
</Style>
```

### 每个控件的操作步骤

1. **C# 文件**：
   - 删除 `ConfigureTransitions` 方法
   - 删除 `OnLoaded` 中的 transition 调用（若 `OnLoaded` 只有 transition 逻辑则整个删除）
   - 删除 `OnUnloaded` 中的 `Transitions = null`（若只有这一行则整个删除）
   - 删除 `OnPropertyChanged` 中 `IsMotionEnabledProperty` 的 transition 处理块
   - 清理不再需要的 `using`（`AtomUI.Animations`、`Avalonia.Animation`、`Avalonia.Interactivity` 等）

2. **AXAML 主题文件**：
   - 在合适位置添加 `<Style Selector="^[IsMotionEnabled=True]">` 样式块

---

## 控件列表

### 第 1 批：`src/AtomUI.Controls/` 基础控件

| # | C# 文件 | 需要迁移的 Transitions | 备注 |
|---|---------|----------------------|------|
| 1 | ~~ScrollViewer/AbstractScrollBarThumb.cs~~ | ~~Background(SolidColorBrush), Width(Double), Height(Double)~~ | ✅ |
| 2 | ~~ScrollViewer/AbstractScrollViewer.cs~~ | ~~ScrollBarsSeparatorOpacity(Double), ScrollBarOpacity(Double)~~ | ✅ |
| 3 | ~~Switch/AbstractToggleSwitch.cs~~ | ~~KnobMovingRect(Rect), OnContentOffset(Point), OffContentOffset(Point), GrooveBackground(SolidColorBrush), SwitchOpacity(Double)~~ | ✅ |
| 4 | ~~Switch/SwitchKnob.cs~~ | ~~Background(SolidColorBrush), Width(Double), Height(Double)~~ | ✅ |
| 5 | ~~ProgressBar/AbstractProgressBar.cs~~ | ~~Background(SolidColorBrush), IndicatorBarBrush(SolidColorBrush)~~ | ✅ |
| 6 | ~~Spin/AbstractSpin.cs~~ | ~~DotBrush(SolidColorBrush)~~ | ✅ |
| 7 | CheckBox/Converters/CheckBoxIndicator.cs | Background(SolidColorBrush), BorderBrush(SolidColorBrush) | |
| 8 | Segmented/AbstractSegmented.cs | Background(SolidColorBrush) | |
| 9 | Segmented/AbstractSegmentedItem.cs | Background(SolidColorBrush), Foreground(SolidColorBrush) | |
| 10 | RadioButton/RadioIndicator.cs | Background(SolidColorBrush), BorderBrush(SolidColorBrush), InnerCircleBrush(SolidColorBrush) | |
| 11 | OptionButtonGroup/AbstractOptionButton.cs | Background(SolidColorBrush), Foreground(SolidColorBrush), BorderBrush(SolidColorBrush) | |
| 12 | Buttons/AbstractHyperLinkButton.cs | Foreground(SolidColorBrush) | |
| 13 | Buttons/AbstractToggleIconButton.cs | Background(SolidColorBrush), Foreground(SolidColorBrush) | 有 `NotifyConfigureTransitions` 虚方法 |
| 14 | Buttons/AbstractIconButton.cs | Background(SolidColorBrush), Foreground(SolidColorBrush) | 有 `NotifyConfigureTransitions` 虚方法 |
| 15 | FloatButton/AbstractFloatButton.cs | Background(SolidColorBrush), Foreground(SolidColorBrush), BorderBrush(SolidColorBrush) | |
| 16 | Rate/RateItem.cs | RenderTransform(TransformOperations) | |

### 第 2 批：`src/AtomUI.Desktop.Controls/` 桌面控件 - Part A

| # | C# 文件 | 需要迁移的 Transitions | 备注 |
|---|---------|----------------------|------|
| 17 | Upload/AbstractUploadListItem.cs | （空 Transitions 基类） | 有 `NotifyConfigureTransitions` 虚方法，子类扩展 |
| 18 | Upload/AbstractUploadPictureContent.cs | MaskOpacity(Double, MotionDurationSlow) | Duration 用 MotionDurationSlow |
| 19 | Upload/UploadDefaultDropArea.cs | BorderBrush(SolidColorBrush) | |
| 20 | Upload/UploadTriggerContent.cs | BorderBrush(SolidColorBrush) | |
| 21 | Upload/DefaultList/UploadTextListItemHeader.cs | Background(SolidColorBrush) | |
| 22 | Cascader/CascaderViewItem.cs | Background(SolidColorBrush) | |
| 23 | Steps/StepsItem.cs | Foreground(SolidColorBrush) | |
| 24 | Steps/StepsItemIndicator.cs | Background(SolidColorBrush), BorderBrush(SolidColorBrush), Foreground(SolidColorBrush) | |
| 25 | Card/Card.cs | BorderBrush(SolidColorBrush) | |
| 26 | Card/CardActionPanel.cs | Background(SolidColorBrush) | |
| 27 | Card/CardGridItem.cs | Background(SolidColorBrush) | |
| 28 | ListView/ListViewItem.cs | Background(SolidColorBrush) | |
| 29 | TreeView/TreeViewItemHeader.cs | Background(SolidColorBrush) | |
| 30 | TreeView/NodeSwitcherButton.cs | Foreground(SolidColorBrush) | |
| 31 | Calendar/BaseCalendarDayButton.cs | Background(SolidColorBrush), Foreground(SolidColorBrush) | |
| 32 | Calendar/HeadTextButton.cs | Background(SolidColorBrush), Foreground(SolidColorBrush) | |
| 33 | Calendar/BaseCalendarButton.cs | Background(SolidColorBrush), Foreground(SolidColorBrush) | |
| 34 | DatePicker/CalendarView/CalendarDayButton.cs | Background(SolidColorBrush), Foreground(SolidColorBrush) | |
| 35 | DatePicker/CalendarView/CalendarButton.cs | Background(SolidColorBrush), Foreground(SolidColorBrush) | |

### 第 3 批：`src/AtomUI.Desktop.Controls/` 桌面控件 - Part B

| # | C# 文件 | 需要迁移的 Transitions | 备注 |
|---|---------|----------------------|------|
| 36 | Dialog/ButtonBox/DialogCaptionButton.cs | Background(SolidColorBrush) | |
| 37 | Dialog/OverlayHost/OverlayDialogHeader.cs | Foreground(SolidColorBrush) | |
| 38 | Dialog/OverlayHost/OverlayDialogHost.cs | Opacity(Double), RenderTransform(TransformOperations) | ⚠️ 使用自定义 Duration 和 CircularEaseOut Easing，需确认 XAML 写法 |
| 39 | Dialog/OverlayHost/OverlayDialogMask.cs | Opacity(Double) | |
| 40 | TextBlock/HyperLinkTextBlock.cs | Foreground(SolidColorBrush) | |
| 41 | Collapse/CollapseItem.cs | Background(SolidColorBrush) | |
| 42 | Slider/SliderThumb.cs | Background(SolidColorBrush), BorderBrush(SolidColorBrush) | |
| 43 | Slider/SliderTrack.cs | Background(SolidColorBrush) | |
| 44 | Primitives/IndicatorScrollViewer/IndicatorScrollViewer.cs | LeftIndicatorOpacity(Double), RightIndicatorOpacity(Double) | |
| 45 | TimePicker/TimeView/TimeViewCell.cs | Background(SolidColorBrush), Foreground(SolidColorBrush) | |
| 46 | NavMenu/Header/BaseNavMenuItemHeader.cs | Background(SolidColorBrush), Foreground(SolidColorBrush) | 有 `NotifyConfigureTransitions` 虚方法 |
| 47 | NavMenu/Header/InlineNavMenuItemHeader.cs | 继承基类 + MenuIndicatorRenderTransform(TransformOperations) | 覆写 `NotifyConfigureTransitions` |
| 48 | NavMenu/Header/HorizontalNavMenuItemHeader.cs | 继承基类 + ActiveBarColor(SolidColorBrush) | 覆写 `NotifyConfigureTransitions` |
| 49 | Carousel/CarouselPageIndicator.cs | Background(SolidColorBrush) | |
| 50 | Carousel/CarouselNavButton.cs | 继承自 AbstractIconButton | 覆写 `NotifyConfigureTransitions` |
| 51 | Statistic/StatisticCountUp.cs | ⚠️ 特殊：AnimatingValue 动画，非标准 Transitions | 需单独分析，可能不适合迁移 |
| 52 | Breadcrumb/BreadcrumbItem.cs | Foreground(SolidColorBrush) | |
| 53 | ComboBox/ComboBoxItem.cs | Background(SolidColorBrush) | |

### 第 4 批：`src/AtomUI.Desktop.Controls/` 桌面控件 - Part C

| # | C# 文件 | 需要迁移的 Transitions | 备注 |
|---|---------|----------------------|------|
| 54 | Drawer/DrawerInfoContainer.cs | RenderTransform(TransformOperations) | |
| 55 | Drawer/DrawerContainer.cs | Background(SolidColorBrush) | 注意是 Border 子元素的属性 |
| 56 | SplitView/SplitView.cs | Width(Double) 或 Height(Double) | ⚠️ 特殊：使用自定义属性 `PaneOpenTransitions`/`PaneCloseTransitions`，非标准 `Transitions` |
| 57 | ListBox/ListBoxItem.cs | Background(SolidColorBrush) | |
| 58 | TabControl/TabItem.cs | Background(SolidColorBrush), Foreground(SolidColorBrush) | |
| 59 | TabControl/TabControl.cs | Background(SolidColorBrush) | |
| 60 | TabControl/TabStrip/TabStrip.cs | Background(SolidColorBrush) | |
| 61 | TabControl/TabStrip/TabStripItem.cs | Background(SolidColorBrush), Foreground(SolidColorBrush) | |
| 62 | Menu/MenuItem.cs | Background(SolidColorBrush) | |
| 63 | Chrome/CaptionButton.cs | Background(SolidColorBrush) | |
| 64 | Chrome/WindowTitleBar.cs | Background(SolidColorBrush) | |
| 65 | Pagination/PaginationNavItem.cs | Background(SolidColorBrush), Foreground(SolidColorBrush), BorderBrush(SolidColorBrush) | |
| 66 | ButtonSpinner/ButtonSpinnerDecoratedBox.cs | Background(SolidColorBrush), BorderBrush(SolidColorBrush) | ⚠️ 方法名为 `ConfigureTransitionsForEnabledState`，非标准命名 |
| 67 | Primitives/AddOnDecoratedBox/AddOnDecoratedBox.cs | Background(SolidColorBrush), BorderBrush(SolidColorBrush), Foreground(SolidColorBrush) | |
| 68 | Primitives/InfoPickerInput/RangeInfoPickerInput.cs | Background(SolidColorBrush), BorderBrush(SolidColorBrush) | |
| 69 | ImagePreviewer/ImageViewer.cs | Background(SolidColorBrush) | |
| 70 | ImagePreviewer/ImagePreviewerCover.cs | MaskOpacity(Double) | |

### 第 5 批：DataGrid / ColorPicker / Core

| # | C# 文件 | 需要迁移的 Transitions | 备注 |
|---|---------|----------------------|------|
| 71 | DataGrid/Row/DataGridRow.cs | Background(SolidColorBrush) | |
| 72 | DataGrid/Column/DataGridColumnHeader.cs | Background(SolidColorBrush) | OnPropertyChanged 有其他逻辑 |
| 73 | DataGrid/Column/DataGridRowExpander.cs | Foreground(SolidColorBrush) | |
| 74 | ColorPicker/AbstractColorPicker.cs | Background(SolidColorBrush), BorderBrush(SolidColorBrush) | |
| 75 | ColorPicker/PaletteColorItem.cs | BorderBrush(SolidColorBrush) | |
| 76 | Core/Controls/Icon/Icon.cs | Foreground(SolidColorBrush) | 有 `IconModeChanged` 额外触发，需确认 |

---

## ⚠️ 特殊处理说明

### 1. 有 `NotifyConfigureTransitions` 虚方法的控件

以下控件使用了虚方法模式，子类通过覆写来追加额外 transitions：

- `AbstractProgressBar` → 子类可扩展
- `AbstractToggleIconButton` → `CarouselNavButton` 覆写
- `AbstractIconButton` → `CarouselNavButton` 覆写
- `AbstractUploadListItem` → 子类可扩展
- `BaseNavMenuItemHeader` → `InlineNavMenuItemHeader`（追加 MenuIndicatorRenderTransform）、`HorizontalNavMenuItemHeader`（追加 ActiveBarColor）覆写

**处理方式**：将基类和所有子类的 transitions 都迁移到各自对应的 AXAML 主题文件中，删除虚方法机制。

### 2. OverlayDialogHost（#38）

使用自定义 Duration 和 `CircularEaseOut` Easing，不是标准的 `MotionDurationMid`。需要在 AXAML 中使用具体的 Duration 值和 Easing 类型。

### 3. SplitView（#56）

使用自定义属性 `PaneOpenTransitions`/`PaneCloseTransitions` 而非标准 `Transitions` 属性，需要单独分析是否可以迁移到 XAML。

### 4. StatisticCountUp（#51）

使用的是 `AnimatingValue` 属性的动画，不是标准的 `Transitions` 机制，可能不适合迁移，需单独评估。

### 5. ButtonSpinnerDecoratedBox（#66）

方法名为 `ConfigureTransitionsForEnabledState`，逻辑可能与 `IsEnabled` 状态相关，需确认是否与 `IsMotionEnabled` 联动。

---

## 进度跟踪

- [ ] 第 1 批：AtomUI.Controls 基础控件（16 个）
  - [x] 4 | Switch/SwitchKnob.cs | Background(SolidColorBrush), Width(Double), Height(Double) | ✅ 已完成
  - [x] 5 | ProgressBar/AbstractProgressBar.cs | Background(SolidColorBrush), IndicatorBarBrush(SolidColorBrush) | ✅ 已完成
  - [x] 6 | Spin/AbstractSpin.cs | DotBrush(SolidColorBrush) | ✅ 已完成
- [ ] 第 2 批：AtomUI.Desktop.Controls Part A（19 个）
- [ ] 第 3 批：AtomUI.Desktop.Controls Part B（18 个）
- [ ] 第 4 批：AtomUI.Desktop.Controls Part C（17 个）
- [ ] 第 5 批：DataGrid / ColorPicker / Core（6 个）
- [ ] 特殊控件处理（OverlayDialogHost, SplitView, StatisticCountUp, ButtonSpinnerDecoratedBox）
